using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Views.WindowServices;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels
{
    public class ListUpdateHistoryViewModel : ViewModelWindowStyleBase
    {
        private readonly ListUpdateHistoryModel _model;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ReadOnlyReactiveCollection<UpdateHistoryItem> RecipeDateItems { get; set; }
        public ReadOnlyReactiveCollection<UpdateRecipeItem> Recipes { get; set; }

        public ICommand RecipeDateSelectionChangedCommand { get; set; }
        public ICommand RecipesSelectionChangedCommand { get; set; }

        public ListUpdateHistoryViewModel(IWindowService windowService, MainWindowViewModel mainWindowViewModel,
            ListUpdateHistoryModel model) : base(windowService, model)
        {
            _model = model;
            _mainWindowViewModel = mainWindowViewModel;

            RecipeDateItems = model.RecipeDateItems.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);
            Recipes = model.RecipeItems.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);

            RecipeDateSelectionChangedCommand = new DelegateCommand<UpdateHistoryItem?>(RecipeDateSelectionChanged);
            RecipesSelectionChangedCommand = new DelegateCommand<UpdateRecipeItem?>(RecipesSelectionChanged);
        }

        public void RecipeDateSelectionChanged(UpdateHistoryItem? item)
        {
            if (item == null)
                return;

            _model.GetRecipes(item.Date);
        }

        public void RecipesSelectionChanged(UpdateRecipeItem? item)
        {
            if (item == null)
                return;

            _mainWindowViewModel.SelectCategory(item);
            _mainWindowViewModel.SelectRecipe(item);
        }

        public override void Dispose()
        {
            base.Dispose();

            _model.Dispose();
        }
    }
}
