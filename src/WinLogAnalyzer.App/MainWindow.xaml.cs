using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using WinLogAnalyzer.App.ViewModels;

namespace WinLogAnalyzer.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        // Analyse automatique au demarrage.
        Loaded += (_, _) => _vm.AnalyzeCommand.Execute(null);
    }

    /// <summary>Ouvre les liens de documentation dans le navigateur par defaut.</summary>
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Impossible d'ouvrir le lien :\n{ex.Message}",
                "WinLog Analyzer", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        e.Handled = true;
    }
}
