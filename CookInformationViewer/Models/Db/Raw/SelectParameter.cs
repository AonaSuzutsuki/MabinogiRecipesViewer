namespace CookInformationViewer.Models.Db.Raw
{
    public struct SelectParameter
    {
        public string ColumnName { get; set; }
        public object? Value { get; set; }
    }
}