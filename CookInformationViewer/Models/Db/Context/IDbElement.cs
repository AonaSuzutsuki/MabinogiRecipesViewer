using System.Collections.Generic;

namespace CookInformationViewer.Models.Db.Context
{
    public interface IDbElement : IDbDeletion
    {
        int Id { get; set; }

        void Apply(object source);
    }
}