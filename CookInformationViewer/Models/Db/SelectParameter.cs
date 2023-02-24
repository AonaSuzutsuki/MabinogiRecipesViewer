namespace CookInformationViewer.Models.Db
{
    public struct SelectParameter
    {
        public string ColumnName { get; set; }
        public object? Value { get; set; }
    }
}