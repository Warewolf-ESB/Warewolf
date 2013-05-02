using Dev2.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dev2.Studio.Core.Configuration
{
    /// <summary>
    /// This repository is intentionally lean, i.e. it doesn't support a find with a predicate.
    /// The reason for this is it deals with assemblies, which once loaded into an appdomain
    /// can't be unloaded. Please keep it lean and avoid loading all assemblies, and avoid loading
    /// an assembly more than once. 
    /// 
    /// The reason these assemblies aren't loaded into a seperate app domain is because they contain
    /// WPF UI elements which need to be used in the main app domain, and these can't cross appdomain 
    /// boundries.
    /// 
    /// As a point of interest, MAF (http://msdn.microsoft.com/en-us/library/bb384200.aspx) could be
    /// used to accomplish this, but it was decided against since the configuration DLLs are less than
    /// 50k each and are only ever loaded once (enforced by hashes of the file). This means that even if 
    /// there were 1000 different versions of the server and the studio connected to every one of those 
    /// versions all the assemblies combined would consume less that 50mb of memory. In addition once
    /// a configuration assembly is loaded the desired behaviour is to keep it in memory.
    /// </summary>
    [Export(typeof(IRuntimeConfigurationAssemblyRepository))]
    public class RuntimeConfigurationAssemblyRepository : IRuntimeConfigurationAssemblyRepository
    {
        #region Constructors

        public RuntimeConfigurationAssemblyRepository()
        {
            Init(GetDefaultPath());
        }

        public RuntimeConfigurationAssemblyRepository(string repositoryPath)
        {
            Init(repositoryPath);
        }

        #endregion

        #region Private Properties

        private string RepositoryPath { get; set; }

        private Dictionary<string, Assembly> AssemblyCache { get; set; }

        #endregion

        #region Methods

        public IEnumerable<string> AllHashes()
        {
            return Directory.GetDirectories(RepositoryPath).Select(d => new DirectoryInfo(d).Name);
        }

        public Assembly Load(string hash)
        {
            // Check hash isn't empty
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("Hash can't be empty");
            }

            // Check assembly cache before hitting teh file system
            Assembly assembly;
            if (AssemblyCache.TryGetValue(hash, out assembly))
            {
                return assembly;
            }

            // Get assembly path
            string assemblyPath = GetAssemblyPath(hash);

            // Check file exists before trying to read it
            if (!File.Exists(assemblyPath))
            {
                throw new IOException(string.Format("Assembly for hash '{0}' doesn't exist.", hash));
            }

            // Load the assembly, add it to the cache then return
            assembly = Assembly.LoadFile(assemblyPath);
            AssemblyCache.Add(hash, assembly);
            return assembly;
        }

        public void Add(string hash, byte[] assemblyData)
        {
            // Check hash isn't empty
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentException("Hash can't be empty");
            }

            // Get assembly path
            string assemblyPath = GetAssemblyPath(hash);

            // Ensure directory exists
            Exception ex;
            if (!TryEnsureDirecoryExists(Path.GetDirectoryName(assemblyPath), out ex))
            {
                throw new IOException("Unable to create the assembly directory.", ex);
            }

            // Write assembly data to the file system
            File.WriteAllBytes(assemblyPath, assemblyData);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initlializes the specified repository path.
        /// </summary>
        /// <param name="repositoryPath">The repository path.</param>
        /// <exception cref="System.IO.IOException">Invalid repository path.</exception>
        private void Init(string repositoryPath)
        {
            AssemblyCache = new Dictionary<string, Assembly>();
            RepositoryPath = repositoryPath;

            // Check path isn't empty
            if (string.IsNullOrWhiteSpace(RepositoryPath))
            {
                throw new IOException("Invalid repository path.");
            }

            // Ensure directory exists
            Exception ex;
            if (!TryEnsureDirecoryExists(RepositoryPath, out ex))
            {
                throw new IOException("Unable to create the repository directory.", ex);
            }
        }

        /// <summary>
        /// Gets the path where the assembly with the specified hash will be stored.
        /// </summary>
        private string GetAssemblyPath(string hash)
        {
            string path = Path.Combine(RepositoryPath, hash);
            path = Path.Combine(path, GlobalConstants.Dev2RuntimeConfigurationAssemblyName);

            return path;
        }

        /// <summary>
        /// Gets the default path.
        /// </summary>
        private string GetDefaultPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            path = Path.Combine(path, StringResources.App_Data_Directory);
            path = Path.Combine(path, StringResources.RuntimeConfigurationAssemblyDirectory);

            return path;
        }

        /// <summary>
        /// Tries the ensure direcory exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="ex">The exception that occured on failure.</param>
        private bool TryEnsureDirecoryExists(string path, out Exception ex)
        {
            ex = null;

            try
            {
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch(Exception e)
            {
                ex = e;
                return false;
            }

            return Directory.Exists(path);
        }

        #endregion

    }
}
