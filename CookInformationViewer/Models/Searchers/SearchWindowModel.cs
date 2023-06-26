using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using Reactive.Bindings;

namespace CookInformationViewer.Models.Searchers
{
    public class SearchNode
    {
        public SearchNode? Parent { get; set; }

        public bool IsCategory { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<SearchNode>? Children { get; set; }

        public bool IsExpanded { get; set; } = true;
    }

    public class SearchWindowModel : ModelBase
    {
        private readonly ContextManager _contextManager = new();
        
        private ObservableCollection<RecipeHeader> _recipes = new();
        

        public ObservableCollection<RecipeHeader> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        public SearchWindowModel()
        {
        }

        public void Search(string word, bool isMaterialSearch)
        {
            if (isMaterialSearch)
            {
                var materials = _contextManager.GetItem(x =>
                    x.CookMaterials.Where(m => m.Name.Contains(word)).Select(m => (int?)m.Id).ToList());

                var recipeMaterials = _contextManager.GetItem(x =>
                    x.CookRecipes.Where(m => m.Name.Contains(word)).Select(m => (int?)m.Id).ToList());

                var recipesWithMaterial = _contextManager.GetItem(x =>
                    (from rec in x.CookRecipes
                     join cat in x.CookCategories on rec.CategoryId equals cat.Id
                     where materials.Contains(rec.Item1Id) || materials.Contains(rec.Item2Id) || materials.Contains(rec.Item3Id)
                     select new Tuple<DbCookRecipes, DbCookCategories>(rec, cat)).ToList()
                );

                var recipes = _contextManager.GetItem(x =>
                    (from rec in x.CookRecipes
                     join cat in x.CookCategories on rec.CategoryId equals cat.Id
                     where recipeMaterials.Contains(rec.Item1RecipeId) || recipeMaterials.Contains(rec.Item2RecipeId) || recipeMaterials.Contains(rec.Item3RecipeId)
                     select new Tuple<DbCookRecipes, DbCookCategories>(rec, cat)).ToList()
                );

                var merge = recipes.Union(recipesWithMaterial).GroupBy(x => x.Item2)
                    .ToDictionary(x => x.Key, x => x.ToList());

                DrawList(merge);
            }
            else
            {
                var recipes = _contextManager.GetItem(x =>
                    (from rec in x.CookRecipes
                     join cat in x.CookCategories on rec.CategoryId equals cat.Id
                     where rec.Name.Contains(word)
                     select new Tuple<DbCookRecipes, DbCookCategories>(rec, cat)).ToList()
                    .GroupBy(x => x.Item2)
                    .ToDictionary(x => x.Key, x => x.ToList())
                );

                DrawList(recipes);
            }
        }
        
        private void DrawList(Dictionary<DbCookCategories, List<Tuple<DbCookRecipes, DbCookCategories>>> items)
        {
            Recipes.Clear();

            foreach (var item in items)
            {
                var category = item.Key;
                var header = new RecipeHeader(category.Name);
                Recipes.Add(header);

                foreach (var tuple in item.Value)
                {
                    var recipe = new RecipeHeader(new RecipeInfo(tuple.Item1)
                    {
                        Category = new CategoryInfo
                        {
                            Id = category.Id,
                            Name = category.Name
                        }
                    });
                    Recipes.Add(recipe);
                }
            }
        }
    }
}
