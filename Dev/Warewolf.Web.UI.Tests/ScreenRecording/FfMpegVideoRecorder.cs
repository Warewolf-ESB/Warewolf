namespace AutoTestSharedTools.VideoRecorder
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
            if (!this.stopped)
            {
                this.StopRecording();
            }
        }

        public bool StartRecording(TestContext TestContext)
        {
            if (this.isRunning)
            {
                return false;
            }

            this.stopped = false;
            this.startDateTime = DateTime.Now;
            
            this.filename = Path.Combine(TestContext.DeploymentDirectory, TestContext.TestName + "_on_" + Environment.MachineName) + "." + VideoExtention;

            if (File.Exists(this.filename))
            {
                File.Delete(this.filename);
            }

            this.recordingTask = Task.Factory.StartNew(this.Record);
            return true;
        }

        public void StopRecording(TestContext TestContext=null)
        {
            this.stopped = true;
            this.recordingTask.Wait();
            if (TestContext == null || TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            {
                File.Delete(this.filename);
            }
            else
            {
                TestContext.AddResultFile(this.filename);
            }
        }

        private void Record()
        {
            try
            {
                this.isRunning = true;

                var writer = new VideoFileWriter();
                writer.Open(
                    this.filename,
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

                var sw = new StreamWriter(Console.OpenStandardError());
                Console.SetOut(sw);
                while (!this.stopped || (DateTime.Now - this.startDateTime < TimeSpan.FromMilliseconds(100)))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                    writer.WriteVideoFrame(bitmap);
                    Thread.Sleep(10);
                }
                sw = new StreamWriter(Console.OpenStandardOutput());
                sw.AutoFlush = true;
                Console.SetOut(sw);

                writer.Close();
                writer.Dispose();
            }
            finally
            {
                this.isRunning = false;
            }
        }
    }
}
