
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Emission.Emitters;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    internal sealed class MetaProperty : MetaTypeElement, IEquatable<MetaProperty>
    {
        #region Instance Fields
        private Type[] _arguments;
        private PropertyAttributes _attributes;
        private IEnumerable<CustomAttributeBuilder> _customAttributes;
        private MetaMethod _getter;
        private MetaMethod _setter;
        private Type _type;
        private PropertyEmitter _emitter;
        private string _name;
        #endregion

        #region Public Properties
        public Type[] Arguments { get { return _arguments; } }
        public bool CanRead { get { return _getter != null; } }
        public bool CanWrite { get { return _setter != null; } }
        public PropertyEmitter Emitter { get { if (_emitter == null) throw new InvalidOperationException("Emitter is not built. You have to build it first using 'BuildPropertyEmitter' method"); return _emitter; } }

        public MethodInfo GetMethod { get { if (!CanRead) throw new InvalidOperationException(); return _getter.Method; } }
        public MetaMethod Getter { get { return _getter; } }
        public MethodInfo SetMethod { get { if (!CanWrite) throw new InvalidOperationException(); return _setter.Method; } }
        public MetaMethod Setter { get { return _setter; } }
        #endregion

        #region Constructor
        public MetaProperty(string dynamicAssemblyName, string name, Type propertyType, Type declaringType, MetaMethod getter, MetaMethod setter, IEnumerable<CustomAttributeBuilder> customAttributes, Type[] arguments)
            : base(declaringType, dynamicAssemblyName)
        {
            _name = name;
            _type = propertyType;
            _getter = getter;
            _setter = setter;
            _attributes = PropertyAttributes.None;
            _customAttributes = customAttributes;
            _arguments = arguments ?? Type.EmptyTypes;
        }
        #endregion

        #region Build Handling
        public void BuildPropertyEmitter(ClassEmitter classEmitter)
        {
            if (_emitter != null) throw new InvalidOperationException("Emitter has already been built.");
            _emitter = classEmitter.CreateProperty(_name, _attributes, _type, _arguments);
            foreach (CustomAttributeBuilder attribute in _customAttributes) _emitter.DefineCustomAttribute(attribute);
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MetaProperty)) return false;
            return Equals((MetaProperty)obj);
        }

        public override int GetHashCode()
        {
            unchecked { return ((GetMethod != null ? GetMethod.GetHashCode() : 0) * 397) ^ (SetMethod != null ? SetMethod.GetHashCode() : 0); }
        }
        #endregion

        #region Equality Handling
        public bool Equals(MetaProperty other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!_type.Equals(other._type)) return false;
            if (!StringComparer.OrdinalIgnoreCase.Equals(_name, other._name)) return false;
            if (_arguments.Length != other._arguments.Length) return false;

            for (var i = 0; i < _arguments.Length; i++)
                if (_arguments[i].Equals(other._arguments[i]) == false)
                    return false;

            return true;
        }
        #endregion

        #region Switch Handling
        internal override void SwitchToExplicitImplementation()
        {
            _name = string.Format("{0}.{1}", sourceType.Name, _name);
            if (_setter != null) _setter.SwitchToExplicitImplementation();
            if (_getter != null) _getter.SwitchToExplicitImplementation();
        }
        #endregion
    }
}
