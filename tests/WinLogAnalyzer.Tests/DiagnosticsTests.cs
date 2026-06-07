using WinLogAnalyzer.Core.Diagnostics;
using WinLogAnalyzer.Core.Models;
using Xunit;

namespace WinLogAnalyzer.Tests;

internal static class Make
{
    public static EventEntry Event(int id, string level, DateTime time, string source = "Src")
        => new()
        {
            EventId = id,
            Level = level,
            LogName = "System",
            Source = source,
            TimeCreated = time,
            Message = $"msg {id}",
            ProcessName = "proc"
        };
}

public class EventGrouperTests
{
    [Fact]
    public void Deduplicate_merges_same_id_and_source_with_count()
    {
        var t = new DateTime(2026, 6, 7, 10, 0, 0);
        var list = new[]
        {
            Make.Event(41, "Critical", t),
            Make.Event(41, "Critical", t.AddMinutes(5)),
            Make.Event(41, "Critical", t.AddMinutes(10)),
            Make.Event(10, "Error", t),
        };

        var grouped = EventGrouper.Deduplicate(list);

        Assert.Equal(2, grouped.Count);
        var g41 = grouped.First(e => e.EventId == 41);
        Assert.Equal(3, g41.Count);
        Assert.Equal(t.AddMinutes(10), g41.TimeCreated); // occurrence la plus recente
    }

    [Fact]
    public void Deduplicate_keeps_distinct_sources_separate()
    {
        var t = DateTime.Now;
        var list = new[]
        {
            Make.Event(1000, "Error", t, "AppA"),
            Make.Event(1000, "Error", t, "AppB"),
        };
        Assert.Equal(2, EventGrouper.Deduplicate(list).Count);
    }
}

public class CorrelatorTests
{
    [Fact]
    public void Correlate_groups_events_within_window()
    {
        var t = new DateTime(2026, 6, 7, 2, 0, 0);
        var list = new[]
        {
            Make.Event(41, "Critical", t),
            Make.Event(6008, "Error", t.AddSeconds(20)),
            Make.Event(1001, "Critical", t.AddSeconds(40)),
            // trou > fenetre -> nouvel incident potentiel (isole donc ignore)
            Make.Event(7000, "Error", t.AddHours(3)),
        };

        var incidents = Correlator.Correlate(list, windowSeconds: 60);

        Assert.Single(incidents);
        Assert.Equal(3, incidents[0].Events.Count);
        Assert.Equal(2, incidents[0].CriticalCount);
    }

    [Fact]
    public void Correlate_ignores_isolated_events()
    {
        var t = DateTime.Now;
        var list = new[]
        {
            Make.Event(1, "Error", t),
            Make.Event(2, "Error", t.AddMinutes(30)),
        };
        Assert.Empty(Correlator.Correlate(list, 60));
    }

    [Fact]
    public void Correlate_empty_returns_empty()
        => Assert.Empty(Correlator.Correlate(Array.Empty<EventEntry>(), 60));
}
