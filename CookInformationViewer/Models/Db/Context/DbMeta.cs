using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.MetaTableName, Schema = Constants.Schema)]
    public class DbMeta
    {
        [Column("id")]
        public string? Id { get; set; }

        [Column("value")]
        public string? Value { get; set; }
    }
}
