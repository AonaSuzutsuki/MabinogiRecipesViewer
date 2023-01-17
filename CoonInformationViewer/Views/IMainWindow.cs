using CookInformationViewer.Views.WindowServices;

namespace CookInformationViewer.Views;

public interface IMainWindow : IGaugeResize
{
    bool IsSearched { get; set; }
    void ScrollItem();
    void WindowFocus();
}