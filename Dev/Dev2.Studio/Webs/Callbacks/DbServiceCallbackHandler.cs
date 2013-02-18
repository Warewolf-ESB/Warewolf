using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;

namespace Dev2.Studio.Webs.Callbacks
{
    public class DbServiceCallbackHandler : WebsiteCallbackHandler
    {
        bool _isEditingSource;
        string _returnUri;

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, jsonObj.ResourceName.Value, ResourceType.Service);
        }

        protected override void Navigate(IEnvironmentModel environmentModel, string uri, dynamic jsonArgs, string returnUri)
        {
            if(environmentModel == null || environmentModel.Resources == null || jsonArgs == null)
            {
                return;
            }

            Guid dataListID;
            var relativeUri = "/services/DatabaseSourceManagement";
            var sourceName = jsonArgs.ResourceName.Value;
            var contextualResource = string.IsNullOrEmpty(sourceName)
                                         ? null
                                         : environmentModel.Resources.All().FirstOrDefault(r => r.ResourceName.Equals(sourceName, StringComparison.InvariantCultureIgnoreCase)) as IContextualResourceModel;
            if(contextualResource == null)
            {
                relativeUri += "?Dev2ServiceType=Database";
                dataListID = Guid.Empty;
            }
            else
            {
                ErrorResultTO errors;
                var args = StudioToWizardBridge.BuildStudioEditPayload(contextualResource.ResourceType.ToString(), contextualResource);
                dataListID = environmentModel.UploadToDataList(args, out errors);
            }

            Uri requestUri;
            if(!Uri.TryCreate(environmentModel.WebServerAddress, relativeUri, out requestUri))
            {
                requestUri = new Uri(new Uri(StringResources.Uri_WebServer), relativeUri);
            }
            var uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);

            _isEditingSource = true;
            _returnUri = returnUri;
            Navigate(uriString);
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
                _isEditingSource = false;
                var xml = XElement.Parse(value);
                var sourceName = xml.ElementSafe("ResourceName");

                const string SourceParam = "sourceName=";
                var idx = _returnUri.IndexOf(SourceParam, StringComparison.InvariantCultureIgnoreCase);
                if(idx > 0)
                {
                    var start = idx + SourceParam.Length;
                    var end = _returnUri.IndexOf('&', start);
                    end = end > 0 ? end : _returnUri.Length;
                    _returnUri = _returnUri.Remove(start, (end - start));
                    _returnUri = _returnUri.Insert(start, sourceName);
                }
                else
                {
                    _returnUri += _returnUri.IndexOf('?') > 0 ? "&" : "?" + SourceParam + sourceName;
                }

                Navigate(_returnUri);
                ReloadResource(EnvironmentRepository.DefaultEnvironment, sourceName, ResourceType.Source);
            }
        }
    }
}
