using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Threading;
using Dev2.Providers.Errors;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Threading;
using Warewolf.Resource.Errors;
using Dev2.Studio.Interfaces;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Dev2.Activities.Designers2.SharePointCopyFile
{
    public class SharePointCopyFileDesignerViewModel : SharepointListDesignerViewModelBase
    {
        public SharePointCopyFileDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), ServerRepository.Instance.ActiveServer)
        {
        }

        public SharePointCopyFileDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IServer envModel)
            : base(modelItem, asyncWorker, envModel, EventPublishers.Aggregator, false)
        {
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Copy_File;
        }

        #region Overrides of ActivityCollectionDesignerViewModel<SharepointSearchTo>

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public string ServerInputPathFrom => GetProperty<string>();
        public string ServerInputPathTo => GetProperty<string>();

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            if (Errors != null && Errors.Count > 0)
            {
                Errors.Clear();
            }

            if (SharepointServerResourceId == Guid.Empty)
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = ErrorResource.SharepointServerRequired } };

                return Errors;
            }

            if (string.IsNullOrEmpty(ServerInputPathFrom))
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = ErrorResource.SharepointServerPathRequired } };

                return Errors;
            }

            if (string.IsNullOrEmpty(ServerInputPathTo))
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = ErrorResource.SharepointServerPathToRequired } };

                return Errors;
            }

            return new List<IActionableErrorInfo>();
        }

        #endregion

        public override string CollectionName => "FilterCriteria";
    }
}
