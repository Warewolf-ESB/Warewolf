using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Explorer;
using Dev2.Shared_Models.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.SharedModels
{
    public class ServerExplorerRepositoryTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerExplorerRepository_Load")]
        public void ServerExplorerRepository_Load_AssertRootLevelIsFolder_ExpectFolder()
        {
            //------------Setup for test--------------------------
            var serverExplorerRepository = new ServerExplorerRepository();

            //------------Execute Test---------------------------
            var root = serverExplorerRepository.Load();
            //------------Assert Results-------------------------
            Assert.AreEqual(root.ExplorerItemType,ExplorerItemType.Folder);
            Assert.AreEqual(2,root.Children.Count );
            Assert.AreEqual(root.Children.First().DisplayName,"Services");
            Assert.AreEqual(root.Children[1].DisplayName,"Bobs");
        }
    }

}
