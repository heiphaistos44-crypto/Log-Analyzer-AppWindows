using WinLogAnalyzer.Core.Tasks;
using Xunit;

namespace WinLogAnalyzer.Tests;

public class IsFailureTests
{
    [Theory]
    [InlineData(0u, false)]            // succes
    [InlineData(0x41301u, false)]      // SCHED_S en cours
    [InlineData(0x41300u, false)]      // SCHED_S pret
    [InlineData(0x800710E0u, false)]   // conditions non remplies (benin)
    [InlineData(0x10000000u, false)]   // code applicatif positif
    [InlineData(0x80070005u, true)]    // acces refuse
    [InlineData(0x80070002u, true)]    // fichier introuvable
    [InlineData(0xC0000005u, true)]    // violation d'acces
    public void IsFailure_classifies_correctly(uint code, bool expected)
        => Assert.Equal(expected, Win32ErrorDecoder.IsFailure(code));
}

public class DecoderBuildTests
{
    [Fact]
    public void Build_with_provided_message_uses_it()
    {
        var s = Win32ErrorDecoder.Build(unchecked((int)0x80070002), "Fichier introuvable.");
        Assert.Contains("0x80070002", s.Title);
        Assert.Contains("Fichier introuvable", s.Explanation);
        Assert.False(string.IsNullOrWhiteSpace(s.Remediation));
    }

    [Fact]
    public void Build_without_message_falls_back_to_system()
    {
        // FormatMessage doit fournir un message pour un code Win32 standard.
        var s = Win32ErrorDecoder.Describe(unchecked((int)0x80070005));
        Assert.False(string.IsNullOrWhiteSpace(s.Title));
        Assert.False(string.IsNullOrWhiteSpace(s.Remediation));
        Assert.Equal("error", s.Severity);
    }

    [Fact]
    public void Describe_unknown_app_code_still_returns_solution()
    {
        var s = Win32ErrorDecoder.Describe(0x10000000);
        Assert.False(string.IsNullOrWhiteSpace(s.Title));
        Assert.Equal("info", s.Severity); // bit de severite a 0
    }

    [Fact]
    public void TryDecodeFromText_finds_embedded_code()
    {
        var s = Win32ErrorDecoder.TryDecodeFromText("Echec avec le code 0x80070005 lors de l'operation.");
        Assert.NotNull(s);
        Assert.Contains("detecte dans le message", s!.Title);
    }

    [Fact]
    public void TryDecodeFromText_ignores_text_without_code()
    {
        Assert.Null(Win32ErrorDecoder.TryDecodeFromText("Aucun code ici."));
    }
}

public class RemediationEngineTests
{
    [Theory]
    [InlineData(0x80070005u, "autorisations")]   // acces refuse
    [InlineData(0x80070002u, "chemin")]          // fichier introuvable
    [InlineData(0x80070020u, "verrouille")]      // sharing violation
    [InlineData(0x00000642u, "")]                // un code MSI quelconque -> non vide
    [InlineData(0x80040154u, "regsvr32")]        // COM class not registered (facilite COM)
    [InlineData(0xC0000005u, "RAM")]             // NTSTATUS violation d'acces
    [InlineData(0x80240022u, "Windows Update")]  // WU
    public void Remediate_returns_targeted_steps(uint code, string mustContain)
    {
        var r = RemediationEngine.Remediate(code);
        Assert.False(string.IsNullOrWhiteSpace(r));
        if (!string.IsNullOrEmpty(mustContain))
            Assert.Contains(mustContain, r, System.StringComparison.OrdinalIgnoreCase);
    }
}
