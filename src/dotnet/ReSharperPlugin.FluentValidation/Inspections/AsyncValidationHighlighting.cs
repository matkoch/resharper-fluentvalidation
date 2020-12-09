using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    [RegisterConfigurableSeverity(
        SeverityId,
        CompoundItemName: null,
        Group: HighlightingGroupIds.CodeSmell,
        Title: Message,
        Description: Description,
        DefaultSeverity: Severity.WARNING)]
    [ConfigurableSeverityHighlighting(
        SeverityId,
        CSharpLanguage.Name,
        OverlapResolve = OverlapResolveKind.ERROR,
        OverloadResolvePriority = 0,
        ToolTipFormatString = Message)]
    public class AsyncValidationHighlighting : IHighlighting
    {
        public const string SeverityId = nameof(AsyncValidationHighlighting);
        public const string Message = "Async validation method is used in ASP.NET Core";
        public const string Description = Message;

        public AsyncValidationHighlighting(IReferenceExpression referenceExpression, string name)
        {
            ReferenceExpression = referenceExpression;
            Name = name;
        }

        public IReferenceExpression ReferenceExpression { get; }
        public string Name { get; }

        public bool IsValid()
        {
            return ReferenceExpression.IsValid();
        }

        public DocumentRange CalculateRange()
        {
            return ReferenceExpression.NameIdentifier.GetHighlightingRange();
        }

        public string ToolTip => Message;

        public string ErrorStripeToolTip => Message;
    }
}