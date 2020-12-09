using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharperPlugin.FluentValidation.Psi
{
    public static class FluentValidationPredefinedTypesExtensions
    {
        private static readonly Key<FluentValidationPredefinedTypeCache> s_typeCacheKey =
            new Key<FluentValidationPredefinedTypeCache>(nameof(FluentValidationPredefinedTypeCache));

        public static FluentValidationPredefinedType GetFluentValidationPredefinedType([NotNull] this IPsiModule module)
        {
            var predefinedTypeCache = module.GetOrCreateDataNoLock(
                s_typeCacheKey,
                module,
                x => x.GetPsiServices().GetComponent<FluentValidationPredefinedTypeCache>());

            return predefinedTypeCache.GetOrCreateFluentValidationPredefinedType(module);
        }

        public static FluentValidationPredefinedType GetFluentValidationPredefinedType([NotNull] this ITreeNode context)
        {
            return GetFluentValidationPredefinedType(context.GetPsiModule());
        }

        [Pure]
        [ContractAnnotation("type:null => false")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPredefinedType([CanBeNull] IType type, [NotNull] IClrTypeName clrName)
        {
            var predefinedCandidate = type as IDeclaredType;
            if (predefinedCandidate == null)
                return false;

            return IsPredefinedTypeElement(predefinedCandidate.GetTypeElement(), clrName);
        }

        [Pure]
        [ContractAnnotation("typeElement:null => false")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPredefinedTypeElement([CanBeNull] ITypeElement typeElement,
            [NotNull] IClrTypeName clrName)
        {
            if (typeElement == null)
                return false;

            var predefinedType = typeElement.Module.GetFluentValidationPredefinedType();
            var truePredefined = predefinedType.TryGetType(clrName).NotNull("NOT PREDEFINED");
            var predefinedTypeElement = truePredefined.GetTypeElement();

            return DeclaredElementEqualityComparer.TypeElementComparer.Equals(typeElement, predefinedTypeElement);
        }

        [PsiComponent]
        internal class FluentValidationPredefinedTypeCache : InvalidatingPsiCache
        {
            private readonly ConcurrentDictionary<IPsiModule, FluentValidationPredefinedType> _predefinedTypes =
                new ConcurrentDictionary<IPsiModule, FluentValidationPredefinedType>();

            protected override void InvalidateOnPhysicalChange(PsiChangedElementType elementType)
            {
                if (elementType == PsiChangedElementType.InvalidateCached)
                    return;

                _predefinedTypes.Clear();
            }

            public FluentValidationPredefinedType GetOrCreateFluentValidationPredefinedType(IPsiModule module)
            {
                return _predefinedTypes.GetOrAdd(module, x => new FluentValidationPredefinedType(x));
            }
        }
    }
}
