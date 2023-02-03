using System.Windows.Input;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels
{
    public class VersionInfoViewModel : ViewModelWindowStyleBase
    {
        private readonly VersionInfoModel _model;

        #region Properties
        public ReactiveProperty<string?> VersionLabel { get; set; }
        public ReactiveProperty<string?> Copyright { get; set; }
        #endregion

        #region EventProperties
        #endregion

        public VersionInfoViewModel(WindowService windowService, VersionInfoModel model) : base(windowService, model)
        {
            this._model = model;

            Loaded = new DelegateCommand(Window_Loaded);

            VersionLabel = model.ObserveProperty(m => m.Version).ToReactiveProperty().AddTo(CompositeDisposable);
            Copyright = model.ObserveProperty(m => m.Copyright).ToReactiveProperty().AddTo(CompositeDisposable);
        }

        #region EventMethods
        public void Window_Loaded()
        {
            _model.SetVersion();
        }
        #endregion
    }
}
