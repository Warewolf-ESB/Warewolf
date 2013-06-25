
namespace Dev2.Studio.Feedback
{
    public interface IFeedBackRecorder
    {
        bool IsRecorderAvailable { get; }
        void StartRecording(string outputpath);
        void StopRecording();
        void KillAllRecordingTasks();
    }
}
