using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Emitters
{
    internal sealed class ClassEmitter : AbstractTypeEmitter
    {
        #region Constants
        private const TypeAttributes DefaultAttributes = TypeAttributes.Public | TypeAttributes.Class;
        #endregion

        #region Static Members
        private static TypeBuilder CreateTypeBuilder(EmissionModule module, string name, Type baseType, IEnumerable<Type> interfaces, TypeAttributes flags)
        {
            return module.DefineType(name, flags);
        }
        #endregion

        #region Instance Fields
        private readonly EmissionModule _module;
        #endregion

        #region Public Properties
        public EmissionModule ModuleScope { get { return _module; } }
        #endregion

        #region Constructors
        public ClassEmitter(EmissionModule module, String name, Type baseType, IEnumerable<Type> interfaces)
            : this(module, name, baseType, interfaces, DefaultAttributes)
        {
        }

        public ClassEmitter(EmissionModule module, String name, Type baseType, IEnumerable<Type> interfaces, TypeAttributes flags)
            : this(CreateTypeBuilder(module, name, baseType, interfaces, flags), module.DynamicAssemblyName)
        {
            interfaces = InitializeGenericArgumentsFromBases(ref baseType, interfaces);

            if (interfaces != null)
                foreach (Type inter in interfaces)
                    TypeBuilder.AddInterfaceImplementation(inter);

            TypeBuilder.SetParent(baseType);
            _module = module;
        }

        public ClassEmitter(TypeBuilder typeBuilder, string dynamicAssemblyName)
            : base(typeBuilder, dynamicAssemblyName)
        {
        }
        #endregion

        #region Initialization Handling
        private IEnumerable<Type> InitializeGenericArgumentsFromBases(ref Type baseType, IEnumerable<Type> interfaces)
        {
            if (baseType != null && baseType.IsGenericTypeDefinition) throw new NotSupportedException("ClassEmitter does not support open generic base types. Type: " + baseType.FullName);
            if (interfaces == null) return interfaces;

            foreach (Type inter in interfaces)
                if (inter.IsGenericTypeDefinition)
                    throw new NotSupportedException("ClassEmitter does not support open generic interfaces. Type: " + inter.FullName);

            return interfaces;
        }
        #endregion
    }
}
