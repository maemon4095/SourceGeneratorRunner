using Xunit;
using SourceGeneratorRunner.Testing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Abstractions;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGeneratorRunner.Test;

public class SourceGeneratorRunnerTest
{
    public SourceGeneratorRunnerTest(ITestOutputHelper helper)
    {
        this.helper = helper;
    }
    readonly ITestOutputHelper helper;

    [Fact]
    public void OnlyInitialSourceProductedWithSourceHasNoPartialClass()
    {
        var runner = GeneratorRunner.Create(() => new IncrementalGeneratorSample()); ;
        var source = @"
using SourceGeneratorRunner.Test;
namespace Namespace 
{
    
}";
        runner.Run(source).Verify(result =>
        {
            this.helper.WriteLine(result.Exception?.Message ?? "");
            Assert.True(result.Succeeded);
            var source = result.GeneratedSources.Single();
            Assert.Equal(IncrementalGeneratorSample.InitialSourceFileName, source.HintName);
            Assert.Equal(IncrementalGeneratorSample.InitialSource, source.SourceText.ToString());
        });
    }

    [Fact]
    public void GeneratedClassHasGeneratedInnerClass()
    {
        var runner = GeneratorRunner.Create(() => new IncrementalGeneratorSample()); ;

        var source = @"
using SourceGeneratorRunner.Test;
namespace Namespace 
{
    [Marker]
    partial class A
    {

    }
}";
        runner.Run(source).Verify(result =>
        {
            var helper = this.helper;
            helper.WriteLine(result.Exception?.Message ?? "");
            helper.WriteLine(string.Join("\n", result.GeneratedSources.Select(source => source.SourceText)));
            Assert.True(result.Succeeded);
            var source = result.GeneratedSources.Single(s => s.HintName == "GeneratedSource.A.g.cs");
            var classSyntax = (source.SyntaxTree.GetRoot().DescendantNodes(node =>
            {
                return node is not ClassDeclarationSyntax;
            }).First(node => node is ClassDeclarationSyntax) as ClassDeclarationSyntax)!;
            var innerClassSyntax = classSyntax.ChildNodes().First(node => node is ClassDeclarationSyntax);
            Assert.Equal(IncrementalGeneratorSample.GeneratedInnerClass, innerClassSyntax.ToString());
        });
    }

    [Fact]
    public void RunnerThrowsArgumentExceptionWithIncompleteSource()
    {
        var runner = GeneratorRunner.Create(() => new IncrementalGeneratorSample()); ;
        var source = @"
using SourceGeneratorRunner.Test;
namespace Namespace 
{
    [Marker]
    partial class A
    {
}";
        Assert.Throws<ArgumentException>(() =>
        {
            runner.Run(source).Verify(result =>
            {

            });
        });
    }

    [Fact]
    public void GeneratedClassHasGeneratedInnerClassAlsoSourceGenrator()
    {
        var runner = GeneratorRunner.Create(() => new IncrementalGeneratorSample().AsSourceGenerator());
        var source = @"
using SourceGeneratorRunner.Test;
namespace Namespace 
{
    [Marker]
    partial class A
    {

    }
}";
        runner.Run(source).Verify(result =>
        {
            var helper = this.helper;
            helper.WriteLine(result.Exception?.Message ?? "");
            helper.WriteLine(string.Join("\n", result.GeneratedSources.Select(source => source.SourceText)));
            Assert.True(result.Succeeded);
            var source = result.GeneratedSources.Single(s => s.HintName == "GeneratedSource.A.g.cs");
            var classSyntax = (source.SyntaxTree.GetRoot().DescendantNodes(node =>
            {
                return node is not ClassDeclarationSyntax;
            }).First(node => node is ClassDeclarationSyntax) as ClassDeclarationSyntax)!;
            var innerClassSyntax = classSyntax.ChildNodes().First(node => node is ClassDeclarationSyntax);
            Assert.Equal(IncrementalGeneratorSample.GeneratedInnerClass, innerClassSyntax.ToString());
        });
    }
}