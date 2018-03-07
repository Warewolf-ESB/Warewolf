/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Threading;
using Dev2.Utilities;
using Moq;

namespace Dev2.Core.Tests.Workflows
{
    public class MergePreviewWorkflowDesignerViewModelMock : MergePreviewWorkflowDesignerViewModel
    {
        public MergePreviewWorkflowDesignerViewModelMock(IWorkflowDesignerWrapper workflowDesignerWrapper, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IEventAggregator eventAggregator, WorkflowDesigner workflowDesigner, bool createDesigner = false)
           : base(workflowDesignerWrapper, eventAggregator, resource, workflowHelper, new Mock<IPopupController>().Object, new SynchronousAsyncWorker(), createDesigner, false)
        {
            _wd = workflowDesigner;
            _workflowDesignerHelper = workflowDesignerWrapper;
        }
    }
}
