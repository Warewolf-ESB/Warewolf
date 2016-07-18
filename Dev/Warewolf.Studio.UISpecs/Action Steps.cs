using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    class Action_Steps
    {
        [Given(@"I ""(.*)""")]
        [When(@"I ""(.*)""")]
        [Then(@"I ""(.*)""")]
        public void TheRecordedActionIsPerformed(string p0)
        {
            MethodInfo getActionRecording = typeof(UIMap).GetMethod(p0);
            if (getActionRecording != null)
            {
                getActionRecording.Invoke(this, new object[] { });
            }
            else
            {
                throw new InvalidOperationException("Cannot find action recording " + p0);
            }
        }

        [BeforeScenario]
        public static void ScenarioInit()
        {
            var uiMap = new UIMap();
            uiMap.SetGlobalPlaybackSettings();
            uiMap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + ScenarioContext.Current.ScenarioInfo.Title + "\" starting on " + System.Environment.MachineName);
        }

        [BeforeScenario]
        [Scope(Tag = "NeedsBlankWorkflow")]
        public static void InitializeABlankWorkflow()
        {
            var map = new UIMap();
            map.Assert_NewWorkFlow_RibbonButton_Exists();
            map.Click_New_Workflow_Ribbon_Button();
            map.Assert_StartNode_Exists();
        }

        [AfterScenario]
        [Scope(Tag = "NeedsBlankWorkflow")]
        public static void CleanupWorkflow()
        {
            try
            {
                var map = new UIMap();
                map.Assert_Close_Tab_Button_Exists();
                map.Click_Close_Tab_Button();
                map.Click_MessageBox_No();
            }
            catch (UITestControlNotFoundException e)
            {
                //Test may have crashed before tab is even openned
            }
        }
    }
}
