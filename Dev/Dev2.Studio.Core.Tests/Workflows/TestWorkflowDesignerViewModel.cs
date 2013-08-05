using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation;
using System.Activities.Presentation.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Workflow;
using Dev2.Utilities;

namespace Dev2.Core.Tests.Workflows
{
    public class TestWorkflowDesignerViewModel : WorkflowDesignerViewModel
    {
        public TestWorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            : base(resource, workflowHelper, createDesigner)
        {
        }

        protected override bool IsDesignerViewVisible
        {
            get
            {
                return true;
            }
        }

        public void TestModelServiceModelChanged(ModelChangedEventArgs e)
        {
            base.ModelServiceModelChanged(null, e);
        }

        public void TestWorkflowDesignerModelChanged()
        {
            base.WdOnModelChanged(new object(), new EventArgs());
        }


        public void TestWorkflowDesignerModelChangedWithNullSender()
        {
            base.WdOnModelChanged(null, new EventArgs());
        }

        public void SetDataObject(dynamic dataobject)
        {
            DataObject = dataobject;
        }

        public int SelectModelItemHitCount { get; set; }
        public ModelItem SelectModelItemValue { get; set; }

        protected override void SelectModelItem(ModelItem selectedModelItem)
        {
            SelectModelItemHitCount++;
            SelectModelItemValue = selectedModelItem;
            base.SelectModelItem(selectedModelItem);
        }

        public int BringIntoViewHitCount { get; set; }
        protected override void BringIntoView(ModelItem selectedModelItem)
        {
            BringIntoViewHitCount++;
            base.BringIntoView(selectedModelItem);
        }
    }
}
