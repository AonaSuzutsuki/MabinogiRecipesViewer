using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Views;
using CookInformationViewer.Views.Searches;
using CookInformationViewer.Views.WindowServices;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels.Searchers
{
    public class SearchWindowViewModel : ViewModelWindowStyleBase
    {
        private readonly SearchWindowModel _model;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ReactiveProperty<string> SearchText { get; set; }
        public ReactiveProperty<bool> IsMaterialSearch { get; set; }
        public ReadOnlyReactiveCollection<RecipeHeader> RecipesList { get; set; }

        public ICommand SearchCommand { get; set; }
        public ICommand SearchSelectedItemChangedCommand { get; set; }

        public SearchWindowViewModel(IWindowService windowService, MainWindowViewModel mainWindowViewModel,
            SearchWindowModel model) : base(windowService, model)
        {
            _model = model;
            _mainWindowViewModel = mainWindowViewModel;

            SearchText = new ReactiveProperty<string>();
            IsMaterialSearch = new ReactiveProperty<bool>();
            RecipesList = model.Recipes.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);

            SearchCommand = new DelegateCommand(Search);
            SearchSelectedItemChangedCommand = new DelegateCommand<RecipeHeader>(SearchSelectedItemChanged);
        }

        public void Search()
        {
            _model.Search(SearchText.Value, IsMaterialSearch.Value);
        }

        public void SearchSelectedItemChanged(RecipeHeader? recipeHeader)
        {
            if (recipeHeader == null || recipeHeader.Category.Id == 0)
                return;

            _mainWindowViewModel.SelectCategory(recipeHeader);

            _mainWindowViewModel.SelectRecipe(recipeHeader);
        }
    }
}
