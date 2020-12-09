using System.Linq;
using FluentValidation;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharperPlugin.FluentValidation.Psi;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    [ElementProblemAnalyzer(
        typeof(IInvocationExpression),
        HighlightingTypes = new[] {typeof(NonAsyncValidationHighlighting)})]
    public class NonAsyncValidationProblemAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
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
            IInvocationExpression element,
            ElementProblemAnalyzerData data,
            IHighlightingConsumer consumer)
        {
            var referenceExpression = element.InvokedExpression as IReferenceExpression;
            var nameIdentifier = referenceExpression?.NameIdentifier;
            if (nameIdentifier == null || nameIdentifier.Name != nameof(AbstractValidator<object>.Validate))
                return;

            if (!element.IsFromFluentValidationProject())
                return;

            var containingType = referenceExpression.QualifierExpression?.Type().GetTypeElement();
            if (containingType == null)
                return;

            foreach (var constructor in containingType.Constructors)
            {
                if (HasAsyncInvocations(constructor))
                    consumer.AddHighlighting(
                        new NonAsyncValidationHighlighting(referenceExpression, nameIdentifier.Name));
            }
        }

        private static bool HasAsyncInvocations(IConstructor constructor)
        {
            var declaration = constructor.GetFirstDeclaration<IConstructorDeclaration>();
            foreach (var invocationExpression in declaration.Descendants<IInvocationExpression>())
            {
                var nameIdentifier = (invocationExpression.InvokedExpression as IReferenceExpression)?.NameIdentifier;
                if (_asyncMethods.Contains(nameIdentifier?.Name))
                    return true;
            }

            return false;
        }
    }
}
