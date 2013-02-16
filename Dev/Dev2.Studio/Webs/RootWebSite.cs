using System;
using System.Xml.Linq;
using Dev2.DynamicServices;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;

namespace Dev2.Studio.Webs
{
    public static class RootWebSite
    {
        public const string SiteName = "wwwroot";

        #region ShowDialog(IContextualResourceModel resourceModel)

        //
        // TWR: 2013.02.14 - refactored to accommodate new requests
        // PBI: 801
        // BUG: 8477
        //
        public static bool ShowDialog(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return false;
            }

            string serviceID;
            var sourceType = enSourceType.Unknown;

            if(string.IsNullOrEmpty(resourceModel.ServiceDefinition))
            {
                serviceID = Guid.Empty.ToString();
                if(resourceModel.IsDatabaseService)
                {
                    sourceType = enSourceType.SqlDatabase;
                }
            }
            else
            {
                var serviceXml = XElement.Parse(resourceModel.ServiceDefinition);

                serviceID = serviceXml.AttributeSafe("ID");
                Enum.TryParse(serviceXml.AttributeSafe("Type"), out sourceType);
            }

            return ShowDialog(resourceModel.Environment, sourceType, serviceID);
        }

        #endregion

        #region ShowDialog(IEnvironmentModel environment, enSourceType sourceType, string idStr = null)

        public static bool ShowDialog(IEnvironmentModel environment, enSourceType sourceType, string idStr = null)
        {
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            Guid id;
            Guid.TryParse(idStr, out id);

            string pageName;
            WebsiteCallbackHandler pageHandler;
            double width;
            double height;

            switch(sourceType)
            {
                case enSourceType.Dev2Server:
                    pageName = "sources/server";
                    pageHandler = new ConnectCallbackHandler();
                    width = 690;
                    height = 500;
                    break;

                case enSourceType.SqlDatabase:
                case enSourceType.MySqlDatabase:
                    pageName = "services/dbservice";
                    pageHandler = new DbServiceCallbackHandler();
                    width = 941;
                    height = 570;
                    break;
                default:
                    return false;
            }
            environment.ShowWebPageDialog(SiteName, string.Format("{0}?rid={1}", pageName, id), pageHandler, width, height);
            return true;
        }

        #endregion
    }
}
