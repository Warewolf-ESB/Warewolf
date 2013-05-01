using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Workspaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Workspaces
{
    [Export(typeof(IWorkspaceItemRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorkspaceItemRepository : IWorkspaceItemRepository
    {
        private IList<IWorkspaceItem> _workspaceItems;

        public IList<IWorkspaceItem> WorkspaceItems
        {
            get { return _workspaceItems ?? (_workspaceItems = Read()); } 
        }

        #region RepositoryPath

        static string _repositoryPath;
        static string RepositoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_repositoryPath))
                {
                    _repositoryPath = Path.Combine(new[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        StringResources.App_Data_Directory,
                        StringResources.User_Interface_Layouts_Directory,
                        "WorkspaceItems.xml"
                    });
                }
                return _repositoryPath;
            }
        }

        #endregion

        #region Read

        private IList<IWorkspaceItem> Read()
        {
            var result = new List<IWorkspaceItem>();
            if (File.Exists(RepositoryPath))
            {
                try
                {
                    var xml = XElement.Parse(File.ReadAllText(RepositoryPath));
                    result.AddRange(xml.Elements().Select(x => new WorkspaceItem(x)));
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // corrupt so ignore
                }
            }
            return result;
        }

        #endregion

        #region Write

        public void Write()
        {
            var root = new XElement("WorkspaceItems");
            foreach (var workspaceItem in WorkspaceItems)
            {
                var itemXml = workspaceItem.ToXml();
                root.Add(itemXml);
            }

            if (!File.Exists(RepositoryPath))
            {
                FileInfo fileInfo = new FileInfo(RepositoryPath);
                string finalDirectoryPath = fileInfo.Directory.FullName;
                
                if (!Directory.Exists(finalDirectoryPath))
                {
                    Directory.CreateDirectory(finalDirectoryPath);
                }
            }
            File.WriteAllText(RepositoryPath, root.ToString());
        }

        #endregion

        public void AddWorkspaceItem(IContextualResourceModel model)
        {
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ServiceName == model.ResourceName);
            if (workspaceItem != null) return;

            var context = (IStudioClientContext)model.Environment.DsfChannel;
            WorkspaceItems.Add(new WorkspaceItem(context.WorkspaceID, context.ServerID)
                {
                    ServiceName = model.ResourceName,
                    ServiceType =
                        model.ResourceType == ResourceType.Source
                            ? WorkspaceItem.SourceServiceType
                            : WorkspaceItem.ServiceServiceType,
                });
            Write();
        }

        public string UpdateWorkspaceItem(IContextualResourceModel resource)
        {
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ServiceName == resource.ResourceName);

            if (workspaceItem == null)
            {
                return string.Empty;
            }

            var securityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();

            workspaceItem.Action = WorkspaceItemAction.Commit;
            dynamic publishRequest = new UnlimitedObject();
            publishRequest.Service = "UpdateWorkspaceItemService";
            publishRequest.Roles = String.Join(",", securityContext.Roles);
            publishRequest.ItemXml = workspaceItem.ToXml();

            string result = resource.Environment.DsfChannel
                                    .ExecuteCommand(publishRequest.XmlString, workspaceItem.WorkspaceID,
                                                    GlobalConstants.NullDataListID) ??
                            string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, publishRequest.Service);
            return result;

        }

        public void Remove(IContextualResourceModel resourceModel)
        {
            var itemToRemove =
                WorkspaceItems.FirstOrDefault(c => c.ServiceName == resourceModel.ResourceName);
            if (itemToRemove == null) return;

            WorkspaceItems.Remove(itemToRemove);
            Write();
        }
    }
}
