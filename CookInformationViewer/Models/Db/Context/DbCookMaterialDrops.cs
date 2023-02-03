using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookMaterialDropsTableName, Schema = Constants.Schema)]
    public class DbCookMaterialDrops : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("material_id")]
        public int MaterialId { get; set; }

        [Column("drop_name")]
        public string DropName { get; set; }

        [Column("location_id")]
        public int LocationId { get; set; }

        [Column("is_craft")]
        public bool IsCraft { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookMaterialDrops categories))
                return;

            MaterialId = categories.MaterialId;
            DropName = categories.DropName;
            LocationId = categories.LocationId;
            IsCraft = categories.IsCraft;
        }
    }
}