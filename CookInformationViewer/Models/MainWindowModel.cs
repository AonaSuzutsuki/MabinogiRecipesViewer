using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.Converters;
using CookInformationViewer.Models.Db;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Models.Settings;
using CookInformationViewer.Models.Updates;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prism.Mvvm;
using SavannahXmlLib.XmlWrapper;
using UpdateLib.Http;
using UpdateLib.Update;
using Brush = System.Windows.Media.Brush;

namespace CookInformationViewer.Models
{
    public class CategoryInfo : BindableBase
    {
        public static CategoryInfo Favorite => new()
        {
            Id = 0,
            Name = FavoriteContent
        };

        public const string FavoriteContent = "お気に入り";

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Brush Foreground => SameFavorite() ? Constants.FavoriteForeground : new SolidColorBrush(Colors.White);

        public bool SameFavorite()
        {
            return Name == FavoriteContent;
        }
    }

    public class RecipeInfo : BindableBase
    {
        private bool _isSelected;
        private bool _isFavorite;
        private Brush _foreground = new SolidColorBrush(Colors.White);

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Item1Name { get; set; }
        public string? Item2Name { get; set; }
        public string? Item3Name { get; set; }

        public bool IsItem1Material => Item1RecipeId == null;
        public bool IsItem2Material => Item2RecipeId == null;
        public bool IsItem3Material => Item3RecipeId == null;

        public int? Item1Id { get; set; }
        
        public int? Item2Id { get; set; }
        
        public int? Item3Id { get; set; }
        
        public int? Item1RecipeId { get; set; }
        
        public int? Item2RecipeId { get; set; }
        
        public int? Item3RecipeId { get; set; }
        
        public decimal Item1Amount { get; set; }
        
        public decimal Item2Amount { get; set; }
        
        public decimal Item3Amount { get; set; }

        public CategoryInfo? Category { get; set; }

        public IEnumerable<LocationItemInfo>? Item1Locations { get; set; }
        public IEnumerable<LocationItemInfo>? Item2Locations { get; set; }
        public IEnumerable<LocationItemInfo>? Item3Locations { get; set; }

        public DateTime? UpdateDate { get; set; }
        public BitmapImage? Image { get; set; }

        public bool IsMaterial { get; set; }

        public bool IsNotFestival { get; set; }

        public int Star { get; set; }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                Foreground = value ? Constants.FavoriteForeground : new SolidColorBrush(Colors.White);
                SetProperty(ref _isFavorite, value);
            }
        }

        public string StarText
        {
            get
            {
                if (IsMaterial)
                    return "料理素材 専用 (食べられないよ)";

                return Star switch
                {
                    1 => "★☆☆☆☆",
                    2 => "★★☆☆☆",
                    3 => "★★★☆☆",
                    4 => "★★★★☆",
                    5 => "★★★★★",
                    6 => "★★★★★",
                    _ => "未確認"
                };
            }
        }

        public string StarMessage
        {
            get
            {
                var text = Star switch
                {
                    1 => "確認済み",
                    2 => "確認済み",
                    3 => "確認済み",
                    4 => "究極の料理 確認済み",
                    5 => "天国の料理 確認済み",
                    6 => "最高の料理 確認済み",
                    _ => ""
                };

                if (IsNotFestival)
                    text = $"{text} フェスティバルフード不可";

                return text;
            }
        }

        public Brush StarBrush
        {
            get
            {
                return Star switch
                {
                    6 => Constants.StarSixForeground,
                    _ => new SolidColorBrush(Colors.White)
                };
            }
        }

        public Brush Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        public string Url { get; set; }

        public IEnumerable<QualityItemInfo>? Effects { get; set; }

        public RecipeInfo(DbCookRecipes recipe)
        {
            Id = recipe.Id;
            Name = recipe.Name;
            Item1Id = recipe.Item1Id;
            Item2Id = recipe.Item2Id;
            Item3Id = recipe.Item3Id;
            Item1RecipeId = recipe.Item1RecipeId;
            Item2RecipeId = recipe.Item2RecipeId;
            Item3RecipeId = recipe.Item3RecipeId;
            Item1Amount = recipe.Item1Amount;
            Item2Amount = recipe.Item2Amount;
            Item3Amount = recipe.Item3Amount;
            UpdateDate = recipe.UpdateDate ?? recipe.CreateDate;

            Url = $"https://mabicook.aonsztk.xyz/Home/Recipe?recipeId={Id}";

            if (recipe.ImageData == null)
            {
                Image = ImageLoader.GetNoImage();
            }
            else
            {
                using var ms = new MemoryStream(recipe.ImageData);
                Image = ImageLoader.CreateBitmapImage(ms, 80, 80);
            }

        }
    }

    public class LocationItemInfo
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
    }

    public class QualityItemInfo
    {
        public int Star { get; set; }
        public string Quality { get; set; }
        public System.Windows.Media.Brush StarBrush
        {
            get
            {
                return Star switch
                {
                    6 => Constants.StarSixForeground,
                    _ => new SolidColorBrush(Colors.White)
                };
            }
        }

        public string Hp { get; set; }
        public string Mp { get; set; }
        public string Sp { get; set; }
        public string Str { get; set; }
        public string Int { get; set; }
        public string Dex { get; set; }
        public string Will { get; set; }
        public string Luck { get; set; }
        public string MaxDamage { get; set; }
        public string MinDamage { get; set; }
        public string MagicDamage { get; set; }
        public string Protection { get; set; }
        public string Defense { get; set; }
        public string MagicProtection { get; set; }
        public string MagicDefense { get; set; }

        public QualityItemInfo(int star, IEnumerable<DbCookEffects> effects)
        {
            var list = effects.ToList();

            Star = star;
            Quality = star switch
            {
                1 => "★☆☆☆☆",
                2 => "★★☆☆☆",
                3 => "★★★☆☆",
                4 => "★★★★☆",
                5 => "★★★★★",
                6 => "★★★★★",
                _ => "",
            };

            Hp = MinMaxString(list, x => x.Hp);
            Mp = MinMaxString(list, x => x.Mana);
            Sp = MinMaxString(list, x => x.Stamina);
            Str = MinMaxString(list, x => x.Str);
            Int = MinMaxString(list, x => x.Int);
            Dex = MinMaxString(list, x => x.Dex);
            Will = MinMaxString(list, x => x.Will);
            Luck = MinMaxString(list, x => x.Luck);
            MaxDamage = MinMaxString(list, x => x.Damage);
            MinDamage = MinMaxString(list, x => x.MinDamage);
            MagicDamage = MinMaxString(list, x => x.MagicDamage);
            Protection = MinMaxString(list, x => x.Protection);
            Defense = MinMaxString(list, x => x.Defense);
            MagicProtection = MinMaxString(list, x => x.MagicProtection);
            MagicDefense = MinMaxString(list, x => x.MagicDefense);
        }

        private string MinMaxString(List<DbCookEffects> effects, Func<DbCookEffects, int> selector)
        {
            var min = effects.MinBy(selector);
            var max = effects.MaxBy(selector);

            if (min == null || max == null)
                return string.Empty;

            var minValue = selector(min);
            var maxValue = selector(max);

            if (minValue == 0 || maxValue == 0)
                return string.Empty;
            
            return minValue.Equals(maxValue) ? minValue.ToString() : $"{minValue}-{maxValue}";
        }
    }

    public class MainWindowModel : ModelBase, IDisposable
    {

        private UpdateContextManager _contextManager = new();
        private readonly SettingLoader _setting = SettingLoader.Instance;

        private ObservableCollection<CategoryInfo> _categories = new();
        private ObservableCollection<RecipeInfo> _recipes = new();
        private List<RecipeInfo> _recipeBaseItems = new();
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

        public ObservableCollection<RecipeInfo> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set => SetProperty(ref _selectedCategoryIndex, value);
        }

        public bool AvailableUpdate => _contextManager.AvailableUpdate;

        #endregion
        

        public void Initialize()
        {
            LoadCategories();
        }

        public async Task CheckDatabaseUpdate()
        {
            if (_setting.IsCheckDataUpdate)
            {
                await _contextManager.AvailableTableUpdateCheck();
            }
        }

        public async Task<bool> CheckUpdate()
        {
            if (_setting.IsCheckProgramUpdate)
            {
                var availableUpdate = await UpdateManager.CheckCanUpdate(UpdateManager.GetUpdateClient());
                return availableUpdate;
            }
            return false;
        }

        public void Reload()
        {
            _contextManager.Dispose();
            _contextManager = new UpdateContextManager();

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
            var recipe = _contextManager.GetItems(x =>
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
                    select new RecipeInfo(m)
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
                    }).ToList();
            });

            Recipes.Clear();
            Recipes.AddRange(recipe);

            _recipeBaseItems = new List<RecipeInfo>(recipe);
        }

        public void SelectFavorite()
        {
            CurrentCategoryInfo = Categories.First();

            var recipeIds = _contextManager.GetRecipe(x =>
                x.Favorites
                    .Where(m => !m.IsDelete)
                    .OrderBy(m => m.CreateDate)
                    .Select(m => m.RecipeId)
                );
            
            var recipe = _contextManager.GetItems(x =>
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
                        orderby subFav.CreateDate
                        select new RecipeInfo(m)
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
                        }).ToList();
            });

            Recipes.Clear();
            Recipes.AddRange(recipe);

            _recipeBaseItems = new List<RecipeInfo>(recipe);
        }

        public RecipeInfo? GetRecipe(int id)
        {
            var recipe = _contextManager.GetRecipe(x =>
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

            var narrowDownItems = _recipeBaseItems.Where(x => x.Name.Contains(searchWord));

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

            var sellers = _contextManager.GetItems(x =>
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

            var drops = _contextManager.GetItems(x =>
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
            var effects = _contextManager.GetItems(x => (from m in x.CookEffects
                where m.RecipeId == recipe.Id
                select m));

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
            var item = _contextManager.GetItems(x => (from m in x.Additionals
                where m.RecipeId == recipe.Id
                select m)).FirstOrDefault();

            if (item == null)
                return;

            var isMaterials = item.IsMaterial == 1;
            var isNotFestival = item.NotFestival == 1;

            recipe.IsMaterial = isMaterials;
            recipe.IsNotFestival = isNotFestival;
        }

        private void SetFavorite(RecipeInfo recipe)
        {
            var favorite = _contextManager.GetRecipe(x =>
                x.Favorites.FirstOrDefault(m => m.RecipeId == recipe.Id));

            if (favorite == null)
                return;

            recipe.IsFavorite = true;
        }

        public RecipeInfo? GetRecipeInfo(SearchNode node)
        {
            var recipe = Recipes.FirstOrDefault(x => x.Name == node.Name);
            if (recipe == null)
                return null;

            return recipe;
        }

        public void SelectCategory(SearchNode node)
        {
            var parent = node.Parent;
            if (parent == null)
                return;

            var categoryInfo = Categories.FirstOrDefault(x => x.Name == parent.Name);
            if (categoryInfo == null)
                return;

            SelectCategory(categoryInfo);
            categoryInfo.IsSelected = true;
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
            var favorite = _contextManager.GetRecipe(x =>
                x.Favorites.FirstOrDefault(m => m.RecipeId == recipe.Id));

            if (favorite == null)
                return;

            _contextManager.Apply(x => x.Favorites.Remove(favorite));

            recipe.IsFavorite = false;
        }

        public void Dispose()
        {
            _contextManager.Dispose();

            _setting.Save();
        }
    }
}
