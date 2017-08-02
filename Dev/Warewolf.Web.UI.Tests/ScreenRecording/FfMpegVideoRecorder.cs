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
                this.StopRecord();
            }
        }

        public bool StartRecord(string outputFilePath)
        {
            int wait = 0;
            while (this.isRunning)
            {
                Thread.Sleep(100);
                if (wait++ > 10)
                {
                    return false;
                }
            }

            this.stopped = false;
            this.startDateTime = DateTime.Now;

            outputFilePath = Path.ChangeExtension(outputFilePath, VideoExtention);
            this.filename = outputFilePath;

            if (File.Exists(this.filename))
            {
                File.Delete(this.filename);
            }

            this.recordingTask = Task.Factory.StartNew(this.Record);
            return true;
        }

        public void StopRecord()
        {
            this.stopped = true;
            this.recordingTask.Wait();
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

                while (!this.stopped || (DateTime.Now - this.startDateTime < TimeSpan.FromMilliseconds(100)))
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
                this.isRunning = false;
            }
        }
    }
}
