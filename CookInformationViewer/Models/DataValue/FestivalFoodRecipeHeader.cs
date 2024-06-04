using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookInformationViewer.Models.DataValue
{
    public class FestivalFoodRecipeHeader : RecipeHeader
    {
        public FestivalFoodRecipeHeader(RecipeInfo recipe) : base(recipe)
        {
        }

        public FestivalFoodRecipeHeader(string name) : base(name)
        {
        }

        public Dictionary<int, List<EffectInfo>> StarEffectMap { get; set; } = new();
    }
}
