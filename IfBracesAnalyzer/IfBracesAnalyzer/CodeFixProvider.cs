using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;

namespace IfBracesAnalyzer
{
    [ExportCodeFixProvider("IfBracesAnalyzerCodeFixProvider", LanguageNames.CSharp), Shared]
    public class IfBracesAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return ImmutableArray.Create(IfBracesAnalyzerAnalyzer.DiagnosticId);
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent
                .AncestorsAndSelf().OfType<IfStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterFix(
                CodeAction.Create("Add braces", 
                c => AddBracesAsync(context.Document, declaration, c)),
                diagnostic);
        }

        private async Task<Document> AddBracesAsync(Document document, 
            IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var nonBlockStatement = ifStatement.Statement as ExpressionStatementSyntax;

            var newBlockStatement = SyntaxFactory.Block(nonBlockStatement)
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode<SyntaxNode, SyntaxNode>(
                nonBlockStatement, newBlockStatement);

            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}