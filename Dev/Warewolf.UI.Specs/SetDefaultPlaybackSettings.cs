using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests;

namespace Warewolf.UISpecs
{
    [Binding]
    [DeploymentItem("Warewolf.UI.Tests.dll")]
    class SetDefaultPlaybackSettings
    {
        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            UIMap.SetPlaybackSettings();
        }

        [BeforeScenario("Explorer")]
        public void EnsureResourceDoesNotExistOnRemote()
        {
            var xmlResourcePath = @"\\tst-ci-remote.dev2.local\C$\ProgramData\Warewolf\Resources\LocalWorkflowWithRemoteSubworkflowToDelete.xml";
            if (File.Exists(xmlResourcePath))
            {
                File.Delete(xmlResourcePath);
            }
            var biteResourcePath = @"\\tst-ci-remote.dev2.local\C$\ProgramData\Warewolf\Resources\LocalWorkflowWithRemoteSubworkflowToDelete.bite";
            if (File.Exists(biteResourcePath))
            {
                File.Delete(biteResourcePath);
            }
        }
    }
}
