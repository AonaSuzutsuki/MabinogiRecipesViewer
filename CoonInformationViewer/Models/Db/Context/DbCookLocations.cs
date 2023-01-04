using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoonInformationViewer.Models.Db.Context
{
    [Table(Constants.CookLocationsTableName, Schema = Constants.Schema)]
    public class DbCookLocations : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookLocations locations))
                return;

            Name = locations.Name;
        }
    }
}