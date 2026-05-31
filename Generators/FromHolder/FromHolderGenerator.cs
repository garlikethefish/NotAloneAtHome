using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class FromHolderGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
        => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

        foreach (var classDecl in receiver.Candidates)
        {
            var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol is null) continue;

            var props = symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes()
                    .Any(a => a.AttributeClass?.Name == "FromHolderAttribute"))
                .ToList();

            if (props.Count == 0) continue;

            var ns = symbol.ContainingNamespace.IsGlobalNamespace
                ? "" : $"namespace {symbol.ContainingNamespace.ToDisplayString()};";

            var assignments = string.Join("\n        ", props.Select(p =>
                $"{p.Name} = Holder.{GetHolderFieldName(p.Type)};"));

            var source = $$"""
                {{ns}}
                public partial class {{symbol.Name}}
                {
                    public void WireComponents()
                    {
                        {{assignments}}
                    }
                }
                """;

            context.AddSource($"{symbol.Name}.wired.g.cs", source);
        }
    }

    private static string GetHolderFieldName(ITypeSymbol type) => type.Name switch
    {
        "ICarrierComponent"           => "CarrierComp",
        "ICarriableComponent"         => "CarriableComp",
        "IDetectableComponent"        => "DetectableComp",
        "IAreaDetectorComponent"      => "AreaDetectorComp",
        "ICastedAreaDetectorComponent"=> "CastedAreaDetectorComp",
        "IThrowableComponent"         => "ThrowableComp",
        "IThrowerComponent"           => "ThrowerComp",
        "IInteractableComponent"      => "InteractableComp",
        "IKillable"                   => "DestroyableComp",
        "ISpawnerComponent"           => "SpawnerComp",
        _                             => type.Name
    };
}

class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Candidates { get; } = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax c &&
            c.Members.OfType<PropertyDeclarationSyntax>()
             .Any(p => p.AttributeLists.Count > 0))
            Candidates.Add(c);
    }
}