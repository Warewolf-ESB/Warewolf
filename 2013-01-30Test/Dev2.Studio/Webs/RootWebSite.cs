using Dev2.DynamicServices;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Custom_Dev2_Controls.Connections;
using System;
using System.Xml.Linq;

namespace Dev2.Studio.Webs
{
    public static class RootWebSite
    {
        public const string SiteName = "wwwroot";

        #region Sources

        public static class Sources
        {
            #region ShowDialog

            public static void ShowDialog(IEnvironmentModel environment, enSourceType sourceType, string idStr = null)
            {
                if(environment == null)
                {
                    throw new ArgumentNullException("environment");
                }

                Guid id;
                Guid.TryParse(idStr, out id);

                string pageName;
                switch(sourceType)
                {
                    case enSourceType.Dev2Server:
                        pageName = "server";
                        break;
                    default:
                        pageName = sourceType.ToString();
                        break;
                }

                environment.ShowWebPageDialog(SiteName, string.Format("sources/{0}?rid={1}", pageName, id), new ConnectCallbackHandler(), 850, 600);
            }

            #endregion
        }

        #endregion

        #region ShowDialog(this IContextualResourceModel resourceModel)

        public static bool ShowWebPageDialog(this IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || string.IsNullOrEmpty(resourceModel.ServiceDefinition))
            {
                return false;
            }

            var serviceXml = XElement.Parse(resourceModel.ServiceDefinition);

            var idAttr = serviceXml.Attribute("ID");
            var typeAttr = serviceXml.Attribute("Type");
            if(idAttr != null && typeAttr != null)
            {
                var serviceID = idAttr.Value;
                var sourceType = (enSourceType)Enum.Parse(typeof(enSourceType), typeAttr.Value);
                if(sourceType == enSourceType.Dev2Server)
                {
                    Sources.ShowDialog(resourceModel.Environment, sourceType, serviceID);
                    return true;
                }
            }

            return false;
        }

        #endregion

    }
}
