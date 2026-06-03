// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp.Syntax;

// [Generator]
// public class MustAssignGenerator : ISourceGenerator
// {
//     public void Initialize(GeneratorInitializationContext context)
//     {
//         context.RegisterForSyntaxNotifications(() => new Receiver());
//     }

//     public void Execute(GeneratorExecutionContext context)
//     {
//         if (context.SyntaxReceiver is not Receiver receiver)
//             return;

//         foreach (var classDecl in receiver.Candidates)
//         {
//             var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
//             var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
//             if (symbol == null)
//                 continue;

//             var ns = symbol.ContainingNamespace.IsGlobalNamespace
//                 ? null
//                 : symbol.ContainingNamespace.ToDisplayString();

//             var sb = new StringBuilder();

//             foreach (var member in symbol.GetMembers().Where(m => m is IFieldSymbol or IPropertySymbol))
//             {
//                 var syntaxRef = member.DeclaringSyntaxReferences.FirstOrDefault();
//                 var memberSyntax = syntaxRef?.GetSyntax() as MemberDeclarationSyntax;
//                 var hasMustAssign = memberSyntax?.AttributeLists
//                     .SelectMany(a => a.Attributes)
//                     .Any(a => a.Name.ToString().Contains("MustAssign")) ?? false;

//                 if (!hasMustAssign)
//                     continue;

//                 sb.AppendLine($"            if ({member.Name} == null)");
//                 sb.AppendLine($"                warnings.Add(\"{member.Name} must be assigned\");");
//             }

//             if (sb.Length == 0)
//                 continue;

//             var source = $@"
// using System.Collections.Generic;
// {(ns != null ? $"namespace {ns} {{" : "")}
//     public partial class {symbol.Name}
//     {{
//         public override string[] _GetConfigurationWarnings()
//         {{
//             var warnings = new List<string>();
// {sb}
//             return warnings.ToArray();
//         }}
//     }}
// {(ns != null ? "}" : "")}
// ";
//             context.AddSource($"{symbol.Name}.MustAssign.g.cs", source);
//         }
//     }

//     class Receiver : ISyntaxReceiver
//     {
//         public List<ClassDeclarationSyntax> Candidates { get; } = new();

//         public void OnVisitSyntaxNode(SyntaxNode node)
//         {
//             if (node is not ClassDeclarationSyntax c)
//                 return;

//             var hasMustAssignMember = c.Members
//                 .OfType<MemberDeclarationSyntax>()
//                 .Any(m => m.AttributeLists
//                     .SelectMany(a => a.Attributes)
//                     .Any(a => a.Name.ToString().Contains("MustAssign")));

//             if (hasMustAssignMember)
//                 Candidates.Add(c);
//         }
//     }
// }
