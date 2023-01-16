using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using CookInformationViewer.Models;
using CookInformationViewer.ViewModels;
using CookInformationViewer.Views.WindowServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CookInformationViewer.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IDisposable, IMainWindow
    {
        private readonly MainWindowModel _model;

        public bool IsSearched { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();

            var model = new MainWindowModel();
            var vm = new MainWindowViewModel(new MainWindowWindowService(this), model);
            DataContext = vm;
            _model = model;

            Loaded += (_, _) =>
            {
                if (RecipesListBox.Template.FindName("PART_ContentHost", RecipesListBox) is not ScrollViewer scrollViewer)
                    return;
                
                // If the recipe list is redrawn
                scrollViewer.ScrollChanged += (_, _) =>
                {
                    if (IsSearched)
                    {
                        ScrollItem();
                        IsSearched = false;
                    }
                };
            };
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

        public void ScrollItem()
        {
            if (RecipesListBox.Template.FindName("PART_ContentHost", RecipesListBox) is not ScrollViewer scrollViewer)
                return;

            var halfList = scrollViewer.ViewportHeight / 2;
            var item = _model.Recipes.FirstOrDefault(x => x.IsSelected);
            if (item == null)
                return;

            var index = _model.Recipes.IndexOf(item);
            var scrollOffset = index - halfList;
            if (scrollOffset > scrollViewer.ScrollableHeight)
            {
                scrollOffset = scrollViewer.ScrollableHeight;
            }
            scrollViewer.ScrollToVerticalOffset(scrollOffset);
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public void Dispose()
        {
            _model.Dispose();
        }
    }
}
