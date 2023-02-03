using CookInformationViewer.Models.Db.Context;
using System.Windows;
using System.Xml.Linq;
using CookInformationViewer.Views.WindowServices;

namespace CookInformationViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window, IGaugeResize
    {
        public Overlay()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };
        }

        public void SetGaugeLength(double length, int number)
        {
            switch (number)
            {
                case 0:
                    Item1Column.Width = new GridLength(length, GridUnitType.Star);
                    break;
                case 1:
                    Item2Column.Width = new GridLength(length, GridUnitType.Star);
                    break;
                case 2:
                    Item3Column.Width = new GridLength(length, GridUnitType.Star);
                    break;
            }
        }
    }
}
