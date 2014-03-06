using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Contains settings that support controls which implement Carousel views (e.g., <see cref="XamCarouselPanel"/> and <see cref="XamCarouselListBox"/>)).
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="XamCarouselPanel"/> and the <see cref="XamCarouselListBox"/> each expose a ViewSettings property (<see cref="XamCarouselPanel.ViewSettings"/> and <see cref="XamCarouselListBox.ViewSettings"/>) of type CarouselViewSettings.
	/// By manipulating properties on the CarouselViewSettings object you can control how the <see cref="XamCarouselPanel"/> arranges and applies effects to items.</p>
    /// <p class="body">A <i>partial</i> list of the properties that affect layout and parent effects include:
	///		<ul>
	///			<li><see cref="CarouselViewSettings.ItemPath"/> - Specifies the path along which items are arranged</li>
	///			<li><see cref="CarouselViewSettings.ItemPathRenderBrush"/> - Specifies a Brush used to fill the path</li>
	///			<li><see cref="CarouselViewSettings.ItemPathRenderPen"/> - Specifies a Pen used to outline the path</li>
	///			<li><see cref="CarouselViewSettings.ItemSize"/> - Specifies the size of items</li>
	///			<li><see cref="CarouselViewSettings.ItemsPerPage"/> - Specifies the maximum number of items that should be displayed at any one time</li>
	///			<li><see cref="CarouselViewSettings.AutoScaleItemContentsToFit"/> - Specifies whether the contents of each item should be scaled to fit within the size specified for each item</li>
	///			<li><see cref="CarouselViewSettings.OpacityEffectStops"/> - Specifies one or more EffectStops (i.e., Offset+Value pairs) that describe how Opacity effects should be applied to each item based on its position in the XamCarouselPanel</li>
	///			<li><see cref="CarouselViewSettings.ScalingEffectStops"/> - Specifies one or more EffectStops (i.e., Offset+Value pairs) that describe how Scaling effects should be applied to each item based on its position in the XamCarouselPanel</li>
	///			<li><see cref="CarouselViewSettings.SkewAngleXEffectStops"/> - Specifies one or more EffectStops (i.e., Offset+Value pairs) that describe how Skewing about the X-axis should be applied to each item based on its position in the XamCarouselPanel</li>
	///			<li><see cref="CarouselViewSettings.SkewAngleYEffectStops"/> - Specifies one or more EffectStops (i.e., Offset+Value pairs) that describe how Skewing about the Y-axis should be applied to each item based on its position in the XamCarouselPanel</li>
	///			<li><see cref="CarouselViewSettings.ZOrderEffectStops"/> - Specifies one or more EffectStops (i.e., Offset+Value pairs) that describe how ZOrder effects should be applied to each item based on its position in the XamCarouselPanel</li>
	///		</ul>
    /// Refer to the documentation contained within for a complete list of the properties supported by this class and the functionality enabled by each property.
	/// </p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselPanel"/>
	/// <seealso cref="XamCarouselPanel.ViewSettings"/>
	/// <seealso cref="XamCarouselListBox"/>
	/// <seealso cref="XamCarouselListBox.ViewSettings"/>
	[StyleTypedProperty(Property="CarouselPanelNavigatorStyle", StyleTargetType=typeof(CarouselPanelNavigator))]	// AS 5/3/07
	public class CarouselViewSettings : ViewSettingsBase
	{
		#region Member Variables

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of CarouselViewSettings.
		/// </summary>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselPanel.ViewSettings"/>
		/// <seealso cref="XamCarouselListBox"/>
		/// <seealso cref="XamCarouselListBox.ViewSettings"/>
		/// <remarks>
		/// <p class="body">The <see cref="XamCarouselPanel"/> and <see cref="XamCarouselListBox"/> will automatically create an instance of this class when their repsective ViewSettings properties are accessed.
		/// You can also create one manually and assign it to the ViewSettings property of one or more instances of these controls if needed.  </p>
		/// </remarks>
		public CarouselViewSettings()
		{
			this.SetValue(CarouselViewSettings.OpacityEffectStopsProperty, new OpacityEffectStopCollection());
			this.SetValue(CarouselViewSettings.ScalingEffectStopsProperty, new ScalingEffectStopCollection());
			this.SetValue(CarouselViewSettings.SkewAngleXEffectStopsProperty, new SkewAngleXEffectStopCollection());
			this.SetValue(CarouselViewSettings.SkewAngleYEffectStopsProperty, new SkewAngleYEffectStopCollection());
			this.SetValue(CarouselViewSettings.ZOrderEffectStopsProperty, new ZOrderEffectStopCollection());
		}

		#endregion //Constructor

		#region Constants

		private static readonly Size							DEFAULT_ITEM_SIZE = new Size(150, 100);
		internal const int										DEFAULT_ITEMS_PER_PAGE = 5;

		private const double									DEFAULT_PATH_PREFIX_PCT = .15;
		private const double									DEFAULT_PATH_SUFFIX_PCT = .15;

		private const double									DEFAULT_WIDTH_IN_INFINITE_CONTAINERS = 400;
		private const double									DEFAULT_HEIGHT_IN_INFINITE_CONTAINERS = 300;

		#endregion //Constants

		#region Base class overrides

			#region OnControlInitialized

		/// <summary>
		/// Called when the control that owns this ViewSettingsBase derived object has its OnInitialized method called.
		/// </summary>
		/// <remarks>
		/// <p class="body">Override this method in derived classes to perform work that needs to wait until after the owning control is initialized.</p>
		/// </remarks>
		internal protected override void OnControlInitialized()
		{
		}

			#endregion //OnControlInitialized	

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property value changes.
		/// </summary>
		/// <param name="e">An instance of DependencyPropertyChangedEventArgs that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property != CarouselViewSettings.VersionProperty)
			{
				// Update our version number.
				// JM 06-30-10 TFS24461 - Do not do this in design mode.
				if (false == DesignerProperties.GetIsInDesignMode(this))
					this.SetValue(CarouselViewSettings.VersionPropertyKey, this.Version + 1);
			}
		}

			#endregion //OnPropertyChanged	
    
			#region Reset

		/// <summary>
		/// Resets all properties to their default values.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Reset()
		{
			this.ResetAutoScaleItemContentsToFit();
			this.ResetCarouselPanelNavigatorStyle();
			this.ResetHeightInInfiniteContainers();
			this.ResetIsListContinuous();
			this.ResetIsNavigatorVisible();
			this.ResetItemHorizontalScrollBarVisibility();
			this.ResetItemPath();
			this.ResetItemPathAutoPad();
			this.ResetItemPathHorizontalAlignment();
			this.ResetItemPathPadding();
			this.ResetItemPathPrefixPercent();
			this.ResetItemPathRenderBrush();
			this.ResetItemPathRenderPen();
			this.ResetItemPathStretch();
			this.ResetItemPathSuffixPercent();
			this.ResetItemPathVerticalAlignment();
			this.ResetItemSize();
			this.ResetItemsPerPage();
			this.ResetItemTransitionStyle();
			this.ResetItemVerticalScrollBarVisibility();
			this.ResetOpacityEffectStopDirection();
			this.ResetOpacityEffectStops();
			this.ResetReserveSpaceForReflections();
			this.ResetRotateItemsWithPathTangent();
			this.ResetScalingEffectStops();
			this.ResetScalingEffectStopDirection();
			this.ResetShouldAnimateItemsOnListChange();	
			this.ResetShouldScrollItemsIntoInitialPosition();
			this.ResetSkewAngleXEffectStops();
			this.ResetSkewAngleXEffectStopDirection();
			this.ResetSkewAngleYEffectStops();
			this.ResetSkewAngleYEffectStopDirection();
			this.ResetUseOpacity();
			this.ResetUseScaling();
			this.ResetUseSkewing();
			this.ResetUseZOrder();
			this.ResetWidthInInfiniteContainers();
			this.ResetZOrderEffectStopDirection();
			this.ResetZOrderEffectStops();
		}

			#endregion //Reset

			#region ShouldSerialize

		/// <summary>
		/// Returns whether this object should be serialized.
		/// </summary>
		/// <returns>Returns true if the value of any property on this object is set to a non-default value, otherwise returns false.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool ShouldSerialize()
		{
			return (this.ShouldSerializeAutoScaleItemContentsToFit()			||
					this.ShouldSerializeCarouselPanelNavigatorStyle()			||
					this.ShouldSerializeHeightInInfiniteContainers()			||
					this.ShouldSerializeIsListContinuous()						||
					this.ShouldSerializeIsNavigatorVisible()					||
					this.ShouldSerializeItemHorizontalScrollBarVisibility()		||
					this.ShouldSerializeItemPath()								||
					this.ShouldSerializeItemPathAutoPad()						||
					this.ShouldSerializeItemPathHorizontalAlignment()			||
					this.ShouldSerializeItemPathPadding()						||
					this.ShouldSerializeItemPathPrefixPercent()					||
					this.ShouldSerializeItemPathRenderBrush()					||
					this.ShouldSerializeItemPathRenderPen()						||
					this.ShouldSerializeItemPathStretch()						||
					this.ShouldSerializeItemPathSuffixPercent()					||
					this.ShouldSerializeItemPathVerticalAlignment()				||
					this.ShouldSerializeItemSize()								||
					this.ShouldSerializeItemsPerPage()							||
					this.ShouldSerializeItemTransitionStyle()					||
					this.ShouldSerializeItemVerticalScrollBarVisibility()		||
					this.ShouldSerializeOpacityEffectStopDirection()			||
					this.ShouldSerializeOpacityEffectStops()					||
					this.ShouldSerializeReserveSpaceForReflections()			||
					this.ShouldSerializeRotateItemsWithPathTangent()			||
					this.ShouldSerializeScalingEffectStopDirection()			||
					this.ShouldSerializeScalingEffectStops()					||
					this.ShouldSerializeShouldAnimateItemsOnListChange()		|| 
					this.ShouldSerializeShouldScrollItemsIntoInitialPosition()	||
					this.ShouldSerializeSkewAngleXEffectStopDirection() ||
					this.ShouldSerializeSkewAngleXEffectStops()			||
					this.ShouldSerializeSkewAngleYEffectStopDirection() ||
					this.ShouldSerializeSkewAngleYEffectStops()		||
					this.ShouldSerializeUseOpacity()				||
					this.ShouldSerializeUseScaling()				||
					this.ShouldSerializeUseSkewing()				||
					this.ShouldSerializeUseZOrder()					||
					this.ShouldSerializeWidthInInfiniteContainers() ||
					this.ShouldSerializeZOrderEffectStopDirection() ||
					this.ShouldSerializeZOrderEffectStops());
		}

			#endregion //ShouldSerialize

		#endregion //Base class overrides
        
        #region Properties

            #region Public Properties

				#region AutoScaleItemContentsToFit

		/// <summary>
		/// Identifies the <see cref="AutoScaleItemContentsToFit"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoScaleItemContentsToFitProperty = DependencyProperty.Register("AutoScaleItemContentsToFit",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether the <see cref="XamCarouselPanel"/> should automatically scale the contents of each item to fit within the bounds of the Item.  The default is False. 
		/// </summary>
		/// <remarks>
		/// <p class="body">The bounds of the Item is determined by the <see cref="ItemSize"/> property.  Note that while you can indirectly affect the size of the items
        /// by using <see cref="ScalingEffectStops"/>, scaling effects are applied to items at render time <i>after</i> the setting of this property has taken effect.</p>
		/// <p class="body">When the AutoScalItemContentsToFit property is set to false, scrollbars will be shown by default within the item if necessary (i.e., if the size of the contents is greater than the size of the item) to enable scrolling of the contents.
		/// For explicit control over whether scrollbars are displayed in this case, you can set the <see cref="ItemHorizontalScrollBarVisibility"/> and <see cref="ItemVerticalScrollBarVisibility"/> properties.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="AutoScaleItemContentsToFitProperty"/>
		/// <seealso cref="ItemHorizontalScrollBarVisibility"/>
		/// <seealso cref="ItemVerticalScrollBarVisibility"/>
        /// <seealso cref="ScalingEffectStops"/>
        //[Description("Returns/sets whether the XamCarouselPanel automatically scales the contents of each item to fit within the Item, the size of which is determined by the ItemSize property.  The default is False")]
		//[Category("Appearance")]
		public bool AutoScaleItemContentsToFit
		{
			get { return (bool)this.GetValue(CarouselViewSettings.AutoScaleItemContentsToFitProperty); }
			set { this.SetValue(CarouselViewSettings.AutoScaleItemContentsToFitProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="AutoScaleItemContentsToFit"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAutoScaleItemContentsToFit()
		{
			return this.AutoScaleItemContentsToFit != (bool)CarouselViewSettings.AutoScaleItemContentsToFitProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="AutoScaleItemContentsToFit"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAutoScaleItemContentsToFit()
		{
			this.ClearValue(AutoScaleItemContentsToFitProperty);
		}

				#endregion //AutoScaleItemContentsToFit

				#region CarouselPanelNavigatorStyle

		/// <summary>
		/// Identifies the <see cref="CarouselPanelNavigatorStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CarouselPanelNavigatorStyleProperty = DependencyProperty.Register("CarouselPanelNavigatorStyle",
			typeof(Style), typeof(CarouselViewSettings), new FrameworkPropertyMetadata((Style)null));

		/// <summary>
		/// Returns/sets the style for the <see cref="CarouselPanelNavigator"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="CarouselPanelNavigator"/> control provides a UI for scrolling items in a <see cref="XamCarouselPanel"/></p>
		/// <p class="body">Setting the <see cref="CarouselPanelNavigator"/> Style is the most convenient way to control the placement of the <see cref="CarouselPanelNavigator"/> in the <see cref="XamCarouselPanel"/>.  
		/// By creating a style that contains Setters for the HorizontalAlignment and/or VerticalAlignment properties along with the Margin property, and assigning it 
		/// to this property, you can place the navigator in a location that works best for the <see cref="ItemPath"/> you are using.</p>
        /// <p class="body">By default the navigator is placed in the lower right corner of the control.</p>
        /// <p class="body">The <see cref="CarouselPanelNavigator"/> displays buttons that execute <see cref="XamCarouselPanelCommands"/> when clicked, in order to scroll items.  
        /// If you want to provide your own UI for scrollig items, you can hide the <see cref="CarouselPanelNavigator"/> by setting the <see cref="IsNavigatorVisible"/> 
        /// property to hidden and then creating a UI that executes the same scrolling commands used by the navigator.</p>
        /// </remarks>
		/// <seealso cref="CarouselPanelNavigatorStyleProperty"/>
		/// <seealso cref="CarouselPanelNavigator"/>
        /// <seealso cref="XamCarouselPanelCommands"/>
        /// <seealso cref="IsNavigatorVisible"/>
        /// <seealso cref="XamCarouselPanelCommands.NavigateToPreviousPage"/>
        /// <seealso cref="XamCarouselPanelCommands.NavigateToNextPage"/>
        /// <seealso cref="XamCarouselPanelCommands.NavigateToPreviousItem"/>
        /// <seealso cref="XamCarouselPanelCommands.NavigateToNextItem"/>
        //[Description("Returns/sets the style of the CarouselPanelNavigator.")]
		//[Category("Appearance")]
		public Style CarouselPanelNavigatorStyle
		{
			get { return (Style)this.GetValue(CarouselViewSettings.CarouselPanelNavigatorStyleProperty); }
			set { this.SetValue(CarouselViewSettings.CarouselPanelNavigatorStyleProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="CarouselPanelNavigatorStyle"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCarouselPanelNavigatorStyle()
		{
			return this.CarouselPanelNavigatorStyle != null;
		}

		/// <summary>
		/// Resets the <see cref="CarouselPanelNavigatorStyle"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCarouselPanelNavigatorStyle()
		{
			this.ClearValue(CarouselPanelNavigatorStyleProperty);
		}

				#endregion //CarouselPanelNavigatorStyle

				#region HeightInInfiniteContainers

		/// <summary>
		/// Identifies the <see cref="HeightInInfiniteContainers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeightInInfiniteContainersProperty = DependencyProperty.Register("HeightInInfiniteContainers",
			typeof(double), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(CarouselViewSettings.DEFAULT_HEIGHT_IN_INFINITE_CONTAINERS), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets a value that is used as the default for the height of the <see cref="XamCarouselPanel"/>, when it is placed in a container with infinite height available.  The default is 300.
		/// </summary>
		/// <remarks>
		/// <p class="body">Certain controls such as <see cref="System.Windows.Controls.ScrollViewer"/> and <see cref="System.Windows.Controls.StackPanel"/> make an infinite amount of height and width available to the controls they contain.
		/// If you place a control that implements a Carousel view (e.g., <see cref="XamCarouselPanel"/> or <see cref="XamCarouselListBox"/>) inside one of these controls you may want to set this property to constrain the
		/// height of the control to a convenient value.  If you don't set this property a default height of 300 will be used.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="HeightInInfiniteContainersProperty"/>
		/// <seealso cref="WidthInInfiniteContainers"/>
		/// <seealso cref="System.Windows.Controls.ScrollViewer"/>
		/// <seealso cref="System.Windows.Controls.StackPanel"/>
		//[Description("Returns/sets a value that is used as the default for the height of the XamCarouselPanel when used in a container with infinite height available.  The default is 300.")]
		//[Category("Appearance")]
		public double HeightInInfiniteContainers
		{
			get { return (double)this.GetValue(CarouselViewSettings.HeightInInfiniteContainersProperty); }
			set { this.SetValue(CarouselViewSettings.HeightInInfiniteContainersProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="HeightInInfiniteContainers"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeightInInfiniteContainers()
		{
			return this.HeightInInfiniteContainers != (double)CarouselViewSettings.HeightInInfiniteContainersProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="HeightInInfiniteContainers"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeightInInfiniteContainers()
		{
			this.ClearValue(HeightInInfiniteContainersProperty);
		}

				#endregion //HeightInInfiniteContainers

				#region IsListContinuous

		/// <summary>
		/// Identifies the <see cref="IsListContinuous"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsListContinuousProperty = DependencyProperty.Register("IsListContinuous",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns true if the <see cref="XamCarouselPanel"/> should wrap around to the beginning of the list and display the first list item after the last item is displayed.  The default is False.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>When this property is set to true and the total number of items in the list is less than the number of <see cref="ItemsPerPage"/>, page scrolling is disabled
		/// and line scrolling does not wrap to the other end of the list.</p>
		/// <p class="body">The <see cref="CarouselPanelItem"/> and <see cref="CarouselListBoxItem"/> objects which wrap each item in the list, expose 2 properties: <see cref="CarouselPanelItem.IsFirstItem"/> and <see cref="CarouselPanelItem.IsLastItem"/>,
		/// which you can use in a replacement Template for those objects to highlight the first and last items in the list when IsListContinuous is set to true.  This makes it easier for the end user to tell when they
		/// have scrolled back to the beginning of the list.</p>
        /// <p class="note"><b>Note: </b>The default styles for the <see cref="CarouselPanelItem"/> and <see cref="CarouselListBoxItem"/> objects <i>do not highlight the first and last items</i>.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselListBox"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="CarouselListBoxItem"/>
		//[Description("Returns true if the XamCarouselPanel should wrap around to the beginning of the list and display the first list item after the last list item.  The default is False.")]
		//[Category("Behavior")]
		public bool IsListContinuous
		{
			get { return (bool)this.GetValue(CarouselViewSettings.IsListContinuousProperty); }
			set { this.SetValue(CarouselViewSettings.IsListContinuousProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="IsListContinuous"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeIsListContinuous()
		{
			return this.IsListContinuous != (bool)CarouselViewSettings.IsListContinuousProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="IsListContinuous"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetIsListContinuous()
		{
			this.ClearValue(IsListContinuousProperty);
		}

				#endregion //IsListContinuous

				#region IsNavigatorVisible

		/// <summary>
		/// Identifies the <see cref="IsNavigatorVisible"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsNavigatorVisibleProperty = DependencyProperty.Register("IsNavigatorVisible",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether the <see cref="CarouselPanelNavigator"/> is visible.  The default is True.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="CarouselPanelNavigator"/> provides a UI for navigating through the list of items in the Carousel.  It displays buttons that 
        /// execute <see cref="XamCarouselPanelCommands"/> when clicked to scroll items.  If you want to provide your own UI for scrollig items, you can hide the CarouselPanelNavigator by setting this property
        /// to hidden and then creating a UI that executes the same scrolling commands used by the navigator.</p>
		/// </remarks>
		/// <seealso cref="CarouselPanelNavigator"/>
		/// <seealso cref="XamCarouselPanelCommands.NavigateToPreviousPage"/>
		/// <seealso cref="XamCarouselPanelCommands.NavigateToNextPage"/>
		/// <seealso cref="XamCarouselPanelCommands.NavigateToPreviousItem"/>
		/// <seealso cref="XamCarouselPanelCommands.NavigateToNextItem"/>
		//[Description("Returns/sets whether the CarouselPanelNavigator is visible.  The CarouselPanelNavigator provides a UI for navigating through the list of items in the Carousel.  The default is True.")]
		//[Category("Appearance")]
		public bool IsNavigatorVisible
		{
			get { return (bool)this.GetValue(CarouselViewSettings.IsNavigatorVisibleProperty); }
			set { this.SetValue(CarouselViewSettings.IsNavigatorVisibleProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="IsNavigatorVisible"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeIsNavigatorVisible()
		{
			return this.IsNavigatorVisible != (bool)CarouselViewSettings.IsNavigatorVisibleProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="IsNavigatorVisible"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetIsNavigatorVisible()
		{
			this.ClearValue(IsNavigatorVisibleProperty);
		}

				#endregion //IsNavigatorVisible

				#region ItemHorizontalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="ItemHorizontalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemHorizontalScrollBarVisibilityProperty = DependencyProperty.Register("ItemHorizontalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityAutoBox), new ValidateValueCallback(OnValidateItemHorizontalScrollBarVisibility));

		private static bool OnValidateItemHorizontalScrollBarVisibility(object value)
		{
			if (!Enum.IsDefined(typeof(ScrollBarVisibility), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ScrollBarVisibility));

			return true;
		}

		/// <summary>
		/// Returns/sets the visibility of the horizontal <see cref="System.Windows.Controls.Primitives.ScrollBar"/> that is displayed within each item when the width of the Item's contents exceeds the Item's width as specified by the <see cref="ItemSize"/> property.  The default is <see cref="System.Windows.Controls.ScrollBarVisibility"/>.Auto.
		/// </summary>
		/// <remarks>
		/// <p class="body">Note that this property is ignored if <see cref="AutoScaleItemContentsToFit"/> is set to true.</p>
		/// </remarks>
		/// <seealso cref="ItemHorizontalScrollBarVisibilityProperty"/>
		/// <seealso cref="ItemVerticalScrollBarVisibility"/>
		/// <seealso cref="AutoScaleItemContentsToFit"/>
		/// <seealso cref="System.Windows.Controls.Primitives.ScrollBar"/>
		/// <seealso cref="System.Windows.Controls.ScrollBarVisibility"/>
		//[Description("Returns/sets the visibility of the Horizontal scrollbar that is displayed within each item when the width of the Item's contents exceeds the Item's width as specified by the ItemSize property.  The default is ScrollBarVisibility.Auto.")]
		//[Category("Appearance")]
		public ScrollBarVisibility ItemHorizontalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)this.GetValue(CarouselViewSettings.ItemHorizontalScrollBarVisibilityProperty); }
			set { this.SetValue(CarouselViewSettings.ItemHorizontalScrollBarVisibilityProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemHorizontalScrollBarVisibility"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemHorizontalScrollBarVisibility()
		{
			return this.ItemHorizontalScrollBarVisibility != (ScrollBarVisibility)CarouselViewSettings.ItemHorizontalScrollBarVisibilityProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemHorizontalScrollBarVisibility"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemHorizontalScrollBarVisibility()
		{
			this.ClearValue(ItemHorizontalScrollBarVisibilityProperty);
		}

				#endregion //ItemHorizontalScrollBarVisibility

				#region ItemVerticalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="ItemVerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemVerticalScrollBarVisibilityProperty = DependencyProperty.Register("ItemVerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityAutoBox), new ValidateValueCallback(OnValidateItemVerticalScrollBarVisibility));

		private static bool OnValidateItemVerticalScrollBarVisibility(object value)
		{
			if (!Enum.IsDefined(typeof(ScrollBarVisibility), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(ScrollBarVisibility));

			return true;
		}

		/// <summary>
		/// Returns/sets the visibility of the vertical <see cref="System.Windows.Controls.Primitives.ScrollBar"/> that is displayed within each item when the height of the Item's contents exceeds the Item's height as specified by the <see cref="ItemSize"/> property.  The default is <see cref="System.Windows.Controls.ScrollBarVisibility"/>.Auto.
		/// </summary>
		/// <remarks>
		/// <p class="body">Note that this property is ignored if <see cref="AutoScaleItemContentsToFit"/> is set to true.</p>
		/// </remarks>
		/// <seealso cref="ItemVerticalScrollBarVisibilityProperty"/>
		/// <seealso cref="ItemHorizontalScrollBarVisibility"/>
		/// <seealso cref="AutoScaleItemContentsToFit"/>
		/// <seealso cref="System.Windows.Controls.Primitives.ScrollBar"/>
		/// <seealso cref="System.Windows.Controls.ScrollBarVisibility"/>
		//[Description("Returns/sets the visibility of the Vertical scrollbar that is displayed within each item when the height of the Item's contents exceeds the Item's height as specified by the ItemSize property.  The default is ScrollBarVisibility.Auto.")]
		//[Category("Appearance")]
		public ScrollBarVisibility ItemVerticalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)this.GetValue(CarouselViewSettings.ItemVerticalScrollBarVisibilityProperty); }
			set { this.SetValue(CarouselViewSettings.ItemVerticalScrollBarVisibilityProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemVerticalScrollBarVisibility"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemVerticalScrollBarVisibility()
		{
			return this.ItemVerticalScrollBarVisibility != (ScrollBarVisibility)CarouselViewSettings.ItemVerticalScrollBarVisibilityProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemVerticalScrollBarVisibility"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemVerticalScrollBarVisibility()
		{
			this.ClearValue(ItemVerticalScrollBarVisibilityProperty);
		}

				#endregion //ItemVerticalScrollBarVisibility

				#region ItemPath

		/// <summary>
		/// Identifies the <see cref="ItemPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathProperty = DependencyProperty.Register("ItemPath",
			typeof(Shape), typeof(CarouselViewSettings), new FrameworkPropertyMetadata((Shape)null));

		/// <summary>
		/// Returns/sets the <see cref="System.Windows.Shapes.Shape"/> derived element (i.e., <see cref="System.Windows.Shapes.Path"/>, <see cref="System.Windows.Shapes.Ellipse"/>, <see cref="System.Windows.Shapes.Line"/>, <see cref="System.Windows.Shapes.Polygon"/>, <see cref="System.Windows.Shapes.Polyline"/>, <see cref="System.Windows.Shapes.Rectangle"/>) which defines the <see cref="System.Windows.Media.Geometry"/> used to arrange items in the <see cref="XamCarouselPanel"/>.  
		/// The default is null.  When this property is set to null, an elliptical path is provided by default.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The <see cref="System.Windows.Media.Geometry"/> defined by the <see cref="System.Windows.Shapes.Shape"/> derived element will be scaled to fit within the <see cref="XamCarouselPanel"/> taking into account the settings
		/// of the <see cref="ItemPathStretch"/>, <see cref="ItemPathAutoPad"/>, <see cref="ItemPathHorizontalAlignment"/> and <see cref="ItemPathVerticalAlignment"/> properties.  These properties let you precisely determine how the path is aligned and scaled within the <see cref="XamCarouselPanel"/>.</p>
		/// <p class="body">A convenient way to create a path to assign to this property is to use Microsoft Expression Blend.  Blend provides a great
		/// designtime experience for creating and manipulating the geometry of <see cref="System.Windows.Shapes.Shape"/> derived elements.</p>
		/// </remarks>
		/// <seealso cref="ItemPathProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPathStretch"/>
		/// <seealso cref="ItemPathAutoPad"/>
		/// <seealso cref="ItemPathPadding"/>
		/// <seealso cref="ItemPathHorizontalAlignment"/>
		/// <seealso cref="ItemPathVerticalAlignment"/>
		/// <seealso cref="System.Windows.Shapes.Shape"/>
		/// <seealso cref="System.Windows.Shapes.Path"/>
		/// <seealso cref="System.Windows.Shapes.Ellipse"/>
		/// <seealso cref="System.Windows.Shapes.Line"/>
		/// <seealso cref="System.Windows.Shapes.Polygon"/>
		/// <seealso cref="System.Windows.Shapes.Polyline"/>
		/// <seealso cref="System.Windows.Shapes.Rectangle"/>
		/// <seealso cref="System.Windows.Media.Geometry"/>
		//[Description("Returns/sets the Shape derived element (i.e., Path, Ellipse, Line, Polygon, Polyline, Rectangle) which defines the Geometry used to arrange items in the XamCarouselPanel.  The default is null.  If this property is not set, an elliptical path is provided by default.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Shape ItemPath
		{
			get { return (Shape)this.GetValue(CarouselViewSettings.ItemPathProperty); }
			set { this.SetValue(CarouselViewSettings.ItemPathProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemPath"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPath()
		{
			return this.ItemPath != (Shape)CarouselViewSettings.ItemPathProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPath"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPath()
		{
			this.ClearValue(ItemPathProperty);
		}

				#endregion //ItemPath

				#region ItemPathAutoPad

		/// <summary>
		/// Identifies the <see cref="ItemPathAutoPad"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathAutoPadProperty = DependencyProperty.Register("ItemPathAutoPad",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether padding should be automatically added within the <see cref="XamCarouselPanel"/> to ensure that items arranged along the path do not extend beyond the bounds of the control. The default is True.
		/// </summary>
		/// <remarks>
		/// <p class="body">Note: If this property is set to true and padding is automatically added, the added padding is in addition to any padding specified in the <see cref="ItemPathPadding"/> property.</p>
		/// </remarks>
		/// <seealso cref="ItemPathAutoPadProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathPadding"/>
		/// <seealso cref="ItemPathStretch"/>
		/// <seealso cref="ItemPathHorizontalAlignment"/>
		/// <seealso cref="ItemPathVerticalAlignment"/>
		//[Description("Returns/sets whether padding should be automatically added within the XamCarouselPanel to ensure that items arranged along the path do not extend beyond the bounds of the control. The default is True.")]
		//[Category("Appearance")]
		public bool ItemPathAutoPad
		{
			get
			{
				return (bool)this.GetValue(CarouselViewSettings.ItemPathAutoPadProperty);
			}
			set
			{
				this.SetValue(CarouselViewSettings.ItemPathAutoPadProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathAutoPad"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathAutoPad()
		{
			return this.ItemPathAutoPad != (bool)CarouselViewSettings.ItemPathAutoPadProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathAutoPad"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathAutoPad()
		{
			this.ClearValue(ItemPathAutoPadProperty);
		}

				#endregion //ItemPathAutoPad

				#region ItemPathHorizontalAlignment

		/// <summary>
		/// Identifies the <see cref="ItemPathHorizontalAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathHorizontalAlignmentProperty = DependencyProperty.Register("ItemPathHorizontalAlignment",
			typeof(HorizontalAlignment), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(HorizontalAlignment.Center));

		/// <summary>
		/// Returns/sets the <see cref="System.Windows.HorizontalAlignment"/> of the <see cref="ItemPath"/> within the item path area.  The default is <see cref="System.Windows.HorizontalAlignment"/>.Center.
		/// </summary>
		/// <remarks>
		/// <p class="body">The item path area is an area within the <see cref="XamCarouselPanel"/> that is defined by applying the <see cref="ItemPathPadding"/> to the bounds of the <see cref="XamCarouselPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="ItemPathHorizontalAlignmentProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathStretch"/>
		/// <seealso cref="ItemPathVerticalAlignment"/>
		/// <seealso cref="ItemPathPadding"/>
		/// <seealso cref="ItemPathAutoPad"/>
		/// <seealso cref="System.Windows.HorizontalAlignment"/>
		//[Description("Returns/sets the horizontal alignment of the item path within the item path area.  The default is HorizontalAlignment.Center.")]
		//[Category("Behavior")]
		public HorizontalAlignment ItemPathHorizontalAlignment
		{
			get
			{
				return (HorizontalAlignment)this.GetValue(CarouselViewSettings.ItemPathHorizontalAlignmentProperty);
			}
			set
			{
				this.SetValue(CarouselViewSettings.ItemPathHorizontalAlignmentProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathHorizontalAlignment"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathHorizontalAlignment()
		{
			return this.ItemPathHorizontalAlignment != (HorizontalAlignment)CarouselViewSettings.ItemPathHorizontalAlignmentProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathHorizontalAlignment"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathHorizontalAlignment()
		{
			this.ClearValue(ItemPathHorizontalAlignmentProperty);
		}

				#endregion //ItemPathHorizontalAlignment

				#region ItemPathPadding

		/// <summary>
		/// Identifies the <see cref="ItemPathPadding"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathPaddingProperty = DependencyProperty.Register("ItemPathPadding",
			typeof(Thickness), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(new Thickness(0)));

		/// <summary>
		/// Returns/sets the padding around the item path area, within which the <see cref="ItemPath"/> is aligned and stretched.  The default is 0.
		/// </summary>
		/// <remarks>
		/// <p class="body">The item path area is an area within the <see cref="XamCarouselPanel"/> that is defined by applying the <see cref="ItemPathPadding"/> to the bounds of the <see cref="XamCarouselPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="ItemPathPaddingProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathAutoPad"/>
		/// <seealso cref="ItemPathStretch"/>
		/// <seealso cref="ItemPathHorizontalAlignment"/>
		/// <seealso cref="ItemPathVerticalAlignment"/>
		/// <seealso cref="System.Windows.Thickness"/>
		//[Description("Returns/sets the padding around the item path area, within which the ItemPath is aligned and stretched.  The default is 0.")]
		//[Category("Appearance")]
		public Thickness ItemPathPadding
		{
			get
			{
				return (Thickness)this.GetValue(CarouselViewSettings.ItemPathPaddingProperty);
			}
			set
			{
				this.SetValue(CarouselViewSettings.ItemPathPaddingProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathPadding"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathPadding()
		{
			return this.ItemPathPadding != (Thickness)CarouselViewSettings.ItemPathPaddingProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathPadding"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathPadding()
		{
			this.ClearValue(ItemPathPaddingProperty);
		}

				#endregion //ItemPathPadding

				#region ItemPathStretch

		/// <summary>
		/// Identifies the <see cref="ItemPathStretch"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathStretchProperty = DependencyProperty.Register("ItemPathStretch",
			typeof(Stretch), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(Stretch.Fill));

		/// <summary>
		/// Returns/sets how the <see cref="ItemPath"/> is stretched within the item path area.  The default is <see cref="System.Windows.Media.Stretch"/>.Fill
		/// </summary>
		/// <remarks>
		/// <p class="body">The item path area is an area within the <see cref="XamCarouselPanel"/> that is defined by applying the <see cref="ItemPathPadding"/> to the bounds of the <see cref="XamCarouselPanel"/>.</p>
        /// <p class="body">The <see cref="System.Windows.Media.Stretch"/> enumeration values are interpreted as follows:
        ///		<ul>
        ///			<li><b>None</b> - The item path preserves its original size.</li>
        ///			<li><b>Fill</b> - The item path is resized to fill the destination dimensions. The aspect ratio is not preserved.</li>
        ///			<li><b>Uniform</b> - The item path is resized to fit in the destination dimensions while it preserves its native aspect ratio. </li>
        ///			<li><b>UniformToFill</b> - The item path is resized to fill the destination dimensions while it preserves its native aspect ratio. If the aspect ratio of the destination rectangle differs from the source, 
        /// the item path is clipped to fit in the destination dimensions. </li>
        ///		</ul>
        /// </p>
		/// </remarks>
		/// <seealso cref="ItemPathStretchProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathAutoPad"/>
		/// <seealso cref="ItemPathStretch"/>
		/// <seealso cref="ItemPathHorizontalAlignment"/>
		/// <seealso cref="ItemPathVerticalAlignment"/>
		/// <seealso cref="System.Windows.Media.Stretch"/>
		//[Description("Returns/sets how the ItemPath is stretched within the item path area.  The default is Stretch.Fill")]
		//[Category("Appearance")]
		public Stretch ItemPathStretch
		{
			get { return (Stretch)this.GetValue(CarouselViewSettings.ItemPathStretchProperty); }
			set { this.SetValue(CarouselViewSettings.ItemPathStretchProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathStretch"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathStretch()
		{
			return this.ItemPathStretch != (Stretch)CarouselViewSettings.ItemPathStretchProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathStretch"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathStretch()
		{
			this.ClearValue(ItemPathStretchProperty);
		}

				#endregion //ItemPathStretch

				#region ItemPathPrefixPercent

		/// <summary>
		/// Identifies the <see cref="ItemPathPrefixPercent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathPrefixPercentProperty = DependencyProperty.Register("ItemPathPrefixPercent",
			typeof(double), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(CarouselViewSettings.DEFAULT_PATH_PREFIX_PCT, null, new CoerceValueCallback(OnCoerceItemPathPrefixPercent)));

		private static object OnCoerceItemPathPrefixPercent(DependencyObject d, object value)
		{
			if ((double)value < 0)
				return 0;

			if ((double)value > .4)
				return .4;

			return value;
		}

		/// <summary>
		/// Returns/sets the size of the path prefix area expressed as a decimal percentage of the overall path size (the percentage must be in the range 0.0 to 0.4).  The default is .15.
		/// </summary>
		/// <remarks>
		/// <p class="body">The prefix area is located at the beginning of the path and is the area within which items are transitioned into and out of view during scrolling.
		/// Items appear in the prefix area only while they are transitioning into or out of view.  When they are 'at rest', items appear in the area between the prefix and suffix areas.</p>
		/// </remarks>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathSuffixPercent"/>
		/// <seealso cref="ItemTransitionStyle"/>
		//[Description("Returns/sets the size of the path prefix area expressed as a percentage of the overall path size (the percentage must be in the range 0.0 to 0.4).  The default is .15.")]
		//[Category("Appearance")]
		public double ItemPathPrefixPercent
		{
			get { return (double)this.GetValue(CarouselViewSettings.ItemPathPrefixPercentProperty); }
			set { this.SetValue(CarouselViewSettings.ItemPathPrefixPercentProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathPrefixPercent"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathPrefixPercent()
		{
			return this.ItemPathPrefixPercent != (double)CarouselViewSettings.ItemPathPrefixPercentProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathPrefixPercent"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathPrefixPercent()
		{
			this.ClearValue(ItemPathPrefixPercentProperty);
		}

				#endregion //ItemPathPrefixPercent

				#region ItemPathSuffixPercent

		/// <summary>
		/// Identifies the <see cref="ItemPathSuffixPercent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathSuffixPercentProperty = DependencyProperty.Register("ItemPathSuffixPercent",
			typeof(double), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(CarouselViewSettings.DEFAULT_PATH_SUFFIX_PCT, null, new CoerceValueCallback(OnCoerceItemPathSuffixPercent)));

		private static object OnCoerceItemPathSuffixPercent(DependencyObject d, object value)
		{
			if ((double)value < 0)
				return 0;

			if ((double)value > .4)
				return .4;

			return value;
		}

		/// <summary>
		/// Returns/sets the size of the path suffix area expressed as a decimal percentage of the overall path size (the percentage must be in the range 0.0 to 0.4).  The default is .15.
		/// </summary>
		/// <remarks>
		/// <p class="body">The suffix area is located at the end of the path and is the area within which items are transitioned into and out of view during scrolling.
		/// Items appear in the suffix area only while they are transitioning into or out of view.  When they are 'at rest', items appear in the area between the prefix and suffix areas.</p>
		/// </remarks>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathPrefixPercent"/>
		/// <seealso cref="ItemTransitionStyle"/>
		//[Description("Returns/sets the size of the path suffix area expressed as a percentage of the overall path size (the percentage must be in the range 0.0 to 0.4).  The default is .15.")]
		//[Category("Appearance")]
		public double ItemPathSuffixPercent
		{
			get { return (double)this.GetValue(CarouselViewSettings.ItemPathSuffixPercentProperty); }
			set { this.SetValue(CarouselViewSettings.ItemPathSuffixPercentProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathSuffixPercent"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathSuffixPercent()
		{
			return this.ItemPathSuffixPercent != (double)CarouselViewSettings.ItemPathSuffixPercentProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathSuffixPercent"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathSuffixPercent()
		{
			this.ClearValue(ItemPathSuffixPercentProperty);
		}

				#endregion //ItemPathSuffixPercent

				#region ItemPathRenderBrush

		/// <summary>
		/// Identifies the <see cref="ItemPathRenderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathRenderBrushProperty = DependencyProperty.Register("ItemPathRenderBrush",
			typeof(Brush), typeof(CarouselViewSettings), new FrameworkPropertyMetadata((Brush)null));

		/// <summary>
		/// Returns/sets the <see cref="System.Windows.Media.Brush"/> used to fill a parent representation of the <see cref="ItemPath"/> behind the items being displayed.  By default, no <see cref="System.Windows.Media.Brush"/> is set.
		/// </summary>
		/// <remarks>
		/// <p class="body">For most path geometries, specifiying an ItemPathRenderBrush results in a compelling highlighting of the path area.  However, using an
		/// ItemPathRenderBrush with some geometries may produce unexpected parent results.  This is a function of how the WPF framework fills geometries.</p>
        /// <p class="note"><b>Note: </b>You can also specify an <see cref="ItemPathRenderPen"/> to provide an outline for the filled area.</p>
		/// </remarks>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathRenderPen"/>
		/// <seealso cref="System.Windows.Media.Brush"/>
		/// <seealso cref="System.Windows.Media.Pen"/>
		//[Description("Returns/sets the Brush used to fill a parent representation of the ItemPath behind the items being displayed.  By default, no brush is set.")]
		//[Category("Appearance")]
		public Brush ItemPathRenderBrush
		{
			get { return (Brush)this.GetValue(CarouselViewSettings.ItemPathRenderBrushProperty); }
			set { this.SetValue(CarouselViewSettings.ItemPathRenderBrushProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathRenderBrush"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathRenderBrush()
		{
			return this.ItemPathRenderBrush != (Brush)CarouselViewSettings.ItemPathRenderBrushProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathRenderBrush"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathRenderBrush()
		{
			this.ClearValue(ItemPathRenderBrushProperty);
		}

				#endregion //ItemPathRenderBrush

				#region ItemPathRenderPen

		/// <summary>
		/// Identifies the <see cref="ItemPathRenderPen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathRenderPenProperty = DependencyProperty.Register("ItemPathRenderPen",
			typeof(Pen), typeof(CarouselViewSettings), new FrameworkPropertyMetadata((Pen)null));

		/// <summary>
		/// Returns/sets the <see cref="System.Windows.Media.Pen"/> used to draw the outline of the <see cref="ItemPath"/> behind the items being displayed.  By default, no <see cref="System.Windows.Media.Pen"/> is set.
		/// </summary>
		/// <remarks>
		/// <p class="body">For most path geometries, specifiying an ItemPathRenderPen results in a compelling highlighting of the path area.  However, using an
		/// ItemPathRenderPen with some geometries may produce unexpected parent results.  This is a function of how the WPF framework outlines geometries.</p>
        /// <p class="note"><b>Note: </b>You can also specify an <see cref="ItemPathRenderBrush"/> to provide a fill for the path area.</p>
        /// </remarks>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathRenderBrush"/>
		/// <seealso cref="System.Windows.Media.Brush"/>
		/// <seealso cref="System.Windows.Media.Pen"/>
		//[Description("Returns/sets the Pen used to draw the outline of the ItemPath behind the items being displayed.  By default, no pen is set.")]
		//[Category("Appearance")]
		public Pen ItemPathRenderPen
		{
			get { return (Pen)this.GetValue(CarouselViewSettings.ItemPathRenderPenProperty); }
			set { this.SetValue(CarouselViewSettings.ItemPathRenderPenProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathRenderPen"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathRenderPen()
		{
			return this.ItemPathRenderPen != (Pen)CarouselViewSettings.ItemPathRenderPenProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathRenderPen"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathRenderPen()
		{
			this.ClearValue(ItemPathRenderPenProperty);
		}

				#endregion //ItemPathRenderPen

				#region ItemPathVerticalAlignment

		/// <summary>
		/// Identifies the <see cref="ItemPathVerticalAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemPathVerticalAlignmentProperty = DependencyProperty.Register("ItemPathVerticalAlignment",
			typeof(VerticalAlignment), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(VerticalAlignment.Center));

		/// <summary>
		/// Returns/sets the <see cref="System.Windows.VerticalAlignment"/> of the <see cref="ItemPath"/> within the item path area.  The default is <see cref="System.Windows.VerticalAlignment"/>.Center.
		/// </summary>
		/// <remarks>
		/// <p class="body">The item path area is an area within the <see cref="XamCarouselPanel"/> that is defined by applying the <see cref="ItemPathPadding"/> to the bounds of the <see cref="XamCarouselPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="ItemPathVerticalAlignmentProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathStretch"/>
		/// <seealso cref="ItemPathHorizontalAlignment"/>
		/// <seealso cref="ItemPathPadding"/>
		/// <seealso cref="ItemPathAutoPad"/>
		/// <seealso cref="System.Windows.VerticalAlignment"/>
		//[Description("Returns/sets the vertical alignment of the item path within the item path area.  The default is VerticalAlignment.Center.")]
		//[Category("Behavior")]
		public VerticalAlignment ItemPathVerticalAlignment
		{
			get
			{
				return (VerticalAlignment)this.GetValue(CarouselViewSettings.ItemPathVerticalAlignmentProperty);
			}
			set
			{
				this.SetValue(CarouselViewSettings.ItemPathVerticalAlignmentProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ItemPathVerticalAlignment"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemPathVerticalAlignment()
		{
			return this.ItemPathVerticalAlignment != (VerticalAlignment)CarouselViewSettings.ItemPathVerticalAlignmentProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemPathVerticalAlignment"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemPathVerticalAlignment()
		{
			this.ClearValue(ItemPathVerticalAlignmentProperty);
		}

				#endregion //ItemPathVerticalAlignment

				#region ItemSize

		/// <summary>
		/// Identifies the <see cref="ItemSize"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register("ItemSize",
			typeof(Size), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(CarouselViewSettings.DEFAULT_ITEM_SIZE));

		/// <summary>
		/// Returns/sets the size of the items in the Carousel.  The default is 150x100 (WxH).
		/// </summary>.
		/// <remarks>
		/// <p class="note"><b>Note: </b>The size specified by this property is applied to all items in the Carousel - you cannot specify different sizes for each item.  
		/// However you can affect the rendering size of individual items implicitly by applying scaling effects to the items using the <see cref="ScalingEffectStops"/>, <see cref="ScalingEffectStopDirection"/> and <see cref="UseScaling"/>
		/// properties.</p>
		/// <p class="note"><b>Note: </b>The <see cref="AutoScaleItemContentsToFit"/> property determines whether the contents of each item is scaled to fit within the bounds of the item
        /// as determined by this property. </p>
		/// </remarks>
		/// <seealso cref="AutoScaleItemContentsToFitProperty"/>
		/// <seealso cref="ItemHorizontalScrollBarVisibility"/>
		/// <seealso cref="ItemVerticalScrollBarVisibility"/>
		/// <seealso cref="ScalingEffectStops"/>
		/// <seealso cref="ScalingEffectStopDirection"/>
		/// <seealso cref="UseScaling"/>
		//[Description("Returns/sets the size of the items in the Carousel.  The default is 150x100 (WxH).")]
		//[Category("Appearance")]
		public Size ItemSize
		{
			get { return (Size)this.GetValue(CarouselViewSettings.ItemSizeProperty); }
			set { this.SetValue(CarouselViewSettings.ItemSizeProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemSize"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemSize()
		{
			return this.ItemSize != (Size)CarouselViewSettings.ItemSizeProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemSize"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemSize()
		{
			this.ClearValue(ItemSizeProperty);
		}

				#endregion //ItemSize

				#region ItemsPerPage

		/// <summary>
		/// Identifies the <see cref="ItemsPerPage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemsPerPageProperty = DependencyProperty.Register("ItemsPerPage",
			typeof(int), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(CarouselViewSettings.DEFAULT_ITEMS_PER_PAGE, null, new CoerceValueCallback(OnCoerceItemsPerPage)));

		private static object OnCoerceItemsPerPage(DependencyObject d, object value)
		{
			if ((int)value < 1)
				return 1;

			return value;
		}

		/// <summary>
		/// Determines the maximum number of items that should be displayed in the <see cref="XamCarouselPanel"/> at one time.  The default is 5.
		/// </summary>
		/// <remarks>
		/// <p class="body">If the total number of items in the list is less than <see cref="ItemsPerPage"/>, the <see cref="XamCarouselPanel"/> will spread out those
		/// items evenly along the <see cref="ItemPath"/> so they take up all the space between the prefix and suffix areas.</p>
        /// <p class="body">For example, if the number of items per page is set to 10 and the total 
		/// number of items in the list is 20, then the 10 displayed items will be arranged along the <see cref="ItemPath"/> between the prefix and suffix areas using even spacing.
		/// If the number of items in the list is less than <see cref="ItemsPerPage"/>, say 5, then those 5 items will still be evenly arranged along the <see cref="ItemPath"/> between the prefix and
		/// suffix areas, but with increased spacing so the entire area is used.  This prevents items from being 'bunched up' at the beginning of the path.</p>
        /// <p class="note"><b>Note: </b>up to 2x <see cref="ItemsPerPage"/> items will be displayed at any given time while the <see cref="XamCarouselPanel"/> is in the process of scrolling.</p>
        /// </remarks>
		/// <seealso cref="ItemsPerPageProperty"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPathPrefixPercent"/>
		/// <seealso cref="ItemPathSuffixPercent"/>
		/// <seealso cref="XamCarouselPanel.ItemsPerPageResolved"/>
		//[Description("Determines the maximum number of items that should be displayed in the XamCarouselPanel at one time.  The default is 5.")]
		//[Category("Behavior")]
		public int ItemsPerPage
		{
			get { return (int)this.GetValue(CarouselViewSettings.ItemsPerPageProperty); }
			set { this.SetValue(CarouselViewSettings.ItemsPerPageProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemsPerPage"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsPerPage()
		{
			return this.ItemsPerPage != (int)CarouselViewSettings.ItemsPerPageProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemsPerPage"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsPerPage()
		{
			this.ClearValue(ItemsPerPageProperty);
		}

				#endregion //ItemsPerPage

				#region ItemTransitionStyle

		/// <summary>
		/// Identifies the <see cref="ItemTransitionStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemTransitionStyleProperty = DependencyProperty.Register("ItemTransitionStyle",
			typeof(PathItemTransitionStyle), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(PathItemTransitionStyle.AdjustOpacity), new ValidateValueCallback(OnValidateItemTransitionStyle));

		private static bool OnValidateItemTransitionStyle(object value)
		{
			if (!Enum.IsDefined(typeof(PathItemTransitionStyle), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(PathItemTransitionStyle));

			return true;
		}

		/// <summary>
		/// Returns/sets a value that determines the effects applied to items as they transition through the prefix and suffix areas of the <see cref="ItemPath"/>.  The default is <see cref="PathItemTransitionStyle"/>.AdjustOpacity.
		/// </summary>
		/// <remarks>
		/// <p class="body">The prefix and suffix areas of the item path are reserved for items that are transitioning into and out of view during scrolling.
		/// The setting of this property determines whether items in these areas have their size or opacity automatically adjusted from normal settings to zero as they exit the path.</p>
		/// <p class="note"><b>Note: </b>The effects specified by the setting of this property are applied <i>in addition to</i> the effects (if any) specified by the various EffectStops properties.</p>
		/// </remarks>
		/// <seealso cref="ItemTransitionStyleProperty"/>
		/// <seealso cref="PathItemTransitionStyle"/>
		/// <seealso cref="ItemPath"/>
		/// <seealso cref="ItemPathSuffixPercent"/>
		/// <seealso cref="ItemPathPrefixPercent"/>
		/// <seealso cref="OpacityEffectStops"/>
		/// <seealso cref="ScalingEffectStops"/>
		/// <seealso cref="SkewAngleXEffectStops"/>
		/// <seealso cref="SkewAngleYEffectStops"/>
		/// <seealso cref="ZOrderEffectStops"/>
		//[Description("Returns/sets a value that determines the effects applied to items as they transition through the prefix and suffix areas of the ItemPath.  The default is PathItemTransitionStyle.AdjustOpacity.")]
		//[Category("Appearance")]
		public PathItemTransitionStyle ItemTransitionStyle
		{
			get { return (PathItemTransitionStyle)this.GetValue(CarouselViewSettings.ItemTransitionStyleProperty); }
			set { this.SetValue(CarouselViewSettings.ItemTransitionStyleProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ItemTransitionStyle"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemTransitionStyle()
		{
			return this.ItemTransitionStyle != (PathItemTransitionStyle)CarouselViewSettings.ItemTransitionStyleProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ItemTransitionStyle"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemTransitionStyle()
		{
			this.ClearValue(ItemTransitionStyleProperty);
		}

				#endregion //ItemTransitionStyle

				#region OpacityEffectStops

		/// <summary>
		/// Identifies the <see cref="OpacityEffectStops"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OpacityEffectStopsProperty = DependencyProperty.Register("OpacityEffectStops",
		    typeof(OpacityEffectStopCollection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnOpacityEffectsChanged)));

		private static void OnOpacityEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselViewSettings carouselViewSettings = d as CarouselViewSettings;
			if (carouselViewSettings != null)
			{
				if (e.OldValue != null)
				{
					OpacityEffectStopCollection oldOpacityEffectStops = e.OldValue as OpacityEffectStopCollection;
					if (oldOpacityEffectStops != null)
						oldOpacityEffectStops.PropertyChanged -= new PropertyChangedEventHandler(carouselViewSettings.OnOpacityEffectStopsCollectionChanged);
				}

				if (e.NewValue != null)
				{
					OpacityEffectStopCollection newOpacityEffectStops = e.NewValue as OpacityEffectStopCollection;
					if (newOpacityEffectStops != null)
						newOpacityEffectStops.PropertyChanged += new PropertyChangedEventHandler(carouselViewSettings.OnOpacityEffectStopsCollectionChanged);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="OpacityEffectStop"/> objects that define the stops to be used when setting the opacity of items along the <see cref="ItemPath"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>the <see cref="UseOpacity"/> property must be set to true for these <see cref="OpacityEffectStop"/>s to be applied.</p>
		/// <p class="body">The way in which the opacity effect stops are applied to each item is determined by the item's location in the display area of the
		/// <see cref="XamCarouselPanel"/> and the setting of the <see cref="OpacityEffectStopDirection"/> property.</p>
		/// <p class="body">For example, if you are using a circular path and you want to give the user the sense that the items at the top of the circle are further away, set the
		/// this property to EffectStopDirection.Vertical and add OpacityEffectStops that range from a value of .6 at an Offset of zero to a value of 1 at an Offset of 1.  This will make the items 'in the distance' appear
		/// a bit more transparent.  To further enhance the perspective effect you can also do a similar thing with ScalingEffectStops - make items in the foreground (i.e., bottom of the circle) bigger and items in the back
		/// (i.e., top of the circle) smaller.</p>
		/// </remarks>
		/// <seealso cref="OpacityEffectStopsProperty"/>
		/// <seealso cref="OpacityEffectStop"/>
		/// <seealso cref="OpacityEffectStopDirection"/>
		/// <seealso cref="UseOpacity"/>
		/// <seealso cref="XamCarouselPanel"/>
		//[Description("Returns a collection of OpacityEffectStop objects that define the stops to be used when setting the opacity of items along the item path")]
		//[Category("Behavior")]
		[Bindable(true)]
		public OpacityEffectStopCollection OpacityEffectStops
		{
			get
			{
				return this.GetValue(CarouselViewSettings.OpacityEffectStopsProperty) as OpacityEffectStopCollection;
			}
			set { this.SetValue(CarouselViewSettings.OpacityEffectStopsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="OpacityEffectStops"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeOpacityEffectStops()
		{
			return this.OpacityEffectStops.ShouldSerialize();
		}

		/// <summary>
		/// Resets the <see cref="OpacityEffectStops"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetOpacityEffectStops()
		{
			this.OpacityEffectStops.Clear();
		}

				#endregion //OpacityEffectStops

				#region OpacityEffectStopDirection

		/// <summary>
		/// Identifies the <see cref="OpacityEffectStopDirection"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OpacityEffectStopDirectionProperty = DependencyProperty.Register("OpacityEffectStopDirection",
			typeof(EffectStopDirection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(EffectStopDirection.Default), new ValidateValueCallback(OnValidateOpacityEffectStopDirection));

		private static bool OnValidateOpacityEffectStopDirection(object value)
		{
			if (!Enum.IsDefined(typeof(EffectStopDirection), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(EffectStopDirection));

			return true;
		}

		/// <summary>
		/// Returns/sets the direction used to evaluate <see cref="OpacityEffectStop"/>s.  The default is <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <remarks>
		/// <p class="body">The direction can be set to evaluate the effect stops based on an item's position along the <see cref="ItemPath"/> or based on the item's vertical or horizontal position within the <see cref="XamCarouselPanel"/>.</p>
		/// <p class="body">For example, if you are using a circular path and you want to give the user the sense that the items at the top of the circle are further away, set the
        /// this property to <see cref="EffectStopDirection"/>.Vertical and add <see cref="OpacityEffectStops"/> that range from a value of .6 at an Offset of zero to a value of 1 at an Offset of 1.  This will make the items 'in the distance' appear
        /// a bit more transparent.  To further enhance the perspective effect you can also do a similar thing with <see cref="ScalingEffectStops"/> - make items in the foreground (i.e., bottom of the circle) bigger and items in the back
		/// (i.e., top of the circle) smaller.</p>
		/// </remarks>
		/// <seealso cref="OpacityEffectStopDirectionProperty"/>
		/// <seealso cref="OpacityEffectStops"/>
		/// <seealso cref="OpacityEffectStop"/>
		/// <seealso cref="UseOpacity"/>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns/sets the direction used to evaluate OpacityEffectStops.  The default is EffectStopDirection.UseItemPath.")]
		//[Category("Appearance")]
		public EffectStopDirection OpacityEffectStopDirection
		{
			get { return (EffectStopDirection)this.GetValue(CarouselViewSettings.OpacityEffectStopDirectionProperty); }
			set { this.SetValue(CarouselViewSettings.OpacityEffectStopDirectionProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="OpacityEffectStopDirection"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeOpacityEffectStopDirection()
		{
			return this.OpacityEffectStopDirection != (EffectStopDirection)CarouselViewSettings.OpacityEffectStopDirectionProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="OpacityEffectStopDirection"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetOpacityEffectStopDirection()
		{
			this.ClearValue(OpacityEffectStopDirectionProperty);
		}

				#endregion //OpacityEffectStopDirection

				#region ReserveSpaceForReflections

		/// <summary>
		/// Identifies the <see cref="ReserveSpaceForReflections"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ReserveSpaceForReflectionsProperty = DependencyProperty.Register("ReserveSpaceForReflections",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether the <see cref="XamCarouselPanel"/> should reserve space for item reflections.  The default is true.
		/// </summary>
		/// <remarks>
        /// <p class="body">If true, each <see cref="CarouselPanelItem"/> will be sized to twice the height specified by the <see cref="ItemSize"/> property to make room for the reflection.</p>
        /// <p class="note"><b>Note: </b>It is the responsibility of the Template within the CarouselPanelItem style to actually show the reflections in the additional space provided
        /// by this property.  The default style for the <see cref="CarouselPanelItem"/> does just that.</p>
        /// </remarks>
		/// <seealso cref="ReserveSpaceForReflectionsProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselPanel.ReflectionVisibility"/>
		/// <seealso cref="ItemSize"/>
		//[Description("Returns/sets whether the XamCarouselPanel should reserve space for item reflections.  If true, each item will be sized to twice the height specified by the ItemSize property.  The default is true.")]
		//[Category("Appearance")]
		public bool ReserveSpaceForReflections
		{
			get { return (bool)this.GetValue(CarouselViewSettings.ReserveSpaceForReflectionsProperty); }
			set { this.SetValue(CarouselViewSettings.ReserveSpaceForReflectionsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ReserveSpaceForReflections"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeReserveSpaceForReflections()
		{
			return this.ReserveSpaceForReflections != (bool)CarouselViewSettings.ReserveSpaceForReflectionsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ReserveSpaceForReflections"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetReserveSpaceForReflections()
		{
			this.ClearValue(ReserveSpaceForReflectionsProperty);
		}

				#endregion //ReserveSpaceForReflections

				#region RotateItemsWithPathTangent

		/// <summary>
		/// Identifies the <see cref="RotateItemsWithPathTangent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RotateItemsWithPathTangentProperty = DependencyProperty.Register("RotateItemsWithPathTangent",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether the <see cref="XamCarouselPanel"/> should rotate each item based on the item position's tangent to the path.  The default is False.
		/// </summary>
		/// <seealso cref="RotateItemsWithPathTangentProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="ItemPath"/>
		//[Description("Returns/sets whether the XamCarouselPanel should rotate each item based on the item position's tangent to the path.  The default is False.")]
		//[Category("Appearance")]
		public bool RotateItemsWithPathTangent
		{
			get { return (bool)this.GetValue(CarouselViewSettings.RotateItemsWithPathTangentProperty); }
			set { this.SetValue(CarouselViewSettings.RotateItemsWithPathTangentProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="RotateItemsWithPathTangent"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeRotateItemsWithPathTangent()
		{
			return this.RotateItemsWithPathTangent != (bool)CarouselViewSettings.RotateItemsWithPathTangentProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="RotateItemsWithPathTangent"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetRotateItemsWithPathTangent()
		{
			this.ClearValue(RotateItemsWithPathTangentProperty);
		}

				#endregion //RotateItemsWithPathTangent

				#region ScalingEffectStops

		/// <summary>
		/// Identifies the <see cref="ScalingEffectStops"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScalingEffectStopsProperty = DependencyProperty.Register("ScalingEffectStops",
			typeof(ScalingEffectStopCollection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnScalingEffectsChanged)));

		private static void OnScalingEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselViewSettings carouselViewSettings = d as CarouselViewSettings;
			if (carouselViewSettings != null)
			{
				if (e.OldValue != null)
				{
					ScalingEffectStopCollection oldScalingEffectStops = e.OldValue as ScalingEffectStopCollection;
					if (oldScalingEffectStops != null)
						oldScalingEffectStops.PropertyChanged -= new PropertyChangedEventHandler(carouselViewSettings.OnScalingEffectStopsCollectionChanged);
				}

				if (e.NewValue != null)
				{
					ScalingEffectStopCollection newScalingEffectStops = e.NewValue as ScalingEffectStopCollection;
					if (newScalingEffectStops != null)
						newScalingEffectStops.PropertyChanged += new PropertyChangedEventHandler(carouselViewSettings.OnScalingEffectStopsCollectionChanged);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="ScalingEffectStop"/> objects that define the stops to be used when scaling items along the <see cref="ItemPath"/> to simulate perspective.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>the <see cref="UseScaling"/> property must be set to true for these <see cref="ScalingEffectStop"/>s to be applied.</p>
		/// <p class="body">The way in which the scaling effect stops are applied to each item is determined by the item's location in the display area of the
		/// <see cref="XamCarouselPanel"/> and the setting of the <see cref="ScalingEffectStopDirection"/> property.</p>
		/// <p class="body">For example, if you are using a circular path and you want to give the user the sense that the items at the top of the circle are further away, set the
		/// this property to <see cref="EffectStopDirection"/>.Vertical and add <see cref="ScalingEffectStop"/>s that range from a value of .6 at an Offset of zero to a value of 1 at an Offset of 1.  This will make the items 'in the distance' appear
		/// a bit smaller.  To further enhance the perspective effect you can also do a similar thing with <see cref="OpacityEffectStop"/>s - make items in the foreground (i.e., bottom of the circle) opaque and items in the back
		/// (i.e., top of the circle) a bit transparent.</p>
		/// </remarks>
		/// <seealso cref="ScalingEffectStopsProperty"/>
		/// <seealso cref="ScalingEffectStop"/>
		/// <seealso cref="ScalingEffectStopDirection"/>
		/// <seealso cref="UseScaling"/>
		//[Description("Returns a collection of ScalingEffectStop objects that define the stops to be used when scaling items to simulate perspective.")]
		//[Category("Data")]
		public ScalingEffectStopCollection ScalingEffectStops
		{
			get
			{
				return this.GetValue(CarouselViewSettings.ScalingEffectStopsProperty) as ScalingEffectStopCollection;
			}
			set { this.SetValue(CarouselViewSettings.ScalingEffectStopsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ScalingEffectStops"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScalingEffectStops()
		{
			return this.ScalingEffectStops.ShouldSerialize();
		}

		/// <summary>
		/// Resets the <see cref="ScalingEffectStops"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScalingEffectStops()
		{
			this.ScalingEffectStops.Clear();
		}

				#endregion //ScalingEffectStops

				#region ScalingEffectStopDirection

		/// <summary>
		/// Identifies the <see cref="ScalingEffectStopDirection"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScalingEffectStopDirectionProperty = DependencyProperty.Register("ScalingEffectStopDirection",
			typeof(EffectStopDirection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(EffectStopDirection.Default), new ValidateValueCallback(OnValidateScalingEffectStopDirection));

		private static bool OnValidateScalingEffectStopDirection(object value)
		{
			if (!Enum.IsDefined(typeof(EffectStopDirection), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(EffectStopDirection));

			return true;
		}

		/// <summary>
		/// Returns/sets the direction used to evaluate <see cref="ScalingEffectStop"/>s.  The default is <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <remarks>
		/// <p class="body">The direction can be set to evaluate the effect stops based on an item's position along the <see cref="ItemPath"/> or based on the item's vertical or horizontal position within the <see cref="XamCarouselPanel"/>.</p>
		/// <p class="body">For example, if you are using a circular path and you want to give the user the sense that the items at the top of the circle are further away, set the
		/// this property to <see cref="EffectStopDirection"/>.Vertical and add <see cref="ScalingEffectStop"/>s that range from a value of .6 at an Offset of zero to a value of 1 at an Offset of 1.  This will make the items 'in the distance' appear
		/// a bit smaller.  To further enhance the perspective effect you can also do a similar thing with <see cref="OpacityEffectStop"/>s - make items in the foreground (i.e., bottom of the circle) opaque and items in the back
		/// (i.e., top of the circle) a bit transparent.</p>
		/// </remarks>
		/// <seealso cref="ScalingEffectStopDirectionProperty"/>
		/// <seealso cref="ScalingEffectStops"/>
		/// <seealso cref="ScalingEffectStop"/>
		/// <seealso cref="UseScaling"/>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns/sets the direction used to evaluate ScalingEffectStops.  The default is EffectStopDirection.UseItemPath.")]
		//[Category("Appearance")]
		public EffectStopDirection ScalingEffectStopDirection
		{
			get { return (EffectStopDirection)this.GetValue(CarouselViewSettings.ScalingEffectStopDirectionProperty); }
			set { this.SetValue(CarouselViewSettings.ScalingEffectStopDirectionProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ScalingEffectStopDirection"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScalingEffectStopDirection()
		{
			return this.ScalingEffectStopDirection != (EffectStopDirection)CarouselViewSettings.ScalingEffectStopDirectionProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ScalingEffectStopDirection"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScalingEffectStopDirection()
		{
			this.ClearValue(ScalingEffectStopDirectionProperty);
		}

				#endregion //ScalingEffectStopDirection

				// JM 06-10-08 - TFS Work Item #4472
				#region ShouldAnimateItemsOnListChange

		/// <summary>
		/// Identifies the <see cref="ShouldAnimateItemsOnListChange"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldAnimateItemsOnListChangeProperty = DependencyProperty.Register("ShouldAnimateItemsOnListChange",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether items should be animated into their positions when the whenever the list of items being displayed in the <see cref="XamCarouselPanel"/> is changed (i.e., items are added, removed, moved or replaced).  The default is False.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> When the list is Reset, the items are always animated.</p>
		/// </remarks>
		/// <seealso cref="ShouldAnimateItemsOnListChangeProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		//[Description("Returns/sets whether items should be animated into their positions when the whenever the list of items being displayed in the XamCarouselPanel is changed (i.e., items are added, removed, moved or replaced).  The default is False.")]
		//[Category("Behavior")]
		public bool ShouldAnimateItemsOnListChange
		{
			get { return (bool)this.GetValue(CarouselViewSettings.ShouldAnimateItemsOnListChangeProperty); }
			set { this.SetValue(CarouselViewSettings.ShouldAnimateItemsOnListChangeProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ShouldAnimateItemsOnListChange"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShouldAnimateItemsOnListChange()
		{
			return this.ShouldAnimateItemsOnListChange != (bool)CarouselViewSettings.ShouldAnimateItemsOnListChangeProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ShouldAnimateItemsOnListChange"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShouldAnimateItemsOnListChange()
		{
			this.ClearValue(ShouldAnimateItemsOnListChangeProperty);
		}

				#endregion //ShouldAnimateItemsOnListChange

				#region ShouldScrollItemsIntoInitialPosition

		/// <summary>
		/// Identifies the <see cref="ShouldScrollItemsIntoInitialPosition"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldScrollItemsIntoInitialPositionProperty = DependencyProperty.Register("ShouldScrollItemsIntoInitialPosition",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether items should be animated into their start positions when the <see cref="XamCarouselPanel"/> is first loaded.  The default is True.
		/// </summary>
		/// <seealso cref="ShouldScrollItemsIntoInitialPositionProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		//[Description("Returns/sets whether items should be animated into their start positions when the XamCarouselPanel is first loaded.  The default is True.")]
		//[Category("Behavior")]
		public bool ShouldScrollItemsIntoInitialPosition
		{
			get { return (bool)this.GetValue(CarouselViewSettings.ShouldScrollItemsIntoInitialPositionProperty); }
			set { this.SetValue(CarouselViewSettings.ShouldScrollItemsIntoInitialPositionProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ShouldScrollItemsIntoInitialPosition"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShouldScrollItemsIntoInitialPosition()
		{
			return this.ShouldScrollItemsIntoInitialPosition != (bool)CarouselViewSettings.ShouldScrollItemsIntoInitialPositionProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ShouldScrollItemsIntoInitialPosition"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShouldScrollItemsIntoInitialPosition()
		{
			this.ClearValue(ShouldScrollItemsIntoInitialPositionProperty);
		}

				#endregion //ShouldScrollItemsIntoInitialPosition

				#region SkewAngleXEffectStops

		/// <summary>
		/// Identifies the <see cref="SkewAngleXEffectStops"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SkewAngleXEffectStopsProperty = DependencyProperty.Register("SkewAngleXEffectStops",
			typeof(SkewAngleXEffectStopCollection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSkewAngleXEffectsChanged)));

		private static void OnSkewAngleXEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselViewSettings carouselViewSettings = d as CarouselViewSettings;
			if (carouselViewSettings != null)
			{
				if (e.OldValue != null)
				{
					SkewAngleXEffectStopCollection oldSkewAngleXEffectStops = e.OldValue as SkewAngleXEffectStopCollection;
					if (oldSkewAngleXEffectStops != null)
						oldSkewAngleXEffectStops.PropertyChanged -= new PropertyChangedEventHandler(carouselViewSettings.OnSkewAngleXEffectStopsCollectionChanged);
				}

				if (e.NewValue != null)
				{
					SkewAngleXEffectStopCollection newSkewAngleXEffectStops = e.NewValue as SkewAngleXEffectStopCollection;
					if (newSkewAngleXEffectStops != null)
						newSkewAngleXEffectStops.PropertyChanged += new PropertyChangedEventHandler(carouselViewSettings.OnSkewAngleXEffectStopsCollectionChanged);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="SkewAngleXEffectStop"/> objects that define the stops to be used when when setting the SkewAngleX angle of items along the <see cref="ItemPath"/> .
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>the <see cref="UseSkewing"/> property must be set to true for these <see cref="SkewAngleXEffectStop"/>s to be applied.</p>
		/// <p class="body">The way in which the scaling effect stops are applied to each item is determined by the item's location in the display area of the
		/// <see cref="XamCarouselPanel"/> and the setting of the <see cref="SkewAngleXEffectStopDirection"/> property.</p>
		/// </remarks>
		/// <seealso cref="SkewAngleXEffectStopsProperty"/>
		/// <seealso cref="SkewAngleXEffectStop"/>
		/// <seealso cref="SkewAngleXEffectStopDirection"/>
		/// <seealso cref="UseSkewing"/>
		//[Description("Returns a collection of SkewAngleXEffectStop objects that define the stops to be used when setting the SkewAngleX angle of items along the ItemPath.")]
		//[Category("Data")]
		public SkewAngleXEffectStopCollection SkewAngleXEffectStops
		{
			get	{ return this.GetValue(CarouselViewSettings.SkewAngleXEffectStopsProperty) as SkewAngleXEffectStopCollection; }
			set { this.SetValue(CarouselViewSettings.SkewAngleXEffectStopsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="SkewAngleXEffectStops"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSkewAngleXEffectStops()
		{
			return this.SkewAngleXEffectStops.ShouldSerialize();
		}

		/// <summary>
		/// Resets the <see cref="SkewAngleXEffectStops"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSkewAngleXEffectStops()
		{
			this.SkewAngleXEffectStops.Clear();
		}

				#endregion //SkewAngleXEffectStops

				#region SkewAngleXEffectStopDirection

		/// <summary>
		/// Identifies the <see cref="SkewAngleXEffectStopDirection"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SkewAngleXEffectStopDirectionProperty = DependencyProperty.Register("SkewAngleXEffectStopDirection",
			typeof(EffectStopDirection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(EffectStopDirection.Default), new ValidateValueCallback(OnValidateSkewAngleXEffectStopDirection));

		private static bool OnValidateSkewAngleXEffectStopDirection(object value)
		{
			if (!Enum.IsDefined(typeof(EffectStopDirection), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(EffectStopDirection));

			return true;
		}

		/// <summary>
		/// Returns/sets the direction used to evaluate <see cref="SkewAngleXEffectStop"/>s.  The default is <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <remarks>
		/// <p class="body">The direction can be set to evaluate the effect stops based on an item's position along the <see cref="ItemPath"/> or based on the item's vertical or horizontal position within the <see cref="XamCarouselPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="SkewAngleXEffectStopDirectionProperty"/>
		/// <seealso cref="SkewAngleXEffectStops"/>
		/// <seealso cref="SkewAngleXEffectStop"/>
		/// <seealso cref="UseSkewing"/>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns/sets the direction used to evaluate SkewAngleXEffectStops.  The default is EffectStopDirection.UseItemPath.")]
		//[Category("Appearance")]
		public EffectStopDirection SkewAngleXEffectStopDirection
		{
			get { return (EffectStopDirection)this.GetValue(CarouselViewSettings.SkewAngleXEffectStopDirectionProperty); }
			set { this.SetValue(CarouselViewSettings.SkewAngleXEffectStopDirectionProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="SkewAngleXEffectStopDirection"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSkewAngleXEffectStopDirection()
		{
			return this.SkewAngleXEffectStopDirection != (EffectStopDirection)CarouselViewSettings.SkewAngleXEffectStopDirectionProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="SkewAngleXEffectStopDirection"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSkewAngleXEffectStopDirection()
		{
			this.ClearValue(SkewAngleXEffectStopDirectionProperty);
		}

				#endregion //SkewAngleXEffectStopDirection

				#region SkewAngleYEffectStops

		/// <summary>
		/// Identifies the <see cref="SkewAngleYEffectStops"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SkewAngleYEffectStopsProperty = DependencyProperty.Register("SkewAngleYEffectStops",
			typeof(SkewAngleYEffectStopCollection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSkewAngleYEffectsChanged)));

		private static void OnSkewAngleYEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselViewSettings carouselViewSettings = d as CarouselViewSettings;
			if (carouselViewSettings != null)
			{
				if (e.OldValue != null)
				{
					SkewAngleYEffectStopCollection oldSkewAngleYEffectStops = e.OldValue as SkewAngleYEffectStopCollection;
					if (oldSkewAngleYEffectStops != null)
						oldSkewAngleYEffectStops.PropertyChanged -= new PropertyChangedEventHandler(carouselViewSettings.OnSkewAngleYEffectStopsCollectionChanged);
				}

				if (e.NewValue != null)
				{
					SkewAngleYEffectStopCollection newSkewAngleYEffectStops = e.NewValue as SkewAngleYEffectStopCollection;
					if (newSkewAngleYEffectStops != null)
						newSkewAngleYEffectStops.PropertyChanged += new PropertyChangedEventHandler(carouselViewSettings.OnSkewAngleYEffectStopsCollectionChanged);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="SkewAngleYEffectStop"/> objects that define the stops to be used when when setting the SkewAngleY angle of items along the ItemPath.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>the <see cref="UseSkewing"/> property must be set to true for these <see cref="SkewAngleYEffectStop"/>s to be applied.</p>
		/// <p class="body">The way in which the scaling effect stops are applied to each item is determined by the item's location in the display area of the
		/// <see cref="XamCarouselPanel"/> and the setting of the <see cref="SkewAngleYEffectStopDirection"/> property.</p>
		/// </remarks>
		/// <seealso cref="SkewAngleYEffectStopsProperty"/>
		/// <seealso cref="SkewAngleYEffectStop"/>
		/// <seealso cref="SkewAngleYEffectStopDirection"/>
		/// <seealso cref="UseSkewing"/>
		//[Description("Returns a collection of SkewAngleYEffectStop objects that define the stops to be used when setting the SkewAngleY angle of items along the ItemPath.")]
		//[Category("Data")]
		public SkewAngleYEffectStopCollection SkewAngleYEffectStops
		{
			get
			{
				return this.GetValue(CarouselViewSettings.SkewAngleYEffectStopsProperty) as SkewAngleYEffectStopCollection;
			}
			set { this.SetValue(CarouselViewSettings.SkewAngleYEffectStopsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="SkewAngleYEffectStops"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSkewAngleYEffectStops()
		{
			return this.SkewAngleYEffectStops.ShouldSerialize();
		}

		/// <summary>
		/// Resets the <see cref="SkewAngleYEffectStops"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSkewAngleYEffectStops()
		{
			this.SkewAngleYEffectStops.Clear();
		}

				#endregion //SkewAngleYEffectStops

				#region SkewAngleYEffectStopDirection

		/// <summary>
		/// Identifies the <see cref="SkewAngleYEffectStopDirection"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SkewAngleYEffectStopDirectionProperty = DependencyProperty.Register("SkewAngleYEffectStopDirection",
			typeof(EffectStopDirection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(EffectStopDirection.Default), new ValidateValueCallback(OnValidateSkewAngleYEffectStopDirection));

		private static bool OnValidateSkewAngleYEffectStopDirection(object value)
		{
			if (!Enum.IsDefined(typeof(EffectStopDirection), value))
				throw new InvalidEnumArgumentException("value", (int)value, typeof(EffectStopDirection));

			return true;
		}

		/// <summary>
		/// Returns/sets the direction used to evaluate <see cref="SkewAngleYEffectStop"/>s.  The default is <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <remarks>
		/// <p class="body">The direction can be set to evaluate the effect stops based on an item's position along the <see cref="ItemPath"/> or based on the item's vertical or horizontal position within the <see cref="XamCarouselPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="SkewAngleYEffectStopDirectionProperty"/>
		/// <seealso cref="SkewAngleYEffectStops"/>
		/// <seealso cref="SkewAngleYEffectStop"/>
		/// <seealso cref="UseSkewing"/>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns/sets the direction used to evaluate SkewAngleYEffectStops.  The default is EffectStopDirection.UseItemPath.")]
		//[Category("Appearance")]
		public EffectStopDirection SkewAngleYEffectStopDirection
		{
			get { return (EffectStopDirection)this.GetValue(CarouselViewSettings.SkewAngleYEffectStopDirectionProperty); }
			set { this.SetValue(CarouselViewSettings.SkewAngleYEffectStopDirectionProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="SkewAngleYEffectStopDirection"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSkewAngleYEffectStopDirection()
		{
			return this.SkewAngleYEffectStopDirection != (EffectStopDirection)CarouselViewSettings.SkewAngleYEffectStopDirectionProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="SkewAngleYEffectStopDirection"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSkewAngleYEffectStopDirection()
		{
			this.ClearValue(SkewAngleYEffectStopDirectionProperty);
		}

				#endregion //SkewAngleYEffectStopDirection

				#region UseOpacity

		/// <summary>
		/// Identifies the <see cref="UseOpacity"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseOpacityProperty = DependencyProperty.Register("UseOpacity",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether <see cref="OpacityEffectStops"/> and <see cref="OpacityEffectStopDirection"/> should be honored and opacity effects applied to items in the Carousel.  The default is False.
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set to false, any <see cref="OpacityEffectStop"/>s that may be defined are ignored.</p>
		/// <p class="note"><b>Note: </b>The <see cref="PathItemTransitionStyle"/> property can be used to apply additional effects to items as they transition into and out of view through the prefix and suffix areas of the <see cref="ItemPath"/>.</p>
		/// </remarks>
		/// <seealso cref="UseOpacityProperty"/>
		/// <seealso cref="OpacityEffectStopDirectionProperty"/>
		/// <seealso cref="OpacityEffectStops"/>
		/// <seealso cref="OpacityEffectStop"/>
		/// <seealso cref="ItemTransitionStyle"/>
		/// <seealso cref="ItemPath"/>
		//[Description("Returns/sets whether OpacityEffectStops and OpacityEffectStopDirection should be honored and opacity effects applied to items in the Carousel.  The default is False.")]
		//[Category("Behavior")]
		public bool UseOpacity
		{
			get { return (bool)this.GetValue(CarouselViewSettings.UseOpacityProperty); }
			set { this.SetValue(CarouselViewSettings.UseOpacityProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="UseOpacity"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseOpacity()
		{
			return this.UseOpacity != (bool)CarouselViewSettings.UseOpacityProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="UseOpacity"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseOpacity()
		{
			this.ClearValue(UseOpacityProperty);
		}

				#endregion //UseOpacity

				#region UseScaling

		/// <summary>
		/// Identifies the <see cref="UseScaling"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseScalingProperty = DependencyProperty.Register("UseScaling",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether <see cref="ScalingEffectStops"/> and <see cref="ScalingEffectStopDirection"/> should be honored and scaling effects applied to items in the Carousel.  The default is True.
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set to false, any <see cref="ScalingEffectStop"/>s that may be defined are ignored.</p>
		/// <p class="note"><b>Note: </b>The <see cref="PathItemTransitionStyle"/> property can be used to apply additional effects to items as they transition into and out of view through the prefix and suffix areas of the <see cref="ItemPath"/>.</p>
		/// </remarks>
		/// <seealso cref="UseScalingProperty"/>
		/// <seealso cref="ScalingEffectStopDirectionProperty"/>
		/// <seealso cref="ScalingEffectStops"/>
		/// <seealso cref="ScalingEffectStop"/>
		/// <seealso cref="ItemTransitionStyle"/>
		/// <seealso cref="ItemPath"/>
		//[Description("Returns/sets whether ScalingEffectStops and ScalingEffectStopDirection should be honored and opacity effects applied to items in the Carousel.  The default is True.")]
		//[Category("Behavior")]
		public bool UseScaling
		{
			get { return (bool)this.GetValue(CarouselViewSettings.UseScalingProperty); }
			set { this.SetValue(CarouselViewSettings.UseScalingProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="UseScaling"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseScaling()
		{
			return this.UseScaling != (bool)CarouselViewSettings.UseScalingProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="UseScaling"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseScaling()
		{
			this.ClearValue(UseScalingProperty);
		}

				#endregion //UseScaling

				#region UseSkewing

		/// <summary>
		/// Identifies the <see cref="UseSkewing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseSkewingProperty = DependencyProperty.Register("UseSkewing",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether <see cref="SkewAngleXEffectStops"/>, <see cref="SkewAngleYEffectStops"/>, <see cref="SkewAngleXEffectStopDirection"/> and <see cref="SkewAngleYEffectStopDirection"/> should be honored and skewing effects applied to items in the Carousel.  The default is False.
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set to false, any <see cref="SkewAngleXEffectStop"/>s or <see cref="SkewAngleYEffectStop"/>s that may be defined are ignored.</p>
		/// <p class="note"><b>Note: </b>The <see cref="PathItemTransitionStyle"/> property can be used to apply additional effects to items as they transition into and out of view through the prefix and suffix areas of the <see cref="ItemPath"/>.</p>
		/// </remarks>
		/// <seealso cref="UseSkewingProperty"/>
		/// <seealso cref="SkewAngleXEffectStopDirectionProperty"/>
		/// <seealso cref="SkewAngleYEffectStopDirectionProperty"/>
		/// <seealso cref="SkewAngleXEffectStops"/>
		/// <seealso cref="SkewAngleYEffectStops"/>
		/// <seealso cref="SkewAngleXEffectStop"/>
		/// <seealso cref="SkewAngleYEffectStop"/>
		/// <seealso cref="ItemTransitionStyle"/>
		/// <seealso cref="ItemPath"/>
		//[Description("Returns/sets whether SkewAngleXEffectStops, SkewAngleYEffectStops, SkewAngleXEffectStopDirection and SkewAngleYEffectStopDirection should be honored and skewing effects applied to items in the Carousel.  The default is False.")]
		//[Category("Behavior")]
		public bool UseSkewing
		{
			get { return (bool)this.GetValue(CarouselViewSettings.UseSkewingProperty); }
			set { this.SetValue(CarouselViewSettings.UseSkewingProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="UseSkewing"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseSkewing()
		{
			return this.UseSkewing != (bool)CarouselViewSettings.UseSkewingProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="UseSkewing"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseSkewing()
		{
			this.ClearValue(UseSkewingProperty);
		}

				#endregion //UseSkewing

				#region UseZOrder

		/// <summary>
		/// Identifies the <see cref="UseZOrder"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseZOrderProperty = DependencyProperty.Register("UseZOrder",
			typeof(bool), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets whether <see cref="ZOrderEffectStops"/> should be honored and ZOrder effects applied to items in the Carousel.  The default is False.
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set to false, any <see cref="ZOrderEffectStop"/>s that may be defined are ignored, and items will be ordered in the Z-space based on their position in the list (items with a lower list position will appear underneath and items with a higher list position with appear on top).</p>
		/// <p class="note"><b>Note: </b>The <see cref="PathItemTransitionStyle"/> property can be used to apply additional effects to items as they transition into and out of view through the prefix and suffix areas of the <see cref="ItemPath"/>.</p>
		/// </remarks>
		/// <seealso cref="UseZOrderProperty"/>
		/// <seealso cref="ZOrderEffectStopDirectionProperty"/>
		/// <seealso cref="ZOrderEffectStops"/>
		/// <seealso cref="ZOrderEffectStop"/>
		/// <seealso cref="ItemTransitionStyle"/>
		/// <seealso cref="ItemPath"/>
		//[Description("Returns/sets whether ZOrderEffectStops should be honored and ZOrder effects applied to items in the Carousel.  The default is False.")]
		//[Category("Behavior")]
		public bool UseZOrder
		{
			get { return (bool)this.GetValue(CarouselViewSettings.UseZOrderProperty); }
			set { this.SetValue(CarouselViewSettings.UseZOrderProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="UseZOrder"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseZOrder()
		{
			return this.UseZOrder != (bool)CarouselViewSettings.UseZOrderProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="UseZOrder"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseZOrder()
		{
			this.ClearValue(UseZOrderProperty);
		}

				#endregion //UseZOrder

				#region WidthInInfiniteContainers

		/// <summary>
		/// Identifies the <see cref="WidthInInfiniteContainers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WidthInInfiniteContainersProperty = DependencyProperty.Register("WidthInInfiniteContainers",
			typeof(double), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(CarouselViewSettings.DEFAULT_WIDTH_IN_INFINITE_CONTAINERS), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets a value that is used as the default for the width of the <see cref="XamCarouselPanel"/>, when it is placed in a container with infinite width available.  The default is 400.
		/// </summary>
		/// <remarks>
		/// <p class="body">Certain controls such as <see cref="System.Windows.Controls.ScrollViewer"/> and <see cref="System.Windows.Controls.StackPanel"/> make an infinite amount of height and width available to the controls they contain.
		/// If you place a control that implements a Carousel view (e.g., <see cref="XamCarouselPanel"/> or <see cref="XamCarouselListBox"/>) inside one of these controls you may want to set this property to constrain the
		/// width of the control to a convenient value.  If you don't set this property a default width of 400 will be used.</p>
		/// </remarks>
		/// <seealso cref="WidthInInfiniteContainersProperty"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="HeightInInfiniteContainers"/>
		/// <seealso cref="System.Windows.Controls.ScrollViewer"/>
		/// <seealso cref="System.Windows.Controls.StackPanel"/>
		//[Description("Returns/sets a value that is used as default for the height of the XamCarouselPanel when used in a container with infinite height available.  The default is 400.")]
		//[Category("Appearance")]
		public double WidthInInfiniteContainers
		{
			get { return (double)this.GetValue(CarouselViewSettings.WidthInInfiniteContainersProperty); }
			set { this.SetValue(CarouselViewSettings.WidthInInfiniteContainersProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="WidthInInfiniteContainers"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWidthInInfiniteContainers()
		{
			return this.WidthInInfiniteContainers != (double)CarouselViewSettings.WidthInInfiniteContainersProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="WidthInInfiniteContainers"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWidthInInfiniteContainers()
		{
			this.ClearValue(WidthInInfiniteContainersProperty);
		}

				#endregion //WidthInInfiniteContainers

				#region ZOrderEffectStops

		/// <summary>
		/// Identifies the <see cref="ZOrderEffectStops"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ZOrderEffectStopsProperty = DependencyProperty.Register("ZOrderEffectStops",
			typeof(ZOrderEffectStopCollection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnZOrderEffectsChanged)));

		private static void OnZOrderEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselViewSettings carouselViewSettings = d as CarouselViewSettings;
			if (carouselViewSettings != null)
			{
				if (e.OldValue != null)
				{
					ZOrderEffectStopCollection oldZOrderEffectStops = e.OldValue as ZOrderEffectStopCollection;
					if (oldZOrderEffectStops != null)
						oldZOrderEffectStops.PropertyChanged -= new PropertyChangedEventHandler(carouselViewSettings.OnZOrderEffectStopsCollectionChanged);
				}

				if (e.NewValue != null)
				{
					ZOrderEffectStopCollection newZOrderEffectStops = e.NewValue as ZOrderEffectStopCollection;
					if (newZOrderEffectStops != null)
						newZOrderEffectStops.PropertyChanged += new PropertyChangedEventHandler(carouselViewSettings.OnZOrderEffectStopsCollectionChanged);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="ZOrderEffectStop"/> objects that define the stops to be used when setting the ZOrder of items along the <see cref="ItemPath"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>the <see cref="UseZOrder"/> property must be set to true for these <see cref="ZOrderEffectStop"/>s to be applied.</p>
		/// <p class="body">The way in which the scaling effect stops are applied to each item is determined by the item's location in the display area of the
		/// <see cref="XamCarouselPanel"/> and the setting of the <see cref="ZOrderEffectStopDirection"/> property.</p>
		/// </remarks>
		/// <seealso cref="ZOrderEffectStopsProperty"/>
		/// <seealso cref="ZOrderEffectStop"/>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStopDirection"/>
		/// <seealso cref="UseZOrder"/>
		//[Description("Returns a collection of ZOrderEffectStop objects that define the stops to be used when setting the ZOrder of items along the ItemPath.")]
		//[Category("Data")]
		public ZOrderEffectStopCollection ZOrderEffectStops
		{
			get
			{
				return this.GetValue(CarouselViewSettings.ZOrderEffectStopsProperty) as ZOrderEffectStopCollection;
			}
			set { this.SetValue(CarouselViewSettings.ZOrderEffectStopsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ZOrderEffectStops"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeZOrderEffectStops()
		{
			return this.ZOrderEffectStops.ShouldSerialize();
		}

		/// <summary>
		/// Resets the <see cref="ZOrderEffectStops"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetZOrderEffectStops()
		{
			this.ZOrderEffectStops.Clear();
		}

				#endregion //ZOrderEffectStops

				#region ZOrderEffectStopDirection

		/// <summary>
		/// Identifies the <see cref="ZOrderEffectStopDirection"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ZOrderEffectStopDirectionProperty = DependencyProperty.Register("ZOrderEffectStopDirection",
			typeof(EffectStopDirection), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(EffectStopDirection.Default), new ValidateValueCallback(OnValidateZOrderEffectStopDirection));

		private static bool OnValidateZOrderEffectStopDirection(object value)
		{
			if (!Enum.IsDefined(typeof(EffectStopDirection), value))
				throw new InvalidEnumArgumentException("value",(int)value, typeof(EffectStopDirection));

			return true;
		}

		/// <summary>
		/// Returns/sets the direction used to evaluate <see cref="ZOrderEffectStop"/>s.  The default is <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <remarks>
		/// <p class="body">The direction can be set to evaluate the effect stops based on an item's position along the <see cref="ItemPath"/> or based on the item's vertical or horizontal position within the <see cref="XamCarouselPanel"/>.</p>
		/// </remarks>
		/// <seealso cref="ZOrderEffectStopDirectionProperty"/>
		/// <seealso cref="ZOrderEffectStops"/>
		/// <seealso cref="ZOrderEffectStop"/>
		/// <seealso cref="UseZOrder"/>
		/// <seealso cref="EffectStop"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns/sets the direction used to evaluate ZOrderEffectStops.  The default is EffectStopDirection.UseItemPath.")]
		//[Category("Appearance")]
		public EffectStopDirection ZOrderEffectStopDirection
		{
			get { return (EffectStopDirection)this.GetValue(CarouselViewSettings.ZOrderEffectStopDirectionProperty); }
			set { this.SetValue(CarouselViewSettings.ZOrderEffectStopDirectionProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="ZOrderEffectStopDirection"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeZOrderEffectStopDirection()
		{
			return this.ZOrderEffectStopDirection != (EffectStopDirection)CarouselViewSettings.ZOrderEffectStopDirectionProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ZOrderEffectStopDirection"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetZOrderEffectStopDirection()
		{
			this.ClearValue(ZOrderEffectStopDirectionProperty);
		}

				#endregion //ZOrderEffectStopDirection

            #endregion // Public Properties

			#region Internal Properties

				#region Version

		private static readonly DependencyPropertyKey VersionPropertyKey =
			DependencyProperty.RegisterReadOnly("Version",
			typeof(int), typeof(CarouselViewSettings), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Identifies the <see cref="Version"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VersionProperty =
			VersionPropertyKey.DependencyProperty;

		internal int Version
		{
			get
			{
				return (int)this.GetValue(CarouselViewSettings.VersionProperty);
			}
		}

				#endregion //Version

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal methods

				#region EnumeratePropertiesWithNonDefaultValues

		internal void EnumeratePropertiesWithNonDefaultValues( PropertyChangedEventHandler callback )
		{
			LocalValueEnumerator enumerator = this.GetLocalValueEnumerator();

			while (enumerator.MoveNext())
			{
				LocalValueEntry entry = enumerator.Current;

				if (!Object.Equals(entry.Property.DefaultMetadata.DefaultValue, entry.Value))
					callback( this, new PropertyChangedEventArgs(entry.Property.Name));
			}
		}

				#endregion //EnumeratePropertiesWithNonDefaultValues	
    
			#endregion //Internal Methods

			#region Private Methods

				#region OnOpacityEffectStopsCollectionChanged

		void OnOpacityEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaisePropertyChangedEvent("OpacityEffectStops");
		}

				#endregion OnOpacityEffectStopsCollectionChanged

				#region OnScalingEffectStopsCollectionChanged

		void OnScalingEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaisePropertyChangedEvent("ScalingEffectStops");
		}

				#endregion OnScalingEffectStopsCollectionChanged

				#region OnSkewAngleXEffectStopsCollectionChanged

		void OnSkewAngleXEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaisePropertyChangedEvent("SkewAngleXEffectStops");
		}

				#endregion OnSkewAngleXEffectStopsCollectionChanged

				#region OnSkewAngleYEffectStopsCollectionChanged

		void OnSkewAngleYEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaisePropertyChangedEvent("SkewAngleYEffectStops");
		}

				#endregion OnSkewAngleYEffectStopsCollectionChanged

				#region OnZOrderEffectStopsCollectionChanged

		void OnZOrderEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaisePropertyChangedEvent("ZOrderEffectStops");
		}

				#endregion OnZorderEffectStopsCollectionChanged

				#region ValidatePositiveDouble

		private static bool ValidatePositiveDouble(object value)
		{
			if (!(value is double))
				return false;

			if (double.IsNaN((double)value))
				return false;

			return !((double)value <= 0.0);
		}

				#endregion //ValidatePositiveDouble	

			#endregion //Private Methods

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