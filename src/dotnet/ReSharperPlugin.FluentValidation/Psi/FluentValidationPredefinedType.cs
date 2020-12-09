using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;

namespace ReSharperPlugin.FluentValidation.Psi
{
    public class FluentValidationPredefinedType
    {
        private static readonly Dictionary<IClrTypeName, int> s_typeNameIndex;

        public static readonly IClrTypeName ABSTRACT_VALIDATOR_FQN =
            new ClrTypeName("FluentValidation.AbstractValidator`1");

        static FluentValidationPredefinedType()
        {
            // Collect predefined type names through reflection
            s_typeNameIndex = new Dictionary<IClrTypeName, int>();

            foreach (var info in typeof(FluentValidationPredefinedType).GetFields())
            {
                if (info.IsStatic && typeof(IClrTypeName).IsAssignableFrom(info.FieldType))
                {
                    var clrTypeName = (IClrTypeName) info.GetValue(obj: null);
                    s_typeNameIndex.Add(clrTypeName, s_typeNameIndex.Count);
                }
            }
        }

        [NotNull] private readonly IDeclaredType[] _types = new IDeclaredType[s_typeNameIndex.Count];

        internal FluentValidationPredefinedType([NotNull] IPsiModule module)
        {
            Module = module;
        }

        [NotNull] public IPsiModule Module { get; }

        [NotNull] public IDeclaredType AbstractValidator => CreateType(ABSTRACT_VALIDATOR_FQN);

        [CanBeNull]
        public IDeclaredType TryGetType([NotNull] IClrTypeName clrTypeName)
        {
            return s_typeNameIndex.TryGetValue(clrTypeName, out var index)
                ? CreateType(index, clrTypeName)
                : null;
        }

        private IDeclaredType CreateType(int index, [NotNull] IClrTypeName clrName)
        {
            if (_types[index] == null)
            {
                lock (_types)
                {
                    if (_types[index] == null)
                        _types[index] = TypeFactory.CreateTypeByCLRName(clrName, Module);
                }
            }

            return _types[index];
        }

        private IDeclaredType CreateType([NotNull] IClrTypeName clrName)
        {
            return CreateType(s_typeNameIndex[clrName], clrName);
        }
    }
}
