// Decompiled with JetBrains decompiler
// Type: System.Emission.NetworkProxyGenerator
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Network;

namespace System.Emission
{
  public sealed class NetworkProxyGenerator : BaseDisposable
  {
    private EmissionRuntime _runtime;
    private EmissionModule _module;
    private string _assemblyFileName;
    private bool _allowSave;

    public string AssemblyFileName => this._assemblyFileName;

    public bool CanSaveAssembly => this._allowSave;

    private static bool IsInternal(Type target)
    {
      bool isNested = target.IsNested;
      bool flag = isNested && (target.IsNestedAssembly || target.IsNestedFamORAssem);
      return ((target.IsVisible ? 0 : (!isNested ? 1 : 0)) | (flag ? 1 : 0)) != 0;
    }

    private static bool IsAccessible(Type target, string dynamicAssemblyName) => NetworkProxyGenerator.IsInternal(target) && target.Assembly.IsInternalToDynamicProxy(dynamicAssemblyName);

    private static bool IsPublic(Type target) => target.IsPublic || target.IsNestedPublic;

    private static void AssertValidType(Type target, string dynamicAssemblyName)
    {
      if (target.IsGenericTypeDefinition)
        throw new InvalidOperationException("Type " + target.FullName + " is a generic type definition. Can not create proxy for open generic types.");
      if (!NetworkProxyGenerator.IsPublic(target) && !NetworkProxyGenerator.IsAccessible(target, dynamicAssemblyName))
        throw new InvalidOperationException("Type " + target.FullName + " is not visible to Weave. Can not create proxy for types that are not accessible.");
    }

    public NetworkProxyGenerator(string assemblyName, string modulePath, bool memoryOnly)
    {
      this._allowSave = !memoryOnly;
      this._runtime = new EmissionRuntime(assemblyName, this._assemblyFileName = modulePath, memoryOnly);
      this._module = this._runtime.CreateModule();
    }

    public NetworkProxyGenerator(bool memoryOnly)
      : this("WeaveNetworkProxy", "WeaveNetworkProxies.dll", memoryOnly)
    {
    }

    public NetworkProxyGenerator()
      : this("WeaveNetworkProxy", "WeaveNetworkProxies.dll", true)
    {
    }

    public Type CreateNetworkProxy(Type interfaceType)
    {
      NetworkProxyGenerator.AssertValidType(interfaceType, this._module.DynamicAssemblyName);
      EmissionProxyOptions options = EmissionProxyOptions.Default;
      using (NetworkInterfaceProxyGenerator interfaceProxyGenerator = new NetworkInterfaceProxyGenerator(this._module, interfaceType))
        return interfaceProxyGenerator.Generate(typeof (__BaseNetworkTransparentProxy), options);
    }

    [Obsolete]
    public void SaveAssembly()
    {
      if (!this._allowSave)
        throw new InvalidOperationException("This generator was created in memory only and cannot save the dynamic assembly to the physical disk.");
      if (this.IsDisposed)
        throw new ObjectDisposedException(this.GetType().Name);
      this._runtime.SaveAssembly();
    }

    protected override void OnDispose()
    {
      this._module.Dispose();
      this._runtime.Dispose();
      this._module = (EmissionModule) null;
      this._runtime = (EmissionRuntime) null;
    }
  }
}
