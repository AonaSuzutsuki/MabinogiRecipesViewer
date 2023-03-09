using CookInformationViewer.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CookInformationViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IDisposable? _mainWindow;

        private void MyApp_Startup(object sender, StartupEventArgs e)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            DispatcherUnhandledException += Application_DispatcherUnhandledException;

            var mainWindow = new MainWindow();
            _mainWindow = mainWindow;
            mainWindow.Show();
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                ShowAndWriteException(exception);
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs? e)
        {
            var exception = e.Exception.InnerException;
            ShowAndWriteException(exception);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowAndWriteException(e.Exception);
            _mainWindow?.Dispose();

            e.Handled = true;
            Shutdown();
        }

        public static void ShowAndWriteException(Exception? exception)
        {
            if (exception == null)
                return;

            var mes = string.Format("予期せぬエラーが発生しました。\r\nお手数ですが、開発者に例外内容を報告してください。\r\n\r\n---\r\n\r\n{0}\r\n\r\n{1}",
                exception.Message, exception.StackTrace);
            MessageBox.Show(mes, "予期せぬエラー", MessageBoxButton.OK, MessageBoxImage.Error);

            var dt = DateTime.Now;
            OutToFile("error-" + dt.ToString("yyyy-MM-dd- HH-mm-ss") + ".log", mes);
        }

        private static void OutToFile(string filename, string text)
        {
            var dirName = "errors";
            var dirInfo = new DirectoryInfo(dirName);
            if (!dirInfo.Exists)
                dirInfo.Create();

            using var fs = new FileStream($"{dirInfo.FullName}\\{filename}", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.Write(text);
        }
    }
}
