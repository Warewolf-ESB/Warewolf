using System.Activities;
using System.Activities.Presentation.Services;

namespace Dev2.Utilities
{
    // BUG 9304 - 2013.05.08 - TWR - Added this
    public interface IWorkflowHelper
    {
        string SerializeWorkflow(ModelService modelService);

        ActivityBuilder CreateWorkflow(string displayName);

        ActivityBuilder EnsureImplementation(ModelService modelService);

        void CompileExpressions(DynamicActivity dynamicActivity);

        void CompileExpressions<TResult>(DynamicActivity<TResult> dynamicActivity);

        string SanitizeXaml(string workflowXaml);
    }
}