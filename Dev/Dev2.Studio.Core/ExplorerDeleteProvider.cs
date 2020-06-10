#pragma warning disable
using System;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    class ExplorerDeleteProvider: IExplorerDeleteProvider
    {
        readonly IExplorerRepository _repository;

        public ExplorerDeleteProvider(IExplorerRepository repository)
        {
            _repository = repository;
        }

        #region Implementation of IExplorerDeleteProvider

        public IDeletedFileMetadata TryDelete(IExplorerItemViewModel explorerItemViewModel)
        {
            try
            {
                return Delete(explorerItemViewModel);
            }
            catch (Exception)
            {
                return new DeletedFileMetadata
                {
                    IsDeleted = false
                };
            }
        }

        IDeletedFileMetadata Delete(IExplorerItemViewModel explorerItemViewModel)
        {
            if (explorerItemViewModel != null)
            {
                var graphGenerator = new DependencyGraphGenerator();
                if (explorerItemViewModel.ResourceType != "Version" && explorerItemViewModel.ResourceType != "Folder")
                {
                    var dep = _repository.QueryManagerProxy.FetchDependants(explorerItemViewModel.ResourceId);
                    var deleteFileMeta = _repository.HasDependencies(explorerItemViewModel, graphGenerator, dep, CustomContainer.Get<IPopupController>());
                    if (deleteFileMeta.IsDeleted || deleteFileMeta.DeleteAnyway)
                    {
                        deleteFileMeta.IsDeleted = true;
                        _repository.UpdateManagerProxy.DeleteResource(explorerItemViewModel.ResourceId);
                    }
                    return deleteFileMeta;
                }
                if (explorerItemViewModel.ResourceType == "Version")
                {
                    _repository.VersionManager.DeleteVersion(explorerItemViewModel.ResourceId, explorerItemViewModel.VersionNumber, explorerItemViewModel.Parent.ResourcePath);
                }
                else
                {
                    if (explorerItemViewModel.ResourceType == "Folder")
                    {
                        return DeleteFolder(explorerItemViewModel, graphGenerator);
                    }
                }
            }
            return new DeletedFileMetadata
            {
                IsDeleted = true
            };
        }

        IDeletedFileMetadata DeleteFolder(IExplorerItemViewModel explorerItemViewModel, DependencyGraphGenerator graphGenerator)
        {
            var explorerItemViewModels = explorerItemViewModel.AsList();

            var deleteFileMetaData = new DeletedFileMetadata
            {
                IsDeleted = true,
                ShowDependencies = false
            };
            var showDependenciesApplyToAll = false;
            foreach (IExplorerItemViewModel itemViewModel in explorerItemViewModels)
            {
                if (itemViewModel.ResourceType != "Folder")
                {
                    var dependants = _repository.QueryManagerProxy.FetchDependants(itemViewModel.ResourceId);
                    var deletedFileMetadata = _repository.HasDependencies(itemViewModel, graphGenerator, dependants, CustomContainer.Get<IPopupController>());
                    ShowDependencies(graphGenerator, deleteFileMetaData, showDependenciesApplyToAll, itemViewModel, dependants, deletedFileMetadata);

                    if (dependants != null && !showDependenciesApplyToAll && deletedFileMetadata.DeleteAnyway && deletedFileMetadata.ApplyToAll)
                    {
                        break;
                    }

                    showDependenciesApplyToAll = ShowDependenciesApplyToAll(explorerItemViewModel, deleteFileMetaData, showDependenciesApplyToAll, itemViewModel, dependants, deletedFileMetadata);

                    var deletedConditions = !deletedFileMetadata.IsDeleted && !(deletedFileMetadata.ApplyToAll && deletedFileMetadata.ShowDependencies) && deletedFileMetadata.ApplyToAll;
                    if (dependants != null && !showDependenciesApplyToAll && deletedConditions)
                    {
                        break;
                    }
                }
            }
            if (deleteFileMetaData.IsDeleted && !string.IsNullOrWhiteSpace(explorerItemViewModel.ResourcePath))
            {
                _repository.UpdateManagerProxy.DeleteFolder(explorerItemViewModel.ResourcePath);
            }

            return deleteFileMetaData;
        }

        static void ShowDependencies(DependencyGraphGenerator graphGenerator, DeletedFileMetadata deleteFileMetaData, bool showDependenciesApplyToAll, IExplorerItemViewModel itemViewModel, Dev2.Common.Interfaces.Infrastructure.Communication.IExecuteMessage dependants, IDeletedFileMetadata deletedFileMetadata)
        {
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

                    if (deletedFileMetadata.DeleteAnyway && deletedFileMetadata.ApplyToAll)
                    {
                        deleteFileMetaData.IsDeleted = true;
                        deleteFileMetaData.ResourceId = itemViewModel.ResourceId;
                    }
                }
            }
        }

        private bool ShowDependenciesApplyToAll(IExplorerItemViewModel explorerItemViewModel, DeletedFileMetadata deleteFileMetaData, bool showDependenciesApplyToAll, IExplorerItemViewModel itemViewModel, Dev2.Common.Interfaces.Infrastructure.Communication.IExecuteMessage dependants, IDeletedFileMetadata deletedFileMetadata)
        {
            if (dependants != null && !showDependenciesApplyToAll)
            {
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
                }
            }

            return showDependenciesApplyToAll;
        }

        #endregion
    }
}