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
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Dev2.Activities.Designers2.SharepointFolderRead
{
    public class SharePointReadFolderDesignerViewModel : SharepointListDesignerViewModelBase
    {
        public SharePointReadFolderDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), EnvironmentRepository.Instance.ActiveEnvironment)
        {
            if (!IsFilesAndFoldersSelected && !IsFoldersSelected && !IsFilesSelected)
            {
                IsFilesSelected = true;
            }
        }

        public SharePointReadFolderDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel envModel)
            : base(modelItem, asyncWorker, envModel, EventPublishers.Aggregator)
        {
            
        }


        #region Overrides of ActivityCollectionDesignerViewModel<SharepointSearchTo>

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }
        
        public bool IsFilesAndFoldersSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        public bool IsFoldersSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        public bool IsFilesSelected { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        public string ServerInputPath { get { return GetProperty<string>(); } }

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            if (Errors != null && Errors.Count > 0)
            {
                Errors.Clear();
            }
           
            if (SharepointServerResourceId == Guid.Empty)
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() { Message = "Please SharePoint source" } };

                return Errors;
            }

            return new List<IActionableErrorInfo>();
        }

        #endregion

        public override string CollectionName { get { return "FilterCriteria"; } }
    }
}
