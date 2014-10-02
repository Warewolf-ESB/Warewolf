
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    [DebuggerDisplay("{Method}")]
    internal sealed class MetaMethod : MetaTypeElement, IEquatable<MetaMethod>
    {
        #region Constants
        private const MethodAttributes ExplicitImplementationAttributes = MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Final;
        #endregion

        #region Instance Fields
        private string _name;
        private MethodAttributes _attributes;
        private bool _hasTarget;
        private MethodInfo _method;
        private MethodInfo _methodOnTarget;
        private bool _proxyable;
        private bool _standalone;
        private MetaMethodSource _source;
        #endregion

        #region Public Properties
        public MethodAttributes Attributes { get { return _attributes; } }
        public bool HasTarget { get { return _hasTarget; } }
        public MethodInfo Method { get { return _method; } }
        public MethodInfo MethodOnTarget { get { return _methodOnTarget; } }
        public string Name { get { return _name; } }
        public bool Proxyable { get { return _proxyable; } }
        public bool Standalone { get { return _standalone; } }
        public MetaMethodSource Source { get { return _source; } }
        #endregion

        #region Constructor
        public MetaMethod(string dynamicAssemblyName, MethodInfo method, MethodInfo methodOnTarget, bool standalone, bool proxyable, bool hasTarget, MetaMethodSource source)
            : base(method.DeclaringType, dynamicAssemblyName)
        {
            _method = method;
            _name = method.Name;
            _methodOnTarget = methodOnTarget;
            _standalone = standalone;
            _proxyable = proxyable;
            _hasTarget = hasTarget;
            _attributes = ObtainAttributes();
            _source = source;
        }
        #endregion

        #region Equality Handling
        public bool Equals(MetaMethod other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!StringComparer.OrdinalIgnoreCase.Equals(_name, other._name)) return false;

            MethodSignatureComparer comparer = MethodSignatureComparer.Instance;

            if (!comparer.EqualSignatureTypes(Method.ReturnType, other.Method.ReturnType)) return false;
            if (!comparer.EqualGenericParameters(Method, other.Method)) return false;
            if (!comparer.EqualParameters(Method, other.Method)) return false;

            return true;
        }
        #endregion

        #region Switch Handling
        internal override void SwitchToExplicitImplementation()
        {
            _attributes = ExplicitImplementationAttributes;
            if (_standalone == false) _attributes |= MethodAttributes.SpecialName;
            _name = string.Format("{0}.{1}", Method.DeclaringType.Name, Method.Name);
        }
        #endregion

        #region Attribute Handling
        private MethodAttributes ObtainAttributes()
        {
            MethodInfo methodInfo = _method;
            MethodAttributes attributes = MethodAttributes.Virtual;

            if (methodInfo.IsFinal || _method.DeclaringType.IsInterface) attributes |= MethodAttributes.NewSlot;
            if (methodInfo.IsPublic) attributes |= MethodAttributes.Public;
            if (methodInfo.IsHideBySig) attributes |= MethodAttributes.HideBySig;

            if (EmissionUtility.IsInternal(methodInfo) && EmissionUtility.IsInternalToDynamicProxy(methodInfo.DeclaringType.Assembly, DynamicAssemblyName)) attributes |= MethodAttributes.Assembly;
            if (methodInfo.IsFamilyAndAssembly) attributes |= MethodAttributes.FamANDAssem;
            else if (methodInfo.IsFamilyOrAssembly) attributes |= MethodAttributes.FamORAssem;
            else if (methodInfo.IsFamily) attributes |= MethodAttributes.Family;

            if (_standalone == false) attributes |= MethodAttributes.SpecialName;

            return attributes;
        }
        #endregion
    }

    [Flags]
    internal enum MetaMethodSource
    {
        Unspecified = 0,
        Event = 1,
        Property = 2,
        Method = 4,
        Acquire = 8,
        Release = 16
    }
}
