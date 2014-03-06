using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Windows.Controls;
using System.Diagnostics;
using Infragistics.Shared;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Decorator used to provide chrome around a <see cref="IRibbonTool"/> inside a <see cref="RibbonGroup"/>, <see cref="ApplicationMenuFooterToolbar"/> or the <see cref="QuickAccessToolbar"/>  
	/// </summary>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="RibbonGroup"/>
	/// <seealso cref="ButtonGroup"/>
	/// <seealso cref="ApplicationMenuFooterToolbar"/>
	/// <seealso cref="XamRibbon.QuickAccessToolbar"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonButtonChrome : Decorator
	{
		#region Private Members

		private FrameworkElement		_tool;
		private ToolLocation			_location;
		private bool					_isActive;
		private RibbonToolSizingMode	_sizingMode = RibbonToolSizingMode.ImageAndTextLarge;
		private MenuButtonArea			_menuButtonArea;

		// AS 10/10/07
		private bool					_isQuickCustomizeMenu;

		private static readonly Thickness ZeroPixelThickness = new Thickness(0);
		private static readonly Thickness OnePixelThickness = new Thickness(1);
		private static readonly Thickness TwoPixelThickness = new Thickness(2);

		private const int				OuterLayer = 0;
		private const int				MiddleLayer = 1;
		private const int				InnerLayer = 2;
		private const int				OverlayLayer = 3;
		private const int				GlowLayer = 4;

		private ChromeLayer[]			_layers;

        // AS 2/5/09 TFS9385
        private Rect                    _lastArrangeRect;

        #endregion //Private Members	
    
		#region Constructor

		static RibbonButtonChrome()
		{
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="RibbonButtonChrome"/> class.
		/// </summary>
		public RibbonButtonChrome()
		{
			this._layers = new ChromeLayer[5];
			this._layers[OuterLayer] = new ChromeLayer(OuterFillBrushProperty, OuterPenBrushProperty);
			this._layers[MiddleLayer] = new ChromeLayer(MiddleFillBrushProperty, MiddlePenBrushProperty);
			this._layers[InnerLayer] = new ChromeLayer(InnerFillBrushProperty, InnerPenBrushProperty);
			this._layers[OverlayLayer] = new ChromeLayer(OverlayFillBrushProperty, null);
			this._layers[GlowLayer] = new ChromeLayer(GlowFillBrushProperty, null);

            // AS 2/5/09 TFS9385
            this.Loaded += new RoutedEventHandler(OnLoaded);
		}

		#endregion //Constructor	
    
		#region Base class overrides

			#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="arrangeSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size arrangeSize)
		{
			UIElement child = this.Child;

			if (null != child)
			{
				Rect childRect = new Rect(arrangeSize);
				childRect = Subtract(childRect, this.CalculateBorderThickness());
				childRect = Subtract(childRect, this.Padding);

				child.Arrange(childRect);
			}

			this.InitializeLayers(arrangeSize);

			return arrangeSize;
		}
			#endregion //ArrangeOverride

			#region HitTestCore

			/// <summary>
			/// Determines if the specified point is within the bounds of this element.
			/// </summary>
			/// <param name="hitTestParameters">Specifies the point for the hit test.</param>
			/// <returns>A <see cref="HitTestResult"/> indicating the result of the hit test operation.</returns>
			/// <remarks>
			/// <p class="body">
			/// This method is overridden on this class to make sure the element gets mouse messages
			/// regardless of whether its background is transparent or not.
			/// </p>
			/// </remarks>
			protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			Rect rect = new Rect(new Point(), this.RenderSize);
			if (rect.Contains(hitTestParameters.HitPoint))
				return new PointHitTestResult(this, hitTestParameters.HitPoint);

			return base.HitTestCore(hitTestParameters);
		}

			#endregion // HitTestCore

			#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size sizeAroundChild = new Size();
			sizeAroundChild = Add(sizeAroundChild, this.Padding);
			sizeAroundChild = Add(sizeAroundChild, this.CalculateBorderThickness());

			Size desiredSize = new Size();
			UIElement child = this.Child;

			if (null != child)
			{
				Size measureSize = constraint;

				if (double.IsPositiveInfinity(constraint.Width) == false)
					measureSize.Width = Math.Max(constraint.Width - sizeAroundChild.Width, 0);

				if (double.IsPositiveInfinity(constraint.Height) == false)
					measureSize.Height = Math.Max(constraint.Height - sizeAroundChild.Height, 0);

				child.Measure(measureSize);

				desiredSize = child.DesiredSize;
			}

			desiredSize.Width += sizeAroundChild.Width;
			desiredSize.Height += sizeAroundChild.Height;

			// round up the width so the right side doesn't overlap the left
			desiredSize.Width = Math.Ceiling(desiredSize.Width);

			return desiredSize;
		}
			#endregion //MeasureOverride

            #region OnInitialized
        /// <summary>
        /// Called after the control has been initialized.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // AS 10/21/08 TFS9385/Optimization
            this.VerifyBrushes();
        }
            #endregion //OnInitialized

			#region OnPropertyChanged
		/// <summary>
		/// Invoked when a property of the element has been changed.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == UIElement.IsMouseOverProperty)
			{
				this.VerifyBrushes();
			}

			base.OnPropertyChanged(e);
		} 
			#endregion //OnPropertyChanged

			#region OnRender
		/// <summary>
		/// Called when the element should render its contents.
		/// </summary>
		/// <param name="drawingContext">An initialized DrawingContext that should be used for all drawing within this method.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			for (int i = 0; i < this._layers.Length; i++)
			{
				if (this._layers[i].IsEmpty == false)
					this._layers[i].Draw(drawingContext, this);
			}

			#region Segmented Border
			Brush penBrush = this.SeparatorPenBrush;

			if (null != penBrush)
			{
				Pen pen = new Pen(penBrush, 1d);
				Rect rect = new Rect(this.RenderSize);

				// do not include the padded area as part of the rendering
				rect = Subtract(rect, this.Padding);

				rect.Inflate(-0.5d, -0.5d);

				// large tools have a 2 pixel border
				if (this._sizingMode == RibbonToolSizingMode.ImageAndTextLarge)
				{
					RenderStates currentState = this.CurrentState;

					// do not overlap the border unless the button is checked and unhotttracked
					if (currentState != RenderStates.Checked)
						rect.Inflate(-0.5d, 0d);

					drawingContext.DrawLine(pen, new Point(rect.Left, rect.Bottom - 1), new Point(rect.Right, rect.Bottom - 1));

					// AS 11/9/07 BR27990
					//pen = new Pen(new SolidColorBrush(Color.FromArgb(179, 255, 255, 255)), 1d);
					Brush lightBrush = null;

					if ((currentState & RenderStates.Disabled) != 0)
						lightBrush = this.FindResource(RibbonBrushKeys.LargeSegmentedDisabledLightSeparatorFillKey) as Brush;

					if (lightBrush == null)
						lightBrush = new SolidColorBrush(Color.FromArgb(179, 255, 255, 255));

					pen = new Pen(lightBrush, 1d);
					drawingContext.DrawLine(pen, new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));
				}
				else if (ButtonGroup.GetIsInButtonGroup(this._tool))
				{
					rect.Inflate(0d, -0.5d);

					if (this.IsDropDownButtonOfSegmentedMenu)
					{
						// draw a light line on the left edge
						drawingContext.DrawLine(pen, new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom));
					}
					else
					{
						drawingContext.DrawLine(pen, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom));
					}
				}
				else
				{
					drawingContext.DrawLine(pen, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom));
				}
			}
			#endregion //Segmented Border
		}
			#endregion //OnRender

			#region OnVisualParentChanged
			/// <summary>
			/// Invoked when the parent element of this element reports a change to its underlying visual parent.
			/// </summary>
			/// <param name="oldParent">The previous parent. This may be null if the element did not have a parent element previously.</param>
			protected override void OnVisualParentChanged(DependencyObject oldParent)
			{
				base.OnVisualParentChanged(oldParent);

				FrameworkElement newTool = Utilities.GetAncestorFromType(this, typeof(IRibbonTool), true, null, typeof(XamRibbon)) as FrameworkElement;

				if (newTool != this._tool)
				{
					if (newTool == null)
					{
						this.ClearValue(IsActiveProperty);
						this.ClearValue(LocationProperty);
						this.ClearValue(SizingModeProperty);
						this.ClearValue(IsInButtonGroupProperty);
						this.ClearValue(IsFirstInButtonGroupProperty);
						this.ClearValue(IsLastInButtonGroupProperty);
						this._tool = null;
					}
					else
					{
						this._tool = newTool;
						this.SetBinding(IsActiveProperty, Utilities.CreateBindingObject(XamRibbon.IsActiveProperty, System.Windows.Data.BindingMode.OneWay, newTool));
						this.SetBinding(LocationProperty, Utilities.CreateBindingObject(XamRibbon.LocationProperty, System.Windows.Data.BindingMode.OneWay, newTool));
						this.SetBinding(SizingModeProperty, Utilities.CreateBindingObject(RibbonToolHelper.SizingModeProperty, System.Windows.Data.BindingMode.OneWay, newTool));
						this.SetBinding(IsInButtonGroupProperty, Utilities.CreateBindingObject(ButtonGroup.IsInButtonGroupProperty, System.Windows.Data.BindingMode.OneWay, newTool));
						this.SetBinding(IsFirstInButtonGroupProperty, Utilities.CreateBindingObject(ButtonGroup.IsFirstInButtonGroupProperty, System.Windows.Data.BindingMode.OneWay, newTool));
						this.SetBinding(IsLastInButtonGroupProperty, Utilities.CreateBindingObject(ButtonGroup.IsLastInButtonGroupProperty, System.Windows.Data.BindingMode.OneWay, newTool));
					}
				}

				this._menuButtonArea = newTool is MenuTool
					? Utilities.GetAncestorFromType(this, typeof(MenuButtonArea), true, null, typeof(XamRibbon)) as MenuButtonArea
					: null;

				if (this._menuButtonArea != null)
				{
					this.SetBinding(IsMouseOverMenuButtonAreaProperty, Utilities.CreateBindingObject(FrameworkElement.IsMouseOverProperty, System.Windows.Data.BindingMode.OneWay, this._menuButtonArea));
					this.SetBinding(IsMenuOpenProperty, Utilities.CreateBindingObject(MenuTool.IsOpenProperty, System.Windows.Data.BindingMode.OneWay, newTool));
					this.SetBinding(IsMenuCheckedProperty, Utilities.CreateBindingObject(MenuTool.IsCheckedProperty, System.Windows.Data.BindingMode.OneWay, newTool));

					// AS 10/10/07
					this._isQuickCustomizeMenu = this._menuButtonArea.IsQuickCustomizeMenu;
				}
				else
				{
					this.ClearValue(IsMouseOverMenuButtonAreaProperty);
					this.ClearValue(IsMenuOpenProperty);
					this.ClearValue(IsMenuCheckedProperty);

					// AS 10/10/07
					this._isQuickCustomizeMenu = false;
				}

				if (this._tool == null && this.IsLoaded)
				{
					// JM 12-14-07 - This scode is getting executed when the XamRibbon is deleted from the VS2008 design surface.  The fact that IsLoaded
					// is returning true here must be a timing issue.  Adding a check for VisualParent != null.
					if (VisualTreeHelper.GetParent(this) != null)
						throw new NotSupportedException(XamRibbon.GetString("LE_InvalidRibbonButtonChromeParent"));
				}
			} 
			#endregion //OnVisualParentChanged

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region IsChecked

		/// <summary>
		/// Identifies the <see cref="IsChecked"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked",
			typeof(bool?), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsCheckedChanged)));

		private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = d as RibbonButtonChrome;

			if (chrome != null)
			{
				if (chrome._menuButtonArea != null)
					chrome._menuButtonArea.InvalidateVisual();

				chrome.VerifyBrushes();
			}
		}

		/// <summary>
		/// Gete/sets whether the chrome should be displayed reflecting the IsChecked state of the associated tool. 
		/// </summary>
		/// <seealso cref="IsCheckedProperty"/>
		//[Description("Gete/sets whether the chrome should be displayed reflecting the IsChecked state of the associated tool. ")]
		//[Category("Ribbon Properties")]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? IsChecked
		{
			get
			{
                // JJD 1/8/08 - BR29451
                // Cast the value to a nullable bool not a bool. Otherwise a null ref excepttion is thrown if value is null.
				//return (bool)this.GetValue(RibbonButtonChrome.IsCheckedProperty);
				return (bool?)this.GetValue(RibbonButtonChrome.IsCheckedProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.IsCheckedProperty, value);
			}
		}

				#endregion //IsChecked

				#region IsPressed

		/// <summary>
		/// Identifies the <see cref="IsPressed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register("IsPressed",
			typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsPressedChanged)));

		private static void OnIsPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((RibbonButtonChrome)d).VerifyBrushes();
		}

		/// <summary>
		/// Gete/sets whether the chrome should be displayed reflecting the IsPressed state of the associated tool. 
		/// </summary>
		/// <seealso cref="IsPressedProperty"/>
		//[Description("Gete/sets whether the chrome should be displayed reflecting the IsPressed state of the associated tool.")]
		//[Category("Ribbon Properties")]
		public bool IsPressed
		{
			get
			{
				return (bool)this.GetValue(RibbonButtonChrome.IsPressedProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.IsPressedProperty, value);
			}
		}

				#endregion //IsPressed

				#region IsSegmentedButton

		/// <summary>
		/// Identifies the <see cref="IsSegmentedButton"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSegmentedButtonProperty = DependencyProperty.Register("IsSegmentedButton",
			typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSegmentedButtonChanged)));

		private static void OnIsSegmentedButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = d as RibbonButtonChrome;

            if (chrome != null && chrome._tool is MenuTool)
                chrome.InvalidateVisual();
		}

		/// <summary>
		/// Returns or sets a boolean indicating if this element represents the button portion of a segmented MenuTool.
		/// </summary>
		/// <seealso cref="IsSegmentedButtonProperty"/>
		//[Description("Returns or sets a boolean indicating if this element represents the button portion of a segmented MenuTool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsSegmentedButton
		{
			get
			{
				return (bool)this.GetValue(RibbonButtonChrome.IsSegmentedButtonProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.IsSegmentedButtonProperty, value);
			}
		}

				#endregion //IsSegmentedButton

				#region Padding

		/// <summary>
		/// Identifies the <see cref="Padding"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding",
			typeof(Thickness), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns the amount of padding between the border and the element.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> The padding of this element is special in that it affects not only where the element will 
		/// position its children but also in that the element will not render to that area. This allows the element to respond to mouse 
		/// messages but not render within an outer area of the element.</p>
		/// </remarks>
		/// <seealso cref="PaddingProperty"/>
		//[Description("Returns the amount of padding between the border and the element.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public Thickness Padding
		{
			get
			{
				return (Thickness)this.GetValue(RibbonButtonChrome.PaddingProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.PaddingProperty, value);
			}
		}

				#endregion //Padding

				// AS 3/26/12 NA 12.1
				// Added property to allow metro theme to disable the rounded borders.
				//
				#region UseRoundedCorners

		/// <summary>
		/// Identifies the <see cref="UseRoundedCorners"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseRoundedCornersProperty = DependencyProperty.Register("UseRoundedCorners",
			typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnUseRoundedCornersChanged)));

		private static void OnUseRoundedCornersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var chrome = d as RibbonButtonChrome;
			var allow = true.Equals(e.NewValue);

			foreach (var layer in chrome._layers)
				layer.AllowRoundedCorners = allow;

			if (chrome.IsArrangeValid && chrome.IsLoaded)
			{
				chrome.InitializeLayers();

				chrome.InvalidateVisual();
			}
		}

		/// <summary>
		/// Returns or sets a value indicating whether the borders should be rounded.
		/// </summary>
		/// <seealso cref="UseRoundedCornersProperty"/>
		[Bindable(true)]
		public bool UseRoundedCorners
		{
			get
			{
				return (bool)this.GetValue(RibbonButtonChrome.UseRoundedCornersProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.UseRoundedCornersProperty, value);
			}
		}

				#endregion //UseRoundedCorners

			#endregion //Public Properties

		#region Private Properties

		#region CurrentState
		private RenderStates CurrentState
		{
			get
			{
				RenderStates state = RenderStates.Normal;

				// JM BR27215 10-10-07
				//if (this._tool.IsEnabled)
				{
					if (this.IsMouseOver || this._isActive)
						state |= RenderStates.HotTracked;

                    // JJD 1/8/08 - BR29451
                    // Don't cast to a bool since IsChecked is a nullable bool.
                    //if ((bool)this.IsChecked)
					if (this.IsChecked == true)
						state |= RenderStates.Checked;

					if (this.IsPressed)
						state |= RenderStates.Pressed;

					MenuTool menu = this._tool as MenuTool;

					if (null != menu)
					{
						bool isSegmented = this.IsSegmentedButton;
						bool isChecked = (state & RenderStates.Checked) != 0;

						if (menu.IsOpen)
						{
							#region Open Menu
							if (isSegmented == false)
							{
								// AS 10/10/07
								// The open quickcustomize looks like its pressed when its open whereas
								// regular menus look like they're checked.
								//
								if (this._isQuickCustomizeMenu)
								{
									// render the open quick customize as pressed
									state &= ~(RenderStates.HotTracked | RenderStates.Checked);
									state |= RenderStates.Pressed;
								}
								else 
								{
									// do not render the dropdown button as hottracked or pressed when the menu is open
									state &= ~(RenderStates.HotTracked | RenderStates.Pressed);
								}
							}
							else // if (isSegmented)
							{
								// a segmented button should render as it does
								// when hottracked by the other button when the menu is open
								// AS 11/9/07 BR27990
								// When its disabled, inactive and unchecked, then use the grayish gradients.
								//
								if (this._isActive == false && isChecked == false && this.IsEnabled == false)
									state |= RenderStates.Disabled;
								else
									state |= RenderStates.HotTracked | RenderStates.HotTrackedByOtherSegment;

								state &= ~RenderStates.Checked;
							} 
							#endregion //Open Menu
						}
						else if (menu.ButtonType != MenuToolButtonType.DropDown && (state & RenderStates.Pressed) == 0)
						{
							#region Closed segmented menu tool
							bool isMouseOver = (state & RenderStates.HotTracked) != 0;

							if (isSegmented)
							{
								#region Button portion of segmented button
								if (isChecked)
								{
									// if its checked and the mouse is anywhere over the tool render as checked hot
									// unless its in a button group 
									if (this.IsMouseOverMenuButtonArea && false == ButtonGroup.GetIsInButtonGroup(this._tool))
										state |= RenderStates.HotTracked;
								}
								// AS 11/9/07 BR27990
								else if (this.IsEnabled == false
									&& this._isActive == false
									&& this.IsMouseOverMenuButtonArea == true
									&& isChecked == false)
								{
									state |= RenderStates.Disabled;
								}
								else if (isMouseOver == false && this.IsMouseOverMenuButtonArea)
								{
									state |= RenderStates.HotTracked | RenderStates.HotTrackedByOtherSegment;
								}
								//else if (this.IsEnabled == false && this._isActive)
								else if (this._isActive)
								{
									// if its unchecked and disabled but the tool is active, draw the overlay
									state |= RenderStates.HotTrackedByOtherSegment;
								}
								#endregion //Button portion of segmented button
							}
							else
							{
								#region Dropdown portion of segmented button

								// the dropdown button of a segmented button
								bool isMenuChecked = menu.IsChecked && menu.ButtonType == MenuToolButtonType.SegmentedState;

								if (isMenuChecked)
								{
									// if the push button is checked...
									if (isMouseOver)
										state |= RenderStates.HotTracked;
									else if (this.IsMouseOverMenuButtonArea)
										state |= RenderStates.HotTracked | RenderStates.HotTrackedByOtherSegment;
									else if (false == ButtonGroup.GetIsInButtonGroup(this._tool))// when checked but the menu button area is not hottracked, don't draw anything
										state |= RenderStates.Checked;
								}
								else
								{
									// AS 11/9/07 BR27990
									// Always draw an overlay when active and not open.
									//
									//if (this._isActive && this.IsEnabled == false)
									if (this._isActive)
									{
										// if the button is disabled and the active tool then just draw the overlay
										state |= RenderStates.HotTrackedByOtherSegment;
									}
									else if (isMouseOver == false && this.IsMouseOverMenuButtonArea)
									{
										// if the menu is not checked or the segmented button is pressed and 
										// the mouse is not over the dropdown button itself, just draw as if
										// hottracked with an overlay
										state |= RenderStates.HotTracked | RenderStates.HotTrackedByOtherSegment;
									}
									else
									{
										// the dropdown should not draw as hottracked
									}
								}
								#endregion //Dropdown portion of segmented button
							} 
							#endregion //Closed segmented menu tool
						}
					}
				}

				return state;
			}
		} 
				#endregion //CurrentState

				#region GlowFillBrush

		private static readonly DependencyProperty GlowFillBrushProperty = DependencyProperty.Register("GlowFillBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush GlowFillBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.GlowFillBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.GlowFillBrushProperty, value);
			}
		}

				#endregion //GlowFillBrush

				#region InnerFillBrush

		private static readonly DependencyProperty InnerFillBrushProperty = DependencyProperty.Register("InnerFillBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush InnerFillBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.InnerFillBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.InnerFillBrushProperty, value);
			}
		}

				#endregion //InnerFillBrush

				#region InnerPenBrush

		private static readonly DependencyProperty InnerPenBrushProperty = DependencyProperty.Register("InnerPenBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush InnerPenBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.InnerPenBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.InnerPenBrushProperty, value);
			}
		}

				#endregion //InnerPenBrush

				#region IsActive

			private static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsActiveChanged)));

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);
			chrome._isActive = (bool)e.NewValue;

			chrome.VerifyBrushes();
		}

				#endregion //IsActive

				#region IsDropDownButtonOfSegmentedMenu
		private bool IsDropDownButtonOfSegmentedMenu
		{
			get
			{
				MenuTool menu = this._tool as MenuTool;

				return menu != null && menu.ButtonType != MenuToolButtonType.DropDown && this.IsSegmentedButton == false;
			}
		} 
				#endregion //IsDropDownButtonOfSegmentedMenu

				#region IsInButtonGroup

			private static readonly DependencyProperty IsInButtonGroupProperty = DependencyProperty.Register("IsInButtonGroup",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsInButtonGroupChanged)));

		private static void OnIsInButtonGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);
			chrome.VerifyBrushes();
		}

				#endregion //IsInButtonGroup

				#region IsFirstInButtonGroup

			private static readonly DependencyProperty IsFirstInButtonGroupProperty = DependencyProperty.Register("IsFirstInButtonGroup",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsFirstInButtonGroupChanged)));

		private static void OnIsFirstInButtonGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);
			chrome.VerifyBrushes();
		}

		private bool IsFirstInButtonGroup
		{
			get { return ButtonGroup.GetIsFirstInButtonGroup(this._tool) && this.IsDropDownButtonOfSegmentedMenu == false; }
		}

				#endregion //IsFirstInButtonGroup

				#region IsLargeRibbonTool
		private bool IsLargeRibbonTool
		{
			get { return this._sizingMode == RibbonToolSizingMode.ImageAndTextLarge && this._location == ToolLocation.Ribbon; }
		} 
				#endregion //IsLargeRibbonTool

				#region IsLastInButtonGroup

			private static readonly DependencyProperty IsLastInButtonGroupProperty = DependencyProperty.Register("IsLastInButtonGroup",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsLastInButtonGroupChanged)));

		private static void OnIsLastInButtonGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);
			chrome.VerifyBrushes();
		}

		private bool IsLastInButtonGroup
		{
			get { return ButtonGroup.GetIsLastInButtonGroup(this._tool) && this.IsSegmentedButton == false; }
		}
				#endregion //IsLastInButtonGroup

				#region IsMenuChecked

			private static readonly DependencyProperty IsMenuCheckedProperty = DependencyProperty.Register("IsMenuChecked",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsMenuCheckedChanged)));

		private static void OnIsMenuCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);

			if (chrome.IsDropDownButtonOfSegmentedMenu)
				chrome.VerifyBrushes();
		}

				#endregion //IsMenuChecked

				#region IsMenuOpen

			private static readonly DependencyProperty IsMenuOpenProperty = DependencyProperty.Register("IsMenuOpen",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsMenuOpenChanged)));

		private static void OnIsMenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);

			if (false == MenuToolButtonType.DropDown.Equals(chrome._tool.GetValue(MenuTool.ButtonTypeProperty)))
				chrome.VerifyBrushes();
		}

				#endregion //IsMenuOpen

				#region IsMouseOverMenuButtonArea

			private static readonly DependencyProperty IsMouseOverMenuButtonAreaProperty = DependencyProperty.Register("IsMouseOverMenuButtonArea",
				typeof(bool), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsMouseOverMenuButtonAreaChanged)));

		private static void OnIsMouseOverMenuButtonAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);

			if (chrome.IsSegmentedButton)
				chrome.InvalidateArrange();

			chrome.VerifyBrushes();
		}

		private bool IsMouseOverMenuButtonArea
		{
			get { return (bool)this.GetValue(IsMouseOverMenuButtonAreaProperty); }
		}

				#endregion //IsMouseOverMenuButtonArea

				#region IsMouseOverResolved
		private bool IsMouseOverResolved
		{
			get
			{
				bool isOver = this._isActive || this.IsMouseOver;

				// if this is chrome for a menu and the mouse is not over the chrome itself
				// but is over the menu button area...
				if (isOver == false && this._tool is MenuToolBase)
				{
					// if the menu is open then render the segmented button as hot
					// as long as its not checked
					if (((MenuToolBase)this._tool).IsOpen)
					{
                        // JJD 1/8/08 - BR29451
                        // Don't cast to a bool since IsChecked is a nullable bool.
                        if (this.IsSegmentedButton && this.IsChecked == false)
							isOver = true;
					}
					else if (this.IsMouseOverMenuButtonArea)
					{
						if (this.IsSegmentedButton == false || this.IsChecked == false)
							isOver = true;
					}
				}

				return isOver;
			}
		} 
				#endregion //IsMouseOverResolved

				#region Location

			private static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location",
				typeof(ToolLocation), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(RibbonKnownBoxes.ToolLocationUnknownBox, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnLocationChanged)));

		private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            RibbonButtonChrome chrome = (RibbonButtonChrome)d;
			chrome._location = (ToolLocation)e.NewValue;

            // AS 10/21/08 TFS9385
            // The brushes used are based on the location so when the location changes 
            // we need to update the brushes used to render the element.
            //
            chrome.VerifyBrushes();
		}

				#endregion //Location

				#region MiddlePenBrush

		private static readonly DependencyProperty MiddlePenBrushProperty = DependencyProperty.Register("MiddlePenBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush MiddlePenBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.MiddlePenBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.MiddlePenBrushProperty, value);
			}
		}

				#endregion //MiddlePenBrush

				#region MiddleFillBrush

		private static readonly DependencyProperty MiddleFillBrushProperty = DependencyProperty.Register("MiddleFillBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush MiddleFillBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.MiddleFillBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.MiddleFillBrushProperty, value);
			}
		}

				#endregion //MiddleFillBrush

				#region OuterFillBrush

		private static readonly DependencyProperty OuterFillBrushProperty = DependencyProperty.Register("OuterFillBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush OuterFillBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.OuterFillBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.OuterFillBrushProperty, value);
			}
		}

				#endregion //OuterFillBrush

				#region OuterPenBrush

		private static readonly DependencyProperty OuterPenBrushProperty = DependencyProperty.Register("OuterPenBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush OuterPenBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.OuterPenBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.OuterPenBrushProperty, value);
			}
		}

				#endregion //OuterPenBrush

				#region OverlayFillBrush

		private static readonly DependencyProperty OverlayFillBrushProperty = DependencyProperty.Register("OverlayFillBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush OverlayFillBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.OverlayFillBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.OverlayFillBrushProperty, value);
			}
		}

				#endregion //OverlayFillBrush

				#region SeparatorPenBrush

		private static readonly DependencyProperty SeparatorPenBrushProperty = DependencyProperty.Register("SeparatorPenBrush",
			typeof(Brush), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		private Brush SeparatorPenBrush
		{
			get
			{
				return (Brush)this.GetValue(RibbonButtonChrome.SeparatorPenBrushProperty);
			}
			set
			{
				this.SetValue(RibbonButtonChrome.SeparatorPenBrushProperty, value);
			}
		}

				#endregion //SeparatorPenBrush

				#region SizingMode

			private static readonly DependencyProperty SizingModeProperty = DependencyProperty.Register("SizingMode",
				typeof(RibbonToolSizingMode), typeof(RibbonButtonChrome), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnSizingModeChanged)));

		private static void OnSizingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonButtonChrome chrome = ((RibbonButtonChrome)d);
			chrome._sizingMode = (RibbonToolSizingMode)e.NewValue;

			if (chrome.IsArrangeValid && chrome.IsLoaded)
				chrome.InitializeLayers();

			chrome.VerifyBrushes();
		}

				#endregion //SizingMode

			#endregion //Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region CalculateBorderThickness

		/// <summary>
		/// Calculates the space around the child element
		/// </summary>
		private Thickness CalculateBorderThickness()
		{
			if (this._tool == null)
				return ZeroPixelThickness;

			MenuTool menu = this._tool as MenuTool;
			bool isSegmented = menu != null && menu.ButtonType != MenuToolButtonType.DropDown;
			bool isTopLeftSegment = isSegmented && this.IsSegmentedButton;
			bool isBottomRightSegment = isSegmented && isTopLeftSegment == false;

			switch (this._location)
			{
				case ToolLocation.ApplicationMenuFooterToolbar:
				case ToolLocation.QuickAccessToolbar:
					if (ButtonGroup.GetIsInButtonGroup(this._tool))
					{
						bool isFirst = this.IsFirstInButtonGroup;
						bool isLast = this.IsLastInButtonGroup;
						Thickness margin = new Thickness(1, 2, 1, 1);

						if (isLast)
							margin.Right = 2;

						if (isFirst || isLast)
							margin.Left = 2;

						return margin;
					}

					// even though we're not using an inner border for amft
					return TwoPixelThickness;

				case ToolLocation.Ribbon:
                case ToolLocation.Unknown:  // AS 2/19/09 TFS6747
					// button group
					if (ButtonGroup.GetIsInButtonGroup(this._tool))
					{
						bool isFirstInGroup = this.IsFirstInButtonGroup;
						bool isLastInGroup = this.IsLastInButtonGroup;
						bool isMiddle = isFirstInGroup == false && isLastInGroup == false;

						Thickness margin = new Thickness(2);

						if (isBottomRightSegment == false)
						{
							if (isFirstInGroup || isMiddle)
								margin.Left = 3;

							if (isMiddle || (isFirstInGroup && isLastInGroup))
								margin.Right = 3;
						}

						return margin;
					}

					if (this._sizingMode == RibbonToolSizingMode.ImageAndTextLarge)
					{
						if (isBottomRightSegment)
							return new Thickness(2, 0, 2, 2);
						else if (isTopLeftSegment)
							return new Thickness(2, 2, 2, 0);
					}
					else if (isBottomRightSegment)
						return new Thickness(0, 2, 2, 2);
					else if (isTopLeftSegment)
						return new Thickness(2, 2, 0, 2);

					return TwoPixelThickness;

				default:
					return ZeroPixelThickness;
			}
		}

				#endregion //CalculateBorderThickness	
    
				#region GetMenuButtonAreaRect
		private Rect GetMenuButtonAreaRect(Size chromeRenderSize)
		{
			Rect areaRect = new Rect(chromeRenderSize);
			MenuTool menu = this._tool as MenuTool;

			// do not include the padding in the render area
			areaRect = Subtract(areaRect, this.Padding);

			if (menu != null && this._menuButtonArea != null && menu.ButtonType != MenuToolButtonType.DropDown)
			{
				areaRect = new Rect(this._menuButtonArea.LastArrangeSize);

				// do not include the padding in the render area
				areaRect = Subtract(areaRect, this.Padding);

				
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				// note: we're not using a transform bounds call here because
				// we're using this within the arrange so the parent hasn't 
				// been arranged by this point
				// if this is the bottom/right portion then adjust the rect
				if (this.IsSegmentedButton == false)
				{
					if (this.IsLargeRibbonTool)
						areaRect.Offset(0, chromeRenderSize.Height - areaRect.Height);
					else
						areaRect.Offset(chromeRenderSize.Width - areaRect.Width, 0);
				}
			}

			return areaRect;
		} 
				#endregion //GetMenuButtonAreaRect

				#region InitializeButtonGroupLayers
		private void InitializeButtonGroupLayers(Rect arrangeRect)
		{
			Debug.Assert(this._tool != null);

			bool isFirstInGroup = this.IsFirstInButtonGroup;
			bool isLastInGroup = this.IsLastInButtonGroup;
			bool isMiddle = isFirstInGroup == false && isLastInGroup == false;

			RenderStates state = this.CurrentState;

			bool isMouseOver = (state & RenderStates.HotTracked) != 0;
			bool isChecked = (state & RenderStates.Checked) != 0;
			bool isPressed = (state & RenderStates.Pressed) != 0;

			#region Outer Layer

			// if we're hottracked then draw the right border as dark
			bool drawRightBorder = isMouseOver || isPressed || isChecked;

			RoundedRectCorners corners;
			RoundedRectSide firstSide;
			RoundedRectSide lastSide;
			bool useCustomBorder = false;

			if (isFirstInGroup && isLastInGroup)
			{
				corners = RoundedRectCorners.All;
				firstSide = lastSide = RoundedRectSide.Left;
			}
			else if (isFirstInGroup)
			{
				corners = RoundedRectCorners.All & ~RoundedRectCorners.Right;
				if (drawRightBorder)
					firstSide = lastSide = RoundedRectSide.Left;
				else
				{
					firstSide = RoundedRectSide.Bottom;
					lastSide = RoundedRectSide.Top;
				}
			}
			else if (isLastInGroup)
			{
				corners = RoundedRectCorners.All & ~RoundedRectCorners.Left;
				firstSide = RoundedRectSide.Top;
				lastSide = RoundedRectSide.Bottom;
			}
			else
			{
				// we need 3 straight sides
				corners = RoundedRectCorners.None;
				firstSide = RoundedRectSide.Top;
				lastSide = RoundedRectSide.Bottom;

				 if (drawRightBorder == false)
					 useCustomBorder = true;
			}

			if (useCustomBorder == false)
			{
				this._layers[OuterLayer].Initialize(arrangeRect, ZeroPixelThickness, 2d, corners, firstSide, lastSide);

				Thickness margin = new Thickness(1);

				if ((corners & RoundedRectCorners.Left) == 0)
					margin.Left = 0;

				if ((corners & RoundedRectCorners.Right) == 0)
					margin.Right = 0;

				this._layers[OverlayLayer].Initialize(arrangeRect, margin, 1d, corners);
			}
			else
			{
				Rect outerRect = Rect.Inflate(arrangeRect, 0d, -0.5d);

				GeometryGroup topBottomGeo = new GeometryGroup();
				topBottomGeo.Children.Add(new LineGeometry(new Point(outerRect.Left, outerRect.Top), new Point(outerRect.Right, outerRect.Top)));
				topBottomGeo.Children.Add(new LineGeometry(new Point(outerRect.Left, outerRect.Bottom), new Point(outerRect.Right, outerRect.Bottom)));
				topBottomGeo.Freeze();

				this._layers[OuterLayer].Initialize(topBottomGeo);
				this._layers[OverlayLayer].Initialize(arrangeRect, new Thickness(0, 1, 0, 1), 1d);
			}
			#endregion //Outer Layer

			if (isPressed)
			{
				#region Pressed
				if (isFirstInGroup && isLastInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(2, 0, 2, 0), 1d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(1, 1, 1, 1), 1d, RoundedRectCorners.Bottom, 0d);
				}
				else if (isFirstInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(2, 0, 1, 0), 0d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(1, 1, 1, 1), 1d, RoundedRectCorners.BottomLeft, 0d);
				}
				else if (isLastInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 0, 2, 0), 1d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(0, 1, 1, 1), 1d, RoundedRectCorners.BottomRight, 0d);
				}
				else
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 0, 1, 0), 1d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(0, 1, 1, 1), 1d, RoundedRectCorners.None, 0d);
				} 
				#endregion //Pressed
			}
			else if (isChecked)
			{
				#region Checked
				if (isFirstInGroup && isLastInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(2, 0, 2, 1), 1d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(1, 1, 1, 2), 1d, RoundedRectCorners.Bottom, 0d);
				}
				else if (isFirstInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(2, 0, 1, 1), 0d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(1, 1, 1, 2), 1d, RoundedRectCorners.BottomLeft, 0d);
				}
				else if (isLastInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 0, 2, 1), 1d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(0, 1, 1, 2), 1d, RoundedRectCorners.BottomRight, 0d);
				}
				else
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 0, 1, 1), 1d, RoundedRectCorners.None, 0d);
					this._layers[InnerLayer].Initialize(arrangeRect, new Thickness(0, 1, 1, 2), 1d, RoundedRectCorners.None, 0d);
				}
				#endregion //Checked
			}
			else if (isMouseOver)
			{
				#region MouseOver

				if (isFirstInGroup)
					this._layers[MiddleLayer].Initialize(arrangeRect, OnePixelThickness, 1d, corners);
				else // use the left most pixel
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 1, 1, 1), 1d, corners); 

				#endregion //MouseOver
			}
			else
			{
				#region Normal
				if (isFirstInGroup && isLastInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, OnePixelThickness, 1d, RoundedRectCorners.All);
					this._layers[InnerLayer].Clear();
				}
				else if (isMiddle)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 1, 0, 1), 1d, RoundedRectCorners.None, RoundedRectSide.Bottom, RoundedRectSide.Top);

					// just a right border
					Geometry g = new LineGeometry(new Point(arrangeRect.Right, arrangeRect.Top + 1d), new Point(arrangeRect.Right, arrangeRect.Bottom - 1));
					this._layers[InnerLayer].Initialize(g);
				}
				else if (isFirstInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(1, 1, 0, 1), 1d, corners, RoundedRectSide.Bottom, RoundedRectSide.Top);

					// just a right border
					Geometry g = new LineGeometry(new Point(arrangeRect.Right, arrangeRect.Top + 1d), new Point(arrangeRect.Right, arrangeRect.Bottom - 1));
					this._layers[InnerLayer].Initialize(g);
				}
				else //if (isLastInGroup)
				{
					this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(0, 1, 1, 1), 1d, corners);
					this._layers[InnerLayer].Clear();
				}
				#endregion //Normal
			}
		} 
				#endregion //InitializeButtonGroupLayers

				#region InitializeLayers
		private void InitializeLayers()
		{
			this.InitializeLayers(this.RenderSize);
		}

		private void InitializeLayers(Size arrangeSize)
		{
			Rect arrangeRect = this.GetMenuButtonAreaRect(arrangeSize);

            // AS 2/5/09 TFS9385
            // Store the last arrange rect so we can compare it in the layout updated.
            //
            _lastArrangeRect = arrangeRect;

			// start by resetting the layers
			for (int i = 0; i < this._layers.Length; i++)
				this._layers[i].Clear();

			if (this._tool != null)
			{
				bool isInButtonGroup = ButtonGroup.GetIsInButtonGroup(this._tool);

				if (isInButtonGroup == false)
				{
					#region Non-ButtonGroup Tools

					this._layers[GlowLayer].Initialize(Rect.Inflate(arrangeRect, -0.5, -0.5), ZeroPixelThickness, 1d);

					switch (this._location)
					{
						case ToolLocation.ApplicationMenuFooterToolbar:
							
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

						case ToolLocation.Ribbon:
						case ToolLocation.QuickAccessToolbar:
                        case ToolLocation.Unknown: // AS 2/19/09 TFS6747

							// when not in a button group, we always want the same set up
							this._layers[OuterLayer].Initialize(arrangeRect, ZeroPixelThickness, 2d);
							this._layers[MiddleLayer].Initialize(arrangeRect, new Thickness(1, 1, 1, 0), 1d, RoundedRectCorners.Top);
							this._layers[InnerLayer].Initialize(arrangeRect, OnePixelThickness, 1d);

							bool isLargeRibbonTool = this.IsLargeRibbonTool;

							// Calculate a rect based on the arrange size but then exclude the padded area
							Rect segmentedAreaRect = new Rect(arrangeSize);
							segmentedAreaRect = Subtract(segmentedAreaRect, this.Padding);

							if (this.IsDropDownButtonOfSegmentedMenu)
							{
								if (isLargeRibbonTool)
									this._layers[OverlayLayer].Initialize(segmentedAreaRect, new Thickness(2, 0, 2, 2), 0d);
								else
									this._layers[OverlayLayer].Initialize(segmentedAreaRect, new Thickness(0, 2, 2, 2), 0d);
							}
							else if (this.IsSegmentedButton)
							{
								if (isLargeRibbonTool)
									this._layers[OverlayLayer].Initialize(segmentedAreaRect, new Thickness(1, 1, 1, 0), 1d, RoundedRectCorners.Top);
								else
									this._layers[OverlayLayer].Initialize(segmentedAreaRect, new Thickness(1, 1, 0, 1), 1d, RoundedRectCorners.Top);
							}
							break;
						default:
							break;
					}

					#endregion //Non-ButtonGroup Tools
				}
				else
					this.InitializeButtonGroupLayers(arrangeRect);
			}
		} 
				#endregion //InitializeLayers

                // AS 2/5/09 TFS9385
                #region OnLayoutUpdated
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (this.IsDropDownButtonOfSegmentedMenu)
            {
                // the rect that we calculated in the arrange may not
                // be valid so we'll double check that rect when the layout
                // is updated
                Rect arrangeRect = this.GetMenuButtonAreaRect(this.RenderSize);

                if (arrangeRect != _lastArrangeRect)
                {
                    _lastArrangeRect = arrangeRect;
                    this.InvalidateVisual();
                }
            }
        } 
                #endregion //OnLayoutUpdated

                // AS 2/5/09 TFS9385
                #region OnLoaded
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
        } 
                #endregion //OnLoaded

                // AS 2/5/09 TFS9385
                #region OnUnloaded
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // we only need to listen to layout updated while loaded
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded -= new RoutedEventHandler(OnUnloaded);
            this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
        } 
                #endregion //OnUnloaded

				#region UpdateBrushReference
		private void UpdateBrushReference(DependencyProperty property, object brushReference)
		{
			if (brushReference is ResourceKey)
				this.SetResourceReference(property, brushReference);
			else if (brushReference is Brush)
				this.SetValue(property, brushReference);
			else
				this.ClearValue(property);
		} 
				#endregion //UpdateBrushReference

				#region VerifyBrushes
		private void VerifyBrushes()
		{
            // AS 10/21/08 TFS9385/Optimization
            if (this.IsInitialized == false)
                return;

			
			object outerPenBrush = null;
			object outerFillBrush = null;
			object middlePenBrush = null;
			object middleFillBrush = null;
			object innerPenBrush = null;
			object innerFillBrush = null;
			object glowFillBrush = null;
			object overlayFillBrush = null;
			object separatorPenBrush = null;

			if (null != this._tool)
			{
				if (this._location == ToolLocation.Menu ||
					this._location == ToolLocation.ApplicationMenuRecentItems ||
                    // AS 2/19/09 TFS6747
                    // Treat unknown like Ribbon.
                    //
					//this._location == ToolLocation.ApplicationMenu ||
					//this._location == ToolLocation.Unknown)
					this._location == ToolLocation.ApplicationMenu)
				{
					// unrecognized location
				}
				else
				{
					RenderStates state = this.CurrentState;

					bool isMouseOver = (state & RenderStates.HotTracked) != 0;
					bool isChecked = (state & RenderStates.Checked) != 0;
					bool isPressed = (state & RenderStates.Pressed) != 0;
					bool drawOverlay = (state & RenderStates.HotTrackedByOtherSegment) != 0;
					bool isSegmented = this.IsSegmentedButton;
					bool isSegmentedDropDown = isSegmented == false && this.IsDropDownButtonOfSegmentedMenu;
					// AS 11/9/07 BR27990
					bool isDisabled = (state & RenderStates.Disabled) != 0;

					if (ButtonGroup.GetIsInButtonGroup(this._tool))
					{
						#region ButtonGroup
						// for button groups we need to reinitialize the layers based on state
						// since the margins for the layers can change
						if (this.IsArrangeValid)
						{
							// start by resetting the layers
							for (int i = 0; i < this._layers.Length; i++)
								this._layers[i].Clear();

							this.InitializeLayers();
						}

						// always draw the outer border
						outerPenBrush = RibbonBrushKeys.ButtonGroupNormalBorderFillKey;

						// small tools
						if (isPressed)
						{
							#region Pressed Tool

							middleFillBrush = RibbonBrushKeys.ButtonGroupPressedBorderFillKey;
							innerFillBrush = RibbonBrushKeys.ButtonGroupPressedCenterFillKey;

							if (isSegmented)
								separatorPenBrush = RibbonBrushKeys.ButtonGroupPressedBorderFillKey;

							#endregion //Pressed Tool
						}
						else if (isChecked)
						{
							#region Checked
							if (isMouseOver)
							{
								middleFillBrush = RibbonBrushKeys.ButtonGroupCheckedHottrackBorderFillKey;
								innerFillBrush = RibbonBrushKeys.ButtonGroupCheckedHottrackCenterFillKey;
							}
							else
							{
								middleFillBrush = RibbonBrushKeys.ButtonGroupCheckedBorderFillKey;
								innerFillBrush = RibbonBrushKeys.ButtonGroupCheckedCenterFillKey;
							}

							if (isSegmented)
								separatorPenBrush = middleFillBrush;

							#endregion //Checked
						}
						else if (isMouseOver)
						{
							#region HotTrack/Active

							// AS 4/20/10 Scenic Ribbon
							outerPenBrush = RibbonBrushKeys.ButtonGroupHoverOuterBorderFillKey;

							middleFillBrush = RibbonBrushKeys.ButtonGroupHoverCenterFillKey;
							middlePenBrush = RibbonBrushKeys.ButtonGroupHoverBorderFillKey;

							if (isSegmented)
								separatorPenBrush = RibbonBrushKeys.ButtonGroupSegmentedHoverDarkSeparatorFillKey;
							else if (isSegmentedDropDown)
								separatorPenBrush = RibbonBrushKeys.ButtonGroupSegmentedHoverLightSeparatorFillKey;

							#endregion //HotTrack/Active
						}
						else if (isDisabled)
						{
							#region Disabled

							// AS 11/9/07 BR27990
							middleFillBrush = RibbonBrushKeys.ButtonGroupDisabledCenterFillKey;
							middlePenBrush = RibbonBrushKeys.ButtonGroupDisabledInnerBorderFillKey;

							if (isSegmented)
								separatorPenBrush = RibbonBrushKeys.ButtonGroupDisabledDarkSeparatorFillKey;

							#endregion //Disabled
						}
						else
						{
							#region Normal

							innerPenBrush = RibbonBrushKeys.ButtonGroupNormalDividerFillKey;

							middleFillBrush = RibbonBrushKeys.ButtonGroupNormalCenterFillKey;
							middlePenBrush = RibbonBrushKeys.ButtonGroupNormalInnerBorderFillKey;

							#endregion //Normal
						} 
						#endregion //ButtonGroup
					}
					else if (this.IsLargeRibbonTool)
					{
						#region Large Tools
						// large tools
						if (isPressed)
						{
							#region Pressed Tool

							outerPenBrush = RibbonBrushKeys.ButtonPressedBorderDarkFillKey;
							middlePenBrush = RibbonBrushKeys.ButtonPressedBorderLightFillKey;
							middleFillBrush = RibbonBrushKeys.ButtonPressedCenterFillKey;

							if (isSegmented)
								separatorPenBrush = RibbonBrushKeys.LargeSegmentedCheckedHottrackSeparatorFillKey;

							#endregion //Pressed Tool
						}
						else if (isChecked)
						{
							#region Checked
							if (isMouseOver)
							{
								outerPenBrush = RibbonBrushKeys.ButtonPressedBorderDarkFillKey;
								innerPenBrush = RibbonBrushKeys.ButtonCheckedHottrackInnerBorderFillKey;
								outerFillBrush = RibbonBrushKeys.ButtonCheckedHottrackCenterFillKey;
								glowFillBrush = RibbonBrushKeys.ButtonCheckedHottrackCenterOverlayFillKey;

								if (isSegmented)
									separatorPenBrush = RibbonBrushKeys.LargeSegmentedCheckedHottrackSeparatorFillKey;
							}
							else
							{
								outerPenBrush = RibbonBrushKeys.ButtonPressedBorderDarkFillKey;
								middlePenBrush = RibbonBrushKeys.ButtonCheckedInnerBorderFillKey;
								middleFillBrush = RibbonBrushKeys.ButtonCheckedCenterFillKey;
								glowFillBrush = RibbonBrushKeys.ButtonCheckedCenterOverlayFillKey;

								if (isSegmented)
									separatorPenBrush = RibbonBrushKeys.LargeSegmentedCheckedSeparatorFillKey;
							}
							#endregion //Checked
						}
						else if (isMouseOver)
						{
							#region HotTrack/Active

							// the dropdown portion of a segmented button only renders - and then 
							// only an overlay when it is not hottracked
							outerPenBrush = RibbonBrushKeys.ButtonHoverBorderDarkFillKey;
							outerFillBrush = RibbonBrushKeys.ButtonHoverCenterFillKey;
							innerPenBrush = RibbonBrushKeys.ButtonHoverBorderLightFillKey;

							if (isSegmented)
								separatorPenBrush = RibbonBrushKeys.LargeSegmentedHoverSeparatorFillKey;

							#endregion //HotTrack/Active
						}
						else if (isDisabled)
						{
							#region Disabled

							// AS 11/9/07 BR27990
							outerPenBrush = RibbonBrushKeys.ButtonDisabledBorderDarkFillKey;
							outerFillBrush = RibbonBrushKeys.ButtonDisabledCenterFillKey;
							innerPenBrush = RibbonBrushKeys.ButtonDisabledBorderLightFillKey;

							if (isSegmented)
								separatorPenBrush = RibbonBrushKeys.LargeSegmentedDisabledDarkSeparatorFillKey;

							#endregion //Disabled
						}

						#endregion //Large Tools
					}
					else if (this._location == ToolLocation.ApplicationMenuFooterToolbar)
					{
						#region ApplicationMenuFooterToolbar
						// AS 11/26/07 BR28569
						// For mouseover and normal state, use the amft specific brushes. For all other
						// states use the ribbon/qat small tools brushes.
						//
						// small tools
						if (isPressed)
						{
							#region Pressed Tool

							outerPenBrush = RibbonBrushKeys.ButtonToolPressedBorderDarkFillKey;
							middlePenBrush = RibbonBrushKeys.ButtonToolPressedInnerBorderFillKey;
							middleFillBrush = RibbonBrushKeys.ButtonToolPressedCenterFillKey;

							#endregion //Pressed Tool
						}
						else if (isChecked)
						{
							#region Checked
							if (isMouseOver)
							{
								outerPenBrush = RibbonBrushKeys.ButtonToolCheckedHottrackBorderFillKey;
								middlePenBrush = RibbonBrushKeys.ButtonToolCheckedHottrackInnerBorderFillKey;
								middleFillBrush = RibbonBrushKeys.ButtonToolCheckedHottrackCenterFillKey;
							}
							else
							{
								outerPenBrush = RibbonBrushKeys.ButtonToolCheckedBorderFillKey;
								middlePenBrush = RibbonBrushKeys.ButtonToolCheckedInnerBorderFillKey;
								middleFillBrush = RibbonBrushKeys.ButtonToolCheckedCenterFillKey;
							}
							#endregion //Checked

						}
						else if (isMouseOver)
						{
							// for some reason they use the sam appearnce for pressed and hottracked
							outerPenBrush = RibbonBrushKeys.ApplicationMenuFooterToolbarButtonHoverBorderFillKey;
							outerFillBrush = RibbonBrushKeys.ApplicationMenuFooterToolbarButtonHoverCenterFillKey;
						}
						else if (isDisabled)
						{
							#region Disabled

							// AS 11/9/07 BR27990
							outerPenBrush = RibbonBrushKeys.ButtonToolDisabledBorderDarkFillKey;
							outerFillBrush = RibbonBrushKeys.ButtonToolDisabledCenterFillKey;
							innerPenBrush = RibbonBrushKeys.ButtonToolDisabledBorderLightFillKey;

							#endregion //Disabled
						}
						else
						{
							outerPenBrush = RibbonBrushKeys.ApplicationMenuFooterToolbarButtonNormalBorderFillKey;
							outerFillBrush = RibbonBrushKeys.ApplicationMenuFooterToolbarButtonNormalCenterFillKey;
						} 
						#endregion //ApplicationMenuFooterToolbar

						// AS 11/26/07 BR28569
						if (isSegmented)
							separatorPenBrush = outerPenBrush;
					}
					else
					{
						#region Small Tools
						// small tools
						if (isPressed)
						{
							#region Pressed Tool

							outerPenBrush = RibbonBrushKeys.ButtonToolPressedBorderDarkFillKey;
							middlePenBrush = RibbonBrushKeys.ButtonToolPressedInnerBorderFillKey;
							middleFillBrush = RibbonBrushKeys.ButtonToolPressedCenterFillKey;

							#endregion //Pressed Tool
						}
						else if (isChecked)
						{
							#region Checked
							if (isMouseOver)
							{
								outerPenBrush = RibbonBrushKeys.ButtonToolCheckedHottrackBorderFillKey;
								middlePenBrush = RibbonBrushKeys.ButtonToolCheckedHottrackInnerBorderFillKey;
								middleFillBrush = RibbonBrushKeys.ButtonToolCheckedHottrackCenterFillKey;
							}
							else
							{
								outerPenBrush = RibbonBrushKeys.ButtonToolCheckedBorderFillKey;
								middlePenBrush = RibbonBrushKeys.ButtonToolCheckedInnerBorderFillKey;
								middleFillBrush = RibbonBrushKeys.ButtonToolCheckedCenterFillKey;
							}
							#endregion //Checked

						}
						else if (isMouseOver)
						{
							#region HotTrack/Active

							outerPenBrush = RibbonBrushKeys.ButtonToolHoverBorderDarkFillKey;
							outerFillBrush = RibbonBrushKeys.ButtonToolHoverCenterFillKey;
							innerPenBrush = RibbonBrushKeys.ButtonToolHoverBorderLightFillKey;

							#endregion //HotTrack/Active
						}
						else if (isDisabled)
						{
							#region Disabled

							// AS 11/9/07 BR27990
							outerPenBrush = RibbonBrushKeys.ButtonToolDisabledBorderDarkFillKey;
							outerFillBrush = RibbonBrushKeys.ButtonToolDisabledCenterFillKey;
							innerPenBrush = RibbonBrushKeys.ButtonToolDisabledBorderLightFillKey;

							#endregion //Disabled
						}
						#endregion //Small Tools

						if (isSegmented)
							separatorPenBrush = outerPenBrush;
					}

					// AS 11/26/07 BR28569
					// Moved out of the if blocks above since this will be common to all.
					//
                    if (drawOverlay)
                    {
                        // AS 2/19/09 TFS14014
                        //overlayFillBrush = new SolidColorBrush(Color.FromArgb(165, 255, 255, 255));
                        overlayFillBrush = RibbonBrushKeys.MenuToolOverlayFillKey;
                    }
				}

				#region Update Brushes

				this.UpdateBrushReference(OuterFillBrushProperty, outerFillBrush);
				this.UpdateBrushReference(OuterPenBrushProperty, outerPenBrush);
				this.UpdateBrushReference(MiddlePenBrushProperty, middlePenBrush);
				this.UpdateBrushReference(MiddleFillBrushProperty, middleFillBrush);
				this.UpdateBrushReference(InnerFillBrushProperty, innerFillBrush);
				this.UpdateBrushReference(InnerPenBrushProperty, innerPenBrush);
				this.UpdateBrushReference(GlowFillBrushProperty, glowFillBrush);
				this.UpdateBrushReference(OverlayFillBrushProperty, overlayFillBrush);
				this.UpdateBrushReference(SeparatorPenBrushProperty, separatorPenBrush);

				#endregion //Update Brushes
			}
		} 
				#endregion //VerifyBrushes

				#region Add (Size, Thickness)
		private static Size Add(Size size, Thickness thickness)
		{
			Size newSize = size;

			if (null != thickness)
			{
				newSize.Width += thickness.Left + thickness.Right;
				newSize.Height += thickness.Top + thickness.Bottom;
			}

			return newSize;
		}
				#endregion //Add (Size, Thickness)

				#region Subtract(Rect, Thickness)
		private static Rect Subtract(Rect rect, Thickness thickness)
		{
			if (null != thickness && rect.IsEmpty == false)
			{
				rect.X += thickness.Left;
				rect.Y += thickness.Top;
				rect.Height = Math.Max(rect.Height - (thickness.Top + thickness.Bottom), 0);
				rect.Width = Math.Max(rect.Width - (thickness.Left + thickness.Right), 0);
			}

			return rect;
		}
				#endregion //Subtract(Rect, Thickness)

			#endregion //Private Methods	
    
		#endregion //Methods	

		#region RenderStates
		private enum RenderStates
		{
			Normal = 0x0,
			HotTracked = 0x1,
			Pressed = 0x2,
			Checked = 0x4,
			HotTrackedByOtherSegment = 0x8,
			// AS 11/9/07 BR27990
			Disabled = 0x10,
		}
		#endregion //RenderStates

		#region ChromeLayer
		private class ChromeLayer
		{
			#region Member Variables

			private DependencyProperty _brushProperty;
			private DependencyProperty _strokeProperty;
			private Rect _rect = Rect.Empty;
			private Geometry _geometry;
			private double _radius;
			private bool _allowRoundedCorners = true;

			#endregion //Member Variables

			#region Constructor
			internal ChromeLayer(DependencyProperty brushProperty, DependencyProperty strokeProperty)
			{
				this._brushProperty = brushProperty;
				this._strokeProperty = strokeProperty;
			}
			#endregion //Constructor

			#region Properties
			public bool IsEmpty
			{
				get { return this._rect.IsEmpty && this._geometry == null; }
			}

			public bool AllowRoundedCorners
			{
				get { return _allowRoundedCorners; }
				set { _allowRoundedCorners = value; }
			}
			#endregion //Properties

			#region Methods
			public void Clear()
			{
				this._geometry = null;
				this._rect = Rect.Empty;
			}

			public void Initialize(Rect elementRect, Thickness margins, double radius)
			{
				this.Initialize(elementRect, margins, radius, RoundedRectCorners.All);
			}

			public void Initialize(Rect elementRect, Thickness margins, double radius, RoundedRectCorners corners)
			{
				double thickness = this._strokeProperty != null ? 1d : 0d;
				this.Initialize(elementRect, margins, radius, corners, thickness);
			}
			public void Initialize(Rect elementRect, Thickness margins, double radius, RoundedRectCorners corners, double edgeThickness)
			{
				this.Initialize(elementRect, margins, radius, corners, edgeThickness, RoundedRectSide.Left, RoundedRectSide.Left);
			}

			public void Initialize(Rect elementRect, Thickness margins, double radius, RoundedRectCorners corners, RoundedRectSide firstSide, RoundedRectSide lastSide)
			{
				double thickness = this._strokeProperty != null ? 1d : 0d;
				this.Initialize(elementRect, margins, radius, corners, thickness, firstSide, lastSide);
			}

			public void Initialize(Rect elementRect, Thickness margins, double radius, RoundedRectCorners corners, double edgeThickness, RoundedRectSide firstSide, RoundedRectSide lastSide)
			{
				this._rect = RibbonButtonChrome.Subtract(elementRect, margins);
				this._radius = radius;

				// if this is not uniform
				if (_allowRoundedCorners && ((corners != RoundedRectCorners.All && corners != RoundedRectCorners.None) || firstSide != lastSide))
				{
					this._geometry = Utilities.CreateRoundedRectGeometry(this._rect, corners, radius, radius, edgeThickness, firstSide, lastSide);

                    // JJD 4/29/10 - Optimization
                    // Freeze the geometry so the framework doesn't need to listen for change
                    if (this._geometry != null && this._geometry.CanFreeze)
                        this._geometry.Freeze();
                }
				else
				{
					if (!_allowRoundedCorners || corners == RoundedRectCorners.None)
						this._radius = 0d;

					this._geometry = null;
				}
			}

			public void Initialize(Geometry geometry)
			{
				this._geometry = geometry;

                // JJD 4/29/10 - Optimization
                // Freeze the geometry so the framework doesn't need to listen for changes
                if (this._geometry != null && this._geometry.CanFreeze)
                    this._geometry.Freeze();

				this._rect = Rect.Empty;
				this._radius = 0d;
			}

			public void Draw(DrawingContext drawingContext, RibbonButtonChrome chrome)
			{
				if (this._rect.IsEmpty == false || this._geometry != null)
				{
					Brush fillBrush = this._brushProperty != null ? chrome.GetValue(this._brushProperty) as Brush : null;
					Brush strokeBrush = this._strokeProperty != null ? chrome.GetValue(this._strokeProperty) as Brush : null;

					Pen pen = strokeBrush != null ? new Pen(strokeBrush, 1d) : null;

					if (pen != null || fillBrush != null)
					{
						if (this._geometry == null)
						{
							Rect roundedRect = this._rect;

							if (pen != null)
							{
								double thickness = pen.Thickness / 2d;
								roundedRect = Rect.Inflate(this._rect, -thickness, -thickness);
							}

							if (this._radius > 0)
								drawingContext.DrawRoundedRectangle(fillBrush, pen, roundedRect, this._radius, this._radius);
							else
								drawingContext.DrawRectangle(fillBrush, pen, roundedRect);
						}
						else
						{
							drawingContext.DrawGeometry(fillBrush, pen, this._geometry);
						}
					}
				}
			}
			#endregion //Methods
		} 
		#endregion //ChromeLayer
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