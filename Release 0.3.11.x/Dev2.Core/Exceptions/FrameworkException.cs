using System;
using Unlimited.Framework;

namespace Dev2 {
    public class FrameworkException : Exception {

        public string ExceptionData { get; set; }
        public FrameworkException(string message, string source, string exceptionData, Exception innerException) : base(message, innerException){
            this.Source = source;
            this.ExceptionData = exceptionData;

            dynamic data = new UnlimitedObject();
            data.Message = message;
            data.ExceptionData = exceptionData;

            if (innerException != null) {
                dynamic innerEx = new UnlimitedObject(innerException);
                data.InnerExceptionData = innerEx.XmlString;
            }

            ExceptionHandling.WriteEventLogEntry("Application", source, data.XmlString, System.Diagnostics.EventLogEntryType.Error);
        }


        
    }
}