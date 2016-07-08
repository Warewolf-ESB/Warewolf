using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class ActionSteps
    {
        [Given(@"I ""(.*)""")]
        [When(@"I ""(.*)""")]
        [Then(@"I ""(.*)""")]
        public void TheRecordedActionIsPerformed(string p0)
        {
            MethodInfo getActionRecording = typeof(UIMap).GetMethod(p0);
            if (getActionRecording != null)
            {
                getActionRecording.Invoke(uiMap, new object[] { });
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
            // Configure the playback engine
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.MaximumRetryCount = 10;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
            Playback.PlaybackSettings.DelayBetweenActions = 500;
            Playback.PlaybackSettings.SearchTimeout = 1000;

            // Add the error handler
            Playback.PlaybackError -= Playback_PlaybackError; // Remove the handler if it's already added
            Playback.PlaybackError += Playback_PlaybackError; // Ta dah...

            var sleepTimer = 5;
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

        /// <summary> Test startup. </summary>
        public static void StartTest()
        {
            // See: http://stackoverflow.com/questions/20367855/coded-ui-test-is-slow-waiting-for-ui-thread
            // Configure the playback engine
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.Disabled;
            Playback.PlaybackSettings.MaximumRetryCount = 10;
            Playback.PlaybackSettings.ShouldSearchFailFast = false;
            Playback.PlaybackSettings.DelayBetweenActions = 1000;
            Playback.PlaybackSettings.SearchTimeout = 1000;

            // Add the error handler
            Playback.PlaybackError -= Playback_PlaybackError; // Remove the handler if it's already added
            Playback.PlaybackError += Playback_PlaybackError; // Ta dah...
        }

        /// <summary> PlaybackError event handler. </summary>
        private static void Playback_PlaybackError(object sender, PlaybackErrorEventArgs e)
        {
            // Wait a second
            System.Threading.Thread.Sleep(1000);

            // Retry the failed test operation
            e.Result = PlaybackErrorOptions.Retry;
        }

        [BeforeScenario]
        public static void LogComputerName()
        {
            Console.WriteLine("Test \"" +ScenarioContext.Current.ScenarioInfo.Title + "\" starting on " + System.Environment.MachineName);
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

        #region Properties and Fields

        UIMap uiMap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
