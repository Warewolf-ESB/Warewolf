using System;

namespace Warewolf.Data
{
    public class ServerFollower
    {
        public string HostName { get; set; }
        public DateTime ConnectedSince { get; set; }
        public DateTime LastSync { get; set; }
    }
}
