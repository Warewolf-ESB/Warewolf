using Dev2.Diagnostics;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dev2.Studio.Diagnostics
{
    public class DebugOutputTreeGenerationStrategy
    {
        #region Class Members

        readonly DebugOutputFilterStrategy _debugOutputFilterStrategy;
        readonly IFrameworkRepository<IEnvironmentModel> _environmentRepository;

        #endregion Class Members

        #region Constructor

        public DebugOutputTreeGenerationStrategy(IFrameworkRepository<IEnvironmentModel> environmentRepository = null)
        {
            _environmentRepository = environmentRepository;
            _debugOutputFilterStrategy = new DebugOutputFilterStrategy();
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Places the content in tree.
        /// </summary>
        /// <param name="content">The content.</param>
        public DebugTreeViewItemViewModel PlaceContentInTree(ObservableCollection<DebugTreeViewItemViewModel> rootItems, List<object> existingContent,
            object newContent, string filterText, bool addedAsParent, int depthLimit)
        {
            int depthCount = 0;
            return PlaceContentInTree(rootItems, existingContent, newContent, filterText, addedAsParent, depthLimit, ref depthCount);
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Places the content in tree.
        /// </summary>
        /// <param name="content">The content.</param>
        private DebugTreeViewItemViewModel PlaceContentInTree(ObservableCollection<DebugTreeViewItemViewModel> rootItems, List<object> existingContent,
            object newContent, string filterText, bool addedAsParent, int depthLimit, ref int operationDepth)
        {
            //
            // Check if content should be placed in the tree
            //
            if(!string.IsNullOrWhiteSpace(filterText) && !_debugOutputFilterStrategy.Filter(newContent, filterText)) return null;

            DebugTreeViewItemViewModel newItem = null;
            IDebugState debugState = newContent as IDebugState;

            if(debugState != null)
            {
                //
                // Find the node which to add the item to
                //
                DebugTreeViewItemViewModel parentItem = null;
                if(!addedAsParent)
                {
                    parentItem = FindParent(rootItems, existingContent, debugState, depthLimit, ref operationDepth);
                }

                if(parentItem == null)
                {
                    parentItem = AddMissingParent(rootItems, existingContent, debugState, depthLimit, ref operationDepth);
                }

                if(depthLimit <= 0 || operationDepth < depthLimit)
                {
                    if(parentItem == null)
                    {
                        //
                        // Add as root node
                        //
                        newItem = new DebugStateTreeViewItemViewModel(_environmentRepository, debugState, null, true, false, addedAsParent);
                        AddOrInsertItem(rootItems, existingContent, debugState, newItem, addedAsParent);
                        return newItem;
                    }
                    else
                    {
                        newItem = new DebugStateTreeViewItemViewModel(_environmentRepository, debugState, parent: parentItem, isExpanded: true, isSelected: false, addedAsParent: addedAsParent);
                        AddOrInsertItem(parentItem.Children, existingContent, debugState, newItem, addedAsParent);
                        return newItem;
                    }
                }
            }
            else if(newContent is string && !string.IsNullOrWhiteSpace(newContent.ToString()))
            {
                newItem = new DebugStringTreeViewItemViewModel(newContent as string, null, true, false, addedAsParent);
                AddOrInsertItem(rootItems, existingContent, newContent, newItem, addedAsParent);
                return newItem;
            }

            return null;
        }

        /// <summary>
        /// Adds the or insert item.
        /// </summary>
        /// <param name="destinationCollection">The destination collection.</param>
        /// <param name="content">The content to insert.</param>
        /// <param name="treeviewItem">The treeview item.</param>
        /// <param name="addedAsParent">if set to <c>true</c> [added as parent].</param>
        /// <exception cref="System.Exception">Content not found in original list.</exception>
        private void AddOrInsertItem(ObservableCollection<DebugTreeViewItemViewModel> destinationCollection, List<object> existingContent,
            object content, DebugTreeViewItemViewModel treeviewItem, bool addedAsParent)
        {
            if(!addedAsParent)
            {
                destinationCollection.Add(treeviewItem);
            }
            else
            {
                int originalIndex = existingContent.IndexOf(content);

                if(originalIndex < 0)
                {
                    throw new Exception("Content not found in original list.");
                }

                bool insterted = false;
                for(int i = 0; i < destinationCollection.Count; i++)
                {
                    int itemIndex = existingContent.IndexOf(destinationCollection[i]);
                    if(itemIndex > originalIndex)
                    {
                        insterted = true;
                        destinationCollection.Insert(i, treeviewItem);
                        break;
                    }
                }

                if(!insterted)
                {
                    destinationCollection.Add(treeviewItem);
                }
            }
        }

        /// <summary>
        /// Finds the parent.
        /// </summary>
        /// <param name="parentID">The parent ID.</param>
        private DebugTreeViewItemViewModel FindParent(ObservableCollection<DebugTreeViewItemViewModel> rootItems, List<object> existingContent,
            IDebugState debugState, int depthLimit, ref int operationDepth)
        {
            if(string.IsNullOrWhiteSpace(debugState.ParentID) || debugState.DisplayName == debugState.ParentID)
            {
                return null;
            }

            foreach(DebugTreeViewItemViewModel item in rootItems)
            {
                DebugTreeViewItemViewModel match = item.FindSelfOrChild(n =>
                {
                    DebugStateTreeViewItemViewModel debugStateTreeViewItemViewModel = n as DebugStateTreeViewItemViewModel;
                    if(debugStateTreeViewItemViewModel == null)
                    {
                        return false;
                    }

                    return debugStateTreeViewItemViewModel.Content.DisplayName == debugState.ParentID;
                });

                if(match != null)
                {
                    operationDepth = match.GetDepth() + 1;
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the parent.
        /// </summary>
        /// <param name="parentID">The parent ID.</param>
        private DebugTreeViewItemViewModel AddMissingParent(ObservableCollection<DebugTreeViewItemViewModel> rootItems, List<object> existingContent,
            IDebugState debugState, int depthLimit, ref int operationDepth)
        {
            if(string.IsNullOrWhiteSpace(debugState.ParentID) || debugState.ID == debugState.ParentID)
            {
                return null;
            }

            IDebugState parent = existingContent.FirstOrDefault(o => o is IDebugState && o != debugState && ((IDebugState)o).ID == debugState.ParentID) as IDebugState;
            if(parent == null)
            {
                return null;
            }

            operationDepth++;
            return PlaceContentInTree(rootItems, existingContent, parent, "", true, depthLimit, ref operationDepth);
        }

        #endregion Private Methods
    }
}
