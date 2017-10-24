/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;


namespace Dev2.Workspaces
{
    public class WorkspaceItemRepository : IWorkspaceItemRepository
    {
        #region Singleton Instance
        
        static volatile IWorkspaceItemRepository _instance;
        static readonly object SyncRoot = new Object();

        public static IWorkspaceItemRepository Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
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

        public IList<IWorkspaceItem> WorkspaceItems => _workspaceItems ?? (_workspaceItems = Read());

        #region CTOR

        public WorkspaceItemRepository()
            : this((string)null)
        {
        }

        public WorkspaceItemRepository(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        public WorkspaceItemRepository(IWorkspaceItemRepository workspaceItemRepository)
        {
#pragma warning disable S3010 // For testing
            _instance = workspaceItemRepository;
#pragma warning restore S3010
        }

        #endregion

        #region RepositoryPath

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
                
                catch
                
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
                if(fileInfo.Directory != null)
                {
                    string finalDirectoryPath = fileInfo.Directory.FullName;

                    if(!Directory.Exists(finalDirectoryPath))
                    {
                        Directory.CreateDirectory(finalDirectoryPath);
                    }
                }
            }
            File.WriteAllText(RepositoryPath, root.ToString());
        }

        #endregion

        #region AddWorkspaceItem

        public void AddWorkspaceItem(IContextualResourceModel model)
        {
            if(model == null)
            {
                throw new ArgumentNullException("model");
            }
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ID == model.ID && wi.EnvironmentID == model.Environment.EnvironmentID);
            if(workspaceItem != null)
            {
                return;
            }

            var context = model.Environment.Connection;
            WorkspaceItems.Add(new WorkspaceItem(context.WorkspaceID, context.ServerID, model.Environment.EnvironmentID, model.ID)
            {
                ServiceName = model.ResourceName,
                IsWorkflowSaved = model.IsWorkflowSaved,
                ServiceType =
                    model.ResourceType == ResourceType.Source
                        ? WorkspaceItem.SourceServiceType
                        : WorkspaceItem.ServiceServiceType,
            });
            Write();
            model.OnResourceSaved += UpdateWorkspaceItemIsWorkflowSaved;
        }

        #endregion

        #region UpdateWorkspaceItem

        public void UpdateWorkspaceItemIsWorkflowSaved(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                throw new ArgumentNullException("resourceModel");
            }
            var workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ID == resourceModel.ID && wi.EnvironmentID == resourceModel.Environment.EnvironmentID);

            if(workspaceItem == null)
            {
                return;
            }
            workspaceItem.IsWorkflowSaved = resourceModel.IsWorkflowSaved;
        }

        #endregion

        #region Remove

        public void Remove(IContextualResourceModel resourceModel)
        {
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
            resourceModel.Environment.ResourceRepository.DeleteResourceFromWorkspaceAsync(resourceModel);
        }

        public void ClearWorkspaceItems(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                return;
            }
            WorkspaceItems.Clear();
            resourceModel.Environment.ResourceRepository.DeleteResourceFromWorkspace(resourceModel);
        }

        #endregion

    }
}
