using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.ViewModels.Merge
{
    public class DifferenceConflictViewModel : CurrentConflictViewModelBase
    {
        public DifferenceConflictViewModel(IApplicationAdaptor applicationAdaptor, IEnumerable<ModelItem> modelItems)
            : base(applicationAdaptor, modelItems)
        {
            string newWorflowName = NewWorkflowNames.Instance.GetNext();
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(shellViewModel.ActiveServer, @"WorkflowService",
                newWorflowName);
            tempResource.Category = @"Unassigned\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(tempResource) as DataListViewModel;
            DataListViewModel.ViewSortDelete = false;
        }


    }
}