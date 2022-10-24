using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace AnalyzerUseExplicitType
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic, Name = nameof(AnalyzerUseExplicitTypeCodeFixProvider)), Shared]
    public class AnalyzerUseExplicitTypeCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(AnalyzerUseExplicitTypeAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            /// the TUTORIAL at https://www.youtube.com/watch?v=XnjZRN9NC7s
            /// https://github.com/dotnet/roslyn/blob/main/docs/wiki/How-To-Write-a-Visual-Basic-Analyzer-and-Code-Fix.md

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().First();

            CodeAction ca = CodeAction.Create(CodeFixResources.CodeFixTitle, c => MakeExplicitDeclaratorAsync(context.Document, declaration, c));
            context.RegisterCodeFix(ca, diagnostic);

        }

        private async Task<Document> MakeExplicitDeclaratorAsync(Document document, 
            VariableDeclaratorSyntax variableDeclarator, CancellationToken cancellationToken) {

            try {

                document.TryGetSemanticModel(out SemanticModel semanticModel);
                if (semanticModel is null) return document; // can't infer - return original document

                var objTypeName = semanticModel.GetOperation(variableDeclarator.Initializer.Value).Type.Name;
                SimpleAsClauseSyntax asClause = SyntaxFactory.SimpleAsClause(SyntaxFactory.IdentifierName(objTypeName));
                VariableDeclaratorSyntax newDeclarator = variableDeclarator.WithAsClause(asClause);

                var root = await document.GetSyntaxRootAsync();
                var newRoot = root.ReplaceNode(variableDeclarator, newDeclarator);

                var newDocument = document.WithSyntaxRoot(newRoot);

                return newDocument;
            } catch (Exception) {
                return document;    // nothing changes
            }
        }
    }
}
