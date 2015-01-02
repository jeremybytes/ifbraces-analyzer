using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IfBracesAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IfBracesAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "IfBracesAnalyzer";
        internal const string Title = "if statement need braces";
        internal const string MessageFormat = "if statement should be enclosed in braces";
        internal const string Category = "Syntax";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, 
            MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.IfStatement);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (ifStatement.Statement.IsKind(SyntaxKind.Block)) return;

            var nonBlockStatement = ifStatement.Statement as ExpressionStatementSyntax;
            if (nonBlockStatement == null) return;

            var diagnostic = Diagnostic.Create(Rule, nonBlockStatement.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
