using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core.Interfaces;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowDesignerViewModelTestClass : WorkflowDesignerViewModel
    {    
        public WorkflowDesignerViewModelTestClass(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = false)
            : base(resource, workflowHelper, createDesigner)
        {
        }
        public void TestCheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel environmentModel)
        {
            CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource,environmentModel);
        }
    }
}
