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

        [AfterScenario("@FileAndFolderCopy", "@FileAndFolderCreate", "@FileAndFolderDelete", "@FileAndFolderMove", "@ReadFile", "@ReadFolder", "@FileAndFolderRename", "@Unzip", "@WriteFile", "@Zip")]
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
