using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Warewolf.Resource.Errors;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Dev2.Activities.Designers2.SharePointFileUpload
{
    public class SharePointFileUploadDesignerViewModel : SharepointListDesignerViewModelBase
    {
        public SharePointFileUploadDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment)
        {
        }

        public SharePointFileUploadDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel envModel)
            : base(modelItem, asyncWorker, envModel, EventPublishers.Aggregator, false)
        {
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Upload_File;
        }

        #region Overrides of ActivityCollectionDesignerViewModel<SharepointSearchTo>

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
        public string LocalInputPath => GetProperty<string>();

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

            if (string.IsNullOrEmpty(LocalInputPath))
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = ErrorResource.LocalPathRequired } };

                return Errors;
            }

            return new List<IActionableErrorInfo>();
        }
        #endregion

        public override string CollectionName => "FilterCriteria";
    }
}
