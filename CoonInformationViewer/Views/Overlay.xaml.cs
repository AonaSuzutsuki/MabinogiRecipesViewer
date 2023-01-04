using CoonInformationViewer.Models.Db.Context;
using System.Windows;
using System.Xml.Linq;

namespace CoonInformationViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        public Overlay()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };

            

            Item1Column.Width = new GridLength(10, GridUnitType.Star);
            Item2Column.Width = new GridLength(20.5, GridUnitType.Star);
            Item3Column.Width = new GridLength(69.5, GridUnitType.Star);

        }
    }
}
