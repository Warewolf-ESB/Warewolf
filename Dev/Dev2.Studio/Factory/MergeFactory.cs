using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Dev2.Factory
{
    public class MergeFactory : IMergeFactory
    {

        public void OpenMergeWindow(IShellViewModel shellViewModel, string item)
        {
            VerifyArgument.IsNotNull(nameof(shellViewModel), shellViewModel);
            var mergeArgs = item.Split(' ');
            var mergeHeadPath = mergeArgs[1];
            using (var stream = File.OpenRead(mergeHeadPath))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var resourceContent = streamReader.ReadToEnd();
                    var resourceXml = resourceContent;
                    var serviceXml = XDocument.Parse(resourceXml);

                    var resourceId = GetAttributeValue(serviceXml, "ID");
                    var serverID = GetAttributeValue(serviceXml, "ServerID");

                    var serverRepo = CustomContainer.Get<IServerRepository>();
                    var localResource = serverRepo.Source.ResourceRepository.LoadContextualResourceModel(new Guid(resourceId));
                    var resoureSource = serverRepo.Get(new Guid(serverID));
                    if (resoureSource != null)
                    {
                        var remoteResource = resoureSource.ResourceRepository.LoadContextualResourceModel(new Guid(resourceId));
                        shellViewModel.OpenMergeConflictsView(localResource, remoteResource, true);
                    }
                    else
                    {
                        var resource = new Resource(resourceContent.ToStringBuilder().ToXElement());
                        ResourceModel remoteResource = new ResourceModel();
                        remoteResource.DisplayName = resource.ResourceName;
                        remoteResource.DataList = resource.DataList.ToString();
                        remoteResource.ID = new Guid(resourceId);
                        remoteResource.WorkflowXaml = serviceXml.Element("Service").Element("Action").ToString(SaveOptions.DisableFormatting).ToStringBuilder();
                        shellViewModel.OpenMergeConflictsView(localResource, remoteResource, true);
                    }
                }
            }
        }

        string GetAttributeValue(XDocument document, string attName)
        {
            XAttribute xAttribute = document.Element("Service").Attribute(attName);
            return xAttribute.Value;
        }
    }

}

