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
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Views;
using CookInformationViewer.Views.Searches;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels.Searchers
{
    public class SearchWindowViewModel : ViewModelWindowStyleBase
    {
        private readonly SearchWindowModel _model;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly MainWindowModel _mainWindowModel;

        public ReactiveProperty<string> SearchText { get; set; }
        public ReactiveProperty<bool> IsMaterialSearch { get; set; }
        public ReadOnlyReactiveCollection<SearchNode> SearchItems { get; set; }

        public ICommand SearchCommand { get; set; }
        public ICommand SearchSelectedItemChangedCommand { get; set; }

        public SearchWindowViewModel(IWindowService windowService, MainWindowViewModel mainWindowViewModel,
            SearchWindowModel model, MainWindowModel mainWindowModel) : base(windowService, model)
        {
            _model = model;
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindowModel = mainWindowModel;

            SearchText = new ReactiveProperty<string>();
            IsMaterialSearch = new ReactiveProperty<bool>();
            SearchItems = model.SearchItems.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);

            SearchCommand = new DelegateCommand(Search);
            SearchSelectedItemChangedCommand = new DelegateCommand<SearchNode?>(SearchSelectedItemChanged);
        }

        public void Search()
        {
            _model.Search(SearchText.Value, IsMaterialSearch.Value);
        }

        public void SearchSelectedItemChanged(SearchNode? args)
        {
            if (args == null || args.IsCategory)
                return;

            _mainWindowModel.SelectCategory(args);
            _mainWindowModel.SetSelectedCategoryIndex();

            var recipe = _mainWindowModel.GetRecipeInfo(args);
            if (recipe != null)
            {
                _mainWindowViewModel.RecipesListSelectionChanged(recipe);
                //_mainWindowModel.SetIsSelectedOnCurrentRecipe();
            }
        }
    }
}
