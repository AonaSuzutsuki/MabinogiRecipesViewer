using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookSellersTableName, Schema = Constants.Schema)]
    public class DbCookSellers : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("location_id")]
        public int LocationId { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookSellers sellers))
                return;

            Name = sellers.Name;
            LocationId = sellers.LocationId;
        }
    }
}