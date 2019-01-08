/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dev2
{

    public interface ILoadResources
    {
        void CheckExampleResources();
        void LoadActivityCache();
        void LoadReferences(Assembly asm, HashSet<string> inspected);
        void LoadResourceCatalog();
        void LoadServerWorkspace();
        void MethodsToBeDepricated();
        void MigrateOldResources();
        void MigrateOldTests();
        void PreloadReferences();
        void ValidateResourceFolder();
    }

    public class LoadResources : ILoadResources
    {
        readonly string _resourceDirectory;
        readonly IWriter _writer;
        readonly IDirectory _directory;
        private IResourceCatalog _catalog;
        private readonly IResourceCatalogFactory _resourceCatalogFactory;

        public LoadResources(string resourceDirectory, IWriter writer)
            :this(resourceDirectory, writer, new DirectoryWrapper(), new ResourceCatalogFactory())
        {
            
        }
        public LoadResources(string resourceDirectory, IWriter writer, IDirectory directory, IResourceCatalogFactory resourceCatalogFactory )
        {
            _writer = writer;
            _directory = directory;
            _resourceDirectory = resourceDirectory;
            _resourceCatalogFactory = resourceCatalogFactory;
            _catalog = _resourceCatalogFactory.New();

        }

        public void CheckExampleResources()
        {
            var serverReleaseResources = Path.Combine(EnvironmentVariables.ApplicationPath, _resourceDirectory);
            if (_directory.Exists(EnvironmentVariables.ResourcePath) && _directory.Exists(serverReleaseResources))
            {
                _catalog.LoadExamplesViaBuilder(serverReleaseResources);
            }
        }
        
        public void LoadResourceCatalog()
        {
            MigrateOldResources();
            ValidateResourceFolder();
            _writer.Write("Loading resource catalog...  ");
            var catalog = _resourceCatalogFactory.New();
            MethodsToBeDepricated();
            _writer.WriteLine("done.");
            _catalog = catalog;
        }

        public void MethodsToBeDepricated()
        {
            _catalog.CleanUpOldVersionControlStructure();
        }

        public void LoadActivityCache()
        {
            PreloadReferences();
            _writer.Write("Loading resource activity cache...  ");
            _catalog.LoadServerActivityCache();
            _writer.WriteLine("done.");
        }

        public void ValidateResourceFolder()
        {
            var folder = EnvironmentVariables.ResourcePath;
            if (!_directory.Exists(folder))
            {
                _directory.CreateDirectory(folder);
            }
        }

        public void MigrateOldResources()
        {
            var serverBinResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            if (!_directory.Exists(EnvironmentVariables.ResourcePath) && _directory.Exists(serverBinResources))
            {
                var dir = new DirectoryHelper();
                dir.Copy(serverBinResources, EnvironmentVariables.ResourcePath, true);
                dir.CleanUp(serverBinResources);
            }
        }

        public void PreloadReferences()
        {
            _writer.Write("Preloading assemblies...  ");
            var currentAsm = typeof(ServerLifecycleManager).Assembly;
            var inspected = new HashSet<string> { currentAsm.GetName().ToString(), "GroupControls" };
            LoadReferences(currentAsm, inspected, new AssemblyLoader());
            _writer.WriteLine("done.");
        }

        public void LoadReferences(Assembly asm, HashSet<string> inspected, AssemblyLoader assemblyLoader)
        {
            var allReferences = assemblyLoader.assemblyNames(asm);

            foreach (var toLoad in allReferences)
            {
                if (!inspected.Contains(toLoad.Name))
                {
                    inspected.Add(toLoad.Name);
                    LoadReferences(assemblyLoader.LoadAndReturn(toLoad), inspected,new AssemblyLoader());
                }
            }
        }
        
        public void LoadServerWorkspace()
        {
            _writer.Write("Loading server workspace...  ");

            var instance = WorkspaceRepository.Instance;
            if (instance != null)
            {
                _writer.WriteLine("done.");
            }
        }
        
        public void MigrateOldTests()
        {
            var serverBinTests = Path.Combine(EnvironmentVariables.ApplicationPath, "Tests");
            if (!_directory.Exists(EnvironmentVariables.TestPath) && _directory.Exists(serverBinTests))
            {
                var dir = new DirectoryHelper();
                dir.Copy(serverBinTests, EnvironmentVariables.TestPath, true);
                dir.CleanUp(serverBinTests);
            }
        }

        public void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            throw new NotImplementedException();
        }
    }
}
