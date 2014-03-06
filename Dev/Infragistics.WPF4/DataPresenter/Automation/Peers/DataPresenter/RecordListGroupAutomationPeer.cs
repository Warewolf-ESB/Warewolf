using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Diagnostics;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Automation peer that represents a group of records within a <see cref="RecordListControl"/>
	/// </summary>
	public class RecordListGroupAutomationPeer : AutomationPeer,
		ITableProvider
	{
		#region Member Variables

		private RecordListGroup _group;
		private HeaderAutomationPeer _headerPeer;

		#endregion //Member Variables

		#region Constructor
		internal RecordListGroupAutomationPeer(RecordListGroup group)
		{
			this._group = group;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetAcceleratorKeyCore
		/// <summary>
		/// Returns the accelerator key for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The accelerator key</returns>
		protected override string GetAcceleratorKeyCore()
		{
			return string.Empty;
		}
		#endregion //GetAcceleratorKeyCore

		#region GetAccessKeyCore
		/// <summary>
		/// Returns the access key for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The access key</returns>
		protected override string GetAccessKeyCore()
		{
			return string.Empty;
		}
		#endregion //GetAccessKeyCore

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>DataGrid</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			// AS 8/28/09 TFS21509
			// We probably shouldn't report this as a grid if we're not supporting the grid pattern. Instead 
			// report we are a list.
			//
			if (!this._group.SupportsGridPattern)
				return AutomationControlType.List;

			return AutomationControlType.DataGrid;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetAutomationIdCore
		/// <summary>
		/// Returns the <see cref="System.Windows.Automation.AutomationIdentifier"/> for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The ui automation identifier</returns>
		protected override string GetAutomationIdCore()
		{
			return string.Empty;
		}
		#endregion //GetAutomationIdCore

		#region GetBoundingRectangleCore
		/// <summary>
		/// Returns the <see cref="Rect"/> that represents the bounding rectangle of the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The bounding rectangle</returns>
		protected override System.Windows.Rect GetBoundingRectangleCore()
		{
			// AS 9/1/09
			// Include the header bounds in the rectangle.
			//
			Rect? bounds = this._group.GetVisibleBounds(_group.SupportsGridPattern ? this.HeaderPeer : null);

			return bounds.HasValue
				? bounds.Value
				// AS 8/28/09 TFS21509
				// An empty rect actually has an infinite width/height.
				//
				//: Rect.Empty;
				: new Rect();
		}
		#endregion //GetBoundingRectangleCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the object that is associated with this <see cref="RecordListControlAutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			List<AutomationPeer> children = this._group.GetRecordPeers();

			if (this.CouldSupportGridPattern())
			{
				AutomationPeer header = this.HeaderPeer;

				if (null != header)
					children.Insert(0, header);
			}

			return children;
		}

		#endregion //GetChildrenCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="CellValuePresenter"/>
		/// </summary>
		/// <returns>A string that contains 'CellValuePresenter'</returns>
		protected override string GetClassNameCore()
		{
			return "RecordListGroup";
		}

		#endregion //GetClassNameCore

		#region GetClickablePointCore
		/// <summary>
		/// Returns the <see cref="Point"/> that represents the clickable space for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The point that represents the clickable space on the element</returns>
		protected override System.Windows.Point GetClickablePointCore()
		{
			return new Point(double.NaN, double.NaN);
		}
		#endregion //GetClickablePointCore

		#region GetHelpTextCore
		/// <summary>
		/// Returns the string that describes the functionality of the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The help text</returns>
		protected override string GetHelpTextCore()
		{
			return string.Empty;
		}
		#endregion //GetHelpTextCore

		#region GetItemStatusCore
		/// <summary>
		/// Returns a string that conveys the visual status for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The status</returns>
		protected override string GetItemStatusCore()
		{
			return string.Empty;
		}
		#endregion //GetItemStatusCore

		#region GetItemTypeCore
		/// <summary>
		/// Returns a human readable string that contains the type of item for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The item type</returns>
		protected override string GetItemTypeCore()
		{
			return string.Empty;
		}
		#endregion //GetItemTypeCore

		#region GetLabeledByCore
		/// <summary>
		/// Returns the <see cref="AutomationPeer"/> for the <see cref="System.Windows.Controls.Label"/> that is targeted to the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The <see cref="LabelAutomationPeer"/> of the <see cref="System.Windows.Controls.Label"/> that targets this element</returns>
		protected override AutomationPeer GetLabeledByCore()
		{
			return null;
		}
		#endregion //GetLabeledByCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore()
		{
			// JM 12-22-09 TFS23799
			if (this._group					!= null && 
				this._group.FieldLayout		!= null && 
				this._group.FieldLayout.Key != null)
				return this._group.FieldLayout.Key.ToString();

			return string.Empty;
		}
		#endregion //GetNameCore

		#region GetOrientationCore
		/// <summary>
		/// Returns the value that indicates the direction in which the <see cref="UIElement"/> is laid out.
		/// </summary>
		/// <returns>The direction of the <see cref="UIElement"/> or <b>AutomationOrientation.None</b> if no direction is specified</returns>
		protected override AutomationOrientation GetOrientationCore()
		{
			return AutomationOrientation.None;
		}
		#endregion //GetOrientationCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Grid || patternInterface == PatternInterface.Table)
			{
				if (false == this.CouldSupportGridPattern())
					return null;

				return this;
			}

			return null;
		}
		#endregion //GetPattern

		#region HasKeyboardFocusCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> currently has the keyboard input focus.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> has the keyboard input focus; otherwise <b>false</b>.</returns>
		protected override bool HasKeyboardFocusCore()
		{
			return false;
		}
		#endregion //HasKeyboardFocusCore

		#region IsContentElementCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> contains data that is presented to the user.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> is a content element; otherwise, <b>false</b>.</returns>
		protected override bool IsContentElementCore()
		{
			return true;
		}
		#endregion //IsContentElementCore

		#region IsControlElementCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is understood by the end user as interactive.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> is a control; otherwise, <b>false</b>.</returns>
		protected override bool IsControlElementCore()
		{
			return true;
		}
		#endregion //IsControlElementCore

		#region IsEnabledCore
		/// <summary>
		/// Returns a value indicating whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can receive and send events.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> can send and receive events; otherwise, <b>false</b>.</returns>
		protected override bool IsEnabledCore()
		{
			return true;
		}
		#endregion //IsEnabledCore

		#region IsKeyboardFocusableCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can accept keyboard focus.
		/// </summary>
		/// <returns><b>True</b> if the element can accept keyboard focus; otherwise, <b>false</b>.</returns>
		protected override bool IsKeyboardFocusableCore()
		{
			return false;
		}
		#endregion //IsKeyboardFocusableCore

		#region IsOffscreenCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is off the screen.
		/// </summary>
		/// <returns><b>True</b> if the element is off the screen; otherwise, <b>false</b>.</returns>
		protected override bool IsOffscreenCore()
		{
			// AS 6/5/07
			// This could cause a recursive situation in the wpf framework.
			//
			//return this.GetBoundingRectangle().IsEmpty;
			// AS 9/1/09
			// We don't return Empty any more for the rect so see if its a 0 rect.
			//
			//return this.GetBoundingRectangleCore().IsEmpty;
			return this.GetBoundingRectangleCore().Equals(new Rect());
		}
		#endregion //IsOffscreenCore

		#region IsPasswordCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> contains protected content.
		/// </summary>
		/// <returns><b>True</b> if the element contains protected content; otherwise, <b>false</b>.</returns>
		protected override bool IsPasswordCore()
		{
			return false;
		}
		#endregion //IsPasswordCore

		#region IsRequiredForFormCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is required to be completed on the form.
		/// </summary>
		/// <returns><b>True</b> if the element is required to be completed; otherwise, <b>false</b>.</returns>
		protected override bool IsRequiredForFormCore()
		{
			return false;
		}
		#endregion //IsRequiredForFormCore

		#region SetFocusCore
		/// <summary>
		/// Sets the keyboard input focus on the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		protected override void SetFocusCore()
		{
		}
		#endregion //SetFocusCore

		#endregion //Base class overrides

		#region Properties

		#region FieldCount
		private int FieldCount
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
		private IRawElementProviderSimple[] FieldHeaders
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

		#region HeaderPeer
		internal HeaderAutomationPeer HeaderPeer
		{
			get
			{
				if (this._headerPeer == null)
				{
					Debug.Assert(this.CouldSupportGridPattern(), "We should not be trying to access the header peer when we cannot be a grid/table!");

					if (this.CouldSupportGridPattern())
						this._headerPeer = new HeaderAutomationPeer(this._group);
				}

				return this._headerPeer;
			}
		}
		#endregion //HeaderPeer

		#region IsHorizontalRowLayout
		internal bool IsHorizontalRowLayout
		{
			get
			{
				return this._group.RecordListAutomationPeer.IsHorizontalRowLayout;
			}
		}
		#endregion //IsHorizontalRowLayout

		#region RecordCount
		private int RecordCount
		{
			get
			{
				return this._group.RecordCount;
			}
		}
		#endregion //RecordCount

		#endregion //Properties

		#region Methods

		#region CouldSupportGridPattern
		private bool CouldSupportGridPattern()
		{
			// AS 8/28/09 TFS21509
			// The owner may not support the grid pattern (which is why we created the groups) but 
			// the group knows now whether it supports the grid pattern.
			//
			//return this._group.RecordListAutomationPeer.CouldSupportGridPattern();
			return this._group.SupportsGridPattern;
		}
		#endregion //CouldSupportGridPattern

		// AS 1/5/12 TFS23077
		#region Initialize
		internal void Initialize(RecordListGroup newGroup)
		{
			CoreUtilities.ValidateNotNull(newGroup);
			_group = newGroup;
		}
		#endregion //Initialize

		#endregion //Methods

		#region ITableProvider

		IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
		{
			return this.IsHorizontalRowLayout
				? new IRawElementProviderSimple[0]
				: this.FieldHeaders;
		}

		IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
		{
			return this.IsHorizontalRowLayout
				? this.FieldHeaders
				: new IRawElementProviderSimple[0];
		}

		System.Windows.Automation.RowOrColumnMajor ITableProvider.RowOrColumnMajor
		{
			get
			{
				return this.IsHorizontalRowLayout
					? RowOrColumnMajor.ColumnMajor
					: RowOrColumnMajor.RowMajor;
			}
		}

		#endregion //ITableProvider

		#region IGridProvider

		int IGridProvider.ColumnCount
		{
			get
			{
				return this.IsHorizontalRowLayout
					? this.RecordCount
					: this.FieldCount;
			}
		}

		IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
		{
			if (row > ((IGridProvider)this).RowCount || row < 0)
				throw new ArgumentOutOfRangeException("row");

			if (column > ((IGridProvider)this).ColumnCount || column < 0)
				throw new ArgumentOutOfRangeException("column");

			if (this.IsHorizontalRowLayout)
			{
				int temp = column;
				column = row;
				row = temp;
			}

			DataRecord record = this._group.GetRecord(row) as DataRecord;

			// JJD 10/26/11 - TFS91364 
			// Ignore HeaderRecords
			//if (null != record)
			if (null != record && !(record is HeaderRecord))
			{
				RecordAutomationPeer recordPeer = record.AutomationPeer;

				if (null != recordPeer)
				{
					Field field = this.HeaderPeer.GetFieldAtIndex(column);
					CellAutomationPeer cellPeer = recordPeer.GetCellPeer(record.Cells[field]);

					if (null != cellPeer)
						return this.ProviderFromPeer(cellPeer);
				}
			}

			return null;
		}

		int IGridProvider.RowCount
		{
			get
			{
				return this.IsHorizontalRowLayout
					? this.FieldCount
					: this.RecordCount;
			}
		}

		#endregion //IGridProvider
	}






	internal class RecordListGroup
	{
		#region Member Variables

		private List<Record> _records = new List<Record>();

		// JM 08-20-09 NA 9.2 EnhancedGridView
		//private RecordListControlAutomationPeer _listAutomationPeer;
		private IRecordListAutomationPeer _recordListAutomationPeer;
		private IListAutomationPeer _listAutomationPeer;

		private FieldLayout _fieldLayout;
		private RecordListGroupAutomationPeer _peer;
		private int _lastRecordCountNotification;

		// AS 8/28/09 TFS21509
		// Previously the group would ask the owning list peer but that may not support the pattern
		// but the group could be a collection of datarecords that do.
		//
		private bool _supportsGridPattern;

		// AS 9/1/09
		internal static readonly Rect EmptyRect = new Rect();

		#endregion //Member Variables

		#region Constructor
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//internal RecordListGroup(RecordListControlAutomationPeer listAutomationPeer, FieldLayout fieldLayout)
		// AS 8/28/09 TFS21509
		//internal RecordListGroup(IRecordListAutomationPeer recordListAutomationPeer, IListAutomationPeer listAutomationPeer, FieldLayout fieldLayout)
		internal RecordListGroup(IRecordListAutomationPeer recordListAutomationPeer, IListAutomationPeer listAutomationPeer, FieldLayout fieldLayout, bool supportsGridPattern)
		{
			this._recordListAutomationPeer = recordListAutomationPeer;
			this._fieldLayout = fieldLayout;
			this._listAutomationPeer = listAutomationPeer;

			// AS 8/28/09 TFS21509
			this._supportsGridPattern = supportsGridPattern;
		}
		#endregion //Constructor

		#region Properties

		#region AutomationPeer
		internal RecordListGroupAutomationPeer AutomationPeer
		{
			get
			{
				if (this._peer == null)
					this._peer = new RecordListGroupAutomationPeer(this);

				return this._peer;
			}
		}
		#endregion //AutomationPeer

		#region FieldLayout
		internal FieldLayout FieldLayout
		{
			get { return this._fieldLayout; }
		}
		#endregion //FieldLayout

		// JM 08-20-09 NA 9.2 EnhancedGridView - Added
		#region RecordListAutomationPeer
		internal IListAutomationPeer ListAutomationPeer
		{
			get
			{
				return this._listAutomationPeer;
			}
		}
		#endregion //ListAutomationPeer

		#region RecordListAutomationPeer
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//internal RecordListControlAutomationPeer ListAutomationPeer
		internal IRecordListAutomationPeer RecordListAutomationPeer
		{
			get
			{
				return this._recordListAutomationPeer;
			}
		}
		#endregion //RecordListAutomationPeer

		#region RecordCount
		internal int RecordCount
		{
			get { return this._records.Count; }
		} 
		#endregion //RecordCount

		// AS 8/28/09 TFS21509
		#region SupportsGridPattern
		internal bool SupportsGridPattern
		{
			get { return _supportsGridPattern; }
		} 
		#endregion //SupportsGridPattern

		#endregion //Properties

		#region Methods

		#region AddRecord
		internal void AddRecord(Record record)
		{
			// AS 8/28/09 TFS21509
			Debug.Assert(null != record && _supportsGridPattern == ListAutomationPeerHelper.CouldSupportGridPattern(record.RecordType));

			this._records.Add(record);
		}
		#endregion //AddRecord

		#region CompareTo
		internal int CompareTo(int recordIndex)
		{
			if (recordIndex < this._records[0].VisibleIndex)
				return -1;
			else if (recordIndex > this._records[this._records.Count - 1].VisibleIndex)
				return 1;
			else
				return 0;
		} 
		#endregion //CompareTo

		#region GetRecord
		internal Record GetRecord(int index)
		{
			return this._records[index];
		} 
		#endregion //GetRecord

		#region GetRecordPeers
		internal List<AutomationPeer> GetRecordPeers()
		{
			List<AutomationPeer> peers = new List<AutomationPeer>(this._records.Count);

			for(int i = 0, count = this._records.Count; i < count; i++)
			{
				Record record = this._records[i];

				AutomationPeer peer = record.AutomationPeer;

				if (peer == null)
					peer = this.RecordListAutomationPeer.CreateAutomationPeer(record);

				peers.Add(peer);
			}

			return peers;
		} 
		#endregion //GetRecordPeers

		#region GetVisibleBounds
		// AS 9/1/09
		// Added headerPeer parameter since we should include its bounds as part of the group's bounds.
		//
		internal Rect? GetVisibleBounds(HeaderAutomationPeer headerPeer)
		{
			bool recursive = _fieldLayout.DataPresenter != null && _fieldLayout.DataPresenter.IsFlatView;
			return GetVisibleBounds(_records, headerPeer, recursive);
		}

		// AS 9/1/09
		// Changed to helper method so we can use this for ViewableRecordCollectionAutomationPeer.
		//
		internal static Rect? GetVisibleBounds(IList<Record> records, HeaderAutomationPeer headerPeer, bool recursive)
		{
			// AS 9/1/09 Not used
			//RecordListControl rlc = this._listAutomationPeer.Owner as RecordListControl;

			Rect? bounds = null;

			for (int i = 0; i < records.Count; i++)
			{
				AutomationPeer peer = records[i].AutomationPeer;

				// AS 1/5/12 TFS23077
				// The peer for the record may not exist but we can still use the 
				// rect from the peer for the associated record presenter. This was 
				// causing the initial hit test to fail because we didn't have a 
				// bounding rectangle and returned that we are off screen.
				//
				//if (null != peer)
				//{
				//    Rect peerRect = peer.GetBoundingRectangle();
				Rect peerRect;

				if (null != peer)
					peerRect = peer.GetBoundingRectangle();
				else
				{
					var rp = records[i].AssociatedRecordPresenter;

					if (null == rp)
						continue;

					peerRect = UIElementAutomationPeer.CreatePeerForElement(rp).GetBoundingRectangle();
				}

				{
					
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

					if (!peerRect.Equals(EmptyRect))
					{
						if (bounds == null)
							bounds = peerRect;
						else
							bounds = Rect.Union(bounds.Value, peerRect);
					}
				}
			}

			// AS 9/1/09
			// We should include the rect of the header peer.
			//
			if (null != headerPeer)
			{
				Rect headerRect = headerPeer.GetBoundingRectangle();

				if (!headerRect.Equals(EmptyRect))
					bounds = bounds == null ? headerRect : Rect.Union(bounds.Value, headerRect);
			}

			return bounds;
		}

		#endregion //GetVisibleBounds

		#region InitializeFrom
		internal void InitializeFrom(RecordListGroup oldGroup)
		{
			Debug.Assert(this._peer == null, "We already initialized the peer!");

			if (null == this._peer && null != oldGroup)
			{
				this._peer = oldGroup._peer;

				// AS 1/5/12 TFS23077
				// The peer was still referencing the old group.
				//
				if (null != _peer)
					_peer.Initialize(this);
			}

			this._lastRecordCountNotification = oldGroup._lastRecordCountNotification;

			// AS 1/5/12 TFS23077
			// If we're reusing a group and the count is different then we need to notify 
			// any listeners that the children of the peer are different.
			//
			if (this.RecordCount != oldGroup.RecordCount && _peer != null)
			{
				AutomationPeerHelper.InvalidateChildren(_peer);
				_peer.InvalidatePeer();
			}
		} 
		#endregion //InitializeFrom

		#region InitializeNotifyRecordCount
		internal void InitializeNotifyRecordCount()
		{
			this._lastRecordCountNotification = this._records.Count;
		}
		#endregion //InitializeNotifyRecordCount

		#region IsSameGroup
		internal bool IsSameGroup(RecordListGroup group)
		{
			if (group._fieldLayout == this._fieldLayout)
			{
				// make sure that this object has the same records
				// as the group specified - note the new group can
				// have more
				foreach (Record record in this._records)
				{
					if (group._records.Contains(record) == false)
						return false;
				}

				return true;
			}

			return false;
		} 
		#endregion //IsSameGroup

		#region IndexOf
		internal int IndexOf(Record record)
		{
			if (record.FieldLayout != this.FieldLayout)
				return -1;

			return this._records.IndexOf(record);
		} 
		#endregion //IndexOf

		#region RaiseRecordCountChange
		internal void RaiseRecordCountChange()
		{
			if (this._lastRecordCountNotification != this._records.Count)
			{
				if (this._peer != null)
				{
					// JM 08-20-09 NA 9.2
					//AutomationProperty property = this._listAutomationPeer.IsHorizontalRowLayout
					AutomationProperty property = this._recordListAutomationPeer.IsHorizontalRowLayout
						? GridPatternIdentifiers.ColumnCountProperty
						: GridPatternIdentifiers.RowCountProperty;

					this._peer.RaisePropertyChangedEvent(property, this._lastRecordCountNotification, this._records.Count);
				}

				this._lastRecordCountNotification = this._records.Count;
			}
		}
		#endregion //RaiseRecordCountChange

		#region RemoveAt
		internal void RemoveAt(int recordIndex)
		{
			this._records.RemoveAt(recordIndex);
		}
		#endregion //RemoveAt

		#endregion //Methods
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