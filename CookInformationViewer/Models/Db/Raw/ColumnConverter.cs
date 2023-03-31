namespace CookInformationViewer.Models.Db.Raw
{
    public static class ColumnConverter
    {
        public static ColumnType ToColumnType(string? text)
        {
            return text.ToLower() switch
            {
                "integer" => ColumnType.Integer,
                "text" => ColumnType.Text,
                "blob" => ColumnType.Blob,
                "real" => ColumnType.Real,
                "numeric" => ColumnType.Numeric,
                _ => ColumnType.Undefined
            };
        }
    }
}