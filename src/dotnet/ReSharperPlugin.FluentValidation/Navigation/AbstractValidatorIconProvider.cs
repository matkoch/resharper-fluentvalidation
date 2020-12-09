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
            PsiIconManager psiIconManager)
        {
            psiIconManager.AddExtension(lifetime, this);
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
