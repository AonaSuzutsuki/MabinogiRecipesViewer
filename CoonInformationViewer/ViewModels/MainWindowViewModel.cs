using System;
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

        #endregion

        #region Properties

        public ReadOnlyReactiveCollection<CategoryInfo> Categories { get; set; }

        public ReadOnlyReactiveCollection<RecipeInfo> RecipesList { get; set; }

        public ReactiveProperty<RecipeInfo> SelectedRecipe { get; set; }

        #endregion

        #region Event Properties

        public ICommand RebuildDataBaseCommand { get; set; }
        public ICommand UpdateTableCommand { get; set; }

        public ICommand CategoriesSelectionChangedCommand { get; set; }
        public ICommand RecipesListSelectionChangedCommand { get; set; }

        public ICommand OpenOverlayCommand { get; set; }

        #endregion

        public MainWindowViewModel(MainWindowWindowService windowService, MainWindowModel model) : base(windowService, model)
        {
            _mainWindowService = windowService;
            _model = model;

            Categories = model.Categories.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            RecipesList = model.Recipes.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            SelectedRecipe = new ReactiveProperty<RecipeInfo>();

            RebuildDataBaseCommand = new DelegateCommand(RebuildDataBase);
            UpdateTableCommand = new DelegateCommand(UpdateTable);
            CategoriesSelectionChangedCommand = new DelegateCommand<CategoryInfo?>(CategoriesSelectionChanged);
            RecipesListSelectionChangedCommand = new DelegateCommand<RecipeInfo>(RecipesListSelectionChanged);
            OpenOverlayCommand = new DelegateCommand(OpenOverlay);
        }

        protected override void MainWindow_Loaded()
        {
            base.MainWindow_Loaded();

            _ = _model.Initialize().ContinueWith(_ =>
            {
                if (!_model.AvailableUpdate)
                    return;

                var dr = WindowManageService.MessageBoxDispatchShow("Available Data Update. Do you want to update?",
                    "Available Data Update",
                    ExMessageBoxBase.MessageType.Exclamation, ExMessageBoxBase.ButtonType.YesNo);

                if (dr == ExMessageBoxBase.DialogResult.Yes)
                {
                    WindowManageService.Dispatch(UpdateTable);
                }
            });
        }

        public void RebuildDataBase()
        {
            _ = _model.RebuildDataBase().ContinueWith(t =>
            {
                var exp = t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);
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
        }

        public void RecipesListSelectionChanged(RecipeInfo? recipe)
        {
            if (recipe == null)
                return;

            SelectedRecipe.Value = _model.SelectRecipe(recipe);
            _overlayModel.SelectedRecipe = SelectedRecipe.Value;

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

        public override void Dispose()
        {
            base.Dispose();

            _model.Dispose();
        }
    }
}
