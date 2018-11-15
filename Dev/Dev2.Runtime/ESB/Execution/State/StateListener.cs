using System;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.Execution
{
    class StateListener : IStateListener
    {
        readonly IDSFDataObject _dsfDataObject;
        readonly IDev2StateAuditLogger _dev2StateAuditLogger;
        public StateListener(IDev2StateAuditLogger dev2StateAuditLogger, IDSFDataObject dsfDataObject)
        {
            _dsfDataObject = dsfDataObject;
            _dev2StateAuditLogger = dev2StateAuditLogger;
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            var serializer = new Dev2JsonSerializer();
            var auditLog = new AuditLog(_dsfDataObject, "LogAdditionalDetail", serializer.Serialize(detail, Formatting.None), null, null);
            LogAuditState(auditLog);
        }

        public void LogExecuteCompleteState(IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogExecuteCompleteState", null, activity, null);
            LogAuditState(auditLog);
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogExecuteException", e.Message, activity, null);
            LogAuditState(auditLog);
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogPostExecuteState", null, previousActivity, nextActivity);
            LogAuditState(auditLog);
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogPreExecuteState", null, null, nextActivity);
            LogAuditState(auditLog);
        }

        public void LogStopExecutionState(IDev2Activity activity)
        {
            var auditLog = new AuditLog(_dsfDataObject, "LogStopExecutionState", null, activity, null);
            LogAuditState(auditLog);
        }

        private void LogAuditState(AuditLog auditLog)
        {
            _dev2StateAuditLogger.LogAuditState(auditLog);
        }

        public void Dispose()
        {
        }
    }
}
