using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace AnalyzerUseExplicitType
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class AnalyzerUseExplicitTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AnalyzerUseExplicitType";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, 
            MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information


            //LogMe("Register RegisterSymbolAction");
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

            LogMe("Register RegisterSyntaxNodeAction");
            context.RegisterSyntaxNodeAction<SyntaxKind> (AnalyzeSyntaxNode, SyntaxKind.VariableDeclarator);

        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {
            var node = context.Node;

            // see if this declaration has an AS clause
            var declaration = node.ChildNodes().Where(n => n.IsKind(SyntaxKind.SimpleAsClause) || n.IsKind(SyntaxKind.AsNewClause)).SingleOrDefault();
            if (declaration is null) {
                var diagnostic = Diagnostic.Create(Rule, node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void LogMe(string text) {
            //string filePath = @"C:\Users\Robert Gelb\Desktop\foolog.txt";
            //File.AppendAllText(filePath, text + Environment.NewLine);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context) {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            LogMe($"AnalyzeSymbol: {namedTypeSymbol.Name}");

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower)) {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
