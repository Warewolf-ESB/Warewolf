using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dev2.Integration.Tests.VersionStrategy
{
    [TestClass]
    public class ServerVersionRepostoryTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("ServerVersionRepostory_CleanUpOldVersionControlStructure")]
        public void ServerVersionRepostory_CleanUpOldVersionControlStructure_MultipleFolders()
        {
            var strat = new Mock<IVersionStrategy>();
            var cat = ResourceCatalog.Instance;
            var file = new FileWrapper();
            var filePath = new PathWrapper();
            var dir = new DirectoryWrapper();
             string rootPath = EnvironmentVariables.ResourcePath;
           
            var pathResource = filePath.Combine(rootPath, "Acceptance Tests", "VersionControl");
            dir.CreateIfNotExists(pathResource);
            try
            {
                File.Move("TestData\\LoopTest.xml", filePath.Combine(rootPath, "Acceptance Tests"));
            }
            catch (System.Exception)
            {
                //
            }
          
            dir.Move("TestData\\VersionControl", filePath.Combine(pathResource));
            Assert.IsTrue(dir.Exists(pathResource));
            //ResourceCatalog.Instance.Reload();
            //------------Setup for test--------------------------
            var serverVersionRepostory = CreateServerVersionRepository(strat.Object, cat, dir, rootPath, file, filePath);
            //------------Execute Test---------------------------
            serverVersionRepostory.CleanUpOldVersionControlStructure(dir);
            //------------Assert Results-------------------------
            var newVersions = new List<string>
            {
               "1_636465118385389893_Save.xml",
               "2_636465118417350181_Save.xml",
               "3_636465118438843826_Save.xml",
               "4_636465118457085391_Save.xml",
               "5_636465118492924213_Save.xml",
               "6_636465118541785216_Save.xml",
            };
            var newPath = filePath.Combine(EnvironmentVariables.VersionsPath, "e296f78f-27ec-40de-817a-0e874528050e");
            var movedVersions = dir.GetFiles(newPath);
            Assert.AreEqual(6, movedVersions.Length);

            foreach (var item in movedVersions.Select(p=>filePath.GetFileName(p)))
            {
                CollectionAssert.Contains(newVersions, item);
            }
            foreach (var item in movedVersions)
            {
                Assert.IsTrue(File.Exists(item), item + " was not created as part of CleanUpOldVersionControlStructure");
                File.Delete(item);
            }
        }
        static ServerVersionRepository CreateServerVersionRepository(IVersionStrategy strat, IResourceCatalog cat, IDirectory dir, string rootPath, IFile file, IFilePath filePath)
        {
            var serverVersionRepostory = new ServerVersionRepository(strat, cat, dir, rootPath, file, filePath);
            return serverVersionRepostory;
        }

        private static void SetUpVersionFile(string versionFileName)
        {
            if (!Directory.Exists(EnvironmentVariables.ResourcePath))
            {
                Directory.CreateDirectory(EnvironmentVariables.ResourcePath);
            }
            if (!Directory.Exists(EnvironmentVariables.ResourcePath + "\\VersionControl"))
            {
                Directory.CreateDirectory(EnvironmentVariables.ResourcePath + "\\VersionControl");
            }
            if (!File.Exists(EnvironmentVariables.ResourcePath + "\\VersionControl\\" + versionFileName))
            {
                var file = File.Create(EnvironmentVariables.ResourcePath + "\\VersionControl\\" + versionFileName);
                file.Dispose();
            }
        }
    }
}
