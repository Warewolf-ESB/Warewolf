using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Media;
using Infragistics.Controls;
using Infragistics.AutomationPeers;
using Infragistics.Collections;
using Infragistics.Controls.Primitives;
using System.Windows.Automation;





namespace Infragistics.Controls.Layouts.Primitives
{
    /// <summary>
    /// A splitter used between maximized and minimized tile areas in a <see cref="XamTileManager"/>
    /// </summary>
	[TemplatePart(Name = "Thumb", Type = typeof(Thumb))]
	[StyleTypedProperty(Property = "PreviewStyle", StyleTargetType = typeof(Control))]
	[DesignTimeVisible(false)]	
	public class TileAreaSplitter : Control
    {
		#region Member Variables
		
		private static readonly Cursor _defaultHorzCursor;
		private static readonly Cursor _defaultVertCursor;

		private Point _lastScreenOrigin;
		private Point _lastControlOffset;
		private Point? _lastReferencePoint = null;
		private double _lastDelta = double.NaN;
		private double _minDelta;
		private double _maxDelta;
		private double _zoomFactor = 1;




		private Thumb _thumb;

		private PopupDragManager _popupManager;

        private XamTileManager _tileManager;

		
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

		#endregion //Member Variables

		#region Constructor

		static TileAreaSplitter()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TileAreaSplitter), new FrameworkPropertyMetadata(typeof(TileAreaSplitter)));

			_defaultHorzCursor = Infragistics.Windows.Utilities.LoadCursor(typeof(TileAreaSplitter), "Cursors/horizontalSplitter.cur") ?? Cursors.SizeNS;
			_defaultVertCursor = Infragistics.Windows.Utilities.LoadCursor(typeof(TileAreaSplitter), "Cursors/verticalSplitter.cur") ?? Cursors.SizeWE;
			




        } 

		internal TileAreaSplitter(XamTileManager tileManager)
		{



            this._tileManager = tileManager;

			AutomationProperties.SetName(this, "TileAreaSplitter");

			this.Cursor = VerticalSplitterCursor;

			
			
			
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template has been applied to the element.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Thumb thumb = this.GetTemplateChild("Thumb") as Thumb;

			if (thumb != _thumb)
			{

				if (_thumb != null)
				{
					_thumb.DragDelta -= new DragDeltaEventHandler(OnDragDelta);
					_thumb.DragCompleted -= new DragCompletedEventHandler(OnDragCompleted);
					
					this.ClearValue(IsDraggingProperty);
				}

				_thumb = thumb;

				if (_thumb != null)
				{
					this.SetBinding(IsDraggingProperty, TileUtilities.CreateBinding(Thumb.IsDraggingProperty, BindingMode.OneWay, _thumb, typeof(Thumb), "IsDraggingProperty"));

					_thumb.DragDelta += new DragDeltaEventHandler(OnDragDelta);
					_thumb.DragCompleted += new DragCompletedEventHandler(OnDragCompleted);
				}
			}
		}

		#endregion //OnApplyTemplate

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns an automation peer that exposes the <see cref="TileAreaSplitter"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="TileAreaSplitterAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new TileAreaSplitterAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer	

		#region OnMouseMove

		/// <summary>
		/// Called when the mouse is moved.
		/// </summary>
		/// <param name="e">arguments</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (this.IsDragging == false)
			{
				_lastControlOffset = e.GetPosition(this);

				if (TileAreaSplitter.GetOrientation(this) == Orientation.Vertical)
					_lastControlOffset.Y = 0;
				else
					_lastControlOffset.X = 0;


				_lastScreenOrigin = Infragistics.Windows.Utilities.PointToScreenSafe(this, new Point());





			}
		}

		#endregion //OnMouseMove	

        #region LogicalChildren

        /// <summary>
        /// Returns an enumerator of the logical children
        /// </summary>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
				
				//if (_previewWindow != null)
				//    return new SingleItemEnumerator(_previewWindow);

                return EmptyEnumerator.Instance;
            }
        } 

        #endregion //LogicalChildren

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region IsDragging

		/// <summary>
		/// Identifies the <see cref="IsDragging"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDraggingProperty = DependencyPropertyUtilities.Register("IsDragging",
			typeof(bool), typeof(TileAreaSplitter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsDraggingChanged))
			);

		private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TileAreaSplitter instance = (TileAreaSplitter)d;

			instance.OnIsDraggingChanged();

		}

		/// <summary>
		/// Returns or sets
		/// </summary>
		/// <seealso cref="IsDraggingProperty"/>
		public bool IsDragging
		{
			get
			{
				return (bool)this.GetValue(TileAreaSplitter.IsDraggingProperty);
			}
			internal set
			{
				this.SetValue(TileAreaSplitter.IsDraggingProperty, value);
			}
		}

		#endregion //IsDragging

		#region Orientation

		/// <summary>
		/// Identifies the Orientation attached dependency property
		/// </summary>
		/// <seealso cref="GetOrientation"/>
		/// <seealso cref="SetOrientation"/>
		public static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.RegisterAttached("Orientation",
			typeof(Orientation), typeof(TileAreaSplitter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationVerticalBox, new PropertyChangedCallback(OnOrientationChanged))
			);


		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TileAreaSplitter instance = d as TileAreaSplitter;

			if (instance != null)
			{
				if (Orientation.Vertical == (Orientation)e.NewValue)
					instance.Cursor = VerticalSplitterCursor;
				else
					instance.Cursor = HorizontalSplitterCursor;
			}
		}

		/// <summary>
		/// Gets the orientation of the splitter bar.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="OrientationProperty"/>
		/// <seealso cref="SetOrientation"/>
		public static Orientation GetOrientation(DependencyObject d)
		{
			return (Orientation)d.GetValue(TileAreaSplitter.OrientationProperty);
		}

		/// <summary>
		/// Sets the orientation of the splitter bar..
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="OrientationProperty"/>
		/// <seealso cref="GetOrientation"/>
		public static void SetOrientation(DependencyObject d, Orientation value)
		{
			d.SetValue(TileAreaSplitter.OrientationProperty, value);
		}

		#endregion //Orientation

		#region PreviewStyle

		/// <summary>
		/// Identifies the <see cref="PreviewStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreviewStyleProperty = DependencyPropertyUtilities.Register("PreviewStyle",
			typeof(Style), typeof(TileAreaSplitter),
			null, null	);

		/// <summary>
		/// Returns or sets <see cref="Style"/> for the <see cref="Control"/> instance that is used during a drag operation to represent the preview of where the splitter will be positioned.
		/// </summary>
		/// <seealso cref="PreviewStyleProperty"/>
		public Style PreviewStyle
		{
			get
			{
				return (Style)this.GetValue(TileAreaSplitter.PreviewStyleProperty);
			}
			set
			{
				this.SetValue(TileAreaSplitter.PreviewStyleProperty, value);
			}
		}

		#endregion //PreviewStyle

		#endregion //Public Properties

		#region Internal Properties

		#region HorizontalSplitterCursor
		/// <summary>
		/// Returns the default cursor for a horizontal pane splitter
		/// </summary>
		internal static Cursor HorizontalSplitterCursor
		{
			get { return _defaultHorzCursor; }
		}
		#endregion // HorizontalSplitterCursor

		#region ShowsPreview

		/// <summary>
		/// Identifies the <see cref="ShowsPreview"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowsPreviewProperty = DependencyPropertyUtilities.Register("ShowsPreview",
			typeof(bool), typeof(TileAreaSplitter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			);

		/// <summary>
		/// Returns/sets a boolean indicating whether a preview is shown while the splitter is dragged.
		/// </summary>
		/// <seealso cref="ShowsPreviewProperty"/>
		//[Description("Returns/sets a boolean indicating whether a preview is shown while the splitter is dragged.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public bool ShowsPreview
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

		#region TileManager

		internal XamTileManager TileManager { get { return this._tileManager; } }

		#endregion //TileManager	
    
		#region VerticalSplitterCursor
		/// <summary>
		/// Returns the default cursor for a vertical pane splitter
		/// </summary>
		internal static Cursor VerticalSplitterCursor
		{
			get { return _defaultVertCursor; }
		} 
		#endregion // VerticalSplitterCursor

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
            if (this._tileManager == null)
                return;

            TileAreaPanel panel = this._tileManager.Panel;

            Debug.Assert(panel != null, "Can't start a TileAreaSplitter drag without a panel");

            if (panel == null)
                return;

            double currentExtent = panel.MinimizedAreaCurrentExtent;

            this._minDelta = panel.MinimizedAreaMinExtent - currentExtent;
            this._maxDelta = panel.MinimizedAreaMaxExtent - currentExtent;

            switch (panel.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
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

			Point screenPt = Infragistics.Windows.Utilities.PointToScreenSafe(this, new Point());





			double delta = TileAreaSplitter.GetOrientation(this) == Orientation.Vertical ? x - screenPt.X : y - screenPt.Y;
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
            if (this._tileManager == null)
                return;

            TileAreaPanel panel = this._tileManager.Panel;

            if (panel != null)
            {
				// JJD 1/5/12 - TFS97999
				// If the drag is complete then adjust the _lastScreenOrigin in case
				// another drag operation is started before the mouse is moved (which
				// would update the _lastScreenOrigin in OnMouseMove)
				if (isComplete)
				{
					// JJD 1/6/12 - TFS97999
					// Adjust for the zoom factor of the browser
					if (TileAreaSplitter.GetOrientation(this) == Orientation.Vertical)
						this._lastScreenOrigin.X += (delta * _zoomFactor * _zoomFactor);
					else
						this._lastScreenOrigin.Y += (delta * _zoomFactor * _zoomFactor);
				}

                switch (panel.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Top:
                        // flip the sign because the minimized area is after the
                        // maximize area a negative delta should increase the size 
                        // of the minimized area
                        delta = -delta;
                        break;
                }
                panel.MinimizedAreaExplicitExtent = panel.MinimizedAreaCurrentExtent + (delta * _zoomFactor);
            }
        }
		#endregion //ProcessDelta

        #endregion //Internal Methods

		#region Private Methods

        #region CreatePreviewControl
        private Control CreatePreviewControl()
        {
            // create the control that will render the preview
            Control ctrl = new ContentControl();
            ctrl.Style = this.PreviewStyle;
            ctrl.IsEnabled = false;

            // pass the orientation to the style in case they want to control the 
            // look based on the orientation
            ctrl.SetValue(TileAreaSplitter.OrientationProperty, GetOrientation(this));

            return ctrl;
        } 
        #endregion //CreatePreviewControl

 		#region OnDragCompleted
		private void OnDragCompleted(object sender, DragCompletedEventArgs e)
		{
			if (e.Canceled == false)
			{
				bool isVert = TileAreaSplitter.GetOrientation(this) == Orientation.Vertical;

				// AS 7/14/09 TFS18424
				// There's a bug in the splitter that the logic used to calculate the delta
				// from the mouse up of the thumb is different than in the mouse move. Since its 
				// possible that MS could fix this, I can't just invert the value. Instead we'll 
				// just use the last drag delta parameter for right to left.
				//
				//splitter.ProcessDragDelta(isVert ? e.HorizontalChange : e.VerticalChange, true);
				double delta = this.FlowDirection == FlowDirection.RightToLeft
					? this._lastDelta
					: isVert ? e.HorizontalChange : e.VerticalChange;

				this.ProcessDragDelta(delta, true);
			}

			this._lastReferencePoint = null;
			this._lastDelta = double.NaN;
		}
		#endregion //OnDragCompleted

		#region OnDragDelta
		private void OnDragDelta(object sender, DragDeltaEventArgs e)
		{

			bool isVert = TileAreaSplitter.GetOrientation(this) == Orientation.Vertical;
			this.ProcessDragDelta(isVert ? e.HorizontalChange : e.VerticalChange, false);
		}
		#endregion //OnDragDelta

		#region OnIsDraggingChanged
		/// <summary>
		/// Invoked when the <see cref="Thumb.IsDragging"/> property has been changed.
		/// </summary>
		private void OnIsDraggingChanged()
		{
			this.InitialDeltaRange();





			// handle the preview
			if (this.IsDragging)
			{
				if (this.ShowsPreview)
				{
					Debug.Assert(_popupManager == null);

					_popupManager = new PopupDragManager();

					_zoomFactor = _popupManager.ZoomFactor;

					Control popupContent = this.CreatePreviewControl();

					popupContent.SetBinding(WidthProperty, TileUtilities.CreateBinding(ActualWidthProperty, BindingMode.OneWay, this, typeof(FrameworkElement), "ActualWidthProperty"));
					popupContent.SetBinding(HeightProperty, TileUtilities.CreateBinding(ActualHeightProperty, BindingMode.OneWay, this, typeof(FrameworkElement), "ActualHeightProperty"));

					Point location = new Point(_lastScreenOrigin.X, _lastScreenOrigin.Y);

					_popupManager.Open(location, this, popupContent, null);

				}
				else
				{







				}
			}
			else
			{
				// remove the preview
				if (this._popupManager != null)
				{
					PopupDragManager popupMgr = _popupManager;
					_popupManager = null;
					popupMgr.Close();
				}
			}
		}
		
		#endregion //OnIsDraggingChanged

		#region ProcessDragDelta

		private void ProcessDragDelta(double delta, bool isComplete)
		{
			bool showsPreview = this.ShowsPreview;


			Point referencePoint = Infragistics.Windows.Utilities.PointFromScreenSafe(this, new Point());


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


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
			    bool isVertical = TileAreaSplitter.GetOrientation(this) == Orientation.Vertical;

				if (_popupManager != null)
				{
					Popup popup = _popupManager.Popup;
					if (popup != null)
					{
						// adjust for the zoom factor of the browser 
						if (isVertical)
							popup.HorizontalOffset = (_lastScreenOrigin.X / _zoomFactor) + (delta * _zoomFactor);
						else
							popup.VerticalOffset = (_lastScreenOrigin.Y / _zoomFactor) + (delta * _zoomFactor);
					}
				}
			}
			else
			    this.ProcessDelta(delta, isComplete);
		}
		#endregion //ProcessDragDelta

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