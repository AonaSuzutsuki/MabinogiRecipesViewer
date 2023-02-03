using System.Windows;
using System.Windows.Controls;

namespace CookInformationViewer.Views.UserControls
{
    public class BindableSelectedItemTreeView : TreeView
    {
        #region Fields
        public static readonly DependencyProperty BindableSelectedItemProperty
            = DependencyProperty.Register(nameof(BindableSelectedItem),
                typeof(object), typeof(BindableSelectedItemTreeView), new UIPropertyMetadata(null));

        //public static readonly DependencyProperty IgnoreNullSelectedItemProperty
        //    = DependencyProperty.Register(nameof(IgnoreNullSelectedItem),
        //        typeof(bool), typeof(BindableSelectedItemTreeView), new UIPropertyMetadata(false));

        public static readonly DependencyProperty IgnoreNullSelectedItemProperty =
            DependencyProperty.Register(
                nameof(IgnoreNullSelectedItem), // プロパティ名を指定
                typeof(bool), // プロパティの型を指定
                typeof(BindableSelectedItemTreeView), // プロパティを所有する型を指定
                new PropertyMetadata(true));
        #endregion

        #region Properties
        public object BindableSelectedItem
        {
            get => GetValue(BindableSelectedItemProperty);
            set => SetValue(BindableSelectedItemProperty, value);
        }
        public bool IgnoreNullSelectedItem
        {
            get => (bool)GetValue(IgnoreNullSelectedItemProperty);
            set => SetValue(IgnoreNullSelectedItemProperty, value);
        }
        #endregion

        #region Constructors
        public BindableSelectedItemTreeView()
        {
            SelectedItemChanged += OnSelectedItemChanged;
        }
        #endregion

        #region Event Methods
        protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IgnoreNullSelectedItem && SelectedItem == null)
            {
                return;
            }

            SetValue(BindableSelectedItemProperty, SelectedItem);
        }
        #endregion
    }
}
