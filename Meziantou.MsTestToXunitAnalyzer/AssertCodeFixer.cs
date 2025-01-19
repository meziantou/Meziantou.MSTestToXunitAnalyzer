using System.Composition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Meziantou.MsTestToXunitAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AssertCodeFixer : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("MSTestXunit100");

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (nodeToFix is null)
            return;

        var title = "Use xunit assertion";
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

        var operation = editor.SemanticModel.GetOperation(nodeToFix, cancellationToken);
        if (operation is IInvocationOperation assertionMethod)
        {
            var assertExpression = generator.TypeExpression(types.XunitAssertSymbol).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation);

            if (SymbolEqualityComparer.Default.Equals(assertionMethod.TargetMethod.ContainingType, types.MSTestAssertSymbol))
            {
                var newName = assertionMethod.TargetMethod.Name switch
                {
                    "IsTrue" => "True",
                    "IsFalse" => "False",
                    "IsNotNull" => "NotNull",
                    "IsNull" => "Null",
                    "AreEqual" => "Equal",
                    "AreNotEqual" => "NotEqual",
                    "AreSame" => "Same",
                    "AreNotSame" => "NotSame",
                    "ThrowsException" => "Throws",
                    "ThrowsExceptionAsync" => "ThrowsAsync",
                    _ => null,
                };

                if (newName is not null)
                {
                    var identifier = generator.IdentifierName(newName);
                    if (assertionMethod.TargetMethod.TypeArguments.Length > 0)
                    {
                        identifier = generator.WithTypeArguments(identifier, assertionMethod.TargetMethod.TypeArguments.Select(generator.TypeExpression));
                    }

                    var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(assertExpression, identifier),
                        Extensions.CopyArguments(assertionMethod))

                        .WithLeadingTrivia(assertionMethod.Syntax.GetLeadingTrivia())
                        .WithTrailingTrivia(assertionMethod.Syntax.GetTrailingTrivia());

                    editor.ReplaceNode(assertionMethod.Syntax, newInvocation);
                    return editor.GetChangedDocument();
                }

                if (assertionMethod.TargetMethod.Name == "IsInstanceOfType" && assertionMethod.Arguments.Length is 1 or 2)
                {
                    var identifier = generator.IdentifierName("IsType");
                    if (assertionMethod.TargetMethod.TypeArguments.Length > 0)
                    {
                        identifier = generator.WithTypeArguments(identifier, assertionMethod.TargetMethod.TypeArguments.Select(generator.TypeExpression));
                    }

                    var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(assertExpression, identifier),
                        Extensions.CopyArgumentsWithReverseTwoFirst(assertionMethod))
                        .CopyTriviaFrom(assertionMethod.Syntax);
                    editor.ReplaceNode(assertionMethod.Syntax, newInvocation);
                    return editor.GetChangedDocument();
                }

                if (assertionMethod.TargetMethod.Name == "IsNotInstanceOfType" && assertionMethod.Arguments.Length is 1 or 2)
                {
                    var identifier = generator.IdentifierName("IsNotType");
                    if (assertionMethod.TargetMethod.TypeArguments.Length > 0)
                    {
                        identifier = generator.WithTypeArguments(identifier, assertionMethod.TargetMethod.TypeArguments.Select(generator.TypeExpression));
                    }

                    var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(assertExpression, identifier),
                        Extensions.CopyArgumentsWithReverseTwoFirst(assertionMethod))
                        .CopyTriviaFrom(assertionMethod.Syntax);
                    editor.ReplaceNode(assertionMethod.Syntax, newInvocation);
                    return editor.GetChangedDocument();
                }
            }
            else if (SymbolEqualityComparer.Default.Equals(assertionMethod.TargetMethod.ContainingType, types.MSTestStringAssertSymbol))
            {
                var newName = assertionMethod.TargetMethod.Name switch
                {
                    "StartsWith" => "StartsWith",
                    "EndsWith" => "EndsWith",
                    "Contains" => "Contains",
                    _ => null,
                };

                if (newName is not null)
                {
                    var identifier = generator.IdentifierName(newName);
                    if (assertionMethod.TargetMethod.TypeArguments.Length > 0)
                    {
                        identifier = generator.WithTypeArguments(identifier, assertionMethod.TargetMethod.TypeArguments.Select(generator.TypeExpression));
                    }

                    var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(assertExpression, identifier),
                        Extensions.CopyArgumentsWithReverseTwoFirst(assertionMethod))
                        .WithLeadingTrivia(assertionMethod.Syntax.GetLeadingTrivia())
                        .WithTrailingTrivia(assertionMethod.Syntax.GetTrailingTrivia());

                    editor.ReplaceNode(assertionMethod.Syntax, newInvocation);
                    return editor.GetChangedDocument();
                }
            }
            else if (SymbolEqualityComparer.Default.Equals(assertionMethod.TargetMethod.ContainingType, types.MSTestCollectionAssertSymbol))
            {
                var (newName, reverseArguments) = assertionMethod.TargetMethod.Name switch
                {
                    "AreEqual" => ("Equal", false),
                    "AreNotEqual" => ("NotEqual", false),
                    "Contains" => ("Contains", true),
                    "DoesNotContain" => ("DoesNotContain", true),
                    "IsSubsetOf" => ("Contains", false),
                    "IsNotSubsetOf" => ("DoesNotContain", false),
                    _ => default,
                };

                if (newName is not null)
                {
                    var identifier = generator.IdentifierName(newName);
                    if (assertionMethod.TargetMethod.TypeArguments.Length > 0)
                    {
                        identifier = generator.WithTypeArguments(identifier, assertionMethod.TargetMethod.TypeArguments.Select(generator.TypeExpression));
                    }

                    var newInvocation = generator.InvocationExpression(
                        generator.MemberAccessExpression(assertExpression, identifier),
                        reverseArguments ? Extensions.CopyArgumentsWithReverseTwoFirst(assertionMethod) : Extensions.CopyArguments(assertionMethod))
                        .WithLeadingTrivia(assertionMethod.Syntax.GetLeadingTrivia())
                        .WithTrailingTrivia(assertionMethod.Syntax.GetTrailingTrivia());

                    editor.ReplaceNode(assertionMethod.Syntax, newInvocation);
                    return editor.GetChangedDocument();
                }
            }
        }

        return editor.GetChangedDocument();
    }
}

file static class Extensions
{
    public static IEnumerable<SyntaxNode> CopyArguments(IInvocationOperation invocation)
    {
        return invocation.Arguments.Select(s => s.Value.Syntax);
    }

    public static IEnumerable<SyntaxNode> CopyArgumentsWithReverseTwoFirst(IInvocationOperation invocation)
    {
        if (invocation.Arguments.Length < 2)
            return invocation.Arguments.Select(s => s.Value.Syntax);

        return [invocation.Arguments[1].Value.Syntax, invocation.Arguments[0].Value.Syntax, .. invocation.Arguments[2..].Select(s => s.Value.Syntax)];
    }
}