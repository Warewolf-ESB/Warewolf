using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Represents an object that controls all the <see cref="XamDataTreeNode"/>s at a given level.
    /// </summary>
    public class NodesManager : IProvideDataItems<XamDataTreeNode>, IComparable<NodesManager>
    {
        #region Members

        List<NodesManager> _visibleChildManagers;
        ReadOnlyCollection<NodesManager> _readOnlyVisibleChildManagers;

        DataManagerBase _dataManager;
        XamDataTreeNodesCollection _nodes;

        Dictionary<object, XamDataTreeNode> _cachedNodes;
        Dictionary<object, int> _duplicateObjectValidator;

        IEnumerable _itemSource;
        NodeLayout _nodeLayout;
        bool _isDisposed, _supressInvalidateNodes;     

        List<WeakReference> _attachedPropertyChangedTargets = new List<WeakReference>();

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodesManager"/> class.
        /// </summary>
        /// <param propertyName="level">The level that <see cref="NodesManager"/> is at.</param>
        /// <param propertyName="nodeLayout">A reference to <see cref="NodeLayout"/> object that corresponds with the <see cref="NodesManager"/></param>
        /// <param propertyName="parentLayoutNode">The owning node of the <see cref="NodesManager"/>. </param>
        internal NodesManager(int level, NodeLayout nodeLayout, XamDataTreeNode parentLayoutNode)
        {
            this.NodeLayout = nodeLayout;
            this._visibleChildManagers = new List<NodesManager>();
            this._readOnlyVisibleChildManagers = new ReadOnlyCollection<NodesManager>(this._visibleChildManagers);

            this.Level = level;
            this.ParentNode = parentLayoutNode;
            this._nodes = new XamDataTreeNodesCollection(this);

            this._cachedNodes = new Dictionary<object, XamDataTreeNode>();
            this._duplicateObjectValidator = new Dictionary<object, int>();

            this.EnsureDataManager();
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region Nodes

        /// <summary>
        /// Gets the collection of child nodes that belongs to the <see cref="NodesManager"/>.
        /// </summary>
        public XamDataTreeNodesCollection Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        #endregion // Nodes

        #region ItemsSource

        /// <summary>
        /// Gets the data source for the <see cref="NodesManager"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return this._itemSource; }
            protected internal set
            {
                if (this._itemSource != value)
                {
                    this._itemSource = value;
                    this.OnItemsSourceChanged();
                }
            }
        }

        #endregion // ItemsSource

        #region ParentNode

        /// <summary>
        /// The <see cref="ParentNode"/> that owns this <see cref="NodesManager"/>.
        /// </summary>
        public virtual XamDataTreeNode ParentNode
        {
            get;
            protected set;
        }

        #endregion // ParentNode

        #region NodeLayout

        /// <summary>
        /// Gets the <see cref="NodeLayout"/> object that is associated with the <see cref="NodesManager"/>.
        /// </summary>
        public NodeLayout NodeLayout
        {
            get { return this._nodeLayout; }
            protected internal set
            {
                if (value != this.NodeLayout)
                {
                    this._isDisposed = false;
                    this.OnNodeLayoutAssigned(value);
                }
            }
        }

        #endregion // NodeLayout

        #region Level

        /// <summary>
        /// Gets the level in the hierarchy of the <see cref="NodesManager"/>.
        /// </summary>
        public virtual int Level
        {
            get;
            protected internal set;
        }

        #endregion // Level

        #endregion // Public

        #region Protected

        #region DataManagerBase

        /// <summary>
        /// Gets a reference to the <see cref="DataManagerBase"/> of the <see cref="NodesManager"/>.
        /// </summary>
        protected internal virtual DataManagerBase DataManager
        {
            get
            {
                this.EnsureDataManager();
                return this._dataManager;
            }
        }
        #endregion // DataManagerBase

        #region VisibleChildManagers

        /// <summary>
        /// Gets a list of currently visible child <see cref="NodesManager"/> objects.
        /// </summary>
        protected internal ReadOnlyCollection<NodesManager> VisibleChildManagers
        {
            get
            {
                return this._readOnlyVisibleChildManagers;
            }
        }

        #endregion // VisibleChildManagers

        #region DataCount

        /// <summary>
        /// Gets the amount of <see cref="XamDataTreeNode"/>s in the <see cref="NodesManager"/>.
        /// </summary>
        protected virtual int DataCount
        {
            get
            {
                DataManagerBase manager = this.DataManager;
                if (manager != null)
                {
                    return manager.RecordCount;
                }
                return 0;
            }
        }
        #endregion // DataCount

        #region FullNodeCount

        /// <summary>
        /// Gets the total amount of nodes that can be displayed for the <see cref="NodesManager"/>.
        /// </summary>
        protected internal virtual int FullNodeCount
        {
            get
            {
                if (this.NodeLayout != null && this.NodeLayout.Visibility != Visibility.Collapsed)
                {
                    return this.DataCount;
                }

                return 0;
            }
        }

        #endregion // FullNodeCount

        #region CachedNodes
        /// <summary>
        /// Gets the Dictionary of nodes that have been cached.
        /// </summary>
        protected Dictionary<object, XamDataTreeNode> CachedNodes
        {
            get { return this._cachedNodes; }
        }
        #endregion // CachedNodes

        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Public

        #region InvalidateNodes
        /// <summary>
        /// Clears nodes and child nodes and lets the tree rerender.
        /// </summary>
        public void InvalidateNodes()
        {
            this.InvalidateNodes(true);
        }

        private void InvalidateNodes(bool unregister)
        {
            if (!this._supressInvalidateNodes)
            {
                if (unregister)
                {
                    // Only clear the nodes, when we're unregistering.
                    this.ClearNodes();

                    this.UnregisterAllChildNodesManager();
                }

                if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    this.NodeLayout.Tree.InvalidateScrollPanel(true, true);
            }
        }
        #endregion // InvalidateNodes

        #region ClearCacheInformation

        /// <summary>
        /// Clears the cached nodes collection for this <see cref="NodesManager"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="NodesManager"/> retains information, linking the data that the node is generated from and the node.  
        /// This information is retained even if the underlying object is removed, so that if the same object appears again in the collection the node can be brought back.
        /// 
        /// If you wish to completely reset this association, this method can be called, however it will remove all associations.
        /// </remarks>
        public void ClearCacheInformation()
        {
            if (this._cachedNodes != null)
                this._cachedNodes.Clear();            
        }

        /// <summary>
        /// Removes the cached node information for this <see cref="NodesManager"/>.        
        /// </summary>
        /// <remarks>
        /// The <see cref="NodesManager"/> retains information, linking the data that the node is generated from and the node.  
        /// This information is retained even if the underlying object is removed, so that if the same object appears again in the collection the node can be brought back.
        /// 
        /// This method can be used to remove the retention information by passing in the dataObject that is associated with the <see cref="XamDataTreeNode"/>.
        /// </remarks>
        /// <param name="dataObject"></param>
        public void ClearCacheInformation(object dataObject)
        {
            if (this._cachedNodes != null)
            {
                if (this._cachedNodes.ContainsKey(dataObject))
                {                    
                    this._cachedNodes.Remove(dataObject);
                }
            }
        }

        #endregion // ClearCacheInformation

        #endregion // Public

        #region Protected

        #region GetDataItem

        /// <summary>
        /// Returns the <see cref="XamDataTreeNode"/> for the given index.
        /// </summary>
        /// <param propertyName="index">The index of the node to retrieve.</param>
        /// <returns></returns>
        protected virtual XamDataTreeNode GetDataItem(int index)
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                object data = manager.GetRecord(index);

                XamDataTreeNode node = null;
                if (data != null)
                {
                    if (this._cachedNodes.ContainsKey(data))
                    {
                        



                        if (!this._duplicateObjectValidator.ContainsKey(data))
                        {
                            node = (XamDataTreeNode)this._cachedNodes[data];
                            node.Manager = this;
                            this._duplicateObjectValidator.Add(data, index);
                            node.Index = index;
                            if (node.IsExpanded)
                            {
                                this.RegisterChildNodesManager(node.ChildNodesManager);
                            }
                            return node;
                        }
                        else
                        {
                            return new XamDataTreeNode(index, this, data, false, null);
                        }
                    }

                    node = new XamDataTreeNode(index, this, data, false, null);
                    this._cachedNodes.Add(data, node);
                    this._duplicateObjectValidator.Add(data, index);
                    if (node.IsExpanded)
                    {
                        this.RegisterChildNodesManager(node.ChildNodesManager);
                    }
                }
                else
                    node = new XamDataTreeNode(index, this, data, false, null);

                if (node != null)
                {
                    this.NodeLayout.Tree.OnInitializeNode(node);
                    if (node.IsExpanded)
                    {
                        this.RegisterChildNodesManager(node.ChildNodesManager);
                    }
                }

                return node;
            }
            return null;
        }

        #endregion // GetDataItem

        #region OnItemsSourceChanged

        /// <summary>
        /// Invoked when the the underlying ItemsSource property changes.
        /// </summary>
        protected virtual void OnItemsSourceChanged()
        {
            if (this._dataManager != null && this.NodeLayout!=null)
            {
                XamDataTree tree = this.NodeLayout.Tree;
                if (tree != null)
                {
                    tree.ActiveNode = null;

                    if (this.ItemsSource != null)
                    {
                        Type t = DataManagerBase.ResolveItemType(this.ItemsSource);
                        if (t == this._dataManager.CachedType)
                        {
                            // Be sure to reset the DataManager, otherwise we could have a memory leak,
                            // as the DataManager tends to hold on to the data somehow.
                            this._dataManager.DataSource = null;
                            this.UnhookDataManager(true, true);
                            this.EnsureDataManager();

                            this.InvalidateNodes();
                            this.InvalidateData();
                            return;
                        }
                    }

                    tree.ResetPanelNodes();

                    this.UnhookDataManager(true, true);

                    this.NodeLayout.IsInitialized = false;
                    this.NodeLayout.DataFields = null;

                    tree.InvalidateScrollPanel(true, true);
                }
            }
            else
            {
                this.EnsureDataManager();
            }
        }

        #endregion // OnItemsSourceChanged

        #region EnsureDataManager
        /// <summary>
        /// This method checks to ensure that a DataManagerBase is created for a given level and if not creates it for that level.
        /// </summary>
        protected virtual void EnsureDataManager()
        {
            if (this.ItemsSource == null)
            {
                if (this.ParentNode != null)
                {
                    if (this.ParentNode.ItemsSource != null)
                    {
                        this.ItemsSource = this.ParentNode.ItemsSource;
                    }
                    else
                    {
                        object data = this.ParentNode.Data;
                        if (data != null)
                        {
                            PropertyInfo info = this.NodeLayout.ResolvePropertyInfo(data);
                            if (info != null)
                            {
                                this.ItemsSource = info.GetValue(this.ParentNode.Data, null) as IEnumerable;
                            }
                            else
                            {
                                object obj = DataManagerBase.ResolveValueFromPropertyPath(this.NodeLayout.Key, data);
                                this.ItemsSource = obj as IEnumerable;
                            }

                            // An ItemSource property, could potentially have a setter, and if the property changes
                            // for that IEnumerable, the Grid should reflect the new data. 
                            INotifyPropertyChanged inpc = data as INotifyPropertyChanged;
                            this.AttachDetachPropertyChanged(inpc, true);
                        }
                    }

                    if (this.ItemsSource != null && this._dataManager == null)
                        this.SetupDataManager();
                }
            }
            else if (this._dataManager == null)
            {
                this.SetupDataManager();
            }
        }

        #endregion // EnsureDataManager

        #region OnNodeLayoutAssigned

        /// <summary>
        /// Called when the NodeLayout assigned to this <see cref="NodesManager"/> changes.
        /// </summary>
        /// <param propertyName="layout"></param>
        protected virtual void OnNodeLayoutAssigned(NodeLayout layout)
        {
            if (this._nodeLayout != null)
            {
                this._nodeLayout.PropertyChanged -= OnNodeLayoutPropertyChanged;
                this._nodeLayout.ChildNodeLayoutRemoved -= NodeLayout_ChildNodeLayoutRemoved;
                this._nodeLayout.ChildNodeLayoutAdded -= NodeLayout_ChildNodeLayoutAdded;
                this._nodeLayout.ChildNodeLayoutVisibilityChanged -= NodeLayout_ChildNodeLayoutVisibilityChanged;
                this._nodeLayout.NodeLayoutDisposed -= NodeLayout_NodeLayoutDisposed;

            }

            this._nodeLayout = layout;

            if (this._nodeLayout != null)
            {
                this._nodeLayout.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnNodeLayoutPropertyChanged);
                this._nodeLayout.ChildNodeLayoutRemoved += new EventHandler<NodeLayoutEventArgs>(NodeLayout_ChildNodeLayoutRemoved);
                this._nodeLayout.ChildNodeLayoutAdded += new EventHandler<NodeLayoutEventArgs>(NodeLayout_ChildNodeLayoutAdded);
                this._nodeLayout.ChildNodeLayoutVisibilityChanged += new EventHandler<NodeLayoutEventArgs>(NodeLayout_ChildNodeLayoutVisibilityChanged);
                this._nodeLayout.NodeLayoutDisposed += new EventHandler<EventArgs>(NodeLayout_NodeLayoutDisposed);
            }
        }



        #endregion // OnNodeLayoutAssigned

        #region OnNodeLayoutPropertyChanged

        /// <summary>
        /// Raised when a property has changed on the NodeLayout that this <see cref="NodesManager"/> represents.
        /// </summary>
        /// <param propertyName="layout"></param>
        /// <param propertyName="propertyName"></param>
        protected virtual void OnNodeLayoutPropertyChanged(NodeLayout layout, string propertyName)
        {
            switch (propertyName)
            {
                case ("IsDraggable"):
                case ("IsDropTarget"):
                    {
                        foreach (XamDataTreeNode node in this.Nodes)
                        {
                            node.RaiseNodeNotifyPropertyChanged(propertyName+"Resolved");
                        }
                        break;
                    }
                case ("InvalidateData"):
                    {
                        this.InvalidateData();
                        break;
                    }

				// MD 8/11/11 - XamFormulaEditor
				case ("IsEnabledMemberPathResolved"):
					{
						foreach (XamDataTreeNode node in this.Nodes)
						{
							node.SetupIsEnabledBinding();
						}
						break;
					}

                case ("IsExpandedMemberPathResolved"):
                    {
                        foreach (XamDataTreeNode node in this.Nodes)
                        {
                            node.SetupIsExpandedBinding();
                        }
                        break;
                    }
                case ("CheckBoxMemberPathResolved"):
                    {
                        foreach (XamDataTreeNode node in this.Nodes)
                        {
                            node.SetupCheckboxBinding();
                        }
                        break;
                    }
                case ("HeaderText"):
                case ("HeaderTemplate"):
                    {
                        if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                            this.NodeLayout.Tree.InvalidateScrollPanel(true, true);
                        break;
                    }
            }
        }

        #endregion // OnNodeLayoutPropertyChanged

        #region OnChildNodeLayoutRemoved

        /// <summary>
        /// Raised when a <see cref="NodeLayout"/> is removed from the owning NodeLayout's Columns collection.
        /// </summary>
        /// <param propertyName="layout">The <see cref="NodeLayout"/> being removed.</param>
        protected virtual void OnChildNodeLayoutRemoved(NodeLayout layout)
        {

        }
        #endregion // OnChildNodeLayoutRemoved

        #region OnChildNodeLayoutAdded

        /// <summary>
        /// Raised when a <see cref="NodeLayout"/> is added to the owning NodeLayout's Columns collection.
        /// </summary>
        /// <param propertyName="layout">The <see cref="NodeLayout"/> being added.</param>
        protected virtual void OnChildNodeLayoutAdded(NodeLayout layout)
        {

        }
        #endregion // OnChildNodeLayoutAdded

        #region OnChildNodeLayoutVisibilityChanged

        /// <summary>
        /// Raised when a child <see cref="NodeLayout"/> of the owning NodeLayout, visibility changes.
        /// </summary>
        /// <param propertyName="layout">The <see cref="NodeLayout"/> that had it's Visibility changed.</param>
        protected virtual void OnChildNodeLayoutVisibilityChanged(NodeLayout layout)
        {

        }
        #endregion // OnChildNodeLayoutVisibilityChanged

        #region UnregisterNodesManager

        /// <summary>
        /// When a NodesManager is no longer needed, this method should be called, to detach all events that are hooked up. 
        /// To avoid Memory leaks.
        /// </summary>
        /// <param name="removeNodeLayout">Whether the NodeLayout should be removed, or just its events.</param>
        /// <param name="clearChildNodesManager">Whether the ChildNodesManager should be disposed of on each node.</param>
        /// <param name="clearSelection">Whether the selected items should be unselected</param>
        protected internal virtual void UnregisterNodesManager(bool removeNodeLayout, bool clearChildNodesManager, bool clearSelection)
        {
            this.UnhookDataManager(clearChildNodesManager, clearSelection);            

            if (this._nodeLayout != null)
            {
                this._nodeLayout.PropertyChanged -= OnNodeLayoutPropertyChanged;
                this._nodeLayout.ChildNodeLayoutRemoved -= NodeLayout_ChildNodeLayoutRemoved;
                this._nodeLayout.ChildNodeLayoutAdded -= NodeLayout_ChildNodeLayoutAdded;
                this._nodeLayout.ChildNodeLayoutVisibilityChanged -= NodeLayout_ChildNodeLayoutVisibilityChanged;

                if (removeNodeLayout)
                {
                    this._nodeLayout.NodeLayoutDisposed -= NodeLayout_NodeLayoutDisposed;
                    this._nodeLayout = null;
                    this._isDisposed = true;
                }

            }
        }

        #endregion // UnregisterNodesManager

        #region RegisterChildNodesManager

        /// <summary>
        /// Adds the specified <see cref="NodesManager"/> as a visible child manager, so that it will be considered
        /// in the rendering of nodes.
        /// </summary>
        /// <param propertyName="manager"></param>
        protected internal void RegisterChildNodesManager(NodesManager manager)
        {
            if (!this.VisibleChildManagers.Contains(manager))
            {
                this._visibleChildManagers.Add(manager);
                this._visibleChildManagers.Sort();
                manager.OnRegisteredAsVisibleChildManager();
            }
        }

        #endregion // RegisterChildNodesManager

        #region UnregisterChildNodesManager

        /// <summary>
        /// Removes the specified <see cref="NodesManager"/> as a visible child manager, so that it will no longer be considered
        /// in the rendering of nodes.
        /// </summary>
        /// <param propertyName="manager"></param>
        protected internal void UnregisterChildNodesManager(NodesManager manager)
        {
            if (this.VisibleChildManagers.Contains(manager))
            {
                this._visibleChildManagers.Remove(manager);
            }
        }

        #endregion // UnregisterChildNodesManager

        #region UnregisterAllChildNodesManager

        /// <summary>
        /// Removes all visible child managers, so that they will no longer be considered
        /// in the rendering of nodes.
        /// </summary>
        protected internal void UnregisterAllChildNodesManager()
        {
            for (int i = this.VisibleChildManagers.Count - 1; i >= 0; i--)
                this.UnregisterChildNodesManager(this.VisibleChildManagers[i]);
        }

        #endregion // UnregisterAllChildNodesManager

        #region ResolveNodeForIndex

        /// <summary>
        /// Returns the <see cref="XamDataTreeNode"/> for the given index. 
        /// </summary>
        /// <param propertyName="index">The index of the node to retrieve.</param>
        /// <returns></returns>
        protected internal XamDataTreeNode ResolveNodeForIndex(int index)
        {
            if (index < 0 || index > this.FullNodeCount - 1)
                return null;

            return this.Nodes[index];
        }

        #endregion // ResolveNodeForIndex

        #region ResolveIndexForNode

        /// <summary>
        /// Returns the index for a given node.
        /// </summary>
        /// <param propertyName="node">The node whose index should be returned.</param>
        /// <returns></returns>
        protected internal int ResolveIndexForNode(XamDataTreeNode node)
        {
            int index = 0;

            index = this.Nodes.IndexOf(node);

            if (index < 0)
                index = 0;

            return index;
        }

        #endregion // ResolveIndexForNode

        #region InvalidateData

        /// <summary>
        /// Triggers all Data operations to be invalidated. 
        /// </summary>
        protected void InvalidateData()
        {
            DataManagerBase manager = this.DataManager;
            if (manager != null)
            {
                manager.Reset();
            }

        }

        #endregion // InvalidateData

        #region InitializeData

        /// <summary>
        /// Looks at the data provided for the <see cref="NodesManager"/> and generates <see cref="NodeLayout"/> objects 
        /// if AutoGenerateNodeLayouts is true.
        /// </summary>
        protected virtual void InitializeData()
        {
            if (this._dataManager != null)
            {
                NodeLayout nodeLayout = null;

                Type dataType = this._dataManager.CachedType;

                if (this.NodeLayout == null)
                    return;

                if (dataType != null)
                {
                    nodeLayout = this.NodeLayout.Tree.GlobalNodeLayouts.InternalFromKey(this.NodeLayout.Key);
                    if (nodeLayout == null || nodeLayout.TargetTypeName != dataType.Name)
                        nodeLayout = this.NodeLayout.Tree.GlobalNodeLayouts.InternalFromType(dataType);
                }

                if (nodeLayout == null)
                    nodeLayout = this.NodeLayout.Tree.GlobalNodeLayouts.InternalFromKey(this.NodeLayout.Key);

                if (nodeLayout != null && this.NodeLayout != nodeLayout)
                {
                    nodeLayout.Tree = this.NodeLayout.Tree;
                    this.NodeLayout = nodeLayout;
                }

                NodeLayoutAssignedEventArgs args = new NodeLayoutAssignedEventArgs();
                args.NodeLayout = this.NodeLayout;
                args.Level = this.Level;
                args.DataType = dataType;
                args.Key = this.NodeLayout.Key;

                this.NodeLayout.Tree.OnNodeLayoutAssigned(args);

                if (args.NodeLayout != null && args.NodeLayout != this.NodeLayout)
                {
                    args.NodeLayout.Tree = this.NodeLayout.Tree;
                    this.NodeLayout = args.NodeLayout;
                }

                if (!this.NodeLayout.Tree.GlobalNodeLayouts.Contains(this.NodeLayout))
                    this.NodeLayout.Tree.GlobalNodeLayouts.AddItemLocally(this.NodeLayout);

                if (!this.NodeLayout.IsInitialized)
                {
                    this.NodeLayout.IsInitialized = true;

                    IEnumerable<DataField> fields = this._dataManager.GetDataProperties();
                    this.NodeLayout.DataFields = fields;

                    GlobalNodeLayoutCollection layouts = this.NodeLayout.Tree.GlobalNodeLayouts;
                    foreach (DataField field in fields)
                    {
                        Type itemType = null;
                        if (field.FieldType.IsGenericType)
                        {
                            Type[] types = field.FieldType.GetGenericArguments();
                            if (types.Length > 0)
                                itemType = types[0];
                        }

                        NodeLayout layout = null;


                        if (itemType != null)
                        {
                            layout = layouts.InternalFromKey(field.Name);
                            if (layout == null || layout.TargetTypeName != itemType.Name)
                                layout = layouts.InternalFromType(itemType);
                        }

                        if (layout == null)
                            layout = layouts.InternalFromKey(field.Name);

                        if (layout != null && this.NodeLayout.NodeLayouts[layout.Key] == null)
                            this.NodeLayout.NodeLayouts.Add(layout);
                    }


                    Collection<string> dataKeys = new Collection<string>();
                    foreach (DataField field in fields)
                    {
                        dataKeys.Add(field.Name);
                    }

                    foreach (NodeLayout layout in this.NodeLayout.NodeLayouts)
                    {
                        if (layout.Key.Contains("[") && layout.Key.Contains("]"))
                        {
                            continue;
                        }

                        string[] keys = layout.Key.Split('.');
                        if (keys.Length > 0)
                        {
                            if (!dataKeys.Contains(keys[0]))
                            {
                                if (layout == null)
                                {
                                    throw new Exception(SRDataTree.GetString("LayoutCannotBeMappedToProperty"));
                                }
                                else if (!layout.IsDefinedGlobally && string.IsNullOrEmpty(layout.TargetTypeName))
                                {
                                    throw new Exception(SRDataTree.GetString("LayoutNotGloballyDefined"));

                                }
                            }
                            else
                            {
                                Type t = dataType;
                                foreach (string key in keys)
                                {
                                    PropertyInfo pi = t.GetProperty(key);
                                    if (pi == null)
                                    {
                                        if (layout == null || (!layout.IsDefinedGlobally && string.IsNullOrEmpty(layout.TargetTypeName)))
                                        {
                                            throw new Exception(SRDataTree.GetString("LE_InvalidKey"));
                                        }
                                    }
                                    
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                                    if (pi != null)
                                        t = pi.PropertyType;
                                }
                            }
                        }
                    }
                }

                if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    this.NodeLayout.Tree.InvalidateScrollPanel(true);
            }
        }
        #endregion // InitializeData

        #region CreateItem
        /// <summary>
        /// Creates a new node object 
        /// </summary>
        /// <param propertyName="data"></param>
        /// <param propertyName="dataManager"></param>
        /// <returns></returns>
        protected internal virtual XamDataTreeNode CreateItem(object data, DataManagerBase dataManager)
        {
            XamDataTreeNode node = null;
            if (dataManager != null)
            {
                if (data == null)
                {
                    throw new NullReferenceException(SRDataTree.GetString("NullDataException"));
                }

                if (!_dataManager.CachedType.IsAssignableFrom(data.GetType()))
                {
                    throw new InvalidCastException(string.Format(SRDataTree.GetString("DataTypeMismatchExceptionVerbose"), data.GetType(), _dataManager.CachedType));
                }
                node = new XamDataTreeNode(-1, this, data, false, null);
            }
            return node;
        }

        /// <summary>
        /// Creates a new node object 
        /// </summary>
        /// <returns></returns>
        protected internal virtual XamDataTreeNode CreateItem()
        {
            XamDataTreeNode node = null;
            DataManagerBase dm = this.DataManager;
            if (dm != null)
                node = this.CreateItem(dm.GenerateNewObject(), dm);
            return node;
        }

        /// <summary>
        /// Creates a new node object 
        /// </summary>
        /// <param propertyName="data"></param>
        /// <returns></returns>
        protected internal virtual XamDataTreeNode CreateItem(object data)
        {
            return this.CreateItem(data, this.DataManager);
        }
        #endregion // CreateItem

        #region AddItem
        /// <summary>
        /// Adds a node to the collection.
        /// </summary>
        /// <param propertyName="addedObject"></param>
        protected internal virtual void AddItem(XamDataTreeNode addedObject)
        {
            if (addedObject == null)
                return;

            DataManagerBase dm = this.DataManager;

            if (dm == null)
                return;
            
            if (addedObject.Manager != this)
            {
                addedObject.Manager = this;
            }

            if (addedObject.NodeLayout != this.NodeLayout)
            {
                if (addedObject.Manager.DataManager.CachedType != this.DataManager.CachedType)
                    throw new Exception(SRDataTree.GetString("LE_NodeLayout_InsertItem"));
            }

            XamDataTree tree = this.NodeLayout.Tree;

            this._supressInvalidateNodes = true;

            dm.AddRecord(addedObject.Data);

            if (!this._cachedNodes.ContainsKey(addedObject.Data))
                this._cachedNodes.Add(addedObject.Data, addedObject);

            this._supressInvalidateNodes = false;

            this.InvalidateNodes();

            int index = dm.ResolveIndexForRecord(addedObject.Data);
            if (index != -1)
            {
                // Ensures the object is added correctly to the underlying nodes collection
                addedObject = (XamDataTreeNode)this.Nodes[index];
            }

            if (this.ParentNode != null)
                this.ParentNode.ValidateParentCheckedState();

            if (this.NodeLayout.Tree != null)
                this.NodeLayout.Tree.InvalidateScrollPanel(true, false);

        }
        #endregion // AddItem

        #region InsertItem
        /// <summary>
        /// Inserts a node at a given index.
        /// </summary>
        /// <param propertyName="index"></param>
        /// <param propertyName="insertedObject"></param>
        protected internal virtual void InsertItem(int index, XamDataTreeNode insertedObject)
        {
            if (insertedObject == null)
                return;

            DataManagerBase dm = this.DataManager;

            if (dm == null)
                return;
            
            if (insertedObject.Manager != this)
            {
                insertedObject.Manager = this;
            }
            
            
            if (insertedObject.NodeLayout != this.NodeLayout)
            {
                if (insertedObject.Manager.DataManager.CachedType != this.DataManager.CachedType)
                    throw new Exception(SRDataTree.GetString("LE_NodeLayout_InsertItem"));
            }

            XamDataTree tree = this.NodeLayout.Tree;

            this._supressInvalidateNodes = true;

            dm.InsertRecord(index, insertedObject.Data);

            insertedObject.UpdateLevel();

            if (!this._cachedNodes.ContainsKey(insertedObject.Data))
                this._cachedNodes.Add(insertedObject.Data, insertedObject);

            this._supressInvalidateNodes = false;

            this.InvalidateNodes();

            // Ensures the object is added correctly to the underlying nodes collection
            insertedObject = (XamDataTreeNode)this.Nodes[index];

            if (this.ParentNode != null)
                this.ParentNode.ValidateParentCheckedState();

            if (this.NodeLayout.Tree != null)
                this.NodeLayout.Tree.InvalidateScrollPanel(true, false);

        }

        #endregion // InsertItem

        #region RemoveItem
        /// <summary>
        /// Removes a node from the underlying ItemSource
        /// </summary>
        /// <param propertyName="removedObject"></param>
        /// <returns>true if the node is removed.</returns>
        protected bool RemoveItem(XamDataTreeNode removedObject)
        {
            return this.RemoveItem(removedObject, this.DataManager);
        }

        /// <summary>
        /// Removes a node from the underlying ItemSource
        /// </summary>
        /// <param name="removedObject">The node to remove</param>
        /// <param name="manager">The Manager that should be performing the removal.</param>
        /// <returns></returns>
        protected virtual bool RemoveItem(XamDataTreeNode removedObject, DataManagerBase manager)
        {
            if (removedObject == null)
                return false;

            if (removedObject.IsHeader)
                return false;

            XamDataTree tree = this.NodeLayout.Tree;

            manager.RemoveRecord(removedObject.Data);

            this._cachedNodes.Remove(removedObject.Data);

            removedObject.Manager = null;

            if (this.ParentNode != null)
                this.ParentNode.ValidateParentCheckedState();

            return true;
        }

        #endregion // RemoveItem

        #region RemoveRange
        /// <summary>
        /// Removes the specified node from the collection.
        /// </summary>
        /// <param propertyName="itemsToRemove"></param>
        protected virtual void RemoveRange(IList<XamDataTreeNode> itemsToRemove)
        {
            if (itemsToRemove == null || itemsToRemove.Count == 0)
                return;

            DataManagerBase manager = this.DataManager;

            foreach (XamDataTreeNode node in itemsToRemove)
                this.RemoveItem(node, manager);

        }
        #endregion // RemoveRange

        #region OnRegisteredAsVisibleChildManager
        /// <summary>
        /// Invoked when a <see cref="NodesManager"/> is now visible, meaning it's Parent node is expanded. 
        /// </summary>
        protected virtual void OnRegisteredAsVisibleChildManager()
        {
            if (this.NodeLayout != null && this.NodeLayout.Tree.EnsureNodeExpansion)
                this.EnsureExpandedChildNodesAreRegistered();
        }
        #endregion // OnRegisteredAsVisibleChildManager

        #region EnsureExpandedChildNodesAreRegistered

        /// <summary>
        /// Processes all nodes in the collection and registers their child nodes manager if expanded.
        /// </summary>
        protected internal void EnsureExpandedChildNodesAreRegistered()
        {
            foreach (XamDataTreeNode node in this.Nodes)
            {
                if (node.IsExpanded)
                    this.RegisterChildNodesManager(node.ChildNodesManager);
            }            
        }

        #endregion // EnsureExpandedChildNodesAreRegistered

        

        #endregion // Protected

        #region Private

        #region SetupDataManager

        private void SetupDataManager()
        {
            this._dataManager = DataManagerBase.CreateDataManager(this.ItemsSource);
            if (this._dataManager != null)
            {
                this._dataManager.DataUpdated += new EventHandler<EventArgs>(DataManager_DataUpdated);

                this._dataManager.NewObjectGeneration += DataManager_NewObjectGeneration;

                this._dataManager.CollectionChanged += DataManager_CollectionChanged;

                this._dataManager.SuspendInvalidateDataSource = true;

                this.InitializeData();
            }
            else
            {
                // If a data source was assigned
                // but there was no data in the collection
                // but the collection implements INotifyCollectionChanged
                // then we can figure out when data is added and create a datamanager at that point. 
                INotifyCollectionChanged incc = this.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                {
                    incc.CollectionChanged -= ItemsSource_CollectionChanged;
                    incc.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
                }
            }
        }

        #endregion // SetupDataManager

        #region UnhookDatamanager

        private void UnhookDataManager(bool clearChildNodes, bool clearSelection)
        {
            this.AttachDetachPropertyChanged(false);
            this.InvalidateNodes();
            this.UnregisterAllChildNodesManager();
            if (clearChildNodes)
            {
                foreach (KeyValuePair<object, XamDataTreeNode> obj in this._cachedNodes)
                {
                    if (obj.Value.ChildNodesManager != null)
                    {                        
                        obj.Value.ChildNodesManager.NodeLayout = null;
                        obj.Value.ChildNodesManager.UnhookDataManager(true, true);
                    }
                }

                this._cachedNodes.Clear();
            }

            if (this._dataManager != null)
            {
                this._dataManager.CollectionChanged -= DataManager_CollectionChanged;
                this._dataManager.DataUpdated -= DataManager_DataUpdated;
                this._dataManager.NewObjectGeneration -= DataManager_NewObjectGeneration;
                this._dataManager = null;
            }
        }

        #endregion // UnhookDataManager

        #region AttachDetachPropertyChanged

        private void AttachDetachPropertyChanged(bool attach)
        {
            int count = this._attachedPropertyChangedTargets.Count;

            for (int i = 0; i < count; i++)
            {
                ((INotifyPropertyChanged)this._attachedPropertyChangedTargets[i].Target).PropertyChanged -= ItemSource_PropertyChanged;
            }

            this._attachedPropertyChangedTargets.Clear();

            if (attach)
            {
                if (this.ItemsSource != null)
                {
                    foreach (object o in this.ItemsSource)
                    {
                        INotifyPropertyChanged inpc = o as INotifyPropertyChanged;
                        if (inpc != null)
                        {
                            WeakReference wr = new WeakReference(inpc);
                            this._attachedPropertyChangedTargets.Add(wr);
                            inpc.PropertyChanged += ItemSource_PropertyChanged;
                        }
                    }
                }
            }        
        }

        private void AttachDetachPropertyChanged(INotifyPropertyChanged inpc, bool attach)
        {
            if (inpc != null)
            {
                int count = this._attachedPropertyChangedTargets.Count;

                for (int i = 0; i < count; i++)
                {
                    INotifyPropertyChanged target = ((INotifyPropertyChanged)this._attachedPropertyChangedTargets[i].Target);
                    if (target == inpc)
                    {
                        target.PropertyChanged -= ItemSource_PropertyChanged;
                        this._attachedPropertyChangedTargets.Remove(this._attachedPropertyChangedTargets[i]);
                        break;
                    }
                }

                if (attach)
                {
                    WeakReference wr = new WeakReference(inpc);
                    this._attachedPropertyChangedTargets.Add(wr);
                    inpc.PropertyChanged += ItemSource_PropertyChanged;
                }
            }
        }

        #endregion // AttachDetachPropertyChanged

        #endregion // Private

        #region Internal

        #region ClearNodes
        internal void ClearNodes()
        {
            this.Nodes.Clear();

            this._duplicateObjectValidator.Clear();

            this.AttachDetachPropertyChanged(false);
        }
        #endregion // ClearNodes

        #region RegisterCachedNode

        internal void RegisterCachedNode(XamDataTreeNode node)
        {
            if (!this._cachedNodes.ContainsValue(node))
            {
                if (!this._cachedNodes.ContainsKey(node.Data))
                {
                    this._cachedNodes.Add(node.Data, node);
                }
                else
                {
                    this._cachedNodes[node.Data] = node;
                }
            }

        }

        #endregion // RegisterCachedNode

        #endregion // Internal

        #endregion // Methods

        #region IProvideDataItems<XamDataTreeNode> Members

        int IProvideDataItems<XamDataTreeNode>.DataCount
        {
            get { return this.DataCount; }
        }

        XamDataTreeNode IProvideDataItems<XamDataTreeNode>.GetDataItem(int index)
        {
            return this.GetDataItem(index);
        }

        XamDataTreeNode IProvideDataItems<XamDataTreeNode>.CreateItem()
        {
            return this.CreateItem();
        }

        XamDataTreeNode IProvideDataItems<XamDataTreeNode>.CreateItem(object dataItem)
        {
            return this.CreateItem(dataItem);
        }

        void IProvideDataItems<XamDataTreeNode>.AddItem(XamDataTreeNode addedObject)
        {
            this.AddItem(addedObject);
        }

        bool IProvideDataItems<XamDataTreeNode>.RemoveItem(XamDataTreeNode removedObject)
        {
            return this.RemoveItem(removedObject);
        }

        void IProvideDataItems<XamDataTreeNode>.RemoveRange(System.Collections.Generic.IList<XamDataTreeNode> itemsToRemove)
        {
            this.RemoveRange(itemsToRemove);
        }

        void IProvideDataItems<XamDataTreeNode>.InsertItem(int index, XamDataTreeNode insertedObject)
        {
            this.InsertItem(index, insertedObject);
        }

        #endregion

        #region EventHandlers

        #region DataManager_DataUpdated
        void DataManager_DataUpdated(object sender, EventArgs e)
        {
            this.InvalidateNodes();
        }
        #endregion // DataManager_DataUpdated

        #region ItemsSource_CollectionChanged
        void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this._dataManager = DataManagerBase.CreateDataManager(this.ItemsSource);
            if (this._dataManager != null)
            {
                INotifyCollectionChanged incc = this.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                    incc.CollectionChanged -= ItemsSource_CollectionChanged;

                this._dataManager.CollectionChanged += DataManager_CollectionChanged;
                this._dataManager.DataUpdated += new EventHandler<EventArgs>(DataManager_DataUpdated);
                this._dataManager.NewObjectGeneration += DataManager_NewObjectGeneration;
                this.InitializeData();
            }
        }

        #endregion // ItemsSource_CollectionChanged

        #region DataManager_CollectionChanged

        void DataManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        #endregion // DataManager_CollectionChanged

        #region ItemSource_PropertyChanged
        void ItemSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] split = this.NodeLayout.Key.Split('.');
            if (e.PropertyName == split[split.Length - 1] || e.PropertyName == split[0] || this.NodeLayout.TargetTypeName != null)
            {
                object data = this.ParentNode.Data;
                PropertyInfo info = this.NodeLayout.ResolvePropertyInfo(data);
                if (info != null)
                {
                    this.ItemsSource = info.GetValue(data, null) as IEnumerable;
                }
                else
                {
                    object obj = DataManagerBase.ResolveValueFromPropertyPath(this.NodeLayout.Key, data);
                    this.ItemsSource = obj as IEnumerable;
                }
                if (this.ItemsSource != null)
                    this.SetupDataManager();

                this.NodeLayout.Tree.InvalidateScrollPanel(true);
            }
        }
        #endregion // ItemSource_PropertyChanged

        #region OnNodeLayoutPropertyChanged

        private void OnNodeLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!this._isDisposed)
                this.OnNodeLayoutPropertyChanged((NodeLayout)sender, e.PropertyName);
            else
            {

            }
        }

        #endregion // OnNodeLayoutPropertyChanged

        #region NodeLayout_ChildNodeLayoutAdded

        void NodeLayout_ChildNodeLayoutAdded(object sender, NodeLayoutEventArgs e)
        {
            this.OnChildNodeLayoutAdded(e.NodeLayout);
        }

        #endregion // NodeLayout_ChildNodeLayoutAdded

        #region NodeLayout_ChildNodeLayoutRemoved

        void NodeLayout_ChildNodeLayoutRemoved(object sender, NodeLayoutEventArgs e)
        {
            this.OnChildNodeLayoutRemoved(e.NodeLayout);
        }

        #endregion // NodeLayout_ChildNodeLayoutRemoved

        #region NodeLayout_ChildNodeLayoutVisibilityChanged

        void NodeLayout_ChildNodeLayoutVisibilityChanged(object sender, NodeLayoutEventArgs e)
        {
            this.OnChildNodeLayoutVisibilityChanged(e.NodeLayout);
        }

        #endregion // NodeLayout_ChildNodeLayoutVisibilityChanged

        #region NodeLayout_NodeLayoutDisposed

        void NodeLayout_NodeLayoutDisposed(object sender, EventArgs e)
        {
            this.UnregisterNodesManager(true, true, true);
        }

        #endregion // NodeLayout_NodeLayoutDisposed

        #region DataManager_NewObjectGeneration

        void DataManager_NewObjectGeneration(object sender, HandleableObjectGenerationEventArgs e)
        {
            this.NodeLayout.Tree.OnDataObjectRequested(e, this.NodeLayout, this.ParentNode);
        }

        #endregion // DataManager_NewObjectGeneration

        #endregion // EventHandlers

        #region IComparable<NodesManager> Members

        /// <summary>
        /// Compares two <see cref="NodesManager"/> objects based on their index.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(NodesManager other)
        {
            int layoutIndex = this.ParentNode.Manager.NodeLayout.NodeLayouts.IndexOf(this.ParentNode.NodeLayout);
            int otherLayoutIndex = other.ParentNode.Manager.NodeLayout.NodeLayouts.IndexOf(other.ParentNode.NodeLayout);

            if (layoutIndex == otherLayoutIndex)
            {
                int index = this.ParentNode.Index;
                int otherIndex = other.ParentNode.Index;

                return index.CompareTo(otherIndex);
            }
            else
                return layoutIndex.CompareTo(otherLayoutIndex);
        }

        #endregion
    }
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved