using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Diagnostics;
using System.Reflection;
using TechTalk.SpecFlow;
using Warewolf.Studio.UISpecs.OutsideWorkflowDesignSurfaceUIMapClasses;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public class ActionSteps
    {
        [BeforeTestRun]
        public static void StartPlayback()
        {
            Playback.Initialize();
        }

        [AfterTestRun]
        public static void StopPlayback()
        {
            if (Playback.IsInitialized) Playback.Cleanup();
        }

        [Given(@"The '(.*)' recorded action is performed")]
        [When(@"The '(.*)' recorded action is performed")]
        [Then(@"The '(.*)' recorded action is performed")]
        public void ThenTheRecordedActionIsPerformed(string p0)
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

        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.WorkingDirectory = Environment.CurrentDirectory;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        #endregion
    }
}
