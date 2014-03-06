using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;

using Infragistics.Windows.Selection;
using Infragistics.Shared;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region CarouselViewPanel Class

	/// <summary>
	/// A <see cref="XamCarouselPanel"/> derived class for use in the <see cref="XamDataCarousel"/> and <see cref="XamDataPresenter"/>'s <see cref="CarouselView"/>.  
	/// This derived panel adds support for <see cref="XamDataPresenter"/> navigation commands, hierarchical data display and the <see cref="CarouselBreadcrumb "/> control.
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>The CarouselViewPanel is designed to be used with the <see cref="XamDataCarousel"/> and <see cref="XamDataPresenter"/> controls and is for Infragistics internal use only.  The control is not
	/// designed to be used outside of the <see cref="XamDataCarousel"/> or <see cref="XamDataPresenter"/>.  You may experience undesired results if you try to do so.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselPanel"/>
	/// <seealso cref="XamDataCarousel"/>
	/// <seealso cref="XamDataPresenter"/>
	//[Description("A XamCarouselPanel derived class used by the XamDataCarousel and XamDataPresenter's CarouselView.  This derived panel adds support for XamDataPresenter navigation commands, hierarchical data display and the CarouselBreadcrumb control.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CarouselViewPanel : XamCarouselPanel,
									 IViewPanel
	{
		#region Member Variables

		private RecordListControl					_recordListControl = null;
		private DataPresenterBase					_dataPresenter = null;
		private Stack<ExpandedRecordInfo>			_expandedRecordInfos = null;

		private RecordExpansionChildSelectionInfo	_recordExpansionChildSelectionInfo = null;

		private CarouselBreadcrumbControl			_carouselBreadcrumbControl = null;
		private bool								_adornerInitialized = false;

		private int									_totalDisappearingItemsRemaining = 0;
		private Storyboard							_tempItemsDisappearingStoryboard = null;

		private Nullable<PanelNavigationDirection>	_lastPanelNavigationDirection = null;

		private bool								_canNavigateUpToParent;

		// JM 09-11-08 [BR35333 TFS6518]
		private bool								_ignoreRecordCollapsed;

		// JM 02-06-09 TFS13616
		private FieldLayout							_fieldLayout;

		// JM 03-03-09 TFS13404
		private bool								_recordExpansionChildSelectionMenuItemWasClicked;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of the CarouselViewPanel.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>For Infragistics internal use only.  An instance of this control is automatically created by the XamDataCarousel and XamDataPresenter
		/// controls when needed.  You should never have to create an instance of this control directly.</p>
		/// </remarks>
		public CarouselViewPanel()
		{
		}

		#endregion //Constructor

		#region Constants

		private static readonly Size				DEFAULT_ITEM_SIZE = new Size(150, 100);
		private const int							DEFAULT_ITEMS_PER_PAGE = 10;
		private const int							SCROLL_SMALL_CHANGE = 1;

		#endregion //Constants

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size size = base.ArrangeOverride(finalSize);

			if (this._adornerInitialized == false)
				this.InitializeAdorner();

			return size;
		}

			#endregion //ArrangeOverride

            // AS 3/11/09 TFS11010
            // The _carouselBreadcrumbControl has this as its logical parent but isn't in 
            // our logical children collection.
            //
            #region LogicalChildren
        /// <summary>
        /// Gets an enumerator that can iterate the logical child elements of this element.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                return new MultiSourceEnumerator(base.LogicalChildren,
                    new SingleItemEnumerator(this._carouselBreadcrumbControl));
            }
        } 
            #endregion //LogicalChildren

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride(availableSize);
		}

			#endregion //MeasureOverride

			#region OnAnimatedScrollComplete

		/// <summary>
		/// Called when the <see cref="XamCarouselPanel"/> completes an animated scroll operation.
		/// </summary>
		/// <param name="newScrollPosition">The new scroll position within the list of items being displayed by the CarouselPanel</param>
		/// <seealso cref="XamCarouselPanel"/>
		protected override void OnAnimatedScrollComplete(int newScrollPosition)
		{
			if (this.ViewPanelInfo != null)
			{
				this.ViewPanelInfo.OverallScrollPosition = newScrollPosition;
				this.ViewPanelInfo.OnRecordsInViewChanged();
			}
		}

			#endregion //OnAnimatedScrollComplete	
    
			// JM 02-06-09 TFS13616
			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

			// JM 02-17-09 TFS13938 - Change logic to look for a change in the DataPresenter.DefaultFieldLayout rather than the FieldLayout associated with the records 
			//						  in our RecordListControl.  This is because our only reason for caring about a FieldLayout change here is so that we can listen to 
			//						  property changes on the new FieldLayout to detect when Grouping occurs (which can only happen on the DataPresenter.DefaultLayout).
			//FieldLayout fl = null;
			//if (this.RecordListControl				!= null	&&
			//    this.RecordListControl.Items.Count	> 0)
			//    fl = ((Record)this.RecordListControl.Items[0]).FieldLayout;

			//if (fl != this._fieldLayout)
			//{
			//    if (this._fieldLayout != null)
			//    {
			//        this._fieldLayout.PropertyChanged		-= new PropertyChangedEventHandler(OnFieldLayoutPropertyChanged);
			//        this._canNavigateUpToParent				= false;
			//        this._recordExpansionChildSelectionInfo = null;
			//        this._lastPanelNavigationDirection		= null;
			//        this._fieldLayout						= null;
			//        if (this._expandedRecordInfos != null)
			//            this._expandedRecordInfos.Clear();
			//    }

			//    if (fl != null)
			//    {
			//        this._fieldLayout					= fl;
			//        this._fieldLayout.PropertyChanged	+= new PropertyChangedEventHandler(OnFieldLayoutPropertyChanged);
			//    }
			//}
			if (this.DataPresenter != null && this.DataPresenter.DefaultFieldLayout != this._fieldLayout)
			{
				if (this._fieldLayout != null)
				{
					this._fieldLayout.PropertyChanged	-= new PropertyChangedEventHandler(OnFieldLayoutPropertyChanged);
					this._fieldLayout					= null;
				}

				if (this.DataPresenter.DefaultFieldLayout != null)
				{
					this._fieldLayout					= this.DataPresenter.DefaultFieldLayout;
					this._fieldLayout.PropertyChanged	+= new PropertyChangedEventHandler(OnFieldLayoutPropertyChanged);
				}
			}
		}

			#endregion // OnItemsChanged
			
			#region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// Short circuit any key processing by the XamCarouselPanel base class.
		}

			#endregion //OnKeyDown

			#region OnPropertyChanged

		/// <summary>
		/// Called when the value of a property changes.
		/// </summary>
		/// <param name="e">An instance of DependencyPropertyChangedEventArgs that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			// [JM 04-18-07 BR21391] Hook/unhook record collapsed event.
			if (e.Property == DataPresenterBase.DataPresenterProperty)
			{
				if (e.OldValue != null)
				{
					DataPresenterBase oldDataPresenter = e.OldValue as DataPresenterBase;
					if (oldDataPresenter != null)
						oldDataPresenter.RecordCollapsed -= new EventHandler<RecordCollapsedEventArgs>(OnRecordCollapsed);
				}

				this._dataPresenter = e.NewValue as DataPresenterBase;

				if (this._dataPresenter != null)
				{
					this._dataPresenter.RecordCollapsed += new EventHandler<RecordCollapsedEventArgs>(OnRecordCollapsed);
				}
			}
			else
			if (e.Property == DataPresenterBase.CurrentViewProperty)
			{
				CarouselView			oldView			= e.OldValue as CarouselView;
				CarouselViewSettings	oldViewSettings = null;
				if (oldView != null)
				{
					this.HookViewDotViewSettingsChangedEvent(oldView, false);
					oldViewSettings = oldView.ViewSettings;
				}
	
				if (oldViewSettings != null)
				{
					// Remove the view settings object as our logical child.
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(oldViewSettings);
					if (logicalParent == this)
						this.RemoveLogicalChild(oldViewSettings);
				}


				// Listen to property changes on the View so we can detect when its ViewSettings property
				// is updated - we will then update the ViewSettings property on our XamCarouselPanel.
				CarouselView			newView			= e.NewValue as CarouselView;
				CarouselViewSettings	newViewSettings = null;
				if (newView != null)
				{
					this.HookViewDotViewSettingsChangedEvent(newView, true);
					// JM 04-20-10 - Do not set the ViewSettings property since that steps on the binding that was setup 
					// in the HookViewDotViewSettingsChangedEvent above.
					//this.ViewSettings = newViewSettings = newView.ViewSettings;
					newViewSettings = newView.ViewSettings;
				}

				if (newViewSettings != null)
				{
					// Add the view settings object as our logical child.
					//
					// Make sure the new view settings object does not already have a logical parent.
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(newViewSettings);
					if (logicalParent != null && logicalParent != this)
					{
						this.RemoveLogicalChild(newViewSettings);
						logicalParent = LogicalTreeHelper.GetParent(newViewSettings);
					}

					if (logicalParent == null)
						this.AddLogicalChild(newViewSettings);
				}
			}
		}

			#endregion //OnPropertyChanged	
    
		#endregion //Base Class Overrides

		#region Implemented Interfaces

			#region IViewPanel Members

				#region EnsureCellIsVisible

		/// <summary>
		/// Ensures that the specified cell is visible.  If it is not, the panel should scroll in the appropriate
		/// direction to make it visible.  NOTE: This routine assumes that the record is already visible.  Call 
		/// EnsureRecordIsVisible to make the record containing the cell visible.  
		/// </summary>
		/// <param name="cell">The cell to be made visible.</param>
		/// <returns>True if cell has been made visible, false if the operation could not be completed.</returns>
		bool IViewPanel.EnsureCellIsVisible(Cell cell)
		{
			//For now just make sure that the record contaning the cell is visible.
			return ((IViewPanel)this).EnsureRecordIsVisible(cell.Record);
		}

				#endregion //EnsureCellIsVisible

				#region EnsureRecordIsVisible

		/// <summary>
		/// Ensures that the specified record is visible. If it is not, the panel should scroll in the appropriate
		/// direction to make it visible.
		/// </summary>
		/// <param name="record">The record to be made visible.</param>
		/// <returns>True if the record was made visible, false if the operation could not be completed.</returns>
		bool IViewPanel.EnsureRecordIsVisible(Record record)
		{
			IViewPanelInfo			info					= this.ViewPanelInfo;
			// JJD 3/06/07
			// Use the index in the list
			//int						recordScrollPosition	= info.GetOverallScrollPositionForRecord( record );
			int						recordScrollPosition	= this.RecordListControl.Items.IndexOf( record );
			CarouselViewSettings	viewSettings			= this.ViewSettings;

			// if the record isn't in the list
			if (recordScrollPosition < 0)
			{
				
				return false;
			}

			if (this.GetIsItemIndexVisible(recordScrollPosition, this.FirstVisibleItemIndex))
				return true;

			if (recordScrollPosition < this.FirstVisibleItemIndex)
			{
				// Check _lastPanelNavigationDirection which was set in GetNavigationTargetRecord to help us
				// figure out which direction to scroll.
				if (this._lastPanelNavigationDirection != null && this._lastPanelNavigationDirection.HasValue)
				{
					switch (this._lastPanelNavigationDirection.Value)
					{
						case PanelNavigationDirection.Below:
						case PanelNavigationDirection.Right:
						case PanelNavigationDirection.Next:
							this.SetVerticalOffset(this.VerticalOffset + 1);
							break;

						default:
							this.SetVerticalOffset(recordScrollPosition);
							break;
					}
				}
				else
					this.SetVerticalOffset(recordScrollPosition);
			}
			else
			{
				// Check _lastPanelNavigationDirection which was set in GetNavigationTargetRecord to help us
				// figure out which direction to scroll.
				if (this._lastPanelNavigationDirection != null && this._lastPanelNavigationDirection.HasValue)
				{
					switch (this._lastPanelNavigationDirection.Value)
					{
						case PanelNavigationDirection.Above:
						case PanelNavigationDirection.Left:
						case PanelNavigationDirection.Previous:
							this.SetVerticalOffset(recordScrollPosition - this.TotalItemCount);
							break;

						default:
							this.SetVerticalOffset(Math.Max(0, recordScrollPosition - base.ItemsPerPageResolved + 1));
							break;
					}
				}
				else
					this.SetVerticalOffset(Math.Max(0, recordScrollPosition - base.ItemsPerPageResolved + 1));
			}

			this._lastPanelNavigationDirection = null;

			return true;
		}

				#endregion //EnsureRecordIsVisible

				#region GetFirstDisplayedRecord

		/// <summary>
		/// Returns the first record in the list of currently displayed records that is of the specified type.
		/// </summary>
		/// <param name="recordType">The type of record to look for and return.</param>
		/// <returns>A record of the specified type or null if no record of the specified type could be found.</returns>
		Record IViewPanel.GetFirstDisplayedRecord(Type recordType)
		{
			if (this.FirstVisibleItemIndex >= 0)
				return this.ViewPanelInfo.GetRecordAtOverallScrollPosition(this.FirstVisibleItemIndex);

			return null;
		}

				#endregion //GetFirstDisplayedRecord

				#region GetFirstOverallRecord

		/// <summary>
		/// Returns the first record in the overall list of records that is of the specified type.
		/// </summary>
		/// <param name="recordType">The type of record to look for and return.</param>
		/// <returns>A record of the specified type or null if no record of the specified type could be found.</returns>
		Record IViewPanel.GetFirstOverallRecord(Type recordType)
		{
			return this.ViewPanelInfo.GetRecordAtOverallScrollPosition(0);
		}

				#endregion //GetFirstOverallRecord

				#region GetLastDisplayedRecord

		/// <summary>
		/// Returns the last record in the list of currently displayed records that is of the specified type.
		/// </summary>
		/// <param name="recordType">The type of record to look for and return.</param>
		/// <returns>A record of the specified type or null if no record of the specified type could be found.</returns>
		Record IViewPanel.GetLastDisplayedRecord(Type recordType)
		{
			if (this.FirstVisibleItemIndex >= 0)
			{
				IViewPanelInfo info = this.ViewPanelInfo;
				return info.GetRecordAtOverallScrollPosition(Math.Min(info.OverallScrollCount - 1, this.FirstVisibleItemIndex + base.ItemsPerPageResolved - 1));
			}

			return null;
		}

				#endregion //GetLastDisplayedRecord

				#region GetLastOverallRecord

		/// <summary>
		/// Returns the last record in the overall list of records that is of the specified type.
		/// </summary>
		/// <param name="recordType">The type of record to look for and return.</param>
		/// <returns>A record of the specified type or null if no record of the specified type could be found.</returns>
		Record IViewPanel.GetLastOverallRecord(Type recordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;

			int scrollCount = info.OverallScrollCount;

			if (scrollCount == 0)
				return null;

			return info.GetRecordAtOverallScrollPosition(scrollCount - 1);
		}

				#endregion //GetLastOverallRecord

				#region GetNavigationTargetRecord

		/// <summary>
		/// Returns the target of a record navigation from the specified currentRecord in the specified navigationDirection 
		/// </summary>
		/// <param name="currentRecord">The starting record for the navigation</param>
		/// <param name="navigationDirection">The direction in which to navigate</param>
		/// <param name="selectionHost">A reference to the current selection host.</param>
		/// <param name="shiftKeyDown">True if the shift key is down</param>
		/// <param name="ctlKeyDown">True if the ctl key is down</param>
		/// <param name="siblingNavigationStyle">Enumeration that specified how to deal with navigation among sibling records</param>
		/// <param name="restrictToRecordType">The Record or Record-derived type used to restrict the return record to a particular type</param>
		/// <returns>The record that is the target of the navigation</returns>
		Record IViewPanel.GetNavigationTargetRecord(Record currentRecord, PanelNavigationDirection navigationDirection, ISelectionHost selectionHost, bool shiftKeyDown, bool ctlKeyDown, PanelSiblingNavigationStyle siblingNavigationStyle, Type restrictToRecordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;

			int scrollCount								= info.OverallScrollCount;
			int currentRecordScrollPosition				= info.GetOverallScrollPositionForRecord( currentRecord );
			int candidateRecordScrollPosition			= -1;

			switch (navigationDirection)
			{
				case PanelNavigationDirection.Above:
				case PanelNavigationDirection.Left:
				case PanelNavigationDirection.Previous:
					if (this.ViewSettings.IsListContinuous == true && currentRecordScrollPosition == 0)
						candidateRecordScrollPosition = scrollCount - 1;
					else
						candidateRecordScrollPosition = Math.Max(0, currentRecordScrollPosition - 1);

					break;

				case PanelNavigationDirection.Below:
				case PanelNavigationDirection.Right:
				case PanelNavigationDirection.Next:
					if (this.ViewSettings.IsListContinuous == true && currentRecordScrollPosition == scrollCount - 1)
						candidateRecordScrollPosition = 0;
					else
						candidateRecordScrollPosition = Math.Min(scrollCount - 1, currentRecordScrollPosition + 1);

					break;
			}

			if (candidateRecordScrollPosition >= 0 &&
				candidateRecordScrollPosition != currentRecordScrollPosition)
			{
				// Get the candidate record.
				Record candidateRecord = info.GetRecordAtOverallScrollPosition(candidateRecordScrollPosition);


				// Get the selection strategy and see if the item can be navigated to.
				SelectionStrategyBase selectionStrategy = selectionHost.GetSelectionStrategyForItem(candidateRecord as ISelectableItem);

				Debug.Assert(selectionStrategy != null);
				if (selectionStrategy == null)
					throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_4" ) );


				if (selectionStrategy.CanItemBeNavigatedTo(candidateRecord, shiftKeyDown, ctlKeyDown) == true)
				{
					// If the record is not in view, save the navigation direction.  When EnsureRecordIsVisible is called
					// to bring this record into view, it will use the navigation direction to know whetehr to place the
					// record at the beginning or the end of the visible records.
					this._lastPanelNavigationDirection = new Nullable<PanelNavigationDirection>(navigationDirection);

					return candidateRecord;
				}
			}


			this._lastPanelNavigationDirection = null;

			return null;
		}

				#endregion //GetNavigationTargetRecord

				#region LayoutStyle

		/// <summary>
		/// Returns the <see cref="PanelLayoutStyle"/> of the panel.
		/// </summary>
		PanelLayoutStyle IViewPanel.LayoutStyle
		{
			get { return PanelLayoutStyle.Custom; }
		}

				#endregion //LayoutStyle

				#region OnActiveRecordChanged

		/// <summary>
		/// Notifies the panel that the active record has changed.
		/// </summary>
		/// <param name="record">The new active record.</param>
		void IViewPanel.OnActiveRecordChanged(Record record)
		{
			((IViewPanel)this).EnsureRecordIsVisible(record);
		}

				#endregion //OnActiveRecordChanged

				#region OnSelectedItemsChanged

		/// <summary>
		/// Notifies the panel that the <see cref="DataPresenterBase.SelectedItems"/> collection has changed.
		/// </summary>
		void IViewPanel.OnSelectedItemsChanged()
		{
		}

				#endregion //OnSelectedItemsChanged

				#region Scroll

		/// <summary>
		/// Scrolls the panel in the direction specified by scrollType.
		/// </summary>
		/// <param name="scrollType">The direction in which to scroll.</param>
		void IViewPanel.Scroll(PanelNavigationScrollType scrollType)
		{
			switch (scrollType)
			{
				case PanelNavigationScrollType.PageAbove:
					this.PageUp();
					break;
				case PanelNavigationScrollType.PageBelow:
					this.PageDown();
					break;
				case PanelNavigationScrollType.PageLeft:
					this.PageLeft();
					break;
				case PanelNavigationScrollType.PageRight:
					this.PageRight();
					break;
			}
		}

				#endregion //Scroll

			#endregion //IViewPanel Members

		#endregion //Implemented Interfaces

		#region Properties

			#region Public Properties

			#endregion //Public Properties

			#region Internal Properties

				#region DataPresenter

		internal DataPresenterBase DataPresenter
		{
			get
			{
				// [JM 04-18-07 BR21391]  We are now setting the this._dataPresenter field in the OnPropertyChanged for the 
				// DataPresenterBase.DataPresenterProperty which the DataPresenter sets on us when a new View is hooked up.
				//if (this._dataPresenter == null)
				//{
				//    if (this.RecordListControl != null)
				//        this._dataPresenter = this.RecordListControl.DataPresenter;
				//}

				return this._dataPresenter;
			}
		}

				#endregion //DataPresenter

				#region ExpandedRecords

		internal Stack<ExpandedRecordInfo> ExpandedRecords
		{
			get
			{
				if (this._expandedRecordInfos == null)
					this._expandedRecordInfos = new Stack<ExpandedRecordInfo>(3);

				return this._expandedRecordInfos;
			}
		}

				#endregion //ExpandedRecords

				#region RecordListControl






		internal RecordListControl RecordListControl
		{
			get
			{
				if (this._recordListControl == null)
					this._recordListControl = ItemsControl.GetItemsOwner(this) as RecordListControl;

				return this._recordListControl;
			}
		}

				#endregion //RecordListControl

				#region ViewPanelInfo

		internal IViewPanelInfo ViewPanelInfo { get { return this.DataPresenter; } }

				#endregion //ViewPanelInfo	
     
			#endregion //Internal Properties

			#region Private Properties

				#region CarouselBreadcrumbControl

		private CarouselBreadcrumbControl CarouselBreadcrumbControl
		{
			get
			{
				if (this._carouselBreadcrumbControl == null)
				{
					this._carouselBreadcrumbControl = new CarouselBreadcrumbControl();

					// Listen to the CarouselBreadcrumbControl's CarouselBreadcrumbClick event.
					this._carouselBreadcrumbControl.CarouselBreadcrumbClick += new EventHandler<CarouselBreadcrumbClickEventArgs>(OnCarouselBreadcrumbClick);

					// Add the CarouselBreadCrumControl as a logical child
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._carouselBreadcrumbControl);
					if (logicalParent == null)
						this.AddLogicalChild(this._carouselBreadcrumbControl);
				}

				return this._carouselBreadcrumbControl;
			}
		}

				#endregion //CarouselBreadcrumbControl

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal methods

				#region OnRecordExpanded

		internal void OnRecordExpanded(Record record)
		{
			Debug.Assert(record != null, "Record is null in OnRecordExpanded!");
			if (record == null)
				return;


			// JM 11-26-07 - Work Item #1238 - If a cell is in edit mode, we need to take the cell out of edit mode before proceeding.
			bool exitedEditMode = this.ExitEditModeOnCurrentActiveCell();
			if (exitedEditMode == false)
				return;


			// Determine the record to use as the binding source for ChildRecords.  If the record has only 1 expandable field
			// then use the first child record as the binding source for ChildRecords (this record will be either an
			// ExpandableFieldRecord or a GroupByRecord).  If there is more than 1 expandable field, ask the user to choose
			// which one to use as the binding source by displaying a context menu.
			Record recordToUseForChildrenBindingSource = null;
			if (record is DataRecord && record.FieldLayout.Fields.ExpandableFieldsCount == 0)	// Shouldn't happen if we're here.
				return;

			record.IsExpanded = true;
			if (record.IsExpanded == false)
				return;

			if (record is GroupByRecord)
				recordToUseForChildrenBindingSource = record;
			else
			if (record.FieldLayout.Fields.ExpandableFieldsCount == 1)
				recordToUseForChildrenBindingSource = record.ChildRecordsInternal[0];
			else
			{
				ContextMenu cm = new ContextMenu();
				MenuItem	mi = null;

				foreach (Record r in record.ChildRecordsInternal)
				{
					mi			= new MenuItem();
					mi.Header	= r.AssociatedField.Name;
					mi.Tag		= new RecordExpansionChildSelectionInfo(record, r);
					mi.Click	+= new RoutedEventHandler(OnRecordExpansionChildSelectionMenuItemClick);
					cm.Items.Add(mi);
				}

				// JM 03-03-09 TFS13404
				this._recordExpansionChildSelectionMenuItemWasClicked = false;

				cm.Closed	+= new RoutedEventHandler(OnRecordExpansionChildSelectionMenuClosed);
				cm.IsOpen	= true;
				return;	// ProcessRecordExpansion will be called in the menuitem click handler.
			}

			this.ProcessRecordExpansion(new RecordExpansionChildSelectionInfo(record, recordToUseForChildrenBindingSource));
		}

				#endregion //OnRecordExpanded

			#endregion //Internal Methods

			#region Private Methods

				// JM 11-26-07 - Work Item #1238
				#region ExitEditModeOnCurrentActiveCell







		private bool ExitEditModeOnCurrentActiveCell()
		{
			if (this.DataPresenter				!= null &&
				this.DataPresenter.ActiveCell	!= null &&
				this.DataPresenter.ActiveCell.IsInEditMode)
			{
				this.DataPresenter.ActiveCell.EndEditMode();

				// If the cell did not exit edit mode, don't continue
				if (null != this.DataPresenter.ActiveCell && this.DataPresenter.ActiveCell.IsInEditMode)
					return false;
			}

			return true;
		}

				#endregion //ExitEditModeOnCurrentActiveCell

				#region HookViewDotViewSettingsChangedEvent

		private void HookViewDotViewSettingsChangedEvent(CarouselView view, bool hook)
		{
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			if (hook)
				this.SetBinding(ViewSettingsProperty, Utilities.CreateBindingObject(CarouselView.ViewSettingsProperty, BindingMode.OneWay, view));
			else
				BindingOperations.ClearBinding(this, ViewSettingsProperty);
		}

				#endregion //HookViewDotViewSettingsChangedEvent

				#region InitializeAdorner

		private void InitializeAdorner()
		{
			if (this._adornerInitialized == true)
				return;

			// Add a BreadcrumbControl to the adorner layer.
			base.AddAdornerChildElement(this.CarouselBreadcrumbControl);

			this._adornerInitialized = true;
		}

				#endregion //InitializeAdorner	

				#region NavigateUpToParent






		private void NavigateUpToParent()
		{
			if (this._canNavigateUpToParent == false)
				return;


			// Pop the most recent expanded record info object off the stack of expanded records.
			ExpandedRecordInfo mostRecentExpandedRecordInfo = this.ExpandedRecords.Pop();
			this.CarouselBreadcrumbControl.PopBreadcrumb();


			// Save a value that we can return from our implementation of the virtual property ScrollOffsetForInitialArrangeAnimation.
			// This will reposition the list to where it was before the expansion when the rebindis done.
			this.ScrollOffsetForInitialArrangeAnimation = mostRecentExpandedRecordInfo.ScrollOffsetBeforeExpansion;


			// Establish the parent data record to use for the re-bind.
			Record parentDataRecord = null;
			// [ JM BR25648 08-13-07]
			//if (mostRecentExpandedRecordInfo.Record	!= null  &&
			//    mostRecentExpandedRecordInfo.Record.ParentCollection is DataRecordCollection)
			//    parentDataRecord = ((DataRecordCollection)mostRecentExpandedRecordInfo.Record.ParentCollection).ParentRecord;
			if (mostRecentExpandedRecordInfo.Record != null &&
				mostRecentExpandedRecordInfo.Record.ParentCollection != null)
				parentDataRecord = mostRecentExpandedRecordInfo.Record.ParentCollection.ParentRecord;


			// Rebind.
			this.ReBindRecordListControlItemsSource(parentDataRecord);


			// Set property indicating whether navigation to a parent record is still possible.
			this._canNavigateUpToParent = this.ExpandedRecords.Count > 0;
		}

				#endregion //NavigateUpToParent

				#region OnCarouselBreadcrumbClick

		private void OnCarouselBreadcrumbClick(object sender, CarouselBreadcrumbClickEventArgs e)
		{
			// JM 09-11-08 [BR35333 TFS6518] - Get the DataRecord associated with the Breadcrumb that was clicked and try
			// to collapse it.  If it can't be collapsed (i.e., the RecordCollapsing event was canceled), then don't go any further.
			// JM 02-06-09 TFS13616 - Cast to Record instead of DataRecord so e pick up GroupByRecords.
			//DataRecord dr = e.Breadcrumb.Content as DataRecord;
			Record dr = e.Breadcrumb.Content as Record;
			if (dr == null)
				return;

			Debug.Assert(dr.IsExpanded == true, "DataRecord.IsExpanded should be true but is returning false in OnCarouselBreadCrumbClick!");
			if (dr.IsExpanded == false)
				return;

			// Set a flag that we check in OnRecordCollapsed to bypass collapse processing.
			this._ignoreRecordCollapsed = true;
			try { dr.IsExpanded = false; }
			finally { this._ignoreRecordCollapsed = false; }
			if (dr.IsExpanded != false)
				return;


			// JM 11-26-07 - Work Item #1238 - If a cell is in edit mode, we need to take the cell out of edit mode before proceeding.
			bool exitedEditMode = this.ExitEditModeOnCurrentActiveCell();
			if (exitedEditMode == false)
				return;


			// Pop all Breadcrumbs up to (but not including) the bread crumb that was clicked.
			int crumbsToPop = this.CarouselBreadcrumbControl.PopBreadcrumbsUpTo(e.Breadcrumb);


			// Pop the same number of entries off of our internal ExpandedRecords stack.
			while (crumbsToPop > 0)
			{
				this.ExpandedRecords.Pop();
				crumbsToPop--;
			}

			// Navigate up to the parent.
			this.NavigateUpToParent();
		}

				#endregion //OnCarouselBreadcrumbClick

				// JM 02-06-09 TFS13616
				#region OnFieldLayoutPropertyChanged

		void OnFieldLayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "GroupByVersion")
			{
				while(this._canNavigateUpToParent)
					this.NavigateUpToParent();
			}
		}

				#endregion //OnFieldLayoutPropertyChanged

				#region OnItemsDisappearingStoryboardCompleted

		void OnItemsDisappearingStoryboardCompleted(object sender, EventArgs e)
		{
			if (this._totalDisappearingItemsRemaining > 1)
			{
				this._totalDisappearingItemsRemaining--;
				return;
			}

			this._tempItemsDisappearingStoryboard.Completed -= new EventHandler(OnItemsDisappearingStoryboardCompleted);
			this.PerformRecordExpansionRebind(this._recordExpansionChildSelectionInfo);
		}

				#endregion //OnItemsDisappearingStoryboardCompleted	
    
				#region PerformRecordExpansionRebind

		private void PerformRecordExpansionRebind(RecordExpansionChildSelectionInfo recordExpansionChildSelectionInfo)
		{
			// JM 02-07-08 BR30117
			this.ScrollOffsetForInitialArrangeAnimation = 0;


			// Re-bind the RecordListControl's ItemsSource property to the ChildRecords property of the record that was expanded.
			this.ReBindRecordListControlItemsSource(recordExpansionChildSelectionInfo.RecordToUseForChildrenBindingSource);


			// Set property indicating that navigation to a parent record is possible.
			this._canNavigateUpToParent = true;
		}

				#endregion //PerformRecordExpansionRebind

				#region OnRecordCollapsed

		private void OnRecordCollapsed(object sender, RecordCollapsedEventArgs e)
		{
			// JM 09-11-08 [BR35333 TFS6518]
			if (this._ignoreRecordCollapsed)
				return;


			// See if the record being collapsed is in the 'Breadcrumb chain'.  If so, pop all breadcrumbs up to and including the
			// record that collapsed.
			bool recordFoundInBreadcrumbChain = false;
			foreach (ExpandedRecordInfo eri in this.ExpandedRecords)
			{
				if (eri.Record == e.Record)
				{
					recordFoundInBreadcrumbChain = true;
					break;
				}
			}

			if (recordFoundInBreadcrumbChain)
			{
				bool targetRecordReached = false;
				while (targetRecordReached == false && this.ExpandedRecords.Count > 0)
				{
					Record nextRecordInStack = this.ExpandedRecords.Peek().Record;
					if (nextRecordInStack == e.Record)
						targetRecordReached = true;
					else
					{
						this.ExpandedRecords.Pop();
						this.CarouselBreadcrumbControl.PopBreadcrumb();
					}
				}

				if (targetRecordReached)
					this.NavigateUpToParent();
			}
		}

				#endregion //OnRecordCollapsed	
    
				#region OnRecordExpansionChildSelectionMenuClosed

		private void OnRecordExpansionChildSelectionMenuClosed(object sender, RoutedEventArgs e)
		{
			// Find the CarouselItem for the record that was expanded and set IsExpanded to false so that
			// the expansion indicator continues to show the '+' sign in case they closed the menu without selecting
			// anything.
			ContextMenu cm = sender as ContextMenu;
			if (cm != null)
			{
				if (cm.Items.Count > 0)
				{
					MenuItem mi = cm.Items[0] as MenuItem;
					if (mi != null)
					{
						RecordExpansionChildSelectionInfo recsi = mi.Tag as RecordExpansionChildSelectionInfo;
						if (recsi != null)
						{
							// AS 7/9/07
							//foreach (UIElement element in this.Children)
							foreach (UIElement element in this.ChildElements)
							{
								CarouselItem carouselItem = element as CarouselItem;
								if (carouselItem != null && carouselItem.Record == recsi.ExpandedRecord)
								{
									carouselItem.IsExpanded = false;

									// JM 03-03-09 TFS13404
									if (this._recordExpansionChildSelectionMenuItemWasClicked == false)
										carouselItem.Record.IsExpanded = false;

									break;
								}
							}
						}
					}
				}
			}
		}

				#endregion //OnRecordExpansionChildSelectionMenuClosed

				#region OnRecordExpansionChildSelectionMenuItemClick

		private void OnRecordExpansionChildSelectionMenuItemClick(object sender, RoutedEventArgs e)
		{
			// JM 03-03-09 TFS13404
			this._recordExpansionChildSelectionMenuItemWasClicked = true;

			MenuItem mi = sender as MenuItem;
			if (mi != null)
				this.ProcessRecordExpansion(mi.Tag as RecordExpansionChildSelectionInfo);
		}

				#endregion //OnRecordExpansionChildSelectionMenuItemClick

				#region OnViewDotViewSettingsChangedEvent
 
// AS 6/3/09 TFS18192
//        private void OnViewDotViewSettingsChangedEvent(object sender, EventArgs e)
//        {
//            CarouselViewSettings newCarouselViewSettings = ((CarouselView)DataPresenterBase.GetCurrentView(this)).GetValue(CarouselView.ViewSettingsProperty) as CarouselViewSettings;
//            this.ViewSettings = newCarouselViewSettings;
//}

				#endregion //OnViewDotViewSettingsChangedEvent

				#region ProcessRecordExpansion

		private void ProcessRecordExpansion(RecordExpansionChildSelectionInfo recordExpansionChildSelectionInfo)
		{
			// Save the record being expanded in the ExpandedRecords stack.
			this.ExpandedRecords.Push(new ExpandedRecordInfo(recordExpansionChildSelectionInfo.ExpandedRecord, this.VerticalOffset));
			this.CarouselBreadcrumbControl.PushBreadcrumb(new CarouselBreadcrumb(recordExpansionChildSelectionInfo.ExpandedRecord));


			// [JM 04-18-07 BR21391]  Reset the active record before rebinding if the active record is not the one being expanded.
			// This will be the case if the record is being expanded via the ExpandRecord command rather than via a user click.
			if (this._dataPresenter.ActiveRecord != null && this._dataPresenter.ActiveRecord != recordExpansionChildSelectionInfo.ExpandedRecord)
				this._dataPresenter.ActiveRecord = null;


			// Set a property on each of the currently visible CarouselItems that indicates they are about to disappear.
			// The style for the CarouselItem can trigger off this property setting to perform animations.
			// AS 7/9/07
			//Storyboard storyboard = (this.Children[0] as CarouselItem).ItemDisappearingStoryboard;
			IList children = this.ChildElements;
			Storyboard storyboard = (children[0] as CarouselItem).ItemDisappearingStoryboard;
			if (storyboard != null)
			{
				// Clone the storyboard so we can add a listener for the Completed event (this is necessary because
				// the storyboard is frozen and it does not allow us to add the listener - this seems like a bug)
				this._tempItemsDisappearingStoryboard			= storyboard.Clone();
				this._tempItemsDisappearingStoryboard.Completed += new EventHandler(OnItemsDisappearingStoryboardCompleted);

				// Save a reference the child selection info so we can use it to do the expansion one the
				// storyboard completes.
				this._recordExpansionChildSelectionInfo = recordExpansionChildSelectionInfo;

				int childrenCount						= children.Count;
				this._totalDisappearingItemsRemaining	= 0;

				for (int i = 0; i < childrenCount; i++)
				{
					CarouselItem carouselItem = children[i] as CarouselItem;
					if (carouselItem != null && carouselItem.ItemDisappearingStoryboard != null)
					{
						this._tempItemsDisappearingStoryboard.Begin(carouselItem);
						this._totalDisappearingItemsRemaining++;
					}
				}
			}
			else
				this.PerformRecordExpansionRebind(recordExpansionChildSelectionInfo);
		}

				#endregion //ProcessRecordExpansion

				#region ReBindRecordListControlItemsSource

		private void ReBindRecordListControlItemsSource(Record parentRecord)
		{
			Binding binding = new Binding();
			binding.Mode	= BindingMode.OneWay;

			if (parentRecord == null)
			{
				binding.Source = this.DataPresenter;
				binding.Path	= new PropertyPath("ViewableRecords");
			}
			else
			{
				binding.Source	= parentRecord;
				binding.Path	= new PropertyPath("ViewableChildRecords");
			}

			this.RecordListControl.SetBinding(ItemsControl.ItemsSourceProperty, binding);
		}

				#endregion //ReBindRecordListControlItemsSource

			#endregion //Private Methods

		#endregion //Methods

		#region Nested Classes

			#region RecordExpansionChildSelectionInfo Private Class

		private class RecordExpansionChildSelectionInfo
		{
			internal Record ExpandedRecord						= null;
			internal Record RecordToUseForChildrenBindingSource = null;

			internal RecordExpansionChildSelectionInfo(Record expandedRecord, Record recordToUseForChildrenBindingSource)
			{
				this.ExpandedRecord							= expandedRecord;
				this.RecordToUseForChildrenBindingSource	= recordToUseForChildrenBindingSource;
			}
		}

			#endregion //RecordExpansionChildSelectionInfo Private Class

			#region ExpandedRecordInfo Internal Class

		internal class ExpandedRecordInfo
		{
			#region Member Variables

			private Record				_record = null;
			private double				_scrollOffsetBeforeExpansion = 0;

			#endregion //Member Variables

			#region Constructor

			internal ExpandedRecordInfo(Record record, double scrollOffsetBeforeExpansion)
			{
				this._record						= record;
				this._scrollOffsetBeforeExpansion	= scrollOffsetBeforeExpansion;
			}

			#endregion //Constructor

			#region Properties

				#region Record

			internal Record Record
			{
				get { return this._record; }
			}

				#endregion //Record

				#region ScrollOffsetBeforeExpansion

			internal double ScrollOffsetBeforeExpansion
			{
				get { return this._scrollOffsetBeforeExpansion; }
			}

				#endregion //ScrollOffsetBeforeExpansion

			#endregion //Properties
		}

			#endregion //ParentRecordInfo Internal Class

		#endregion //Nested Classes
	}

	#endregion //CarouselViewPanel Class
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