using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using ChinhDo.Transactions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Compiler;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Resource.Errors;
// ReSharper disable InconsistentNaming

namespace Dev2.Runtime.ResourceCatalogImpl
{
    internal class ResourceSaveProvider : IResourceSaveProvider
    {
        private readonly IResourceCatalog _resourceCatalog;
        private readonly IServerVersionRepository _serverVersionRepository;
        private readonly FileWrapper _dev2FileWrapper = new FileWrapper();
        public ResourceSaveProvider(IResourceCatalog resourceCatalog, IServerVersionRepository serverVersionRepository)
        {
            _resourceCatalog = resourceCatalog;
            _serverVersionRepository = serverVersionRepository;
        }

        #region Implementation of IResourceSaveProvider

        public ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason = "", string user = "")
        {
            try
            {
                if (resourceXml == null || resourceXml.Length == 0)
                {
                    throw new ArgumentNullException(nameof(resourceXml));
                }

                var @lock = Common.GetWorkspaceLock(workspaceID);
                lock (@lock)
                {
                    var xml = resourceXml.ToXElement();

                    var resource = new Resource(xml);
                    GlobalConstants.InvalidateCache(resource.ResourceID);
                    Dev2Logger.Info("Save Resource." + resource);
                    _serverVersionRepository.StoreVersion(resource, user, reason, workspaceID, savedPath);

                    resource.UpgradeXml(xml, resource);

                    StringBuilder result = xml.ToStringBuilder();

                    return CompileAndSave(workspaceID, resource, result, savedPath);
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Save Error", err);
                throw;
            }
        }

        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath, string reason = "", string user = "")
        {
            _serverVersionRepository.StoreVersion(resource, user, reason, workspaceID, savedPath);

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            var @lock = Common.GetWorkspaceLock(workspaceID);
            lock (@lock)
            {
                if (resource.ResourceID == Guid.Empty)
                {
                    resource.ResourceID = Guid.NewGuid();
                }
                GlobalConstants.InvalidateCache(resource.ResourceID);
                savedPath = Common.SanitizePath(savedPath);
                var result = resource.ToStringBuilder();
                return CompileAndSave(workspaceID, resource, result, savedPath);
            }
        }

        public Action<IResource> ResourceSaved { get; set; }
        public Action<Guid, IList<ICompileMessageTO>> SendResourceMessages { get; set; }

        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason = "", string user = "")
        {
            _serverVersionRepository.StoreVersion(resource, user, reason, workspaceID, savedPath);
            ResourceCatalogResult saveResult = null;
            Dev2.Common.Utilities.PerformActionInsideImpersonatedContext(Dev2.Common.Utilities.ServerUser, () => { PerformSaveResult(out saveResult, workspaceID, resource, contents, true, savedPath); });
            return saveResult;
        }

        internal ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting, string savedPath = "")
        {
            ResourceCatalogResult saveResult = null;
            Dev2.Common.Utilities.PerformActionInsideImpersonatedContext(Dev2.Common.Utilities.ServerUser, () => { PerformSaveResult(out saveResult, workspaceID, resource, contents, overwriteExisting, savedPath); });
            return saveResult;
        }

        #endregion

        #region Private Methods
        private ResourceCatalogResult CompileAndSave(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath = "")
        {
            // Find the service before edits ;)
            DynamicService beforeService = _resourceCatalog.GetDynamicObjects<DynamicService>(workspaceID, resource.ResourceID).FirstOrDefault();

            ServiceAction beforeAction = null;
            if (beforeService != null)
            {
                beforeAction = beforeService.Actions.FirstOrDefault();
            }

            var result = ((ResourceCatalog)_resourceCatalog).SaveImpl(workspaceID, resource, contents, true, savedPath);

            if (result.Status == ExecStatus.Success)
            {
                if (workspaceID == GlobalConstants.ServerWorkspaceID)
                {
                    CompileTheResourceAfterSave(workspaceID, resource, contents, beforeAction);
                    UpdateResourceDependencies(resource, contents);
                    SavedResourceCompileMessage(workspaceID, resource, result.Message);
                }
                if (ResourceSaved != null)
                {
                    if (workspaceID == GlobalConstants.ServerWorkspaceID)
                    {
                        ResourceSaved(resource);
                    }
                }
            }

            return result;
        }

        private void UpdateResourceDependencies(IResource resource, StringBuilder contents)
        {
            resource.LoadDependencies(contents.ToXElement());
        }

        protected void CompileTheResourceAfterSave(Guid workspaceID, IResource resource, StringBuilder contents, ServiceAction beforeAction)
        {
            if (beforeAction != null)
            {
                // Compile the service 
                ServiceModelCompiler smc = new ServiceModelCompiler();

                var messages = GetCompileMessages(resource, contents, beforeAction, smc);
                if (messages != null)
                {
                    var keys = ((ResourceCatalog)_resourceCatalog).WorkspaceResources.Keys.ToList();
                    CompileMessageRepo.Instance.AddMessage(workspaceID, messages); //Sends the message for the resource being saved

                    var dependsMessageList = new List<ICompileMessageTO>();
                    keys.ForEach(workspace =>
                    {
                        dependsMessageList.AddRange(UpdateDependantResourceWithCompileMessages(workspace, resource, messages));
                    });
                    _resourceCatalog.SendResourceMessages?.Invoke(resource.ResourceID, dependsMessageList);
                }
            }
        }
        private IEnumerable<ICompileMessageTO> UpdateDependantResourceWithCompileMessages(Guid workspaceID, IResource resource, IList<ICompileMessageTO> messages)
        {
            var resourceId = resource.ResourceID;
            var dependants = _resourceCatalog.GetDependentsAsResourceForTrees(workspaceID, resourceId);
            var dependsMessageList = new List<ICompileMessageTO>();
            foreach (var dependant in dependants)
            {
                var affectedResource = _resourceCatalog.GetResource(workspaceID, dependant.ResourceID);
                foreach (var compileMessageTO in messages)
                {
                    compileMessageTO.WorkspaceID = workspaceID;
                    compileMessageTO.UniqueID = dependant.UniqueID;
                    if (affectedResource != null)
                    {
                        compileMessageTO.ServiceName = affectedResource.ResourceName;
                        compileMessageTO.ServiceID = affectedResource.ResourceID;
                    }
                    dependsMessageList.Add(compileMessageTO.Clone());
                }
                if (affectedResource != null)
                {
                    Common.UpdateResourceXml(_resourceCatalog, workspaceID, affectedResource, messages);
                }
            }
            CompileMessageRepo.Instance.AddMessage(workspaceID, dependsMessageList);
            return dependsMessageList;
        }


        private static IList<ICompileMessageTO> GetCompileMessages(IResource resource, StringBuilder contents, ServiceAction beforeAction, ServiceModelCompiler smc)
        {
            List<ICompileMessageTO> messages = new List<ICompileMessageTO>();
            switch (beforeAction.ActionType)
            {
                case enActionType.Workflow:
                    messages.AddRange(smc.Compile(resource.ResourceID, ServerCompileMessageType.WorkflowMappingChangeRule, beforeAction.ResourceDefinition, contents));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return messages;
        }

        private void SavedResourceCompileMessage(Guid workspaceID, IResource resource, string saveMessage)
        {
            var savedResourceCompileMessage = new List<ICompileMessageTO>
            {
                new CompileMessageTO
                {
                    ErrorType = ErrorType.None,
                    MessageID = Guid.NewGuid(),
                    MessagePayload = saveMessage,
                    ServiceID = resource.ResourceID,
                    ServiceName = resource.ResourceName,
                    MessageType = CompileMessageType.ResourceSaved,
                    WorkspaceID = workspaceID,
                }
            };

            CompileMessageRepo.Instance.AddMessage(workspaceID, savedResourceCompileMessage);
        }
        private XElement SaveToDisk(IResource resource, StringBuilder contents, string directoryName, TxFileManager fileManager)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (_dev2FileWrapper.Exists(resource.FilePath))
            {
                // Remove readonly attribute if it is set
                var attributes = _dev2FileWrapper.GetAttributes(resource.FilePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    _dev2FileWrapper.SetAttributes(resource.FilePath, attributes ^ FileAttributes.ReadOnly);
                }
            }

            XElement xml = contents.ToXElement();
            xml = resource.UpgradeXml(xml, resource);
            StringBuilder result = xml.ToStringBuilder();

            var signedXml = HostSecurityProvider.Instance.SignXml(result);

            lock (Common.GetFileLock(resource.FilePath))
            {
                signedXml.WriteToFile(resource.FilePath, Encoding.UTF8, fileManager);
            }
            return xml;
        }
        private static bool AddToCatalog(IResource resource, List<IResource> resources, TxFileManager fileManager, XElement xml)
        {
            var index = resources.IndexOf(resource);
            var updated = false;
            if (index != -1)
            {
                var existing = resources[index];
                if (!string.Equals(existing.FilePath, resource.FilePath, StringComparison.CurrentCultureIgnoreCase))
                {
                    fileManager.Delete(existing.FilePath);
                }
                resources.RemoveAt(index);
                updated = true;
            }
            resource.GetInputsOutputs(xml);
            resource.ReadDataList(xml);
            resource.SetIsNew(xml);
            resource.UpdateErrorsBasedOnXML(xml);

            resources.Add(resource);
            return updated;
        }

        private void PerformSaveResult(out ResourceCatalogResult saveResult, Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting, string savedPath)
        {
            var fileManager = new TxFileManager();
            using (TransactionScope tx = new TransactionScope())
            {
                try
                {
                    var resources = _resourceCatalog.GetResources(workspaceID);
                    var conflicting = resources.FirstOrDefault(r => resource.ResourceID != r.ResourceID && r.GetResourcePath(workspaceID) != null && r.GetResourcePath(workspaceID).Equals(savedPath + "\\" + resource.ResourceName, StringComparison.InvariantCultureIgnoreCase) && r.ResourceName.Equals(resource.ResourceName, StringComparison.InvariantCultureIgnoreCase));
                    if (conflicting != null && !conflicting.IsNewResource || conflicting != null && !overwriteExisting)
                    {
                        saveResult = ResourceCatalogResultBuilder.CreateDuplicateMatchResult(string.Format(ErrorResource.TypeConflict, conflicting.ResourceType));
                        return;
                    }
                    var res = resources.FirstOrDefault(p => p.ResourceID == resource.ResourceID);
                    if (res != null)//Found Existing resource
                    {
                        if (res.ResourceName != resource.ResourceName) // Renamed while open
                        {
                            var resourceXml = contents.ToXElement();
                            resourceXml.SetAttributeValue("Name", res.ResourceName);
                            resourceXml.SetElementValue("DisplayName", res.ResourceName);
                            var actionElement = resourceXml.Element("Action");
                            var xamlElement = actionElement?.Element("XamlDefinition");
                            if (xamlElement != null)
                            {
                                var xamlContent = xamlElement.Value;
                                xamlElement.Value = xamlContent.
                                    Replace("x:Class=\"" + resource.ResourceName + "\"", "x:Class=\"" + res.ResourceName + "\"")
                                    .Replace("Flowchart DisplayName=\""+ resource.ResourceName + "\"", "Flowchart DisplayName=\"" + res.ResourceName + "\"");
                            }
                            resource.ResourceName = res.ResourceName;
                            contents = resourceXml.ToStringBuilder();
                            
                        }
                    }
                    var directoryName = SetResourceFilePath(workspaceID, resource, ref savedPath);

                    #region Save to disk

                    var xml = SaveToDisk(resource, contents, directoryName, fileManager);

                    #endregion

                    #region Add to catalog

                    var updated = AddToCatalog(resource, resources, fileManager, xml);

                    #endregion
                    Dev2Logger.Debug($"Removing Execution Plan for {resource.ResourceID} for workspace {workspaceID}");
                    ((ResourceCatalog)_resourceCatalog).RemoveFromResourceActivityCache(workspaceID, resource);
                    Dev2Logger.Debug($"Removed Execution Plan for {resource.ResourceID} for workspace {workspaceID}");
                    Dev2Logger.Debug($"Adding Execution Plan for {resource.ResourceID} for workspace {workspaceID}");
                    ((ResourceCatalog)_resourceCatalog).Parse(workspaceID, resource.ResourceID);
                    Dev2Logger.Debug($"Added Execution Plan for {resource.ResourceID} for workspace {workspaceID}");
                    tx.Complete();
                    saveResult = ResourceCatalogResultBuilder.CreateSuccessResult($"{(updated ? "Updated" : "Added")} {resource.ResourceType} '{resource.ResourceName}'");
                }
                catch (Exception)
                {
                    Transaction.Current.Rollback();
                    throw;
                }
            }
        }

        public string SetResourceFilePath(Guid workspaceID, IResource resource, ref string savedPath)
        {
            if(savedPath.EndsWith("\\"))
            {
                savedPath = savedPath.TrimEnd('\\');
            }
            if(savedPath.StartsWith("\\"))
            {
                savedPath = savedPath.TrimStart('\\');
            }
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var directoryName = Path.Combine(workspacePath, savedPath);
            var resourceFilePath = Path.Combine(directoryName, resource.ResourceName + ".xml");
            resource.FilePath = resourceFilePath;
            return directoryName;
        }

        #endregion
    }
}