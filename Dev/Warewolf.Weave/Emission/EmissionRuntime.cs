// Decompiled with JetBrains decompiler
// Type: System.Emission.EmissionRuntime
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  public sealed class EmissionRuntime : BaseDisposable
  {
    private AssemblyBuilder _builder;
    private string _assemblyName;
    private string _moduleName;
    private string _moduleExtension;
    private string _moduleDirectory;
    private AssemblyBuilderAccess _assemblyAccess;
    private int _moduleCount;
    private object _builderGuard;

    public EmissionRuntime(string assemblyName, string modulePath, bool memoryOnly)
    {
      if (string.IsNullOrEmpty(assemblyName))
        throw new ArgumentException("Assembly Name cannot be a null or empty string.", nameof (assemblyName));
      if (string.IsNullOrEmpty(modulePath))
        throw new ArgumentException("Module Path cannot be a null or empty string.", nameof (modulePath));
      this._builderGuard = new object();
      this._assemblyName = assemblyName;
      if (Path.HasExtension(modulePath))
      {
        this._moduleName = Path.GetFileNameWithoutExtension(modulePath);
        this._moduleExtension = Path.GetExtension(modulePath);
        this._moduleDirectory = Path.GetDirectoryName(modulePath);
      }
      else
      {
        this._moduleName = Path.GetFileName(modulePath);
        this._moduleExtension = ".dll";
      }
      if (string.IsNullOrEmpty(this._moduleDirectory))
        this._moduleDirectory = (string) null;
      this._assemblyAccess = AssemblyBuilderAccess.Run;
    }

    private void EnsureAssembly()
    {
      if (this.IsDisposed)
        throw new ObjectDisposedException(nameof (EmissionRuntime));
      if (!((Assembly) this._builder == (Assembly) null))
        return;
      this._builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName()
      {
        Name = this._assemblyName
      }, this._assemblyAccess);
    }

    public EmissionModule CreateModule() => this.CreateModule((IDesignatingScope) new DesignatingScope());

    public EmissionModule CreateModule(IDesignatingScope designatingScope) => designatingScope != null ? new EmissionModule(this.AcquireModule(), designatingScope, this._assemblyName) : throw new ArgumentNullException(nameof (designatingScope));

    internal ModuleBuilder AcquireModule()
    {
      lock (this._builderGuard)
      {
        this.EnsureAssembly();
        return this._builder.DefineDynamicModule(this._moduleName + this._moduleCount++.ToString() + this._moduleExtension);
      }
    }

    public void SaveAssembly()
    {
    }

    protected override void OnDispose()
    {
      lock (this._builderGuard)
        this._builder = (AssemblyBuilder) null;
    }
  }
}
