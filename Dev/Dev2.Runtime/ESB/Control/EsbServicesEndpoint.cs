using System.IO;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Newtonsoft.Json;
using ServiceStack.Common.Extensions;

namespace Dev2.DynamicServices
{

    /// <summary>
    /// Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint : IFrameworkDuplexDataChannel, IEsbWorkspaceChannel
    {

        #region IFrameworkDuplexDataChannel Members
        Dictionary<string, IFrameworkDuplexCallbackChannel> _users = new Dictionary<string, IFrameworkDuplexCallbackChannel>();
        public void Register(string userName)
        {
            if (_users.ContainsKey(userName))
            {
                _users.Remove(userName);
            }

            _users.Add(userName, OperationContext.Current.GetCallbackChannel<IFrameworkDuplexCallbackChannel>());
            NotifyAllClients(string.Format("User '{0}' logged in", userName));

        }

        public void Unregister(string userName)
        {
            if (UserExists(userName))
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
            if (userName == "System")
            {
                suffix = string.Empty;
            }
            NotifyAllClients(string.Format("{0} {1} {2}", userName, suffix, message));
        }

        public void SendPrivateMessage(string userName, string targetUserName, string message)
        {
            string suffix = " Said:";
            if (userName == "System")
            {
                suffix = string.Empty;
            }
            if (UserExists(userName))
            {
                if (!UserExists(targetUserName))
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
            // 5782: Removed static global variable: ds
            //if (UserExists(userName))
            //{
            //    var targetService = ds.Services.Find(c => c.Name == serviceName);
            //    if (targetService != null)
            //    {

            //        var debuggerUser = targetService.Debuggers.Find(debugger => debugger == userName);
            //        if (!string.IsNullOrEmpty(debuggerUser))
            //        {
            //            if (!debugOn)
            //            {
            //                targetService.Debuggers.Remove(userName);
            //            }
            //        }
            //        else
            //        {
            //            if (debugOn)
            //            {
            //                targetService.Debuggers.Add(userName);
            //            }
            //        }
            //    }
            //}
        }

        public void Rollback(string userName, string serviceName, int versionNo)
        {
            //if (UserExists(userName)) {
            //    var targetService = ds.Services.Find(service => service.Name == serviceName);
            //    if (targetService != null) {
            //        string fileName = string.Format("{0}\\{1}.V{2}.xml", "Services\\VersionControl", serviceName, versionNo.ToString());
            //        if (File.Exists(fileName)) {
            //            var items = ds.GenerateObjectGraphFromString(File.ReadAllText(fileName));
            //            dynamic response = ds.AddResources(items, "Domain Admins,Business Design Studio Developers,Business Design Studio Testers");
            //            SendPrivateMessage("System", userName, response.XmlString);
            //        }
            //        else {
            //            SendPrivateMessage("System", userName, "Version not found!");

            //        }
            //    }
            //}
        }

        public void Rename(string userName, string resourceType, string resourceName, string newResourceName)
        {


        }

        public void ReloadSpecific(string userName, string serviceName)
        {


        }

        public void Reload()
        {
            // 5782: Removed static global variable: ds
            //ds.RestoreResources(new string[] { "Sources", "Services", "ActivityDefs" });
        }

        private bool UserExists(string userName)
        {
            return _users.ContainsKey(userName) || userName.Equals("System", StringComparison.InvariantCultureIgnoreCase);
        }

        private void NotifyAllClients(string message)
        {
            _users.ToList().ForEach(c => NotifyClient(c.Key, message));
        }

        private void NotifyClient(string userName, string message)
        {

            try
            {
                if (UserExists(userName))
                {
                    _users[userName].CallbackNotification(message);
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                _users.Remove(userName);
            }
        }

        #endregion
        //private ParallelCommandExecutor _parallel;

        /// <summary>
        ///Loads service definitions.
        ///This is a singleton service so this object
        ///will be visible in every call 
        /// </summary>
        public EsbServicesEndpoint()
        {
            try
            {
                //_parallel = new ParallelCommandExecutor(this);
            }
            catch (Exception ex)
            {
                ServerLogger.LogError(ex);
                throw ex;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return true;
            }
        }

        #region Travis' New Entry Point
        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            Guid resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // If no DLID, we need to make it based upon the request ;)
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                string theShape = null;
                try
                {
                    theShape = FindServiceShape(workspaceID, dataObject.ServiceName);

                }
                catch (Exception ex)
                {
                    ServerLogger.LogError(ex);
                    errors.AddError(string.Format("Service [ {0} ] not found.", dataObject.ServiceName));
                   return resultID;
                }

                ErrorResultTO invokeErrors;
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            try
            {
                ErrorResultTO invokeErrors;
                // Setup the invoker endpoint ;)
                var invoker = new DynamicServicesInvoker(this, this, theWorkspace);

                // Should return the top level DLID
                resultID = invoker.Invoke(dataObject, out invokeErrors);
                errors.MergeErrors(invokeErrors);

            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }

            return resultID;
        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            var serviceName = dataObject.ServiceName;
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            var invoker = new DynamicServicesInvoker(this, this, theWorkspace);
            var generateInvokeContainer = invoker.GenerateInvokeContainer(dataObject, serviceName);
            var curDlid = generateInvokeContainer.Execute(out errors);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var convertFrom = compiler.ConvertFrom(curDlid, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
            var jsonSerializerSettings = new JsonSerializerSettings();
            var deserializeObject = JsonConvert.DeserializeObject<T>(convertFrom, jsonSerializerSettings);
            return deserializeObject;
        }

        /// <summary>
        /// Executes the transactionally scoped request, caller must delete datalist
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteTransactionallyScopedRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            var invoker = new DynamicServicesInvoker(this, this, theWorkspace);
            errors = new ErrorResultTO();
            string theShape;
            Guid oldID = new Guid();
            Guid innerDatalistID = new Guid();
            ErrorResultTO invokeErrors;

            // Account for silly webpages...
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // If no DLID, we need to make it based upon the request ;)
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                theShape= FindServiceShape(workspaceID, dataObject.ServiceName);
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), 
                    dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            if (!dataObject.IsDataListScoped)
            {
                theShape = FindServiceShape(workspaceID, dataObject.ServiceName);
                innerDatalistID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                    dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                var mergedID = compiler.Merge(innerDatalistID, dataObject.DataListID,
                                                      enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite,
                                                      true, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                oldID = dataObject.DataListID;
                dataObject.DataListID = mergedID;
            }


            EsbExecutionContainer executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName);
            Guid result = dataObject.DataListID;

            if (executionContainer != null)
            {
                result = executionContainer.Execute(out errors);
            }
            else
            {
                errors.AddError("Null container returned");
            }

            if (!dataObject.IsDataListScoped)
            {
                compiler.DeleteDataListByID(oldID);
                compiler.DeleteDataListByID(innerDatalistID);
            }

            return result;

        }

        /// <summary>
        /// Fetches the execution payload.
        /// </summary>
        /// <param name="dataObj">The data obj.</param>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string FetchExecutionPayload(IDSFDataObject dataObj,DataListFormat targetFormat, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string targetShape = FindServiceShape(dataObj.WorkspaceID, dataObj.ServiceName);
            string result = string.Empty;

            if (targetShape != null)
            {
                string translatorShape = ManipulateDataListShapeForOutput(targetShape);
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO invokeErrors;
                result = compiler.ConvertAndFilter(dataObj.DataListID, targetFormat,translatorShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                errors.AddError("Could not locate service shape for " + dataObj.ServiceName);
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        public string FindServiceShape(Guid workspaceID, string serviceName)
        {
            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceName);

            var tmp = services.FirstOrDefault();
            var result = "<DataList></DataList>";

            if (tmp != null)
            {
                result = tmp.DataListSpecification;
            }

            return result;
        }

        /// <summary>
        /// Manipulates the data list shape for output.
        /// </summary>
        /// <param name="preShape">The pre shape.</param>
        /// <returns></returns>
        private string ManipulateDataListShapeForOutput(string preShape)
        {
            XDocument xDoc = XDocument.Load(new StringReader(preShape));

            XElement rootEl = xDoc.Element("DataList");
            if (rootEl == null) return xDoc.ToString();

            rootEl.Elements().Where(el =>
            {
                var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                    var removeCondition = firstOrDefault != null &&
                                          (firstOrDefault.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           firstOrDefault.Value == enDev2ColumnArgumentDirection.None.ToString());
                return (removeCondition && !el.HasElements);
            }).Remove();

            var xElements = rootEl.Elements().Where(el => el.HasElements);
            var enumerable = xElements as IList<XElement> ?? xElements.ToList();
            enumerable.Elements().Where(element =>
            {
                var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                    var removeCondition = xAttribute != null &&
                                          (xAttribute.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           xAttribute.Value == enDev2ColumnArgumentDirection.None.ToString());
                return removeCondition;
            }).Remove();
            enumerable.Where(element => !element.HasElements).Remove();
            return xDoc.ToString();
        }


    }
}
