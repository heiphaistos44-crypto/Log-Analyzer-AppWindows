using System.Diagnostics;
using System.Windows;

namespace WinLogAnalyzer.App.Infrastructure;

/// <summary>
/// Lance des outils Windows de diagnostic. Les actions a effet de bord (reboot, scan disque)
/// demandent une confirmation explicite. Aucune action destructive automatique.
/// </summary>
public static class ActionRunner
{
    // H10 — liste blanche des outils autorisés (prévient l'exécution arbitraire de processus)
    private static readonly HashSet<string> AllowedTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "services.msc",
        "perfmon.msc",
        "perfmon",
        "eventvwr.msc",
        "mdsched.exe",
        "taskmgr.exe",
        "resmon.exe",
        "compmgmt.msc",
        "devmgmt.msc",
        "diskmgmt.msc"
    };

    /// <summary>Lance un outil sans confirmation (ouverture d'une console d'admin).</summary>
    public static void Open(string fileName, string? args = null)
    {
        // Vérification whitelist : seul le nom de fichier (sans chemin) est comparé
        var name = System.IO.Path.GetFileName(fileName);
        if (!AllowedTools.Contains(name) && !AllowedTools.Contains(fileName))
        {
            MessageBox.Show($"Outil '{fileName}' non autorisé.",
                "WinLog Analyzer", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args ?? "",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Impossible de lancer '{fileName}' :\n{ex.Message}",
                "WinLog Analyzer", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>Lance une action a effet de bord apres confirmation utilisateur.</summary>
    public static void RunWithConfirm(string title, string warning, string fileName, string? args = null)
    {
        var r = MessageBox.Show(
            warning + "\n\nContinuer ?",
            title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes)
            Open(fileName, args);
    }
}
