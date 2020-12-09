using System;
using FluentValidation;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    public sealed class ValidationMethodFix : QuickFixBase
    {
        public ValidationMethodFix(IReferenceExpression referenceExpression, string name)
        {
            ReferenceExpression = referenceExpression;
            Name = name;
        }

        public IReferenceExpression ReferenceExpression { get; }
        public string Name { get; }
        public bool IsCurrentlyAsync => Name.EndsWith("Async");
        public string NewName => IsCurrentlyAsync ? Name.TrimFromEnd("Async") : Name + "Async";

        public override string Text => $"Use '{NewName}' instead of '{Name}'";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return ReferenceExpression.IsValid();
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            ReferenceExpression.SetName(NewName);

            if (!Name.StartsWith(nameof(AbstractValidator<object>.Validate)))
            {
                // adapt lambda expression
                var invocationExpression = InvocationExpressionNavigator.GetByInvokedExpression(ReferenceExpression);
                if (invocationExpression != null)
                {
                    foreach (var argument in invocationExpression.Arguments)
                    {
                        var lambdaExpression = argument.Expression as ILambdaExpression;
                        if (lambdaExpression == null)
                            continue;

                        lambdaExpression.SetAsync(!IsCurrentlyAsync);
                        // foreach (var awaitExpression in lambdaExpression.BodyBlock.Descendants<IAwaitExpression>())
                        // {
                        //     EcmaDesc.Mod
                        // }
                    }
                }
            }

            return null;
        }
    }
}
