#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Views.ResourceManagement;

namespace Dev2.Utils
{
    public interface IResourceChangeHandler
    {
        void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants);
        void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants, IResourceChangedDialog resourceChangedDialog);
    }

    public interface IResourceChangeHandlerFactory
    {
        IResourceChangeHandler Create(IEventAggregator eventPublisher);
    }

    [ExcludeFromCodeCoverage]
    public class ResourceChangeHandlerFactory : IResourceChangeHandlerFactory
    {
        public IResourceChangeHandler Create(IEventAggregator eventPublisher) => new ResourceChangeHandler(eventPublisher);
    }

    public class ResourceChangeHandler : IResourceChangeHandler
    {
        readonly IEventAggregator _eventPublisher;
        public ResourceChangeHandler(IEventAggregator eventPublisher)
        {
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException("eventPublisher");
        }

        public void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants) => ShowResourceChanged(resource, numberOfDependants, null);
        public void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants, IResourceChangedDialog resourceChangedDialog)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            if(numberOfDependants == null)
            {
                throw new ArgumentNullException("numberOfDependants");
            }
            if(resourceChangedDialog == null)
            {
                resourceChangedDialog = new ResourceChangedDialog(resource, numberOfDependants.Count);
            }
            resourceChangedDialog.ShowDialog();
            if(resourceChangedDialog.OpenDependencyGraph)
            {
                if(numberOfDependants.Count == 1)
                {
                    var shellViewModel = CustomContainer.Get<IShellViewModel>();
                    shellViewModel.OpenResourceAsync(Guid.Parse(numberOfDependants[0]),shellViewModel.ActiveServer);                    
                }
                else
                {
                    Dev2Logger.Info("Publish message of type - " + typeof(ShowReverseDependencyVisualizer), "Warewolf Info");
                    _eventPublisher.Publish(new ShowReverseDependencyVisualizer(resource));
                }
            }
        }
    }
}
