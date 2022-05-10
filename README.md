# SourceGeneratorRunner

Runner for single ISourceGenerator or IIncrementalGenerator. Useful for unit testing of source generators.

## Usage

```C#
using SourceGeneratorRunner;
using SourceGeneratorRunner.Testing;

var runner = SourceGeneratorRunner.Create(() => new YourSourceGenerator()); 
var source = "source code ...";
runner.Run(source).Verify(result =>
{
    // verification ...
});
```
if you want to run generator with custom options

```C#
using SourceGeneratorRunner;
using SourceGeneratorRunner.Testing;

var config = RunnerConfig.Default with 
{
    // customize config ... 
};
var runner = SourceGeneratorRunner.Create(config, () => new YourSourceGenerator()); 
// run generator ...
```

## Remarks

Since [CSharpGeneratorDriver](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.codeanalysis.csharp.csharpgeneratordriver?view=roslyn-dotnet), [CSharpCompilation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.csharpcompilation?view=roslyn-dotnet-4.1.0), and [CSharpSyntaxTree](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.csharpsyntaxtree?view=roslyn-dotnet-4.1.0) are used internally, it is recommended to refer to these specifications.
