using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookCategoriesTableName, Schema = Constants.Schema)]
    public class DbCookCategories : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("skill_rank")]
        public string SkillRank { get; set; }

        [Column("is_hidden")]
        public bool IsHidden { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookCategories categories))
                return;

            Name = categories.Name;
            SkillRank = categories.SkillRank;
            IsHidden = categories.IsHidden;
        }
    }
}