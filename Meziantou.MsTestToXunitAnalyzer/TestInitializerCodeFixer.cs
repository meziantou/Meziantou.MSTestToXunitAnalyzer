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
using Microsoft.CodeAnalysis.CSharp;

namespace Meziantou.MsTestToXunitAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class TestInitializerCodeFixer : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("MSTestXunit207");

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: false);
        if (nodeToFix is null)
            return;

        var title = $"Use ctor";
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

        var operation = editor.SemanticModel.GetOperation(nodeToFix) as IAttributeOperation;
        if (operation is null)
            return document;

        var method = (MethodDeclarationSyntax)operation.Syntax.AncestorsAndSelf().FirstOrDefault(node => node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MethodDeclaration));
        if (method is null)
            return document;

        var typeDeclaration = operation.Syntax.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (typeDeclaration is null)
            return document;

        var ctor = generator.ConstructorDeclaration(
            typeDeclaration.Identifier.Text,
            accessibility: Accessibility.Public,
            modifiers: DeclarationModifiers.None,
            statements: GetStatements(method))
            .CopyTriviaFrom(method);

        editor.ReplaceNode(method, ctor);
        return editor.GetChangedDocument();
    }

    private static IEnumerable<SyntaxNode> GetStatements(MethodDeclarationSyntax method)
    {
        if (method.Body is not null)
        {
            return method.Body.Statements;
        }
        else if (method.ExpressionBody is not null)
        {
            return new[] { SyntaxFactory.ExpressionStatement(method.ExpressionBody.Expression) };
        }
        else
        {
            return [];
        }
    }
}
