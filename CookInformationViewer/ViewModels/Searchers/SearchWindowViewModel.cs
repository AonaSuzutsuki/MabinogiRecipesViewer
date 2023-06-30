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

        public List<SearchStatusItem> StatusItems { get; set; } = new List<SearchStatusItem>
        {
            new("体力", SearchStatusItem.SearchStatusEnum.Hp),
            new("マナ", SearchStatusItem.SearchStatusEnum.Mp),
            new("スタミナ", SearchStatusItem.SearchStatusEnum.Sp),
            new("Str", SearchStatusItem.SearchStatusEnum.Str),
            new("Int", SearchStatusItem.SearchStatusEnum.Int),
            new("Dex", SearchStatusItem.SearchStatusEnum.Dex),
            new("Will", SearchStatusItem.SearchStatusEnum.Will),
            new("Luck", SearchStatusItem.SearchStatusEnum.Luck),
            new("最大攻撃", SearchStatusItem.SearchStatusEnum.MaxDamage),
            new("最小攻撃", SearchStatusItem.SearchStatusEnum.MinDamage),
            new("魔法攻撃", SearchStatusItem.SearchStatusEnum.MagicDamage),
            new("防御", SearchStatusItem.SearchStatusEnum.Defense),
            new("保護", SearchStatusItem.SearchStatusEnum.Protection),
            new("魔法防御", SearchStatusItem.SearchStatusEnum.MagicDefense),
            new("魔法保護", SearchStatusItem.SearchStatusEnum.MagicProtection)
        };
        public ReactiveProperty<SearchStatusItem> SelectedStatusItem { get; set; }

        public ReactiveProperty<string> SearchText { get; set; }
        public ReactiveProperty<bool> IsMaterialSearch { get; set; }
        public ReactiveProperty<bool> IsStatusSearch { get; set; }
        public ReactiveProperty<bool> IgnoreNotFestival { get; set; }
        public ReadOnlyReactiveCollection<RecipeHeader> RecipesList { get; set; }

        public ICommand SearchCommand { get; set; }
        public ICommand SearchSelectedItemChangedCommand { get; set; }

        public SearchWindowViewModel(IWindowService windowService, MainWindowViewModel mainWindowViewModel,
            SearchWindowModel model) : base(windowService, model)
        {
            _model = model;
            _mainWindowViewModel = mainWindowViewModel;

            SelectedStatusItem = new ReactiveProperty<SearchStatusItem>();
            SearchText = new ReactiveProperty<string>();
            IsMaterialSearch = new ReactiveProperty<bool>();
            IsStatusSearch = new ReactiveProperty<bool>();
            IgnoreNotFestival = new ReactiveProperty<bool>();
            RecipesList = model.Recipes.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);

            SearchCommand = new DelegateCommand(Search);
            SearchSelectedItemChangedCommand = new DelegateCommand<RecipeHeader>(SearchSelectedItemChanged);
        }

        public void Search()
        {
            if (!IsStatusSearch.Value)
                _model.Search(SearchText.Value, IsMaterialSearch.Value);
            else
                _model.StatusSearch(SearchText.Value, IsMaterialSearch.Value, SelectedStatusItem.Value, IgnoreNotFestival.Value);
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
