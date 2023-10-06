using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookMemosTableName, Schema = Constants.Schema)]
    public class DbCookMemos : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("memo")]
        public string Memo { get; set; } = string.Empty;

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookMemos memo))
                return;

            RecipeId = memo.RecipeId;
            Memo = memo.Memo;
        }
    }
}