using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dev2.Common;
using Warewolf.Resource.Errors;
// ReSharper disable NonLocalizedString

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    public interface IAssemblyLoader
    {
        bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly);
        // ReSharper disable once UnusedMemberInSuper.Global
        void LoadDepencencies(Assembly asm, string assemblyLocation);
    }
    public class AssemblyLoader : IAssemblyLoader
    {
        readonly List<string> _loadedAssemblies = new List<string>();
        #region Implementation of IAssemblyLoader

        public bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly)
        {
            loadedAssembly = null;

            if (assemblyLocation != null && assemblyLocation.StartsWith(GlobalConstants.GACPrefix))
            {
                try
                {
                    loadedAssembly = Assembly.Load(assemblyName);
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e);
                    throw;

                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message);
                }
            }
            else
            {
                try
                {
                    if (assemblyLocation != null)
                    {
                        loadedAssembly = Assembly.LoadFrom(assemblyLocation);
                        LoadDepencencies(loadedAssembly, assemblyLocation);
                    }
                    return true;
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e);
                    throw;
                }
                catch
                {
                    try
                    {
                        if (assemblyLocation != null)
                        {
                            loadedAssembly = Assembly.UnsafeLoadFrom(assemblyLocation);
                            LoadDepencencies(loadedAssembly, assemblyLocation);
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e);
                    }
                }
                try
                {
                    if (assemblyLocation != null)
                    {
                        var objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                        var loadedObject = objHAndle.Unwrap();
                        loadedAssembly = Assembly.GetAssembly(loadedObject.GetType());
                    }
                    LoadDepencencies(loadedAssembly, assemblyLocation);
                    return true;
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e);
                    throw;
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e);
                }
            }
            return false;
        }

        public void LoadDepencencies(Assembly asm, string assemblyLocation)
        {
            if (asm != null)
            {
                var toLoadAsm = asm.GetReferencedAssemblies();

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
                        depAsm = Assembly.Load(toLoad);
                    }
                    catch
                    {
                        var path = Path.GetDirectoryName(assemblyLocation);
                        if (path != null)
                        {
                            var myLoad = Path.Combine(path, toLoad.Name + ".dll");
                            try
                            {
                                depAsm = Assembly.LoadFrom(myLoad);
                            }
                            catch(Exception)
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
                throw new Exception(String.Format(ErrorResource.CouldNotLocateAssembly, assemblyLocation));
            }
        }

        #endregion
    }
}