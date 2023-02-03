using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
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

        private ObservableCollection<SearchNode> _searchNodes = new();

        public ObservableCollection<SearchNode> SearchItems
        {
            get => _searchNodes;
            set => SetProperty(ref _searchNodes, value);
        }

        public SearchWindowModel()
        {
        }

        public void Search(string word, bool isMaterialSearch)
        {
            if (isMaterialSearch)
            {
                var materials = _contextManager.GetRecipe(x => 
                    x.CookMaterials.Where(m => m.Name.Contains(word)).Select(m => (int?)m.Id).ToList());

                var recipeMaterials = _contextManager.GetRecipe(x =>
                    x.CookRecipes.Where(m => m.Name.Contains(word)).Select(m => (int?)m.Id).ToList());

                var recipesWithMaterial = _contextManager.GetRecipe(x =>
                    (from rec in x.CookRecipes
                        join cat in x.CookCategories on rec.CategoryId equals cat.Id
                     where materials.Contains(rec.Item1Id) || materials.Contains(rec.Item2Id) || materials.Contains(rec.Item3Id)
                     select new Tuple<DbCookRecipes, DbCookCategories>(rec, cat)).ToList()
                );
                
                var recipes = _contextManager.GetRecipe(x =>
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
                var recipes = _contextManager.GetRecipe(x =>
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
            SearchItems.Clear();

            var dict = new Dictionary<DbCookCategories, SearchNode>();

            foreach (var item in items)
            {
                SearchNode categoryNode;
                if (dict.ContainsKey(item.Key))
                {
                    categoryNode = dict[item.Key];
                }
                else
                {
                    categoryNode = new SearchNode
                    {
                        Name = item.Key.Name,
                        IsCategory = true
                    };
                    dict.Add(item.Key, categoryNode);
                }

                categoryNode.Children = new List<SearchNode>();
                categoryNode.Children.AddRange(item.Value.Select(x => new SearchNode
                {
                    Name = x.Item1.Name,
                    Parent = categoryNode
                }));
            }

            SearchItems.AddRange(dict.Values.ToList());
        }
    }
}
