using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.DB_Sanity;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Workspaces;
using Microsoft.Win32.SafeHandles;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;
using enActionType = Dev2.DataList.Contract.enActionType;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Database Execution Container
    /// </summary>
    public class DatabaseServiceContainer : EsbExecutionContainer
    {

        #region Constuctors

        public DatabaseServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel)
            : base(sa, dataObj, workspace, esbChannel)
        {
        }

        #endregion

        #region Execute

        public override Guid Execute(out ErrorResultTO errors)
        {
            // BUG 9710 - 2013.06.20 - TWR - refactored
            errors = new ErrorResultTO();

            var result = DataObject.DataListID;

            var compiler = DataListFactory.CreateDataListCompiler();
            var itrs = new List<IDev2DataListEvaluateIterator>(5);
            var itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

            try
            {
                ErrorResultTO invokeErrors;
                if(ServiceAction.ServiceActionInputs.Count == 0)
                {
                    ExecuteService(itrCollection, itrs, compiler, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                else
                {
                    #region Build iterators for each ServiceActionInput

                    foreach(var sai in ServiceAction.ServiceActionInputs)
                    {
                        var val = sai.Source;
                        var toInject = AppServerStrings.NullConstant;

                        if(val != null)
                        {
                            toInject = DataListUtil.AddBracketsToValueIfNotExist(sai.Source);
                        }
                        else if(!sai.EmptyToNull)
                        {
                            toInject = sai.DefaultValue;
                        }

                        var expressionEntry = compiler.Evaluate(DataObject.DataListID, enActionType.User, toInject, false, out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                        var expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
                        itrCollection.AddIterator(expressionIterator);
                        itrs.Add(expressionIterator);
                    }

                    #endregion

                    while(itrCollection.HasMoreData())
                    {
                        ExecuteService(itrCollection, itrs, compiler, out invokeErrors);
                        errors.MergeErrors(invokeErrors);
                    }
                }
            }
            catch(Exception ex)
            {
                errors.AddError(ex.Message);
            }

            return result;
        }

        #endregion

        #region ExecuteService

        // BUG 9710 - 2013.06.20 - TWR - refactored
        void ExecuteService(IDev2IteratorCollection itrCollection, IList<IDev2DataListEvaluateIterator> itrs, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            // Get XAML data from service action
            var xmlDbResponse = GetXmlDataFromSqlServiceAction(ServiceAction, itrCollection, itrs);

            if(string.IsNullOrEmpty(xmlDbResponse))
            {
                // If there was no data returned add error
                errors.AddError("The request yielded no response from the data store.");
            }
            else
            {
                // Get the output formatter from the service action
                var outputFormatter = GetOutputFormatterFromServiceAction(ServiceAction);
                if(outputFormatter == null)
                {
                    // If there was an error getting the output formatter from the service action
                    errors.AddError(string.Format("Output format in service action {0} is invalid.", ServiceAction.Name));
                }
                else
                {
                    // Format the XML data
                    var formatedPayload = outputFormatter.Format(xmlDbResponse).ToString();

                    // Create a shape from the service action outputs
                    var dlShape = compiler.ShapeDev2DefinitionsToDataList(ServiceAction.OutputSpecification, enDev2ArgumentType.Output, false, out errors);
                    errors.MergeErrors(errors);

                    // Push formatted data into a datalist using the shape from the service action outputs
                    var tmpID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), formatedPayload, dlShape, out errors);
                    errors.MergeErrors(errors);

                    // Attach a parent ID to the newly created datalist
                    compiler.SetParentID(tmpID, DataObject.DataListID);

                    // Merge each result into the datalist ;)
                    compiler.Merge(DataObject.DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);

                    //compiler.Shape(tmpID, enDev2ArgumentType.Output_Append_Style, ServiceAction.OutputSpecification, out errors);
                    errors.MergeErrors(errors);
                    compiler.ForceDeleteDataListByID(tmpID); // clean up ;)
                }
            }
        }

        #endregion

        private SqlCommand CreateSqlCommand(SqlConnection connection, ServiceAction serviceAction, IDev2IteratorCollection iteratorCollection, IEnumerable<IDev2DataListEvaluateIterator> itrs)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = serviceAction.SourceMethod;
            cmd.CommandTimeout = serviceAction.CommandTimeout;

            //Add the parameters to the SqlCommand
            if(serviceAction.ServiceActionInputs.Any())
            {
                // Loop iterators ;)
                int pos = 0;
                foreach(IDev2DataListEvaluateIterator itr in itrs)
                {
                    var injectVal = iteratorCollection.FetchNextRow(itr);
                    ServiceActionInput sai = serviceAction.ServiceActionInputs[pos];

                    // 16.10.2012 : Travis.Frisinger - Convert empty to null
                    if(sai.EmptyToNull && (injectVal == null || string.Compare(injectVal.TheValue, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        cmd.Parameters.AddWithValue(string.Format("@{0}", sai.Source), DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(string.Format("@{0}", sai.Source), injectVal.TheValue);
                    }

                    pos++;
                }
            }

            return cmd;
        }


        #region DB Impersionation

        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        const int LOGON32_LOGON_INTERACTIVE = 2;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out PathOperations.SafeTokenHandle phToken);

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        #endregion

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected virtual string GetXmlDataFromSqlServiceAction(ServiceAction serviceAction, IDev2IteratorCollection iteratorCollection, IList<IDev2DataListEvaluateIterator> itrs)
        {
            string xmlData;

            using(var dataset = new DataSet())
            {

                string UserName;
                string Password;
                string connectionString = serviceAction.Source.ConnectionString;
                string conStr = MsSqlBroker.ImpersonateDomainUser(connectionString, out UserName, out Password);

                try
                {

                    if(!MsSqlBroker.RequiresAuth(UserName))
                    {
                        using(var connection = new SqlConnection(conStr))
                        {
                            SqlCommand cmd = null;

                            try
                            {
                                cmd = CreateSqlCommand(connection, serviceAction, iteratorCollection, itrs);
                            }
                            catch(Exception ex)
                            {
                                if(cmd != null)
                                {
                                    cmd.Dispose();
                                }
                                ServerLogger.LogError(ex);
                                throw;
                            }

                            connection.Open();

                            using(cmd)
                            {
                                using(var adapter = new SqlDataAdapter(cmd))
                                {
                                    adapter.Fill(dataset);
                                }
                            }
                            connection.Close();
                        }
                    }
                    else
                    {
                        // handle UNC path
                        PathOperations.SafeTokenHandle safeTokenHandle;

                        try
                        {
                            string user = MsSqlBroker.ExtractUserName(UserName);
                            string domain = MsSqlBroker.ExtractDomain(UserName);
                            bool loginOk = LogonUser(user, domain, Password, LOGON32_LOGON_INTERACTIVE,
                                                     LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                            if(loginOk)
                            {
                                using(safeTokenHandle)
                                {

                                    WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                                    using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                                    {
                                        // Do the operation here
                                        using(var connection = new SqlConnection(conStr))
                                        {
                                            SqlCommand cmd = null;

                                            try
                                            {
                                                cmd = CreateSqlCommand(connection, serviceAction, iteratorCollection, itrs);
                                            }
                                            catch(Exception ex)
                                            {
                                                if(cmd != null)
                                                {
                                                    cmd.Dispose();
                                                }
                                                ServerLogger.LogError(ex);
                                                throw;
                                            }

                                            connection.Open();

                                            using(cmd)
                                            {
                                                using(var adapter = new SqlDataAdapter(cmd))
                                                {
                                                    adapter.Fill(dataset);
                                                }
                                            }
                                        }


                                        impersonatedUser.Undo(); // remove impersonation now
                                    }
                                }
                            }
                            else
                            {
                                // login failed
                                throw new Exception("Failed to authenticate with user [ " + UserName + " ].");
                            }
                        }
                        catch(Exception ex)
                        {
                            ServerLogger.LogError(ex);
                            throw;
                        }
                    }
                }
                catch(Exception e)
                {
                    ServerLogger.LogError(e);
                }


                xmlData = DataSanitizerFactory.GenerateNewSanitizer(enSupportedDBTypes.MSSQL).SanitizePayload(dataset.GetXml()); ;

                return xmlData;

            }
        }

        private IOutputFormatter GetOutputFormatterFromServiceAction(ServiceAction serviceAction)
        {
            string outputDescription = serviceAction.OutputDescription.Replace("<Dev2XMLResult>", "").Replace("</Dev2XMLResult>", "").Replace("<JSON />", "");

            IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();

            if(outputDescriptionSerializationService == null)
            {
                return null;
            }

            IOutputDescription outputDescriptionInstance = outputDescriptionSerializationService.Deserialize(outputDescription);

            if(outputDescriptionInstance == null)
            {
                return null;
            }

            return OutputFormatterFactory.CreateOutputFormatter(outputDescriptionInstance, "root");
        }

    }
}
