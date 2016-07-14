using System.Reflection;
using System.Threading;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
    using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System;
    using System.Collections.Generic;
    using System.CodeDom.Compiler;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using System.Drawing;
    using System.Windows.Input;
    using System.Text.RegularExpressions;
    
    [Binding]
    public partial class UIMap
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

        [BeforeTestRun]
        public static void WaitForStudioStart()
        {
            Playback.Initialize();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
            Playback.PlaybackSettings.SearchTimeout = 10000;

            var sleepTimer = 10;
            while (true)
            {
                try
                {
                    WpfWindow getStudioWindow = new UIMap().MainStudioWindow;
                    getStudioWindow.WaitForControlExist(100);
                    if (getStudioWindow.Exists)
                    {
                        break;
                    }
                }
                catch (UITestControlNotFoundException)
                {
                    Thread.Sleep(100);
                }
                if (sleepTimer-- <= 0)
                {
                    throw new InvalidOperationException("Warewolf studio is not running. You are expected to run \"Dev\\TestScripts\\Studio\\Startup.bat\" as an administrator and wait for it to complete before running any coded UI tests");
                }
            }
        }

        [BeforeScenario]
        public static void LogComputerName()
        {
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
