#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Common;
using Dev2.Common.X6;
using Newtonsoft.Json.Linq;
using Dev2.WorkflowConverters;


namespace Dev2.Activities
{
    [ToolDescriptorInfo("MicrosoftSQL", "SQL Server", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_SQL_Server")]
    public class DsfSqlServerDatabaseActivity : DsfActivity, IEquatable<DsfSqlServerDatabaseActivity>
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceExecution ServiceExecution { get; protected set; }

        public string ProcedureName { get; set; }
        public int? CommandTimeout { get; set; }

        public string ExecuteActionString { get; set; }

        public DsfSqlServerDatabaseActivity()
        {
            Type = "SQL Server Database";
            DisplayName = "SQL Server Database";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            var execErrors = new ErrorResultTO();
            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(execErrors);
            try
            {
                if (string.IsNullOrEmpty(ProcedureName))
                {
                    tmpErrors.AddError(ErrorResource.NoActionsInSelectedDB);
                    DisplayAndWriteError(dataObject, DisplayName, tmpErrors);
                    return;
                }

                // Diagnostics for WOLF-8510: under concurrent load a minority of executions fail
                // with a bare "Object reference not set to an instance of an object.", which the
                // catch below reports as ex.Message with the stack discarded - leaving nothing in
                // Application Insights but a stackless System.Exception. Name the null member
                // explicitly instead of letting a NullReferenceException escape anonymously.
                // Note the type check below silently no-ops when ServiceExecution is null, so
                // without this guard the failure only surfaces at ServiceExecution.Execute.
                if (ServiceExecution is null || Inputs is null || Outputs is null)
                {
                    var missing = new List<string>();
                    if (ServiceExecution is null)
                    {
                        missing.Add(nameof(ServiceExecution));
                    }
                    if (Inputs is null)
                    {
                        missing.Add(nameof(Inputs));
                    }
                    if (Outputs is null)
                    {
                        missing.Add(nameof(Outputs));
                    }

                    var detail =
                        $"SQL Server activity '{DisplayName}' (UniqueID={UniqueID}, ProcedureName='{ProcedureName}', SourceId={SourceId}) " +
                        $"cannot execute: {string.Join(", ", missing)} {(missing.Count == 1 ? "is" : "are")} null. " +
                        $"ExecutionID={dataObject?.ExecutionID}.";

                    Dev2Logger.Error(detail, dataObject?.ExecutionID.ToString());
                    execErrors.AddError(detail);
                    return;
                }

                if (ServiceExecution is DatabaseServiceExecution databaseServiceExecution)
                {
                    if (databaseServiceExecution.SourceIsNull())
                    {
                        databaseServiceExecution.GetSource(SourceId);
                    }

                    databaseServiceExecution.Inputs = Inputs.Select(a => new ServiceInput { EmptyIsNull = a.EmptyIsNull, Name = a.Name, RequiredField = a.RequiredField, Value = a.Value, TypeName = a.TypeName } as IServiceInput).ToList();
                    databaseServiceExecution.Outputs = Outputs;
                }

                ServiceExecution.Execute(out execErrors, update);
            }
            catch (Exception ex)
            {
                // Log the exception object itself (not just ex.Message) so the type and stack
                // reach Application Insights - AddError below deliberately keeps the original
                // message-only text so downstream error handling and tests are unaffected.
                Dev2Logger.Error(
                    $"SQL Server activity '{DisplayName}' (UniqueID={UniqueID}, ProcedureName='{ProcedureName}', ExecutionID={dataObject?.ExecutionID}) threw {ex.GetType().FullName}.",
                    ex,
                    GlobalConstants.WarewolfError);
                execErrors.AddError(ex.Message);
            }
            finally
            {
                if (execErrors.HasErrors())
                {
                    if (!this.IsErrorHandled)
                    {
                        var fetchErrors = execErrors.FetchErrors();
                        foreach (var error in fetchErrors)
                        {
                            dataObject.Environment.Errors.Add(error);
                        }
                    }
                }

                tmpErrors.MergeErrors(execErrors);
                execErrors.ClearErrors();
            }
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }

            base.GetDebugInputs(env, update);

            if (Inputs != null)
            {
                foreach (var serviceInput in Inputs)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugEvalResult(serviceInput.Value, serviceInput.Name, env, update), debugItem);
                    _debugInputs.Add(debugItem);
                }
            }

            return _debugInputs;
        }

        protected override void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            base.BeforeExecutionStart(dataObject, tmpErrors);
            var databaseServiceExecution = new DatabaseServiceExecution(dataObject)
            {
                ProcedureName = ProcedureName,
            };
            if (CommandTimeout != null)
            {
                databaseServiceExecution.CommandTimeout = CommandTimeout.Value;
            }

            if (!string.IsNullOrEmpty(ExecuteActionString))
            {
                databaseServiceExecution.ProcedureName = ExecuteActionString;
            }

            ServiceExecution = databaseServiceExecution;
            ServiceExecution.GetSource(SourceId);
            ServiceExecution.BeforeExecution(tmpErrors);
        }


        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(DsfSqlServerDatabaseActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var eq = base.Equals(other);
            eq &= string.Equals(SourceId.ToString(), other.SourceId.ToString());
            eq &= string.Equals(ProcedureName, other.ProcedureName);
            eq &= CommandTimeout == other.CommandTimeout;
            eq &= string.Equals(ExecuteActionString, other.ExecuteActionString);
            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfSqlServerDatabaseActivity instance)
            {
                return Equals(instance);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SourceId.GetHashCode());
                if (ProcedureName != null)
                {
                    hashCode = (hashCode * 397) ^ (ProcedureName.GetHashCode());
                }

                if (CommandTimeout != null)
                {
                    hashCode = (hashCode * 397) ^ CommandTimeout.Value;
                }

                if (ExecuteActionString != null)
                {
                    hashCode = (hashCode * 397) ^ (ExecuteActionString.GetHashCode());
                }

                return hashCode;
            }
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new System.Collections.Generic.Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.SQLSERVERDATABASEACTIVITY;
            cell.data[Constants.TYPE] = Constants.SQLSERVERDATABASEACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_SQLSERVERDATABASE;
            cell.data[Constants.UNIQUEID] = UniqueID;

            cell.data.TryAdd(Constants.DATABASE_PROCEDURENAME, ProcedureName);
            cell.data.TryAdd(Constants.DATABASE_EXECUTEACTIONSTRING, ExecuteActionString);
            cell.data.TryAdd(Constants.DATABASE_SERVICESERVER, ServiceServer);
            cell.data.TryAdd(Constants.WEBMETHOD_SOURCEID, SourceId);
            cell.data.TryAdd(Constants.DATABASE_COMMANDTIMEOUT, CommandTimeout);
            
            cell.data.TryAdd(Constants.WEBMETHOD_ISOBJECT, IsObject);
            cell.data.TryAdd(Constants.WEBMETHOD_OBJECTNAME, ObjectName);
            cell.data.TryAdd(Constants.WEBMETHOD_OBJECTRESULT, ObjectResult);

            cell.data.TryAdd(Constants.WEBMETHOD_INPUTS, Inputs);
            cell.data.TryAdd(Constants.WEBMETHOD_OUTPUTS, Outputs);
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) UniqueID = uniqueId;

            if (cell.data.TryGetString(Constants.DATABASE_PROCEDURENAME, out var procedureName)) ProcedureName = procedureName;
            if (cell.data.TryGetString(Constants.DATABASE_EXECUTEACTIONSTRING, out var execAction)) ExecuteActionString = execAction;
            if (cell.data.TryGetGuid(Constants.DATABASE_SERVICESERVER, out var serviceServer)) ServiceServer = serviceServer;
            if (cell.data.TryGetGuid(Constants.WEBMETHOD_SOURCEID, out var sourceId)) SourceId = sourceId;
            if (cell.data.TryGetInt(Constants.DATABASE_COMMANDTIMEOUT, out var commandtimeout)) CommandTimeout = commandtimeout;

            if (cell.data.TryGetBool(Constants.WEBMETHOD_ISOBJECT, out var isObject)) IsObject = isObject;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTNAME, out var objectName)) ObjectName = objectName;
            if (cell.data.TryGetString(Constants.WEBMETHOD_OBJECTRESULT, out var objectResult)) ObjectResult = objectResult;

            if (cell.data.TryGetInputs(out var inputs)) Inputs = inputs;
            if (cell.data.TryGetOutputs(out var outputs)) Outputs = outputs;
        }
    }
}