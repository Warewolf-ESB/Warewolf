
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
using System.Text;
using System.Threading.Tasks;
using System.Emission.Contributors;
using System.Emission.Meta;
using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
    internal sealed class NetworkInterfaceProxyGenerator : BaseDisposable
    {
        #region Static Members
        private static void AssertNotGenericTypeDefinition(Type type, string argumentName)
        {
            if (type != null && type.IsGenericTypeDefinition)
                throw new ArgumentException("Type cannot be a generic type definition. Type: " + type.FullName, argumentName);
        }

        private static void AssertNotGenericTypeDefinitions(IEnumerable<Type> types, string argumentName)
        {
            if (types == null) return;
            foreach (Type t in types) AssertNotGenericTypeDefinition(t, argumentName);
        }

        private static void EnsureValidBaseType(Type type)
        {
            if (type == null) throw new ArgumentException("Base type for proxy is null reference. Please set it to System.Object or some other valid type.");
            if (!type.IsClass) ThrowInvalidBaseType(type, "it is not a class type");
            if (type.IsSealed) ThrowInvalidBaseType(type, "it is sealed");

            ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (constructor == null || constructor.IsPrivate) ThrowInvalidBaseType(type, "it does not have accessible parameterless constructor");
        }

        private static bool ImplementedByTarget(ICollection<Type> targetInterfaces, Type interfaceType)
        {
            return targetInterfaces.Contains(interfaceType);
        }

        private static void ThrowInvalidBaseType(Type type, string doesNotHaveAccessibleParameterlessConstructor)
        {
            string format = "Type {0} is not valid base type for interface proxy, because {1}. Only a non-sealed class with non-private default constructor can be used as base type for interface proxy. Please use some other valid type.";
            throw new ArgumentException(string.Format(format, type, doesNotHaveAccessibleParameterlessConstructor));
        }
        #endregion

        #region Instance Fields
        private EmissionModule _module;
        private Type _targetType;
        private EmissionProxyOptions _emissionOptions;
        private FieldReference _targetField;
        #endregion

        #region Constructor
        internal NetworkInterfaceProxyGenerator(EmissionModule module, Type targetType)
        {
            _module = module;
            _targetType = targetType;
            AssertNotGenericTypeDefinition(targetType, "targetType");
        }
        #endregion

        #region Generation Handling
        public Type Generate(Type proxyBaseType, EmissionProxyOptions options)
        {
            AssertNotGenericTypeDefinition(proxyBaseType, "proxyBaseType");
            EnsureValidBaseType(options.InterfaceProxyBaseType);

            _emissionOptions = options;

            EmissionCacheKey cacheKey = new EmissionCacheKey(proxyBaseType, _targetType, Type.EmptyTypes, options);
            return ObtainProxyType(cacheKey, proxyBaseType);
        }

        private Type GenerateType(string typeName, Type proxyBaseType, IDesignatingScope namingScope)
        {
            IEnumerable<IEmissionContributor> contributors;
            IEnumerable<Type> allInterfaces = GetTypeImplementerMapping(Type.EmptyTypes, _targetType, out contributors, namingScope);
            MetaType model = new MetaType();

            foreach (IEmissionContributor contributor in contributors) contributor.CollectElements(_emissionOptions.Hook, model);
            _emissionOptions.Hook.MethodsInspected();

            ClassEmitter emitter;
            Type baseType = Init(typeName, out emitter, proxyBaseType, allInterfaces);

            ConstructorEmitter cctor = GenerateStaticConstructor(emitter);

            foreach (var contributor in contributors)
            {
                contributor.Generate(emitter, _emissionOptions);
            }



            var ctorArguments = new List<FieldReference>() { _targetField };
            var selector = emitter.GetField("__selector");
            if (selector != null)
            {
                ctorArguments.Add(selector);
            }

            FieldReference template = emitter.GetField("__template");

            if (template != null)
            {
                ctorArguments.Add(template);
            }

            GenerateConstructors(emitter, baseType, ctorArguments.ToArray());

            // Complete type initializer code body
            CompleteInitCacheMethod(cctor.CodeBuilder);

            // Crosses fingers and build type
            var generatedType = emitter.BuildType();

            InitializeStaticFields(generatedType);
            return generatedType;
        }
        #endregion

        private Type Init(string typeName, out ClassEmitter emitter, Type proxyTargetType, IEnumerable<Type> interfaces)
        {
            Type baseType = _emissionOptions.InterfaceProxyBaseType;
            emitter = BuildClassEmitter(typeName, baseType, interfaces);
            CreateFields(emitter, proxyTargetType);
            CreateTypeAttributes(emitter);
            return baseType;
        }

        private ClassEmitter BuildClassEmitter(string typeName, Type parentType, IEnumerable<Type> interfaces)
        {
            AssertNotGenericTypeDefinition(parentType, "parentType");
            AssertNotGenericTypeDefinitions(interfaces, "interfaces");

            return new ClassEmitter(_module, typeName, parentType, interfaces);
        }

        private void CreateFields(ClassEmitter emitter, Type proxyTargetType)
        {
            emitter.CreateField("__template", typeof(System.Network.PacketTemplate), FieldAttributes.Private);

            _targetField = emitter.CreateField("__target", proxyTargetType, FieldAttributes.Private);
        }

        private void CreateTypeAttributes(ClassEmitter emitter)
        {
            emitter.AddCustomAttributes(_emissionOptions);
        }

        private void CompleteInitCacheMethod(ConstructorCodeBuilder constCodeBuilder)
        {
            constCodeBuilder.AddStatement(new ReturnStatement());
        }

        private void InitializeStaticFields(Type builtType)
        {
            
        }

        private void GenerateConstructors(ClassEmitter emitter, Type baseType, params FieldReference[] fields)
        {
            ConstructorInfo[] constructors = baseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (ConstructorInfo constructor in constructors)
            {
                if (!IsConstructorVisible(constructor)) continue;
                GenerateConstructor(emitter, constructor, fields);
            }
        }

        private void GenerateConstructor(ClassEmitter emitter, ConstructorInfo baseConstructor, params FieldReference[] fields)
        {
            ArgumentReference[] args;
            ParameterInfo[] baseConstructorParams = null;

            if (baseConstructor != null) baseConstructorParams = baseConstructor.GetParameters();

            if (baseConstructorParams != null && baseConstructorParams.Length != 0)
            {
                args = new ArgumentReference[fields.Length + baseConstructorParams.Length];
                int offset = fields.Length;

                for (int i = offset; i < offset + baseConstructorParams.Length; i++)
                {
                    ParameterInfo paramInfo = baseConstructorParams[i - offset];
                    args[i] = new ArgumentReference(paramInfo.ParameterType);
                }
            }
            else args = new ArgumentReference[fields.Length];

            for (int i = 0; i < fields.Length; i++) args[i] = new ArgumentReference(fields[i].Reference.FieldType);

            ConstructorEmitter constructor = emitter.CreateConstructor(args);

            if (baseConstructorParams != null && baseConstructorParams.Length != 0)
            {
                ParameterInfo last = baseConstructorParams.Last();

                if (last.ParameterType.IsArray && last.HasAttribute<ParamArrayAttribute>())
                {
                    ParameterBuilder parameter = constructor.ConstructorBuilder.DefineParameter(args.Length, ParameterAttributes.None, last.Name);
                    CustomAttributeBuilder builder = AttributeUtil.CreateBuilder<ParamArrayAttribute>();
                    parameter.SetCustomAttribute(builder);
                }
            }

            for (var i = 0; i < fields.Length; i++)
            {
                constructor.CodeBuilder.AddStatement(new AssignStatement(fields[i], args[i].ToExpression()));
            }

            if (baseConstructor != null)
            {
                ArgumentReference[] slice = new ArgumentReference[baseConstructorParams.Length];
                Array.Copy(args, fields.Length, slice, 0, baseConstructorParams.Length);
                constructor.CodeBuilder.InvokeBaseConstructor(baseConstructor, slice);
            }
            else constructor.CodeBuilder.InvokeBaseConstructor();

            constructor.CodeBuilder.AddStatement(new ReturnStatement());
        }

        private ConstructorEmitter GenerateStaticConstructor(ClassEmitter emitter)
        {
            return emitter.CreateTypeConstructor();
        }

        private bool IsConstructorVisible(ConstructorInfo constructor)
        {
            return constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly;
        }

        #region Mapping Handling
        private IEnumerable<Type> GetTypeImplementerMapping(Type[] interfaces, Type proxyTargetType, out IEnumerable<IEmissionContributor> contributors, IDesignatingScope designatingScope)
        {
            IDictionary<Type, IEmissionContributor> typeImplementerMapping = new Dictionary<Type, IEmissionContributor>();

            ICollection<Type> targetInterfaces = proxyTargetType.GetAllInterfaces();
            ICollection<Type> additionalInterfaces = TypeUtil.GetAllInterfaces(interfaces);
            IEmissionContributor target = AddMapping(typeImplementerMapping, proxyTargetType, targetInterfaces, additionalInterfaces, designatingScope);

            NetworkInterfaceProxyContributor additionalInterfacesContributor = new NetworkInterfaceProxyContributor(designatingScope, _module.DynamicAssemblyName);

            foreach (Type interfaceType in additionalInterfaces)
            {
                if (typeImplementerMapping.ContainsKey(interfaceType)) continue;
                additionalInterfacesContributor.AddInterfaceToProxy(interfaceType);
                AddMappingUnsafe(interfaceType, additionalInterfacesContributor, typeImplementerMapping);
            }

            contributors = new List<IEmissionContributor>() { target, additionalInterfacesContributor };

            return typeImplementerMapping.Keys;
        }

        private IEmissionContributor AddMapping(IDictionary<Type, IEmissionContributor> interfaceTypeImplementerMapping, Type proxyTargetType, ICollection<Type> targetInterfaces, ICollection<Type> additionalInterfaces, IDesignatingScope designatingScope)
        {
            NetworkInterfaceProxyContributor contributor = new NetworkInterfaceProxyContributor(designatingScope, _module.DynamicAssemblyName);

            foreach (Type interfaceType in _targetType.GetAllInterfaces())
            {
                contributor.AddInterfaceToProxy(interfaceType);
                AddMappingUnsafe(interfaceType, contributor, interfaceTypeImplementerMapping);
            }

            return contributor;
        }

        private void AddMappingUnsafe(Type interfaceType, IEmissionContributor implementer, IDictionary<Type, IEmissionContributor> mapping)
        {
            mapping.Add(interfaceType, implementer);
        }
        #endregion

        #region Cache Handling
        private Type ObtainProxyType(EmissionCacheKey cacheKey, Type proxyBaseType)
        {
            _module.CacheLock.EnterUpgradeableReadLock();

            try
            {
                Type cacheType = _module.GetFromCache(cacheKey);
                if (cacheType != null) return cacheType;


                _module.CacheLock.EnterWriteLock();

                try
                {
                    cacheType = _module.GetFromCache(cacheKey);
                    if (cacheType != null) return cacheType;

                    string name = _module.DesignatingScope.GenerateDesignation("Weave.Proxies." + _targetType.Name + "Proxy");

                    Type proxyType = GenerateType(name, proxyBaseType, _module.DesignatingScope.GenerateChildScope("Weave.Proxies.ChildScope"));
                    _module.RegisterInCache(cacheKey, proxyType);

                    return proxyType;
                }
                finally
                {
                    _module.CacheLock.ExitWriteLock();
                }

            }
            finally
            {
                _module.CacheLock.ExitUpgradeableReadLock();
            }
        }
        #endregion

        #region Disposal Handling
        protected override void OnDispose()
        {
            _module = null;
            _emissionOptions = null;
            _targetType = null;
        }
        #endregion
    }
}
