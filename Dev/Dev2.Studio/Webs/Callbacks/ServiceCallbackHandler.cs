
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
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
{
    public class ServiceCallbackHandler : WebsiteCallbackHandler
    {
        bool _isEditingSource;
        string _returnUri;
        IEnvironmentModel _environmentModel;

        public ServiceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public ServiceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : base(EventPublishers.Aggregator, currentEnvironmentRepository)
        {
        }

        public ServiceCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, IShowDependencyProvider provider)
            : base(eventPublisher, currentEnvironmentRepository, provider)
        {
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            // NOTE : When using dynamics be very careful!
            try
            {
                Guid resourceID;
                if(Guid.TryParse(jsonObj.ResourceID.Value, out resourceID))
                {
                    _environmentModel = environmentModel;
                    var getDynamicResourceType = jsonObj.ResourceType.Value;
                    if(getDynamicResourceType != null)
                    {
                        //2013.04.29: Ashley Lewis - PBI 8721 database source and plugin source wizards can be called from with their respective service wizards
                        if(getDynamicResourceType == Common.Interfaces.Data.ResourceType.DbSource.ToString() ||
                            getDynamicResourceType == Common.Interfaces.Data.ResourceType.PluginSource.ToString() ||
                            getDynamicResourceType == Common.Interfaces.Data.ResourceType.WebSource.ToString())
                        {
                            //2013.03.12: Ashley Lewis - BUG 9208
                            ReloadResource(environmentModel, resourceID, ResourceType.Source);
                        }
                        else
                        {
                            ReloadResource(environmentModel, resourceID, ResourceType.Service);
                        }
                    }
                    else
                    {
                        ReloadResource(environmentModel, resourceID, ResourceType.Service);
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
        }

        public override void Cancel()
        {
            if(_isEditingSource)
            {
                _isEditingSource = false;
                Navigate(_returnUri);
            }
            else
            {
                Close();
            }
        }

        public override void Dev2SetValue(string value)
        {
            if(_isEditingSource)
            {
                // DB source invokes this
                var xml = XElement.Parse(value);
                Guid sourceID;
                if(Guid.TryParse(xml.ElementSafe("ResourceID"), out sourceID))
                {
                    NavigateBack(sourceID);
                }
            }
        }

        public override void Dev2ReloadResource(Guid resourceID, string resourceType)
        {
            if(_isEditingSource)
            {
                // DB source invoked this from new window
                NavigateBack(resourceID);
            }
        }

        #region NavigateBack

        void NavigateBack(Guid sourceID)
        {
            var uri = _returnUri;
            _isEditingSource = false;
            _returnUri = null;

            const string SourceParam = "rid=";
            var idx = uri.IndexOf(SourceParam, StringComparison.InvariantCultureIgnoreCase);
            if(idx > 0)
            {
                var start = idx + SourceParam.Length;
                var end = uri.IndexOf('&', start);
                end = end > 0 ? end : uri.Length;
                uri = uri.Remove(start, (end - start));
                uri = uri.Insert(start, sourceID.ToString());
            }
            else
            {
                uri += (uri.IndexOf('?') > 0 ? "&" : "?") + SourceParam + sourceID;
            }

            Navigate(uri);
            ReloadResource(_environmentModel, sourceID, ResourceType.Source);
        }

        #endregion

    }
}
