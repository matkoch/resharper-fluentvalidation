using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Reflection;

namespace ReSharperPlugin.FluentValidation.Psi
{
    public static class FluentValidationPsiExtensions
    {
        private const string AspNetCoreMvcAssemblyName = "Microsoft.AspNetCore.Mvc";
        private const string FluentValidationAssemblyName = nameof(FluentValidation);

        public static bool IsFromAspNetCoreMvcProject(this ITreeNode node)
        {
            return node.GetSourceFile()?.GetProject().IsAspNetCoreMvcProject() ?? false;
        }

        public static bool IsAspNetCoreMvcProject(this IProject project)
        {
            return ReferencedAssembliesService.IsProjectReferencingAssemblyByName(
                project,
                project.GetCurrentTargetFrameworkId(),
                AssemblyNameInfoFactory.Create2(AspNetCoreMvcAssemblyName, version: null),
                out _);
        }

        public static bool IsFromFluentValidationProject(this ITreeNode node)
        {
            return node.GetSourceFile()?.GetProject().IsFluentValidationProject() ?? false;
        }

        public static bool IsFluentValidationProject(this IProject project)
        {
            return ReferencedAssembliesService.IsProjectReferencingAssemblyByName(
                project,
                project.GetCurrentTargetFrameworkId(),
                AssemblyNameInfoFactory.Create2(FluentValidationAssemblyName, version: null),
                out _);
        }

        public static bool IsFluentValidationAbstractValidator(this ITypeElement element)
        {
            if (element == null)
                return false;

            var type = element.Module.GetFluentValidationPredefinedType().AbstractValidator.GetTypeElement();
            return element.IsDescendantOf(type);
        }
    }
}
