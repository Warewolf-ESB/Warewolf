using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utilities;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public class WorkflowDesignerViewModelMock : WorkflowDesignerViewModel
    {

        Mock<WorkflowDesigner> moq = new Mock<WorkflowDesigner>();

        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = false)
            : base(
                new Mock<IEventAggregator>().Object,
                resource, workflowHelper,
                new Mock<IPopupController>().Object,
                createDesigner)
        {
            moq.SetupAllProperties();
            _wd = moq.Object;
        }

        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper, IEventAggregator eventAggregator, bool createDesigner = false)
            : base(
                eventAggregator,
                resource, workflowHelper,
                new Mock<IPopupController>().Object, createDesigner)
        {
            moq.SetupAllProperties();
            _wd = moq.Object;
        }

        bool _isDesignerViewVisible = true;
        protected override bool IsDesignerViewVisible
        {
            get
            {
                return _isDesignerViewVisible;
            }
        }

        public void SetIsDesignerViewVisible(bool isVisible)
        {
            _isDesignerViewVisible = isVisible;
        }

        public List<ModelItem> SelectedDebugModelItems
        {
            get
            {
                return SelectedDebugItems;
            }
        }

        public void TestCheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel environmentModel)
        {
            CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource, environmentModel);
        }

        public void TestModelServiceModelChanged(ModelChangedEventArgs e)
        {
            ModelServiceModelChanged(null, e);
        }

        public void TestWorkflowDesignerModelChanged()
        {
            WdOnModelChanged(new object(), new EventArgs());
        }


        public void TestWorkflowDesignerModelChangedWithNullSender()
        {
            WdOnModelChanged(null, new EventArgs());
        }

        public string FetchDesignerText()
        {
            return _wd.Text;
        }

        public void VerifyLoadCalled()
        {
            moq.Verify(w => w.Load(It.IsAny<ActivityBuilder>()), Times.Once());
        }

        public void LoadXaml()
        {

            base.LoadDesignerXaml();
        }

        public void SetDataObject(dynamic dataobject)
        {
            DataObject = dataobject;
        }



        public int GetSelectedModelItemHitCount { get; private set; }
        protected override ModelItem GetSelectedModelItem(Guid itemID, Guid parentID)
        {
            GetSelectedModelItemHitCount++;
            return base.GetSelectedModelItem(itemID, parentID);
        }


        public int BringIntoViewHitCount { get; private set; }
        protected override void BringIntoView(ModelItem selectedModelItem)
        {
            BringIntoViewHitCount++;
            base.BringIntoView(selectedModelItem);
        }

        public void TestHandleMouseClick(DependencyObject dp, DesignerView dv)
        {
            HandleMouseClick(MouseButtonState.Pressed, 2, dp, dv);
        }

        public ModelItem TestPerformAddItems(ModelItem modelItems)
        {
            return PerformAddItems(new List<ModelItem> { modelItems }).FirstOrDefault();
        }
    }
}
