using RUISDK_5_3_1;

namespace Dev2.Instrumentation
{
    public interface IApplicationTracker
    {
        // Enable application Tracking in the application
        void EnableApplicationTracker(string productVersion, string username);

        //Track the event in the application
        void TrackEvent(string category, string eventName);

        //Track the custom events in the application
        void TrackCustomEvent(string category, string eventName, string customValues);

        //Disable  appplication tracking in the studio 
        void DisableApplicationTracker();

        RUIResult EnableApplicationResultStatus { get; set; }
    }
}