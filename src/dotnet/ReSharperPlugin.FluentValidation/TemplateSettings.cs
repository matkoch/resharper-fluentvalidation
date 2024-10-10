// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/resharper/blob/master/LICENSE

using System.IO;
using JetBrains.Application.Settings;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Settings;

namespace ReSharperPlugin.FluentValidation
{
    [DefaultSettings(typeof (LiveTemplatesSettings))]
    public class SampleTemplateSettings : IHaveDefaultSettingsStream
    {
        public string Name => "FluentValidation Template Settings";

        public Stream GetDefaultSettingsStream(Lifetime lifetime)
        {
            var manifestResourceStream = typeof(SampleTemplateSettings).Assembly
                .GetManifestResourceStream(typeof(SampleTemplateSettings).Namespace + ".Templates.DotSettings").NotNull();
            lifetime.OnTermination(manifestResourceStream);
            return manifestResourceStream;
        }
    }
}
