using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using System.Diagnostics;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="ViewableRecordCollection"/> types to UI Automation
	/// </summary>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
	public class ViewableRecordCollectionAutomationPeer : AutomationPeer,
															IListAutomationPeer,
															IRecordListAutomationPeer,
															ISelectionProvider,
															ITableProvider
	{
		#region Member Variables

		private RecordListControl					_recordListControl;
		private ViewableRecordCollection			_viewableRecordCollection;
		private ListAutomationPeerHelper			_listAutomationPeerHelper;
		private Dictionary<object, RecycleableItemAutomationPeer> 
													_itemPeers = new Dictionary<object, RecycleableItemAutomationPeer>();
		private bool								_isRootLevel;

		private const bool							DebugGetChildren = false;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of the <see cref="ViewableRecordCollectionAutomationPeer"/> class./>
		/// </summary>
		/// <param name="recordListControl">The root RecordListControl if this peer represents the root ViewableRecordCollection, otherwise null.</param>
		/// <param name="viewableRecordCollection">The ViewableRecordCollection that this peer represents.</param>
		/// <param name="isRootLevel">The ViewableRecordCollection that this peer represents is at the root level.</param>
		public ViewableRecordCollectionAutomationPeer(RecordListControl recordListControl, ViewableRecordCollection viewableRecordCollection, bool isRootLevel)
		{
			if (recordListControl == null)
				throw new ArgumentException("recordListControl");
			if (viewableRecordCollection == null)
				throw new ArgumentException("viewableRecordCollection");

			this._recordListControl			= recordListControl;
			this._isRootLevel				= isRootLevel;

			// JM 12-21-09 TFS22151
			//this._viewableRecordCollection = viewableRecordCollection;
			//this._viewableRecordCollection.CollectionChanged 
			//                                += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnCollectionChanged);
			this.InitializeViewableRecordsCollection(viewableRecordCollection);

			this._listAutomationPeerHelper	= new ListAutomationPeerHelper(this, this);
			this._listAutomationPeerHelper.InitializeFieldLayout();

			// AS 7/26/11 TFS80926
			DataPresenterBaseAutomationPeer.AddProxyPeerHost(this, viewableRecordCollection.RecordManager.DataPresenter);
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
			// JM 10-22-09 TFS24157 - Return AutomationControlType DataGrid if we don't have groups and we support the Table Pattern.  Otherwise return AutomationControlType List.
			//return AutomationControlType.Custom;
			if (this._listAutomationPeerHelper.HasRecordGroups == false)
			{
				if (this.GetPattern(PatternInterface.Table) != null)
					return AutomationControlType.DataGrid;
			}

			return AutomationControlType.List;
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
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			Rect rect = new Rect();

			if (this.IsRootLevel)
			{
				try
				{
					Point pt = Utilities.PointToScreenSafe(this._recordListControl, new System.Windows.Point());
					rect = new Rect(pt.X, pt.Y, Utilities.ConvertFromLogicalPixels(_recordListControl.ActualWidth), Utilities.ConvertFromLogicalPixels(_recordListControl.ActualHeight));
				}
				catch
				{
				}
			}
			else
			{
				HeaderAutomationPeer headerPeer = GetPattern(PatternInterface.Table) != null ? _listAutomationPeerHelper.HeaderPeer : null;
				rect = RecordListGroup.GetVisibleBounds(_viewableRecordCollection, headerPeer, true) ?? new Rect();
			}

			return rect;
		}
			#endregion //GetBoundingRectangleCore

			#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the object that is associated with this <see cref="AutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			// AS 4/13/11 TFS72669
			AutomationPeerHelper.RemovePendingChildrenInvalidation(this);

			// AS 7/26/11 TFS80926
			if (this.ProcessVisualTreeOnly)
			{
				_itemPeers.Clear();

				if (_isRootLevel)
					return new FrameworkElementAutomationPeer(_recordListControl).GetChildren();
				else
					return null;
			}

			if (this._listAutomationPeerHelper.HasRecordGroups)
			{
				List<AutomationPeer> baseItems = null;
				if (this.IsRootLevel)
					baseItems = this.GetChildrenHelperBaseItems();

				List<RecordListGroup> groups = this._listAutomationPeerHelper.GetRecordListGroups();
				Converter<RecordListGroup, AutomationPeer> converter = new Converter<RecordListGroup, AutomationPeer>(delegate(RecordListGroup group)
				{
					return group.AutomationPeer;
				});

				List<AutomationPeer> groupPeers = groups.ConvertAll<AutomationPeer>(converter);

				if (null != baseItems)
					groupPeers.AddRange(baseItems);

				return groupPeers;
			}

			List<AutomationPeer> children = this.GetChildrenHelper();

			// lastly add the headers
			if (this.CouldSupportGridPattern())
			{
				AutomationPeer header = this._listAutomationPeerHelper.HeaderPeer;

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
			return "ViewableRecordCollection";
		}

			#endregion //GetClassNameCore

			#region GetClickablePointCore
		/// <summary>
		/// Returns the <see cref="Point"/> that represents the clickable space for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The point that represents the clickable space on the element</returns>
		protected override System.Windows.Point GetClickablePointCore()
		{
			// AS 9/1/09
			// Only return a clickable point for the root element.
			//
			//System.Windows.Point p = Utilities.PointToScreenSafe(this._recordListControl, new System.Windows.Point(0, 0));
			//return p;
			Point pt = new Point(double.NaN, double.NaN);

			if (this.IsRootLevel)
			{
				try
				{
					// uielementautomationpeer uses the midpt of the control so we should use that as well
					pt = Utilities.PointToScreenSafe(_recordListControl, new System.Windows.Point(_recordListControl.ActualWidth * 0.5, _recordListControl.ActualHeight * 0.5));
				}
				catch
				{
				}
			}

			return pt;
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
			// AS 1/25/10
			// We need to have a unique peer (at least with respect to the sibling collections of a 
			// record) for the record collection so they can be identified in ui testing.
			// 
			//return string.Empty;
			return "Records";
		}
			#endregion //GetNameCore

			#region GetOrientationCore
		/// <summary>
		/// Returns the value that indicates the direction in which the <see cref="UIElement"/> is laid out.
		/// </summary>
		/// <returns>The direction of the <see cref="UIElement"/> or <b>AutomationOrientation.None</b> if no direction is specified</returns>
		protected override AutomationOrientation GetOrientationCore()
		{
			DataPresenterBase dp = this._recordListControl.DataPresenter;
			if (dp != null)
			{
				return dp.CurrentViewInternal.LogicalOrientation == System.Windows.Controls.Orientation.Vertical ?
																			AutomationOrientation.Vertical :
																			AutomationOrientation.Horizontal;
			}

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
			if (patternInterface == PatternInterface.Scroll)
			{
				RecordListControl list = this._recordListControl;
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
			if (this.ProcessVisualTreeOnly)
				return null;

			if (patternInterface == PatternInterface.Selection)
			{
				if (this._viewableRecordCollection.Count < 1 ||
					this._viewableRecordCollection[0] is ExpandableFieldRecord)
					return null;

				return this;
			}

			if (patternInterface == PatternInterface.Grid || patternInterface == PatternInterface.Table)
			{
				if (false == this.CouldSupportGridPattern())
					return null;

				if (this._listAutomationPeerHelper.HasRecordGroups)
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
			return this._recordListControl.IsKeyboardFocused;
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
			// JM 08-26-09 TFS21509 - Need to return true here so this peer shows up in the tree within the Visual UI Automation Verify tool.
			//return false;
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
			return this._recordListControl.IsEnabled;
		}
			#endregion //IsEnabledCore

			#region IsKeyboardFocusableCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can accept keyboard focus.
		/// </summary>
		/// <returns><b>True</b> if the element can accept keyboard focus; otherwise, <b>false</b>.</returns>
		protected override bool IsKeyboardFocusableCore()
		{
			return this._recordListControl.Focusable;
		}
			#endregion //IsKeyboardFocusableCore

			#region IsOffscreenCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is off the screen.
		/// </summary>
		/// <returns><b>True</b> if the element is off the screen; otherwise, <b>false</b>.</returns>
		protected override bool IsOffscreenCore()
		{
			return false;
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

			#region Private Properties

				#region IsHorizontalRowLayout
		private bool IsHorizontalRowLayout
		{
			get
			{
				DataPresenterBase dp = this._recordListControl.DataPresenter;

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
				
				#region IsRootLevel
		private bool IsRootLevel
		{
			get { return this._isRootLevel; }
		}
				#endregion //IsRootLevel

				// AS 7/26/11 TFS80926
				#region ProcessVisualTreeOnly
		private bool ProcessVisualTreeOnly
		{
			get { return DataPresenterBaseAutomationPeer.ProcessVisualTreeOnly; }
		}
				#endregion //ProcessVisualTreeOnly

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region CouldSupportGridPattern
		private bool CouldSupportGridPattern()
		{
			RecordCollectionBase rcb = this._viewableRecordCollection.RecordCollection;
			if (rcb != null)
			{
				// AS 8/28/09 TFS21509
				// Moved logic for evaluating the type into a helper method.
				//
				if (ListAutomationPeerHelper.CouldSupportGridPattern(rcb.RecordsType))
					return _listAutomationPeerHelper.IsHomogenousCollection();
			}

			return false;
		}
				#endregion //CouldSupportGridPattern

				#region CreateAutomationPeer
		private RecordAutomationPeer CreateAutomationPeer(Record record)
		{
			if (null == record)
				throw new ArgumentNullException("record");

			return ListAutomationPeerHelper.CreateRecordAutomationPeer(record, this, this);
		}
				#endregion //CreateAutomationPeer

				#region GetChildrenHelper


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private List<AutomationPeer> GetChildrenHelper()
		{
			List<AutomationPeer> baseItems	= this.GetChildrenHelperBaseItems();
			List<AutomationPeer> children	= new List<AutomationPeer>();

			Debug.Assert(this._recordListControl.IsGrouping == false, "The automation object is not currently set up to handle grouping!");

			// store the peers and reuse them
			Dictionary<object, RecycleableItemAutomationPeer> oldItemPeers = this._itemPeers;
			this._itemPeers = new Dictionary<object, RecycleableItemAutomationPeer>();

			foreach (object item in this._viewableRecordCollection)
			{
				if (null != item)
				{
					RecycleableItemAutomationPeer peer;

					// try to reuse the peers and if we don't have one then create a new one
					if (false == oldItemPeers.TryGetValue(item, out peer))
					{
						
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

						peer = this.CreateAutomationPeer((Record)item);
					}

					if (peer != null)
					{
						AutomationPeer wrapperPeer = ((IListAutomationPeer)this).GetUnderlyingPeer(item);

						// any events raised for the wrapped peer should raise
						// the events for the record automation peer since that is
						// what the clients will have a reference to
						if (null != wrapperPeer)
							wrapperPeer.EventsSource = peer;
					}

					children.Add(peer);
					this._itemPeers[item] = peer;
				}
			}

			if (null != baseItems)
				children.AddRange(baseItems);

			Debug.WriteLineIf(DebugGetChildren, string.Format("Returning {0} Peers from ViewableRecordCollectionAutomationPeer.GetChildrenHelper.  IsRootLevel = {1}", new object[] { children.Count.ToString(), this.IsRootLevel.ToString() }));
			return children;
		}
				#endregion //GetChildrenHelper

				#region GetChildrenHelperBaseItems


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private List<AutomationPeer> GetChildrenHelperBaseItems()
		{
			// we need to get the base automation objects since 
			// there could be things other than items
			FrameworkElementAutomationPeer fep = new FrameworkElementAutomationPeer(this._recordListControl);
			List<AutomationPeer> baseItems = fep.GetChildren();

			if (null != baseItems)
			{
				for (int i = baseItems.Count - 1; i >= 0; i--)
				{
					FrameworkElementAutomationPeer frameworkBaseItemPeer = baseItems[i] as FrameworkElementAutomationPeer;

					if (null != frameworkBaseItemPeer &&
						frameworkBaseItemPeer.Owner is ScrollViewer)
					{
						List<AutomationPeer> baseItemChildren = baseItems[i].GetChildren();

						if (null != baseItemChildren)
						{
							for (int j = baseItemChildren.Count - 1; j >= 0; j--)
							{
								FrameworkElementAutomationPeer frameworkPeer = baseItemChildren[j] as FrameworkElementAutomationPeer;

								// remove any records since we will add our wrappers
								if (null != frameworkPeer)
								{
									if (frameworkPeer.Owner is System.Windows.Controls.Primitives.ScrollBar)
										baseItemChildren.RemoveAt(j);
									else if (frameworkPeer is RecordPresenterAutomationPeer)
										baseItemChildren.RemoveAt(j);
								}
							}
						}

						// remove the scroll viewer
						baseItems.RemoveAt(i);

						// promote the children
						if (null != baseItemChildren)
							baseItems.AddRange(baseItemChildren);
					}
					else if (this._recordListControl.ItemContainerGenerator.ItemFromContainer(frameworkBaseItemPeer.Owner) != DependencyProperty.UnsetValue)
						baseItems.RemoveAt(i);
				}
			}

			Debug.WriteLineIf(DebugGetChildren, string.Format("Returning {0} Peers from ViewableRecordCollectionAutomationPeer.GetChildrenHelperBaseItems.  IsRootLevel = {1}", new object[] { baseItems.Count.ToString(), this.IsRootLevel.ToString() }));
			return baseItems;
		}
				#endregion //GetChildrenHelperBaseItems

				#region GetRecordCollection
		private RecordCollectionBase GetRecordCollection()
		{
			return this._viewableRecordCollection.RecordCollection;
		}
				#endregion //GetRecordCollection

				#region OnCollectionChanged
		void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// AS 7/26/11 TFS80926
			if (this.ProcessVisualTreeOnly)
				return;

			// JM TFS65805 2-14-11 - Add null check.
			if (null != this._listAutomationPeerHelper)
				this._listAutomationPeerHelper.ProcessListChange(e);
		}
				#endregion //OnCollectionChanged

			#endregion //Private Methods

			#region Internal Methods

		internal void InitializeViewableRecordsCollection(ViewableRecordCollection viewableRecordCollection)
		{
			if (this._viewableRecordCollection != null)
			{
				this._viewableRecordCollection.CollectionChanged
												-= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnCollectionChanged);
			}

			this._viewableRecordCollection = viewableRecordCollection;
			this._viewableRecordCollection.CollectionChanged
											+= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnCollectionChanged);
		}

			#endregion //Internal methods

		#endregion //Methods

		#region IListAutomationPeer Members

		DependencyObject IListAutomationPeer.ContainerFromItem(object item)
		{
			Record record = item as Record;
			if (record != null)
				return record.AssociatedRecordPresenter;

			throw new NotSupportedException("Don't know how to provide a container for the specified item!");
		}

		object IListAutomationPeer.GetPattern(PatternInterface patternInterface)
		{
			return this.GetPattern(patternInterface);
		}

		AutomationPeer IListAutomationPeer.GetUnderlyingPeer(object item)
		{
			Record record = item as Record;
			if (record != null && record.AssociatedRecordPresenter != null)
				return UIElementAutomationPeer.CreatePeerForElement(record.AssociatedRecordPresenter);

			return null;
		}

		System.Windows.Controls.Panel IListAutomationPeer.ItemsControlPanel
		{
			get 
			{ 
				if (this._recordListControl.DataPresenter	!= null)
					return this._recordListControl.DataPresenter.CurrentPanel; 
				else
					return null;
			}
		}

		UIElement IListAutomationPeer.Owner
		{
			get { return this._recordListControl; }
		}

		#endregion

		#region IRecordListAutomationPeer Members

		bool IRecordListAutomationPeer.IsHorizontalRowLayout
		{
			get { return this.IsHorizontalRowLayout; }
		}

		RecordAutomationPeer IRecordListAutomationPeer.CreateAutomationPeer(Record record)
		{
			return this.CreateAutomationPeer(record);
		}

		HeaderAutomationPeer IRecordListAutomationPeer.GetHeaderPeer(Record record)
		{
			return this._listAutomationPeerHelper.GetHeaderPeer(record);
		}

		bool IRecordListAutomationPeer.CouldSupportGridPattern()
		{
			return this.CouldSupportGridPattern();
		}

		RecordCollectionBase IRecordListAutomationPeer.GetRecordCollection()
		{
			return this.GetRecordCollection();
		}

		IList<Record> IRecordListAutomationPeer.GetRecordList()
		{
			return this._viewableRecordCollection as IList<Record>;
		}

		int IRecordListAutomationPeer.GetTableRowIndex(Cell cell)
		{
			return this._listAutomationPeerHelper.GetTableRowIndex(cell);
		}

		AutomationPeer IRecordListAutomationPeer.GetContainingGrid(Cell cell)
		{
			return this._listAutomationPeerHelper.GetContainingGrid(cell);
		}

		bool IRecordListAutomationPeer.IsRootLevel
		{
			get { return this.IsRootLevel; }
		}

		IRawElementProviderSimple IRecordListAutomationPeer.ProviderFromPeer(AutomationPeer peer)
		{
			return this.ProviderFromPeer(peer);
		}

		#endregion
	
		#region ISelectionProvider Members

	bool  ISelectionProvider.CanSelectMultiple
	{
		get { return this._listAutomationPeerHelper.ISelectionProvider_CanSelectMultiple; }
	}

	IRawElementProviderSimple[]  ISelectionProvider.GetSelection()
	{
		return this._listAutomationPeerHelper.ISelectionProvider_GetSelection();
	}

	bool  ISelectionProvider.IsSelectionRequired
	{
		get { return this._listAutomationPeerHelper.ISelectionProvider_IsSelectionRequired; }
	}

		#endregion

		#region ITableProvider Members

	IRawElementProviderSimple[]  ITableProvider.GetColumnHeaders()
	{
			return this._listAutomationPeerHelper.ITableProvider_GetColumnHeaders();
	}

	IRawElementProviderSimple[]  ITableProvider.GetRowHeaders()
	{
			return this._listAutomationPeerHelper.ITableProvider_GetRowHeaders();
	}

	System.Windows.Automation.RowOrColumnMajor  ITableProvider.RowOrColumnMajor
	{
		get { return this._listAutomationPeerHelper.ITableProvider_RowOrColumnMajor; }
	}

		#endregion

		#region IGridProvider Members

	int  IGridProvider.ColumnCount
	{
		get { return this._listAutomationPeerHelper.IGridProvider_ColumnCount; }
	}

	IRawElementProviderSimple  IGridProvider.GetItem(int row, int column)
	{
		return this._listAutomationPeerHelper.IGridProvider_GetItem(row, column);
	}

	int  IGridProvider.RowCount
	{
		get { return this._listAutomationPeerHelper.IGridProvider_RowCount; }
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