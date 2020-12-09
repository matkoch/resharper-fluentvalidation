using JetBrains.Application;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Intentions.CSharp.QuickFixes;
using ReSharperPlugin.FluentValidation.Inspections;

namespace ReSharperPlugin.FluentValidation
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
