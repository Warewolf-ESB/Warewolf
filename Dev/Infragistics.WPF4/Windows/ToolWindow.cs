using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Interop;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.Themes;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using Infragistics.Shared;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers;
using Infragistics.Windows.Internal;


namespace Infragistics.Windows.Controls
{
    
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

    // AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// A stylable control used display elements as if hosted within a window.
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note:</b> This class is for Infragistics internal use only.</p>
	/// </remarks>

    // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,               GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,             GroupName = VisualStateUtilities.GroupActive)]

	[TemplatePart(Name = "PART_Caption", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderLeft", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderRight", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderTop", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderBottom", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderTopLeft", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderTopRight", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderBottomLeft", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BorderBottomRight", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ResizeGrip", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ToolWindow : ContentControl, IMinMaxWatcherOwner
	{
		#region Member Variables

		private FrameworkElement[] _parts;
		private static readonly int PartCount;
		private static object[] BoxedParts;
		private EventHandlerList _events;

		// hold calculated min/max for related coersion methods
		private object _calculatedMinWidth;
		private object _calculatedMinHeight;
		private object _calculatedMaxWidth;
		private object _calculatedMaxHeight;

		// cached sizes of element without content and without content & caption
		private Size _sizeWithoutContent;
		private Size _sizeWithoutCaption;

		// helper class to watch for changes to the min/max size of the child
		private MinMaxWatcher _minMaxWatcher;

		private bool _isCalculatingSizeWithoutContent = false;

		private bool _activateOnShow = true;

        // AS 10/13/08 TFS6107/BR34010
        private bool _isDelayedMinMaxPending = false;

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        private bool? _dialogResult;
        private bool _showingAsDialog;

        // AS 8/15/08
        private bool _isLoadedAfterShow = false;

        // AS 3/30/09 TFS16355 - WinForms Interop
        private bool _preferPopup = false;

		// AS 9/11/09 TFS21330
		private bool _isReshowing = false;

		// AS 4/28/11 TFS73532
		private DeferredShowInfo _deferredShowInfo;


        // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		private Rect _restoreBounds = Rect.Empty;
		private bool _restoreToMaximized = false;
		private WindowState _cachedWindowState;

        #endregion //Member Variables

		#region Constructor
		static ToolWindow()
		{
			// AS 5/9/08
			// register the groupings that should be applied when the theme property is changed
			ThemeManager.RegisterGroupings(typeof(ToolWindow), new string[] { PrimitivesGeneric.Location.Grouping });

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(typeof(ToolWindow)));
			KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));

			FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRelativePositionSettingChanged)));
			FrameworkElement.VerticalAlignmentProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRelativePositionSettingChanged)));

			FrameworkElement.MinWidthProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMinWidth)));
			FrameworkElement.MinHeightProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMinHeight)));
			FrameworkElement.MaxWidthProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMaxWidth)));
			FrameworkElement.MaxHeightProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMaxHeight)));
			FrameworkElement.WidthProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceWidth)));
			FrameworkElement.HeightProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceHeight)));

			FrameworkElement.FlowDirectionProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFlowDirectionChanged)));

			// AS 11/17/11 TFS91061
			FrameworkElement.VisibilityProperty.OverrideMetadata(typeof(ToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged)));

			CommandManager.RegisterClassCommandBinding(typeof(ToolWindow), new CommandBinding(CloseCommand, new ExecutedRoutedEventHandler(OnExecuteCommand), new CanExecuteRoutedEventHandler(OnCanExecuteCommand)));

			// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
			CommandManager.RegisterClassCommandBinding(typeof(ToolWindow), new CommandBinding(MinimizeCommand, new ExecutedRoutedEventHandler(OnExecuteCommand), new CanExecuteRoutedEventHandler(OnCanExecuteCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(ToolWindow), new CommandBinding(MaximizeCommand, new ExecutedRoutedEventHandler(OnExecuteCommand), new CanExecuteRoutedEventHandler(OnCanExecuteCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(ToolWindow), new CommandBinding(RestoreCommand, new ExecutedRoutedEventHandler(OnExecuteCommand), new CanExecuteRoutedEventHandler(OnCanExecuteCommand)));

			PartCount = Enum.GetValues(typeof(ToolWindowPart)).Length;

			BoxedParts = new object[PartCount];

			for (int i = 0; i < PartCount; i++)
				BoxedParts[i] = Enum.ToObject(typeof(ToolWindowPart), i);
		}

		/// <summary>
		/// Initializes a new <see cref="ToolWindow"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This class is for Infragistics internal use only.</p>
		/// </remarks>
		public ToolWindow()
		{
			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			_cachedWindowState = (WindowState)this.GetValue(WindowStateProperty);

			this.SetValue(ToolWindowPropertyKey, this);

            // AS 8/15/08
            // Track when the window is loaded so we can handle only honoring
            // relative positioning for a ShowDialog call until it has been 
            // positioned initially.
            //
            this.Loaded += new RoutedEventHandler(OnLoaded);
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (null == this._parts)
				this._parts = new FrameworkElement[PartCount];
			else
			{
				for (int i = 0; i < PartCount; i++)
				{
					FrameworkElement oldElement = this._parts[i];

					if (null != oldElement)
					{
						oldElement.ClearValue(HitTestCodePropertyKey);
						oldElement.ClearValue(ToolWindowPartProperty);
						oldElement.MouseLeftButtonDown -= new MouseButtonEventHandler(OnPartMouseLeftButtonDown);
						oldElement.QueryCursor -= new QueryCursorEventHandler(OnPartQueryCursor);
					}
				}
			}

			for (int i = 0; i < PartCount; i++)
			{
				ToolWindowPart part = (ToolWindowPart)i;
				// AS 5/20/08 BR32842
				// The enum is obfuscated so perform the mapping manually.
				//
				//FrameworkElement newElement = this.GetTemplateChild("PART_" + part.ToString()) as FrameworkElement;
				string partName = GetPartName(part);
				FrameworkElement newElement = this.GetTemplateChild(partName) as FrameworkElement;

				if (null != newElement && part != ToolWindowPart.Content)
				{
					newElement.SetValue(HitTestCodePropertyKey, NativeWindowMethods.GetHitTestResult(part));
					newElement.SetValue(ToolWindowPartProperty, BoxedParts[i]);
					newElement.MouseLeftButtonDown += new MouseButtonEventHandler(OnPartMouseLeftButtonDown);
					newElement.QueryCursor += new QueryCursorEventHandler(OnPartQueryCursor);
				}

				this._parts[i] = newElement;
			}

			this.CalculateMinMaxExtents(true);

			// deal with RTL and swapping the hittest result for ResizeGrip
			this.VerifyResizeGripHitTestResult();

            // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

		}

		#endregion //OnApplyTemplate

		#region OnContentChanged
		/// <summary>
		/// Invoked when the content of the element has been changed.
		/// </summary>
		/// <param name="oldContent">The object that represents the old content</param>
		/// <param name="newContent">The object that represents the new content</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			this.CalculateMinMaxExtents(true);

			if (null != this._minMaxWatcher)
				BindingOperations.ClearAllBindings(this._minMaxWatcher);

			this._minMaxWatcher = newContent is FrameworkElement ? new MinMaxWatcher(this, (FrameworkElement)newContent) : null;
		}
		#endregion //OnContentChanged

		#region OnContextMenuOpening
		/// <summary>
		/// Invoked when the element should display its context menu.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected sealed override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			base.OnContextMenuOpening(e);

			if (this.ShowContextMenu(e.OriginalSource, new Point(e.CursorLeft, e.CursorTop), true))
				e.Handled = true;
		}
		#endregion //OnContextMenuOpening

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns <see cref="ToolWindow"/> Automation Peer Class <see cref="ToolWindowAutomationPeer"/>
        /// </summary> 
        /// <returns>AutomationPeer</returns>

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToolWindowAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

        #region OnInitialized
        /// <summary>
		/// Invoked when the element has been initialized.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.InitializeBorderThicknessBindings();
		}

		#endregion //OnInitialized

		#region OnPreviewMouseDown
		/// <summary>
		/// Invoked when any mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			if (null != this.Host)
				this.Host.Activate();

			base.OnPreviewMouseDown(e);
		}
		#endregion //OnPreviewMouseDown

		#region ToString
		/// <summary>
		/// Returns a string representation of the window.
		/// </summary>
		/// <returns>The type of the class and the string representation of the content</returns>
		public override string ToString()
		{
			return string.Format("{0} Content: {1}", this.GetType().Name, this.Content);
		} 
		#endregion //ToString

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region AllowClose

		/// <summary>
		/// Identifies the <see cref="AllowClose"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register("AllowClose",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAllowCloseChanged)));

		private static void OnAllowCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}

		/// <summary>
		/// Returns/sets a boolean indicating if the window may be closed.
		/// </summary>
		/// <seealso cref="AllowCloseProperty"/>
		//[Description("Returns/sets a boolean indicating if the window may be closed.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowClose
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.AllowCloseProperty);
			}
			set
			{
				this.SetValue(ToolWindow.AllowCloseProperty, value);
			}
		}

		#endregion //AllowClose

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region AllowMaximize

		/// <summary>
		/// Identifies the <see cref="AllowMaximize"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register("AllowMaximize",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnAllowMinMaxChanged), new CoerceValueCallback(CoerceAllowMaximize)));

		private static object CoerceAllowMaximize(DependencyObject d, object baseValue)
		{
			ToolWindow tw = d as ToolWindow;

			if (tw.ResizeMode == ResizeMode.NoResize)
				return KnownBoxes.FalseBox;

			return baseValue;
		}

		/// <summary>
		/// Returns or sets a value indicating whether the ToolWindow supports being maximized.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This feature is only supported when the ToolWindow is hosted within a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="AllowMaximizeProperty"/>
		//[Description("Returns or sets a value indicating whether the ToolWindow supports being maximized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowMaximize
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.AllowMaximizeProperty);
			}
			set
			{
				this.SetValue(ToolWindow.AllowMaximizeProperty, value);
			}
		}

		#endregion //AllowMaximize

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region AllowMinimize

		/// <summary>
		/// Identifies the <see cref="AllowMinimize"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register("AllowMinimize",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnAllowMinMaxChanged), new CoerceValueCallback(CoerceAllowMinimize)));

		private static object CoerceAllowMinimize(DependencyObject d, object baseValue)
		{
			ToolWindow tw = d as ToolWindow;

			switch (tw.ResizeMode)
			{
				case ResizeMode.NoResize:
					return KnownBoxes.FalseBox;
				case ResizeMode.CanMinimize:
					return KnownBoxes.TrueBox;
			}

			return baseValue;
		}

		/// <summary>
		/// Returns or sets a value indicating whether the ToolWindow supports being minimized.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This feature is only supported when the ToolWindow is hosted within a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="AllowMinimizeProperty"/>
		//[Description("Returns or sets a value indicating whether the ToolWindow supports being Minimized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowMinimize
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.AllowMinimizeProperty);
			}
			set
			{
				this.SetValue(ToolWindow.AllowMinimizeProperty, value);
			}
		}

		#endregion //AllowMinimize

		// AS 1/20/11 TFS63757
		#region AllowsTransparency

		/// <summary>
		/// Identifies the <see cref="AllowsTransparency"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowsTransparencyProperty = DependencyProperty.Register("AllowsTransparency",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAllowsTransparencyChanged)));

		private static void OnAllowsTransparencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ToolWindow)d).OnSupportsAllowsTransparencyChanged();
		}

		/// <summary>
		/// Returns/sets a boolean indicating if AllowsTransparency of the hosting <see cref="Popup"/> or <see cref="Window"/> should be true.
		/// </summary>
		/// <summary>
		/// <p class="note">This property is not used when the ToolWindow is hosted in a WPF Window and the <see cref="UseOSNonClientArea"/> is true.</p>
		/// </summary>
		/// <seealso cref="AllowsTransparencyProperty"/>
		//[Description("Returns/sets a boolean indicating if the window may be closed.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowsTransparency
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.AllowsTransparencyProperty);
			}
			set
			{
				this.SetValue(ToolWindow.AllowsTransparencyProperty, value);
			}
		}

		#endregion //AllowsTransparency

		// AS 6/24/11 FloatingWindowCaptionSource
		#region CaptionVisibility

		private static readonly DependencyPropertyKey CaptionVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("CaptionVisibility",
			typeof(Visibility), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Identifies the <see cref="CaptionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CaptionVisibilityProperty =
			CaptionVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns or sets the preferred visibility of the ToolWindow caption when not using the operating system non-client area.
		/// </summary>
		/// <seealso cref="CaptionVisibilityProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility CaptionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ToolWindow.CaptionVisibilityProperty);
			}
			protected set
			{
				this.SetValue(ToolWindow.CaptionVisibilityPropertyKey, value);
			}
		}

		#endregion //CaptionVisibility

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region DialogResult

        /// <summary>
        /// Returns or sets the dialog result to return to the caller when displayed using the <see cref="ShowDialog"/> method.
        /// </summary>
        //[Description("Returns or sets the dialog result to return to the caller when displayed using the ShowDialog method.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public bool? DialogResult
        {
            get
            {
                this.VerifyAccess();

                return this._dialogResult;
            }
            set
            {
                this.VerifyAccess();

                if (false == this._showingAsDialog)
                    throw new InvalidOperationException(SR.GetString("LE_CannotSetDialogResult"));

                if (value != this._dialogResult)
                {
                    this._dialogResult = value;

                    this.Close();
                }
            }
        }

        #endregion //DialogResult

		#region HorizontalAlignmentMode

		/// <summary>
		/// Identifies the <see cref="HorizontalAlignmentMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalAlignmentModeProperty = DependencyProperty.Register("HorizontalAlignmentMode",
			typeof(ToolWindowAlignmentMode), typeof(ToolWindow), new FrameworkPropertyMetadata(ToolWindowAlignmentMode.Manual, new PropertyChangedCallback(OnRelativePositionSettingChanged)));

		/// <summary>
		/// Returns or sets a value indicating whether the <see cref="Left"/> or <see cref="HorizontalAlignment"/> property should be honored when determining the horizontal position of the element.
		/// </summary>
		/// <seealso cref="HorizontalAlignmentModeProperty"/>
		/// <seealso cref="FrameworkElement.HorizontalAlignment"/>
		/// <seealso cref="HorizontalAlignmentOffset"/>
		/// <seealso cref="Left"/>
		//[Description("Returns or sets a value indicating whether the 'Left' or 'HorizontalAlignment' property should be honored when determining the horizontal position of the element.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ToolWindowAlignmentMode HorizontalAlignmentMode
		{
			get
			{
				return (ToolWindowAlignmentMode)this.GetValue(ToolWindow.HorizontalAlignmentModeProperty);
			}
			set
			{
				this.SetValue(ToolWindow.HorizontalAlignmentModeProperty, value);
			}
		}

		#endregion //HorizontalAlignmentMode

		#region HorizontalAlignmentOffset

		/// <summary>
		/// Identifies the <see cref="HorizontalAlignmentOffset"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalAlignmentOffsetProperty = DependencyProperty.Register("HorizontalAlignmentOffset",
			typeof(double), typeof(ToolWindow), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnRelativePositionSettingChanged)));

		/// <summary>
		/// Returns or sets the amount that the element should be adjusted horizontally when the <see cref="HorizontalAlignmentMode"/> is set to 'UseAlignment'.
		/// </summary>
		/// <seealso cref="HorizontalAlignmentOffsetProperty"/>
		/// <seealso cref="HorizontalAlignmentMode"/>
		/// <seealso cref="FrameworkElement.HorizontalAlignment"/>
		//[Description("Returns or sets the amount that the element should be adjusted horizontally when the 'HorizontalAlignmentMode' is set to 'UseAlignment'.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public double HorizontalAlignmentOffset
		{
			get
			{
				return (double)this.GetValue(ToolWindow.HorizontalAlignmentOffsetProperty);
			}
			set
			{
				this.SetValue(ToolWindow.HorizontalAlignmentOffsetProperty, value);
			}
		}

		#endregion //HorizontalAlignmentOffset

		#region IsActive

		internal static readonly DependencyPropertyKey IsActivePropertyKey =
			DependencyProperty.RegisterReadOnly("IsActive",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveChanged)));

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow content = (ToolWindow)d;

			if (true.Equals(e.NewValue))
				content.OnActivated(EventArgs.Empty);
			else
				content.OnDeactivated(EventArgs.Empty);

            // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
            content.UpdateVisualStates(false);

        }

		/// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty =
			IsActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the window is active.
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		//[Description("Returns a boolean indicating if the window is active.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.IsActiveProperty);
			}
		}

		#endregion //IsActive

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region IsOwnedWindow

		/// <summary>
		/// Identifies the <see cref="IsOwnedWindow"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsOwnedWindowProperty = DependencyProperty.Register("IsOwnedWindow",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsOwnedWindowChanged)));

		private static void OnIsOwnedWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow tw = d as ToolWindow;

			if (tw.Host != null && tw.Host.IsWindow && tw.IsVisible)
			{
				try
				{
					tw.VerifyIsOwnedState();
				}
				catch (System.Security.SecurityException)
				{
				}
			}
		}

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region VerifyIsOwnedWindow
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void VerifyIsOwnedState()
		{
			if (this.Host == null || this.Host.IsWindow == false)
				return;

			if (this.IsOwnedWindow)
				this.SetWindowOwner(this.Host as Window, GetOwningWindow(this.Owner));
			else
				this.ClearWindowOwner(this.Host as Window);
		} 
		#endregion //VerifyIsOwnedWindow

		/// <summary>
		/// Returns or sets a value indicating whether the ToolWindow will be hosted in an owned window when displayed within a WPF Window.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This feature is only supported when the ToolWindow is hosted within a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="IsOwnedWindowProperty"/>
		//[Description("Returns or sets a value indicating whether the ToolWindow will be hosted in an owned window when displayed within a WPF Window.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsOwnedWindow
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.IsOwnedWindowProperty);
			}
			set
			{
				this.SetValue(ToolWindow.IsOwnedWindowProperty, value);
			}
		}

		#endregion //IsOwnedWindow

		#region IsUsingOSNonClientArea

		private static readonly DependencyPropertyKey IsUsingOSNonClientAreaPropertyKey =
			DependencyProperty.RegisterReadOnly("IsUsingOSNonClientArea",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsUsingOSNonClientAreaChanged)));

		private static void OnIsUsingOSNonClientAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow tw = d as ToolWindow;

			tw.InitializeBorderThicknessBindings();

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// The Window is no longer binding to this property once it has been shown.
			// This is necessary because the window cannot have its WindowStyle changed from 
			// None to ToolWindow/SingleBorderWindow when it had its AllowsTransparency set 
			// to true before it was shown. Similarly when changing from 
			// ToolWindow/SingleBorderWindow to None, the AllowsTransparency cannot be set 
			// to true once the Window has been shown or the WPF window class will throw an 
			// exception. To avoid both of these issues, we will just reshow the toolwindow 
			// as we would if the resolved AllowsTransparency is changed.
			//
			if (tw.Host != null && !tw.IsModal && tw.Host.IsWindow && tw.IsVisible)
			{
				tw.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MethodInvoker(tw.ReshowIfVisible));
			}
		}

		/// <summary>
		/// Identifies the <see cref="IsUsingOSNonClientArea"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsUsingOSNonClientAreaProperty =
			IsUsingOSNonClientAreaPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the content is currently using the operating system to provide the operating system non-client area.
		/// </summary>
		/// <seealso cref="IsUsingOSNonClientAreaProperty"/>
		//[Description("Returns a boolean indicating if the content is currently using the operating system to provide the operating system non-client area.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsUsingOSNonClientArea
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.IsUsingOSNonClientAreaProperty);
			}
		}

		#endregion //IsUsingOSNonClientArea

		#region Left

		/// <summary>
		/// Identifies the <see cref="Left"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LeftProperty = Window.LeftProperty.AddOwner(typeof(ToolWindow));

		/// <summary>
		/// Returns the horizontal coordinate for the left edge of the window.
		/// </summary>
		/// <seealso cref="LeftProperty"/>
		//[Description("Returns the horizontal coordinate for the left edge of the window.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(LengthConverter))]
		public double Left
		{
			get
			{
				return (double)this.GetValue(ToolWindow.LeftProperty);
			}
			set
			{
				this.SetValue(ToolWindow.LeftProperty, value);
			}
		}

		#endregion //Left

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region MaximizeButtonVisibility

		/// <summary>
		/// Identifies the <see cref="MaximizeButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizeButtonVisibilityProperty = DependencyProperty.Register("MaximizeButtonVisibility",
			typeof(Visibility), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, null, new CoerceValueCallback(CoerceMinMaxButtonVisibility)));

		/// <summary>
		/// Returns or sets a value indicating if the maximize button should be displayed in the template.
		/// </summary>
		/// <remarks>
		/// <p class="note">Note: This property does not affect the non-client area when <see cref="UseOSNonClientArea"/> is true. Also this property 
		/// will be coerced to collapsed if both the <see cref="AllowMinimize"/> and <see cref="AllowMaximize"/> properties are false.</p>
		/// </remarks>
		/// <seealso cref="MaximizeButtonVisibilityProperty"/>
		/// <seealso cref="MinimizeButtonVisibility"/>
		/// <seealso cref="AllowMinimize"/>
		/// <seealso cref="AllowMaximize"/>
		//[Description("Returns or sets a value indicating if the maximize button should be displayed in the template.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Visibility MaximizeButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ToolWindow.MaximizeButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(ToolWindow.MaximizeButtonVisibilityProperty, value);
			}
		}

		#endregion //MaximizeButtonVisibility

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region MinimizeButtonVisibility

		/// <summary>
		/// Identifies the <see cref="MinimizeButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimizeButtonVisibilityProperty = DependencyProperty.Register("MinimizeButtonVisibility",
			typeof(Visibility), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, null, new CoerceValueCallback(CoerceMinMaxButtonVisibility)));

		/// <summary>
		/// Returns or sets a value indicating if the minimize button should be displayed in the template.
		/// </summary>
		/// <remarks>
		/// <p class="note">Note: This property does not affect the non-client area when <see cref="UseOSNonClientArea"/> is true. Also this property 
		/// will be coerced to collapsed if both the <see cref="AllowMinimize"/> and <see cref="AllowMaximize"/> properties are false.</p>
		/// </remarks>
		/// <seealso cref="MinimizeButtonVisibilityProperty"/>
		/// <seealso cref="MaximizeButtonVisibility"/>
		/// <seealso cref="AllowMinimize"/>
		/// <seealso cref="AllowMaximize"/>
		//[Description("Returns or sets a value indicating if the minimize button should be displayed in the template.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Visibility MinimizeButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ToolWindow.MinimizeButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(ToolWindow.MinimizeButtonVisibilityProperty, value);
			}
		}

		#endregion //MinimizeButtonVisibility

		#region Owner

		private static readonly DependencyPropertyKey OwnerPropertyKey =
			DependencyProperty.RegisterReadOnly("Owner",
			typeof(FrameworkElement), typeof(ToolWindow), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnOwnerChanged)));

		private static void OnOwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow window = (ToolWindow)d;
			FrameworkElement oldOwner = e.OldValue as FrameworkElement;
			FrameworkElement newOwner = e.NewValue as FrameworkElement;

			// if the window is closed before it is shown then we could be hooked into the loaded event
			if (null != oldOwner)
			{
				oldOwner.Loaded -= new RoutedEventHandler(window.OnOwnerLoaded);
				oldOwner.Unloaded -= new RoutedEventHandler(window.OnOwnerUnloaded);
			}
			
			// AS 4/30/08
			// Hook/unhook the unloaded. When the owner is unloaded, we will automatically
			// close the window.
			//
			if (null != newOwner)
				newOwner.Unloaded += new RoutedEventHandler(window.OnOwnerUnloaded);
		}

		/// <summary>
		/// Identifies the <see cref="Owner"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OwnerProperty =
			OwnerPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the owning element for which the ToolWindow was displayed.
		/// </summary>
		/// <seealso cref="OwnerProperty"/>
		//[Description("Returns the owning element for which the ToolWindow was displayed.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public FrameworkElement Owner
		{
			get
			{
				return (FrameworkElement)this.GetValue(ToolWindow.OwnerProperty);
			}
		}

		#endregion //Owner

		#region ResizeMode

		/// <summary>
		/// Identifies the <see cref="ResizeMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register("ResizeMode",
			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// We can now support CanMinimize
			//
			//typeof(ResizeMode), typeof(ToolWindow), new FrameworkPropertyMetadata(ResizeMode.CanResize, new PropertyChangedCallback(OnResizeModeChanged), new CoerceValueCallback(CoerceResizeMode)), new ValidateValueCallback(OnValidateResizeMode));
			typeof(ResizeMode), typeof(ToolWindow), new FrameworkPropertyMetadata(ResizeMode.CanResize, new PropertyChangedCallback(OnResizeModeChanged), new CoerceValueCallback(CoerceResizeMode)));

		private static void OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ToolWindow)d).InitializeBorderThicknessBindings();
		}

		private static object CoerceResizeMode(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;
            
            // AS 10/21/08 TFS9284
            // By returned NoResize, the non-client area calculations done while 
            // calculating the size with content is returning a different value
            // than it will when resizing is reenabled since the border width 
            // may change. I think we only want to remove the resize grip so it 
            // doesn't interfere with the attempt to get the non-client area size.
            //
			//return content._isCalculatingSizeWithoutContent ? ResizeMode.NoResize : newValue;
            if (content._isCalculatingSizeWithoutContent &&
                ((ResizeMode)newValue) == ResizeMode.CanResizeWithGrip)
                return ResizeMode.CanResize;

            return newValue;
		}

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar [Start]
		//private static bool OnValidateResizeMode(object newValue)
		//{
		//    if (ResizeMode.CanMinimize.Equals(newValue))
		//        throw new NotSupportedException(SR.GetString("LE_CanMinimizeNotSupport"));
		//
		//    return true;
		//}

		/// <summary>
		/// Returns/sets how/whether the window may be resized.
		/// </summary>
		/// <seealso cref="ResizeModeProperty"/>
		//[Description("Returns/sets how/whether the window may be resized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ResizeMode ResizeMode
		{
			get
			{
				return (ResizeMode)this.GetValue(ToolWindow.ResizeModeProperty);
			}
			set
			{
				this.SetValue(ToolWindow.ResizeModeProperty, value);
			}
		}

		#endregion //ResizeMode

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ShowInTaskbar

		/// <summary>
		/// Identifies the <see cref="ShowInTaskbar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowInTaskbarProperty = DependencyProperty.Register("ShowInTaskbar",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns or sets a value indicating whether the ToolWindow will be displayed in the taskbar when hosted in a WPF Window
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects a ToolWindow that is hosted within a top level WPF window.</p>
		/// </remarks>
		/// <seealso cref="ShowInTaskbarProperty"/>
		//[Description("Returns or sets a value indicating whether the ToolWindow supports being maximized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool ShowInTaskbar
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.ShowInTaskbarProperty);
			}
			set
			{
				this.SetValue(ToolWindow.ShowInTaskbarProperty, value);
			}
		}

		#endregion //ShowInTaskbar

		#region PreferredBorderThickness

		private static readonly DependencyPropertyKey PreferredBorderThicknessPropertyKey =
			DependencyProperty.RegisterReadOnly("PreferredBorderThickness",
			typeof(Thickness), typeof(ToolWindow), new FrameworkPropertyMetadata(new Thickness()));

		/// <summary>
		/// Identifies the <see cref="PreferredBorderThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredBorderThicknessProperty =
			PreferredBorderThicknessPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred border thickness based on the ResizeMode and whether the window is using the os to render the non-client area.
		/// </summary>
		/// <seealso cref="PreferredBorderThicknessProperty"/>
		//[Description("Returns the preferred border thickness based on the ResizeMode and whether the window is using the os to render the non-client area.")]
		//[Category("Appearance")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Thickness PreferredBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(ToolWindow.PreferredBorderThicknessProperty);
			}
		}

		#endregion //PreferredBorderThickness

		#region Theme

		
#region Infragistics Source Cleanup (Region)



















































































































#endregion // Infragistics Source Cleanup (Region)

		/// <summary>
		/// Identifies the 'Theme' dependency property
		/// </summary>
		public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(ToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

		/// <summary>
		/// Event ID for the 'ThemeChanged' routed event
		/// </summary>
		public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(ToolWindow));

		private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow control = target as ToolWindow;

			// JJD 2/26/07
			// we need to call UpdateLayout after we change the merged dictionaries.
			// Otherwise, the styles from the new merged dictionary are not picked
			// up right away. It seems the framework must be caching some information
			// that doesn't get refreshed until the next layout update
			if (control.IsLoaded)
			{
				control.InvalidateMeasure();
				control.UpdateLayout();
			}

			control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
		}

		/// <summary>
		/// Gets/sets the default look for the control.
		/// </summary>
		/// <remarks>
		/// <para class="body">If left set to null then the default 'Generic' theme will be used. 
		/// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b> The following themes are pre-registered by this assembly but additional themes can be registered as well.
		/// <ul>
		/// <li>"Aero" - a theme that is compatible with Vista's 'Aero' theme.</li>
		/// <li>"Generic" - the default theme.</li>
		/// <li>"LunaNormal" - a theme that is compatible with XP's 'blue' theme.</li>
		/// <li>"LunaOlive" - a theme that is compatible with XP's 'olive' theme.</li>
		/// <li>"LunaSilver" - a theme that is compatible with XP's 'silver' theme.</li>
		/// <li>"Office2k7Black" - a theme that is compatible with MS Office 2007's 'Black' theme.</li>
		/// <li>"Office2k7Blue" - a theme that is compatible with MS Office 2007's 'Blue' theme.</li>
		/// <li>"Office2k7Silver" - a theme that is compatible with MS Office 2007's 'Silver' theme.</li>
		/// <li>"Onyx" - a theme that features black and orange highlights.</li>
		/// <li>"Royale" - a theme that features subtle blue highlights.</li>
		/// <li>"RoyaleStrong" - a theme that features strong blue highlights.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
		/// <seealso cref="ThemeProperty"/>
		//[Description("Gets/sets the general look of the control")]
		//[Category("Appearance")]
		[Bindable(true)]
		[DefaultValue((string)null)]
		[TypeConverter(typeof(ThemeTypeConverter))]
		public string Theme
		{
			get
			{
				return (string)this.GetValue(ToolWindow.ThemeProperty);
			}
			set
			{
				this.SetValue(ToolWindow.ThemeProperty, value);
			}
		}


		/// <summary>
		/// Called when property 'Theme' changes
		/// </summary>
		protected virtual void OnThemeChanged(string previousValue, string currentValue)
		{
			RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
			newEvent.RoutedEvent = ToolWindow.ThemeChangedEvent;
			newEvent.Source = this;
			RaiseEvent(newEvent);
		}

		/// <summary>
		/// Occurs when the 'Theme' property changes
		/// </summary>
		//[Description("Occurs when the 'Theme' property changes")]
		//[Category("Behavior")]
		public event RoutedPropertyChangedEventHandler<string> ThemeChanged
		{
			add
			{
				base.AddHandler(ToolWindow.ThemeChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(ToolWindow.ThemeChangedEvent, value);
			}
		}

		#endregion //Theme

		#region Title

		/// <summary>
		/// Identifies the <see cref="Title"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TitleProperty = Window.TitleProperty.AddOwner(typeof(ToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceTitle)));

		private static object CoerceTitle(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			// AS 5/30/08 BR32685
			// Changing the title when the os is providing the non-client area causes
			// the window to repaint its non-client area with the new caption. We don't
			// need to do this anyway when the os is providing the non-client area since
			// we were only doing this to try and calculate the size of the non-client
			// area when it is provided by the template.
			//
			if (content.IsUsingOSNonClientArea)
				return newValue;

			return content._isCalculatingSizeWithoutContent ? "W" : newValue;
		}

		/// <summary>
		/// Returns/sets the title for the window.
		/// </summary>
		/// <seealso cref="TitleProperty"/>
		//[Description("Returns/sets the title for the window.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public string Title
		{
			get
			{
				return (string)this.GetValue(ToolWindow.TitleProperty);
			}
			set
			{
				this.SetValue(ToolWindow.TitleProperty, value);
			}
		}

		#endregion //Title

		#region Top

		/// <summary>
		/// Identifies the <see cref="Top"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TopProperty = Window.TopProperty.AddOwner(typeof(ToolWindow));

		/// <summary>
		/// Returns the vertical coordinate for the top edge of the window.
		/// </summary>
		/// <seealso cref="TopProperty"/>
		//[Description("Returns the vertical coordinate for the top edge of the window.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[TypeConverter(typeof(LengthConverter))]
		public double Top
		{
			get
			{
				return (double)this.GetValue(ToolWindow.TopProperty);
			}
			set
			{
				this.SetValue(ToolWindow.TopProperty, value);
			}
		}

		#endregion //Top

		#region Topmost

		/// <summary>
		/// Identifies the <see cref="Topmost"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TopmostProperty = Window.TopmostProperty.AddOwner(typeof(ToolWindow));

		/// <summary>
		/// Returns or sets a boolean whether the window should remain above other floating windows.
		/// </summary>
		/// <seealso cref="TopmostProperty"/>
		//[Description("Returns or sets a boolean whether the window should remain above other floating windows.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool Topmost
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.TopmostProperty);
			}
			set
			{
				this.SetValue(ToolWindow.TopmostProperty, value);
			}
		}

		#endregion //Topmost

		#region UseOSNonClientArea

		/// <summary>
		/// Identifies the <see cref="UseOSNonClientArea"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseOSNonClientAreaProperty = DependencyProperty.Register("UseOSNonClientArea",
			typeof(bool), typeof(ToolWindow), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnUseOSNonClientAreaChanged)));

		private static void OnUseOSNonClientAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ToolWindow)d).VerifyIsUsingOSNonClientArea();
		}

		/// <summary>
		/// Returns or sets a boolean indicating whether the content should use the operating system nonclient area when possible - i.e. when hosted within a Window.
		/// </summary>
		/// <seealso cref="UseOSNonClientAreaProperty"/>
		//[Description("Returns or sets a boolean indicating whether the content should use the operating system nonclient area when possible - i.e. when hosted within a Window.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool UseOSNonClientArea
		{
			get
			{
				return (bool)this.GetValue(ToolWindow.UseOSNonClientAreaProperty);
			}
			set
			{
				this.SetValue(ToolWindow.UseOSNonClientAreaProperty, value);
			}
		}

		#endregion //UseOSNonClientArea

		#region VerticalAlignmentMode

		/// <summary>
		/// Identifies the <see cref="VerticalAlignmentMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalAlignmentModeProperty = DependencyProperty.Register("VerticalAlignmentMode",
			typeof(ToolWindowAlignmentMode), typeof(ToolWindow), new FrameworkPropertyMetadata(ToolWindowAlignmentMode.Manual, new PropertyChangedCallback(OnRelativePositionSettingChanged)));

		/// <summary>
		/// Returns or sets a value indicating whether the <see cref="Top"/> or <see cref="VerticalAlignment"/> property should be honored when determining the vertical position of the element.
		/// </summary>
		/// <seealso cref="VerticalAlignmentModeProperty"/>
		/// <seealso cref="FrameworkElement.VerticalAlignment"/>
		/// <seealso cref="VerticalAlignmentOffset"/>
		/// <seealso cref="Top"/>
		//[Description("Returns or sets a value indicating whether the 'Top' or 'VerticalAlignment' property should be honored when determining the vertical position of the element.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ToolWindowAlignmentMode VerticalAlignmentMode
		{
			get
			{
				return (ToolWindowAlignmentMode)this.GetValue(ToolWindow.VerticalAlignmentModeProperty);
			}
			set
			{
				this.SetValue(ToolWindow.VerticalAlignmentModeProperty, value);
			}
		}

		#endregion //VerticalAlignmentMode

		#region VerticalAlignmentOffset

		/// <summary>
		/// Identifies the <see cref="VerticalAlignmentOffset"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalAlignmentOffsetProperty = DependencyProperty.Register("VerticalAlignmentOffset",
			typeof(double), typeof(ToolWindow), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnRelativePositionSettingChanged)));

		/// <summary>
		/// Returns or sets the amount that the element should be adjusted vertically when the <see cref="VerticalAlignmentMode"/> is set to 'UseAlignment'.
		/// </summary>
		/// <seealso cref="VerticalAlignmentOffsetProperty"/>
		/// <seealso cref="VerticalAlignmentMode"/>
		/// <seealso cref="FrameworkElement.VerticalAlignment"/>
		//[Description("Returns or sets the amount that the element should be adjusted vertically when the 'VerticalAlignmentMode' is set to 'UseAlignment'.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public double VerticalAlignmentOffset
		{
			get
			{
				return (double)this.GetValue(ToolWindow.VerticalAlignmentOffsetProperty);
			}
			set
			{
				this.SetValue(ToolWindow.VerticalAlignmentOffsetProperty, value);
			}
		}

		#endregion //VerticalAlignmentOffset

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		#region WindowStartupLocation

		/// <summary>
		/// Identifies the <see cref="WindowStartupLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WindowStartupLocationProperty = DependencyProperty.Register("WindowStartupLocation",
			typeof(ToolWindowStartupLocation), typeof(ToolWindow), new FrameworkPropertyMetadata(ToolWindowStartupLocation.Manual));

		/// <summary>
		/// Returns or sets a value indicating how the window should be displayed when initially displayed.
		/// </summary>
		/// <seealso cref="WindowStartupLocationProperty"/>
		//[Description("Returns or sets a value indicating how the window should be displayed when initially displayed.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ToolWindowStartupLocation WindowStartupLocation
		{
			get
			{
				return (ToolWindowStartupLocation)this.GetValue(ToolWindow.WindowStartupLocationProperty);
			}
			set
			{
				this.SetValue(ToolWindow.WindowStartupLocationProperty, value);
			}
		}

		#endregion //WindowStartupLocation

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WindowState

		/// <summary>
		/// Identifies the <see cref="WindowState"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState",
			typeof(WindowState), typeof(ToolWindow), new FrameworkPropertyMetadata(WindowState.Normal, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnWindowStateChanged)));

		private static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow tw = d as ToolWindow;

			tw._cachedWindowState = (WindowState)e.NewValue;

			d.CoerceValue(MinHeightProperty);
			d.CoerceValue(MinWidthProperty);
			d.CoerceValue(MaxHeightProperty);
			d.CoerceValue(MaxWidthProperty);
			d.CoerceValue(HeightProperty);
			d.CoerceValue(WidthProperty);
		}

		/// <summary>
		/// Returns/sets the current state of the window.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This feature is only supported when the ToolWindow is hosted within a WPF Window.</p>
		/// </remarks>
		/// <seealso cref="WindowStateProperty"/>
		//[Description("Returns/sets the current state of the window.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public WindowState WindowState
		{
			get
			{
				return _cachedWindowState;
			}
			set
			{
				this.SetValue(ToolWindow.WindowStateProperty, value);
			}
		}

		#endregion //WindowState

		#endregion //Public Properties

		#region Internal Properties

		// AS 8/24/09 TFS19861
		#region AllowsTransparencyResolved
		internal bool AllowsTransparencyResolved
		{
			get
			{
				if (!this.SupportsAllowsTransparency)
					return false;

				// AS 1/20/11 TFS63757
				//// in the future we may consider exposing a property to control this
				//return true;
				return this.AllowsTransparency;
			}
		}
		#endregion //AllowsTransparencyResolved

		// AS 1/5/10 TFS24684
		#region CaptionElement
		internal FrameworkElement CaptionElement
		{
			get { return this.GetTemplateChild("PART_Caption") as FrameworkElement; }
		} 
		#endregion //CaptionElement

		// AS 8/4/11 TFS83465/TFS83469
		#region EnsureOnScreen
		internal void EnsureOnScreen(bool fullyInView)
		{
			IToolWindowHost host = this.Host;

			if (host == null)
				return;

			host.EnsureOnScreen(fullyInView);
		} 
		#endregion //EnsureOnScreen

		#region HitTestCode

		private static readonly DependencyPropertyKey HitTestCodePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HitTestCode",
			typeof(NativeWindowMethods.HitTestResults), typeof(ToolWindow), new FrameworkPropertyMetadata(NativeWindowMethods.HitTestResults.HTNOWHERE));

		/// <summary>
		/// Identifies the HitTestCode" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetHitTestCode"/>
		internal static readonly DependencyProperty HitTestCodeProperty =
			HitTestCodePropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'HitTestCode' attached readonly property
		/// </summary>
		/// <seealso cref="HitTestCodeProperty"/>
		internal static NativeWindowMethods.HitTestResults GetHitTestCode(DependencyObject d)
		{
			return (NativeWindowMethods.HitTestResults)d.GetValue(ToolWindow.HitTestCodeProperty);
		}

		#endregion //HitTestCode

		#region Host

		/// <summary>
		/// Identifies the <see cref="Host"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty HostProperty = DependencyProperty.Register("Host",
			typeof(IToolWindowHost), typeof(ToolWindow), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHostChanged)));

		private static void OnHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 6/24/11 FloatingWindowCaptionSource
			// This isn't specific to this but when showing the non-client area ourselves 
			// the content could be clipped and it can be if the window rounds down.
			//
			//((ToolWindow)d).VerifyIsUsingOSNonClientArea();
			var tw = d as ToolWindow;

			tw.VerifyIsUsingOSNonClientArea();

			tw.CoerceValue(WidthProperty);
			tw.CoerceValue(HeightProperty);
		}


		/// <summary>
		/// Returns/sets the object hosting the tool window content.
		/// </summary>
		/// <seealso cref="HostProperty"/>
		internal IToolWindowHost Host
		{
			get
			{
				return (IToolWindowHost)this.GetValue(ToolWindow.HostProperty);
			}
			set
			{
				this.SetValue(ToolWindow.HostProperty, value);
			}
		}

		#endregion //Host

		#region IsCalculatingSizeWithoutContent
		internal bool IsCalculatingSizeWithoutContent
		{
			get { return this._isCalculatingSizeWithoutContent; }
		}
		#endregion //IsCalculatingSizeWithoutContent

		// AS 4/28/11 TFS73532
		#region IsDeferredShowPending
		internal bool IsDeferredShowPending
		{
			get { return _deferredShowInfo != null; }
		}
		#endregion //IsDeferredShowPending

		// AS 10/13/08 TFS6107/BR34010
        #region IsDelayedMinMaxPending
        internal bool IsDelayedMinMaxPending
        {
            get { return this._isDelayedMinMaxPending; }
        }
        #endregion //IsDelayedMinMaxPending

        #region IsModal
        /// <summary>
        /// Indicates if the window is being displayed modally.
        /// </summary>
        internal bool IsModal
        {
            get { return this._showingAsDialog; }
        } 
        #endregion //IsModal

		// AS 9/11/09 TFS21330
		#region IsReshowing
		internal bool IsReshowing
		{
			get { return _isReshowing; }
		} 
		#endregion //IsReshowing

		// AS 8/3/11 TFS83465/TFS83469
		#region KeepOnScreen
		internal virtual bool KeepOnScreen
		{
			get { return true; }
		}
		#endregion //KeepOnScreen

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region RestoreToMaximized
		internal bool RestoreToMaximized
		{
			get { return _restoreToMaximized; }
			set 
			{ 
				_restoreToMaximized = value; 
			}
		} 
		#endregion //RestoreToMaximized

		// AS 2/22/12 TFS101038
		#region IsNonClientTouchDrag
		internal bool IsNonClientTouchDrag
		{
			get;
			set;
		}
		#endregion //IsNonClientTouchDrag

		#region ToolWindow

		private static readonly DependencyPropertyKey ToolWindowPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ToolWindow",
			typeof(ToolWindow), typeof(ToolWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, new PropertyChangedCallback(OnToolWindowChanged)));

		private static void OnToolWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 4/25/08
			// I noticed this in xbap. Basically, if you drag a pane out of a window that is active,
			// the old window will continue to think its active and the new window will not realize its
			// active. So if the focused element leaves a window or enters a window, we should also 
			// update the state.
			//
			if (Keyboard.FocusedElement == d)
			{
				if (e.OldValue != null)
					((ToolWindow)e.OldValue).ClearValue(IsActivePropertyKey);
				else if (e.NewValue != null)
					((ToolWindow)e.NewValue).SetValue(ToolWindow.IsActivePropertyKey, KnownBoxes.TrueBox);
			}
		}

		/// <summary>
		/// Identifies the ToolWindow attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetToolWindow"/>
		public static readonly DependencyProperty ToolWindowProperty =
			ToolWindowPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the owning <see cref="ToolWindow"/> for the specified object
		/// </summary>
		/// <seealso cref="ToolWindowProperty"/>
		public static ToolWindow GetToolWindow(DependencyObject d)
		{
			return (ToolWindow)d.GetValue(ToolWindow.ToolWindowProperty);
		}

		#endregion //ToolWindow

        #region UsesRelativePosition
        internal bool UsesRelativePosition
        {
            get
            {
                if (this._isLoadedAfterShow && this._showingAsDialog)
                    return false;

                return this.VerticalAlignmentMode == ToolWindowAlignmentMode.UseAlignment ||
                     this.HorizontalAlignmentMode == ToolWindowAlignmentMode.UseAlignment;
            }
        }
        #endregion //UsesRelativePosition

		#endregion //Internal Properties

		#region Protected Properties

		#region Events property

		/// <summary>
		/// Returns the list of event handlers that are attached to this component.
		/// </summary>
		protected EventHandlerList Events
		{
			get
			{
				if (this._events == null)
					this._events = new EventHandlerList();

				return this._events;
			}
		}
		#endregion //Events property

		#endregion //Protected Properties

		#region Private Properties

		#region PreferredBorderWidth

		/// <summary>
		/// Identifies the <see cref="PreferredBorderWidth"/> dependency property
		/// </summary>
		private static readonly DependencyProperty PreferredBorderWidthProperty = DependencyProperty.Register("PreferredBorderWidth",
			typeof(double), typeof(ToolWindow), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnPreferredBorderSizeChanged)));

		private static void OnPreferredBorderSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ToolWindow)d).UpdatePreferredBorderThickness();
		}

		/// <summary>
		/// Returns the preferred border width based on the resize mode.
		/// </summary>
		/// <seealso cref="PreferredBorderWidthProperty"/>
		private double PreferredBorderWidth
		{
			get
			{
				return (double)this.GetValue(ToolWindow.PreferredBorderWidthProperty);
			}
			set
			{
				this.SetValue(ToolWindow.PreferredBorderWidthProperty, value);
			}
		}

		#endregion //PreferredBorderWidth

		#region PreferredBorderHeight

		/// <summary>
		/// Identifies the <see cref="PreferredBorderHeight"/> dependency property
		/// </summary>
		private static readonly DependencyProperty PreferredBorderHeightProperty = DependencyProperty.Register("PreferredBorderHeight",
			typeof(double), typeof(ToolWindow), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnPreferredBorderSizeChanged)));

		/// <summary>
		/// Returns the preferred border height based on the resize mode.
		/// </summary>
		/// <seealso cref="PreferredBorderHeightProperty"/>
		private double PreferredBorderHeight
		{
			get
			{
				return (double)this.GetValue(ToolWindow.PreferredBorderHeightProperty);
			}
			set
			{
				this.SetValue(ToolWindow.PreferredBorderHeightProperty, value);
			}
		}

		#endregion //PreferredBorderHeight

		#region ToolWindowPart

		/// <summary>
		/// Identifies the <see cref="ToolWindowPart"/> dependency property
		/// </summary>
		private static readonly DependencyProperty ToolWindowPartProperty = DependencyProperty.Register("ToolWindowPart",
			// AS 3/19/08
			// Can't use an internal/private type as a dependency property type.
			//
			//typeof(ToolWindowPart?), typeof(ToolWindow), new FrameworkPropertyMetadata(null));
			typeof(object), typeof(ToolWindow), new FrameworkPropertyMetadata(null), new ValidateValueCallback(ValidateToolWindowPart));

		private static bool ValidateToolWindowPart(object newValue)
		{
			return newValue == null || newValue is ToolWindowPart;
		}

		/// <summary>
		/// Returns the part that the element represents
		/// </summary>
		private ToolWindowPart? GetToolWindowPart(DependencyObject d)
		{
			return (ToolWindowPart?)d.GetValue(ToolWindow.ToolWindowPartProperty);
		}

		#endregion //ToolWindowPart

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AdjustRelativeRect
		internal void AdjustRelativeRect(Rect ownerRect, ref Rect rect)
		{
            // AS 8/15/08
            // When showing the toolwindow modally we just want to use the relative
            // positioning as a means of controlling the initial position.
            //
            if (this.UsesRelativePosition == false)
                return;

			if (this.VerticalAlignmentMode == ToolWindowAlignmentMode.UseAlignment)
			{
				switch (this.VerticalAlignment)
				{
					case VerticalAlignment.Bottom:
						rect.Y = ownerRect.Bottom - rect.Height;
						break;
					case VerticalAlignment.Top:
						rect.Y = ownerRect.Top;
						break;
					case VerticalAlignment.Center:
						rect.Y = ownerRect.Top + (ownerRect.Height - rect.Height) / 2;
						break;
					case VerticalAlignment.Stretch:
						rect.Y = ownerRect.Top;
						rect.Height = ownerRect.Height;
						break;
				}

				rect.Y += this.VerticalAlignmentOffset;
			}

			if (this.HorizontalAlignmentMode == ToolWindowAlignmentMode.UseAlignment)
			{
				HorizontalAlignment hAlign = this.HorizontalAlignment;
				bool isLeftToRight = this.Owner != null && this.Owner.FlowDirection == FlowDirection.LeftToRight;

				// AS 7/14/09 TFS18424
				// In a right to left situation, swap the orientations.
				//
				if (!isLeftToRight)
				{
					switch (hAlign)
					{
						case HorizontalAlignment.Left:
							hAlign = HorizontalAlignment.Right;
							break;
						case HorizontalAlignment.Right:
							hAlign = HorizontalAlignment.Left;
							break;
					}
				}

				switch (hAlign)
				{
					case HorizontalAlignment.Right:
						rect.X = ownerRect.Right - rect.Width;
						break;
					case HorizontalAlignment.Left:
						rect.X = ownerRect.Left;
						break;
					case HorizontalAlignment.Center:
						rect.X = ownerRect.Left + (ownerRect.Width - rect.Width) / 2;
						break;
					case HorizontalAlignment.Stretch:
						rect.X = ownerRect.Left;
						rect.Width = ownerRect.Width;
						break;
				}

				// AS 7/14/09 TFS18424
				// Similarly we need to adjust the offset.
				//
				//rect.X += this.HorizontalAlignmentOffset;
				if (isLeftToRight)
					rect.X += this.HorizontalAlignmentOffset;
				else
					rect.X -= this.HorizontalAlignmentOffset;
			}
		} 
		#endregion //AdjustRelativeRect

		#region CalculateMinMaxExtents
        internal void CalculateMinMaxExtents(bool async)
        {
			// clear the cached sizes.
			this._sizeWithoutCaption = this._sizeWithoutContent = Size.Empty;

            // JJD 3/29/08 - added support for printing.
            // We can't do asynchronous operations during a print
            //if (async)
			if (async && ReportSection.GetIsInReport(this) == false)
			{
                // AS 10/13/08 TFS6107/BR34010
                // When hosted in a window, we can't use the begininvoke to process
                // the delayed measure since that ends up happening after the window
                // has been shown which results in a "jump" on screen as we calculate
                // the min/max size.
                //
                _isDelayedMinMaxPending = true;

                if (this.Host == null || this.Host.HandlesDelayedMinMaxRequests)
                    return;

				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind,
					new MethodInvoker(this.CalculateMinMaxExtentsImpl));
			}
			else
				this.CalculateMinMaxExtentsImpl();
        }
		#endregion //CalculateMinMaxExtents

		// AS 9/21/11 TFS88634
		// Moved here from the Close method so the PaneToolWindow could override 
		// it and potentially hide the window.
		//
		#region CloseOverride
		internal virtual void CloseOverride()
		{
			if (null != this.Host)
				this.Host.Close();
			else if (null != this.Owner)
			{
				// AS 12/18/08 TFS11401
				// If the ToolWindow hadn't been shown then we would
				// have an owner and would be waiting for its Loaded
				// but we wouldn't have a Host.
				//
				this.ClearValue(OwnerPropertyKey);
			}
		}
		#endregion //CloseOverride

		// AS 5/8/09 TFS17053
		// If we can get to the HWND associated with the owner of the toolwindow then
		// we will try to use a WPF Window and set its owner using the WindowInteropHelper.
		//
		#region GetOwnerHwnd
		private static bool _getOwnerHwndFailed = false;

		internal static IntPtr GetOwnerHwnd(FrameworkElement owner)
		{
			if (_getOwnerHwndFailed)
				return IntPtr.Zero;

			// don't try to use a wpf window in an xbap
			if (BrowserInteropHelper.IsBrowserHosted)
				return IntPtr.Zero;

			try
			{
				return GetOwnerHwndImpl(owner);
			}
			catch (System.Security.SecurityException)
			{
				_getOwnerHwndFailed = true;
				return IntPtr.Zero;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static IntPtr GetOwnerHwndImpl(FrameworkElement owner)
		{
			HwndSource hs = HwndSource.FromVisual(owner) as HwndSource;

			if (hs == null)
				return IntPtr.Zero;

			return hs.Handle;
		}
		#endregion //GetOwnerHwnd

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region GetRestoreBounds
		internal Rect GetRestoreBounds(bool queryHost)
		{
			if (queryHost && this.Host != null)
				return this.Host.GetRestoreBounds();
			else
				return _restoreBounds;
		}
		#endregion //GetRestoreBounds

		// AS 1/20/11 Optimization
		#region GetTemplatePart
		internal FrameworkElement GetTemplatePart(ToolWindowPart part)
		{
			if (null != _parts)
				return _parts[(int)part];

			return null;
		} 
		#endregion //GetTemplatePart

		// AS 1/10/12 TFS90890
		#region IsMouseOverRoot
		internal static bool IsMouseOverRoot(FrameworkElement relativeElement, Point relativePoint)
		{
			var hwnd = GetOwnerHwnd(relativeElement);

			if (hwnd != IntPtr.Zero)
				return IsMouseOverRootUnsafe(hwnd, relativeElement, relativePoint);

			var rootVisual = Utilities.GetCommonAncestor(relativeElement, null);
			Point topLeft = Utilities.PointToScreenSafe(rootVisual, new Point());
			Point bottomRight = Utilities.PointToScreenSafe(rootVisual, VisualTreeHelper.GetDescendantBounds(rootVisual).BottomRight);
			var rootRect = Utilities.RectFromPoints(topLeft, bottomRight);
			Point screenPt = Utilities.PointToScreenSafe(relativeElement, relativePoint);
			return rootRect.Contains(screenPt);
		}

		private static bool IsMouseOverRootUnsafe(IntPtr hwnd, FrameworkElement relativeElement, Point relativePoint)
		{
			Point screenPt = Utilities.PointToScreenSafe(relativeElement, relativePoint);
			return NativeWindowMethods.GetWindowInfo(hwnd).rcWindow.ToRect().Contains(screenPt);
		} 
		#endregion //IsMouseOverRoot

		#region OnClosedInternal
		internal void OnClosedInternal()
		{
            // AS 7/24/08 NA 2008 Vol 2 - ShowDialog
            // To follow along with what a window does, we need to 
            // set the dialog result to false if its being closed and
            // the result hasn't been set yet
            if (this._showingAsDialog && this._dialogResult == null)
                this._dialogResult = false;

            // AS 8/15/08
            this._isLoadedAfterShow = false;

			// AS 4/28/11 TFS73532
			// Do not raise the closing/closed while cancelling.
			//
			if (_deferredShowInfo == null || !_deferredShowInfo.IsCancelled)
			{
				this.OnClosed(EventArgs.Empty);
			}

			this.ClearValue(OwnerPropertyKey);

            // AS 7/24/08 NA 2008 Vol 2 - ShowDialog
            if (this._showingAsDialog)
            {
                this.OnEndShowDialog();
            }
		}
		#endregion //OnClosedInternal

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region OnClosingInternal
        internal bool OnClosingInternal(CancelEventArgs e)
        {
			// AS 4/28/11 TFS73532
			// Do not raise the closing/closed while cancelling.
			//
			if (_deferredShowInfo != null && _deferredShowInfo.IsCancelled)
				return false;

            this.OnClosing(e);

			// AS 9/11/09 TFS21330
			// We do not support canceling the closing when we are reshowing the window.
			//
			if (_isReshowing)
				e.Cancel = false;

            if (e.Cancel)
                this._dialogResult = null;

            return e.Cancel;
        } 
        #endregion //OnClosingInternal

		#region OnDoubleClickCaption
		/// <summary>
		/// Invoked when the user double clicks on the caption element.
		/// </summary>
		/// <param name="pt">The point relative to the upper left of the window.</param>
		/// <param name="mouse">Mouse device used to trigger the action</param>
		/// <returns>True if the action was handled; otherwise false.</returns>
		internal protected virtual bool OnDoubleClickCaption(Point pt, MouseDevice mouse)
		{
			// AS 6/9/11 TFS76337
			// If we're in a WPF window that is providing the non-client area then we can 
			// let it handle it but otherwise we need to get involved.
			//
			if (this.Host != null)
			{
				WindowState targetState = this.WindowState;

				switch(this.WindowState)
				{
					case WindowState.Minimized:
						if (this.AllowMinimize)
						{
							if (this.RestoreToMaximized && this.AllowMaximize)
								targetState = WindowState.Maximized;
							else
								targetState = WindowState.Normal;
						}
						break;
					case WindowState.Maximized:
						if (this.AllowMaximize)
							targetState = WindowState.Normal;
						break;
					case WindowState.Normal:
						if (this.AllowMaximize)
							targetState = WindowState.Maximized;
						break;
				}

				if (targetState != this.WindowState)
				{
					this.WindowState = targetState;
					return true;
				}
			}

			return false;
		}
		#endregion //OnDoubleClickCaption

		#region OnDragCaption
		/// <summary>
		/// Invoked when a drag operation by clicking on the caption is about to start.
		/// </summary>
		/// <param name="pt">The point relative to the upper left of the window.</param>
		/// <param name="mouse">Mouse device used to trigger the action</param>
		/// <returns>True if the drag action was handled and therefore the window should not start a drag operation; false to allow the drag operation to occur.</returns>
		internal protected virtual bool OnDragCaption(Point pt, MouseDevice mouse)
		{
			return false;
		}
		#endregion //OnDragCaption

		#region OnMoveCancelled
		/// <summary>
		/// Invoked when OnDragCaption returns false and the window is not able to start a move operation.
		/// </summary>
		internal virtual void OnMoveCancelled()
		{

		} 
		#endregion //OnMoveCancelled

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnEnterMove
		/// <summary>
		/// Invoked when the window has entered a move operation to determine if the OnWindowMoving should be invoked during the operation
		/// </summary>
		/// <returns>Returns false to indicate that the OnWindowMoving should not be invoked.</returns>
		internal virtual bool OnEnterMove()
		{
			return false;
		}
		#endregion //OnEnterMove

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnExitMove
		/// <summary>
		/// Invoked when the window is exiting a move operation
		/// </summary>
		internal virtual bool OnExitMove()
		{
			// AS 5/8/12 TFS107054
			// Added a boolean return value to know if the window did anything.
			//
			return false;
		}
		#endregion //OnExitMove

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowMoving
		internal virtual void OnWindowMoving(Point pt, MouseDevice mouse, Rect logicalScreenRect)
		{
		}
		#endregion //OnWindowMoving

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SetRestoreBounds
		internal void SetRestoreBounds(Rect restoreBounds, bool updateHost)
		{
			if (updateHost && null != this.Host)
				this.Host.SetRestoreBounds(restoreBounds);
			else
				_restoreBounds = restoreBounds;
		}
		#endregion //SetRestoreBounds

		#region ShowSystemContextMenu
		internal bool ShowSystemContextMenu(Point location)
		{
			return this.ShowContextMenu(this, location, false);
		}
		#endregion //ShowSystemContextMenu

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WillHostInWindow
		internal static bool WillHostInWindow(FrameworkElement owner)
		{
			return GetOwningWindow(owner) != null ||
				GetOwnerHwnd(owner) != IntPtr.Zero;
		}
		#endregion //WillHostInWindow

		#region VerifyIsUsingOSNonClientArea
		internal void VerifyIsUsingOSNonClientArea()
		{
			// if hosted within a ToolWindow and we're told to use the os nonclient area
			this.SetValue(IsUsingOSNonClientAreaPropertyKey, Window.GetWindow(this) == this.Host && this.UseOSNonClientArea == true
				? KnownBoxes.TrueBox
				: DependencyProperty.UnsetValue);
		}
		#endregion //VerifyIsUsingOSNonClientArea

		#endregion //Internal Methods

		#region Private Methods

		#region CalculateMinMaxExtentsImpl
		private void CalculateMinMaxExtentsImpl()
		{
			if (this._parts == null)
				return;

            // AS 10/13/08 TFS6107/BR34010
            this._isDelayedMinMaxPending = false;

			this.CalculateSizeWithoutContent();

			// get the element within the content presenter
			FrameworkElement contentElement = this._parts[(int)ToolWindowPart.Content];
			contentElement = contentElement != null && VisualTreeHelper.GetChildrenCount(contentElement) == 1
				? VisualTreeHelper.GetChild(contentElement, 0) as FrameworkElement
				: null;

			// assume no content and use size required as min and no max by default
			this._calculatedMinWidth = this._sizeWithoutContent.Width;
			this._calculatedMinHeight = this._sizeWithoutContent.Height;
			this._calculatedMaxWidth = double.PositiveInfinity;
			this._calculatedMaxHeight = double.PositiveInfinity;

			// if there is a content element...
			if (contentElement != null)
			{
				bool isCaptionAboveContent = true;

				// if the caption is above the content...
				if (isCaptionAboveContent)
				{
					// use the sum of the chrome plus the content minwidth as the min
					this._calculatedMinWidth = Math.Max(this._sizeWithoutContent.Width, this._sizeWithoutCaption.Width + contentElement.MinWidth);
					this._calculatedMinHeight = this._sizeWithoutContent.Height + contentElement.MinHeight;

					// use the sum of the maxwidth with the chrome width if there is a max width on the content
					if (false == double.IsInfinity(contentElement.MaxWidth))
						this._calculatedMaxWidth = this._sizeWithoutCaption.Width + contentElement.MaxWidth;

					if (false == double.IsInfinity(contentElement.MaxHeight))
						this._calculatedMaxHeight = this._sizeWithoutContent.Height + contentElement.MaxHeight;
				}
				else
				{
					// use the sum of the chrome plus the content minwidth as the min
					this._calculatedMinHeight = Math.Max(this._sizeWithoutContent.Height, this._sizeWithoutCaption.Height + contentElement.MinHeight);
					this._calculatedMinWidth = this._sizeWithoutContent.Width + contentElement.MinWidth;

					// use the sum of the maxwidth with the chrome width if there is a max width on the content
					if (false == double.IsInfinity(contentElement.MaxHeight))
						this._calculatedMaxHeight = this._sizeWithoutCaption.Height + contentElement.MaxHeight;

					if (false == double.IsInfinity(contentElement.MaxWidth))
						this._calculatedMaxWidth = this._sizeWithoutContent.Width + contentElement.MaxWidth;
				}
			}

			// refresh the values
			this.CoerceValue(MinWidthProperty);
			this.CoerceValue(MinHeightProperty);
			this.CoerceValue(MaxWidthProperty);
			this.CoerceValue(MaxHeightProperty);
		}
		#endregion //CalculateMinMaxExtentsImpl

		#region CalculateSizeWithoutContent
		private void CalculateSizeWithoutContent()
		{
			if (this._parts == null)
				return;

			bool wasVerifying = this._isCalculatingSizeWithoutContent;
			this._isCalculatingSizeWithoutContent = true;
			try
			{
				// we need to remove the constraints because they will affect the measures below
				this._calculatedMaxHeight = this._calculatedMaxWidth = this._calculatedMinHeight = this._calculatedMinWidth = null;

				// we need to remove the constraints because they will affect the measures below
				this.CoerceValue(MinWidthProperty);
				this.CoerceValue(MinHeightProperty);
				this.CoerceValue(MaxWidthProperty);
				this.CoerceValue(MaxHeightProperty);
				this.CoerceValue(WidthProperty);
				this.CoerceValue(HeightProperty);
				this.CoerceValue(TitleProperty);
				this.CoerceValue(ResizeModeProperty);

				this.CalculateSizeWithoutContentImpl();
			}
			finally
			{
				this._isCalculatingSizeWithoutContent = wasVerifying;

				this.CoerceValue(WidthProperty);
				this.CoerceValue(HeightProperty);
				this.CoerceValue(TitleProperty);
				this.CoerceValue(ResizeModeProperty);
			}
		}

		private void CalculateSizeWithoutContentImpl()
		{
			FrameworkElement contentPart = this._parts[(int)ToolWindowPart.Content];
			FrameworkElement captionPart = this._parts[(int)ToolWindowPart.Caption];
			Size oldSize = this._sizeWithoutContent;

			using (TempValueReplacement tempContentVis = new TempValueReplacement(contentPart, FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox))
			{
				if (null != contentPart)
					Utilities.InvalidateMeasure(contentPart, this);

				this.InvalidateMeasure();

				this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // AS 4/17/19 TFS16677
                // The measure may have failed if the layout was suspended. If so
                // then assume we cannot get the size.
                //
                if (this.IsMeasureValid)
                {
                    this._sizeWithoutContent = this.DesiredSize;

                    using (TempValueReplacement tempCaptionVis = new TempValueReplacement(captionPart, FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox))
                    {
                        if (null != captionPart)
                            Utilities.InvalidateMeasure(captionPart, this);

                        this.InvalidateMeasure();

                        this.Measure(this._sizeWithoutContent);
                        this._sizeWithoutCaption = this.DesiredSize;
                    }
                }
                else
                {
                    // AS 4/17/19 TFS16677
                    _sizeWithoutContent = _sizeWithoutCaption = new Size();
                }
            }

			this.InvalidateMeasure();
		}
		#endregion //CalculateSizeWithoutContent

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region CanExecute
		private bool? CanExecute(ICommand command)
		{
			if (command == ToolWindow.CloseCommand)
				return this.AllowClose;
			else if (command == ToolWindow.MinimizeCommand)
				return this.AllowMinimize;
			else if (command == ToolWindow.MaximizeCommand)
				return this.AllowMaximize;
			else if (command == ToolWindow.RestoreCommand)
			{
				switch (this.WindowState)
				{
					case WindowState.Normal:
						return false;
					case WindowState.Minimized:
						return this.AllowMinimize;
					case WindowState.Maximized:
						return this.AllowMaximize;
				}
			}

			return null;
		}
		#endregion //CanExecute

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region CanExecuteDialogCommand
        private void CanExecuteDialogCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            RoutedCommand rc = e.Command as RoutedCommand;

            if (rc.OwnerType == typeof(Window) && rc.Name == "DialogCancel")
            {
                // if we're not hosted in a window then we need to handle processing this command
                // AS 3/30/09 TFS16355 - WinForms Interop
                // We can't assume based on whether we could be shown in a 
                // Window. Instead we'll check the actual owner.
                //
                //if (ToolWindow.GetOwningWindow(this.Owner) == null)
                if (this.Host != null && !this.Host.IsWindow)
                {
                    e.CanExecute = true;
                    e.Handled = true;
                }
            }
        }
        #endregion //CanExecuteDialogCommand

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ClearWindowOwner
		private void ClearWindowOwner(Window toolWindow)
		{
			Debug.Assert(toolWindow == null || toolWindow is ToolWindowHostWindow);

			if (toolWindow.Owner != null)
				toolWindow.Owner = null;
			else
			{
				IntPtr ownerHwnd = GetOwnerHwnd(this.Owner);

				if (IntPtr.Zero != ownerHwnd)
				{
					WindowInteropHelper wih = new WindowInteropHelper(toolWindow);

					if (wih.Owner == ownerHwnd)
						wih.Owner = IntPtr.Zero;
				}
			}
		}
		#endregion //ClearWindowOwner

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region CoerceMinMaxButtonVisibility
		private static object CoerceMinMaxButtonVisibility(DependencyObject d, object newValue)
		{
			ToolWindow tw = d as ToolWindow;

			if (tw.AllowMinimize == false && tw.AllowMaximize == false)
				return KnownBoxes.VisibilityCollapsedBox;

			return newValue;
		}
		#endregion //CoerceMinMaxButtonVisibility

        #region CoerceMinWidth
		private static object CoerceMinWidth(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			if (content._isCalculatingSizeWithoutContent)
				return 0d;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (content.WindowState != WindowState.Normal)
				return 0d;

			return CoerceMinMaxValue(newValue, 0d, content._calculatedMinWidth);
		}
		#endregion //CoerceMinWidth

		#region CoerceMinHeight
		private static object CoerceMinHeight(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			if (content._isCalculatingSizeWithoutContent)
				return 0d;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (content.WindowState != WindowState.Normal)
				return 0d;

			return CoerceMinMaxValue(newValue, 0d, content._calculatedMinHeight);
		}
		#endregion //CoerceMinHeight

		#region CoerceMaxWidth
		private static object CoerceMaxWidth(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			if (content._isCalculatingSizeWithoutContent)
				return double.PositiveInfinity;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (content.WindowState != WindowState.Normal)
				return double.PositiveInfinity;

			return CoerceMinMaxValue(newValue, double.PositiveInfinity, content._calculatedMaxWidth);
		}
		#endregion //CoerceMaxWidth

		#region CoerceMaxHeight
		private static object CoerceMaxHeight(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			if (content._isCalculatingSizeWithoutContent)
				return double.PositiveInfinity;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (content.WindowState != WindowState.Normal)
				return double.PositiveInfinity;

			return CoerceMinMaxValue(newValue, double.PositiveInfinity, content._calculatedMaxHeight);
		}
		#endregion //CoerceMaxHeight

		#region CoerceWidth
		private static object CoerceWidth(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (content.WindowState != WindowState.Normal)
				return double.NaN;

			// AS 6/24/11 FloatingWindowCaptionSource
			// This isn't specific to this but when showing the non-client area ourselves 
			// the content could be clipped and it can be if the window rounds down.
			//
			double dblValue = (double)newValue;

			if (!double.IsNaN(dblValue) &&
				content.Host != null &&
				content.Host.IsWindow)
			{
				newValue = Math.Round(dblValue, MidpointRounding.AwayFromZero);
			}

			return content._isCalculatingSizeWithoutContent ? double.NaN : newValue;
		}
		#endregion //CoerceMaxWidth

		#region CoerceHeight
		private static object CoerceHeight(DependencyObject d, object newValue)
		{
			ToolWindow content = (ToolWindow)d;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (content.WindowState != WindowState.Normal)
				return double.NaN;

			// AS 6/24/11 FloatingWindowCaptionSource
			// This isn't specific to this but when showing the non-client area ourselves 
			// the content could be clipped and it can be if the window rounds down.
			//
			double dblValue = (double)newValue;

			if (!double.IsNaN(dblValue) &&
				content.Host != null &&
				content.Host.IsWindow)
			{
				newValue = Math.Round(dblValue, MidpointRounding.AwayFromZero);
			}

			return content._isCalculatingSizeWithoutContent ? double.NaN : newValue;
		}
		#endregion //CoerceHeight

		#region CoerceMinMaxValue
		private static object CoerceMinMaxValue(object newValue, double requiredValue, object calculatedValue)
		{
			if (newValue == null || (double)newValue == requiredValue)
			{
				if (null != calculatedValue)
					return calculatedValue;
			}

			return newValue;
		}
		#endregion //CoerceMinMaxValue

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ExecuteCommand
		private bool ExecuteCommand(ICommand command)
		{
			if (this.CanExecute(command) != true)
				return false;

			if (command == ToolWindow.CloseCommand)
				this.Close();
			else
			{
				WindowState? newState = null;

				if (command == ToolWindow.MaximizeCommand)
					newState = WindowState.Maximized;
				else if (command == ToolWindow.MinimizeCommand)
					newState = WindowState.Minimized;
				else if (command == ToolWindow.RestoreCommand)
				{
					if (this.WindowState == WindowState.Minimized && this.RestoreToMaximized)
						newState = WindowState.Maximized;
					else
						newState = WindowState.Normal;
				}

				if (newState != null)
				{
					if (this.Host != null)
						this.Host.SetWindowState(newState.Value);
					else
						this.WindowState = newState.Value;
				}
			}

			return true;
		}
		#endregion //ExecuteCommand

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region ExecuteDialogCommand
        private void ExecuteDialogCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedCommand rc = e.Command as RoutedCommand;

            if (rc.OwnerType == typeof(Window) && rc.Name == "DialogCancel")
            {
                this.DialogResult = false;
                e.Handled = true;
            }
        }
        #endregion //ExecuteDialogCommand

        #region GetAdorner
		private static ToolWindowAdorner GetAdorner(FrameworkElement owner, bool createIfNull)
		{
			UIElement elementToAdorn;
			AdornerLayer adornerLayer = Utilities.GetRootAdornerLayer(owner, out elementToAdorn);

			if (null != adornerLayer)
			{
				ToolWindowAdorner windowAdorner = null;

				// see if we have a toolwindowadorner
				for (int i = 0, count = VisualTreeHelper.GetChildrenCount(adornerLayer); i < count; i++)
				{
					windowAdorner = VisualTreeHelper.GetChild(adornerLayer, i) as ToolWindowAdorner;

					if (null != windowAdorner)
					{
						if (windowAdorner.AdornedElement == elementToAdorn)
							break;

						windowAdorner = null;
					}
				}

				// if we didn't find one then create a new one and add it
				if (createIfNull && windowAdorner == null)
				{
					windowAdorner = new ToolWindowAdorner(elementToAdorn);
					adornerLayer.Add(windowAdorner);
				}

				return windowAdorner;
			}

			return null;
		}
		#endregion //GetAdorner

		#region GetOwningWindow
		private static Window GetOwningWindow(FrameworkElement owner)
		{
			return BrowserInteropHelper.IsBrowserHosted ? null : Window.GetWindow(owner);
		}
		#endregion //GetOwningWindow

		// AS 5/20/08 BR32842
		#region GetPartName
		private static string GetPartName(ToolWindowPart part)
		{
			switch (part)
			{
				case ToolWindowPart.BorderBottom:
					return "PART_BorderBottom";
				case ToolWindowPart.BorderBottomLeft:
					return "PART_BorderBottomLeft";
				case ToolWindowPart.BorderBottomRight:
					return "PART_BorderBottomRight";
				case ToolWindowPart.BorderLeft:
					return "PART_BorderLeft";
				case ToolWindowPart.BorderRight:
					return "PART_BorderRight";
				case ToolWindowPart.BorderTop:
					return "PART_BorderTop";
				case ToolWindowPart.BorderTopLeft:
					return "PART_BorderTopLeft";
				case ToolWindowPart.BorderTopRight:
					return "PART_BorderTopRight";
				case ToolWindowPart.Caption:
					return "PART_Caption";
				case ToolWindowPart.Content:
					return "PART_Content";
				case ToolWindowPart.ResizeGrip:
					return "PART_ResizeGrip";
				default:
					Debug.Fail("Unrecognized part:" + part.ToString());
					return string.Empty;
			}
		}
		#endregion //GetPartName

		#region InitializeBorderThicknessBindings
		private void InitializeBorderThicknessBindings()
		{
			if (this.IsUsingOSNonClientArea)
			{
				this.ClearValue(PreferredBorderWidthProperty);
				this.ClearValue(PreferredBorderHeightProperty);
			}
			else
			{
				bool isResizable = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;

				ResourceKey widthKey = isResizable ? SystemParameters.ResizeFrameVerticalBorderWidthKey : SystemParameters.FixedFrameVerticalBorderWidthKey;
				ResourceKey heightKey = isResizable ? SystemParameters.ResizeFrameHorizontalBorderHeightKey : SystemParameters.FixedFrameHorizontalBorderHeightKey;

				this.SetResourceReference(PreferredBorderWidthProperty, widthKey);
				this.SetResourceReference(PreferredBorderHeightProperty, heightKey);
			}
		} 
		#endregion //InitializeBorderThicknessBindings

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnAllowMinMaxChanged
		private static void OnAllowMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(MinimizeButtonVisibilityProperty);
			d.CoerceValue(MaximizeButtonVisibilityProperty);
		} 
		#endregion //OnAllowMinMaxChanged

		#region OnCanExecuteCommand
		private static void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			ToolWindow window = sender as ToolWindow;

			// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//if (e.Command == ToolWindow.CloseCommand)
			//{
			//    e.CanExecute = window.AllowClose;
			//    e.Handled = true;
			//}
			bool? canExecute = window.CanExecute(e.Command);

			if (null != canExecute)
			{
				e.CanExecute = canExecute.Value;
				e.Handled = true;
			}
		} 
		#endregion //OnCanExecuteCommand

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region OnEndShowDialog
        private void OnEndShowDialog()
        {
            if (this._showingAsDialog)
            {
                this._showingAsDialog = false;
                CommandManager.RemoveCanExecuteHandler(this, new CanExecuteRoutedEventHandler(this.CanExecuteDialogCommand));
                CommandManager.RemoveExecutedHandler(this, new ExecutedRoutedEventHandler(this.ExecuteDialogCommand));
            }
        }
        #endregion //OnEndShowDialog

        #region OnExecuteCommand
		private static void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			ToolWindow window = sender as ToolWindow;

			// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//if (e.Command == ToolWindow.CloseCommand && window.AllowClose)
			//{
			//    window.Close();
			//    e.Handled = true;
			//}
			if (window.ExecuteCommand(e.Command))
			{
				e.Handled = true;
			}
		} 
		#endregion //OnExecuteCommand

		#region OnFlowDirectionChanged
		private static void OnFlowDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ToolWindow)d).VerifyResizeGripHitTestResult();
		}
		#endregion //OnFlowDirectionChanged

        // AS 8/15/08
        #region OnLoaded
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this._isLoadedAfterShow = true;
        }
        #endregion //OnLoaded

        #region OnOwnerLoaded
		private void OnOwnerLoaded(object sender, RoutedEventArgs e)
		{
			Debug.Assert(this.Owner != null);
			this.Owner.Loaded -= new RoutedEventHandler(OnOwnerLoaded);
			// AS 4/28/11 TFS73532
			//this.ShowImpl(this.Owner, this._activateOnShow);
			var deferShow = this.ShowImpl(this.Owner);

			// AS 4/28/11 TFS73532
			Debug.Assert(deferShow != null, "The deferred info should have been prepared");
			if (null != deferShow)
				deferShow.Show(_activateOnShow);
		} 
		#endregion //OnOwnerLoaded

		#region OnOwnerUnloaded
		private void OnOwnerUnloaded(object sender, RoutedEventArgs e)
		{
			// AS 4/30/08
			// We need to close the toolwindow when the owner is unloaded. This will handle
			// the cases where the owner is on a tab/page and you navigate to another tab/page.
			//
			this.Close();
		}
		#endregion //OnOwnerUnloaded

		#region OnPartMouseLeftButtonDown
		private void OnPartMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (null == this.Host)
				return;

			DependencyObject d = sender as DependencyObject;
			ToolWindowPart? part = d != null ? GetToolWindowPart(d) : null;

			if (part.HasValue)
			{
				if (ToolWindowPart.Caption == part.Value)
				{
					if (e.ClickCount == 2)
						e.Handled = this.OnDoubleClickCaption(e.GetPosition(this), e.MouseDevice);
					else if (e.ClickCount == 1)
					{
						// if the derived window handles the drag then do not let the host handle it
						e.Handled = this.OnDragCaption(e.GetPosition(this), e.MouseDevice);
					}
					else
						return;

					if (e.Handled)
						return;

					if (null != this.Host)
					{
                        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
                        //this.Host.DragMove();
						this.Host.DragMove(e);
						e.Handled = true;
					}
				}
				else
				{
					if (e.ClickCount != 1)
						return;

					ToolWindowResizeElementLocation location = ToolWindowResizeElementLocation.Top;

					switch (part.Value)
					{
						case ToolWindowPart.BorderTop:
							location = ToolWindowResizeElementLocation.Top;
							break;
						case ToolWindowPart.BorderLeft:
							location = ToolWindowResizeElementLocation.Left;
							break;
						case ToolWindowPart.BorderRight:
							location = ToolWindowResizeElementLocation.Right;
							break;
						case ToolWindowPart.BorderBottom:
							location = ToolWindowResizeElementLocation.Bottom;
							break;
						case ToolWindowPart.BorderTopLeft:
							location = ToolWindowResizeElementLocation.TopLeft;
							break;
						case ToolWindowPart.BorderTopRight:
							location = ToolWindowResizeElementLocation.TopRight;
							break;
						case ToolWindowPart.BorderBottomLeft:
							location = ToolWindowResizeElementLocation.BottomLeft;
							break;
						case ToolWindowPart.BorderBottomRight:
							location = ToolWindowResizeElementLocation.BottomRight;
							break;
						case ToolWindowPart.ResizeGrip:
							location = this.FlowDirection == FlowDirection.LeftToRight
								? ToolWindowResizeElementLocation.BottomRight
								: ToolWindowResizeElementLocation.BottomLeft;
							break;
					}

					if (null != this.Host)
					{
                        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
                        //this.Host.DragResize(location, (Cursor)d.GetValue(FrameworkElement.CursorProperty));
						this.Host.DragResize(location, (Cursor)d.GetValue(FrameworkElement.CursorProperty), e);
						e.Handled = true;
					}
				}
			}
		}
		#endregion //OnPartMouseLeftButtonDown

		#region OnPartQueryCursor
		private void OnPartQueryCursor(object sender, QueryCursorEventArgs e)
		{
			if (null == this.Host)
				return;

			DependencyObject d = sender as DependencyObject;
			ToolWindowPart? part = d != null ? GetToolWindowPart(d) : null;

			if (part.HasValue)
			{
				if (part.Value == ToolWindowPart.Caption)
					return;

				e.Handled = true;

				switch (part.Value)
				{
					case ToolWindowPart.BorderTop:
					case ToolWindowPart.BorderBottom:
						e.Cursor = Cursors.SizeNS;
						break;
					case ToolWindowPart.BorderLeft:
					case ToolWindowPart.BorderRight:
						e.Cursor = Cursors.SizeWE;
						break;
					case ToolWindowPart.BorderTopLeft:
					case ToolWindowPart.BorderBottomRight:
						e.Cursor = Cursors.SizeNWSE;
						break;
					case ToolWindowPart.BorderTopRight:
					case ToolWindowPart.BorderBottomLeft:
						e.Cursor = Cursors.SizeNESW;
						break;
					case ToolWindowPart.ResizeGrip:
						e.Cursor = this.FlowDirection == FlowDirection.LeftToRight
							? Cursors.SizeNWSE
							: Cursors.SizeNESW;
						break;
				}

				e.Handled = true;
			}
		}
		#endregion //OnPartQueryCursor

		#region OnRelativePositionSettingChanged
		private static void OnRelativePositionSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow window = (ToolWindow)d;

			if (null != window.Host)
			{
				window.CalculateMinMaxExtents(true);
				window.Host.RelativePositionStateChanged();
			}
		} 
		#endregion //OnRelativePositionSettingChanged

		// AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region OnStartShowDialog
        private void OnStartShowDialog()
        {
            this._showingAsDialog = true;
            CommandManager.AddCanExecuteHandler(this, new CanExecuteRoutedEventHandler(this.CanExecuteDialogCommand));
            CommandManager.AddExecutedHandler(this, new ExecutedRoutedEventHandler(this.ExecuteDialogCommand));
        }
        #endregion //OnStartShowDialog

		// AS 11/17/11 TFS91061
		#region OnVisibilityChanged
		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindow tw = d as ToolWindow;

			// The Window is no longer binding to this property once it has been shown.
			// This is necessary because the window cannot have its WindowStyle changed from 
			// None to ToolWindow/SingleBorderWindow when it had its AllowsTransparency set 
			// to true before it was shown. Similarly when changing from 
			// ToolWindow/SingleBorderWindow to None, the AllowsTransparency cannot be set 
			// to true once the Window has been shown or the WPF window class will throw an 
			// exception. To avoid both of these issues, we will just reshow the toolwindow 
			// as we would if the resolved AllowsTransparency is changed.
			//
			if (tw.Host != null && !tw.IsModal && tw.Host.IsWindow && !Visibility.Collapsed.Equals(e.NewValue) && tw.IsUsingOSNonClientArea != tw.Host.IsUsingOsNonClientArea)
			{
				tw.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new MethodInvoker(tw.ReshowIfVisible));
			}
		}
		#endregion //OnVisibilityChanged

		// AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region PrepareForShow
        private void PrepareForShow(FrameworkElement owner, bool modal)
        {
            if (VisualTreeHelper.GetParent(this) != null)
                throw new InvalidOperationException(SR.GetString("LE_ElementHasVisualParent",owner));

            if (null == owner)
                throw new ArgumentNullException("owner");

            Debug.Assert(false == this._showingAsDialog);

            if (this._showingAsDialog)
                throw new InvalidOperationException(SR.GetString("LE_CannotShowOnDialog"));

            if (modal)
            {
                if (false == owner.IsLoaded)
                    throw new InvalidOperationException(SR.GetString("LE_DialogOwnerNotLoaded"));

                if (false == this.SupportsShowDialog)
                    throw new NotSupportedException(SR.GetString("LE_ShowDialogNotSupported"));
            }

            // clear the previous dialog result
            this._dialogResult = null;

            if (false == modal)
            {
                FrameworkElement currentOwner = this.Owner;

                if (null != currentOwner)
                {
                    if (currentOwner != owner)
                        throw new InvalidOperationException(SR.GetString("LE_ToolWindowHasOwner"));

                    // this element is already owned by this element
                    return;
                }
            }

            // keep a reference to the owning element
            this.SetValue(OwnerPropertyKey, owner);

            // AS 8/15/08
            this._isLoadedAfterShow = false;
        }
        #endregion //PrepareForShow

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        // Moved from ShowInToolWindowHostWindow
        #region PrepareShowInHostWindow
        private ToolWindowHostWindow PrepareShowInHostWindow(Window owningWindow)
        {
            Debug.Assert(VisualTreeHelper.GetParent(this) == null);

            // the owner is in a window so we are free to use one as well
            ToolWindowHostWindow toolWindow = new ToolWindowHostWindow();
            toolWindow.Content = this;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moved impl to a helper method and only call it when the new IsOwnedWindow
			// is true, which it is by default to maintain the previous behavior.
			//
			//// setting the owner when the toolwindow is topmost but the owner is not
			//// is causing the owner to get brought to the foreground. however I don't
			//// want to not set the owner since that would prevent the gettoolwindows
			//// method from working. if we need to not set the owner then we probably 
			//// want to look into another way of getting the toolwindows or enumerate
			//// all the windows.
			//toolWindow.Owner = owningWindow;
			//
			//// AS 5/8/09 TFS17053
			//// Set the owner using the WindowInteropHelper if the owner was not in a wpf window.
			////
			//if (owningWindow == null)
			//{
			//    IntPtr ownerHwnd = GetOwnerHwnd(this.Owner);
			//    Debug.Assert(ownerHwnd != IntPtr.Zero);
			//
			//    if (IntPtr.Zero != ownerHwnd)
			//        new WindowInteropHelper(toolWindow).Owner = ownerHwnd;
			//}
			if (this.IsOwnedWindow)
				this.SetWindowOwner(toolWindow, owningWindow);

			// AS 11/2/10 TFS49402/TFS49912/TFS51985
			switch ( this.WindowStartupLocation )
			{
				case ToolWindowStartupLocation.Manual:
					toolWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
					break;
				case ToolWindowStartupLocation.CenterScreen:
					toolWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
					break;
				case ToolWindowStartupLocation.CenterOwnerWindow:
					toolWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
					break;
			}

            // I found this while coding the xdm dragging. Basically I was reusing a toolwindow
            // for a drop preview and the min/max was calculating to the previous size because
            // when we tried to calculate the min/max, it bailed out because the layout was 
            // suspended. If the window's layout is suspended then we would not have been able 
            // to calculate the min/max so do so asynchronously.
            //
            if (this.IsMeasureValid == false)
                this.CalculateMinMaxExtents(true);

			// AS 3/23/11 TFS68833
			Utilities.EnableModelessKeyboardInteropForWindow(toolWindow);
			this.OnWindowShowing(EventArgs.Empty);

            return toolWindow;
        }
        #endregion //PrepareShowInHostWindow

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region Reshow
		private void Reshow()
		{
			// if its not currently shown then exit
			if (null == this.Host)
				return;

			// AS 2/17/12 TFS100638
			// I noticed this while debugging this issue. Since we have a deferred show that hasn't been started 
			// yet there is no need to close and reshow.
			//
			if (_deferredShowInfo != null)
				return;

			if (this.IsModal)
				throw new InvalidOperationException(SR.GetString("LE_CannotChangeModalToolWindowTransparency"));

			if (_isReshowing)
				throw new InvalidOperationException(SR.GetString("LE_ToolWindowIsReshowing"));

			bool isActive = this.IsActive;
			FrameworkElement owner = this.Owner;
			bool preferPopup = this._preferPopup;

			_isReshowing = true;

			try
			{
				this.Close();

				this.Show(owner, isActive, preferPopup);
			}
			finally
			{
				_isReshowing = false;
			}
		}
		#endregion //Reshow

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ReshowIfVisible
		private void ReshowIfVisible()
		{
			// AS 11/17/11 TFS91061
			//if (this.Host != null && this.IsVisible)
			if (this.Host != null && this.IsVisible && this.IsUsingOSNonClientArea != this.Host.IsUsingOsNonClientArea)
				this.Reshow();
		}
		#endregion //ReshowIfVisible

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SetWindowOwner
		private void SetWindowOwner(Window toolWindow, Window owningWindow)
		{
			Debug.Assert(toolWindow == null || toolWindow is ToolWindowHostWindow);

			// setting the owner when the toolwindow is topmost but the owner is not
			// is causing the owner to get brought to the foreground. however I don't
			// want to not set the owner since that would prevent the gettoolwindows
			// method from working. if we need to not set the owner then we probably 
			// want to look into another way of getting the toolwindows or enumerate
			// all the windows.
			toolWindow.Owner = owningWindow;

			// AS 5/8/09 TFS17053
			// Set the owner using the WindowInteropHelper if the owner was not in a wpf window.
			//
			if (owningWindow == null)
			{
				IntPtr ownerHwnd = GetOwnerHwnd(this.Owner);
				Debug.Assert(ownerHwnd != IntPtr.Zero);

				if (IntPtr.Zero != ownerHwnd)
					new WindowInteropHelper(toolWindow).Owner = ownerHwnd;
			}
		}
		#endregion //SetWindowOwner

		#region ShowContextMenu
		/// <summary>
		/// Helper method to show the context menu 
		/// </summary>
		/// <param name="originalSource"></param>
		/// <param name="location"></param>
		/// <param name="showDefaultOnly"></param>
		/// <returns>Returns a boolean indicating whether the action was handled.</returns>
		private bool ShowContextMenu(object originalSource, Point location, bool showDefaultOnly)
		{
			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// When hosted in a window and we are minimized we want the os menu to show
			// so just return false.
			//
			if (this.WindowState == WindowState.Minimized &&
				this.Host != null &&
				this.Host.IsWindow)
			{
				return false;
			}

			// if the menu was explicitly set to null then don't show any menu. if we are supposed
			// to show the menu even if its explicitly set then return true to indicate that the
			// message was "handled"
			if (this.ReadLocalValue(ContextMenuProperty) == null)
				return showDefaultOnly ? false : true;

			ContextMenu menu = this.ContextMenu;

			// if we're only supposed to show one if we create it and there is one then bail
			if (menu != null && showDefaultOnly)
				return false;

			if (menu == null)
			{
				menu = this.CreateDefaultContextMenu(originalSource);

				if (menu == null)
					return false;
			}

            // AS 1/29/08 TFS11854
            // If the menu doesn't have any items then we shouldn't try to show it.
            // I'm going to always return true because we don't want an ancestor 
            // elements (or the system menu) to be shown.
            //
            if (menu.Items.Count == 0)
                return true;

			menu.PlacementTarget = this;

			if (location.X == -1d && location.Y == -1d)
			{
				menu.Placement = PlacementMode.Center;
			}
			else
			{
				Point pt = new Point();

				DependencyObject relativeElement = originalSource as DependencyObject;

				while (relativeElement is ContentElement)
					relativeElement = LogicalTreeHelper.GetParent(relativeElement);

				Debug.Assert(relativeElement is UIElement);

				if (relativeElement is UIElement)
					pt = this.TranslatePoint(pt, (UIElement)relativeElement);

				menu.Placement = PlacementMode.RelativePoint;
				menu.HorizontalOffset = location.X - pt.X;
				menu.VerticalOffset = location.Y - pt.Y;
			}

			// AS 5/23/11 TFS73513
			// If the HwndSource associated with the popup is disposed before the 
			// Closed event is raised (which happens asynchronously usually), then 
			// when the popup call's its DestroyWindow it will not call the 
			// UpdatePlacementTargetRegistration which removes the Popup from the 
			// internal RegisteredPopupsField. The Popup remains rooted by the 
			// original placementtarget; the popup roots the context menu and the 
			// contextmenu roots the menu item that was clicked (actually this is 
			// done by the ItemsControl class via its _focusedItem field). The 
			// menu item roots the ContentPane since properties like the 
			// CommandTarget and CommandParameter are set to the ContentPane.
			//
			menu.Unloaded += delegate(object sender, RoutedEventArgs e)
			{
				((ContextMenu)sender).PlacementTarget = null;
			};

			menu.IsOpen = true;
			return true;
		}
		#endregion //ShowContextMenu

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region ShowModalToolWindowHostWindow
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ShowModalToolWindowHostWindow(Window owningWindow, ShowDialogCallback callback)
        {
            ToolWindowHostWindow toolWindow = this.PrepareShowInHostWindow(owningWindow);

            bool? result = toolWindow.ShowDialog();

            if (null != callback)
                callback(this, result);
        }
        #endregion //ShowModalToolWindowHostWindow

        #region ShowImpl
		// AS 4/28/11 TFS73532
		// Refactored to return an object that can show the toolwindow if its been prepared
		// and removed the activate since that will be up to the caller.
		//
		//private void ShowImpl(FrameworkElement owner, bool activate)
		private IDeferShow ShowImpl(FrameworkElement owner)
		{
			Debug.Assert(owner == this.Owner && owner.IsLoaded);

			if (owner != this.Owner || owner.IsLoaded == false)
				return null;

			// AS 4/28/11 TFS73532
			if (_deferredShowInfo != null)
				throw new InvalidOperationException("A deferred show is already in progress.");

			// AS 3/31/11 TFS70707
			this.CoerceValue(MinimizeButtonVisibilityProperty);
			this.CoerceValue(MaximizeButtonVisibilityProperty);

            // AS 3/30/09 TFS16355 - WinForms Interop
            if (this._preferPopup)
            {
				// AS 4/28/11 TFS73532
				//ToolWindowHostPopup.Show(this, activate);
				//return;
                return ToolWindowHostPopup.Show(this);
            }

			Window owningWindow = GetOwningWindow(owner);

			// AS 5/8/09 TFS17053
			//if (null != owningWindow)
			if (null != owningWindow || GetOwnerHwnd(owner) != IntPtr.Zero)
			{
				// AS 5/14/08 BR32842
				// Cannot reference ToolWindowHostWindow inline.
				//
				// AS 4/28/11 TFS73532
				//ShowInToolWindowHostWindow(activate, owningWindow);
				return ShowInToolWindowHostWindow(owningWindow);
			}
			else
			{
				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				// Found this while debugging. Basically we're trying to show a modeless window
				// in the adorner layer when we have a modal toolwindow shown within a popup.
				//
				ToolWindow ancestorToolWindow = ToolWindow.GetToolWindow(owner);

				if ( ancestorToolWindow != null )
				{
					if ( ancestorToolWindow.Host is ToolWindowHostPopup )
					{
						_preferPopup = true;
						// AS 4/28/11 TFS73532
						//ToolWindowHostPopup.Show(this, activate);
						//return;
						return ToolWindowHostPopup.Show(this);
					}
				}

				ToolWindowAdorner windowAdorner = GetAdorner(owner, true);

				if (null != windowAdorner)
				{
					// AS 4/28/11 TFS73532
					//windowAdorner.AddWindow(this, owner);
					//
					//if (activate)
					//	windowAdorner.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
					return windowAdorner.AddWindow(this, owner);
				}
			}

			// AS 4/28/11 TFS73532
			return null;
		}

		// AS 5/14/08 BR32842
		[MethodImpl(MethodImplOptions.NoInlining)]
		// AS 4/28/11 TFS73532
		// Added return value and removed activate since the caller will deal with that.
		//
		//private void ShowInToolWindowHostWindow(bool activate, Window owningWindow)
		private IDeferShow ShowInToolWindowHostWindow(Window owningWindow)
		{
            // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
            // Moved impl to helper method.
            //
            ToolWindowHostWindow toolWindow = this.PrepareShowInHostWindow(owningWindow);

			// AS 4/28/11 TFS73532
			//toolWindow.Show(activate);
			return new ToolWindowHostWindow.DeferShowHelper(toolWindow);
		}

		#endregion //Show

		#region UpdatePreferredBorderThickness
		private void UpdatePreferredBorderThickness()
		{
			Thickness thickness = new Thickness();

			if (this.IsUsingOSNonClientArea == false)
			{
				thickness.Left = thickness.Right = this.PreferredBorderWidth;
				thickness.Top = thickness.Bottom = this.PreferredBorderHeight;
			}

			this.SetValue(PreferredBorderThicknessPropertyKey, thickness);
		} 
		#endregion //UpdatePreferredBorderThickness

		#region VerifyResizeGripHitTestResult
		private void VerifyResizeGripHitTestResult()
		{
            // AS 1/29/09 TFS13199
            // While debugging this issue I found this bug where we were accessing the 
            // _parts member before its initialized.
            //
            if (null == _parts)
                return;

			FrameworkElement resizeGrip = this._parts[(int)ToolWindowPart.ResizeGrip];

			if (null != resizeGrip)
			{
				ToolWindowPart part = this.FlowDirection == FlowDirection.LeftToRight
					? ToolWindowPart.BorderBottomRight
					: ToolWindowPart.BorderBottomLeft;

				resizeGrip.SetValue(HitTestCodePropertyKey, NativeWindowMethods.GetHitTestResult(part));
			}
		}
		#endregion //VerifyResizeGripHitTestResult

		#endregion //Private Methods

		#region Protected Methods

		#region CalculateNonClientSize
		/// <summary>
		/// Adds the non-client size to the specified client size.
		/// </summary>
		/// <param name="clientSize">The client size to adjust.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected Size CalculateNonClientSize(Size clientSize)
		{
			if (this.IsUsingOSNonClientArea)
			{
				// AS 5/14/08 BR32842
				// Cannot reference ToolWindowHostWindow inline.
				clientSize = AddToolWindowHostNonClient(clientSize);
			}
			else
			{
				if (this._sizeWithoutCaption.IsEmpty == false)
				{
					clientSize.Width += this._sizeWithoutCaption.Width;

					// AS 8/25/11 TFS84720
					// The caption is part of the non-client area. At least for now we will assume that the 
					// caption is at the top of the content and therefore we can add in the height of the 
					// size without the content so the caption height is considered as well as the resizable 
					// border height.
					//
					//clientSize.Height += this._sizeWithoutCaption.Height;
					clientSize.Height += this._sizeWithoutContent.Height;
				}
			}
			return clientSize;
		}

		// AS 5/14/08 BR32842
		[MethodImpl(MethodImplOptions.NoInlining)]
		private Size AddToolWindowHostNonClient(Size clientSize)
		{
			Size newSize = clientSize;

			ToolWindowHostWindow host = this.Host as ToolWindowHostWindow;

			if (null != host)
				newSize = host.AddNonClientSize(clientSize);

			return newSize;
		}
		#endregion //CalculateNonClientSize

		#region CreateDefaultContextMenu
		/// <summary>
		/// Invoked when the context menu for the class is about to be shown and one has not been specified.
		/// </summary>
		/// <param name="originalSource">The original source for the context menu request</param>
		/// <returns>Returns null by default</returns>
		protected virtual ContextMenu CreateDefaultContextMenu(object originalSource)
		{
			return null;
		}
		#endregion //CreateDefaultContextMenu

		// AS 9/11/09 TFS21330
		#region OnSupportsAllowsTransparencyChanged
		/// <summary>
		/// Invoked when the derived class has a different value for the SupportsAllowsTransparency property.
		/// </summary>
		protected void OnSupportsAllowsTransparencyChanged()
		{
			// if its not currently shown then exit
			if (null == this.Host)
				return;

			// if the resolved state is the current state do nothing
			if (this.AllowsTransparencyResolved == this.Host.AllowsTransparency)
				return;

			// if the host cannot show with the new state then stay the same
			if (!this.Host.SupportsAllowTransparency(this.AllowsTransparencyResolved))
				return;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moved to a helper routine.
			//
			//if (this.IsModal)
			//    throw new InvalidOperationException(SR.GetString("LE_CannotChangeModalToolWindowTransparency"));
			//
			//if (_isReshowing)
			//    throw new InvalidOperationException(SR.GetString("LE_ToolWindowIsReshowing"));
			//
			//bool isActive = this.IsActive;
			//FrameworkElement owner = this.Owner;
			//bool preferPopup = this._preferPopup;
			//
			//_isReshowing = true;
			//
			//try
			//{
			//    this.Close();
			//
			//    this.Show(owner, isActive, preferPopup);
			//}
			//finally
			//{
			//    _isReshowing = false;
			//}
			this.Reshow();
		} 
		#endregion //OnSupportsAllowsTransparencyChanged

		// AS 8/24/09 TFS19861
		// Added a way for a derived toolwindow to opt out of using AllowsTransparency
		// in whatever element hosts the ToolWindow. This is needed because HwndHost 
		// doesn't support being contained in a Window/Popup where AllowsTransparency 
		// is true.
		//
		#region SupportsAllowsTransparency
		/// <summary>
		/// Returns a boolean indicating whether the class supports being shown in a host where AllowsTransparency is true.
		/// </summary>
		protected virtual bool SupportsAllowsTransparency
		{
			get { return true; }
		}
		#endregion //SupportsAllowsTransparency

		// AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region SupportsShowDialog
        /// <summary>
        /// Returns a boolean indicating whether the class supports being shown modally.
        /// </summary>
        protected virtual bool SupportsShowDialog
        {
            get { return true; }
        }
        #endregion //SupportsShowDialog

		#endregion //Protected Methods

        #region VisualState... Methods


        // JJD 4/26/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            if (this.IsActive)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);
         }

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ToolWindow toolwindow = target as ToolWindow;

            toolwindow.UpdateVisualStates();
        }

        // JJD 4/023/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

		#region Public Methods

		#region Activate
		/// <summary>
		/// Activates the containing window.
		/// </summary>
		public void Activate()
		{
			if (null != this.Host)
				this.Host.Activate();
		} 
		#endregion //Activate

		#region BringToFront
		/// <summary>
		/// Brings the window to the front of the z-order.
		/// </summary>
		public void BringToFront()
		{
			if (this.IsVisible && this.Owner != null)
			{
				
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				if (null != this.Host)
					this.Host.BringToFront();
			}
		}
		#endregion //BringToFront

		#region Close
		/// <summary>
		/// Closed the window.
		/// </summary>
		public void Close()
		{
			// AS 4/28/11 TFS73532
			// handle the case where a deferred show was initiated
			// but was cancelled before shown.
			//
			if (null != _deferredShowInfo)
			{
				_deferredShowInfo.Cancel();
			}

			// AS 9/21/11 TFS88634
			// Moved to a helper routine.
			//
			//if (null != this.Host)
			//    this.Host.Close();
			//else if (null != this.Owner)
			//{
			//    // AS 12/18/08 TFS11401
			//    // If the ToolWindow hadn't been shown then we would
			//    // have an owner and would be waiting for its Loaded
			//    // but we wouldn't have a Host.
			//    //
			//    this.ClearValue(OwnerPropertyKey);
			//}
			this.CloseOverride();
		} 
		#endregion //Close

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region GetRestoreBounds
		/// <summary>
		/// Returns the bounds to which the window will be positioned when the WindowState is changed from Minimized or Maximized to Normal.
		/// </summary>
		/// <returns>The bounds with which the window will be positioned when the WindowState is restored to Normal.</returns>
		public Rect GetRestoreBounds()
		{
			return this.GetRestoreBounds(true);
		} 
		#endregion //GetRestoreBounds

		#region GetScreenPoint
		/// <summary>
		/// Helper method for returning a point relative to the screen for the specified owner.
		/// </summary>
		/// <param name="owner">The element that would be the owner of the <see cref="ToolWindow"/></param>
		/// <param name="e">The mouse event args</param>
		/// <returns>A point relative to the logical screen for the specified owner. This value can be used to initialize the Top and Left properties of the tool window.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Point GetScreenPoint(FrameworkElement owner, MouseEventArgs e)
		{
            
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

            Utilities.ThrowIfNull(owner, "owner");
            Utilities.ThrowIfNull(e, "e");

            return GetScreenPoint(owner, e.GetPosition(owner), owner);
        }

        // AS 2/12/09 TFS11410
        // Added an overload that would take relative points instead of just a MouseEventArgs.
        //
		/// <summary>
		/// Helper method for returning a point relative to the screen for the specified owner.
		/// </summary>
		/// <param name="owner">The element that would be the owner of the <see cref="ToolWindow"/></param>
        /// <param name="relativePoint">A point relative to the <paramref name="relativeElement"/></param>
        /// <param name="relativeElement">The element to which the <paramref name="relativePoint"/> is relative or null if the relativePoint is in screen coordinates.</param>
		/// <returns>A point relative to the logical screen for the specified owner. This value can be used to initialize the Top and Left properties of the tool window.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Point GetScreenPoint(FrameworkElement owner, Point relativePoint, FrameworkElement relativeElement)
        {
            Utilities.ThrowIfNull(owner, "owner");

            Window window = GetOwningWindow(owner);

            // we need the point relative to the owner element
            Point ownerRelativePoint = relativePoint;

            // if the relative element is different...
            if (relativeElement != owner)
            {
                // if we're not given screen coordinates, then convert to screen based on the relative element
                if (relativeElement != null)
                    ownerRelativePoint = Utilities.PointToScreenSafe(relativeElement, ownerRelativePoint);

                // then convert from screen to the owner's coordinate system
                ownerRelativePoint = Utilities.PointFromScreenSafe(owner, ownerRelativePoint);
            }

			// AS 5/8/09 TFS17053
			//if (null != window)
            if (null != window || GetOwnerHwnd(owner) != IntPtr.Zero)
            {
                // AS 2/12/09 TFS11410
                //// AS 7/9/08 BR34723
                ////return owner.PointToScreen(e.GetPosition(owner));
                //Point pt = owner.PointToScreen(e.GetPosition(owner));
                Point pt = owner.PointToScreen(ownerRelativePoint);
                return Utilities.ConvertToLogicalPixels((int)pt.X, (int)pt.Y, owner);
            }
            else
            {
                
                
                
                
                
                
                

                UIElement elementToAdorn;
                AdornerLayer adornerLayer = Utilities.GetRootAdornerLayer(owner, out elementToAdorn);

                if (null != elementToAdorn)
                {
                    // AS 2/12/09 TFS11410
                    //return e.GetPosition(elementToAdorn);
                    Point ownerScreenPoint = Utilities.PointToScreenSafe(owner, ownerRelativePoint);
                    return Utilities.PointFromScreenSafe(elementToAdorn, ownerScreenPoint);
                }
                else
                {
                    // AS 2/12/09 TFS11410
                    //return e.GetPosition(owner);
                    return ownerRelativePoint;
                }
            }
        }
		#endregion //GetScreenPoint

		#region GetToolWindows
		/// <summary>
		/// Returns an array of the <see cref="ToolWindow"/> instances displayed for the specified owner.
		/// </summary>
		/// <param name="owner">The owning element for which one or more <see cref="ToolWindow"/> instances have been shown.</param>
		/// <returns>An array of the ToolWindows. The list is sorted based on the zorder where the first item is topmost and the last item is bottommost.</returns>
		public static ToolWindow[] GetToolWindows(FrameworkElement owner)
		{
			// AS 1/10/12 TFS90890
			int ownerWindowIndex;
			return GetToolWindows(owner, out ownerWindowIndex);
		}

		// AS 1/10/12 TFS90890
		// Added overload that would indicate where the owning window is relative to the toolwindows since they may be unowned.
		//
		internal static ToolWindow[] GetToolWindows(FrameworkElement owner, out int ownerWindowIndex)
		{
            Utilities.ThrowIfNull(owner, "owner");

			List<ToolWindow> windows = new List<ToolWindow>();

            
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

            Window owningWindow = GetOwningWindow(owner);

			// AS 5/8/09 TFS17053
			//if (null != owningWindow)
            if (null != owningWindow || GetOwnerHwnd(owner) != IntPtr.Zero)
            {
                // AS 5/14/08 BR32842
                // Cannot reference ToolWindowHostWindow inline.
				GetToolWindowHostWindows(owner, windows, owningWindow, out ownerWindowIndex );
            }
            else
            {
                ToolWindowHostPopup.AddToolWindows(owner, windows);
				ownerWindowIndex = windows.Count; // AS 1/10/12 TFS90890
            }

            // lastly add the adorner elements since those will always
            // be below the floating and popups
            ToolWindowAdorner adorner = GetAdorner(owner, false);

            if (null != adorner)
            {
				adorner.GetToolWindows(owner, windows, ref ownerWindowIndex );
            }

			return windows.ToArray();
		}

		// AS 5/14/08 BR32842
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void GetToolWindowHostWindows(FrameworkElement owner, List<ToolWindow> windows, Window owningWindow, out int ownerWindowIndex )
		{
			ToolWindowHostWindow.GetToolWindows(owningWindow, owner, windows, out ownerWindowIndex );
		} 
		#endregion //GetToolWindows

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SetRestoreBounds
		/// <summary>
		/// Sets the bounds with which the window will be positioned when the WindowState is changed from Minimized or Maximized to Normal.
		/// </summary>
		/// <param name="restoreBounds">The bounds to which the window will be restored.</param>
		/// <exception cref="InvalidOperationException">The <paramref name="restoreBounds"/> cannot be Rect.Empty.</exception>
		public void SetRestoreBounds(Rect restoreBounds)
		{
			if (restoreBounds.IsEmpty)
				throw new InvalidOperationException();

			this.SetRestoreBounds(restoreBounds, true);
		} 
		#endregion //SetRestoreBounds

		#region Show
		/// <summary>
		/// Displays a <see cref="ToolWindow"/>
		/// </summary>
		/// <param name="owner">The owning element. This element will be used to determine the owner of the window or adorner layer depending upon how the content will be shown.</param>
		public void Show(FrameworkElement owner)
		{
			this.Show(owner, true);
		}

		/// <summary>
		/// Displays a <see cref="ToolWindow"/>
		/// </summary>
		/// <param name="owner">The owning element. This element will be used to determine the owner of the window or adorner layer depending upon how the content will be shown.</param>
		/// <param name="activate">True if the window should be activated during the show; false to prevent activation of the new window.</param>
        public void Show(FrameworkElement owner, bool activate)
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            this.Show(owner, activate, false);
        }

        // AS 3/30/09 TFS16355 - WinForms Interop
        // Added overload to allow displaying within a popup.
        //
		/// <summary>
		/// Displays a <see cref="ToolWindow"/>
		/// </summary>
		/// <param name="owner">The owning element. This element will be used to determine the owner of the window or adorner layer depending upon how the content will be shown.</param>
		/// <param name="activate">True if the window should be activated during the show; false to prevent activation of the new window.</param>
        /// <param name="preferPopup">True to use a Popup to host the ToolWindow if possible.</param>
		public void Show(FrameworkElement owner, bool activate, bool preferPopup)
		{
			// AS 4/28/11 TFS73532
			// Moved the implementation into a helper method that can be used to do the actual show asynchronously.
			// Rather than have 2 different implementations we'll just use that deferred method and then dispose the 
			// returned disposable which will cause the show to be invoked. If it comes back null then we're waiting 
			// for the loaded event which will be deferred but not something we can control.
			//
			IDisposable showHelper = this.ShowAsync(owner, activate, preferPopup);

			if (null != showHelper)
				showHelper.Dispose();
		}

		// AS 4/28/11 TFS73532
		// Moved the impl from the Show(FrameworkElement, bool, bool) here.
		//
		/// <summary>
		/// Prepares a ToolWindow to be shown but defers the actual show/activate implementation until the returned object is disposed.
		/// </summary>
		/// <param name="owner">The owning element. This element will be used to determine the owner of the window or adorner layer depending upon how the content will be shown.</param>
		/// <param name="activate">True if the window should be activated during the show; false to prevent activation of the new window.</param>
		/// <param name="preferPopup">True to use a Popup to host the ToolWindow if possible.</param>
		/// <returns>A disposable object that when disposed will show the toolwindow that has been prepared for the show or null if the specified owner is not loaded.</returns>
		internal IDisposable ShowAsync(FrameworkElement owner, bool activate, bool preferPopup)
		{
            
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            this.PrepareForShow(owner, false);

			// store whether the window should be activated so we know how to handle it 
			// if we have to wait for the load event
			this._activateOnShow = activate;

            // AS 3/30/09 TFS16355 - WinForms Interop
            this._preferPopup = preferPopup;

			// AS 4/28/11 TFS73532
			// This isn't specific to this issue. I moved this out of the if block below 
			// since it is possible that we got in here when the owner wasn't loaded and 
			// hooked the loaded event but then got the show call before the loaded event 
			// but while we were hooked in.
			//
			// AS 12/18/08 TFS11401
            // This wasn't related to the issue but I noticed that previously
            // (i.e. before the ShowDialog refactoring moving the top code to 
            // the PrepareForShow method), we would have returned out if Show
            // was called twice for the same owner but since we don't know that
            // the owner hadn't changed we would end up hooking its Loaded 
            // twice. To avoid this, we'll unhook first just to make sure.
            //
			owner.Loaded -= new RoutedEventHandler(OnOwnerLoaded);

			// if the owner isn't loaded then we need to wait until it is loaded
			if (owner.IsLoaded == false)
			{
				// AS 4/28/11 TFS73532
				// Moved out of if block.
				//
				//// AS 12/18/08 TFS11401
				//// This wasn't related to the issue but I noticed that previously
				//// (i.e. before the ShowDialog refactoring moving the top code to 
				//// the PrepareForShow method), we would have returned out if Show
				//// was called twice for the same owner but since we don't know that
				//// the owner hadn't changed we would end up hooking its Loaded 
				//// twice. To avoid this, we'll unhook first just to make sure.
				////
				//owner.Loaded -= new RoutedEventHandler(OnOwnerLoaded);

				owner.Loaded += new RoutedEventHandler(OnOwnerLoaded);
				return null;
			}

			// AS 4/28/11 TFS73532
			//this.ShowImpl(owner, activate);
			var deferShow = this.ShowImpl(owner);
			Debug.Assert(null != deferShow, "The deferred info should have been prepared by the call");

			if (null == deferShow)
				return null;

			_deferredShowInfo = new DeferredShowInfo(this, activate, deferShow);
			return _deferredShowInfo;
		}
		#endregion //Show

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region ShowDialog
        /// <summary>
        /// Displays a <see cref="ToolWindow"/> modally
        /// </summary>
        /// <param name="owner">The owning element. This element will be used to determine the owner of the window or adorner layer depending upon how the content will be shown.</param>
        /// <param name="callback">The callback used to indicate when the dialog has been closed and provide the dialog result. This is needed because the call may not be blocking. For example, when used in an xbap environment, the method will return 
        /// before the dialog has been closed. In this case, the callback will be called when the dialog is closed.</param>
        public void ShowDialog(FrameworkElement owner, ShowDialogCallback callback)
        {
            this.PrepareForShow(owner, true);

            // AS 3/30/09 TFS16355 - WinForms Interop
            this._preferPopup = false;

            Window owningWindow = GetOwningWindow(owner);

            this.OnStartShowDialog();

			// AS 5/8/09 TFS17053
			// We can also use a wpf window if we have the rights to get to the hwnd.
			//
			//if (null != owningWindow)
			if (null != owningWindow || GetOwnerHwnd(owner) != IntPtr.Zero)
            {
                this.ShowModalToolWindowHostWindow(owningWindow, callback);
            }
            else
            {
                ToolWindowHostPopup.ShowDialog(this, callback);
            }
        }
        #endregion //ShowDialog

		#endregion //Public Methods

		#endregion //Methods

		#region Commands

		/// <summary>
		/// Closes the containing window.
		/// </summary>
		public static readonly RoutedCommand CloseCommand = new RoutedCommand("Close", typeof(ToolWindow));

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		/// <summary>
		/// Minimizes the containing window.
		/// </summary>
		public static readonly RoutedCommand MinimizeCommand = new RoutedCommand("Minimize", typeof(ToolWindow));

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		/// <summary>
		/// Maximizes the containing window.
		/// </summary>
		public static readonly RoutedCommand MaximizeCommand = new RoutedCommand("Maximize", typeof(ToolWindow));

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		/// <summary>
		/// Restores the containing window to the Normal <see cref="WindowState"/>.
		/// </summary>
		public static readonly RoutedCommand RestoreCommand = new RoutedCommand("Restore", typeof(ToolWindow));

		#endregion //Commands

        #region Events

        #region Event Objects

        private static readonly object EventActivated = new object();
		private static readonly object EventDeactivated = new object();
		private static readonly object EventClosing = new object();
		private static readonly object EventClosed = new object();
		private static readonly object EventWindowShowing = new object(); // AS 3/23/11 TFS68833

		#endregion //Event Objects

		#region Event Declarations
		/// <summary>
		/// Event that occurs when the window becomes the active window.
		/// </summary>
		//[Description("Occurs when the window becomes the active window.")]
		//[Category("Behavior")]
		public event EventHandler Activated
		{
			add { this.Events.AddHandler(ToolWindow.EventActivated, value); }
			remove { this.Events.RemoveHandler(ToolWindow.EventActivated, value); }
		}

		/// <summary>
		/// Event that occurs when the window is no longer the active window.
		/// </summary>
		//[Description("Occurs when the window is no longer the active window.")]
		//[Category("Behavior")]
		public event EventHandler Deactivated
		{
			add { this.Events.AddHandler(ToolWindow.EventDeactivated, value); }
			remove { this.Events.RemoveHandler(ToolWindow.EventDeactivated, value); }
		}

		/// <summary>
		/// Event that occurs when the window is about to be closed.
		/// </summary>
		//[Description("Occurs when the window is about to be closed.")]
		//[Category("Behavior")]
		public event CancelEventHandler Closing
		{
			add { this.Events.AddHandler(ToolWindow.EventClosing, value); }
			remove { this.Events.RemoveHandler(ToolWindow.EventClosing, value); }
		}

		/// <summary>
		/// Event that occurs when the window has been closed.
		/// </summary>
		//[Description("Occurs when the window has been closed.")]
		//[Category("Behavior")]
		public event EventHandler Closed
		{
			add { this.Events.AddHandler(ToolWindow.EventClosed, value); }
			remove { this.Events.RemoveHandler(ToolWindow.EventClosed, value); }
		}

		// AS 3/23/11 TFS68833
		internal event EventHandler WindowShowing
		{
			add { this.Events.AddHandler(ToolWindow.EventWindowShowing, value); }
			remove { this.Events.RemoveHandler(ToolWindow.EventWindowShowing, value); }
		}

		#endregion //Event Declarations

		#region Protected virtual OnXXX

		#region OnActivated
		/// <summary>
		/// Raises the <see cref="Activated"/> event.
		/// </summary>
		/// <param name="e">A <see cref="EventArgs"/> that provides data for the event.</param>
		/// <remarks>
		/// <p class="body">Raising an event invokes the event handler through a delegate.</p>
		/// <p class="body">The <b>OnActivated</b> method also allows derived classes to handle the event without attaching a delegate. This is the preferred technique for handling the event in a derived class.</p>
		/// <p class="note">Notes to Inheritors:  When overriding <b>OnActivated</b> in a derived class, be sure to call the base class's <b>OnActivated</b> method so that registered delegates receive the event.</p>
		/// </remarks>
		/// <seealso cref="Activated"/>
		protected virtual void OnActivated(EventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			EventHandler eDelegate = (EventHandler)Events[ToolWindow.EventActivated];

			if (eDelegate != null)
				eDelegate(this, e);
		}
		#endregion //OnActivated

		#region OnClosed
		/// <summary>
		/// Raises the <see cref="Closed"/> event.
		/// </summary>
		/// <param name="e">A <see cref="EventArgs"/> that provides data for the event.</param>
		/// <remarks>
		/// <p class="body">Raising an event invokes the event handler through a delegate.</p>
		/// <p class="body">The <b>OnClosed</b> method also allows derived classes to handle the event without attaching a delegate. This is the preferred technique for handling the event in a derived class.</p>
		/// <p class="note">Notes to Inheritors:  When overriding <b>OnClosed</b> in a derived class, be sure to call the base class's <b>OnClosed</b> method so that registered delegates receive the event.</p>
		/// </remarks>
		/// <seealso cref="Closed"/>
		protected virtual void OnClosed(EventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			EventHandler eDelegate = (EventHandler)Events[ToolWindow.EventClosed];

			if (eDelegate != null)
				eDelegate(this, e);
		}
		#endregion //OnClosed

		#region OnClosing
		/// <summary>
		/// Raises the <see cref="Closing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="EventArgs"/> that provides data for the event.</param>
		/// <remarks>
		/// <p class="body">Raising an event invokes the event handler through a delegate.</p>
		/// <p class="body">The <b>OnClosing</b> method also allows derived classes to handle the event without attaching a delegate. This is the preferred technique for handling the event in a derived class.</p>
		/// <p class="note">Notes to Inheritors:  When overriding <b>OnClosing</b> in a derived class, be sure to call the base class's <b>OnClosing</b> method so that registered delegates receive the event.</p>
		/// </remarks>
		/// <seealso cref="Closing"/>
		internal protected virtual void OnClosing(CancelEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			CancelEventHandler eDelegate = (CancelEventHandler)Events[ToolWindow.EventClosing];

			if (eDelegate != null)
				eDelegate(this, e);
		}
		#endregion //OnClosing

		#region OnDeactivated
		/// <summary>
		/// Raises the <see cref="Deactivated"/> event.
		/// </summary>
		/// <param name="e">A <see cref="EventArgs"/> that provides data for the event.</param>
		/// <remarks>
		/// <p class="body">Raising an event invokes the event handler through a delegate.</p>
		/// <p class="body">The <b>OnDeactivated</b> method also allows derived classes to handle the event without attaching a delegate. This is the preferred technique for handling the event in a derived class.</p>
		/// <p class="note">Notes to Inheritors:  When overriding <b>OnDeactivated</b> in a derived class, be sure to call the base class's <b>OnDeactivated</b> method so that registered delegates receive the event.</p>
		/// </remarks>
		/// <seealso cref="Deactivated"/>
		protected virtual void OnDeactivated(EventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			EventHandler eDelegate = (EventHandler)Events[ToolWindow.EventDeactivated];

			if (eDelegate != null)
				eDelegate(this, e);
		}
		#endregion //OnDeactivated

		// AS 3/23/11 TFS68833
		#region OnWindowShowing
		internal void OnWindowShowing(EventArgs e)
		{
			Utilities.ValidateNotNull(e, "e");

			EventHandler eDelegate = (EventHandler)Events[ToolWindow.EventWindowShowing];

			if (eDelegate != null)
				eDelegate(this, e);
		} 
		#endregion //OnWindowShowing

		#endregion //Protected virtual OnXXX

		#endregion //Events

		#region MethodInvoker

		internal delegate void MethodInvoker();

		#endregion //MethodInvoker

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        #region ShowDialogCallback

        /// <summary>
        /// Delegate used for the <see cref="ShowDialog"/> method
        /// </summary>
        /// <param name="window">The window that was shown using the <see cref="ShowDialog"/> method.</param>
        /// <param name="dialogResult">The result when the dialog was closed.</param>
        public delegate void ShowDialogCallback(ToolWindow window, bool? dialogResult);

        #endregion //ShowDialogCallback

		#region IMinMaxWatcherOwner Members

		void IMinMaxWatcherOwner.OnMinMaxChanged(MinMaxWatcher watcher)
		{
			this.CalculateMinMaxExtents(true);
		}

		#endregion //IMinMaxWatcherOwner Members

		// AS 4/28/11 TFS73532
		#region DeferredShowInfo class
		private class DeferredShowInfo : IDisposable
		{
			#region Member Variables

			private bool _activate;
			private bool? _result;
			private ToolWindow _toolWindow;
			private IDeferShow _deferShow;

			#endregion //Member Variables

			#region Constructor
			internal DeferredShowInfo(ToolWindow toolWindow, bool activate, IDeferShow deferShow)
			{
				Utilities.ValidateNotNull(toolWindow, "toolWindow");
				Utilities.ValidateNotNull(deferShow, "deferShow");

				_toolWindow = toolWindow;
				_activate = activate;
				_deferShow = deferShow;
			}
			#endregion //Constructor

			#region Properties

			#region IsCancelled
			internal bool IsCancelled
			{
				get { return _result.HasValue && _result.Value == false; }
			}
			#endregion //IsCancelled

			#endregion //Properties

			#region Internal Methods

			#region Cancel
			internal void Cancel()
			{
				this.ProcessShow(true);
			}
			#endregion //Cancel

			#region Show
			internal void Show()
			{
				this.ProcessShow(false);
			}
			#endregion //Show

			#endregion //Internal Methods

			#region Private Methods

			#region ProcessShow
			private void ProcessShow(bool cancel)
			{
				Debug.Assert(_result == null || _result.Value == cancel, "Already ended but with a different cancel value?");

				if (_result != null)
					return;

				Debug.Assert(_toolWindow._deferredShowInfo == this, "Deferred show info differs?");

				_result = cancel;

				if (cancel)
				{
					_deferShow.Cancel();
				}
				else
				{
					_deferShow.Show(_activate);
				}

				// we're going to suppress the closed event while cancelling so 
				// clear the member after the cancel/show. 
				_toolWindow._deferredShowInfo = null;
			}
			#endregion //ProcessShow

			#endregion //Private Methods

			#region IDisposable Members
			void IDisposable.Dispose()
			{
				this.ProcessShow(false);
			}
			#endregion //IDisposable Members
		} 
		#endregion //DeferredShowInfo class

		// AS 4/28/11 TFS73532
		#region IDeferShow interface
		internal interface IDeferShow
		{
			void Show(bool activate);
			void Cancel();
		}
		#endregion //IDeferShow interface
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