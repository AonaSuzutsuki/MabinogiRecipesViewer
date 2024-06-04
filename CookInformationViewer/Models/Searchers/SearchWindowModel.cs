using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Context;
using CookInformationViewer.Models.Db.Manager;
using CookInformationViewer.ViewModels.Searchers;
using KimamaSqlExecutorLib.Db.Raw;
using KimamaSqliteExecutorLib.Db.Raw;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Reactive.Bindings;

namespace CookInformationViewer.Models.Searchers
{
    public class SearchStatusItem
    {
        public enum SearchStatusEnum
        {
            Hp,
            Mp,
            Sp,
            Str,
            Int,
            Dex,
            Will,
            Luck,
            MaxDamage,
            MinDamage,
            MagicDamage,
            Defense,
            Protection,
            MagicDefense,
            MagicProtection
        }

        public string Name { get; }
        
        public string PhysicName { get; }

        public SearchStatusEnum Value { get; }
        
        public SearchStatusItem(SearchStatusEnum value)
        {
            Value = value;
            Name = ConvertLogicStatus();
            PhysicName = ConvertPhysicStatus();
        }

        public string ConvertLogicStatus()
        {
            return Value switch
            {
                SearchStatusEnum.Hp => "体力",
                SearchStatusEnum.Mp => "マナ",
                SearchStatusEnum.Sp => "スタミナ",
                SearchStatusEnum.Str => "Str",
                SearchStatusEnum.Int => "Int",
                SearchStatusEnum.Dex => "Dex",
                SearchStatusEnum.Will => "Will",
                SearchStatusEnum.Luck => "Luck",
                SearchStatusEnum.MaxDamage => "最大攻撃",
                SearchStatusEnum.MinDamage => "最小攻撃",
                SearchStatusEnum.MagicDamage => "魔法攻撃",
                SearchStatusEnum.Defense => "防御",
                SearchStatusEnum.Protection => "保護",
                SearchStatusEnum.MagicDefense => "魔法防御",
                SearchStatusEnum.MagicProtection => "魔法保護",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public string ConvertPhysicStatus()
        {
            return Value switch
            {
                SearchStatusEnum.Hp => "hp",
                SearchStatusEnum.Mp => "mana",
                SearchStatusEnum.Sp => "stamina",
                SearchStatusEnum.Str => "str",
                SearchStatusEnum.Int => "inteli",
                SearchStatusEnum.Dex => "dex",
                SearchStatusEnum.Will => "will",
                SearchStatusEnum.Luck => "luck",
                SearchStatusEnum.MaxDamage => "damage",
                SearchStatusEnum.MinDamage => "min_damage",
                SearchStatusEnum.MagicDamage => "magic_damage",
                SearchStatusEnum.Defense => "defense",
                SearchStatusEnum.Protection => "protection",
                SearchStatusEnum.MagicDefense => "magic_defense",
                SearchStatusEnum.MagicProtection => "magic_protection",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

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
        #region Fields

        public ObservableCollection<SearchStatusItem> StatusItems { get; set; } = new ObservableCollection<SearchStatusItem>
        {
            new(SearchStatusItem.SearchStatusEnum.Hp),
            new(SearchStatusItem.SearchStatusEnum.Mp),
            new(SearchStatusItem.SearchStatusEnum.Sp),
            new(SearchStatusItem.SearchStatusEnum.Str),
            new(SearchStatusItem.SearchStatusEnum.Int),
            new(SearchStatusItem.SearchStatusEnum.Dex),
            new(SearchStatusItem.SearchStatusEnum.Will),
            new(SearchStatusItem.SearchStatusEnum.Luck),
            new(SearchStatusItem.SearchStatusEnum.MaxDamage),
            new(SearchStatusItem.SearchStatusEnum.MinDamage),
            new(SearchStatusItem.SearchStatusEnum.MagicDamage),
            new(SearchStatusItem.SearchStatusEnum.Defense),
            new(SearchStatusItem.SearchStatusEnum.Protection),
            new(SearchStatusItem.SearchStatusEnum.MagicDefense),
            new(SearchStatusItem.SearchStatusEnum.MagicProtection)
        };

        private readonly ContextManager _contextManager = new();

        private ObservableCollection<RecipeHeader> _recipes = new();

        #endregion

        #region Properties
        public bool IsOpened { get; set; }

        public SearchStatusItem? SelectedStatusItem { get; set; }

        public string SearchText { get; set; } = string.Empty;
        public bool IsMaterialSearch { get; set; }
        public bool IsStatusSearch { get; set; }
        public bool IgnoreNotFestival { get; set; }

        public ObservableCollection<RecipeHeader> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        #endregion

        public SearchWindowModel()
        {
        }

        public void Search(string word, bool isMaterialSearch)
        {
            if (isMaterialSearch)
            {
                var merge = GetMaterialRecipes(word)
                    .GroupBy(x => x.Item2)
                    .ToDictionary(x => x.Key, x => x.ToList()); ;

                DrawList(merge);
            }
            else
            {
                var recipes = _contextManager.GetItem(x =>
                    (from rec in x.CookRecipes
                     join cat in x.CookCategories on rec.CategoryId equals cat.Id
                     join add in x.Additionals on rec.Id equals add.RecipeId into addj
                     from addItem in addj.DefaultIfEmpty()
                     where rec.Name.Contains(word)
                     select new Tuple<DbCookRecipes, DbCookCategories, DbCookAdditionals>(rec, cat, addItem)).ToList()
                    .GroupBy(x => x.Item2)
                    .ToDictionary(x => x.Key, x => x.ToList())
                );

                DrawList(recipes);
            }
        }

        public void StatusSearch(string word, bool isMaterialSearch, SearchStatusItem? statusItem, bool ignoreNotFestival)
        {
            if (statusItem == null)
                return;

            var selectQuery = "eff.recipe_id, " +
                              "rec.name, " +
                              "MAX({0}) as status, " +
                              "MAX(star) as star, " +
                              "mat1.name as material1, " +
                              "mat2.name as material2, " +
                              "mat3.name as material3, " +
                              "rec1.name as recipe1, " +
                              "rec2.name as recipe2, " +
                              "rec3.name as recipe3";
            var tableName = $"{Constants.CookEffectsTableName} eff";
            var sql = SqlCreator.Select(selectQuery.FormatString(statusItem.PhysicName), tableName).Where($"{statusItem.PhysicName} > 0");

            sql.InnerJoin($"{Constants.CookRecipesTableName} rec", "eff.recipe_id = rec.id");
            sql.LeftJoin($"{Constants.CookMaterialsTableName} mat1", "rec.item1_id = mat1.id");
            sql.LeftJoin($"{Constants.CookMaterialsTableName} mat2", "rec.item2_id = mat2.id");
            sql.LeftJoin($"{Constants.CookMaterialsTableName} mat3", "rec.item3_id = mat3.id");
            sql.LeftJoin($"{Constants.CookRecipesTableName} rec1", "rec.item1_recipe_id = rec1.id");
            sql.LeftJoin($"{Constants.CookRecipesTableName} rec2", "rec.item2_recipe_id = rec2.id");
            sql.LeftJoin($"{Constants.CookRecipesTableName} rec3", "rec.item3_recipe_id = rec3.id");

            sql.GroupBy("eff.recipe_id, rec.name");

            if (ignoreNotFestival)
            {
                sql.Where($"and exists(select 1 from {Constants.CookAdditionalsTableName} addi where eff.recipe_id = addi.recipe_id and not_festival = 0)");
            }

            if (isMaterialSearch)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    sql.Where($"and (" +
                              $"mat1.name like '%{word}%'" +
                              $"or mat2.name like '%{word}%'" +
                              $"or mat3.name like '%{word}%'" +
                              $"or rec1.name like '%{word}%'" +
                              $"or rec2.name like '%{word}%'" +
                              $"or rec3.name like '%{word}%'" +
                              $")");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(word))
                {
                    sql.Where($"and rec.name like '%{word}%'");
                }
            }

            var rowItems = _contextManager.Execute(sql).OrderByDescending(x => x["status"].GetLong()).ToList();

            var recipeIds = new HashSet<long>(rowItems.Select(x => x["recipe_id"].GetLong()));
            var statusIdMap = rowItems.ToDictionary(x => x["recipe_id"].GetLong(),
                x => x);

            var items = _contextManager.GetItem(context => (from x in context.CookRecipes
                join category in context.CookCategories on x.CategoryId equals category.Id
                where !x.IsDelete && recipeIds.Any(m => m == x.Id)
                select new
                {
                    recipe = x,
                    category
                })).ToList();

            var sortedItems = items.Select(x => new
                {
                    x.recipe,
                    x.category,
                    status = statusIdMap[x.recipe.Id]
                })
                .OrderByDescending(x => statusIdMap[x.recipe.Id]["status"].GetLong())
                .ToList();

            Recipes.Clear();

            foreach (var item in sortedItems)
            {
                var category = new CategoryInfo
                {
                    Id = item.category.Id,
                    Name = item.category.Name
                };

                var recipe = new RecipeHeader(new RecipeInfo(item.recipe)
                {
                    Star = item.status["star"].GetInt(),
                    Category = category
                })
                {
                    Additional = $"{statusItem.ConvertLogicStatus()} {item.status["status"].GetLong()}",
                    Category = category
                };
                Recipes.Add(recipe);
            }
        }

        private IEnumerable<Tuple<DbCookRecipes, DbCookCategories, DbCookAdditionals>> GetMaterialRecipes(string word)
        {
            var materials = _contextManager.GetItem(x =>
                x.CookMaterials.Where(m => m.Name.Contains(word)).Select(m => (int?)m.Id).ToList());

            var recipeMaterials = _contextManager.GetItem(x =>
                x.CookRecipes.Where(m => m.Name.Contains(word)).Select(m => (int?)m.Id).ToList());

            var recipesWithMaterial = _contextManager.GetItem(x =>
                (from rec in x.CookRecipes
                    join cat in x.CookCategories on rec.CategoryId equals cat.Id
                    join add in x.Additionals on rec.Id equals add.RecipeId into addj
                    from addItem in addj.DefaultIfEmpty()
                    where materials.Contains(rec.Item1Id) || materials.Contains(rec.Item2Id) || materials.Contains(rec.Item3Id)
                    select new Tuple<DbCookRecipes, DbCookCategories, DbCookAdditionals>(rec, cat, addItem)).ToList()
            );

            var recipes = _contextManager.GetItem(x =>
                (from rec in x.CookRecipes
                    join cat in x.CookCategories on rec.CategoryId equals cat.Id
                    join add in x.Additionals on rec.Id equals add.RecipeId into addj
                    from addItem in addj.DefaultIfEmpty()
                    where recipeMaterials.Contains(rec.Item1RecipeId) || recipeMaterials.Contains(rec.Item2RecipeId) || recipeMaterials.Contains(rec.Item3RecipeId)
                    select new Tuple<DbCookRecipes, DbCookCategories, DbCookAdditionals>(rec, cat, addItem)).ToList()
            );

            var merge = recipes.Union(recipesWithMaterial);

            return merge;
        }

        private void DrawList(Dictionary<DbCookCategories, List<Tuple<DbCookRecipes, DbCookCategories, DbCookAdditionals>>> items)
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
                        Star = tuple.Item3.CheckStar,
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
