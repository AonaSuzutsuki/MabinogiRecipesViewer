namespace CookInformationViewer.Models.Db
{
    public class SelectValue
    {
        private readonly object _value;

        private readonly ColumnType _type;

        public SelectValue(object value, ColumnType type)
        {
            _value = value;
            _type = type;
        }

        public T GetValue<T>()
        {
            if (_value is T value)
                return value;
            return default;
        }

        public override bool Equals(object obj)
        {
            return obj is SelectValue selectValue && Equals(selectValue);
        }

        protected bool Equals(SelectValue other)
        {
            return Equals(_value, other._value) && _type == other._type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_value != null ? _value.GetHashCode() : 0) * 397 ^ (int)_type;
            }
        }

        public override string ToString()
        {
            return GetValue<string>();
        }
    }
}