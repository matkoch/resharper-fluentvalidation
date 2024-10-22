using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.Util;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    [ShellComponent(Instantiation.DemandAnyThreadSafe)]
    internal class QuickFixRegistrar : IQuickFixesProvider
    {
        public void Register(IQuickFixesRegistrar table)
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

        public IEnumerable<Type> Dependencies => EmptyArray<Type>.Instance;
    }
}
