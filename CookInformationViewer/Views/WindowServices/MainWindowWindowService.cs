namespace CookInformationViewer.Views.WindowServices;

public class MainWindowWindowService : CommonStyleLib.Views.WindowService
{
    public MainWindowWindowService(MainWindow window) : base(window)
    {
        GaugeResize = window;
        MainWindow = window;
    }

    public MainWindowWindowService()
    {
    }

    public IGaugeResize? GaugeResize { get; set; }

    public IMainWindow? MainWindow { get; set; }
}