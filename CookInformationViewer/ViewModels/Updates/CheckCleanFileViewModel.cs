using System.Windows.Input;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models.Updates;
using CookInformationViewer.Models.Updates.Node;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels.Updates
{
    public class CheckCleanFileViewModel : ViewModelWindowStyleBase
    {
        private readonly CheckCleanFileModel _model;

        public ReadOnlyReactiveCollection<DirectoryNode> TreeViewItems { get; set; }

        public ICommand AllSelectCommand { get; set; }
        public ICommand AllDeSelectCommand { get; set; }
        public ICommand DoUpdateCommand { get; set; }

        public CheckCleanFileViewModel(IWindowService windowService, CheckCleanFileModel model) : base(windowService, model)
        {
            _model = model;

            TreeViewItems = model.TreeViewItems.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);

            AllSelectCommand = new DelegateCommand(AllSelect);
            AllDeSelectCommand = new DelegateCommand(AllDeSelect);
            DoUpdateCommand = new DelegateCommand(DoUpdate);
        }

        public void AllSelect()
        {
            _model.SetAllDeleteTarget();
        }

        public void AllDeSelect()
        {
            _model.SetAllNotDeleteTarget();
        }

        public void DoUpdate()
        {
            _model.CanCleanUpdate = true;
            WindowManageService.Close();
        }
    }
}
