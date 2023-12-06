using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Xml.Linq;
using CommonExtensionLib.Extensions;
using CookInformationViewer.Models.DataValue;

namespace CookInformationViewer.ViewModels
{
    public class IgnoreEvent
    {
        private readonly HashSet<string> _ignorePropertyNames = new();

        public void Add(string propertyName)
        {
            if (_ignorePropertyNames.Contains(propertyName))
                return;

            _ignorePropertyNames.Add(propertyName);
        }

        public void Remove(string propertyName)
        {
            if (!_ignorePropertyNames.Contains(propertyName))
                return;

            _ignorePropertyNames.Remove(propertyName);
        }

        public bool CheckIgnore(string? propertyName)
        {
            if (propertyName == null)
                return false;

            var result = _ignorePropertyNames.Contains(propertyName);
            if (result)
            {
                Remove(propertyName);
            }

            return result;
        }
    }

    public class MainWindowViewModel : ViewModelWindowStyleBase
    {
        #region Fields

        private readonly MainWindowWindowService _mainWindowService;
        private readonly MainWindowModel _model;

        private OverlayModel? _overlayModel;
        private MainWindowWindowService? _overlayWindowService;

        private readonly Stack<RecipeInfo> _historyBack = new();
        private readonly Stack<RecipeInfo> _historyForward = new();

        private readonly Dictionary<string, bool> _previousTabSelectedMap = new();

        private bool _isMemoChanged;

        #endregion

        #region Properties

        public IgnoreEvent IgnoreEvent { get; } = new(); 

        public bool IsDebugMode => Constants.IsDebugMode;

        public ReactiveProperty<string> UnderMessageLabelText { get; set; }

        public ReactiveProperty<bool> CanGoBack { get; set; }
        public ReactiveProperty<bool> CanGoForward { get; set; }

        public ReactiveProperty<string> SearchText { get; set; }

        public ReadOnlyReactiveCollection<CategoryInfo> Categories { get; set; }
        public ReactiveProperty<CategoryInfo> SelectedCategory { get; set; }
        public ReactiveProperty<int> SelectedCategoryIndex { get; set; }

        public ReadOnlyReactiveCollection<RecipeHeader> RecipesList { get; set; }

        public ReactiveProperty<RecipeInfo?> SelectedRecipe { get; set; }

        public ReactiveProperty<bool> IsEffectSelected { get; set; }

        public ReactiveProperty<bool> IsMemoSelected { get; set; }

        #endregion

        #region Event Properties

        public ICommand OpenSettingCommand { get; set; }
        public ICommand OpenCalcMaterialsCommand { get; set; }
        public ICommand OpenDatabaseCommand { get; set; }
        public ICommand OpenSearchWindowCommand { get; set; }
        public ICommand UpdateTableCommand { get; set; }
        public ICommand OpenUpdateProgramCommand { get; set; }
        public ICommand OpenListUpdateHistoryCommand { get; set; }
        public ICommand OpenVersionInfoCommand { get; set; }

        public ICommand CategoriesSelectionChangedCommand { get; set; }
        public ICommand RecipesListSelectionChangedCommand { get; set; }

        public ICommand OpenOverlayCommand { get; set; }
        public ICommand FavoriteCommand { get; set; }

        public ICommand NavigateBackCommand { get; set; }
        public ICommand NavigateGoCommand { get; set; }
        public ICommand MaterialLinkCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        public ICommand BottomTabChangedCommand { get; set; }
        public ICommand MemoTextBoxLostFocusCommand { get; set; }
        public ICommand MemoTextBoxTextChangedCommand { get; set; }

        public ICommand CopyRecipeNameCommand { get; set; }

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

            SelectedCategoryIndex = model.ObserveProperty(m => m.SelectedCategoryIndex).ToReactiveProperty();
            
            SelectedCategory = new ReactiveProperty<CategoryInfo>();
            SelectedCategory.PropertyChangedAsObservable().Subscribe(x =>
            {
                if (IgnoreEvent.CheckIgnore(nameof(SelectedCategory)))
                    return;

                CategoriesSelectionChangedCommand?.Execute(SelectedCategory.Value);
            });
            Categories = model.Categories.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            Categories.ObserveAddChanged().Subscribe(item =>
            {
                if (item.IsSelected)
                {
                    SelectedCategory.Value = item;
                }

                item.IsSelected = false;
            });
            RecipesList = model.Recipes.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            SelectedRecipe = new ReactiveProperty<RecipeInfo?>();
            IsEffectSelected = new ReactiveProperty<bool>(true);
            IsMemoSelected = new ReactiveProperty<bool>();

            OpenSettingCommand = new DelegateCommand(OpenSetting);
            OpenCalcMaterialsCommand = new DelegateCommand(OpenCalcMaterials);
            OpenDatabaseCommand = new DelegateCommand(OpenDatabase);
            OpenSearchWindowCommand = new DelegateCommand(OpenSearchWindow);
            UpdateTableCommand = new DelegateCommand(UpdateTable);
            OpenUpdateProgramCommand = new DelegateCommand(OpenUpdateProgram);
            OpenListUpdateHistoryCommand = new DelegateCommand(OpenListUpdateHistory);
            OpenVersionInfoCommand = new DelegateCommand(OpenVersionInfo);
            CategoriesSelectionChangedCommand = new DelegateCommand<CategoryInfo?>(CategoriesSelectionChanged);
            RecipesListSelectionChangedCommand = new DelegateCommand<RecipeHeader>(RecipesListSelectionChanged);
            OpenOverlayCommand = new DelegateCommand(OpenOverlay);
            FavoriteCommand = new DelegateCommand(Favorite);
            NavigateBackCommand = new DelegateCommand(NavigateBack);
            NavigateGoCommand = new DelegateCommand(NavigateGo);
            MaterialLinkCommand = new DelegateCommand<int?>(NavigateMaterialLink);
            OpenBrowserCommand = new DelegateCommand(OpenBrowser);
            BottomTabChangedCommand = new DelegateCommand(BottomTabChanged);
            MemoTextBoxLostFocusCommand = new DelegateCommand<string?>(MemoTextBoxLostFocus);
            MemoTextBoxTextChangedCommand = new DelegateCommand(() => _isMemoChanged = true);
            CopyRecipeNameCommand = new DelegateCommand(CopyRecipeName);
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

        public void OpenCalcMaterials()
        {
            if (SelectedRecipe.Value == null)
                return;

            var model = new CalcMaterialsModel();
            var vm = new CalcMaterialsViewModel(new WindowService(), model, this);
            WindowManageService.Show<CalcMaterials>(vm);

            _ = model.Create(SelectedRecipe.Value);
        }

        public void OpenDatabase()
        {
            using var p = Process.Start("C:\\Valve\\DB Browser for SQLite\\DB Browser for SQLite.exe", Constants.DatabaseFileName);
        }

        public void OpenSearchWindow()
        {
            var model = new SearchWindowModel();
            var vm = new SearchWindowViewModel(new WindowService(), this, model);
            WindowManageService.ShowNonOwner<SearchWindow>(vm);
        }

        public void UpdateTable()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                WindowManageService.MessageBoxShow("ネットワークに未接続な状態でこの機能はしようできません。",
                    "ネットワーク未接続", ExMessageBoxBase.MessageType.Exclamation);
                return;
            }

            _model.CloseContext();

            var model = new TableDownloadModel(_model);
            var vm = new TableDownloadViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<TableDownloadView>(vm);

            _model.Reload();
        }

        public void OpenUpdateProgram()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                WindowManageService.MessageBoxShow("ネットワークに未接続な状態でこの機能はしようできません。",
                    "ネットワーク未接続", ExMessageBoxBase.MessageType.Exclamation);
                return;
            }

            var updFormModel = new UpdFormModel();
            var vm = new UpdFormViewModel(new WindowService(), updFormModel);
            WindowManageService.ShowNonOwner<UpdForm>(vm);
        }

        public void OpenListUpdateHistory()
        {
            var model = _model.CreateListUpdateHistoryModel();
            var vm = new ListUpdateHistoryViewModel(new WindowService(), this, model);
            WindowManageService.Show<ListUpdateHistory>(vm);
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

            if (category.SameFavorite())
                _model.SelectFavorite();
            else
                _model.SelectCategory(category);

            _model.NarrowDownRecipes(SearchText.Value);
        }

        public void RecipesListSelectionChanged(RecipeHeader? header)
        {
            if (header == null || header.IsHeader)
                return;
            
            SetMessage("レシピを読み込み中...");

            _model.SelectRecipe(header.Recipe);
            SelectedRecipe.Value = header.Recipe;

            _historyBack.Clear();
            _historyForward.Clear();

            ChangeRecipeCommon(header.Recipe);

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

        public void Favorite()
        {
            if (SelectedRecipe.Value == null)
                return;

            var recipe = SelectedRecipe.Value;

            if (!recipe.IsFavorite)
            {
                _model.RegisterFavorite(SelectedRecipe.Value);
            }
            else
            {
                _model.RemoveFavorite(recipe);
            }

            if (_model.CurrentCategoryInfo?.SameFavorite() ?? false)
                _model.SelectFavorite();
        }

        public void NavigateBack()
        {
            if (_historyBack.Count <= 0 || SelectedRecipe.Value == null)
                return;

            var recipe = _historyBack.Pop();
            _historyForward.Push(SelectedRecipe.Value);
            SelectedRecipe.Value = recipe;

            ChangeRecipeCommon(recipe);
        }

        public void NavigateGo()
        {
            if (_historyForward.Count <= 0 || SelectedRecipe.Value == null)
                return;

            var recipe = _historyForward.Pop();
            _historyBack.Push(SelectedRecipe.Value);
            SelectedRecipe.Value = recipe;

            ChangeRecipeCommon(recipe);
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

            ChangeRecipeCommon(recipe);
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

        public void SelectCategory(UpdateRecipeItem item)
        {
            var category = _model.SelectCategory(item);
            if (category == null)
                return;

            if (SelectedCategory.Value.Name != category.Name)
                IgnoreEvent.Add(nameof(SelectedCategory));

            SelectedCategory.Value = category;
        }

        public void SelectCategory(RecipeHeader recipeHeader)
        {
            var category = _model.SelectCategory(recipeHeader);
            if (category == null)
                return;

            if (SelectedCategory.Value.Name != category.Name)
                IgnoreEvent.Add(nameof(SelectedCategory));

            SelectedCategory.Value = category;
        }

        public void SelectRecipe(UpdateRecipeItem item)
        {
            var recipe = _model.GetRecipeInfo(item);
            if (recipe == null)
                return;

            RecipesListSelectionChanged(recipe);
            recipe.Recipe.IsSelected = true;

            if (_mainWindowService.MainWindow == null)
                return;

            _mainWindowService.MainWindow.IsSearched = true;
            _mainWindowService.MainWindow.ScrollItem();
            _mainWindowService.MainWindow.WindowFocus();
        }

        public void SelectRecipe(RecipeHeader recipeHeader)
        {
            var recipe = _model.GetRecipeInfo(recipeHeader);
            if (recipe == null)
                return;

            RecipesListSelectionChanged(recipe);
            recipe.Recipe.IsSelected = true;

            if (_mainWindowService.MainWindow == null)
                return;

            _mainWindowService.MainWindow.IsSearched = true;
            _mainWindowService.MainWindow.ScrollItem();
            _mainWindowService.MainWindow.WindowFocus();
        }

        public void MemoTextBoxLostFocus(string? text)
        {
            if (text == null || !_isMemoChanged)
                return;

            _model.SaveMemo(text);
            _isMemoChanged = false;
        }

        public void BottomTabChanged()
        {
            if (SelectedRecipe.Value == null)
                return;

            if (SelectedRecipe.Value.IsMaterial)
                return;

            // Save selected if no material is selected.
            _previousTabSelectedMap.Put(nameof(IsEffectSelected), IsEffectSelected.Value);
            _previousTabSelectedMap.Put(nameof(IsMemoSelected), IsMemoSelected.Value);

        }

        public void CopyRecipeName()
        {
            if (SelectedRecipe.Value == null)
                return;

            Clipboard.SetText(SelectedRecipe.Value.Name);

            SetMessage("レシピ名のコピーが完了しました。");

            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                WindowManageService.Dispatch(() => SetMessage());
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            _overlayWindowService?.Close();
            _model.Dispose();
        }

        private void ChangeRecipeCommon(RecipeInfo recipe)
        {
            SetEnabledNavigateButton();

            if (_mainWindowService.GaugeResize == null)
                return;

            _mainWindowService.GaugeResize.SetGaugeLength((double)recipe.Item1Amount, 0);
            _mainWindowService.GaugeResize.SetGaugeLength((double)recipe.Item2Amount, 1);
            _mainWindowService.GaugeResize.SetGaugeLength((double)recipe.Item3Amount, 2);

            SelectTabItem(recipe);
        }

        private void SelectTabItem(RecipeInfo recipe)
        {
            if (recipe.IsMaterial && IsEffectSelected.Value)
            {
                IsEffectSelected.Value = false;
                IsMemoSelected.Value = true;
            }
            else
            {
                IsEffectSelected.Value = _previousTabSelectedMap.Get(nameof(IsEffectSelected), true);
                IsMemoSelected.Value = _previousTabSelectedMap.Get(nameof(IsMemoSelected));
            }
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
