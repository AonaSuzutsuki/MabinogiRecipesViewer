using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonStyleLib.ExMessageBox;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models.Settings;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CookInformationViewer.ViewModels.Settings
{
    public class SettingViewModel : ViewModelWindowStyleBase
    {

        #region Fields

        private readonly SettingModel _model;

        #endregion

        #region Properties

        public ReactiveProperty<bool> IsCheckAutoData { get; set; }
        public ReactiveProperty<bool> IsCheckProgram { get; set; }

        #endregion

        #region Event Properties

        public ICommand ResetOverlayPositionCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        #endregion

        public SettingViewModel(IWindowService windowService, SettingModel model) : base(windowService, model)
        {
            _model = model;

            IsCheckAutoData = model.ToReactivePropertyAsSynchronized(x => x.IsCheckAutoData).AddTo(CompositeDisposable);
            IsCheckProgram = model.ToReactivePropertyAsSynchronized(x => x.IsCheckProgram).AddTo(CompositeDisposable);

            ResetOverlayPositionCommand = new DelegateCommand(ResetOverlayPosition);
            SaveCommand = new DelegateCommand(Save);
        }

        public void ResetOverlayPosition()
        {
            _model.ResetOverlayPosition();

            WindowManageService.MessageBoxShow("オーバーレイウィンドウの座標をリセットしました。OKを押すと確定されます。",
                "リセット完了", ExMessageBoxBase.MessageType.Exclamation);
        }

        public void Save()
        {
            _model.Save();
            WindowManageService.Close();
        }
    }
}
