using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Infrastructure.Tests
{
    [TestClass]
    public class TestEnvironmentVariables
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetServerSettingsFolder")]
        public void EnvironmentVariables_GetServerSettingsFolder_ShouldReturnProgramDataFolder()
        {
            //------------Setup for test--------------------------
            const string serverSettingsFolderPart = "ProgramData\\Warewolf\\Server Settings";
            //------------Execute Test---------------------------
            var serverSettingsFolder = EnvironmentVariables.ServerSettingsFolder;
            //------------Assert Results-------------------------
            StringAssert.Contains(serverSettingsFolder,serverSettingsFolderPart);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetServerSecurityFile")]
        public void EnvironmentVariables_GetServerSecurityFile_ShouldReturnSecuritySettingsFileInProgramData()
        {
            //------------Setup for test--------------------------
            const string filePart = "ProgramData\\Warewolf\\Server Settings\\Settings.config";
            //------------Execute Test---------------------------
            var filePath = EnvironmentVariables.ServerLogSettingsFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(filePath, filePart);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetServerLogSettingsFile")]
        public void EnvironmentVariables_GetServerLogSettingsFile_ShouldReturnLogSettingsFileInProgramData()
        {
            //------------Setup for test--------------------------
            const string serverSecurityFilePart = "ProgramData\\Warewolf\\Server Settings\\secure.config";
            //------------Execute Test---------------------------
            var serverSecurityFilePath = EnvironmentVariables.ServerSecuritySettingsFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(serverSecurityFilePath, serverSecurityFilePart);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetServerLogFile")]
        public void EnvironmentVariables_GetServerLogFile_ShouldReturnLogFileInProgramData()
        {
            //------------Setup for test--------------------------
            const string filePart = "ProgramData\\Warewolf\\Server Log\\warewolf-Server.log";
            //------------Execute Test---------------------------
            var filePath = EnvironmentVariables.ServerLogFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(filePath, filePart);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetResourcePath")]
        public void EnvironmentVariables_GetResourcePath_ShouldReturnResourceFolderInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Resources";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.ResourcePath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetTestPath")]
        public void EnvironmentVariables_GetTestPath_ShouldReturnTestFolderInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Tests";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.TestPath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentVariables_GetWorkspacePath")]
        public void EnvironmentVariables_GetWorkspacePath_ShouldReturnWorkspaceFolderInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Workspaces";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.WorkspacePath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }
    }
}
