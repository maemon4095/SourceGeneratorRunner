using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
namespace SourceGeneratorRunner;

public readonly struct RunnerConfig
{
    public static RunnerConfig Default
    {
        get
        {
            return new RunnerConfig()
            {
                ParseOptions = CSharpParseOptions.Default,
                References = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                AssemblyName = "SourceGeneratorTest",
            };
        }
    }

    public CSharpParseOptions? ParseOptions { get; init; }
    public IEnumerable<MetadataReference> References { get; init; }
    public CSharpCompilationOptions? CompilationOptions { get; init; }
    public string AssemblyName { get; init; }
}