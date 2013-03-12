using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Dev2.DynamicServices;
using Unlimited.Framework;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// List all resources in the system
    /// </summary>
    public class FindResources : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder("<Payload>");
            IDynamicServicesHost theHost = theWorkspace.Host;

            string resourceName;
            string roles;
            string type;
            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("Roles", out roles);
            values.TryGetValue("ResourceType", out type);


            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(roles))
            {
                throw new InvalidDataContractException("ResourceType or ResourceName or Roles not provided");
            }

            if(resourceName == "*")
            {
                resourceName = string.Empty;
            }

            switch (type)
            {
                case "WorkflowService":
                {
                    IEnumerable<DynamicService> services;
                    theHost.LockServices();

                    try
                    {
                        services = theHost.Services.Where(c => c.Name.Contains(resourceName));
                    }
                    finally
                    {
                        theHost.UnlockServices();
                    }

                    IEnumerable<DynamicService> workflowServices = services.Where(c => c.Actions.Any(d => d.ActionType == enActionType.Workflow));

                    workflowServices.ToList()
                                    .ForEach(
                                        c =>
                                        result.Append(c.ResourceDefinition));
                }
                break;
                    
                case "Service":
                {
                    IEnumerable<DynamicService> svc;
                    theHost.LockServices();

                    try
                    {
                        svc = theHost.Services.Where(c => c.Name.Contains(resourceName));
                    }
                    finally
                    {
                        theHost.UnlockServices();
                    }

                    IEnumerable<DynamicService> svcs = svc.Where(c => c.Actions.Any(d => d.ActionType != enActionType.Workflow));

                    svcs.ToList()
                        .ForEach(
                            c =>
                            result.Append(c.ResourceDefinition));
                }
                break;

                case "Source":
                {
                    IEnumerable<Source> sources;
                    theHost.LockSources();

                    try
                    {
                        sources = theHost.Sources.Where(c => c.Name.Contains(resourceName));
                    }
                    finally
                    {
                        theHost.UnlockSources();
                    }

                    sources.ToList()
                           .ForEach(
                               c =>
                               result.Append(c.ResourceDefinition));
                }
                break;
                    
                case "*":
                {
                    IEnumerable<Source> resources;
                    theHost.LockSources();

                    try
                    {
                        resources = theHost.Sources.Where(c => c.Name.Contains(resourceName)); 
                    }
                    finally
                    {
                        theHost.UnlockSources();
                    }

                    resources.ToList()
                             .ForEach(
                                 c =>
                                 result.Append(c.ResourceDefinition));
                    IEnumerable<DynamicService> wfservices;
                    theHost.LockServices();

                    try
                    {
                        wfservices = theHost.Services.Where(c => c.Name.Contains(resourceName));
                    }
                    finally
                    {
                        theHost.UnlockServices();
                    }

                    IEnumerable<DynamicService> workflows = wfservices.Where(c => c.Actions.Any(d => d.ActionType == enActionType.Workflow));

                    workflows.ToList()
                             .ForEach(
                                 c =>
                                 result.Append(c.ResourceDefinition));

                    IEnumerable<DynamicService> svc2;
                    theHost.LockServices();

                    try
                    {
                        svc2 = theHost.Services.Where(c => c.Name.Contains(resourceName));
                    }
                    finally
                    {
                        theHost.UnlockServices();
                    }

                    IEnumerable<DynamicService> svcs2 = svc2.Where(c => c.Actions.Any(d => d.ActionType != enActionType.Workflow));

                    svcs2.ToList()
                         .ForEach(
                             c =>
                             result.Append(c.ResourceDefinition));

                }
                break;

            }

            // close out the payload ;)
            result.Append("</Payload>");

            dynamic serviceData = new UnlimitedObject("Dev2Resources");
            string returnVal = "";
            //zuko.mgwili@dev2.co.za
            //18/06/2010
            //we need to be able to return the worker service name and the category it belongs to
            if (type.Equals("Service"))
            {
                returnVal = GetWorkerServiceResourceAsXml(result.ToString(), serviceData);
            }
            else if (type.Equals("Source"))
            {
                returnVal = GetSourceResourceAsXml(result.ToString(), serviceData);
            }
            else
            {
                returnVal = GetAllDefsAsXML(result.ToString(), serviceData);
            }

            return returnVal;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findAllService = new DynamicService();
            findAllService.Name = HandlesType();
            findAllService.DataListSpecification = "<root><ResourceType/><Roles/><ResourceName/></root>";
            
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
            foreach (XmlNode node in workerServices)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if (node.Name.Equals("Service"))
                {
                    serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    XmlNode tmpNode = node.SelectSingleNode("//Category");
                    if (tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    //serviceInfo.Dev2WorkerServiceCategory = node.ChildNodes[3].InnerText;
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
            foreach (XmlNode node in workerServices)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if (node.Name.Equals("Service"))
                {
                    serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    XmlNode tmpNode =
                        node.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "Category").FirstOrDefault();
                    if (tmpNode != null)
                    {
                        serviceInfo.Dev2WorkerServiceCategory = tmpNode.InnerText;
                    }
                    serviceInfo.Dev2WorkerServiceContents = node.OuterXml;
                    serviceData.AddResponse(serviceInfo);
                }
                else if (node.Name.Equals("Source"))
                {
                    serviceInfo.Dev2WorkerService = node.Attributes["Name"].Value;
                    XmlNode tmpNode =
                        node.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "Category").FirstOrDefault();
                    if (tmpNode != null)
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
            foreach (XmlNode node in sourceItem)
            {
                dynamic serviceInfo = new UnlimitedObject("Dev2Resource");
                if (node.Name.Equals("Source"))
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
