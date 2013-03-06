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
    public class DbSourceCallbackHandler : WebsiteCallbackHandler
    {

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, jsonObj.ResourceName.Value, ResourceType.Source);
        }

        public override void Cancel()
        {
            Close();
        }

    }
}
