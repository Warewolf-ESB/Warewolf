using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class DirectoryEntryFactoryTest
    {
        public DirectoryEntryFactoryTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("DirectoryEntryTest_Create")]
        public void DirectoryEntryFactoryTest_EntryNameAndMachineName_AreEqual()
        {
            //-----------------Arrage------------------
            var path = "WinNT://" + Environment.MachineName + ",computer";
            IDirectoryEntryFactory _directoryEntryFactory = new DirectoryEntryFactory();
            //-----------------Act------------------
            var entry = _directoryEntryFactory.Create(path);
            //-----------------Assert------------------
            Assert.IsNotNull(entry);
            Assert.AreEqual(Environment.MachineName, entry.Name);
        }

    }
}
