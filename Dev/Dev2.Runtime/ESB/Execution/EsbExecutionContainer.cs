#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Wrapper class for all executable types in our ESB
    /// </summary>
    public abstract class EsbExecutionContainer : IEsbExecutionContainer
    {
        protected ServiceAction ServiceAction { get; set; }
        protected IDSFDataObject DataObject { get; set; }
        protected IWorkspace TheWorkspace { get; set; }
        protected EsbExecuteRequest Request { get; set; }

        public string InstanceOutputDefinition { get; set; }
        public string InstanceInputDefinition { get; set; }

        public IDSFDataObject GetDataObject() => DataObject;

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObject, theWorkspace, esbChannel, null)
        {
        }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
        {
            ServiceAction = sa;
            DataObject = dataObject;
            TheWorkspace = theWorkspace;
            Request = request;
            DataObject.EsbChannel = esbChannel;
        }

        protected EsbExecutionContainer()
        {
        }

        public abstract Guid Execute(out ErrorResultTO errors, int update);
        public abstract bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext);

        public abstract IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity);
        public virtual SerializableResource FetchRemoteResource(Guid serviceId, string serviceName, bool isDebugMode) { throw new NotImplementedException(); }
        /// <summary>
        /// TODO: This should not be initialized here once 
        /// we have the front end for creating settings.
        /// </summary>
        public virtual Dev2WorkflowSettingsTO GetWorkflowSetting() =>
            new Dev2WorkflowSettingsTO
            {
                ExecutionLogLevel = Config.Server.ExecutionLogLevel,
                EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                LoggerType = LoggerType.JSON,
                KeepLogsForDays = 2,
                CompressOldLogFiles = true
            };

    }
}