
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
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    public class WebActivity : IWebActivity
    {
        public string WebsiteServiceName
        {
            get { return GetPropertyValue(WebActivityObject, "WebsiteServiceName"); }
            set { SetPropertyValue(WebActivityObject, "WebsiteServiceName", value); }
        }

        public string MetaTags
        {
            get { return GetPropertyValue(WebActivityObject, "MetaTags"); }
            set { SetPropertyValue(WebActivityObject, "MetaTags", value); }
        }

        public string FormEncodingType
        {
            get { return GetPropertyValue(WebActivityObject, "FormEncodingType"); }
            set { SetPropertyValue(WebActivityObject, "FormEncodingType", value); }
        }

        public string XMLConfiguration
        {
            get { return GetPropertyValue(WebActivityObject, "XMLConfiguration"); }
            set { SetPropertyValue(WebActivityObject, "XMLConfiguration", value); }
        }

        public string Html
        {
            get { return GetPropertyValue(WebActivityObject, "Html"); }
            set { SetPropertyValue(WebActivityObject, "Html", value); }
        }

        public string ServiceName
        {
            get { return GetPropertyValue(WebActivityObject, "ServiceName"); }
            set { SetPropertyValue(WebActivityObject, "ServiceName", value); }
        }

        public string LiveInputMapping { get; set; }
        public string LiveOutputMapping { get; set; }

        public string SavedInputMapping
        {
            get { return GetPropertyValue(WebActivityObject, "InputMapping"); }
            set { SetPropertyValue(WebActivityObject, "InputMapping", value); }
        }

        public string SavedOutputMapping
        {
            get { return GetPropertyValue(WebActivityObject, "OutputMapping"); }
            set { SetPropertyValue(WebActivityObject, "OutputMapping", value); }
        }

        public object WebActivityObject { get; set; }

        public Type UnderlyingWebActivityObjectType
        {
            get
            {
                if(WebActivityObject is ModelItem)
                {
                    return (WebActivityObject as ModelItem).ItemType;
                }

                return null;
            }
        }


        public IContextualResourceModel ResourceModel { get; set; }

        private string GetPropertyValue(object modelItemObject, string propertyName)
        {
            var modelItem = modelItemObject as ModelItem;
            if(modelItem != null && modelItem.Properties[propertyName] != null)
            {
                return modelItem.Properties[propertyName].ComputedValue == null
                           ? string.Empty
                           : modelItem.Properties[propertyName].ComputedValue.ToString();
            }
            return string.Empty;
        }

        private void SetPropertyValue(object modelItemObject, string propertyName, object value)
        {
            var modelItem = modelItemObject as ModelItem;
            if(modelItem != null && modelItem.Properties[propertyName] != null)
            {
                modelItem.Properties[propertyName].SetValue(value);
            }
        }
        public bool IsNotAvailable()
        {
            return !ResourceModel.Environment.IsLocalHost && !ResourceModel.Environment.HasLoadedResources;
        }
    }
}
