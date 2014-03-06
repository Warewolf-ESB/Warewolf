using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.ComponentModel;
using System.Diagnostics;

using Infragistics.Windows.Controls;
using Infragistics.Windows.Selection;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Data;
using Infragistics.Windows.Scrolling;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Virtualization;
using Infragistics.Collections;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
	// JJD 1/30/07 - BR18513
	// Changed from an internal class to public which will allow us to run in partial trust 
	/// <summary>
    /// A <see cref="RecyclingItemsPanel"/> derived abtract base class used by the <see cref="DataPresenterBase"/> derived controls such as <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> to arrange <see cref="RecordPresenter"/> instances in a tabular fashion, either horizontally or vertically.  
	/// </summary>
    /// <seealso cref="XamDataGrid"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="GridViewPanelFlat"/>
    /// <seealso cref="GridViewPanelNested"/>
    //[Description("A 'VirtualizingPanel' derived class used by the 'DataPresenterBase' derived controls such as 'XamDataGrid' and 'XamDataPresenter' to arrange 'RecordPresenter' instances in a tabular fashion, either horizontally or vertically.  The GridViewPanel supports unlimited nesting to display hierarchical data and a single unified scrolling capability for all nested panels.")]
	// JJD 5/19/07
	// Optimization - derive from RecyclingItemsPanel
	//public sealed class GridViewPanel : VirtualizingPanel, 
    // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
    // Made abstract
    public abstract class GridViewPanel : RecyclingItemsPanel, 
										IScrollInfo,
										IViewPanel,
										IWeakEventListener,
										IDeferredScrollPanel
	{
		#region Member Variables

		private DataPresenterBase					_dataPresenter;
		private RecordListControl					_recordListControl;
		private ScrollData							_scrollingData;
		private GridViewSettings					_viewSettings;

        // AS 3/11/09 TFS11010
        private List<DependencyObject>              _logicalChildren;

		// AS 5/10/07 Optimization
		private bool								_isHidingRecord = false;

        // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
        private FixedFieldInfo                      _fixedFieldInfo;

		private bool								_isRecordManagerWeakEventListenerHookedUp;
		private bool								_setOffsetTriggeredByPageBack;

		// AS 7/27/09 NA 2009.2 Field Sizing
		private Vector								_lastArrangeScrollOffset;

		#endregion //Member Variables

		#region Constructor
		static GridViewPanel()
		{
			// AS 7/27/09 NA 2009.2 Field Sizing
			// The ScrollViewer ends up queueing the requests to bring elements into view and then calling 
			// MakeVisible on the scrollinfo from LayoutUpdated. However, there are times that we need to 
			// process this request synchronously because the caller needs to process synchronously.
			//
			EventManager.RegisterClassHandler(typeof(GridViewPanel), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GridViewPanel"/> class
		/// </summary>
        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        // Made ctor internal
        internal GridViewPanel()
		{
		}

		#endregion //Constructor

		#region Constants

		#endregion //Constants

		#region Base Class Overrides

			#region Properties

				#region HasLogicalOrientation
		/// <summary>
		/// Gets a value that indicates whether the Panel arranges its descendants in a single dimension.
		/// </summary>
		/// <remarks>Always returns True.</remarks>
		protected override bool HasLogicalOrientation
		{
			get { return true; }
		}

				#endregion //HasLogicalOrientation

                // AS 3/11/09 TFS11010
                #region LogicalChildren
        /// <summary>
        /// Gets an enumerator that can iterate the logical child elements of this element.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                // AS 3/11/09 TFS11010
                if (null != _logicalChildren)
                    return new MultiSourceEnumerator(base.LogicalChildren, _logicalChildren.GetEnumerator());

                return base.LogicalChildren;
            }
        } 
                #endregion //LogicalChildren

				#region LogicalOrientation

		/// <summary>
		/// The orientation of the panel
		/// </summary>
		protected override Orientation LogicalOrientation
		{
			get 
			{
				GridViewSettings viewSettings = this.ViewSettings;

				if (viewSettings == null)
					return Orientation.Vertical;

				return viewSettings.Orientation; 
			}
		}

				#endregion //LogicalOrientation

			#endregion //Properties

			#region Methods
    
				#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			return base.GetVisualChild(index);
		}

				#endregion //GetVisualChild

				// AS 5/10/07 Optimization
				// When hiding a record in the ArrangeOverride, the record will try to invalidate
				// the measure of the parent. However, since we know that the record will not be 
				// in view and have handled the measure there is no reason to cause another measure/arrange.
				//
				#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overridden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._isHidingRecord)
				return;
			
			// JJD 2/14/11 - TFS66166 - Optimization
			// Ignore records whose visibility is collapsed
			if (child.Visibility == Visibility.Collapsed)
				return;

			base.OnChildDesiredSizeChanged(child);
		} 
				#endregion //OnChildDesiredSizeChanged

				#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			// JJD 8/23/11 - TFS82978
			// If we get a Reset notification and the focus is within the panel and
			// we aren't Recycling then set the focus to the DP before we call the base implementation. 
			// The reason for this is that the framework's ItemContainerGenerator completely discards
			// all containers on a reset. This would cause focus to shift somewhere else.
			if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				if (this.IsKeyboardFocusWithin)
				{
					DataPresenterBase dp = this.DataPresenter;

					if (dp != null && dp.RecordContainerGenerationMode != ItemContainerGenerationMode.Recycle)
					{
						// JJD 10/18/11 - TFS24665 
						// instead of just focusing the dp call ToggleFocusAsync
						// which will set focus to the dp and hen asynchronously set the
						// focus back to the active cell.
						// This fixes a regression caused by the call to dp.Focus()
						// which was needed to fix TFS82978 described above
						//dp.Focus();
						dp.ToggleFocusAsync();
					}
				}
			}

			base.OnItemsChanged(sender, args);

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Move:
					// JJD 4/17/07
					// On a move get rid of the old items (using the OldPosition)
					this.RemoveRangeOfChldren(args.OldPosition, args.ItemUICount);
					break;
				case NotifyCollectionChangedAction.Remove:
					// JJD 4/17/07
					// Call the new RemoveRangeOfChldren helper method
					//if (args.Position.Index > -1)
					//    this.RemoveInternalChildRange(args.Position.Index, 1);
					this.RemoveRangeOfChldren(args.Position, args.ItemUICount);
					break;

				case NotifyCollectionChangedAction.Replace:
					// JJD 4/17/07
					// On a replace get rid of the old items
					this.RemoveRangeOfChldren(args.Position, args.ItemUICount);
					break;
			}
		}

				#endregion //OnItemsChanged

				#region OnPropertyChanged

		/// <summary>
		/// Called whena property has changed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);


			if (e.Property == DataPresenterBase.DataPresenterProperty)
			{
				this._dataPresenter = e.NewValue as DataPresenterBase;
			}
			else
			if (e.Property == GridViewPanel.ViewSettingsProperty)
			{
				// unhook from old
				if (this._viewSettings != null)
					PropertyChangedEventManager.RemoveListener(this._viewSettings, this, string.Empty);

				this._viewSettings = e.NewValue as GridViewSettings;

				// hook new change event
				if (this._viewSettings != null)
				{
					PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);
					this._viewSettings.EnumeratePropertiesWithNonDefaultValues(this.OnViewSettingsPropertyChanged);

                    // JJD 7/23/09 - NA 2009 vol 2 - Enhanced grid view
                    if ( this._dataPresenter != null )
                        this._dataPresenter.InitializeItemsPanel();
				}
			}
			else
			if (e.Property == DataPresenterBase.CurrentViewProperty)
			{
				GridViewSettings viewSettings = this.ViewSettings;

				// Unhook from old
				if (viewSettings != null)
				{
					// Remove the view settings object as our logical child.
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(viewSettings);
					if (logicalParent == this)
						this.RemoveLogicalChild(viewSettings);
				}


				// Bind our ViewSettings property to the View's ViewSettings property.
				this.SetBinding(GridViewPanel.ViewSettingsProperty, Utilities.CreateBindingObject(GridView.ViewSettingsProperty, BindingMode.OneWay, DataPresenterBase.GetCurrentView(this)));

				viewSettings = this.ViewSettings;

				// Hook new change event
				if (viewSettings != null)
				{
					// Add the view settings object as our logical child.
					//
					// Make sure the new view settings object does not already have a logical parent.
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(viewSettings);
					if (logicalParent != null && logicalParent != this)
					{
						this.RemoveLogicalChild(viewSettings);
						logicalParent = LogicalTreeHelper.GetParent(viewSettings);
					}

					if (logicalParent == null)
						this.AddLogicalChild(viewSettings);
				}
			}
		}

				#endregion //OnPropertyChanged	

			#endregion //Methods

		#endregion //Base Class Overrides

		#region Properties

            #region Public Properties

				#region IsRootPanel

        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        // Made public abstract
        /// <summary>
        /// Returns true if this is the root panel
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
        public abstract bool IsRootPanel { get; }

				#endregion //IsRootPanel

                #region RootPanel

        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        // Made public abstract
        /// <summary>
        /// Returns the root panel
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
        public abstract GridViewPanel RootPanel { get; }

                #endregion RootPanel

            #endregion //Public Properties	

			#region Internal Properties

                // JJD 5/27/08 - BR32292 - added
                #region CanRecordsBeScrolled

        internal bool CanRecordsBeScrolled
        {
            get
            {
                if (!this.IsRootPanelInInfiniteContainer)
                    return true;

                // JJD 3/24/10 - TFS28905
                // If we have scrolled down off the first record then return true
                // since we need to allow the user to scroll back up
                if (this.EffectiveScrollPosition > 0)
                   return true;

                double extentDesired;
                double extentInInfiniteContainer;

                if (this.LogicalOrientation == Orientation.Vertical)
                {
                    extentInInfiniteContainer = this.HeightInfiniteContainersResolved;
                    extentDesired = this.DesiredSize.Height;
                }
                else
                {
                    extentInInfiniteContainer = this.WidthInfiniteContainersResolved;
                    extentDesired = this.DesiredSize.Width;
                }

                return !(double.IsPositiveInfinity(extentInInfiniteContainer) ||
                                                extentDesired < extentInInfiniteContainer);
            }
        }

                #endregion //CanRecordsBeScrolled	

				#region CleanupThreshhold

		internal int CleanupThreshhold
		{
			get { return 10; }
		}

				#endregion //CleanupThreshhold

				#region DataPresenter

		internal DataPresenterBase DataPresenter
		{
			get
			{
				return this._dataPresenter;
			}
		}

				#endregion //DataPresenter

				#region DisplayNestedContent






		internal static readonly DependencyProperty DisplayNestedContentProperty = DependencyProperty.Register("DisplayNestedContent",
			typeof(bool), typeof(GridViewPanel), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));







		internal bool DisplayNestedContent
		{
			get
			{
				return (bool)this.GetValue(GridViewPanel.DisplayNestedContentProperty);
			}
			set
			{
				this.SetValue(GridViewPanel.DisplayNestedContentProperty, value);
			}
		}

				#endregion //DisplayNestedContent
    
				#region EffectiveScrollPosition

        internal abstract int EffectiveScrollPosition { get; set; }

				#endregion EffectiveScrollPosition

				#region FirstScrollableRecord

        internal abstract Record FirstScrollableRecord { get; }

				#endregion //FirstScrollableRecord	

                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
                #region FixedFieldInfo
        internal FixedFieldInfo FixedFieldInfo
        {
            get 
            {
                if (null == _fixedFieldInfo)
                {
                    if (this.IsRootPanel)
                    {
                        if (null != _dataPresenter && _dataPresenter.IsFixedFieldsSupportedResolved)
                            _fixedFieldInfo = new FixedFieldInfo();
                    }
                }

                return _fixedFieldInfo;
            }
        } 
                #endregion //FixedFieldInfo
    
				#region HeightInfiniteContainersResolved

		internal double HeightInfiniteContainersResolved
		{
			get
			{
				GridViewSettings viewSettings = this.ViewSettings;

				if (viewSettings != null)
				{
					double value = viewSettings.HeightInInfiniteContainers;

					if (!double.IsNaN(value))
						return value;
				}

				// JJD 2/21/07 - BR20212
				// Use our SafeSystemInformation properties
				return SafeSystemInformation.VirtualScreenHeight;
			}
		}

				#endregion //HeightInfiniteContainersResolved	

                #region IsHidingRecord

        internal bool IsHidingRecord
        {
            get { return this._isHidingRecord; }
            set { this._isHidingRecord = value; }
        }

                #endregion //IsHidingRecord	

                #region IsRootPanelInInfiniteContainer

        internal abstract bool IsRootPanelInInfiniteContainer { get; }

                #endregion //IsRootPanelInInfiniteContainer	
        
				#region IsScrolling

		internal bool IsScrolling
		{
			get { return this.IsRootPanel  &&  this.ScrollingData._scrollOwner != null; }
		}

				#endregion //IsScrolling
    
				#region MouseWheelScrollMultiplier

		internal int MouseWheelScrollMultiplier
		{
			get { return SystemParameters.WheelScrollLines; }
		}

				#endregion //MouseWheelScrollMultiplier

				#region RecordAtEffectiveScrollPosition

		internal Record RecordAtEffectiveScrollPosition
		{
			get
			{
				int scrollPosition = this.EffectiveScrollPosition;

				return this.ViewPanelInfo.GetRecordAtOverallScrollPosition(scrollPosition);
			}
		}

				#endregion RecordAtEffectiveScrollPosition

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

                // JJD 1/27/09 - added
                #region RootPanelTopFixedOffset

        internal abstract int RootPanelTopFixedOffset { get; }

                #endregion //RootPanelTopFixedOffset	
        
                #region SetOffsetTriggeredByPageBack

        internal bool SetOffsetTriggeredByPageBack
        {
            get { return this._setOffsetTriggeredByPageBack; }
            set { this._setOffsetTriggeredByPageBack = value; }
        }

                #endregion //SetOffsetTriggeredByPageBack	
    
				#region ScrollingData

		internal ScrollData ScrollingData
		{
			get
			{
				if (this._scrollingData == null)
					this._scrollingData = new ScrollData();

				return this._scrollingData;
			}
		}

				#endregion //#region ScrollingData

                #region ScrollPositionOfLastVisibleRecord

        internal abstract int ScrollPositionOfLastVisibleRecord { get; }

                #endregion //ScrollPositionOfLastVisibleRecord	
    
				#region ScrollSmallChangeInNonOrientationDimension







		internal double ScrollSmallChangeInNonOrientationDimension
		{
			get { return 20; }
		}

				#endregion //ScrollSmallChangeInNonOrientationDimension

				#region ScrollSmallChangeInOrientationDimension







		internal double ScrollSmallChangeInOrientationDimension
		{
			get	{ return 1;	}
		}

				#endregion //ScrollSmallChangeInOrientationDimension

				#region ShowLastItemAtBottomOfVisibleArea

//#if DEBUG
//        /// <summary>
//        /// Identifies the <see cref="ShowLastItemAtBottomOfVisibleArea"/> dependency property
//        /// </summary>
//#endif
//        internal static readonly DependencyProperty ShowLastItemAtBottomOfVisibleAreaProperty = DependencyProperty.Register("ShowLastItemAtBottomOfVisibleArea",
//            typeof(bool), typeof(GridViewPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

//#if DEBUG
//        /// <summary>
//        /// Returns/sets whether the last item in the associated list is displayed at the top of the visible area or at the bottom.
//        /// </summary>
//        /// <seealso cref="ShowLastItemAtBottomOfVisibleAreaProperty"/>
//#endif
//        internal bool ShowLastItemAtBottomOfVisibleArea
//        {
//            get	{ return (bool)this.GetValue(GridViewPanel.ShowLastItemAtBottomOfVisibleAreaProperty); }
//            set	{ this.SetValue(GridViewPanel.ShowLastItemAtBottomOfVisibleAreaProperty, value); }
//        }

				#endregion //ShowLastItemAtBottomOfVisibleArea

				#region TotalGeneratedChildrenCount

		internal int TotalGeneratedChildrenCount
		{
			// JJD 5/31/07
			// Return the CountOfActiveContainers
			//get { return (base.IsItemsHost ? base.InternalChildren.Count : 0); }
			// AS 7/9/07
			//get { return (base.IsItemsHost ? this.Children.Count : 0); }
			get { return (base.IsItemsHost ? this.ChildElements.Count : 0); }
		}

				#endregion //TotalGeneratedChildrenCount

				#region ViewPanelInfo

		internal IViewPanelInfo ViewPanelInfo { get { return this.DataPresenter; } }

				#endregion //ViewPanelInfo	

				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ViewSettingsProperty = DependencyProperty.Register("ViewSettings",
			typeof(GridViewSettings), typeof(GridViewPanel), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
			if (value == null)
				return new GridViewSettings();

			return value;
		}

		/// <summary>
		/// Returns/set the <see cref="CarouselViewSettings"/> object for this <see cref="XamCarouselPanel"/>.
		/// </summary>
		/// <seealso cref="ViewSettingsProperty"/>
		//[Description("Returns/set the CarouselViewSettings object for this XamCarouselPanel.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public GridViewSettings ViewSettings
		{
			get
			{
				//if (this._viewSettings == null)
				//{
				//    this._viewSettings					= new GridViewSettings();
				//    PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);
				//}

				//return this._viewSettings;
				return this.GetValue(GridViewPanel.ViewSettingsProperty) as GridViewSettings;
			}
			set
			{
				this.SetValue(GridViewPanel.ViewSettingsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (GridViewSettings)GridViewPanel.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ViewSettings = (GridViewSettings)GridViewPanel.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

				#endregion //ViewSettings

				#region WidthInfiniteContainersResolved

		internal double WidthInfiniteContainersResolved
		{
			get
			{
				GridViewSettings viewSettings = this.ViewSettings;

				if (viewSettings != null)
				{
					double value = viewSettings.WidthInInfiniteContainers;

					if (!double.IsNaN(value))
						return value;
				}

				// JJD 2/21/07 - BR20212
				// Use our SafeSystemInformation properties
				return SafeSystemInformation.VirtualScreenWidth;
			}
		}

				#endregion //WidthInfiniteContainersResolved	

			#endregion //Internal Properties

            #region Private Properties
    
            #endregion //Private Properties	
    
		#endregion //Properties

		#region Methods

			#region Private Methods
        
				// 5/21/07 - Optimization
				// Derive from RecyclingItemsPanel 
				#region Obsolete code

//                #region CleanupUnusedGeneratedElements

//#if DEBUG
//        /// <summary>
//        /// Causes previously generated elements that are no longer in view to be 'un-generated'
//        /// and removed from our visual children collection.
//        /// </summary>
//#endif
//        private void CleanupUnusedGeneratedElements()
//        {
//            UIElementCollection internalChildren = base.InternalChildren;
//            if (this.TotalGeneratedChildrenCount < this.CleanupThreshhold)
//                return;


//            IItemContainerGenerator generator = this.GetGenerator();
//            int indexofFirstItemToGenerate = Math.Max(0, this._firstVisualScrollableItemIndex);
//            int indexofLastItemToGenerate = Math.Min(this.TotalItems - 1, (this._firstVisualScrollableItemIndex + this._numberOfScrollableRecordsToLayout - 1));

//            for (int currentChildIndex = 0; currentChildIndex < this.TotalGeneratedChildrenCount; currentChildIndex++)
//            {
//                if (this.GetIsIndexOutOfRange(this.GetGeneratedIndexFromChildIndex(currentChildIndex), indexofFirstItemToGenerate, indexofLastItemToGenerate))
//                {
//                    // Call CleanUp on the generated element. 
//                    RecordPresenter rp = internalChildren[currentChildIndex] as RecordPresenter;
//                    if (rp != null)
//                    {
//                        Record record = rp.Record;

//                        // JJD 4/17/07
//                        // We can have a null record if the last record's Visibility was set to
//                        // 'Collapsed' in its InitializeRecord event handler. In this case we want to
//                        // cleanuo this record presenter
//                        //if (record == null)
//                        //    continue;

//                        DataRecord dr = record as DataRecord;

//                        if (dr == null || !dr.IsDeleted)
//                        {
//                            if (rp.IsKeyboardFocusWithin)
//                                continue;

//                            // JJD 4/17/07
//                            // Make sure that record is not null
//                            if (record != null)
//                            {
//                                if (record.IsFixed || record.IsSpecialRecord)
//                                    continue;
//                            }
//                        }

//                        rp.CleanUp();
//                    }


//                    generator.Remove(new GeneratorPosition(currentChildIndex, 0), 1);
//                    this.RemoveInternalChildRange(currentChildIndex, 1);

//                    if (currentChildIndex < this._firstVisualScrollableItemIndex)
//                        this._firstVisualScrollableItemIndex--;

//                    if (currentChildIndex < this._firstVisualTopFixedItemIndex)
//                        this._firstVisualTopFixedItemIndex--;

//                    if (currentChildIndex < this._firstVisualBottomFixedItemIndex)
//                        this._firstVisualBottomFixedItemIndex--;

//                    currentChildIndex--;
//                }
//            }
//        }

//        #endregion //CleanupUnusedGeneratedElements

				#endregion //Obsolete code	
    
				#region EnsureIntInRange

		private int EnsureIntInRange(int value, int minValue, int maxValue)
		{
			return Math.Max(Math.Min(maxValue, value), minValue);
		}

				#endregion //EnsureIntInRange

				#region GetDescendantDataRecord

		// JJD 3/06/07 - BR20856 added
		private DataRecord GetDescendantDataRecord(Record parent, bool getNext)
		{
			// See if the parent record can expand
			if (parent.CanExpand)
			{
				// JJD 09/22/11  - TFS84708 - Optimization
				// Use ViewableChildRecordsIfNeeded instead
				//ViewableRecordCollection vrc = parent.ViewableChildRecords;
				ViewableRecordCollection vrc = parent.ViewableChildRecordsIfNeeded;

				int count = vrc != null ? vrc.Count : 0;

				if (count > 0)
				{
					Record childRecord;
					int startIndex;
					int endIndex;
					int increment;

					if (getNext)
					{
						startIndex = 0;
						endIndex = count - 1;
						increment = 1;
					}
					else
					{
						startIndex = count - 1;
						endIndex = 0;
						increment = -1;
					}

					// walk over the records looking for the 1st or last DatRecord
					for (int i = startIndex;
						getNext ? i < count : i >= 0;
						i += increment)
					{
						childRecord = vrc[i];

						// JJD 10/26/11 - TFS91364 
						// Ignore HeaderReords 
						//if (childRecord is DataRecord)
						if (childRecord is DataRecord &&
							!(childRecord is HeaderRecord))
							return childRecord as DataRecord;

						// since this collection doesn't contain DataRecords
						// call this routine recursively to check for descendant DataRecords
						childRecord = this.GetDescendantDataRecord(childRecord, getNext);

						if (childRecord != null)
							return childRecord as DataRecord;
					}

				}
			}

			return null;
		}

				#endregion //GetDescendantDataRecord	

				#region GetEffectiveGeneratedCount

		private int GetEffectiveGeneratedCount(UIElementCollection children)
		{
			if (base.IsItemsHost == false)
				return 0;
			else
				return children.Count;
		}

				#endregion //GetEffectiveGeneratedCount

				#region GetNavigationTargetRecord

		private Record GetNavigationTargetRecord(PanelNavigationDirection navigationDirection, Record currentRecord, PanelSiblingNavigationStyle siblingNavigationStyle, Type restrictToRecordType)
		{
			// Delegate to the root panel if we are not the root.
			if (this.IsRootPanel == false)
				return this.RootPanel.GetNavigationTargetRecord(navigationDirection, currentRecord, siblingNavigationStyle, restrictToRecordType);

			GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return null;

			// Setup.
			DataPresenterBase		dp						= this.DataPresenter;
			IViewPanelInfo			info					= this.ViewPanelInfo;
			int						scrollCount				= info.OverallScrollCount;
			bool					orientationIsVertical	= (viewSettings.Orientation == Orientation.Vertical);
			bool					getPrevious				= false;
			bool					getNext					= false;


			// Establish whether we are looking for the next record or the previous record.
			switch (navigationDirection)
			{
				case PanelNavigationDirection.Above:
					if (orientationIsVertical)
						getPrevious = true;
					else
						return null;

					break;
				case PanelNavigationDirection.Below:
					if (orientationIsVertical)
						getNext = true;
					else
						return null;

					break;
				case PanelNavigationDirection.Left:
					if (orientationIsVertical)
						return null;
					else
						getPrevious = true;

					break;
				case PanelNavigationDirection.Right:
					if (orientationIsVertical)
						return null;
					else
						getNext = true;

					break;
				case PanelNavigationDirection.Next:
					getNext = true;

					break;
				case PanelNavigationDirection.Previous:
					getPrevious = true;

					break;
			}



			// Look for the previous/next record in the overall list or the sibling list.
			Record fromRecord	= currentRecord;
			Record targetRecord	= null;

			while (targetRecord == null)
			{
				if (siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsAndWrap ||
					siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsNoWrap)
				{
					// Determine the 'from record's' position in the overall list.
					int fromRecordScrollPosition = info.GetOverallScrollPositionForRecord( fromRecord );

					if (fromRecordScrollPosition == -1)
						return null;

					if ((getPrevious == true && fromRecordScrollPosition < 1))
					{
						if (siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsNoWrap)
							return null;
						else
							targetRecord = info.GetRecordAtOverallScrollPosition(scrollCount - 1);
					}
                    else // JJD 1/9/09 - added else
                    if ((getNext == true && fromRecordScrollPosition > scrollCount - 2))
                    {
                        if (siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsNoWrap)
                            return null;
                        else
                            targetRecord = info.GetRecordAtOverallScrollPosition(0);
                    }
                    else // JJD 1/9/09 - added else
                    {
                        // Return the previous/next record in the overall list.
                        if (getPrevious == true)
                            targetRecord = info.GetRecordAtOverallScrollPosition(fromRecordScrollPosition - 1);
                        else
                            if (getNext == true)
                                targetRecord = info.GetRecordAtOverallScrollPosition(fromRecordScrollPosition + 1);
                    }
				}
				else
				{
					// Determine the 'from record's' position among its siblings.

                    // JJD 12/18/09 - TFS25279
                    // Use the parent collection's ViewableRecord's collection instead to make
                    // sure we don't include any filtered out records
					//IList					fromRecordSiblings				= fromRecord.ParentCollection;
					ViewableRecordCollection fromRecordSiblings				= fromRecord.ParentCollection.ViewableRecords;
					int						fromRecordIndexAmongSiblings	= fromRecordSiblings.IndexOf(fromRecord);

					if (fromRecordIndexAmongSiblings == -1)
						return null;

					if ((getPrevious == true && fromRecordIndexAmongSiblings < 1))
					{
						if (siblingNavigationStyle == PanelSiblingNavigationStyle.StayWithinParentNoWrap)
							return null;
						else
							targetRecord = fromRecordSiblings[fromRecordSiblings.Count - 1] as Record;
					}
                    else // JJD 1/9/09 - added else
                    if ((getNext == true && fromRecordIndexAmongSiblings > fromRecordSiblings.Count - 2))
                    {
                        if (siblingNavigationStyle == PanelSiblingNavigationStyle.StayWithinParentNoWrap)
                            return null;
                        else
                            targetRecord = fromRecordSiblings[0] as Record;
                    }
                    else // JJD 1/9/09 - added else
                    {
                        // Return the previous/next record in the overall list.
                        if (getPrevious == true)
                            targetRecord = fromRecordSiblings[fromRecordIndexAmongSiblings - 1] as Record;
                        else
                            if (getNext == true)
                                targetRecord = fromRecordSiblings[fromRecordIndexAmongSiblings + 1] as Record;
                    }
				}


				// Make sure the target record we found matches the restrictToRecordType.
				// JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (targetRecord == null ||
				//    targetRecord.GetType() == restrictToRecordType  ||
				//    targetRecord.GetType().IsSubclassOf(restrictToRecordType))
				if (targetRecord == null ||
					restrictToRecordType.IsAssignableFrom(targetRecord.GetType()))
					return targetRecord;


				// Since the target record does not match the restrictToRecordType set our 'from record'
				// to the target record and try again to find a target record of the restrictToRecordType.
				fromRecord		= targetRecord;
				targetRecord	= null;

				// [BR20828] 3-1-07
				// If we are looking for a record of type DataRecord and the new 'from' record is a GroupByRecord
				// or an ExpandableFieldRecord, and the reord is not expanded, expand it now so we can get to its
				// child data records.
				if (restrictToRecordType == typeof(DataRecord)  &&	(fromRecord.GetType() == typeof(GroupByRecord)	||
																	 fromRecord.GetType() == typeof(ExpandableFieldRecord)))
				{
					// JJD 3/06/07 - BR20856
					// Get a descendant DataRecord if the fromRecord is not ant ancestor of the passed in record
					if (!fromRecord.IsAncestorOf(currentRecord))
					{
						DataRecord descendantDataRecord = this.GetDescendantDataRecord(fromRecord, getNext);

						if (descendantDataRecord != null)
						{
							descendantDataRecord.ParentRecord.IsExpanded = true;
							return descendantDataRecord;
						}
					}
				}

			}

			return null;
		}

				#endregion //GetNavigationTargetRecord

				#region Old code - commented out

		//                #region GetScrollIndexOfLastVisibleRecord

		//#if DEBUG
		//        /// <summary>
		//        /// Starts at the top record and iterates through successive records getting each record's
		//        /// AssociatedRecordPresenter and checking to see if the RecordPresenter's top or left (depending 
		//        /// on our orientation) is within the bounds of the RootPanel.  If so a count is incremented.
		//        /// When the first out-of-bounds record is encountered (or when we run out of records) the count
		//        /// is added to the TopRecord's scrollindex and the result returned.
		//        /// </summary>
		//        /// <returns></returns>
		//#endif
		//        private int GetScrollIndexOfLastVisibleRecord()
		//        {
		//            GridViewSettings viewSettings = this.ViewSettings;

		//            if (viewSettings == null)
		//                return 0;

		//            //IViewPanelInfo info							= this.ViewPanelInfo;
		//            //bool		orientationIsVertical			= (viewSettings.Orientation == Orientation.Vertical);
		//            //Record		lastVisibleRecord				= null;
		//            //Record		currentRecord					= this.FirstScrollableRecord;
		//            //int			scrollCount						= info.OverallScrollCount;

		//            //// JJD 3/5/07
		//            //// if there are no scrollable records then return the scrollcount - 1 (which includes fixed records)
		//            //if (currentRecord == null)
		//            //    return scrollCount - 1;

		//            //DataPresenterBase dp						= this.DataPresenter;
		//            //Rect		rootPanelRect					= new Rect(new Size(this.RootPanel.ActualWidth, this.ActualHeight));
		//            //int			lastVisibleRecordScrollIndex	= info.GetOverallScrollPositionForRecord( currentRecord );

		//            //if ((this._extentUsedByFixedRecordsOnBottom + this._extentUsedByFixedRecordsOnTop) > 0)
		//            //{
		//            //    if (orientationIsVertical)
		//            //    {
		//            //        rootPanelRect.Y			+= this._extentUsedByFixedRecordsOnTop;
		//            //        rootPanelRect.Height	= Math.Max(0, rootPanelRect.Height - (this._extentUsedByFixedRecordsOnTop + this._extentUsedByFixedRecordsOnBottom));
		//            //    }
		//            //    else
		//            //    {
		//            //        rootPanelRect.X			+= this._extentUsedByFixedRecordsOnTop;
		//            //        rootPanelRect.Width		= Math.Max(0, rootPanelRect.Width - (this._extentUsedByFixedRecordsOnTop + this._extentUsedByFixedRecordsOnBottom));
		//            //    }
		//            //}


		//            //while (lastVisibleRecord == null && currentRecord != null)
		//            //{
		//            //    if (currentRecord.AssociatedRecordPresenter == null)
		//            //        break;

		//            //    Point	currentRecordTopLeft		= currentRecord.AssociatedRecordPresenter.TranslatePoint(new Point(0, 0), this.RootPanel);
		//            //    bool	currentRecordTopLeftVisible	= false;

		//            //    if (currentRecord.IsExpanded)
		//            //    {
		//            //        if (orientationIsVertical)
		//            //            currentRecordTopLeftVisible = (LocationsAreClose(currentRecordTopLeft.Y, rootPanelRect.Top)		|| currentRecordTopLeft.Y >= rootPanelRect.Top) &&
		//            //                                          (LocationsAreClose(currentRecordTopLeft.Y, rootPanelRect.Bottom)	|| currentRecordTopLeft.Y <= rootPanelRect.Bottom);
		//            //        else
		//            //            currentRecordTopLeftVisible = (LocationsAreClose(currentRecordTopLeft.X, rootPanelRect.Left)	|| currentRecordTopLeft.X >= rootPanelRect.Left) &&
		//            //                                          (LocationsAreClose(currentRecordTopLeft.X, rootPanelRect.Right)	|| currentRecordTopLeft.X <= rootPanelRect.Right);
		//            //    }
		//            //    else
		//            //    {
		//            //        Rect currentRecordRect = new Rect(currentRecordTopLeft, new Size(currentRecord.AssociatedRecordPresenter.ActualWidth,
		//            //                                                                         currentRecord.AssociatedRecordPresenter.ActualHeight));

		//            //        if (orientationIsVertical)
		//            //            currentRecordTopLeftVisible = (LocationsAreClose(currentRecordRect.Top, rootPanelRect.Top)			|| currentRecordRect.Top >= rootPanelRect.Top) &&
		//            //                                          (LocationsAreClose(currentRecordRect.Bottom, rootPanelRect.Bottom)	|| currentRecordRect.Bottom <= rootPanelRect.Bottom);
		//            //        else
		//            //            currentRecordTopLeftVisible = (LocationsAreClose(currentRecordRect.Left, rootPanelRect.Left)		|| currentRecordRect.Left >= rootPanelRect.Left) &&
		//            //                                          (LocationsAreClose(currentRecordRect.Right, rootPanelRect.Right)		|| currentRecordRect.Right <= rootPanelRect.Right);
		//            //    }

		//            //    if (currentRecordTopLeftVisible)
		//            //    {
		//            //        lastVisibleRecordScrollIndex	= info.GetOverallScrollPositionForRecord( currentRecord );

		//            //        // Make sure that the record returned by the manager for lastVisibleRecordScrollIndex + 1 is not the
		//            //        // same as the current record.
		//            //        Record oldCurrentRecord			= currentRecord;
		//            //        currentRecord					= info.GetRecordAtOverallScrollPosition(lastVisibleRecordScrollIndex + 1);
		//            //        if (currentRecord == oldCurrentRecord)
		//            //        {
		//            //            Debug.Assert(currentRecord != oldCurrentRecord, "The same Record appears twice in two consecutive scroll positions!");
		//            //            break;
		//            //        }
		//            //    }
		//            //    else
		//            //        break;
		//            //}


		//            //return lastVisibleRecordScrollIndex;
		//        }

		//                #endregion //GetScrollIndexOfLastVisibleRecord

				#endregion //Old code	

				// AS 7/27/09 NA 2009.2 Field Sizing
				#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			GridViewPanel panel = (GridViewPanel)sender;

			if (e.OriginalSource != sender && panel.IsScrolling)
			{
				// during normal processing of the request bring into view, the panel calls 
				// into IsOkToScroll. this method will take the current edit cell out of 
				// edit mode. however since a cell can be wider than the visible area we 
				// want to avoid coming out of edit mode in that case. to do this the VDRCP 
				// temporarily disables the logic that would cause the IsOkToScroll to end 
				// edit mode. however the scrollviewer normally would have caught this message 
				// and waited until the layout updated to process it (i.e. it processes it 
				// asynchronously) and therefore the flag to suppress exiting edit mode 
				// has been reset so the panel needs to handle this message itself
				Rect targetRect = e.TargetRect;

				// the MakeVisible method is not expecting an empty rect since the scrollviewer
				// doesn't ever provide one so we need to change that to an actual rect if 
				// it is empty
				if (targetRect.IsEmpty)
				{
					UIElement targetElement = e.TargetObject as UIElement;

					if (null != targetElement)
						targetRect = new Rect(targetElement.RenderSize);
					else
						targetRect = new Rect();
				}

				Visual visual = e.TargetObject as Visual;

				// AS 9/1/09
				// Found a case where we got a visual from a different hwndsource
				// when dropping down the operator dropdown in the filter cell.
				// We should ignore visuals in another element.
				//
				if (visual == null || !visual.IsDescendantOf(panel))
				    return;

				Rect rect = panel.MakeVisible(visual, targetRect);
				e.Handled = true;
				panel.BringIntoView(rect);
			}
		} 
				#endregion //OnRequestBringIntoView

				#region OnViewSettingsPropertyChanged

		void OnViewSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            // JJD 1/24/09 
            // If we haven't been initialized yet then we can ignore the change notification
            if (this._dataPresenter == null)
                return;

			switch (e.PropertyName)
			{
                // JJD 7/23/09 - NA 2009 vol 2 - Enhanced grid view
                case "UseNestedPanels":
                    this._dataPresenter.InitializeItemsPanel();

                    return;

				case "Orientation":
					{
						ScrollData scrollData = this.ScrollingData;

						// Reset extent and viewport - they will get re-calculated on the next measure.
						scrollData._extent = new Size(0, 0);
						scrollData._viewport = new Size(0, 0);

						// JJD 6/6/07
						// Added separate viewport and extent settings to control scrollbar thumb
						scrollData._extentForScrollbar = new Size(0, 0);
						scrollData._viewportForScrollbar = new Size(0, 0);
						scrollData._averageHeightOfRecords = 0;
						scrollData._highestRecordDisplayed = 0;


						// Move the offset value to the new primary scrolling dimension (i.e., the dimension along which panel
						// items are stacked)
						if (this.ViewSettings.Orientation == Orientation.Horizontal)
						{
							scrollData._offset.X = scrollData._offset.Y;
							scrollData._offset.Y = 0;

							// JJD 6/14/07
							// scroll back up to the first record
							this.SetHorizontalOffset(0);
						}
						else
						{
							scrollData._offset.Y = scrollData._offset.X;
							scrollData._offset.X = 0;

							// JJD 6/14/07
							// scroll back up to the first record
							this.SetVerticalOffset(0);
						}

                        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                        this.VerifyFixedFieldInfo();

						this.InvalidateMeasure();

						UIElement parent = this.Parent as UIElement;

						if (parent != null)
							parent.InvalidateMeasure();
					}

					break;
			}
		}
		
				#endregion ///OnViewSettingsPropertyChanged

				// 5/21/07 - Optimization
				// Derive from RecyclingItemsPanel 
				#region Obsolete code

//                #region ProcessQueuedCleanupRequest

//#if DEBUG
//        /// <summary>
//        /// Called by the Dispatcher when an asynchronous request to cleanup unused generated elements is received.
//        /// </summary>
//        /// <param name="args"></param>
//        /// <returns></returns>
//#endif
//        private object ProcessQueuedCleanupRequest(object args)
//        {
//            try
//            {
//                this.CleanupUnusedGeneratedElements();

//            }
//            finally
//            {
//                this._queuedCleanupRequest = null;
//            }

//            return null;
//        }

//                #endregion //ProcessQueuedCleanupRequest

				#endregion //Obsolete code	
        
				#region RemoveRangeOfChldren

		// JJD 4/17/07
		// Added routine for removing a range of child elements
		private void RemoveRangeOfChldren(GeneratorPosition position, int count)
		{
			if (count < 1)
				return;

			if (this.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle)
				return;

			int index = position.Index;

			if (position.Offset > 0)
				index += position.Offset;

			if (index < 0)
				return;

			int childCount = this.TotalGeneratedChildrenCount;

			if (index < childCount)
				base.RemoveInternalChildRange(index, Math.Min(count, childCount - index));
		}

				#endregion //RemoveRangeOfChldren	
                
                #region SetOffsetInternal

        internal abstract void SetOffsetInternal(double newOffset, double oldOffset, bool isSettingVerticalOffset);

                #endregion //SetOffsetInternal	

				#region SetupRecordManagerWeakEventListener

		private void SetupRecordManagerWeakEventListener(RecordManager recordManager)
		{
			if (this._isRecordManagerWeakEventListenerHookedUp == true || recordManager == null)
				return;


			// Listen to the record manager's PropertyChanged event.  Use the weak event manager so we don't get rooted.
			PropertyChangedEventManager.AddListener(recordManager, this, string.Empty);


			this._isRecordManagerWeakEventListenerHookedUp = true;
		}

				#endregion //SetupRecordManagerWeakEventListener	
        
			#endregion //Private Methods

			#region Internal Methods

				#region AddLogicalChildInternal

		internal void AddLogicalChildInternal(DependencyObject child)
		{
            // AS 3/11/09 TFS11010
            //this.AddLogicalChild(child);
            if (null != child)
            {
                if (null == this._logicalChildren)
                    this._logicalChildren = new List<DependencyObject>();

                Debug.Assert(false == this._logicalChildren.Contains(child));
                this._logicalChildren.Add(child);
                this.AddLogicalChild(child);
            }
		}

				#endregion //AddLogicalChildInternal	

				#region EnsureCellIsVisible



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal bool EnsureCellIsVisible(Cell cell)
		{
			if (this.IsRootPanel == false)
				return this.RootPanel.EnsureCellIsVisible(cell);

			Debug.Assert(cell != null);

			if (cell == null)
				return false;

			DataRecord record = cell.Record;

			Debug.Assert(record != null);

			if (record == null)
				return false;

			GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return false;

			DataRecordPresenter	dataRecordPresenter = record.AssociatedRecordPresenter as DataRecordPresenter;

			if (dataRecordPresenter == null)
			{
				this.EnsureRecordIsVisible(cell.Record);

				// JJD 3/01/07 - BR18170
				// call UpdateLayout to force the record to come into view before returning
				this.UpdateLayout();

				dataRecordPresenter = record.AssociatedRecordPresenter as DataRecordPresenter;
			}

			if (dataRecordPresenter != null)
			{
				CellValuePresenter[] cellPresenters	= dataRecordPresenter.GetChildCellValuePresenters();
				CellValuePresenter	cvp	= null;

				foreach (CellValuePresenter fc in cellPresenters)
				{
					if (fc.Field == cell.Field)
					{
						cvp = fc;
						break;
					}
				}

				if (cvp != null)
				{
                    
#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)


                    // JJD 6/11/08 - BR32431, BR31938
                    // Only call IsOkToScroll on DataPresenter if this cell
                    // is not already in edit moe. Otherwise, that implementation
                    // will end edit mode. Since it is in edit mode it must
                    // already be partially visible
                    if (!cell.IsInEditMode)
                    {
					    // check with the dp if it is ok to scroll
                        if (!((IViewPanelInfo)this.DataPresenter).IsOkToScroll())
                            return false;
                    }

                    
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

                    cvp.BringIntoView();

					return true;
				}
			}

			return false;
		}

				#endregion //EnsureCellIsVisible

				#region EnsureRecordIsVisible

        internal abstract bool EnsureRecordIsVisible(Record record);

				#endregion //EnsureRecordIsVisible
   
				#region FindSlotForNewlyRealizedRecordPresenter

		internal int FindSlotForNewlyRealizedRecordPresenter(int newItemIndex)
		{
			// JJD 5/22/07 - Optimization
			// GridViewPanel now derives from RecyclingControlPanel 
			// so use CountOfActiveContainers instead
			//UIElementCollection internalChildren = base.InternalChildren;

			//int count = internalChildren.Count;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int activeChildrenCount = this.CountOfActiveContainers;
			// AS 7/9/07
			//IList	children = this.Children;
			IList	children = this.ChildElements;
			int		count = children.Count;

			if (count == 0)
				return 0;

			IItemContainerGenerator generator = this.GetGenerator();
			for (int i = 0; i < count; i++)
			{
				int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

				if (itemIndex >= newItemIndex)
					return i;
			}

			return count;
		}

				#endregion //FindSlotForNewlyRealizedRecordPresenter	

				#region GetGeneratedIndexFromChildIndex

		internal int GetGeneratedIndexFromChildIndex(int childIndex)
		{
			return this.GetGenerator().IndexFromGeneratorPosition(new GeneratorPosition(childIndex, 0));
		}

				#endregion //GetGeneratedIndexFromChildIndex

				#region GetGeneratorPositionFromItemIndex

		internal GeneratorPosition GetGeneratorPositionFromItemIndex(int itemIndex, out int childIndex)
		{
			IItemContainerGenerator generator			= this.GetGenerator();
			GeneratorPosition		generatorPosition	= (generator != null) ? generator.GeneratorPositionFromIndex(itemIndex) :
																				new GeneratorPosition(-1, itemIndex + 1);

			childIndex = (generatorPosition.Offset == 0) ? generatorPosition.Index :
														   generatorPosition.Index + 1;

			return generatorPosition;
		}

				#endregion //GetGeneratorPositionFromItemIndex

				#region GetGenerator







		internal IItemContainerGenerator GetGenerator()
		{
			if (this.RecordListControl == null)
				throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_11"));

			// JJD 5/22/07 - Optimization
			// GridViewPanel now derives from RecyclingControlPanel 
			// so return the ActiveItemContainerGenerator
			//return this.GetGenerator(this.RecordListControl);
			return this.ActiveItemContainerGenerator;
		}

				// JJD 5/22/07 - Optimization
				#region Obsolete code

		//#if DEBUG
		//        /// <summary>
		//        /// Returns the IItemContainerGenerator for the RecordListControl that this panel is associated with.
		//        /// </summary>
		//        /// <param name="recordListControl"></param>
		//        /// <returns></returns>
		//#endif
		//        private ItemContainerGenerator GetGenerator(RecordListControl recordListControl)
		//        {
		//            return recordListControl.ItemContainerGenerator;
		//        }

				#endregion //Obsolete code	
    
				#endregion //GetGenerator	

				#region GetIsPreviousChildGenerated



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool GetIsPreviousChildGenerated(int childIndex)
		{
			GeneratorPosition generatorPosition = new GeneratorPosition(childIndex, 0);
			generatorPosition					= this.GetGenerator().GeneratorPositionFromIndex(this.GetGenerator().IndexFromGeneratorPosition(generatorPosition) - 1);

			return generatorPosition.Offset == 0;
		}

				#endregion //GetIsPreviousChildGenerated

				#region GetIsIndexOutOfRange

		internal bool GetIsIndexOutOfRange(int index, int minIndex, int maxIndex)
		{
			return index < minIndex || index > maxIndex;
		}

				#endregion //GetIsIndexOutOfRange
                
                // JJD 1/27/09 - added
                #region GetScrollOffsetFromRecord

        internal virtual int GetScrollOffsetFromRecord(Record record)
		{
			IViewPanelInfo info = this.ViewPanelInfo;

            int scrollOffset = info.GetOverallScrollPositionForRecord(record)
                                - this.RootPanelTopFixedOffset;

			return Math.Min(Math.Max(scrollOffset, 0), info.OverallScrollCount - 1);
		}

				#endregion //GetScrollOffsetFromRecord
    
				#region GetTopRecordFromScrollOffset

		internal virtual Record GetTopRecordFromScrollOffset(int scrollOffset)
		{
			IViewPanelInfo info = this.ViewPanelInfo;

            scrollOffset += this.RootPanelTopFixedOffset;

			return info.GetRecordAtOverallScrollPosition(Math.Min(Math.Max(scrollOffset, 0), info.OverallScrollCount - 1));
		}

				#endregion //GetTopRecordFromScrollOffset

				#region HorizontalPageDelta

		// JJD 3/09/07 - BR20942
		// Added HorizontalPageDelta property
		internal double HorizontalPageDelta
		{
			get
			{
				if (this.IsRootPanel == false)
					return this.RootPanel.HorizontalPageDelta;

				// JJD 6/6/07
				// Added separate viewport setting to control scrollbar
				//double delta = this.ViewportWidth;
				double delta = this.ScrollingData._viewport.Width;
				double smallChange;

				if (this.LogicalOrientation == Orientation.Vertical)
				{
                    // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
                    // We need to account for the amount of fixed area.
                    //
                    if (null != _fixedFieldInfo)
                        delta = _fixedFieldInfo.GetScrollableViewportExtent();

					smallChange = this.ScrollSmallChangeInNonOrientationDimension;
					// [JJD 06-14-07] We don't appear to need need these tweaks anymore - this may have been
					// compensated for somewhere else.
					//delta -= smallChange;
				}
				else
				{
					smallChange = this.ScrollSmallChangeInOrientationDimension;
					// [JJD 06-14-07] We don't appear to need need these tweaks anymore - this may have been
					// compensated for somewhere else.
					//delta -= smallChange;

					//// tweak the delta by 1 if we have a reasonable # of items in view to ensure
					//// the previous last record that was fully in view will now be the 1st record
					//if (delta > 2)
					//    delta--;
				}

				return Math.Max(smallChange, delta);
			}
		}

				#endregion //HorizontalPageDelta	

				#region NormalizeScrollOffset



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal int NormalizeScrollOffset(double scrollOffset, int maxNormalizedValue)
		{
			if (double.IsPositiveInfinity(scrollOffset))
				return maxNormalizedValue;
			else
			if (double.IsNegativeInfinity(scrollOffset))
				return 0;
			else
				return this.EnsureIntInRange((int)scrollOffset, 0, maxNormalizedValue);
		}

				#endregion //NormalizeScrollOffset

				// AS 7/27/09 NA 2009.2 Field Sizing
				#region OnArrangeComplete
		internal void OnArrangeComplete()
		{
			if (this.IsRootPanel)
			{
				// within the MakeVisible routine we were using the current value of the 
				// ScrollingData._offset when determining if the specified rect was in 
				// view. however the rect we are receiving could have been calculated
				// before the elements have been updated if we got another call after 
				// the last arrange but before the next arrange. since the element 
				// positioning will not be updated until the next arrange we need to continue
				// to use the old scroll offset
				_lastArrangeScrollOffset = this.ScrollingData._offset;
			}
		} 
				#endregion //OnArrangeComplete

                // JJD 8/19/09 - NA 2009 Vol 2 - Enhanced grid view
                
                #region OnDeferredRecordScrollStart

        internal virtual void OnDeferredRecordScrollStart() { }

                #endregion //OnDeferredRecordScrollStart	

				#region OnScrollInfoChange







		internal void OnScrollInfoChange()
		{
			if (this.ScrollingData._scrollOwner != null)
				this.ScrollingData._scrollOwner.InvalidateScrollInfo();
		}

				#endregion OnScrollInfoChange
        
				#region RemoveLogicalChildInternal

		internal void RemoveLogicalChildInternal(DependencyObject child)
		{
            // AS 3/11/09 TFS11010
            //this.RemoveLogicalChild(child);
            if (null != this._logicalChildren)
            {
                this._logicalChildren.Remove(child);
                this.RemoveLogicalChild(child);
            }
		}

				#endregion //RemoveLogicalChildInternal	
    
				#region SetBoundaryRecordFlags






		internal abstract void SetBoundaryRecordFlags();

				#endregion //SetBoundaryRecordFlags

				// JJD 12/8/11 - TFS97329 - added
				#region SetRecordPresenterClip

				// JJD 12/8/11 - TFS97329
				// Refactored - created helper method that can be called by any GridViewPanel derived class
		internal static void SetRecordPresenterClip(bool orientationIsVertical, double extentUsedByLastItem, double clipExtent, Rect arrangeRect, bool shouldClip, RecordPresenter rp)
		{
			if (rp != null)
			{
				Geometry clipGeometry = null;

				// create a clip geometry if necessary so bottom fixed records don't
				// get overlapped by 
				if (shouldClip && extentUsedByLastItem > clipExtent)
				{
					if (clipExtent <= 0)
						clipGeometry = Geometry.Empty;
					else
					{
						Rect clipRect;
						if (orientationIsVertical)
						{
							// JJD 12/8/11 - TFS97329
							// Offset clip geometry up by a pixal, i.e. -1 in the Y dimension.
							// Otherwise pixel rounding errors can cause a 1 pixel appearance gap above the record
							//clipRect = new Rect(new Size(arrangeRect.Width, clipExtent));
							clipRect = new Rect(new Point(0, -1), new Size(arrangeRect.Width, clipExtent + 1));
						}
						else
						{
							// JJD 12/8/11 - TFS97329
							// Offset clip geometry to the left by a pixal, i.e. -1 in the X dimension.
							// Otherwise  pixel rounding errors can cause a 1 pixel appearance gap to the left of the record
							//clipRect = new Rect(new Size(clipExtent, arrangeRect.Height));
							clipRect = new Rect(new Point(-1, 0), new Size(clipExtent + 1, arrangeRect.Height));
						}

						clipGeometry = new RectangleGeometry(clipRect);

						// JJD 4/29/10 - Optimization
						// Freeze the geometry so the framework doesn't need to listen for changes
						clipGeometry.Freeze();
					}
				}

				rp.InternalClip = clipGeometry;
			}
		}

				#endregion //SetRecordPresenterClip	
    
                // JJD 7/15/09 - TFS19156 - added
                #region VerifyAllParentsAreInView

        // JJD 7/15/09 - TFS19156
        // Make sure all of the parent records are in view also so the complete last
        // record can be fully displayed including the chrome from its parent records
        internal bool VerifyAllParentsAreInView(Record record, Size panelSize)
        {
            bool isVertical = this.LogicalOrientation == Orientation.Vertical;

            double panelExtent = isVertical ? panelSize.Height : panelSize.Width;

            if (!(panelExtent > 0) || record == null)
                return true;

            Record parent = record.ParentRecord;

            // walk up parent chain to make sure all parents are in view
            while (parent != null)
            {
                RecordPresenter rpParent = parent.AssociatedRecordPresenter;

                if (rpParent == null)
                    break;

                Point ptRightBottom = rpParent.TranslatePoint(new Point(rpParent.ActualWidth, rpParent.ActualHeight), this);

                double testValue = isVertical ? ptRightBottom.Y : ptRightBottom.X;

                // if the border edge is outside the panel extent then return false
                if (testValue > panelExtent)
                    return false;

                // walk up the parent chain
                parent = parent.ParentRecord;
            }

            return true;

        }

                #endregion //VerifyAllParentsAreInView	
    
                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                #region VerifyFixedFieldInfo
        internal void VerifyFixedFieldInfo()
        {
            FixedFieldInfo ffi = this.FixedFieldInfo;

            if (null != ffi)
            {
                double offset = 0;
                double extent = 0;
                double viewport = 0;
                ScrollData sd = this.ScrollingData;

                if (null != sd)
                {
                    if (this.LogicalOrientation == Orientation.Horizontal)
                    {
                        offset = sd._offset.Y;
                        extent = sd._extent.Height;
                        viewport = sd._viewport.Height;
                    }
                    else
                    {
                        offset = sd._offset.X;
                        extent = sd._extent.Width;
                        viewport = sd._viewport.Width;
                    }
                }

                ffi.Offset = offset;
                ffi.Extent = extent;
                ffi.ViewportExtent = viewport;
				ffi.FixedAreaElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
            }
        } 
                #endregion //VerifyFixedFieldInfo
    
				#region VerticalPageDelta

		// JJD 3/09/07 - BR20942
		// Added HorizontalPageDelta property
		internal double VerticalPageDelta
		{
			get
			{
				if (this.IsRootPanel == false)
					return this.RootPanel.VerticalPageDelta;

				// JJD 6/6/07
				// Added separate viewport setting to control scrollbar
				//double delta = this.ViewportHeight;
				double delta = this.ScrollingData._viewport.Height;

				double smallChange;

				if (this.LogicalOrientation == Orientation.Horizontal)
				{
                    // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
                    // We need to account for the amount of fixed area.
                    //
                    if (null != _fixedFieldInfo)
                        delta = _fixedFieldInfo.GetScrollableViewportExtent();

					smallChange = this.ScrollSmallChangeInNonOrientationDimension;
					// [JM 05-04-07] We don't appear to need need this tweak anymore - this may have been
					// compensated for somewhere else.
//					delta -= smallChange;
				}
				else
				{
					smallChange = this.ScrollSmallChangeInOrientationDimension;
					// [JM 05-04-07] We don't appear to need need these tweaks anymore - this may have been
					// compensated for somewhere else.
//					delta -= smallChange;
//
//					// tweak the delta by 1 if we have a reasonable # of items in view to ensure
//					// the previous last record that was fully in view will now be the 1st record
//					if (delta > 2)
//						delta--;
				}

				return Math.Max(smallChange, delta);
			}
		}

				#endregion //VerticalPageDelta	
    
			#endregion //Internal Methods

		#endregion //Methods

		#region IScrollInfo Members

			#region ExtentHeight, ExtentHeight

		/// <summary>
		/// Returns the overall logical height of the scrollable area.
		/// </summary>
		public double ExtentHeight
		{
			get
			{
				if (this.ScrollingData == null)
					return 0;

				// JJD 6/6/07
				// Added separate extent setting to control scrollbar
				//return this.ScrollingData._extent.Height;
				return this.ScrollingData._extentForScrollbar.Height;
			}
		}

		/// <summary>
		/// Returns the overall logical width of the scrollable area.
		/// </summary>
		public double ExtentWidth
		{
			get
			{
				if (this.ScrollingData == null)
					return 0;

				// JJD 6/6/07
				// Added separate extent setting to control scrollbar
				//return this.ScrollingData._extent.Width;
				return this.ScrollingData._extentForScrollbar.Width;
			}
		}

			#endregion //ExtentWidth, ExtentHeight

			#region HorizontalOffset, VerticalOffset

		/// <summary>
		/// Returns the logical horizontal offset of the scrollable area.
		/// </summary>
		public double HorizontalOffset
		{
			get
			{
				if (this.ScrollingData == null)
					return 0;

				// if we're in a deferred drag and the records are horizontal...
				if (this.ScrollingData._isInDeferredDrag && this.LogicalOrientation == Orientation.Horizontal)
					return this.ScrollingData._deferredDragOffset;

				return this.ScrollingData._offset.X;
			}
		}

		/// <summary>
		/// Returns the logical vertical offset of the scrollable area.
		/// </summary>
		public double VerticalOffset
		{
			get
			{
				if (this.ScrollingData == null)
					return 0;

				// if we're in a deferred drag and the records are vertical...
				if (this.ScrollingData._isInDeferredDrag && this.LogicalOrientation == Orientation.Vertical)
					return this.ScrollingData._deferredDragOffset;

				return this.ScrollingData._offset.Y;
			}
		}

		/// <summary>
		/// Sets the horizontal scroll offset.
		/// </summary>
		/// <param name="newOffset"></param>
		public void SetHorizontalOffset(double newOffset)
		{
			// JJD 6/12/07
			// Refactored logic into helper method that supports 
			// both vertical and horizontal offsets
			this.SetOffsetInternal(newOffset, this.HorizontalOffset, false);
		}

		/// <summary>
		/// Sets the vertical scroll offset
		/// </summary>
		/// <param name="newOffset"></param>
		public void SetVerticalOffset(double newOffset)
		{
			// JJD 6/12/07
			// Refactored logic into helper method that supports 
			// both vertical and horizontal offsets
			this.SetOffsetInternal(newOffset, this.VerticalOffset, true);
		}

			#endregion //HorizontalOffset, VerticalOffset

			#region LineDown, LineUp, LineLeft, LineRight

		/// <summary>
		/// Scrolls down 1 line.
		/// </summary>
		public void LineDown()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.LineDown();
				return;
			}


			this.SetVerticalOffset(this.VerticalOffset + (this.LogicalOrientation == Orientation.Vertical ? 
																					 this.ScrollSmallChangeInOrientationDimension :
																					 this.ScrollSmallChangeInNonOrientationDimension));
		}

		/// <summary>
		/// Scrolls left 1 line.
		/// </summary>
		public void LineLeft()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.LineUp();
				return;
			}


			this.SetHorizontalOffset(this.HorizontalOffset - (this.LogicalOrientation == Orientation.Horizontal ?
																						 this.ScrollSmallChangeInOrientationDimension :
																						 this.ScrollSmallChangeInNonOrientationDimension));
		}

		/// <summary>
		/// Scrolls right 1 line.
		/// </summary>
		public void LineRight()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.LineRight();
				return;
			}


			this.SetHorizontalOffset(this.HorizontalOffset + (this.LogicalOrientation == Orientation.Horizontal ?
																						 this.ScrollSmallChangeInOrientationDimension :
																						 this.ScrollSmallChangeInNonOrientationDimension));
		}

		/// <summary>
		/// Scrolls up 1 line.
		/// </summary>
		public void LineUp()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.LineUp();
				return;
			}


			this.SetVerticalOffset(this.VerticalOffset - (this.LogicalOrientation == Orientation.Vertical ?
																					 this.ScrollSmallChangeInOrientationDimension :
																					 this.ScrollSmallChangeInNonOrientationDimension));
		}

			#endregion //LineDown, LineUp, LineLeft, LineRight

			#region MouseWheelDown, MouseWheelUp, MouseWheelLeft, MouseWheelRight

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelDown.
		/// </summary>
		public void MouseWheelDown()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.MouseWheelDown();
				return;
			}


			this.SetVerticalOffset(this.VerticalOffset +
								  (this.MouseWheelScrollMultiplier * (this.LogicalOrientation == Orientation.Vertical ?
		  															  this.ScrollSmallChangeInOrientationDimension :
																	  this.ScrollSmallChangeInNonOrientationDimension)));
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelLeft.
		/// </summary>
		public void MouseWheelLeft()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.MouseWheelLeft();
				return;
			}


			this.SetHorizontalOffset(this.HorizontalOffset -
									(this.MouseWheelScrollMultiplier * (this.LogicalOrientation == Orientation.Horizontal ?
																		this.ScrollSmallChangeInOrientationDimension :
																		this.ScrollSmallChangeInNonOrientationDimension)));
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelRight.
		/// </summary>
		public void MouseWheelRight()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.MouseWheelRight();
				return;
			}


			this.SetHorizontalOffset(this.HorizontalOffset +
									(this.MouseWheelScrollMultiplier * (this.LogicalOrientation == Orientation.Horizontal ?
																		this.ScrollSmallChangeInOrientationDimension :
																		this.ScrollSmallChangeInNonOrientationDimension)));
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelUp.
		/// </summary>
		public void MouseWheelUp()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.MouseWheelUp();
				return;
			}


			this.SetVerticalOffset(this.VerticalOffset -
								  (this.MouseWheelScrollMultiplier * (this.LogicalOrientation == Orientation.Vertical ?
																	  this.ScrollSmallChangeInOrientationDimension :
																	  this.ScrollSmallChangeInNonOrientationDimension)));
		}

			#endregion //MouseWheelDown, MouseWheelUp, MouseWheelLeft, MouseWheelRight

			#region PageUp, PageDown, PageLeft, PageRight

		/// <summary>
		/// Scrolls down 1 page.
		/// </summary>
		public void PageDown()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.PageDown();
				return;
			}


			// JJD 3/09/07 - BR20942
			// Use new VerticalPageDelta property
			//double newOffset = this.VerticalOffset + this.ViewportHeight;
			double newOffset = this.VerticalOffset + this.VerticalPageDelta;

			if (this.LogicalOrientation == Orientation.Horizontal)
			{
				// JJD 6/6/07
				// Added separate viewport setting to control scrollbar
				//if ((newOffset + this.ViewportHeight) > this.ExtentHeight)
				//    newOffset = this.ExtentHeight - this.ViewportHeight;
				double viewport = this.ScrollingData._viewport.Height;
				double extent = this.ScrollingData._extent.Height;
				if ((newOffset + viewport) > extent)
					newOffset = extent - viewport;
			}

			this.SetVerticalOffset(newOffset);
		}

		/// <summary>
		/// Scrolls left 1 page.
		/// </summary>
		public void PageLeft()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.PageLeft();
				return;
			}


			this._setOffsetTriggeredByPageBack = true;

			// JJD 3/09/07 - BR20942
			// Use new HorizontalPageDelta property
			//this.SetHorizontalOffset(this.HorizontalOffset - this.ViewportWidth);
			this.SetHorizontalOffset(this.HorizontalOffset - this.HorizontalPageDelta);
		}

		/// <summary>
		/// Scrolls right 1 page.
		/// </summary>
		public void PageRight()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.PageRight();
				return;
			}


			// JJD 3/09/07 - BR20942
			// Use new HorizontalPageDelta property
			//double newOffset = this.HorizontalOffset + this.ViewportWidth;
			double newOffset = this.HorizontalOffset + this.HorizontalPageDelta;

			if (this.LogicalOrientation == Orientation.Vertical)
			{
				// JJD 6/6/07
				// Added separate viewport setting to control scrollbar
				//if ((newOffset + this.ViewportWidth) > this.ExtentWidth)
				//    newOffset = this.ExtentWidth - this.ViewportWidth;
				double viewport = this.ScrollingData._viewport.Width;
				double extent = this.ScrollingData._extent.Width;
				if ((newOffset + viewport) > extent)
					newOffset = extent - viewport;
			}

			this.SetHorizontalOffset(newOffset);
		}

		/// <summary>
		/// Scrolls up 1 page.
		/// </summary>
		public void PageUp()
		{
			if (this.IsRootPanel == false)
			{
				this.RootPanel.PageUp();
				return;
			}


			this._setOffsetTriggeredByPageBack = true;

			// JJD 3/09/07 - BR20942
			// Use new VerticalPageDelta property
			//this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight);
			this.SetVerticalOffset(this.VerticalOffset - this.VerticalPageDelta);
		}

			#endregion //PageUp, PageDown, PageLeft, PageRight

			#region ScrollOwner

		/// <summary>
		/// Returns/sets the scroll owner.
		/// </summary>
		public ScrollViewer ScrollOwner
		{
			get
			{
				return this.ScrollingData._scrollOwner;
			}
			set
			{
				if (value != this.ScrollingData._scrollOwner)
				{
					this.ScrollingData.Reset();
					this.ScrollingData._scrollOwner = value;

                    // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                    this.VerifyFixedFieldInfo();
                }
			}
		}

			#endregion //ScrollOwner

			#region ViewportHeight, ViewportWidth

		/// <summary>
		/// Returns the height of the Viewport.
		/// </summary>
		public double ViewportHeight
		{
			get
			{
				if (this.ScrollingData == null)
					return 0;

				// JJD 6/6/07
				// Added separate viewport setting to control scrollbar
				//return this.ScrollingData._viewport.Height;
				return this.ScrollingData._viewportForScrollbar.Height;
			}
		}

		/// <summary>
		/// Returns the wodth of the Viewport.
		/// </summary>
		public double ViewportWidth
		{
			get
			{
				if (this.ScrollingData == null)
					return 0;

				// JJD 6/6/07
				// Added separate viewport setting to control scrollbar
				//return this.ScrollingData._viewport.Width;
				return this.ScrollingData._viewportForScrollbar.Width;
			}
		}

			#endregion //ViewportHeight, ViewportWidth

			#region MakeVisible

		/// <summary>
		/// Ensures that the supplied visual is visible.
		/// </summary>
		/// <param name="visual"></param>
		/// <param name="rect"></param>
		/// <returns></returns>
		public Rect MakeVisible(Visual visual, Rect rect)
		{
            
#region Infragistics Source Cleanup (Region)








































































#endregion // Infragistics Source Cleanup (Region)

            if (this.IsScrolling == false)
                return visual.TransformToAncestor(this).TransformBounds(rect);

			// AS 7/24/09 NA 2009.2 Field Sizing
			// I found this while implementing field sizing.
			//
            //Rect descendantRect = visual.TransformToAncestor(this).TransformBounds(rect);
			Rect descendantRect = Rect.Empty;

			try
			{
				descendantRect = visual.TransformToAncestor(this).TransformBounds(rect);
			}
			catch
			{
				return Rect.Empty;
			}

			// AS 9/2/09 TFS18572
			// Actually we want to adjust the descendantRect based on the difference in 
			// offset since the last measure. Otherwise we may consider something in 
			// view because it was during the last arrange but not since we adjusted 
			// the horizontal offset (e.g. for another bringintoview request that came 
			// in since the last arrange.
			//
			// The transform is going to be based on where the element was arranged but the 
			// current scroll offset may already have been adjusted based on a previous 
			// bring into view request but after the last arrange.
			//
			descendantRect.X += _lastArrangeScrollOffset.X - this.ScrollingData._offset.X;
			descendantRect.Y += _lastArrangeScrollOffset.Y - this.ScrollingData._offset.Y;

            IScrollInfo si = this;
            Rect panelRect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);

            // only consider the visible scrollable area
            if (this.LogicalOrientation == Orientation.Vertical)
            {
                panelRect.Width = this.ScrollingData._viewport.Width;
            }
            else
            {
                panelRect.Height = this.ScrollingData._viewport.Height;
            }

            //Debug.WriteLine(string.Format("Visual: {0}, Rect:{1}, DescendantRect:{2}, PanelRect:{3}", visual, rect, descendantRect, panelRect));

			// JJD 2/9/11 - TFS62289
			// Added flag so we can ensure that the record is in view before we exit the method
			bool ensureRecordinView = false;

            Rect intersection = Rect.Intersect(descendantRect, panelRect);

            if (intersection.Width != descendantRect.Width)
            {
                double offsetX = 0;

                // try to get the right side in view
                if (descendantRect.Right > panelRect.Right)
                    offsetX = descendantRect.Right - panelRect.Right;

                // make sure that the left side is in view
                if (descendantRect.Left - offsetX - panelRect.Left < 0)
                    offsetX += descendantRect.Left - offsetX - panelRect.Left;

                if (!GridUtilities.AreClose(0, offsetX))
                {
					descendantRect.X -= offsetX;

					// JJD 2/9/11 - TFS62289
					// If the orientation is horizontal then just set a flag 
					// so we ensure that the record is in view since we can't
					// just offset the scroll position in this dimension since 
					// the scroll offset is in records not pixels
					if (this.LogicalOrientation == Orientation.Horizontal)
						ensureRecordinView = true;
					else
					{
						// AS 9/2/09 TFS18572
						//// AS 7/27/09 NA 2009.2 Field Sizing
						////offsetX += this.ScrollingData._offset.X;
						//offsetX += _lastArrangeScrollOffset.X;
						offsetX += this.ScrollingData._offset.X;
						this.SetHorizontalOffset(offsetX);
					}
                }
            }

            if (intersection.Height != descendantRect.Height)
            {
                double offsetY = 0;

                // try to get the right side in view
                if (descendantRect.Bottom > panelRect.Bottom)
                    offsetY = descendantRect.Bottom - panelRect.Bottom;

                // make sure that the left side is in view
                if (descendantRect.Top - offsetY - panelRect.Top < 0)
                    offsetY += descendantRect.Top - offsetY - panelRect.Top;

                if (!GridUtilities.AreClose(0, offsetY))
                {
					descendantRect.Y -= offsetY;

					// JJD 2/9/11 - TFS62289
					// If the orientation is vertical then just set a flag 
					// so we ensure that the record is in view since we can't
					// just offset the scroll position in this dimension since 
					// the scroll offset is in records not pixels
					if (this.LogicalOrientation == Orientation.Vertical)
						ensureRecordinView = true;
					else
					{
						// AS 9/2/09 TFS18572
						//// AS 7/27/09 NA 2009.2 Field Sizing
						////offsetY += this.ScrollingData._offset.Y;
						//offsetY += _lastArrangeScrollOffset.Y;
						offsetY += this.ScrollingData._offset.Y;
						this.SetVerticalOffset(offsetY);
					}
				}
            }

			// JJD 2/9/11 - TFS62289
			// check the ensure rcd in view flag set above
			if (ensureRecordinView)
			{
				// JJD 2/9/11 - TFS62289
				// Get the ancestor of the visual that is the panel's immediate descendant
				DependencyObject immediateChild = GridUtilities.GetImmediateDescendant(visual, this);

				if (immediateChild != null)
				{
					// JJD 2/9/11 - TFS62289
					// get the RecordPresenter and call EnsureRecordIsVisible
					RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(immediateChild);

					if (rp != null && rp.Record != null)
						this.EnsureRecordIsVisible(rp.Record);
				}
			}

            return descendantRect;
		}
			#endregion //MakeVisible

        //private Rect EnsureRecordPresenterIsVisible(RecordPresenter rp, Rect rect)
        //{
        //    // we don't want to do anything for RecordPresenters since one rp can
        //    // represent more that one record (i.e. the parent record and all of its
        //    // expanded descendants). We handle logical scrolling differently via
        //    // nested GridViewPanels.
        //    return rect;
        //}

			#region CanVerticallyScroll, CanHorizontallyScroll

		/// <summary>
		/// Returns true if vertical scrolling can be performed.
		/// </summary>
		public bool CanVerticallyScroll
		{
			get
			{

				return this.ScrollingData._canVerticallyScroll;
			}
			set
			{
				if (this.ScrollingData._canVerticallyScroll != value)
				{
					this.ScrollingData._canVerticallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

		/// <summary>
		/// Returns true if horizontal scrolling can be performed.
		/// </summary>
		public bool CanHorizontallyScroll
		{
			get
			{

				return this.ScrollingData._canHorizontallyScroll;
			}
			set
			{
				if (this.ScrollingData._canHorizontallyScroll != value)
				{
					this.ScrollingData._canHorizontallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

		#endregion //CanVerticallyScroll, CanHorizontallyScroll

		#endregion //IScrollInfo Members

		#region ScrollData Private Class

		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer		_scrollOwner = null;
			internal Size				_extent = new Size();
			internal Size				_viewport = new Size();
			internal Vector				_offset = new Vector();

			// JJD 6/6/07
			// Added separate viewport setting to control scrollbar
			internal Size				_extentForScrollbar = new Size();
			internal Size				_viewportForScrollbar = new Size();
			
			// JJD 6/11/07
			// Added members to calculate average viewport size
			internal double				_averageHeightOfRecords;
			internal double				_highestRecordDisplayed;

			// JJD 6/6/07 
			// added flag so we know if the last scrollable record is fully in view
			internal bool				_isLastRecordInView = false;
			internal bool				_hasOffsetChanged = false;

			internal bool				_canHorizontallyScroll = false;
			internal bool				_canVerticallyScroll = false;

			internal ToolTip			_scrollTip;

			internal bool				_isInDeferredDrag;
			internal double				_deferredDragOffset;

			// AS 4/12/11 TFS62951
			// See the comments in GridViewPanelFlat.ArrangeOverride for details.
			//
			internal Size				_availableScrollSize;

			#endregion //Member Variables

			#region Methods

				#region InitializeScrollTip
			internal void InitializeScrollTip(Record topRecord)
			{
				if (this._scrollTip != null)
				{
					List<RecordScrollTipInfo> list = new List<RecordScrollTipInfo>();

					// SSP 12/17/08 - NAS9.1 Record Filtering
					// Check for topRecord being null. A scroll position can correspond with no record
					// if records are lazily filtered out as they are allocated. To reproduce this scenario,
					// in data presenter, have a record filter that filters out most of the records. Then
					// drag the thumb down. As records are allocated and filtered out, total scroll count
					// may dip below current scroll thumb position in which case there won't be any record 
					// that corresponds to the current scroll thumb position
					// 
					if ( null != topRecord )
						list.Add(RecordScrollTipInfo.Create(topRecord));

					this._scrollTip.DataContext = list;
					this._scrollTip.Content = this._scrollTip.DataContext;

					this._scrollTip.IsOpen = true;
				}
			} 
				#endregion //InitializeScrollTip

				#region Reset

			internal void Reset()
			{
				this._offset			= new Vector();
				this._extent			= new Size();
				this._viewport			= new Size();
				this._extentForScrollbar	= new Size();
				this._viewportForScrollbar	= new Size();
				// JJD 6/6/07 
				// added flag so we know if the last scrollable record is fully in view
				this._isLastRecordInView = false;
			}

				#endregion //Reset

			#endregion //Methods
		}

		#endregion //ScrollData Private Class

		#region IViewPanel Members

			#region EnsureCellIsVisible

		bool IViewPanel.EnsureCellIsVisible(Cell cell)
		{
			return this.RootPanel.EnsureCellIsVisible(cell);
		}

			#endregion //EnsureCellIsVisible

			#region EnsureRecordIsVisible

		bool IViewPanel.EnsureRecordIsVisible(Record record)
		{
			return this.RootPanel.EnsureRecordIsVisible(record);
		}

			#endregion //EnsureRecordIsVisible

			#region GetFirstDisplayedRecord

		Record IViewPanel.GetFirstDisplayedRecord(Type recordType)
		{
            // JJD 1/12/09 - NA 2009 vol 1
            // Convert the passed in type to our RecordType enum to comapre
            // so we don't mistake a filter record for a data record
            RecordType? rcdTypeEnum = GridUtilities.GetRecordTypeFromType(recordType);

            // JJD 2/5/09 - TFS13478
            // If they pass in the base Record type we need to let that process
            if (rcdTypeEnum == null && recordType != typeof(Record))
                return null;

			Record topRecord = this.FirstScrollableRecord;

			// If the top record is null, return null.
			if (topRecord == null)
				return null;


			// If the top record is of the requested type, return it.
			// JJD 6/14/07
			// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
			//if (topRecord.GetType() == recordType ||
			//    topRecord.GetType().IsSubclassOf(recordType))

            // JJD 1/12/09 - NA 2009 vol 1
            // Use IsRecordOfType method instead 
			//if ( recordType.IsAssignableFrom( topRecord.GetType()))
            if (GridUtilities.IsRecordOfType(topRecord, rcdTypeEnum, recordType))
                return topRecord;

			DataPresenterBase dp = this.DataPresenter;
			IViewPanelInfo info = this.ViewPanelInfo;


			// Since the top record wasn't of the requested type, try looking forwards from the TopRecord
			// for a record of the requested type.
			int						startIndex = info.GetOverallScrollPositionForRecord( topRecord ) + 1;
			// JJD 3/12/07 Use the cached value on the rootpanel
			//int					scrollIndexOfLastVisibleRecord = this.GetScrollIndexOfLastVisibleRecord();
			int						scrollIndexOfLastVisibleRecord = this.RootPanel.ScrollPositionOfLastVisibleRecord;
			
			Record					record;

			for (int i = startIndex; i <= scrollIndexOfLastVisibleRecord; i++)
			{
				record = info.GetRecordAtOverallScrollPosition(i);

                // JJD 10/19/09 - TFS16793
                // Bypass special records
                if (record == null || record.IsSpecialRecord)
                    continue;

                // JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (record.GetType() == recordType ||
				//    record.GetType().IsSubclassOf(recordType))

                // JJD 1/12/09 - NA 2009 vol 1
                // Use IsRecordOfType method instead. 
                //if (record != null && recordType.IsAssignableFrom(record.GetType()))
                if (GridUtilities.IsRecordOfType(record, rcdTypeEnum, recordType))
 					return record;
			}

			return null;
		}

			#endregion //GetFirstDisplayedRecord

			#region GetFirstOverallRecord

		Record IViewPanel.GetFirstOverallRecord(Type recordType)
		{
            // JJD 1/12/09 - NA 2009 vol 1
            // Convert the passed in type to our RecordType enum to comapre
            // so we don't mistake a filter record for a data record
            RecordType? rcdTypeEnum = GridUtilities.GetRecordTypeFromType(recordType);

            // JJD 2/5/09 - TFS13478
            // If they pass in the base Record type we need to let that process
            if (rcdTypeEnum == null && recordType != typeof(Record))
                return null;

			IViewPanelInfo info = this.ViewPanelInfo;
			int					scrollCount		= info.OverallScrollCount;
			Record				record;

			for (int i = 0; i < scrollCount; i++)
			{
				record = info.GetRecordAtOverallScrollPosition(i);

                // JJD 10/19/09 - TFS16793
                // Bypass special records
                if (record == null || record.IsSpecialRecord)
                    continue;

                // JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (record.GetType() == recordType ||
				//    record.GetType().IsSubclassOf(recordType))
                // JJD 1/12/09 - NA 2009 vol 1
                // Use IsRecordOfType method instead. 
                //if (record != null && recordType.IsAssignableFrom(record.GetType()))
                if (GridUtilities.IsRecordOfType(record, rcdTypeEnum, recordType))
                    return record;
			}

			return null;
		}

			#endregion //GetFirstOverallRecord

			#region GetLastDisplayedRecord

		Record IViewPanel.GetLastDisplayedRecord(Type recordType)
		{
            // JJD 1/12/09 - NA 2009 vol 1
            // Convert the passed in type to our RecordType enum to comapre
            // so we don't mistake a filter record for a data record
            RecordType? rcdTypeEnum = GridUtilities.GetRecordTypeFromType(recordType);

            // JJD 2/5/09 - TFS13478
            // If they pass in the base Record type we need to let that process
            if (rcdTypeEnum == null && recordType != typeof(Record))
                return null;

			// JJD 3/12/07 Use the cached value on the rootpanel
			//int scrollIndexOfLastVisibleRecord = this.GetScrollIndexOfLastVisibleRecord();
			int scrollIndexOfLastVisibleRecord = this.RootPanel.ScrollPositionOfLastVisibleRecord;
			if (scrollIndexOfLastVisibleRecord < 0)
				return null;


			IViewPanelInfo		info					= this.ViewPanelInfo;

            // JJD 7/17/09 - TFS19588
            // Make sure that info is not null;
            if (info == null)
                return null;

            Record lastVisibleRecord = info.GetRecordAtOverallScrollPosition(scrollIndexOfLastVisibleRecord);


			// [JM 05-30-07 BR22778]
			if (lastVisibleRecord == null)
				return null;


			// If the last visible record is of the requested type, return it.
			// JJD 6/14/07
			// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
			//if (lastVisibleRecord.GetType() == recordType ||
			//    lastVisibleRecord.GetType().IsSubclassOf(recordType))

            // JJD 1/12/09 - NA 2009 vol 1
            // Use IsRecordOfType method instead. 
            //if (recordType.IsAssignableFrom(lastVisibleRecord.GetType()))
            if (GridUtilities.IsRecordOfType(lastVisibleRecord, rcdTypeEnum, recordType))
                return lastVisibleRecord;


			// Since the last visible record wasn't of the requested type, try looking backwards from the last visible record
			// for a record of the requested type.
			int		startIndex						= scrollIndexOfLastVisibleRecord - 1;
			Record	record;

			for (int i = startIndex; i > -1; i--)
			{
				record = info.GetRecordAtOverallScrollPosition(i);

				// [JM 05-30-07 BR22778]
				if (record == null)
					continue;

                // JJD 10/19/09 - TFS16793
                // Bypass special records
                if (record.IsSpecialRecord)
                    continue;

				// JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (record.GetType() == recordType ||
				//    record.GetType().IsSubclassOf(recordType))

                // JJD 1/12/09 - NA 2009 vol 1
                // Use IsRecordOfType method instead. 
                //if (record != null && recordType.IsAssignableFrom(record.GetType()))
                if (GridUtilities.IsRecordOfType(record, rcdTypeEnum, recordType))
                    return record;
			}

			return null;
		}

			#endregion //GetLastDisplayedRecord

			#region GetLastOverallRecord

		Record IViewPanel.GetLastOverallRecord(Type recordType)
		{

            // JJD 1/12/09 - NA 2009 vol 1
            // Convert the passed in type to our RecordType enum to comapre
            // so we don't mistake a filter record for a data record
            RecordType? rcdTypeEnum = GridUtilities.GetRecordTypeFromType(recordType);

            // JJD 2/5/09 - TFS13478
            // If they pass in the base Record type we need to let that process
            if (rcdTypeEnum == null && recordType != typeof(Record))
                return null;

			IViewPanelInfo info = this.ViewPanelInfo;

            // JJD 7/17/09 - TFS19588
            // Make sure that info is not null;
            if (info == null)
                return null;

            int scrollCount = info.OverallScrollCount;
			Record					record;

			for (int i = scrollCount - 1; i >= 0; i--)
			{
				record = info.GetRecordAtOverallScrollPosition(i);

                // JJD 10/19/09 - TFS16793
                // Bypass special records
                if (record == null || record.IsSpecialRecord)
                    continue;

				// JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (record.GetType() == recordType ||
				//    record.GetType().IsSubclassOf(recordType))

                // JJD 1/12/09 - NA 2009 vol 1
                // Use IsRecordOfType method instead. 
                //if (record != null && recordType.IsAssignableFrom(record.GetType()))
                if (GridUtilities.IsRecordOfType(record, rcdTypeEnum, recordType))
                    return record;
			}

			return null;
		}

			#endregion //GetLastOverallRecord

			#region GetNavigationTargetRecord

		Record IViewPanel.GetNavigationTargetRecord(Record currentRecord, PanelNavigationDirection navigationDirection, ISelectionHost selectionHost, bool shiftKeyDown, bool ctlKeyDown, PanelSiblingNavigationStyle siblingNavigationStyle, Type restrictToRecordType)
		{
			// Validate parameters.
			if (currentRecord == null)
				throw new ArgumentNullException("currentRecord");
			if (selectionHost == null)
				throw new ArgumentNullException("selectionHost");


			// Delegate to the root panel if we are not the root.
			if (this.IsRootPanel == false)
				return ((IViewPanel)this.RootPanel).GetNavigationTargetRecord(currentRecord, navigationDirection, selectionHost, shiftKeyDown, ctlKeyDown, siblingNavigationStyle, restrictToRecordType);


			// Get the selection strategy
			SelectionStrategyBase selectionStrategy = selectionHost.GetSelectionStrategyForItem(currentRecord as ISelectableItem);

			Debug.Assert(selectionStrategy != null);
			if (selectionStrategy == null)
				throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidOperationException_4"));


			// Establish the 'candidate record' we would like to navigate to based on the current record and the navigation
			// direction and see if the selection strategy will let us go there.  First try looking for a candidate without
			// limiting the search to the same parent as the current record (if our caller allows this).  If that record 
			// cannot be navigated to, then look with the same parent.
			Record candidateRecord;
			if (siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsAndWrap  ||
				siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsNoWrap)
			{

                
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


                candidateRecord = this.GetNavigationTargetRecordHelper(currentRecord, navigationDirection, shiftKeyDown, ctlKeyDown, siblingNavigationStyle, restrictToRecordType, selectionStrategy);
                if (candidateRecord != null)
                    return candidateRecord;
			}


			// Since the strategy won't let us navigate to the candidate record, try a candidate that has the same parent (i.e.,
			// is a sibling of the current record)
			PanelSiblingNavigationStyle sns = (siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsAndWrap) ?
																			PanelSiblingNavigationStyle.StayWithinParentAndWrap :
										 (siblingNavigationStyle == PanelSiblingNavigationStyle.AcrossParentsNoWrap) ?
																			PanelSiblingNavigationStyle.StayWithinParentNoWrap :
																			siblingNavigationStyle;

            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


            candidateRecord = this.GetNavigationTargetRecordHelper(currentRecord, navigationDirection, shiftKeyDown, ctlKeyDown, sns, restrictToRecordType, selectionStrategy);
            if (candidateRecord != null)
                return candidateRecord;

			return null;
		}

        private Record GetNavigationTargetRecordHelper(Record currentRecord,
                                                        PanelNavigationDirection navigationDirection,
                                                        bool shiftKeyDown,
                                                        bool ctlKeyDown,
                                                        PanelSiblingNavigationStyle siblingNavigationStyle,
                                                        Type restrictToRecordType,
                                                        SelectionStrategyBase selectionStrategy)
        {
            Record candidateRecord = currentRecord;

            // JJD 1/9/09
            // keep navigating in the same direction until we find a record that 
            // is enabled and can be navigated to based on the selection strategy
            while (candidateRecord != null)
            {
                candidateRecord = this.GetNavigationTargetRecord(navigationDirection, candidateRecord, siblingNavigationStyle, restrictToRecordType);
                if (candidateRecord == null)
                    return null;

                if (candidateRecord.IsEnabledResolved &&
                    selectionStrategy.CanItemBeNavigatedTo(candidateRecord, shiftKeyDown, ctlKeyDown) == true)
                    return candidateRecord;
            }

            return null;
        }

			#endregion //GetNavigationTargetRecord

			#region LayoutStyle

		PanelLayoutStyle IViewPanel.LayoutStyle
		{
			get 
			{
				switch (this.LogicalOrientation)
				{
					case Orientation.Horizontal:
						return PanelLayoutStyle.GridViewHorizontal;
					case Orientation.Vertical:
						return PanelLayoutStyle.GridViewVertical;
					default:
						return PanelLayoutStyle.GridViewVertical;
				}
			}
		}

			#endregion //LayoutStyle

			#region OnActiveRecordChanged

		void IViewPanel.OnActiveRecordChanged(Record record)
		{
		}

			#endregion //OnActiveRecordChanged

			#region OnSelectedItemsChanged

		void IViewPanel.OnSelectedItemsChanged()
		{
		}

			#endregion //OnSelectedItemsChanged

			#region Scroll

		void IViewPanel.Scroll(PanelNavigationScrollType scrollType)
		{
			switch (scrollType)
			{
				case PanelNavigationScrollType.PageAbove:
					this.RootPanel.PageUp();
					break;
				case PanelNavigationScrollType.PageBelow:
					this.RootPanel.PageDown();
					break;
				case PanelNavigationScrollType.PageLeft:
					this.RootPanel.PageLeft();
					break;
				case PanelNavigationScrollType.PageRight:
					this.RootPanel.PageRight();
					break;
			}
		}

			#endregion //Scroll

		#endregion //IViewPanel Members

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					if (sender is GridViewSettings)
					{
						if (sender == this._viewSettings)
						{
							this.OnViewSettingsPropertyChanged(sender, args);
							return true;
						}

						//Debug.Fail("Invalid sender object in ReceiveWeakEvent for GridViewPanel, sender type: " + sender != null ? sender.ToString() : "null");
						return true;
					}
					else if (sender is RecordManager)
					{
						switch (args.PropertyName)
						{
							case "UnderlyingDataVersion":
								{
									IViewPanelInfo info = this.ViewPanelInfo;
									int effectiveScrollPosition = this.EffectiveScrollPosition;

									if (effectiveScrollPosition != (int)this.VerticalOffset ||
										effectiveScrollPosition >= info.OverallScrollCount)
										this.EffectiveScrollPosition = Math.Max(0, effectiveScrollPosition);;

									this.InvalidateMeasure();
									return true;
								}

							default:
								return true;
						}
					}

					Debug.Fail("Invalid sender type in ReceiveWeakEvent for GridViewPanel, sender type: " + sender != null ? sender.GetType().ToString() : "null");
					return false;
				}

				Debug.Fail("Invalid args in ReceiveWeakEvent for GridViewPanel, arg type: " + e != null ? e.ToString() : "null");

				return false;
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for GridViewPanel, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion //IWeakEventListener Members

		#region IDeferredScrollPanel Members

		void IDeferredScrollPanel.OnThumbDragComplete(bool cancelled)
		{
			if (this.ScrollingData._isInDeferredDrag)
			{
				// turn off the thumb drag flag and perform the scroll
				this._scrollingData._isInDeferredDrag = false;

				bool isVertical = this.LogicalOrientation == Orientation.Vertical;

				// JJD 10/31/11 - TFS89202
				// Added the following code to detect when the thumb has been dragged to the very bottom
				double extent;
				double viewport;

				if (isVertical)
				{
					extent = _scrollingData._extentForScrollbar.Height;
					viewport = _scrollingData._viewportForScrollbar.Height;
				}
				else
				{
					extent = _scrollingData._extentForScrollbar.Width;
					viewport = _scrollingData._viewportForScrollbar.Width;
				}

				// JJD 10/31/11 - TFS89202
				// If the thumb was dragged to the very bottom then
				// set the offset to the actual extent - 1. This will
				// cause the normal pull down logic to be invoked so
				// that the last record will be fully in view
				if (_scrollingData._deferredDragOffset + viewport >= extent)
				{
					double actualExtent = isVertical ? _scrollingData._extent.Height : _scrollingData._extent.Width;

					// JJD 11/16/11
					// Set the _deferredDragOffset to one up from the bottom
					//_scrollingData._deferredDragOffset = actualExtent - 1;
					_scrollingData._deferredDragOffset = Math.Max( actualExtent - 2, _scrollingData._deferredDragOffset );
				}

				if (isVertical)
					this.SetVerticalOffset(this._scrollingData._deferredDragOffset);
				else
					this.SetHorizontalOffset(this._scrollingData._deferredDragOffset);
			}

			// hide the tooltip and release the reference
			if (this.ScrollingData._scrollTip != null)
			{
				this.ScrollingData._scrollTip.IsOpen = false;
				this.ScrollingData._scrollTip = null;
			}
        }

		void IDeferredScrollPanel.OnThumbDragStart(Thumb thumb, Orientation scrollBarOrientation)
		{
			if (this.DataPresenter.ScrollingMode == ScrollingMode.DeferredWithScrollTips ||
				this.DataPresenter.ScrollingMode == ScrollingMode.Deferred)
			{

                this.ScrollingData._isInDeferredDrag = true;
				this.ScrollingData._deferredDragOffset = this.LogicalOrientation == Orientation.Vertical
					? this.ScrollingData._offset.Y
					: this.ScrollingData._offset.X;

                // JJD 8/19/09 - NA 2009 Vol 2 - Enhanced grid view
                
                // Call the virtual OnDeferredRecordScrollStart to let the derived panels know
                // to check for any pending updates
                this.OnDeferredRecordScrollStart();
			}

			if (this.DataPresenter.ScrollingMode == ScrollingMode.DeferredWithScrollTips)
			{
				// create the scroll tip
				RecordScrollTip toolTip = new RecordScrollTip();

				this.ScrollingData._scrollTip = toolTip;

				// AS 3/19/07 BR21256
				//Point pt = thumb.TranslatePoint(new Point(), this);
				//Rect rect = new Rect(pt, thumb.RenderSize);
				Point pt = thumb.TranslatePoint(new Point(), this);
				Rect rect = new Rect(pt, thumb.RenderSize);
				const double ScrollTipOffset = 5d;

				if (this.LogicalOrientation == Orientation.Vertical)
					rect.Inflate(ScrollTipOffset, 0);
				else
					rect.Inflate(0, ScrollTipOffset);

				toolTip.PlacementRectangle = rect;
				toolTip.PlacementTarget = this;

				toolTip.Placement = this.LogicalOrientation == Orientation.Vertical
					? PlacementMode.Left
					: PlacementMode.Top;

				IViewPanelInfo info = this.ViewPanelInfo;
				Record topRecord = this.GetTopRecordFromScrollOffset(info.OverallScrollPosition);

				this.ScrollingData.InitializeScrollTip(topRecord);
			}
		}

		bool IDeferredScrollPanel.SupportsDeferredScrolling(Orientation scrollBarOrientation, ScrollViewer scrollViewer)
		{
			return scrollViewer != null &&
				this.IsRootPanel &&
				scrollViewer.TemplatedParent == this.RecordListControl &&
				scrollBarOrientation == this.LogicalOrientation;
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