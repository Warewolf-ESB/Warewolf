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
using TechTalk.SpecFlow;
using Warewolf.Studio.UISpecs.OutsideWorkflowDesignSurfaceUIMapClasses;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class ActionSteps
    {
        [Given(@"I '(.*)'")]
        [When(@"I '(.*)'")]
        [Then(@"I '(.*)'")]
        public void TheRecordedActionIsPerformed(string p0)
        {
            Type workflowDesignerMapType = Uimap.GetType();
            Type outsideWorkflowDesignerMapType = OutsideWorkflowDesignSurfaceUiMap.GetType();
            MethodInfo workflowDesignerAction = workflowDesignerMapType.GetMethod(p0);
            MethodInfo outsideWorkflowDesignerAction = outsideWorkflowDesignerMapType.GetMethod(p0);
            if (workflowDesignerAction != null && outsideWorkflowDesignerAction != null)
            {
                throw new InvalidOperationException("Cannot distinguish between duplicated action recordings, both named '" + p0 + "' in different UI maps.");
            }
            else
            {
                if (outsideWorkflowDesignerAction != null)
                {
                    outsideWorkflowDesignerAction.Invoke(OutsideWorkflowDesignSurfaceUiMap, new object[] { });
                }
                if (workflowDesignerAction != null)
                {
                    workflowDesignerAction.Invoke(Uimap, new object[] { });
                }
            }
        }

        [BeforeTestRun]
        public static void WaitForStudioStart()
        {
            Playback.Initialize();
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

        [BeforeScenario]
        public static void LogComputerName()
        {
            Console.WriteLine("Test \"" +ScenarioContext.Current.ScenarioInfo.Title + "\" starting on " + System.Environment.MachineName);
        }

        [BeforeScenario]
        [Scope(Tag = "NeedsBlankWorkflow")]
        public static void InitializeABlankWorkflow()
        {
            var outsideWorkflowDesignSurfaceUiMap = new OutsideWorkflowDesignSurfaceUIMap();
            outsideWorkflowDesignSurfaceUiMap.Assert_NewWorkFlow_RibbonButton_Exists();
            outsideWorkflowDesignSurfaceUiMap.Click_New_Workflow_Ribbon_Button();
            new UIMap().Assert_StartNode_Exists();
        }

        [AfterScenario]
        [Scope(Tag = "NeedsBlankWorkflow")]
        public static void CleanupWorkflow()
        {
            var uiMap = new UIMap();
            uiMap.Assert_Close_Tab_Button_Exists();
            uiMap.Click_Close_Tab_Button();
            try
            {
                new OutsideWorkflowDesignSurfaceUIMap().Click_MessageBox_No();
            }
            catch (UITestControlNotFoundException e)
            {
                //Might not prompt for save if no changes made.
            }
        }

        #region Properties and Fields

        UIMap Uimap
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

        OutsideWorkflowDesignSurfaceUIMap OutsideWorkflowDesignSurfaceUiMap
        {
            get
            {
                if ((_outsideWorkflowDesignSurfaceUiMap == null))
                {
                    _outsideWorkflowDesignSurfaceUiMap = new OutsideWorkflowDesignSurfaceUIMap();
                }

                return _outsideWorkflowDesignSurfaceUiMap;
            }
        }

        private OutsideWorkflowDesignSurfaceUIMap _outsideWorkflowDesignSurfaceUiMap;

        #endregion
    }
}
