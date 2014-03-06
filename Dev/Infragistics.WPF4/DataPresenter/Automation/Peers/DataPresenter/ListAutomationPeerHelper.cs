using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DataPresenter;
using System.Diagnostics;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;
using System.Collections.Specialized;
using System.Windows.Automation;
using Infragistics.Windows.Selection;
using System.Collections;
using System.Windows.Threading;
using Infragistics.Windows.Helpers;
using System.Threading;
using Infragistics.Collections;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	// JM 08-20-09 NA 9.2 EnhancedGridView
	//
	// Helper class that was created to support the new ViewableRecordCollectionAutomationPeer class as well as the existing RecordListControlAutomationPeer class.
	// Code from the RecordListControlAutomationPeer class that was useful to the new ViewableRecordCollectionAutomationPeer class was moved here and both classes
	// call into methods on this class to access that shared functionality.  There are both instance methods and static methods on this class.
	internal class ListAutomationPeerHelper
	{
		#region Member Variables

		private FieldLayout						_fieldLayout;
		private IRecordListAutomationPeer		_recordListAutomationPeer;
		private IListAutomationPeer				_listAutomationPeer;
		private HeaderAutomationPeer			_headerPeer;
		private Nullable<bool>					_hasRecordGroups = null;
		private bool							_isGroupsDirty = true;
		private List<RecordListGroup>			_groups;
		private int								_recordCount;
		private bool							_isHomogenousCollection;
		private bool							_isHomogenousCollectionDirty = true;


		#endregion //Member Variables

		#region Constructor

		internal ListAutomationPeerHelper(IRecordListAutomationPeer recordListAutomationPeer, IListAutomationPeer listAutomationPeer)
		{
			if (recordListAutomationPeer == null)
				throw new ArgumentNullException("recordListAutomationPeer");
			if (listAutomationPeer == null)
				throw new ArgumentNullException("listAutomationPeer");

			this._recordListAutomationPeer	= recordListAutomationPeer;
			this._listAutomationPeer		= listAutomationPeer;

			// store the initial record count so we can send row change notifications
			this._recordCount = this._recordListAutomationPeer.GetRecordList().Count;
		}

		#endregion //Constructor

		#region Properties

			#region FieldCount
		internal int FieldCount
		{
			get
			{
				HeaderAutomationPeer headerPeer = this.HeaderPeer;

				return null != headerPeer
					? headerPeer.GetHeaderItemCount()
					: 0;
			}
		}
			#endregion //FieldCount

			#region FieldHeaders
		internal IRawElementProviderSimple[] FieldHeaders
		{
			get
			{
				HeaderAutomationPeer headerPeer = this.HeaderPeer;

				if (headerPeer == null)
					return new IRawElementProviderSimple[0];

				return headerPeer.GetHeaderItems();
			}
		}
			#endregion //FieldHeaders

			#region FieldLayout
		private FieldLayout FieldLayout
		{
			get
			{
				// AS 9/19/07 BR26515
				if (this._fieldLayout == null)
					this.InitializeFieldLayout();

				Debug.Assert(this.GetRecordCollectionFieldLayout() == this._fieldLayout, "Field layouts are out of sync!");

				return this._fieldLayout;
			}
		}
			#endregion //FieldLayout
			
			#region HasRecordGroups
		internal bool HasRecordGroups
		{
			get
			{
				if (this._hasRecordGroups == null)
				{
					this._hasRecordGroups = false == this.IsHomogenousCollection();

					this._isGroupsDirty = true;
				}

				return this._hasRecordGroups == true;
			}
		}
			#endregion //HasRecordGroups

			#region HeaderPeer
		internal HeaderAutomationPeer HeaderPeer
		{
			get
			{
				if (this._headerPeer == null)
				{
					Debug.Assert(this._recordListAutomationPeer.CouldSupportGridPattern(), "We should not be trying to access the header peer when we cannot be a grid/table!");

					// AS 9/19/07 BR26515
					//if (this.CouldSupportGridPattern())
					FieldLayout fl = this.FieldLayout;

					if (this._recordListAutomationPeer.CouldSupportGridPattern() && fl != null)
					{
						//this._headerPeer = new HeaderAutomationPeer(fl, this);	// JM 08-20-09 NA 9.2 EnhancedGridView
						this._headerPeer = new HeaderAutomationPeer(fl, this._recordListAutomationPeer, this._listAutomationPeer);
					}
				}

				return this._headerPeer;
			}
		}
			#endregion //HeaderPeer

			#region IsHomogenousCollection
		internal bool IsHomogenousCollection()
		{
			if (this._isHomogenousCollectionDirty == false)
				return this._isHomogenousCollection;

			this._isHomogenousCollectionDirty = false;

			IList<Record> records = this._recordListAutomationPeer.GetRecordList();

			// AS 1/22/08
			//Debug.Assert(records != null, "We're expecting the list to be a record collection!");
			if (records == null)
				return true;

			FieldLayout listFieldLayout = this.GetRecordCollectionFieldLayout();

			// AS 9/19/07 BR26515
			// This can happen if the record collection is not yet initialized.
			//
			//Debug.Assert(listFieldLayout != null, "We're expecting the list to have a field layout!");
			if (records.Count > 0)
			{
				// AS 8/28/09 TFS21509
				bool supportsGrid = CouldSupportGridPattern(records[0].RecordType);

				foreach (Record record in records)
				{
					// AS 8/28/09 TFS21509
					// We also need to consider heterogenous if we have a mix of datarecord and non-datarecord types.
					//
					//if (record.FieldLayout != listFieldLayout)
					if (record.FieldLayout != listFieldLayout || supportsGrid != CouldSupportGridPattern(record.RecordType))
					{
						this._isHomogenousCollection = false;
						return this._isHomogenousCollection;
					}
				}
			}

			this._isHomogenousCollection = true;
			return this._isHomogenousCollection;
		}
			#endregion //IsHomogenousCollection

			#region RecordCount
		private int RecordCount
		{
			get	{ return this._recordListAutomationPeer.GetRecordList().Count; }
		}
			#endregion //RecordCount

		#endregion //Properties

		#region Methods

			#region Static Methods

				// AS 8/28/09 TFS21509
				// Added helper method to determine based on a record type if we can support the grid pattern.
				//
				#region CouldSupportGridPattern
		internal static bool CouldSupportGridPattern(RecordType collectionRecordType)
		{
			switch (collectionRecordType)
			{
				case RecordType.DataRecord:
				case RecordType.FilterRecord:
				case RecordType.HeaderRecord:
					return true;
				case RecordType.SummaryRecord:
				case RecordType.GroupByField:
				case RecordType.GroupByFieldLayout:
				case RecordType.ExpandableFieldRecord:
					break;
				default:
					Debug.Fail("Could this support the grid pattern?");
					break;
			}

			return false;
		} 
				#endregion //CouldSupportGridPattern

				#region CreateAutomationPeer

		internal static RecordAutomationPeer CreateRecordAutomationPeer(Record record, IRecordListAutomationPeer recordListAutomationPeer, IListAutomationPeer listAutomationPeer)
		{
			if (null == record)
				throw new ArgumentNullException("record");

			return new RecordAutomationPeer(record, recordListAutomationPeer, listAutomationPeer);
		}

				#endregion //CreateAutomationPeer

				#region RaiseAutomationSelectionEvents


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void RaiseAutomationSelectionEvents(DataPresenterBase.SelectedItemHolder oldSelection, DataPresenterBase.SelectedItemHolder newSelection)
		{
			// when a single cell is selected, we'll just send a OnElementSelected on behave
			// of the record list that contains the cell
			if (newSelection.HasCells &&
				newSelection.Cells.Count == 1 &&
				(oldSelection.HasCells == false || oldSelection.Cells.Count > 1 || oldSelection.Cells[0] != newSelection.Cells[0]))
			{
				if (false == newSelection.Cells[0].RaiseAutomationSelectedChange(AutomationEvents.SelectionItemPatternOnElementSelected))
				{
					// try to get the listcontrol for the parent collection and raise an invalidated notification
					RaiseListSelectionInvalidated(newSelection.Cells[0]);
				}

				return;
			}

			// when a single row is selected, we'll just send a OnElementSelected on behalf
			// of the record list that contains the record
			if (newSelection.HasRecords &&
				newSelection.Records.Count == 1 &&
				(oldSelection.HasRecords == false || oldSelection.Records.Count > 1 || oldSelection.Records[0] != newSelection.Records[0]))
			{
				if (false == newSelection.Records[0].RaiseAutomationSelectedChange(AutomationEvents.SelectionItemPatternOnElementSelected))
				{
					// try to get the listcontrol for the parent collection and raise an invalidated notification
					RaiseListSelectionInvalidated(newSelection.Records[0]);
				}

				return;
			}

			// for all other selection, we need to build a list so we know who to raise
			// the notification for and what type of notification to send.

			// start by getting a collection of all the records whose
			// selection state has been modified
			Record[] changedRecords;
			Cell[] changedCells;

			if (oldSelection.HasRecords == true && newSelection.HasRecords == false)
			{
				// everything was unselected...
				changedRecords = oldSelection.Records.ToArray();

				// if there are cells selected now...
				changedCells = newSelection.HasCells
					? newSelection.Cells.ToArray()
					: null;
			}
			else if (oldSelection.HasRecords == false && newSelection.HasRecords == true)
			{
				// nothing was selected before but now there is
				changedRecords = newSelection.Records.ToArray();

				// if there are cells selected now...
				changedCells = oldSelection.HasCells
					? oldSelection.Cells.ToArray()
					: null;
			}
			else if (oldSelection.HasCells == true && newSelection.HasCells == false)
			{
				// everything was unselected...
				changedCells = oldSelection.Cells.ToArray();

				// if there are records selected now...
				changedRecords = newSelection.HasRecords
					? newSelection.Records.ToArray()
					: null;
			}
			else if (oldSelection.HasCells == false && newSelection.HasCells == true)
			{
				// nothing was selected before but now there is
				changedCells = newSelection.Cells.ToArray();

				// can't be any records or we would have gotten into the first if block
				changedRecords = null;
			}
			else
			{
				// get the diff lists - i.e. the list of cells/records that were selected/unselected
				changedRecords = GetDiffList<Record>(oldSelection.Records, newSelection.Records).ToArray();
				changedCells = GetDiffList<Cell>(oldSelection.Cells, newSelection.Cells).ToArray();
			}

			// since we could have cells being selected and rows being unselected or 
			// vice versa, we need to build a combined list of the items whose selection
			// state has changed...
			ISelectableItem[] items;

			if (changedRecords == null || changedRecords.Length == 0)
				items = changedCells;
			else if (changedCells == null || changedCells.Length == 0)
				items = changedRecords;
			else
			{
				items = new ISelectableItem[changedCells.Length + changedRecords.Length];
				changedRecords.CopyTo(items, 0);
				changedCells.CopyTo(items, changedRecords.Length);
			}

			// now we have to sort the items by parent collection and split
			// them by parent collection

			// sort by the parent collection
			Utilities.SortMerge(items, SelectedItemComparer.Default);

			// now split by parent collection
			Dictionary<RecordCollectionBase, List<ISelectableItem>> splitItems = SplitItems(items, null);

			// now we can send the notifications for the records/cells
			foreach (KeyValuePair<RecordCollectionBase, List<ISelectableItem>> entry in splitItems)
			{
				// we'll put a selection threshold of 20 items. if you've selected/unselected
				// more then that within a single record collection, we'll just invalidate the
				// entire collection
				if (entry.Value.Count < AutomationInteropProvider.InvalidateLimit)
				{
					bool invalidateList = false;

					foreach (ISelectableItem item in entry.Value)
					{
						if (item is Record)
						{
							Record record = (Record)item;

							if (false == record.RaiseAutomationSelectedChange(record.IsSelected
									? AutomationEvents.SelectionItemPatternOnElementAddedToSelection
									: AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
							{
								invalidateList = true;
								break;
							}
						}
						else if (item is Cell)
						{
							Cell cell = (Cell)item;

							if (false == cell.RaiseAutomationSelectedChange(cell.IsSelected
									? AutomationEvents.SelectionItemPatternOnElementAddedToSelection
									: AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
							{
								invalidateList = true;
								break;
							}
						}
					}

					if (invalidateList == false)
						continue;
				}

				// get the peer for the list control and send an invalidated if we couldn't send a notification
				// for each item or if there were too many items whose selected state has changed
				RecordListControl rl = entry.Key.LastRecordList;

				if (rl == null && entry.Key.ParentRecordManager != null)
				{
					ViewableRecordCollection visibleRecords = entry.Key.ParentRecordManager.ViewableRecords as ViewableRecordCollection;

					if (visibleRecords != null)
						rl = visibleRecords.LastRecordList;
				}

				if (null != rl)
				{
					AutomationPeer peer = UIElementAutomationPeer.FromElement(rl) as AutomationPeer;

					if (null != peer)
						peer.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
				}
			}
		}
				#endregion //RaiseAutomationSelectionEvents

				#region RaiseListSelectionInvalidated
		private static void RaiseListSelectionInvalidated(Record record)
		{
			if (record != null)
				RaiseListSelectionInvalidated(record.ParentRecordList);
		}

		private static void RaiseListSelectionInvalidated(Cell cell)
		{
			if (null != cell.Record)
				RaiseListSelectionInvalidated(cell.Record);
		}

		private static void RaiseListSelectionInvalidated(RecordListControl listControl)
		{
			if (null != listControl)
			{
				AutomationPeer peer = UIElementAutomationPeer.FromElement(listControl) as AutomationPeer;

				if (null != peer)
					peer.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
			}
		}
				#endregion //RaiseListSelectionInvalidated

			#endregion //Static Methods

			#region Internal Methods

				#region CreateGroups
		private List<RecordListGroup> CreateGroups()
		{
			// get the previous groups
			List<RecordListGroup> oldGroups = this._groups;

			// create new groups...
			IList<Record> records = this._recordListAutomationPeer.GetRecordList();
			List<RecordListGroup> groups = new List<RecordListGroup>();
			RecordListGroup group = null;

			// JM 03-26-10 TFS29903
			if (records == null)
				return null;

			foreach (Record record in records)
			{
				// AS 8/28/09 TFS21509
				// We also need to consider whether it supports the grid pattern so all records in the 
				// group are or are not grid items.
				//
				//if (group == null || group.FieldLayout != record.FieldLayout)
				bool supportsGridPattern = ListAutomationPeerHelper.CouldSupportGridPattern(record.RecordType);

				if (group == null || group.FieldLayout != record.FieldLayout || group.SupportsGridPattern != supportsGridPattern)
				{
					// JM 08-20-09 NA 9.2 EnhancedGridView
					//group = new RecordListGroup(this, record.FieldLayout);
					// AS 8/28/09 TFS21509
					// Pass a flag that indicates if the group supports the grid pattern. Previously it would 
					// ask the owning listautomationpeer but the owner may not support the grid pattern 
					// because its has hetergenous records.
					//
					group = new RecordListGroup(this._recordListAutomationPeer, this._listAutomationPeer, record.FieldLayout, supportsGridPattern);
					groups.Add(group);
				}

				group.AddRecord(record);
			}

			foreach (RecordListGroup newGroup in groups)
				newGroup.InitializeNotifyRecordCount();

			// reuse the automation peers where possible
			if (oldGroups != null)
			{
				foreach (RecordListGroup newGroup in groups)
				{
					for (int i = 0; i < oldGroups.Count; i++)
					{
						RecordListGroup oldGroup = oldGroups[i];

						// note the new group could have more records as long
						// as it has all the old ones. this way we can handle 
						// when new records are added and we can keep at least
						// one group when there were two and they became joined
						// because the records between were removed
						if (oldGroup != null && oldGroup.IsSameGroup(newGroup))
						{
							oldGroups[i] = null;
							newGroup.InitializeFrom(oldGroup);
							break; // AS 1/5/12 TFS23077 - Don't keep looking for a match if we find one
						}
					}
				}
			}

			return groups;
		}
				#endregion //CreateGroups

				#region GetContainingGrid
		internal AutomationPeer GetContainingGrid(Cell cell)
		{
			if (this.HasRecordGroups)
			{
				RecordListGroup group = this.GetRecordListGroup(cell.Record);

				return null != group
					? group.AutomationPeer
					: null;
			}

			return this._recordListAutomationPeer as AutomationPeer;
		}
				#endregion //GetContainingGrid

				#region GetDiffList
		private static List<T> GetDiffList<T>(SelectedItemCollectionBase oldItems, SelectedItemCollectionBase newItems)
			where T : class, ISelectableItem
		{
			// build a diff list of the items that have changed their state
			List<T> list = new List<T>();

			for (int i = 0, count = newItems.Count; i < count; i++)
				list.Add(newItems.GetItem(i) as T);

			for (int i = 0, count = oldItems.Count; i < count; i++)
			{
				T item = oldItems.GetItem(i) as T;

				// if the record was selected before, remove it
				if (item.IsSelected)
					list.Remove(item);
				else
					list.Add(item);
			}

			return list;
		}
				#endregion //GetDiffList
		
				#region GetHeaderPeer
		internal HeaderAutomationPeer GetHeaderPeer(Record record)
		{
			if (this.HasRecordGroups)
			{
				RecordListGroup group = this.GetRecordListGroup(record);

				return null != group
					? group.AutomationPeer.HeaderPeer
					: null;
			}

			return this.HeaderPeer;
		}
				#endregion //GetHeaderPeer

				#region GetRecordCollectionFieldLayout
		private FieldLayout GetRecordCollectionFieldLayout()
		{
			RecordCollectionBase records = this._recordListAutomationPeer.GetRecordCollection();

			return records.FieldLayout;
		} 
				#endregion //GetRecordCollectionFieldLayout

				#region GetRecordListGroup
		internal RecordListGroup GetRecordListGroup(Record record)
		{
			List<RecordListGroup> groups = this.GetRecordListGroups();

			if (groups != null && null != record)
			{
				// AS 8/28/09 TFS21509
				// The previous code would have always gotten the first group.
				//
				//int recordIndex = 0;
				int recordIndex = record.VisibleIndex;
				int start = 0;
				int end = groups.Count - 1;

				// do a binary search to find the group that contains the record
				while (start < end)
				{
					int index = start + ((end - start) / 2);

					Debug.Assert(index >= 0 && index < groups.Count);

					RecordListGroup group = groups[index];
					int comparison = group.CompareTo(recordIndex);

					if (comparison == 0)
						return group;
					else if (comparison < 0)
						start = index + 1;
					else //if (comparison > 0)
						end = index - 1;
				}

				if (groups.Count == 1)
					return groups[0];
			}

			return null;
		}
				#endregion //GetRecordListGroup
		
				#region GetRecordListGroups
		internal List<RecordListGroup> GetRecordListGroups()
		{
			// only recreate the collection if we didn't know...
			if (this._isGroupsDirty)
			{
				// AS 12/7/09 TFS24565/TFS23077
				// We should be resetting this flag so we know that the 
				// groups are synchronized.
				//
				_isGroupsDirty = false;

				if (this.HasRecordGroups)
					this._groups = this.CreateGroups();
				else
					this._groups = null;
			}

			return this._groups;
		}
				#endregion //GetRecordListGroups
		
				#region GetSelectedItems
		private List<ISelectableItem> GetSelectedItems()
		{
			// AS 1/5/12 TFS23077
			// Found this while debugging. We may get in here before the fieldlayout is initialized.
			//
			if (null == _fieldLayout)
				return null;

			DataPresenterBase dp = this._fieldLayout.DataPresenter;

			// we only need to worry about selected records/rows and cells
			if (null != dp &&
				(dp.SelectedItems.HasRecords || dp.SelectedItems.HasCells))
			{
				IList<Record> records = this._recordListAutomationPeer.GetRecordList();

				// don't bother if the list doesn't have any rows
				if (records != null && records.Count > 0)
				{
					// selection of the rows/cells are mutually exclusive so just get one
					ISelectableItem[] allItems = dp.SelectedItems.HasRecords
						? (ISelectableItem[])dp.SelectedItems.Records.ToArray()
						: (ISelectableItem[])dp.SelectedItems.Cells.ToArray();

					// now we have to sort the items by parent collection and split
					// them by parent collection
					Utilities.SortMerge(allItems, SelectedItemComparer.Default);

					RecordCollectionBase parentCollection = this._recordListAutomationPeer.GetRecordCollection();

					// now split by parent collection
					Dictionary<RecordCollectionBase, List<ISelectableItem>> splitRecords = SplitItems(allItems, parentCollection);

					List<ISelectableItem> items;
					if (splitRecords.TryGetValue(parentCollection, out items))
						return items;
				}
			}

			return null;
		}
				#endregion //GetSelectedItems

				#region GetTableRowIndex
		internal int GetTableRowIndex(Cell cell)
		{
			if (this.HasRecordGroups)
			{
				RecordListGroup group = this.GetRecordListGroup(cell.Record);

				Debug.Assert(null != group, "Unable to locate the containing group!");

				return null != group
					? group.IndexOf(cell.Record)
					: 0;
			}

			Record record = cell.Record;

			if (null != record)
			{
				int index = this._recordListAutomationPeer.GetRecordList().IndexOf(record);

				return index;
			}

			return 0;
		}
				#endregion //GetTableRowIndex

				#region InitializeFieldLayout
		internal void InitializeFieldLayout()
		{
			FieldLayout oldFieldLayout = this._fieldLayout;
			FieldLayout newFieldLayout = null;

			if (this._recordListAutomationPeer.CouldSupportGridPattern())
				newFieldLayout = this.GetRecordCollectionFieldLayout();

			// if the layout has changed...
			if (oldFieldLayout != newFieldLayout)
			{
				if (null != this._headerPeer)
				{
					this._headerPeer.Deactivate();
					this._headerPeer = null;
				}

				// store the new field layout
				this._fieldLayout = newFieldLayout;
			}
		}
				#endregion //InitializeFieldLayout

				#region ProcessListChange
		internal void ProcessListChange(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// JM 09-10-09 TFS21947
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				this.InitializeFieldLayout();

			this._isHomogenousCollectionDirty = true;

			// AS 12/7/09 TFS24565/TFS23077
			// I'm not sure why were bailing out here previously but even if we had groups we need to 
			// 
			//if (false == this._recordListAutomationPeer.CouldSupportGridPattern())
			//	return;

			bool hadRecordGroups = this.HasRecordGroups;

			#region Remove records

			// if we have record groups then we need to remove any removed/replaced records
			// so we can try to reuse the group is possible
			if (hadRecordGroups)
			{
				List<RecordListGroup> groups = this.GetRecordListGroups();

				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Remove:
					case NotifyCollectionChangedAction.Replace:
						// find the groups that contain the records and remove them
						// 
						foreach (object item in e.OldItems)
						{
							Record record = item as Record;

							for (int i = 0, count = groups.Count; i < count; i++)
							{
								int index = groups[i].IndexOf(record);

								if (index >= 0)
								{
									groups[i].RemoveAt(index);
									break;
								}
							}
						}
						break;
				}
			}
			#endregion //Remove records

			// now since we're unsure if we need groups, dirty the state
			// AS 12/7/09 TFS24565/TFS23077
			//this._hasRecordGroups = null;
			this.SetHasRecordGroups(null);

			// check if we have record groups
			if (this.HasRecordGroups)
			{
				// if so, get them and send notifications for any group that has 
				// changed. new groups or removed groups will be handled by the 
				// invalidate peer below
				List<RecordListGroup> groups = this.GetRecordListGroups();

				// JM 03-26-10 TFS29903 - Check for null groups.
				if (groups != null)
				{
					foreach (RecordListGroup group in groups)
					{
						group.RaiseRecordCountChange();
					}
				}
			}
			else if (false == hadRecordGroups)
			{
				// if we didn't have groups before and still don't then we can
				// send a record count change. if we had groups then they didn't
				// know we had records but we'll handle that below by sending
				// a change to whether we have the grid pattern
				IList<Record> records = this._recordListAutomationPeer.GetRecordList();

				// JM 03-26-10 TFS29903
				//if (this._recordCount != records.Count)
				if (records != null && this._recordCount != records.Count)
				{
					AutomationProperty property = this._recordListAutomationPeer.IsHorizontalRowLayout
						? GridPatternIdentifiers.ColumnCountProperty
						: GridPatternIdentifiers.RowCountProperty;

					// JM 08-20-09 NA 9.2 EnhancedGridView
					//this.RaisePropertyChangedEvent(property, this._recordCount, records.Count);
					((AutomationPeer)this._recordListAutomationPeer).RaisePropertyChangedEvent(property, this._recordCount, records.Count);
					this._recordCount = records.Count;
				}
			}

			// if we went from having record groups to not having them or vice/versa
			// then the grid pattern is either available or not available
			if (hadRecordGroups != this.HasRecordGroups)
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//this.RaisePropertyChangedEvent(AutomationElementIdentifiers.IsGridPatternAvailableProperty, false == hadRecordGroups, false == this.HasRecordGroups);
				((AutomationPeer)this._recordListAutomationPeer).RaisePropertyChangedEvent(AutomationElementIdentifiers.IsGridPatternAvailableProperty, false == hadRecordGroups, false == this.HasRecordGroups);
			}

			// we need to indicate that items were added/removed so reset the child cache
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//this.InvalidatePeer();
			// AS 4/13/11 TFS72669
			//((AutomationPeer)this._recordListAutomationPeer).InvalidatePeer();
			AutomationPeer peer = this._recordListAutomationPeer as AutomationPeer;
			peer.InvalidatePeer();
			AutomationPeerHelper.InvalidateChildren(peer);
		}
				#endregion //ProcessListChange

				#region SetHasRecordGroups
		internal void SetHasRecordGroups(bool? value)
		{
			this._hasRecordGroups = value;

			// AS 12/7/09 TFS24565/TFS23077
			// Since we are now resetting this flag to false when we craete the groups, 
			// we need to change it to dirty when the state changes.
			//
			this._isGroupsDirty = true;
		}
				#endregion //SetHasRecordGroups
				
				#region SplitItems


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private static Dictionary<RecordCollectionBase, List<ISelectableItem>> SplitItems(ISelectableItem[] items, RecordCollectionBase targetParentCollection)
		{
			Dictionary<RecordCollectionBase, List<ISelectableItem>> table = new Dictionary<RecordCollectionBase, List<ISelectableItem>>();
			RecordCollectionBase lastParentCollection = null;
			List<ISelectableItem> list = null;

			for (int i = 0; i < items.Length; i++)
			{
				ISelectableItem item = items[i];

				RecordCollectionBase records = item is Record
					? ((Record)item).ParentCollection
					: ((Cell)item).Record.ParentCollection;

				if (records == null)
					continue;

				// skip the record when looking for ones from a specific parent
				if (targetParentCollection != null && targetParentCollection != records)
					continue;

				// if its a different parent collection then start a new list...
				if (records != lastParentCollection)
				{
					Debug.Assert(table.ContainsKey(records) == false, "The list was supposed to be sorted based on the parent collection and we have already added this parent collection into the table!");

					lastParentCollection = records;
					list = new List<ISelectableItem>();
					table.Add(lastParentCollection, list);
				}

				// add it to the list we're dealing with
				list.Add(item);
			}

			return table;
		}
				#endregion //SplitItems

			#endregion //Internal Methods

			#region ISelectionProvider Methods

		internal bool ISelectionProvider_CanSelectMultiple
		{
			get
			{
				List<ISelectableItem> selectedItems = this.GetSelectedItems();

				// base the ability to select multiple on the first selected item
				if (selectedItems != null && selectedItems.Count > 0)
				{
					DataPresenterBase dp = this._fieldLayout.DataPresenter;
					SelectionStrategyBase selectionStrategy = ((ISelectionHost)dp).GetSelectionStrategyForItem(selectedItems[0]);

					if (null != selectionStrategy)
						return selectionStrategy.IsMultiSelect;
				}

				// otherwise assume multi selection is allowed
				return true;
			}
		}

		internal IRawElementProviderSimple[] ISelectionProvider_GetSelection()
		{
			List<ISelectableItem> selectedItems = this.GetSelectedItems();

			if (selectedItems != null && selectedItems.Count > 0)
			{
				List<IRawElementProviderSimple> selectedItemPeers = new List<IRawElementProviderSimple>(selectedItems.Count);

				foreach (ISelectableItem item in selectedItems)
				{
					AutomationPeer itemPeer = null;

					// get the automation peer for the record or cell
					if (item is Record)
						itemPeer = ((Record)item).AutomationPeer;
					else if (item is Cell)
						itemPeer = ((Cell)item).AutomationPeer;

					if (itemPeer != null)
						selectedItemPeers.Add(this._recordListAutomationPeer.ProviderFromPeer(itemPeer));
				}

				return selectedItemPeers.ToArray();
			}

			return null;
		}

		internal bool ISelectionProvider_IsSelectionRequired
		{
			get { return false; }
		}

			#endregion //ISelectionProvider Methods

			#region ITableProvider Methods

		internal IRawElementProviderSimple[] ITableProvider_GetColumnHeaders()
		{
			return this._recordListAutomationPeer.IsHorizontalRowLayout
				? new IRawElementProviderSimple[0]
				: this.FieldHeaders;
		}

		internal IRawElementProviderSimple[] ITableProvider_GetRowHeaders()
		{
			return this._recordListAutomationPeer.IsHorizontalRowLayout
				? this.FieldHeaders 
				: new IRawElementProviderSimple[0];
		}

		internal System.Windows.Automation.RowOrColumnMajor ITableProvider_RowOrColumnMajor
		{
			get
			{
				return this._recordListAutomationPeer.IsHorizontalRowLayout
						? RowOrColumnMajor.ColumnMajor
						: RowOrColumnMajor.RowMajor;
			}
		}

				#endregion //ITableProvider Methods

			#region IGridProvider Methods

		internal int IGridProvider_ColumnCount
		{
			get
			{
				return this._recordListAutomationPeer.IsHorizontalRowLayout
					? this.RecordCount
					: this.FieldCount;
			}
		}

		internal IRawElementProviderSimple IGridProvider_GetItem(int row, int column)
		{
			// JM 08-26-09 TFS21509
			//if (row > ((IGridProvider)this).RowCount || row < 0)
			if (row > IGridProvider_RowCount || row < 0)
				throw new ArgumentOutOfRangeException("row");

			// JM 08-26-09 TFS21509
			//if (column > ((IGridProvider)this).ColumnCount || column < 0)
			if (column > IGridProvider_ColumnCount || column < 0)
				throw new ArgumentOutOfRangeException("column");

			if (this._recordListAutomationPeer.IsHorizontalRowLayout)
			{
				int temp = column;
				column = row;
				row = temp;
			}

			IList<Record> list = this._recordListAutomationPeer.GetRecordList();
			DataRecord record = list[row] as DataRecord;

			// we only care about data records
			// JJD 10/26/11 - TFS91364 
			// Ignore HeaderRecords
			//if (null == record)
			if (null == record || record is HeaderRecord)
			{
				return null;
			}

			RecordAutomationPeer recordPeer = record.AutomationPeer;

			if (recordPeer == null)
			{
				// get the children to force the peers to be created
				List<AutomationPeer> recordPeers = ((AutomationPeer)this._recordListAutomationPeer).GetChildren();

				// now get the peer for the record
				recordPeer = record.AutomationPeer;
			}

			if (recordPeer != null)
			{
				Field field = this.HeaderPeer.GetFieldAtIndex(column);

				CellAutomationPeer			cellPeer	= recordPeer.GetCellPeer(record.Cells[field]);
				IRawElementProviderSimple	ireps		= null;
				if (cellPeer != null)
					ireps = this._recordListAutomationPeer.ProviderFromPeer(cellPeer);

				return ireps;
			}

			return null;
		}

		internal int IGridProvider_RowCount
		{
			get
			{
				return this._recordListAutomationPeer.IsHorizontalRowLayout
					? this.FieldCount
					: this.RecordCount;
			}
		}

			#endregion //IGridProvider Methods

		#endregion //Methods

		#region SelectedItemComparer Nested Class
		private class SelectedItemComparer : IComparer
		{
			#region Member Variables

			private static readonly SelectedItemComparer _default;

			#endregion //Member Variables

			#region Constructor
			static SelectedItemComparer()
			{
				_default = new SelectedItemComparer();
			}

			private SelectedItemComparer()
			{
			}
			#endregion //Constructor

			#region Properties
			internal static SelectedItemComparer Default
			{
				get { return SelectedItemComparer._default; }
			}
			#endregion //Properties

			#region IComparer

			int IComparer.Compare(object x, object y)
			{
				Record recordX = null;
				Record recordY = null;

				if (x is Cell)
					recordX = ((Cell)x).Record;
				else
					recordX = x as Record;

				if (y is Cell)
					recordY = ((Cell)y).Record;
				else
					recordY = y as Record;

				// we don't really care about the order other than to have all the same
				// parent collections together

				// if they're from different collections...
				if (recordX.ParentCollection != recordY.ParentCollection)
					return recordX.ParentCollection.GetHashCode().CompareTo(recordY.ParentCollection.GetHashCode());

				return 0;
			}

			#endregion //IComparer<Record>
		}
		#endregion //SelectedItemComparer Nested Class
	}

	
#region Infragistics Source Cleanup (Region)




































































#endregion // Infragistics Source Cleanup (Region)

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