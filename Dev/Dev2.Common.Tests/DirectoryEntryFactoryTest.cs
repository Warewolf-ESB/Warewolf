/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class DirectoryEntryFactoryTest
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DirectoryEntryFactory))]
        public void DirectoryEntryFactory_EntryNameAndMachineName_AreEqual()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Assert.Inconclusive("Test uses Windows-specific directory entries");
			//-----------------Arrage------------------
			var path = "WinNT://" + Environment.MachineName + ",computer";
            IDirectoryEntryFactory _directoryEntryFactory = new DirectoryEntryFactory();
            //-----------------Act------------------
            var entry = _directoryEntryFactory.Create(path);
            //-----------------Assert------------------
            Assert.IsNotNull(entry);
            Assert.AreEqual(Environment.MachineName, entry.Name);

            var count = 0;
            foreach (var child in entry.Children)
            {
                count++;
            }
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DirectoryEntryFactory))]
        public void DirectoryEntryFactory_DirectoryEntry_IsDisposed()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Assert.Inconclusive("Test uses Windows-specific directory entries");
			//-----------------Arrage------------------
			var path = "WinNT://" + Environment.MachineName + ",computer";
            IDirectoryEntryFactory _directoryEntryFactory = new DirectoryEntryFactory();
            //-----------------Act------------------
            var entry = _directoryEntryFactory.Create(path);
            //-----------------Assert------------------
            Assert.IsNotNull(entry);
            Assert.AreEqual(Environment.MachineName, entry.Name);
            //----------------Test if it does dispose-----------
            //----------------Act-------------------------------
            entry.Dispose();
            //----------------Assert-------------------------------
            Assert.ThrowsException<ObjectDisposedException>(()=>entry.Name);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DirectoryEntryFactory))]
        public void DirectoryEntryFactory_DirectoryEntryPath_IsTrue()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				Assert.Inconclusive("Test uses Windows-specific directory entries");
			//-----------------Arrage------------------
			IDirectoryEntryFactory _directoryEntryFactory = new DirectoryEntryFactory();
            //-----------------Act------------------
            var entry = _directoryEntryFactory.Create("Administrator");
            //-----------------Assert------------------
            Assert.IsNotNull(entry);
#if WINDOWS || NETFRAMEWORK
            Assert.IsTrue(entry.Instance.Path == "Administrator");
#endif
		}
	}
}