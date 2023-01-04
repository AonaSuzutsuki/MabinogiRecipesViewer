using System.Text;

namespace CoonInformationViewer.Models.Db
{
    public class ColumnInfo
    {
        public TableInfo Table { get; internal set; }

        public string ColumnName { get; set; }
        public ColumnType Type { get; set; }
        public bool PrimaryKey { get; set; }
        public bool NotNull { get; set; }
        public bool Unique { get; set; }
        public bool AutoIncrement { get; set; }
        public object Default { get; set; }

        public ColumnInfo ForeignKey { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ColumnInfo columnInfo && Equals(columnInfo);
        }

        protected bool Equals(ColumnInfo other)
        {
            return Equals(Table, other.Table) &&
                   ColumnName == other.ColumnName &&
                   Type == other.Type &&
                   PrimaryKey == other.PrimaryKey &&
                   NotNull == other.NotNull &&
                   Unique == other.Unique &&
                   AutoIncrement == other.AutoIncrement &&
                   Default == other.Default &&
                   Equals(ForeignKey, other.ForeignKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Table != null ? Table.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (ColumnName != null ? ColumnName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ PrimaryKey.GetHashCode();
                hashCode = (hashCode * 397) ^ NotNull.GetHashCode();
                hashCode = (hashCode * 397) ^ Unique.GetHashCode();
                hashCode = (hashCode * 397) ^ AutoIncrement.GetHashCode();
                hashCode = (hashCode * 397) ^ (Default != null ? Default.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ForeignKey != null ? ForeignKey.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{ColumnName} {Type.ToString().ToLower()}");
            if (NotNull)
                sb.Append(" not null");
            if (Unique)
                sb.Append(" unique");
            if (Default != null)
                sb.Append(Type == ColumnType.Text ? $" default '{Default}'" : $" default {Default}");
            return sb.ToString();
        }
    }
}