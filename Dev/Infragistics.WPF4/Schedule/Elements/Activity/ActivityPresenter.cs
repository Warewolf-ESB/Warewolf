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
using System.Windows.Threading;
using Infragistics.AutomationPeers;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom control that represents a specific <see cref="ActivityBase"/> instance
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateNormal,		GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,	GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDisabled,		GroupName = VisualStateUtilities.GroupCommon)]
	
	[TemplateVisualState(Name = VisualStateUtilities.StateEditable,		GroupName = VisualStateUtilities.GroupEdit)]
	[TemplateVisualState(Name = VisualStateUtilities.StateUneditable,	GroupName = VisualStateUtilities.GroupEdit)]

	[TemplateVisualState(Name = VisualStateUtilities.StateDisplay,		GroupName = VisualStateUtilities.GroupInteraction)]
	[TemplateVisualState(Name = VisualStateUtilities.StateEditing,		GroupName = VisualStateUtilities.GroupInteraction)]

	[TemplateVisualState(Name = VisualStateUtilities.StateSelected,		GroupName = VisualStateUtilities.GroupSelection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateUnselected,	GroupName = VisualStateUtilities.GroupSelection)]
	
	[TemplatePart(Name = EditArea, Type = typeof(UIElement))]
	[TemplatePart(Name = Editor, Type = typeof(Control))]
	[TemplatePart(Name = BorderPath, Type = typeof(Path))] // JJD 3/21/11 - TFS65145 - added BorderPath part
	[DesignTimeVisible(false)]
	public class ActivityPresenter : Control
		, ICalendarBrushClient
		, IPropertyChangeListener
		, ISupportPropertyChangeNotifications
	{
		#region Member Variables
		
		internal const string EditArea = "EditArea";
		private const string Editor = "Editor";
		private const string BorderPath = "BorderPath"; // JJD 3/21/11 - TFS65145 - added BorderPath part
		private const double NotchWidth = 7.0;
		private const double DayEdgeMarginExtent = 8.0;
		internal const string CanResizeCriteria = "CanResizeCriteria";

		private InternalFlags _flags;
		private MouseHelper _mouseHelper;
		private long? _mouseDownTimeStamp;
		private Point? _mouseDownPoint;
		private DispatcherTimer _timer;
		private Control _editor;
		private UIElement _editArea;
		private LostFocusTracker _tracker;
		private Path _borderPath; // JJD 3/21/11 - TFS65145 - added BorderPath part
		private double _notchOffset;
		private double _notchExtent;
		private ActivityBase _activity;
		private object _listeners;
		private ScheduleControlBase _control;
		private bool _isMouseOver;

		// JJD 5/10/11 - TFS74042 - added
		private static Brush IndicatorForeground_Normal;
		private static Brush IndicatorForeground_White;
		
		// JJD 11/4/11 - TFS74042 - added
		private static Brush OutOfRangeIndicatorForeground_Normal;
		private static Brush OutOfRangeIndicatorForeground_White;
		private static Brush OutOfRangeIndicatorBackground_Normal;
		private static Brush OutOfRangeIndicatorBackground_White;

		// AS 5/9/12 TFS104555
		private Size _lastArrangeSize = Size.Empty;
		private Size _lastBorderGeometrySize = Size.Empty; 

		#endregion //Member Variables

		#region Constructor
		static ActivityPresenter()
		{
			// JJD 5/10/11 - TFS74042 - added
			// Create 2 brushed to use for the indicators
			IndicatorForeground_White = new SolidColorBrush(Colors.White);
			
			LinearGradientBrush brush = new LinearGradientBrush();
			brush.StartPoint = new Point(.5, 0);
			brush.EndPoint = new Point(.5, 1);
			GradientStop stop = new GradientStop();
			stop.Color = Color.FromArgb(255, 102, 102, 102);
			stop.Offset = 0;
			brush.GradientStops.Add( stop );
			stop = new GradientStop();
			stop.Color = Color.FromArgb(255, 64, 64, 64);
			stop.Offset = .994;
			brush.GradientStops.Add( stop );
			IndicatorForeground_Normal = brush;

			// JJD 11/4/11 - TFS74042 - added
			// Create 4 more brushed to use for the out of range indicators
			OutOfRangeIndicatorForeground_White = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
			OutOfRangeIndicatorForeground_Normal = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64));

			brush = new LinearGradientBrush();
			brush.StartPoint = new Point(0.571, 0.2);
			brush.EndPoint = new Point(0.570, 0.814);
			stop = new GradientStop();
			stop.Color = Colors.White;
			stop.Offset = 0;
			brush.GradientStops.Add( stop );
			stop = new GradientStop();
			stop.Color = Color.FromArgb(255, 200, 200, 200);
			stop.Offset = .994;
			brush.GradientStops.Add( stop );
			OutOfRangeIndicatorBackground_White = brush;
			
			brush = new LinearGradientBrush();
			brush.StartPoint = new Point(0.571, 0.2);
			brush.EndPoint = new Point(0.570, 0.814);
			stop = new GradientStop();
			stop.Color = Color.FromArgb(255, 153, 153, 153);
			stop.Offset = 0;
			brush.GradientStops.Add( stop );
			stop = new GradientStop();
			stop.Color = Color.FromArgb(255, 51, 51, 51);
			stop.Offset = .994;
			brush.GradientStops.Add( stop );
			OutOfRangeIndicatorBackground_Normal = brush;

			// JJD 5/10/11 - TFS74042 
			// freeze the brushes created above
			IndicatorForeground_Normal.Freeze();
			IndicatorForeground_White.Freeze();
			
			// JJD 11/4/11 - TFS74042 
			// freeze the brushes created above
			OutOfRangeIndicatorForeground_Normal.Freeze();
			OutOfRangeIndicatorForeground_White.Freeze();
			OutOfRangeIndicatorBackground_Normal.Freeze();
			OutOfRangeIndicatorBackground_White.Freeze();

			// JJD 5/10/11 - TFS74042 - added
			UIElement.IsEnabledProperty.OverrideMetadata(typeof(ActivityPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsEnabledPropertyChanged)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ActivityPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923


			// JJD 5/10/11 - TFS74042 
			// Now that we have created the default brush above we can register the ComputedIndicatorForeground property
			ComputedIndicatorForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedIndicatorForeground",
						typeof(Brush), typeof(ActivityPresenter), IndicatorForeground_Normal, null);
			ComputedIndicatorForegroundProperty = ComputedIndicatorForegroundPropertyKey.DependencyProperty;

			// JJD 11/4/11 - TFS74042 
			// Now that we have created the default brush above we can register the OutOfRangeIndicatorBackground 
			// and OutOfRangeIndicatorForeground properties
			OutOfRangeIndicatorBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("OutOfRangeIndicatorBackground",
						typeof(Brush), typeof(ActivityPresenter), OutOfRangeIndicatorBackground_Normal, null);
			OutOfRangeIndicatorBackgroundProperty = OutOfRangeIndicatorBackgroundPropertyKey.DependencyProperty;

			OutOfRangeIndicatorForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("OutOfRangeIndicatorForeground",
						typeof(Brush), typeof(ActivityPresenter), OutOfRangeIndicatorForeground_Normal, null);
			OutOfRangeIndicatorForegroundProperty = OutOfRangeIndicatorForegroundPropertyKey.DependencyProperty;

		}

		/// <summary>
		/// Initializes a new <see cref="ActivityPresenter"/>
		/// </summary>
		public ActivityPresenter()
		{



			this.ToolTipInfo = new ActivityToolTipInfo(this);

		}

		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			// AS 5/9/12 TFS104555
			// We may get arranged with more space then we had during the measure (or more likely 
			// we didn't know the size of the element in the measure) so we need to fix up the 
			// border. We'll only do that if the size differs from the border geometry size.
			//
			_lastArrangeSize = finalSize;

			if (!this.IsSingleLineDisplay)
			{
				if (_lastBorderGeometrySize.IsEmpty || !CoreUtilities.AreClose(finalSize, _lastBorderGeometrySize))
					this.VerifyBorderGeometry(finalSize);
			}

			return base.ArrangeOverride(finalSize);
		}
		#endregion //ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			// AS 5/9/12 TFS104555
			// If we're measured with infinity then we need some size to use for the border size.
			//
			//this.VerifyBorderGeometry(constraint);
			Size borderSize = constraint;
			if (!this.IsSingleLineDisplay && !_lastArrangeSize.IsEmpty)
			{
				if (double.IsPositiveInfinity(borderSize.Width))
					borderSize.Width = _lastArrangeSize.Width;
				if (double.IsPositiveInfinity(borderSize.Height))
					borderSize.Height = _lastArrangeSize.Height;
			}
			this.VerifyBorderGeometry(borderSize);

			// AS 10/1/10 TFS50023
			// If the criteria that is used for whether resizing is allowed or not 
			// has changed then send a change notification so the resizer listening
			// can update its state.
			//
			if ( this.GetFlag(InternalFlags.IsNearOrFarInPanelChanged) )
			{
				this.SetFlag(InternalFlags.IsNearOrFarInPanelChanged, false);
				this.RaiseChangeNotifications(CanResizeCriteria, null);
			}

			return base.MeasureOverride(constraint);
		}

		#endregion //MeasureOverride	
    
		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			
			ScheduleControlBase control = this.Control;

			if (control != null)
			{
				if (control is ScheduleTimeControlBase)
					this.SetValue(IndicatorAreaVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
			}

			_editor = this.GetTemplateChild(Editor) as Control;
			_editArea = this.GetTemplateChild(EditArea) as UIElement;

			// JJD 3/21/11 - TFS65145 - added BorderPath part
			_borderPath = this.GetTemplateChild(BorderPath) as Path;

			this.ChangeVisualState(false);
		}

		#endregion //OnApplyTemplate	

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ActivityPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="ActivityPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new ActivityPresenterAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (this.IsInEditMode)
			{
				switch (PresentationUtilities.GetKey(e))
				{
					case Key.Escape:
						this.EndEdit(true, true);
						e.Handled = true;
						break;
					case Key.Enter:
						this.EndEdit(false, false);
						e.Handled = true;
						break;
				}
			}


			base.OnKeyDown(e);
		}
		
		#endregion //OnKeyDown

		#region OnLostMouseCapture

		/// <summary>
		/// Called when mouse capture has been lost
		/// </summary>
		/// <param name="e">The mouse event args</param>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);

			this._mouseDownPoint = null;
		}

		#endregion //OnLostMouseCapture	

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
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			this.StopEditTimer();

			ScheduleControlBase control = this.Control;

			if (this.MouseHelper.OnMouseLeftButtonDown(e))
			{
				if (this.MouseHelper.ClickCount == 2)
				{
					this.ReleaseMouseCapture();

					// AS 3/7/12 TFS102945
					// Defer processing the double click until we know we had a click.
					//
					if (control == null || !control.ShouldQueueTouchActions)
					{
						// AS 3/23/12 TFS105953
						// See a similar change in TimeRangePresenterBase for details.
						//
						if (e.StylusDevice != null)
						{

							this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action( () => { this.OnProcessDoubleClick(e); }));
							e.Handled = true;
							return;

						}

						this.OnProcessDoubleClick(e);
					}
					else
					{
						e.Handled = true;
						control.EnqueueTouchAction((a) => { this.OnProcessDoubleClick(e); });
					}

					if (e.Handled)
					{
						this._mouseDownPoint = null;
						this._mouseDownTimeStamp = null;
						return;
					}
				}
			}

			bool wasSelected = this.IsSelected;

			// make sure the associated calendar gets selected
			if (control != null)
			{
				// AS 3/7/12 TFS102945
				if (control.ShouldQueueTouchActions)
				{
					control.EnqueueTouchAction((a) => { this.ProcessDeferredDown(a, e); });
					e.Handled = true;
					return;
				}

				// AS 9/29/10 TFS49543
				// Added if block. We don't want to take focus while in edit mode or else 
				// we will end the edit operation.
				//
				if (!this.IsInEditMode)
					control.Focus();
				else
				{
					// JJD 4/6/11 - TFS61001
					// While in edit mode if the user presses the left mouse down outside the edit area we want to exit edit mode and commit the changes 
					if (_editArea != null)
					{
						Point pt = e.GetPosition(this._editArea);

						bool isMouseOverEditArea = pt.X >= 0 && pt.Y >= 0 && pt.X <= this._editArea.RenderSize.Width && pt.Y <= this._editArea.RenderSize.Height;
						
						// JJD 11/10/11 - TFS77428
						// If we are over the edit area see if the edit area has a TextBox for editing
						// and make sure the mouse is not above or below the textbox. If it is
						// then consider the mouse not to be over the edit area so we will end edit below.
						if ( isMouseOverEditArea )
						{
							TextBox textBox = PresentationUtilities.GetVisualDescendant<TextBox>(_editArea, null);

							if (textBox != null)
							{
								Point ptOverTextBox = e.GetPosition(textBox);

								isMouseOverEditArea = pt.Y >= 0 && pt.Y <= textBox.RenderSize.Height;
							}
						}

						if (!isMouseOverEditArea)
						{
							this.EndEdit(false, false);
							e.Handled = true;
							return;
						}
					}
				}

				ResourceCalendar calendar = this._activity != null ? this._activity.OwningCalendar : null;

				if (calendar != null)
				{
					control.SelectCalendar(calendar, true, this);

					bool toggle = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

					int index =   control.SelectedActivities.IndexOf(this._activity);

					if (toggle && index >= 0)
						control.SelectedActivities.RemoveAt(index);
					else
					{
						if (index < 0)
							control.SelectActivity(this._activity, toggle);
					}
				}
			}

			// JJD 9/1/10 - TFS37547
			// Fist clear the time stamp and don't set it for entering edit mode on the MouseUp 
			// unless the activity was already selected and the Ctrl key is not down
			this._mouseDownTimeStamp = null;

			if (this._editor != null && wasSelected && ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) &&
				this.IsInEditMode == false && this.CanEditInPlace)
			{
				bool isMouseOverEditArea = false;

				if (this._editArea != null)
				{
					Point pt = e.GetPosition(this._editArea);

					isMouseOverEditArea = pt.X >=0 && pt.Y >=0 && pt.X <= this._editArea.RenderSize.Width && pt.Y <= this._editArea.RenderSize.Height;

					if (isMouseOverEditArea)
					{
						// Now check to make sure the point is over the editor portion of the edit area vertically only
						pt = e.GetPosition(this._editor);
					
						isMouseOverEditArea = pt.Y >=0 && pt.Y <= this._editArea.RenderSize.Height;

					}
				}

				this._mouseDownTimeStamp = isMouseOverEditArea ? DateTime.Now.Ticks : (long?)null;

				if (isMouseOverEditArea)
					e.Handled = true;
			}

			// if we can drag capture the mouse and save the original point.
			// We will start the drag operation in OnMouseMove once the mouse has moved outside of the
			// threshold
			if (this.CanDrag)
			{
				this._mouseDownPoint = e.GetPosition(this);

				if (!this.CaptureMouse())
					_mouseDownPoint = null;

				e.Handled = true;
			}

			base.OnMouseLeftButtonDown(e);
		}

		#endregion //OnMouseLeftButtonDown

		#region OnMouseLeftButtonUp

		/// <summary>
		/// Invoked when the left mouse button is released.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			this.EndMouseCapture();

			if (this._mouseDownTimeStamp == null)
			{
				base.OnMouseLeftButtonUp(e);
				return;
			}

			long delta = MouseHelper.DoubleClickTime + this._mouseDownTimeStamp.Value - DateTime.Now.Ticks;

			this._mouseDownTimeStamp = null;

			if (delta <= 0)
				this.BeginEdit();
			else
			{
				this.StartEditTimer(delta);
			}

			base.OnMouseLeftButtonUp(e);
		}

		#endregion //OnMouseLeftButtonUp

		#region OnMouseMove

		/// <summary>
		/// Called when the mouse has noved
		/// </summary>
		/// <param name="e">The mouse event args</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (this._mouseDownPoint.HasValue)
			{
				Point pt = e.GetPosition(this);
				Point mouseDownPoint = this._mouseDownPoint.Value;

				if (Math.Abs(pt.X - mouseDownPoint.X) > MouseHelper.DoubleClickSizeX ||
					Math.Abs(pt.Y - mouseDownPoint.Y) > MouseHelper.DoubleClickSizeY)
				{
					this.ReleaseMouseCapture();

					this.InitiateDrag(mouseDownPoint);
				}

				e.Handled = true;

			}
		}

		#endregion //OnMouseMove	
    
		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region Activity

		/// <summary>
		/// Identifies the <see cref="Activity"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityProperty = DependencyProperty.Register("Activity",
			typeof(ActivityBase), typeof(ActivityPresenter),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnActivityChanged))
			);

		private static void OnActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter item = (ActivityPresenter)d;

			// AS 1/5/11 NA 11.1 Activity Categories
			bool wasInitializing = item.IsInitializing;
			item.IsInitializing = true;

			item._activity = e.NewValue as ActivityBase;

			if (item._activity != null && item._activity.OwningCalendar != null)
			{
				item._activity.OwningCalendar.BindToBrushVerion(item);
				item.UpdateVisualState();
			}
			else
				item.ClearValue(ScheduleControlBase.BrushVersionProperty);

			if (item._activity != null)
				item.ToolTipInfo.Activity = item._activity;
			else
				item.ToolTipInfo.ClearValue(ActivityToolTipInfo.ActivityPropertyKey);

			item.OnActivityChanged(e.OldValue as ActivityBase, item._activity);

			// AS 1/5/11 NA 11.1 Activity Categories
			if ( !wasInitializing )
				item.IsInitializing = false;
			
			item.RaiseChangeNotifications("Activity", null);

		}

		/// <summary>
		/// Invoked when the associated activity has changed.
		/// </summary>
		/// <param name="oldValue">Old activity</param>
		/// <param name="newValue">New activity</param>
		protected virtual void OnActivityChanged(ActivityBase oldValue, ActivityBase newValue)
		{
			if (oldValue != null)
			{
				ScheduleUtilities.RemoveListener(oldValue, this);

				if ( oldValue.RootActivity != null )
					ScheduleUtilities.RemoveListener(oldValue.RootActivity, this);
			}

			if (newValue != null)
			{
				ScheduleUtilities.AddListener(newValue, this, true);

				if ( newValue.RootActivity != null )
					ScheduleUtilities.AddListener(newValue.RootActivity, this, true);
			}

			// AS 1/5/11 NA 11.1 Activity Categories
			this.InitializeCategories();

			if (null != newValue)
			{
				this.InitializeStartEndLocal();
				this.InitializeHasPendingOperation();
				this.InitializeError();
				this.InitializeReminderVisibility();
			}
		}

		/// <summary>
		/// Returns or sets the activity associated with a given element.
		/// </summary>
		/// <seealso cref="ActivityProperty"/>
		public ActivityBase Activity
		{
			get
			{
				return _activity;
			}
			set
			{
				this.SetValue(ActivityPresenter.ActivityProperty, value);
			}
		}

		#endregion //Activity

		#region AdditionalText

		internal static readonly DependencyPropertyKey AdditionalTextPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AdditionalText",
			typeof(string), typeof(ActivityPresenter), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="AdditionalText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AdditionalTextProperty = AdditionalTextPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns additional text for dispslay (read-only)
		/// </summary>
		/// <seealso cref="AdditionalTextProperty"/>
		public string AdditionalText
		{
			get
			{
				return (string)this.GetValue(ActivityPresenter.AdditionalTextProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.AdditionalTextPropertyKey, value);
			}
		}

		#endregion //AdditionalText

		#region AdditionalTextVisibility

		internal static readonly DependencyPropertyKey AdditionalTextVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AdditionalTextVisibility",
			typeof(Visibility), typeof(ActivityPresenter),
			KnownBoxes.VisibilityCollapsedBox,
			null
			);

		/// <summary>
		/// Identifies the read-only <see cref="AdditionalTextVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AdditionalTextVisibilityProperty = AdditionalTextVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the AdditionalText field (read-only)
		/// </summary>
		/// <seealso cref="AdditionalTextVisibilityProperty"/>
		public Visibility AdditionalTextVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityPresenter.AdditionalTextVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.AdditionalTextVisibilityPropertyKey, value);
			}
		}

		#endregion //AdditionalTextVisibility

		// AS 1/5/11 NA 11.1 Activity Categories
		#region Categories

		private static readonly DependencyPropertyKey CategoriesPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Categories",
			typeof(IEnumerable<ActivityCategory>), typeof(ActivityPresenter),
			null,
			new PropertyChangedCallback(OnCategoriesChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="Categories"/> dependency property
		/// </summary>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public static readonly DependencyProperty CategoriesProperty = CategoriesPropertyKey.DependencyProperty;

		private static void OnCategoriesChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ActivityPresenter instance = (ActivityPresenter)d;

			var oldNotifier = e.OldValue as ISupportPropertyChangeNotifications;
			var newNotifier = e.NewValue as ISupportPropertyChangeNotifications;

			if ( oldNotifier != null )
				oldNotifier.RemoveListener(instance);

			if ( newNotifier != null )
				newNotifier.AddListener(instance, true);

			instance.SetProviderBrushes();
		}

		/// <summary>
		/// Returns the categories associated with the <see cref="Activity"/> represented by the element.
		/// </summary>
		/// <seealso cref="CategoriesProperty"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public IEnumerable<ActivityCategory> Categories
		{
			get
			{
				return (IEnumerable<ActivityCategory>)this.GetValue(ActivityPresenter.CategoriesProperty);
			}
			private set
			{
				this.SetValue(ActivityPresenter.CategoriesPropertyKey, value);
			}
		}

		#endregion //Categories

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(ActivityPresenter), ScheduleUtilities.GetBrush(Colors.White), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region ComputedBorderBrush

		private static readonly DependencyPropertyKey ComputedBorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderBrush",
			typeof(Brush), typeof(ActivityPresenter), ScheduleUtilities.GetBrush(Colors.Black), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderBrushProperty = ComputedBorderBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBorderBrushProperty"/>
		public Brush ComputedBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.ComputedBorderBrushProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedBorderBrushPropertyKey, value);
			}
		}

		#endregion //ComputedBorderBrush

		#region ComputedBorderStrokeThickness

		private static readonly DependencyPropertyKey ComputedBorderStrokeThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderStrokeThickness",
			typeof(double), typeof(ActivityPresenter), 1.0d, new PropertyChangedCallback(OnComputedBorderStrokeThicknessChanged));

		private static void OnComputedBorderStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter item = (ActivityPresenter)d;

			// JJD 10/28/20 - TFS58313.
			// Invalidate the measure to refresh the display
			item.InvalidateMeasure();
		}

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderStrokeThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderStrokeThicknessProperty = ComputedBorderStrokeThicknessPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the stroke thickness for the border around the element (read-only)
		/// </summary>
		/// <seealso cref="ComputedBorderStrokeThicknessProperty"/>
		public double ComputedBorderStrokeThickness
		{
			get
			{
				return (double)this.GetValue(ActivityPresenter.ComputedBorderStrokeThicknessProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedBorderStrokeThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedBorderStrokeThickness

		#region ComputedContentHorizontalAlignment

		private static readonly DependencyPropertyKey ComputedContentHorizontalAlignmentPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedContentHorizontalAlignment",
			typeof(HorizontalAlignment), typeof(ActivityPresenter), KnownBoxes.HorizontalAlignmentLeftBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedContentHorizontalAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedContentHorizontalAlignmentProperty = ComputedContentHorizontalAlignmentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the horizontal alignment for the content (read-only)
		/// </summary>
		/// <seealso cref="ComputedContentHorizontalAlignmentProperty"/>
		public HorizontalAlignment ComputedContentHorizontalAlignment
		{
			get
			{
				return (HorizontalAlignment)this.GetValue(ActivityPresenter.ComputedContentHorizontalAlignmentProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedContentHorizontalAlignmentPropertyKey, value);
			}
		}

		#endregion //ComputedContentHorizontalAlignment
		
		#region ComputedContentMargin

		private static readonly DependencyPropertyKey ComputedContentMarginPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedContentMargin",
			typeof(Thickness), typeof(ActivityPresenter), new Thickness(3), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedContentMargin"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedContentMarginProperty = ComputedContentMarginPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the margin around the content area
		/// </summary>
		/// <seealso cref="ComputedContentMarginProperty"/>
		public Thickness ComputedContentMargin
		{
			get
			{
				return (Thickness)this.GetValue(ActivityPresenter.ComputedContentMarginProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedContentMarginPropertyKey, value);
			}
		}

		#endregion //ComputedContentMargin

		#region ComputedDateTimeForeground

		private static readonly DependencyPropertyKey ComputedDateTimeForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedDateTimeForeground",
			typeof(Brush), typeof(ActivityPresenter), ScheduleUtilities.GetBrush(Colors.Black), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedDateTimeForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedDateTimeForegroundProperty = ComputedDateTimeForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground or date time (prefix/suffix) elements based on the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedDateTimeForegroundProperty"/>
		public Brush ComputedDateTimeForeground
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.ComputedDateTimeForegroundProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedDateTimeForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedDateTimeForeground

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(ActivityPresenter), ScheduleUtilities.GetBrush(Colors.Black), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedForegroundProperty = ComputedForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		public Brush ComputedForeground
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region ComputedGeometry

		private static readonly DependencyPropertyKey ComputedGeometryPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedGeometry",
			typeof(Geometry), typeof(ActivityPresenter), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedGeometry"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedGeometryProperty = ComputedGeometryPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a geometry for the activity that takes into account a possible notch for appointment that don't take up at least 1 timeslot (read-only)
		/// </summary>
		/// <seealso cref="ComputedGeometryProperty"/>
		public Geometry ComputedGeometry
		{
			get
			{
				return (Geometry)this.GetValue(ActivityPresenter.ComputedGeometryProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedGeometryPropertyKey, value);
			}
		}

		#endregion //ComputedGeometry

		// JJD 5/10/11 - TFS74042 - added
		#region ComputedIndicatorForeground

		private static readonly DependencyPropertyKey ComputedIndicatorForegroundPropertyKey;
		//private static readonly DependencyPropertyKey ComputedIndicatorForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedIndicatorForeground",
		//    typeof(Brush), typeof(ActivityPresenter), IndicatorForeground_Normal, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedIndicatorForeground"/> dependency property
		/// </summary>
		//public static readonly DependencyProperty ComputedIndicatorForegroundProperty = ComputedIndicatorForegroundPropertyKey.DependencyProperty;
		public static readonly DependencyProperty ComputedIndicatorForegroundProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedIndicatorForegroundProperty"/>
		public Brush ComputedIndicatorForeground
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.ComputedIndicatorForegroundProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ComputedIndicatorForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedIndicatorForeground

		#region EndLocal

		private static readonly DependencyPropertyKey EndLocalPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("EndLocal",
			typeof(DateTime), typeof(ActivityPresenter), DateTime.Today, null);

		/// <summary>
		/// Identifies the read-only <see cref="EndLocal"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndLocalProperty = EndLocalPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="ActivityBase.End"/> in local time.
		/// </summary>
		/// <seealso cref="EndLocalProperty"/>
		public DateTime EndLocal
		{
			get
			{
				return (DateTime)this.GetValue(ActivityPresenter.EndLocalProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.EndLocalPropertyKey, value);
			}
		}

		#endregion //EndLocal

		#region EndOutOfRangeIndicatorVisibility

		private static readonly DependencyPropertyKey EndOutOfRangeIndicatorVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("EndOutOfRangeIndicatorVisibility",
			typeof(Visibility), typeof(ActivityPresenter), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="EndOutOfRangeIndicatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndOutOfRangeIndicatorVisibilityProperty = EndOutOfRangeIndicatorVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the End date out of reange indicator (read-only)
		/// </summary>
		/// <seealso cref="EndOutOfRangeIndicatorVisibilityProperty"/>
		public Visibility EndOutOfRangeIndicatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityPresenter.EndOutOfRangeIndicatorVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.EndOutOfRangeIndicatorVisibilityPropertyKey, value);
			}
		}

		#endregion //EndOutOfRangeIndicatorVisibility

		#region HasPendingOperation

		private static readonly DependencyPropertyKey HasPendingOperationPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasPendingOperation",
			typeof(bool), typeof(ActivityPresenter), KnownBoxes.FalseBox, new PropertyChangedCallback(OnHasPendingOperationChanged));

		private static void OnHasPendingOperationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter item = (ActivityPresenter)d;
			item.RaiseChangeNotifications("HasPendingOperation", e);
		}

		/// <summary>
		/// Identifies the read-only <see cref="HasPendingOperation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasPendingOperationProperty = HasPendingOperationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether there is an operation pending (read-only)
		/// </summary>
		/// <seealso cref="HasPendingOperationProperty"/>
		public bool HasPendingOperation
		{
			get
			{
				return (bool)this.GetValue(ActivityPresenter.HasPendingOperationProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.HasPendingOperationPropertyKey, value);
			}
		}

		#endregion //HasPendingOperation

		#region IndicatorAreaVisibility

		private static readonly DependencyPropertyKey IndicatorAreaVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IndicatorAreaVisibility",
			typeof(Visibility), typeof(ActivityPresenter), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IndicatorAreaVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IndicatorAreaVisibilityProperty = IndicatorAreaVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns th visibility of the indicator area (read-only);
		/// </summary>
		/// <seealso cref="IndicatorAreaVisibilityProperty"/>
		public Visibility IndicatorAreaVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityPresenter.IndicatorAreaVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.IndicatorAreaVisibilityPropertyKey, value);
			}
		}

		#endregion //IndicatorAreaVisibility

		#region IsInEditMode

		private static readonly DependencyPropertyKey IsInEditModePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsInEditMode",
			typeof(bool), typeof(ActivityPresenter),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsInEditModeChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsInEditMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInEditModeProperty = IsInEditModePropertyKey.DependencyProperty;

		private static void OnIsInEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter instance = (ActivityPresenter)d;

			instance.SetProviderBrushes();

			if (true == (bool)e.NewValue)
			{
				if (instance._editor != null)
					instance.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(instance.FocusEditSite));
			}
			else
			{
				if (instance._editor != null)
				{
					if (instance._tracker != null)
					{
						instance._tracker.Deactivate(true);
						instance._tracker = null;
					}

					// do not shift the focus. this could be called during the handling 
					// of lost keyboard focus which will be before the logical focus has 
					// been shifted which is what HasFocus changes.
					//if (PresentationUtilities.HasFocus(instance._editor))
					//{
					//    instance.Focus();
					//}
				}
			}

			instance.UpdateVisualState();
		}

		/// <summary>
		/// Determines if this activity is being edited in place
		/// </summary>
		/// <seealso cref="IsInEditModeProperty"/>
		public bool IsInEditMode
		{
			get
			{
				return (bool)this.GetValue(ActivityPresenter.IsInEditModeProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.IsInEditModePropertyKey, value);
			}
		}

		#endregion //IsInEditMode

		#region IsSelected

		private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelected",
			typeof(bool), typeof(ActivityPresenter),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsSelectedChanged)
			);

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter item = (ActivityPresenter)d;
			item.SetFlag(InternalFlags.IsSelected, (bool)e.NewValue);
			item.UpdateVisualState();

			item.RaiseChangeNotifications("IsSelected", null);
		}

		/// <summary>
		/// Returns a boolean indicating if the object is currently selected.
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		public bool IsSelected
		{
			get
			{
				return this.GetFlag(InternalFlags.IsSelected);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.IsSelectedPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsSelected

		#region IsSingleLineDisplay

		private static readonly DependencyPropertyKey IsSingleLineDisplayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSingleLineDisplay",
			typeof(bool), typeof(ActivityPresenter), KnownBoxes.FalseBox, OnIsSingleLineDisplayChanged);

		/// <summary>
		/// Identifies the read-only <see cref="IsSingleLineDisplay"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSingleLineDisplayProperty = IsSingleLineDisplayPropertyKey.DependencyProperty;

		private static void OnIsSingleLineDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter instance = (ActivityPresenter)d;

			instance.SetProviderBrushes();
		}

		/// <summary>
		/// Returns true if this element should display its contents in a single line (read-only)
		/// </summary>
		/// <seealso cref="IsSingleLineDisplayProperty"/>
		public bool IsSingleLineDisplay
		{
			get
			{
				return (bool)this.GetValue(ActivityPresenter.IsSingleLineDisplayProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.IsSingleLineDisplayPropertyKey, value);
			}
		}

		#endregion //IsSingleLineDisplay

		// JJD 11/4/11 - TFS74042 - added
		#region OutOfRangeIndicatorBackground

		private static readonly DependencyPropertyKey OutOfRangeIndicatorBackgroundPropertyKey;

		/// <summary>
		/// Identifies the read-only <see cref="OutOfRangeIndicatorBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OutOfRangeIndicatorBackgroundProperty;

		/// <summary>
		/// Returns the brush to use for the Background of the indicator for the 'Start out of range' or the 'End out of range' states./>
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		/// <seealso cref="StartOutOfRangeIndicatorVisibility"/>
		/// <seealso cref="EndOutOfRangeIndicatorVisibility"/>
		public Brush OutOfRangeIndicatorBackground
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.OutOfRangeIndicatorBackgroundProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.OutOfRangeIndicatorBackgroundPropertyKey, value);
			}
		}

		#endregion //OutOfRangeIndicatorBackground

		// JJD 11/4/11 - TFS74042 - added
		#region OutOfRangeIndicatorForeground

		private static readonly DependencyPropertyKey OutOfRangeIndicatorForegroundPropertyKey;

		/// <summary>
		/// Identifies the read-only <see cref="OutOfRangeIndicatorForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OutOfRangeIndicatorForegroundProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground of the indicator for the 'Start out of range' or the 'End out of range' states./>
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		/// <seealso cref="StartOutOfRangeIndicatorVisibility"/>
		/// <seealso cref="EndOutOfRangeIndicatorVisibility"/>
		public Brush OutOfRangeIndicatorForeground
		{
			get
			{
				return (Brush)this.GetValue(ActivityPresenter.OutOfRangeIndicatorForegroundProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.OutOfRangeIndicatorForegroundPropertyKey, value);
			}
		}

		#endregion //OutOfRangeIndicatorForeground

		#region PrefixFormatType

		private static readonly DependencyPropertyKey PrefixFormatTypePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("PrefixFormatType",
			typeof(DateRangeFormatType), typeof(ActivityPresenter), DateRangeFormatType.None, null);

		/// <summary>
		/// Identifies the read-only <see cref="PrefixFormatType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PrefixFormatTypeProperty = PrefixFormatTypePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns and enumeration that determines how the activity prefix will be formatted.
		/// </summary>
		/// <seealso cref="PrefixFormatTypeProperty"/>
		public DateRangeFormatType PrefixFormatType
		{
			get
			{
				return (DateRangeFormatType)this.GetValue(ActivityPresenter.PrefixFormatTypeProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.PrefixFormatTypePropertyKey, value);
			}
		}

		#endregion //PrefixFormatType

		#region ReminderVisibility

		private static readonly DependencyPropertyKey ReminderVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ReminderVisibility",
			typeof(Visibility), typeof(ActivityPresenter), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ReminderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ReminderVisibilityProperty = ReminderVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating the visibility of the reminder indicator.
		/// </summary>
		/// <remarks>
		/// <p class="body">Reminders are only active for the calendars associated with the <see cref="XamScheduleDataManager.CurrentUser"/>. This property 
		/// considers the <see cref="ActivityBase.OwningResource"/> and the <see cref="ActivityBase.ReminderEnabled"/> state to determine whether the reminder 
		/// is active.</p>
		/// </remarks>
		/// <seealso cref="ReminderVisibilityProperty"/>
		public Visibility ReminderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityPresenter.ReminderVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ReminderVisibilityPropertyKey, value);
			}
		}

		#endregion //ReminderVisibility

		#region SeparatorVisibility

		internal static readonly DependencyPropertyKey SeparatorVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SeparatorVisibility",
			typeof(Visibility), typeof(ActivityPresenter), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="SeparatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SeparatorVisibilityProperty = SeparatorVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of th separator text between the sunject and the AdditionalText (read-only)
		/// </summary>
		/// <seealso cref="SeparatorVisibilityProperty"/>
		public Visibility SeparatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityPresenter.SeparatorVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.SeparatorVisibilityPropertyKey, value);
			}
		}

		#endregion //SeparatorVisibility

		#region StartLocal

		private static readonly DependencyPropertyKey StartLocalPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("StartLocal",
			typeof(DateTime), typeof(ActivityPresenter), DateTime.Today, null);

		/// <summary>
		/// Identifies the read-only <see cref="StartLocal"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartLocalProperty = StartLocalPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="ActivityBase.Start"/> in local time.
		/// </summary>
		/// <seealso cref="StartLocalProperty"/>
		public DateTime StartLocal
		{
			get
			{
				return (DateTime)this.GetValue(ActivityPresenter.StartLocalProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.StartLocalPropertyKey, value);
			}
		}

		#endregion //StartLocal

		#region StartOutOfRangeIndicatorVisibility

		private static readonly DependencyPropertyKey StartOutOfRangeIndicatorVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("StartOutOfRangeIndicatorVisibility",
			typeof(Visibility), typeof(ActivityPresenter), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="StartOutOfRangeIndicatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartOutOfRangeIndicatorVisibilityProperty = StartOutOfRangeIndicatorVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the start date out of reange indicator (read-only)
		/// </summary>
		/// <seealso cref="StartOutOfRangeIndicatorVisibilityProperty"/>
		public Visibility StartOutOfRangeIndicatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityPresenter.StartOutOfRangeIndicatorVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.StartOutOfRangeIndicatorVisibilityPropertyKey, value);
			}
		}

		#endregion //StartOutOfRangeIndicatorVisibility

		#region SuffixFormatType

		private static readonly DependencyPropertyKey SuffixFormatTypePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SuffixFormatType",
			typeof(DateRangeFormatType), typeof(ActivityPresenter), DateRangeFormatType.None, null);

		/// <summary>
		/// Identifies the read-only <see cref="SuffixFormatType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SuffixFormatTypeProperty = SuffixFormatTypePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns and enumeration that determines how the activity suffix will be formatted.
		/// </summary>
		/// <seealso cref="SuffixFormatTypeProperty"/>
		public DateRangeFormatType SuffixFormatType
		{
			get
			{
				return (DateRangeFormatType)this.GetValue(ActivityPresenter.SuffixFormatTypeProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.SuffixFormatTypePropertyKey, value);
			}
		}

		#endregion //SuffixFormatType

		#region ToolTipInfo

		private static readonly DependencyPropertyKey ToolTipInfoPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ToolTipInfo",
			typeof(ActivityToolTipInfo), typeof(ActivityPresenter), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ToolTipInfo"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ToolTipInfoProperty = ToolTipInfoPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an object with toolip info (read-only)
		/// </summary>
		/// <seealso cref="ToolTipInfoProperty"/>
		public ActivityToolTipInfo ToolTipInfo
		{
			get
			{
				return (ActivityToolTipInfo)this.GetValue(ActivityPresenter.ToolTipInfoProperty);
			}
			internal set
			{
				this.SetValue(ActivityPresenter.ToolTipInfoPropertyKey, value);
			}
		}

		#endregion //ToolTipInfo
		
		#endregion //Public Properties

		#region Internal Properties

		#region BrushIds

		internal virtual CalendarBrushId BackgroundBrushId { get { return CalendarBrushId.AppointmentBackground; } }
		internal virtual CalendarBrushId BorderBrushId { get { return CalendarBrushId.AppointmentBorder; } }
		internal virtual CalendarBrushId DateTimeForegroundBrushId { get { return CalendarBrushId.AppointmentDateTimeForeground; } }

		// AS 1/5/11 NA 11.1 Activity Categories
		//internal virtual CalendarBrushId GetForegroundBrushId(bool allowOverlay)
		//{
		//    if (!allowOverlay || this.IsOwningCalendarSelected)
		//        return CalendarBrushId.AppointmentForeground;
		//
		//    return CalendarBrushId.AppointmentForegroundOverlayed; 
		//}
		internal virtual CalendarBrushId GetForegroundBrushId( bool useOverlay )
		{
			if ( useOverlay )
				return CalendarBrushId.AppointmentForegroundOverlayed;

			return CalendarBrushId.AppointmentForeground;
		}

		#endregion //BrushIds	
    
		#region NotchExtent

		internal double NotchExtent
		{
			get { return this._notchExtent; }
			set
			{
				if (this._notchExtent != value)
				{
					this._notchExtent = value;
					this.InvalidateMeasure();
				}
			}
		}

		#endregion //NotchExtent	
    
		#region NotchOffset

		internal double NotchOffset
		{
			get { return this._notchOffset; }
			set
			{
				if (this._notchOffset != value)
				{
					this._notchOffset = value;
					this.InvalidateMeasure();
				}
			}
		}

		#endregion //NotchOffset	
    
		#region CanDrag

		internal virtual bool CanDrag
		{
			get
			{
				return this.CanEdit;
			}
		}

		#endregion //CanDrag

		#region CanEdit

		internal virtual bool CanEdit
		{
			get
			{
				ActivityBase activity = this.Activity;

				if (activity == null)
					return false;

				XamScheduleDataManager dm = this.DataManager;

				return dm != null ? dm.IsActivityOperationAllowedHelper(activity, ActivityOperation.Edit) : false;
			}
		}

		#endregion //CanEdit	

		#region CanEditInPlace

		internal virtual bool CanEditInPlace
		{
			get
			{
				if (this._editArea == null ||
					 this._editor == null)
					return false;

				if (this.CanEdit == false)
					return false;

				var activityPanel = VisualTreeHelper.GetParent(this) as ScheduleActivityPanel;

				Debug.Assert(null != activityPanel || this.Tag == ScheduleControlBase.MeasureOnlyItemId, "Cannot edit an activity that isn't within an activity panel.");

				if (activityPanel == null)
					return false;

				try
				{
					GeneralTransform transform = this.TransformToVisual(activityPanel);

					Point pt = transform.Transform(new Point(0, 0));

					if (activityPanel.TimeslotOrientation == Orientation.Vertical)
						return pt.Y >= 0;
					else
						return pt.X >= 0;
				}
				catch (Exception)
				{
					// in SL the TransformToVisual can result in an ArgumentException when the elements 
					// are not in the tree and there doesn't seem to be any properties/methods we can 
					// check to detect this so we have to just catch the exception
					return false;
				}
			}
		}

		#endregion //CanEditInPlace	

		#region Control

		internal ScheduleControlBase Control
		{
			get
			{
				if ( _control == null )
				{
					UIElement parent = VisualTreeHelper.GetParent(this) as UIElement;

					_control = parent != null ? ScheduleUtilities.GetControl(parent) : null;
				}

				return _control;
			}
		}

		#endregion //Control	

		#region DataManager

		private XamScheduleDataManager DataManager
		{
			get
			{
				ScheduleControlBase control = this.Control;
				return control != null ? control.DataManagerResolved : null;
			}
		}

		#endregion //DataManager

		#region IsFarInPanel 

		internal bool IsFarInPanel
		{
			get { return this.GetFlag(InternalFlags.IsFarInPanel); }
			set 
			{
				if (value != this.IsFarInPanel)
				{
					this.SetFlag(InternalFlags.IsFarInPanel, value);
					this.SetFlag(InternalFlags.IsNearOrFarInPanelChanged, true); // AS 10/1/10 TFS50023
					this.InvalidateMeasure();
				}
			}
		} 
		#endregion // IsFarInPanel 

		#region IsHiddenDragSource
		internal bool IsHiddenDragSource
		{
			get { return this.GetFlag(InternalFlags.IsHiddenDragSource); }
			set
			{
				if (value != this.IsHiddenDragSource)
				{
					this.SetFlag(InternalFlags.IsHiddenDragSource, value);

					
					if (value)
						this.Opacity = 0;
					else
						this.Opacity = 1;
				}
			}
		} 
		#endregion // IsHiddenDragSource

		// AS 1/5/11 NA 11.1 Activity Categories
		#region IsInitializing
		internal bool IsInitializing
		{
			get { return this.GetFlag(InternalFlags.IsInitializing); }
			set
			{
				if ( value != this.IsInitializing )
				{
					this.SetFlag(InternalFlags.IsInitializing, value);

					if ( value == false )
					{
						if (this.GetFlag(InternalFlags.SetProviderBrushesPending))
							this.SetProviderBrushes();

						if ( this.GetFlag(InternalFlags.PrefixSuffixFormatPending) )
							this.VerifyPrefixSuffixFormat();
					}
				}
			}
		}
		#endregion // IsInitializing

		#region IsMultiDay
		internal bool IsMultiDay 
		{
			get { return this.GetFlag(InternalFlags.IsMultiDay); }
			set 
			{
				if ( value != this.IsMultiDay )
				{
					this.SetFlag(InternalFlags.IsMultiDay, value);
					this.SetProviderBrushes();
				}
			}
		}
		#endregion // IsMultiDay

		#region IsNearInPanel

		internal bool IsNearInPanel
		{
			get { return this.GetFlag(InternalFlags.IsNearInPanel); }
			set 
			{
				if (value != this.IsNearInPanel)
				{
					this.SetFlag(InternalFlags.IsNearInPanel, value);
					this.SetFlag(InternalFlags.IsNearOrFarInPanelChanged, true); // AS 10/1/10 TFS50023
					this.InvalidateMeasure();
				}
			}
		}
		#endregion // IsNearInPanel 

		#region IsOwningCalendarSelected

		internal bool IsOwningCalendarSelected
		{
			get { return this.GetFlag(InternalFlags.IsOwningCalendarSelected); }
			set
			{
				if (value != this.IsOwningCalendarSelected)
				{
					this.SetFlag(InternalFlags.IsOwningCalendarSelected, value);
					this.SetProviderBrushes();
				}
			}
		} 
		#endregion // IsOwningCalendarSelected

		#region MouseHelper
		internal MouseHelper MouseHelper
		{
			get
			{
				if (_mouseHelper == null)
					_mouseHelper = new MouseHelper(this);

				return _mouseHelper;
			}
		}
		#endregion // MouseHelper

		#region NeedsFromIndicator

		internal bool NeedsFromIndicator
		{
			get { return this.GetFlag(InternalFlags.NeedsFromIndicator); }
			set 
			{
				if (value != NeedsFromIndicator)
				{
					this.SetFlag(InternalFlags.NeedsFromIndicator, value);

					if (value == true)
						this.SetValue(StartOutOfRangeIndicatorVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
					else
						this.ClearValue(StartOutOfRangeIndicatorVisibilityPropertyKey);
					
					this.VerifyPrefixSuffixFormat();
				}
			}
		}
		#endregion // NeedsFromIndicator

		#region NeedsToIndicator

		internal bool NeedsToIndicator
		{
			get { return this.GetFlag(InternalFlags.NeedsToIndicator); }
			set
			{
				if (value != NeedsToIndicator)
				{
					this.SetFlag(InternalFlags.NeedsToIndicator, value);

					if (value == true)
						this.SetValue(EndOutOfRangeIndicatorVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
					else
						this.ClearValue(EndOutOfRangeIndicatorVisibilityPropertyKey);

					this.VerifyPrefixSuffixFormat();
				}
			}

		}
		#endregion // NeedsToIndicator

		// AS 1/5/11 NA 11.1 Activity Categories
		#region PrimaryCategory
		internal ActivityCategory PrimaryCategory
		{
			get
			{
				var categories = this.Categories;
				return categories == null ? null : categories.FirstOrDefault(( ActivityCategory category ) => { return category != null && category.Color != null; });
			}
		}
		#endregion // PrimaryCategory

		#region SpansLogicalDays

		internal bool SpansLogicalDays
		{
			get { return this.GetFlag(InternalFlags.SpansLogicalDays); }
			set 
			{
				if (value != this.SpansLogicalDays)
				{
					this.SetFlag(InternalFlags.SpansLogicalDays, value);
					this.VerifyPrefixSuffixFormat();
				}
			}
		}
		#endregion // SpansLogicalDays

		#endregion //Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region CanResizeEdge
		internal bool CanResizeEdge(bool leadingEdge)
		{
			ActivityBase activity = this.Activity;

			if (activity == null)
				return false;

			if (leadingEdge && !this.IsNearInPanel)
				return false;

			if (!leadingEdge && !this.IsFarInPanel)
				return false;

			XamScheduleDataManager dm = this.DataManager;

			if (dm == null || !dm.IsActivityResizeAllowed(activity, leadingEdge))
				return false;

			return true;
		}
		#endregion // CanResizeEdge

		#region ChangeVisualState
		internal virtual void ChangeVisualState(bool useTransitions)
		{
			if (this.IsEnabled)
			{
				if (_isMouseOver)
					VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
				else
					VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
			}
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);

			if (this.IsInEditMode)
				this.GoToState(VisualStateUtilities.StateEditing, useTransitions);
			else
				this.GoToState(VisualStateUtilities.StateDisplay, useTransitions);

			if ( this.CanEditInPlace )
				this.GoToState(VisualStateUtilities.StateEditable, useTransitions);
			else
				this.GoToState(VisualStateUtilities.StateUneditable, useTransitions);

			if (this.IsSelected)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateSelected, VisualStateUtilities.StateUnselected);
			else
				this.GoToState(VisualStateUtilities.StateUnselected, useTransitions);

			this.SetProviderBrushes();
		}
		#endregion //ChangeVisualState
    
		#region DoDefaultAction
		internal virtual void DoDefaultAction()
		{
			this.DisplayActivityDialog();
		}
		#endregion // DoDefaultAction

		#region EndEdit

		internal void EndEdit(bool cancel, bool force)
		{
			if (this.IsInEditMode)
			{
				ActivityBase activity = this.Activity;

				if (activity == null)
					return;

				ScheduleControlBase control = this.Control;

				if (control != null)
					control.EditHelper.EndEdit(this, cancel, force);
			}
		}

		#endregion //EndEdit

		#region ForceLostFocusBindingUpdates
		internal void ForceLostFocusBindingUpdates()
		{
			if (_tracker != null)
				PresentationUtilities.ForceLostFocusBindingUpdate(this, _tracker.ElementWithFocus);
		} 
		#endregion // ForceLostFocusBindingUpdates

		#region GoToState

		internal void GoToState(string stateName, bool useTransitions)
		{
			VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		#endregion // GoToState

		// AS 1/5/11 NA 11.1 Activity Categories
		#region InitializeCategories
		internal void InitializeCategories()
		{
			IEnumerable<ActivityCategory> categories = null;

			if ( _activity != null )
			{
				var ctrl = this.Control;
				var dm = ctrl != null ? ctrl.DataManagerResolved : null;

				if ( null != dm )
					categories = dm.ResolveActivityCategories(_activity);
			}

			if ( categories == null )
				this.ClearValue(CategoriesPropertyKey);
			else
				this.SetValue(CategoriesPropertyKey, categories);
		}
		#endregion // InitializeCategories

		#region OnActivityPropertyChanged
		internal virtual void OnActivityPropertyChanged(string property, object extraInfo)
		{
			bool isAllProps = string.IsNullOrEmpty(property);

			if (isAllProps || property == "OwningCalendar")
			{
				var fe = VisualTreeHelper.GetParent(this) as FrameworkElement;
				var selectedCalendar = fe != null ? fe.DataContext as ResourceCalendar : null;
				this.IsOwningCalendarSelected = selectedCalendar == this.Activity.OwningCalendar;

				// reget the brushes anyway since some other state may have changed (e.g. location for appt)
				this.SetProviderBrushes();
			}

			if (isAllProps)
			{
				this.InitializeStartEndLocal();
				this.InitializeHasPendingOperation();
				this.InitializeReminderVisibility();
				this.InitializeError();
				this.RaiseChangeNotifications(CanResizeCriteria, null); // AS 10/21/10 TFS57876
			}
			else
			{
				switch (property)
				{
					case "OwningResource":
					case "ReminderEnabled":
						this.InitializeReminderVisibility();
						break;
					case "Start":
					case "StartTimeZoneId":
					case "End":
					case "EndTimeZoneId":
					case "IsTimeZoneNeutral":
						this.InitializeStartEndLocal();
						break;
					case "Error":
						this.InitializeError();
						break;
					case "PendingOperation":
						this.InitializeHasPendingOperation();
						break;
					// AS 10/21/10 TFS57876
					case "IsAddNew":
					case "IsLocked":
						this.RaiseChangeNotifications(CanResizeCriteria, null);
						break;
					// JJD 4/5/11 - TFS68245
					case "Categories":
						this.InitializeCategories();
						break;
					default:
						this.VerifyPrefixSuffixFormat();
						break;
				}

			}
		}
		#endregion // OnActivityPropertyChanged

		#region OnCurrentUserChanged
		internal void OnCurrentUserChanged()
		{
			this.InitializeReminderVisibility();
		} 
		#endregion // OnCurrentUserChanged

		#region OnTextInputCreation

		internal void OnTextInputCreation(TextCompositionEventArgs e)
		{
			TextBox tbox = this._editor as TextBox;

			if (tbox != null)
			{
				tbox.Text += e.Text;
				tbox.SelectionStart = tbox.Text.Length;
				return;
			}

			PasswordBox pbox = this._editor as PasswordBox;

			if (pbox != null)
			{
				pbox.Password += e.Text;
				return;
			}
		}

		#endregion //OnTextInputCreation	
    
		#region OnProcessDoubleClick

		internal virtual void OnProcessDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (this.DisplayActivityDialog() != null)
				e.Handled = true;
		}

		#endregion //OnProcessDoubleClick

		#region SetProviderBrushes

		internal virtual void SetProviderBrushes()
		{
			// AS 1/5/11 NA 11.1 Activity Categories
			if ( this.IsInitializing )
			{
				this.SetFlag(InternalFlags.SetProviderBrushesPending, true);
				return;
			}

			ActivityBase activity = this.Activity;

			if (activity == null)
				return;

			ScheduleControlBase control = this.Control;

			if ( control == null )
				return;

			HorizontalAlignment contentAlignment = HorizontalAlignment.Left;

			if (!(control is ScheduleTimeControlBase))
			{
				if (this.IsMultiDay || this.SpansLogicalDays)
					contentAlignment = System.Windows.HorizontalAlignment.Center;
			}
			else
			{
				// If this is a multi-day activity and panel arranges the timeslots horizontally thn we want to center the contents
				if (this.IsMultiDay)
				{
					ScheduleActivityPanel panel = VisualTreeHelper.GetParent(this) as ScheduleActivityPanel;
					contentAlignment = panel != null && panel.TimeslotOrientation == Orientation.Horizontal ? HorizontalAlignment.Center : HorizontalAlignment.Left;
				}
			}

			if (contentAlignment == HorizontalAlignment.Left)
				this.ClearValue(ComputedContentHorizontalAlignmentPropertyKey);
			else
				this.ComputedContentHorizontalAlignment = contentAlignment;

			ResourceCalendar calendar = activity.OwningCalendar;

			CalendarBrushProvider brushProvider = calendar != null ? calendar.BrushProvider : null;

			if (brushProvider == null)
				return;

			this.SetValue(CalendarBrushProvider.BrushProviderProperty, brushProvider);

			// AS 1/5/11 NA 11.1 Activity Categories
			//Brush br = brushProvider.GetBrush(this.BackgroundBrushId);
			Brush br = null;
			Color? activityColor = null;
			ActivityCategory category = this.PrimaryCategory;

			if ( category != null )
			{
				activityColor = category.Color;
			}

			if ( activityColor != null )
				br = ScheduleUtilities.GetActivityCategoryBrush(activityColor.Value, ActivityCategoryBrushId.Background);
			else
				br = brushProvider.GetBrush(this.BackgroundBrushId);

			// JJD 5/10/11 - TFS74042 
			// Create a stack variable to hold the back color
			Color backColor = Colors.White;

			if (br != null)
			{
				this.ComputedBackground = br;

				// JJD 5/10/11 - TFS74042 
				// Try a to get a color from the brush
				if (br is SolidColorBrush)
					backColor = ((SolidColorBrush)br).Color;
				else
				{
					GradientBrush gradient = br as GradientBrush;

					if (gradient != null && gradient.GradientStops.Count > 0)
						backColor = gradient.GradientStops[gradient.GradientStops.Count - 1].Color;
				}
			}

			if (activityColor.HasValue)
				backColor = activityColor.Value;

			// JJD 5/10/11 - TFS74042 
			// Set the ComputedIndicatorForeground based on the background color to
			// either a white brush or our default
			if (ScheduleUtilities.CalculateForeground(backColor) == Colors.White)
			{
				this.SetValue(ComputedIndicatorForegroundPropertyKey, IndicatorForeground_White);

				// JJD 11/4/11 - TFS74042 
				// Set the 'out of range' indicator background and foreground also
				this.SetValue(OutOfRangeIndicatorBackgroundPropertyKey, OutOfRangeIndicatorBackground_White);
				this.SetValue(OutOfRangeIndicatorForegroundPropertyKey, OutOfRangeIndicatorForeground_White);
			}
			else
			{
				this.ClearValue(ComputedIndicatorForegroundPropertyKey);

				// JJD 11/4/11 - TFS74042 
				// Clear the 'out of range' indicator background and foreground also
				this.ClearValue(OutOfRangeIndicatorBackgroundPropertyKey);
				this.ClearValue(OutOfRangeIndicatorForegroundPropertyKey);
			}
			
			bool isSelected = this.IsSelected;
			
			if (isSelected)
				this.ComputedBorderStrokeThickness = 3;
			else
				this.ClearValue(ComputedBorderStrokeThicknessPropertyKey);

			// AS 1/5/11 NA 11.1 Activity Categories
			//br = brushProvider.GetBrush(isSelected ? CalendarBrushId.SelectedActivityBorder : this.BorderBrushId);
			if (isSelected)
				br = brushProvider.GetBrush(CalendarBrushId.SelectedActivityBorder);
			else if (activityColor != null)
				br = ScheduleUtilities.GetActivityCategoryBrush(activityColor.Value, ActivityCategoryBrushId.Border);
			else
				br = brushProvider.GetBrush(this.BorderBrushId);

			if (br != null)
				this.ComputedBorderBrush = br;

			// AS 1/5/11 NA 11.1 Activity Categories
			//br = brushProvider.GetBrush(this.DateTimeForegroundBrushId);
			if ( activityColor != null )
				br = ScheduleUtilities.GetActivityCategoryBrush(activityColor.Value, ActivityCategoryBrushId.Foreground);
			else
				br = brushProvider.GetBrush(this.DateTimeForegroundBrushId);

			if (br != null)
				this.ComputedDateTimeForeground = br;

			ScheduleControlBase ctrl = ScheduleUtilities.GetControl(this);

			// AS 1/5/11 NA 11.1 Activity Categories
			//br = brushProvider.GetBrush(this.GetForegroundBrushId(ctrl == null || ctrl.CalendarHeaderAreaVisibilityResolved != Visibility.Collapsed));
			bool useOverlay = ctrl != null && ctrl.CalendarHeaderAreaVisibilityResolved != Visibility.Collapsed && !this.IsOwningCalendarSelected;
			if ( activityColor == null )
				br = brushProvider.GetBrush(this.GetForegroundBrushId(useOverlay));
			else if ( useOverlay )
			{
				// if it's not the overlay then we can use the same color as above. but if it's an overlay then we need to 
				// calculate the best foreground
				br = ScheduleUtilities.GetActivityCategoryBrush(activityColor.Value, ActivityCategoryBrushId.ForegroundOverlay);
			}

			if (br != null)
				this.ComputedForeground = br;
		}

		#endregion //SetProviderBrushes

		#region UpdateVisualState
		internal void UpdateVisualState()
		{
			
			this.ChangeVisualState(true);
		}
		#endregion // UpdateVisualState

		#region VerifyBorderGeometry

		internal void VerifyBorderGeometry(Size size)
		{

			if (double.IsPositiveInfinity(size.Width) || double.IsPositiveInfinity(size.Height))
			{
				_lastBorderGeometrySize = Size.Empty; // AS 5/9/12 TFS104555
				this.ClearValue(ComputedContentMarginPropertyKey);
				return;
			}

			_lastBorderGeometrySize = size; // AS 5/9/12 TFS104555
			ScheduleControlBase control = this.Control;

			// AS 12/17/10 TFS62030
			//bool isinDayArea = false;
			//
			//if (control is ScheduleTimeControlBase)
			//{
			//    if (control is XamDayView)
			//    {
			//        TimeslotPanelBase panel = VisualTreeHelper.GetParent(this) as TimeslotPanelBase;
			//
			//        isinDayArea = panel != null && !panel.IsVertical;
			//    }
			//}
			//else
			//{
			//    isinDayArea = true;
			//}
			bool indentLeading, indentTrailing;

			if ( null != control )
				control.ShouldIndentActivityEdge(this, out indentLeading, out indentTrailing);
			else
				indentTrailing = indentLeading = false;

			bool hasAdjustment = false;
			Thickness dayEdgeAdjustment = new Thickness(3);
			Rect rect = new Rect(new Point(0,0), size);

			// AS 12/17/10 TFS62030
			//if (isinDayArea)
			{
				// adjust the left edge inward
				// AS 12/17/10 TFS62030
				//if (this.IsNearInPanel)
				if (indentLeading)
				{
					rect.X = DayEdgeMarginExtent;
					rect.Width = Math.Max(rect.Width - DayEdgeMarginExtent, 0);
					dayEdgeAdjustment.Left = DayEdgeMarginExtent;
					hasAdjustment = true;
				}

				// adjust the right edge inward
				// AS 12/17/10 TFS62030
				//if (this.IsFarInPanel)
				if (indentTrailing)
				{
					rect.Width = Math.Max(rect.Width - DayEdgeMarginExtent, 0);
					dayEdgeAdjustment.Right = DayEdgeMarginExtent;
					hasAdjustment = true;
				}
			}

			Geometry geometry = this.CreateGeometry(rect, this.ComputedBorderStrokeThickness);


			geometry.Freeze();

			if (geometry is PathGeometry)
			{
				this.ComputedContentMargin = new Thickness(NotchWidth + 3 + dayEdgeAdjustment.Left, 3, 3 + dayEdgeAdjustment.Right, 3);
			}
			else if (hasAdjustment)
				this.ComputedContentMargin = dayEdgeAdjustment;
			else
				this.ClearValue(ComputedContentMarginPropertyKey);

			this.ComputedGeometry = geometry;

			// JJD 3/21/11 - TFS65145 - added BorderPath part
			// Instead of binding to the ComputedGeometry the template now has a template part for the border Path element.
			// If we have one set its Data property explicitly to the new geometry. This avoids
			// an exception in Silverlight that gets thrown when a local ActivityPresenter style is supplied and it gets unloaded, 
			// e.g. if the schedule control is on a tab that gets de-selected.
			if (this._borderPath != null)
				this._borderPath.Data = geometry;
		}

		#endregion //VerifyBorderGeometry
    
		#endregion //Internal Methods

		#region Private Methods

		#region AddLineSegment

		private static void AddLineSegment(PathSegmentCollection segments, Point pt)
		{

			segments.Add(new LineSegment(pt, true));





		}

		#endregion //AddLineSegment	

		#region AddListener

		internal void AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			_listeners = ListenerList.Add(_listeners, listener, useWeakReference);
		}

		#endregion // AddListener
    
		#region BeginEdit

		private void BeginEdit()
		{
			if (this.IsInEditMode || !this.CanEditInPlace)
				return;

			ActivityBase activity = this.Activity;

			if (activity == null)
				return;

			ScheduleControlBase control = this.Control;

			if (control != null)
				control.EditHelper.BeginEdit(this);
		}

		#endregion //BeginEdit

		#region CreateGeometry

		private Geometry CreateGeometry(Rect rect, double strokeThickness)
		{
			// must take the midline into account
			double adjustment = strokeThickness / 2;
			Rect overallRect = new Rect(rect.Left + adjustment, rect.Top + adjustment, Math.Max(1, rect.Width - strokeThickness), Math.Max(1, rect.Height - strokeThickness));

			if (this._notchExtent < 1)
			{
				RectangleGeometry rg = new RectangleGeometry();
				rg.Rect = overallRect;
				return rg;
			}

			if (overallRect.Width <= NotchWidth + 1)
			{
				RectangleGeometry rg = new RectangleGeometry();
				rg.Rect = new Rect(adjustment, this._notchOffset + adjustment, Math.Max(1, overallRect.Width - strokeThickness), Math.Max(1, this._notchExtent));
				return rg;
			}

			PathFigure figure = new PathFigure();

			PathSegmentCollection segments = new PathSegmentCollection();
			Point pt = new Point(adjustment, this._notchOffset + adjustment);

			AddLineSegment(segments, pt);
			figure.StartPoint = pt;

			if (this._notchOffset > 0)
			{
				pt.X = NotchWidth + adjustment;
				AddLineSegment(segments, pt);
				pt.Y = overallRect.Y;
				AddLineSegment(segments, pt);
			}

			pt.X = overallRect.Right;
			AddLineSegment(segments, pt);

			pt.Y = overallRect.Bottom;
			AddLineSegment(segments, pt);

			if (this._notchOffset + this._notchExtent + adjustment < pt.Y)
			{
				pt.X = NotchWidth + adjustment;
				AddLineSegment(segments, pt);

				pt.Y = this._notchOffset + this._notchExtent + adjustment;
				AddLineSegment(segments, pt);

				pt.X = adjustment;
				AddLineSegment(segments, pt);
			}
			else
			{
				pt.X = adjustment;
				AddLineSegment(segments, pt);
			}

			figure.Segments = segments;
			figure.IsClosed = true;
			PathFigureCollection figures = new PathFigureCollection();
			
			figures.Add(figure);

			PathGeometry path = new PathGeometry();
			path.Figures = figures;

			return path;
		}

		#endregion //CreateGeometry	
    
		#region DisplayActivityDialog

		private bool? DisplayActivityDialog()
		{
			ScheduleControlBase control = this.Control;

			if (control != null)
				return control.DisplayActivityDialog(this.Activity);

			return null;
		}

		#endregion //DisplayActivityDialog

		#region EndMouseCapture
		private void EndMouseCapture()
		{
			if (_mouseDownPoint != null)
			{
				this.ReleaseMouseCapture();
			}
		}
		#endregion // EndMouseCapture
    	
		#region FocusEditSite

		private void FocusEditSite()
		{
			if (this._editor != null &&
				 this.IsInEditMode)
			{
				this._editor.Focus();

				this._tracker = new LostFocusTracker(this, this.OnEditSiteLostFocus);
			}
		}

		#endregion //FocusEditSite

		#region GetFlag
		/// <summary>
		/// Returns true if any of the specified bits are true.
		/// </summary>
		/// <param name="flag">Flag(s) to evaluate</param>
		/// <returns></returns>
		private bool GetFlag(InternalFlags flag)
		{
			return (_flags & flag) != 0;
		}
		#endregion // GetFlag

		#region InitializeError

		private void InitializeError()
		{
			var activity = this.Activity;

			if (null == activity)
				return;

			DataErrorInfo error = activity.Error;

			if ( error == null )
			{
				ActivityBase root = _activity.RootActivity;

				error = root != null ? root.Error : null;
			}

			ActivityToolTipInfo tt = this.ToolTipInfo;

			if (error != null )
			{
			    tt.SetValue(ActivityToolTipInfo.ErrorPropertyKey, error);
			}
			else
			{
			    tt.ClearValue(ActivityToolTipInfo.ErrorPropertyKey);
			}
		}

		#endregion //InitializeError

		#region InitializeHasPendingOperation

		private void InitializeHasPendingOperation()
		{
			var activity = this.Activity;

			if (null == activity)
				return;

			bool hasPendingOperation = activity.PendingOperation != null;

			if ( !hasPendingOperation )
			{
				ActivityBase root = _activity.RootActivity;

				hasPendingOperation = root != null && root.PendingOperation != null;
			}

			if (hasPendingOperation)
				this.SetValue(HasPendingOperationPropertyKey, KnownBoxes.TrueBox);
			else
				this.ClearValue(HasPendingOperationPropertyKey);
		}

		#endregion //InitializeHasPendingOperation	
    
		#region InitializeReminderVisibility
		private void InitializeReminderVisibility()
		{
			if (ScheduleUtilities.GetIsReminderActive(_activity, ScheduleUtilities.GetDataManager(this)))
				this.SetValue(ReminderVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
			else
				this.ClearValue(ReminderVisibilityPropertyKey);
		} 
		#endregion // InitializeReminderVisibility

		#region InitializeStartEndLocal
		private void InitializeStartEndLocal()
		{
			var activity = this.Activity;

			if (null == activity)
				return;

			var ctrl = ScheduleUtilities.GetControl(this);

			if (null == ctrl)
				return;

			var tz = ctrl.TimeZoneInfoProviderResolved;

			this.StartLocal = activity.GetStartLocal(tz.LocalToken);
			this.EndLocal = activity.GetEndLocal(tz.LocalToken);
			
			this.VerifyPrefixSuffixFormat();
		}

		#endregion // InitializeStartEndLocal

		// AS 3/7/12 TFS102945
		// Moved here from the OnMouseMove since we may need to initiate a drag from a touch operation.
 		//
		#region InitiateDrag
		private bool InitiateDrag(Point mouseDownPoint)
		{
			if (this.CanDrag)
			{
				ScheduleControlBase control = this.Control;

				if (control != null)
					return control.EditHelper.BeginDrag(this, mouseDownPoint, (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control, false, 0, 0);
			}
			return false;
		}
		#endregion //InitiateDrag

		#region IsAlignedToLogicalDayBoundaries
		private static bool IsAlignedToLogicalDayBoundaries(ActivityPresenter presenter, XamScheduleDataManager dm)
		{
			DateTime start = presenter.StartLocal;
			DateTime end = presenter.EndLocal;
			TimeSpan logicalDayOffset = dm.LogicalDayOffset;
			DateRange logicalDayRange = ScheduleUtilities.GetLogicalDayRange(start, logicalDayOffset);

			return start.TimeOfDay == logicalDayRange.Start.TimeOfDay && end.TimeOfDay == logicalDayRange.End.TimeOfDay;
		}
		#endregion // IsAlignedToLogicalDayBoundaries

		#region OnEditSiteLostFocus

		private void OnEditSiteLostFocus()
		{
			if (this._editor != null)
			{
				if (this.IsInEditMode)
					this.EndEdit(false, true);
			}

			this._tracker = null;
		}

		#endregion //OnEditSiteLostFocus	

		#region OnIsEnabledChanged


		private static void OnIsEnabledPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ActivityPresenter instance = target as ActivityPresenter;

			instance.UpdateVisualState();
		}







		#endregion //OnIsEnabledChanged	
    
		#region OnTimerTick

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (this._timer != null)
			{
				this.StopEditTimer();
				this.EndMouseCapture();
				this.BeginEdit();
			}

		}

		#endregion //OnTimerTick	

		// AS 3/7/12 TFS102945
		// Added helper method that performs the appropriate action when a tap/drag occurred 
		// beyond the scroll timeout threshold.
		//
		#region ProcessDeferredDown
		private void ProcessDeferredDown(ScrollInfoTouchAction action, MouseButtonEventArgs e)
		{
			// stop the edit mode timer that we have going
			this.StopEditTimer();
			this.ReleaseMouseCapture();

			ScheduleControlBase control = this.Control;

			if (control == null)
				return;

			bool wasSelected = this.IsSelected;

			// make sure it's selected
			ResourceCalendar calendar = this._activity != null ? this._activity.OwningCalendar : null;

			if (calendar != null)
			{
				control.SelectCalendar(calendar, true, this);

				bool toggle = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

				int index = control.SelectedActivities.IndexOf(this._activity);

				if (toggle && index >= 0)
					control.SelectedActivities.RemoveAt(index);
				else
				{
					if (index < 0)
						control.SelectActivity(this._activity, toggle);
				}
			}

			if (action == ScrollInfoTouchAction.Click)
			{
				// if it was already selected and we are touching it again then put it in edit mode but 
				// wait in case there is a double tap
				if (this._editor != null && wasSelected && ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) &&
					this.IsInEditMode == false && this.CanEditInPlace)
				{
					this.StartEditTimer(MouseHelper.DoubleClickTime);
				}
			}
			else
			{
				this.InitiateDrag(e.GetPosition(this));
			}
		}
		#endregion //ProcessDeferredDown

		#region RaiseChangeNotifications

		private void RaiseChangeNotifications(string propname, object extraInfo)
		{
			if (null != _listeners)
				ListenerList.RaisePropertyChanged<object, string>(_listeners, this, propname, extraInfo);
		}

		#endregion //RaiseChangeNotifications	
    
		#region RemoveListener

		internal void RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			_listeners = ListenerList.Remove(_listeners, listener);
		}

		#endregion // RemoveListener

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		// AS 3/7/12 TFS102945
		// Created helper method since we use this from multiple spots now.
		//
		#region StartEditTimer
		private void StartEditTimer(long ticksInterval)
		{
			this.StopEditTimer();

			this._timer = new DispatcherTimer();
			this._timer.Interval = TimeSpan.FromTicks(ticksInterval);
			this._timer.Tick += new EventHandler(OnTimerTick);
			this._timer.Start();
		}
		#endregion //StartEditTimer

		// AS 3/7/12 TFS102945
		// Added helper method.
		//
		#region StopEditTimer
		private void StopEditTimer()
		{
			if (this._timer != null)
			{
				this._timer.Stop();
				this._timer.Tick -= new EventHandler(OnTimerTick);
				this._timer = null;
			}
		}
		#endregion //StopEditTimer

		#region VerifyPrefixSuffixFormat

		private void VerifyPrefixSuffixFormat()
		{
			// AS 1/5/11 NA 11.1 Activity Categories
			if ( this.IsInitializing )
			{
				this.SetFlag(InternalFlags.PrefixSuffixFormatPending, true);
				return;
			}

			ScheduleControlBase control = this.Control;

			DateRangeFormatType prefixFormat = DateRangeFormatType.None;
			DateRangeFormatType suffixFormat = DateRangeFormatType.None;

			XamScheduleDataManager dm = control != null ? control.DataManagerResolved : null;

			if ( dm != null && _activity != null ) 
			{
				bool hasEndTime = _activity.Duration.Ticks > 0;
				bool showStartTime = false;
				bool showEndTime = false;
				bool spansDays = hasEndTime && this.SpansLogicalDays;

				if (control is XamScheduleView)
				{
					// do nothing
				}
				else if (control is XamMonthView)
				{
					if (!hasEndTime)
						showStartTime = true;
					else
					{
						// if start or end are not on logical day boundaries
						showStartTime = !IsAlignedToLogicalDayBoundaries(this, dm);
					}
				}
				else
				{
					if (this.SpansLogicalDays)
					{
						// if start or end are not on logical day boundaries
						showStartTime = !IsAlignedToLogicalDayBoundaries(this, dm);
					}
				}

				if ( hasEndTime )
				{
					if ( showStartTime )
						showEndTime = true;
				}

				if ( showStartTime )
				{
					if ( showEndTime )
					{
						if ( spansDays == false && control is XamMonthView )
						{
							prefixFormat = DateRangeFormatType.StartAndEndTime;
						}
						else
						{
							prefixFormat = DateRangeFormatType.StartTimeOnly;
							suffixFormat = DateRangeFormatType.EndTimeOnly;
						}
					
					}
					else
					{
						prefixFormat = DateRangeFormatType.StartTimeOnly;
					}
				}
				else
				{
					Debug.Assert(showEndTime == false, "Not sure how we could get here");
					if ( showEndTime )
						suffixFormat = DateRangeFormatType.EndTimeOnly;
				}
			}

			if (this.NeedsFromIndicator)
			{
				prefixFormat = DateRangeFormatType.StartDateOutOfView;

				if (suffixFormat == DateRangeFormatType.None)
				{
					if (control is XamScheduleView)
					{
						if (this.NeedsToIndicator == false && this.IsFarInPanel)
							suffixFormat = DateRangeFormatType.EndTimeOnly;
					}
				}
			}

			if (this.NeedsToIndicator)
			{
				suffixFormat = DateRangeFormatType.EndDateOutOfView;
				
				if (prefixFormat == DateRangeFormatType.None)
				{
					if (control is XamScheduleView)
					{
						if (this.NeedsFromIndicator == false && this.IsNearInPanel)
							prefixFormat = DateRangeFormatType.StartTimeOnly;
					}
				}
			}

			if ( prefixFormat == DateRangeFormatType.None )
				this.ClearValue(PrefixFormatTypePropertyKey);
			else
				this.PrefixFormatType = prefixFormat;

			if ( suffixFormat == DateRangeFormatType.None )
				this.ClearValue(SuffixFormatTypePropertyKey);
			else
				this.SuffixFormatType = suffixFormat;

		}
		#endregion //VerifyPrefixSuffixFormat	
    	
		#endregion //Private Methods	
	
		#endregion //Methods

		#region ICalendarBrushClient Members

		void ICalendarBrushClient.OnBrushVersionChanged()
		{
			this.SetProviderBrushes();
		}

		#endregion

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			if (dataItem is ActivityBase)
			{
				if (dataItem == this._activity)
					this.OnActivityPropertyChanged(property, extraInfo);
				else
				{
					// if the dataitem isn't the activity then it must be the root activity
					Debug.Assert(_activity != null, "the actvity should not be null in the ActivityPresenter.OnPropertyValueChanged");
					Debug.Assert(_activity != null && _activity.RootActivity == dataItem, "the dataitem should be the root actvity in the ActivityPresenter.OnPropertyValueChanged");

					switch (property)
					{
						case "Error":
							this.InitializeError();
							break;
						case "PendingOperation":
							this.InitializeHasPendingOperation();
							break;
					}
				}
			}
			// AS 1/5/11 NA 11.1 Activity Categories
			else if ( dataItem is IEnumerable<ActivityCategory> )
			{
				this.SetProviderBrushes();
			}
			// AS 1/5/11 NA 11.1 Activity Categories
			else if ( dataItem is ActivityCategory )
			{
				if ( string.IsNullOrEmpty(property) || property == "Color" )
				{
					if ( dataItem == this.PrimaryCategory )
					{
						this.SetProviderBrushes();
					}
				}

				// JJD 4/5/11 - TFS68245
				if (string.IsNullOrEmpty(property) || property == "CategoryName")
					this.InitializeCategories();
			}
		}

		#endregion //ITypedPropertyChangeListener<object,string> Members

		#region ISupportPropertyChangeNotifications Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			this.AddListener(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			this.RemoveListener(listener);
		}

		#endregion // ISupportPropertyChangeNotifications Implementation

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : short
		{
			IsMultiDay = 0x1,
			IsSelected = 0x2,
			IsFarInPanel = 0x4,
			IsNearInPanel = 0x8,
			IsOwningCalendarSelected = 0x10,
			NeedsFromIndicator = 0x20,
			NeedsToIndicator = 0x40,
			IsHiddenDragSource = 0x80,
			SpansLogicalDays = 0x100,
			IsNearOrFarInPanelChanged = 0x200,  // AS 10/1/10 TFS50023
			// AS 1/5/11 NA 11.1 Activity Categories
			IsInitializing = 0x400,
			SetProviderBrushesPending = 0x800,
			PrefixSuffixFormatPending = 0x1000,
		}
		#endregion // InternalFlags enum
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