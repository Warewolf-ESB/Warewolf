using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Collections;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Interop;
using Infragistics.Collections;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Class used inside a <see cref="Popup"/> control to position a resizerbar above or below its content.
	/// </summary>
	/// <remarks>
	/// <para class="body">This is placed inside the shadow chrome and main border of a <see cref="Popup"/> control.</para>
	/// <para class="note"><b>Note:</b> the implementation uses a RenderTransform to arrange the resizer bar appropriately. Therefore, any RenderTransform placed on its immediate child element will be ignored.</para>
	/// </remarks>
	/// <seealso cref="PopupResizerBar"/>
	/// <seealso cref="ResizeMode"/>
	/// <seealso cref="ResizerBarLocation"/>
	/// <seealso cref="ResizerBarStyle"/>
	[TemplatePart(Name = "PART_Thumb", Type = typeof(Thumb))]
	[StyleTypedProperty(Property = "ResizerBarStyle", StyleTargetType = typeof(PopupResizerBar))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public sealed class PopupResizerDecorator : Decorator
	{
		#region Private Members

		private PopupResizerBar _resizerBar;
		private Thumb _thumb;
		private Popup _popup;
		private PlacementMode		_popupOriginalPlacement;
		private double				_popupOriginalVerticalOffset;
		private double				_popupOriginalHorizontalOffset;

        // JJD 11/08/07 
        // Cache original CustomPopupPlacementCallback 
        private CustomPopupPlacementCallback
                                    _popupOriginalCustomPopupCallback;
        // JJD 11/08/07 - cache plament point to prevent flickering
        private Point               _customPlacmentPoint; 

        private FrameworkElement    _popupRoot;
		private UIElement			_commonAncestor; // used in XBAP applications only

		private Dictionary<FrameworkElement, Constraints> _constraintDictionary;
		private bool _draggingConstraintsDirty;

		// JJD 10/24/07
		// Initialization is now done in OnDragStarted, this flag is no longer needed
		//private bool _firstResizeDone;

		private double _minDraggingWidth;
		private double _minDraggingHeight;
		private double _maxDraggingWidth;
		private double _maxDraggingHeight;

        // JJD 11/06/07 - BR28049 - Added
        private Rect _resizeWorkArea;

		// SSP 11/9/07 BR27357
		// A flag we use to see if we need to invalidate measure. Invalidating measure
		// when not necessary causes BR27357 for some reason.
		// 
		private bool _popupWasResized = false;

        // JJD 11/27/07 - BR28507
        // cache our custom placement callback 
        private CustomPopupPlacementCallback _popupPlacementCallback;

		// AS 6/14/11 NA 11.2 Excel Style Filtering
		// We shouldn't set the Width/Height of the PopupResizeDecorator because the PopupRoot 
		// may override that if the popup is deemed too large. In which case the PopupRoot will 
		// adjust the size of the popup but since the Width/Height were explicitly set our 
		// element will be that size regardless of how it is measured and so some of the content 
		// will be clipped. Instead we will store the intended width/height and use that from 
		// within the measureoverride.
		//
		private double _preferredWidth = double.PositiveInfinity;
		private double _preferredHeight = double.PositiveInfinity;

		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="PopupResizerDecorator"/>
		/// </summary>
		public PopupResizerDecorator()
		{
		}

        static PopupResizerDecorator()
        {
            // JJD 7/1/08 - added ResizerBarFlowDirection to control positioning
            // of resizerbar glyphs based on Placement.
            // We need to be notified when the FlowDirection has changed
            FrameworkElement.FlowDirectionProperty.OverrideMetadata(typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFlowDirectionChanged)));
            
			// JJD 1/11/12 - TFS98223
			// Added coerce value callback for the width and height properties
			FrameworkElement.HeightProperty.OverrideMetadata(typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceHeight)));
			FrameworkElement.WidthProperty.OverrideMetadata(typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceWidth)));
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
			Rect childRect = new Rect(arrangeSize);

			if (this._resizerBar != null)
			{
				Rect barRect = new Rect( arrangeSize );

				barRect.Height = Math.Min( arrangeSize.Height, this._resizerBar.DesiredSize.Height );
				childRect.Height = Math.Max(childRect.Height - barRect.Height, 0);

				if (this._resizerBar.Location == PopupResizerBarLocation.Top)
					childRect.Y = barRect.Height;
				else
					barRect.Y = childRect.Height;

				this._resizerBar.Arrange(barRect);
			}

			UIElement child = this.Child;

			if (child != null)
				child.Arrange(childRect);
			
			return arrangeSize;
		}

			#endregion //ArrangeOverride	

			#region GetVisualChild

		/// <summary>
		/// Gets a child element at the specied index.
		/// </summary>
		/// <param name="index">The zero-based index of the child element</param>
		/// <returns>The child element.</returns>
		protected override Visual GetVisualChild(int index)
		{
			int baseCount = base.VisualChildrenCount;

			if (index == baseCount && this._resizerBar != null)
				return this._resizerBar;

			return base.GetVisualChild(index);
		}

			#endregion //GetVisualChild

			#region LogicalChildren

		/// <summary>
		/// Gets an enumerator that can iterate the logical child elements of this element.
		/// </summary>
		/// <value>An IEnumerator. This property has no default value.</value>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				if ( this._resizerBar == null )
					return base.LogicalChildren;

				return new MultiSourceEnumerator(new IEnumerator[] { base.LogicalChildren, new SingleItemEnumerator(this._resizerBar) });
			}
		}

			#endregion //LogicalChildren	
    
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			// AS 6/14/11 NA 11.2 Excel Style Filtering
			// If the popup has been resized then we will have a preferred width/height 
			// which reflects the intended resized popup content size. We will try to use 
			// that size for the measurements although the popuproot may decide to provide 
			// less.
			//
			if (constraint.Width > _preferredWidth)
				constraint.Width = _preferredWidth;

			if (constraint.Height > _preferredHeight)
				constraint.Height = _preferredHeight;

			Size resizerBarSize = new Size();

			if (this.ResizeMode == PopupResizeMode.None)
			{
				// cleanup an old resizer bar
				if (this._resizerBar != null)
				{
					this.RemoveLogicalChild(this._resizerBar);
					this.RemoveVisualChild(this._resizerBar);
					this._resizerBar = null;
				}
			}
			else
			{
				// If we haven't created a resizerbar then do it now
				if (this._resizerBar == null)
				{
					this._resizerBar = new PopupResizerBar();
					this.AddLogicalChild(this._resizerBar);
					this.AddVisualChild(this._resizerBar);
					this._resizerBar.SetBinding(PopupResizerBar.ResizeModeProperty, Utilities.CreateBindingObject(ResizeModeProperty, BindingMode.OneWay, this));
					this._resizerBar.SetBinding(PopupResizerBar.LocationProperty, Utilities.CreateBindingObject(ResizerBarLocationProperty, BindingMode.OneWay, this));

                    // JJD 5/25/10
                    // Call SynchronizeResizerBarStyle instead of binding the style property of the resizerbar to
                    // the ResizerBarStyle property. If this property is null (the default) we want to clear the
                    // value instead of setting it to null which prevents implicit styles for PopupResizerBar
                    // to be applied
                    //this._resizerBar.SetBinding(PopupResizerBar.StyleProperty, Utilities.CreateBindingObject(ResizerBarStyleProperty, BindingMode.OneWay, this));
                    this.SynchronizeResizerBarStyle();

                    // JJD 7/1/08 - added ResizerBarFlowDirection to control positioning of glyphs
                    this.VerifyResizerBarFlowDirection();
					this._resizerBar.SetBinding(FrameworkElement.FlowDirectionProperty, Utilities.CreateBindingObject(ResizerBarFlowDirectionProperty, BindingMode.OneWay, this));
                }

				// call its measure
				this._resizerBar.Measure(constraint);

				resizerBarSize = this._resizerBar.DesiredSize;

				// adjust the height constraint if it isn't infinity
				if ( !double.IsPositiveInfinity(constraint.Height ))
					constraint.Height = Math.Max(constraint.Height - this._resizerBar.DesiredSize.Height, 1);
			}

			Size desiredSize = base.MeasureOverride(constraint);

			// JJD 05/07/12 - TFS101860 
			// Only adjust the desiredSize height with the resizerbar height here if the
			// resizer bar is on the bottom
			if ( _resizerBar != null && _resizerBar.Location == PopupResizerBarLocation.Bottom )
				desiredSize.Height += resizerBarSize.Height;

			// AS 6/14/11 NA 11.2 Excel Style Filtering
			// If we've been resized then use that as the desired size.
			//
			if (!double.IsPositiveInfinity(_preferredHeight) && desiredSize.Height < constraint.Height)
				desiredSize.Height = constraint.Height;

			// JJD 05/07/12 - TFS101860 
			// When the resizer bar is on top make th desired size adjustment after
			// the constraint height adjustment above
			if (_resizerBar != null && _resizerBar.Location == PopupResizerBarLocation.Top)
				desiredSize.Height += resizerBarSize.Height;

			if (!double.IsPositiveInfinity(_preferredWidth) && desiredSize.Width < constraint.Width)
				desiredSize.Width = constraint.Width;

			return desiredSize;
		}

			#endregion //MeasureOverride	

			#region OnRender

		/// <summary>
		/// Called when the element should render its contents.
		/// </summary>
		/// <param name="drawingContext">An initialized DrawingContext that should be used for all drawing within this method.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			this.InitializeLocation();
		}

			#endregion //OnRender

			#region VisualChildrenCount

		/// <summary>
		/// Returns the count of the parent child elements
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int count = base.VisualChildrenCount;

				if (this._resizerBar != null)
					count++;

				return count;
			}
		}

			#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region ResizeConstraints

		/// <summary>
		/// Identifies the ResizeConstraints attached dependency property
		/// </summary>
		/// <seealso cref="GetResizeConstraints"/>
		/// <seealso cref="SetResizeConstraints"/>
		public static readonly DependencyProperty ResizeConstraintsProperty = DependencyProperty.RegisterAttached("ResizeConstraints",
			typeof(Constraints), typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnResizeConstraintsChanged)));

		private static void OnResizeConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement fe = target as FrameworkElement;

			if (fe == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_6"));

			Constraints constraints = e.OldValue as Constraints;

			if (constraints != null)
				constraints.Initialize(null);

			 constraints = e.NewValue as Constraints;

			if (constraints != null)
				constraints.Initialize(fe);
			
		}

		/// <summary>
		/// Gets the value of the 'ResizeConstraints' attached property
		/// </summary>
		/// <param name="element">The target element.</param>
		/// <returns>The constraints object or null if not set.</returns>
		/// <seealso cref="ResizeConstraintsProperty"/>
		/// <seealso cref="SetResizeConstraints"/>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants=true)]
		public static Constraints GetResizeConstraints(FrameworkElement element)
		{
			return (Constraints)element.GetValue(PopupResizerDecorator.ResizeConstraintsProperty);
		}

		/// <summary>
		/// Sets the value of the 'ResizeConstraints' attached property
		/// </summary>
		/// <param name="element">The target element.</param>
		/// <param name="value">The new constrant object to apply to this element.</param>
		/// <seealso cref="ResizeConstraintsProperty"/>
		/// <seealso cref="GetResizeConstraints"/>
		public static void SetResizeConstraints(FrameworkElement element, Constraints value)
		{
			element.SetValue(PopupResizerDecorator.ResizeConstraintsProperty, value);
		}

				#endregion //ResizeConstraints

				#region ResizeMode

		/// <summary>
		/// Identifies the <see cref="ResizeMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register("ResizeMode",
			typeof(PopupResizeMode), typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(PopupResizeMode.None, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Gets/sets how the popup will be able to be resized.
		/// </summary>
		/// <seealso cref="ResizeModeProperty"/>
		//[Description("Gets/sets how the popup will be able to be resized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public PopupResizeMode ResizeMode
		{
			get
			{
				return (PopupResizeMode)this.GetValue(PopupResizerDecorator.ResizeModeProperty);
			}
			set
			{
				this.SetValue(PopupResizerDecorator.ResizeModeProperty, value);
			}
		}

				#endregion //ResizeMode

				#region ResizerBarLocation

		private static readonly DependencyPropertyKey ResizerBarLocationPropertyKey =
			DependencyProperty.RegisterReadOnly("ResizerBarLocation",
			typeof(PopupResizerBarLocation), typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(PopupResizerBarLocation.Bottom, FrameworkPropertyMetadataOptions.AffectsArrange));

		/// <summary>
		/// Identifies the <see cref="ResizerBarLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizerBarLocationProperty =
			ResizerBarLocationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns where the resizer bar is positioned (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this is determined based on where the popup is finally positioned relative to its PlacementTarget.</para></remarks>
		/// <seealso cref="ResizerBarLocationProperty"/>
		/// <seealso cref="PopupResizerBarLocation"/>
		/// <seealso cref="ResizeMode"/>
		//[Description("Returns where the resizer bar is positioned (read-only).")]
		//[Category("Behavior")]
		[ReadOnly(true)]
		[Bindable(true)]
		public PopupResizerBarLocation ResizerBarLocation
		{
			get
			{
				return (PopupResizerBarLocation)this.GetValue(PopupResizerDecorator.ResizerBarLocationProperty);
			}
		}

				#endregion //ResizerBarLocation

				#region ResizerBarStyle

		/// <summary>
		/// Identifies the <see cref="ResizerBarStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizerBarStyleProperty = DependencyProperty.Register("ResizerBarStyle",
			typeof(Style), typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnResizerBarStyleChanged)));

        // JJD 5/25/10 - Added
        static void OnResizerBarStyleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            PopupResizerDecorator prd = target as PopupResizerDecorator;

            if ( prd != null )
                prd.SynchronizeResizerBarStyle();
        }

		/// <summary>
		/// Gets/sets the style used for the PopupResizerBar
		/// </summary>
		/// <seealso cref="ResizerBarStyleProperty"/>
		//[Description("Gets/sets the style used for the PopupResizerBar")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Style ResizerBarStyle
		{
			get
			{
				return (Style)this.GetValue(PopupResizerDecorator.ResizerBarStyleProperty);
			}
			set
			{
				this.SetValue(PopupResizerDecorator.ResizerBarStyleProperty, value);
			}
		}

				#endregion //ResizerBarStyle

			#endregion //Public Properties

			#region Private Proiperties

				#region CommonAncestor - only used in Browser

		private UIElement CommonAncestor
		{
			get
			{
				if (this._commonAncestor != null)
					return this._commonAncestor;

				if (this._popup == null || !BrowserInteropHelper.IsBrowserHosted )
					return null;
				
                // JJD 11/6/07 - BR28049
                // Use the child of the Popup to find the commom ancestor
                //this._commonAncestor = this._popup.FindCommonVisualAncestor(this) as UIElement;

                // JJD 12/6/07 - BR28815
                // Use the GetCommonAncestor method that handles nested popups properly
                //this._commonAncestor = this._popup.Child.FindCommonVisualAncestor(this) as UIElement;
                this._commonAncestor = Utilities.GetCommonAncestor( this._popup, null ) as UIElement;

				Debug.Assert(this._commonAncestor != null);

				return this._commonAncestor;
			}
		}

				#endregion //CommonAncestor	

				// AS 6/14/11 NA 11.2 Excel Style Filtering
				#region PreferredWidth
		private double PreferredWidth
		{
			get { return _preferredWidth; }
			set
			{
				_preferredWidth = value;

				// JJD 1/11/12 - TFS98223
				// Coerce the Width since the _preferredWidth has changed
				this.CoerceValue(WidthProperty);

				this.InvalidateMeasure();
			}
		}
				#endregion //PreferredWidth

				// AS 6/14/11 NA 11.2 Excel Style Filtering
				#region PreferredHeight
		private double PreferredHeight
		{
			get { return _preferredHeight; }
			set
			{
				_preferredHeight = value;

				// JJD 1/11/12 - TFS98223
				// Coerce the Height since the _preferredHeight has changed
				this.CoerceValue(HeightProperty);

				this.InvalidateMeasure();
			}
		}
				#endregion //PreferredHeight

                // JJD 7/1/08 - added ResizerBarFlowDirection to control positioning
                // of resizerbar glyphs based on Placement
                #region ResizerBarFlowDirection

        private static readonly DependencyPropertyKey ResizerBarFlowDirectionPropertyKey =
            DependencyProperty.RegisterReadOnly("ResizerBarFlowDirection",
            typeof(FlowDirection), typeof(PopupResizerDecorator), new FrameworkPropertyMetadata(FlowDirection.LeftToRight));

        private static readonly DependencyProperty ResizerBarFlowDirectionProperty =
            ResizerBarFlowDirectionPropertyKey.DependencyProperty;

        private FlowDirection ResizerBarFlowDirection
        {
            get
            {
                return (FlowDirection)this.GetValue(PopupResizerDecorator.ResizerBarFlowDirectionProperty);
            }
        }

                #endregion //ResizerBarFlowDirection
    
			#endregion //Private Proiperties	
    
		#endregion //Properties
	
		#region Methods

			#region Internal Methods

        #region PerformMove
        internal void PerformMove(double deltaX, double deltaY)
        {
            this.ProcessOnDragStarted();
            this.ProcessOnDragDelta(deltaX, deltaY);
            this.ProcessOnDragCompleted();
        } 
        #endregion //PerformMove



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal void RegisterDescendantResizeConstraints(FrameworkElement element, Constraints constraints)
		{
			if ( constraints == null )
				throw new ArgumentNullException("constraints");

			if ( element == null || !element.IsDescendantOf(this))
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_5"));

			// allocate a dictionary if we don't already have one
			if (this._constraintDictionary == null)
				this._constraintDictionary = new Dictionary<FrameworkElement, Constraints>();

			this._constraintDictionary.Add(element, constraints);

		}

		internal void UnregisterDescendantResizeConstraints(FrameworkElement element)
		{
			if (this._constraintDictionary != null && 
				this._constraintDictionary.ContainsKey(element))
				this._constraintDictionary.Remove(element);
		}

			#endregion Internal Methods

			#region Private Methods

				#region ApplyExplicitElementConstraints

		private static void ApplyExplicitElementConstraints(double explicitExtent,
														double explicitMinExtent,
														double explicitMaxExtent,
														ref double minDraggingExtent,
														ref double maxDraggingExtent)
		{
			if (!double.IsNaN(explicitExtent))
			{
				minDraggingExtent = explicitExtent;
				maxDraggingExtent = explicitExtent;
			}

			if (!double.IsNaN(explicitMaxExtent))
				maxDraggingExtent = Math.Min(explicitMaxExtent, maxDraggingExtent);

			if (!double.IsNaN(explicitMinExtent))
			{
				minDraggingExtent = Math.Max(explicitMinExtent, minDraggingExtent);
				maxDraggingExtent = Math.Max(explicitMinExtent, maxDraggingExtent);
			}

		}

				#endregion //ApplyExplicitElementConstraints	
    
				#region CalculateConstrainedDelta

		private static double CalculateConstrainedDelta(double currentValue, double delta, double minimum, double maximum)
		{
			double newValue = currentValue + delta;

			if (!double.IsPositiveInfinity(maximum))
				newValue = Math.Min(newValue, maximum);

			newValue = Math.Max(newValue, minimum);

			return newValue - currentValue;
		}

				#endregion //CalculateConstrainedDelta	

				// JJD 1/11/12 - TFS98223
				// Added coerce value callback for the width and height properties
				#region CoerceHeight/Width

		private static object CoerceHeight(DependencyObject target, object value)
		{
			PopupResizerDecorator instance = target as PopupResizerDecorator;

			// JJD 1/11/12 - TFS98223
			// If the preferred height is set then return Nan so we allow
			// the normal measure logic to work without short circuiting it.
			if (!double.IsPositiveInfinity(instance._preferredHeight))
				return double.NaN;

			return value;
		}


		private static object CoerceWidth(DependencyObject target, object value)
		{
			PopupResizerDecorator instance = target as PopupResizerDecorator;

			// JJD 1/11/12 - TFS98223
			// If the preferred width is set then return Nan so we allow
			// the normal measure logic to work without short circuiting it.
			if (!double.IsPositiveInfinity(instance._preferredWidth))
				return double.NaN;

			return value;
		}

				#endregion //CoerceHeight/Width	
     
                // JJD 11/08/07 
                // Added CustomPopupPlacementCallback to prevent flickering when resizing begins
                #region CustomPopupPlacementCallback

        private CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            //Debug.WriteLine("CustomPopupPlacementCallback popupSize: " + popupSize.ToString() + ", targetSize: " + targetSize.ToString() + ", offset: " + offset.ToString());
			// AS 12/7/07 RightToLeft
			//return new CustomPopupPlacement[] { new CustomPopupPlacement(this._customPlacmentPoint, PopupPrimaryAxis.None) };
			Point pt = this._customPlacmentPoint;

            // JJD 7/1/08 - use ResizerBarFlowDirection instead
            //if (this.FlowDirection == FlowDirection.RightToLeft)
            if (this.ResizerBarFlowDirection == FlowDirection.RightToLeft)
				pt.X -= popupSize.Width;
            
            //Debug.WriteLine("CustomPopupPlacementCallback pt: " + pt.ToString());

			return new CustomPopupPlacement[] { new CustomPopupPlacement(pt, PopupPrimaryAxis.None) };
        }

                #endregion //CustomPopupPlacementCallback	
    
				#region DetermineLocation

		private PopupResizerBarLocation DetermineLocation()
		{
			PopupResizerBarLocation location = PopupResizerBarLocation.Bottom;

			if (this.ResizeMode == PopupResizeMode.None)
				return location;


			if (this._popup == null)
			{
				this._popup = Utilities.GetAncestorFromType(this, typeof(Popup), true) as Popup;

				if (this._popup != null)
				{
					this._popup.Closed					+= new EventHandler(OnPopupClosed);
					this._popupOriginalPlacement		= this._popup.Placement;
					this._popupOriginalVerticalOffset	= this._popup.VerticalOffset;
					this._popupOriginalHorizontalOffset	= this._popup.HorizontalOffset;
					this._popupRoot						= this._popup.Child as FrameworkElement;

                    // JJD 11/08/07 
                    // Cache the origonal CustomPopupPlacementCallback  which will be replaced
                    // on a resize operation to prevent flickering when resizing begins
                    this._popupOriginalCustomPopupCallback = this._popup.CustomPopupPlacementCallback;

                    // JJD 7/1/08 - added ResizerBarFlowDirection to control positioning
                    // of resizerbar glyphs based on Placement.
                    // We need to re-verify when the ResizerBarFlowDirection here
                    this.VerifyResizerBarFlowDirection();
                }
			}

			if (this._popup == null)
			{
				Trace.TraceError("Popup element not found");
				return location;
			}

			switch (this._popupOriginalPlacement)
			{
				case PlacementMode.AbsolutePoint:
				case PlacementMode.Center:
				case PlacementMode.Top:
				case PlacementMode.Bottom:
				case PlacementMode.RelativePoint:

					// Verify the location Asynchronously since the first time this is called the popup root hasn't
					// been positioned on the screen yet
					this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new MethodDelegate(VerifyLocation));
					break;
			}
 
			return location;
		}

				#endregion //DetermineLocation	

				#region GetExplicitElementConstraint

		private static void GetExplicitElementConstraint(FrameworkElement element, DependencyProperty property, double adjustment, ref double constraint)
		{
			if (!double.IsNaN(constraint))
				return;

			double propValue = (double)element.GetValue(property);

			if (!double.IsNaN(propValue) &&
				!double.IsPositiveInfinity(propValue) &&
				propValue != (double)(property.GetMetadata(element.GetType()).DefaultValue))
				constraint = propValue - adjustment;
		}

				#endregion //GetExplicitElementConstraint	
        
				#region InitializeLocation

		private void InitializeLocation()
		{
			if (this._resizerBar == null)
				return;

			Thumb thumb = this._resizerBar.GetThumb();

			if ( thumb == null )
				Trace.TraceError("A Thumb with the name PART_Thumb not found in PopupResizerBar");

			if (thumb != this._thumb)
			{
				if (this._thumb != null)
				{
					this._thumb.DragStarted -= new DragStartedEventHandler(OnDragStarted);
				}

				this._thumb = thumb;

				if (this._thumb != null)
				{
					this._thumb.DragStarted += new DragStartedEventHandler(OnDragStarted);
				}
			}

			if (this._popup == null)
			{
				PopupResizerBarLocation location = this.DetermineLocation();

				if (location == PopupResizerBarLocation.Bottom)
					this.ClearValue(ResizerBarLocationPropertyKey);
				else
					this.SetValue(ResizerBarLocationPropertyKey, location);
			}
		}

				#endregion //InitializeLocation
   
				#region InvalidateMeasure


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void InvalidateMeasure(UIElement descendant, UIElement ancestor)
		{
			while (true)
			{
				UIElement parent = VisualTreeHelper.GetParent(descendant) as UIElement;

				if (parent == null || parent == ancestor)
					break;

				parent.InvalidateMeasure();
				descendant = parent;
			}
		}
				#endregion //InvalidateMeasure

				#region OnConstraintPropertyChanged

		private void OnConstraintPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this._draggingConstraintsDirty = true;
		}

				#endregion //OnConstraintPropertyChanged	

                // JJD 7/1/08 - added 
                #region OnFlowDirectionChanged

        private static void OnFlowDirectionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            PopupResizerDecorator prd = target as PopupResizerDecorator;

            // JJD 7/1/08 - added ResizerBarFlowDirection to control positioning
            // of resizerbar glyphs based on Placement.
            // We need to re-verify when the ResizerBarFlowDirection when FlowDirection has changed
            if (prd != null)
                prd.VerifyResizerBarFlowDirection();
        }

                #endregion //OnFlowDirectionChanged	
    
				#region OnDragCompleted

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            this._thumb.DragDelta -= new DragDeltaEventHandler(OnDragDelta);
            this._thumb.DragCompleted -= new DragCompletedEventHandler(OnDragCompleted);

            e.Handled = true;

            // AS 9/22/08
            this.ProcessOnDragCompleted();
        }

        private void ProcessOnDragCompleted()
        {
			// loop over the element constraints and unwire the ProeprtyChanged handlers
			if (this._constraintDictionary != null && this._constraintDictionary.Count > 0)
			{
				PropertyChangedEventHandler handler = new PropertyChangedEventHandler(OnConstraintPropertyChanged);

				foreach (KeyValuePair<FrameworkElement, Constraints> keyValue in this._constraintDictionary)
				{
					Constraints constraints = keyValue.Value;

					constraints.PropertyChanged -= handler;
				}
			}
		}

				#endregion //OnDragCompleted	
    
				#region OnDragDelta

		private bool _processingDragDelta;

		private void OnDragDelta(object sender, DragDeltaEventArgs e)
		{
			e.Handled = true;

            // AS 9/22/08
            this.ProcessOnDragDelta(e.HorizontalChange, e.VerticalChange);
        }

        // AS 9/22/08
        // Refactored into a separate routine so we can reuse this logic for the
        // automation peer.
        //
        private void ProcessOnDragDelta(double horizontalChange, double verticalChange)
        {
			if (this._popup == null)
				return;

			// check anti-recusrion flag
			if (this._processingDragDelta)
				return;
			
			// set anti-recusrion flag
			this._processingDragDelta = true;

			try
			{
                // JJD 11/26/07 - BR28592
                // Don't call this on every drag operation. It is is called from OnDragStart since
                // calling it inside a dragging operation causes inprecise results
                //this.VerifyDraggingConstraints(false);

				bool resizingWidth = false;
				bool resizingHeight = false;

				switch (this.ResizeMode)
				{
					case PopupResizeMode.Both:
						resizingWidth = true;
						resizingHeight = true;
						break;

					case PopupResizeMode.VerticalOnly:
						resizingHeight = true;
						break;
				}

				Thickness margin = this.Margin;

				// JJD 10/24/07
				// Initialization is now done in OnDragStarted, this logic was moved there is no longer needed
				#region Old code

				//if (this._popup.Placement != PlacementMode.Absolute)
				//{
				//    FrameworkElement parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
				//    FrameworkElement topLevelParent = parent;

				//    while (parent != null)
				//    {
				//        topLevelParent = parent;

				//        parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
				//    }

				//    if (topLevelParent != null)
				//    {
				//        Point clientLeftTop = this.PointToTopLevelElement( topLevelParent, new Point());

				//        this._popup.VerticalOffset = clientLeftTop.Y;
				//        this._popup.HorizontalOffset = clientLeftTop.X;
				//        this._popup.Placement = PlacementMode.Absolute;
				//    }
				//    else
				//        return;

				//}

				#endregion //Old code	
    
                //// get the current top left point of this element in screen coordinates
                FrameworkElement refElement = this._popupRoot ?? this;

                // JJD 11/6/07 - BR28049
				// Initialization is now done in OnDragStarted, this logic was moved there is no longer needed
                //Thickness refElementMargin = refElement.Margin;

				Point ptLeftTopOnScreen = this.PointToTopLevelElement( refElement, new Point());

                // JJD 11/6/07 - BR28049
				// Initialization is now done in OnDragStarted, this logic was moved there is no longer needed
				//Rect workArea = NativeWindowMethods.WorkArea;
                //workArea.Height -= refElementMargin.Bottom;
                //workArea.Width -= refElementMargin.Right;

				if (resizingHeight &&
					// JJD 10/24/07
					// Initialization is now done in OnDragStarted, _firstResizeDone flag is no longer used
					//(e.VerticalChange != 0 || this._firstResizeDone == false))
					verticalChange != 0 )
				{
					double actualHeight = this.ActualHeight;
					double currentValue = actualHeight - (margin.Top + margin.Bottom);
					double delta = verticalChange;

					if (this._resizerBar.Location == PopupResizerBarLocation.Top)
					{
						delta = -delta;

						// constrain the size to the screen work area
						if (delta > 0)
							delta = Math.Max(Math.Min(ptLeftTopOnScreen.Y - ( 2 + _resizeWorkArea.Y ), delta), 0);
					}
					else
					{
						// constrain the size to the screen work area
						if (delta > 0)
							delta = Math.Max(Math.Min((_resizeWorkArea.Bottom) - (ptLeftTopOnScreen.Y + refElement.ActualHeight + 2), delta), 0);
					}

					if (delta != 0)
					{
						delta = CalculateConstrainedDelta(currentValue, delta, this._minDraggingHeight, this._maxDraggingHeight);

						if (delta != 0)
							this._draggingConstraintsDirty = true;
					}

					// JJD 10/24/07
					// Check to make sure we have a non-0 delta
					if (delta != 0)
					{
						if (this._resizerBar.Location == PopupResizerBarLocation.Top)
						{
							this._popup.VerticalOffset -= delta;
							// AS 6/14/11 NA 11.2 Excel Style Filtering
							//this.Height = currentValue + delta;
							this.PreferredHeight = currentValue + delta;
						}
						else
						{
							// AS 6/14/11 NA 11.2 Excel Style Filtering
							//this.Height = currentValue + delta;
							this.PreferredHeight = currentValue + delta;
						}

						// JJD 10/24/07
						// Make sure all the ancestors are invalidated
						PopupResizerDecorator.InvalidateMeasure(this, null);
					}
				}

				if (resizingWidth &&
					// JJD 10/24/07
					// Initialization is now done in OnDragStarted, _firstResizeDone flag is no longer used
					//(e.HorizontalChange != 0 || this._firstResizeDone == false))
					horizontalChange != 0 )
				{
					double actualWidth = this.ActualWidth;
					double currentValue = actualWidth - (margin.Left + margin.Right);
					double delta = CalculateConstrainedDelta(currentValue, horizontalChange, this._minDraggingWidth, this._maxDraggingWidth);
					
					// constrain the size to the screen work area
					if (delta > 0)
					{
                        // JJD 7/2/08 - if the ResizerBarFlowDirection is RightToLeft 
                        // then we are resizing to the left so we need to stop when
                        // we get to the left edge of the screen
                        if (this.ResizerBarFlowDirection == FlowDirection.RightToLeft)
                        {
                            if ( ptLeftTopOnScreen.X - delta <= this._resizeWorkArea.X )
                                delta = Math.Max(ptLeftTopOnScreen.X - this._resizeWorkArea.X, 0);
                        }
                        else
                        {
						    // AS 12/7/07 RightToLeft
						    //delta = Math.Max(0, Math.Min((_resizeWorkArea.Right) - (ptLeftTopOnScreen.X + refElement.ActualWidth + 2), delta));
						    Point ptRightBottonOnScreen = Utilities.PointToScreenSafe(refElement, new Point(refElement.ActualWidth + delta, refElement.ActualHeight));
						    Rect refElementRect = Utilities.RectFromPoints(ptLeftTopOnScreen, ptRightBottonOnScreen);
						    Rect intersection = Rect.Intersect(this._resizeWorkArea, refElementRect);
                            if (intersection.Width < refElementRect.Width)
                                delta = Math.Max(intersection.Width - refElement.ActualWidth, 0);
                        }
					}

					if (delta != 0)
					{
						// AS 6/14/11 NA 11.2 Excel Style Filtering
						//this.Width = currentValue + delta;
						this.PreferredWidth = currentValue + delta;

						// JJD 10/24/07
						// Make sure all the ancestors are invalidated
						PopupResizerDecorator.InvalidateMeasure(this, null);
					}
				}

				// JJD 10/24/07
				// Initialization is now done in OnDragStarted, _firstResizeDone flag is no longer used
				//this._firstResizeDone = true;

			}
			finally
			{

				// reset anti-recusrion flag
				this._processingDragDelta = false;
			}
		}

				#endregion //OnDragDelta
	
				#region OnDragStarted

		private void OnDragStarted(object sender, DragStartedEventArgs e)
		{
			this._thumb.DragDelta += new DragDeltaEventHandler(OnDragDelta);
			this._thumb.DragCompleted += new DragCompletedEventHandler(OnDragCompleted);

			e.Handled = true;

            // AS 9/22/08
            this.ProcessOnDragStarted();
        }

        private void ProcessOnDragStarted()
        {
			// set the dirty flag
			this._draggingConstraintsDirty = true;

			// calculate the dragging constraints
			this.VerifyDraggingConstraints(true);

            // JJD 10/24/07
            // Moved this logic here from OnDragDelta
            FrameworkElement parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
			FrameworkElement topLevelParent = parent;

			while (parent != null)
			{
				topLevelParent = parent;

				parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
			}

            Debug.Assert(topLevelParent != null);

            if (topLevelParent != null)
            {
                Point clientLeftTop = this.PointToTopLevelElement(topLevelParent, new Point());

                // JJD 11/6/07 - BR28049
                // Cache the screen work area for use in the drag delta
                FrameworkElement refElement = this._popupRoot ?? this;
                Thickness refElementMargin = refElement.Margin;
                
                // JJD 12/6/07 - BR28815
                // In a browser application we need to constrain the resize area to
                // the browser page we are hosted in
                // this._resizeWorkArea = NativeWindowMethods.GetWorkArea(clientLeftTop);
                UIElement commonAncestor = null;

                if (BrowserInteropHelper.IsBrowserHosted)
                {
                    commonAncestor = this.CommonAncestor;

                    // JJD 12/6/07 - BR28815
                    // The common ancestor should return a navigation window. Its content
                    // will be the Page
                    if (commonAncestor != null)
                    {
                        UIElement elementThatBoundsResizeArea = commonAncestor;

                        if (elementThatBoundsResizeArea is ContentControl && ((ContentControl)elementThatBoundsResizeArea).Content is UIElement)
                            elementThatBoundsResizeArea = ((ContentControl)elementThatBoundsResizeArea).Content as UIElement;
 
                        this._resizeWorkArea = new Rect(Utilities.PointToScreenSafe(commonAncestor, new Point()), elementThatBoundsResizeArea.RenderSize);
                    }
                }

                // JJD 12/6/07 - BR28815
                // Only get the screen work area if we aren't in a browser
				if (commonAncestor == null)
				{
					this._resizeWorkArea = NativeWindowMethods.GetWorkArea(clientLeftTop);

					// AS 6/28/11 TFS78202
					// GetWorkArea returns screen coordinates so we need to convert to logical units 
					// since all these coordinates are relative and not using our PointToScreenSafe 
					// routines.
					//
					_resizeWorkArea = new Rect(Utilities.ConvertToLogicalPixels((int)_resizeWorkArea.X),
						Utilities.ConvertToLogicalPixels((int)_resizeWorkArea.Y),
						Utilities.ConvertToLogicalPixels((int)_resizeWorkArea.Width),
						Utilities.ConvertToLogicalPixels((int)_resizeWorkArea.Height));
				}

                this._resizeWorkArea.Height -= refElementMargin.Bottom;
                this._resizeWorkArea.Width -= refElementMargin.Right;

				// AS 12/7/07 RightToLeft
                // JJD 7/1/08 - use ResizerBarFlowDirection instead
                //if (refElement.FlowDirection == FlowDirection.RightToLeft)
                if (this.ResizerBarFlowDirection == FlowDirection.RightToLeft)
                    this._resizeWorkArea.X += refElementMargin.Right;

                // JJD 10/24/07
                // Moved this logic here from OnDragDelta
                // JJD 11/27/07 - BR28507
                // We should only do this once so check if the CustomPopupPlacementCallback
                // is ours before performing the if block
                //if (this._popup.Placement != PlacementMode.Absolute )
                if (this._popup.Placement != PlacementMode.Absolute &&
                    !(this._popup.Placement == PlacementMode.Custom && this._popup.CustomPopupPlacementCallback == this._popupPlacementCallback))
                {
                    // JJD 10/24/07
                    // Explicitly set the width and height properties before changing the Placment mode
                    // on the popup
                    Thickness margin = this.Margin;
					// AS 6/14/11 NA 11.2 Excel Style Filtering
					//this.Width = this.ActualWidth - (margin.Left + margin.Right);
                    //this.Height = this.ActualHeight - (margin.Top + margin.Bottom);
                    this.PreferredWidth = this.ActualWidth - (margin.Left + margin.Right);
                    this.PreferredHeight = this.ActualHeight - (margin.Top + margin.Bottom);

                    // JJD 11/27/07 - BR27268
                    // Iniitalize the _popupWasResized flag to true so we can cleanup properly
                    this._popupWasResized = true;

                    UIElement placementTarget = this._popup.PlacementTarget;

					// AS 6/14/11 NA 11.2 Excel Style Filtering
					// Actually the framework seems to prefer the visual parent of the popup. I found 
					// this while testing within a popupresizedecorator in the template of a menu using 
					// the intrinsic margin/padding/borderthickness in windows 7.
					//
					if (placementTarget == null)
						placementTarget = VisualTreeHelper.GetParent(this._popup) as UIElement;

                    // JJD 12/6/07 - BR28815
                    // If a placementTarget wasn't explicitly set then use the TemplatedParent
                    if (placementTarget == null)
                        placementTarget = this._popup.TemplatedParent as UIElement;

                    if (placementTarget != null && refElement != this)
                    {
                        // JJD 11/08/07 
                        // Added CustomPopupPlacementCallback to prevent flickering when resizing begins
                        // calculate the custome placement point relative to the placement target
                        this._customPlacmentPoint = refElement.TranslatePoint(new Point(), placementTarget);

                        // JJD 12/6/07 - BR28815
                        // Adjust the point to account for the specified offsets
                        this._customPlacmentPoint.X = this._customPlacmentPoint.X - this._popup.HorizontalOffset;
                        this._customPlacmentPoint.Y = this._customPlacmentPoint.Y - this._popup.VerticalOffset;

						// AS 12/7/07 RightToLeft
 //                       if (refElement.FlowDirection == FlowDirection.RightToLeft)
//                            this._customPlacmentPoint.X -= refElementMargin.Right;
                        if (this.ResizerBarFlowDirection == FlowDirection.RightToLeft)
                        {
                            // JJD 7/2/08 - if the ResizerBarFlowDirection is RightToLeft but
                            // the real FlowDirection is LeftToRight then we need to offset the point 
                            // by the width of the top level parent.
                            // Otherwise, on the initial resize the popup will shift left that
                            // much because of the logic in the CustomPopupPlacementCallback method.
                            if (refElement.FlowDirection == FlowDirection.LeftToRight)
                                this._customPlacmentPoint.X += topLevelParent.ActualWidth + refElementMargin.Right;
                            else
                            {
                                // JJD 7/2/08 - if the real FlowDirection is RightToLeft 
                                // then we have to set the X of the point to the negative of its value.
                                // Not sure why this works but it appears to offset some unusual
                                // processing in the framework
                                this._customPlacmentPoint.X = -(this._customPlacmentPoint.X + refElementMargin.Right);
                            }
                        }

                        // JJD 12/6/07 - BR28815
                        // In a browser application we need to further adjust the point
                        if (BrowserInteropHelper.IsBrowserHosted)
                        {
                            this._customPlacmentPoint.X = Math.Ceiling(this._customPlacmentPoint.X);
                            this._customPlacmentPoint.Y = Math.Ceiling(this._customPlacmentPoint.Y);
                        }

                        // JJD 11/27/07 - BR28507
                        // cache our custom placement callback 
                        //this._popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(this.CustomPopupPlacementCallback);
                        if (this._popupPlacementCallback == null)
                            this._popupPlacementCallback = new CustomPopupPlacementCallback(this.CustomPopupPlacementCallback);
                        
                        this._popup.CustomPopupPlacementCallback = this._popupPlacementCallback;

                        this._popup.Placement = PlacementMode.Custom;
                    }
                    else
                    {
                        this._popup.VerticalOffset = clientLeftTop.Y;
                        this._popup.HorizontalOffset = clientLeftTop.X;
                        this._popup.Placement = PlacementMode.Absolute;
                    }
                }
            }

			// AS NA 11.2 Excel Style Filtering
			// Found this while implementing the feature. The menu item whose popup 
			// is being resized may have open child items but they are not closing 
			// themself so we'll do it when the resize starts
			//
			MenuItem mi = _popup == null ? null : _popup.TemplatedParent as MenuItem;

			if (mi != null)
			{
				var generator = mi.ItemContainerGenerator;

				for (int i = 0; i < mi.Items.Count; i++)
				{
					MenuItem child = generator.ContainerFromIndex(i) as MenuItem;

					if (null != child && child.IsSubmenuOpen)
					{
						child.IsSubmenuOpen = false;
						break;
					}
				}
			}
		}

				#endregion //OnDragStarted	

				#region OnPopupClosed
    
    	private void OnPopupClosed(object sender, EventArgs e)
		{
			// AS 6/14/11 NA 11.2 Excel Style Filtering
			//this.ClearValue(WidthProperty);
			//this.ClearValue(HeightProperty);
			this.PreferredHeight = this.PreferredWidth = double.PositiveInfinity;

			// JJD 10/24/07
			// Clear all the entries in the constraint dictionary so they will
			// get recreated on the next drop down
			if (this._constraintDictionary != null &&
				this._constraintDictionary.Count > 0)
			{
				int count = this._constraintDictionary.Count;

				List<FrameworkElement> elements = new List<FrameworkElement>(count);
				
				foreach (KeyValuePair<FrameworkElement, Constraints> keyValue in this._constraintDictionary)
					elements.Add( keyValue.Key );

				// Calling clear will trigger an unregister and remove the
				// entry from the constraint dictionary
				for (int i = 0; i < count; i++)
					elements[i].ClearValue(PopupResizerDecorator.ResizeConstraintsProperty);


				Debug.Assert(this._constraintDictionary.Count == 0);
			}

			if (this._popup != null)
			{
				this._popup.ClearValue(Popup.PlacementProperty);
				this._popup.ClearValue(Popup.VerticalOffsetProperty);
				this._popup.ClearValue(Popup.HorizontalOffsetProperty);
                // JJD 11/08/07 
                // Restore the origonal CustomPopupPlacementCallback 
                if (this._popupOriginalCustomPopupCallback != null)
                     this._popup.CustomPopupPlacementCallback = this._popupOriginalCustomPopupCallback;
                else
                    this._popup.ClearValue(Popup.CustomPopupPlacementCallbackProperty);

				
				
				
				this._popup.Closed -= new EventHandler( OnPopupClosed );

                this._popup = null;
			}

			this._popupRoot					= null;
			
			// JJD 10/24/07
			// Initialization is now done in OnDragStarted, _firstResizeDone flag is no longer used
			//this._firstResizeDone = false;

			this.InvalidateVisual();

			// SSP 11/9/07 BR27357
			// Only invalidate measure if the popup was actually resized. Otherwise it causes
			// BR27357. Enclosed the existing code into the if block.
			// 
			if ( _popupWasResized )
			{
				_popupWasResized = false;

				// we need to invalidate the measure of this element along with all of its ancestors
				// otherwise, if the popup was resized, the size will be reused on the next dropdown
				InvalidateMeasure( this, null );
			}
		}

   				#endregion //OnPopupClosed	
 
				#region PointFromTopLevelElement

		private Point PointFromTopLevelElement(UIElement referenceElement, Point point)
		{
			// when we aren't in a browser just return the point in screen coordinates
			if (!BrowserInteropHelper.IsBrowserHosted)
				return referenceElement.PointFromScreen(point);

			UIElement commonAncestor = this.CommonAncestor;

			if (commonAncestor == null)
				return point;

			return commonAncestor.TranslatePoint(point, referenceElement);
		}

				#endregion //PointFromTopLevelElement	
		    
				#region PointToTopLevelElement

		private Point PointToTopLevelElement(UIElement referenceElement, Point point)
		{
			// when we aren't in a browser just return the point in screen coordinates
			if (!BrowserInteropHelper.IsBrowserHosted)
				return referenceElement.PointToScreen(point);

			UIElement commonAncestor = this.CommonAncestor;

			if (commonAncestor == null)
				return point;

			return referenceElement.TranslatePoint(point, commonAncestor);
		}

				#endregion //PointToTopLevelElement	

                // JJD 5/25/10 - Added
                #region SynchronizeResizerBarStyle

        private void SynchronizeResizerBarStyle()
        {
            // JJD 5/25/10
            // This method in in lieu of binding the style property of the resizerbar to
            // the ResizerBarStyle property. If this property is null (the default) we want to clear the
            // value instead of setting it to null which prevents implicit styles for PopupResizerBar
            // to be applied
            if (this._resizerBar == null)
                return;

            Style style = this.ResizerBarStyle;

            if (style != null)
                this._resizerBar.SetValue(StyleProperty, style);
            else
                this._resizerBar.ClearValue(StyleProperty);
        }

                #endregion //SynchronizeResizerBarStyle	
            
				#region VerifyDraggingConstraints

		private void VerifyDraggingConstraints( bool dragStarting )
		{
			if (!this._draggingConstraintsDirty || this._popup == null)
				return;

			UIElement child = this.Child;

			if (child == null)
				return;

			#region Aggregate the registered descendant element constraint deltas
			
			List<FrameworkElement> elementsToRemove = null;

			double maxHorizontalDeltaNegative = double.NaN;
			double maxHorizontalDeltaPositive = double.NaN;
			double maxVerticalDeltaNegative = double.NaN;
			double maxVerticalDeltaPositive = double.NaN;

			// process the existing list
			if (this._constraintDictionary != null && this._constraintDictionary.Count > 0)
			{
				PropertyChangedEventHandler handler = new PropertyChangedEventHandler(OnConstraintPropertyChanged);

				foreach (KeyValuePair<FrameworkElement, Constraints> keyValue in this._constraintDictionary)
				{
					Constraints constraints = keyValue.Value;

					FrameworkElement element = constraints.Element;

					bool remove = element == null;

					// if the element is no longer a descendant then add it to the remove list if the drag is
					// just starting. We don't want to incur the overhead of checking the element's
					// ancestry on each drag delta 
					if (dragStarting &&
						 !remove &&
						 !element.IsDescendantOf(this))
						remove = true;

					if (remove)
					{
						if (elementsToRemove == null)
							elementsToRemove = new List<FrameworkElement>();

						elementsToRemove.Add(keyValue.Key);
						continue;
					}

                    Debug.WriteLine("constraints.Element: " + constraints.Element.ToString());
                    Debug.WriteLine("constraints.Element.DesiredSize: " + constraints.Element.DesiredSize.ToString());
                    Debug.WriteLine("constraints.Element.RenderSize: " + constraints.Element.RenderSize.ToString());
                    Debug.WriteLine("constraints.MinimumWidth: " + constraints.MinimumWidth.ToString());
                    Debug.WriteLine("maxHorizontalDeltaNegative before: " + maxHorizontalDeltaNegative.ToString());

					// otherwise aggregate the constraints
					constraints.Aggregate(ref maxHorizontalDeltaNegative,
											ref maxHorizontalDeltaPositive,
											ref maxVerticalDeltaNegative,
											ref maxVerticalDeltaPositive);
                    
                    Debug.WriteLine("maxHorizontalDeltaNegative after: " + maxHorizontalDeltaNegative.ToString());

					// on drag starting wire up the PropertyChanged handler in case the constraints change during resizing
					if (dragStarting)
						constraints.PropertyChanged += handler;
				}

			}

			// process the removed guys
			if (elementsToRemove != null)
			{
				foreach (FrameworkElement element in elementsToRemove)
				{
					// Remove it from our dictionary
					this._constraintDictionary.Remove(element);
				}
			}

			#endregion //Aggregate the registered descendant element constraints	

    
			#region Calculate the min and max dragging extents in both dimensions

			Thickness margin = this.Margin;

			double width = this.ActualWidth - (margin.Left + margin.Right);
			double height = this.ActualHeight - (margin.Top + margin.Bottom);

			Size childDesiredSize = child.DesiredSize;
			Size childRenderSize = child.RenderSize;

			double extraHeightAvailable = Math.Max(childRenderSize.Height - childDesiredSize.Height, 0);
			double extraWidthAvailable = Math.Max(childRenderSize.Width - childDesiredSize.Width, 0);

            Debug.WriteLine("width: " + width.ToString());
            Debug.WriteLine("height: " + height.ToString());
            Debug.WriteLine("childDesiredSize: " + childDesiredSize.ToString());
            Debug.WriteLine("childRenderSize: " + childRenderSize.ToString());
            Debug.WriteLine("extraWidthAvailable: " + extraWidthAvailable.ToString());
            Debug.WriteLine("extraHeightAvailable: " + extraHeightAvailable.ToString());
            Debug.WriteLine("maxHorizontalDeltaNegative: " + maxHorizontalDeltaNegative.ToString());
            Debug.WriteLine("maxVerticalDeltaNegative: " + maxVerticalDeltaNegative.ToString());

            if (!double.IsNaN(maxHorizontalDeltaNegative))
            {
                // JJD 11/27/07 - BR27268
                // The min dragging width should be based on the greater of the maxHorizontalDeltaNegative and
                // the extraWidthAvailable, not their sum
                //this._minDraggingWidth = Math.Max(2, width - (maxHorizontalDeltaNegative + extraWidthAvailable));
                this._minDraggingWidth = Math.Max(width - (Math.Max(maxHorizontalDeltaNegative, extraWidthAvailable)), 2);
            }
            else
            {
                // JJD 11/27/07 - BR27268
                // In this case just set it to a mimimum value
                //this._minDraggingWidth = Math.Max(2, extraWidthAvailable);
                this._minDraggingWidth = 2;
            }

            if (!double.IsNaN(maxVerticalDeltaNegative))
            {
                // JJD 11/27/07 - BR27268
                // The min dragging height should be based on the greater of the maxVerticalDeltaNegative and
                // the extraHeightAvailable, not their sum
                //this._minDraggingHeight = Math.Max(2, height - (maxVerticalDeltaNegative + extraHeightAvailable));
                this._minDraggingHeight = Math.Max(height - (Math.Max(maxVerticalDeltaNegative, extraHeightAvailable)), 2);
            }
            else
            {
                // JJD 11/27/07 - BR27268
                // In this case just set it to a mimimum value
                //this._minDraggingHeight = Math.Max(2, extraHeightAvailable);
                this._minDraggingHeight = 2;
            }

			if (!double.IsNaN(maxHorizontalDeltaPositive))
				this._maxDraggingWidth = width + maxHorizontalDeltaPositive;
			else
				this._maxDraggingWidth = double.PositiveInfinity;

			if (!double.IsNaN(maxVerticalDeltaPositive))
				this._maxDraggingHeight = height + maxVerticalDeltaPositive;
			else
				this._maxDraggingHeight = double.PositiveInfinity;

			#endregion //Calculate the min and max dragging extents in both dimensions	
    

			#region Apply explicitly set constraints from popup

			double explicitWidth		= this._popup.Width;
			double explicitHeight		= this._popup.Height;
			double explicitMinWidth		= this._popup.MinWidth;
			double explicitMinHeight	= this._popup.MinHeight;
			double explicitMaxWidth		= this._popup.MaxWidth;
			double explicitMaxHeight	= this._popup.MaxHeight;

			// JJD 10/24//07 - BR27370
			// Account for any differences in the size of this element relative
			// to its popuproot as well as the resizer bar
			if (this._resizerBar != null)
			{
				#region Adjust explicit settings

				Debug.Assert(this._popupRoot != null);

				Size extraPadding = new Size();

				if (this._popupRoot != null)
				{
					Size rootSize = this._popupRoot.RenderSize;
					Size ourSize = this.RenderSize;

					// get the difference in size
					extraPadding.Width = Math.Max(rootSize.Width - ourSize.Width, 0);
					extraPadding.Height = Math.Max(rootSize.Height - ourSize.Height, 0);

					// addin the margin around the root element
					Thickness rootMargin = this._popupRoot.Margin;
					extraPadding.Width += rootMargin.Left + rootMargin.Right;
					extraPadding.Height += rootMargin.Top + rootMargin.Bottom;

					// adjust the explicit min settings accordingly
					explicitMinHeight = Math.Max(explicitMinHeight -= extraPadding.Height, 0);
					explicitMinWidth = Math.Max(explicitMinWidth -= extraPadding.Width, 0);
				}

				// decrement any explicit maxheight by the extra padding
				if (!double.IsNaN(explicitMaxWidth) &&
					!double.IsPositiveInfinity(explicitMaxWidth))
				{
					explicitMaxWidth -= extraPadding.Width;

                    // JJD 11/27/07 - BR27279
                    // Make sure we use an explicit max width no less than the current ActualWidth
                    // to prevent any posibility of jumpiness during the first resize delta 
                    //explicitMaxWidth = Math.Max(0, explicitMaxWidth);
                    explicitMaxWidth = Math.Max(this.ActualWidth, explicitMaxWidth);
				}

				// decrement any explicit maxheight by the height of the resizer bar plus the extra padding
				if (!double.IsNaN(explicitMaxHeight) &&
					!double.IsPositiveInfinity(explicitMaxHeight))
				{
					explicitMaxHeight -= this._resizerBar.ActualHeight + extraPadding.Height;
                    
                    // JJD 11/27/07 - BR27279
                    // Make sure we use an explicit max height no less than the current ActualHeight
                    // to prevent any posibility of jumpiness during the first resize delta 
                    //explicitMaxHeight = Math.Max(0, explicitMaxHeight);
					explicitMaxHeight = Math.Max(this.ActualHeight, explicitMaxHeight);
				}

				// Make sure we don't get too small vertically
				explicitMinHeight = Math.Max(explicitMinHeight, this._resizerBar.ActualHeight * 2);

				#endregion //Adjust explicit settings	
			}

            Debug.WriteLine("minDraggingWidth before: " + this._minDraggingWidth.ToString());
            Debug.WriteLine("maxDraggingWidth before: " + this._maxDraggingWidth.ToString());
            Debug.WriteLine("minDraggingHeight before: " + this._minDraggingHeight.ToString());
            Debug.WriteLine("maxDraggingHeight before: " + this._maxDraggingHeight.ToString());

			ApplyExplicitElementConstraints(explicitWidth,
											explicitMinWidth,
											explicitMaxWidth,
											ref this._minDraggingWidth,
											ref this._maxDraggingWidth);

			ApplyExplicitElementConstraints(explicitHeight,
											explicitMinHeight,
											explicitMaxHeight,
											ref this._minDraggingHeight,
											ref this._maxDraggingHeight);

            Debug.WriteLine("minDraggingWidth after: " + this._minDraggingWidth.ToString());
            Debug.WriteLine("maxDraggingWidth after: " + this._maxDraggingWidth.ToString());
            Debug.WriteLine("minDraggingHeight after: " + this._minDraggingHeight.ToString());
            Debug.WriteLine("maxDraggingHeight after: " + this._maxDraggingHeight.ToString());

			#endregion //Apply explicitly set constraints from popup	
    

			this._draggingConstraintsDirty = false;
		}

				#endregion //VerifyDraggingConstraints	

				#region VerifyLocation

		private delegate void MethodDelegate();

		private void VerifyLocation()
		{
			DependencyObject root = null;
			DependencyObject ancestor = this;

			//walk up the visual tree to get the PopupRoot
			while (ancestor != null)
			{
				root = ancestor;
				ancestor = VisualTreeHelper.GetParent(root);
			}

			Debug.Assert(root != null);

			FrameworkElement referenceElement = null;

			// get the child of the root which will be a decorator that is used for clipping
			// its child during animations
			if (root != null)
			{
				Debug.Assert(root.GetType().Name == "PopupRoot");

				referenceElement = VisualTreeHelper.GetChild(root, 0) as FrameworkElement;

				Debug.Assert(referenceElement is Decorator);
			}

			referenceElement = referenceElement ?? this;

			Point offset = new Point();

			UIElement target = this._popup.PlacementTarget;

			if (target == null)
			{
				target = this._popup.TemplatedParent as UIElement;
				if (target == null)
					return;
			}

			switch (this._popup.Placement)
			{
				case PlacementMode.AbsolutePoint:
					// get the offset in screen coordinates
					offset = new Point(this._popup.HorizontalOffset, this._popup.VerticalOffset);

					// translate the offset from screen to target 
					offset = this.PointFromTopLevelElement( target, offset);
					break;

				case PlacementMode.Center:
					offset = new Point(target.RenderSize.Width / 2, target.RenderSize.Height / 2);
					break;
				case PlacementMode.Top:
					break;
				case PlacementMode.Bottom:
					offset = new Point(0, target.RenderSize.Height );
					break;
				case PlacementMode.RelativePoint:
					offset = new Point(this._popup.HorizontalOffset, this._popup.VerticalOffset);
					break;
			}

			Point referencePoint = referenceElement.TranslatePoint(new Point(0, Math.Max(referenceElement.RenderSize.Height - 10, 10)), target);

			Debug.WriteLine("VerifyLocation");
			Debug.WriteLine("Render size: " + referenceElement.RenderSize.ToString());
			Debug.WriteLine("Offset: " + offset.ToString());
			Debug.WriteLine("Reference Point: " + referencePoint.ToString());
			//Debug.WriteLine("this._popup.VerticalOffset: "			+ this._popup.VerticalOffset.ToString());
			//Debug.WriteLine("this._popup.HorizontalOffset: "		+ this._popup.HorizontalOffset.ToString());


			PopupResizerBarLocation location = PopupResizerBarLocation.Bottom;

			// If we are above the test point then return top since we have been flipped on the y axis
			if (referencePoint.Y < offset.Y)
			{
				location = PopupResizerBarLocation.Top;
			}

			Debug.WriteLine("location: " + location.ToString());

			this.SetValue(ResizerBarLocationPropertyKey, location);
		}

				#endregion //VerifyLocation	

                // JJD 7/1/08 - added 
                #region VerifyResizerBarFlowDirection

        // JJD 7/1/08 - added ResizerBarFlowDirection to control positioning
        // of resizerbar glyphs based on Placement
        private void VerifyResizerBarFlowDirection()
        {
            Popup popup;

            if (this._popup != null)
                popup = this._popup;
            else
                popup = Utilities.GetAncestorFromType(this, typeof(Popup), true) as Popup;

            PlacementMode placement;
            FlowDirection flowDirection;

            if (popup != null)
            {
                placement       = popup.Placement;
                flowDirection   = popup.FlowDirection;
            }
            else
            {
                placement       = PlacementMode.Absolute;
                flowDirection   = this.FlowDirection;
            }

            switch (placement)
            {
                 case PlacementMode.Left:
                    // since the placement is 'Left' flip the FlowDirection so the
                    // resizer glyphs get flipped
                    if (flowDirection == FlowDirection.LeftToRight)
                        flowDirection = FlowDirection.RightToLeft;
                    break;
            }

            this.SetValue(ResizerBarFlowDirectionPropertyKey, flowDirection);
        }

                #endregion //VerifyResizerBarFlowDirection	
        
			#endregion //Private Methods	
        
		#endregion //Methods

		#region Constraints public class 

		/// <summary>
		/// Resize constraints passed into the RegisterDescendantResizeConstraints method
		/// </summary>
		/// <see cref="PopupResizerDecorator.RegisterDescendantResizeConstraints"/>
		public class Constraints : PropertyChangeNotifier
		{
			#region Private Members

			private WeakReference _elementReference;
			private PopupResizerDecorator _resizeDecorator;
			private double _minimumHeight = double.NaN;
			private double _minimumWidth = double.NaN;
			private double _maximumHeight = double.NaN;
			private double _maximumWidth = double.NaN;

			#endregion //Private Members

			#region Constructor

			/// <summary>
			/// Initializes a new instance of <see cref="Constraints"/>
			/// </summary>
			public Constraints()
			{
			}

			#endregion //Constructor

			#region Properties

				#region Public Properties

					#region MaximumHeight

			/// <summary>
			/// The maximum height this element can be resized to.
			/// </summary>
			/// <value>The maximum height in device-independent units (1/96th inch per unit), or double.NaN if no maximum is set.</value>
			/// <seealso cref="MinimumHeight"/>
			/// <seealso cref="MinimumWidth"/>
			/// <seealso cref="MaximumWidth"/>
			/// <exception cref="ArgumentOutOfRangeException">If value is negative.</exception>
			public double MaximumHeight
			{
				get { return this._maximumHeight; }
				set
				{
					if (value != this._maximumHeight)
					{
						if (value < 0)
							throw new ArgumentOutOfRangeException("MaximumHeight", value, SR.GetString("LE_ValueCannotBeNegative"));

						this._maximumHeight = value;
						this.RaisePropertyChangedEvent("MaximumHeight");
					}
				}
			}

					#endregion //MaximumHeight

					#region MaximumWidth

			/// <summary>
			/// The maximum height this element can be resized to.
			/// </summary>
			/// <value>The maximum height in device-independent units (1/96th inch per unit), or double.NaN if no maximum is set.</value>
			/// <seealso cref="MaximumHeight"/>
			/// <seealso cref="MaximumWidth"/>
			/// <seealso cref="MaximumHeight"/>
			/// <seealso cref="MaximumWidth"/>
			/// <exception cref="ArgumentOutOfRangeException">If value is negative.</exception>
			public double MaximumWidth
			{
				get { return this._maximumWidth; }
				set
				{
					if (value != this._maximumWidth)
					{
						if (value < 0)
							throw new ArgumentOutOfRangeException("MaximumWidth", value, SR.GetString("LE_ValueCannotBeNegative"));

						this._maximumWidth = value;
						this.RaisePropertyChangedEvent("MaximumWidth");
					}
				}
			}

					#endregion //MaximumWidth

					#region MinimumHeight

			/// <summary>
			/// The minimum height this element can be resized to.
			/// </summary>
			/// <value>The minimum height in device-independent units (1/96th inch per unit), or double.NaN if no minimum is set.</value>
			/// <seealso cref="MinimumWidth"/>
			/// <seealso cref="MaximumHeight"/>
			/// <seealso cref="MaximumWidth"/>
			/// <exception cref="ArgumentOutOfRangeException">If value is negative.</exception>
			public double MinimumHeight
			{
				get { return this._minimumHeight; }
				set
				{
					if (value != this._minimumHeight)
					{
						if (value < 0)
							throw new ArgumentOutOfRangeException("MinimumHeight", value, SR.GetString("LE_ValueCannotBeNegative"));

						this._minimumHeight = value;
						this.RaisePropertyChangedEvent("MinimumHeight");
					}
				}
			}

					#endregion //MinimumHeight

					#region MinimumWidth

			/// <summary>
			/// The minimum height this element can be resized to.
			/// </summary>
			/// <value>The minimum height in device-independent units (1/96th inch per unit), or double.NaN if no minimum is set.</value>
			/// <seealso cref="MinimumHeight"/>
			/// <seealso cref="MaximumHeight"/>
			/// <seealso cref="MaximumWidth"/>
			/// <exception cref="ArgumentOutOfRangeException">If value is negative.</exception>
			public double MinimumWidth
			{
				get { return this._minimumWidth; }
				set
				{
					if (value != this._minimumWidth)
					{
						if (value < 0)
							throw new ArgumentOutOfRangeException("MinimumWidth", value, SR.GetString("LE_ValueCannotBeNegative"));

						this._minimumWidth = value;
						this.RaisePropertyChangedEvent("MinimumWidth");
					}
				}
			}

					#endregion //MinimumWidth

				#endregion //Public Properties

				#region Internal Properties

					#region Element

			internal FrameworkElement Element
			{
				get
				{
                    
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                    return null != _elementReference ? Utilities.GetWeakReferenceTargetSafe(_elementReference) as FrameworkElement : null;
				}
			}

					#endregion //Element

				#endregion Internal Properties

			#endregion //Properties

			#region Methods

				#region Aggregate

			internal void Aggregate(ref double maxHorizontalDeltaNegative,
									ref double maxHorizontalDeltaPositive,
									ref double maxVerticalDeltaNegative,
									ref double maxVerticalDeltaPositive)
			{
				FrameworkElement fe = this.Element;

				if (fe == null)
					return;

				Thickness margin = fe.Margin;
				double width = fe.ActualWidth - (margin.Left + margin.Right);
				double height = fe.ActualHeight - (margin.Top + margin.Bottom);


				AggregateHelper(ref maxHorizontalDeltaNegative,
								ref maxHorizontalDeltaPositive,
								width,
								this._minimumWidth,
								this._maximumWidth,
                                false); // JJD 11/26/07 - BR28592 added isStackedDimension

				AggregateHelper(ref maxVerticalDeltaNegative,
								ref maxVerticalDeltaPositive,
								height,
								this._minimumHeight,
								this._maximumHeight,
                                true); // JJD 11/26/07 - BR28592 added isStackedDimension
			}

			private static void AggregateHelper(ref double maxDeltaNegative,
												  ref double maxDeltaPositive,
													double actual,
													double minimum,
													double maximum,
                                                    bool isStackedDimension // JJD 11/26/07 - BR28592 added isStackedDimension
                                                    )
			{
				if (!double.IsNaN(minimum) &&
					!double.IsPositiveInfinity(minimum))
				{
					double delta = 0;
                    
                    // JJD 11/26/07 - BR28592 
                    // Always calculate the delta
					//if (minimum < actual)
						delta = actual - minimum;

                    if (double.IsNaN(maxDeltaNegative))
                    {
                        // JJD 11/26/07 - BR28592 
                        // if isStackedDimension then do what we were doing, initialize the value to 0.
                        // Otherwise initialize it to the actual width.
                        if (isStackedDimension)
                            maxDeltaNegative = 0;
                        else
                            maxDeltaNegative = actual;
                    }

                    // JJD 11/26/07 - BR28592 
                    // Handle zero case here also
                    //if (delta > 0)
                    if (delta >= 0)
                    {
                        // JJD 11/26/07 - BR28592 
                        // if isStackedDimension then do what we were doing, add it to the accumulated max delta.
                        // Otherwise get the smaller of the 2
                        if (isStackedDimension)
                            maxDeltaNegative += delta;
                        else
                            maxDeltaNegative = Math.Min(maxDeltaNegative, delta);
                    }
                    else
                    {
                        // JJD 11/26/07 - BR28592 
                        // if not isStackedDimension then set maxDeltaNegative to zero
                        // since we are already below the minimum
                        if (!isStackedDimension)
                            maxDeltaNegative = 0;
                    }
				}


				if (!double.IsNaN(maximum) &&
					!double.IsPositiveInfinity(maximum) &&
					maximum > actual)
				{
					double delta = maximum - actual;

					if (double.IsNaN(maxDeltaPositive))
                    {
                        // JJD 11/26/07 - BR28592 
                        // if isStackedDimension then do what we were doing, initialize the value to 0.
                        // Otherwise initialize it to the actual width.
                        if (isStackedDimension)
                            maxDeltaPositive = 0;
                        else
                            maxDeltaPositive = double.MaxValue;
                    }

					if (delta > 0)
                    {
                        // JJD 11/26/07 - BR28592 
                        // if isStackedDimension then do what we were doing, add it to the accumulated max delta.
                        // Otherwise get the smaller of the 2
                        if (isStackedDimension)
						    maxDeltaPositive += delta;
                        else
                            maxDeltaPositive = Math.Min(maxDeltaPositive, delta);
                     }
                     else
                     {
                         // JJD 11/26/07 - BR28592 
                         // if not isStackedDimension then set maxDeltaPositive to zero
                         // since we are already above the maximum
                         if (!isStackedDimension)
                             maxDeltaPositive = 0;
                     }
                 }
			}

				#endregion //Aggregate

				#region Initialize

			internal void Initialize(FrameworkElement element)
			{
				FrameworkElement oldElement = this.Element;

				if (oldElement != null)
				{
					if (this._resizeDecorator != null)
					{
						this._resizeDecorator.UnregisterDescendantResizeConstraints(oldElement);
						this._resizeDecorator = null;
					}
					else
						if (!oldElement.IsLoaded)
							oldElement.Loaded -= new RoutedEventHandler(OnElementLoaded);

					this._elementReference = null;
				}

				if (element != null)
				{
					this._elementReference = new WeakReference(element);

					this._resizeDecorator = Utilities.GetAncestorFromType(element, typeof(PopupResizerDecorator), false, null, typeof(Popup)) as PopupResizerDecorator;

					if (this._resizeDecorator != null)
						this._resizeDecorator.RegisterDescendantResizeConstraints(element, this);
					else
						if (!element.IsLoaded)
							element.Loaded += new RoutedEventHandler(OnElementLoaded);
				}

				// AS 6/14/11 NA 11.2 Excel Style Filtering
				// When the popup associated with the PopupResizeDecorator is closed we release all references 
				// to the constraints. However the measure of the element and its parent is not dirtied so in 
				// my case where the parent panel was setting the constraint this meant that the constraint 
				// was not set the next time the popup was opened if the popup was not resized when it was 
				// opened previously (because the measure of my panel wasn't invalidated). I added a way to 
				// have a derived constraint know when it was detached.
				//
				this.OnElementChanged(oldElement, element);
			}

				#endregion //Initialize

				// AS 6/14/11 NA 11.2 Excel Style Filtering
				#region OnElementChanged
			internal virtual void OnElementChanged(FrameworkElement oldElement, FrameworkElement newElement)
			{
			} 
				#endregion //OnElementChanged

				#region OnElementLoaded

			private void OnElementLoaded(object sender, RoutedEventArgs e)
			{
				FrameworkElement fe = sender as FrameworkElement;

				FrameworkElement element = this.Element;

				Debug.Assert(fe == this.Element);

				fe.Loaded -= new RoutedEventHandler(OnElementLoaded);

				if (element == fe)
				{
					this._resizeDecorator = Utilities.GetAncestorFromType(element, typeof(PopupResizerDecorator), false) as PopupResizerDecorator;

					if (this._resizeDecorator != null)
						this._resizeDecorator.RegisterDescendantResizeConstraints(element, this);
				}
			}

				#endregion //OnElementLoaded

			#endregion //Methods
		}

		#endregion //Constraints public class
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