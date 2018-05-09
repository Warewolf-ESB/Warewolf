using System;
using Dev2.Data.Decision;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2ActivityIOPathUtilsTests
    {
        [TestMethod]
        public void ExtractFullDirectoryPath_Given_Directory()
        {
            const string resourcesPath = @"C:\ProgramData\Warewolf\Resources";
            var fullDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(resourcesPath);
            Assert.AreEqual(resourcesPath, fullDir);
        }

        [TestMethod]
        public void ExtractFullDirectoryPath_Given_FilePath()
        {
            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.log";
            const string containingFolder = @"C:\ProgramData\Warewolf\Server Log\";
            var results = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(serverLogFile);
            Assert.AreEqual(containingFolder, results);
        }
        [TestMethod]
        public void IsStarWildCard_Given_Star_In_Path_Returns_True()
        {
            const string resourcesPath = @"C:\ProgramData\Warewolf\*.*";
            var results = Dev2ActivityIOPathUtils.IsStarWildCard(resourcesPath);
            Assert.IsTrue(results);
        }
        [TestMethod]
        public void IsDirectory_Given_Drive_Returns_True()
        {
            const string resourcesPath = @"C:\\";
            var results = Dev2ActivityIOPathUtils.IsDirectory(resourcesPath);
            Assert.IsTrue(results);
        }
    }
}
