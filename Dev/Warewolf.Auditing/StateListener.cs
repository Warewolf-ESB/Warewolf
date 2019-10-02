/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces.Logging;
using Dev2.Common.Serializers;
using Dev2.Interfaces;
using Newtonsoft.Json;
using System;

namespace Warewolf.Auditing
{
    class StateListener : IStateListener
    {
        readonly IDSFDataObject _dsfDataObject;
        readonly IWarewolfLogWriter _logWriter;
        public StateListener(IWarewolfLogWriter logWriter, IDSFDataObject dsfDataObject)
        {
            _dsfDataObject = dsfDataObject;
            _logWriter = logWriter;
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            var serializer = new Dev2JsonSerializer();
            var auditLog = new Audit(_dsfDataObject, "LogAdditionalDetail", serializer.Serialize(detail, Formatting.None), null, null);
            LogAuditState(auditLog);
        }

        public void LogExecuteCompleteState(IDev2Activity activity)
        {
            var auditLog = new Audit(_dsfDataObject, "LogExecuteCompleteState", null, activity, null);
            LogAuditState(auditLog);
        }

        public void LogExecuteException(Exception exception, IDev2Activity activity)
        {
            var auditLog = new Audit(_dsfDataObject, "LogExecuteException", exception.Message, activity, null, exception);
            LogAuditState(auditLog);
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            var auditLog = new Audit(_dsfDataObject, "LogPostExecuteState", null, previousActivity, nextActivity);
            LogAuditState(auditLog);
        }

        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            var auditLog = new Audit(_dsfDataObject, "LogPreExecuteState", null, null, nextActivity);
            LogAuditState(auditLog);
        }

        public void LogStopExecutionState(IDev2Activity activity)
        {
            var auditLog = new Audit(_dsfDataObject, "LogStopExecutionState", null, activity, null);
            LogAuditState(auditLog);
        }

        private void LogAuditState(Audit auditLog)
        {
            _logWriter.LogAuditState(auditLog);
        }

        public void Dispose()
        {
        }
    }
}
