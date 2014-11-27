
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
