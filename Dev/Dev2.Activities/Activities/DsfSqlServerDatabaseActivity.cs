using System;
using System.ComponentModel;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.DataList.Contract;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
// ReSharper disable ConvertToAutoProperty

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Resources-Service", "Sql Server Connector", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Resources", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfSqlServerDatabaseActivity : DsfActivity
    {

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] 
        public IServiceExecution ServiceExecution { get; protected set; }
        public string ProcedureName { get; set; }

        public DsfSqlServerDatabaseActivity()
        {
            Type = "Sql Server Database Connector";
            DisplayName = "Sql Server Database Connector";
        }

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            var execErrors = new ErrorResultTO();

            errors = new ErrorResultTO();
            errors.MergeErrors(execErrors);

            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            if(databaseServiceExecution != null)
            {
                databaseServiceExecution.Inputs = Inputs;
                databaseServiceExecution.Outputs = Outputs;
            }
            var result = ServiceExecution.Execute(out execErrors, update);
            var fetchErrors = execErrors.FetchErrors();
            foreach(var error in fetchErrors)
            {
                dataObject.Environment.Errors.Add(error);
            }
            errors.MergeErrors(execErrors);
            return result;
        }

        protected override void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            base.BeforeExecutionStart(dataObject, tmpErrors);
            ServiceExecution = new DatabaseServiceExecution(dataObject);
            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            databaseServiceExecution.ProcedureName = ProcedureName;
            ServiceExecution.GetSource(SourceId);
            ServiceExecution.BeforeExecution(tmpErrors);
        }

        protected override void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            base.AfterExecutionCompleted(tmpErrors);
            ServiceExecution.AfterExecution(tmpErrors);
        }


        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

    }
}