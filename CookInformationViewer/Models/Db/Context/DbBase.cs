using System;
using System.Collections.Generic;

namespace CookInformationViewer.Models.Db.Context
{
    public abstract class DbBase : IDbElement
    {
        public abstract int Id { get; set; }
        public abstract DateTime? CreateDate { get; set; }
        public abstract DateTime? UpdateDate { get; set; }
        public abstract bool IsDelete { get; set; }

        public virtual void Apply(object source)
        {
            if (!(source is DbBase dbBase))
                return;

            Id = dbBase.Id;
            IsDelete = dbBase.IsDelete;
            CreateDate = dbBase.CreateDate;
            UpdateDate = dbBase.UpdateDate;
        }
    }
}