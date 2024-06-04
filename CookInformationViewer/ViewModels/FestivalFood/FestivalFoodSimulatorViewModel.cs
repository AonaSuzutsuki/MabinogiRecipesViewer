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
using CookInformationViewer.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.FestivalFood;
using CookInformationViewer.Models.Searchers;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels.FestivalFood
{
    public class FestivalFoodSettingItem : BindableBase
    {
        private RecipeInfo? _recipeInfo;

        public EffectInfo? EffectInfo { get; set; }

        public List<FestivalFoodQualityComboItem> EffectComboItem { get; set; } = new();

        public RecipeInfo? RecipeInfo
        {
            get => _recipeInfo;
            set => SetProperty(ref _recipeInfo, value);
        }
    }

    public class FestivalFoodSimulatorViewModel : ViewModelWindowStyleBase
    {
        private readonly FestivalFoodSimulatorModel _model;
        
        public MainWindowModel? MainModel { get; set; }

        public ReadOnlyReactiveCollection<FestivalFoodSearchStatusItem> Categories { get; set; }

        public ReadOnlyReactiveCollection<FestivalFoodRecipeHeader> RecipeHeaders { get; set; }

        public ReactiveProperty<FestivalFoodSearchStatusItem> SelectedCategory { get; set; }

        public ReactiveCollection<FestivalFoodQualityComboItem> QualityItems { get; set; }

        public ReactiveProperty<int> QualityItemsSelectedIndex { get; set; }

        public ReactiveProperty<string> SearchText { get; set; }

        public ReactiveProperty<EffectInfo> SelectedEffect { get; set; }

        public ReactiveProperty<RecipeInfo> SelectedRecipe { get; set; }

        public ReactiveProperty<bool> AddRecipeToFestivalEnabled { get; set; }

        public ReactiveProperty<bool> IsApplyFoodMastery { get; set; }

        public ReactiveProperty<EffectInfo> TotalEffect { get; set; }

        public ReactiveCollection<FestivalFoodSettingItem> SettingItems { get; set; }

        public ReactiveProperty<FestivalFoodSettingItem> SettingItem1 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem2 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem3 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem4 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem5 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem6 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem7 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem8 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem9 { get; set; }
        public ReactiveProperty<FestivalFoodSettingItem> SettingItem10 { get; set; }

        public ICommand CategoriesSelectionChangedCommand { get; set; }
        public ICommand RecipesListSelectionChangedCommand { get; set; }
        public ICommand QualityItemsSelectionChangedCommand { get; set; }
        public ICommand AddRecipeToFestivalCommand { get; set; }
        public ICommand RemoveFestivalFoodCommand { get; set; }
        public ICommand FavoriteCommand { get; set; }

        public FestivalFoodSimulatorViewModel(IWindowService windowService, FestivalFoodSimulatorModel model) : base(windowService, model)
        {
            _model = model;

            Categories = model.StatusNames.ToReadOnlyReactiveCollection();
            RecipeHeaders = model.RecipeHeaders.ToReadOnlyReactiveCollection();
            SelectedCategory = new ReactiveProperty<FestivalFoodSearchStatusItem>();
            SearchText = new ReactiveProperty<string>();
            SearchText.PropertyChanged += (_, _) =>
            {
                model.NarrowDownRecipes(SearchText.Value);
            };

            QualityItems = new ReactiveCollection<FestivalFoodQualityComboItem>();
            QualityItemsSelectedIndex = new ReactiveProperty<int>();
            SelectedEffect = new ReactiveProperty<EffectInfo>();
            TotalEffect = new ReactiveProperty<EffectInfo>();
            SettingItems = new ReactiveCollection<FestivalFoodSettingItem>();
            AddRecipeToFestivalEnabled = new ReactiveProperty<bool>();
            IsApplyFoodMastery = new ReactiveProperty<bool>(true);
            IsApplyFoodMastery.PropertyChanged += (sender, args) =>
            {
                ApplySettingItems();
            };
            SelectedRecipe = new ReactiveProperty<RecipeInfo>();

            foreach (var i in Enumerable.Range(0, 10))
            {
                SettingItems.Add(new FestivalFoodSettingItem());
            }

            SettingItem1 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem2 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem3 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem4 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem5 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem6 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem7 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem8 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem9 = new ReactiveProperty<FestivalFoodSettingItem>();
            SettingItem10 = new ReactiveProperty<FestivalFoodSettingItem>();

            CategoriesSelectionChangedCommand = new DelegateCommand<FestivalFoodSearchStatusItem?>(CategoriesSelectionChanged);
            RecipesListSelectionChangedCommand = new DelegateCommand<FestivalFoodRecipeHeader?>(RecipesListSelectionChanged);
            QualityItemsSelectionChangedCommand = new DelegateCommand<FestivalFoodQualityComboItem?>(QualityItemsSelectionChanged);
            AddRecipeToFestivalCommand = new DelegateCommand(AddRecipeToFestival);
            RemoveFestivalFoodCommand = new DelegateCommand<string?>(RemoveFestivalFood);
            FavoriteCommand = new DelegateCommand(Favorite);
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

            SelectedRecipe.Value = recipeHeader.Recipe;

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
            AddRecipeToFestivalEnabled.Value = true;
        }

        public void QualityItemsSelectionChanged(FestivalFoodQualityComboItem? qualityItem)
        {
            if (qualityItem == null)
                return;

            SelectedEffect.Value = qualityItem.Effects.First();
        }

        public void AddRecipeToFestival()
        {
            var qualityItem = QualityItems[QualityItemsSelectedIndex.Value];


            var item = SettingItems.FirstOrDefault(x => x.RecipeInfo == null);

            if (item == null)
                return;

            item.RecipeInfo = SelectedRecipe.Value;
            item.EffectComboItem = new List<FestivalFoodQualityComboItem>(QualityItems);
            item.EffectInfo = qualityItem.Effects.First();

            ApplySettingItems();
        }

        public void RemoveFestivalFood(string? indexText)
        {
            if (indexText == null)
                return;

            if (!int.TryParse(indexText, out var index))
                return;

            SettingItems[index].RecipeInfo = null;
            SettingItems[index].EffectInfo = null;
            SettingItems[index].EffectComboItem.Clear();

            ApplySettingItems();
        }

        public void Favorite()
        {
            if (!SelectedRecipe.Value.IsFavorite)
                _model.RegisterFavorite(SelectedRecipe.Value);
            else
                _model.RemoveFavorite(SelectedRecipe.Value);

            if (SelectedCategory.Value.Name == "お気に入り")
                _model.SearchFavorite();

            if (MainModel?.CurrentCategoryInfo?.SameFavorite() ?? false)
                MainModel.SelectFavorite();

            MainModel?.ChangeFavoriteFromRecipe(SelectedRecipe.Value);
        }

        private void ApplySettingItems()
        {
            SettingItem1.Value = SettingItems[0];
            SettingItem2.Value = SettingItems[1];
            SettingItem3.Value = SettingItems[2];
            SettingItem4.Value = SettingItems[3];
            SettingItem5.Value = SettingItems[4];
            SettingItem6.Value = SettingItems[5];
            SettingItem7.Value = SettingItems[6];
            SettingItem8.Value = SettingItems[7];
            SettingItem9.Value = SettingItems[8];
            SettingItem10.Value = SettingItems[9];

            TotalEffect.Value = new EffectInfo();

            foreach (var item in SettingItems)
            {
                TotalEffect.Value.Hp += item.EffectInfo?.Hp ?? 0;
                TotalEffect.Value.Mana += item.EffectInfo?.Mana ?? 0;
                TotalEffect.Value.Stamina += item.EffectInfo?.Stamina ?? 0;
                TotalEffect.Value.Str += item.EffectInfo?.Str ?? 0;
                TotalEffect.Value.Dex += item.EffectInfo?.Dex ?? 0;
                TotalEffect.Value.Int += item.EffectInfo?.Int ?? 0;
                TotalEffect.Value.Will += item.EffectInfo?.Will ?? 0;
                TotalEffect.Value.Luck += item.EffectInfo?.Luck ?? 0;
                TotalEffect.Value.MinDamage += item.EffectInfo?.MinDamage ?? 0;
                TotalEffect.Value.Damage += item.EffectInfo?.Damage ?? 0;
                TotalEffect.Value.MagicDamage += item.EffectInfo?.MagicDamage ?? 0;
                TotalEffect.Value.Defense += item.EffectInfo?.Defense ?? 0;
                TotalEffect.Value.Protection += item.EffectInfo?.Protection ?? 0;
                TotalEffect.Value.MagicDefense += item.EffectInfo?.MagicDefense ?? 0;
                TotalEffect.Value.MagicProtection += item.EffectInfo?.MagicProtection ?? 0;
            }

            if (IsApplyFoodMastery.Value)
            {
                TotalEffect.Value.Hp += TotalEffect.Value.Hp > 0 ? 10 : 0;
                TotalEffect.Value.Mana += TotalEffect.Value.Mana > 0 ? 10 : 0;
                TotalEffect.Value.Stamina += TotalEffect.Value.Stamina > 0 ? 10 : 0;
                TotalEffect.Value.Str += TotalEffect.Value.Str > 0 ? 10 : 0;
                TotalEffect.Value.Dex += TotalEffect.Value.Dex > 0 ? 10 : 0;
                TotalEffect.Value.Int += TotalEffect.Value.Int > 0 ? 10 : 0;
                TotalEffect.Value.Will += TotalEffect.Value.Will > 0 ? 10 : 0;
                TotalEffect.Value.Luck += TotalEffect.Value.Luck > 0 ? 10 : 0;
                TotalEffect.Value.MinDamage += TotalEffect.Value.MinDamage > 0 ? 10 : 0;
                TotalEffect.Value.Damage += TotalEffect.Value.Damage > 0 ? 10 : 0;
                TotalEffect.Value.MagicDamage += TotalEffect.Value.MagicDamage > 0 ? 10 : 0;
                TotalEffect.Value.Defense += TotalEffect.Value.Defense > 0 ? 10 : 0;
                TotalEffect.Value.Protection += TotalEffect.Value.Protection > 0 ? 10 : 0;
                TotalEffect.Value.MagicDefense += TotalEffect.Value.MagicDefense > 0 ? 10 : 0;
                TotalEffect.Value.MagicProtection += TotalEffect.Value.MagicProtection > 0 ? 10 : 0;
            }
        }
    }
}
