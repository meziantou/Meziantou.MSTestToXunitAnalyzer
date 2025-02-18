using System.Composition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Meziantou.MsTestToXunitAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AttributesCodeFixer : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("MSTestXunit200", "MSTestXunit201", "MSTestXunit202", "MSTestXunit203");

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: false);
        if (nodeToFix is null)
            return;

        var title = $"Use xunit attributes";
        var codeAction = CodeAction.Create(
            title,
            ct => Rewrite(context.Document, nodeToFix, context.CancellationToken),
            equivalenceKey: title);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }

    private static async Task<Document> Rewrite(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = editor.Generator;
        var compilation = editor.SemanticModel.Compilation;
        var types = new WellKnownTypes(compilation);

        var operation = editor.SemanticModel.GetOperation(nodeToFix);
        if (operation is IAttributeOperation attributeOperation)
        {
            var attributeType = attributeOperation.Operation.Type;
            if (SymbolEqualityComparer.Default.Equals(attributeType, types.MSTestTestClassAttributeSymbol))
            {
                editor.RemoveNode(nodeToFix);
            }
            else if (SymbolEqualityComparer.Default.Equals(attributeType, types.MSTestTestMethodAttributeSymbol))
            {
                var attr = generator.Attribute(generator.TypeExpression(types.XunitFactAttributeSymbol));
                editor.ReplaceNode(operation.Syntax, attr);
            }
            else if (SymbolEqualityComparer.Default.Equals(attributeType, types.MSTestDataTestMethodAttributeSymbol))
            {
                var attr = generator.Attribute(generator.TypeExpression(types.XunitTheoryAttributeSymbol));
                attr = CopyAttributeArguments(attr, attributeOperation);

                editor.ReplaceNode(operation.Syntax, attr);
            }
            else if (SymbolEqualityComparer.Default.Equals(attributeType, types.MSTestDataRowAttributeSymbol))
            {
                var attr = generator.Attribute(generator.TypeExpression(types.XunitInlineDataAttributeSymbol));
                attr = CopyAttributeArguments(attr, attributeOperation);

                editor.ReplaceNode(operation.Syntax, attr);
            }
            else if (SymbolEqualityComparer.Default.Equals(attributeType, types.MSTestDynamicDataAttributeSymbol) && attributeOperation.Operation is IObjectCreationOperation objectCreation)
            {
                var args = objectCreation.Arguments;
                var name = args.FirstOrDefault(arg => arg.Parameter.Name == "dynamicDataSourceName")?.Value.Syntax;
                var type = GetMemberType();
                var newArgs = new List<SyntaxNode>();
                if (name is not null)
                {
                    newArgs.Add(generator.AttributeArgument(name: null, name));
                }

                if (type is not null)
                {
                    newArgs.Add(generator.AttributeArgument("MemberType", type));
                }

                var attr = generator.Attribute(generator.TypeExpression(types.XunitMemberDataAttributeSymbol), newArgs);
                editor.ReplaceNode(operation.Syntax, attr);

                SyntaxNode GetMemberType()
                {
                    if (objectCreation.Initializer is null)
                        return null;

                    foreach (var initializer in objectCreation.Initializer.Initializers)
                    {
                        if (initializer is not ISimpleAssignmentOperation assignment)
                            continue;

                        if (assignment.Target is not IMemberReferenceOperation memberReference)
                            continue;

                        if (memberReference.Member.Name == "DynamicDataDisplayNameDeclaringType")
                            return assignment.Value.Syntax;
                    }

                    return null;
                }
            }
        }

        return editor.GetChangedDocument();
    }

    private static SyntaxNode CopyAttributeArguments(SyntaxNode node, IAttributeOperation operation)
    {
        if (node is AttributeListSyntax attributeListSyntax && operation.Syntax is AttributeSyntax attributeSyntax)
        {
            return attributeListSyntax.ReplaceNode(attributeListSyntax.Attributes[0], attributeListSyntax.Attributes[0].WithArgumentList(attributeSyntax.ArgumentList));
        }

        throw new InvalidOperationException($"Cannot handle node of type {node.GetType().FullName} or operation of type {operation.Syntax.GetType().FullName}");
    }
}
