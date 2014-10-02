
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ConstructorCollection = System.Collections.ObjectModel.Collection<System.Emission.Emitters.ConstructorEmitter>;
using EventCollection = System.Collections.ObjectModel.Collection<System.Emission.Emitters.EventEmitter>;
using MethodCollection = System.Collections.ObjectModel.Collection<System.Emission.Emitters.MethodEmitter>;
using NestedClassCollection = System.Collections.ObjectModel.Collection<System.Emission.Emitters.NestedClassEmitter>;
using PropertiesCollection = System.Collections.ObjectModel.Collection<System.Emission.Emitters.PropertyEmitter>;

namespace System.Emission.Emitters
{
    internal abstract class AbstractTypeEmitter
    {
        #region Constants
        private const MethodAttributes DefaultAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public;
        #endregion

        #region Instance Fields
        private ConstructorCollection _constructors;
        private EventCollection _events;
        private IDictionary<string, FieldReference> _fields = new Dictionary<string, FieldReference>(StringComparer.OrdinalIgnoreCase);
        private MethodCollection _methods;
        private Dictionary<String, GenericTypeParameterBuilder> _nameGenericTypeMapping;
        private NestedClassCollection _nested;
        private PropertiesCollection _properties;
        private TypeBuilder _typeBuilder;
        private GenericTypeParameterBuilder[] _genericTypeParams;
        private string _dynamicAssemblyName;
        private TypeConstructorEmitter _classConstructor;
        #endregion

        #region Public Properties
        public Type BaseType { get { if (_typeBuilder.IsInterface) throw new InvalidOperationException("This emitter represents an interface; interfaces have no base types."); return _typeBuilder.BaseType; } }
        public TypeConstructorEmitter ClassConstructor { get { return _classConstructor; } }
        public ConstructorCollection Constructors { get { return _constructors; } }
        public GenericTypeParameterBuilder[] GenericTypeParams { get { return _genericTypeParams; } }
        public NestedClassCollection Nested { get { return _nested; } }
        public TypeBuilder TypeBuilder { get { return _typeBuilder; } }
        public string DynamicAssemblyName { get { return _dynamicAssemblyName; } }
        #endregion

        #region Constructor
        protected AbstractTypeEmitter(TypeBuilder typeBuilder, string dynamicAssemblyName)
        {
            _typeBuilder = typeBuilder;
            _dynamicAssemblyName = dynamicAssemblyName;
            _nested = new NestedClassCollection();
            _methods = new MethodCollection();
            _constructors = new ConstructorCollection();
            _properties = new PropertiesCollection();
            _events = new EventCollection();
            _nameGenericTypeMapping = new Dictionary<String, GenericTypeParameterBuilder>();
        }
        #endregion

        #region Build Handling
        public virtual Type BuildType()
        {
            EnsureBuildersAreInAValidState();
            Type type = CreateType(_typeBuilder);
            foreach (NestedClassEmitter builder in _nested) builder.BuildType();
            return type;
        }
        #endregion

        #region Creation Handling
        protected Type CreateType(TypeBuilder type)
        {
            return type.CreateType();
        }

        public ConstructorEmitter CreateConstructor(params ArgumentReference[] arguments)
        {
            if (_typeBuilder.IsInterface) throw new InvalidOperationException("Interfaces cannot have constructors.");
            ConstructorEmitter member = new ConstructorEmitter(this, arguments);
            _constructors.Add(member);
            return member;
        }

        public void CreateDefaultConstructor()
        {
            if (_typeBuilder.IsInterface) throw new InvalidOperationException("Interfaces cannot have constructors.");
            _constructors.Add(new ConstructorEmitter(this));
        }

        public EventEmitter CreateEvent(string name, EventAttributes atts, Type type)
        {
            EventEmitter eventEmitter = new EventEmitter(this, name, atts, type);
            _events.Add(eventEmitter);
            return eventEmitter;
        }

        public FieldReference CreateField(string name, Type fieldType)
        {
            return CreateField(name, fieldType, FieldAttributes.Public);
        }

        public FieldReference CreateField(string name, Type fieldType, FieldAttributes atts)
        {
            FieldBuilder fieldBuilder = _typeBuilder.DefineField(name, fieldType, atts);
            FieldReference reference = new FieldReference(fieldBuilder);
            _fields[name] = reference;
            return reference;
        }

        public MethodEmitter CreateMethod(string name, MethodAttributes attrs, Type returnType, params Type[] argumentTypes)
        {
            MethodEmitter member = new MethodEmitter(this, name, attrs, returnType, argumentTypes ?? Type.EmptyTypes);
            _methods.Add(member);
            return member;
        }

        public MethodEmitter CreateMethod(string name, Type returnType, params Type[] parameterTypes)
        {
            return CreateMethod(name, DefaultAttributes, returnType, parameterTypes);
        }

        public MethodEmitter CreateMethod(string name, MethodInfo methodToUseAsATemplate)
        {
            return CreateMethod(name, DefaultAttributes, methodToUseAsATemplate);
        }

        public MethodEmitter CreateMethod(string name, MethodAttributes attributes, MethodInfo methodToUseAsATemplate)
        {
            MethodEmitter method = new MethodEmitter(this, name, attributes, methodToUseAsATemplate);
            _methods.Add(method);
            return method;
        }

        public PropertyEmitter CreateProperty(string name, PropertyAttributes attributes, Type propertyType, Type[] arguments)
        {
            PropertyEmitter propEmitter = new PropertyEmitter(this, name, attributes, propertyType, arguments);
            _properties.Add(propEmitter);
            return propEmitter;
        }

        public FieldReference CreateStaticField(string name, Type fieldType)
        {
            return CreateStaticField(name, fieldType, FieldAttributes.Public);
        }

        public FieldReference CreateStaticField(string name, Type fieldType, FieldAttributes atts)
        {
            return CreateField(name, fieldType, atts | FieldAttributes.Static);
        }

        public ConstructorEmitter CreateTypeConstructor()
        {
            TypeConstructorEmitter member = new TypeConstructorEmitter(this);
            _constructors.Add(member);
            _classConstructor = member;
            return member;
        }
        #endregion

        #region Attribute Handling
        public void AddCustomAttributes(EmissionProxyOptions options)
        {
            foreach (CustomAttributeBuilder attribute in options.AdditionalAttributes)
                _typeBuilder.SetCustomAttribute(attribute);
        }

        public void DefineCustomAttribute(CustomAttributeBuilder attribute)
        {
            _typeBuilder.SetCustomAttribute(attribute);
        }

        public void DefineCustomAttribute<TAttribute>(object[] constructorArguments) where TAttribute : Attribute
        {
            CustomAttributeBuilder customAttributeBuilder = AttributeUtil.CreateBuilder(typeof(TAttribute), constructorArguments);
            _typeBuilder.SetCustomAttribute(customAttributeBuilder);
        }

        public void DefineCustomAttribute<TAttribute>() where TAttribute : Attribute, new()
        {
            CustomAttributeBuilder customAttributeBuilder = AttributeUtil.CreateBuilder<TAttribute>();
            _typeBuilder.SetCustomAttribute(customAttributeBuilder);
        }

        public void DefineCustomAttributeFor<TAttribute>(FieldReference field) where TAttribute : Attribute, new()
        {
            CustomAttributeBuilder customAttributeBuilder = AttributeUtil.CreateBuilder<TAttribute>();
            FieldBuilder fieldbuilder = field.Fieldbuilder;
            if (fieldbuilder == null) throw new ArgumentException("Invalid field reference.This reference does not point to field on type being generated", "field");
            fieldbuilder.SetCustomAttribute(customAttributeBuilder);
        }
        #endregion

        #region Field Handling
        public IEnumerable<FieldReference> GetAllFields()
        {
            return _fields.Values;
        }

        public FieldReference GetField(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            FieldReference value;
            _fields.TryGetValue(name, out value);
            return value;
        }
        #endregion

        #region Argument Handling
        public Type GetGenericArgument(String genericArgumentName)
        {
            return _nameGenericTypeMapping[genericArgumentName];
        }

        public Type[] GetGenericArgumentsFor(Type genericType)
        {
            List<Type> types = new List<Type>();

            foreach (Type genType in genericType.GetGenericArguments())
            {
                if (genType.IsGenericParameter)
                    types.Add(_nameGenericTypeMapping[genType.Name]);
                else
                    types.Add(genType);
            }

            return types.ToArray();
        }

        public Type[] GetGenericArgumentsFor(MethodInfo genericMethod)
        {
            List<Type> types = new List<Type>();
            foreach (Type genType in genericMethod.GetGenericArguments()) types.Add(_nameGenericTypeMapping[genType.Name]);
            return types.ToArray();
        }

        public void SetGenericTypeParameters(GenericTypeParameterBuilder[] genericTypeParameterBuilders)
        {
            _genericTypeParams = genericTypeParameterBuilders;
        }

        public void CopyGenericParametersFromMethod(MethodInfo methodToCopyGenericsFrom)
        {
            if (_genericTypeParams != null) throw new InvalidOperationException("CopyGenericParametersFromMethod: cannot invoke me twice");
            SetGenericTypeParameters(GenericUtil.CopyGenericArguments(methodToCopyGenericsFrom, _typeBuilder, _nameGenericTypeMapping));
        }
        #endregion

        #region Ensure(...)
        protected virtual void EnsureBuildersAreInAValidState()
        {
            if (!_typeBuilder.IsInterface && _constructors.Count == 0)
                CreateDefaultConstructor();

            foreach (IEmissionMemberEmitter builder in _properties)
            {
                builder.EnsureValidCodeBlock();
                builder.Generate();
            }

            foreach (IEmissionMemberEmitter builder in _events)
            {
                builder.EnsureValidCodeBlock();
                builder.Generate();
            }

            foreach (IEmissionMemberEmitter builder in _constructors)
            {
                builder.EnsureValidCodeBlock();
                builder.Generate();
            }

            foreach (IEmissionMemberEmitter builder in _methods)
            {
                builder.EnsureValidCodeBlock();
                builder.Generate();
            }
        }
        #endregion
    }
}
