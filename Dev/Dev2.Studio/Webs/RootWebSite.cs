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

        #region Sources

        public static class Sources
        {
            #region ShowDialog

            //
            // TWR: 2013.02.14 - refactored to accommodate new requests
            // PBI: 801
            // BUG: 8477
            //
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
                        pageName = "server";
                        pageHandler = new ConnectCallbackHandler();
                        width = 850;
                        height = 600;
                        break;
                    case enSourceType.SqlDatabase:
                    case enSourceType.MySqlDatabase:
                        pageName = "dbservice";
                        pageHandler = new DbServiceCallbackHandler();
                        width = 1040;
                        height = 630;
                        break;
                    default:
                        return false;
                }
                environment.ShowWebPageDialog(SiteName, string.Format("sources/{0}?rid={1}", pageName, id), pageHandler, width, height);
                return true;
            }

            #endregion
        }

        #endregion

        #region ShowDialog(this IContextualResourceModel resourceModel)

        //
        // TWR: 2013.02.14 - refactored to accommodate new requests
        // PBI: 801
        // BUG: 8477
        //
        public static bool ShowWebPageDialog(this IContextualResourceModel resourceModel)
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

            return Sources.ShowDialog(resourceModel.Environment, sourceType, serviceID);
        }

        #endregion

    }
}
