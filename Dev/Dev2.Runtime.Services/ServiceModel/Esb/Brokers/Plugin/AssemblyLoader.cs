using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Warewolf.Resource.Errors;


namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    public interface IAssemblyLoader
    {
        bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly);
        
        void LoadDepencencies(Assembly asm, string assemblyLocation);
    }
    public class AssemblyLoader : IAssemblyLoader
    {
        private readonly IAssemblyWrapper _assemblyWrapper;

        public AssemblyLoader()
            : this(new AssemblyWrapper())
        {

        }
        public AssemblyLoader(IAssemblyWrapper assemblyWrapper)
        {
            _assemblyWrapper = assemblyWrapper;
        }

        private readonly List<string> _loadedAssemblies = new List<string>();
        #region Implementation of IAssemblyLoader

        public bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly)
        {
            loadedAssembly = null;

            var gacPrefix = GlobalConstants.GACPrefix;
            if (assemblyLocation != null && assemblyLocation.StartsWith(gacPrefix))
            {
                try
                {
                    try
                    {
                        loadedAssembly = _assemblyWrapper.Load(assemblyName);
                    }
                    catch (Exception e)
                    {
                        if (assemblyLocation.StartsWith(gacPrefix) && loadedAssembly == null)
                        {

                            var assemblyQualified = assemblyLocation.Replace(gacPrefix, "");
                            var indexOf = assemblyQualified.IndexOf(", processor", StringComparison.InvariantCultureIgnoreCase);
                            var correctAssemblyName = assemblyQualified.Substring(0, indexOf);
                            loadedAssembly = _assemblyWrapper.Load(correctAssemblyName);

                        }
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    }

                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    throw;

                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, GlobalConstants.WarewolfError);
                }
            }
            else
            {
                try
                {
                    if (assemblyLocation != null)
                    {
                        loadedAssembly = _assemblyWrapper.LoadFrom(assemblyLocation);
                        LoadDepencencies(loadedAssembly, assemblyLocation);
                    }
                    return true;
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    throw;
                }
                catch
                {
                    try
                    {
                        if (assemblyLocation != null)
                        {
                            loadedAssembly = _assemblyWrapper.UnsafeLoadFrom(assemblyLocation);
                            LoadDepencencies(loadedAssembly, assemblyLocation);
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    }
                }
                try
                {
                    if (assemblyLocation != null)
                    {
                        var objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                        var loadedObject = objHAndle.Unwrap();
                        loadedAssembly = _assemblyWrapper.GetAssembly(loadedObject.GetType());
                    }
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    throw;
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }
            }
            return false;
        }

        public void LoadDepencencies(Assembly asm, string assemblyLocation)
        {
            if (asm != null)
            {
                var toLoadAsm = _assemblyWrapper.GetReferencedAssemblies(asm);

                foreach (var toLoad in toLoadAsm)
                {
                    var fullName = toLoad.FullName;
                    if (_loadedAssemblies.Contains(fullName))
                    {
                        continue;
                    }
                    Assembly depAsm = null;
                    try
                    {
                        depAsm = _assemblyWrapper.Load(toLoad);
                    }
                    catch
                    {
                        var path = Path.GetDirectoryName(assemblyLocation);
                        if (path != null)
                        {
                            var myLoad = Path.Combine(path, toLoad.Name + ".dll");
                            try
                            {
                                depAsm = _assemblyWrapper.LoadFrom(myLoad);
                            }
                            catch (Exception)
                            {
                                if (!_loadedAssemblies.Contains(fullName))
                                {
                                    _loadedAssemblies.Add(fullName);
                                }
                            }
                        }
                    }
                    if (depAsm != null)
                    {
                        if (!_loadedAssemblies.Contains(fullName))
                        {
                            _loadedAssemblies.Add(fullName);
                        }
                        LoadDepencencies(depAsm, assemblyLocation);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format(ErrorResource.CouldNotLocateAssembly, assemblyLocation));
            }
        }

        #endregion
    }


}