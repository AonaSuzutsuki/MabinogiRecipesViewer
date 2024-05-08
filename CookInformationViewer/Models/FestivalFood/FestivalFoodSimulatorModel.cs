using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using CookInformationViewer.Models.Extensions;
using CommonStyleLib.Models;
using CookInformationViewer.Models.DataValue;
using CookInformationViewer.Models.Db.Manager;
using CookInformationViewer.Models.Extensions;
using CookInformationViewer.Models.Searchers;
using KimamaSqliteExecutorLib.Db.Raw;
using CommonExtensionLib.Extensions;
using System.Drawing;
using CookInformationViewer.Models.Db.Context;
using System.Windows.Media;

namespace CookInformationViewer.Models.FestivalFood
{
    public class FestivalFoodSimulatorModel : ModelBase
    {
        private readonly ContextManager _contextManager = new();

        private ObservableCollection<FestivalFoodRecipeHeader> _recipeHeaders = new();

        public ObservableCollection<FestivalFoodSearchStatusItem> StatusNames = new()
        {
            new FestivalFoodSearchStatusItem("お気に入り"),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Hp)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Mp)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Sp)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Str)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Int)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Dex)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Will)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Luck)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.MaxDamage)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.MinDamage)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.MagicDamage)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Defense)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.Protection)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.MagicDefense)),
            new FestivalFoodSearchStatusItem(new SearchStatusItem(SearchStatusItem.SearchStatusEnum.MagicProtection)),
        };

        public ObservableCollection<FestivalFoodRecipeHeader> RecipeHeaders
        {
            get => _recipeHeaders;
            set => SetProperty(ref _recipeHeaders, value);
        }

        public void SearchFavorite()
        {
            var recipes = _contextManager.GetItem(c => (
                from x in c.Favorites
                join recipe in c.CookRecipes on x.RecipeId equals recipe.Id
                join category in c.CookCategories on recipe.CategoryId equals category.Id
                join add in c.Additionals on recipe.Id equals add.RecipeId into addItem
                from addDefaultItem in addItem.DefaultIfEmpty()
                where !x.IsDelete
                orderby x.CreateDate
                select new
                {
                    Recipe = recipe,
                    Category = category,
                    Additional = addDefaultItem
                }
            ).ToList());

            var recipeIds = recipes.Select(x => x.Recipe.Id).ToList();

            var effects = _contextManager.GetItem(c => (
                from eff in c.CookEffects
                where recipeIds.Any(x => x == eff.RecipeId)
                select new
                {
                    RecipeId = eff.RecipeId,
                    Effect = eff
                }
            ).ToList());

            var effectMap = effects.GroupBy(x => x.RecipeId)
                .ToDictionary(x => x.Key, x =>
                    x.GroupBy(n => n.Effect.Star).ToDictionary(n => n.Key, n =>
                        n.Select(m => new EffectInfo(m.Effect)).ToList()
                    ));

            RecipeHeaders.Clear();

            foreach (var item in recipes)
            {
                var category = new CategoryInfo
                {
                    Id = item.Category.Id,
                    Name = item.Category.Name
                };

                var recipeEffects = effectMap.ContainsKey(item.Recipe.Id) ? effectMap[item.Recipe.Id] : new Dictionary<int, List<EffectInfo>>();
                var qualityList = new List<QualityItemInfo>();

                foreach (var recipeEffect in recipeEffects)
                {
                    var quality = new QualityItemInfo(recipeEffect.Key, recipeEffect.Value);
                    qualityList.Add(quality);
                }

                var recipe = new FestivalFoodRecipeHeader(new RecipeInfo(item.Recipe)
                {
                    Star = item.Additional.CheckStar,
                    Category = category,
                    Effects = qualityList
                })
                {
                    Category = category,
                    StarEffectMap = recipeEffects
                };
                RecipeHeaders.Add(recipe);
            }
        }

        public void SearchRecipes(SearchStatusItem statusItem)
        {
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

            sql.Where($"and exists(select 1 from {Constants.CookAdditionalsTableName} addi where eff.recipe_id = addi.recipe_id and not_festival = 0)");

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

            var effects = _contextManager.GetItem(c => (
                from eff in c.CookEffects
                where recipeIds.Any(x => x == eff.RecipeId)
                select new
                {
                    RecipeId = eff.RecipeId,
                    Effect = eff
                }
            ).ToList());

            var effectMap = effects.GroupBy(x => x.RecipeId)
                .ToDictionary(x => x.Key, x => 
                    x.GroupBy(n => n.Effect.Star).ToDictionary(n => n.Key, n =>
                        SortEffectList(statusItem, n.Select(m => new EffectInfo(m.Effect)).ToList())
                        ));

            RecipeHeaders.Clear();

            foreach (var item in sortedItems)
            {
                var category = new CategoryInfo
                {
                    Id = item.category.Id,
                    Name = item.category.Name
                };

                var recipeEffects = effectMap.ContainsKey(item.recipe.Id) ? effectMap[item.recipe.Id] : new Dictionary<int, List<EffectInfo>>();
                var qualityList = new List<QualityItemInfo>();

                foreach (var recipeEffect in recipeEffects)
                {
                    var quality = new QualityItemInfo(recipeEffect.Key, recipeEffect.Value);
                    qualityList.Add(quality);
                }

                var recipe = new FestivalFoodRecipeHeader(new RecipeInfo(item.recipe)
                {
                    Star = item.status["star"].GetInt(),
                    Category = category,
                    Effects = qualityList
                })
                {
                    Additional = $"{statusItem.ConvertLogicStatus()} {item.status["status"].GetLong()}",
                    Category = category,
                    StarEffectMap = recipeEffects
                };
                RecipeHeaders.Add(recipe);
            }
        }

        private List<EffectInfo> SortEffectList(SearchStatusItem statusItem, List<EffectInfo> effects)
        {
            switch (statusItem.Value)
            {
                case SearchStatusItem.SearchStatusEnum.Hp:
                    return effects.OrderBy(x => x.Hp).ToList();
                case SearchStatusItem.SearchStatusEnum.Mp:
                    return effects.OrderBy(x => x.Mana).ToList();
                case SearchStatusItem.SearchStatusEnum.Sp:
                    return effects.OrderBy(x => x.Stamina).ToList();
                case SearchStatusItem.SearchStatusEnum.Str:
                    return effects.OrderBy(x => x.Str).ToList();
                case SearchStatusItem.SearchStatusEnum.Int:
                    return effects.OrderBy(x => x.Int).ToList();
                case SearchStatusItem.SearchStatusEnum.Dex:
                    return effects.OrderBy(x => x.Dex).ToList();
                case SearchStatusItem.SearchStatusEnum.Will:
                    return effects.OrderBy(x => x.Will).ToList();
                case SearchStatusItem.SearchStatusEnum.Luck:
                    return effects.OrderBy(x => x.Luck).ToList();
                case SearchStatusItem.SearchStatusEnum.MaxDamage:
                    return effects.OrderBy(x => x.Damage).ToList();
                case SearchStatusItem.SearchStatusEnum.MinDamage:
                    return effects.OrderBy(x => x.MinDamage).ToList();
                case SearchStatusItem.SearchStatusEnum.MagicDamage:
                    return effects.OrderBy(x => x.MagicDamage).ToList();
                case SearchStatusItem.SearchStatusEnum.Defense:
                    return effects.OrderBy(x => x.Defense).ToList();
                case SearchStatusItem.SearchStatusEnum.Protection:
                    return effects.OrderBy(x => x.Protection).ToList();
                case SearchStatusItem.SearchStatusEnum.MagicDefense:
                    return effects.OrderBy(x => x.MagicDefense).ToList();
                case SearchStatusItem.SearchStatusEnum.MagicProtection:
                    return effects.OrderBy(x => x.MagicProtection).ToList();
                default:
                    return effects;
            }
        }
    }
}
