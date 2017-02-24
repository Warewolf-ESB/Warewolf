using System;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    internal class ExplorerDeleteProvider: IExplorerDeleteProvider
    {
        private readonly IExplorerRepository _repository;

        public ExplorerDeleteProvider(IExplorerRepository repository)
        {
            _repository = repository;
        }

        #region Implementation of IExplorerDeleteProvider

        public IDeletedFileMetadata Delete(IExplorerItemViewModel explorerItemViewModel)
        {
            try
            {
                if (explorerItemViewModel != null)
                {
                    var graphGenerator = new DependencyGraphGenerator();
                    if (explorerItemViewModel.ResourceType != "Version" && explorerItemViewModel.ResourceType != "Folder")
                    {
                        var dep = _repository.QueryManagerProxy.FetchDependants(explorerItemViewModel.ResourceId);
                        var deleteFileMeta = _repository.HasDependencies(explorerItemViewModel, graphGenerator, dep);
                        if (deleteFileMeta.IsDeleted || deleteFileMeta.DeleteAnyway)
                        {
                            deleteFileMeta.IsDeleted = true;
                            _repository.UpdateManagerProxy.DeleteResource(explorerItemViewModel.ResourceId);
                        }
                        return deleteFileMeta;
                    }
                    if (explorerItemViewModel.ResourceType == "Version")
                    {
                        _repository.VersionManager.DeleteVersion(explorerItemViewModel.ResourceId, explorerItemViewModel.VersionNumber, explorerItemViewModel.ResourcePath);
                    }
                    else if (explorerItemViewModel.ResourceType == "Folder")
                    {
                        var explorerItemViewModels = explorerItemViewModel.AsList();
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        var deleteFileMetaData = new DeletedFileMetadata
                        {
                            IsDeleted = true,
                            ShowDependencies = false
                        };
                        bool showDependenciesApplyToAll = false;
                        foreach (IExplorerItemViewModel itemViewModel in explorerItemViewModels)
                        {
                            if (itemViewModel.ResourceType != "Folder")
                            {
                                var dependants = _repository.QueryManagerProxy.FetchDependants(itemViewModel.ResourceId);
                                if (dependants != null)
                                {
                                    if (showDependenciesApplyToAll)
                                    {
                                        var graph = graphGenerator.BuildGraph(dependants.Message, "", 1000, 1000, 1);
                                        if (graph.Nodes.Count > 1)
                                        {
                                            itemViewModel.ShowDependencies();
                                        }
                                    }
                                    else
                                    {
                                        var deletedFileMetadata = _repository.HasDependencies(itemViewModel, graphGenerator, dependants);

                                        if (deletedFileMetadata.DeleteAnyway && deletedFileMetadata.ApplyToAll)
                                        {
                                            deleteFileMetaData.IsDeleted = true;
                                            deleteFileMetaData.ResourceId = itemViewModel.ResourceId;
                                            break;
                                        }
                                        if (deletedFileMetadata.DeleteAnyway && !deletedFileMetadata.ApplyToAll)
                                        {
                                            explorerItemViewModel.RemoveChild(itemViewModel);
                                            _repository.UpdateManagerProxy.DeleteResource(itemViewModel.ResourceId);
                                        }

                                        if (!deletedFileMetadata.IsDeleted)
                                        {
                                            deleteFileMetaData.IsDeleted = false;
                                            deleteFileMetaData.ShowDependencies = true;
                                            deleteFileMetaData.ResourceId = itemViewModel.ResourceId;

                                            if (deletedFileMetadata.ApplyToAll && deletedFileMetadata.ShowDependencies)
                                            {
                                                showDependenciesApplyToAll = deletedFileMetadata.ShowDependencies;
                                            }
                                            else if (deletedFileMetadata.ApplyToAll)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (deleteFileMetaData.IsDeleted)
                        {
                            if (!string.IsNullOrWhiteSpace(explorerItemViewModel.ResourcePath))
                            {
                                _repository.UpdateManagerProxy.DeleteFolder(explorerItemViewModel.ResourcePath);
                            }
                        }
                        return deleteFileMetaData;
                    }
                }
                return new DeletedFileMetadata
                {
                    IsDeleted = true
                };
            }
            catch (Exception)
            {
                return new DeletedFileMetadata
                {
                    IsDeleted = false
                };
            }
        }

        #endregion
    }
}