using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class MustAssignGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new Receiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not Receiver receiver)
            return;

        foreach (var classDecl in receiver.Candidates)
        {
            var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

            if (symbol == null)
                continue;

            var ns = symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : symbol.ContainingNamespace.ToDisplayString();

            var sb = new StringBuilder();

            var members = symbol.GetMembers()
                .Where(m => m is IFieldSymbol or IPropertySymbol);

            foreach (var member in members)
            {
                var attr = member.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == "MustAssignAttribute");

                if (attr == null)
                    continue;

                var name = member.Name;

                // ONLY ONE MODE: Null check
                sb.AppendLine($$"""
                    if ({{name}} == null)
                        warnings.AppendLine("{{name}} must be assigned");
                """);
            }

            var source = $@"
using System.Text;
using Godot;

{(ns != null ? $"namespace {ns} {{" : "")}

    public partial class {symbol.Name} : Node
    {{
        public override string _GetConfigurationWarning()
        {{
            var warnings = new StringBuilder();

{sb}

            return warnings.ToString();
        }}
    }}

{(ns != null ? "}" : "")}
";

            context.AddSource($"{symbol.Name}.MustAssign.g.cs", source);
        }
    }

    class Receiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Candidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode node)
        {
            if (node is not ClassDeclarationSyntax c)
                return;

            if (c.AttributeLists.Count == 0)
                return;

            foreach (var attrList in c.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString().Contains("MustAssign"))
                    {
                        Candidates.Add(c);
                        return;
                    }
                }
            }
        }
    }
}