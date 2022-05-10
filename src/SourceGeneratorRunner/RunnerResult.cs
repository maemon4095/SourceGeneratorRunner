using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SourceGeneratorRunner.Testing;
namespace SourceGeneratorRunner;

public readonly struct RunnerResult
{
    public RunnerResult(RunnerConfig config, SyntaxTree sourceSyntaxTree, Compilation compilation, GeneratorRunResult result)
    {
        this.Config = config;
        this.SourceSyntaxTree = sourceSyntaxTree;
        this.Compilation = compilation;
        this.GeneratorDiagnostics = result.Diagnostics;
        this.Exception = result.Exception;
        this.GeneratedSources = result.GeneratedSources;
    }

    public RunnerConfig Config { get; }
    public SyntaxTree SourceSyntaxTree { get; }
    public Compilation Compilation { get; }
    public Exception? Exception { get; }
    public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }
    public ImmutableArray<GeneratedSourceResult> GeneratedSources { get; }
    public bool Succeeded => this.GetAllDiagnostics().Verify();
    public IEnumerable<Diagnostic> GetAllDiagnostics() => this.GeneratorDiagnostics.Concat(this.Compilation.GetDiagnostics());
}