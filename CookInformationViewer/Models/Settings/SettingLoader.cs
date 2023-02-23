using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonCoreLib.Ini;

namespace CookInformationViewer.Models.Settings
{
    public class SettingLoader
    {
        public const double DefaultOverlayLeft = 10;
        public const double DefaultOverlayTop = 10;

        public static SettingLoader Instance { get; } = new();

        private readonly IniLoader _iniLoader;

        private const string MainClassName = "Main";

        #region Properties

        public bool IsCheckDataUpdate { get; set; }
        public bool IsCheckProgramUpdate { get; set; }

        public double OverlayLeft { get; set; }
        public double OverlayTop { get; set; }

        #endregion

        private SettingLoader()
        {
            _iniLoader = new IniLoader(CommonCoreLib.AppInfo.GetAppPath() + "\\settings.ini");

            Load();
        }

        public void Load()
        {
            IsCheckDataUpdate = _iniLoader.GetValue(MainClassName, nameof(IsCheckDataUpdate), true);
            IsCheckProgramUpdate = _iniLoader.GetValue(MainClassName, nameof(IsCheckProgramUpdate), true);
            OverlayLeft = _iniLoader.GetValue(MainClassName, nameof(OverlayLeft), DefaultOverlayLeft);
            OverlayTop = _iniLoader.GetValue(MainClassName, nameof(OverlayTop), DefaultOverlayTop);
        }

        public void Save()
        {
            _iniLoader.SetValue(MainClassName, nameof(IsCheckDataUpdate), IsCheckDataUpdate);
            _iniLoader.SetValue(MainClassName, nameof(IsCheckProgramUpdate), IsCheckProgramUpdate);
            _iniLoader.SetValue(MainClassName, nameof(OverlayLeft), OverlayLeft);
            _iniLoader.SetValue(MainClassName, nameof(OverlayTop), OverlayTop);
        }
    }
}
