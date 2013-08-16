using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utilities;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public class WorkflowDesignerViewModelMock : WorkflowDesignerViewModel
    {
        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = false)
            : base(
                new Mock<IEventAggregator>().Object,
                resource, workflowHelper,
                new Mock<IFrameworkSecurityContext>().Object,
                new Mock<IPopupController>().Object,
                new Mock<IWizardEngine>().Object, createDesigner)
        {
        }        
        
        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper,IEventAggregator eventAggregator, bool createDesigner = false)
            : base(
                eventAggregator,
                resource, workflowHelper,
                new Mock<IFrameworkSecurityContext>().Object,
                new Mock<IPopupController>().Object,
                new Mock<IWizardEngine>().Object, createDesigner)
        {
        }

        protected override bool IsDesignerViewVisible
        {
            get
            {
                return true;
            }
        }

        public void TestCheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel environmentModel)
        {
            CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource, environmentModel);
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

        public void TestHandleMouseClick(DependencyObject dp, DesignerView dv)
        {
            HandleMouseClick(MouseButtonState.Pressed, 2, dp, dv);
        }
    }
}
