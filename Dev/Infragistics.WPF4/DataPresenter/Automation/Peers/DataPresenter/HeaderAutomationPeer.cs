using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes the header area of the <see cref="FieldLayout"/> to UI Automation
	/// </summary>
	public class HeaderAutomationPeer : AutomationPeerProxy,
		IWeakEventListener
	{
		#region Member Variables

		private FieldLayout _fieldLayout;

		// JM 08-20-09 NA 9.2 EnhancedGridView
		//private RecordListControlAutomationPeer _listAutomationPeer;
		private IRecordListAutomationPeer _recordListAutomationPeer;
		private IListAutomationPeer _listAutomationPeer;

		private Dictionary<Field, LabelAutomationPeer> _labelPeers;
		private int _fieldCollectionVersion = -1;
		private RecordListGroup _group;

		// AS 8/31/09 Optimization
		// Added caching to try and make the header handling more effecient.
		//
		private WeakReference _headerRecordPresenter;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="HeaderAutomationPeer"/>
		/// </summary>
		/// <param name="fieldLayout">FieldLayout that this header represents</param>
		/// <param name="recordListAutomationPeer">An instance of an AutomationPeer that implements IRecordListAutomationPeer</param>
		/// <param name="listAutomationPeer">An instance of an AutomationPeer that implements IListAutomationPeer</param>
		// JM 08-20-09 NA 9.2 EnhancedGridView
		//public HeaderAutomationPeer(FieldLayout fieldLayout, RecordListControlAutomationPeer listAutomationPeer)
		public HeaderAutomationPeer(FieldLayout fieldLayout, IRecordListAutomationPeer recordListAutomationPeer, IListAutomationPeer listAutomationPeer)
		{
			if (fieldLayout == null)
				throw new ArgumentNullException("fieldLayout");

			// JM 08-20-09 NA 9.2 EnhancedGridView
			//if (recordListAutomationPeer == null)
			//    throw new ArgumentNullException("recordListAutomationPeer");

			this._fieldLayout = fieldLayout;
			this._recordListAutomationPeer	= recordListAutomationPeer;
			this._listAutomationPeer		= listAutomationPeer;

			// listen for collection changes...
			CollectionChangedEventManager.AddListener(this._fieldLayout.Fields, this);
		}

		/// <summary>
		/// Initializes a new <see cref="HeaderAutomationPeer"/>
		/// </summary>
		/// <param name="group">The group list control that contains the automation peer</param>
		internal HeaderAutomationPeer(RecordListGroup group) : this(group.FieldLayout, group.RecordListAutomationPeer, group.ListAutomationPeer)
		{
			if (group == null)
				throw new ArgumentNullException("group");

			this._group = group;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetUnderlyingPeer
		/// <summary>
		/// Returns the automation peer for which this proxy is associated.
		/// </summary>
		/// <returns>A <see cref="HeaderPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer GetUnderlyingPeer()
		{
			HeaderPresenter header = this.GetHeaderElement();

			// JM 09-10-09 TFS21947
			//return null != header
			//    ? UIElementAutomationPeer.CreatePeerForElement(header)
			//    : null;
			AutomationPeer peer = null != header
				? UIElementAutomationPeer.CreatePeerForElement(header)
				: null;

			if (null != peer)
				peer.EventsSource = this;

			return peer;
		} 
		#endregion //GetUnderlyingPeer

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Header</b> enumeration value</returns>
		protected override System.Windows.Automation.Peers.AutomationControlType GetAutomationControlTypeCore()
		{
			return System.Windows.Automation.Peers.AutomationControlType.Header;
		} 
		#endregion //GetAutomationControlTypeCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the <see cref="Record"/> that is associated with this <see cref="RecordAutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			List<AutomationPeer> list = base.GetChildrenCore();

			if (list != null)
			{
				// remove all the cell value presenter peer since we will be 
				// exposing wrapper ones instead
				list.RemoveAll(new Predicate<AutomationPeer>(delegate(AutomationPeer peer)
				{
					return peer is FrameworkElementAutomationPeer &&
						((FrameworkElementAutomationPeer)peer).Owner is LabelPresenter;
				}));
			}
			else
				list = new List<AutomationPeer>();

			this.VerifyLabelPeers();

			Dictionary<Field, LabelAutomationPeer> labelPeers = this.LabelPeers;

			// AS 5/31/11 TFS76934
			// Get all the label presenters currently in use in the header element.
			//
			var header = this.GetHeaderElement();
			var labels = new Dictionary<Field, LabelPresenter>();

			if (header != null)
			{
				Utilities.DependencyObjectSearchCallback<LabelPresenter> callback = new Utilities.DependencyObjectSearchCallback<LabelPresenter>(delegate(LabelPresenter element)
				{
					Field f = element.Field;

					if (null != f)
						labels[f] = element;

					return false;
				});
				Utilities.GetDescendantFromType<LabelPresenter>(header, true, callback, new Type[] { typeof(LabelPresenter) });
			}

			foreach (Field field in this._fieldLayout.Fields)
			{
				LabelAutomationPeer labelPeer;

				if (labelPeers.TryGetValue(field, out labelPeer))
				{
					// AS 5/31/11 TFS76934
					// If we find an element for which the event source would now be different 
					// then update the cached children of the new peer and the old associated 
					// peer.
					//
					LabelPresenter labelElem;
					if (labels.TryGetValue(field, out labelElem))
					{
						var labelElemPeer = UIElementAutomationPeer.FromElement(labelElem);

						if (labelElemPeer != null && labelElemPeer.EventsSource != labelPeer)
						{
							// update the children of the old one
							if (labelElemPeer.EventsSource != null)
								AutomationPeerHelper.InvalidateChildren(labelElemPeer.EventsSource);

							labelElemPeer.EventsSource = labelPeer;
							AutomationPeerHelper.InvalidateChildren(labelPeer);
						}
					}

					list.Add(labelPeer);
				}
			}

			return list;
		}

		#endregion //GetChildrenCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the class
		/// </summary>
		/// <returns>A string that contains 'Header'</returns>
		protected override string GetClassNameCore()
		{
			return "Header";
		} 
		#endregion //GetClassNameCore

		#endregion //Base class overrides

		#region Properties

		#region FieldLayout
		internal FieldLayout FieldLayout
		{
			get { return this._fieldLayout; }
		}
		#endregion //FieldLayout 

		#region LabelPeers
		private Dictionary<Field, LabelAutomationPeer> LabelPeers
		{
			get
			{
				if (this._labelPeers == null)
					this._labelPeers = new Dictionary<Field, LabelAutomationPeer>();

				return this._labelPeers;
			}
		}
		#endregion //LabelPeers

		#region Version
		internal int Version
		{
			get { return this._fieldCollectionVersion; }
		} 
		#endregion //Version

		#endregion //Properties

		#region Methods

		#region CreateLabelAutomationPeer
		/// <summary>
		/// Creates an automation peer used to represent the specified <see cref="Field"/>
		/// </summary>
		/// <param name="field">The field for which an automation peer is to be created</param>
		/// <returns>A <see cref="LabelAutomationPeer"/></returns>
		internal virtual LabelAutomationPeer CreateLabelAutomationPeer(Field field)
		{
			return new LabelAutomationPeer(field, this);
		}
				#endregion //CreateLabelAutomationPeer

		#region Deactivate
		internal void Deactivate()
		{
			// listen for collection changes...
			CollectionChangedEventManager.RemoveListener(this._fieldLayout.Fields, this);
		}
		#endregion //Deactivate

		#region GetFieldIndex
		internal int GetFieldIndex(Field field)
		{
			this.VerifyLabelPeers();

			int visibleFieldIndex = 0;
			LabelAutomationPeer labelPeer;

			foreach (Field visibleField in this._fieldLayout.Fields)
			{
				if (this._labelPeers.TryGetValue(visibleField, out labelPeer))
				{
					if (field == visibleField)
						return visibleFieldIndex;

					visibleFieldIndex++;
				}
			}

			return -1;
		}
		#endregion //GetFieldIndex

		#region GetFieldAtIndex
		internal Field GetFieldAtIndex(int index)
		{
			this.VerifyLabelPeers();

			int visibleFieldIndex = 0;
			LabelAutomationPeer labelPeer;

			foreach (Field field in this._fieldLayout.Fields)
			{
				if (this._labelPeers.TryGetValue(field, out labelPeer))
				{
					if (visibleFieldIndex == index)
						return field;

					visibleFieldIndex++;
				}
			}

			return null;
		}
		#endregion //GetFieldAtIndex

		// AS 9/1/09 Optimization
		#region GetFlatViewHeaderElement
		private RecordPresenter GetFlatViewHeaderElement(DataPresenterBase dp)
		{
			FlatScrollRecordsCollection records = dp.InternalRecords as FlatScrollRecordsCollection;
			RecordPresenter rp = null;
			Debug.Assert(null != records);
			List<HeaderRecord> headers = null != records ? records.HeadersUsedInLastMeasure : null;

			if (null != headers && headers.Count > 0)
			{
				RecordCollectionBase owningRecords = _recordListAutomationPeer.GetRecordCollection();

				for (int i = 0, count = headers.Count; i < count; i++)
				{
					HeaderRecord hr = headers[i];

					if (null == hr || hr.FieldLayout != _fieldLayout)
						continue;

					Record r = hr.AttachedToRecord;

					// AS 1/13/11 TFS59762
					// An AttachedToRecord of null is valid when there are no records in 
					// the grid. We can make this assumption that its valid because we're 
					// get it from the flat collection based on the last measure.
					//
					if (r != null)
					{
						// AS 1/13/11 TFS59762
						//if (r == null || r.ParentCollection != owningRecords)
						if (r.ParentCollection != owningRecords)
							continue;

						if (_group != null && 0 != _group.CompareTo(r.VisibleIndex))
							continue;
					}

					rp = hr.AssociatedRecordPresenter;
					break;
				}
			}

			Debug.Assert(null == rp || IsValidRecordPresenter(rp));

			return rp;
		}
		#endregion //GetFlatViewHeaderElement 

		#region GetHeaderItem
		internal LabelAutomationPeer GetHeaderItem(Cell cell)
		{
			this.VerifyLabelPeers();

			LabelAutomationPeer peer;
			if (false == this.LabelPeers.TryGetValue(cell.Field, out peer))
				return null;

			return peer;
		} 
		#endregion //GetHeaderItem

		#region GetHeaderItems
		internal IRawElementProviderSimple[] GetHeaderItems()
		{
			this.VerifyLabelPeers();

			List<IRawElementProviderSimple> labels = new List<IRawElementProviderSimple>(this._labelPeers.Count);
			LabelAutomationPeer labelPeer;

			foreach (Field field in this._fieldLayout.Fields)
			{
				if (this._labelPeers.TryGetValue(field, out labelPeer))
					labels.Add(this.ProviderFromPeer(labelPeer));
			}

			return labels.ToArray();
		}
		#endregion //GetHeaderItems

		#region GetHeaderItemCount
		internal int GetHeaderItemCount()
		{
			this.VerifyLabelPeers();

			return this._labelPeers.Count;
		}
		#endregion //GetHeaderItemCount

		#region GetHeaderElement
		internal HeaderPresenter GetHeaderElement()
		{
			RecordPresenter recordWithHeader = null;

			// AS 8/31/09 Optimization
			// Added some basic caching so we can try to avoid the element tree walk to find the 
			// HeaderPresenter associated with this peer.
			//
			if (_headerRecordPresenter != null)
			{
				recordWithHeader = Utilities.GetWeakReferenceTargetSafe(_headerRecordPresenter) as RecordPresenter;

				if (recordWithHeader != null)
				{
					if (!recordWithHeader.IsLoaded || !IsValidRecordPresenter(recordWithHeader))
						recordWithHeader = null;
				}
			}

			if (null == recordWithHeader)
			{
				// AS 9/1/09 Optimization
				// For flat view we don't need to do an element tree walk. We can check 
				// the list of header records that have been arranged.
				//
				DataPresenterBase dp = _fieldLayout.DataPresenter;

				if (dp != null && dp.IsFlatView)
				{
					recordWithHeader = this.GetFlatViewHeaderElement(dp);
				}
				else
				{
					// JM 08-20-09 NA 9.2 EnhancedGridView
					//ItemsControl itemsControl = this._recordListAutomationPeer.Owner as ItemsControl;
					ItemsControl itemsControl = this._listAutomationPeer.Owner as ItemsControl;

					Utilities.DependencyObjectSearchCallback<RecordPresenter> callback = new Utilities.DependencyObjectSearchCallback<RecordPresenter>(delegate(RecordPresenter record)
					{
						
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

						return this.IsValidRecordPresenter(record);
					});

					// AS 8/31/09 Optimization
					// In neither case (flat view or nested) do we need to walk into a record presenter.
					//
					//RecordPresenter recordWithHeader = Utilities.GetDescendantFromType<RecordPresenter>(itemsControl, true, callback, new Type[] { typeof(ItemsControl) });
					recordWithHeader = Utilities.GetDescendantFromType<RecordPresenter>(itemsControl, true, callback, new Type[] { typeof(RecordPresenter) });
				}
			}

			_headerRecordPresenter = null == recordWithHeader ? null : new WeakReference(recordWithHeader);

			if (null != recordWithHeader)
			{
				// AS 8/31/09 Optimization
				// We should have been looking within the record we found and not the whole items control again. Also, we should 
				// have started with the HeaderContentSite for effeciency.
				//
				//HeaderPresenter header = Utilities.GetDescendantFromType<HeaderPresenter>(itemsControl, true, null, new Type[] { typeof(ItemsControl) });
				FrameworkElement startingElement = recordWithHeader.GetHeaderContentSite();

				if (startingElement == null)
					return null;

				HeaderPresenter header = Utilities.GetDescendantFromType<HeaderPresenter>(startingElement, true, null, new Type[] { typeof(ItemsControl) });

				if (null != header)
					return header;
			}

			return null;
		}

		#endregion //GetHeaderElement

		// AS 8/31/09 Optimization
		// Refactored into a helper method and also updated to handle flat view.
		//
		#region IsValidRecordPresenter
		internal bool IsValidRecordPresenter(RecordPresenter rp)
		{
			DataPresenterBase dp = _fieldLayout.DataPresenter;

			if (null == dp)
				return false;

			if (!rp.HasHeaderContent || rp.FieldLayout != _fieldLayout)
				return false;

			// get a sibling record for the presenter
			Record r = rp.GetRecordForMeasure();

			HeaderRecord hr = r as HeaderRecord;

			if (null != hr)
			{
				r = hr.AttachedToRecord;

				// AS 1/13/11 TFS59762
				// A HeaderRecord without an attached to record is valid for example 
				// when there are no records in the grid.
				//
				if (r == null)
				{
					if (rp.Visibility == Visibility.Collapsed)
						return false;

					if (_group != null)
						return false;

					return true;
				}
			}

			if (r == null)
				return false;

			if (dp.IsFlatView)
			{
				if (r.ParentCollection != _recordListAutomationPeer.GetRecordCollection())
					return false;
			}

			if (this._group != null && this._group.IndexOf(r) < 0)
				return false;

			return true;
		} 
		#endregion //IsValidRecordPresenter

		#region OnVisibleFieldsChanged
		internal void OnVisibleFieldsChanged()
		{
			// reset the version so we're dirty
			this._fieldCollectionVersion = -1;

			// get the old peers and newPeers
			Dictionary<Field, LabelAutomationPeer> oldPeers = this._labelPeers;

			this.VerifyLabelPeers();
			Dictionary<Field, LabelAutomationPeer> currentPeers = this.LabelPeers;

			int oldPeerCount = oldPeers != null ? oldPeers.Count : 0;
			int newPeerCount = currentPeers != null ? currentPeers.Count : 0;
			bool hasChanged = oldPeerCount != newPeerCount;

			// if we have the same number of peers
			if (false == hasChanged && oldPeerCount > 0)
			{
				foreach (KeyValuePair<Field, LabelAutomationPeer> entry in currentPeers)
				{
					if (oldPeers.ContainsKey(entry.Key) == false)
					{
						hasChanged = true;
						break;
					}
				}
			}

			if (hasChanged)
			{
				// raise a column chain notification
				if (oldPeerCount != newPeerCount)
				{
					AutomationPeer gridAutomation = this._group != null
						? (AutomationPeer)this._group.AutomationPeer
						: (AutomationPeer)this._recordListAutomationPeer;

					bool isHorizontal = this._group != null
						? this._group.AutomationPeer.IsHorizontalRowLayout
						: this._recordListAutomationPeer.IsHorizontalRowLayout;

					AutomationProperty columnProp = false == isHorizontal
							? GridPatternIdentifiers.ColumnCountProperty
							: GridPatternIdentifiers.RowCountProperty;

					gridAutomation.RaisePropertyChangedEvent(columnProp, oldPeerCount, newPeerCount);
				}

				// raise a structure change notification for the list since this affects
				// the header as well as the records (specifically their cells)
				// JM 08-20-09 NA 9.2 EnhancedGridView
				//this._listAutomationPeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
				if (this._recordListAutomationPeer is AutomationPeer)
					((AutomationPeer)this._recordListAutomationPeer).RaiseAutomationEvent(AutomationEvents.StructureChanged);
			}
		} 
		#endregion //OnVisibleFieldsChanged

		#region VerifyLabelPeers
		private void VerifyLabelPeers()
		{
			// release any cells that are no longer part of the field layout
			if (this._fieldLayout.Fields.Version != this._fieldCollectionVersion)
			{
				this._fieldCollectionVersion = this._fieldLayout.Fields.Version;

				// store the old ones for reuse
				Dictionary<Field, LabelAutomationPeer> oldLabelPeers = this._labelPeers;
				this._labelPeers = null;

				Dictionary<Field, LabelAutomationPeer> labelPeers = this.LabelPeers;

				foreach (Field field in this._fieldLayout.Fields)
				{
					if (field.IsVisibleInCellArea == false)
						continue;

					LabelAutomationPeer labelPeer;

					if (oldLabelPeers == null || false == oldLabelPeers.TryGetValue(field, out labelPeer))
						labelPeer = this.CreateLabelAutomationPeer(field);

					labelPeers.Add(field, labelPeer);
				}
			}
		}
		#endregion //VerifyLabelPeers

		#endregion //Methods

		#region IWeakEventListener

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(CollectionChangedEventManager))
			{
				this.OnVisibleFieldsChanged();
				return true;
			}

			return false;
		}

		#endregion //IWeakEventListener
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