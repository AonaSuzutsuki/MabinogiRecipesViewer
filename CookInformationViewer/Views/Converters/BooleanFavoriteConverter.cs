using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CookInformationViewer.Views.Converters
{
    public class BooleanFavoriteConverter : MarkupExtension, IValueConverter
    {
        private const string Favorite = "★";
        private const string NonFavorite = "☆";

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool isVisible)
                return NonFavorite;

            return isVisible ? Favorite : NonFavorite;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
                return false;

            return text == Favorite ? true : false;
        }
    }
}
