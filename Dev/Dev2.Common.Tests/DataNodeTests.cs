/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class DataNodeTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DataNode))]
        public void DataNode_SetProperty_AreEqual_GetProperty_ExpectTrue()
        {
            //--------------------------Arrange-----------------------------
            //--------------------------Act---------------------------------
            var dataNode = new DataNode
            {
                IsDeleted = true,
                IsFile = true,
                IsFolder = true,
                PathLower = @"C:\test\path.txt"
            };
            //--------------------------Assert------------------------------
            Assert.IsTrue(dataNode.IsDeleted);
            Assert.IsTrue(dataNode.IsFile);
            Assert.IsTrue(dataNode.IsFolder);
            Assert.AreEqual(@"C:\test\path.txt", dataNode.PathLower);
        }
    }
}
