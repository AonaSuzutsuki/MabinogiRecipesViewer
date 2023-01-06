using CommonStyleLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookInformationViewer.Models
{
    public class OverlayModel : ModelBase
    {
        private RecipeInfo? _selectedRecipe;

        public RecipeInfo? SelectedRecipe
        {
            get => _selectedRecipe;
            set => SetProperty(ref _selectedRecipe, value);
        }
    }
}
