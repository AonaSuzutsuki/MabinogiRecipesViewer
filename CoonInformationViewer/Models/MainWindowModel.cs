using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
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
using CookInformationViewer.Models.Converter;
using CookInformationViewer.Models.Db;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using CookInformationViewer.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SavannahXmlLib.XmlWrapper;
using UpdateLib.Http;
using UpdateLib.Update;

namespace CookInformationViewer.Models
{
    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RecipeInfo
    {
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

        public IEnumerable<LocationItemInfo>? Item1Locations { get; set; }
        public IEnumerable<LocationItemInfo>? Item2Locations { get; set; }
        public IEnumerable<LocationItemInfo>? Item3Locations { get; set; }

        public DateTime? UpdateDate { get; set; }
        public BitmapImage? Image { get; set; }

        public int Star { get; set; }

        public string StarText
        {
            get
            {
                return Star switch
                {
                    1 => "★☆☆☆☆",
                    2 => "★★☆☆☆",
                    3 => "★★★☆☆",
                    4 => "★★★★☆",
                    5 => "★★★★★",
                    6 => "★★★★★",
                    _ => ""
                };
            }
        }

        public string StarMessage
        {
            get
            {
                return Star switch
                {
                    1 => "確認済み",
                    2 => "確認済み",
                    3 => "確認済み",
                    4 => "究極の料理 確認済み",
                    5 => "天国の料理 確認済み",
                    6 => "最高の料理 確認済み",
                    _ => "未確認"
                };
            }
        }

        public System.Windows.Media.Brush StarBrush
        {
            get
            {
                return Star switch
                {
                    6 => (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff5307") ??
                                           new SolidColorBrush(Colors.White)),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
        }

        public string Url { get; set; }

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

    public class MainWindowModel : ModelBase, IDisposable
    {

        private UpdateContextManager _contextManager = new();
        private readonly SettingLoader _setting = SettingLoader.Instance;

        private ObservableCollection<CategoryInfo> _categories = new();
        private ObservableCollection<RecipeInfo> _recipes = new();
        private List<RecipeInfo> _recipeBaseItems = new();

        private CategoryInfo? _currentCategoryInfo;
        private RecipeInfo? _currentRecipeInfo;

        #region Properties

        public ObservableCollection<CategoryInfo> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<RecipeInfo> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        public bool AvailableUpdate => _contextManager.AvailableUpdate;

        #endregion

        public MainWindowModel()
        {

        }

        public async Task Initialize()
        {
            LoadCategories();

            if (_setting.IsCheckUpdate)
            {
                await _contextManager.AvailableTableUpdateCheck();
            }
        }

        public void Reload()
        {
            _contextManager.Dispose();
            _contextManager = new UpdateContextManager();

            LoadCategories();

            if (_currentCategoryInfo != null)
                SelectCategory(_currentCategoryInfo);
        }

        public void LoadCategories()
        {
            var categories = _contextManager.GetCategories(x =>
            {
                return x.CookCategories.Where(m => !m.IsDelete && x.CookRecipes.Any(n => n.CategoryId == m.Id))
                    .OrderByDescending(m => m.SkillRank);
            });

            Categories.Clear();
            Categories.AddRange(categories.Select(x => new CategoryInfo
            {
                Id = x.Id,
                Name = x.Name
            }));
        }

        public void SelectCategory(CategoryInfo category)
        {
            _currentCategoryInfo = category;

            var recipe = _contextManager.GetItems(x =>
            {
                return (from m in x.CookRecipes
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
                    where !m.IsDelete && m.CategoryId == category.Id
                    orderby m.Name
                    select new RecipeInfo(m)
                    {
                        Item1Name = subRec1.Name ?? subMat1.Name,
                        Item2Name = subRec2.Name ?? subMat2.Name,
                        Item3Name = subRec3.Name ?? subMat3.Name
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

            return recipe;
        }

        public void NarrowDownRecipes(string searchWord)
        {
            if (string.IsNullOrEmpty(searchWord))
                return;

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

        public void SetEffects(RecipeInfo recipe)
        {
            var effects = _contextManager.GetItems(x => (from m in x.CookEffects
                where m.RecipeId == recipe.Id
                select m));

            var max = effects.MaxBy(x => x.Star);
            recipe.Star = max?.Star ?? 0;

        }

        public async Task RebuildDataBase()
        {
            await _contextManager.RebuildDataBase();
        }

        public void Dispose()
        {
            _contextManager.Dispose();

            _setting.Save();
        }
    }
}
