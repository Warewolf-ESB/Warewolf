using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Workspaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Workspaces
{   
    public class WorkspaceItemRepository : IWorkspaceItemRepository
    {
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //

        static volatile IWorkspaceItemRepository _instance;
        static readonly object SyncRoot = new Object();

        public static IWorkspaceItemRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new WorkspaceItemRepository();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        private IList<IWorkspaceItem> _workspaceItems;

        public IList<IWorkspaceItem> WorkspaceItems
        {
            get { return _workspaceItems ?? (_workspaceItems = Read()); }
        }

        #region CTOR

        public WorkspaceItemRepository()
            : this((string)null)
        {
        }

        // BUG 9492 - 2013.06.08 - TWR : added constructor - use for testing only!
        public WorkspaceItemRepository(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        public WorkspaceItemRepository(IWorkspaceItemRepository workspaceItemRepository)
        {
            _instance = workspaceItemRepository;
        }

        #endregion

        #region RepositoryPath

        // BUG 9492 - 2013.06.08 - TWR : made public and non-static
        string _repositoryPath;
        public string RepositoryPath
        {
            get
            {
                if(string.IsNullOrEmpty(_repositoryPath))
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
            if(File.Exists(RepositoryPath))
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
            foreach(var workspaceItem in WorkspaceItems)
            {
                var itemXml = workspaceItem.ToXml();
                root.Add(itemXml);
            }

            if(!File.Exists(RepositoryPath))
            {
                FileInfo fileInfo = new FileInfo(RepositoryPath);
                string finalDirectoryPath = fileInfo.Directory.FullName;

                if(!Directory.Exists(finalDirectoryPath))
                {
                    Directory.CreateDirectory(finalDirectoryPath);
                }
            }
            File.WriteAllText(RepositoryPath, root.ToString());
        }

        #endregion

        #region AddWorkspaceItem

        public void AddWorkspaceItem(IContextualResourceModel model)
        {
            // BUG 9492 - 2013.06.08 - TWR : added null check
            if(model == null)
            {
                throw new ArgumentNullException("model");
            }
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ID == model.ID && wi.EnvironmentID == model.Environment.ID);
            if(workspaceItem != null)
            {
                return;
            }

            var context = (IStudioClientContext)model.Environment.DsfChannel;
            WorkspaceItems.Add(new WorkspaceItem(context.WorkspaceID, context.ServerID,model.Environment.ID,model.ID)
            {
                ServiceName = model.ResourceName,
                IsWorkflowSaved = model.IsWorkflowSaved,
                ServiceType =
                    model.ResourceType == ResourceType.Source
                        ? WorkspaceItem.SourceServiceType
                        : WorkspaceItem.ServiceServiceType,
            });
            Write();
            model.OnResourceSaved +=UpdateWorkspaceItemIsWorkflowSaved;
        }

        #endregion

        #region UpdateWorkspaceItem

        public void UpdateWorkspaceItemIsWorkflowSaved(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                throw new ArgumentNullException("resourceModel");
            }
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ID == resourceModel.ID && wi.EnvironmentID == resourceModel.Environment.ID);

            if (workspaceItem == null)
            {
                return;
            }
            workspaceItem.IsWorkflowSaved = resourceModel.IsWorkflowSaved;
        }

        public string UpdateWorkspaceItem(IContextualResourceModel resource, bool isLocalSave)
        {
            // BUG 9492 - 2013.06.08 - TWR : added null check
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ID == resource.ID && wi.EnvironmentID == resource.Environment.ID);

            if(workspaceItem == null)
            {
                return string.Empty;
            }

            var securityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();

            workspaceItem.Action = WorkspaceItemAction.Commit;
            dynamic publishRequest = new UnlimitedObject();
            publishRequest.Service = "UpdateWorkspaceItemService";
            publishRequest.Roles = String.Join(",", securityContext.Roles);
            publishRequest.ItemXml = workspaceItem.ToXml();
            publishRequest.IsLocalSave = isLocalSave;

            string result = resource.Environment.DsfChannel
                                    .ExecuteCommand(publishRequest.XmlString, workspaceItem.WorkspaceID,
                                        GlobalConstants.NullDataListID) ??
                            string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, publishRequest.Service);
            return result;

        }

        #endregion

        #region Remove

        public void Remove(IContextualResourceModel resourceModel)
        {
            // BUG 9492 - 2013.06.08 - TWR : added null check
            if(resourceModel == null)
            {
                return;
            }
            var itemToRemove = WorkspaceItems.FirstOrDefault(c => c.ServiceName == resourceModel.ResourceName);
            if(itemToRemove == null)
            {
                return;
            }

            WorkspaceItems.Remove(itemToRemove);
            Write();
        }

        #endregion

    }
}
