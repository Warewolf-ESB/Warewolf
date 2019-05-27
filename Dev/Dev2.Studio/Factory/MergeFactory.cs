#pragma warning disable
using Dev2.Common.Common;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using System;
using System.IO;
using System.Xml.Linq;

namespace Dev2.Factory
{
    public class MergeFactory : IMergeFactory
    {
        public void OpenMergeWindow(IShellViewModel shellViewModel, WarwolfStartupEventArgs args)
        {
            VerifyArgument.IsNotNull(nameof(shellViewModel), shellViewModel);
            VerifyArgument.IsNotNull(nameof(args), args);
            var cleanPath = args.Args[1].Replace('^', ' ');
            var mergeHeadPath = cleanPath;
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
                        var remoteResource = new ResourceModel(serverRepo.ActiveServer)
                        {
                            DisplayName = resource.ResourceName,
                            DataList = resource.DataList.ToString(),
                            ID = new Guid(resourceId),
                            WorkflowXaml = serviceXml.Element("Service").Element("Action").ToString(SaveOptions.DisableFormatting).ToStringBuilder()
                        };
                        shellViewModel.OpenMergeConflictsView(localResource, remoteResource, false);
                    }
                }
            }
        }

        static string GetAttributeValue(XDocument document, string attName)
        {
            var xAttribute = document.Element("Service").Attribute(attName);
            return xAttribute.Value;
        }
    }

}

