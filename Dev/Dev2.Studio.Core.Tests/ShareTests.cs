
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
using Dev2.Intellisense.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ShareTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_Constructor")]
        public void Share_Constructor_Construct_ExpectValid()
        {
            //------------Setup for test--------------------------
            var share = new Share("a","b",ShareType.Disk);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(@"\\a\b",share.ToString());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_IsFileSystem")]
        public void Share_IsFileSystem_ExpectValid()
        {
            //------------Setup for test--------------------------
            var share = new Share("a", "b", ShareType.Disk);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(share.IsFileSystem);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_IsFileSystem")]
        public void Share_IsFileSystem_IPC_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var share = new Share("a", "b", ShareType.IPC);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(share.IsFileSystem);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_IsFileSystem")]
        public void Share_IsFileSystem_Special_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var share = new Share("a", "b", ShareType.Special);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(share.IsFileSystem);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_IsFileSystem")]
        public void Share_IsFileSystem_DeviveType_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var share = new Share("a", "b", ShareType.Device);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(share.IsFileSystem);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_IsFileSystem")]
        public void Share_Constructor_IPC_ExpectCorrectType()
        {
            //------------Setup for test--------------------------
            var share = new Share("a", "IPC$", ShareType.Special);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(ShareType.Special|ShareType.IPC, share.ShareType);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Share_IsFileSystem")]
        public void Share_ToString_BlankServer_ExpectLocalName()
        {
            //------------Setup for test--------------------------
            var share = new Share("", "IPC$", ShareType.Special);


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("\\\\"+Environment.MachineName+"\\IPC$", share.ToString());
        }
    }
    
}
