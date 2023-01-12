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
        public static SettingLoader Instance => new();

        private readonly IniLoader _iniLoader;

        private const string MainClassName = "Main";

        #region Properties

        public bool IsCheckDataUpdate { get; set; }
        public bool IsCheckProgramUpdate { get; set; }

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
        }

        public void Save()
        {
            _iniLoader.SetValue(MainClassName, nameof(IsCheckDataUpdate), IsCheckDataUpdate);
            _iniLoader.SetValue(MainClassName, nameof(IsCheckProgramUpdate), IsCheckProgramUpdate);
        }
    }
}
