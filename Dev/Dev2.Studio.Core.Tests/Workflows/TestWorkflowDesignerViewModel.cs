using System;
using System.Activities.Presentation.Services;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utilities;

namespace Dev2.Core.Tests.Workflows
{
    public class TestWorkflowDesignerViewModel : WorkflowDesignerViewModel
    {
        public TestWorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            : base(resource, workflowHelper, createDesigner)
        {
        }

        public void TestModelServiceModelChanged(ModelChangedEventArgs e)
        {
            base.ModelServiceModelChanged(null, e);
        }

        public void TestWorkflowDesignerModelChanged()
        {
            base.WdOnModelChanged(null,new EventArgs());
        }
        
        public void SetDataObject(dynamic dataobject)
        {
            DataObject = dataobject;
        }
    }
}
