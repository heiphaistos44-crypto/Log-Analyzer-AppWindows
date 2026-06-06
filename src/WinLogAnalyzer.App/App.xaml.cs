using System.Windows;
using System.Windows.Threading;

namespace WinLogAnalyzer.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Filet de securite : aucune exception UI ne doit fermer l'app sans message.
        DispatcherUnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Erreur inattendue :\n\n{e.Exception.Message}",
            "WinLog Analyzer",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
