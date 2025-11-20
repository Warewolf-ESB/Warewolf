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

using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.X6;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Dev2.WorkflowConverters;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.WorkflowConverters;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Database", "Oracle", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA10", "Dev2.Activities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_Oracle")]
    public class DsfOracleDatabaseActivity : DsfActivity,IEquatable<DsfOracleDatabaseActivity>
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceExecution ServiceExecution { get; protected set; }
        public string ProcedureName { get; set; }
        public int? CommandTimeout { get; set; }

        public DsfOracleDatabaseActivity()
        {
            Type = "Oracle Connector";
            DisplayName = "Oracle Database";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            var execErrors = new ErrorResultTO();

            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(execErrors);
            if (string.IsNullOrEmpty(ProcedureName))
            {
                tmpErrors.AddError(ErrorResource.NoActionsInSelectedDB);
                return;
            }
            if (ServiceExecution is DatabaseServiceExecution databaseServiceExecution)
            {
                databaseServiceExecution.Inputs = Inputs.Select(a => new ServiceInput { EmptyIsNull = a.EmptyIsNull, Name = a.Name, RequiredField = a.RequiredField, Value = a.Value, TypeName = a.TypeName } as IServiceInput).ToList();
                databaseServiceExecution.Outputs = Outputs;
            }
            ServiceExecution.Execute(out execErrors, update);
            var fetchErrors = execErrors.FetchErrors();
            foreach (var error in fetchErrors)
            {
                dataObject.Environment.Errors.Add(error);
            }
            tmpErrors.MergeErrors(execErrors);
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
            ServiceExecution = databaseServiceExecution;
            ServiceExecution.GetSource(SourceId);
            ServiceExecution.BeforeExecution(tmpErrors);
        }

        protected override void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            base.AfterExecutionCompleted(tmpErrors);
            ServiceExecution.AfterExecution(tmpErrors);
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public bool Equals(DsfOracleDatabaseActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && string.Equals(SourceId.ToString(), other.SourceId.ToString())
                && string.Equals(ProcedureName, other.ProcedureName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfOracleDatabaseActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ SourceId.GetHashCode();
                if (ProcedureName != null)
                {
                    hashCode = (hashCode * 397) ^ ProcedureName.GetHashCode();
                }
                return hashCode;
            }
        }
        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new System.Collections.Generic.Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.ORACLESQLDATABASEACTIVITY;
            cell.data[Constants.TYPE] = Constants.ORACLESQLDATABASEACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_ORACLESQLDATABASE;
            cell.data[Constants.UNIQUEID] = UniqueID;

            cell.data.TryAdd(Constants.DATABASE_PROCEDURENAME, ProcedureName);
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
