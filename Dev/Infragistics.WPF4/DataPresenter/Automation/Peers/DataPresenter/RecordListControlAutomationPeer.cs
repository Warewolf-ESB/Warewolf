using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Selection;
using System.Windows.Automation;
using System.Collections;
using System.Collections.Specialized;
using Infragistics.Windows.Virtualization;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="RecordListControl"/> types to UI Automation
	/// </summary>
	// AS 6/8/07 UI Automation
	//public class RecordListControlAutomationPeer : FrameworkElementAutomationPeer,
	public class RecordListControlAutomationPeer : RecyclingItemsControlAutomationPeer,
		ISelectionProvider,
		ITableProvider,
		IRecordListAutomationPeer  		// JM 08-20-09 NA 9.2 EnhancedGridView
		// AS 1/21/10 TFS26545
		// I also found an issue whereby the automation tree was incorrect when we 
		// had our container wrapper (i.e. when using the wpf container generator).
		//
		, IListAutomationPeer
	{
		#region Member Variables

		// JM 08-20-09 NA 9.2 EnhancedGridView - these member variables moved to ListAutomationPeerHelper
		//private Nullable<bool> _hasRecordGroups = null;
		//private bool _isGroupsDirty = true;
		//private List<RecordListGroup> _groups;
		//private HeaderAutomationPeer _headerPeer;

		// AS 6/5/07 Not Used
		//private Dictionary<Record, RecordAutomationPeer> _fixedRecords;
		// AS 6/8/07 UI Automation
		//private Dictionary<Record, RecordAutomationPeer> _itemPeers = new Dictionary<Record, RecordAutomationPeer>();

		// JM 08-20-09 NA 9.2 EnhancedGridView - these member variables moved to ListAutomationPeerHelper
		//private FieldLayout _fieldLayout;
		// private int _recordCount;

		// AS 6/5/07
		// AS 6/8/07 UI Automation
		//private Panel _itemsControlPanel;

		// JM 08-20-09 NA 9.2 EnhancedGridView
		private ListAutomationPeerHelper _listAutomationPeerHelper;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="RecordListControlAutomationPeer"/> class
		/// </summary>
		/// <param name="control">The <see cref="RecordListControl"/> for which the peer is being created</param>
		public RecordListControlAutomationPeer(RecordListControl control)
			: base(control)
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
			//IList<Record> records = this.GetRecordList();
			//// store the initial record count so we can send row change notifications
			//this._recordCount = records.Count;


			// JM 08-20-09 NA 9.2 EnhancedGridView
			//this.InitializeFieldLayout();
			this._listAutomationPeerHelper = new ListAutomationPeerHelper(this, this);
			this._listAutomationPeerHelper.InitializeFieldLayout();


			// AS 7/26/11 TFS80926
			DataPresenterBaseAutomationPeer.AddProxyPeerHost(this, control.DataPresenter);
		}
		#endregion //Constructor

		#region Base class overrides

		// AS 6/8/07 UI Automation
		#region CreateAutomationPeer
		/// <summary>
		/// Returns a <see cref="RecordAutomationPeer"/> that represents the specified record.
		/// </summary>
		/// <param name="item">The <see cref="Record"/> from the list.</param>
		/// <returns>A <see cref="RecordAutomationPeer"/> that represents the record</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="item"/> is null</exception>
		/// <exception cref="ArgumentException">The <paramref name="item"/> is not a <see cref="Record"/>.</exception>
		protected override RecycleableItemAutomationPeer CreateAutomationPeer(object item)
		{
			if (item is Record == false)
				throw new ArgumentException();

			// JM 08-20-09 NA 9.2 EnhancedGridView
			//return this.CreateAutomationPeer((Record)item);
			return ListAutomationPeerHelper.CreateRecordAutomationPeer((Record)item, this, this);
		}
		#endregion //CreateAutomationPeer

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>List</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			// return datagrid if we don't have groups and we will return a table interface
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//if (this.HasRecordGroups == false)
			if (this._listAutomationPeerHelper.HasRecordGroups == false)
			{
				if (this.GetPattern(PatternInterface.Table) != null)
					return AutomationControlType.DataGrid;
			}

			return AutomationControlType.List;
		}
		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="RecordListControl"/>
		/// </summary>
		/// <returns>A string that contains 'RecordListControl'</returns>
		protected override string GetClassNameCore()
		{
			return "RecordListControl";
		}
		#endregion //GetClassNameCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the <see cref="Record"/> that is associated with this <see cref="RecordListControlAutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			#region Old Code
			
#region Infragistics Source Cleanup (Region)























































































































#endregion // Infragistics Source Cleanup (Region)


			#endregion //Old Code

			// AS 4/13/11 TFS72669
			AutomationPeerHelper.RemovePendingChildrenInvalidation(this);

			// AS 7/26/11 TFS80926
			if (this.ProcessVisualTreeOnly)
			{
				return new FrameworkElementAutomationPeer(this.Owner as RecordListControl).GetChildren();
			}

			// JM 08-20-09 NA 9.2 EnhancedGridView
			//if (this.HasRecordGroups)
			if (this._listAutomationPeerHelper.HasRecordGroups)
			{
				List<AutomationPeer> baseItems = this.GetChildrenCoreBase(true);

				// JM 08-20-09 NA 9.2 EnhancedGridView
				//List<RecordListGroup> groups = this.GetRecordListGroups();
				List<RecordListGroup> groups = this._listAutomationPeerHelper.GetRecordListGroups();

				// JM 03-26-10 TFS29903 - Check for null groups.
				if (groups != null)
				{
					Converter<RecordListGroup, AutomationPeer> converter = new Converter<RecordListGroup, AutomationPeer>(delegate(RecordListGroup group)
					{
						return group.AutomationPeer;
					});

					List<AutomationPeer> groupPeers = groups.ConvertAll<AutomationPeer>(converter);

					if (null != baseItems)
						groupPeers.AddRange(baseItems);

					return groupPeers;
				}
			}

			List<AutomationPeer> children = base.GetChildrenCore();

			// lastly add the headers
			if (this.CouldSupportGridPattern())
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//AutomationPeer header = this.HeaderPeer;
				AutomationPeer header = this._listAutomationPeerHelper.HeaderPeer;

				if (null != header)
					children.Insert(0, header);
			}

			return children;
		}
		#endregion //GetChildrenCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the <see cref="Record"/> that is associated with this <see cref="RecordAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Scroll)
			{
				RecordListControl list = (RecordListControl)this.Owner;

				if (this.IsRootLevel)
				{
					if (list.ScrollInfo != null && list.ScrollInfo.ScrollOwner != null)
					{
						AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(list.ScrollInfo.ScrollOwner);

						if (null != peer && peer is IScrollProvider)
						{
							peer.EventsSource = this;
							return peer;
						}
					}
				}

				// we do not want to bother returning a scroll interface unless this
				// is the root panel
				return null;
			}

			// AS 7/26/11 TFS80926
			if (!this.ProcessVisualTreeOnly)
			{
				if (patternInterface == PatternInterface.Selection)
				{
					RecordListControl list = (RecordListControl)this.Owner;

					if (list.ItemsSource is ExpandableFieldRecordCollection)
						return null;

					return this;
				}

				if (patternInterface == PatternInterface.Grid || patternInterface == PatternInterface.Table)
				{
					if (false == this.CouldSupportGridPattern())
						return null;

					// JM 08-20-09 NA 9.2 EnhancedGridView
					//if (this.HasRecordGroups)
					if (this._listAutomationPeerHelper.HasRecordGroups)
						return null;

					return this;
				}
			}

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#endregion //Base class overrides

		#region Properties

		#region AssociatedRecordManager
		private RecordManager AssociatedRecordManager
		{
			get
			{
				RecordCollectionBase records = this.GetRecordCollection();

				// if there are fixed records...
				if (null != records &&
					records.ParentRecordManager != null &&
					records.ParentRecordManager.ParentRecord == records.ParentRecord)
				{
					return records.ParentRecordManager;
				}

				return null;
			}
		}
		#endregion //AssociatedRecordManager

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region FieldHeaders
		//private IRawElementProviderSimple[] FieldHeaders
		//{
		//    get
		//    {
		//        HeaderAutomationPeer headerPeer = this.HeaderPeer;

		//        if (headerPeer == null)
		//            return new IRawElementProviderSimple[0];

		//        return headerPeer.GetHeaderItems();
		//    }
		//} 
		#endregion //FieldHeaders

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region FieldCount
		//private int FieldCount
		//{
		//    get
		//    {
		//        HeaderAutomationPeer headerPeer = this.HeaderPeer;

		//        return null != headerPeer
		//            ? headerPeer.GetHeaderItemCount()
		//            : 0;
		//    }
		//} 
		#endregion //FieldCount

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region FieldLayout
		//private FieldLayout FieldLayout
		//{
		//    get
		//    {
		//        // AS 9/19/07 BR26515
		//        if (this._fieldLayout == null)
		//            this.InitializeFieldLayout();

		//        Debug.Assert(this.GetRecordCollectionFieldLayout() == this._fieldLayout, "Field layouts are out of sync!");

		//        return this._fieldLayout;
		//    }
		//} 
		#endregion //FieldLayout

		#region FixedRecords
		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		#endregion //FixedRecords

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region HasRecordGroups
		//private bool HasRecordGroups
		//{
		//    get
		//    {
		//        if (this._hasRecordGroups == null)
		//        {
		//            this._hasRecordGroups = false == this.IsHomogenousCollection();

		//            this._isGroupsDirty = true;
		//        }

		//        return this._hasRecordGroups == true;
		//    }
		//}
		#endregion //HasRecordGroups

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region HeaderPeer
		//private HeaderAutomationPeer HeaderPeer
		//{
		//    get
		//    {
		//        if (this._headerPeer == null)
		//        {
		//            Debug.Assert(this.CouldSupportGridPattern(), "We should not be trying to access the header peer when we cannot be a grid/table!");

		//            // AS 9/19/07 BR26515
		//            //if (this.CouldSupportGridPattern())
		//            FieldLayout fl = this.FieldLayout;

		//            if (this.CouldSupportGridPattern() && fl != null)
		//            {
		//                //this._headerPeer = new HeaderAutomationPeer(fl, this);	// JM 08-20-09 NA 9.2 EnhancedGridView
		//                this._headerPeer = new HeaderAutomationPeer(fl, this, this);
		//            }
		//        }

		//        return this._headerPeer;
		//    }
		//} 
		#endregion //HeaderPeer

		#region IsHorizontalRowLayout
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//internal bool IsHorizontalRowLayout
		private bool IsHorizontalRowLayout
		{
			get
			{
				DataPresenterBase dp = ((RecordListControl)this.Owner).DataPresenter;

				if (dp != null)
				{
					Infragistics.Windows.DataPresenter.ViewBase currentView = dp.CurrentViewInternal;

					return currentView.HasLogicalOrientation &&
							currentView.LogicalOrientation == Orientation.Horizontal;
				}

				return false;
			}
		}
		#endregion //IsHorizontalRowLayout

		// JM 08-20-09 NA 9.2 - Added
		#region IsRootLevel
		private bool IsRootLevel
		{
			get
			{
				RecordListControl list = (RecordListControl)this.Owner;
				return	list				!= null &&
						list.DataPresenter	!= null &&
						list.DataPresenter.RootRecordListControl == list;
			}
		}
		#endregion //IsRootLevel

		// AS 6/5/07
		#region ItemsControlPanel
		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		#endregion //ItemsControlPanel

		// AS 7/26/11 TFS80926
		#region ProcessVisualTreeOnly
		private bool ProcessVisualTreeOnly
		{
			get { return DataPresenterBaseAutomationPeer.ProcessVisualTreeOnly; }
		}
		#endregion //ProcessVisualTreeOnly

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region RecordCount
		//private int RecordCount
		//{
		//    get
		//    {
		//        int count = this.GetRecordList().Count;

		//        return count;
		//    }
		//} 
		#endregion //RecordCount

		#endregion //Properties

		#region Methods

		// AS 6/5/07
		#region ContainerFromItem
		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

		#endregion //ContainerFromItem

		#region CouldSupportGridPattern
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//internal bool CouldSupportGridPattern()
		private bool CouldSupportGridPattern()
		{
			RecordListControl list = (RecordListControl)this.Owner;

			// JM 08-20-09 NA 9.2 EnhancedGridView - tightening up this logic: If the ItemsSource
			// is DataRecordCollection or a ViewableRecordCollection we should only return true if the
			// the collections contain a list of homogenous DataRecords.
			// we'll just return null to indicate its not supported
			//if (list.ItemsSource is DataRecordCollection == false &&
			//    list.ItemsSource is ViewableRecordCollection == false)
			//{
			//    return false;
			//}
			RecordCollectionBase rcb = null;

			if (list.ItemsSource is DataRecordCollection)
				rcb = ((DataRecordCollection)list.ItemsSource).ViewableRecords.RecordCollection;
			else
			if (list.ItemsSource is ViewableRecordCollection)
				rcb = ((ViewableRecordCollection)list.ItemsSource).RecordCollection;

			if (rcb != null)
			{
				// AS 8/28/09 TFS21509
				// Moved logic for evaluating the type into a helper method.
				//
				if (ListAutomationPeerHelper.CouldSupportGridPattern(rcb.RecordsType))
					return this._listAutomationPeerHelper.IsHomogenousCollection();
			}

			return false;
		}
		#endregion //CouldSupportGridPattern

		// JM 08-20-09 NA 9.2 EnhancedGridView - replaced by ListAutomationPeerHelper.CreateRecordAutomationPeer
		#region CreateAutomationPeer
		//internal RecordAutomationPeer CreateAutomationPeer(Record record)
		//{
		//    if (null == record)
		//        throw new ArgumentNullException("record");

		//    return new RecordAutomationPeer(record, this);
		//} 
		#endregion //CreateAutomationPeer

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region CreateGroups
		//private List<RecordListGroup> CreateGroups()
		//{
		//    // get the previous groups
		//    List<RecordListGroup> oldGroups = this._groups;

		//    // create new groups...
		//    IList<Record> records = this.GetRecordList();
		//    List<RecordListGroup> groups = new List<RecordListGroup>();
		//    RecordListGroup group = null;

		//    foreach (Record record in records)
		//    {
		//        if (group == null || group.FieldLayout != record.FieldLayout)
		//        {
		//            // JM 08-20-09 NA 9.2 EnhancedGridView
		//            //group = new RecordListGroup(this, record.FieldLayout);
		//            group = new RecordListGroup(this, this, record.FieldLayout);
		//            groups.Add(group);
		//        }

		//        group.AddRecord(record);
		//    }

		//    foreach (RecordListGroup newGroup in groups)
		//        newGroup.InitializeNotifyRecordCount();

		//    // reuse the automation peers where possible
		//    if (oldGroups != null)
		//    {
		//        foreach(RecordListGroup newGroup in groups)
		//        {
		//            for (int i = 0; i < oldGroups.Count; i++)
		//            {
		//                RecordListGroup oldGroup = oldGroups[i];

		//                // note the new group could have more records as long
		//                // as it has all the old ones. this way we can handle 
		//                // when new records are added and we can keep at least
		//                // one group when there were two and they became joined
		//                // because the records between were removed
		//                if (oldGroup != null && oldGroup.IsSameGroup(newGroup))
		//                {
		//                    oldGroups[i] = null;
		//                    newGroup.InitializeFrom(oldGroup);
		//                }
		//            }
		//        }
		//    }

		//    return groups;
		//} 
		#endregion //CreateGroups

		// AS 6/5/07
		#region FindPanel
		
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		#endregion //FindPanel

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetContainingGrid
		//internal AutomationPeer GetContainingGrid(Cell cell)
		//{
		//    if (this.HasRecordGroups)
		//    {
		//        RecordListGroup group = this.GetRecordListGroup(cell.Record);

		//        return null != group
		//            ? group.AutomationPeer
		//            : null;
		//    }

		//    return this;
		//}
		#endregion //GetContainingGrid

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetDiffList
		//private static List<T> GetDiffList<T>(SelectedItemCollectionBase oldItems, SelectedItemCollectionBase newItems)
		//    where T : class, ISelectableItem
		//{
		//    // build a diff list of the items that have changed their state
		//    List<T> list = new List<T>();

		//    for (int i = 0, count = newItems.Count; i < count; i++)
		//        list.Add(newItems.GetItem(i) as T);

		//    for (int i = 0, count = oldItems.Count; i < count; i++)
		//    {
		//        T item = oldItems.GetItem(i) as T;

		//        // if the record was selected before, remove it
		//        if (item.IsSelected)
		//            list.Remove(item);
		//        else
		//            list.Add(item);
		//    }

		//    return list;
		//} 
		#endregion //GetDiffList

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetHeaderPeer
		//private HeaderAutomationPeer GetHeaderPeer(Record record)
		//{
		//    if (this.HasRecordGroups)
		//    {
		//        RecordListGroup group = this.GetRecordListGroup(record);

		//        return null != group
		//            ? group.AutomationPeer.HeaderPeer
		//            : null;
		//    }

		//    return this.HeaderPeer;
		//} 
		#endregion //GetHeaderPeer

		#region GetRecordList
		private IList<Record> GetRecordList()
		{
			RecordListControl list = (RecordListControl)this.Owner;

			// AS 1/22/08
			//Debug.Assert(list != null && list.ItemsSource is IList<Record>, "We're expecting an IList source!");
			Debug.Assert(list != null && (list.ItemsSource == null || list.ItemsSource is IList<Record>), "We're expecting an IList source!");

			return list.ItemsSource as IList<Record>;
		}
		#endregion //GetRecordList

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetRecordCollectionFieldLayout
		//private FieldLayout GetRecordCollectionFieldLayout()
		//{
		//    RecordCollectionBase records = this.GetRecordCollection();

		//    return records.FieldLayout;
		//} 
		#endregion //GetRecordCollectionFieldLayout

		#region GetRecordCollection
		private RecordCollectionBase GetRecordCollection()
		{
			RecordListControl list = (RecordListControl)this.Owner;
			RecordCollectionBase parentCollection = list.ItemsSource as RecordCollectionBase;

			if (parentCollection == null)
			{
				// this assert is no longer valid since we don't use the itemssource
				//Debug.Assert(list.ItemsSource is ViewableRecordCollection, "We need to get to the parent collection.");

                // JJD 8/7/09 - NA 2009 Vol 2 - Enhanced grid view
                // Use ViewableRecords property instead
                //ViewableRecordCollection visibleRecords = list.ItemsSource as ViewableRecordCollection;
                ViewableRecordCollection visibleRecords = list.ViewableRecords;

				if (null != visibleRecords)
				{
					//parentCollection = visibleRecords.RecordManager.Sorted;
					parentCollection = visibleRecords.RecordCollection;
				}
			}

			return parentCollection;
		}
		#endregion //GetRecordCollection

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetRecordListGroup
		//internal RecordListGroup GetRecordListGroup(Record record)
		//{
		//    List<RecordListGroup> groups = this.GetRecordListGroups();

		//    if (groups != null)
		//    {
		//        int recordIndex = 0;
		//        int start = 0;
		//        int end = groups.Count - 1;

		//        // do a binary search to find the group that contains the record
		//        while (start < end)
		//        {
		//            int index = start + ((end - start) / 2);

		//            Debug.Assert(index >= 0 && index < groups.Count);

		//            RecordListGroup group = groups[index];
		//            int comparison = group.CompareTo(recordIndex);

		//            if (comparison == 0)
		//                return group;
		//            else if (comparison < 0)
		//                start = index + 1;
		//            else //if (comparison > 0)
		//                end = index - 1;
		//        }

		//        if (groups.Count == 1)
		//            return groups[0];
		//    }

		//    return null;
		//} 
		#endregion //GetRecordListGroup

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetRecordListGroups
		//private List<RecordListGroup> GetRecordListGroups()
		//{
		//    // only recreate the collection if we didn't know...
		//    if (this._isGroupsDirty)
		//    {
		//        if (this.HasRecordGroups)
		//            this._groups = this.CreateGroups();
		//        else
		//            this._groups = null;
		//    }

		//    return this._groups;
		//} 
		#endregion //GetRecordListGroups

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetSelectedItems
		//private List<ISelectableItem> GetSelectedItems()
		//{
		//    RecordListControl list = (RecordListControl)this.Owner;
		//    DataPresenterBase dp = list.DataPresenter;

		//    // we only need to worry about selected records/rows and cells
		//    if (null != dp && 
		//        (dp.SelectedItems.HasRecords || dp.SelectedItems.HasCells))
		//    {
		//        IList<Record> records = this.GetRecordList();

		//        // don't bother if the list doesn't have any rows
		//        if (records != null && records.Count > 0)
		//        {
		//            // selection of the rows/cells are mutually exclusive so just get one
		//            ISelectableItem[] allItems = dp.SelectedItems.HasRecords
		//                ? (ISelectableItem[])dp.SelectedItems.Records.ToArray()
		//                : (ISelectableItem[])dp.SelectedItems.Cells.ToArray();

		//            // now we have to sort the items by parent collection and split
		//            // them by parent collection
		//            Utilities.SortMerge(allItems, SelectedItemComparer.Default);

		//            RecordCollectionBase parentCollection = this.GetRecordCollection();

		//            // now split by parent collection
		//            Dictionary<RecordCollectionBase, List<ISelectableItem>> splitRecords = SplitItems(allItems, parentCollection);

		//            List<ISelectableItem> items;
		//            if (splitRecords.TryGetValue(parentCollection, out items))
		//                return items;
		//        }
		//    }

		//    return null;
		//}
		#endregion //GetSelectedItems

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region GetTableRowIndex
		//internal int GetTableRowIndex(Cell cell)
		//{
		//    if (this.HasRecordGroups)
		//    {
		//        RecordListGroup group = this.GetRecordListGroup(cell.Record);

		//        Debug.Assert(null != group, "Unable to locate the containing group!");

		//        return null != group
		//            ? group.IndexOf(cell.Record)
		//            : 0;
		//    }

		//    Record record = cell.Record;

		//    if (null != record)
		//    {
		//        int index = this.GetRecordList().IndexOf(record);

		//        return index;
		//    }

		//    return 0;
		//} 
		#endregion //GetTableRowIndex

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region InitializeFieldLayout
		//private void InitializeFieldLayout()
		//{
		//    FieldLayout oldFieldLayout = this._fieldLayout;
		//    FieldLayout newFieldLayout = null;

		//    if (this.CouldSupportGridPattern())
		//        newFieldLayout = this.GetRecordCollectionFieldLayout();

		//    // if the layout has changed...
		//    if (oldFieldLayout != newFieldLayout)
		//    {
		//        if (null != this._headerPeer)
		//        {
		//            this._headerPeer.Deactivate();
		//            this._headerPeer = null;
		//        }

		//        // store the new field layout
		//        this._fieldLayout = newFieldLayout;
		//    }
		//} 
		#endregion //InitializeFieldLayout

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region IsHomogenousCollection
		//private bool IsHomogenousCollection()
		//{
		//    IList<Record> records = this.GetRecordList();

		//    // AS 1/22/08
		//    //Debug.Assert(records != null, "We're expecting the list to be a record collection!");
		//    if (records == null)
		//        return true;

		//    FieldLayout listFieldLayout = this.GetRecordCollectionFieldLayout();

		//    // AS 9/19/07 BR26515
		//    // This can happen if the record collection is not yet initialized.
		//    //
		//    //Debug.Assert(listFieldLayout != null, "We're expecting the list to have a field layout!");

		//    foreach (Record record in records)
		//    {
		//        if (record.FieldLayout != listFieldLayout)
		//            return false;
		//    }

		//    return true;
		//} 
		#endregion //IsHomogenousCollection

		// AS 6/5/07
		#region ItemFromContainer
		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

		#endregion //ItemFromContainer

		#region OnItemsChanged
		// AS 6/8/07 UI Automation
		//internal void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		//{
		/// <summary>
		/// Invoked when the contents of the items collection of the associated <see cref="RecyclingItemsControl"/> changed.
		/// </summary>
		/// <param name="e">Event arguments indicating the change that occurred.</param>
		internal protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			// AS 7/26/11 TFS80926
			if (this.ProcessVisualTreeOnly)
				return;

			// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
			this._listAutomationPeerHelper.ProcessListChange(e);
			//if (false == this.CouldSupportGridPattern())
			//    return;

			//// JM 08-20-09 NA 9.2 EnhancedGridView
			////bool hadRecordGroups = this.HasRecordGroups;
			//bool hadRecordGroups = this._listAutomationPeerHelper.HasRecordGroups;

			//#region Remove records

			//// if we have record groups then we need to remove any removed/replaced records
			//// so we can try to reuse the group is possible
			//if (hadRecordGroups)
			//{
			//    // JM 08-20-09 NA 9.2 EnhancedGridView
			//    //List<RecordListGroup> groups = this.GetRecordListGroups();
			//    List<RecordListGroup> groups = this._listAutomationPeerHelper.GetRecordListGroups();

			//    switch (e.Action)
			//    {
			//        case NotifyCollectionChangedAction.Remove:
			//        case NotifyCollectionChangedAction.Replace:
			//            // find the groups that contain the records and remove them
			//            // 
			//            foreach (object item in e.OldItems)
			//            {
			//                Record record = item as Record;

			//                for (int i = 0, count = groups.Count; i < count; i++)
			//                {
			//                    int index = groups[i].IndexOf(record);

			//                    if (index >= 0)
			//                    {
			//                        groups[i].RemoveAt(index);
			//                        break;
			//                    }
			//                }
			//            }
			//            break;
			//    }
			//} 
			//#endregion //Remove records

			//// now since we're unsure if we need groups, dirty the state
			//// JM 08-20-09 NA 9.2 EnhancedGridView
			////this._hasRecordGroups = null;
			//this._listAutomationPeerHelper.SetHasRecordGroups(null);

			//// check if we have record groups
			//// JM 08-20-09 NA 9.2 EnhancedGridView
			////if (this.HasRecordGroups)
			//if (this._listAutomationPeerHelper.HasRecordGroups)
			//{
			//    // if so, get them and send notifications for any group that has 
			//    // changed. new groups or removed groups will be handled by the 
			//    // invalidate peer below
			//    // JM 08-20-09 NA 9.2 EnhancedGridView
			//    //List<RecordListGroup> groups = this.GetRecordListGroups();
			//    List<RecordListGroup> groups = this._listAutomationPeerHelper.GetRecordListGroups();

			//    foreach (RecordListGroup group in groups)
			//    {
			//        group.RaiseRecordCountChange();
			//    }
			//}
			//else if (false == hadRecordGroups)
			//{
			//    // if we didn't have groups before and still don't then we can
			//    // send a record count change. if we had groups then they didn't
			//    // know we had records but we'll handle that below by sending
			//    // a change to whether we have the grid pattern
			//    IList<Record> records = this.GetRecordList();

			//    if (this._recordCount != records.Count)
			//    {
			//        AutomationProperty property = this.IsHorizontalRowLayout
			//            ? GridPatternIdentifiers.ColumnCountProperty
			//            : GridPatternIdentifiers.RowCountProperty;

			//        this.RaisePropertyChangedEvent(property, this._recordCount, records.Count);
			//        this._recordCount = records.Count;
			//    }
			//}

			//// if we went from having record groups to not having them or vice/versa
			//// then the grid pattern is either available or not available
			//// JM 08-20-09 NA 9.2 EnhancedGridView
			////if (hadRecordGroups != this.HasRecordGroups)
			//if (hadRecordGroups != this._listAutomationPeerHelper.HasRecordGroups)
			//{
			//    // JM 08-20-09 NA 9.2 EnhancedGridView
			//    //this.RaisePropertyChangedEvent(AutomationElementIdentifiers.IsGridPatternAvailableProperty, false == hadRecordGroups, false == this.HasRecordGroups);
			//    this.RaisePropertyChangedEvent(AutomationElementIdentifiers.IsGridPatternAvailableProperty, false == hadRecordGroups, false == this._listAutomationPeerHelper.HasRecordGroups);
			//}
			
			//// we need to indicate that items were added/removed so reset the child cache
			//this.InvalidatePeer();
		}
		#endregion //OnItemsChanged

		// AS 9/19/07 BR26515
		#region OnItemsSourceChanged
		/// <summary>
		/// Invoked when the ItemsSource of the associated <see cref="RecyclingItemsControl"/> has changed.
		/// </summary>
		/// <param name="oldValue">Old item source</param>
		/// <param name="newValue">New item source</param>
		internal protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			base.OnItemsSourceChanged(oldValue, newValue);

			// now since we're unsure if we need groups, dirty the state
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//this._hasRecordGroups = null;
			this._listAutomationPeerHelper.SetHasRecordGroups(null);

			// reinitialize the field layout
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//this.InitializeFieldLayout();
			this._listAutomationPeerHelper.InitializeFieldLayout();

			// we need to indicate that items were added/removed so reset the child cache
			this.InvalidatePeer();

			// AS 4/13/11 TFS72669
			AutomationPeerHelper.InvalidateChildren(this);

			// raise a structure change notification
			this.RaiseAutomationEvent(AutomationEvents.StructureChanged);
		} 
		#endregion //OnItemsSourceChanged

		// AS 6/5/07
		#region OnItemsPanelChanged
		
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnItemsPanelChanged

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region RaiseAutomationSelectionEvents
//#if DEBUG
//        /// <summary>
//        /// Helper method for raising automation selection change notifications for records/cells.
//        /// </summary>
//        /// <param name="oldSelection">Items that were selected before the selection change</param>
//        /// <param name="newSelection">Items that are now selected</param>
//#endif
//        internal static void RaiseAutomationSelectionEvents(DataPresenterBase.SelectedItemHolder oldSelection, DataPresenterBase.SelectedItemHolder newSelection)
//        {
//            // when a single cell is selected, we'll just send a OnElementSelected on behave
//            // of the record list that contains the cell
//            if (newSelection.HasCells && 
//                newSelection.Cells.Count == 1 &&
//                (oldSelection.HasCells == false || oldSelection.Cells.Count > 1 || oldSelection.Cells[0] != newSelection.Cells[0]))
//            {
//                if (false == newSelection.Cells[0].RaiseAutomationSelectedChange(AutomationEvents.SelectionItemPatternOnElementSelected))
//                {
//                    // try to get the listcontrol for the parent collection and raise an invalidated notification
//                    RaiseListSelectionInvalidated(newSelection.Cells[0]);
//                }

//                return;
//            }

//            // when a single row is selected, we'll just send a OnElementSelected on behalf
//            // of the record list that contains the record
//            if (newSelection.HasRecords && 
//                newSelection.Records.Count == 1 &&
//                (oldSelection.HasRecords == false || oldSelection.Records.Count > 1 || oldSelection.Records[0] != newSelection.Records[0]))
//            {
//                if (false == newSelection.Records[0].RaiseAutomationSelectedChange(AutomationEvents.SelectionItemPatternOnElementSelected))
//                {
//                    // try to get the listcontrol for the parent collection and raise an invalidated notification
//                    RaiseListSelectionInvalidated(newSelection.Records[0]);
//                }

//                return;
//            }

//            // for all other selection, we need to build a list so we know who to raise
//            // the notification for and what type of notification to send.

//            // start by getting a collection of all the records whose
//            // selection state has been modified
//            Record[] changedRecords;
//            Cell[] changedCells;

//            if (oldSelection.HasRecords == true && newSelection.HasRecords == false)
//            {
//                // everything was unselected...
//                changedRecords = oldSelection.Records.ToArray();

//                // if there are cells selected now...
//                changedCells = newSelection.HasCells
//                    ? newSelection.Cells.ToArray()
//                    : null;
//            }
//            else if (oldSelection.HasRecords == false && newSelection.HasRecords == true)
//            {
//                // nothing was selected before but now there is
//                changedRecords = newSelection.Records.ToArray();

//                // if there are cells selected now...
//                changedCells = oldSelection.HasCells
//                    ? oldSelection.Cells.ToArray()
//                    : null;
//            }
//            else if (oldSelection.HasCells == true && newSelection.HasCells == false)
//            {
//                // everything was unselected...
//                changedCells = oldSelection.Cells.ToArray();

//                // if there are records selected now...
//                changedRecords = newSelection.HasRecords
//                    ? newSelection.Records.ToArray()
//                    : null;
//            }
//            else if (oldSelection.HasCells == false && newSelection.HasCells == true)
//            {
//                // nothing was selected before but now there is
//                changedCells = newSelection.Cells.ToArray();

//                // can't be any records or we would have gotten into the first if block
//                changedRecords = null;
//            }
//            else
//            {
//                // get the diff lists - i.e. the list of cells/records that were selected/unselected
//                changedRecords = GetDiffList<Record>(oldSelection.Records, newSelection.Records).ToArray();
//                changedCells = GetDiffList<Cell>(oldSelection.Cells, newSelection.Cells).ToArray();
//            }

//            // since we could have cells being selected and rows being unselected or 
//            // vice versa, we need to build a combined list of the items whose selection
//            // state has changed...
//            ISelectableItem[] items;

//            if (changedRecords == null || changedRecords.Length == 0)
//                items = changedCells;
//            else if (changedCells == null || changedCells.Length == 0)
//                items = changedRecords;
//            else
//            {
//                items = new ISelectableItem[changedCells.Length + changedRecords.Length];
//                changedRecords.CopyTo(items, 0);
//                changedCells.CopyTo(items, changedRecords.Length);
//            }

//            // now we have to sort the items by parent collection and split
//            // them by parent collection

//            // sort by the parent collection
//            Utilities.SortMerge(items, SelectedItemComparer.Default);

//            // now split by parent collection
//            Dictionary<RecordCollectionBase, List<ISelectableItem>> splitItems = SplitItems(items, null);

//            // now we can send the notifications for the records/cells
//            foreach (KeyValuePair<RecordCollectionBase, List<ISelectableItem>> entry in splitItems)
//            {
//                // we'll put a selection threshold of 20 items. if you've selected/unselected
//                // more then that within a single record collection, we'll just invalidate the
//                // entire collection
//                if (entry.Value.Count < AutomationInteropProvider.InvalidateLimit)
//                {
//                    bool invalidateList = false;

//                    foreach (ISelectableItem item in entry.Value)
//                    {
//                        if (item is Record)
//                        {
//                            Record record = (Record)item;

//                            if (false == record.RaiseAutomationSelectedChange(record.IsSelected
//                                    ? AutomationEvents.SelectionItemPatternOnElementAddedToSelection
//                                    : AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
//                            {
//                                invalidateList = true;
//                                break;
//                            }
//                        }
//                        else if (item is Cell)
//                        {
//                            Cell cell = (Cell)item;

//                            if (false == cell.RaiseAutomationSelectedChange(cell.IsSelected
//                                    ? AutomationEvents.SelectionItemPatternOnElementAddedToSelection
//                                    : AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
//                            {
//                                invalidateList = true;
//                                break;
//                            }
//                        }
//                    }

//                    if (invalidateList == false)
//                        continue;
//                }

//                // get the peer for the list control and send an invalidated if we couldn't send a notification
//                // for each item or if there were too many items whose selected state has changed
//                RecordListControl rl = entry.Key.LastRecordList;

//                if (rl == null && entry.Key.ParentRecordManager != null)
//                {
//                    ViewableRecordCollection visibleRecords = entry.Key.ParentRecordManager.ViewableRecords as ViewableRecordCollection;

//                    if (visibleRecords != null)
//                        rl = visibleRecords.LastRecordList;
//                }

//                if (null != rl)
//                {
//                    AutomationPeer peer = UIElementAutomationPeer.FromElement(rl) as AutomationPeer;

//                    if (null != peer)
//                        peer.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
//                }
//            }
//        }
		#endregion //RaiseAutomationSelectionEvents

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region RaiseListSelectionInvalidated
		//private static void RaiseListSelectionInvalidated(Record record)
		//{
		//    if (record != null)
		//        RaiseListSelectionInvalidated(record.ParentRecordList);
		//}

		//private static void RaiseListSelectionInvalidated(Cell cell)
		//{
		//    if (null != cell.Record)
		//        RaiseListSelectionInvalidated(cell.Record);
		//}

		//private static void RaiseListSelectionInvalidated(RecordListControl listControl)
		//{
		//    if (null != listControl)
		//    {
		//        AutomationPeer peer = UIElementAutomationPeer.FromElement(listControl) as AutomationPeer;

		//        if (null != peer)
		//            peer.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
		//    }
		//}
		#endregion //RaiseListSelectionInvalidated

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region SplitItems
//#if DEBUG
//        /// <summary>
//        /// Helper method for splitting records/cells based on the parent collection to which they belong. It is assumed that the collection is sorted.
//        /// </summary>
//        /// <param name="items">Collection of items sorted based on the parent collection to which they belong</param>
//        /// <param name="targetParentCollection">The parent collection whose selected items are to be returned or null to split all the items based on their parent collection</param>
//        /// <returns></returns>
//#endif
//        private static Dictionary<RecordCollectionBase, List<ISelectableItem>> SplitItems(ISelectableItem[] items, RecordCollectionBase targetParentCollection)
//        {
//            Dictionary<RecordCollectionBase, List<ISelectableItem>> table = new Dictionary<RecordCollectionBase, List<ISelectableItem>>();
//            RecordCollectionBase lastParentCollection = null;
//            List<ISelectableItem> list = null;

//            for (int i = 0; i < items.Length; i++)
//            {
//                ISelectableItem item = items[i];

//                RecordCollectionBase records = item is Record
//                    ? ((Record)item).ParentCollection
//                    : ((Cell)item).Record.ParentCollection;

//                if (records == null)
//                    continue;

//                // skip the record when looking for ones from a specific parent
//                if (targetParentCollection != null && targetParentCollection != records)
//                    continue;

//                // if its a different parent collection then start a new list...
//                if (records != lastParentCollection)
//                {
//                    Debug.Assert(table.ContainsKey(records) == false, "The list was supposed to be sorted based on the parent collection and we have already added this parent collection into the table!");

//                    lastParentCollection = records;
//                    list = new List<ISelectableItem>();
//                    table.Add(lastParentCollection, list);
//                }

//                // add it to the list we're dealing with
//                list.Add(item);
//            }

//            return table;
//        }
		#endregion //SplitItems

		#endregion //Methods

		#region ISelectionProvider

		bool ISelectionProvider.CanSelectMultiple
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
				return this._listAutomationPeerHelper.ISelectionProvider_CanSelectMultiple;
				//List<ISelectableItem> selectedItems = this.GetSelectedItems();

				//// base the ability to select multiple on the first selected item
				//if (selectedItems != null && selectedItems.Count > 0)
				//{
				//    DataPresenterBase dp = ((RecordListControl)this.Owner).DataPresenter;
				//    SelectionStrategyBase selectionStrategy = ((ISelectionHost)dp).GetSelectionStrategyForItem(selectedItems[0]);

				//    if (null != selectionStrategy)
				//        return selectionStrategy.IsMultiSelect;
				//}

				//// otherwise assume multi selection is allowed
				//return true;
			}
		}

		IRawElementProviderSimple[] ISelectionProvider.GetSelection()
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
			return this._listAutomationPeerHelper.ISelectionProvider_GetSelection();
			//List<ISelectableItem> selectedItems = this.GetSelectedItems();

			//if (selectedItems != null && selectedItems.Count > 0)
			//{
			//    List<IRawElementProviderSimple> selectedItemPeers = new List<IRawElementProviderSimple>(selectedItems.Count);

			//    foreach (ISelectableItem item in selectedItems)
			//    {
			//        AutomationPeer itemPeer = null;

			//        // get the automation peer for the record or cell
			//        if (item is Record)
			//            itemPeer = ((Record)item).AutomationPeer;
			//        else if (item is Cell)
			//            itemPeer = ((Cell)item).AutomationPeer;

			//        if (itemPeer != null)
			//            selectedItemPeers.Add(this.ProviderFromPeer(itemPeer));
			//    }

			//    return selectedItemPeers.ToArray();
			//}

			//return null;
		}

		bool ISelectionProvider.IsSelectionRequired
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
			//get { return false; }
			get { return this._listAutomationPeerHelper.ISelectionProvider_IsSelectionRequired; }
		}

		#endregion //ISelectionProvider

		#region ITableProvider

		IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
			return this._listAutomationPeerHelper.ITableProvider_GetColumnHeaders();
			//return this.IsHorizontalRowLayout
			//    ? new IRawElementProviderSimple[0]
			//    : this._listAutomationPeerHelper.FieldHeaders; // this.FieldHeaders;  JM 08-20-09 NA 9.2 EnhancedGridView 
		}

		IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
			return this._listAutomationPeerHelper.ITableProvider_GetRowHeaders();
			//return this.IsHorizontalRowLayout
			//    ? this._listAutomationPeerHelper.FieldHeaders   /* this.FieldHeaders  JM 08-20-09 NA 9.2 EnhancedGridView */
			//    : new IRawElementProviderSimple[0];
		}

		System.Windows.Automation.RowOrColumnMajor ITableProvider.RowOrColumnMajor
		{
			get 
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
				return this._listAutomationPeerHelper.ITableProvider_RowOrColumnMajor;
				//return this.IsHorizontalRowLayout
				//    ? RowOrColumnMajor.ColumnMajor
				//    : RowOrColumnMajor.RowMajor;
			}
		}

		#endregion //ITableProvider

		#region IGridProvider

		int IGridProvider.ColumnCount
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
				return this._listAutomationPeerHelper.IGridProvider_ColumnCount;
				//return this.IsHorizontalRowLayout
				//    ? this.RecordCount
				//    : this._listAutomationPeerHelper.FieldCount; // this.FieldCount;  JM 08-20-09 NA 9.2 EnhancedGridView
			}
		}

		IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
			return this._listAutomationPeerHelper.IGridProvider_GetItem(row, column);
			//if (row > ((IGridProvider)this).RowCount || row < 0)
			//    throw new ArgumentOutOfRangeException("row");

			//if (column > ((IGridProvider)this).ColumnCount || column < 0)
			//    throw new ArgumentOutOfRangeException("column");

			//if (this.IsHorizontalRowLayout)
			//{
			//    int temp = column;
			//    column = row;
			//    row = temp;
			//}

			//IList<Record> list = this.GetRecordList();
			//DataRecord record = list[row] as DataRecord;

			//// we only care about data records
			//if (null == record)
			//{
			//    return null;
			//}

			//RecordAutomationPeer recordPeer = record.AutomationPeer;

			//if (recordPeer == null)
			//{
			//    // get the children to force the peers to be created
			//    List<AutomationPeer> recordPeers = this.GetChildren();

			//    // now get the peer for the record
			//    recordPeer = record.AutomationPeer;
			//}

			//if (recordPeer != null)
			//{
			//    // JM 08-20-09 NA 9.2 EnhancedGridView
			//    //Field field = this.HeaderPeer.GetFieldAtIndex(column);
			//    Field field = this._listAutomationPeerHelper.HeaderPeer.GetFieldAtIndex(column);

			//    CellAutomationPeer cellPeer = recordPeer.GetCellPeer(record.Cells[field]);
			//    return cellPeer != null
			//        ? this.ProviderFromPeer(cellPeer)
			//        : null;
			//}

			//return null;
		}

		int IGridProvider.RowCount
		{
			get
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView - delegate to the ListAutomationPeerHelper
				return this._listAutomationPeerHelper.IGridProvider_RowCount;
				//return this.IsHorizontalRowLayout
				//    ? this._listAutomationPeerHelper.FieldCount   /* this.FieldCount   JM 08-20-09 NA 9.2 EnhancedGridView*/
				//    : this.RecordCount;
			}
		}

		#endregion //IGridProvider

		// JM 08-20-09 NA 9.2 EnhancedGridView - Moved to ListAutomationPeerHelper
		#region SelectedItemComparer
		//private class SelectedItemComparer : IComparer
		//{
		//    #region Member Variables

		//    private static readonly SelectedItemComparer _default;

		//    #endregion //Member Variables

		//    #region Constructor
		//    static SelectedItemComparer()
		//    {
		//        _default = new SelectedItemComparer();
		//    }

		//    private SelectedItemComparer()
		//    {
		//    }
		//    #endregion //Constructor

		//    #region Properties
		//    internal static SelectedItemComparer Default
		//    {
		//        get { return SelectedItemComparer._default; }
		//    }
		//    #endregion //Properties

		//    #region IComparer

		//    int IComparer.Compare(object x, object y)
		//    {
		//        Record recordX = null;
		//        Record recordY = null;

		//        if (x is Cell)
		//            recordX = ((Cell)x).Record;
		//        else
		//            recordX = x as Record;

		//        if (y is Cell)
		//            recordY = ((Cell)y).Record;
		//        else
		//            recordY = y as Record;

		//        // we don't really care about the order other than to have all the same
		//        // parent collections together

		//        // if they're from different collections...
		//        if (recordX.ParentCollection != recordY.ParentCollection)
		//            return recordX.ParentCollection.GetHashCode().CompareTo(recordY.ParentCollection.GetHashCode());

		//        return 0;
		//    }

		//    #endregion //IComparer<Record>
		//} 
		#endregion //SelectedItemComparer

		#region IRecordListAutomationPeer Members

		bool IRecordListAutomationPeer.IsHorizontalRowLayout
		{
			get { return this.IsHorizontalRowLayout; }
		}

		bool IRecordListAutomationPeer.CouldSupportGridPattern()
		{
			return this.CouldSupportGridPattern();
		}

		RecordAutomationPeer IRecordListAutomationPeer.CreateAutomationPeer(Record record)
		{
			return ListAutomationPeerHelper.CreateRecordAutomationPeer(record, this, this);
		}

		HeaderAutomationPeer IRecordListAutomationPeer.GetHeaderPeer(Record record)
		{
			return this._listAutomationPeerHelper.GetHeaderPeer(record);
		}

		RecordCollectionBase IRecordListAutomationPeer.GetRecordCollection()
		{
			return this.GetRecordCollection();
		}

		IList<Record> IRecordListAutomationPeer.GetRecordList()
		{
			return this.GetRecordList();
		}

		int	IRecordListAutomationPeer.GetTableRowIndex(Cell cell)
		{
			return this._listAutomationPeerHelper.GetTableRowIndex(cell);
		}

		AutomationPeer IRecordListAutomationPeer.GetContainingGrid(Cell cell)
		{
			return this._listAutomationPeerHelper.GetContainingGrid(cell);
		}

		bool IRecordListAutomationPeer.IsRootLevel
		{
			get
			{
				return this.IsRootLevel;
			}
		}

		IRawElementProviderSimple IRecordListAutomationPeer.ProviderFromPeer(AutomationPeer peer)
		{
			return this.ProviderFromPeer(peer);
		}

		#endregion

		#region IListAutomationPeer
		AutomationPeer IListAutomationPeer.GetUnderlyingPeer(object item)
		{
			UIElement element = this.ContainerFromItem(item) as UIElement;

			// AS 1/21/10 TFS26545
			// The container won't have an associated peer if its our container that is used 
			// when using the framework itemgenerator and don't know the item type when the 
			// getcontainer is invoked.
			//
			RecordListItemContainer rlic = element as RecordListItemContainer;
			if (null != rlic)
				element = rlic.RecordPresenter;

			return null != element
				? UIElementAutomationPeer.CreatePeerForElement(element)
				: null;
		} 
		#endregion //IListAutomationPeer	
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