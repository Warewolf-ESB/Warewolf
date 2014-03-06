using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Layouts;
using System.Diagnostics;
using System.Collections.Generic;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Schedules.Primitives
{
	// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth

	internal interface IResizerBarHost
	{
		/// <summary>
		/// Returns the orientation of the resizer bar
		/// </summary>
		Orientation ResizerBarOrientation { get; }

		/// <summary>
		/// Returns a boolean indicating if the bar may be resized.
		/// </summary>
		/// <returns>True if a resize operation may begin and false if it may not.</returns>
		bool CanResize();

		/// <summary>
		/// Sets the extent to the specified value.
		/// </summary>
		/// <param name="extent">The new extent</param>
		void SetExtent( double extent );

		/// <summary>
		/// Returns the constraints for the resize operation.
		/// </summary>
		/// <returns>Returns null if the resize operation cannot be performed</returns>
		ResizeInfo GetResizeInfo();
	}

	internal interface IMultiResizerBarHost : IResizerBarHost
	{
		/// <summary>
		/// Returns the current resizing mode.
		/// </summary>
		ResizeMode? ResizeMode { get; }

		/// <summary>
		/// Returns the amount of the offset per pixel.
		/// </summary>
		double OffsetPerPixel { get; }
	}

	internal enum ResizeMode
	{
		Immediate,
		Deferred,
	}

	/// <summary>
	/// Provides information about a resize operation
	/// </summary>
	internal class ResizeInfo
	{
		#region Constructor
		/// <summary>
		/// Initializes the information for the resize operation.
		/// </summary>
		/// <param name="referenceElement">The element used to determine the relative position of the mouse to calculate the movement during the drag. This object should be stationary throughout the drag operation</param>
		/// <param name="actualExtent">The current extent from which the drag is starting</param>
		/// <param name="cancelExtent">The extent that should be set if the drag operation is cancelled. If null, the drag will just end and leave whatever value has been modified to up to the point at which the drag was cancelled</param>
		/// <param name="minimum">The minimum value for the resize operation</param>
		/// <param name="maximum">The maximum value for the resize operation</param>
		/// <param name="canIncreaseMinimum">True if a larger value may be used as the minimum. Elements that expand to fill may increase the value to try to keep the viewable area filled.</param>
		public ResizeInfo( UIElement referenceElement, double actualExtent, double? cancelExtent, double minimum = double.MinValue, double maximum = double.MaxValue, bool canIncreaseMinimum = true  )
		{
			CoreUtilities.ValidateNotNull(referenceElement, "referenceElement");
			ReferenceElement = referenceElement;
			ActualExtent = actualExtent;
			CancelExtent = cancelExtent;
			Minimum = minimum;
			Maximum = maximum;
			CanIncreaseMinimum = canIncreaseMinimum; // AS 5/4/11 TFS74447
		} 
		#endregion // Constructor

		#region Properties

		// AS 5/4/11 TFS74447
		/// <summary>
		/// Returns a boolean indicating if the minimum may be increased to ensure that the items fill the available area.
		/// </summary>
		public bool CanIncreaseMinimum { get; private set; }

		/// <summary>
		/// The value to set as the extent if the drag is cancelled.
		/// </summary>
		public double? CancelExtent { get; private set; }

		/// <summary>
		/// The value to use as the starting extent. This value will be used when compared with the mouse delta of the original position to the move position relative to the <see cref="ReferenceElement"/>
		/// </summary>
		public double ActualExtent { get; private set; }

		/// <summary>
		/// The minimum value for the drag operation
		/// </summary>
		public double Minimum { get; private set; }

		/// <summary>
		/// The maximum value for the drag operation
		/// </summary>
		public double Maximum { get; private set; }

		/// <summary>
		/// The element to use as the source for getting the mouse position. This element should not move during the drag operation.
		/// </summary>
		public UIElement ReferenceElement { get; private set; }

		#endregion // Properties
	}

	/// <summary>
	/// Custom element used to resize the header area within a <see cref="XamDayView"/> or <see cref="XamScheduleView"/>
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateNormal, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMouseOver, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDragging, GroupName = VisualStateUtilities.GroupDrag)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDraggingDeferred, GroupName = VisualStateUtilities.GroupDrag)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNotDragging, GroupName = VisualStateUtilities.GroupDrag)]
	public class ScheduleResizerBar : Control
	{
		#region Member Variables

		private MouseHelper _helper;
		private DragInfo _dragInfo;
		private IResizerBarHost _resizeHost;
		private bool _isMouseOver;

		#endregion // Member Variables

		#region Constructor
		static ScheduleResizerBar()
		{

			ScheduleResizerBar.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScheduleResizerBar), new FrameworkPropertyMetadata(typeof(ScheduleResizerBar)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ScheduleResizerBar), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleResizerBar"/>
		/// </summary>
		public ScheduleResizerBar()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.ChangeVisualState(false);
		}

		#endregion //OnApplyTemplate	

		#region OnLostMouseCapture
		/// <summary>
		/// Invoked when the element loses mouse capture.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnLostMouseCapture( MouseEventArgs e )
		{
			// if capture was lost then restore the original value
			this.EndDrag(true);

			base.OnLostMouseCapture(e);
		}
		#endregion // OnLostMouseCapture

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the element
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			_isMouseOver = true;
			this.UpdateVisualState();
		}
		#endregion // OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the element
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			_isMouseOver = false;
			this.UpdateVisualState();
		}
		#endregion // OnMouseLeave

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the event.</param>
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			if ( null == _helper )
				_helper = new MouseHelper(this);

			if ( _helper.OnMouseLeftButtonDown(e) )
			{
				if ( _helper.ClickCount == 1 && _dragInfo == null )
				{
					_dragInfo = this.CreateDragInfo(e);

					if ( _dragInfo != null )
					{
						// notify about the is resizing before we capture the mouse since that 
						// could result in a mouse move notification which would mean we would 
						// try to set an extent on the host before we have raised the IsResizingChanged
						this.OnIsResizingChanged();

						// once IsResizing is true we can get the offset per pixel but we need 
						// to do that before the capture mouse
						var hostEx = _resizeHost as IMultiResizerBarHost;
						_dragInfo.OffsetPerPixel = hostEx != null ? hostEx.OffsetPerPixel : 1;

						if (this.CaptureMouse())
						{
							// process initial move
							this.ProcessDrag(e);
						}
						else
						{
							this.EndDrag(true);
						}
					}
				}
				else if ( _helper.ClickCount == 2 )
				{
					_resizeHost.SetExtent(double.NaN);
				}
			}

			base.OnMouseLeftButtonDown(e);
		}
		#endregion // OnMouseLeftButtonDown

		#region OnMouseLeftButtonUp
		/// <summary>
		/// Invoked when the mouse is released over the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			this.EndDrag(false);

			base.OnMouseLeftButtonUp(e);
		}
		#endregion // OnMouseLeftButtonUp

		#region OnMouseMove
		/// <summary>
		/// Invoked when the mouse is moved during a drag operation.
		/// </summary>
		/// <param name="e">Provides information about the event.</param>
		protected override void OnMouseMove( MouseEventArgs e )
		{
			if ( _dragInfo != null )
			{
				this.ProcessDrag(e);
			}

			base.OnMouseMove(e);
		}
		#endregion // OnMouseMove

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(ScheduleResizerBar), new SolidColorBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state.
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(ScheduleResizerBar.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(ScheduleResizerBar.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#endregion //Public Properties

		#region Internal Properties

		#region CurrentResizeMode
		internal ResizeMode? CurrentResizeMode
		{
			get
			{
				var hostEx = _resizeHost as IMultiResizerBarHost;

				if (null != hostEx)
					return hostEx.ResizeMode;

				if (_dragInfo == null)
					return null;

				return ResizeMode.Immediate;
			}
		} 
		#endregion //CurrentResizeMode

		#region Host
		/// <summary>
		/// Returns or sets the object that will be resized.
		/// </summary>
		internal IResizerBarHost Host
		{
			get { return _resizeHost; }
			set
			{
				if ( value != _resizeHost )
				{
					this.EndDrag(true);
					_resizeHost = value;

					if ( null != _resizeHost )
					{
						switch ( _resizeHost.ResizerBarOrientation )
						{
							case Orientation.Horizontal:
								this.Cursor = Cursors.SizeNS;
								break;
							case Orientation.Vertical:
								this.Cursor = Cursors.SizeWE;
								break;
						}
					}
					else
					{
						this.ClearValue(CursorProperty);
					}
				}
			}
		}
		#endregion // Host

		#region IsResizing
		internal bool IsResizing
		{
			get { return _dragInfo != null; }
		}
		#endregion // IsResizing

		#endregion //Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region NotifyResizeModeChanged
		internal void NotifyResizeModeChanged()
		{
			this.UpdateVisualState();
		}
		#endregion //NotifyResizeModeChanged

		#endregion //Internal Methods

		#region Private Methods

		#region ChangeVisualState
		private void ChangeVisualState(bool useTransitions)
		{
			if (_isMouseOver)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

			ResizeMode? resizeMode = this.CurrentResizeMode;

			if (resizeMode == ResizeMode.Deferred)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDraggingDeferred, VisualStateUtilities.StateDragging, VisualStateUtilities.StateNotDragging);
			else if (resizeMode == ResizeMode.Immediate)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDragging, VisualStateUtilities.StateNotDragging);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNotDragging, useTransitions);

			this.SetProviderBrushes();
		}
		#endregion //ChangeVisualState

		#region CreateDragInfo
		private DragInfo CreateDragInfo( MouseEventArgs e )
		{
			if (_resizeHost != null && _resizeHost.CanResize())
			{
				var resizeInfo = _resizeHost.GetResizeInfo();

				if ( null != resizeInfo )
				{
					Debug.Assert(!double.IsNaN(resizeInfo.ActualExtent) && !double.IsInfinity(resizeInfo.ActualExtent), "Must have an actual starting double value");

					if ( !double.IsNaN(resizeInfo.ActualExtent) && !double.IsInfinity(resizeInfo.ActualExtent) )
					{
						var dragInfo = new DragInfo();

						dragInfo.ResizeInfo = resizeInfo;
						dragInfo.IsVerticalDrag = _resizeHost.ResizerBarOrientation == Orientation.Horizontal;
						dragInfo.OriginalPoint = e.GetPosition(resizeInfo.ReferenceElement);

						var keyTracker = new KeyEventTracker();
						keyTracker.KeyEvent += new KeyEventHandler(OnKeyTrackerKeyEvent);
						keyTracker.Activate();
						dragInfo.KeyTracker = keyTracker;

						return dragInfo;
					}
				}
			}

			return null;
		}
		#endregion // CreateDragInfo

		#region EndDrag
		private void EndDrag( bool cancel )
		{
			if ( _dragInfo == null )
				return;

			if ( _dragInfo != null )
			{
				var dragInfo = _dragInfo;
				_dragInfo = null;

				if ( dragInfo.KeyTracker != null )
				{
					dragInfo.KeyTracker.KeyEvent -= new KeyEventHandler(OnKeyTrackerKeyEvent);
					dragInfo.KeyTracker.Deactivate();
				}

				this.ReleaseMouseCapture();

				// if the operation was a no-op or returned to the original offset the cancel the operation
				if ( cancel || dragInfo.LastNewExtent == null || CoreUtilities.AreClose(dragInfo.LastNewExtent.Value, dragInfo.ResizeInfo.ActualExtent) )
				{
					double? restoreValue = dragInfo.ResizeInfo.CancelExtent;

					if ( null != restoreValue )
						_resizeHost.SetExtent(restoreValue.Value);
				}

				this.OnIsResizingChanged();
			}
		}
		#endregion // EndDrag

		#region OnIsResizingChanged
		private void OnIsResizingChanged()
		{
			var handler = this.IsResizingChanged;

			if ( null != handler )
				handler(this, EventArgs.Empty);
		}
		#endregion // OnIsResizingChanged

		#region OnKeyTrackerKeyEvent
		private void OnKeyTrackerKeyEvent( object sender, KeyEventArgs e )
		{
			e.Handled = true;

			if ( e.Key == Key.Escape )
			{
				this.EndDrag(true);
			}
		}
		#endregion // OnKeyTrackerKeyEvent

		#region ProcessDrag
		private void ProcessDrag( MouseEventArgs e )
		{
			if ( _dragInfo == null )
				return;

			Point currentPos = e.GetPosition(_dragInfo.ResizeInfo.ReferenceElement);

			double newExtent;

			if ( _dragInfo.IsVerticalDrag )
				newExtent = (currentPos.Y - _dragInfo.OriginalPoint.Y);
			else
				newExtent = (currentPos.X - _dragInfo.OriginalPoint.X);

			// when there are multiple resizer bars it may take multiple pixels 
			// of movement to cause a single pixel adjustment in the size
			newExtent /= _dragInfo.OffsetPerPixel;

			newExtent += _dragInfo.ResizeInfo.ActualExtent;

			newExtent = Math.Max(_dragInfo.ResizeInfo.Minimum, Math.Min(_dragInfo.ResizeInfo.Maximum, newExtent));

			_dragInfo.LastNewExtent = newExtent;
			_resizeHost.SetExtent(newExtent);
		}
		#endregion // ProcessDrag

		#region SetProviderBrushes
		private void SetProviderBrushes()
		{
			ScheduleControlBase ctrl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ctrl != null ? ctrl.DefaultBrushProvider : null;

			if (brushProvider == null)
				return;

			CalendarBrushProvider.SetBrushProvider(this, brushProvider);

			Brush br = this.CurrentResizeMode == ResizeMode.Deferred 
				? brushProvider.GetBrush(CalendarBrushId.ResizerBarPreviewBackground) 
				: ScheduleUtilities.GetBrush(Colors.Transparent);

			this.ComputedBackground = br;
		}
		#endregion //SetProviderBrushes

		#region UpdateVisualState
		private void UpdateVisualState()
		{
			this.ChangeVisualState(true);
		}
		#endregion // UpdateVisualState

		#endregion //Private Methods

		#endregion // Methods

		#region Events

		internal EventHandler IsResizingChanged; 

		#endregion // Events

		#region DragInfo class
		private class DragInfo
		{
			internal double? LastNewExtent;
			internal ResizeInfo ResizeInfo;
			internal Point OriginalPoint;
			internal bool IsVerticalDrag;
			internal KeyEventTracker KeyTracker;
			internal double OffsetPerPixel = 1d;
		} 
		#endregion // DragInfo class
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