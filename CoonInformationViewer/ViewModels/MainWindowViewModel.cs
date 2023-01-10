﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using CookInformationViewer.Views;
using CookInformationViewer.Views.WindowService;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CookInformationViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private readonly MainWindowWindowService _mainWindowService;
        private readonly MainWindowModel _model;
        private readonly OverlayModel _overlayModel = new();

        private readonly Stack<RecipeInfo> _historyBack = new();
        private readonly Stack<RecipeInfo> _historyForward = new();

        #endregion

        #region Properties

        public ReactiveProperty<bool> CanGoBack { get; set; }
        public ReactiveProperty<bool> CanGoForward { get; set; }

        public ReactiveProperty<string> SearchText { get; set; }

        public ReadOnlyReactiveCollection<CategoryInfo> Categories { get; set; }

        public ReadOnlyReactiveCollection<RecipeInfo> RecipesList { get; set; }

        public ReactiveProperty<RecipeInfo?> SelectedRecipe { get; set; }

        #endregion

        #region Event Properties
        
        public ICommand UpdateTableCommand { get; set; }

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

            CanGoBack = new ReactiveProperty<bool>();
            CanGoForward = new ReactiveProperty<bool>();

            SearchText = new ReactiveProperty<string>(string.Empty);
            SearchText.PropertyChanged += (_, _) =>
            {
                model.NarrowDownRecipes(SearchText.Value);
            };

            Categories = model.Categories.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            RecipesList = model.Recipes.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            SelectedRecipe = new ReactiveProperty<RecipeInfo?>();

            UpdateTableCommand = new DelegateCommand(UpdateTable);
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

            _ = _model.Initialize().ContinueWith(_ =>
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

        public void UpdateTable()
        {
            var model = new TableDownloadModel(_model);
            var vm = new TableDownloadViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<TableDownloadView>(vm);

            _model.Reload();
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

            _model.SelectRecipe(recipe);
            SelectedRecipe.Value = recipe;
            _overlayModel.SelectedRecipe = recipe;

            _historyBack.Clear();
            _historyForward.Clear();

            SetEnabledNavigateButton();

            if (_mainWindowService.GaugeResize == null)
                return;

            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item1Amount, 0);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item2Amount, 1);
            _mainWindowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item3Amount, 2);
        }

        public void OpenOverlay()
        {
            var windowService = new MainWindowWindowService();
            WindowManageService.ShowNonOwner<Overlay>(window =>
            {
                windowService.GaugeResize = window;
                var vm = new OverlayViewModel(windowService, _overlayModel);
                return vm;
            });

            //if (windowService.GaugeResize == null)
            //    return;

            //windowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item1Amount, 0);
            //windowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item2Amount, 1);
            //windowService.GaugeResize.SetGaugeLength((double)SelectedRecipe.Value.Item3Amount, 2);
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

            _model.Dispose();
        }
    }
}
