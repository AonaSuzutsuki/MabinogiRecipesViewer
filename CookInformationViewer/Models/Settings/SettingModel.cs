using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStyleLib.Models;
using Reactive.Bindings;

namespace CookInformationViewer.Models.Settings
{
    public class SettingModel : ModelBase
    {

        #region Fields

        private SettingLoader _settingLoader = SettingLoader.Instance;

        private bool _isCheckAutoData;
        private bool _isCheckProgram;

        #endregion

        #region Properties

        public bool IsCheckAutoData
        {
            get => _isCheckAutoData;
            set => SetProperty(ref _isCheckAutoData, value);
        }

        public bool IsCheckProgram
        {
            get => _isCheckProgram;
            set => SetProperty(ref _isCheckProgram, value);
        }

        #endregion

        public SettingModel()
        {
            IsCheckAutoData = _settingLoader.IsCheckDataUpdate;
            IsCheckProgram = _settingLoader.IsCheckProgramUpdate;
        }

        public void ResetOverlayPosition()
        {
            _settingLoader.OverlayLeft = SettingLoader.DefaultOverlayLeft;
            _settingLoader.OverlayTop = SettingLoader.DefaultOverlayTop;
        }

        public void Save()
        {
            _settingLoader.IsCheckDataUpdate = IsCheckAutoData;
            _settingLoader.IsCheckProgramUpdate = IsCheckProgram;

            _settingLoader.Save();
        }
    }
}
