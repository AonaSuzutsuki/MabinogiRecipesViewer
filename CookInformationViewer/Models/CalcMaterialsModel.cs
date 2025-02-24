using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using CommonStyleLib.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using Newtonsoft.Json.Bson;
using Reactive.Bindings;

namespace CookInformationViewer.Models
{
    public class CalcMaterialInfo
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsRecipe { get; set; }

        public bool CanPurchasable { get; set; }

        public CalcMaterialInfo? Parent { get; set; }

        public List<CalcMaterialInfo> Children { get; set; } = new();

        public List<LocationItemInfo> LocationItems { get; set; } = new();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Name}, {CanPurchasable}");
            return sb.ToString();
        }
    }

    public class CalcMaterialFlatInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public ObservableCollection<CalcMaterialInfo> UsedRecipes { get; set; } = new();
        public ObservableCollection<LocationItemInfo> LocationItems { get; set; } = new();
    }

    public class CalcMaterialsModel : ModelBase
    {
        public ObservableCollection<CalcMaterialFlatInfo> Materials { get; set; } = new();

        private readonly ContextManager _contextManager = new();
        
        private CalcMaterialInfo? _root;

        private bool _ignoreCanPurchasableChecked;

        public bool IgnoreCanPurchasableChecked
        {
            get => _ignoreCanPurchasableChecked;
            set => SetProperty(ref _ignoreCanPurchasableChecked, value);
        }

        public async Task Create(RecipeInfo recipe)
        {
            _root = new CalcMaterialInfo()
            {
                Id = recipe.Id,
                Name = recipe.Name,
                IsRecipe = true
            };

            await Task.Run(() =>
            {
                Open(_root, recipe);
                Analyze(1);
            });
        }

        public void Open(CalcMaterialInfo parent, RecipeInfo recipe)
        {
            var material1Id = recipe.Item1Id;
            var material2Id = recipe.Item2Id;
            var material3Id = recipe.Item3Id;
            var recipe1Id = recipe.Item1RecipeId;
            var recipe2Id = recipe.Item2RecipeId;
            var recipe3Id = recipe.Item3RecipeId;

            ParseRecipe(parent, material1Id, recipe1Id);
            ParseRecipe(parent, material2Id, recipe2Id);
            ParseRecipe(parent, material3Id, recipe3Id);
        }

        public void Analyze(int recipeCount)
        {
            var recipes = new List<CalcMaterialInfo>();
            if (_root != null)
                InternalAnalyze(_root, recipes, IgnoreCanPurchasableChecked);

            var test = recipes.GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => x.ToList());

            Materials.Clear();

            foreach (var keyValue in test)
            {
                var name = keyValue.Key;

                if (recipeCount > 1)
                {
                    name += $"({keyValue.Value.Count}) x {recipeCount}";
                }

                Materials.Add(new CalcMaterialFlatInfo
                {
                    Name = name,
                    Count = keyValue.Value.Count * recipeCount,
                    UsedRecipes = new ObservableCollection<CalcMaterialInfo>(keyValue.Value.Select(x => x.Parent ?? new CalcMaterialInfo())),
                    LocationItems = new ObservableCollection<LocationItemInfo>(keyValue.Value.First().LocationItems)
                });
            }
        }

        public RecipeHeader? ToRecipeHeader(CalcMaterialInfo materialInfo)
        {
            var recipe = _contextManager.GetItem(c => (
                    from x in c.CookRecipes
                    join category in c.CookCategories on x.CategoryId equals category.Id
                    where x.Id == materialInfo.Id
                    select new
                    {
                        recipe = x,
                        category
                    }
            ).ToList().FirstOrDefault());

            if (recipe == null)
                return null;

            var header = new RecipeHeader(new RecipeInfo(recipe.recipe)
            {
                Category = new CategoryInfo(recipe.category) 
            });

            return header;
        }

        private void InternalAnalyze(CalcMaterialInfo parent, List<CalcMaterialInfo> recipes, bool onlyCanPurchasable)
        {
            foreach (var child in parent.Children)
            {
                if (child.IsRecipe)
                {
                    if (onlyCanPurchasable && child.CanPurchasable)
                        recipes.Add(child);
                    else
                        InternalAnalyze(child, recipes, onlyCanPurchasable);
                }
                else
                {
                    recipes.Add(child);
                }
            }
        }

        private void ParseRecipe(CalcMaterialInfo parent, int? materialId, int? recipeId)
        {
            if (recipeId != null)
            {
                var dbRecipe = _contextManager.GetItem(c => c.CookRecipes.FirstOrDefault(x => x.Id == recipeId));

                if (dbRecipe != null)
                {
                    var recipeInfo = new RecipeInfo(dbRecipe);
                    var canPurchasable = _contextManager.GetItem(c => c.CookMaterials.Any(x => x.Name == recipeInfo.Name));
                    
                    var info = new CalcMaterialInfo
                    {
                        Id = recipeInfo.Id,
                        Name = recipeInfo.Name,
                        Parent = parent,
                        IsRecipe = true,
                        CanPurchasable = canPurchasable,
                        LocationItems = new List<LocationItemInfo>
                        {
                            LocationItemInfo.CookItem
                        }
                    };

                    parent.Children.Add(info);

                    Open(info, recipeInfo);
                }
            }
            else
            {
                var material = _contextManager.GetItem(c => c.CookMaterials.FirstOrDefault(x => x.Id == materialId));
                var sellers = _contextManager.GetItem(c => (
                        from x in c.CookMaterialSellers
                        join seller in c.CookSellers on x.SellerId equals seller.Id
                        join location in c.CookLocations on seller.LocationId equals location.Id
                        where x.MaterialId == materialId
                        select new
                        {
                            seller,
                            location
                        }
                    ).ToList());
                var drops = _contextManager.GetItem(c => (
                        from x in c.CookMaterialDrops
                        join location in c.CookLocations on x.LocationId equals location.Id
                        where x.MaterialId == materialId
                        select new
                        {
                            drop = x,
                            location
                        }
                    ).ToList());

                if (material != null)
                {
                    var info = new CalcMaterialInfo
                    {
                        Id = material.Id,
                        Name = material.Name,
                        Parent = parent
                    };
                    info.LocationItems.AddRange(sellers.Select(x => new LocationItemInfo
                    {
                        Name = x.seller.Name,
                        Location = x.location.Name,
                        Type = LocationItemInfo.TypeNpc
                    }));
                    info.LocationItems.AddRange(drops.Select(x => new LocationItemInfo
                    {
                        Name = x.drop.DropName,
                        Location = x.location.Name,
                        Type = LocationItemInfo.TypeNpc
                    }));

                    parent.Children.Add(info);
                }
            }
        }
    }
}
