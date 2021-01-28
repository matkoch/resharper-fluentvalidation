using JetBrains.Application;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.QuickFixes;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    [ShellComponent]
    internal class QuickFixRegistrar
    {
        public QuickFixRegistrar(IQuickFixes table)
        {
            table.RegisterQuickFix<NonAsyncValidationHighlighting>(
                Lifetime.Eternal,
                h => new ValidationMethodFix(h.ReferenceExpression, h.Name),
                typeof(ValidationMethodFix));
            table.RegisterQuickFix<AsyncValidationHighlighting>(
                Lifetime.Eternal,
                h => new ValidationMethodFix(h.ReferenceExpression, h.Name),
                typeof(ValidationMethodFix));
        }
    }
}
