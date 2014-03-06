using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Diagnostics;
using System.IO.Packaging;
using Infragistics.Windows.Themes;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DockManager;
using Infragistics.Windows.Controls;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A customizable element used to redistribute space between panes in a <see cref="XamDockManager"/>
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public abstract class PaneSplitter : Thumb
	{
		#region Member Variables
		
		private static readonly Cursor _defaultHorzCursor;
		private static readonly Cursor _defaultVertCursor;

		private SplitterPreviewAdorner _adorner;
		private Point? _lastReferencePoint = null;
		private double _lastDelta = double.NaN;

        // AS 3/30/09 TFS16355 - WinForms Interop
        private ToolWindow _previewWindow;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneSplitter"/>
		/// </summary>
		protected PaneSplitter()
		{
		}

		static PaneSplitter()
		{
			_defaultHorzCursor = DockManagerUtilities.LoadCursor(typeof(PaneSplitter), "ResourceSets/Cursors/horizontalSplitter.cur") ?? Cursors.SizeNS;
			_defaultVertCursor = DockManagerUtilities.LoadCursor(typeof(PaneSplitter), "ResourceSets/Cursors/verticalSplitter.cur") ?? Cursors.SizeWE;

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneSplitter), new FrameworkPropertyMetadata(typeof(PaneSplitter)));
			ThumbDragService.CancelThumbDragOnEscapeProperty.OverrideMetadata(typeof(PaneSplitter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			EventManager.RegisterClassHandler(typeof(PaneSplitter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
			EventManager.RegisterClassHandler(typeof(PaneSplitter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		} 
		#endregion //Constructor

		#region Base class overrides

        #region LogicalChildren
        /// <summary>
        /// Returns an enumerator of the logical children
        /// </summary>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (_previewWindow != null)
                    return new SingleItemEnumerator(_previewWindow);

                return EmptyEnumerator.Instance;
            }
        } 
        #endregion //LogicalChildren

		#region OnDraggingChanged
		/// <summary>
		/// Invoked when the <see cref="Thumb.IsDragging"/> property has been changed.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnDraggingChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnDraggingChanged(e);

			// handle the preview
			if (this.IsDragging && this.ShowsPreview)
			{
                XamDockManager dm = XamDockManager.GetDockManager(this);

                // AS 3/30/09 TFS16355 - WinForms Interop
                // Show the splitter in a popup if required.
                //
                if (null != dm && dm.ShowSplitterInPopup)
                {
                    Debug.Assert(_previewWindow == null);

                    _previewWindow = new ToolWindow();

                    _previewWindow.UseOSNonClientArea = false;
                    _previewWindow.ResizeMode = ResizeMode.NoResize;
                    _previewWindow.SetValue(XamDockManager.DockManagerPropertyKey, dm);
                    _previewWindow.Template = Infragistics.Windows.DockManager.Dragging.DragManager.GetIndicatorToolWindowTemplate(dm);

                    _previewWindow.VerticalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;
                    _previewWindow.HorizontalAlignmentMode = ToolWindowAlignmentMode.UseAlignment;

                    if (this.Orientation == Orientation.Vertical)
                    {
                        _previewWindow.VerticalAlignment = VerticalAlignment.Stretch;
                        _previewWindow.HorizontalAlignment = HorizontalAlignment.Center;
                        _previewWindow.SetBinding(WidthProperty, Utilities.CreateBindingObject(ActualWidthProperty, BindingMode.OneWay, this));
                    }
                    else
                    {
                        _previewWindow.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _previewWindow.VerticalAlignment = VerticalAlignment.Center;
                        _previewWindow.SetBinding(HeightProperty, Utilities.CreateBindingObject(ActualHeightProperty, BindingMode.OneWay, this));
                    }

                    _previewWindow.Content = this.CreatePreviewControl();
                    this.AddLogicalChild(_previewWindow);
                }
                else
                {
                    // show the preview
                    Debug.Assert(null == this._adorner);

                    if (null == this._adorner)
                    {
                        AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);

                        if (null != layer)
                        {
                            this._adorner = new SplitterPreviewAdorner(this);
                            layer.Add(this._adorner);
                        }
                    }
                }
			}
			else
			{
				// remove the preview
				if (this._adorner != null)
				{
					AdornerLayer layer = VisualTreeHelper.GetParent(this._adorner) as AdornerLayer;

					Debug.Assert(null != layer);

					if (null != layer)
						layer.Remove(this._adorner);

					this._adorner = null;
				}

                // AS 3/30/09 TFS16355 - WinForms Interop
                if (this._previewWindow != null)
                {
                    ToolWindow previewWindow = _previewWindow;
                    _previewWindow = null;
                    previewWindow.Close();
                    this.RemoveLogicalChild(_previewWindow);
                }
			}
		} 
		#endregion //OnDraggingChanged

		#endregion //Base class overrides

		#region Resource Keys

		#region PreviewStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the <see cref="Control"/> instance that is used during a drag operation to represent the preview of where the splitter will be positioned.
		/// </summary>
		public static readonly ResourceKey PreviewStyleKey = new StaticPropertyResourceKey(typeof(PaneSplitter), "PreviewStyleKey");

		#endregion //PreviewStyleKey

		#endregion //Resource Keys

		#region Properties

		#region Public Properties

		#region HorizontalSplitterCursor
		/// <summary>
		/// Returns the default cursor for a horizontal pane splitter
		/// </summary>
		public static Cursor HorizontalSplitterCursor
		{
			get { return _defaultHorzCursor; }
		}
		#endregion // HorizontalSplitterCursor

		#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(PaneSplitter), new FrameworkPropertyMetadata(KnownBoxes.OrientationVerticalBox));

		/// <summary>
		/// Returns/sets the orientation of the splitter bar.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns/sets the orientation of the splitter bar.")]
		//[Category("DockManager Properties")] // Appearance
		[Bindable(true)]
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(PaneSplitter.OrientationProperty);
			}
			set
			{
				this.SetValue(PaneSplitter.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		// FUTURE do we want to expose this publicly?
		#region ShowsPreview

		/// <summary>
		/// Identifies the <see cref="ShowsPreview"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty ShowsPreviewProperty = DependencyProperty.Register("ShowsPreview",
			typeof(bool), typeof(PaneSplitter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether a preview is shown while the splitter is dragged.
		/// </summary>
		/// <seealso cref="ShowsPreviewProperty"/>
		//[Description("Returns/sets a boolean indicating whether a preview is shown while the splitter is dragged.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		internal bool ShowsPreview
		{
			get
			{
				return (bool)this.GetValue(PaneSplitter.ShowsPreviewProperty);
			}
			set
			{
				this.SetValue(PaneSplitter.ShowsPreviewProperty, value);
			}
		}

		#endregion //ShowsPreview

		#region SplitterType
		/// <summary>
		/// Returns the type of splitter
		/// </summary>
		public abstract PaneSplitterType SplitterType { get; }

		#endregion //SplitterType

		#region VerticalSplitterCursor
		/// <summary>
		/// Returns the default cursor for a vertical pane splitter
		/// </summary>
		public static Cursor VerticalSplitterCursor
		{
			get { return _defaultVertCursor; }
		} 
		#endregion // VerticalSplitterCursor

		#endregion //Public Properties

		#region Internal Properties

		#region ElementBeforeSplitter

		/// <summary>
		/// Identifies the <see cref="ElementBeforeSplitter"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty ElementBeforeSplitterProperty = DependencyProperty.Register("ElementBeforeSplitter",
			typeof(FrameworkElement), typeof(PaneSplitter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnElementBeforeSplitterChanged)));

		private static void OnElementBeforeSplitterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PaneSplitter splitter = d as PaneSplitter;
			FrameworkElement element = e.NewValue as FrameworkElement;

			// AS 10/5/09 NA 2010.1 - LayoutMode
			// Since the old pane may have been hidden as a result we need to recoerce its visibility.
			//
			FrameworkElement oldElement = e.OldValue as FrameworkElement;

			if (null != oldElement)
				oldElement.CoerceValue(VisibilityProperty);

			// base the visibility of the splitter on that of the pane
			if (element == null)
				BindingOperations.ClearBinding(splitter, FrameworkElement.VisibilityProperty);
			else
			{
				// AS 10/5/09 NA 2010.1 - LayoutMode
				// Previously the default behavior for a SinglePane PaneSplitterMode was to collapse the 
				// splitter when the pane was collapsed. Now however we may also want to collapse it 
				// for other reasons - or in the future have it visible even though the pane is not.
				//
				//splitter.SetBinding(FrameworkElement.VisibilityProperty, Utilities.CreateBindingObject(FrameworkElement.VisibilityProperty, BindingMode.OneWay, e.NewValue));
				splitter.SetBinding(FrameworkElement.VisibilityProperty, Utilities.CreateBindingObject(splitter.ElementBeforeVisibilityProperty, BindingMode.OneWay, e.NewValue));
			}
		}

		// AS 10/5/09 NA 2010.1 - LayoutMode
		internal virtual DependencyProperty ElementBeforeVisibilityProperty
		{
			get { return UIElement.VisibilityProperty; }
		}

		/// <summary>
		/// The main pane that the element is associated with. The splitter should be positioned after this pane.
		/// </summary>
		/// <seealso cref="ElementBeforeSplitterProperty"/>
		internal FrameworkElement ElementBeforeSplitter
		{
			get
			{
				return (FrameworkElement)this.GetValue(PaneSplitter.ElementBeforeSplitterProperty);
			}
			set
			{
				this.SetValue(PaneSplitter.ElementBeforeSplitterProperty, value);
			}
		}

		#endregion //ElementBeforeSplitter

		#region Splitter

		internal static readonly DependencyPropertyKey SplitterPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("Splitter",
			typeof(PaneSplitter), typeof(PaneSplitter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns the splitter associated with an element in the collection.
		/// </summary>
		/// <seealso cref="GetSplitter"/>
		internal static readonly DependencyProperty SplitterProperty =
			SplitterPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'Splitter' attached readonly property
		/// </summary>
		/// <seealso cref="SplitterProperty"/>
		internal static PaneSplitter GetSplitter(DependencyObject d)
		{
			return (PaneSplitter)d.GetValue(PaneSplitter.SplitterProperty);
		}

		#endregion //Splitter

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region SplitterVisibility

		/// <summary>
		/// SplitterVisibility Read-Only Dependency Property
		/// </summary>
		internal static readonly DependencyPropertyKey SplitterVisibilityPropertyKey
			= DependencyProperty.RegisterAttachedReadOnly("SplitterVisibility", typeof(Visibility), typeof(PaneSplitter),
				new FrameworkPropertyMetadata((Visibility)KnownBoxes.VisibilityVisibleBox));

		internal static readonly DependencyProperty SplitterVisibilityProperty
			= SplitterVisibilityPropertyKey.DependencyProperty;

		internal static Visibility GetSplitterVisibility(DependencyObject d)
		{
			return (Visibility)d.GetValue(SplitterVisibilityProperty);
		}

		#endregion // SplitterVisibility

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

        #region Internal Methods

        #region PerformMove
        internal void PerformMove(double x, double y)
        {
            Point screenPt = Utilities.PointToScreenSafe(this, new Point());

            double delta = this.Orientation == Orientation.Vertical ? x - screenPt.X : y - screenPt.Y;
            this.ProcessDragDelta(delta, true);

        }
        #endregion //PerformMove

        #endregion //Internal Methods

		#region Protected Methods

		#region ConstrainDelta
		/// <summary>
		/// Invoked to allow a derived class to control the range with which the splitter may be dragged.
		/// </summary>
		/// <param name="delta">The offset from the current position.</param>
		/// <returns>The contrained delta</returns>
		protected virtual double ConstrainDelta(double delta)
		{
			return delta;
		}
		#endregion //ConstrainDelta

		#region ProcessDelta
		/// <summary>
		/// Used to have the derived splitter class perform the resize operation.
		/// </summary>
		/// <param name="delta">The offset</param>
		/// <param name="isComplete">True if the delta change is invoked because the drag is complete</param>
		protected abstract void ProcessDelta(double delta, bool isComplete);
		#endregion //ProcessDelta

		#endregion //Protected Methods

		#region Private Methods

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region CreatePreviewControl
        private Control CreatePreviewControl()
        {
            // create the control that will render the preview
            Control ctrl = new Control();
            ctrl.SetResourceReference(FrameworkElement.StyleProperty, PaneSplitter.PreviewStyleKey);
            ctrl.IsEnabled = false;

            // pass the orientation to the style in case they want to control the 
            // look based on the orientation
            ctrl.SetValue(PaneSplitter.OrientationProperty, this.Orientation);

            return ctrl;
        } 
        #endregion //CreatePreviewControl

		#region OnDragCompleted
		private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
		{
			PaneSplitter splitter = sender as PaneSplitter;

			if (e.Canceled == false)
			{
				bool isVert = splitter.Orientation == Orientation.Vertical;

				// AS 7/14/09 TFS18424
				// There's a bug in the splitter that the logic used to calculate the delta
				// from the mouseup of the thumb is different than in the mousemove. Since its 
				// possible that MS could fix this, I can't just invert the value. Instead we'll 
				// just use the last drag delta parameter for right to left.
				//
				//splitter.ProcessDragDelta(isVert ? e.HorizontalChange : e.VerticalChange, true);
				double delta = splitter.FlowDirection == FlowDirection.RightToLeft
					? splitter._lastDelta
					: isVert ? e.HorizontalChange : e.VerticalChange;

				splitter.ProcessDragDelta(delta, true);
			}

			splitter._lastReferencePoint = null;
			splitter._lastDelta = double.NaN;
		}
		#endregion //OnDragCompleted

		#region OnDragDelta
		private static void OnDragDelta(object sender, DragDeltaEventArgs e)
		{
			PaneSplitter splitter = sender as PaneSplitter;

			bool isVert = splitter.Orientation == Orientation.Vertical;
			splitter.ProcessDragDelta(isVert ? e.HorizontalChange : e.VerticalChange, false);
		}
		#endregion //OnDragDelta

		#region ProcessDragDelta

		private void ProcessDragDelta(double delta, bool isComplete)
		{
			bool showsPreview = this.ShowsPreview;
			Point referencePoint = Utilities.PointFromScreenSafe(this, new Point());
			double oldDelta = this._lastDelta;
			this._lastDelta = delta;

			if ((isComplete || this.ShowsPreview == false))
			{
				if (this._lastReferencePoint != null &&
					this._lastReferencePoint == referencePoint)
				{
					delta -= oldDelta;
				}

				this._lastReferencePoint = referencePoint;
			}

			if (showsPreview == false && isComplete)
				delta = 0d;

			double deltaBefore = delta;

			// allow a derived class to constrain the delta
			delta = this.ConstrainDelta(delta);

			// augment the cached last delta to exclude the portion ignored
			this._lastDelta -= (deltaBefore - delta);

			// update the preview
			if (isComplete == false && showsPreview)
			{
                // AS 3/30/09 TFS16355 - WinForms Interop
                bool isVertical = this.Orientation == Orientation.Vertical;

				if (this._adorner != null)
				{
					if (isVertical)
						this._adorner.OffsetX = delta;
					else
						this._adorner.OffsetY = delta;
				}

                // AS 3/30/09 TFS16355 - WinForms Interop
                if (_previewWindow != null)
                {
                    if (isVertical)
                        _previewWindow.HorizontalAlignmentOffset = delta;
                    else
                        _previewWindow.VerticalAlignmentOffset = delta;

                    if (!_previewWindow.IsVisible)
                        _previewWindow.Show(this, false, true);
                }
			}
			else
				this.ProcessDelta(delta, isComplete);
		}
		#endregion //ProcessDragDelta

		#endregion //Private Methods

		#endregion //Methods

		#region SplitterPreviewAdorner
		private class SplitterPreviewAdorner : Adorner
		{
			#region Member Variables

			private UIElement _element;
			private TranslateTransform _transform;

			#endregion //Member Variables

			#region Constructor
			public SplitterPreviewAdorner(PaneSplitter splitter)
				: base(splitter)
			{
                
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

                Control ctrl = splitter.CreatePreviewControl();

				Decorator decorator = new Decorator();
				decorator.Child = ctrl;
				this._element = decorator;

				this._transform = new TranslateTransform();
				this._element.RenderTransform = this._transform;

				this.AddVisualChild(this._element);
			}
			#endregion //Constructor

			#region Properties
			public double OffsetX
			{
				get { return this._transform.X; }
				set { this._transform.X = value; }
			}

			public double OffsetY
			{
				get { return this._transform.Y; }
				set { this._transform.Y = value; }
			}
			#endregion //Properties

			#region Base class overrides
			protected override int VisualChildrenCount
			{
				get
				{
					return 1;
				}
			}

			protected override Visual GetVisualChild(int index)
			{
				if (index == 0)
					return this._element;

				return base.GetVisualChild(index);
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				this._element.Arrange(new Rect(finalSize));
				return finalSize;
			}
			#endregion //Base class overrides
		} 
		#endregion //SplitterPreviewAdorner
    }

	/// <summary>
	/// Splitter used to resize 2 adjacent panes within a <see cref="SplitPane"/>
	/// </summary>
	public class SplitPaneSplitter : PaneSplitter
	{
		#region Member Variables

		private SplitDragInfo _dragInfo;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="SplitPaneSplitter"/>
		/// </summary>
		internal SplitPaneSplitter()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="SplitPaneSplitter"/> Automation Peer Class <see cref="SplitPaneSplitterAutomationPeer"/>
        /// </summary>
        /// <returns>PaneSplitter</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SplitPaneSplitterAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

		#region ConstrainDelta
		/// <summary>
		/// Adjusts the delta based on the area available.
		/// </summary>
		/// <param name="delta">The proposed adjustment</param>
		/// <returns>A modified adjustment to ensure the splitter doesn't go outside the parent.</returns>
		protected override double ConstrainDelta(double delta)
		{
			if (delta < this._dragInfo.BeforeExtent)
				delta = this._dragInfo.BeforeExtent;
			else if (delta > this._dragInfo.AfterExtent)
				delta = this._dragInfo.AfterExtent;

			return delta;
		}
		#endregion //ConstrainDelta

		#region OnDraggingChanged
		/// <summary>
		/// Invoked when the <see cref="Thumb.IsDragging"/> property is changed.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnDraggingChanged(DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				// determine the panes that will be affected
				SplitPane split = VisualTreeHelper.GetParent(this) as SplitPane;

				FrameworkElement elementBefore = this.ElementBeforeSplitter;
				FrameworkElement elementAfter = null;

				Debug.Assert(null != split);

				// AS 7/1/10 TFS35312
				// If a splitpanesplitter is to be resized and we don't have an explicit size on 
				// the floating window that contains this then we have to set one if we have an 
				// hwndhost because the desiredsize of the hwndhost is based on the last arranged 
				// size. What was happening is that since we had no constraining size on the floating 
				// window we were measured with the screen dimensions. We would measure the elements 
				// and use the percentage of their desired size. We would arrange the element with 
				// their percentage which could be bigger than its desired. When we got back into 
				// the measure, the hwndhost would return the size it was last arranged out so we 
				// would get a bigger desired size for it and the ultimate desired size would be 
				// larger resulting in the window getting larger.
				//
				if (DockManagerUtilities.IsFloating(XamDockManager.GetPaneLocation(split)))
				{
					ToolWindow tw = ToolWindow.GetToolWindow(split);
					SplitPane rootSplit = tw != null ? tw.Content as SplitPane : null;

					if (null != rootSplit && XamDockManager.GetFloatingSize(rootSplit).IsEmpty)
					{
						List<ContentPane> panes = DockManagerUtilities.GetAllPanes(split, PaneFilterFlags.AllVisible);

						if (null != panes)
						{
							foreach (ContentPane cp in panes)
							{
								if (cp.HasHwndHost)
								{
									XamDockManager.SetFloatingSize(rootSplit, rootSplit.RenderSize);
									break;
								}
							}
						}
					}
				}

				int index = split.Panes.IndexOf(elementBefore);

				for (int i = index + 1, count = split.Panes.Count; i < count; i++)
				{
					FrameworkElement element = split.Panes[i];

					if (element.Visibility == Visibility.Visible)
					{
						elementAfter = element;
						break;
					}
				}

				if (elementAfter == null)
				{
					if (this.IsMouseCaptured)
						this.ReleaseMouseCapture();
					else
						this.CancelDrag();
				}
				else
					this._dragInfo = new SplitDragInfo(this, split, elementBefore, elementAfter);
			}
			else
			{
				//this._dragInfo = null;
			}

			base.OnDraggingChanged(e);
		} 
		#endregion //OnDraggingChanged

		#region ProcessDelta
		/// <summary>
		/// Invoked when the pane should process a resize operation.
		/// </summary>
		/// <param name="delta">The amount of the change</param>
		/// <param name="isComplete">True if the resize operation is completed</param>
		protected override void ProcessDelta(double delta, bool isComplete)
		{
			// handle process delta for split panes
			this._dragInfo.ProcessDelta(delta, isComplete);

			if (isComplete)
				this._dragInfo = null;
		}
		#endregion //ProcessDelta

		#region SplitterType
		/// <summary>
		/// Returns the type of splitter
		/// </summary>
		public override PaneSplitterType SplitterType { get { return PaneSplitterType.SplitPane;} }

		#endregion //SplitterType

		#endregion //Base class overrides

		#region SplitDragInfo
		private class SplitDragInfo
		{
			#region Member Variables

			private FrameworkElement _elementBefore;
			private FrameworkElement _elementAfter;
			private SplitPane _split;
			private PaneSplitter _splitter;
			private double _beforeExtent = double.NaN;
			private double _afterExtent = double.NaN;
			private bool _isVertical;
			private const double MinExtraSpace = 10;

			#endregion //Member Variables

			#region Constructor
			internal SplitDragInfo(PaneSplitter splitter, SplitPane split, FrameworkElement elementBefore, FrameworkElement elementAfter)
			{
				Debug.Assert(null != splitter && null != split && null != elementBefore && null != elementAfter);

				this._splitter = splitter;
				this._elementBefore = elementBefore;
				this._elementAfter = elementAfter;
				this._split = split;
				this._isVertical = splitter.Orientation == Orientation.Vertical;
			}
			#endregion //Constructor

			#region Properties
			internal double BeforeExtent
			{
				get
				{
					if (double.IsNaN(this._beforeExtent))
						this.CalculateBeforeAfterExtent();

					return this._beforeExtent;
				}
			}

			internal double AfterExtent
			{
				get
				{
					if (double.IsNaN(this._afterExtent))
						this.CalculateBeforeAfterExtent();

					return this._afterExtent;
				}
			}
			#endregion //Properties

			#region Methods

			#region CalculateBeforeAfterExtent
			private void CalculateBeforeAfterExtent()
			{
				bool isVertical = this._isVertical;
				Size minSizeBefore = new Size(this._elementBefore.MinWidth, this._elementBefore.MinHeight);
				Size maxSizeBefore = new Size(this._elementBefore.MaxWidth, this._elementBefore.MaxHeight);

				Size minSizeAfter = new Size(this._elementAfter.MinWidth, this._elementAfter.MinHeight);
				Size maxSizeAfter = new Size(this._elementAfter.MaxWidth, this._elementAfter.MaxHeight);

				Size sizeBefore = new Size(this._elementBefore.ActualWidth, this._elementBefore.ActualHeight);
				Size sizeAfter = new Size(this._elementAfter.ActualWidth, this._elementAfter.ActualHeight);

				double MinWidth = MinExtraSpace + (isVertical ? SystemParameters.VerticalScrollBarWidth : SystemParameters.HorizontalScrollBarHeight);

				// get the extents so we don't need to deal with direction
				double currentExtentBefore = (isVertical ? sizeBefore.Width : sizeBefore.Height);
				double currentExtentAfter = (isVertical ? sizeAfter.Width : sizeAfter.Height);
				double minExtentBefore = Math.Max(Math.Min(currentExtentBefore, MinWidth), (isVertical ? minSizeBefore.Width : minSizeBefore.Height));
				double maxExtentBefore = (isVertical ? maxSizeBefore.Width : maxSizeBefore.Height);
				double minExtentAfter = Math.Max(Math.Min(currentExtentAfter, MinWidth), (isVertical ? minSizeAfter.Width : minSizeAfter.Height));
				double maxExtentAfter = (isVertical ? maxSizeAfter.Width : maxSizeAfter.Height);

				double splitterWidth = (isVertical ? this._splitter.ActualWidth : this._splitter.ActualHeight);
				double beforeExtent, afterExtent;

				// if either of the minimums, exceed the maximum there should be no size change
				if ((minExtentBefore >= maxExtentBefore && double.IsNaN(maxExtentBefore) == false)
					|| (minExtentAfter >= maxExtentAfter && double.IsNaN(maxExtentAfter) == false))
				{
					beforeExtent = afterExtent = 0;
				}

				// if the max for the before pane is equal to or greater than the current size
				// its size cannot be increased
				if (double.IsNaN(maxExtentBefore) == false && currentExtentBefore >= maxExtentBefore)
					afterExtent = 0;
				else if (currentExtentAfter <= minExtentAfter)
					// if the after pane is at or less than its minimum
					afterExtent = 0;
				else if (maxExtentBefore != 0)
					// as long as there is a max value for the before, the amount after
					// is the lesser of the remaining space before the after pane's min or
					// the remaining space before the before pane' max
					afterExtent = (int)Math.Min(currentExtentAfter - minExtentAfter,
						maxExtentBefore - currentExtentBefore);
				else // otherwise return the remaining after space before its min
					afterExtent = currentExtentAfter - minExtentAfter;

				// if the max for the after pane is equal to or greater than the current size
				// its size cannot be increased
				if (maxExtentAfter != 0 && currentExtentAfter >= maxExtentAfter)
					beforeExtent = 0;
				else if (currentExtentBefore <= minExtentBefore)
					// if the before pane is at or less than its minimum
					beforeExtent = 0;
				else if (maxExtentAfter != 0)
					// as long as there is a max value for the before, the amount after
					// is the lesser of the remaining space before the after pane's min or
					// the remaining space before the before pane' max
					beforeExtent = -(int)Math.Min(currentExtentBefore - minExtentBefore,
						maxExtentAfter - currentExtentAfter);
				else // otherwise return the remaining after space before its min
					beforeExtent = -(currentExtentBefore - minExtentBefore);

				this._beforeExtent = beforeExtent;
				this._afterExtent = afterExtent;
			}
			#endregion //CalculateBeforeAfterExtent

			#region Dirty
			internal void Dirty()
			{
				this._beforeExtent = this._afterExtent = double.NaN;
			}
			#endregion //Dirty

			#region ProcessDelta
			internal void ProcessDelta(double delta, bool isComplete)
			{
				Size relativeSizeBefore = SplitPane.GetRelativeSize(this._elementBefore);
				Size relativeSizeAfter = SplitPane.GetRelativeSize(this._elementAfter);

				// store the original relative sizes
				double relativeBefore = this._isVertical ? relativeSizeBefore.Width : relativeSizeBefore.Height;
				double relativeAfter = this._isVertical ? relativeSizeAfter.Width : relativeSizeAfter.Height;

				double extentBefore = this._isVertical ? this._elementBefore.ActualWidth : this._elementBefore.ActualHeight;
				double extentAfter = this._isVertical ? this._elementAfter.ActualWidth : this._elementAfter.ActualHeight;

				double totalRelative = relativeBefore + relativeAfter;
				double totalExtent = extentBefore + extentAfter;

				extentBefore += delta;
				extentAfter -= delta;

				double newRelativeBefore = totalRelative * (extentBefore / Math.Max(totalExtent, 1d));
				double newRelativeAfter = totalRelative - newRelativeBefore;

				if (this._isVertical)
				{
					relativeSizeBefore.Width = newRelativeBefore;
					relativeSizeAfter.Width = newRelativeAfter;
				}
				else
				{
					relativeSizeBefore.Height = newRelativeBefore;
					relativeSizeAfter.Height = newRelativeAfter;
				}

				SplitPane.SetRelativeSize(this._elementBefore, relativeSizeBefore);
				SplitPane.SetRelativeSize(this._elementAfter, relativeSizeAfter);

				if (isComplete == false)
					this.Dirty();
			} 
			#endregion //ProcessDelta

			#endregion //Methods
		} 
		#endregion //SplitDragInfo
	}

	/// <summary>
	/// Splitter used to resize a root <see cref="SplitPane"/> within the <see cref="XamDockManager"/>
	/// </summary>
	public class DockedPaneSplitter : PaneSplitter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DockedPaneSplitter"/>
		/// </summary>
		internal DockedPaneSplitter()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		// AS 10/5/09 NA 2010.1 - LayoutMode
		#region ElementBeforeVisibilityProperty
		internal override DependencyProperty ElementBeforeVisibilityProperty
		{
			get { return PaneSplitter.SplitterVisibilityProperty; }
		} 
		#endregion //ElementBeforeVisibilityProperty

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="DockedPaneSplitter"/> Automation Peer Class <see cref="DockedPaneSplitterAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DockedPaneSplitterAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

		#region ConstrainDelta
		/// <summary>
		/// Adjusts the delta based on the area available.
		/// </summary>
		/// <param name="delta">The proposed adjustment</param>
		/// <returns>A modified adjustment to ensure the splitter doesn't go outside the parent.</returns>
		protected override double ConstrainDelta(double delta)
		{
			FrameworkElement element = this.ElementBeforeSplitter;
			bool isHorz = this.Orientation == Orientation.Vertical;

			double current = isHorz ? element.ActualWidth : element.ActualHeight;

			// AS 4/29/08 BR32338
			// Enforce a minimum draggable extent
			//
			// AS 5/16/08
			// Was using isHorz as if it were the orientation of the splitter instead
			// of the orientation of the resize operation.
			//
			double MinExtent = 10 + (isHorz ? SystemParameters.VerticalScrollBarWidth : SystemParameters.HorizontalScrollBarHeight);

			
			double min = Math.Min(current, Math.Max(MinExtent, isHorz ? element.MinWidth : element.MinHeight));
			double max = Math.Max(current, isHorz ? element.MaxWidth : element.MaxHeight);

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager && dockManager.DockPanel != null)
			{
				Size contentSize = dockManager.DockPanel.AvailableContentSize;

				// AS 5/16/08 BR32019 [Start]
				// Also allow for overlapping existing panes that are docked in the 
				// same orientation but not at the same "level".
				//
				SplitPane root = element as SplitPane;
				Debug.Assert(null != root);
				int startingIndex = dockManager.Panes.IndexOf(root);
				bool hasChangedOrientation = false;
				bool isRootLeftRight = root.IsDockedLeftRight;

				for (int i = startingIndex + 1, count = dockManager.Panes.Count; i < count; i++)
				{
					SplitPane siblingPane = dockManager.Panes[i];

					// only evaluate visible docked panes after the root pane since those are the
					// ones we can "take away from"
					if (DockManagerUtilities.IsDocked(siblingPane) && siblingPane.Visibility == Visibility.Visible)
					{
						bool isLeftRight = siblingPane.IsDockedLeftRight;

						if (isLeftRight != isRootLeftRight)
						{
							// once we hit a split in the opposite orientation, we need to know
							// because any subsequent panes that are split in the same direction
							// as the associated root split pane can be "crushed" towards their
							// minimum.
							hasChangedOrientation = true;
						}
						else if (hasChangedOrientation)
						{
							// all panes in the same "level" before we hit an intervening split
							// docked in the opposite orientation can be "crushed"
							if (hasChangedOrientation)
							{
								double siblingCurrent = siblingPane.ActualWidth;
								double siblingMin = DockManagerPanel.GetMinimumExtent(siblingPane, isLeftRight);
								double diff = siblingCurrent - siblingMin;

								if (diff > 0)
								{
									if (isLeftRight)
										contentSize.Width += diff;
									else
										contentSize.Height += diff;
								}
							}
						}
					}
				}
				// AS 5/16/08 BR32019 [End]

				max = Math.Min(max, current + (isHorz ? contentSize.Width : contentSize.Height));
			}

			double newExtent = ProcessDeltaHelper(delta, false);

			// e.g. min=20, new=1, current=5 (s/b 5)
			// e.g. max=20, new=30, current=25 (s/b 25)
			// e.g. min=20, new=10, current=5 (s/b 10)
			// e.g. max=20, new=22, current=25 (s/b 22)
			if (newExtent > max)
				delta += AdjustDeltaForLocation(max - newExtent);
			else if (newExtent < min)
				delta += AdjustDeltaForLocation(min - newExtent);

			return delta;
		}
		#endregion //ConstrainDelta

		#region ProcessDelta
		/// <summary>
		/// Invoked when the pane should process a resize operation.
		/// </summary>
		/// <param name="delta">The amount of the change</param>
		/// <param name="isComplete">True if the resize operation is completed</param>
		protected override void ProcessDelta(double delta, bool isComplete)
		{
			ProcessDeltaHelper(delta, true);
		}
		#endregion //ProcessDelta 

		#region SplitterType
		/// <summary>
		/// Returns the type of splitter
		/// </summary>
		public override PaneSplitterType SplitterType { get { return PaneSplitterType.DockedPane; } }

		#endregion //SplitterType

		#endregion //Base class overrides

		#region Methods

		#region AdjustDeltaForLocation
		private double AdjustDeltaForLocation(double delta)
		{
			// if the pane is on the bottom/right, invert the value since going
			// up/left should increase the size of the pane
			switch (XamDockManager.GetPaneLocation(this.ElementBeforeSplitter))
			{
				case PaneLocation.DockedBottom:
				case PaneLocation.DockedRight:
					return -delta;
				default:
					return delta;
			}
		}
		#endregion //AdjustDeltaForLocation

		#region ProcessDeltaHelper
		private double ProcessDeltaHelper(double delta, bool resizeElement)
		{
			FrameworkElement element = this.ElementBeforeSplitter;
			bool isVert = this.Orientation == Orientation.Vertical;
			DependencyProperty propertyToSet;
			double newExtent = AdjustDeltaForLocation(delta);

			if (isVert)
			{
				propertyToSet = FrameworkElement.WidthProperty;
				// AS 9/24/09 TFS22599
				//newExtent += (double.IsNaN(element.Width) ? element.ActualWidth : element.Width);
				newExtent += DockManagerUtilities.GetWidth(element, true);
			}
			else
			{
				propertyToSet = FrameworkElement.HeightProperty;
				// AS 9/24/09 TFS22599
				//newExtent += (double.IsNaN(element.Height) ? element.ActualHeight : element.Height);
				newExtent += DockManagerUtilities.GetHeight(element, true);
			}

			if (resizeElement)
			{
				// ensure its not < 0
				newExtent = Math.Max(newExtent, 0);

				element.SetValue(propertyToSet, newExtent);
			}

			return newExtent;
		}
		#endregion //ProcessDeltaHelper

		#endregion //Methods
	}

	/// <summary>
	/// Splitter used to resize the control displayed in the <see cref="UnpinnedTabFlyout"/> of an <see cref="UnpinnedTabArea"/>
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note:</b> This class is only meant to be used within the template of a <see cref="UnpinnedTabFlyout"/>.</p>
	/// </remarks>
	public class UnpinnedTabFlyoutSplitter : PaneSplitter
	{
		#region Member Variables

		private const double MinExtraSpace = 10;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UnpinnedTabFlyoutSplitter"/>
		/// </summary>
		public UnpinnedTabFlyoutSplitter()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="UnpinnedTabFlyoutSplitter"/> Automation Peer Class <see cref="UnpinnedTabFlyoutSplitterAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new UnpinnedTabFlyoutSplitterAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

		#region ConstrainDelta
		/// <summary>
		/// Adjusts the delta based on the area available.
		/// </summary>
		/// <param name="delta">The proposed adjustment</param>
		/// <returns>A modified adjustment to ensure the splitter doesn't go outside the parent.</returns>
		protected override double ConstrainDelta(double delta)
		{
			FrameworkElement element = this.TemplatedParent as UnpinnedTabFlyout;
			bool isHorz = this.Orientation == Orientation.Vertical;

			double current = isHorz ? element.ActualWidth : element.ActualHeight;

			// AS 4/29/08 BR32338
			// Enforce a minimum draggable extent
			//
			// AS 5/16/08
			// Was using isHorz as if it were the orientation of the splitter instead
			// of the orientation of the resize operation. Also including the extent
			// of the splitter since it is within the flyout that we are resizing.
			//
			double MinExtent = 10 + (isHorz
				? SystemParameters.VerticalScrollBarWidth + this.ActualWidth
				: SystemParameters.HorizontalScrollBarHeight + this.ActualHeight);

			
			double min = Math.Min(current, Math.Max(MinExtent, isHorz ? element.MinWidth : element.MinHeight));
			double max = Math.Max(current, isHorz ? element.MaxWidth : element.MaxHeight);

			XamDockManager dockManager = XamDockManager.GetDockManager(this);
			DockManagerPanel panel = dockManager != null ? dockManager.DockPanel : null;

			if (null != panel)
			{
				// AS 5/16/08
				// Leave space for a scrollbar plus a little.
				//
				//const double ExtraSpace = 10;
				// AS 6/15/12 TFS114790
				//double ExtraSpace = MinExtraSpace + (isHorz ? SystemParameters.VerticalScrollBarWidth : SystemParameters.HorizontalScrollBarHeight);
				double ExtraSpace = GetReservedSpace(isHorz);
				max = Math.Max(Math.Min(max, -ExtraSpace + (isHorz ? panel.ActualWidth : panel.ActualHeight)), 0);
			}

			double newExtent = ProcessDeltaHelper(delta, false);

			// e.g. min=20, new=1, current=5 (s/b 5)
			// e.g. max=20, new=30, current=25 (s/b 25)
			// e.g. min=20, new=10, current=5 (s/b 10)
			// e.g. max=20, new=22, current=25 (s/b 22)
			if (newExtent > max)
				delta += AdjustDeltaForLocation(max - newExtent);
			else if (newExtent < min)
				delta += AdjustDeltaForLocation(min - newExtent);

			return delta;
		}
		#endregion //ConstrainDelta

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (this.TemplatedParent is UnpinnedTabFlyout == false)
				return;

			base.OnMouseLeftButtonDown(e);
		} 
		#endregion //OnMouseLeftButtonDown

		#region ProcessDelta
		/// <summary>
		/// Invoked when the pane should process a resize operation.
		/// </summary>
		/// <param name="delta">The amount of the change</param>
		/// <param name="isComplete">True if the resize operation is completed</param>
		protected override void ProcessDelta(double delta, bool isComplete)
		{
			ProcessDeltaHelper(delta, true);
		}
		#endregion //ProcessDelta

		#region SplitterType
		/// <summary>
		/// Returns the type of splitter
		/// </summary>
		public override PaneSplitterType SplitterType { get { return PaneSplitterType.UnpinnedPane; } }

		#endregion //SplitterType

		#endregion //Base class overrides

		#region Methods

		#region AdjustDeltaForLocation
		private double AdjustDeltaForLocation(double delta)
		{
			UnpinnedTabFlyout flyout = this.TemplatedParent as UnpinnedTabFlyout;

			if (null != flyout)
			{
				switch (flyout.Side)
				{
					// if the pane is on the bottom/right, invert the value since going
					// up/left should increase the size of the pane
					case Dock.Right:
					case Dock.Bottom:
						return -delta;
				}
			}

			return delta;
		}
		#endregion //AdjustDeltaForLocation

		// AS 6/15/12 TFS114790
		// Moved from the ConstrainDelta so we can use the same constraint elsewhere.
		//
		#region GetReservedSpace
		internal static double GetReservedSpace(bool isHorizontal)
		{
			return MinExtraSpace + (isHorizontal ? SystemParameters.VerticalScrollBarWidth : SystemParameters.HorizontalScrollBarHeight);
		} 
		#endregion //GetReservedSpace

		#region ProcessDeltaHelper
		private double ProcessDeltaHelper(double delta, bool resizeElement)
		{
			FrameworkElement element = this.TemplatedParent as UnpinnedTabFlyout;
			bool isVert = this.Orientation == Orientation.Vertical;
			DependencyProperty propertyToSet;
			double newExtent = AdjustDeltaForLocation(delta);

			if (isVert)
			{
				propertyToSet = FrameworkElement.WidthProperty;
				// AS 9/24/09 TFS22599
				//newExtent += (double.IsNaN(element.Width) ? element.ActualWidth : element.Width);
				newExtent += DockManagerUtilities.GetWidth(element, true);
			}
			else
			{
				propertyToSet = FrameworkElement.HeightProperty;
				// AS 9/24/09 TFS22599
				//newExtent += (double.IsNaN(element.Height) ? element.ActualHeight : element.Height);
				newExtent += DockManagerUtilities.GetHeight(element, true);
			}

			if (resizeElement)
			{
				// ensure its not < 0
				newExtent = Math.Max(newExtent, 0);

				element.SetValue(propertyToSet, newExtent);
			}

			return newExtent;
		}
		#endregion //ProcessDeltaHelper

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