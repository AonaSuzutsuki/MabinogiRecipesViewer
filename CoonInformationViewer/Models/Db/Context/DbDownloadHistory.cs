using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoonInformationViewer.Models.Db.Context
{
    [Table(Constants.DownloadHistoriesTableName, Schema = Constants.Schema)]
    public class DbDownloadHistory : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("date")]
        public string Date { get; set; } = string.Empty;

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbDownloadHistory history))
                return;

            Name = history.Name;
            Date = history.Date;
        }
    }
}