
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Views.ResourceManagement;

namespace Dev2.Utils
{
    public interface IResourceChangeHandler
    {
        void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants, IResourceChangedDialog resourceChangedDialog = null);
    }

    public interface IResourceChangeHandlerFactory
    {
        IResourceChangeHandler Create(IEventAggregator eventPublisher);
    }

    public  class ResourceChangeHandlerFactory : IResourceChangeHandlerFactory
    {
        #region Implementation of IResourceChangeHandlerFactory

        public IResourceChangeHandler Create(IEventAggregator eventPublisher)
        {
            return new ResourceChangeHandler(eventPublisher);
        }

        #endregion
    }

    public class ResourceChangeHandler : IResourceChangeHandler
    {
        readonly IEventAggregator _eventPublisher;
        public ResourceChangeHandler(IEventAggregator eventPublisher)
        {
            if(eventPublisher == null)
            {
                throw new ArgumentNullException("eventPublisher");
            }
            _eventPublisher = eventPublisher;
        }

        public void ShowResourceChanged(IContextualResourceModel resource, IList<string> numberOfDependants, IResourceChangedDialog resourceChangedDialog = null)
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
                    var resourceModel = resource.Environment.ResourceRepository.FindSingle(model => model.ResourceName == numberOfDependants[0]);
                    if(resourceModel != null)
                    {
                        WorkflowDesignerUtils.EditResource(resourceModel, _eventPublisher);
                    }
                }
                else
                {
                    Dev2Logger.Log.Info("Publish message of type - " + typeof(ShowReverseDependencyVisualizer));
                    _eventPublisher.Publish(new ShowReverseDependencyVisualizer(resource));
                }
            }
        }
    }
}
