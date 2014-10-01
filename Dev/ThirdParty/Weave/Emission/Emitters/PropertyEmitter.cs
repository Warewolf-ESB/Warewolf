
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Emitters
{
    internal sealed class PropertyEmitter : IEmissionMemberEmitter
    {
        #region Instance Fields
        private PropertyBuilder _builder;
        private AbstractTypeEmitter _parentTypeEmitter;
        private MethodEmitter _getMethod;
        private MethodEmitter _setMethod;
        #endregion

        #region Public Properties
        public MemberInfo Member { get { return null; } }
        public Type ReturnType { get { return _builder.PropertyType; } }
        #endregion

        #region Constructor
        public PropertyEmitter(AbstractTypeEmitter parentTypeEmitter, string name, PropertyAttributes attributes, Type propertyType, Type[] arguments)
        {
            _parentTypeEmitter = parentTypeEmitter;
            _builder = parentTypeEmitter.TypeBuilder.DefineProperty(name, attributes, CallingConventions.HasThis, propertyType, null, null, arguments, null, null);
        }
        #endregion

        #region Creation Handling
        public MethodEmitter CreateGetMethod(string name, MethodAttributes attributes, MethodInfo methodToOverride)
        {
            return CreateGetMethod(name, attributes, methodToOverride, Type.EmptyTypes);
        }

        public MethodEmitter CreateGetMethod(string name, MethodAttributes attrs, MethodInfo methodToOverride, params Type[] parameters)
        {
            if (_getMethod != null) throw new InvalidOperationException("A get method exists");
            _getMethod = new MethodEmitter(_parentTypeEmitter, name, attrs, methodToOverride);
            return _getMethod;
        }

        public MethodEmitter CreateSetMethod(string name, MethodAttributes attributes, MethodInfo methodToOverride)
        {
            return CreateSetMethod(name, attributes, methodToOverride, Type.EmptyTypes);
        }

        public MethodEmitter CreateSetMethod(string name, MethodAttributes attrs, MethodInfo methodToOverride, params Type[] parameters)
        {
            if (_setMethod != null) throw new InvalidOperationException("A set method exists");
            _setMethod = new MethodEmitter(_parentTypeEmitter, name, attrs, methodToOverride);
            return _setMethod;
        }

        public void DefineCustomAttribute(CustomAttributeBuilder attribute)
        {
            _builder.SetCustomAttribute(attribute);
        }
        #endregion

        #region Generation Handling
        public void Generate()
        {
            if (_setMethod != null)
            {
                _setMethod.Generate();
                _builder.SetSetMethod(_setMethod.MethodBuilder);
            }

            if (_getMethod != null)
            {
                _getMethod.Generate();
                _builder.SetGetMethod(_getMethod.MethodBuilder);
            }
        }

        public void EnsureValidCodeBlock()
        {
            if (_setMethod != null) _setMethod.EnsureValidCodeBlock();
            if (_getMethod != null) _getMethod.EnsureValidCodeBlock();
        }
        #endregion
    }
}
