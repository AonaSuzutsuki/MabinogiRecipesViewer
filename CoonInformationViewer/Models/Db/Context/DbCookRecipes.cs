using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CommonCoreLib.Bool;

namespace CoonInformationViewer.Models.Db.Context
{
    [Table(Constants.CookRecipesTableName, Schema = Constants.Schema)]
    public class DbCookRecipes : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("item1_id")]
        public int? Item1Id { get; set; }

        [Column("item2_id")]
        public int? Item2Id { get; set; }

        [Column("item3_id")]
        public int? Item3Id { get; set; }
        
        [Column("item1_recipe_id")]
        public int? Item1RecipeId { get; set; }

        [Column("item2_recipe_id")]
        public int? Item2RecipeId { get; set; }

        [Column("item3_recipe_id")]
        public int? Item3RecipeId { get; set; }

        [Column("item1_amount")]
        public decimal Item1Amount { get; set; }

        [Column("item2_amount")]
        public decimal Item2Amount { get; set; }

        [Column("item3_amount")]
        public decimal Item3Amount { get; set; }

        [Column("image_path")]
        public string? ImagePath { get; set; }

        [Column("image_data")]
        public byte[]? ImageData { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookRecipes recipe))
                return;

            Name = recipe.Name;
            CategoryId = recipe.CategoryId;
            Item1Id = recipe.Item1Id;
            Item2Id = recipe.Item2Id;
            Item3Id = recipe.Item3Id;
            Item1RecipeId = recipe.Item1RecipeId;
            Item2RecipeId = recipe.Item2RecipeId;
            Item3RecipeId = recipe.Item3RecipeId;
            Item1Amount = recipe.Item1Amount;
            Item2Amount = recipe.Item2Amount;
            Item3Amount = recipe.Item3Amount;
            ImagePath = recipe.ImagePath;
        }
    }
}