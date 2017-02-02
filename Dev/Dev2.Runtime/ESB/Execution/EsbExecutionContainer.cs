/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Wrapper class for all executable types in our ESB
    /// </summary>
    public abstract class EsbExecutionContainer : IEsbExecutionContainer
    {
        protected ServiceAction ServiceAction { get; private set; }
        protected IDSFDataObject DataObject { get; private set; }
        protected IWorkspace TheWorkspace { get; private set; }
        private IEsbChannel EsbChannel { get; set; }
        protected EsbExecuteRequest Request { get; private set; }

        public string InstanceOutputDefinition { get; set; }
        public string InstanceInputDefinition { get; set; }

        public IDSFDataObject GetDataObject()
        {
            return DataObject;
        }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObject, theWorkspace, esbChannel, null)
        {
        }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
        {
            ServiceAction = sa;
            DataObject = dataObject;
            TheWorkspace = theWorkspace;
            EsbChannel = esbChannel;
            Request = request;
            DataObject.EsbChannel = EsbChannel;
        }

        protected EsbExecutionContainer()
        {
        }

        public abstract Guid Execute(out ErrorResultTO errors, int update);
        public abstract bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext);

        public abstract IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity);
        public virtual SerializableResource FetchRemoteResource(Guid serviceId, string serviceName, bool isDebugMode) { throw new NotImplementedException(); }
    }
}
