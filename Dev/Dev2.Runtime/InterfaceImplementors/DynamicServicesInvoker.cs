
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#region Change Log

//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    The data layer of the Dynamic Service Engine
//                  This is where all actions get executed.

#endregion

using System.Xml.XPath;
using Dev2.Common;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.DB_Helper;
using Dev2.DB_Sanity;
using Dev2.DB_Support;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Enums;
using Dev2.PathOperations;
using Dev2.Reflection;
using Dev2.Runtime.Security;
using Dev2.Server.Datalist;
using Dev2.Workspaces;
using Microsoft.CSharp;
using System;
using System.Activities;
using System.Activities.Statements;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using Unlimited.Framework;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;
using enActionType = Dev2.DynamicServices.enActionType;
using TransactionScope = System.Transactions.TransactionScope;

namespace Dev2.Runtime.InterfaceImplementors
{

    #region Dynamic Invocation Class - Invokes Dynamic Endpoint and returns responses to the Caller

    /// <summary>
    ///     Author:         Sameer Chunilall
    ///     Date:           2010-01-24
    ///     Description:    Responsible for the invocation of a Dynamic Service and/or bizrule
    ///     The invocation of the service is not internal to any dynamic service object
    ///     as the service is a singleton and we want every service request to be
    ///     executed in insolation of other as we are working in a static and
    ///     multi-threaded environment
    /// </summary>
    public class DynamicServicesInvoker : IDynamicServicesInvoker, IDisposable
    {
        #region Fields

        private static readonly IServerDataListCompiler SvrCompiler = DataListFactory.CreateServerDataListCompiler();
        private static readonly IDataListCompiler ClientCompiler = DataListFactory.CreateDataListCompiler();

        private readonly IFrameworkDataChannel _dsfChannel;

        private readonly bool _loggingEnabled;
        private readonly IFrameworkDuplexDataChannel _managementChannel;
        private readonly IWorkspace _workspace;

        private readonly dynamic _returnVal = new UnlimitedObject(Resources.DynamicService_ServiceResponseTag);
        //We use the following to impersonate a user in the current execution environment
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
                                             int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        #endregion

        #region public properties
        public bool IsLoggingEnabled
        {
            get { return _loggingEnabled; }
        }

        public IDynamicServicesHost Host
        {
            get { return _workspace.Host; }
            set { }
        }
        #endregion

        #region Constructors

        public DynamicServicesInvoker()
        {
        }

        public DynamicServicesInvoker(IFrameworkDataChannel dsfChannel,
                                      IFrameworkDuplexDataChannel managementChannel = null, bool loggingEnabled = false,
                                      IWorkspace workspace = null)
        {
            _dsfChannel = dsfChannel;
            _loggingEnabled = loggingEnabled;
            if(managementChannel != null)
            {
                _managementChannel = managementChannel;
            }

            // 2012.10.17 - 5782: TWR - Added workspace parameter
            _workspace = workspace;
        }

        #endregion

        #region Public Methods

        #region Invoke Method

        /// <summary>
        ///     Responsible for the processing of all inbound requests
        ///     This method is reentrant and will call itself to
        ///     for every invocation required in every generation
        ///     of nesting. e.g services made up of services
        /// </summary>
        /// <param name="resourceDirectory">The singleton instance of the service library that contains all the logical services</param>
        /// <param name="xmlRequest">The actual client request message</param>
        /// <param name="dataListId">The id of the data list</param>
        /// <param name="errors">Errors resulting from this invoke</param>
        /// <returns></returns>
        public Guid Invoke(IDynamicServicesHost resourceDirectory, dynamic xmlRequest, Guid dataListId,
                           out ErrorResultTO errors)
        {
            // Host = resourceDirectory

            #region Async processing of client request - queue the work item asynchronously

            //Get an UnlimitedObject from the xml string provided by the caller
            //TraceWriter.WriteTraceIf(_managementChannel != null && _loggingEnabled, "Inspecting inbound data request", Resources.TraceMessageType_Message);
            Guid result = GlobalConstants.NullDataListID;

            var allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();

            if(xmlRequest.Async is string)
            {
                //TraceWriter.WriteTrace(_managementChannel, "Caller requested async execution");
                bool isAsync;

                bool.TryParse(xmlRequest.Async, out isAsync);

                if(isAsync)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        ErrorResultTO tmpErrors;
                        //TraceWriter.WriteTrace(_managementChannel, "Queuing Asynchronous work", Resources.TraceMessageType_Message);
                        xmlRequest.RemoveElementByTagName("Async");
                        IDynamicServicesInvoker invoker = new DynamicServicesInvoker(_dsfChannel, _managementChannel);
                        result = invoker.Invoke(resourceDirectory, xmlRequest, dataListId, out tmpErrors);
                        if(tmpErrors.HasErrors())
                        {
                            allErrors.MergeErrors(tmpErrors);
                        }
                        //TraceWriter.WriteTrace(result.XmlString);
                        if(result != GlobalConstants.NullDataListID)
                        {
                            // PBI : 5376
                            SvrCompiler.DeleteDataListByID(result, true); //TODO: Clean it up ;)
                        }
                    });
                    dynamic returnData = new UnlimitedObject();
                    returnData.Load(string.Format("<ServiceResponse>{0} Work Item Queued..</ServiceResponse>",
                                                  DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")));
                    return returnData;
                }
            }

            #endregion

            #region Get a handle on the service that is being requested from the service directory

            string serviceName = string.Empty;

            //Set the service name as this is a complex message
            //with multiple services requests embedded in the inbound data
            //This will allow us to 
            if(xmlRequest.Service is IEnumerable<UnlimitedObject>)
            {
                IEnumerable<UnlimitedObject> services = xmlRequest.Service;
                dynamic serviceData = services.First();
                if(serviceData.Service is string)
                {
                    serviceName = serviceData.Service;
                }
            }


            //If there is only a single service request then get the service name
            if(xmlRequest.Service is string)
            {
                serviceName = xmlRequest.Service;
            }

            //If the service name does not exist return an error to the caller
            if(string.IsNullOrEmpty(serviceName))
            {
                xmlRequest.Error = Resources.DynamicServiceError_ServiceNotSpecified;
            }

            //Try to retrieve the service from the service directory

            IEnumerable<DynamicService> service;
            Host.LockServices();

            try
            {
                service = from c in resourceDirectory.Services
                          where serviceName != null
                          && c.Name.Trim().Equals(serviceName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                          select c;
            }
            finally
            {
                Host.UnlockServices();
            }

            service = service.ToList();

            if(!service.Any())
            {
                TraceWriter.WriteTrace(_managementChannel, string.Format("Service '{0}' Not Found", serviceName),
                                       Resources.TraceMessageType_Error);

                allErrors.AddError(string.Format("Service '{0}' Not Found", serviceName));

                throw new InvalidOperationException(string.Format("Service '{0}' Not Found", serviceName));

                //xmlRequest.Error = Resources.DynamicServiceError_ServiceNotFound;
            }

            #endregion

            //Instantiate a Dynamic Invocation type to invoke the service

            #region  Transactionalized Service Invocation with support for MS-DTC

            dynamic dseException = null;

            //The transactionScope is used to create an ambient transaction that every action will be subject to
            //This transaction 
            try
            {
                //TraceWriter.WriteTrace(_managementChannel, string.Format("Setting up transaction scope", serviceName), Resources.TraceMessageType_Message);
                using(var transactionScope = new TransactionScope())
                {
                    //TraceWriter.WriteTrace(_managementChannel, string.Format("Invoking Service '{0}'", serviceName), Resources.TraceMessageType_Message);

                    #region Process several requests to different services as a single unit of work

                    //Type 3 request (read above)
                    //This is handled differently to type 1 and 2 requests
                    //as it can execute in the context of either a single or 
                    //multiple services.
                    //if (xmlRequest.IsMultipleRequests) {
                    //    TraceWriter.WriteTrace(_managementChannel, "Caller requested multiple service execution in single request", Resources.TraceMessageType_Message);
                    //    dynamic results = new UnlimitedObject();

                    //    foreach (dynamic request in xmlRequest.Requests) {

                    //        dynamic result = new DynamicServicesInvoker(_dsfChannel, _managementChannel).Invoke(resourceDirectory, request);
                    //        if (result.HasError) {
                    //            return result;
                    //        }
                    //        results.AddResponse(result);
                    //    }
                    //    transactionScope.Complete();

                    //    return results;
                    //}

                    #endregion

                    DynamicService s = service.First();
                    result = Invoke(s, xmlRequest, dataListId, out errors);
                    if(result == GlobalConstants.NullDataListID)
                    {
                        allErrors.AddError("Failed to invoke service");
                 
                    }
                    
                    if(!ClientCompiler.HasErrors(result))
                    {
                        transactionScope.Complete();
                    }

                    //The service exists so invoke the service which runs all actions defined for the service
                    //Return the response
                    //return xmlResponses;
                }
            }
            //Occurs when an operation is attempted on a rolled back transaction
            catch(TransactionAbortedException abortEx)
            {
                dseException = abortEx;
            }
            //This exception is thrown when an action is attempted on a transaction that is in doubt. 
            //A transaction is in doubt when the state of the transaction cannot be determined. 
            //Specifically, the final outcome of the transaction, whether it commits or aborts, is never known for this transaction.
            catch(TransactionInDoubtException inDoubtEx)
            {
                dseException = inDoubtEx;
            }
            //Thrown when a resource manager cannot communicate with the transaction manager. 
            catch(TransactionManagerCommunicationException transactionManagerEx)
            {
                dseException = transactionManagerEx;
            }
            //Thrown when a promotion fails
            catch(TransactionPromotionException promotionException)
            {
                dseException = promotionException;
            }
            catch(TransactionException transactionEx)
            {
                dseException = transactionEx;
            }

            if(dseException != null)
            {
                TraceWriter.WriteTrace(_managementChannel,
                                       string.Format("Service Execution Failed With Error\r\n{0}",
                                                     new UnlimitedObject(dseException).XmlString),
                                       Resources.TraceMessageType_Error);
            }

            // set error variable
            errors = allErrors;
            if(errors.HasErrors())
            {
                DispatchDebugState(xmlRequest, dataListId, allErrors);
            }

            return result;

            #endregion
        }

        #endregion

        private readonly WorkflowApplicationFactory _wf = new WorkflowApplicationFactory();

        public Guid Invoke(DynamicService service, dynamic xmlRequest, Guid dataListId, out ErrorResultTO errors)
        {
            //dynamic result = new UnlimitedObject();
            //dynamic forwardResult = new UnlimitedObject();
            var allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();

            if(service == null)
            {
                allErrors.AddError("Dynamic Service not found exception");
                return GlobalConstants.NullDataListID;
            }

            string dataList = service.DataListSpecification;


            // PBI : 5376 Amendedment for DataList Server
            Guid result = GlobalConstants.NullDataListID;
            string data = xmlRequest.XmlString.Trim();
            byte[] incomingData = Encoding.UTF8.GetBytes(data);
            Guid serviceDataId;


            var performDataListInMerge = false;

            if(dataList != string.Empty)
            {
                serviceDataId = SvrCompiler.ConvertTo(null, DataListFormat.CreateFormat(GlobalConstants._XML),
                                                       incomingData, dataList, out errors);
                errors = new ErrorResultTO(); // re-set to avoid carring

                // PBI : 5376
                // If dataListID == NullID, create a new list and set it as the current ID
                // Else, create a new list, union the old data into the new and continue on ;)
                if(dataListId != GlobalConstants.NullDataListID)
                {
                    serviceDataId = SvrCompiler.Merge(null, serviceDataId, dataListId, enDataListMergeTypes.Union,
                                                       enTranslationDepth.Data, false, out errors);
                }
                else
                {
                    performDataListInMerge = true;
                }
            }
            else
            {
                serviceDataId = SvrCompiler.CloneDataList(dataListId, out errors);
                performDataListInMerge = true;
            }

            if (errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }
            IDSFDataObject dataObject = new DsfDataObject(xmlRequest.XmlString, serviceDataId);
            dataObject.DataList = dataList;

            if(performDataListInMerge)
            {
                SvrCompiler.ConditionalMerge(null, DataListMergeFrequency.Always, serviceDataId,
                                              dataObject.DatalistInMergeID,
                                              DataListMergeFrequency.Always, dataObject.DatalistInMergeType,
                                              dataObject.DatalistInMergeDepth);
            }

            // TODO  : Reset the AmbientDataList to this ID?

            // Fetch data for Input binding...
            DataListTranslatedPayloadTO tmpData = null;
            dynamic inputBinding = null;

            // End PBI Amendments

            foreach(ServiceAction serviceAction in service.Actions)
            {
                //TraceWriter.WriteTrace(_managementChannel, string.Format("Validating the inputs of Service '{0}'", service.Name), Resources.TraceMessageType_Message);
                foreach(ServiceActionInput sai in serviceAction.ServiceActionInputs)
                {
                    //Assigning the input the value from the callers request data
                    //TraceWriter.WriteTrace(_managementChannel, string.Format("Discovered input '{0}'", sai.Name), Resources.TraceMessageType_Message);
                    if(sai.CascadeSource)
                    {
                        TraceWriter.WriteTrace(_managementChannel, string.Format("Input '{0}' is cascaded", sai.Name),
                                               Resources.TraceMessageType_Message);
                        //This is a cascaded input so retrieve the value from the
                        //previous actions response
                        //sai.Value = forwardResult.GetValue(sai.Name);
                    }
                    else
                    {
                        if(tmpData == null)
                        {
                            tmpData = SvrCompiler.ConvertFrom(null, serviceDataId, enTranslationDepth.Data,
                                                               DataListFormat.CreateFormat(GlobalConstants._XML),
                                                               out errors);

                            if(!DataListUtil.isNullADL(tmpData.FetchAsString()))
                            {
                                inputBinding = new UnlimitedObject(tmpData.FetchAsString());
                            }
                            else
                            {
                                // Empty data, try the incoming stream?!
                                inputBinding = new UnlimitedObject(xmlRequest.XmlString);
                            }
                        }

                        // 16.10.2012 : Travis.Frisinger - EmptyToNull amendments
                        string tmpVal = inputBinding.GetValue(sai.Name);
                        if(sai.EmptyToNull && tmpVal == string.Empty)
                        {
                            tmpVal = AppServerStrings.NullConstant;
                        }

                        sai.Value = tmpVal;
                    }

                    //TraceWriter.WriteTrace(string.Format("Assigning value {0} to input '{1}'", sai.Value.ToString(), sai.Name));

                    //Validating inputs if there is validation set up in the service definition
                    if(sai.IsRequired && string.IsNullOrEmpty(sai.DefaultValue))
                    {
                        if(!sai.Validate())
                        {
                            allErrors.AddError(string.Format("Validation Failure. Argument '{0}' failed validation.",
                                                             sai.Name));

                            //TraceWriter.WriteTrace(_managementChannel, string.Format("Argument '{0}' failed validation", sai.Name), Resources.TraceMessageType_Message);
                            //xmlRequest.Error = string.Format("Validation Failure. Argument '{0}' failed validation.", sai.Name);
                            //return xmlRequest;
                        }
                    }
                    //This input does not have any value in the callers request
                    //so we are setting the input value to the full request
                    if(string.IsNullOrEmpty(sai.Value.ToString()))
                    {
                        sai.Value = !string.IsNullOrEmpty(sai.DefaultValue) ? sai.DefaultValue : string.Empty;
                    }
                }

                //if (service.Mode == enDynamicServiceMode.ValidationOnly)
                //{
                //    xmlRequest.ValidationOnly.Result = true;

                //    return xmlRequest;
                //}

                if(serviceAction.ActionType == enActionType.Switch)
                {
                    if(!string.IsNullOrEmpty(serviceAction.Cases.DataElementName))
                    {
                        ////Assigning the input the value from the callers request data
                        //if (serviceAction.Cases.CascadeSource)
                        //{
                        //This is a cascaded input so retrieve the value from the
                        //previous actions response
                        //sai.Value = actionResponse.GetValue(sai.Name);
                        //serviceAction.Cases.DataElementValue = forwardResult.GetValue(serviceAction.Cases.DataElementName);
                        //}
                        //else
                        //{
                        serviceAction.Cases.DataElementValue = xmlRequest.GetValue(serviceAction.Cases.DataElementName);
                        //}
                    }
                }


                //
                //Juries - This is a dirty hack, naughty naughty.
                //Hijacked current functionality to enable erros to be added to an item after its already been added to the tree
                //
                if(allErrors.HasErrors())
                {
                    DebugDispatcher.Instance.Write(new DebugState()
                    {
                        ParentID = dataObject.ParentInstanceID,
                        WorkspaceID = dataObject.WorkspaceID,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                        IsSimulation = false,
                        ServerID = dataObject.ServerID,
                        Server = string.Empty,
                        Version = string.Empty,
                        Name = GetType().Name,
                        HasError = true,
                        ErrorMessage = allErrors.MakeDisplayReady(),
                        ActivityType = ActivityType.Workflow,
                        StateType = StateType.Append
                    });
                }
                
                // TODO : properly build up DataList prior to this....
                result = Invoke(serviceAction, dataObject, dataList);

                // Remember to clean up ;)
                if(dataListId != GlobalConstants.NullDataListID)
                {
                    // Merge the execution DL into the mainDL ;)

                    Guid mergeId = SvrCompiler.Merge(null, dataListId, serviceDataId, enDataListMergeTypes.Union,
                                                      enTranslationDepth.Data, false, out errors);
                    SvrCompiler.DeleteDataListByID(serviceDataId, true);

                    // Now reset the DataListID on DataObject ;)
                    if(result != serviceDataId)
                    {
                        throw new Exception("FATAL ERROR : DataList Execution Mis-Match!");
                    }

                    dataObject.DataListID = mergeId;
                    result = mergeId;
                }
                else
                {
                    SvrCompiler.ConditionalMerge(null,
                                                  DataListMergeFrequency.Always | DataListMergeFrequency.OnCompletion,
                                                  dataObject.DatalistOutMergeID, dataObject.DataListID,
                                                  dataObject.DatalistOutMergeFrequency, dataObject.DatalistOutMergeType,
                                                  dataObject.DatalistOutMergeDepth);
                } // else we want to keep the DL around until we end execution

                #region Terminate the service if this Service Action is marked to terminate on fault

                //If this action should immediately terminate the execution of this service 
                //then stop here and return the results thus far
                if(serviceAction.TerminateServiceOnFault && errors.HasErrors())
                {
                    result = GlobalConstants.NullDataListID;
                }

                #endregion
            }

            return result;
        }

        //public dynamic Invoke(ServiceAction serviceAction, dynamic xmlRequest, string dataList)
        public Guid Invoke(ServiceAction serviceAction, IDSFDataObject dataObj, string dataList)
        {
            Guid result = GlobalConstants.NullDataListID;

            //TraceWriter.WriteTrace(_managementChannel, string.Format("Invoking service action '{0}' of Service '{1}'", serviceAction.Name, serviceAction.ServiceName??string.Empty), Resources.TraceMessageType_Message);
            switch(serviceAction.ActionType)
            {
                case enActionType.BizRule:
                    //result = BizRule(serviceAction, xmlRequest);
                    break;

                case enActionType.InvokeDynamicService:
                    result = DynamicService(serviceAction, dataObj.DataListID);
                    break;

                case enActionType.InvokeManagementDynamicService:
                    result = ManagementDynamicService(serviceAction, dataObj);
                    break;

                case enActionType.InvokeServiceMethod:
                    break;

                case enActionType.InvokeStoredProc:
                    switch(serviceAction.Source.Type)
                    {
                        case enSourceType.MySqlDatabase:
                            //result = MySqlDatabase(serviceAction, xmlRequest);
                            break;

                        case enSourceType.SqlDatabase:
                            result = SqlDatabaseCommand(serviceAction, dataObj);
                            break;
                    }
                    break;

                case enActionType.InvokeWebService:
                    //result = WebService(serviceAction, xmlRequest);
                    break;

                case enActionType.Plugin:
                    result = Plugin(serviceAction, dataObj);
                    break;

                case enActionType.Switch:
                    //result = Switch(serviceAction, xmlRequest);
                    break;

                case enActionType.Workflow:
                    result = WorkflowApplication(serviceAction, dataObj, dataList);
                    break;
            }

            return result;
        }

        //public dynamic WorkflowApplication(ServiceAction action, dynamic xmlRequest, string dataList)
        public dynamic WorkflowApplication(ServiceAction action, IDSFDataObject dataObj, string dataList)
        {
            //var dataObject = new DsfDataObject(xmlRequest.XmlString);
            ErrorResultTO errors;
            Guid instanceId = Guid.Empty;
            Guid parentWorkflowInstanceId;
            Guid parentInstanceId = Guid.Empty;
            string bookmark = string.Empty;

            // PBI : 5376 Refactored 
            IBinaryDataListEntry tmp = SvrCompiler.Evaluate(null, dataObj.DataListID,
                                                             DataList.Contract.enActionType.System,
                                                             enSystemTag.Bookmark.ToString(), out errors);
            if(tmp != null)
            {
                bookmark = tmp.FetchScalar().TheValue;
            }

            tmp = SvrCompiler.Evaluate(null, dataObj.DataListID, DataList.Contract.enActionType.System,
                                        enSystemTag.InstanceId.ToString(), out errors);
            if(tmp != null)
            {
                Guid.TryParse(tmp.FetchScalar().TheValue, out instanceId);
            }

            tmp = SvrCompiler.Evaluate(null, dataObj.DataListID, DataList.Contract.enActionType.System,
                                        enSystemTag.ParentWorkflowInstanceId.ToString(), out errors);
            if(tmp != null)
            {
                Guid.TryParse(tmp.FetchScalar().TheValue, out parentWorkflowInstanceId);
            }

            //tmp = SvrCompiler.Evaluate(null, dataObj.DataListID, DataList.Contract.enActionType.System,
            //                            enSystemTag.ParentInstanceID.ToString(), out errors);
            //if (tmp != null)
            //{
            //    Guid.TryParse(tmp.FetchScalar().TheValue, out parentInstanceId);
            //}

            //bool isDebug = false;

            //if (xmlRequest.BDSDebugMode is string)
            //{
            //    bool.TryParse(xmlRequest.BDSDebugMode, out isDebug);
            //}

            // End PBI Mods

            dataObj.ServiceName = action.ServiceName;

            // Travis : Now set Data List
            dataObj.DataList = dataList;

            Exception wfException = null;
            IDSFDataObject data = null;
            PooledServiceActivity activity = action.PopActivity();

            try
            {

                data = _wf.InvokeWorkflow(activity.Value, dataObj, new List<object> { _dsfChannel }, instanceId, _workspace, bookmark, dataObj.IsDebug);
            }
            catch(Exception ex)
            {
                wfException = ex;
            }
            finally
            {
                action.PushActivity(activity);
            }

            if(data != null)
            {
                return data.DataListID;

                //return UnlimitedObject.GetStringXmlDataAsUnlimitedObject(data.XmlData);
            }
            // ReSharper disable RedundantIfElseBlock
            else
            // ReSharper restore RedundantIfElseBlock
            {
                dynamic returnError = new UnlimitedObject("Error");
                if(wfException != null)
                {
                    returnError.ErrorDetail = new UnlimitedObject(wfException);
                }

                return GlobalConstants.NullDataListID;
            }
        }

        #region Invoke Workflow

        // PBI : 5376 - Broke this signature to avoid use ;)
        public dynamic Workflow(ServiceAction workflowAction, dynamic xmlRequest, int i)
        {
            dynamic returnVal = new UnlimitedObject();
            IDictionary<string, object> inputs = new Dictionary<string, object>();
            inputs.Add("AmbientDataList", new List<string> { xmlRequest.XmlString });

            // ReSharper disable RedundantAssignment
            IDictionary<string, object> output = new Dictionary<string, object>();
            // ReSharper restore RedundantAssignment

            var workflowInvoker = new WorkflowInvoker(workflowAction.WorkflowActivity);
            workflowInvoker.Extensions.Add(_dsfChannel);


            try
            {
                output = workflowInvoker.Invoke(inputs);
                foreach(var data in output)
                {
                    if(data.Value != null)
                    {
                        if(data.Value is List<string>)
                        {
                            foreach(string result in (data.Value as List<string>))
                            {
                                returnVal.AddResponse(
                                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(string.Format("<{0}>{1}</{0}>",
                                                                                                    Resources
                                                                                                        .DynamicService_ServiceResponseTag,
                                                                                                    result)));
                            }
                        }
                    }
                    else
                    {
                        returnVal = xmlRequest;
                    }
                }
            }
            catch(WorkflowApplicationAbortedException workflowAbortedEx)
            {
                returnVal.Error = "Workflow Execution Was Aborted";
                returnVal.ErrorDetail = new UnlimitedObject(workflowAbortedEx).XmlString;
            }
            catch(Exception workflowEx)
            {
                returnVal.Error = "Error occurred executing workflow";
                returnVal.ErrorDetail = new UnlimitedObject(workflowEx).XmlString;
            }


            //ExceptionHandling.WriteEventLogEntry(
            //    "Application",
            //    string.Format("{0}.{1}", this.GetType().Name, "WorkflowCommand"),
            //    string.Format("Exception:{0}\r\n", returnVal.XmlString),
            //    EventLogEntryType.Error
            //);

            //if (returnVal.DSFResult is string) {
            //    return UnlimitedObject.GetStringXmlDataAsUnlimitedObject(returnVal.DSFResult);
            //}
            //else {
            //    if (returnVal.DSFResult is UnlimitedObject) {

            //        if (!string.IsNullOrEmpty(returnVal.DSFResult.XmlString)) {
            //            return returnVal.DSFResult;
            //        }
            //    }
            //}

            return returnVal;
        }

        #endregion

        #region Invoke and Evaluate BizRule

        public dynamic BizRule(ServiceAction sa, dynamic xmlRequest, Guid dataListID)
        {
            dynamic bizRuleException = null;

            // ReSharper disable JoinDeclarationAndInitializer
            dynamic result;
            // ReSharper restore JoinDeclarationAndInitializer

            ErrorResultTO errors;
            IDynamicServicesInvoker invoker = new DynamicServicesInvoker();
            result = invoker.Invoke(sa.Service, xmlRequest, dataListID, out errors);

            // ReSharper disable InconsistentNaming
            string Expression = sa.BizRule.Expression;
            // ReSharper restore InconsistentNaming

            string[] expressionColumns = sa.BizRule.ExpressionColumns;

            for(int count = 0; count < expressionColumns.Length; count++)
            {
                //Retrieve the value from the response of service
                object inputValue = result.GetValue(expressionColumns[count]);
                //The value does not exist so we stop right here
                if(inputValue.GetType() != typeof(string))
                {
                    _returnVal.Error = string.Format("Unable to execute business rule '{0}'", sa.BizRule.Name);
                    _returnVal.ErrorDetail = string.Format(
                        "Value of '{0}' does not exist in response from action '{1}'", expressionColumns[count], sa.Name);
                    return _returnVal;
                }
                //Build an executable c# expression that we can evaluate this must always evaluate to a boolean value
                //true=input passed the business rule;  false=input failed the business rule.
                
                // ReSharper disable SpecifyACultureInStringConversionExplicitly
                Expression = Expression.Replace("{" + count.ToString() + "}", inputValue.ToString());
                // ReSharper restore SpecifyACultureInStringConversionExplicitly
            }

            #region Evaluate the c# expression dynamically

            try
            {
                object o = Eval(Expression);

                bool bizRuleSuccess;

                bool.TryParse(o.ToString(), out bizRuleSuccess);

                if(!bizRuleSuccess)
                {
                    _returnVal.Error = string.Format("Request Failed Business Rule '{0}'", sa.BizRule.Name);
                    return _returnVal;
                }
            }
            catch(InvalidExpressionException invalidExpressionEx)
            {
                bizRuleException = invalidExpressionEx;
            }
            catch(Exception ex)
            {
                bizRuleException = ex;
            }

            if(bizRuleException != null)
            {
                ExceptionHandling.WriteEventLogEntry(
                    Resources.DynamicService_EventLogTarget
                    , Resources.DynamicService_EventLogSource
                    // ReSharper disable PossibleNullReferenceException
                    , bizRuleException.ToString()
                    // ReSharper restore PossibleNullReferenceException
                    , EventLogEntryType.Error);

                _returnVal.Error = string.Format("Could not evaluate business rule expression {0}", Expression);
                _returnVal.ErrorDetail = bizRuleException.ToString();
                return _returnVal;
            }

            #endregion

            return result;
        }

        #endregion

        #region Invoke Dynamic Service

        /// <summary>
        /// Invoke a Dynamic Service
        /// </summary>
        /// <param name="sa">The action of type InvokeDynamicService</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns>
        /// UnlimitedObject
        /// </returns>
        public Guid DynamicService(ServiceAction sa, Guid dataListID)
        {
            ErrorResultTO errors;
            IDynamicServicesInvoker invoker = new DynamicServicesInvoker();
            // Issue request with blank data payload
            return invoker.Invoke(sa.Service, (new UnlimitedObject()), dataListID, out errors);
        }

        #endregion

        #region Invoke Plugin

        /// <summary>
        /// Invokes a plugin assembly
        /// </summary>
        /// <param name="plugin">The action of type Plugin</param>
        /// <param name="req">The req.</param>
        /// <returns>
        /// Unlimited object
        /// </returns>
        public Guid Plugin(ServiceAction plugin, IDSFDataObject req)
        {
            return Plugin(plugin, req, true);
        }

        /// <summary>
        /// Invokes a plugin assembly
        /// </summary>
        /// <param name="plugin">The action of type Plugin</param>
        /// <param name="req">The req.</param>
        /// <param name="formatOutput">Indicates if the output of the plugin should be run through the formatter</param>
        /// <returns>
        /// Unlimited object
        /// </returns>
        public Guid Plugin(ServiceAction plugin, IDSFDataObject req, bool formatOutput)
        {
            Guid result = GlobalConstants.NullDataListID;
            Guid tmpID = GlobalConstants.NullDataListID;

            var errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();

            try
            {
                AppDomain tmpDomain = plugin.PluginDomain;

                //Instantiate the Remote Oject handler which will allow cross application domain access
                var remoteHandler =
                    (RemoteObjectHandler)
                    tmpDomain.CreateInstanceFromAndUnwrap(typeof(IFrameworkDataChannel).Module.Name,
                                                          typeof(RemoteObjectHandler).ToString());

                var dataBuilder = new StringBuilder("<Args><Args>");
                foreach(ServiceActionInput sai in plugin.ServiceActionInputs)
                {
                    dataBuilder.Append("<Arg>");
                    dataBuilder.Append("<TypeOf>");
                    dataBuilder.Append(sai.NativeType);
                    dataBuilder.Append("</TypeOf>");
                    dataBuilder.Append("<Value>");
                    dataBuilder.Append(sai.Value); // Fetch value and assign
                    dataBuilder.Append("</Value>");
                    dataBuilder.Append("</Arg>");
                }

                dataBuilder.Append("</Args></Args>");

                //xele.Value = (remoteHandler.RunPlugin(plugin.Source.AssemblyLocation, plugin.Source.AssemblyName, plugin.SourceMethod, data));
                string exeValue =
                    (remoteHandler.RunPlugin(plugin.Source.AssemblyLocation, plugin.Source.AssemblyName,
                                             plugin.SourceMethod, dataBuilder.ToString(), plugin.OutputDescription,
                                             formatOutput));

                // TODO : Now create a new dataList and merge the result into the current dataList ;)
                string dlShape = ClientCompiler.ShapeDev2DefinitionsToDataList(plugin.ServiceActionOutputs,
                                                                                enDev2ArgumentType.Output, false,
                                                                                out errors);
                allErrors.MergeErrors(errors);
                tmpID = ClientCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), exeValue, dlShape,
                                                  out errors);
                ClientCompiler.SetParentID(tmpID, req.DataListID); // set parent for merge op in finally...

                allErrors.MergeErrors(errors);

                //Unload the temporary application domain
                //AppDomain.Unload(tmpDomain); -- throws exception when attempting to access after first unload?! -- A strange world C# / Winblows dev is
            }
            catch(Exception ex)
            {
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // handle any errors that might have occured
                IBinaryDataListEntry be = Dev2BinaryDataListFactory.CreateEntry(enSystemTag.Error.ToString(),
                                                                                string.Empty);
                string error;
                be.TryPutScalar(
                    Dev2BinaryDataListFactory.CreateBinaryItem(allErrors.MakeDataListReady(),
                                                               enSystemTag.Error.ToString()), out error);
                errors.AddError(error);
                SvrCompiler.Upsert(null, req.DataListID,
                                    DataListUtil.BuildSystemTagForDataList(enSystemTag.Error, true), be, out errors);

                // now merge and delete tmp
                if(tmpID != GlobalConstants.NullDataListID)
                {
                    // Travis.Frisinger - 29.01.2013 - Bug 8352
                    // We merge here since we never have the shape generated here in the request DL ;)
                    Guid mergeID = ClientCompiler.Merge(req.DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);

                    if(mergeID == GlobalConstants.NullDataListID)
                    {
                        allErrors.AddError("Failed to merge data from Plugin Invoke");
                        allErrors.MergeErrors(errors);
                    }

                    //ClientCompiler.Shape(tmpID, enDev2ArgumentType.Output, plugin.ServiceActionOutputs, out errors);
                    ClientCompiler.DeleteDataListByID(tmpID);
                    result = req.DataListID;
                }
            }

            return result;
        }

        #endregion

        #region Invoke WebService

        /// <summary>
        /// Invokes a web service
        /// </summary>
        /// <param name="service">The action of type InvokeWebService</param>
        /// <param name="xmlRequest">The XML request.</param>
        /// <returns>
        /// UnlimitedObject
        /// </returns>
        public dynamic WebService(ServiceAction service, dynamic xmlRequest)
        {
            //dynamic webServiceException = null;
            dynamic error = new UnlimitedObject();

            if(service.ActionType == enActionType.InvokeWebService)
            {
                string svc = service.Source.Invoker.AvailableServices.FirstOrDefault();

                if(string.IsNullOrEmpty(svc))
                {
                    error.Error = "Web Service not found in dynamic proxy";
                }

                string method = service.SourceMethod;

                var arguments = new List<string>();
                service.ServiceActionInputs.ForEach(c => { if(c.Value != null) arguments.Add(c.Value.ToString()); });

                string[] args = arguments.ToArray();

                string result;
                try
                {
                    // ReSharper disable CoVariantArrayConversion
                    result = service.Source.Invoker.InvokeMethod<string>(svc, method, args);
                    // ReSharper restore CoVariantArrayConversion
                }
                catch(Exception ex)
                {
                    error.Error = "Error Processing Web Service Request";
                    error.ErrorDetail = new UnlimitedObject(ex).XmlString;
                    ExceptionHandling.WriteEventLogEntry("Application",
                                                         string.Format("{0}.{1}", GetType().Name, "WebServiceCommand"),
                                                         error.XmlString, EventLogEntryType.Error);
                    return error;
                }

                return
                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(string.Format("<{0}>{1}</{0}>", "WebServiceResult",
                                                                                    result));
            }
            return new UnlimitedObject().XmlString;
        }

        #endregion

        #region Invoke SqlServer DbCommand

        /// <summary>
        /// Invoke Stored Procedure on Sql Server
        /// </summary>
        /// <param name="serviceAction">Action of type Sql Server</param>
        /// <param name="req">The req.</param>
        /// <returns>
        /// UnlimitedObject
        /// </returns>
        //public dynamic SqlDatabaseCommand(ServiceAction serviceAction, IDSFDataObject req)
        //{
        //    Guid result = GlobalConstants.NullDataListID;
        //    Guid tmpID = GlobalConstants.NullDataListID;

        //    var errors = new ErrorResultTO();
        //    var allErrors = new ErrorResultTO();

        //    using(var cn = new SqlConnection(serviceAction.Source.ConnectionString))
        //    {
        //        var dataset = new DataSet();
        //        try
        //        {
        //            //Create a SqlCommand to execute at the source
        //            var cmd = new SqlCommand();
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Connection = cn;
        //            cmd.CommandText = serviceAction.SourceMethod;
        //            cmd.CommandTimeout = serviceAction.CommandTimeout;

        //            //Add the parameters to the SqlCommand
        //            if(serviceAction.ServiceActionInputs.Count() > 0)
        //            {
        //                foreach(ServiceActionInput sai in serviceAction.ServiceActionInputs)
        //                {
        //                    var injectVal = (string)sai.Value;

        //                    // 16.10.2012 : Travis.Frisinger - Convert empty to null
        //                    if(sai.EmptyToNull && injectVal == AppServerStrings.NullConstant)
        //                    {
        //                        cmd.Parameters.AddWithValue(sai.Source, DBNull.Value);
        //                    }
        //                    else
        //                    {
        //                        cmd.Parameters.AddWithValue(sai.Source, sai.Value);
        //                    }
        //                }
        //            }

        //            cn.Open();
        //            var xmlDbResponse = new StringBuilder();

        //            var adapter = new SqlDataAdapter(cmd);
        //            adapter.Fill(dataset);

        //            string res =
        //                DataSanitizerFactory.GenerateNewSanitizer(enSupportedDBTypes.MSSQL)
        //                                    .SanitizePayload(dataset.GetXml());

        //            xmlDbResponse.Append(res);

        //            cn.Close();

        //            //Alert the caller that request returned no data
        //            if(string.IsNullOrEmpty(xmlDbResponse.ToString()))
        //            {
        //                // handle any errors that might have occured
        //                IBinaryDataListEntry be = Dev2BinaryDataListFactory.CreateEntry(enSystemTag.Error.ToString(),
        //                                                                                string.Empty);
        //                string error;
        //                be.TryPutScalar(
        //                    Dev2BinaryDataListFactory.CreateBinaryItem(
        //                        "The request yielded no response from the data store.", enSystemTag.Error.ToString()),
        //                    out error);
        //                if(error != string.Empty)
        //                {
        //                    errors.AddError(error);
        //                }
        //                SvrCompiler.Upsert(null, req.DataListID,
        //                                    DataListUtil.BuildSystemTagForDataList(enSystemTag.Error, true), be,
        //                                    out errors);
        //            }
        //            else
        //            {
        //                string tmpData = xmlDbResponse.ToString();
        //                string od = serviceAction.OutputDescription;

        //                od = od.Replace("<Dev2XMLResult>", "").Replace("</Dev2XMLResult>", "").Replace("<JSON />", "");

        //                if(!string.IsNullOrWhiteSpace(od))
        //                {
        //                    IOutputDescriptionSerializationService outputDescriptionSerializationService =
        //                        OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService
        //                            ();
        //                    IOutputDescription outputDescriptionInstance =
        //                        outputDescriptionSerializationService.Deserialize(od);

        //                    if(outputDescriptionInstance != null)
        //                    {
        //                        IOutputFormatter outputFormatter =
        //                            OutputFormatterFactory.CreateOutputFormatter(outputDescriptionInstance);
        //                        string formatedPayload = outputFormatter.Format(tmpData).ToString();
        //                        // TODO : Now create a new dataList and merge the result into the current dataList ;)
        //                        string dlShape =
        //                            ClientCompiler.ShapeDev2DefinitionsToDataList(serviceAction.ServiceActionOutputs,
        //                                                                           enDev2ArgumentType.Output, false,
        //                                                                           out errors);
        //                        allErrors.MergeErrors(errors);
        //                        tmpID = ClientCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
        //                                                          formatedPayload, dlShape, out errors);
        //                        var parentID = ClientCompiler.FetchParentID(req.DataListID);
        //                        //ClientCompiler.SetParentID(tmpID, req.DataListID);
        //                        ClientCompiler.SetParentID(tmpID, parentID);
        //                        // set parent for merge op in finally...

        //                        allErrors.MergeErrors(errors);
        //                    }
        //                }
        //            }

        //            cmd.Dispose();
        //        }
        //        catch(Exception ex)
        //        {
        //            allErrors.AddError(ex.Message);
        //        }
        //        finally
        //        {
        //            // handle any errors that might have occured
        //            IBinaryDataListEntry be = Dev2BinaryDataListFactory.CreateEntry(enSystemTag.Error.ToString(),
        //                                                                            string.Empty);
        //            string error;
        //            be.TryPutScalar(
        //                Dev2BinaryDataListFactory.CreateBinaryItem(allErrors.MakeDataListReady(),
        //                                                           enSystemTag.Error.ToString()), out error);
        //            if(error != string.Empty)
        //            {
        //                errors.AddError(error);
        //            }
        //            SvrCompiler.Upsert(null, req.DataListID,
        //                                DataListUtil.BuildSystemTagForDataList(enSystemTag.Error, true), be, out errors);

        //            // now merge and delete tmp
        //            if(tmpID != GlobalConstants.NullDataListID)
        //            {
        //                ClientCompiler.Shape(tmpID, enDev2ArgumentType.Output, serviceAction.ServiceActionOutputs,
        //                                      out errors);
        //                ClientCompiler.DeleteDataListByID(tmpID);
        //                result = req.DataListID;
        //            }

        //            //ExceptionHandling.WriteEventLogEntry("Application", string.Format("{0}.{1}", this.GetType().Name, "SqlDatabaseCommand"), string.Format("Exception:{0}\r\nInputData:{1}", xmlResponse.XmlString, xmlRequest.XmlString), EventLogEntryType.Error);
        //        }


        //        return result;
        //    }
        //}

        public dynamic SqlDatabaseCommand(ServiceAction serviceAction, IDSFDataObject req)
        {
            Guid result = GlobalConstants.NullDataListID;
            Guid tmpID = GlobalConstants.NullDataListID;

            var errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();

                try
                {
                // Get XAML data from service action
                string xmlDbResponse = GetXmlDataFromSqlServiceAction(serviceAction);

                if (string.IsNullOrEmpty(xmlDbResponse))
                    {
                    // If there was no data returned add error
                    allErrors.AddError("The request yielded no response from the data store.");
                            }
                            else
                            {
                    // Get the output formatter from the service action
                    IOutputFormatter outputFormatter = GetOutputFormatterFromServiceAction(serviceAction);
                    if (outputFormatter == null)
                        {
                        // If there was an error getting the output formatter from the service action
                        allErrors.AddError(string.Format("Output format in service action {0} is invalid.", serviceAction.Name));
                    }
                    else
                    {
                        // Format the XML data
                        string formatedPayload = outputFormatter.Format(xmlDbResponse).ToString();

                        // Create a shape from the service action outputs
                        string dlShape = ClientCompiler.ShapeDev2DefinitionsToDataList(serviceAction.ServiceActionOutputs, enDev2ArgumentType.Output, false, out errors);
                        allErrors.MergeErrors(errors);

                        // Push formatted data into a datalist using the shape from the service action outputs
                        tmpID = ClientCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), formatedPayload, dlShape, out errors);
                                allErrors.MergeErrors(errors);

                        // Attach a parent ID to the newly created datalist
                                var parentID = ClientCompiler.FetchParentID(req.DataListID);
                                ClientCompiler.SetParentID(tmpID, parentID);
                        }
                    }
                }
            catch (Exception ex)
                {
                    allErrors.AddError(ex.Message);
                }
                finally
                {
                // If a datalist was ceated
                if (tmpID != GlobalConstants.NullDataListID)
                    {
                    // Merge into it's parent
                    ClientCompiler.Shape(tmpID, enDev2ArgumentType.Output, serviceAction.ServiceActionOutputs, out errors);
                    allErrors.MergeErrors(errors);

                    // Delete data list
                        ClientCompiler.DeleteDataListByID(tmpID);
                        result = req.DataListID;
                    }

                // Add any errors that occured to the datalist
                AddErrorsToDataList(allErrors, req.DataListID);
                }

                return result;
            }

        #endregion

        #region Invoke Management Method

        /// <summary>
        /// Invoke a management method which is a statically coded method in the service implementation for service engine administrators
        /// </summary>
        /// <param name="serviceAction">Action of type InvokeManagementDynamicService</param>
        /// <param name="xmlRequest">The XML request.</param>
        /// <returns>
        /// UnlimitedObject
        /// </returns>
        public Guid  ManagementDynamicService(ServiceAction serviceAction, IDSFDataObject xmlRequest)
        {
            var errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            Guid result = GlobalConstants.NullDataListID;

            try
            {
                object[] parameterValues = null;
                //Instantiate a Endpoint object that contains all static management methods
                object o = this;
                //Activator.CreateInstance(typeof(Unlimited.Framework.DynamicServices.DynamicServicesEndpoint), new object[] {string.Empty});

                //Get the management method
                MethodInfo m = o.GetType().GetMethod(serviceAction.SourceMethod);
                //Infer the parameters of the management method
                ParameterInfo[] parameters = m.GetParameters();
                //If there are parameters then retrieve them from the service action input values
                if(parameters.Count() > 0)
                {
                    IEnumerable<object> parameterData = from c in serviceAction.ServiceActionInputs
                                                        select c.Value;

                    parameterValues = parameterData.ToArray();
                }
                //Invoke the management method and store the return value
                string val = m.Invoke(o, parameterValues).ToString();

                result = ClientCompiler.UpsertSystemTag(xmlRequest.DataListID, enSystemTag.ManagmentServicePayload, val,
                                                         out errors);

                //_clientCompiler.Upsert(xmlRequest.DataListID, DataListUtil.BuildSystemTagForDataList(enSystemTag.ManagmentServicePayload, true), val, out errors);
                allErrors.MergeErrors(errors);

                //returnval = new UnlimitedObject(GetXElement(val));
            }
            catch(Exception ex)
            {
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // handle any errors that might have occured

                if(allErrors.HasErrors())
                {
                    IBinaryDataListEntry be = Dev2BinaryDataListFactory.CreateEntry(enSystemTag.Error.ToString(),
                                                                                    string.Empty);
                    string error;
                    be.TryPutScalar(
                        Dev2BinaryDataListFactory.CreateBinaryItem(allErrors.MakeDataListReady(),
                                                                   enSystemTag.Error.ToString()), out error);
                    if(error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                }

                // No cleanup to happen ;)
            }

            return result;
        }

        #endregion

        #region GetXElement Method

        /// <summary>
        /// A utility method to create an Unlimited Object from an xml string
        /// </summary>
        /// <param name="xmlRequest">The XML request.</param>
        /// <returns></returns>
        public dynamic GetXElement(string xmlRequest)
        {
            try
            {
                return XElement.Parse(xmlRequest);
            }
            catch(Exception ex)
            {
                return new XElement("XmlData", new XElement("Error", ex.Message));
            }
        }

        #endregion

        #region Expression Validator

        /// <summary>
        ///     Evaluates a string of C# code and returns the result.
        /// </summary>
        /// <param name="sExpression">Valid C# code</param>
        /// <returns>object</returns>
        private object Eval(string sExpression)
        {
            //Here we compiling user code at run time so 
            //we need to instantiate a C# code provder to compile the source code 
            //expression being passed in. Since we can't compile and expression
            //in isolatio we need to generate a type library

            var c = new CSharpCodeProvider();
            var cp = new CompilerParameters();

            //We need to add a reference to the system assembly
            //to create a valid runtime object
            cp.ReferencedAssemblies.Add("system.dll");
            //We are compiling the source into a class library
            cp.CompilerOptions = "/t:library";
            //We are specifying that we want the library generated in memory
            cp.GenerateInMemory = true;

            //Build the runtime class to be compiled and inject the user expression
            //into the code body
            var sb = new StringBuilder("");
            sb.Append("using System;\n");

            sb.Append("namespace CSCodeEvaler{ \n");
            sb.Append("public class CSCodeEvaler{ \n");
            sb.Append("public object EvalCode(){\n");
            sb.Append("return " + sExpression + "; \n");
            sb.Append("} \n");
            sb.Append("} \n");
            sb.Append("}\n");

            //Compile the user code and report on any errors
            CompilerResults cr = c.CompileAssemblyFromSource(cp, sb.ToString());
            if(cr.Errors.Count > 0)
            {
                throw new InvalidExpressionException(
                    string.Format("Error ({0}) evaluating: {1}",
                    cr.Errors[0].ErrorText, sExpression));
            }

            //Instantiate the newly create c# assembly
            Assembly a = cr.CompiledAssembly;
            object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");

            if (o != null)
            {
                Type t = o.GetType();
                MethodInfo mi = t.GetMethod("EvalCode");

                //Invoke the method and return the results to the caller
                object s = mi.Invoke(o, null);
                return s;
            }

            return null;
        }

        #endregion

        #region TODO: Add MySql support

        public dynamic MySqlDatabase(ServiceAction serviceAction, dynamic xmlRequest)
        {
            dynamic result = "Not supported";
            return result;
        }

        #endregion

        #region Evaluate Switch and execute resulting actions

        public dynamic Switch(ServiceAction serviceAction, dynamic xmlRequest, Guid dataListID)
        {
            dynamic result = new UnlimitedObject();

            DynamicService anonymousService = new DynamicService();
            if(!string.IsNullOrEmpty(serviceAction.Cases.DataElementValue))
            {
                List<ServiceAction> anonymousServiceActions;

                IEnumerable<ServiceActionCase> caseMatch =
                    serviceAction.Cases.Cases.Where(c => Regex.IsMatch(serviceAction.Cases.DataElementValue, c.Regex));
                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                // ReSharper disable PossibleMultipleEnumeration
                if(caseMatch.Any())
                // ReSharper restore PossibleMultipleEnumeration
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                {
                // ReSharper disable PossibleMultipleEnumeration
                    anonymousServiceActions = caseMatch.First().Actions;
                // ReSharper restore PossibleMultipleEnumeration
                }
                else
                {
                    anonymousServiceActions = serviceAction.Cases.DefaultCase.Actions;
                }


                anonymousService.Name = string.Format("serviceOf{0}", serviceAction.Name);
                anonymousService.Actions = anonymousServiceActions;

                IDynamicServicesInvoker invoker = new DynamicServicesInvoker();
                ErrorResultTO errors;
                result = invoker.Invoke(anonymousService, xmlRequest, dataListID, out errors);
            }

            return result;
        }

        #endregion

        #endregion
        
        #region Management Methods

        #region InvokeService

        public string InvokeService(string name, string action, string args)
        {
            const string servicesAssemblyName = "Dev2.Runtime.Services.{0}, Dev2.Runtime.Services";

            try
            {
                var serviceType = Type.GetType(string.Format(servicesAssemblyName, name));
                if(serviceType != null)
                {
                    var method = serviceType.GetMethod(action);
                    if(method != null)
                    {
                        var service = method.IsStatic ? null : Activator.CreateInstance(serviceType);
                        var actionResult = method.Invoke(service, new object[] { args });
                        if(actionResult != null)
                        {
                            return actionResult.ToString();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                var error = new ErrorResultTO();
                error.AddError(ex.Message);
                return error.MakeUserReady();
            }
            return string.Empty;
        }

        #endregion

        #region FindSourcesByType

        // PBI 6597: TWR
        public string FindSourcesByType(string type)
        {
            if(string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type");
            }

            enSourceType sourceType;
            if(!Enum.TryParse(type, true, out sourceType))
            {
                sourceType = enSourceType.Unknown;
            }

            Host.LockSources();
            var resources = new List<DynamicServiceObjectBase>();
            try
            {
                resources.AddRange(Host.Sources.Where(c => c.Type == sourceType));
            }
            finally
            {
                Host.UnlockSources();
            }
            dynamic returnData = new UnlimitedObject();
            resources.ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
            return returnData.XmlString;
        }

        #endregion

        #region FindSourcesByID

        // PBI 6597: TWR - only implemented for sources
        public string FindResourcesByID(string guidCsv, string type)
        {
            if(guidCsv == null)
            {
                throw new ArgumentNullException("guidCsv");
            }
            if(type == null)
            {
                throw new ArgumentNullException("type");
            }

            var guids = new List<Guid>();
            foreach(var guidStr in guidCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Guid guid;
                if(Guid.TryParse(guidStr, out guid))
                {
                    guids.Add(guid);
                }
            }

            var resources = new List<DynamicServiceObjectBase>();
            if(guids.Count > 0)
            {
                switch(type)
                {
                    default: // Sources
                        Host.LockSources();
                        try
                        {
                            resources.AddRange(Host.Sources.Where(c => guids.Contains(c.ID)));
                        }
                        finally
                        {
                            Host.UnlockSources();
                        }
                        break;
                }
            }

            dynamic returnData = new UnlimitedObject();
            resources.ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
            return returnData.XmlString;
        }

        #endregion

        #region UpdateWorkspaceItem

        // 2012.10.18 - 5782: TWR
        public string UpdateWorkspaceItem(string itemXml, string roles)
        {
            dynamic xmlResponse = new UnlimitedObject(Resources.DynamicService_ServiceResponseTag);
            if(string.IsNullOrEmpty(itemXml))
            {
                xmlResponse.Error = "Invalid workspace item definition";
            }
            else
            {
                try
                {
                    var workspaceItem = new WorkspaceItem(XElement.Parse(itemXml));
                    if(workspaceItem.WorkspaceID != _workspace.ID)
                    {
                        xmlResponse.Error = "Cannot update a workspace item from another workspace";
                    }
                    else
                    {
                        _workspace.Update(workspaceItem, roles);
                        xmlResponse.Response = "Workspace item updated";
                    }
                }
                catch(Exception ex)
                {
                    xmlResponse.Error = "Error updating workspace item";
                    xmlResponse.ErrorDetail = ex.Message;
                    xmlResponse.ErrorStackTrace = ex.StackTrace;
                }
            }

            return xmlResponse.XmlString;
        }

        #endregion

        #region GetLatest

        // 2012.10.18 - 5782: TWR
        public string GetLatest(string editedItemsXml)
        {
            dynamic xmlResponse = new UnlimitedObject(Resources.DynamicService_ServiceResponseTag);
            try
            {
                var editedItems = new List<string>();

                if(!string.IsNullOrWhiteSpace(editedItemsXml))
                {
                    editedItems.AddRange(XElement.Parse(editedItemsXml)
                        .Elements()
                        .Select(x => x.Attribute("ServiceName").Value));
                }

                WorkspaceRepository.Instance.GetLatest(_workspace, editedItems);
                xmlResponse.Response = "Workspace updated";
            }
            catch(Exception ex)
            {
                xmlResponse.Error = "Error updating workspace";
                xmlResponse.ErrorDetail = ex.Message;
                xmlResponse.ErrorStackTrace = ex.StackTrace;
            }

            return xmlResponse.XmlString;
        }

        #endregion

        // ReSharper disable InconsistentNaming
        private List<string> visitedServices;
        // ReSharper restore InconsistentNaming

        /// <summary>
        ///     Simple ping test that returns the date the message was processed back to the caller
        /// </summary>
        /// <returns>string</returns>
        public string PingTest()
        {
            dynamic returnData = new UnlimitedObject();

            returnData.Pong = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
            //returnData.TimeStamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");

            return returnData.XmlString;
        }

        /// <summary>
        /// This is a management method that allows administrative users to realod a specific service
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        public string ReloadResource(string resourceName, string resourceType)
        {
            dynamic xmlResponse = new UnlimitedObject(Resources.DynamicService_ServiceResponseTag);

            try
            {
                // 2012.10.01: TWR - 5392 - Server does not dynamically reload resources 
                if(resourceName == "*")
                {
                    Host.RestoreResources(new[] { "Sources", "Services" });
                }
                else
                {
                    //
                    // Ugly conversion between studio resource type and server resource type
                    //
                    enDynamicServiceObjectType serviceType;
                    string directory;
                    if (resourceType == "WorkflowService" ||
                        resourceType == "Website" || resourceType == "HumanInterfaceProcess")
                    {
                        directory = "Services";
                        serviceType = enDynamicServiceObjectType.WorkflowActivity;
                    }
                    else if (resourceType == "Service")
                    {
                        directory = "Services";
                        serviceType = enDynamicServiceObjectType.DynamicService;
                    }
                    else if (resourceType == "Source")
                    {
                        directory = "Sources";
                        serviceType = enDynamicServiceObjectType.Source;
                    }
                    else
                    {
                        throw new Exception("Unexpected resource type '" + resourceType + "'.");
                    }

                    //
                    // Copy the file from the server workspace into the current workspace
                    //
                    _workspace.Update(new WorkspaceItem(_workspace.ID, HostSecurityProvider.Instance.ServerID)
                        {
                            Action = WorkspaceItemAction.Edit,
                            ServiceName = resourceName,
                            ServiceType = serviceType.ToString()
                        });
                    
                    //
                    // Reload resources
                    //
                    Host.RestoreResources(new[] { directory }, resourceName);
                }
                xmlResponse.Response = string.Concat("'", resourceName, "' Reloaded...");
            }
            catch(Exception ex)
            {
                xmlResponse.Error = string.Concat("Error reloading '", resourceName, "'...");
                xmlResponse.ErrorDetail = ex.Message;
                xmlResponse.ErrorStackTrace = ex.StackTrace;
            }

            return xmlResponse.XmlString;
        }

        public string DeleteResource(string resourceName, string type, string roles)
        {
            dynamic returnData = new UnlimitedObject();

            if(resourceName == "*")
            {
                returnData.Error = "Delete resources does not accept wildcards.";
            }
            else
            {
                switch(type)
                {
                    case "WorkflowService":
                        {
                            IEnumerable<DynamicService> services;

                            Host.LockServices();

                            try
                            {
                                services = Host.Services
                                    .Where(c => c.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));
                            }
                            finally
                            {
                                Host.UnlockServices();
                            }

                            //IEnumerable<DynamicService> workflowServices =
                            //    services.Where(
                            //        c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0)
                            //            .Where(
                            //                c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));

                            IEnumerable<DynamicService> workflowServices =
                                services.Where(
                                    c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Any());
                            List<DynamicService> match = workflowServices.ToList();

                            if(match.Count != 1)
                            {
                                if(match.Count == 0)
                                {
                                    returnData.Error = "WorkflowService \"" + resourceName + "\" was not found.";
                                }
                                else
                                {
                                    returnData.Error = "Multiple matches found for WorkflowService \"" + resourceName +
                                                       "\".";
                                }
                            }
                            else
                            {
                                // ReSharper disable RedundantArgumentDefaultValue
                                returnData = Host.RemoveDynamicService(match[0], roles, true);
                                // ReSharper restore RedundantArgumentDefaultValue
                            }

                            //workflowServices.ToList().ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                            break;
                        }

                    case "Service":
                        {
                            IEnumerable<DynamicService> svc;

                            Host.LockServices();

                            try
                            {
                                svc = Host.Services.Where(c => c.Name.Contains(resourceName));
                            }
                            finally
                            {
                                Host.UnlockServices();
                            }

                            IEnumerable<DynamicService> svcs =
                                // ReSharper disable UseMethodAny.0
                                // ReSharper disable ReplaceWithSingleCallToCount
                                svc.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                                // ReSharper restore ReplaceWithSingleCallToCount
                                // ReSharper restore UseMethodAny.0
                                   //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                            List<DynamicService> match = svcs.ToList();

                            if(match.Count != 1)
                            {
                                if(match.Count == 0)
                                {
                                    returnData.Error = "Service \"" + resourceName + "\" was not found.";
                                }
                                else
                                {
                                    returnData.Error = "Multiple matches found for Service \"" + resourceName + "\".";
                                }
                            }
                            else
                            {
                                // ReSharper disable RedundantArgumentDefaultValue
                                returnData = Host.RemoveDynamicService(match[0], roles, true);
                                // ReSharper restore RedundantArgumentDefaultValue
                            }


                            //svcs.ToList().ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                            break;
                        }

                    case "Source":
                        {
                            IEnumerable<Source> sources;
                            Host.LockSources();

                            try
                            {
                                //Juries - Bug cant delete resources when more than one contains the name
                                //Shoot me if everything uses a fuzzy lookup.
                                sources =
                                    Host.Sources.Where(c => c.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));
                                        //.Where(
                                            //c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                            }
                            finally
                            {
                                Host.UnlockSources();
                            }

                            List<Source> match = sources.ToList();

                            if(match.Count != 1)
                            {
                                if(match.Count == 0)
                                {
                                    returnData.Error = "Source \"" + resourceName + "\" was not found.";
                                }
                                else
                                {
                                    returnData.Error = "Multiple matches found for Source \"" + resourceName + "\".";
                                }
                            }
                            else
                            {
                                // ReSharper disable RedundantArgumentDefaultValue
                                returnData = Host.RemoveSource(match[0], roles, true);
                                // ReSharper restore RedundantArgumentDefaultValue
                            }

                            //sources.ToList().ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                            break;
                        }
                }
                
            }

            return returnData.XmlString;
        }

        /// <summary>
        ///     This is a management method and allows administrative
        ///     users to reload services.
        ///     This will be called when service definitions have
        ///     been ammended and need to become live
        /// </summary>
        /// <returns>string containing the response(s) to the request</returns>
        public string ReloadServices()
        {
            dynamic xmlResponse = new UnlimitedObject(Resources.DynamicService_ServiceResponseTag);

            try
            {
                Host.RestoreResources(new[] { "Sources", "Services", "ActivityDefs" });
                xmlResponse.Response = "All Services Reloaded...";
            }
            catch(Exception ex)
            {
                xmlResponse.Error = "Error reloading services...";
                xmlResponse.ErrorDetail = ex.Message;
                xmlResponse.ErrorStackTrace = ex.StackTrace;
            }

            return xmlResponse.XmlString;
        }


        public string FindDependencies(string resourceName)
        {
            visitedServices = new List<string>();
            string result = string.Format("<graph title=\"Dependency Graph Of {0}\">", resourceName) +
                            FindDependenciesRecursive(resourceName) + "</graph>";


            return UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result).XmlString;
        }


        public string FindDependenciesRecursive(string resourceName)
        {
            var sb = new StringBuilder();
            var reverseDependencyList = new List<string>();
            var brokenDependencies = new HashSet<string>();

            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceName));


            bool isService = false;
            IEnumerable<DynamicService> services;

            Host.LockServices();

            try
            {
                // ReSharper disable ImplicitlyCapturedClosure
                services = Host.Services.Where(c => c.Name == resourceName);
                // ReSharper restore ImplicitlyCapturedClosure
            }
            finally
            {
                Host.UnlockServices();
            }

            // ReSharper disable PossibleMultipleEnumeration
            if(services.Any())
            // ReSharper restore PossibleMultipleEnumeration
            {
                // ReSharper disable PossibleMultipleEnumeration
                foreach(DynamicService item in services)
                // ReSharper restore PossibleMultipleEnumeration
                {
                    item.Actions.ForEach(c => c.ServiceActionInputs.ForEach(d =>
                                                                            sb.Append(
                                                                                string.Format("<input name=\"{0}\" />"
                        , d.Name))));
                }

                isService = true;
            }

            //This is a service so we can find all reverse dependencies
            // i.e these are items that need this service
            if(isService)
            {
                IEnumerable<DynamicService> reverseDependencies;
                Host.LockServices();

                try
                {
                    reverseDependencies =
                        // ReSharper disable ImplicitlyCapturedClosure
                        // ReSharper disable UseMethodAny.0
                        // ReSharper disable ReplaceWithSingleCallToCount
                        Host.Services.Where(c => c.Actions.Where(d => d.ServiceName == resourceName).Count() > 0);
                        // ReSharper restore ReplaceWithSingleCallToCount
                        // ReSharper restore UseMethodAny.0
                        // ReSharper restore ImplicitlyCapturedClosure
                }
                finally
                {
                    Host.UnlockServices();
                }

                reverseDependencies.ToList().ForEach(c => { reverseDependencyList.Add(c.Name); });

                reverseDependencies.ToList().ForEach(c =>
                {
                    c.Actions.ForEach(e =>
                    {
                        var activity = e.WorkflowActivity as DynamicActivity;
                        if(activity != null)
                        {
                            Activity flowChart;
                            //flowChart = activity.Implementation.Invoke();

                            try
                            {
                                flowChart = activity.Implementation.Invoke();
                            }
                            catch(XamlObjectWriterException)
                            {
                                flowChart = null;

                                if(!reverseDependencyList.Contains(activity.Name))
                                {
                                    reverseDependencyList.Add(activity.Name);
                                    brokenDependencies.Add(activity.Name);
                                }

                                // This exception occurs due to invalid workflows that still reference the old ForEach activity. (AdditionalData)
                            }


                            var workflow = flowChart as Flowchart;
                            if(workflow != null)
                            {
                                //Activities that are used by this resource
                                foreach(dynamic fn in workflow.Nodes)
                                {
                                    if((fn is FlowStep) && fn.Action.GetType().Name == "DsfActivity")
                                    {
                                        if(fn.Action.ServiceName == resourceName)
                                        {
                                            reverseDependencyList.Add(c.Name);
                                        }
                                    }
                                }
                            }
                        }
                    });
                });
            }

            IEnumerable<Source> sources;
            Host.LockSources();

            try
            {
                // ReSharper disable ImplicitlyCapturedClosure
                sources = Host.Sources.Where(c => c.Name == resourceName);
                // ReSharper restore ImplicitlyCapturedClosure
            }
            finally
            {
                Host.UnlockSources();
            }

            //Source Resource Name was passed in 
            //find all services that 
            var serviceUsingSource = new List<string>();
            // ReSharper disable PossibleMultipleEnumeration
            if(sources.Any())
            // ReSharper restore PossibleMultipleEnumeration
            {
                Host.LockServices();

                try
                {
                    // ReSharper disable PossibleMultipleEnumeration
                    sources.ToList().ForEach(e =>
                    // ReSharper restore PossibleMultipleEnumeration
                                             Host.Services.ForEach(c => c.Actions.ForEach(d =>
                            {
                                if(d.SourceName == e.Name)
                                {
                                    serviceUsingSource.Add(c.Name);
                                }
                            })));
                }
                finally
                {
                    Host.UnlockServices();
                }

                serviceUsingSource.ForEach(c => sb.Append(string.Format("<dependency id=\"{0}\" />", c)));
            }


            IEnumerable<DynamicService> workflowServices =
                // ReSharper disable UseMethodAny.0
                // ReSharper disable PossibleMultipleEnumeration
                services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                // ReSharper restore PossibleMultipleEnumeration
                // ReSharper restore UseMethodAny.0


            var svcNames = new List<string>();
            workflowServices.ToList().ForEach(resource =>
            {
                IEnumerable<ServiceAction> workflowMatch =
                    resource.Actions.Where(d => d.ActionType == enActionType.Workflow);
                // ReSharper disable PossibleMultipleEnumeration
                if(workflowMatch.Any())
                // ReSharper restore PossibleMultipleEnumeration
                {
                    // ReSharper disable PossibleMultipleEnumeration
                    workflowMatch.ToList().ForEach(e =>
                    // ReSharper restore PossibleMultipleEnumeration
                    {
                        var activity = e.WorkflowActivity as DynamicActivity;
                        if(activity != null)
                        {
                            Activity flowChart;


                            try
                            {
                                flowChart = activity.Implementation.Invoke();
                            }
                            catch(XamlObjectWriterException)
                            {
                                flowChart = null;
                                // This exception occurs due to invalid workflows that still reference the old ForEach activity. (AdditionalData)
                            }


                            var workflow = flowChart as Flowchart;
                            if(workflow != null)
                            {
                                foreach(dynamic fn in workflow.Nodes)
                                {
                                    if((fn is FlowStep) && fn.Action.GetType().Name == "DsfActivity")
                                    {
                                        if(!svcNames.Contains(fn.Action.ServiceName))
                                        {
                                            svcNames.Add(fn.Action.ServiceName);
                                        }
                                        sb.Append(string.Format("<dependency id=\"{0}\" />",
                                                                fn.Action.ServiceName));
                                    }
                                }
                            }
                        }
                    });
                }
            });


            IEnumerable<DynamicService> dsfServices;
            Host.LockServices();

            try
            {
                // ReSharper disable ImplicitlyCapturedClosure
                dsfServices = Host.Services.Where(c => c.Actions.Any(d => d.SourceName == resourceName));
                // ReSharper restore ImplicitlyCapturedClosure
            }
            finally
            {
                Host.UnlockServices();
            }

            dsfServices.ToList().ForEach(c => { if(!svcNames.Contains(c.Name)) svcNames.Add(c.Name); });


            sb.Append("</node>");

            if(serviceUsingSource.Count() > 0)
            {
                serviceUsingSource.ForEach(
                    c => sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\"/>", c)));
            }

            if(reverseDependencyList.Count > 0)
            {
                reverseDependencyList.ForEach(c =>
                {
                    sb.Append(
                        string.Format(
                            "<node id=\"{0}\" x=\"\" y=\"\" broken=\"" +
                            (brokenDependencies.Contains(c) ? "true" : "false") + "\">", c));
                    sb.Append(string.Format("<dependency id=\"{0}\" />", resourceName));
                    sb.Append("</node>");
                });
            }

            visitedServices.Add(resourceName);


            IEnumerable<string> circularref = visitedServices.Intersect(svcNames).ToList();
            if(circularref.Count() > 0)
            {
                var circ = new StringBuilder();
                circularref.ToList().ForEach(s => circ.Append(string.Format("<dep>{0}</dep>", s)));

                //Jurie.Smit removed - circular references allowed, should be graphed
                //throw new Exception(string.Format("Circular Reference Detected between {0} and {1}!", resourceName, circ));
            }

            svcNames.Except(circularref).ToList().ForEach(c => sb.Append(FindDependenciesRecursive(c)));

            return sb.ToString();
        }

        public string InterrogatePlugin(string assemblyLocation, string assemblyName, string method, string args)
        {
            AppDomain tmpDomain = AppDomain.CreateDomain("PluginInterrogator");

            var remoteHandler =
                (RemoteObjectHandler)
                tmpDomain.CreateInstanceFromAndUnwrap(typeof(IFrameworkDataChannel).Module.Name,
                                                      typeof(RemoteObjectHandler).ToString());
            string result = remoteHandler.InterrogatePlugin(assemblyLocation, assemblyName, method, args);
            result = string.Concat("<InterrogationResult>", result, "</InterrogationResult>");
            AppDomain.Unload(tmpDomain);

            return result;
        }

        // Travis.Frisinger : 09.08.2012 - List service data in a manor we can bind from it ;)
        public string FindResourceForBinding(string resourceName, string type, string roles)
        {
            dynamic returnData = new UnlimitedObject();

            if(resourceName == "*")
            {
                resourceName = string.Empty;
            }

            switch(type)
            {
                case "WorkflowService":
                    {
                        IEnumerable<DynamicService> services;
                        Host.LockServices();

                        try
                        {
                            services = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> workflowServices =
                            services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));

                        workflowServices.ToList()
                                        .ForEach(
                                            c =>
                                            returnData.AddResponse(
                                                UnlimitedObject.GetStringXmlDataAsUnlimitedObject(
                                                    c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }
                case "Service":
                    {
                        IEnumerable<DynamicService> svc;
                        Host.LockServices();

                        try
                        {
                            svc = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }


                        IEnumerable<DynamicService> svcs =
                            // ReSharper disable UseMethodAny.0
                            // ReSharper disable ReplaceWithSingleCallToCount
                        svc.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                        // ReSharper restore ReplaceWithSingleCallToCount
                        // ReSharper restore UseMethodAny.0
                               //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        svcs.ToList()
                            .ForEach(
                                c =>
                                returnData.AddResponse(
                                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }

                //case "Source":

                //    var sources = Host.Sources.Where(c => c.Name.Contains(resourceName)).Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                //    sources.ToList().ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                //break;

                //case "Activity":
                //    var activitydefs = Host.WorkflowActivityDefs.Where(c => c.Name.Contains(resourceName)).Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                //    activitydefs.ToList().ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                //break;
            }
            

            // now extract the attributed junk for the server to properly use
            var attributes = new[] { "Name" };
            var childTags = new[] { "Category" };
            string returnValue = DataListUtil.ExtractAttributeFromTagAndMakeRecordset(returnData.XmlString, "Service",
                                                                                      attributes, childTags);
            returnValue = returnValue.Replace("<Service>", "<Dev2Service>").Replace("</Service>", "</Dev2Service>");

            return returnValue;
        }

        public string GetResource(string resourceName, string resourceType, string roles)
        {
            dynamic returnData = new UnlimitedObject();

            switch(resourceType)
            {
                case "WorkflowService":
                    {
                        IEnumerable<DynamicService> services;
                        Host.LockServices();

                        try
                        {
                            services = Host.Services.Where(c => c.Name == resourceName);
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> workflowServices =
                            // ReSharper disable UseMethodAny.0
                            services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                            // ReSharper restore UseMethodAny.0
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        workflowServices.ToList()
                                        .ForEach(
                                            c =>
                                            returnData.AddResponse(
                                                UnlimitedObject.GetStringXmlDataAsUnlimitedObject(
                                                    c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }
                case "Service":
                    {
                        IEnumerable<DynamicService> svc;
                        Host.LockServices();

                        try
                        {
                            svc = Host.Services.Where(c => c.Name == resourceName);
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> svcs =
                            svc.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                               //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        svcs.ToList()
                            .ForEach(
                                c =>
                                returnData.AddResponse(
                                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }

                case "Source":
                    {
                        IEnumerable<Source> sources;
                        Host.LockSources();

                        try
                        {
                            sources =
                                Host.Sources.Where(c => c.Name == resourceName);
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        }
                        finally
                        {
                            Host.UnlockSources();
                        }

                        sources.ToList()
                               .ForEach(
                                   c =>
                                   returnData.AddResponse(
                                       UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                         "<NoDef/>")));
                        break;
                    }
                case "Activity":
                    {
                        IEnumerable<WorkflowActivityDef> activitydefs;
                        Host.LockActivities();

                        try
                        {
                            activitydefs =
                                Host.WorkflowActivityDefs.Where(c => c.Name == resourceName);
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        }
                        finally
                        {
                            Host.UnlockActivities();
                        }

                        activitydefs.ToList()
                                    .ForEach(
                                        c =>
                                        returnData.AddResponse(
                                            UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                              "<NoDef/>")));
                        break;
                    }
                case "ReservedService":
                    {
                        IEnumerable<DynamicService> reserved;
                        Host.LockReservedServices();

                        try
                        {
                            reserved = Host.ReservedServices.Where(c => c.Name == resourceName);
                        }
                        finally
                        {
                            Host.UnlockReservedServices();
                        }

                        reserved.ToList()
                                .ForEach(
                                    c =>
                                    returnData.AddResponse(
                                        UnlimitedObject.GetStringXmlDataAsUnlimitedObject("<ReservedName>" + c.Name +
                                                                                          "</ReservedName>")));
                        break;
                    }
            }


            return returnData.XmlString;
        }

        public string FindResource(string resourceName, string type, string roles)
        {
            dynamic returnData = new UnlimitedObject();

            if(resourceName == "*")
                resourceName = string.Empty;

            switch(type)
            {
                case "WorkflowService":
                    {
                        IEnumerable<DynamicService> services;
                        Host.LockServices();

                        try
                        {
                            services = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> workflowServices =
                            services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        workflowServices.ToList()
                                        .ForEach(
                                            c =>
                                            returnData.AddResponse(
                                                UnlimitedObject.GetStringXmlDataAsUnlimitedObject(
                                                    c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }
                case "Service":
                    {
                        IEnumerable<DynamicService> svc;
                        Host.LockServices();

                        try
                        {
                            svc = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> svcs =
                            svc.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                               //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        svcs.ToList()
                            .ForEach(
                                c =>
                                returnData.AddResponse(
                                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }

                case "Source":
                    {
                        IEnumerable<Source> sources;
                        Host.LockSources();

                        try
                        {
                            sources =
                                Host.Sources.Where(c => c.Name.Contains(resourceName));
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        }
                        finally
                        {
                            Host.UnlockSources();
                        }


                        sources.ToList()
                               .ForEach(
                                   c =>
                                   returnData.AddResponse(
                                       UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                         "<NoDef/>")));
                        break;
                    }
                case "Activity":
                    {
                        IEnumerable<WorkflowActivityDef> activitydefs;
                        Host.LockActivities();

                        try
                        {
                            activitydefs =
                                Host.WorkflowActivityDefs.Where(c => c.Name.Contains(resourceName));
                                    //.Where(c => c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins"));
                        }
                        finally
                        {
                            Host.UnlockActivities();
                        }

                        activitydefs.ToList()
                                    .ForEach(
                                        c =>
                                        returnData.AddResponse(
                                            UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                              "<NoDef/>")));
                        break;
                    }
                case "ReservedService":
                    {
                        IEnumerable<DynamicService> reserved;
                        Host.LockReservedServices();

                        try
                        {
                            reserved = Host.ReservedServices.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockReservedServices();
                        }

                        reserved.ToList()
                                .ForEach(
                                    c =>
                                    returnData.AddResponse(
                                        UnlimitedObject.GetStringXmlDataAsUnlimitedObject("<ReservedName>" + c.Name +
                                                                                          "</ReservedName>")));
                        break;
                    }
            }
           
            return returnData.XmlString;
        }

        public string FindResources(string resourceName, string type, string roles)
        {
            dynamic returnData = new UnlimitedObject();

            if(resourceName == "*")
                resourceName = string.Empty;

            switch(type)
            {
                case "WorkflowService":
                    {
                        IEnumerable<DynamicService> services;
                        Host.LockServices();

                        try
                        {
                            services = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> workflowServices =
                            services.Where(c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                                    //.Where(
                                        //c =>
                                        //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                        //roles == "*");
                        workflowServices.ToList()
                                        .ForEach(
                                            c =>
                                            returnData.AddResponse(
                                                UnlimitedObject.GetStringXmlDataAsUnlimitedObject(
                                                    c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }
                case "Service":
                    {
                        IEnumerable<DynamicService> svc;
                        Host.LockServices();

                        try
                        {
                            svc = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> svcs =
                            svc.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                               //.Where(
                                   //c =>
                                   //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                   //roles == "*");
                        svcs.ToList()
                            .ForEach(
                                c =>
                                returnData.AddResponse(
                                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));
                        break;
                    }

                case "Source":
                    {
                        IEnumerable<Source> sources;
                        Host.LockSources();

                        try
                        {
                            sources =
                                Host.Sources.Where(c => c.Name.Contains(resourceName));
                                    //.Where(
                                        //c =>
                                        //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                        //roles == "*");
                        }
                        finally
                        {
                            Host.UnlockSources();
                        }

                        sources.ToList()
                               .ForEach(
                                   c =>
                                   returnData.AddResponse(
                                       UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                         "<NoDef/>")));
                        break;
                    }
                case "Activity":
                    {
                        IEnumerable<WorkflowActivityDef> activitydefs;
                        Host.LockActivities();

                        try
                        {
                            activitydefs =
                                Host.WorkflowActivityDefs.Where(c => c.Name.Contains(resourceName));
                                    //.Where(
                                        //c =>
                                        //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                        //roles == "*");
                        }
                        finally
                        {
                            Host.UnlockActivities();
                        }


                        activitydefs.ToList()
                                    .ForEach(
                                        c =>
                                        returnData.AddResponse(
                                            UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                              "<NoDef/>")));
                        break;
                    }
                case "*":
                    {
                        IEnumerable<Source> resources;
                        Host.LockSources();

                        try
                        {
                            resources =
                                Host.Sources.Where(c => c.Name.Contains(resourceName));
                                    //.Where(
                                        //c =>
                                        //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                        //roles == "*");
                        }
                        finally
                        {
                            Host.UnlockSources();
                        }

                        resources.ToList()
                                 .ForEach(
                                     c =>
                                     returnData.AddResponse(
                                         UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                           "<NoDef/>")));
                        IEnumerable<DynamicService> wfservices;
                        Host.LockServices();

                        try
                        {
                            wfservices = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> workflows =
                            wfservices.Where(
                                c => c.Actions.Where(d => d.ActionType == enActionType.Workflow).Count() > 0);
                                      //.Where(
                                          //c =>
                                          //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                          //roles == "*");
                        workflows.ToList()
                                 .ForEach(
                                     c =>
                                     returnData.AddResponse(
                                         UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                           "<NoDef/>")));

                        IEnumerable<DynamicService> svc2;
                        Host.LockServices();

                        try
                        {
                            svc2 = Host.Services.Where(c => c.Name.Contains(resourceName));
                        }
                        finally
                        {
                            Host.UnlockServices();
                        }

                        IEnumerable<DynamicService> svcs2 =
                            svc2.Where(c => c.Actions.Where(d => d.ActionType != enActionType.Workflow).Count() > 0);
                                //.Where(
                                    //c =>
                                    //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                    //roles == "*");
                        svcs2.ToList()
                             .ForEach(
                                 c =>
                                 returnData.AddResponse(
                                     UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                       "<NoDef/>")));

                        /* 
                         * Travis : 02-07-2012
                         * why the flip have a specific condition above only to blindly add all services bar the role and other meta data??????!!!!!!!
                         * 
                         */
                        //var intsvc = Host.Services.Where(c => c.Name.Contains(resourceName));
                        //intsvc.ToList().ForEach(c => returnData.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ?? "<NoDef/>")));

                        IEnumerable<WorkflowActivityDef> activityinfo;
                        Host.LockActivities();

                        try
                        {
                            activityinfo =
                                Host.WorkflowActivityDefs.Where(c => c.Name.Contains(resourceName));
                                    //.Where(
                                        //c =>
                                        //c.IsUserInRole(roles, c.AuthorRoles) || roles.Contains("Domain Admins") ||
                                        //roles == "*");
                        }
                        finally
                        {
                            Host.UnlockActivities();
                        }

                        activityinfo.ToList()
                                    .ForEach(
                                        c =>
                                        returnData.AddResponse(
                                            UnlimitedObject.GetStringXmlDataAsUnlimitedObject(c.ResourceDefinition ??
                                                                                              "<NoDef/>")));
                        break;
                    }
            }
            
            dynamic serviceData = new UnlimitedObject("Dev2Resources");
            string returnVal;
            //zuko.mgwili@dev2.co.za
            //18/06/2010
            //we need to be able to return the worker service name and the category it belongs to
            if(type.Equals("Service"))
            {
                returnVal = GetWorkerServiceResourceAsXml(returnData.XmlString, serviceData);
            }
            else if(type.Equals("Source"))
            {
                returnVal = GetSourceResourceAsXml(returnData.XmlString, serviceData);
            }
            else
            {
                returnVal = GetAllDefsAsXML(returnData.XmlString, serviceData);
                //returnVal = GetSourceResourceAsXml(returnData.XmlString, serviceData);
            }

            return returnVal;
        }

        //This returns a list of supported database servers in the system
        public string FindSupportedDatabaseServers()
        {
            dynamic returnData = new UnlimitedObject();
            dynamic serviceData = new UnlimitedObject("Dev2Resources");
            dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
            serviceInfo.Dev2DatabaseServer = enSourceType.MySqlDatabase;
            serviceData.AddResponse(serviceInfo);
            serviceInfo = new UnlimitedObject("Dev2Resource");
            serviceInfo.Dev2DatabaseServer = enSourceType.SqlDatabase;
            serviceData.AddResponse(serviceInfo);
            string returnVal = GetResourceNameAsXml(returnData.XmlString, serviceData);
            return returnVal;
        }

        //This returns a list of SQLSERVER database given a serverName
        public string FindSqlDatabases(string serverName, string username, string password)
        {
            IDBHelper helper = DBHelperFactory.GenerateNewHelper(enSupportedDBTypes.MSSQL);
            var props = new DBConnectionProperties();
            props.Server = serverName;
            props.DB = "master";
            props.User = username;
            props.Pass = password;

            string result = helper.ListDatabases(helper.CreateConnectionString(props));

            return result;
        }

        //Returns SQL SERVER database schema
        // 26.09.2012 - Travis.Frisinger : Amended to properly intergate the schema
        public string FindSqlDatabaseSchema(string serverName, string databaseName, string username, string password)
        {
            dynamic xmlResponse = new UnlimitedObject();

            IDBHelper helper = DBHelperFactory.GenerateNewHelper(enSupportedDBTypes.MSSQL);
            var props = new DBConnectionProperties();
            props.Server = serverName;
            props.DB = databaseName;
            props.User = username;
            props.Pass = password;
            string result = helper.ExtractCodedEntities(helper.CreateConnectionString(props));

            return result;
        }

        public string FindDirectory(string directoryPath, string domain, string username, string password)
        {
            IntPtr accessToken = IntPtr.Zero;
            const int logon32ProviderDefault = 0;
            const int logon32LogonInteractive = 2;
            dynamic returnData = new UnlimitedObject();
            bool isDir = false;
            try
            {
                if(username.Length > 0)
                {
                    domain = (domain.Length > 0 && domain != ".") ? domain : Environment.UserDomainName;
                    bool success = LogonUser(username, domain, password, logon32LogonInteractive,
                                             logon32ProviderDefault, ref accessToken);
                    if(success)
                    {
                        var identity = new WindowsIdentity(accessToken);
                        WindowsImpersonationContext context = identity.Impersonate();
                        // get the file attributes for file or directory
                        FileAttributes attr = File.GetAttributes(directoryPath);

                        //detect whether its a directory or file
                        // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                        if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                        {
                            isDir = true;
                        }

                        if(isDir)
                        {
                            var directory = new DirectoryInfo(directoryPath);
                            returnData.JSON = GetDirectoryInfoAsJSON(directory);
                        }
                        else
                        {
                            returnData.JSON = "[]";
                        }
                        context.Undo();
                    }
                    else
                    {
                        returnData.Result = new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    // get the file attributes for file or directory
                    FileAttributes attr = File.GetAttributes(directoryPath);

                    //detect whether its a directory or file
                    // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                    {
                        isDir = true;
                    }

                    if(isDir)
                    {
                        var directory = new DirectoryInfo(directoryPath);
                        returnData.JSON = GetDirectoryInfoAsJSON(directory);
                    }
                    else
                    {
                        returnData.JSON = "[]";
                    }
                }
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        public string FindDrive(string domain, string username, string password)
        {
            IntPtr accessToken = IntPtr.Zero;
            const int logon32ProviderDefault = 0;
            const int logon32LogonInteractive = 2;
            dynamic returnData = new UnlimitedObject();
            try
            {
                if(username.Length > 0)
                {
                    domain = (domain.Length > 0 && domain != ".") ? domain : Environment.UserDomainName;
                    bool success = LogonUser(username, domain, password, logon32LogonInteractive,
                                             logon32ProviderDefault, ref accessToken);
                    if(success)
                    {
                        var identity = new WindowsIdentity(accessToken);
                        WindowsImpersonationContext context = identity.Impersonate();
                        DriveInfo[] drives = DriveInfo.GetDrives();
                        returnData.JSON = GetDriveInfoAsJSON(drives);
                        context.Undo();
                    }
                    else
                    {
                        returnData.Result = new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    returnData.JSON = GetDriveInfoAsJSON(drives);
                }
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex.Message).XmlString;
            }
            return returnData.XmlString;
        }

        public string FindServerUsername()
        {
            dynamic returnData = new UnlimitedObject();
            returnData.Result = Environment.UserDomainName + "\\" + Environment.UserName;
            return returnData.XmlString;
        }

        public string FindNetworkComputers()
        {
            dynamic returnData = new UnlimitedObject();
            string json = "[";
            try
            {
                var root = new DirectoryEntry("WinNT:");
                foreach(DirectoryEntry dom in root.Children)
                {
                    foreach(DirectoryEntry entry in dom.Children)
                    {
                        if(entry.SchemaClassName == "Computer")
                        {
                            json += @"{""ComputerName"":""" + entry.Name + @"""},";
                        }
                    }
                }
                json += "]";
                json = json.Replace(",]", "]"); // remove last comma
                returnData.JSON = json;
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        public string RegisteredAssembly()
        {
            dynamic returnData = new UnlimitedObject();
            try
            {
                IAssemblyName assemblyName;
                IAssemblyEnum assemblyEnum = GAC.CreateGACEnum();
                string json = "[";
                while(GAC.GetNextAssembly(assemblyEnum, out assemblyName) == 0)
                {
                    json += @"{""AssemblyName"":""" + GAC.GetName(assemblyName) + @"""}";
                    json += ",";
                }
                json += "]";
                json = json.Replace(",]", "]"); //remove the last comma in the string in order to have valid json
                returnData.JSON = json;
            }
            catch(Exception ex)
            {
                returnData.Error = new UnlimitedObject(ex).XmlString;
            }
            return returnData.XmlString;
        }

        public string CheckCredentials(string domain, string username, string password)
        {
            dynamic returnData = new UnlimitedObject();
            try
            {
                if (domain.Equals("."))
                {
                    domain = Environment.UserDomainName;
                }
                bool isValid;
                using(var context = new PrincipalContext(ContextType.Domain, domain))
                {
                    isValid = context.ValidateCredentials(username, password);

                    context.Dispose();
                }
                if(isValid)
                {
                    returnData.Result = "Connection successful!";
                }
                else
                {
                    returnData.Result = "Connection failed. Ensure your username and password are valid";
                }
            }
            catch
            {
                returnData.Result = "Connection failed. Ensure your username and password are valid";
            }
            return returnData.XmlString;
        }

        public string CheckPermissions(string path, string username, string password)
        {
            dynamic returnData = new UnlimitedObject();


            if(username == string.Empty)
            {
                if(!FileIO.CheckPermissions(WindowsIdentity.GetCurrent(), path, FileSystemRights.Read) &&
                    !FileIO.CheckPermissions(WindowsIdentity.GetCurrent(), path, FileSystemRights.ReadData))
                {
                    returnData.Error = string.Concat("Insufficient permission for '", path, "'.");
                }
            }
            else
            {
                // we have a username and password set :)

                if(!FileIO.CheckPermissions(username, password, path, FileSystemRights.Read) &&
                    FileIO.CheckPermissions(username, password, path, FileSystemRights.ReadData))
                {
                    returnData.Error = string.Concat("Insufficient permission for '", path, "'.");
                }
            }

            return returnData.XmlString;
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        /// <summary>
        /// Gets the drive info as JSON.
        /// </summary>
        /// <param name="drives">The drives.</param>
        /// <returns></returns>
        private string GetDriveInfoAsJSON(DriveInfo[] drives)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            string json = "[";
            foreach(DriveInfo drive in drives)
            {
                if(drive.DriveType == DriveType.Fixed)
                {
                    var directory = new DirectoryInfo(drive.Name);
                    string name = Regex.Replace(directory.Name, @"\\", @"/");
                    json += @"{""title"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                            name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") +
                            @""", ""isLazy"": true, ""children"": [";
                    // Travis.Frisinger : 20.09.2012 - Removed for speed
                    json += "]}";
                    //if (j < drives.Count() - 1) {
                    json += ",";
                    //}
                }
            }
            json = json.Substring(0, (json.Length - 1));
            json += "]";
            return json;
        }

        private string GetDirectoryInfoAsJSON(DirectoryInfo directory)
        {
            int count = 0;
            string name;
            string json = "[";
            try
            {
                foreach(DirectoryInfo d in directory.GetDirectories())
                {
                    name = Regex.Replace(d.Name, @"\\", @"\\");
                    json += @"{""title"":""" + name + @""", ""isFolder"": true, ""key"":""" +
                            name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    if(count < (directory.GetDirectories().Count() + directory.GetFiles().Count() - 1))
                    {
                        json += ',';
                    }
                    count++;
                }

                foreach(FileInfo f in directory.GetFiles())
                {
                    json += @"{""title"":""" + f.Name + @""", ""key"":""" +
                            f.Name.Replace(" ", "_").Replace("(", "40").Replace(")", "41") + @""", ""isLazy"": true}";
                    if(count < (directory.GetDirectories().Count() + directory.GetFiles().Count() - 1))
                    {
                        json += ',';
                    }
                    count++;
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            json += "]";
            return json;
        }

        private string GetResourceNameAsXml(string source, dynamic serviceData)
        {
            XElement elements = XElement.Parse(source);
            elements.Descendants()
                    .Where(node => node.Descendants().All(c => c.Name.LocalName == "XamlDefinition"))
                    .Remove();
            elements.DescendantsAndSelf().Where(node => node.Name.LocalName == "DataList").Remove();
            elements.DescendantsAndSelf().Where(node => node.Name.LocalName == "Plugin").Remove();

            if(elements.HasElements)
            {
                //foreach(XElement element in elements.Elements())
                //{
                //    //dynamic orphan = GetResourceNameAsXml(element.ToString(), serviceData);
                //    //serviceData.Dev2Service = orphan;
                //}
            }
            while(elements.HasAttributes)
            {
                XAttribute attrib = elements.LastAttribute;
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(attrib.Name == "Name")
                {
                    serviceInfo.Dev2ResourceName = attrib.Value;
                    serviceData.AddResponse(serviceInfo);
                }
                attrib.Remove();
            }
            return serviceData.XmlString;
        }

        // Travis.Frisinger - 09.09.2012 : added mode and dbType
        public string CallProcedure(string serverName, string databaseName, string procedure, string parameters,
                                    string mode, string username, string password)
        {
            string result = "<Error>Invalid Parameters</Error>";

            if(mode == "interrogate")
            {
                IDBHelper helper = DBHelperFactory.GenerateNewHelper(enSupportedDBTypes.MSSQL);

                var props = new DBConnectionProperties();
                props.Server = serverName;
                props.DB = databaseName;
                props.User = username;
                props.Pass = password;

                result = helper.TickleDBLogic(helper.CreateConnectionString(props), procedure, parameters);
            }
            return result;
        }

        public string AddResource(string resourceDefinition, string roles)
        {
            List<DynamicServiceObjectBase> compiledResources = Host.GenerateObjectGraphFromString(resourceDefinition);
            if(compiledResources.Count == 0)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_BuildFailed);
            }

            dynamic returnData = Host.AddResources(compiledResources, roles);
            return returnData.XmlString;
        }

        public string DeployResource(string resourceDefinition, string roles)
        {
            string result = WorkspaceRepository.Instance.ServerWorkspace.Save(resourceDefinition, roles);
            WorkspaceRepository.Instance.RefreshWorkspaces();

            return result;
        }

        public string CompileResource(string resourceDefinition)
        {
            dynamic returnData = new UnlimitedObject();

            List<DynamicServiceObjectBase> compiledResources = Host.GenerateObjectGraphFromString(resourceDefinition);
            if(compiledResources.Count == 0)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_BuildFailed);
            }

            compiledResources.ForEach(c =>
            {
                if(c.ObjectType == enDynamicServiceObjectType.DynamicService)
                {
                    var dynamicService = c as DynamicService;
                    if (dynamicService != null)
                        dynamicService.Actions.ForEach(action => Host.MapServiceActionDependencies(action));
                }
                if(c.ObjectType == enDynamicServiceObjectType.WorkflowActivity)
                {
                    Host.MapActivityToService((c as WorkflowActivityDef));
                }
                c.Compile();
                returnData.AddResponse(c.GetCompilerErrors());
            });

            return returnData.XmlString;
        }

        // PluginMetaDataRegistry
        // Travis : Added to allow plugin meta data discovery
        // includeSignatrues  is true, false
        public string PluginMetaDataRegistry(string asmLoc, string nameSpace, string protectionLevel, string methodName)
        {
            var pluginData = new StringBuilder();

            asmLoc = asmLoc.Replace(@"//", "/");

            // new app domain to avoid security concerns resulting from blinding loading code into Server's space
            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            IEnumerable<string> plugins = null;

            AppDomain pluginDomain = AppDomain.CreateDomain("PluginMetaDataDiscoveryDomain", null, setup);

            string baseLocation = string.Empty;
            string gacQualifiedName = String.Empty;

            if(asmLoc == string.Empty || asmLoc.StartsWith("Plugins"))
            {
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";

                baseLocation = @"Plugins\";

                if(asmLoc == string.Empty)
                {
                    // now interigate the file system and build up a list of plugins and data
                    plugins = Directory.EnumerateFiles(pluginDomain.BaseDirectory);
                }
                else
                {
                    plugins = new[] { pluginDomain.BaseDirectory + asmLoc.Replace("/", @"\") };
                }
            }
            else
            {
                if(!String.IsNullOrEmpty(asmLoc))
                {
                    if(asmLoc.StartsWith("GAC:"))
                    {
                        baseLocation = "GAC:";
                        // we have a plugin loaded into the global assembly cache
                        gacQualifiedName = asmLoc.Substring(4);
                    }
                    else
                    {
                        baseLocation = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(asmLoc);
                        // we have a plugin relative to the file system
                        plugins = new[] { asmLoc };
                    }
                }
            }

            bool includePublic = true;
            bool includePrivate = true;

            // default to all if no params
            if(protectionLevel != string.Empty)
            {
                // only include public methods
                if(protectionLevel.ToLower() == "public")
                {
                    includePrivate = false;
                }
            }

            if(plugins != null)
            {
                plugins
                    .ToList()
                    .ForEach(plugin =>
                    {
                        int pos = plugin.LastIndexOf(@"\", StringComparison.Ordinal);
                        pos += 1;
                        string shortName = plugin.Substring(pos, (plugin.Length - pos));

                        // only attempt to load assemblies
                        if(shortName.EndsWith(".dll"))
                        {
                            try
                            {
                                Assembly asm = Assembly.LoadFrom(plugin);

                                // only include matching references
                                InterogatePluginAssembly(pluginData, asm, shortName, baseLocation + shortName,
                                                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                                         includePublic, includePrivate, methodName, nameSpace);
                                                        // ReSharper restore ConditionIsAlwaysTrueOrFalse

                                // remove the plugin
                                try
                                {
                                    Assembly.UnsafeLoadFrom(plugin);
                                }
                                // ReSharper disable EmptyGeneralCatchClause
                                catch
                                // ReSharper restore EmptyGeneralCatchClause
                                {
                                    // Best effort... ;)
                                }
                            }
                            catch(Exception e)
                            {
                                TraceWriter.WriteTrace("Plugin Load Error : " + e.Message + Environment.NewLine +
                                                       e.StackTrace);
                                pluginData.Append("<Dev2Plugin><Dev2PluginName>" + shortName + "</Dev2PluginName>");
                                pluginData.Append(
                                    "<Dev2PluginStatus>Error</Dev2PluginStatus><Dev2PluginStatusMessage>");
                                pluginData.Append(e.Message + "</Dev2PluginStatusMessage>");
                                pluginData.Append("<Dev2PluginSourceNameSpace></Dev2PluginSourceNameSpace>");
                                pluginData.Append("<Dev2PluginSourceLocation>" + baseLocation + shortName +
                                                  "</Dev2PluginSourceLocation>");
                                pluginData.Append("<Dev2PluginExposedMethod></Dev2PluginExposedMethod>");
                                pluginData.Append("</Dev2Plugin>");
                            }
                        }
                    });
            }
            else if(!String.IsNullOrEmpty(gacQualifiedName))
            {
                GACAssemblyName gacName = GAC.TryResolveGACAssembly(gacQualifiedName);

                if(gacName == null)
                    if(GAC.RebuildGACAssemblyCache(true))
                        gacName = GAC.TryResolveGACAssembly(gacQualifiedName);

                if(gacName != null)
                {
                    try
                    {
                        Assembly asm = Assembly.Load(gacName.ToString());
                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                        InterogatePluginAssembly(pluginData, asm, gacName.Name, baseLocation + gacName, includePublic,
                        // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                                 includePrivate, methodName, nameSpace);
                    }
                    catch(Exception e)
                    {
                        pluginData.Append("<Dev2Plugin><Dev2PluginName>" + gacName.Name + "</Dev2PluginName>");
                        pluginData.Append("<Dev2PluginStatus>Error</Dev2PluginStatus><Dev2PluginStatusMessage>");
                        pluginData.Append(e.Message + "</Dev2PluginStatusMessage>");
                        pluginData.Append("<Dev2PluginSourceNameSpace></Dev2PluginSourceNameSpace>");
                        pluginData.Append("<Dev2PluginSourceLocation>" + baseLocation + gacName +
                                          "</Dev2PluginSourceLocation>");
                        pluginData.Append("<Dev2PluginExposedMethod></Dev2PluginExposedMethod>");
                        pluginData.Append("</Dev2Plugin>");
                    }
                }
            }


            string theResult = "<Dev2PluginRegistration>" + pluginData + "</Dev2PluginRegistration>";

            return theResult;
        }

        private static string BuildMethodSignature(ParameterInfo[] args, string methodName)
        {
            // add method signature as well ;)
            var toAdd = new StringBuilder();
            toAdd.Append("<Dev2PluginExposedSignature>");
            toAdd.Append("<Dev2PluginMethod>");
            toAdd.Append(methodName);
            toAdd.Append("</Dev2PluginMethod>");

            foreach(ParameterInfo p in args)
            {
                string t = p.ParameterType.Name;
                string name = p.Name;
                toAdd.Append("<Dev2PluginArg>");
                if(!t.Contains("<"))
                {
                    t = t.Replace("`", "");
                    var r = new Regex("(?<!\\.[0-9a-z]*)[0-9]");
                    t = r.Replace(t, "");

                    toAdd.Append(t + " : " + name);
                }
                toAdd.Append("</Dev2PluginArg>");
            }

            toAdd.Append("</Dev2PluginExposedSignature>");

            return toAdd.ToString();
        }

        private static void InterogatePluginAssembly(StringBuilder pluginData, Assembly asm, string shortName,
                                                     string sourceLocation, bool includePublic, bool includePrivate,
                                                     string methodName, string nameSpace)
        {
            Type[] types = asm.GetTypes();

            int pos = 0;
            bool found = false;
            bool defaultNameSpace = false;
            // take all namespaces 
            if(nameSpace == string.Empty)
            {
                defaultNameSpace = true;
            }

            while(pos < types.Length && !found)
            {
                Type t = types[pos];
                string classString = t.FullName;
                // ensure no funny xml fragments are present

                if(classString.IndexOf("<", StringComparison.Ordinal) < 0 && (defaultNameSpace || (classString == nameSpace)))
                {
                    var exposedMethodsXml = new StringBuilder();

                    MethodInfo[] methods = t.GetMethods();

                    IList<string> exposedMethods = new List<string>();
                    IList<string> methodSignatures = new List<string>();

                    int pos1 = 0;
                    while(pos1 < methods.Length && !found)
                    {
                        MethodInfo m = methods[pos1];

                        if(m.IsPublic && includePublic)
                        {
                            if(!exposedMethods.Contains(m.Name) && methodName == string.Empty)
                            {
                                exposedMethods.Add(m.Name);
                            }
                            else if(methodName == m.Name)
                            {
                                exposedMethods.Add(m.Name);
                                methodSignatures.Add(BuildMethodSignature(m.GetParameters(), m.Name));
                                found = true;
                            }
                        }
                        else if(m.IsPrivate && includePrivate)
                        {
                            if(!exposedMethods.Contains(m.Name) && methodName == string.Empty)
                            {
                                exposedMethods.Add(m.Name);
                            }
                            else if(methodName == m.Name)
                            {
                                exposedMethods.Add(m.Name);
                                methodSignatures.Add(BuildMethodSignature(m.GetParameters(), m.Name));
                                found = true;
                            }
                        }

                        pos1++;
                    }

                    // ReSharper disable StringCompareToIsCultureSpecific
                    exposedMethods.ToList().Sort((x, y) => x.ToLower().CompareTo(y.ToLower()));
                    // ReSharper restore StringCompareToIsCultureSpecific

                    foreach(string m in exposedMethods)
                    {
                        exposedMethodsXml = exposedMethodsXml.Append("<Dev2PluginExposedMethod>");
                        exposedMethodsXml = exposedMethodsXml.Append(m);
                        exposedMethodsXml = exposedMethodsXml.Append("</Dev2PluginExposedMethod>");
                    }

                    var methodSigsXml = new StringBuilder();

                    foreach(string ms in methodSignatures)
                    {
                        methodSigsXml.Append(ms);
                    }

                    if(!classString.Contains("+"))
                    {
                        pluginData.Append("<Dev2Plugin><Dev2PluginName>" + shortName + "</Dev2PluginName>");
                        pluginData.Append("<Dev2PluginStatus>Registered</Dev2PluginStatus>");
                        pluginData.Append("<Dev2PluginSourceNameSpace>" + classString + "</Dev2PluginSourceNameSpace>");
                        pluginData.Append("<Dev2PluginSourceLocation>" + sourceLocation + "</Dev2PluginSourceLocation>");
                        pluginData.Append(exposedMethodsXml);
                        pluginData.Append("<Dev2PluginSourceExposedMethodSignatures>");
                        if(methodSignatures.Count > 0)
                        {
                            pluginData.Append(methodSigsXml);
                        }
                        pluginData.Append("</Dev2PluginSourceExposedMethodSignatures>");
                        pluginData.Append("</Dev2Plugin>");
                    }
                }

                pos++;
            }
        }

        // Find MachineName
        public string FindMachineName()
        {
            dynamic returnData = new UnlimitedObject();
            returnData.Dev2MachineName = Environment.MachineName;
            return returnData.XmlString;
        }

        public string SetDynamicServiceMode(string xmlRequest)
        {
            dynamic xmlReq = new UnlimitedObject();
            xmlReq.Load(xmlRequest);

            dynamic service = xmlReq.Service;

            if(service is string)
            {
                DynamicService svc;
                Host.LockServices();

                try
                {
                    svc = Host.Services.Find(c => c.Name == service);
                }
                finally
                {
                    Host.UnlockServices();
                }


                if(svc != null)
                {
                    dynamic mode = xmlReq.Mode;
                    if(mode is string)
                    {
                        enDynamicServiceMode modeToSet;
                        if(Enum.TryParse<enDynamicServiceMode>(xmlReq.Mode, out modeToSet))
                        {
                            svc.Mode = modeToSet;
                        }
                        else
                        {
                            xmlReq.Error = "Bad Mode in Request";
                        }
                    }
                }
            }
            else
            {
                xmlReq.Error = "Bad Request";
            }
            return xmlReq.XmlString;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        private SqlCommand CreateSqlCommand(SqlConnection connection, ServiceAction serviceAction)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = serviceAction.SourceMethod;
            cmd.CommandTimeout = serviceAction.CommandTimeout;

            //Add the parameters to the SqlCommand
            if (serviceAction.ServiceActionInputs.Any())
            {
                foreach (ServiceActionInput sai in serviceAction.ServiceActionInputs)
                {
                    var injectVal = (string)sai.Value;

                    // 16.10.2012 : Travis.Frisinger - Convert empty to null
                    if (sai.EmptyToNull && injectVal == AppServerStrings.NullConstant)
                    {
                        cmd.Parameters.AddWithValue(sai.Source, DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(sai.Source, sai.Value);
                    }
                }
            }

            return cmd;
        }

        private string GetXmlDataFromSqlServiceAction(ServiceAction serviceAction)
        {
            string xmlData;

            using (var dataset = new DataSet())
            {
                using (var connection = new SqlConnection(serviceAction.Source.ConnectionString))
                {
                    var cmd = CreateSqlCommand(connection, serviceAction);
                    connection.Open();

                    using (cmd)
                    {
                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataset);
                        }
                    }
                    connection.Close();
                }
                xmlData = DataSanitizerFactory.GenerateNewSanitizer(enSupportedDBTypes.MSSQL).SanitizePayload(dataset.GetXml());
            }

            return xmlData;
        }

        private IOutputFormatter GetOutputFormatterFromServiceAction(ServiceAction serviceAction)
        {
            string outputDescription = serviceAction.OutputDescription.Replace("<Dev2XMLResult>", "").Replace("</Dev2XMLResult>", "").Replace("<JSON />", "");

            IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            if (outputDescriptionSerializationService == null)
            {
                return null;
            }

            IOutputDescription outputDescriptionInstance = outputDescriptionSerializationService.Deserialize(outputDescription);

            if (outputDescriptionInstance == null)
            {
                return null;
            }

            return OutputFormatterFactory.CreateOutputFormatter(outputDescriptionInstance);
        }

        private void AddErrorsToDataList(ErrorResultTO errors, Guid dataListID)
        {
            // Upsert any errors that might have occured into the datalist
            IBinaryDataListEntry be = Dev2BinaryDataListFactory.CreateEntry(enSystemTag.Error.ToString(), string.Empty);
            string error;
            be.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(errors.MakeDataListReady(), enSystemTag.Error.ToString()), out error);
            if (!string.IsNullOrWhiteSpace(error))
            {
                //At this point there was an error while trying to handle errors so we throw an exception
                throw new Exception(string.Format("The error '{0}' occured while creating the error entry for the following errors: {1}", error, errors.MakeDisplayReady()));
            }

            var upsertErrors = new ErrorResultTO();
            SvrCompiler.Upsert(null, dataListID, DataListUtil.BuildSystemTagForDataList(enSystemTag.Error, true), be, out upsertErrors);
            if (upsertErrors.HasErrors())
            {
                //At this point there was an error while trying to handle errors so we throw an exception
                throw new Exception(string.Format("The error '{0}' occured while upserting the following errors to the datalist: {1}", errors.MakeDisplayReady(), errors.MakeDisplayReady()));
            }
        }

        //Author: Zuko Mgwili
        //Date: 18/06/2012
        //Purpose of the method is to return worker service name and worker service category
        private string GetWorkerServiceResourceAsXml(string source, dynamic serviceData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList list = doc.ChildNodes;
            XmlNode root = list[0];
            XmlNodeList workerServices = root.ChildNodes;
            foreach(XmlNode node in workerServices)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(node.Name.Equals("Service"))
                {
                    if (node.Attributes != null)
                    {
                        serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    }

                    XmlNode tmpNode = node.SelectSingleNode("//Category");
                    if(tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    //serviceInfo.Dev2WorkerServiceCategory = node.ChildNodes[3].InnerText;
                    serviceInfo.Dev2WorkerServiceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
            }
            return serviceData.XmlString;
        }

        // Travis : Fixed all of Zuko's crap service defs
        private string GetAllDefsAsXML(string source, dynamic serviceData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList list = doc.ChildNodes;
            XmlNode root = list[0];
            XmlNodeList workerServices = root.ChildNodes;
            foreach(XmlNode node in workerServices)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(node.Name.Equals("Service"))
                {
                    if (node.Attributes != null)
                    {
                        serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    }
                    XmlNode tmpNode =
                        // ReSharper disable ReplaceWithSingleCallToFirstOrDefault
                        node.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "Category").FirstOrDefault();
                        // ReSharper restore ReplaceWithSingleCallToFirstOrDefault
                    if(tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    serviceInfo.Dev2WorkerServiceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
                else if(node.Name.Equals("Source"))
                {
                    if (node.Attributes != null)
                    {
                        serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    }

                    XmlNode tmpNode =
                        node.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "Category").FirstOrDefault();
                    if(tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    serviceInfo.Dev2SourceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
            }
            return serviceData.XmlString;
        }

        //Author: Zuko Mgwili
        //Date: 19/06/2012
        //Purpose of the method is to return source name and type
        private string GetSourceResourceAsXml(string source, dynamic serviceData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList list = doc.ChildNodes;
            XmlNode root = list[0];
            XmlNodeList sourceItem = root.ChildNodes;
            foreach(XmlNode node in sourceItem)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(node.Name.Equals("Source"))
                {
                    if (node.Attributes != null)
                    {
                        serviceInfo.Dev2SourceName = node.Attributes["Name"].Value;
                        serviceInfo.Dev2SourceType = node.Attributes["Type"].Value;
                    }
                    serviceInfo.Dev2SourceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
            }
            return serviceData.XmlString;
        }

        /// <summary>
        /// Dispatches the error state to the client
        /// </summary>
        /// <param name="xmlRequest">The XML request.</param>
        /// <param name="dataListId">The data list id.</param>
        /// <param name="allErrors">All errors.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/02/18</date>
        private void DispatchDebugState(dynamic xmlRequest, Guid dataListId, ErrorResultTO allErrors)
        {
            var debugState = new DebugState()
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                IsSimulation = false,
                Server = string.Empty,
                Version = string.Empty,
                Name = GetType().Name,
                HasError = true,
                ActivityType = ActivityType.Service,
                StateType = StateType.All,
                ServerID = HostSecurityProvider.Instance.ServerID
            };

            try
            {
                var xmlReader = XmlReader.Create(new StringReader(xmlRequest.XmlString));
                var xmlDoc = XDocument.Load(xmlReader);
                var workSpaceID = (from n in xmlDoc.Descendants("wid")
                                   select n).FirstOrDefault();
                var invokedService = (from n in xmlDoc.Descendants("Service")
                                      select n).FirstOrDefault();
                if (workSpaceID != null)
                {
                    debugState.WorkspaceID = Guid.Parse(workSpaceID.Value);
                }
                if (invokedService != null)
                {
                    debugState.DisplayName = invokedService.Value;
                }

                //ParentID = dataObject.ParentInstanceID
            }
            catch (Exception exception)
            {
                //TODO what if not an xmlRequest ? IDSFDataObject dataObject = new DsfDataObject(xmlRequest, dataListId);
                throw;
            }

            debugState.ErrorMessage = XmlHelper.MakeErrorsUserReadable(allErrors.MakeDisplayReady());

            DebugDispatcher.Instance.Write(debugState);
        }
        #endregion Private Methods
    }

    #endregion
}
