using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Resource.Errors;
// ReSharper disable PrivateMembersMustHaveComments
// ReSharper disable PublicMembersMustHaveComments

namespace Dev2.Runtime.ResourceCatalogImpl
{
    public class ResourceRenameProvider : IResourceRenameProvider
    {
        private readonly IServerVersionRepository _versionRepository;
        private readonly FileWrapper _dev2FileWrapper = new FileWrapper();

        public ResourceRenameProvider(IServerVersionRepository versionRepository)
        {
            _versionRepository = versionRepository;
        }

        #region Implementation of IResourceRenameProvider

        public ResourceCatalogResult RenameResource(Guid workspaceID, Guid? resourceID, string newName)
        {
            GlobalConstants.HandleEmptyParameters(resourceID, "resourceID");
            GlobalConstants.HandleEmptyParameters(newName, "newName");
            var resourcesToUpdate = ResourceCatalog.Instance.GetResources(workspaceID).Where(resource => resource.ResourceID == resourceID).ToArray();
            try
            {
                if (!resourcesToUpdate.Any())
                {
                    return ResourceCatalogResultBuilder.CreateFailResult($"{ErrorResource.FailedToFindResource} '{resourceID}' to '{newName}'");
                }

                {
                    // ReSharper disable once PossibleInvalidOperationException
                    _versionRepository.StoreVersion(ResourceCatalog.Instance.GetResource(Guid.Empty, resourceID.Value), "unknown", "Rename", workspaceID);
                    //rename and save to workspace
                    var renameResult = UpdateResourceName(workspaceID, resourcesToUpdate[0], newName);
                    if (renameResult.Status != ExecStatus.Success)
                    {
                        return ResourceCatalogResultBuilder.CreateFailResult($"{ErrorResource.FailedToRenameResource} '{resourceID}' to '{newName}'");
                    }
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                return ResourceCatalogResultBuilder.CreateFailResult($"{ErrorResource.FailedToRenameResource} '{resourceID}' to '{newName}'");

            }
            return ResourceCatalogResultBuilder.CreateSuccessResult($"{"Renamed Resource"} '{resourceID}' to '{newName}'");
        }

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory)
        {
            GlobalConstants.HandleEmptyParameters(oldCategory, "oldCategory");
            GlobalConstants.HandleEmptyParameters(newCategory, "newCategory");
            try
            {
                var resourcesToUpdate = ResourceCatalog.Instance.GetResources(workspaceID).Where(resource => resource.ResourcePath.StartsWith(oldCategory + "\\", StringComparison.OrdinalIgnoreCase)).ToList();

                return RenameCategory(workspaceID, oldCategory, newCategory, resourcesToUpdate);
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Rename Category error", err);
                return ResourceCatalogResultBuilder.CreateFailResult($"<CompilerMessage>{"Failed to Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");
            }
        }

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory, List<IResource> resourcesToUpdate)
        {
            if (resourcesToUpdate.Count == 0)
            {
                return ResourceCatalogResultBuilder.CreateNoMatchResult($"<CompilerMessage>No Resources found in '{oldCategory}'</CompilerMessage>");
            }
            return PerformUpdate(workspaceID, oldCategory, newCategory, resourcesToUpdate);
        }

        #endregion

        #region Private Methods

        ResourceCatalogResult PerformUpdate(Guid workspaceID, string oldCategory, string newCategory, IEnumerable<IResource> resourcesToUpdate)
        {
            try
            {
                var hasError = false;
                foreach (var resource in resourcesToUpdate)
                {
                    var resourceCatalogResult = UpdateResourcePath(workspaceID, resource, oldCategory, newCategory);
                    if (resourceCatalogResult.Status != ExecStatus.Success)
                    {
                        hasError = true;
                    }
                }

                var failureResult = ResourceCatalogResultBuilder.CreateFailResult($"<CompilerMessage>{"Failed to Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");
                var successResult = ResourceCatalogResultBuilder.CreateSuccessResult($"<CompilerMessage>{"Updated Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");

                return hasError ? failureResult : successResult;
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Rename Category error", err);
                return ResourceCatalogResultBuilder.CreateFailResult($"<CompilerMessage>{"Failed to Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");
            }
        }
        ResourceCatalogResult UpdateResourcePath(Guid workspaceID, IResource resource, string oldCategory, string newCategory)
        {
            var resourceContents = ResourceCatalog.Instance.GetResourceContents(workspaceID, resource.ResourceID);
            var oldPath = resource.ResourcePath;
            var cat = oldCategory.Replace("\\", "\\\\");
            var newPath = Regex.Replace(oldPath, cat, newCategory, RegexOptions.IgnoreCase);
            resource.ResourcePath = newPath;
            var resourceElement = resourceContents.ToXElement();
            var categoryElement = resourceElement.Element("Category");
            if (categoryElement == null)
            {
                resourceElement.Add(new XElement("Category", newPath));
            }
            else
            {
                categoryElement.SetValue(newPath);
            }
            var contents = resourceElement.ToStringBuilder();
            var resourceCatalogResult = ResourceCatalog.Instance.SaveImpl(workspaceID, resource, contents, false);
            if (resourceCatalogResult.Status != ExecStatus.Success)
            {
                resource.ResourcePath = oldPath;
            }
            return resourceCatalogResult;
        }
        ResourceCatalogResult UpdateResourceName(Guid workspaceID, IResource resource, string newName)
        {
            //rename where used
            var oldCategory = resource.ResourcePath;
            string newCategory = "";
            var indexOfCategory = resource.ResourcePath.LastIndexOf(resource.ResourceName, StringComparison.Ordinal);
            if (indexOfCategory > 0)
            {
                newCategory = oldCategory.Substring(0, indexOfCategory) + newName;
            }
            RenameWhereUsed(ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, resource.ResourceID), workspaceID, resource.ResourcePath, newName);

            //rename resource
            var resourceContents = ResourceCatalog.Instance.GetResourceContents(workspaceID, resource.ResourceID);

            var resourceElement = resourceContents.ToXElement();
            //xml name attibute
            var nameAttrib = resourceElement.Attribute("Name");
            string oldName = null;
            if (nameAttrib == null)
            {
                resourceElement.Add(new XAttribute("Name", newName));
            }
            else
            {
                oldName = nameAttrib.Value;
                nameAttrib.SetValue(newName);
            }
            //xaml
            var actionElement = resourceElement.Element("Action");
            var xaml = actionElement?.Element("XamlDefinition");
            xaml?.SetValue(xaml.Value
                .Replace("x:Class=\"" + oldName, "x:Class=\"" + newName)
                .Replace("ToolboxFriendlyName=\"" + oldName, "ToolboxFriendlyName=\"" + newName)
                .Replace("DisplayName=\"" + oldName, "DisplayName=\"" + newName)
                .Replace("Category=\"" + oldCategory, "Category=\"" + newCategory))
                ;
            //xml display name element
            var displayNameElement = resourceElement.Element("DisplayName");
            displayNameElement?.SetValue(newName);
            var categoryElement = resourceElement.Element("Category");
            categoryElement?.SetValue(newCategory);
            var resPath = CalcResPath(resource);

            //delete old resource in local workspace without updating dependants with compile messages
            lock (Common.GetFileLock(resource.FilePath))
            {
                if (_dev2FileWrapper.Exists(resource.FilePath))
                {
                    lock (Common.GetFileLock(resource.FilePath))
                    {
                        _dev2FileWrapper.Delete(resource.FilePath);
                    }
                }
            }

            resource.ResourcePath = resPath + newName;
            resource.ResourceName = newName;
            //update file path
            if (oldName != null)
            {
                resource.FilePath = resource.FilePath.Replace(oldName, newName);
            }
            //re-create, resign and save to file system the new resource
            StringBuilder contents = resourceElement.ToStringBuilder();

            return ResourceCatalog.Instance.SaveImpl(workspaceID, resource, contents);

        }

        static string CalcResPath(IResource resource)
        {
            var originalRes = resource.ResourcePath ?? "";
            int indexOfName = originalRes.LastIndexOf(resource.ResourceName, StringComparison.Ordinal);
            var resPath = resource.ResourcePath;
            if (indexOfName >= 0)
            {
                resPath = originalRes.Substring(0, originalRes.LastIndexOf(resource.ResourceName, StringComparison.Ordinal));
            }
            return resPath;
        }

        private void RenameWhereUsed(IEnumerable<ResourceForTree> dependants, Guid workspaceID, string oldName, string newName)
        {
            foreach (var dependant in dependants)
            {
                var dependantResource = ResourceCatalog.Instance.GetResource(workspaceID, dependant.ResourceID);
                //rename where used
                var resourceContents = ResourceCatalog.Instance.GetResourceContents(workspaceID, dependantResource.ResourceID);

                var resourceElement = resourceContents.ToXElement();
                //in the xaml only
                var actionElement = resourceElement.Element("Action");
                if (actionElement != null)
                {
                    var xaml = actionElement.Element("XamlDefinition");
                    var newNameWithPath = newName;
                    if (oldName.IndexOf('\\') > 0)
                        newNameWithPath = oldName.Substring(0, 1 + oldName.LastIndexOf("\\", StringComparison.Ordinal)) + newName;
                    xaml?.SetValue(xaml.Value
                        .Replace("DisplayName=\"" + oldName, "DisplayName=\"" + newNameWithPath)
                        .Replace("ServiceName=\"" + oldName, "ServiceName=\"" + newName)
                        .Replace("ToolboxFriendlyName=\"" + oldName, "ToolboxFriendlyName=\"" + newName));
                }
                //delete old resource
                lock(Common.GetFileLock(dependantResource.FilePath))
                {
                    if (_dev2FileWrapper.Exists(dependantResource.FilePath))
                    {
                        lock (Common.GetFileLock(dependantResource.FilePath))
                        {
                            _dev2FileWrapper.Delete(dependantResource.FilePath);
                        }
                    }
                }
                //update dependencies
                var renameDependent = dependantResource.Dependencies.FirstOrDefault(dep => dep.ResourceName == oldName);
                if (renameDependent != null)
                {
                    renameDependent.ResourceName = newName;
                }
                //re-create, resign and save to file system the new resource
                StringBuilder result = resourceElement.ToStringBuilder();

                ResourceCatalog.Instance.SaveImpl(workspaceID, dependantResource, result);
            }
        }

        #endregion

    }
}