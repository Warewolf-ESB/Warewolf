using System;

namespace WarewolfCOMIPC.Client
{
    [Serializable]
    public class CallData
    {
        public CallData()
        {
            Status = KeepAliveStatus.KeepAlive;
            Execute = Execute.ExecuteSpecifiedMethod;
        }

        /// <summary>
        /// Name of the library to call.
        /// </summary>
        public Guid CLSID { get; set; }

        /// <summary>
        /// Name of the procedure to call.
        /// </summary>
        public string MethodToCall { get; set; }

        /// <summary>
        /// Array of parameters to pass to the function call.
        /// </summary>
        public object[] Parameters { get; set; }
        
        /// <summary>
        /// Status indicating if the wrapper executable should close the connection and terminate itself
        /// </summary>
        public KeepAliveStatus Status { get; set; }
        public Execute Execute { get; set; }
        public string ExecuteType { get; set; }
    }

    public enum Execute
    {
        GetMethods,
        GetNamespaces,
        GetType,
        ExecuteSpecifiedMethod
    }

    public enum KeepAliveStatus
    {
        KeepAlive,
        Close
    }
}