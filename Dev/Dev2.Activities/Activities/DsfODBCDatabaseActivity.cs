#pragma warning disable
﻿using System;
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
    [ToolDescriptorInfo("Odbc", "ODBC", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EE11", "Dev2.Activities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_ODBC")]
    public class DsfODBCDatabaseActivity : DsfActivity,IEquatable<DsfODBCDatabaseActivity>
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceExecution ServiceExecution { get; protected set; }
        public string CommandText { get; set; }
        public int? CommandTimeout { get; set; }
        public DsfODBCDatabaseActivity()
        {
            Type = "ODBC Connector";
            DisplayName = "ODBC Data Source";
            CommandText = "";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            var execErrors = new ErrorResultTO();

            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(execErrors);
            if (string.IsNullOrEmpty(CommandText))
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

            var databaseServiceExecution = new DatabaseServiceExecution(dataObject);
            if (CommandTimeout != null)
            {
                databaseServiceExecution.CommandTimeout = CommandTimeout.Value;
            }
            databaseServiceExecution.ProcedureName = databaseServiceExecution.OdbcMethod(CommandText);
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

        public bool Equals(DsfODBCDatabaseActivity other)
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
                && (SourceId != null && SourceId.Equals(other.SourceId))
                && string.Equals(CommandText, other.CommandText);
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

            return Equals((DsfODBCDatabaseActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SourceId.GetHashCode() );
                hashCode = (hashCode * 397) ^ (CommandText.GetHashCode() );
                return hashCode;
            }
        }
    }
}
