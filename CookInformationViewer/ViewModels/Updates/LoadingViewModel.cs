using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models.Updates;

namespace CookInformationViewer.ViewModels.Updates
{
    public class LoadingViewModel : ViewModelBase
    {
        private readonly LoadingModel _model;
        public LoadingViewModel(WindowService windowService, LoadingModel model) : base(windowService, model)
        {
            _model = model;
        }
    }
}
