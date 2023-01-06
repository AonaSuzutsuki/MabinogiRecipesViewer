using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using CookInformationViewer.Views.WindowService;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;


namespace CookInformationViewer.ViewModels
{
    public class OverlayViewModel : ViewModelBase
    {
        private MainWindowWindowService _mainWindowService;

        public ReactiveProperty<RecipeInfo?> SelectedRecipe { get; set; }

        public OverlayViewModel(MainWindowWindowService windowService, OverlayModel model) : base(windowService, model)
        {
            _mainWindowService = windowService;

            SelectedRecipe = model.ObserveProperty(x => x.SelectedRecipe).ToReactiveProperty()
                .AddTo(CompositeDisposable);
            SelectedRecipe.PropertyChanged += (s, e) =>
            {
                if (model.SelectedRecipe == null || windowService.GaugeResize == null)
                    return;

                windowService.GaugeResize.SetGaugeLength((double)model.SelectedRecipe.Item1Amount, 0);
                windowService.GaugeResize.SetGaugeLength((double)model.SelectedRecipe.Item2Amount, 1);
                windowService.GaugeResize.SetGaugeLength((double)model.SelectedRecipe.Item3Amount, 2);
            };
        }
    }
}
