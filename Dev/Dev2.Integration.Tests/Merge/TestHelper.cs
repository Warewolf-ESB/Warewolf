using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Merge
{
    public class TestHelper
    {
        public static IContextualResourceModel CreateContextualResourceModel(string pathToTestResource)
        {
            var xElement = XML.MergeRemote.XmlResource.Fetch(pathToTestResource);
            var element = xElement.Element("Action");
            var instanceActiveServer = ServerRepository.Instance.Source;
            var resource = new Resource(xElement);
            var serializableResource = new SerializableResource();
            
            Mapper.AddMap<Resource, SerializableResource>();
            Mapper.Map(resource, serializableResource,false, "Errors");
            var hydrateResourceModel = ((ResourceRepository)instanceActiveServer.ResourceRepository).HydrateResourceModel(serializableResource, instanceActiveServer.ServerID.GetValueOrDefault(),true);
            Assert.IsNotNull(element);
            var contextualResourceModel = ResourceModelFactory.CreateResourceModel(instanceActiveServer);
            hydrateResourceModel.WorkflowXaml = new StringBuilder(element.ToString(SaveOptions.DisableFormatting));
            contextualResourceModel.Update(hydrateResourceModel);

            return contextualResourceModel;
        }
    }
}