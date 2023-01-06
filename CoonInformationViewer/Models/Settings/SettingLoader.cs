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

        public bool IsCheckUpdate { get; set; }

        #endregion

        private SettingLoader()
        {
            _iniLoader = new IniLoader(CommonCoreLib.AppInfo.GetAppPath() + "\\settings.ini");

            Load();
        }

        public void Load()
        {
            IsCheckUpdate = _iniLoader.GetValue(MainClassName, nameof(IsCheckUpdate), true);
        }

        public void Save()
        {
            _iniLoader.SetValue(MainClassName, nameof(IsCheckUpdate), IsCheckUpdate);
        }
    }
}
