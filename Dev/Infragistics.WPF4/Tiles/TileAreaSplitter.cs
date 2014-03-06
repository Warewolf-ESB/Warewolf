using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Windows.Themes;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Media;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Automation.Peers.Tiles;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
    /// <summary>
    /// A slpitter used between the
    /// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class TileAreaSplitter : Thumb
    {
		#region Member Variables
		
		private static readonly Cursor _defaultHorzCursor;
		private static readonly Cursor _defaultVertCursor;

		private SplitterPreviewAdorner _adorner;
		private Point? _lastReferencePoint = null;
		private double _lastDelta = double.NaN;
		private double _minDelta;
		private double _maxDelta;

        private ToolWindow _previewWindow;

        private XamTilesControl _tilesControl;

        private static readonly ControlTemplate IndicatorToolWindowTemplate;
        private static readonly ControlTemplate IndicatorToolWindowTemplateWhite;

		#endregion //Member Variables

		#region Constructor
		internal TileAreaSplitter(XamTilesControl tilesControl)
		{
            this._tilesControl = tilesControl;
		}

		static TileAreaSplitter()
		{
			_defaultHorzCursor = Utilities.LoadCursor(typeof(TileAreaSplitter), "ResourceSets/Cursors/horizontalSplitter.cur") ?? Cursors.SizeNS;
			_defaultVertCursor = Utilities.LoadCursor(typeof(TileAreaSplitter), "ResourceSets/Cursors/verticalSplitter.cur") ?? Cursors.SizeWE;

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TileAreaSplitter), new FrameworkPropertyMetadata(typeof(TileAreaSplitter)));
			//ThumbDragService.CancelThumbDragOnEscapeProperty.OverrideMetadata(typeof(TileAreaSplitter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			EventManager.RegisterClassHandler(typeof(TileAreaSplitter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
			EventManager.RegisterClassHandler(typeof(TileAreaSplitter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
            // create the ToolWindow template without chrome for the drop indicators
            FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(ContentPresenter));
            fefRoot.Name = "PART_Content";
            ControlTemplate template = new ControlTemplate(typeof(ToolWindow));
            template.VisualTree = fefRoot;
            template.Seal();
            IndicatorToolWindowTemplate = template;

            // AS 3/30/09 TFS16355 - WinForms Interop
            // When a Popup is displayed as a child control instead of a top level window - 
            // which happens when used in a browser - the Popup does not support transparency.
            // So we need to provide a background element in that case that has a backcolor 
            // or else you will see the unrendered/uninitialized areas of the popup's hwnd
            // as black (or corrupted).
            //
            FrameworkElementFactory fefRoot2 = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory fef2 = new FrameworkElementFactory(typeof(ContentPresenter));
            fef2.Name = "PART_Content";
            fefRoot2.SetValue(Control.BackgroundProperty, System.Windows.Media.Brushes.White);
            fefRoot2.AppendChild(fef2);
            ControlTemplate template2 = new ControlTemplate(typeof(ToolWindow));
            template2.VisualTree = fefRoot2;
            template2.Seal();
            IndicatorToolWindowTemplateWhite = template2;
        } 
		#endregion //Constructor

		#region Base class overrides

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns an automation peer that exposes the <see cref="TileAreaSplitter"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Tiles.TileAreaSplitterAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new TileAreaSplitterAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer	

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

            this.InitialDeltaRange();

			// handle the preview
			if (this.IsDragging && this.ShowsPreview)
			{
                XamTilesControl tc = this._tilesControl;

                // Show the splitter in a popup if required.
                //
                if (null != tc && tc.ShowSplitterInPopup)
                {
                    Debug.Assert(_previewWindow == null);

                    _previewWindow = new ToolWindow();

                    _previewWindow.UseOSNonClientArea = false;
                    _previewWindow.ResizeMode = ResizeMode.NoResize;
                    _previewWindow.Template = GetIndicatorToolWindowTemplate(tc);

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
		public static readonly ResourceKey PreviewStyleKey = new StaticPropertyResourceKey(typeof(TileAreaSplitter), "PreviewStyleKey");

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
			typeof(Orientation), typeof(TileAreaSplitter), new FrameworkPropertyMetadata(KnownBoxes.OrientationVerticalBox));

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
				return (Orientation)this.GetValue(TileAreaSplitter.OrientationProperty);
			}
			set
			{
				this.SetValue(TileAreaSplitter.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		// FUTURE do we want to expose this publicly?
		#region ShowsPreview

		/// <summary>
		/// Identifies the <see cref="ShowsPreview"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty ShowsPreviewProperty = DependencyProperty.Register("ShowsPreview",
			typeof(bool), typeof(TileAreaSplitter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

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
				return (bool)this.GetValue(TileAreaSplitter.ShowsPreviewProperty);
			}
			set
			{
				this.SetValue(TileAreaSplitter.ShowsPreviewProperty, value);
			}
		}

		#endregion //ShowsPreview

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

        internal XamTilesControl TilesControl { get { return this._tilesControl; } }

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

        #region Internal Methods

		#region ConstrainDelta


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal double ConstrainDelta(double delta)
		{
            if (delta < this._minDelta)
                return this._minDelta;

            if (delta > this._maxDelta)
                return this._maxDelta;

			return delta;
		}
		#endregion //ConstrainDelta

        #region InitialDeltaRange

        internal void InitialDeltaRange()
        {
            if (this._tilesControl == null)
                return;

            TilesPanel panel = this._tilesControl.Panel;

            Debug.Assert(panel != null, "Can't start a TileAreaSplitter drag without a panel");

            if (panel == null)
                return;

            double currentExtent = panel.MinimizedAreaCurrentExtent;

            this._minDelta = panel.MinimizedAreaMinExtent - currentExtent;
            this._maxDelta = panel.MinimizedAreaMaxExtent - currentExtent;

            switch (panel.MaximizedModeSettingsSafe.MaximizedTileLocation)
            {
                case MaximizedTileLocation.Left:
                case MaximizedTileLocation.Top:
                    {
                        // flip the sign and the min/max caches
                        double holdMinDelta = this._minDelta;
                        this._minDelta = -this._maxDelta;
                        this._maxDelta = -holdMinDelta;
                    }
                    break;

                default:
                    break;
            }

            // constrain the values so they make sense
            this._minDelta = Math.Min(0, this._minDelta);
            this._maxDelta = Math.Max(this._maxDelta, 0);

        }

        #endregion //InitialDeltaRange	
    
        #region PerformMove
        internal void PerformMove(double x, double y)
        {
            Point screenPt = Utilities.PointToScreenSafe(this, new Point());

            double delta = this.Orientation == Orientation.Vertical ? x - screenPt.X : y - screenPt.Y;
            this.ProcessDragDelta(delta, true);

        }
        #endregion //PerformMove

		#region ProcessDelta
		/// <summary>
		/// Used to have the derived splitter class perform the resize operation.
		/// </summary>
		/// <param name="delta">The offset</param>
		/// <param name="isComplete">True if the delta change is invoked because the drag is complete</param>
        internal void ProcessDelta(double delta, bool isComplete)
        {
            if (this._tilesControl == null)
                return;

            TilesPanel panel = this._tilesControl.Panel;

            if (panel != null)
            {
                switch (panel.MaximizedModeSettingsSafe.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Top:
                        // flip the sign because the minimized area is after the
                        // maximize area a negative delta should increase the size 
                        // of the miminized area
                        delta = -delta;
                        break;
                }
                panel.MinimizedAreaExplicitExtent = panel.MinimizedAreaCurrentExtent + delta;
            }
        }
		#endregion //ProcessDelta

        #endregion //Internal Methods

		#region Private Methods

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region CreatePreviewControl
        private Control CreatePreviewControl()
        {
            // create the control that will render the preview
            Control ctrl = new Control();
            ctrl.SetResourceReference(FrameworkElement.StyleProperty, TileAreaSplitter.PreviewStyleKey);
            ctrl.IsEnabled = false;

            // pass the orientation to the style in case they want to control the 
            // look based on the orientation
            ctrl.SetValue(TileAreaSplitter.OrientationProperty, this.Orientation);

            return ctrl;
        } 
        #endregion //CreatePreviewControl

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region GetIndicatorToolWindowTemplate
        internal static ControlTemplate GetIndicatorToolWindowTemplate(XamTilesControl tc)
        {
            // when hosted in a browser, the popup will not be shown in a top level window
            // and therefore will not be allowed to support transparency
            return tc != null && tc.ShowToolWindowsInPopup && TileUtilities.IsPopupInChildWindow
                ? IndicatorToolWindowTemplateWhite
                : IndicatorToolWindowTemplate;
        }
        #endregion //GetIndicatorToolWindowTemplate

		#region OnDragCompleted
		private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
		{
			TileAreaSplitter splitter = sender as TileAreaSplitter;

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
			TileAreaSplitter splitter = sender as TileAreaSplitter;

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
			public SplitterPreviewAdorner(TileAreaSplitter splitter)
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