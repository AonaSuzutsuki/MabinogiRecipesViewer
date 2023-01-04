using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoonInformationViewer.Models.Db.Context
{
    [Table(Constants.CookMaterialSellersTableName, Schema = Constants.Schema)]
    public class DbCookMaterialSellers : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("material_id")]
        public int MaterialId { get; set; }

        [Column("seller_id")]
        public int SellerId { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookMaterialSellers categories))
                return;

            MaterialId = categories.MaterialId;
            SellerId = categories.SellerId;
        }
    }
}