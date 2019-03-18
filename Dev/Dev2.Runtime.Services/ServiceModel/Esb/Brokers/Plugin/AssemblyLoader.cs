#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
        readonly IAssemblyWrapper _assemblyWrapper;

        public AssemblyLoader()
            : this(new AssemblyWrapper())
        {

        }
        public AssemblyLoader(IAssemblyWrapper assemblyWrapper)
        {
            _assemblyWrapper = assemblyWrapper;
        }

        readonly List<string> _loadedAssemblies = new List<string>();
        #region Implementation of IAssemblyLoader

        public bool TryLoadAssembly(string assemblyLocation, string assemblyName, out Assembly loadedAssembly)
        {
            loadedAssembly = null;

            var gacPrefix = GlobalConstants.GACPrefix;
            if (assemblyLocation != null && assemblyLocation.StartsWith(gacPrefix))
            {
                try
                {
                    return LoadAssembly(assemblyLocation, assemblyName, gacPrefix, ref loadedAssembly);
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
                    return LoadAssembly(assemblyLocation, ref loadedAssembly);
                }
                catch (BadImageFormatException e)//WOLF-1640
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    throw;
                }
                catch (Exception ex)
                {
                    try
                    {
                        return LoadUnsafeAssembly(assemblyLocation, ref loadedAssembly);
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

        private bool LoadUnsafeAssembly(string assemblyLocation, ref Assembly loadedAssembly)
        {
            if (assemblyLocation != null)
            {
                loadedAssembly = _assemblyWrapper.UnsafeLoadFrom(assemblyLocation);
                LoadDepencencies(loadedAssembly, assemblyLocation);
            }
            return true;
        }

        bool LoadAssembly(string assemblyLocation, ref Assembly loadedAssembly)
        {
            if (assemblyLocation != null)
            {
                loadedAssembly = _assemblyWrapper.LoadFrom(assemblyLocation);
                LoadDepencencies(loadedAssembly, assemblyLocation);
            }
            return true;
        }

        bool LoadAssembly(string assemblyLocation, string assemblyName, string gacPrefix, ref Assembly loadedAssembly)
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
                    catch (Exception ex)
                    {
                        depAsm = LoadDependantAssembly(assemblyLocation, toLoad, fullName);
                    }
                    if (depAsm != null)
                    {
                        TryLoadDepencencies(assemblyLocation, fullName, depAsm);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format(ErrorResource.CouldNotLocateAssembly, assemblyLocation));
            }
        }

        void TryLoadDepencencies(string assemblyLocation, string fullName, Assembly depAsm)
        {
            if (!_loadedAssemblies.Contains(fullName))
            {
                _loadedAssemblies.Add(fullName);
            }
            LoadDepencencies(depAsm, assemblyLocation);
        }

        Assembly LoadDependantAssembly(string assemblyLocation, AssemblyName toLoad, string fullName)
        {
            Assembly depAsm = null;
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

            return depAsm;
        }

        #endregion
    }


}