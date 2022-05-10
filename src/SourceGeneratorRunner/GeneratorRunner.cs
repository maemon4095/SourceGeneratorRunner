using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGeneratorRunner.Testing;

namespace SourceGeneratorRunner;

public readonly struct GeneratorRunner
{
    public static GeneratorRunner Create(RunnerConfig config, Func<IIncrementalGenerator> generatorSource)
    {
        return new(config, generatorSource);
    }
    public static GeneratorRunner Create(RunnerConfig config, Func<ISourceGenerator> generatorSource)
    {
        return new(config, generatorSource);
    }
    public static GeneratorRunner Create(Func<IIncrementalGenerator> generatorSource)
    {
        return Create(RunnerConfig.Default, generatorSource);
    }
    public static GeneratorRunner Create(Func<ISourceGenerator> generatorSource)
    {
        return Create(RunnerConfig.Default, generatorSource);
    }

    private GeneratorRunner(RunnerConfig config, Func<object> generator)
    {
        this.config = config;
        this.generatorSource = generator;
    }

    readonly RunnerConfig config;
    readonly Func<object> generatorSource;

    public RunnerResult Run(string source)
    {
        var config = this.config;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, config.ParseOptions);
        var compilation = CSharpCompilation.Create(config.AssemblyName, new[] { syntaxTree }, config.References, config.CompilationOptions);
        var driver = this.generatorSource() switch
        {
            IIncrementalGenerator g => CSharpGeneratorDriver.Create(g),
            ISourceGenerator g => CSharpGeneratorDriver.Create(g),
            _ => throw new InvalidOperationException()
        };
        if (!syntaxTree.GetDiagnostics().Verify()) throw new ArgumentException("Source has syntax error", nameof(source));
        var result = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _).GetRunResult();
        return new RunnerResult(config, syntaxTree, outputCompilation, result.Results.First());
    }
}