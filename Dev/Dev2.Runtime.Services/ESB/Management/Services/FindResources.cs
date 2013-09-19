using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Unlimited.Framework;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// List all resources in the system
    /// </summary>
    public class FindResources : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceName;
            string roles;
            string type;
            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("Roles", out roles);
            values.TryGetValue("Type", out type);

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var result = ResourceCatalog.Instance.GetPayload(theWorkspace.ID, resourceName, type, roles);

            dynamic serviceData = new UnlimitedObject("Dev2Resources");
            var returnVal = "";
            //zuko.mgwili@dev2.co.za
            //18/06/2010
            //we need to be able to return the worker service name and the category it belongs to
            if(type.Equals("Service"))
            {
                returnVal = GetWorkerServiceResourceAsXml(result, serviceData);
            }
            else if(type.Equals("Source"))
            {
                returnVal = GetSourceResourceAsXml(result, serviceData);
            }
            else
            {
                returnVal = GetAllDefsAsXML(result, serviceData);
            }

            return returnVal;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findAllService = new DynamicService();
            findAllService.Name = HandlesType();
            findAllService.DataListSpecification = "<DataList><Type/><Roles/><ResourceName/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findAllServiceAction = new ServiceAction();
            findAllServiceAction.Name = HandlesType();
            findAllServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findAllServiceAction.SourceName = HandlesType();
            findAllServiceAction.SourceMethod = HandlesType();

            findAllService.Actions.Add(findAllServiceAction);

            return findAllService;
        }

        public string HandlesType()
        {
            return "FindResourcesService";
        }

        #region Private Methods
        //Author: Zuko Mgwili
        //Date: 18/06/2012
        //Purpose of the method is to return worker service name and worker service category
        private string GetWorkerServiceResourceAsXml(string source, dynamic serviceData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList list = doc.ChildNodes;
            XmlNode root = list[0];
            XmlNodeList workerServices = root.ChildNodes;
            foreach(XmlNode node in workerServices)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(node.Name.Equals("Service"))
                {
                    serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    XmlNode tmpNode = node.SelectSingleNode("//Category");
                    if(tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    serviceInfo.Dev2WorkerServiceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
            }
            return serviceData.XmlString;
        }

        // Travis : Fixed all of Zuko's crap service defs
        private string GetAllDefsAsXML(string source, dynamic serviceData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList list = doc.ChildNodes;
            XmlNode root = list[0];
            XmlNodeList workerServices = root.ChildNodes;
            foreach(XmlNode node in workerServices)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(node.Name.Equals("Service"))
                {
                    serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    XmlNode tmpNode =
                        node.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "Category").FirstOrDefault();
                    if(tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    serviceInfo.Dev2WorkerServiceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
                else if(node.Name.Equals("Source"))
                {
                    serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    XmlNode tmpNode =
                        node.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "Category").FirstOrDefault();
                    if(tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    serviceInfo.Dev2SourceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
            }
            return serviceData.XmlString;
        }

        //Author: Zuko Mgwili
        //Date: 19/06/2012
        //Purpose of the method is to return source name and type
        private string GetSourceResourceAsXml(string source, dynamic serviceData)
        {
            var doc = new XmlDocument();
            doc.LoadXml(source);
            XmlNodeList list = doc.ChildNodes;
            XmlNode root = list[0];
            XmlNodeList sourceItem = root.ChildNodes;
            foreach(XmlNode node in sourceItem)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if(node.Name.Equals("Source"))
                {
                    serviceInfo.Dev2SourceName = node.Attributes["Name"].Value;
                    serviceInfo.Dev2SourceType = node.Attributes["Type"].Value;
                    serviceInfo.Dev2SourceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
            }
            return serviceData.XmlString;
        }
        #endregion
    }
}
