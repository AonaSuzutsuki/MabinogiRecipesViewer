using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommonStyleLib.ExMessageBox;
using CommonStyleLib.ExMessageBox.ViewModels;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Models.Settings;
using CookInformationViewer.ViewModels.Searchers;
using CookInformationViewer.Models.Updates;
using CookInformationViewer.ViewModels.Settings;
using CookInformationViewer.ViewModels.Updates;
using CookInformationViewer.Views;
using CookInformationViewer.Views.Searches;
using CookInformationViewer.Views.Settings;
using CookInformationViewer.Views.Updates;
using CookInformationViewer.Views.WindowServices;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CookInformationViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelWindowStyleBase
    {
        #region Fields

        private readonly MainWindowWindowService _mainWindowService;
        private readonly MainWindowModel _model;

        private OverlayModel? _overlayModel;
        private MainWindowWindowService? _overlayWindowService;

        private readonly Stack<RecipeInfo> _historyBack = new();
        private readonly Stack<RecipeInfo> _historyForward = new();

        #endregion

        #region Properties

        public bool IsDebugMode => Constants.IsDebugMode;

        public ReactiveProperty<string> UnderMessageLabelText { get; set; }

        public ReactiveProperty<bool> CanGoBack { get; set; }
        public ReactiveProperty<bool> CanGoForward { get; set; }

        public ReactiveProperty<string> SearchText { get; set; }

        public ReadOnlyReactiveCollection<CategoryInfo> Categories { get; set; }
        public ReactiveProperty<int> SelectedCategoryIndex { get; set; }

        public ReadOnlyReactiveCollection<RecipeInfo> RecipesList { get; set; }

        public ReactiveProperty<RecipeInfo?> SelectedRecipe { get; set; }

        #endregion

        #region Event Properties
        
        public ICommand OpenSettingCommand { get; set; }
        public ICommand OpenDatabaseCommand { get; set; }
        public ICommand OpenSearchWindowCommand { get; set; }
        public ICommand UpdateTableCommand { get; set; }
        public ICommand OpenUpdateProgramCommand { get; set; }
        public ICommand OpenVersionInfoCommand { get; set; }

        public ICommand CategoriesSelectionChangedCommand { get; set; }
        public ICommand RecipesListSelectionChangedCommand { get; set; }

        public ICommand OpenOverlayCommand { get; set; }

        public ICommand NavigateBackCommand { get; set; }
        public ICommand NavigateGoCommand { get; set; }
        public ICommand MaterialLinkCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        #endregion

        public MainWindowViewModel(MainWindowWindowService windowService, MainWindowModel model) : base(windowService, model)
        {
            _mainWindowService = windowService;
            _model = model;

            UnderMessageLabelText = new ReactiveProperty<string>();

            CanGoBack = new ReactiveProperty<bool>();
            CanGoForward = new ReactiveProperty<bool>();

            SearchText = new ReactiveProperty<string>(string.Empty);
            SearchText.PropertyChanged += (_, _) =>
            {
                model.NarrowDownRecipes(SearchText.Value);
            };

            SelectedCategoryIndex = model.ObserveProperty(m => m.SelectedCategoryIndex).ToReactiveProperty()
                .AddTo(CompositeDisposable);
            Categories = model.Categories.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            RecipesList = model.Recipes.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            SelectedRecipe = new ReactiveProperty<RecipeInfo?>();

            OpenSettingCommand = new DelegateCommand(OpenSetting);
            OpenDatabaseCommand = new DelegateCommand(OpenDatabase);
            OpenSearchWindowCommand = new DelegateCommand(OpenSearchWindow);
            UpdateTableCommand = new DelegateCommand(UpdateTable);
            OpenUpdateProgramCommand = new DelegateCommand(OpenUpdateProgram);
            OpenVersionInfoCommand = new DelegateCommand(OpenVersionInfo);
            CategoriesSelectionChangedCommand = new DelegateCommand<CategoryInfo?>(CategoriesSelectionChanged);
            RecipesListSelectionChangedCommand = new DelegateCommand<RecipeInfo>(RecipesListSelectionChanged);
            OpenOverlayCommand = new DelegateCommand(OpenOverlay);
            NavigateBackCommand = new DelegateCommand(NavigateBack);
            NavigateGoCommand = new DelegateCommand(NavigateGo);
            MaterialLinkCommand = new DelegateCommand<int?>(NavigateMaterialLink);
            OpenBrowserCommand = new DelegateCommand(OpenBrowser);
        }

        protected override void MainWindow_Loaded()
        {
            base.MainWindow_Loaded();

            Task.Factory.StartNew(async () =>
            {
                SetMessage("カテゴリー読み込み中...");

                _model.Initialize();

                if (SettingLoader.Instance.IsCheckDataUpdate)
                {
                    SetMessage("データベースの更新を確認中...");

                    await _model.CheckDatabaseUpdate().ContinueWith(_ =>
                    {
                        if (!_model.AvailableUpdate)
                            return;

                        var dr = WindowManageService.MessageBoxDispatchShow("データベースに更新があります。更新を行いますか？",
                            "データベースに更新があります",
                            ExMessageBoxBase.MessageType.Exclamation, ExMessageBoxBase.ButtonType.YesNo);

                        if (dr == ExMessageBoxBase.DialogResult.Yes)
                        {
                            WindowManageService.Dispatch(UpdateTable);
                        }
                    });
                }

                if (SettingLoader.Instance.IsCheckProgramUpdate)
                {
                    SetMessage("プログラムの更新を確認中...");

                    await CheckUpdate();
                }

                SetMessage();
            });
        }

        public void OpenSetting()
        {
            var model = new SettingModel();
            var vm = new SettingViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<SettingView>(vm);
        }

        public void OpenDatabase()
        {
            using var p = Process.Start("C:\\Valve\\DB Browser for SQLite\\DB Browser for SQLite.exe", Constants.DatabaseFileName);
        }

        public void OpenSearchWindow()
        {
            var model = new SearchWindowModel();
            var vm = new SearchWindowViewModel(new WindowService(), _mainWindowService, this, model, _model);
            WindowManageService.ShowNonOwner<SearchWindow>(vm);
        }

        public void UpdateTable()
        {
            var model = new TableDownloadModel(_model);
            var vm = new TableDownloadViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<TableDownloadView>(vm);

            _model.Reload();
        }

        public void OpenUpdateProgram()
        {
            var updFormModel = new UpdFormModel();
            var vm = new UpdFormViewModel(new WindowService(), updFormModel);
            WindowManageService.ShowNonOwner<UpdForm>(vm);
        }

        public void OpenVersionInfo()
        {
            var model = new VersionInfoModel();
            var vm = new VersionInfoViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<VersionInfo>(vm);
        }

        public void CategoriesSelectionChanged(CategoryInfo? category)
        {
            if (category == null)
                return;

            _model.SelectCategory(category);
            _model.NarrowDownRecipes(SearchText.Value);
        }

        public void RecipesListSelectionChanged(RecipeInfo? recipe)
        {
            if (recipe == null)
                return;
            
            SetMessage("レシピを読み込み中...");

            _model.SelectRecipe(recipe);
            SelectedRecipe.Value = recipe;

            _historyBack.Clear();
            _historyForward.Clear();

            SetEnabledNavigateButton();

            if (_mainWindowService.GaugeResize == null)
                return;

            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item1Amount, 0);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item2Amount, 1);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item3Amount, 2);

            SetMessage();
        }

        public void OpenOverlay()
        {
            if (_overlayModel != null && !_overlayModel.Closed)
            {
                _overlayModel.SelectedRecipe = SelectedRecipe.Value;

                return;
            }

            var windowService = new MainWindowWindowService();
            var model = new OverlayModel
            {
                SelectedRecipe = SelectedRecipe.Value
            };
            _overlayModel = model;
            _overlayWindowService = windowService;

            WindowManageService.ShowNonOwner<Overlay>(window =>
            {
                windowService.GaugeResize = window;
                windowService.MainWindow = _mainWindowService.MainWindow;
                var vm = new OverlayViewModel(windowService, model);
                return vm;
            });
        }

        public void NavigateBack()
        {
            if (_historyBack.Count <= 0 || SelectedRecipe.Value == null)
                return;

            var recipe = _historyBack.Pop();
            _historyForward.Push(SelectedRecipe.Value);
            SelectedRecipe.Value = recipe;

            SetEnabledNavigateButton();

            if (_mainWindowService.GaugeResize == null)
                return;

            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item1Amount, 0);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item2Amount, 1);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item3Amount, 2);
        }

        public void NavigateGo()
        {
            if (_historyForward.Count <= 0 || SelectedRecipe.Value == null)
                return;

            var recipe = _historyForward.Pop();
            _historyBack.Push(SelectedRecipe.Value);
            SelectedRecipe.Value = recipe;

            SetEnabledNavigateButton();

            if (_mainWindowService.GaugeResize == null)
                return;

            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item1Amount, 0);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item2Amount, 1);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item3Amount, 2);
        }

        public void NavigateMaterialLink(int? arg)
        {
            if (arg == null)
                return;

            var recipeId = arg.Value;

            var recipe = _model.GetRecipe(recipeId);

            if (recipe == null || SelectedRecipe.Value == null)
                return;

            _model.SelectRecipe(recipe);

            _historyBack.Push(SelectedRecipe.Value);
            _historyForward.Clear();
            SelectedRecipe.Value = recipe;

            SetEnabledNavigateButton();

            if (_mainWindowService.GaugeResize == null)
                return;

            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item1Amount, 0);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item2Amount, 1);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item3Amount, 2);
        }

        public void OpenBrowser()
        {
            if (SelectedRecipe.Value == null || string.IsNullOrEmpty(SelectedRecipe.Value.Url)) 
                return;

            var dr = WindowManageService.MessageBoxShow("選択したレシピをデフォルトブラウザで開きます。よろしいですか？", "ブラウザで開きます",
                ExMessageBoxBase.MessageType.Asterisk, ExMessageBoxBase.ButtonType.YesNo);

            if (dr == ExMessageBoxBase.DialogResult.No)
                return;

            var processInfo = new ProcessStartInfo
            {
                FileName = SelectedRecipe.Value.Url,
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        public void SetEnabledNavigateButton()
        {
            CanGoForward.Value = _historyForward.Any();
            CanGoBack.Value = _historyBack.Any();
        }

        public override void Dispose()
        {
            base.Dispose();

            _overlayWindowService?.Close();
            _model.Dispose();
        }

        private async Task CheckUpdate()
        {
            var availableUpdate = await _model.CheckUpdate();
            if (availableUpdate)
            {
                var dialogResult = _mainWindowService.MessageBoxDispatchShow("更新プログラムがあります。更新しますか？",
                    "更新プログラムがあります", ExMessageBoxBase.MessageType.Asterisk, ExMessageBoxBase.ButtonType.YesNo);
                if (dialogResult == ExMessageBoxBase.DialogResult.Yes)
                {
                    var updFormModel = new UpdFormModel();
                    await updFormModel.Initialize();

                    WindowManageService.Dispatch(() =>
                    {
                        var vm = new UpdFormViewModel(new WindowService(), updFormModel, true);
                        WindowManageService.Show<UpdForm>(vm);
                    });
                }
            }
        }

        private void SetMessage(string? message = null)
        {
            UnderMessageLabelText.Value = message ?? "準備完了";
        }
    }
}
