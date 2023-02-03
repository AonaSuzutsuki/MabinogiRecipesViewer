using System;
using System.Reflection;
using System.Windows;
using CommonStyleLib.ExMessageBox;
using CommonStyleLib.Models;

namespace CookInformationViewer.Models
{
    public class VersionInfoModel : ModelBase
    {

        private string _version = string.Empty;
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private string _copyright = string.Empty;

        public string Copyright
        {
            get => _copyright;
            set => SetProperty(ref _copyright, value);
        }

        public void SetVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            //var ver = asm.GetName().Version;
            Copyright = Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute))
                is AssemblyCopyrightAttribute copyrightAttribute ? copyrightAttribute.Copyright : string.Empty;
            Version = Constants.Version; //ver.ToString() + "b";
        }
    }
}
