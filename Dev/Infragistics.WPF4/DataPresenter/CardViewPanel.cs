using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows.Threading;
using Infragistics.Windows;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Virtualization;
using Infragistics.Windows.Scrolling;
using Infragistics.Windows.Selection;
using System.Windows.Data;
using System.Collections;
using Infragistics.Windows.Tiles;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A <see cref="RecyclingItemsPanel"/> derived class that wraps and arranges items in an array of <see cref="CardViewCard"/> elements that are organized into rows and columns.  
	/// </summary>
	/// <seealso cref="RecyclingItemsPanel"/>
	/// <seealso cref="CardViewCard"/>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CardViewPanel : TilesPanelBase,
								 IViewPanel,
								 IWeakEventListener	
	{
		#region Member Variables

		private DataPresenterBase					_dataPresenter = null;
		private IViewPanelInfo						_dataPresenterViewPanelInfo = null;

		private Infragistics.Windows.DataPresenter.ViewBase	
													_view = null;
		private CardViewSettings					_viewSettings;
		private bool								_itemsPanelInitialized;
		private RecordListControl					_recordListControl;
		private RecordScrollTip						_recordScrollTip;

		private List<List<ItemInfoBase>>			_layoutColumns;

		private double								_uncollapsedCardWidth = double.NaN;

		private PropertyValueTracker				_specialRecordsVersionTracker;
		private FieldLayout							_specialRecordsVersionTrackerFieldLayout;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of the CardViewPanel.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>For Infragistics internal use only.  An instance of this control is automatically created by the XamDataCards and XamDataPresenter
		/// controls when needed.  You should never have to create an instance of this control directly.</p>
		/// </remarks>
		public CardViewPanel()
		{
		}

		#endregion //Constructor	

		#region Constants

		#endregion //Constants
    
		#region Base Class Overrides

			#region GetContainerConstraints

		/// <summary>
		/// Gets any explicit constraints for a container
		/// </summary>
		/// <param name="container">The container in question.</param>
		/// <param name="state">The current state of the container.</param>
		/// <returns>A <see cref="ITileConstraints"/> object or null.</returns>
		internal protected override ITileConstraints GetContainerConstraints(DependencyObject container, TileState state)
		{
			TileConstraints tc = new TileConstraints();

			tc.MaxHeight	= double.PositiveInfinity;
			tc.MaxWidth		= double.PositiveInfinity;

			CardViewSettings	viewSettings	= this.ViewSettings;
			CardViewCard		card			= container as CardViewCard;
			if (viewSettings == null || card == null)
				return tc;

			if (double.IsNaN(viewSettings.CardHeight) == false)
			{
				if (card.IsCollapsed == false)
					tc.PreferredHeight	= viewSettings.CardHeight;
			}

			if (double.IsNaN(viewSettings.CardWidth) == false)
				tc.PreferredWidth	= viewSettings.CardWidth;


			switch (viewSettings.AutoFitCards)
			{
				case AutoFitCards.Horizontally:
					tc.HorizontalAlignment	= HorizontalAlignment.Stretch;
					break;

				case AutoFitCards.Vertically:
					tc.VerticalAlignment	= VerticalAlignment.Stretch;
					break;

				case AutoFitCards.HorizontallyAndVertically:
					tc.HorizontalAlignment	= HorizontalAlignment.Stretch;
					tc.VerticalAlignment	= VerticalAlignment.Stretch;
					break;
			}
			
			return tc;
		}

			#endregion //GetContainerConstraints

			#region GetHorizontalTileAreaAlignment

		/// <summary>
        /// Determines the horizontal alignment of the complete block of visible tiles within the control.
        /// </summary>
        internal protected override HorizontalAlignment GetHorizontalTileAreaAlignment()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return HorizontalAlignment.Left;

			AutoFitCards autoFitCards = this.ViewSettings.AutoFitCards;
			if (autoFitCards == AutoFitCards.Horizontally || autoFitCards == AutoFitCards.HorizontallyAndVertically)
				return HorizontalAlignment.Stretch;
			else
				return HorizontalAlignment.Left;
		}

            #endregion //GetHorizontalTileAreaAlignment	

            #region GetInterTileSpacing

        /// <summary>
        /// Gets the amount of spacing between tiles in a specific state.
        /// </summary>
        /// <param name="vertical">True for vertical spacing, false for horzontal spacing.</param>
        /// <param name="state">The state of the tiles.</param>
        internal protected override double GetInterTileSpacing(bool vertical, TileState state)
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return 0;

			return vertical ? this.ViewSettings.InterCardSpacingY :
							  this.ViewSettings.InterCardSpacingX;
        }

            #endregion //GetInterTileSpacing	
    
            #region GetIsInDeferredScrollingMode

        /// <summary>
        /// Returns true if scrolling is deferred until the scroll thumb is released.
        /// </summary>
		internal protected override bool GetIsInDeferredScrollingMode()
        {
			return (this.DataPresenter != null && (this.DataPresenter.ScrollingMode	== ScrollingMode.DeferredWithScrollTips ||
												   this.DataPresenter.ScrollingMode	== ScrollingMode.Deferred));
		}

			#endregion //GetIsInDeferredScrollingMode

            #region GetMin/Max/Columns/Rows

        /// <summary>
        /// Gets the maximum number of colums to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMaxColumns()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return 0;

            return this.ViewSettings.MaxCardCols;
        }

        /// <summary>
        /// Gets the maximum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMaxRows()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return 0;

			return this.ViewSettings.MaxCardRows;
        }

        /// <summary>
        /// Gets the minimum number of colums to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMinColumns()
        {
            return 1;
        }

        /// <summary>
        /// Gets the minimum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMinRows()
        {
            return 1;
        }

            #endregion //GetMin/Max/Columns/Rows	

            #region GetRepositionAnimation

        /// <summary>
        /// Determines how a tile> animates from one location to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="GetShouldAnimate()"/> returns 'False'.</para>
        /// </remarks>
        protected override DoubleAnimationBase GetRepositionAnimation()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return null;

			return this.ViewSettings.RepositionAnimation;
        }

            #endregion //GetRepositionAnimation	

            #region GetShouldAnimate

        /// <summary>
        /// Gets/sets whether tiles will animate to their new position and size
        /// </summary>
        internal protected override bool GetShouldAnimate()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return true;

            return this.ViewSettings.ShouldAnimateCardPositioning;
        }

            #endregion //GetShouldAnimate	
    
			#region GetSupportsDeferredScrolling

		/// <summary>
		/// Returns a boolean indicating whether scrolling in the specified orientation can be deferred.
		/// </summary>
		/// <param name="scrollBarOrientation">Orientation of the scrollbar whose thumb is being dragged.</param>
		/// <param name="scrollViewer">ScrollViewer whose scroll thumb is being dragged.</param>
		/// <returns>Returns true if the panel could support deferred scrolling in the specified orientation.</returns>
		internal protected override bool GetSupportsDeferredScrolling(Orientation scrollBarOrientation, ScrollViewer scrollViewer)
		{
			if (scrollViewer == null)
				return false;

			if (scrollViewer.TemplatedParent != this.RecordListControl)
				return false;

			return scrollBarOrientation == this.ScrollBarOrientation;
		}

			#endregion //GetSupportsDeferredScrolling

            #region GetTileAreaPadding

        /// <summary>
        /// Get the amount of space between the panel and the area where the tiles are arranged.
        /// </summary>
        internal protected override Thickness GetTileAreaPadding()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return new Thickness();

            return this.ViewSettings.Padding;
        }

            #endregion //GetTileAreaPadding

            #region GetTileLayoutOrder

        /// <summary>
        /// Determines how the panel will layout the tiles.
        /// </summary>
        internal protected override TileLayoutOrder GetTileLayoutOrder()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return TileLayoutOrder.Horizontal;

            return this.ViewSettings.Orientation == Orientation.Vertical ? TileLayoutOrder.VerticalVariable :
																		   TileLayoutOrder.Horizontal;
        }

            #endregion //GetTileLayoutOrder

            #region GetVerticalTileAreaAlignment

        /// <summary>
        /// Determines the vertical alignment of the complete block of visible tiles within the control.
        /// </summary>
        internal protected override VerticalAlignment GetVerticalTileAreaAlignment()
        {
			// JM 02-02-10 TFS27011
			if (this.ViewSettings == null)
				return VerticalAlignment.Top;

			AutoFitCards autoFitCards = this.ViewSettings.AutoFitCards;
            if (autoFitCards == AutoFitCards.Vertically || autoFitCards == AutoFitCards.HorizontallyAndVertically)
				return VerticalAlignment.Stretch;
			else
				return VerticalAlignment.Top;
        }

            #endregion //GetVerticalTileAreaAlignment

			#region IsOkToCleanupUnusedGeneratedElements

		/// <summary>
		/// Called when elements are about to be cleaned up.  Return true to allow cleanup, false to prevent cleanup.
		/// </summary>
		protected override bool IsOkToCleanupUnusedGeneratedElements
		{
			get	{ return this.IsAnimationInProgress == false; }
		}

			#endregion IsOkToCleanupUnusedGeneratedElements

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size measureSize = base.MeasureOverride(availableSize);

			//JM 01-18-10 TFS25888
			if (this.DataPresenter != null)
				((IViewPanelInfo)this.DataPresenter).OverallScrollPosition = base.ScrollPosition;

			this._layoutColumns = null;

			return measureSize;
		}

			#endregion //MeasureOverride

			
			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					if (args.Position.Index > -1)
						this.RemoveInternalChildRange(args.Position.Index, 1);

					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
			}
		}

			#endregion //OnItemsChanged
    
            #region OnDeferredScrollingStarted

        /// <summary>
		/// Called when the user has initiated a scrolling operation by dragging the scroll thumb.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> This method will only be called if <see cref="GetIsInDeferredScrollingMode"/> returns true.</para>
		/// <para></para>
		/// This is a convenient place for derived classes to create a ToolTip to use as a scroll tip.  The <see cref="OnDeferredScrollingEnded"/> method will be called when the user stops dragging the scroll thumb.
		/// </remarks>
		/// <seealso cref="GetIsInDeferredScrollingMode"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingEnded"/> cref=""/>
		internal protected override void OnDeferredScrollingStarted(Thumb thumb, Orientation scrollBarOrientation)
        {
			if (this.DataPresenter != null &&
				this.DataPresenter.ScrollingMode == ScrollingMode.DeferredWithScrollTips)
			{
				// create the scroll tip
				this._recordScrollTip = new RecordScrollTip();


				Point	pt		= thumb.TranslatePoint(new Point(), this);
				Rect	rect	= new Rect(pt, thumb.RenderSize);
				const double ScrollTipOffset = 5d;

				if (this.ScrollBarOrientation == Orientation.Vertical)
					rect.Inflate(ScrollTipOffset, 0);
				else
					rect.Inflate(0, ScrollTipOffset);

				this._recordScrollTip.PlacementRectangle	= rect;
				this._recordScrollTip.PlacementTarget		= this;

				this._recordScrollTip.Placement				= this.ScrollBarOrientation == Orientation.Horizontal
					? PlacementMode.Top
					: PlacementMode.Left;

				if (this.ViewPanelInfo != null)
					this.InitializeScrollTip(this.ViewPanelInfo.GetRecordAtOverallScrollPosition(this.ViewPanelInfo.OverallScrollPosition));
			}
		}

			#endregion //OnDeferredScrollingStarted
    
            #region OnDeferredScrollingEnded

        /// <summary>
		/// Called when the user has completed a scroll thumb drag operation by releasing the mouse.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> This method will only be called if <see cref="GetIsInDeferredScrollingMode"/> returns true.</para>
		/// <para></para>
		/// This is a convenient place for derived classes to cleanup a ToolTip previously created in OnDeferredScrollingStarted.
		/// </remarks>
		/// <seealso cref="GetIsInDeferredScrollingMode"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingStarted"/> cref=""/>
		internal protected override void OnDeferredScrollingEnded(bool cancelled)
        {
			// Hide the tooltip and release the reference
			if (this._recordScrollTip != null)
			{
				this._recordScrollTip.IsOpen	= false;
				this._recordScrollTip			= null;
			}
		}

			#endregion //OnDeferredScrollingEnded
    
            #region OnDeferredScrollOffsetChanged

		/// <summary>
		/// Called when the scroll position changes while we are in deferred drag mode.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> This method will only be called if <see cref="GetIsInDeferredScrollingMode"/> returns true.</para>
		/// </remarks>
		/// <seealso cref="GetIsInDeferredScrollingMode"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingStarted"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingEnded"/> cref=""/>
		internal protected override void OnDeferredScrollOffsetChanged(double newDeferredScrollPosition)
        {
			//JM 01-18-10 TFS25888 - Call the new GetScrollPositionFromOffset method on TilesPanelBase to convert from  Offset to
			// scroll position.
			if (this.ViewPanelInfo		!= null &&
				this.RecordListControl	!= null &&
				this.RecordListControl.HasItems)
				//this.InitializeScrollTip(this.ViewPanelInfo.GetRecordAtOverallScrollPosition((int)newDeferredScrollPosition));
				this.InitializeScrollTip(this.ViewPanelInfo.GetRecordAtOverallScrollPosition((int)base.GetScrollPositionFromOffset(newDeferredScrollPosition)));
		}

			#endregion //OnDeferredScrollOffsetChanged

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has changed.
		/// </summary>
		/// <param name="e">Information about the property that was changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == DataPresenterBase.DataPresenterProperty)
			{
				this._dataPresenter					= e.NewValue as DataPresenterBase;
				this._dataPresenterViewPanelInfo	= this._dataPresenter as IViewPanelInfo;

				if (this._itemsPanelInitialized == false)
					this._dataPresenter.InitializeItemsPanel();
			}
			else
			if (e.Property == CardViewPanel.ViewSettingsProperty)
			{
				// unhook from old
				if (this._viewSettings != null)
					PropertyChangedEventManager.RemoveListener(this._viewSettings, this, string.Empty);

				this._viewSettings = e.NewValue as CardViewSettings;

				// hook new change event
				if (this._viewSettings != null)
				{
					PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);
					this._viewSettings.EnumeratePropertiesWithNonDefaultValues(this.OnViewSettingsPropertyChanged);

					if (this._dataPresenter != null)
					{
						this._dataPresenter.InitializeItemsPanel();
						this._itemsPanelInitialized = true;
					}
				}
			}
			else
			if (e.Property == DataPresenterBase.CurrentViewProperty)
			{
				this._view = e.NewValue as Infragistics.Windows.DataPresenter.ViewBase;

				CardViewSettings viewSettings = this.ViewSettings;

				// Unhook from old
				if (viewSettings != null)
				{
					// Remove the view settings object as our logical child.
					DependencyObject logicalParent = LogicalTreeHelper.GetParent(viewSettings);
					if (logicalParent == this)
						this.RemoveLogicalChild(viewSettings);
				}


				// Bind our ViewSettings property to the View's ViewSettings property.
				this.SetBinding(CardViewPanel.ViewSettingsProperty, Utilities.CreateBindingObject(CardView.ViewSettingsProperty, BindingMode.OneWay, DataPresenterBase.GetCurrentView(this)));

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

			// JM 02-25-10 TFS28237
			#region OnItemsInViewChanged

		/// <summary>
		/// Called when animations have completed after the items in view have changed
		/// </summary>
		protected override void OnItemsInViewChanged()
		{
			IViewPanelInfo info = this.ViewPanelInfo;
			if (info != null)
				info.OnRecordsInViewChanged();
		}

			#endregion //OnItemsInViewChanged

			#region UpdateTransform

		/// <summary>
        /// Called during animations to reposition, resize elements.
        /// </summary>
        /// <remarks>
        /// <para clas="note"><b>Note:</b> derived classeds must override this method to update the RenderTransform for the container</para>
        /// </remarks>
        /// <param name="container">The element being moved.</param>
        /// <param name="originalRect">The original location of the element before the animation started.</param>
        /// <param name="currentRect">The current location of the element.</param>
        /// <param name="targetRect">The target location of the element once the animation has completed.</param>
        /// <param name="offset">Any addition offset to apply to the current rect.</param>
        /// <param name="resizeFactor">A number used during a resize animation where 0 repreents the starting size and 1 represents the ending size.</param>
        /// <param name="calledFromArrange">True is called during the initial arrange pass.</param>
        protected override void UpdateTransform(DependencyObject container, Rect originalRect, Rect currentRect, Rect targetRect, Vector offset, double resizeFactor, bool calledFromArrange)
        {
            CardViewCard card = container as CardViewCard;

            if (card != null)
                card.UpdateTransform(originalRect, currentRect, targetRect, offset, calledFromArrange);
            else
				base.UpdateTransform(container, originalRect, currentRect, targetRect, offset, resizeFactor, calledFromArrange);
        }

            #endregion //UpdateTransform	
        
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ViewSettingsProperty = CardView.ViewSettingsProperty.AddOwner(
			typeof(CardViewPanel), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
			if (value == null)
				return new CardViewSettings();

			return value;
		}

		/// <summary>
		/// Returns/set the <see cref="CarouselViewSettings"/> object for this <see cref="XamCarouselPanel"/>.
		/// </summary>
		/// <seealso cref="ViewSettingsProperty"/>
		//[Description("Returns/set the CardViewSettings object for this CardViewPanel.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public CardViewSettings ViewSettings
		{
			get { return this.GetValue(CardViewPanel.ViewSettingsProperty) as CardViewSettings;	}
			set	{ this.SetValue(CardViewPanel.ViewSettingsProperty, value);	}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (CardViewSettings)CardViewPanel.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ViewSettings = (CardViewSettings)CardViewPanel.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

				#endregion //ViewSettings
	
			#endregion //Public Properties

			#region Private Properties

				#region DataPresenter

		private DataPresenterBase DataPresenter
		{
			get { return this._dataPresenter; }
		}

				#endregion //DataPresenter	

				#region LayoutColumns

		private List<List<ItemInfoBase>> LayoutColumns
		{
			get
			{
				if (this._layoutColumns == null)
				{
					int totalColumns			= base.GetTotalColumnsDisplayed();
					this._layoutColumns			= new List<List<ItemInfoBase>>(totalColumns);

					// Populate this._layoutColumns with 1 List<ItemInfoBase> for each column
					for (int i = 0; i < totalColumns; i++)
					{
						this._layoutColumns.Add(new List<ItemInfoBase>());
					}

					// Add the itemInfos to the appropriate column list
					ItemInfoBase[]	itemInfos		= base.GetItemsInView();
					Array.Sort(itemInfos, new ItemInfoComparer());

					double			lastLeft		= double.MinValue;
					int				currentColumn	= -1;
					foreach (ItemInfoBase itemInfo in itemInfos)
					{
						double left = itemInfo.GetCurrentTargetRect().Left;
						if (left > lastLeft)
						{
							currentColumn++;
							lastLeft = left;
						}

						// JM 03-04-10 TFS28935
						if (currentColumn >= this.LayoutColumns.Count)
							break;

						this._layoutColumns[currentColumn].Add(itemInfo);
					}
				}

				return this._layoutColumns;
			}
		}

				#endregion //LayoutColumns

				#region RecordListControl

		private RecordListControl RecordListControl
		{
			get
			{
				if (this._recordListControl == null)
					this._recordListControl = ItemsControl.GetItemsOwner(this) as RecordListControl;

				return this._recordListControl;
			}
		}

				#endregion //RecordListControl

				#region ScrollBarOrientation

		private Orientation ScrollBarOrientation
		{
			get	
			{ 
				return this.ViewSettings.Orientation == Orientation.Horizontal ?
										Orientation.Vertical :
										Orientation.Horizontal;
			}
		}

				#endregion //ScrollBarOrientation	

				#region ViewPanelInfo

		private IViewPanelInfo ViewPanelInfo
		{
			get { return this._dataPresenterViewPanelInfo; }
		}

				#endregion //ViewPanelInfo

			#endregion //Private Properties

			#region Internal Properties

				#region UncollapsedCardWidth

		internal double UncollapsedCardWidth
		{
			get { return this._uncollapsedCardWidth; }
			set { this._uncollapsedCardWidth = value; }
		}

				#endregion //UncollapsedCardWidth

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Protected Methods

			#endregion //Protected Methods

			#region Internal Methods

				//JM 01-18-10 TFS25888 - Added.
				#region SetScrollPosition

		internal void SetScrollPosition(int scrollPosition)
		{
			base.ScrollPosition = scrollPosition;
		}

				#endregion //SetScrollPosition

				#region SetupSpecialRecordsVersionTracker

		internal void SetupSpecialRecordsVersionTracker(FieldLayout fieldLayout)
		{
			if (fieldLayout != this._specialRecordsVersionTrackerFieldLayout)
			{
				this._specialRecordsVersionTrackerFieldLayout	= fieldLayout;
				this._specialRecordsVersionTracker				= new PropertyValueTracker(fieldLayout, "SpecialRecordsVersion", this.OnSpecialRecordsVersionChanged, true);
			}
		}

				#endregion //SetupSpecialRecordsVersionTracker

			#endregion //InternalMethods

			#region Private Methods

				#region EnsureNotCollapsed

		private Record EnsureNotCollapsed(Record record)
		{
			// JM 03-05-10 TFS29042
			if (record == null)
				return null;

			if (record.IsContainingCardCollapsed == true)
				record.ToggleContainingCardCollapsedState(true);

			return record;
		}

				#endregion //EnsureNotCollapsed

				#region GetLayoutColumn

		private int GetLayoutColumn(ItemInfoBase itemInfo)
		{
			int currentColumn = 0;
			foreach (List<ItemInfoBase> itemInfos in this.LayoutColumns)
			{
				if (itemInfos.Contains(itemInfo))
					return currentColumn;

				currentColumn++;
			}

			return -1;
		}

				#endregion //GetLayoutColumn

				#region GetNavigationTargetRecord

		private Record GetNavigationTargetRecord(PanelNavigationDirection navigationDirection, Record currentRecord, PanelSiblingNavigationStyle siblingNavigationStyle, Type restrictToRecordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;
			if (info == null)
				return null;

			// Setup.
			CardViewSettings viewSettings	= this.ViewSettings;
			bool	orientationIsVertical	= (viewSettings.Orientation == Orientation.Vertical);
			bool	orientationIsHorizontal	= (viewSettings.Orientation == Orientation.Horizontal);

			#region Vertical orientation Logic
			// Look for the target record.
			//
			// If the orientation is vertical, the cards are not necessarily laid out in a uniform grid so we need to analyze the actual layout 
			// rects of the cards to find the target record.
			//
			// If the orientation is horizontal, that means the cards are laid out in a uniform grid pattern and we can find the target record 
			// by analyzing the logical row and column of the current record.
			if (orientationIsVertical)
			{
				ItemInfoBase	currentItemInfo				= base.GetManager().GetItemInfo(currentRecord);
				Rect			currentItemInfoRect			= currentItemInfo.GetCurrentTargetRect();
				int				currentItemInfoColumn		= this.GetLayoutColumn(currentItemInfo);
				int				currentRecordScrollPosition = info.GetOverallScrollPositionForRecord(currentRecord);
				int				totalColumns				= base.GetTotalColumnsDisplayed();

				switch (navigationDirection)
				{
					case PanelNavigationDirection.Above:
					case PanelNavigationDirection.Previous:
						return this.EnsureNotCollapsed(info.GetRecordAtOverallScrollPosition(Math.Max(currentRecordScrollPosition - 1, 0)));

					case PanelNavigationDirection.Below:
					case PanelNavigationDirection.Next:
						return this.EnsureNotCollapsed(info.GetRecordAtOverallScrollPosition(Math.Min(currentRecordScrollPosition + 1, info.OverallScrollCount)));

					case PanelNavigationDirection.Left:
					case PanelNavigationDirection.Right:
					{
						int	targetItemInfoColumn = navigationDirection == PanelNavigationDirection.Left ? currentItemInfoColumn - 1 :
																										  currentItemInfoColumn + 1;

						// If the Target column is not visible, we need to scroll forward/backward to reveal the previous/next column.
						if (targetItemInfoColumn < 0 || targetItemInfoColumn > totalColumns - 1)
						{
							// Save the current offset and compare it below after the scroll to see if we were actually ablt to scroll
							// (i.e., we are not at the beginning/end of the list.
							double currentOffset = base.GetOffsetFromScrollPosition(info.OverallScrollPosition);

							if (targetItemInfoColumn < 0)
							{
								info.OverallScrollPosition = Math.Max(info.OverallScrollPosition - 1, 0);
								this.InvalidateMeasure();
								this.UpdateLayout();

								// Refresh totalColumns toreflect the new layout after the scroll.
								totalColumns			= base.GetTotalColumnsDisplayed();

								targetItemInfoColumn	= 0;
							}
							else
							if (targetItemInfoColumn > totalColumns - 1)
							{
								// Bump the scroll count by the number of items in the first column.
								info.OverallScrollPosition = Math.Min(info.OverallScrollPosition + this.LayoutColumns[0].Count, info.OverallScrollCount);
								this.InvalidateMeasure();
								this.UpdateLayout();

								// Refresh totalColumns toreflect the new layout after the scroll.
								totalColumns			= base.GetTotalColumnsDisplayed();

								targetItemInfoColumn	= totalColumns - 1;
							}

							// If the offset is the same, then we couldn't scroll so just return null.
							if (currentOffset == base.GetOffsetFromScrollPosition(info.OverallScrollPosition))
								return null;
						}

						// Establish the column to look in for the target record.
						for (int i = 0; i < this.LayoutColumns[targetItemInfoColumn].Count; i++)
						{
							ItemInfoBase itemInfo = this.LayoutColumns[targetItemInfoColumn][i];

							// If the top of the Target ItemInfo is in the top half of the Current ItemInfo or
							// if the bottom of the Target ItemInfo is in the bottom half of the Current ItemInfo, return it.
							double midPoint = currentItemInfoRect.Top + ((currentItemInfoRect.Bottom - currentItemInfoRect.Top) / 2);
							if (itemInfo.GetCurrentTargetRect().Top >= currentItemInfoRect.Top &&
								itemInfo.GetCurrentTargetRect().Top <= midPoint)
								return this.EnsureNotCollapsed(itemInfo.Item as Record);

							if (itemInfo.GetCurrentTargetRect().Bottom <= currentItemInfoRect.Bottom &&
								itemInfo.GetCurrentTargetRect().Bottom >= midPoint)
								return this.EnsureNotCollapsed(itemInfo.Item as Record);
						}

						// Return the last record in the target column.
						// JM 03-08-10 TFS29042
						//return this.EnsureNotCollapsed(this.LayoutColumns[targetItemInfoColumn][this.LayoutColumns[targetItemInfoColumn].Count].Item as Record);
						return this.EnsureNotCollapsed(this.LayoutColumns[targetItemInfoColumn][this.LayoutColumns[targetItemInfoColumn].Count - 1].Item as Record);
					}
				}

				return null;
			}
			#endregion //Vertical orientation Logic

			#region Horizontal orientation Logic
			// Orientation is Horizontal.
			bool	getPrevious				= false;
			bool	getNext					= false;
			bool	getSameInPreviousRow	= false;
			bool	getSameInNextRow		= false;


			// Establish whether we are looking for the next record or the previous record.
			switch (navigationDirection)
			{
				case PanelNavigationDirection.Above:
					if (orientationIsVertical)
						getPrevious = true;
					else
						getSameInPreviousRow = true;

					break;
				case PanelNavigationDirection.Below:
					if (orientationIsVertical)
						getNext = true;
					else
						getSameInNextRow = true;

					break;
				case PanelNavigationDirection.Left:
					if (orientationIsVertical)
						getSameInPreviousRow = true;
					else
						getPrevious = true;

					break;
				case PanelNavigationDirection.Right:
					if (orientationIsVertical)
						getSameInNextRow = true;
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

			Record	fromRecord			= currentRecord;
			Record	targetRecord		= null;
			int		totalLogicalCols	= orientationIsVertical ? base.GetTotalRowsDisplayed() : base.GetTotalColumnsDisplayed();
			while (targetRecord == null)
			{
				// Determine the 'from record's' position in the overall list.
				int fromRecordScrollPosition = info.GetOverallScrollPositionForRecord(fromRecord);

				if (fromRecordScrollPosition == -1)
					return null;

				int targetRecordScrollPosition = -1;

				if (getPrevious == true)
					targetRecordScrollPosition = fromRecordScrollPosition - 1;
				else
				if (getNext == true)
					targetRecordScrollPosition = fromRecordScrollPosition + 1;
				else
				if (getSameInPreviousRow == true)
					targetRecordScrollPosition = fromRecordScrollPosition - totalLogicalCols;
				else
				if (getSameInNextRow == true)
					targetRecordScrollPosition = fromRecordScrollPosition + totalLogicalCols;

				if (targetRecordScrollPosition == -1	||
					targetRecordScrollPosition < 0		||
					targetRecordScrollPosition > info.OverallScrollCount - 1)
					return null;

				targetRecord = info.GetRecordAtOverallScrollPosition(targetRecordScrollPosition);


				// Make sure the target record we found matches the restrictToRecordType.
				if (targetRecord			== null					||
					targetRecord.GetType()	== restrictToRecordType ||
					targetRecord.GetType().IsSubclassOf(restrictToRecordType))
					return this.EnsureNotCollapsed(targetRecord);


				// Since the target record does not match the restrictToRecordType set our 'from record'
				// to the target record and try again to find a target record of the restrictToRecordType.
				fromRecord		= targetRecord;
				targetRecord	= null;
			}

			return null;
			#endregion //Horizontal orientation Logic
		}

				#endregion //GetNavigationTargetRecord

				#region InitializeScrollTip

			private void InitializeScrollTip(Record topRecord)
			{
				if (this._recordScrollTip != null)
				{
					List<RecordScrollTipInfo> list = new List<RecordScrollTipInfo>();
					list.Add(RecordScrollTipInfo.Create(topRecord));

					this._recordScrollTip.DataContext	= list;
					this._recordScrollTip.Content		= this._recordScrollTip.DataContext;
					this._recordScrollTip.IsOpen		= true;
				}
			} 

				#endregion //InitializeScrollTip

				#region InvalidateMeasureHelper

		private void InvalidateMeasureHelper()
		{
			this.InvalidateMeasure();

			UIElement parent = this.Parent as UIElement;
			if (parent != null)
				parent.InvalidateMeasure();
		}

				#endregion //InvalidateMeasureHelper

				#region OnSpecialRecordsVersionChanged

		private void OnSpecialRecordsVersionChanged()
		{
			this.InvalidateMeasure();
		}

				#endregion //OnSpecialRecordsVersionChanged

				#region OnViewSettingsPropertyChanged

		void OnViewSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            // If we haven't been initialized yet then we can ignore the change notification
            if (this._dataPresenter == null)
                return;

			switch (e.PropertyName)
			{
				case "AutoFitCards":
				case "CardHeight":
				case "CardWidth":
				case "CollapseCardButtonVisibility":
				case "CollapseEmptyCellsButtonVisibility":
				case "HeaderPath":
				case "HeaderVisibility":
				case "InterCardSpacingX":
				case "InterCardSpacingY":
				case "MaxCardCols":
				case "MaxCardRows":
				case "Orientation":
				case "Padding":
				case "ShouldCollapseCards":
				case "ShouldCollapseEmptyCells":
					this.InvalidateMeasureHelper();

					break;

				default:
					break;
			}
		}
		
				#endregion ///OnViewSettingsPropertyChanged

			#endregion //Private Methods

		#endregion //Methods

		#region IViewPanel Members

			#region EnsureCellIsVisible

		bool IViewPanel.EnsureCellIsVisible(Cell cell)
		{
			Debug.Assert(cell != null);
			if (cell == null)
				return false;

			DataRecord record = cell.Record;
			Debug.Assert(record != null);
			if (record == null)
				return false;

			DataRecordPresenter dataRecordPresenter = record.AssociatedRecordPresenter as DataRecordPresenter;
			if (dataRecordPresenter == null)
			{
				((IViewPanel)this).EnsureRecordIsVisible(cell.Record);

				// Call UpdateLayout to force the record to come into view before returning
				this.UpdateLayout();

				dataRecordPresenter = record.AssociatedRecordPresenter as DataRecordPresenter;
			}

			if (dataRecordPresenter != null)
			{
				CellValuePresenter[] cellPresenters = dataRecordPresenter.GetChildCellValuePresenters();
				CellValuePresenter cvp				= null;

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
					cvp.BringIntoView();

					return true;
				}
			}

			return false;
		}

			#endregion //EnsureCellIsVisible

			#region EnsureRecordIsVisible

		bool IViewPanel.EnsureRecordIsVisible(Record record)
		{
		    if (record == null)
		        throw new ArgumentNullException("record");

		    if (this.DataPresenter == null)
		        return false;

		    // Check with the dp if it is ok to scroll
			IViewPanelInfo info = this.ViewPanelInfo;
			if (!info.IsOkToScroll())
		        return false;

			int scrollPosOfRecordToMakeVisible = info.GetOverallScrollPositionForRecord(record);

			// fixed records return -1 for scrollpos so we can ignore these
			if (scrollPosOfRecordToMakeVisible < 0)
				return true;

			int firstRecordScrollPos	= this.ScrollIndexOfFirstArrangedItem;
			int lastRecordScrollPos		= this.ScrollIndexOfLastArrangedItem;
			if (scrollPosOfRecordToMakeVisible >= firstRecordScrollPos &&
				scrollPosOfRecordToMakeVisible <= lastRecordScrollPos)
			{
				return true;
			}


			if (scrollPosOfRecordToMakeVisible < firstRecordScrollPos)
			{
				base.ScrollPosition = scrollPosOfRecordToMakeVisible;

				return true;
			}


			// The record is beyond the last visible record.
			int scrollDifferential			= scrollPosOfRecordToMakeVisible - lastRecordScrollPos;
			int newScrollPositionCandidate	= base.ScrollPosition + scrollDifferential;
			
			// If the record represented by the newScrollPositionCandidate is not the first record in the row/column
			// (TilesPanelBase will not necessarily make the record at the new scrollposition the first record - it makes
			// the row/col containing the record at the new scrollposition the first row/col) add enough to the
			// scrollposition  to make it so.
			if (newScrollPositionCandidate != 0)
			{
				double offsetOfTargetRecord			= base.GetOffsetFromScrollPosition(newScrollPositionCandidate);
				double offsetOfTargetRecordMinusOne = base.GetOffsetFromScrollPosition(newScrollPositionCandidate - 1);
				if (offsetOfTargetRecord == offsetOfTargetRecordMinusOne)
				{
					int scrollPosTemp = newScrollPositionCandidate + 1;
					while (true)
					{
						if (base.GetOffsetFromScrollPosition(scrollPosTemp) == offsetOfTargetRecord)
						{
							scrollPosTemp++;
							if (scrollPosTemp >= info.OverallScrollCount)
								break;
						}
						else
							break;
					}

					newScrollPositionCandidate = scrollPosTemp;
				}
			}

			base.ScrollPosition = newScrollPositionCandidate;

			return true;
		}

			#endregion //EnsureRecordIsVisible

			#region GetFirstDisplayedRecord

		Record IViewPanel.GetFirstDisplayedRecord(Type recordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;
			if (info == null)
		        return null;


		    // If there are no records, return null.
		    if (info.OverallScrollCount < 1)
		        return null;


		    // If the top record is of the requested type, return it.
			Record topRecord = info.GetRecordAtOverallScrollPosition(info.OverallScrollPosition);
			// JM 12/23/09 TFS25903 - Check for null
			if (topRecord != null && (topRecord.GetType() == recordType	|| topRecord.GetType().IsSubclassOf(recordType)))
		        return topRecord;


			// Since the top record wasn't of the requested type, try looking forwards from the TopRecord
			// for a record of the requested type.
			int startIndex						= info.OverallScrollPosition + 1;
			int scrollIndexOfLastVisibleRecord	= this.ScrollIndexOfLastArrangedItem;
			Record record;

			for (int i = startIndex; i <= scrollIndexOfLastVisibleRecord; i++)
			{
				record = info.GetRecordAtOverallScrollPosition(i);
				// JM 12/23/09 TFS25903 - Check for null
				if (record != null && (record.GetType() == recordType || record.GetType().IsSubclassOf(recordType)))
					return record;
			}

		    return null;
		}

			#endregion //GetFirstDisplayedRecord

			#region GetFirstOverallRecord

		Record IViewPanel.GetFirstOverallRecord(Type recordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;
		    if (info == null)
		        return null;


		    int		scrollCount = info.OverallScrollCount;
		    Record	record;

		    for (int i = 0; i < scrollCount; i++)
		    {
		        record = info.GetRecordAtOverallScrollPosition(i);
				// JM 12/23/09 TFS25903 - Check for null
				if (record != null && (record.GetType() == recordType || record.GetType().IsSubclassOf(recordType)))
		            return record;
		    }

		    return null;
		}

			#endregion //GetFirstOverallRecord

			#region GetLastDisplayedRecord

		Record IViewPanel.GetLastDisplayedRecord(Type recordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;
		    if (info == null)
		        return null;

			int scrollIndexOfLastVisibleRecord = this.ScrollIndexOfLastArrangedItem;
			if (scrollIndexOfLastVisibleRecord < 0)
				return null;


			Record lastVisibleRecord = info.GetRecordAtOverallScrollPosition(scrollIndexOfLastVisibleRecord);


			// If the last visible record is of the requested type, return it.
			// JM 12/23/09 TFS25903 - Check for null
			if (lastVisibleRecord != null && (lastVisibleRecord.GetType() == recordType || lastVisibleRecord.GetType().IsSubclassOf(recordType)))
				return lastVisibleRecord;


			// Since the last visible record wasn't of the requested type, try looking backwards from the last visible record
			// for a record of the requested type.
			int startIndex = scrollIndexOfLastVisibleRecord - 1;
			Record record;

			for (int i = startIndex; i > -1; i--)
			{
				record = info.GetRecordAtOverallScrollPosition(i);
				// JM 12/23/09 TFS25903 - Check for null
				if (record != null && (record.GetType() == recordType || record.GetType().IsSubclassOf(recordType)))
					return record;
			}

		    return null;
		}

			#endregion //GetLastDisplayedRecord

			#region GetLastOverallRecord

		Record IViewPanel.GetLastOverallRecord(Type recordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;
		    if (info == null)
		        return null;

		    int		scrollCount = info.OverallScrollCount;
		    Record	record;

		    for (int i = scrollCount - 1; i >= 0; i--)
		    {
		        record = info.GetRecordAtOverallScrollPosition(i);
				// JM 12/23/09 TFS25903 - Check for null
				if (record != null && (record.GetType() == recordType || record.GetType().IsSubclassOf(recordType)))
		            return record;
		    }

		    return null;
		}

			#endregion //GetLastOverallRecord

			#region GetNavigationTargetRecord

		Record IViewPanel.GetNavigationTargetRecord(Record currentRecord, PanelNavigationDirection navigationDirection, ISelectionHost selectionHost, bool shiftKeyDown, bool ctlKeyDown, PanelSiblingNavigationStyle siblingNavigationStyle, Type restrictToRecordType)
		{
			IViewPanelInfo info = this.ViewPanelInfo;
		    if (info == null)
		        return null;

		    // Validate parameters.
		    if (currentRecord == null)
		        throw new ArgumentNullException("currentRecord");
		    if (selectionHost == null)
		        throw new ArgumentNullException("selectionHost");


		    // Get the selection strategy
		    SelectionStrategyBase selectionStrategy = selectionHost.GetSelectionStrategyForItem(currentRecord as ISelectableItem);

		    Debug.Assert(selectionStrategy != null);
		    if (selectionStrategy == null)
		        throw new InvalidOperationException("SelectionStrategy is null!");


		    // Establish the 'candidate record' we would like to navigate to based on the current record and the navigation
		    // direction and see if the selection strategy will let us go there.  First try looking for a candidate without
		    // limiting the search to the same parent as the current record (if our caller allows this).  If that record 
		    // cannot be navigated to, then look with the same parent.
		    Record candidateRecord = this.GetNavigationTargetRecord(navigationDirection, currentRecord, siblingNavigationStyle, restrictToRecordType);
		    if (candidateRecord == null)
		        return null;

		    if (selectionStrategy.CanItemBeNavigatedTo(candidateRecord, shiftKeyDown, ctlKeyDown) == true)
		        return candidateRecord;

		    return null;
		}

			#endregion //GetNavigationTargetRecord

			#region LayoutStyle

		PanelLayoutStyle IViewPanel.LayoutStyle
		{
		    get { return PanelLayoutStyle.Custom; }
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
			// JM 02-24-10 TFS28244 - Need to turn off animations because callers of this routine are calling UpdateLayout
			// after the scroll and expecting the scroll to happen synchronously. If animations are turned on that will not
			// be the case.
			bool animationsTurnedOff = false;
			if (this.ViewSettings.ShouldAnimateCardPositioning)
			{
				this.ViewSettings.ShouldAnimateCardPositioning = false;
				animationsTurnedOff = true;
			}

		    switch (scrollType)
		    {
		        case PanelNavigationScrollType.PageAbove:
					if (this.ScrollBarOrientation == Orientation.Vertical)
						((IScrollInfo)this).PageUp();

		            break;

		        case PanelNavigationScrollType.PageBelow:
					if (this.ScrollBarOrientation == Orientation.Vertical)
						((IScrollInfo)this).PageDown();

		            break;

		        case PanelNavigationScrollType.PageLeft:
					if (this.ScrollBarOrientation == Orientation.Horizontal)
						((IScrollInfo)this).PageLeft();

		            break;

		        case PanelNavigationScrollType.PageRight:
					if (this.ScrollBarOrientation == Orientation.Horizontal)
						((IScrollInfo)this).PageRight();

		            break;
		    }

			// JM 02-24-10 TFS28244 - If we turned off animations above, call UpdateLayout before turning them back on so that
			// callers will see the effect of the scroll.  Note that callers are probably calling UpdateLayout after this method
			// returns, but that call will be too late since we would have already turned animations back on here.
			if (animationsTurnedOff)
			{
				this.UpdateLayout();
				this.ViewSettings.ShouldAnimateCardPositioning = true;
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
					if (sender is CardViewSettings)
					{
						if (sender == this._viewSettings)
						{
							this.OnViewSettingsPropertyChanged(sender, args);
							return true;
						}

						Debug.Fail("Invalid sender object in ReceiveWeakEvent for CardViewPanel, sender type: " + sender != null ? sender.ToString() : "null");
						return true;
					}
					
					//else if (sender is RecordManager)
					//{
					//    switch (args.PropertyName)
					//    {
					//        case "UnderlyingDataVersion":
					//            {
					//                IViewPanelInfo info = this.ViewPanelInfo;
					//                int effectiveScrollPosition = this.EffectiveScrollPosition;

					//                if (effectiveScrollPosition != (int)this.VerticalOffset ||
					//                    effectiveScrollPosition >= info.OverallScrollCount)
					//                    this.EffectiveScrollPosition = Math.Max(0, effectiveScrollPosition);;

					//                this.InvalidateMeasure();
					//                return true;
					//            }

					//        default:
					//            return true;
					//    }
					//}

					Debug.Fail("Invalid sender type in ReceiveWeakEvent for CardViewPanel, sender type: " + sender != null ? sender.GetType().ToString() : "null");
					return false;
				}

				Debug.Fail("Invalid args in ReceiveWeakEvent for GridViewPanel, arg type: " + e != null ? e.ToString() : "null");

				return false;
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for GridViewPanel, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion //IWeakEventListener Members

		#region Nested ItemInfoComparer Class

		/// <summary>
		/// Comparer used for sorting ItemInfoBase instances based on their rects, top to bottom, left to right.
		/// </summary>
		private class ItemInfoComparer : IComparer<ItemInfoBase>
		{
			internal ItemInfoComparer()
			{
			}

			public int Compare(ItemInfoBase x, ItemInfoBase y)
			{
				Rect xRect = x.GetCurrentTargetRect();
				Rect yRect = y.GetCurrentTargetRect();

				if (xRect.Left != yRect.Left)
					return (int)xRect.Left - (int)yRect.Left;

				return (int)xRect.Top - (int)yRect.Top;
			}
		}

		#endregion // ItemInfoComparer Class

        #region Nested TileConstraints class

        private class TileConstraints : ITileConstraints
        {
            internal HorizontalAlignment? HorizontalAlignment { get; set; }
            internal Thickness? Margin { get; set; }
            internal double MaxHeight { get; set; }
            internal double MaxWidth { get; set; }
            internal double MinHeight { get; set; }
            internal double MinWidth { get; set; }
            internal double PreferredHeight { get; set; }
            internal double PreferredWidth { get; set; }
            internal VerticalAlignment? VerticalAlignment { get; set; }

            internal TileConstraints()
            {
                MaxHeight = double.PositiveInfinity;
                MaxWidth = double.PositiveInfinity;
                PreferredHeight = double.NaN;
                PreferredWidth = double.NaN;
            }

            #region ITileConstraints Members

            HorizontalAlignment? ITileConstraints.HorizontalAlignment
            {
                get { return HorizontalAlignment; }
            }

            Thickness? ITileConstraints.Margin
            {
                get { return Margin; }
            }

            double ITileConstraints.MaxHeight
            {
                get { return MaxHeight; }
            }

            double ITileConstraints.MaxWidth
            {
                get { return MaxWidth; }
            }

            double ITileConstraints.MinHeight
            {
                get { return MinHeight; }
            }

            double ITileConstraints.MinWidth
            {
                get { return MinWidth; }
            }

            double ITileConstraints.PreferredHeight
            {
                get { return PreferredHeight; }
            }

            double ITileConstraints.PreferredWidth
            {
                get { return PreferredWidth; }
            }

            VerticalAlignment? ITileConstraints.VerticalAlignment
            {
                get { return VerticalAlignment; }
            }

            #endregion

        #endregion //Nested TileConstraints class
        }
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