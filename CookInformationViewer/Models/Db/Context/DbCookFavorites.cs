using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookFavoritesTableName, Schema = Constants.Schema)]
    public class DbCookFavorites : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookFavorites favorites))
                return;

            RecipeId = favorites.RecipeId;
        }
    }
}