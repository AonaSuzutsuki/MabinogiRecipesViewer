using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.FestivalFood;
using CookInformationViewer.Models.Searchers;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels.FestivalFood
{
    public class FestivalFoodSimulatorViewModel : ViewModelWindowStyleBase
    {
        private readonly FestivalFoodSimulatorModel _model;

        public ReadOnlyReactiveCollection<FestivalFoodSearchStatusItem> Categories { get; set; }

        public ReadOnlyReactiveCollection<FestivalFoodRecipeHeader> RecipeHeaders { get; set; }

        public ReactiveProperty<FestivalFoodSearchStatusItem> SelectedCategory { get; set; }

        public ReactiveCollection<FestivalFoodQualityComboItem> QualityItems { get; set; }

        public ReactiveProperty<int> QualityItemsSelectedIndex { get; set; }

        public ReactiveProperty<string> SearchText { get; set; }

        public ReactiveProperty<EffectInfo> SelectedEffect { get; set; }

        public ICommand CategoriesSelectionChangedCommand { get; set; }
        public ICommand RecipesListSelectionChangedCommand { get; set; }
        public ICommand QualityItemsSelectionChangedCommand { get; set; }

        public FestivalFoodSimulatorViewModel(IWindowService windowService, FestivalFoodSimulatorModel model) : base(windowService, model)
        {
            _model = model;

            Categories = model.StatusNames.ToReadOnlyReactiveCollection();
            RecipeHeaders = model.RecipeHeaders.ToReadOnlyReactiveCollection();
            SelectedCategory = new ReactiveProperty<FestivalFoodSearchStatusItem>();
            SearchText = new ReactiveProperty<string>();
            QualityItems = new ReactiveCollection<FestivalFoodQualityComboItem>();
            QualityItemsSelectedIndex = new ReactiveProperty<int>();
            SelectedEffect = new ReactiveProperty<EffectInfo>();

            CategoriesSelectionChangedCommand = new DelegateCommand<FestivalFoodSearchStatusItem?>(CategoriesSelectionChanged);
            RecipesListSelectionChangedCommand = new DelegateCommand<FestivalFoodRecipeHeader?>(RecipesListSelectionChanged);
            QualityItemsSelectionChangedCommand = new DelegateCommand<FestivalFoodQualityComboItem?>(QualityItemsSelectionChanged);
        }

        protected override void MainWindow_Loaded()
        {
            base.MainWindow_Loaded();
        }

        public void CategoriesSelectionChanged(FestivalFoodSearchStatusItem? searchStatusItem)
        {
            if (searchStatusItem == null)
                return;

            if (searchStatusItem.SearchStatusItem  != null)
                _model.SearchRecipes(searchStatusItem.SearchStatusItem);
            else
                _model.SearchFavorite();
        }

        public void RecipesListSelectionChanged(FestivalFoodRecipeHeader? recipeHeader)
        {
            if (recipeHeader == null)
                return;

            var recipe = recipeHeader.Recipe;

            QualityItems.Clear();

            foreach (var pair in recipeHeader.StarEffectMap.OrderByDescending(x => x.Key))
            {
                var effects = pair.Value;

                QualityItems.Add(new FestivalFoodQualityComboItem
                {
                    Star = pair.Key,
                    Name = QualityItemInfo.GetStarString(pair.Key),
                    Effects = effects
                });
            }

            QualityItemsSelectedIndex.Value = 0;
        }

        public void QualityItemsSelectionChanged(FestivalFoodQualityComboItem? qualityItem)
        {
            if (qualityItem == null)
                return;

            SelectedEffect.Value = qualityItem.Effects.First();
        }
    }
}
