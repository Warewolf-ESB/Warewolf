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



namespace Dev2.Activities
{
    [ToolDescriptorInfo("MicrosoftSQL", "SQL Server", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_SQL_Server")]
    public class DsfSqlServerDatabaseActivity : DsfActivity,IEquatable<DsfSqlServerDatabaseActivity>
    {

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceExecution ServiceExecution { get; protected set; }
        public string ProcedureName { get; set; }

        public string ExecuteActionString { get; set; }
        public DsfSqlServerDatabaseActivity()
        {
            Type = "SQL Server Database";
            DisplayName = "SQL Server Database";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            var execErrors = new ErrorResultTO();

            errors = new ErrorResultTO();
            errors.MergeErrors(execErrors);
            if (string.IsNullOrEmpty(ProcedureName))
            {
                errors.AddError(ErrorResource.NoActionsInSelectedDB);
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
            var fetchErrors = execErrors.FetchErrors();
            foreach (var error in fetchErrors)
            {
                dataObject.Environment.Errors.Add(error);
            }
            errors.MergeErrors(execErrors);
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
            ServiceExecution = new DatabaseServiceExecution(dataObject);
            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            databaseServiceExecution.ProcedureName = ProcedureName;
            if (!string.IsNullOrEmpty(ExecuteActionString))
            {
                databaseServiceExecution.ProcedureName = ExecuteActionString;
            }

            ServiceExecution.GetSource(SourceId);
        }

        protected override void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            base.AfterExecutionCompleted(tmpErrors);
        }


        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

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

            return base.Equals(other)
                && string.Equals(SourceId.ToString(), other.SourceId.ToString())
                && string.Equals(ProcedureName, other.ProcedureName) 
                && string.Equals(ExecuteActionString, other.ExecuteActionString);
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

            return Equals((DsfSqlServerDatabaseActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SourceId != null ? SourceId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProcedureName != null ? ProcedureName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExecuteActionString != null ? ExecuteActionString.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}