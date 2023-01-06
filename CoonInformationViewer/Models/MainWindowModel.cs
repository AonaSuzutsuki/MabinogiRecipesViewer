using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int? Item1Id { get; set; }
        
        public int? Item2Id { get; set; }
        
        public int? Item3Id { get; set; }
        
        public int? Item1RecipeId { get; set; }
        
        public int? Item2RecipeId { get; set; }
        
        public int? Item3RecipeId { get; set; }
        
        public decimal Item1Amount { get; set; }
        
        public decimal Item2Amount { get; set; }
        
        public decimal Item3Amount { get; set; }
        
        public DateTime? UpdateDate { get; set; }
        public BitmapImage? Image { get; set; }

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

    public class MainWindowModel : ModelBase, IDisposable
    {

        private UpdateContextManager _contextManager = new();
        private readonly SettingLoader _setting = SettingLoader.Instance;

        private ObservableCollection<CategoryInfo> _categories = new();
        private ObservableCollection<RecipeInfo> _recipes = new();

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

            var recipe = _contextManager.GetRecipes(x =>
            {
                return x.CookRecipes.Where(m => !m.IsDelete && m.CategoryId == category.Id)
                    .OrderByDescending(m => m.Name);
            });

            Recipes.Clear();
            Recipes.AddRange(recipe.Select(x => new RecipeInfo(x)));
        }

        public RecipeInfo SelectRecipe(RecipeInfo recipe)
        {
            _currentRecipeInfo = recipe;
            return recipe;
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
