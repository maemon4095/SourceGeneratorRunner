using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace SourceGeneratorRunner.Test;

[Generator]
class IncrementalGeneratorSample : IIncrementalGenerator
{
    struct Bundle
    {
        public Bundle(INamedTypeSymbol symbol, ClassDeclarationSyntax syntax)
        {
            this.Symbol = symbol;
            this.Syntax = syntax;
        }

        public INamedTypeSymbol Symbol { get; }
        public ClassDeclarationSyntax Syntax { get; }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ProductInitialSource);
        context.RegisterSourceOutput(CreateProvider(context), ProductSource);
    }

    static string Namespace => "SourceGeneratorRunner.Test";
    static string AttributeName => "MarkerAttribute";
    static string AttributeFullName => $"{Namespace}.{AttributeName}";
    static SymbolDisplayFormat FormatTypeDecl { get; } = new SymbolDisplayFormat(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeConstraints,
        kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword
    );

    public static string InitialSourceFileName => "InitialSource.g.cs";
    public static string InitialSource => @$"
namespace {Namespace}
{{
    [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
    class {AttributeName} : global::System.Attribute
    {{

    }}
}}";
    public static string GeneratedInnerClass => "public class GeneratedInnerClass { }";

    static void ProductInitialSource(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(InitialSourceFileName, InitialSource);
    }

    static void ProductSource(SourceProductionContext context, Bundle bundle)
    {
        var symbol = bundle.Symbol;
        var stringWriter = new StringWriter();
        using var writer = new IndentedTextWriter(stringWriter);
        var containingDepth = 0;
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            writer.Write("namespace ");
            writer.WriteLine(symbol.ContainingNamespace.ToDisplayString());
            writer.WriteLine('{');
            writer.Indent++;
            containingDepth++;
        }

        foreach (var containingType in ContainingTypes(symbol))
        {
            writer.Write("partial ");
            writer.WriteLine(containingType.ToDisplayString(FormatTypeDecl));
            writer.WriteLine('{');
            writer.Indent++;
            containingDepth++;
        }

        writer.Write("partial ");
        writer.WriteLine(symbol.ToDisplayString(FormatTypeDecl));
        writer.WriteLine('{');
        writer.Indent++;

        writer.WriteLine(GeneratedInnerClass);

        writer.Indent--;
        writer.WriteLine('}');


        for (var i = 0; i < containingDepth; ++i)
        {
            writer.Indent--;
            writer.WriteLine('}');
        }

        context.AddSource($"GeneratedSource.{symbol.Name}.g.cs", stringWriter.ToString() ?? string.Empty);
    }


    static IEnumerable<INamedTypeSymbol> ContainingTypes(INamedTypeSymbol symbol)
    {
        var type = symbol.ContainingType;
        if (type is null) yield break;
        foreach (var c in ContainingTypes(type))
        {
            yield return c;
        }
        yield return type;
    }

    static IncrementalValuesProvider<Bundle> CreateProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            static (node, token) =>
            {
                token.ThrowIfCancellationRequested();
                return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
            },
            static (context, token) =>
            {
                token.ThrowIfCancellationRequested();
                var syntax = (context.Node as ClassDeclarationSyntax)!;
                var symbol = context.SemanticModel.GetDeclaredSymbol(syntax, token);
                return (syntax, symbol);
            })
            .Where(tuple => tuple.symbol is not null).Combine(context.CompilationProvider)
            .Select((tuple, token) =>
            {
                token.ThrowIfCancellationRequested();
                var ((syntax, symbol), compilation) = tuple;
                var attributeSymbol = compilation.GetTypeByMetadataName(AttributeFullName) ?? throw new NullReferenceException("Marker attribute was not found.");
                var attributeData = symbol!.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
                return (syntax, symbol, attributeData);
            })
            .Where(tuple => tuple.attributeData is not null)
            .Select((tuple, token) =>
            {
                token.ThrowIfCancellationRequested();
                var (syntax, symbol, attributeData) = tuple;
                return new Bundle(symbol, syntax);
            });
    }
}