
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class NodeWireGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        foreach (var classDecl in receiver.Candidates)
        {
            var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol == null) continue;

            var nodeMembers = symbol.GetMembers()
                .Where(m => m.GetAttributes().Any(a =>
                    a.AttributeClass?.Name is "NodeAttribute" or "Node"))
                .ToList();

            if (nodeMembers.Count == 0)
                continue;

            var ns = symbol.ContainingNamespace.IsGlobalNamespace
                ? ""
                : $"namespace {symbol.ContainingNamespace.ToDisplayString()};";

            var assignments = new StringBuilder();

            foreach (var member in nodeMembers)
            {
                var attr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name is "NodeAttribute" or "Node");

                var pathArg = attr?.ConstructorArguments.FirstOrDefault();

                string name = member.Name;

                ITypeSymbol? typeSymbol = member switch
                {
                    IFieldSymbol f => f.Type,
                    IPropertySymbol p => p.Type,
                    _ => null
                };

                if (typeSymbol == null)
                    continue;

                var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                string nodePath = (pathArg.HasValue && pathArg.Value.Value is string s && !string.IsNullOrWhiteSpace(s))
                    ? s
                    : name;

                assignments.AppendLine($$"""
        {{name}} = GetNodeOrNull<{{typeName}}>("{{nodePath}}");
        """);

                assignments.AppendLine($$"""
        if ({{name}} == null)
            throw new System.Exception($"WireNodes: missing node for {{name}}");
        """); 
            }

            var source = $$"""
            {{ns}}

            public partial class {{symbol.Name}}
            {
                public void WireNodes()
                {
            {{assignments}}
                }
            }
            """;

            context.AddSource($"{symbol.Name}.NodeWire.g.cs", source);
        }
    }
}

public class SyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Candidates { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax c &&
            c.AttributeLists.SelectMany(a => a.Attributes)
                .Any(a => a.Name.ToString().Contains("Scene")))
        {
            Candidates.Add(c);
        }
    }
}


