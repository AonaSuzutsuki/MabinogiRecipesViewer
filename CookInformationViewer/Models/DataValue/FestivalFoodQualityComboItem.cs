using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CookInformationViewer.Models.DataValue
{
    public class FestivalFoodQualityComboItem
    {
        public int Star { get; set; }
        public string Name { get; set; } = string.Empty;
        public Brush StarBrush
        {
            get
            {
                return Star switch
                {
                    6 => Constants.StarSixForeground,
                    _ => new SolidColorBrush(Colors.White)
                };
            }
        }
        public List<EffectInfo> Effects { get; set; } = new();
    }
}
