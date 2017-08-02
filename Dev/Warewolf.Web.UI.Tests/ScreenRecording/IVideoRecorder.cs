namespace AutoTestSharedTools.VideoRecorder
{
    public interface IVideoRecorder
    {
        bool StartRecord(string outputFilePath);
        void StopRecord();
    }
}
