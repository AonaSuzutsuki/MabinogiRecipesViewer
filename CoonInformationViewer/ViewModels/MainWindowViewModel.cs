using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CoonInformationViewer.Models;
using CoonInformationViewer.Views;
using Prism.Commands;

namespace CoonInformationViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private readonly MainWindowModel _model;

        #endregion

        #region Event Properties

        public ICommand RebuildDataBaseCommand { get; set; }
        public ICommand UpdateTableCommand { get; set; }

        #endregion

        public MainWindowViewModel(IWindowService windowService, MainWindowModel model) : base(windowService, model)
        {
            _model = model;

            RebuildDataBaseCommand = new DelegateCommand(RebuildDataBase);
            UpdateTableCommand = new DelegateCommand(UpdateTable);
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
            var model = new TableDownloadModel(_model.AvailableTableUpdate, _model.CurrentTableVersion());
            var vm = new TableDownloadViewModel(new WindowService(), model);
            WindowManageService.ShowDialog<TableDownloadView>(vm);
        }
    }
}
