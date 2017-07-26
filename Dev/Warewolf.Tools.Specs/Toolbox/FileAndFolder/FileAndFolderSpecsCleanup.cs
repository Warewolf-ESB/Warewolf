using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [AfterScenario("FileAndFolderCopy")]
        [AfterScenario("FileAndFolderCreate")]
        [AfterScenario("FileAndFolderDelete")]
        [AfterScenario("FileAndFolderMove")]
        [AfterScenario("ReadFile")]
        [AfterScenario("ReadFolder")]
        [AfterScenario("FileAndFolderRename")]
        [AfterScenario("Unzip")]
        [AfterScenario("WriteFile")]
        [AfterScenario("Zip")]
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
