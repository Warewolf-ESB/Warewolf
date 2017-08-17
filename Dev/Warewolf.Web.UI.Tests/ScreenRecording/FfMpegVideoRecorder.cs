namespace Warewolf.Web.UI.Tests.ScreenRecording
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Accord.Video.FFMPEG;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class FfMpegVideoRecorder : IVideoRecorder, IDisposable
    {
        public const string VideoExtention = "avi";

        private string filename;
        private bool isRunning;
        private Task recordingTask;
        private DateTime startDateTime;
        private bool stopped;

        public void Dispose()
        {
            if (!stopped)
            {
                StopRecording(UnitTestOutcome.Error);
            }
        }

        public bool StartRecording(TestContext TestContext)
        {
            if (isRunning)
            {
                return false;
            }

            stopped = false;
            startDateTime = DateTime.Now;
            
            filename = Path.Combine(TestContext.DeploymentDirectory, TestContext.TestName + "_on_" + TestContext.DataRow[0].ToString() + "_on_" + Environment.MachineName) + "." + VideoExtention;

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            recordingTask = Task.Factory.StartNew(Record);
            return true;
        }

        public void StopRecording(UnitTestOutcome TestOutcome)
        {
            stopped = true;
            recordingTask.Wait();
            if (TestOutcome == UnitTestOutcome.Passed)
            {
                File.Delete(filename);
            }
            else
            {
                if (!File.Exists(filename))
                {
                    string TestResultsRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(filename).ToString()).ToString()).ToString();
                    File.Move(filename, Path.Combine(TestResultsRootPath, Path.GetFileName(filename)));
                }
            }
        }

        private void Record()
        {
            try
            {
                isRunning = true;

                var writer = new VideoFileWriter();
                writer.Open(
                    filename,
                    Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height,
                    10,
                    VideoCodec.MPEG4,
                    2304000);
                var bitmap = new Bitmap(
                    Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height,
                    PixelFormat.Format24bppRgb);

                var graphics = Graphics.FromImage(bitmap);
                
                while (!stopped || (DateTime.Now - startDateTime < TimeSpan.FromMilliseconds(100)))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                    writer.WriteVideoFrame(bitmap);
                    Thread.Sleep(10);
                }

                writer.Close();
                writer.Dispose();
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}
