using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.ReSharper.Feature.Services.Explanatory;

namespace ReSharperPlugin.FluentValidation.Inspections
{
    [ShellComponent]
    public class FluentValidationCodeInspectionWikiDataProvider : ICodeInspectionWikiDataProvider
    {
        private const string WikiRoot = "https://docs.fluentvalidation.net/en/latest/";

        private static readonly IDictionary<string, string> Urls =
            new Dictionary<string, string>
            {
                { AsyncValidationHighlighting.SeverityId, WikiRoot + "async.html" },
                { NonAsyncValidationHighlighting.SeverityId, WikiRoot + "async.html" },
            };

        public bool TryGetValue(string attributeId, out string url)
        {
            return Urls.TryGetValue(attributeId, out url);
        }
    }
}
