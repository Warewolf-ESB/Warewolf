using System.ComponentModel;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
// ReSharper disable NonLocalizedString

// ReSharper disable InconsistentNaming

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Odbc", "ODBC", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EE11", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfODBCDatabaseActivity : DsfActivity
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IServiceExecution ServiceExecution { get; protected set; }
        public string CommandText { get; set; }

        public DsfODBCDatabaseActivity()
        {
            Type = "ODBC Connector";
            DisplayName = "ODBC Data Source";
            CommandText = "";
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors, int update)
        {
            var execErrors = new ErrorResultTO();

            errors = new ErrorResultTO();
            errors.MergeErrors(execErrors);
            if (string.IsNullOrEmpty(CommandText))
            {
                errors.AddError(ErrorResource.NoActionsInSelectedDB);
                return;
            }
            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            if (databaseServiceExecution != null)
            {
                databaseServiceExecution.Inputs = Inputs;
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

        protected override void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {

            base.BeforeExecutionStart(dataObject, tmpErrors);
            ServiceExecution = new DatabaseServiceExecution(dataObject);
            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            databaseServiceExecution.ProcedureName = databaseServiceExecution.ODBCMethod(CommandText);

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
