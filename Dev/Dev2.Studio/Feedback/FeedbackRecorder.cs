//This class uses psr.exe, the ussage of which is detailed below:
// ussage of psr.exe
// psr.exe [/start |/stop][/output <fullfilepath>] [/sc (0|1)] [/maxsc <value>]
//     [/sketch (0|1)] [/slides (0|1)] [/gui (o|1)]
//     [/arcetl (0|1)] [/arcxml (0|1)] [/arcmht (0|1)]
//     [/stopevent <eventname>] [/maxlogsize <value>] [/recordpid <pid>]

// /start            :Start Recording. (Outputpath flag SHOULD be specified)
// /stop            :Stop Recording.
// /sc            :Capture screenshots for recorded steps.
// /maxsc            :Maximum number of recent screen captures.
// /maxlogsize        :Maximum log file size (in MB) before wrapping occurs.
// /gui            :Display control GUI.
// /arcetl            :Include raw ETW file in archive output.
// /arcxml            :Include MHT file in archive output.
// /recordpid        :Record all actions associated with given PID.
// /sketch            :Sketch UI if no screenshot was saved. ********This parameter seems to cause an error
// /slides            :Create slide show HTML pages.
// /output            :Store output of record session in given path.
// /stopevent        :Event to signal after output files are generated.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Studio.AppResources.Exceptions;
using Dev2.Studio.Controller;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Feedback
{
    public class FeedbackRecorder : IFeedBackRecorder
    {
        #region Class Members

        private static DateTime _lastRecordingStartDateTimeStamp;

        private const string Executable = "psr.exe";
        private const string StartParameters = "/start /gui 0 /output \"{0}\"";
        private const string StopParameters = "/stop";
        private readonly Func<string, bool> _fileExistsFunction;
        private IList<ProcessController> _runningProcesses;

        #endregion Class Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRecorder" /> class.
        /// </summary>
        public FeedbackRecorder() :
            this(File.Exists)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackRecorder" /> class.
        /// </summary>
        /// <param name="fileExistsFunction">The file exists function.</param>
        /// <exception cref="System.ArgumentNullException">fileExistsFunction</exception>
        public FeedbackRecorder(Func<string, bool> fileExistsFunction)
        {
            if(fileExistsFunction == null)
            {
                throw new ArgumentNullException("fileExistsFunction");
            }

            _fileExistsFunction = fileExistsFunction;
        }

        #endregion Constructor

        #region Properties
        public IList<ProcessController> RunningProcesses
        {
            get { return _runningProcesses ?? (_runningProcesses = new List<ProcessController>()); }
        }

        /// <summary>
        /// Gets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        public string OutputPath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the recorder is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is recorder available; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecorderAvailable
        {
            get
            {
                return
                    _fileExistsFunction(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                                                     Executable)) ||
                    _fileExistsFunction(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                                                     Executable));
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Starts the recording.
        /// </summary>
        /// <param name="outputpath">The output path for the recording.</param>
        /// <exception cref="System.InvalidOperationException">Can't start the feedback recorder because one is already running.</exception>
        public void StartRecording(string outputpath)
        {
            if(outputpath == null)
            {
                throw new ArgumentNullException("outputpath");
            }

            if(CheckIfProcessIsRunning())
            {
                throw new FeedbackRecordingInprogressException("Can't start the feedback recorder because one is already running.");
            }

            OutputPath = outputpath;
            EnsurePathIsvalid();
            StartProcess();

            if(!CheckIfProcessIsRunning()) //2013.02.06: Ashley Lewis - Bug 8611
            {
                throw new FeedbackRecordingProcessFailedToStartException("Feedback recorder is unable to start at this time.");
            }
        }

        /// <summary>
        /// Stops the recording.
        /// </summary>
        public void StopRecording()
        {
            if(!CheckIfProcessIsRunning())
            {
                return;
            }

            StopProcess();
        }

        /// <summary>
        /// Kills all recording tasks.
        /// </summary>
        public void KillAllRecordingTasks()
        {
            StopProcess();
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Starts the recording process.
        /// </summary>
        private void StartProcess()
        {
            var processController = new ProcessController
            {
                Arguments = string.Format(StartParameters, OutputPath),
                CmdLine = Executable,
                // Verb = "runas",
                UseShellExecute = true
            };

            processController.Start();
            _lastRecordingStartDateTimeStamp = DateTime.Now;
            RunningProcesses.Add(processController);
        }

        /// <summary>
        /// Stops the recording process.
        /// </summary>
        /// <exception cref="System.Exception">No processes exit to stop.</exception>
        private void StopProcess()
        {
            //2013.08.26: Ashley Lewis for bug 10196: This process stops the recording using stop params TODO refactor to ProcessController
            var startInfo = new ProcessStartInfo(Executable, StopParameters)
            {
                UseShellExecute = true,
                ErrorDialog = false,

            };

            var stopRecordingProcess = new Process { StartInfo = startInfo };

            //
            // This check is to prevent stop from being called to soon after pse.exe has been started.
            // Otherwise the /stop paramater on the stop call is ignored and the process which was originally
            // started just continues to run until manually closed.
            // It isn't very pretty but psr.exe doesn't provide a mechanism to check if it has finished initializing, 
            // and there is no standard in .net to tell if a process that has been started is 'ready'.
            //
            if((DateTime.Now - _lastRecordingStartDateTimeStamp).TotalMilliseconds < 500)
            {
                Thread.Sleep(500);
            }

            try
            {
                stopRecordingProcess.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(@"Start Info : " + startInfo);
                Console.WriteLine(@"Stacktrace : " + e.StackTrace);
                throw;
            }

            RunningProcesses.Add(new ProcessController(stopRecordingProcess));

            WaitForProcessesToEnd(RunningProcesses.ToArray());

            RunningProcesses.Clear();
        }

        /// <summary>
        /// Ensures the path isvalid.
        /// </summary>
        /// <exception cref="System.IO.IOException">File specified in the output path already exists.</exception>
        private void EnsurePathIsvalid()
        {
            FileInfo path = new FileInfo(OutputPath);

            string extension = Path.GetExtension(OutputPath);
            if(string.Compare(extension, ".zip", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(extension, ".xml", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new InvalidOperationException("The output path for a recording can only be to a 'xml' or 'zip' file.");
            }

            if(path.Exists)
            {
                throw new IOException("File specified in the output path already exists.");
            }

            if(path.Directory == null)
            {
                throw new IOException("Output path is invalid.");
            }

            if(!path.Directory.Exists)
            {
                path.Directory.Create();
            }
        }

        /// <summary>
        /// Checks if process is running.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfProcessIsRunning()
        {
            return (RunningProcesses.Count > 0);
        }

        /// <summary>
        /// Waits for processes to end.
        /// </summary>
        /// <param name="processes">The processes.</param>
        /// <exception cref="System.Exception">Stop recording timed out. This occurs when the recorder requests the 'Problem Step Recorder' to stop but it doesn't exit with in 10 seconds. This is usually caused by calling 'StopRecording()' to soon after 'StartRecording()'</exception>
        private void WaitForProcessesToEnd(ProcessController[] processes)
        {
            foreach(ProcessController process in processes)
            {
                if(!process.UtilityProcess.HasExited)
                {
                    process.UtilityProcess.WaitForExit(10000);
                    if(!process.UtilityProcess.HasExited)
                    {
                        try
                        {
                            process.Kill("psr");
                        }
                        catch
                        {
                            throw new FeedbackRecordingTimeoutException(
                                "There was a problem stopping the recording and generating the recording file. Make sure the disk is not full and that you have access to this user's Windows temperary storage file.");
                        }
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
