using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Models.Settings;
using CookInformationViewer.Models.Updates;

namespace CookInformationViewer.Models
{
    public class MainWindowModel : ModelBase, IDisposable
    {

        private ContextManager _contextManager = new();
        private UpdateContextManager _updateContextManager = new();
        private readonly SettingLoader _setting = SettingLoader.Instance;

        private ObservableCollection<CategoryInfo> _categories = new();
        private ObservableCollection<RecipeHeader> _recipes = new();
        private List<RecipeHeader> _recipeBaseItems = new();
        private int _selectedCategoryIndex;

        private CategoryInfo? _currentCategoryInfo;
        private RecipeInfo? _currentRecipeInfo;


        #region Properties

        public ObservableCollection<CategoryInfo> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public CategoryInfo? CurrentCategoryInfo
        {
            get => _currentCategoryInfo;
            set => SetProperty(ref _currentCategoryInfo, value);
        }

        public ObservableCollection<RecipeHeader> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set => SetProperty(ref _selectedCategoryIndex, value);
        }

        public bool AvailableUpdate => _updateContextManager.AvailableUpdate;

        #endregion
        

        public void Initialize()
        {
            LoadCategories();
        }

        public async Task CheckDatabaseUpdate()
        {
            if (_setting.IsCheckDataUpdate && System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                await _updateContextManager.AvailableTableUpdateCheck();
            }
        }

        public async Task<bool> CheckUpdate()
        {
            if (_setting.IsCheckProgramUpdate && System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var availableUpdate = await UpdateManager.CheckCanUpdate(UpdateManager.GetUpdateClient());
                return availableUpdate;
            }
            return false;
        }

        public void CloseContext()
        {
            _contextManager.Dispose();
        }

        public void Reload()
        {
            _contextManager = new ContextManager();

            LoadCategories();

            if (CurrentCategoryInfo != null)
            {
                if (CurrentCategoryInfo.SameFavorite())
                {
                    SelectFavorite();
                    CurrentCategoryInfo.IsSelected = true;

                    return;
                }

                var category = _categories.FirstOrDefault(x => x.Id == CurrentCategoryInfo.Id);
                if (category != null)
                {
                    SelectCategory(category);
                    category.IsSelected = true;
                }
            }
        }

        public void LoadCategories()
        {
            var categories = _contextManager.GetCategories(x =>
            {
                return x.CookCategories.Where(m => !m.IsDelete && x.CookRecipes.Any(n => n.CategoryId == m.Id))
                    .OrderByDescending(m => m.SkillRank);
            });

            var categoryInfos = new List<CategoryInfo>
            {
                CategoryInfo.Favorite
            };
            
            categoryInfos.AddRange(categories.Select(x => new CategoryInfo
            {
                Id = x.Id,
                Name = x.Name
            }));

            Categories.Clear();
            Categories.AddRange(categoryInfos);
        }

        public void SelectCategory(CategoryInfo category)
        {
            CurrentCategoryInfo = category;
            var recipe = _contextManager.GetItem(x =>
            {
                return (from m in x.CookRecipes
                    join cat in x.CookCategories on m.CategoryId equals cat.Id
                    join mat1 in x.CookMaterials on m.Item1Id equals mat1.Id
                        into mat1Item
                    from subMat1 in mat1Item.DefaultIfEmpty()
                    join mat2 in x.CookMaterials on m.Item2Id equals mat2.Id
                        into mat2Item
                    from subMat2 in mat2Item.DefaultIfEmpty()
                    join mat3 in x.CookMaterials on m.Item3Id equals mat3.Id
                        into mat3Item
                    from subMat3 in mat3Item.DefaultIfEmpty()
                    join rec1 in x.CookRecipes on m.Item1RecipeId equals rec1.Id
                        into rec1Item
                    from subRec1 in rec1Item.DefaultIfEmpty()
                    join rec2 in x.CookRecipes on m.Item2RecipeId equals rec2.Id
                        into rec2Item
                    from subRec2 in rec2Item.DefaultIfEmpty()
                    join rec3 in x.CookRecipes on m.Item3RecipeId equals rec3.Id
                        into rec3Item
                    from subRec3 in rec3Item.DefaultIfEmpty()
                    join fav in x.Favorites on m.Id equals fav.RecipeId
                        into favItem
                    from subFav in favItem.DefaultIfEmpty()
                    where !m.IsDelete && m.CategoryId == category.Id
                    orderby m.Name
                    select new RecipeHeader(new RecipeInfo(m)
                    {
                        Category = new CategoryInfo
                        {
                            Id = cat.Id,
                            Name = cat.Name
                        },
                        Item1Name = subRec1.Name ?? subMat1.Name,
                        Item2Name = subRec2.Name ?? subMat2.Name,
                        Item3Name = subRec3.Name ?? subMat3.Name,
                        IsFavorite = subFav != null
                    })).ToList();
            });

            Recipes.Clear();
            Recipes.AddRange(recipe);

            _recipeBaseItems = new List<RecipeHeader>(recipe);
        }

        public void SelectFavorite()
        {
            CurrentCategoryInfo = Categories.First();

            var recipeIds = _contextManager.GetItem(x =>
                x.Favorites
                    .Where(m => !m.IsDelete)
                    .OrderBy(m => m.CreateDate)
                    .Select(m => m.RecipeId)
                );
            
            var recipe = _contextManager.GetItem(x =>
            {
                return (from m in x.CookRecipes
                        join cat in x.CookCategories on m.CategoryId equals cat.Id
                        join mat1 in x.CookMaterials on m.Item1Id equals mat1.Id
                            into mat1Item
                        from subMat1 in mat1Item.DefaultIfEmpty()
                        join mat2 in x.CookMaterials on m.Item2Id equals mat2.Id
                            into mat2Item
                        from subMat2 in mat2Item.DefaultIfEmpty()
                        join mat3 in x.CookMaterials on m.Item3Id equals mat3.Id
                            into mat3Item
                        from subMat3 in mat3Item.DefaultIfEmpty()
                        join rec1 in x.CookRecipes on m.Item1RecipeId equals rec1.Id
                            into rec1Item
                        from subRec1 in rec1Item.DefaultIfEmpty()
                        join rec2 in x.CookRecipes on m.Item2RecipeId equals rec2.Id
                            into rec2Item
                        from subRec2 in rec2Item.DefaultIfEmpty()
                        join rec3 in x.CookRecipes on m.Item3RecipeId equals rec3.Id
                            into rec3Item
                        from subRec3 in rec3Item.DefaultIfEmpty()
                        join fav in x.Favorites on m.Id equals fav.RecipeId
                            into favItem
                        from subFav in favItem.DefaultIfEmpty()
                        where !m.IsDelete && recipeIds.Contains(m.Id)
                        orderby cat.SkillRank descending, m.Name
                        select new RecipeHeader(new RecipeInfo(m)
                        {
                            Category = new CategoryInfo
                            {
                                Id = cat.Id,
                                Name = cat.Name
                            },
                            Item1Name = subRec1.Name ?? subMat1.Name,
                            Item2Name = subRec2.Name ?? subMat2.Name,
                            Item3Name = subRec3.Name ?? subMat3.Name,
                            IsFavorite = subFav != null
                        })).ToList();
            });

            var dict = recipe.GroupBy(x => x.Category.Name)
                .ToDictionary(x => x.Key, x => x.ToList());

            var recipes = new List<RecipeHeader>();
            foreach (var pair in dict)
            {
                recipes.Add(new RecipeHeader(pair.Key));
                recipes.AddRange(pair.Value);
            }

            Recipes.Clear();
            Recipes.AddRange(recipes);

            _recipeBaseItems = new List<RecipeHeader>(recipes);
        }

        public RecipeInfo? GetRecipe(int id)
        {
            var recipe = _contextManager.GetItem(x =>
            {
                var item = (from m in x.CookRecipes
                    join cat in x.CookCategories on m.CategoryId equals cat.Id 
                    join mat1 in x.CookMaterials on m.Item1Id equals mat1.Id
                        into mat1Item
                    from subMat1 in mat1Item.DefaultIfEmpty()
                    join mat2 in x.CookMaterials on m.Item2Id equals mat2.Id
                        into mat2Item
                    from subMat2 in mat2Item.DefaultIfEmpty()
                    join mat3 in x.CookMaterials on m.Item3Id equals mat3.Id
                        into mat3Item
                    from subMat3 in mat3Item.DefaultIfEmpty()
                    join rec1 in x.CookRecipes on m.Item1RecipeId equals rec1.Id
                        into rec1Item
                    from subRec1 in rec1Item.DefaultIfEmpty()
                    join rec2 in x.CookRecipes on m.Item2RecipeId equals rec2.Id
                        into rec2Item
                    from subRec2 in rec2Item.DefaultIfEmpty()
                    join rec3 in x.CookRecipes on m.Item3RecipeId equals rec3.Id
                        into rec3Item
                    from subRec3 in rec3Item.DefaultIfEmpty()
                    where m.Id == id
                    select new RecipeInfo(m)
                    {
                        Category = new CategoryInfo
                        {
                            Id = cat.Id,
                            Name = cat.Name
                        },
                        Item1Name = subRec1.Name ?? subMat1.Name,
                        Item2Name = subRec2.Name ?? subMat2.Name,
                        Item3Name = subRec3.Name ?? subMat3.Name
                    });
                return item.First();
            });
            return recipe;
        }

        public RecipeInfo SelectRecipe(RecipeInfo recipe)
        {
            _currentRecipeInfo = recipe;

            recipe.Item1Locations = GetMaterialLocation(recipe.Item1Id, recipe.Item1RecipeId);
            recipe.Item2Locations = GetMaterialLocation(recipe.Item2Id, recipe.Item2RecipeId);
            recipe.Item3Locations = GetMaterialLocation(recipe.Item3Id, recipe.Item3RecipeId);

            SetEffects(recipe);
            SetAdditional(recipe);
            SetFavorite(recipe);

            return recipe;
        }

        public void NarrowDownRecipes(string searchWord)
        {
            if (string.IsNullOrEmpty(searchWord))
            {
                Recipes.Clear();
                Recipes.AddRange(_recipeBaseItems);
                return;
            }

            var narrowDownItems = _recipeBaseItems.Where(x => x.Recipe.Name.Contains(searchWord));

            Recipes.Clear();
            Recipes.AddRange(narrowDownItems);
        }

        public IEnumerable<LocationItemInfo> GetMaterialLocation(int? materialId, int? recipeId)
        {
            var locations = new List<LocationItemInfo>();
            if (recipeId != null)
            {
                locations.Add(new LocationItemInfo
                {
                    Name = "料理",
                    Location = "スキル",
                    Type = "クラフト"
                });
            }

            if (materialId == null)
                return locations;

            var sellers = _contextManager.GetItem(x =>
            {
                var items = from m in x.CookMaterialSellers
                    join seller in x.CookSellers on m.SellerId equals seller.Id
                    join location in x.CookLocations on seller.LocationId equals location.Id
                    where m.MaterialId == materialId
                    orderby seller.Name
                    select new LocationItemInfo
                    {
                        Name = seller.Name,
                        Location = location.Name,
                        Type = "NPC"
                    };
                return items;
            });
            locations.AddRange(sellers);

            var drops = _contextManager.GetItem(x =>
            {
                var items = from m in x.CookMaterialDrops
                    join location in x.CookLocations on m.LocationId equals location.Id
                    where m.MaterialId == materialId
                    orderby m.DropName
                    select new LocationItemInfo
                    {
                        Name = m.DropName,
                        Location = location.Name,
                        Type = "ドロップ"
                    };
                return items;
            });

            locations.AddRange(drops);

            return locations;
        }

        private void SetEffects(RecipeInfo recipe)
        {
            var effects = _contextManager.GetItem(x => (from m in x.CookEffects
                where m.RecipeId == recipe.Id
                select m)).ToList();

            var max = effects.MaxBy(x => x.Star);
            recipe.Star = max?.Star ?? 0;

            var effectDictionary = effects.OrderByDescending(x => x.Star)
                .GroupBy(x => x.Star)
                .ToDictionary(x => x.Key, x => x.ToList());

            var result = new Dictionary<int, QualityItemInfo>();
            foreach (var (star, list) in effectDictionary)
            {
                var item = new QualityItemInfo(star, list);
                result.Put(star, item);
            }

            recipe.Effects = result.Values;
        }

        private void SetAdditional(RecipeInfo recipe)
        {
            var item = _contextManager.GetItem(x => (from m in x.Additionals
                where m.RecipeId == recipe.Id
                select m)).FirstOrDefault();

            if (item == null)
                return;

            var isMaterials = item.IsMaterial == 1;
            var isNotFestival = item.NotFestival == 1;

            recipe.IsMaterial = isMaterials;
            recipe.IsNotFestival = isNotFestival;
            recipe.Special = item.Special;
        }

        private void SetFavorite(RecipeInfo recipe)
        {
            var favorite = _contextManager.GetItem(x =>
                x.Favorites.FirstOrDefault(m => m.RecipeId == recipe.Id));

            if (favorite == null)
                return;

            recipe.IsFavorite = true;
        }

        public RecipeHeader? GetRecipeInfo(SearchNode node)
        {
            var recipe = Recipes.FirstOrDefault(x => x.Recipe.Name == node.Name);

            return recipe;
        }

        public RecipeHeader? GetRecipeInfo(UpdateRecipeItem item)
        {
            var recipe = Recipes.FirstOrDefault(x => x.Recipe.Name == item.Name);

            return recipe;
        }

        public CategoryInfo? SelectCategory(SearchNode node)
        {
            var parent = node.Parent;
            if (parent == null)
                return null;

            var categoryInfo = Categories.FirstOrDefault(x => x.Name == parent.Name);
            if (categoryInfo == null)
                return null;

            SelectCategory(categoryInfo);
            return categoryInfo;
        }

        public CategoryInfo? SelectCategory(UpdateRecipeItem item)
        {
            var categoryInfo = Categories.FirstOrDefault(x => x.Name == item.CategoryName);
            if (categoryInfo == null)
                return null;

            SelectCategory(categoryInfo);
            return categoryInfo;
        }

        public void RegisterFavorite(RecipeInfo recipe)
        {
            _contextManager.Apply(x =>
            {
                var favorite = new DbCookFavorites
                {
                    RecipeId = recipe.Id,
                    CreateDate = DateTime.Now
                };

                x.Favorites.Add(favorite);
            });

            recipe.IsFavorite = true;
        }

        public void RemoveFavorite(RecipeInfo recipe)
        {
            var favorite = _contextManager.GetItem(x =>
                x.Favorites.FirstOrDefault(m => m.RecipeId == recipe.Id));

            if (favorite == null)
                return;

            _contextManager.Apply(x => x.Favorites.Remove(favorite));

            recipe.IsFavorite = false;
        }

        public ListUpdateHistoryModel CreateListUpdateHistoryModel()
        {
            return new ListUpdateHistoryModel(_contextManager);
        }

        public void Dispose()
        {
            _contextManager.Dispose();
            _updateContextManager.Dispose();
            _setting.Save();
        }
    }
}
