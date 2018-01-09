using System;
using System.Collections.Generic;

namespace WarewolfCOMIPC.Client
{
    [Serializable]
    public class CallData
    {
        public CallData()
        {
            Status = KeepAliveStatus.KeepAlive;
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
        public ParameterInfoTO[] Parameters { get; set; }

        /// <summary>
        /// Status indicating if the wrapper executable should close the connection and terminate itself
        /// </summary>
        public KeepAliveStatus Status { get; set; }
        public Execute Execute { get; set; }
        public string ExecuteType { get; set; }
    }

    [Serializable]
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
    
    [Serializable]
    public class MethodInfoTO
    {
        
        public MethodInfoTO()
        {
            
        }
        public string Name { get; set; }
        public List<ParameterInfoTO> Parameters {get;set;}
    }
    [Serializable]
    public class ParameterInfoTO
    {

        
        public ParameterInfoTO()
        {
            
        }
        public object DefaultValue { get; set; }        
        public bool IsRequired { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
    }


}