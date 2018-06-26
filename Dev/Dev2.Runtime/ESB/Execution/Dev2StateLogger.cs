using System;
using System.IO;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Common;
using Dev2.Common.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using System.Security.Principal;
using Dev2.Communication;
using Dev2.Common.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Runtime.ESB.Execution
{
    class Dev2StateLogger : IDev2StateLogger
    {
        readonly StreamWriter writer;
        readonly JsonTextWriter jsonTextWriter;
        readonly IDSFDataObject dsfDataObject;

        public Dev2StateLogger(IDSFDataObject dsfDataObject)
            : this(dsfDataObject, new FileWrapper())
        {
        }

        public Dev2StateLogger(IDSFDataObject dsfDataObject, IFile fileWrapper)
        {
            writer = fileWrapper.AppendText(GetDetailLogFilePath(dsfDataObject));
            jsonTextWriter = new JsonTextWriter(writer);
            this.dsfDataObject = dsfDataObject;
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            writer.WriteLine("header:LogPreExecuteState");
            WriteHeader(null, nextActivity);
            dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            writer.WriteLine("header:LogPostExecuteState");
            WriteHeader(previousActivity, nextActivity);
            dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            writer.WriteLine("header:LogExecuteException");
            WriteHeader(activity, e);
            dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogExecuteCompleteState()
        {
            writer.WriteLine("header:LogExecuteCompleteState");
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            jsonTextWriter.WriteEndObject();
            writer.WriteLine();
            writer.Flush();
            dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogStopExecutionState()
        {
            writer.WriteLine("header:LogStopExecutionState");
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            jsonTextWriter.WriteEndObject();
            writer.WriteLine();
            writer.Flush();
            dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        private void WriteHeader(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            if (!(previousActivity is null))
            {
                jsonTextWriter.WritePropertyName("PreviousActivity");
                jsonTextWriter.WriteValue(previousActivity.UniqueID);
            }
            if (!(nextActivity is null))
            {
                jsonTextWriter.WritePropertyName("NextActivity");

                jsonTextWriter.WriteStartObject();
                jsonTextWriter.WritePropertyName("Id");
                jsonTextWriter.WriteValue(nextActivity.UniqueID);
                jsonTextWriter.WritePropertyName("Type");
                jsonTextWriter.WriteValue(nextActivity.GetType().ToString());
                jsonTextWriter.WritePropertyName("DisplayName");
                jsonTextWriter.WriteValue(nextActivity.GetDisplayName());
                if (nextActivity is DsfActivity dsfActivity)
                {
                    jsonTextWriter.WritePropertyName("Inputs");
                    var jsonSerializer = new Dev2JsonSerializer();
                    jsonTextWriter.WriteRawValue(jsonSerializer.Serialize(dsfActivity.Inputs, Formatting.None));
                    jsonTextWriter.WritePropertyName("Outputs");
                    jsonTextWriter.WriteRawValue(jsonSerializer.Serialize(dsfActivity.Outputs, Formatting.None));
                }
                jsonTextWriter.WriteEndObject();
            }
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        private void WriteHeader(IDev2Activity activity, Exception exception)
        {
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            jsonTextWriter.WritePropertyName("PreviousActivity");
            jsonTextWriter.WriteValue(activity.UniqueID);
            jsonTextWriter.WritePropertyName("Exception");
            jsonTextWriter.WriteValue(exception.Message);
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public static string GetDetailLogFilePath(IDSFDataObject dsfDataObject) =>
            Path.Combine(EnvironmentVariables.WorkflowDetailLogPath(dsfDataObject.ResourceID, dsfDataObject.ServiceName), "Detail.log");

        public void Close()
        {
            jsonTextWriter.Close();
        }

        public void Dispose()
        {
            ((IDisposable)jsonTextWriter).Dispose();
        }
    }


    static class DsfDataObjectMethods
    {
        public static void LogState(this IDSFDataObject dsfDataObject, JsonTextWriter jsonTextWriter)
        {
            jsonTextWriter.WriteRaw("{\"DsfDataObject\":");
            jsonTextWriter.WriteStartObject();

            jsonTextWriter.WritePropertyName("ServerID");
            jsonTextWriter.WriteValue(dsfDataObject.ServerID);

            jsonTextWriter.WritePropertyName("ParentID");
            jsonTextWriter.WriteValue(dsfDataObject.ParentID);

            jsonTextWriter.WritePropertyName("ClientID");
            jsonTextWriter.WriteValue(dsfDataObject.ClientID);

            jsonTextWriter.WritePropertyName("ExecutingUser");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutingUser.Identity.ToJson());

            jsonTextWriter.WritePropertyName("ExecutionID");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionID);

            jsonTextWriter.WritePropertyName("ExecutionOrigin");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionOrigin);

            jsonTextWriter.WritePropertyName("ExecutionOriginDescription");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionOriginDescription);

            jsonTextWriter.WritePropertyName("ExecutionToken");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionToken.ToJson());

            jsonTextWriter.WritePropertyName("IsSubExecution");
            jsonTextWriter.WriteValue(dsfDataObject.IsSubExecution);

            jsonTextWriter.WritePropertyName("IsRemoteWorkflow");
            jsonTextWriter.WriteValue(dsfDataObject.IsRemoteWorkflow());

            jsonTextWriter.WritePropertyName("Environment");
            jsonTextWriter.WriteRawValue(dsfDataObject.Environment.ToJson());

            jsonTextWriter.WriteEndObject();
            jsonTextWriter.WriteRaw("}");
        }
    }

    static class IIdentityExtensionMethods
    {
        public static string ToJson(this IIdentity identity)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(identity, Formatting.None);
        }
    }

    static class ExecutionTokenExtensionMethods
    {
        public static string ToJson(this IExecutionToken executionToken)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(executionToken, Formatting.None);
        }
    }
}
