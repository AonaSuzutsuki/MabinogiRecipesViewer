using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonStyleLib.ExMessageBox;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using CookInformationViewer.Views;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private readonly MainWindowModel _model;

        #endregion

        #region Properties

        public ReadOnlyReactiveCollection<string> Categories { get; set; }

        #endregion

        #region Event Properties

        public ICommand RebuildDataBaseCommand { get; set; }
        public ICommand UpdateTableCommand { get; set; }

        #endregion

        public MainWindowViewModel(IWindowService windowService, MainWindowModel model) : base(windowService, model)
        {
            _model = model;

            Categories = model.Categories.ToReadOnlyReactiveCollection().AddTo(CompositeDisposable);

            RebuildDataBaseCommand = new DelegateCommand(RebuildDataBase);
            UpdateTableCommand = new DelegateCommand(UpdateTable);
        }

        protected override void MainWindow_Loaded()
        {
            base.MainWindow_Loaded();

            _ = _model.Initialize().ContinueWith(_ =>
            {
                if (!_model.AvailableUpdate)
                    return;

                var dr = WindowManageService.MessageBoxDispatchShow("Available Data Update. Do you want to update?",
                    "Available Data Update",
                    ExMessageBoxBase.MessageType.Exclamation, ExMessageBoxBase.ButtonType.YesNo);

                if (dr == ExMessageBoxBase.DialogResult.Yes)
                {
                    WindowManageService.Dispatch(UpdateTable);
                }
            });
        }

        public void RebuildDataBase()
        {
            _ = _model.RebuildDataBase().ContinueWith(t =>
            {
                var exp = t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void UpdateTable()
        {
            var model = new TableDownloadModel(_model);
            var vm = new TableDownloadViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<TableDownloadView>(vm);
        }

        public override void Dispose()
        {
            base.Dispose();

            _model.Dispose();
        }
    }
}
