using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookAdditionalsTableName, Schema = Constants.Schema)]
    public class DbCookAdditionals : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("check_star")]
        public int CheckStar { get; set; }

        [Column("is_material")]
        public int IsMaterial { get; set; }

        [Column("not_festival")]
        public int NotFestival { get; set; }

        [Column("special")]
        public string? Special { get; set; }

        [Column("can_not_make")]
        public bool CanNotMake { get; set; }


        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookAdditionals additional))
                return;

            RecipeId = additional.RecipeId;
            CheckStar = additional.CheckStar;
            IsMaterial = additional.IsMaterial;
            NotFestival = additional.NotFestival;
            CanNotMake = additional.CanNotMake;
        }
    }
}
