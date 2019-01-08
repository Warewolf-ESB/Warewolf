using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        readonly IWriter _writer;
        private ResourceCatalog _catalog;

        public LoadResources(IWriter writer)
        {
            _writer = writer;
        }

        public void CheckExampleResources()
        {
            var serverReleaseResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            if (Directory.Exists(EnvironmentVariables.ResourcePath) && Directory.Exists(serverReleaseResources))
            {
                ResourceCatalog.Instance.LoadExamplesViaBuilder(serverReleaseResources);
            }
        }


        public void LoadResourceCatalog()
        {
            MigrateOldResources();
            ValidateResourceFolder();
            _writer.Write("Loading resource catalog...  ");
            var catalog = ResourceCatalog.Instance;
            MethodsToBeDepricated();
            _writer.WriteLine("done.");
            _catalog = catalog;
        }


        public void MethodsToBeDepricated()
        {
            ResourceCatalog.Instance.CleanUpOldVersionControlStructure();
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
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }


        public void MigrateOldResources()
        {
            var serverBinResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            if (!Directory.Exists(EnvironmentVariables.ResourcePath) && Directory.Exists(serverBinResources))
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
            LoadReferences(currentAsm, inspected);
            _writer.WriteLine("done.");
        }

        public void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            var allReferences = asm.GetReferencedAssemblies();

            foreach (var toLoad in allReferences)
            {
                if (!inspected.Contains(toLoad.Name))
                {
                    inspected.Add(toLoad.Name);
                    var loaded = AppDomain.CurrentDomain.Load(toLoad);
                    LoadReferences(loaded, inspected);
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
            if (!Directory.Exists(EnvironmentVariables.TestPath) && Directory.Exists(serverBinTests))
            {
                var dir = new DirectoryHelper();
                dir.Copy(serverBinTests, EnvironmentVariables.TestPath, true);
                dir.CleanUp(serverBinTests);
            }
        }
        
    }
}
