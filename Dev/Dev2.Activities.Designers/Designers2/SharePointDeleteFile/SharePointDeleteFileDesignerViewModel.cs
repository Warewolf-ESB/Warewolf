/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Activities.Designers2.SharePointDeleteFile
{
    public class SharePointDeleteFileDesignerViewModel : SharepointListDesignerViewModelBase
    {
        private readonly IShellViewModel _shellViewModel;

        [ExcludeFromCodeCoverage]
        public SharePointDeleteFileDesignerViewModel(ModelItem modelItem)
            : this(modelItem, new AsyncWorker(), ServerRepository.Instance.ActiveServer, CustomContainer.Get<IShellViewModel>())
        {
        }

        public SharePointDeleteFileDesignerViewModel(ModelItem modelItem, IAsyncWorker asyncWorker, IServer envModel, IShellViewModel shellViewModel)
            : base(modelItem, asyncWorker, envModel, EventPublishers.Aggregator, false)
        {
            _shellViewModel = shellViewModel;
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_SharePoint_Delete_File;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            _shellViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        string ServerInputPath => GetProperty<string>();

        protected override IEnumerable<IActionableErrorInfo> ValidateThis()
        {
            if (Errors != null && Errors.Count > 0)
            {
                Errors.Clear();
            }

            if (SharepointServerResourceId == Guid.Empty)
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo
                    {
                        Message = ErrorResource.SharepointServerRequired
                    }
                };
                return Errors;
            }

            if (string.IsNullOrEmpty(ServerInputPath))
            {
                Errors = new List<IActionableErrorInfo>
                {
                    new ActionableErrorInfo
                    {
                        Message = ErrorResource.SharepointServerPathRequired
                    }
                };
                return Errors;
            }

            return new List<IActionableErrorInfo>();
        }

        public override string CollectionName => "FilterCriteria";
    }
}
