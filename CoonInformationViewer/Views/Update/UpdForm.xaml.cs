using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CookInformationViewer.ViewModels.Update;

namespace CookInformationViewer.Views.Update
{
    /// <summary>
    /// UpdForm.xaml の相互作用ロジック
    /// </summary>
    public partial class UpdForm : Window
    {
        public UpdForm()
        {
            InitializeComponent();
        }

        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as UpdFormViewModel;
            var hyperLink = sender as Hyperlink;
            if (vm == null || hyperLink == null)
                return;

            var url = hyperLink.NavigateUri.ToString();

            vm.OpenLinkCommand.Execute(url);
        }
    }
}
