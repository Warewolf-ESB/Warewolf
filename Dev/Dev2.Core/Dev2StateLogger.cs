using System;
using System.IO;

using Dev2.Interfaces;

namespace Dev2.Diagnostics.State
{
    class Dev2StateLogger : IDev2StateLogger
    {
        readonly StreamWriter writer;
        readonly IDSFDataObject dsfDataObject;
        public Dev2StateLogger(IDSFDataObject dsfDataObject, StreamWriter output)
        {
            this.dsfDataObject = dsfDataObject;
            writer = output;
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            writer.WriteLine("header:LogPreExecuteState");
            writer.WriteLine(nextActivity.UniqueID);
            dsfDataObject.LogPreExecuteState(writer);
            writer.Flush();
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            writer.WriteLine("header:LogPostExecuteState");
            writer.WriteLine(previousActivity.UniqueID);
            if (!(nextActivity is null))
            {
                writer.WriteLine(nextActivity.UniqueID);
            }
            dsfDataObject.LogPreExecuteState(writer);
            writer.Flush();
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            writer.WriteLine("header:LogExecuteException");
            writer.WriteLine(activity.UniqueID);
            writer.WriteLine(e);
            dsfDataObject.LogPreExecuteState(writer);
            writer.Flush();
        }

        public void LogExecuteCompleteState()
        {
            writer.WriteLine("header:LogExecuteCompleteState");
            dsfDataObject.LogPreExecuteState(writer);
            writer.Flush();
        }
    }
}

static class DsfDataObjectMethods
{
    public static void LogPreExecuteState(this IDSFDataObject dsfDataObject, StreamWriter writer)
    {
        writer.WriteLine(dsfDataObject.ClientID);
        writer.WriteLine(dsfDataObject.ExecutingUser);
        writer.WriteLine(dsfDataObject.ExecutionID);
        writer.WriteLine(dsfDataObject.ExecutionOrigin);
        writer.WriteLine(dsfDataObject.ExecutionOriginDescription);
        writer.WriteLine(dsfDataObject.ExecutionToken);
        writer.WriteLine(dsfDataObject.IsSubExecution);
        writer.WriteLine(dsfDataObject.IsRemoteWorkflow());

        writer.WriteLine(dsfDataObject.Environment.ToJson());
    }
}
