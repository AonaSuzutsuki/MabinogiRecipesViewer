using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models.Searchers;
using CookInformationViewer.Views.Searches;

namespace CookInformationViewer.ViewModels.Searchers
{
    public class SearchWindowViewModel : ViewModelWindowStyleBase
    {
        public SearchWindowViewModel(IWindowService windowService, SearchWindowModel model) : base(windowService, model)
        {
        }
    }
}
