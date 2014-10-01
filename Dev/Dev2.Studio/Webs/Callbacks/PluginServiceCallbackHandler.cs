
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
    public class PluginServiceCallbackHandler : WebsiteCallbackHandler
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
            var relativeUri = "/services/PluginSourceManagement";
            var sourceName = jsonArgs.ResourceName.Value;
            var contextualResource = string.IsNullOrEmpty(sourceName)
                                         ? null
                                         : environmentModel.Resources.All().FirstOrDefault(r => r.ResourceName.Equals(sourceName, StringComparison.InvariantCultureIgnoreCase)) as IContextualResourceModel;
            if(contextualResource == null)
            {
                relativeUri += "?Dev2ServiceType=Plugin";
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
                // DB source invokes this
                var xml = XElement.Parse(value);
                var sourceName = xml.ElementSafe("ResourceName");
                NavigateBack(sourceName);
            }
        }

        public override void Dev2ReloadResource(string resourceName, string resourceType)
        {
            if(_isEditingSource)
            {
                // DB source invoked this from new window
                NavigateBack(resourceName);
            }
        }

        #region NavigateBack

        void NavigateBack(string sourceName)
        {
            var uri = _returnUri;
            _isEditingSource = false;
            _returnUri = null;

            const string SourceParam = "sourceName=";
            var idx = uri.IndexOf(SourceParam, StringComparison.InvariantCultureIgnoreCase);
            if(idx > 0)
            {
                var start = idx + SourceParam.Length;
                var end = uri.IndexOf('&', start);
                end = end > 0 ? end : uri.Length;
                uri = uri.Remove(start, (end - start));
                uri = uri.Insert(start, sourceName);
            }
            else
            {
                uri += (uri.IndexOf('?') > 0 ? "&" : "?") + SourceParam + sourceName;
            }

            Navigate(uri);
            ReloadResource(EnvironmentRepository.DefaultEnvironment, sourceName, ResourceType.Source);
        }

        #endregion

    }
}
