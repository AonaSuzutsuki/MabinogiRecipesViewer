using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using CommonStyleLib.Views;
using CookInformationViewer.Models;
using CookInformationViewer.Views.WindowServices;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;


namespace CookInformationViewer.ViewModels
{
    public class OverlayViewModel : ViewModelWindowStyleBase
    {
        private MainWindowWindowService _mainWindowService;
        private OverlayModel _model;

        public ReactiveProperty<RecipeInfo?> SelectedRecipe { get; set; }

        public ReactiveProperty<double> Opacity { get; set; }
        public ReactiveProperty<Visibility> TransparentButtonVisibility { get; set; }
        public ReactiveProperty<Brush> WindowBackground { get; set; }
        public ReactiveProperty<bool> TransparentChecked { get; set; }

        public ICommand DoTransparentCommand { get; set; }
        public ICommand ChangeTransparentCommand { get; set; }

        public OverlayViewModel(MainWindowWindowService windowService, OverlayModel model) : base(windowService, model)
        {
            _mainWindowService = windowService;
            _model = model;

            SelectedRecipe = model.ObserveProperty(x => x.SelectedRecipe).ToReactiveProperty()
                .AddTo(CompositeDisposable);
            SelectedRecipe.PropertyChanged += (s, e) =>
            {
                if (model.SelectedRecipe == null || windowService.GaugeResize == null)
                    return;

                //windowService.GaugeResize.SetGaugeLength((double)Math.Ceiling((244 * (model.SelectedRecipe.Item1Amount / 100))), 0);
                //windowService.GaugeResize.SetGaugeLength((double)Math.Ceiling((244 * (model.SelectedRecipe.Item2Amount / 100))), 1);
                //windowService.GaugeResize.SetGaugeLength((double)Math.Ceiling((244 * (model.SelectedRecipe.Item3Amount / 100))), 2);
                windowService.GaugeResize.SetGaugeLength((double)model.SelectedRecipe.Item1Amount, 0);
                windowService.GaugeResize.SetGaugeLength((double)model.SelectedRecipe.Item2Amount, 1);
                windowService.GaugeResize.SetGaugeLength((double)model.SelectedRecipe.Item3Amount, 2);
            };
            Opacity = new ReactiveProperty<double>(1.0);
            TransparentButtonVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);

            WindowBackground = new ReactiveProperty<Brush>();
            TransparentChecked = new ReactiveProperty<bool>(false);

            DoTransparentCommand = new DelegateCommand(DoTransparent);
            ChangeTransparentCommand = new DelegateCommand<bool?>(ChangeTransparent);
        }

        protected override void MainWindow_Loaded()
        {
            base.MainWindow_Loaded();

            var background = WindowManageService.Owner.Resources["MainColor"];
            if (background is Brush brush)
                WindowBackground.Value = brush;
        }

        public void DoTransparent()
        {
            Opacity.Value = .2;
            TransparentButtonVisibility.Value = Visibility.Collapsed;
            WindowBackground.Value = new SolidColorBrush(Colors.Transparent);
            //WindowManageService.Owner.BorderBrush = new SolidColorBrush(Colors.Transparent);
            TransparentChecked.Value = true;
        }

        public void ChangeTransparent(bool? arg)
        {
            if (arg == null)
                return;

            var boolValue = arg.Value;
            if (boolValue)
            {
                DoTransparent();
            }
            else
            {
                Opacity.Value = 1.0;
                TransparentButtonVisibility.Value = Visibility.Visible;

                var background = WindowManageService.Owner.Resources["MainColor"];
                if (background is Brush brush)
                    WindowBackground.Value = brush;
                else
                    WindowBackground.Value = new SolidColorBrush(Colors.Transparent);

                //if (WindowManageService.Owner.FindResource("AroundBorderColor") is SolidColorBrush color)
                //    WindowManageService.Owner.BorderBrush = color;
            }
        }

        protected override void MainWindowCloseBt_Click()
        {
            _mainWindowService.MainWindow?.WindowFocus();

            base.MainWindowCloseBt_Click();
        }

        public override void Dispose()
        {
            base.Dispose();

            _model.SaveSetting();
            _model.Closed = true;
        }
    }
}
