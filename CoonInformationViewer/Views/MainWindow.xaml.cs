using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CookInformationViewer.Models;
using CookInformationViewer.ViewModels;
using CookInformationViewer.Views.WindowService;

namespace CookInformationViewer.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window , IGaugeResize
    {
        public MainWindow()
        {
            InitializeComponent();

            var model = new MainWindowModel();
            var vm = new MainWindowViewModel(new MainWindowWindowService(this), model);
            DataContext = vm;
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

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
