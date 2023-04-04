using CommonStyleLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookInformationViewer.Models.Settings;
using CookInformationViewer.Models.DataValue;

namespace CookInformationViewer.Models
{
    public class OverlayModel : ModelBase
    {
        private SettingLoader _setting;

        private RecipeInfo? _selectedRecipe;

        public RecipeInfo? SelectedRecipe
        {
            get => _selectedRecipe;
            set => SetProperty(ref _selectedRecipe, value);
        }

        public bool Closed { get; set; }

        public OverlayModel()
        {
            _setting = SettingLoader.Instance;

            LoadSetting();
        }

        public void LoadSetting()
        {
            var overlayLeft = _setting.OverlayLeft;
            var overlayTop = _setting.OverlayTop;

            Left = overlayLeft;
            Top = overlayTop;
        }

        public void SaveSetting()
        {
            _setting.OverlayLeft = Left;
            _setting.OverlayTop = Top;

            _setting.Save();
        }
    }
}
