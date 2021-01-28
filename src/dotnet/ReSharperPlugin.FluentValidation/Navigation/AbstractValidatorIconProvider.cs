using System.Diagnostics;
using JetBrains.Application.Notifications;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.Icons;
using ReSharperPlugin.FluentValidation.Psi;

namespace ReSharperPlugin.FluentValidation.Navigation
{
    [SolutionComponent]
    public class AbstractValidatorIconProvider : IDeclaredElementIconProvider
    {
        public AbstractValidatorIconProvider(
            Lifetime lifetime,
            PsiIconManager psiIconManager,
            UserNotifications userNotifications)
        {
            psiIconManager.AddExtension(lifetime, this);

            userNotifications.CreateNotification(lifetime, body: "body", title: "title",
                executed: new UserNotificationCommand("Open Sponsor website", () =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://google.com",
                        UseShellExecute = true
                    });
                }));
        }

        public IconId GetImageId(
            IDeclaredElement declaredElement,
            PsiLanguageType languageType,
            out bool canApplyExtensions)
        {
            canApplyExtensions = false;
            var typeElement = declaredElement as ITypeElement;
            if (typeElement == null)
                return null;

            return typeElement.IsFluentValidationAbstractValidator()
                ? FluentValidationLogo.Id
                : null;
        }
    }
}
