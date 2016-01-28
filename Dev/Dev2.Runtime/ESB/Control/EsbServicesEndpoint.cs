
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ESB.Control
{

    /// <summary>
    /// Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint : IFrameworkDuplexDataChannel, IEsbWorkspaceChannel
    {

        #region IFrameworkDuplexDataChannel Members

        readonly Dictionary<string, IFrameworkDuplexCallbackChannel> _users = new Dictionary<string, IFrameworkDuplexCallbackChannel>();

        public void Register(string userName)
        {
            if(_users.ContainsKey(userName))
            {
                _users.Remove(userName);
            }

            _users.Add(userName, OperationContext.Current.GetCallbackChannel<IFrameworkDuplexCallbackChannel>());
            NotifyAllClients(string.Format("User '{0}' logged in", userName));

        }

        public void Unregister(string userName)
        {
            if(UserExists(userName))
            {
                _users.Remove(userName);
                NotifyAllClients(string.Format("User '{0}' logged out", userName));
            }
        }

        public void ShowUsers(string userName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=========Current Users==========");
            sb.Append("\r\n");
            _users.ToList().ForEach(c => sb.Append(c.Key + "\r\n"));
            SendPrivateMessage("System", userName, sb.ToString());

        }

        public void SendMessage(string userName, string message)
        {
            string suffix = " Said:";
            if(userName == "System")
            {
                suffix = string.Empty;
            }
            NotifyAllClients(string.Format("{0} {1} {2}", userName, suffix, message));
        }

        public void SendPrivateMessage(string userName, string targetUserName, string message)
        {
            string suffix = " Said:";
            if(userName == "System")
            {
                suffix = string.Empty;
            }
            if(UserExists(userName))
            {
                if(!UserExists(targetUserName))
                {
                    NotifyClient(userName, string.Format("System: Message failed - User '{0}' has logged out ", targetUserName));
                }
                else
                {
                    NotifyClient(targetUserName, string.Format("{0} {1} {2}", userName, suffix, message));
                }
            }
        }

        public void SetDebug(string userName, string serviceName, bool debugOn)
        {

        }

        public void Rollback(string userName, string serviceName, int versionNo)
        {

        }

        public void Rename(string userName, string resourceType, string resourceName, string newResourceName)
        {


        }

        public void ReloadSpecific(string userName, string serviceName)
        {


        }

        public void Reload()
        {

        }

        bool UserExists(string userName)
        {
            return _users.ContainsKey(userName) || userName.Equals("System", StringComparison.InvariantCultureIgnoreCase);
        }

        void NotifyAllClients(string message)
        {
            _users.ToList().ForEach(c => NotifyClient(c.Key, message));
        }

        void NotifyClient(string userName, string message)
        {

            try
            {
                if(UserExists(userName))
                {
                    _users[userName].CallbackNotification(message);
                }
            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex);
                _users.Remove(userName);
            }
        }

        #endregion


        /// <summary>
        ///Loads service definitions.
        ///This is a singleton service so this object
        ///will be visible in every call 
        /// </summary>
        public EsbServicesEndpoint()
        {
            try
            {

            }
            catch(Exception ex)
            {
                Dev2Logger.Error(ex);
                throw;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="request"></param>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors)
        {

            var resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = null;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () =>
            {
                theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            });
            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                IResource resource;
                try
                {
                    resource = dataObject.ResourceID == Guid.Empty ? GetResource(workspaceId, dataObject.ServiceName) : GetResource(workspaceId, dataObject.ResourceID);
                }
                catch(Exception ex)
                {
                    Dev2Logger.Error(ex);
                    errors.AddError(string.Format("Service [ {0} ] not found.", dataObject.ServiceName));
                    return resultID;
                }

                // TODO : Amend here to respect Inputs only when creating shape ;)
                if(resource != null)
                {
                    if(resource.DataList != null)
                    {
                        Dev2Logger.Debug("Mapping Inputs from Environment");
                        ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(dataObject, dataObject.RawPayload, resource.DataList.ToString(), 0);
                    }
                }
                dataObject.RawPayload = new StringBuilder();

                // We need to create the parentID around the system ;)
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;

            }

            try
            {
                // Setup the invoker endpoint ;)
                Dev2Logger.Debug("Creating Invoker");
                using(var invoker = new EsbServiceInvoker(this, this, theWorkspace, request))
                {
                    // Should return the top level DLID
                    ErrorResultTO invokeErrors;
                    resultID = invoker.Invoke(dataObject, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch(Exception ex)
            {
                errors.AddError(ex.Message);
            }



            return resultID;
        }


        static IResource GetResource(Guid workspaceId, Guid resourceId)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            
            return resource;
        }

        static IResource GetResource(Guid workspaceId, string resourceName)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceName) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);
            return resource;
        }

       

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update)
        {
            errors = null;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var executionContainer = new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
            executionContainer.PerformLogExecution(uri, update);
        }



        /// <summary>
        /// Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="update"></param>
        /// <param name="handleErrors"> buble up errors or not</param>
        /// <returns></returns>
        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors, int update, bool handleErrors)
        {
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = CreateEsbServicesInvoker(theWorkspace);
            ErrorResultTO invokeErrors;
            var oldID = dataObject.DataListID;
            errors = new ErrorResultTO();
            
            // local non-scoped execution ;)
            var isLocal = !dataObject.IsRemoteWorkflow();

            var principle = Thread.CurrentPrincipal;
            Dev2Logger.Info("SUB-EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE  [ " + dataObject.ServiceName + " ]");

            if(dataObject.RunWorkflowAsync)
            {
        
                ExecuteRequestAsync(dataObject, inputDefs, invoker, isLocal, oldID, out invokeErrors, update);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                if (isLocal)
                {
                    if (GetResource(workspaceId, dataObject.ResourceID) == null && GetResource(workspaceId, dataObject.ServiceName) == null)
                    {
                        errors.AddError(string.Format("Resource {0} not found.", dataObject.ServiceName));
                
                        return null;
                    }
                }

                var executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName, isLocal, oldID);
                if (executionContainer != null)
                {
                    CreateNewEnvironmentFromInputMappings(dataObject, inputDefs,update);
                    if (!isLocal)
                    {
                        SetRemoteExecutionDataList(dataObject, executionContainer, errors);
                    }
                    if (!errors.HasErrors())
                    {
                        executionContainer.InstanceInputDefinition = inputDefs;
                        executionContainer.InstanceOutputDefinition = outputDefs;
                        executionContainer.Execute(out invokeErrors, update);
                        var env = UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dataObject, outputDefs, update, handleErrors, errors);
                        
                        errors.MergeErrors(invokeErrors);
                        string errorString = dataObject.Environment.FetchErrors();
                        invokeErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                        errors.MergeErrors(invokeErrors);                        
                        return env;
                    }
                    errors.AddError(string.Format("Resource {0} not found.", dataObject.ServiceName));
                }
            }
            return new ExecutionEnvironment();
        }

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            var innerEnvironment = dataObject.Environment;
            dataObject.PopEnvironment();
            DataListUtil.OutputsToEnvironment(innerEnvironment, dataObject.Environment, outputDefs, update);
            if (innerEnvironment.HasErrors() && !handleErrors)
            {
                foreach (var error in innerEnvironment.AllErrors)
                {
                    if (!dataObject.Environment.AllErrors.Contains(error))
                    {
                        dataObject.Environment.AllErrors.Add(error);
                        errors.AddError(error);
                    }
                }
                foreach (var error in innerEnvironment.Errors)
                {
                    if (!dataObject.Environment.AllErrors.Contains(error))
                    {
                        dataObject.Environment.AllErrors.Add(error);
                        errors.AddError(error);
                    }
                }
            }
            if (innerEnvironment.HasErrors() && handleErrors)
            {
                foreach (var error in innerEnvironment.AllErrors)
                {
                    if (!dataObject.Environment.AllErrors.Contains(error))
                    {
               
                        errors.AddError(error);
                    }
                }
                foreach (var error in innerEnvironment.Errors)
                {
                    if (!dataObject.Environment.AllErrors.Contains(error))
                    {
          
                        errors.AddError(error);
                    }
                }
            }
            return innerEnvironment;
        }

        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update)
        {
            var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs,update);
            dataObject.PushEnvironment(shapeDefinitionsToEnvironment);
        }

        static void SetRemoteExecutionDataList(IDSFDataObject dataObject, IEsbExecutionContainer executionContainer, ErrorResultTO errors)
        {
            var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
            if(remoteContainer != null)
            {
                var fetchRemoteResource = remoteContainer.FetchRemoteResource(dataObject.ResourceID,dataObject.ServiceName,dataObject.IsDebugMode());
                if(fetchRemoteResource != null)
                {
                    fetchRemoteResource.DataList = fetchRemoteResource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                    var remoteDataList = fetchRemoteResource.DataList;
                    dataObject.RemoteInvokeResultShape = new StringBuilder(remoteDataList);
                    dataObject.ServiceName = fetchRemoteResource.ResourceCategory;
                }
                else
                {
                    var message = string.Format("Remote Execution Failed. Service: {0} not found.", dataObject.ServiceName);
                    errors.AddError(message);
                }
            }
        }

        void ExecuteRequestAsync(IDSFDataObject dataObject, string inputDefs, IEsbServiceInvoker invoker, bool isLocal, Guid oldID, out ErrorResultTO invokeErrors, int update)
        {
            var clonedDataObject = dataObject.Clone();
            invokeErrors = new ErrorResultTO();
            var executionContainer = invoker.GenerateInvokeContainer(clonedDataObject, clonedDataObject.ServiceName, isLocal, oldID);
            if(executionContainer != null)
            {
                if(!isLocal)
                {
                    var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
                    if(remoteContainer != null)
                    {
                        if(!remoteContainer.ServerIsUp())
                        {
                            invokeErrors.AddError("Asynchronous execution failed: Remote server unreachable");
                        }
                        SetRemoteExecutionDataList(dataObject, executionContainer, invokeErrors);
                    }
                }
                if(!invokeErrors.HasErrors())
                {
                    var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
                    Task.Factory.StartNew(() =>
                    {
                        Dev2Logger.Info("ASYNC EXECUTION USER CONTEXT IS [ " + Thread.CurrentPrincipal.Identity.Name + " ]");
                        ErrorResultTO error;
                        clonedDataObject.Environment = shapeDefinitionsToEnvironment;
                        executionContainer.Execute(out error, update);
                        return clonedDataObject;
                    }).ContinueWith(task =>
                    {
                        if (task.Result != null)
                        {
                            task.Result.Environment = null;
                        }
                    });
                   
                }
            }
            else
            {
                invokeErrors.AddError("Asynchronous execution failed: Resource not found");
            }

        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceId, out ErrorResultTO errors, int update)
        {
            var serviceID = dataObject.ResourceID;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = new EsbServiceInvoker(this, this, theWorkspace);
            var generateInvokeContainer = invoker.GenerateInvokeContainer(dataObject, serviceID, true);
            generateInvokeContainer.Execute(out errors,update);
            var convertFrom = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObject, "", update);
            var jsonSerializerSettings = new JsonSerializerSettings();
            var deserializeObject = JsonConvert.DeserializeObject<T>(convertFrom, jsonSerializerSettings);
            return deserializeObject;
        }


        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="resourceId">Name of the service.</param>
        /// <returns></returns>
        public StringBuilder FindServiceShape(Guid workspaceId, Guid resourceId)
        {
            var result = new StringBuilder();
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            if(resource == null)
            {
                return EmptyDataList;
            }

            if(resource.DataList != null)
            {
                result = resource.DataList;
            }

            // Handle services ;)
            if(result.ToString() == "<DataList />" && resource.ResourceType != ResourceType.WorkflowService)
            {
                ErrorResultTO errors;
                result = GlueInputAndOutputMappingSegments(resource.Outputs, resource.Inputs, out errors);
            }


            if(string.IsNullOrEmpty(result.ToString()))
            {
                return EmptyDataList;
            }
            result.Replace(GlobalConstants.SerializableResourceQuote, "\"");
            result.Replace(GlobalConstants.SerializableResourceSingleQuote, "\'");
            return result;
        } 
      

        static readonly StringBuilder EmptyDataList = new StringBuilder("<DataList></DataList>");


        /// <summary>
        /// Glues the input and output mapping segments.
        /// </summary>
        /// <param name="inputFragment">The input fragment.</param>
        /// <param name="outputFragment">The output fragment.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        StringBuilder GlueInputAndOutputMappingSegments(string inputFragment, string outputFragment, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            if(!string.IsNullOrEmpty(outputFragment))
            {
                try
                {
                    // finally glue the two together ;)
                    XmlDocument oDLXDoc = new XmlDocument();
                    oDLXDoc.LoadXml(outputFragment);

                    if(oDLXDoc.DocumentElement != null)
                    {
                        outputFragment = oDLXDoc.DocumentElement.InnerXml;
                    }
                }
                catch(Exception e)
                {
                    Dev2Logger.Error(e);
                    errors.AddError(e.Message);
                }
            }

            if(!string.IsNullOrEmpty(inputFragment))
            {

                try
                {
                    // finally glue the two together ;)
                    XmlDocument iDLXDoc = new XmlDocument();
                    iDLXDoc.LoadXml(inputFragment);

                    if(iDLXDoc.DocumentElement != null)
                    {
                        inputFragment = iDLXDoc.DocumentElement.InnerXml;
                    }
                }
                catch(Exception e)
                {
                    Dev2Logger.Error(e);
                }
            }
            StringBuilder result = new StringBuilder();
            result.Append("<DataList>");
            result.Append(outputFragment);
            result.Append(inputFragment);
            result.Append("</DataList>");
            return result;
        }

        protected virtual IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace)
        {
            return new EsbServiceInvoker(this, this, theWorkspace);
        }

    }
}
