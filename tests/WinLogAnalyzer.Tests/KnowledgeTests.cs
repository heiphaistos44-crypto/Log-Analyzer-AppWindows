using WinLogAnalyzer.Core.Knowledge;
using WinLogAnalyzer.Core.Settings;
using WinLogAnalyzer.Core.Tasks;
using Xunit;

namespace WinLogAnalyzer.Tests;

public class SolutionProviderTests
{
    private static string WriteTemp(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"sol_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void Lookup_finds_by_event_id_and_composite_key()
    {
        var path = WriteTemp("""
        {
          "41": {"title":"t41","explanation":"e","remediation":"r","severity":"critical"},
          "Provider-X:99": {"title":"t99","explanation":"e","remediation":"r","severity":"error"}
        }
        """);
        try
        {
            var p = new SolutionProvider(path);
            Assert.NotNull(p.Lookup(41));
            Assert.Equal("t99", p.Lookup(99, "Provider-X")!.Title);  // cle composite
            Assert.Null(p.Lookup(99));                                // sans source -> absent
            Assert.Null(p.Lookup(12345));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Missing_file_yields_empty_provider()
    {
        var p = new SolutionProvider(Path.Combine(Path.GetTempPath(), "does_not_exist.json"));
        Assert.Equal(0, p.Count);
        Assert.Null(p.Lookup(1));
    }
}

public class ResultCodeProviderTests
{
    [Fact]
    public void Describe_falls_back_to_decoder_for_unknown_code()
    {
        var p = new ResultCodeProvider(Path.Combine(Path.GetTempPath(), "no_codes.json"));
        var s = p.Describe(unchecked((int)0x80070005)); // absent du json -> decodeur
        Assert.False(string.IsNullOrWhiteSpace(s.Title));
        Assert.False(string.IsNullOrWhiteSpace(s.Remediation));
    }
}

public class AppSettingsTests
{
    [Fact]
    public void SelectedLevels_defaults_to_critical_and_error()
    {
        var s = new AppSettings();
        Assert.Equal(new[] { 1, 2 }, s.SelectedLevels());
    }

    [Fact]
    public void SelectedLevels_never_empty()
    {
        var s = new AppSettings { LevelCritical = false, LevelError = false };
        Assert.Equal(new[] { 1, 2 }, s.SelectedLevels()); // fallback
    }

    [Fact]
    public void SelectedLogs_reflects_flags_and_never_empty()
    {
        var s = new AppSettings { LogSystem = false, LogApplication = false, LogSecurity = false };
        Assert.Equal(new[] { "System" }, s.SelectedLogs());

        var s2 = new AppSettings { LogSystem = true, LogApplication = true, LogSecurity = true };
        Assert.Equal(new[] { "System", "Application", "Security" }, s2.SelectedLogs());
    }
}
