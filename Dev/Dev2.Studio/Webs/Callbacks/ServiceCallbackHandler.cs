using Caliburn.Micro;
using Dev2.Common.Common;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using System;
using System.Xml.Linq;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
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
                        if(getDynamicResourceType == Data.ServiceModel.ResourceType.DbSource.ToString() ||
                            getDynamicResourceType == Data.ServiceModel.ResourceType.PluginSource.ToString() ||
                            getDynamicResourceType == Data.ServiceModel.ResourceType.WebSource.ToString())
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
                this.LogError(e);
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
                uri += (uri.IndexOf('?') > 0 ? "&" : "?") + SourceParam + sourceID.ToString();
            }

            Navigate(uri);
            ReloadResource(_environmentModel, sourceID, ResourceType.Source);
        }

        #endregion

    }
}
