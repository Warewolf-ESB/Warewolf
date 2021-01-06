using System;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.Tools.Specs.Toolbox.FileAndFolder
{
    [Binding]
    class FileAndFolderSpecsCleanup : FileToolsBase
    {
        public FileAndFolderSpecsCleanup(ScenarioContext scenarioContext) : base(scenarioContext)
        {
        }

        [AfterScenario("@FileAndFolderCopy",
            "@FileAndFolderCreate",
            "@FileAndFolderDelete",
            "@FileAndFolderMove",
            "@ReadFile",
            "@ReadFolder",
            "@FileAndFolderRename",
            "@Unzip",
            "@WriteFile",
            "@Zip",
            "@CopyFileFromLocal",
            "@CopyFileFromFTP",
            "@CopyFileFromFTPS",
            "@CopyFileFromSFTP",
            "@FileMoveFromLocal",
            "@FileMoveFromFTP",
            "@FileMoveFromFTPS",
            "@FileMoveFromSFTP",
            "@FileRenameFromLocal",
            "@FileRenameFromFTP",
            "@FileRenameFromFTPS",
            "@FileRenameFromSFTP",
            "@UnzipFromLocal",
            "@UnzipFromFTP",
            "@UnzipFromFTPS",
            "@UnzipFromSFTP",
            "@ZipFromLocal",
            "@ZipFromFTP",
            "@ZipFromFTPS",
            "@ZipFromSFTP")]
        public void CleanUpFiles()
        {
            try
            {
                RemovedFilesCreatedForTesting();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);

            }
        }
    }
}
