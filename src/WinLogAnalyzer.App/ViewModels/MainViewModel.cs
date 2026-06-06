using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using WinLogAnalyzer.App.Infrastructure;
using WinLogAnalyzer.Core.Knowledge;
using WinLogAnalyzer.Core.Process;
using WinLogAnalyzer.Core.Reader;

namespace WinLogAnalyzer.App.ViewModels;

/// <summary>ViewModel principal : pilote l'analyse, le filtre et les statistiques.</summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly SolutionProvider _solutions;

    private string _selectedLog = "System";
    private int _maxCount = 100;
    private string _searchText = "";
    private string _statusText = "Pret.";
    private bool _isLoading;

    public MainViewModel()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data", "solutions.json");
        _solutions = new SolutionProvider(path);

        Events = new ObservableCollection<EventItemViewModel>();
        EventsView = CollectionViewSource.GetDefaultView(Events);
        EventsView.Filter = o => o is EventItemViewModel vm && vm.Matches(_searchText);

        AnalyzeCommand = new RelayCommand(async () => await AnalyzeAsync(), () => !_isLoading);

        StatusText = $"Pret. {_solutions.Count} solutions chargees.";
    }

    public ObservableCollection<EventItemViewModel> Events { get; }
    public ICollectionView EventsView { get; }
    public RelayCommand AnalyzeCommand { get; }

    public string[] AvailableLogs { get; } = { "System", "Application", "Security" };

    public string SelectedLog
    {
        get => _selectedLog;
        set => SetField(ref _selectedLog, value);
    }

    public int MaxCount
    {
        get => _maxCount;
        set => SetField(ref _maxCount, Math.Clamp(value, 1, 1000));
    }

    public string SearchText
    {
        get => _searchText;
        set { if (SetField(ref _searchText, value)) EventsView.Refresh(); }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetField(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsNotLoading));
                AnalyzeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsNotLoading => !_isLoading;

    // --- Statistiques ---
    public int TotalCount => Events.Count;
    public int CriticalCount => Events.Count(e => e.Level == "Critical");
    public int ErrorCount => Events.Count(e => e.Level == "Error");
    public int SolvedCount => Events.Count(e => e.HasSolution);

    private async Task AnalyzeAsync()
    {
        IsLoading = true;
        StatusText = $"Analyse du journal {SelectedLog}…";
        Events.Clear();
        RaiseStats();

        try
        {
            string log = SelectedLog;
            int max = MaxCount;

            // Lecture hors thread UI (cache PID frais a chaque analyse).
            var entries = await Task.Run(() =>
            {
                var service = new EventLogService(new ProcessResolver(), _solutions);
                return service.GetRecentCritical(log, max);
            });

            foreach (var e in entries)
                Events.Add(new EventItemViewModel(e));

            EventsView.Refresh();
            RaiseStats();
            StatusText = TotalCount == 0
                ? "Aucune erreur critique trouvee."
                : $"{TotalCount} evenements · {CriticalCount} critiques · {SolvedCount} avec solution.";
        }
        catch (InvalidOperationException ex)
        {
            StatusText = $"Erreur : {ex.Message}";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur inattendue : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RaiseStats()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(CriticalCount));
        OnPropertyChanged(nameof(ErrorCount));
        OnPropertyChanged(nameof(SolvedCount));
    }
}
