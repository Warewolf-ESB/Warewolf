using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Studio.Controller;

namespace Dev2.Studio
{
    public static class ResourceExtensionHelper
    {
        static bool IsSource(StringBuilder serviceData)
        {
            return serviceData.IndexOf("<Source ", 0, false) == 0;
        }
        public static IContextualResourceModel HandleResourceNotInResourceFolder(string filePath, string fileName, Common.Interfaces.Studio.Controller.IPopupController popupController)
        {
            var saveResource = popupController.ShowResourcesNotInCorrectPath();
            if (saveResource == MessageBoxResult.OK)
            {
                using (var stream = File.OpenRead(filePath))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        ResourceModel resourceModel = null;
                        var resourceContent = streamReader.ReadToEnd();
                        var serverRepo = CustomContainer.Get<IServerRepository>();
                        var serviceXml = XDocument.Parse(resourceContent);
                        if (IsSource(serviceXml.Elements().FirstOrDefault().ToStringBuilder()))
                        {
                            CreateResourceModel(serviceXml);
                        }
                        else
                        {
                            var resourceId = serviceXml.Element("Service").Attribute("ID").Value;
                            var resource = new Resource(resourceContent.ToStringBuilder().ToXElement());
                            var def = serviceXml.Element("Service").Element("Action").Element("XamlDefinition").ToStringBuilder();
                            XElement xaml = def.Unescape().Replace("<XamlDefinition>", "").Replace("</XamlDefinition>", "").ToXElement();
                            resourceModel = new ResourceModel(serverRepo.ActiveServer)
                            {
                                Category = Path.Combine(EnvironmentVariables.ResourcePath, resource.ResourceName),
                                DisplayName = resource.ResourceName,
                                ResourceName = resource.ResourceName,
                                DataList = resource.DataList.ToString(),
                                ID = new Guid(resourceId),
                                WorkflowXaml = xaml.ToString(SaveOptions.DisableFormatting).ToStringBuilder(),
                                UserPermissions = Common.Interfaces.Security.Permissions.Administrator
                            };
                        }
                        return resourceModel;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        public static IContextualResourceModel CreateResourceModel(XDocument serviceXml)
        {
            var type = serviceXml.Element("Source").Attribute("ID").Value;
            //switch (type)
            //{
            //    case "SqlDatabase":
            //        break;
            //    case "ODBC":
            //        break;
            //    case "Oracle":
            //        break;
            //    case "PostgreSQL":
            //        break;
            //    case "MySqlDatabase":
            //        break;
            //    case "EmailSource":
            //        break;
            //    case "WebSource":
            //        break;
            //    case "ComPluginSource":
            //        break;
            //    case "ExchangeSource":
            //        break;
            //    case "OauthSource":
            //    case "DropBoxSource":
            //        break;
            //    case "Server":
            //    case "Dev2Server":
            //    case "ServerSource":
            //        break;
            //    case "SharepointServerSource":
            //        break;
            //    case "RabbitMQSource":
            //        break;
            //    case "WcfSource":
            //        break;
            //    case "PluginSource":
            //        break;
            //}

            return null;
        }
    }
}