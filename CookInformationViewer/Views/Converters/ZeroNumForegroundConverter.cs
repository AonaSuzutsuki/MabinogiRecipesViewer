using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace CookInformationViewer.Views.Converters
{
    public class ZeroNumForegroundConverter : MarkupExtension, IValueConverter
    {
        private static readonly SolidColorBrush DefaultColor = new(Colors.White);
        private static readonly SolidColorBrush ZeroColor = new(Colors.DimGray);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int num)
                return DefaultColor;

            if (num > 0)
                return DefaultColor;

            return ZeroColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
