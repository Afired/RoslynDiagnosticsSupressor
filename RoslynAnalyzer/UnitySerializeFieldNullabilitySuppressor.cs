using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnitySerializeFieldNullabilitySuppressor : DiagnosticSuppressor
    {
        private static readonly SuppressionDescriptor SerializeFieldSuppression =
            new SuppressionDescriptor(
                id: "USF001",
                suppressedDiagnosticId: "CS8618",
                justification:
                    "Unity serializes and initializes fields marked with [SerializeField]. " +
                    "The field's non-nullability is treated as a project-level contract.");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
            ImmutableArray.Create(SerializeFieldSuppression);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (Diagnostic diagnostic in context.ReportedDiagnostics)
            {
                if (diagnostic.Id != "CS8618")
                    continue;

                if (diagnostic.Location == Location.None)
                    continue;

                SyntaxTree syntaxTree = diagnostic.Location.SourceTree;

                if (syntaxTree == null)
                    continue;

                SyntaxNode root = syntaxTree.GetRoot(context.CancellationToken);

                SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan);

                VariableDeclaratorSyntax variable = node.FirstAncestorOrSelf<VariableDeclaratorSyntax>();

                if (variable == null)
                    continue;

                SemanticModel semanticModel = context.GetSemanticModel(syntaxTree);

                IFieldSymbol field = semanticModel.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;

                if (field == null)
                    continue;

                if (field.IsStatic)
                    continue;

                if (!HasSerializeFieldAttribute(field))
                    continue;

                context.ReportSuppression(Suppression.Create(SerializeFieldSuppression, diagnostic));
            }
        }

        private static bool HasSerializeFieldAttribute(IFieldSymbol field)
        {
            foreach (AttributeData attribute in field.GetAttributes())
            {
                INamedTypeSymbol attributeType = attribute.AttributeClass;

                if (attributeType == null)
                    continue;

                if (attributeType.ToDisplayString() == "UnityEngine.SerializeField")
                {
                    return true;
                }
            }

            return false;
        }
    }
}