using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission
{
    public sealed class NetworkProxyGenerator : BaseDisposable
    {
        private static bool IsInternal(Type target)
        {
            bool isTargetNested = target.IsNested;
            bool isNestedAndInternal = isTargetNested && (target.IsNestedAssembly || target.IsNestedFamORAssem);
            bool isInternalNotNested = target.IsVisible == false && isTargetNested == false;
            return isInternalNotNested || isNestedAndInternal;
        }

        private static bool IsAccessible(Type target, string dynamicAssemblyName)
        {
            return IsInternal(target) && target.Assembly.IsInternalToDynamicProxy(dynamicAssemblyName);
        }

        private static bool IsPublic(Type target)
        {
            return target.IsPublic || target.IsNestedPublic;
        }

        private static void AssertValidType(Type target, string dynamicAssemblyName)
        {
            if (target.IsGenericTypeDefinition)
            {
                throw new InvalidOperationException("Type " + target.FullName + " is a generic type definition. Can not create proxy for open generic types.");
            }
            if (IsPublic(target) == false && IsAccessible(target, dynamicAssemblyName) == false)
            {
                throw new InvalidOperationException("Type " + target.FullName + " is not visible to Weave. Can not create proxy for types that are not accessible.");
            }
        }

        private EmissionRuntime _runtime;
        private EmissionModule _module;
        private string _assemblyFileName;
        private bool _allowSave;

        public string AssemblyFileName { get { return _assemblyFileName; } }
        public bool CanSaveAssembly { get { return _allowSave; } }

        #region Constructor
        public NetworkProxyGenerator(string assemblyName, string modulePath, bool memoryOnly)
        {
            _allowSave = !memoryOnly;
            _runtime = new EmissionRuntime(assemblyName, _assemblyFileName = modulePath, memoryOnly);
            _module = _runtime.CreateModule();
        }

        public NetworkProxyGenerator(bool memoryOnly)
            : this("WeaveNetworkProxy", "WeaveNetworkProxies.dll", memoryOnly)
        {
        }

        public NetworkProxyGenerator()
            : this("WeaveNetworkProxy", "WeaveNetworkProxies.dll", true)
        {
        }
        #endregion

        #region Creation Handling
        public Type CreateNetworkProxy(Type interfaceType)
        {
            AssertValidType(interfaceType, _module.DynamicAssemblyName);

            EmissionProxyOptions options = EmissionProxyOptions.Default;
            Type type = null;

            using (NetworkInterfaceProxyGenerator generator = new NetworkInterfaceProxyGenerator(_module, interfaceType))
            {
                type = generator.Generate(typeof(System.Network.__BaseNetworkTransparentProxy), options);
            }

            return type;
        }
        #endregion

        #region Save Handling
        public void SaveAssembly()
        {
            if (!_allowSave) throw new InvalidOperationException("This generator was created in memory only and cannot save the dynamic assembly to the physical disk.");
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            _runtime.SaveAssembly();
        }
        #endregion

        #region Disposal Handling
        protected override void OnDispose()
        {
            _module.Dispose();
            _runtime.Dispose();
            _module = null;
            _runtime = null;
        }
        #endregion
    }
}
