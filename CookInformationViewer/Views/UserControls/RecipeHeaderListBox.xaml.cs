using CookInformationViewer.Models.DataValue;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CookInformationViewer.Views.UserControls
{
    /// <summary>
    /// RecipeHeaderListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class RecipeHeaderListBox : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty RecipesListProperty = DependencyProperty.Register(nameof(RecipesList),
            typeof(IEnumerable<RecipeHeader>),
            typeof(RecipeHeaderListBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MouseDoubleClickCommandProperty = DependencyProperty.Register(nameof(MouseDoubleClickCommand),
            typeof(ICommand),
            typeof(RecipeHeaderListBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register(nameof(SelectionChangedCommand),
            typeof(ICommand),
            typeof(RecipeHeaderListBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        public IEnumerable<RecipeHeader> RecipesList
        {
            get => (IEnumerable<RecipeHeader>)GetValue(RecipesListProperty);
            set => SetValue(RecipesListProperty, value);
        }

        public ICommand MouseDoubleClickCommand
        {
            get => (ICommand)GetValue(MouseDoubleClickCommandProperty);
            set => SetValue(MouseDoubleClickCommandProperty, value);
        }

        public ICommand SelectionChangedCommand
        {
            get => (ICommand)GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }

        public RecipeHeaderListBox()
        {
            InitializeComponent();
        }

        public ScrollViewer? GetScrollViewer()
        {
            if (RecipesListBox.Template.FindName("PART_ContentHost", RecipesListBox) is not ScrollViewer scrollViewer)
                return null;

            return scrollViewer;
        }

        public void ScrollItem()
        {
            if (RecipesListBox.Template.FindName("PART_ContentHost", RecipesListBox) is not ScrollViewer scrollViewer)
                return;

            var recipes = new List<RecipeHeader>(RecipesList);

            var halfList = scrollViewer.ViewportHeight / 2;
            var item = recipes.FirstOrDefault(x => x.Recipe.IsSelected);
            if (item == null)
                return;

            var index = recipes.IndexOf(item);
            var scrollOffset = index - halfList;
            if (scrollOffset > scrollViewer.ScrollableHeight)
            {
                scrollOffset = scrollViewer.ScrollableHeight;
            }
            scrollViewer.ScrollToVerticalOffset(scrollOffset);
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            MouseDoubleClickCommand?.Execute(listBox.SelectedItem);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            SelectionChangedCommand?.Execute(listBox.SelectedItem);
        }
    }
}
