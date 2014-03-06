using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Security;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="Record"/> types to UI Automation
	/// </summary>
	// AS 6/8/07 UI Automation
	//public class RecordAutomationPeer : AutomationPeerProxy,
	public class RecordAutomationPeer : RecycleableItemAutomationPeer,
		IExpandCollapseProvider,
		IScrollItemProvider,
		ISelectionItemProvider
	{
		#region Member Variables

		private Record _record;
		// AS 6/8/07 UI Automation
		//private RecordListControlAutomationPeer _listAutomationPeer;

		// JM 08-20-09 NA 9.2 EnhancedGridView
		private IRecordListAutomationPeer	_recordListAutomationPeer;

		private int _fieldCollectionVersion = -1;
		private Dictionary<Cell, CellAutomationPeer> _cellPeers;

		// AS 8/28/09 TFS21509
		// We need to cache the child collection peer.
		//
		private ViewableRecordCollectionAutomationPeer _childRecordsPeer;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="RecordAutomationPeer"/> class
		/// </summary>
		/// <param name="record">The <see cref="Record"/> for which the peer is being created</param>
		/// <param name="recordListAutomationPeer">The <see cref="IRecordListAutomationPeer"/> interface implemented by the containing automation peer that is the parent of the <see cref="RecordPresenterAutomationPeer"/> being created</param>
		/// <param name="listAutomationPeer">The <see cref="IListAutomationPeer"/> interface implemented by the containing automation peer that is the parent of the <see cref="RecordPresenterAutomationPeer"/> being created</param>
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//public RecordAutomationPeer(Record record, RecordListControlAutomationPeer listAutomationPeer)
		public RecordAutomationPeer(Record record, IRecordListAutomationPeer recordListAutomationPeer, IListAutomationPeer listAutomationPeer)
			// AS 6/8/07 UI Automation
			//: base()
			: base(record, listAutomationPeer)
		{
			
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			this._record = record;
			// AS 6/8/07 UI Automation
			//this._listAutomationPeer = listAutomationPeer;

			// JM 08-20-09 NA 9.2 EnhancedGridView
			this._recordListAutomationPeer = recordListAutomationPeer;

			record.InitializeAutomationPeer(this);
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>DataItem</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			// should be list item when not representing a data record
			// AS 8/17/11 TFS83442
			//if (this._record is DataRecord)
			//    return AutomationControlType.DataItem;
			//else
			//    return AutomationControlType.ListItem;
			switch (_record.RecordType)
			{
				case RecordType.DataRecord:
				case RecordType.FilterRecord:
					return AutomationControlType.DataItem;
				default:
					return AutomationControlType.ListItem;
			}
		}

		#endregion //GetAutomationControlTypeCore

		#region GetBoundingRectangleCore
		/// <summary>
		/// Returns the <see cref="Rect"/> that represents the bounding rectangle of the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The bounding rectangle</returns>
		protected override Rect GetBoundingRectangleCore()
		{
			// AS 9/1/09
			// Previously the uielement for a record encompassed the children. However in a flat view
			// each record element is sibling to the ancestors so we need to aggregate the records.
			//
			if (_childRecordsPeer != null)
			{
				Rect rect = base.GetBoundingRectangleCore();
				Rect childrenRect = _childRecordsPeer.GetBoundingRectangle();

				if (rect.Equals(RecordListGroup.EmptyRect))
					return childrenRect;
				else if (childrenRect.Equals(RecordListGroup.EmptyRect))
					return rect;

				return Rect.Union(rect, childrenRect);
			}

			return base.GetBoundingRectangleCore();
		} 
		#endregion //GetBoundingRectangleCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the <see cref="Record"/> that is associated with this <see cref="RecordAutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			// AS 6/21/11 TFS79160
			AutomationPeerHelper.RemovePendingChildrenInvalidation(this);

			List<AutomationPeer> list = base.GetChildrenCore();

			DataRecord dataRecord = this._record as DataRecord;

			// AS 8/17/11 TFS83442
			//if (null != dataRecord)
			if (null != dataRecord && dataRecord is HeaderRecord == false)
			{
				if (list != null)
				{
					
#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)

					RecordPresenter rp = dataRecord.AssociatedRecordPresenter;

					if (null != rp)
					{
						FrameworkElementAutomationPeer recordPeer = this.GetUnderlyingPeer() as FrameworkElementAutomationPeer;

						// so if the peer that we are reporting as our underlying peer is not 
						// the peer of the record presenter then we want to get the peer from 
						// the record presenter and include his actual children
						if (recordPeer == null || recordPeer.Owner is RecordPresenter == false)
						{
							RecordPresenterAutomationPeer rpPeer = UIElementAutomationPeer.CreatePeerForElement(rp) as RecordPresenterAutomationPeer;

							if (null != rpPeer)
							{
								Debug.Assert(rpPeer.IsWithinRecordContainer);

								// JM 02-23-10 TFS28163 - Make sure the returned list is not null;
								//list.AddRange(rpPeer.GetActualChildren());
								List<AutomationPeer> actualChildren = rpPeer.GetActualChildren();
								if (actualChildren != null)
									list.AddRange(actualChildren);
							}
						}
					}

					// remove all the cell value presenter peer since we will be 
					// exposing wrapper ones instead
					list.RemoveAll(new Predicate<AutomationPeer>(delegate(AutomationPeer peer)
					{
						return peer is FrameworkElementAutomationPeer &&
							((FrameworkElementAutomationPeer)peer).Owner is CellValuePresenter;
					}));
				}
				else
					list = new List<AutomationPeer>();

				// JM 08-27-09 TFS21509
				//this.VerifyCellPeers();
				this.VerifyCellPeers(false);

				Dictionary<Cell, CellAutomationPeer> cellPeers = this.CellPeers;

				foreach (Cell cell in dataRecord.Cells)
				{
					CellAutomationPeer cellPeer;

					if (cellPeers.TryGetValue(cell, out cellPeer))
						list.Add(cellPeer);
				}
			}

			// JM 08-20-09 NA 9.2 EnhancedGridView
			Record record = this._record;

			// AS 8/28/09 TFS21509
			// Refactored this block. We shouldn't check the type of record since all records could 
			// have children. Also, we need to cache the peer for the child collection.
			//
			DataPresenterBase dp = record.DataPresenter;
			bool needChildren = dp != null && dp.IsFlatView && record.HasChildren;

			if (needChildren)
			{
				if (_childRecordsPeer == null)
				{
					IListAutomationPeer ilap = this._recordListAutomationPeer as IListAutomationPeer;

					if (ilap != null && ilap.Owner is RecordListControl)
						_childRecordsPeer = new ViewableRecordCollectionAutomationPeer(ilap.Owner as RecordListControl, record.ChildRecordsInternal.ViewableRecords, false);
					else
						Debug.Assert(false, "Cannot find RecordListControl for Record to create ViewableRecordCollectionAutomationPeer!");
				}

				if (null != _childRecordsPeer)
				{
					if (list == null)
						list = new List<AutomationPeer>();

					list.Add(_childRecordsPeer);
				}
			}
			else
			{
				_childRecordsPeer = null;
			}

			return list;
		}

		#endregion //GetChildrenCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="Record"/>
		/// </summary>
		/// <returns>A string that contains 'Record'</returns>
		protected override string GetClassNameCore()
		{
			return "Record";
		}

		#endregion //GetClassNameCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();

			if (String.IsNullOrEmpty(name))
			{
				// AS 8/17/11 TFS83442
				// Use the recordtype instead of the runtime type.
				//
				//if (this._record is DataRecord)
				//{
				//    object dataItem = ((DataRecord)this._record).DataItem;
				//
				//    if (null != dataItem)
				//        name = dataItem.ToString();
				//}
				//// JM 09-17-09 TFS22160
				//else
				//if (this._record is GroupByRecord)
				//    name = ((GroupByRecord)this._record).Description;
				//// JM 12-22-09 TFS23799
				//else
				//if (this._record is ExpandableFieldRecord)
				//    name = ((ExpandableFieldRecord)this._record).Field.Name;
				switch (_record.RecordType)
				{
					case RecordType.DataRecord:
						object dataItem = ((DataRecord)this._record).DataItem;

						if (null != dataItem)
							name = dataItem.ToString();
						break;
					case RecordType.GroupByField:
					case RecordType.GroupByFieldLayout:
						if (this._record is GroupByRecord)
							name = ((GroupByRecord)this._record).Description;
						break;
					case RecordType.ExpandableFieldRecord:
						if (this._record is ExpandableFieldRecord)
							name = ((ExpandableFieldRecord)this._record).Field.Name;
						break;
					case RecordType.HeaderRecord:
					case RecordType.FilterRecord:
					case RecordType.SummaryRecord:
						break;
					default:
						Debug.Fail("Unrecognized record type:" + _record.RecordType.ToString());
						break;
				}
			}

			return name;
		}
		#endregion //GetNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the <see cref="Record"/> that is associated with this <see cref="RecordAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.ExpandCollapse)
				return this;

			if (patternInterface == PatternInterface.ScrollItem)
				return this;

			if (patternInterface == PatternInterface.SelectionItem)
			{
				// the expandable field record does not support selection
				if (this._record is ExpandableFieldRecord)
					return null;

				return this;
			}

			return null;
		}
		#endregion //GetPattern

		#region GetUnderlyingPeer
		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetUnderlyingPeer

		#region IsOffscreenCore
		/// <summary>
		/// Returns a value that indicates whether the System.Windows.UIElement that corresponds with the object that is associated with this System.Windows.Automation.Peers.AutomationPeer is off the screen.
		/// </summary>
		protected override bool IsOffscreenCore()
		{
			// AS 9/1/09
			// The element for this record may not be on screen but its children
			// in a flat view could so we'll base this on the bounding rectangle.
			//
			return this.GetBoundingRectangleCore().Equals(new Rect());
		} 
		#endregion //IsOffscreenCore

		#endregion //Base class overrides

		#region Properties

		#region CellPeers
		private Dictionary<Cell, CellAutomationPeer> CellPeers
		{
			get
			{
				if (this._cellPeers == null)
					this._cellPeers = new Dictionary<Cell, CellAutomationPeer>();

				return this._cellPeers;
			}
		}
		#endregion //CellPeers

		#region RecordListAutomationPeer
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//internal RecordListControlAutomationPeer ListAutomationPeer
		internal IRecordListAutomationPeer RecordListAutomationPeer
		{
			// AS 6/8/07 UI Automation
			//get { return this._listAutomationPeer; }
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//get { return (RecordListControlAutomationPeer)this.ItemsControlAutomationPeer; }
			get { return this._recordListAutomationPeer; }
		} 
		#endregion //RecordListAutomationPeer

		#endregion //Properties

		#region Methods

		// JM 01-05-10 TFS22151 - Added
		#region ClearChildRecordsPeer
		internal void ClearChildRecordsPeer()
		{
			this._childRecordsPeer = null;
		}
		#endregion //ClearChildRecordsPeer

		#region CreateCellAutomationPeer
		/// <summary>
		/// Creates an automation peer used to represent the specified <see cref="Cell"/>
		/// </summary>
		/// <param name="cell">The cell for which an automation peer is to be created</param>
		/// <returns>A <see cref="CellAutomationPeer"/></returns>
		internal virtual CellAutomationPeer CreateCellAutomationPeer(Cell cell)
		{
			return new CellAutomationPeer(cell, this.RecordListAutomationPeer);
		}
		#endregion //CreateCellAutomationPeer

		#region GetCellPeer

		internal CellAutomationPeer GetCellPeer(Cell cell)
		{
			// JM 08-27-09 TFS21509
			//this.VerifyCellPeers();
			this.VerifyCellPeers(true);

			CellAutomationPeer peer;

			if (this.CellPeers.TryGetValue(cell, out peer))
				return peer;

			return null;
		} 
		#endregion //GetCellPeer

		#region VerifyCellPeers
		// JM 08-27-09 TFS21509
		//private void VerifyCellPeers()
		private void VerifyCellPeers(bool forceGetChildrenAfterCellPeerAddition)
		{
			DataRecord dataRecord = this._record as DataRecord;

			// AS 8/17/11 TFS83442
			//if (null != dataRecord)
			if (null != dataRecord && dataRecord.Cells != null)
			{
				// JM 08-20-09 NA 9.2 EnhancedGridView - Added
				//int headerVersion = dataRecord.FieldLayout == null ? int.MinValue : this.ListAutomationPeer.GetHeaderPeer(this._record).Version;
				// AS 8/28/09 TFS21509
				// Get the version from the field layout since the header peer may be dirty.
				//
				//int headerVersion = dataRecord.FieldLayout == null ? int.MinValue : this.RecordListAutomationPeer.GetHeaderPeer(this._record).Version;
				FieldLayout fl = _record.FieldLayout;
				int headerVersion = fl.Fields.Version;

				// release any cells that are no longer part of the field layout
				if (this._fieldCollectionVersion != headerVersion)
				{
					this._fieldCollectionVersion = headerVersion;

					// store the old ones for reuse
					Dictionary<Cell, CellAutomationPeer> oldCellPeers = this._cellPeers;
					this._cellPeers = null;

					Dictionary<Cell, CellAutomationPeer> cellPeers = this.CellPeers;

					bool cellPeerAdded = false;	// JM 08-27-09 TFS21509
					foreach (Cell cell in dataRecord.Cells)
					{
						if (cell.Field.IsVisibleInCellArea == false)
							continue;

						CellAutomationPeer cellPeer;

						if (oldCellPeers == null || false == oldCellPeers.TryGetValue(cell, out cellPeer))
						{
							// JM 08-27-09 TFS21509
							cellPeerAdded = true;

							cellPeer = this.CreateCellAutomationPeer(cell);
						}

						cellPeers.Add(cell, cellPeer);
					}

					// JM/AS 08-27-09 TFS21509
					// AS 8/28/09 TFS21509
					// We need to consider the children dirty if we added peers but also if 
					// we no longer have certain peers.
					//
					bool hasChanged = cellPeerAdded ||										// created a new child
						(oldCellPeers != null && oldCellPeers.Count != cellPeers.Count);	// didn't reuse all the peers

					if (forceGetChildrenAfterCellPeerAddition && hasChanged)
					{
						// AS 08/28/09 TFS21509
						// Instead of asking for the children (which will not actually ask for the children 
						// if it doesn't think the core collection is dirty), force the children to be cached 
						// because we know the collection to be dirty.
						//
						this.ResetChildrenCache();
					}
				}
			}
		} 
		#endregion //VerifyCellPeers

		#endregion //Methods

		#region IExpandCollapseProvider

		void IExpandCollapseProvider.Collapse()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
				throw new InvalidOperationException();

			this._record.IsExpanded = false;
		}

		void IExpandCollapseProvider.Expand()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			if (((IExpandCollapseProvider)this).ExpandCollapseState == ExpandCollapseState.LeafNode)
				throw new InvalidOperationException();

			this._record.IsExpanded = true;
		}

		ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
		{
			get
			{
				if (this._record.HasVisibleChildren)
				{
					if (this._record.IsExpanded)
						return ExpandCollapseState.Expanded;
					else
						return ExpandCollapseState.Collapsed;
				}
				else
					return ExpandCollapseState.LeafNode;
			}
		}

		#endregion //IExpandCollapseProvider

		#region IScrollItemProvider

		void IScrollItemProvider.ScrollIntoView()
		{
			bool scrolledIntoView = false;

			Record parent = this._record.ParentRecord;

			// make sure the record can be in view
			if (null != parent)
				parent.IsExpanded = true;

			DataPresenterBase dp = this._record.DataPresenter;
			IViewPanel panelNavigator = dp != null ? dp.CurrentPanel as IViewPanel : null;
			scrolledIntoView = panelNavigator.EnsureRecordIsVisible(this._record);

			if (false == scrolledIntoView)
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_3", this._record.GetType( ).Name ) );
		}

		#endregion //IScrollItemProvider

		#region ISelectionItemProvider

		void ISelectionItemProvider.AddToSelection()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			ISelectionProvider provider = this.ListAutomationPeer.GetPattern(PatternInterface.Selection) as ISelectionProvider;

			if (null != provider && 
				provider.CanSelectMultiple == false &&
				provider.GetSelection() != null)
			{
				throw new InvalidOperationException();
			}

			this._record.IsSelected = true;
		}

		bool ISelectionItemProvider.IsSelected
		{
			get { return this._record.IsSelected; }
		}

		void ISelectionItemProvider.RemoveFromSelection()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			this._record.IsSelected = false;
		}

		void ISelectionItemProvider.Select()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			DataPresenterBase dp = this._record.DataPresenter;

			if (null != dp)
				dp.InternalSelectItem(this._record, true, true);
		}

		IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView - Added
			//get { return this.ProviderFromPeer(this.ListAutomationPeer); }
			get { return this.ProviderFromPeer(this.ListAutomationPeer as AutomationPeer); }
		}

		#endregion //ISelectionItemProvider

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