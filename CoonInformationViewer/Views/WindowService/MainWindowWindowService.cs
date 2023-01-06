using System.Windows;
using CommonStyleLib.Views;

namespace CookInformationViewer.Views.WindowService;

public class MainWindowWindowService : CommonStyleLib.Views.WindowService
{
    public MainWindowWindowService(MainWindow window) : base(window)
    {
        GaugeResize = window;
    }

    public MainWindowWindowService()
    {
    }

    public IGaugeResize? GaugeResize { get; set; }
}