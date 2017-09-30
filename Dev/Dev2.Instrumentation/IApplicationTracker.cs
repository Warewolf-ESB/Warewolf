using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Instrumentation
{

    // Interface 
    public interface IApplicationTracker

    {
        // Enable application Tracking in the application
        void EnableAppplicationTracker(string productVersion, string username);

        //Track the custom event in the application
        void TrackApplicationEvent(string actions);

        //Track the custom events in the application
        void TrackCustomEvent(string category, string eventName, string actions);
        
        //Disable  appplication tracking in the studio 
        void DisableAppplicationTracker();

    }
}
