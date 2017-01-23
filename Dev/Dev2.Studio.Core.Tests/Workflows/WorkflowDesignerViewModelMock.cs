/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Threading;
using Dev2.Utilities;
using Dev2.Utils;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public class WorkflowDesignerViewModelMock : WorkflowDesignerViewModel
    {
        readonly Mock<WorkflowDesigner> _moq = new Mock<WorkflowDesigner>();

        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper, IExternalProcessExecutor processExecutor, bool createDesigner = false)
            : base(
                new Mock<IEventAggregator>().Object,
                resource, workflowHelper,
                new Mock<IPopupController>().Object,
                new SynchronousAsyncWorker(), processExecutor,
                createDesigner, false)
        {
            _moq.SetupAllProperties();
            _wd = _moq.Object;
        }

        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper, IEventAggregator eventAggregator, bool createDesigner = false)
            : base(
                eventAggregator,
                resource, workflowHelper,
                new Mock<IPopupController>().Object, new SynchronousAsyncWorker(), new Mock<IExternalProcessExecutor>().Object, createDesigner, false)
        {
            _moq.SetupAllProperties();
            _wd = _moq.Object;
        }

        public WorkflowDesignerViewModelMock(IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, IExternalProcessExecutor processExecutor, bool createDesigner = false)
            : base(
                new Mock<IEventAggregator>().Object,
                resource, workflowHelper,
                popupController, new SynchronousAsyncWorker(), processExecutor, createDesigner, false)
        {
            _moq.SetupAllProperties();
            _wd = _moq.Object;
        }

        bool _isDesignerViewVisible = true;

        protected override bool IsDesignerViewVisible => _isDesignerViewVisible;

        public void SetActiveEnvironment(IEnvironmentModel environmentModel)
        {
            ActiveEnvironment = environmentModel;
        }

        public IEnvironmentModel GetActiveEnvironment()
        {
            return ActiveEnvironment;
        }

        public void SetIsDesignerViewVisible(bool isVisible)
        {
            _isDesignerViewVisible = isVisible;
        }

        public List<ModelItem> SelectedDebugModelItems => SelectedDebugItems;

        public void TestCheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel environmentModel)
        {
            WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource, environmentModel);
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

        public void LoadXaml()
        {

            LoadDesignerXaml();
        }

        public void SetDataObject(dynamic dataobject)
        {
            DataObject = dataobject;
        }

        public bool SetApplyForDrop(IDataObject dataObject)
        {
            return ApplyForDrop(dataObject);
        }

        public void SetupGetWorkflowFieldsFromFlowNodes(IEnumerable<ModelItem> flowNodes)
        {
            GetWorkflowFieldsFromFlowNodes(flowNodes);
        }

        public void SetupRequestExapandAll()
        {
            RequestedExpandAll = false;
            DesignerManagementService.ExpandAllRequested += (sender, args) =>
            {
                RequestedExpandAll = true;
            };
        }

        public void SetupRequestRestoreAll()
        {
            RequestedExpandAll = false;
            DesignerManagementService.RestoreAllRequested += (sender, args) =>
            {
                RequestedRestoreAll = true;
            };
        }

        public void SetupRequestCollapseAll()
        {
            RequestedExpandAll = false;
            DesignerManagementService.CollapseAllRequested += (sender, args) =>
            {
                RequestedCollapseAll = true;
            };
        }

        public bool RequestedExpandAll { get; set; }
        public bool RequestedRestoreAll { get; set; }
        public bool RequestedCollapseAll { get; set; }

        public int GetSelectedModelItemHitCount { get; private set; }
        protected override ModelItem GetSelectedModelItem(Guid itemId, Guid parentId)
        {
            GetSelectedModelItemHitCount++;
            return base.GetSelectedModelItem(itemId, parentId);
        }

        public int BringIntoViewHitCount { get; private set; }

        protected override void BringIntoView(ModelItem selectedModelItem)
        {
            BringIntoViewHitCount++;
            base.BringIntoView(selectedModelItem);
        }

        public ModelItem TestPerformAddItems(ModelItem modelItems)
        {
            return PerformAddItems(new List<ModelItem> { modelItems }).FirstOrDefault();
        }

        public void FireWorkflowChanged()
        {
            WorkflowChanged.Invoke();
        }
    }
}
