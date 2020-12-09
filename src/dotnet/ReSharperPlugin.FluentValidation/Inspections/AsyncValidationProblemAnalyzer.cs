using System.Linq;
using FluentValidation;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharperPlugin.FluentValidation.Psi;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    [ElementProblemAnalyzer(
        typeof(IClassDeclaration),
        HighlightingTypes = new[] {typeof(AsyncValidationHighlighting)})]
    public class AsyncValidationProblemAnalyzer : ElementProblemAnalyzer<IClassDeclaration>
    {
        private static string[] _nonAsyncMethods = new[]
        {
            nameof(AbstractValidator<object>.When),
            nameof(DefaultValidatorExtensions.Must),
            nameof(DefaultValidatorExtensions.Custom)
        };

        private static string[] _asyncMethods = new[]
        {
            nameof(AbstractValidator<object>.WhenAsync),
            nameof(DefaultValidatorExtensions.MustAsync),
            nameof(DefaultValidatorExtensions.CustomAsync)
        };

        protected override void Run(
            IClassDeclaration element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            if (!element.IsFromAspNetCoreMvcProject() ||
                !element.IsFromFluentValidationProject() ||
                !element.DeclaredElement.IsFluentValidationAbstractValidator())
                return;

            foreach (var constructorDeclaration in element.ConstructorDeclarations)
            {
                foreach (var invocationExpression in constructorDeclaration.Descendants<IInvocationExpression>())
                {
                    var referenceExpression = invocationExpression.InvokedExpression as IReferenceExpression;
                    var nameIdentifier = referenceExpression?.NameIdentifier;
                    var nameIdentifierName = nameIdentifier?.Name;
                    if (_asyncMethods.Contains(nameIdentifierName))
                    {
                        consumer.AddHighlighting(
                            new AsyncValidationHighlighting(referenceExpression, nameIdentifierName));
                    }
                }
            }
        }
    }
}
