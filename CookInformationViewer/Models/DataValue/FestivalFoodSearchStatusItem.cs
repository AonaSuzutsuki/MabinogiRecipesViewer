using CookInformationViewer.Models.Searchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CookInformationViewer.Models.DataValue
{
    public class FestivalFoodSearchStatusItem
    {
        public string Name { get; set; }

        public SearchStatusItem? SearchStatusItem { get; set; }
        public System.Windows.Media.Brush StarBrush
        {
            get
            {
                return Name switch
                {
                    "お気に入り" => Constants.FavoriteForeground,
                    _ => new SolidColorBrush(Colors.White)
                };
            }
        }

        public FestivalFoodSearchStatusItem(SearchStatusItem statusItem)
        {
            SearchStatusItem = statusItem;
            Name = statusItem.Name;
        }
        public FestivalFoodSearchStatusItem(string name)
        {
            Name = name;
        }
    }
}
