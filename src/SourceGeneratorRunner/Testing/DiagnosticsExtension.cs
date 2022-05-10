using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;

namespace SourceGeneratorRunner.Testing;

static class DiagnosticsExtension
{
    public static bool Verify(this IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        return !diagnostics.Any(d => d.Severity >= severity);
    }

    public static void Verify(this IEnumerable<Diagnostic> diagnostics, Action<Diagnostic> verifier)
    {
        foreach (var d in diagnostics)
        {
            verifier(d);
        }
    }
}