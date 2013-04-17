using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Emitters
{
    internal sealed class NestedClassEmitter : AbstractTypeEmitter
    {
        #region Static Members
        private static TypeBuilder CreateTypeBuilder(AbstractTypeEmitter maintype, string name, TypeAttributes attributes, Type baseType, Type[] interfaces)
        {
            return maintype.TypeBuilder.DefineNestedType(name, attributes, baseType, interfaces);
        }
        #endregion

        #region Constructors
        public NestedClassEmitter(AbstractTypeEmitter maintype, String name, Type baseType, Type[] interfaces)
            : this(maintype, CreateTypeBuilder(maintype, name, TypeAttributes.Sealed | TypeAttributes.NestedPublic | TypeAttributes.Class, baseType, interfaces))
        {
        }

        public NestedClassEmitter(AbstractTypeEmitter maintype, String name, TypeAttributes attributes, Type baseType, Type[] interfaces)
            : this(maintype, CreateTypeBuilder(maintype, name, attributes, baseType, interfaces))
        {
        }

        public NestedClassEmitter(AbstractTypeEmitter maintype, TypeBuilder typeBuilder)
            : base(typeBuilder, maintype.DynamicAssemblyName)
        {
            maintype.Nested.Add(this);
        }
        #endregion
    }
}
