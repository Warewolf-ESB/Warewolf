using System;
using System.IO;
using Newtonsoft.Json;

using Dev2.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    class Dev2StateLogger : IDev2StateLogger
    {
        readonly StreamWriter writer;
        readonly JsonTextWriter jsonTextWriter;

        readonly IDSFDataObject dsfDataObject;
        public Dev2StateLogger(IDSFDataObject dsfDataObject, StreamWriter output)
        {
            this.dsfDataObject = dsfDataObject;
            writer = output;
            jsonTextWriter = new JsonTextWriter(writer);
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
            jsonTextWriter.WriteValue(System.DateTime.Now);
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
            jsonTextWriter.WriteValue(System.DateTime.Now);
            if (!(previousActivity is null))
            {
                jsonTextWriter.WritePropertyName("PreviousActivity");
                jsonTextWriter.WriteValue(previousActivity.UniqueID);
            }
            if (!(nextActivity is null))
            {
                jsonTextWriter.WritePropertyName("NextActivity");
                jsonTextWriter.WriteValue(nextActivity.UniqueID);
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
            jsonTextWriter.WriteValue(System.DateTime.Now);
            jsonTextWriter.WritePropertyName("PreviousActivity");
            jsonTextWriter.WriteValue(activity.UniqueID);
            jsonTextWriter.WritePropertyName("Exception");
            jsonTextWriter.WriteValue(exception);
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }
        public void Close()
        {
            jsonTextWriter.Close();
        }

        public void Dispose()
        {
            ((IDisposable)jsonTextWriter).Dispose();
        }
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
        jsonTextWriter.WriteValue(dsfDataObject.ExecutingUser.Identity.ToString());

        jsonTextWriter.WritePropertyName("ExecutionID");
        jsonTextWriter.WriteValue(dsfDataObject.ExecutionID);

        jsonTextWriter.WritePropertyName("ExecutionOrigin");
        jsonTextWriter.WriteValue(dsfDataObject.ExecutionOrigin);

        jsonTextWriter.WritePropertyName("ExecutionOriginDescription");
        jsonTextWriter.WriteValue(dsfDataObject.ExecutionOriginDescription);

        jsonTextWriter.WritePropertyName("ExecutionToken");
        jsonTextWriter.WriteValue(dsfDataObject.ExecutionToken.ToString());

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
