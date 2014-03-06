//#define DEBUG_ANIMATIONS




using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Windows.Controls;
using System.Windows.Interop;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Stylable element used to represent the flyout from an <see cref="UnpinnedTabArea"/> that will host an unpinned <see cref="ContentPane"/> when it is visible to the end user.
	/// </summary>
	//[ToolboxItem(false)]

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager

    [TemplateVisualState(Name = VisualStateUtilities.StateBottom,    GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,      GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,     GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateTop,       GroupName = VisualStateUtilities.GroupLocation )]


	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class UnpinnedTabFlyout : Control
	{
		#region Member Variables

		private UnpinnedFlyoutState _currentState = UnpinnedFlyoutState.Closed;
		private Storyboard _currentStoryboard;
		private ScaleTransform _scaleTransform;
		private TranslateTransform _translateTransform;

		private const double AutoHideDelay = 500d;
		private const double ShowDurationTime = 250d;
		private const double HideDurationTime = 250d;

		private DispatcherTimer _showDelayTimer;

		// AS 9/11/09 TFS21329
		private DispatcherTimer _mouseOverTimer;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor
		static UnpinnedTabFlyout()
		{
			// the element will be hidden until needed so start it off hidden
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(typeof(UnpinnedTabFlyout)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));

			XamDockManager.DockManagerPropertyKey.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDockManagerChanged)));
			UIElement.RenderTransformOriginProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceRenderTransformOrigin)));
			UIElement.RenderTransformProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceRenderTransform)));

			// AS 6/15/12 TFS114790
			// When the extent is coming from the FlyoutExtent set on a pane, that may be too large. E.g. we may have 
			// had lots of space when it was previously shown and sized but then the available space was reduced (e.g. 
			// because the window was rsized smaller) so we should constrain the extent based on the available space.
			//
			FrameworkElement.WidthProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceWidth)));
			FrameworkElement.HeightProperty.OverrideMetadata(typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceHeight)));




		}

		/// <summary>
		/// Initializes a new <see cref="UnpinnedTabFlyout"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This class is instantiated by the <see cref="XamDockManager"/>. The constructor is only public for styling purposes.</p>
		/// </remarks>
		public UnpinnedTabFlyout()
		{
			// AS 9/11/09 TFS21329
			this.Loaded += new RoutedEventHandler(OnLoaded);
		}
		#endregion //Constructor

		#region Base class overrides

		// AS 6/15/12 TFS114790
		// This is primarily to handle the case where the flyout is hosted in a popup and therefore isn't 
		// constrained by the dockmanagerpanel itself. In that case we want the same layout behavior - 
		// that the flyout is constrained based on the dockmanager panel size.
		//
		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			var dm = XamDockManager.GetDockManager(this);
			var dmPanel = dm != null ? dm.DockPanel : null;

			if (null != dmPanel)
			{
				switch (this.Side)
				{
					case Dock.Left:
					case Dock.Right:
						{
							var extent = constraint.Width;

							if (this.ConstrainFlyoutExtent(ref extent, this.MaxFlyoutWidth, true))
								constraint.Width = extent;
							break;
						}
					case Dock.Top:
					case Dock.Bottom:
						{
							var extent = constraint.Height;

							if (this.ConstrainFlyoutExtent(ref extent, this.MaxFlyoutHeight, false))
								constraint.Height = extent;
							break;
						}
				}
			}

			return base.MeasureOverride(constraint);
		}
		#endregion //MeasureOverride

        #region OnApplyTemplate

        /// <summary>
        /// Invoked when the template for the control has been changed.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

        #endregion //OnApplyTemplate

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (null != this.Pane)
				UnpinnedTabFlyout.ShowFlyoutOnMouseEnter(this.Pane);
		}
		#endregion //OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (null != this.Pane)
				UnpinnedTabFlyout.HideFlyoutOnMouseLeave(this.Pane);
		}
		#endregion //OnMouseLeave

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Pane

		private static readonly DependencyPropertyKey PanePropertyKey =
			DependencyProperty.RegisterReadOnly("Pane",
			typeof(ContentPane), typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPaneChanged)));

		private static void OnPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContentPane pane = e.NewValue as ContentPane;
			UnpinnedTabFlyout flyout = (UnpinnedTabFlyout)d;

			// AS 4/29/08 BR32006
			ContentPane oldPane = e.OldValue as ContentPane;

			if (null != oldPane)
				oldPane.RemoveHandler(ContentPane.ClosedEvent, new RoutedEventHandler(flyout.OnFlyoutPaneClosed));

			// AS 2/3/10 TFS24080
			// This isn't directly related to this issue but I noticed a flicker when 
			// clicking on a different pane tab item. This seems to be because we are 
			// reparenting the pane which contains a hwndhost and that hwndhost is 
			// still visible so when it gets reparented to the root window its control 
			// is still visible for an instant. To try and get around that we will force 
			// the hwndhosts to hide if the pane in the flyout had any and is about to 
			// be closed.
			//
			if (oldPane != null && pane != oldPane && oldPane.IsPinned == false)
			{
				oldPane.HideAllHwndHosts();
			}

			if (null == pane)
			{
				// AS 2/3/10 TFS24080
				// This is the real cause of the issue. When we are hosting the flyout 
				// within a popup and focus is within the popup we have to shift focus 
				// out of the popup before hiding the flyout or else the wpf framework 
				// will incorrectly shift it out later. Essentially what happens is 
				// that the keyboard device sees that the IsVisible of the focused 
				// element changes when we change the flyout's visibility to hidden. 
				// As a result they do an async operation to verify that the focused 
				// element is within the active device. Unfortunately they seem to 
				// consider an element existing within a hwndsource that belongs to a 
				// popup to be a different source and as a result shift focus out of 
				// the popup back to the root window. This causes the pane that we 
				// activate after this that happens to be in the flyout to be 
				// deactivated/lose focus which causes us to think we should close the 
				// flyout. Since we don't know the cause of the focus change we can't 
				// ignore it. Instead what we will do is if focus is within the flyout 
				// and it is within a popup, we'll force focus to the dockmanager 
				// temporarily and then restore it when we are done (which could fail 
				// if the element is hidden).
				//
				IInputElement oldFocus = Keyboard.FocusedElement;
				XamDockManager dockManager = flyout.DockManager;

				// if the keyboard focus is within the flyout and that is within the popup
				// then try to shift focus out of the flyout before we hide it
				bool shiftFocusOutOfFlyout = oldFocus != null &&
					flyout.IsInPopup &&
					flyout.IsKeyboardFocusWithin &&
					null != dockManager;

				if (shiftFocusOutOfFlyout)
					dockManager.ForceFocus();

				// AS 4/29/08 BR32006
				flyout.ClearValue(UnpinnedTabFlyout.FlyoutPaneVisibilityProperty);

				flyout.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityHiddenBox);

                // AS 3/30/09 TFS16355 - WinForms Interop
                ToolWindow tw = ToolWindow.GetToolWindow(flyout);

				if (null != tw && flyout.IsInPopup)
					tw.Close();

                flyout.ClearValue(WidthProperty);
				flyout.ClearValue(HeightProperty);

				// AS 2/3/10 TFS24080
				// If we shifted focus out, then try to restore it when we are done.
				// 
				if (shiftFocusOutOfFlyout)
					oldFocus.Focus();
			}
			else
			{
				// AS 4/29/08 BR32006
				pane.AddHandler(ContentPane.ClosedEvent, new RoutedEventHandler(flyout.OnFlyoutPaneClosed), true);

				flyout.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.VisibilityVisibleBox);

				// AS 4/29/08 BR32006
				// Just in case the customer changes the visibility of the pane without closing it, we'll handle that too.
				//
				flyout.SetBinding(UnpinnedTabFlyout.FlyoutPaneVisibilityProperty, Utilities.CreateBindingObject(FrameworkElement.VisibilityProperty, BindingMode.OneWay, pane));

				XamDockManager dockManager = flyout.DockManager;

				Debug.Assert(null != dockManager);

				if (null != dockManager)
					flyout.SetValue(SidePropertyKey, KnownBoxes.FromValue(dockManager.GetDockedSide(pane)));

                // AS 3/30/09 TFS16355 - WinForms Interop [Start]
                // We need to get the dockpanel because this pane may not 
                // support being shown in a flyout.
                //
                DockManagerPanel dockPanel = dockManager != null ? dockManager.DockPanel : null;

                if (null != dockPanel)
                {
                    // if the pane cannot be shown in a popup then make sure that
                    // we will not use a popup
                    bool? forceUseToolWindow = pane.CanShowInPopup ? (bool?)null : false;
                    dockPanel.VerifyFlyoutPanelHost(forceUseToolWindow);
                }

                // get the toolwindow that contains the flyout
                ToolWindow tw = ToolWindow.GetToolWindow(flyout);

                bool isInPopup = flyout.IsInPopup;

                if (!isInPopup)
                    tw = null;

                if (null != tw)
                {
                    // make sure the toolwindow isn't still caching the width
                    // height used the last time we flew out
                    tw.ClearValue(WidthProperty);
                    tw.ClearValue(HeightProperty);

					// AS 9/8/09 TFS19683
					// The ToolWindow is getting measured by the PopupRoot with the same 
					// value it was before but the ContentPresenter's measure is not invalid 
					// so its just returning the value it did previously so we need to make 
					// sure its measure is invalid when we change panes.
					//
					if (VisualTreeHelper.GetChildrenCount(tw) > 0)
					{
						UIElement twChild = VisualTreeHelper.GetChild(tw, 0) as UIElement;

						if (null != twChild)
							twChild.InvalidateMeasure();
					}
                }
                // AS 3/30/09 TFS16355 - WinForms Interop [End]

				switch (flyout.Side)
				{
					case Dock.Left:
					case Dock.Right:
						flyout.SetBinding(WidthProperty, Utilities.CreateBindingObject(UnpinnedTabFlyout.FlyoutExtentProperty, BindingMode.TwoWay, pane));
						flyout.ClearValue(HeightProperty);
						break;
					case Dock.Top:
					case Dock.Bottom:
						flyout.SetBinding(HeightProperty, Utilities.CreateBindingObject(UnpinnedTabFlyout.FlyoutExtentProperty, BindingMode.TwoWay, pane));
						flyout.ClearValue(WidthProperty);
						break;
				}

				// AS 6/15/12 TFS114790
				// This is just in case. Since we constrain the size of the pane based on the available space
				// we should make sure we get into the measure when the pane is changed.
				//
				flyout.InvalidateMeasure();

                // AS 10/13/08 TFS6032
                flyout.CoerceValue(FlyoutAnimationProperty);

				flyout._scaleTransform = null; // do not reuse the transforms
				flyout._translateTransform = null; // do not reuse the transforms
				flyout.CoerceValue(RenderTransformProperty);

                // AS 3/30/09 TFS16355 - WinForms Interop
                flyout.InitializeToolWindowAlignment();

                if (null != tw
                    && tw.IsVisible == false)
                {
					// AS 8/24/09 TFS19861
					DockManagerUtilities.InitializePreventAllowsTransparency(tw);

                    tw.Show(dockPanel, false, true);
                }
            }

			// AS 9/11/09 TFS21329
			flyout.VerifyMouseOverTimer();
		}

		/// <summary>
		/// Identifies the <see cref="Pane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneProperty =
			PanePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the unpinned <see cref="ContentPane"/> that is being displayed in the flyout.
		/// </summary>
		/// <seealso cref="PaneProperty"/>
		//[Description("Returns the unpinned 'ContentPane' that is being displayed in the flyout.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public ContentPane Pane
		{
			get
			{
				return (ContentPane)this.GetValue(UnpinnedTabFlyout.PaneProperty);
			}
			private set
			{
				ContentPane oldPane = this.Pane;

				if (value != oldPane)
				{
					DockManagerPanel panel = this.GetUIParentCore() as DockManagerPanel;

                    // AS 3/30/09 TFS16355 - WinForms Interop
                    if (panel == null)
                    {
                        XamDockManager dm = XamDockManager.GetDockManager(this);

                        if (null != dm)
                            panel = dm.DockPanel;
                    }

                    // if we're going to put a pane into the flyout, make sure its removed
					// from the visual tree of the unpinned container
					if (null != panel && value != null)
						panel.RemoveUnpinnedPane(value);

					// set the pane
					this.SetValue(PanePropertyKey, value == null ? DependencyProperty.UnsetValue : value);

					// if there was an element in the flyout then put it in the unpinned container
					// when the pane is removed from the flyout.
					if (null != panel && null != oldPane)
						panel.AddUnpinnedPane(oldPane);
				}
			}
		}

		#endregion //Pane

		#region Side

		private static readonly DependencyPropertyKey SidePropertyKey =
			DependencyProperty.RegisterReadOnly("Side",
			typeof(Dock), typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(KnownBoxes.DockLeftBox, new PropertyChangedCallback(OnSideChanged)));

		private static void OnSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(UIElement.RenderTransformOriginProperty);
			d.CoerceValue(UIElement.RenderTransformProperty);

			// AS 6/15/12 TFS114790
			d.CoerceValue(FrameworkElement.WidthProperty);
			d.CoerceValue(FrameworkElement.HeightProperty);


            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            ((UnpinnedTabFlyout)d).UpdateVisualStates();

        }

		/// <summary>
		/// Identifies the <see cref="Side"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SideProperty =
			SidePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the side that indicates whose unpinned tab area the flyout is associated with.
		/// </summary>
		/// <seealso cref="SideProperty"/>
		//[Description("Returns the side that indicates whose unpinned tab area the flyout is associated with.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public Dock Side
		{
			get
			{
				return (Dock)this.GetValue(UnpinnedTabFlyout.SideProperty);
			}
		}

		#endregion //Side

		#endregion //Public Properties

		#region Internal Properties

		#region CurrentState
		/// <summary>
		/// Returns an enumeration indicating the current state of the pane
		/// </summary>
		internal UnpinnedFlyoutState CurrentState
		{
			get { return this._currentState; }
			private set
			{
				if (value != this._currentState)
				{




					this._currentState = value;

					if (value == UnpinnedFlyoutState.Closed)
					{
						// use the property so we can perform cleanup after setting
						//this.ClearValue(PanePropertyKey);
						this.Pane = null;
					}
					else if (value == UnpinnedFlyoutState.Shown)
						this.InvalidateVisual();
				}
			}
		}
		#endregion //CurrentState

		#region FlyoutExtent

		/// <summary>
		/// Identifies the FlyoutExtent attached dependency property
		/// </summary>
		/// <seealso cref="GetFlyoutExtent"/>
		/// <seealso cref="SetFlyoutExtent"/>
		internal static readonly DependencyProperty FlyoutExtentProperty = DependencyProperty.RegisterAttached("FlyoutExtent",
			typeof(double), typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Gets the value of the 'FlyoutExtent' attached property
		/// </summary>
		/// <seealso cref="FlyoutExtentProperty"/>
		/// <seealso cref="SetFlyoutExtent"/>
		internal static double GetFlyoutExtent(DependencyObject d)
		{
			return (double)d.GetValue(UnpinnedTabFlyout.FlyoutExtentProperty);
		}

		/// <summary>
		/// Sets the value of the 'FlyoutExtent' attached property
		/// </summary>
		/// <seealso cref="FlyoutExtentProperty"/>
		/// <seealso cref="GetFlyoutExtent"/>
		internal static void SetFlyoutExtent(DependencyObject d, double value)
		{
			d.SetValue(UnpinnedTabFlyout.FlyoutExtentProperty, value);
		}

		#endregion //FlyoutExtent

		#region IsClosing
		internal bool IsClosing
		{
			get
			{
				return this._currentState == UnpinnedFlyoutState.Closing ||
					this._currentState == UnpinnedFlyoutState.ClosingIgnoreMouse;
			}
		} 
		#endregion //IsClosing

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region IsInPopup
        internal bool IsInPopup
        {
            get
            {
                ToolWindow tw = ToolWindow.GetToolWindow(this);

                if (tw != null && tw.Tag == DockManagerPanel.FlyoutToolWindowId)
                {
					// AS 1/3/12 TFS98767
					// If the popup contains the flyout itself then we are in a popup.
					//
					if (tw.Content == this)
						return true;

                    XamDockManager dm = XamDockManager.GetDockManager(this);

                    if (null != dm &&
                        dm.ShowFlyoutInToolWindow &&
                        tw != ToolWindow.GetToolWindow(dm))
                        return true;
                }

                return false;
            }
        } 
        #endregion //IsInPopup

		// AS 9/11/09 TFS21329
		#region IsMouseOverEx
		internal bool IsMouseOverEx
		{
			get
			{
				if (this.IsMouseOver)
					return true;

				// when the mouse is over an hwndhost, the mouse leave for the unpinned 
				// tab flyout will fire and the ismouseover will be false. this results 
				// in the flyout closing when the pane is not active. we need to fallback 
				// to checking the location of the mouse and seeing if its over this element
				if (this.Pane != null
					&& this.Pane.HasHwndHost
					&& DockManagerUtilities.IsMouseOver(this))
				{
					return true;
				}

				return false;
			}
		}
		#endregion //IsMouseOverEx

		// AS 6/15/12 TFS114790
		// This is used to obtain the available size in the dockmanagerpanel.
		//
		#region MaxFlyoutHeight

		/// <summary>
		/// Identifies the <see cref="MaxFlyoutHeight"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty MaxFlyoutHeightProperty = DependencyPropertyUtilities.Register("MaxFlyoutHeight",
			typeof(double), typeof(UnpinnedTabFlyout),
			DependencyPropertyUtilities.CreateMetadata(double.PositiveInfinity, new PropertyChangedCallback(OnMaxFlyoutHeightChanged))
			);

		private static void OnMaxFlyoutHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UnpinnedTabFlyout instance = (UnpinnedTabFlyout)d;
			instance.CoerceValue(FrameworkElement.HeightProperty);

			switch (instance.Side)
			{
				case Dock.Top:
				case Dock.Bottom:
					instance.InvalidateMeasure();
					break;
			}
		}

		/// <summary>
		/// Returns or sets the maximum height that should be used when measuring/arranging the flyout when docked to the top/bottom.
		/// </summary>
		/// <seealso cref="MaxFlyoutHeightProperty"/>
		internal double MaxFlyoutHeight
		{
			get
			{
				return (double)this.GetValue(UnpinnedTabFlyout.MaxFlyoutHeightProperty);
			}
			set
			{
				this.SetValue(UnpinnedTabFlyout.MaxFlyoutHeightProperty, value);
			}
		}

		#endregion //MaxFlyoutHeight

		// AS 6/15/12 TFS114790
		// This is used to obtain the available size in the dockmanagerpanel.
		//
		#region MaxFlyoutWidth

		/// <summary>
		/// Identifies the <see cref="MaxFlyoutWidth"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty MaxFlyoutWidthProperty = DependencyPropertyUtilities.Register("MaxFlyoutWidth",
			typeof(double), typeof(UnpinnedTabFlyout),
			DependencyPropertyUtilities.CreateMetadata(double.PositiveInfinity, new PropertyChangedCallback(OnMaxFlyoutWidthChanged))
			);

		private static void OnMaxFlyoutWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UnpinnedTabFlyout instance = (UnpinnedTabFlyout)d;
			instance.CoerceValue(FrameworkElement.WidthProperty);

			switch (instance.Side)
			{
				case Dock.Left:
				case Dock.Right:
					instance.InvalidateMeasure();
					break;
			}
		}

		/// <summary>
		/// Returns or sets the maximum width that should be used when measuring/arranging the flyout when docked to the left/right.
		/// </summary>
		/// <seealso cref="MaxFlyoutWidthProperty"/>
		internal double MaxFlyoutWidth
		{
			get
			{
				return (double)this.GetValue(UnpinnedTabFlyout.MaxFlyoutWidthProperty);
			}
			set
			{
				this.SetValue(UnpinnedTabFlyout.MaxFlyoutWidthProperty, value);
			}
		}

		#endregion //MaxFlyoutWidth

		#endregion //Internal Properties

		#region Private Properties

		#region DockManager
		private XamDockManager DockManager
		{
			get { return XamDockManager.GetDockManager(this); }
		} 
		#endregion //DockManager

		#region FlyoutAnimation

		/// <summary>
		/// Identifies the <see cref="FlyoutAnimation"/> dependency property
		/// </summary>
		private static readonly DependencyProperty FlyoutAnimationProperty = DependencyProperty.Register("FlyoutAnimation",
			typeof(PaneFlyoutAnimation), typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(PaneFlyoutAnimation.Slide, new PropertyChangedCallback(OnFlyoutAnimationChanged), 
                // AS 10/13/08 TFS6032
                new CoerceValueCallback(CoerceFlyoutAnimation)));

        private static object CoerceFlyoutAnimation(DependencyObject d, object newValue)
        {
            UnpinnedTabFlyout flyout = (UnpinnedTabFlyout)d;
            ContentPane pane = flyout.Pane;

            // AS 10/13/08 TFS6032
            // The WindowsFormsHost doesn't participate nicely with the animations so
            // we'll try to skip the animations when we have one hosted.
            //
            if (null != pane && false == pane.AllowAnimations)
                return PaneFlyoutAnimation.None;

            // AS 3/30/09 TFS16355 - WinForms Interop
            // The popup doesn't support transparency when hosted as a child
            // which happens when in an xbap so if we're using a popup and 
            // we're in a browser then do not animate the flyout.
            //
            XamDockManager dm = XamDockManager.GetDockManager(d);

            if (flyout.IsInPopup && DockManagerUtilities.IsPopupInChildWindow)
                return PaneFlyoutAnimation.None;

            return newValue;
        }

		private static void OnFlyoutAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(UIElement.RenderTransformProperty);
			d.CoerceValue(UIElement.RenderTransformOriginProperty);
		}

		private PaneFlyoutAnimation FlyoutAnimation
		{
			get
			{
				return (PaneFlyoutAnimation)this.GetValue(FlyoutAnimationProperty);
			}
		}
		#endregion //FlyoutAnimation

		// AS 4/29/08 BR32006
		#region FlyoutPaneVisibility

		private static readonly DependencyProperty FlyoutPaneVisibilityProperty = DependencyProperty.Register("FlyoutPaneVisibility",
			typeof(Visibility), typeof(UnpinnedTabFlyout), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnFlyoutPaneVisibilityChanged)));

		private static void OnFlyoutPaneVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (Visibility.Collapsed.Equals(e.NewValue))
			{
				UnpinnedTabFlyout flyout = (UnpinnedTabFlyout)d;

				flyout.HideFlyout(flyout.Pane, false, false, true, false);
			}
		}
		#endregion //FlyoutPaneVisibility

		#region IsLeftOrRightEdge
		private bool IsLeftOrRightEdge
		{
			get { return this.Side == Dock.Left || this.Side == Dock.Right; }
		} 
		#endregion //IsLeftOrRightEdge

		#region IsShowingOrShown
		private bool IsShowingOrShown
		{
			get { return this._currentState == UnpinnedFlyoutState.Showing || this._currentState == UnpinnedFlyoutState.Shown; }
		} 
		#endregion //IsShowingOrShown

		#endregion //Private Properties

		#endregion //Properties

		#region Methods
        
        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {

            string state = null;
 
            // set Location states
            switch (this.Side)
            {
                case Dock.Bottom:
                    state = VisualStateUtilities.StateBottom;
                    break;
                case Dock.Top:
                    state = VisualStateUtilities.StateTop;
                    break;
                case Dock.Left:
                    state = VisualStateUtilities.StateLeft;
                    break;
                case Dock.Right:
                    state = VisualStateUtilities.StateRight;
                    break;
            }
            
            if ( state != null )
                VisualStateManager.GoToState(this, state, useTransitions);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UnpinnedTabFlyout flyout = target as UnpinnedTabFlyout;

            flyout.UpdateVisualStates();
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

        #endregion //Protected Methods

		#region Internal Methods

		#region HideFlyout
		internal void HideFlyout(ContentPane pane, bool checkMouseOverState, bool allowAnimation, bool ignoreMouseWhileClosing, bool allowAutoHideDelay)
		{
			this.StopShowTimer();

			if (this._currentState == UnpinnedFlyoutState.Closed)
				return;

			if (this.IsClosing && allowAnimation)
			{
				if (ignoreMouseWhileClosing && this._currentState == UnpinnedFlyoutState.Closing)
					this._currentState = UnpinnedFlyoutState.ClosingIgnoreMouse;

				return;
			}

			// if we were given a pane to hide but its not the one we're showing then
			// don't do anything
			if (pane != null && pane != this.Pane)
				return;

			#region Check Mouse State
			if (checkMouseOverState &&
				this.Pane != null &&
				this.Pane.IsActivePane == false)
			{
				// AS 9/11/09 TFS21329
				//if (this.IsMouseOver)
				if (this.IsMouseOverEx)
				{




					return;
				}

				// get the pane's tab item
				UnpinnedTabAreaInfo tabAreaInfo = this.Pane.PlacementInfo.CurrentContainer as UnpinnedTabAreaInfo;

				Debug.Assert(null != tabAreaInfo, "The pane is unpinned but not in an unpinned tab area?");
				UnpinnedTabArea tabArea = null != tabAreaInfo ? tabAreaInfo.TabArea : null;

				if (null != tabArea)
				{
					PaneTabItem tabItem = tabArea.ItemContainerGenerator.ContainerFromItem(this.Pane) as PaneTabItem;

					if (null != tabItem && tabItem.IsMouseOver)
					{




						return;
					}
				}





			} 
			#endregion // Check Mouse State

			// create a hide storyboard
			this.CurrentState = ignoreMouseWhileClosing ? UnpinnedFlyoutState.ClosingIgnoreMouse : UnpinnedFlyoutState.Closing;

			Storyboard sb = allowAnimation ? CreateStoryboard(false, allowAutoHideDelay) : null;
			this.StartStoryboard(sb);
		}

		#endregion //HideFlyout

		#region HideFlyoutOnMouseLeave
		/// <summary>
		/// Helper method to attempt to close the flyout if the mouse leaves the bounds of the tab item and flyout.
		/// </summary>
		/// <param name="pane">Pane for which the flyout would be closing</param>
		internal static void HideFlyoutOnMouseLeave(ContentPane pane)
		{





			if (null != pane &&
				PaneLocation.Unpinned == XamDockManager.GetPaneLocation(pane) &&
				pane.IsActivePane == false)
			{
				XamDockManager dockManager = XamDockManager.GetDockManager(pane);

				if (null != dockManager)
				{
					dockManager.HideFlyout(pane, true, true, false, true);
				}
			}
		}
		#endregion //HideFlyoutOnMouseLeave

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region InitializeToolWindowAlignment
        internal void InitializeToolWindowAlignment()
        {
            ToolWindow tw = ToolWindow.GetToolWindow(this);

            if (null != tw && this.IsInPopup)
            {
                // calculate the position
                switch (this.Side)
                {
                    default:
                    case Dock.Left:
                        tw.VerticalAlignment = VerticalAlignment.Stretch;
                        tw.HorizontalAlignment = HorizontalAlignment.Left;
                        break;

                    case Dock.Right:
                        tw.VerticalAlignment = VerticalAlignment.Stretch;
                        tw.HorizontalAlignment = HorizontalAlignment.Right;
                        break;

                    case Dock.Top:
                        tw.VerticalAlignment = VerticalAlignment.Top;
                        tw.HorizontalAlignment = HorizontalAlignment.Stretch;
                        break;

                    case Dock.Bottom:
                        tw.VerticalAlignment = VerticalAlignment.Bottom;
                        tw.HorizontalAlignment = HorizontalAlignment.Stretch;
                        break;
                }
            }
        }
        #endregion //InitializeToolWindowAlignment

        #region ShowAndHideFlyout
		internal void ShowAndHideFlyout(ContentPane contentPane)
		{
			// show the flyout immediately
			this.ShowFlyout(contentPane, false, false, false);

			// then animate the hide
			this.HideFlyout(contentPane, false, true, true, true);
		}
		#endregion //ShowAndHideFlyout

		#region ShowFlyout
		/// <summary>
		/// Shows the specified pane in the flyout
		/// </summary>
		/// <param name="pane">The pane to be shown</param>
		/// <param name="allowDelay">True if the pane can be shown after a delay.</param>
		/// <param name="basedOnMouseOver">True if the flyout is being shown because of the mouse entering a flyout related element</param>
		/// <param name="allowAnimation">True if the show can be animated</param>
		internal void ShowFlyout(ContentPane pane, bool allowDelay, bool basedOnMouseOver, bool allowAnimation)
		{



			if (basedOnMouseOver 
				&& pane == this.Pane
				&& this._currentState == UnpinnedFlyoutState.ClosingIgnoreMouse)
				return;

			// AS 7/1/10 TFS34388
			// We can only show the flyout for an unpinned pane and then only 
			// for one associated with out pane.
			//
			if (pane.PaneLocation != PaneLocation.Unpinned && this.DockManager == XamDockManager.GetDockManager(pane))
				return;

			// if we're already in the process of showing for the specified pane
			// then there is nothing to do
			if (this.Pane == pane && this.IsShowingOrShown)
			{
				this.StopShowTimer();
				return;
			}

			XamDockManager dockManager = this.DockManager;

			if (null == dockManager)
			{
				Debug.Fail("Not associated with DockManager!");
				this.StopShowTimer();
				return;
			}

			// AS 9/29/09 NA 2010.1 - UnpinnedTabHoverAction
			// If this is the result of a mouse over of the tab item and this is not 
			// the pane in the flyout then we do not want to show the flyout for this 
			// pane when the hover action is not flyout.
			//
			if (basedOnMouseOver && 
				pane != this.Pane &&
				dockManager.UnpinnedTabHoverAction != UnpinnedTabHoverAction.Flyout)
			{
				return;
			}

			var originalPane = this.Pane;

			// if we're trying to show for a different pane then start a timer and wait
			// to show the pane
			if (this.Pane != pane && this.Pane != null)
			{
				if (allowDelay)
				{
					this.StartShowTimer(pane);
					return;
				}
				else
					this.HideFlyout(this.Pane, false, false, true, false);
			}

			this.StopShowTimer();

			// if we're hiding for this pane then stop that animation and start showing again
			if (pane != this.Pane)
			{
				// use the property so we can perform cleanup after setting
				//this.SetValue(PanePropertyKey, pane);
				this.Pane = pane;

                // AS 3/30/09 TFS16355 - WinForms Interop
                // Only call update layout if we're not using a popup.
                //
                if (!this.IsInPopup)
                {
                    // AS 5/2/08 BR32027
                    // The flyout is displayed by the dockmanager panel so we need to
                    // invalidate its arrange so the flyout position can be changed.
                    //
                    dockManager.DockPanel.InvalidateArrange();

                    dockManager.InvalidateArrange();
                    dockManager.UpdateLayout();
                }
			}

			this.CurrentState = UnpinnedFlyoutState.Showing;

			// AS 12/15/11 TFS98010
			// If we are showing the flyout for a new pane then raise the event but not if 
			// we are reopening it because the mouse went back over the pane.
			//
			if (pane != originalPane)
				dockManager.RaiseFlyoutOpening(EventArgs.Empty);

			if (this.Pane != pane || this.CurrentState != UnpinnedFlyoutState.Showing)
				return;

			// create a show storyboard
			Storyboard sb = allowAnimation ? CreateStoryboard(true, true) : null;

			this.StartStoryboard(sb);
		}
		#endregion //ShowFlyout

		#region ShowFlyoutOnMouseEnter
		/// <summary>
		/// Helper method for showing the flyout when the mouse enters the flyout or tab item.
		/// </summary>
		/// <param name="pane">The pane for which the flyout would be shown.</param>
		internal static void ShowFlyoutOnMouseEnter(ContentPane pane)
		{




			
			if (null != pane && PaneLocation.Unpinned == XamDockManager.GetPaneLocation(pane))
			{
				XamDockManager dockManager = XamDockManager.GetDockManager(pane);

				if (null != dockManager && dockManager.FlyoutState != UnpinnedFlyoutState.Closed)
				{
					dockManager.ShowFlyout(pane, true, true);
				}
			}
		}
		#endregion //ShowFlyoutOnMouseEnter

		// AS 9/11/09 TFS21329
		#region VerifyMouseOverTimer
		internal void VerifyMouseOverTimer()
		{
			bool shouldUseTimer = false;

			if (this.Pane != null && 
				this.Pane.HasHwndHost &&
				this.IsLoaded)
			{
				shouldUseTimer = true;
			}

			if (shouldUseTimer)
			{
				if (null == _mouseOverTimer)
				{
					const int HwndHostMouseInterval = 250;
					_mouseOverTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(HwndHostMouseInterval), DispatcherPriority.Input, new EventHandler(OnMouseOverTick), this.Dispatcher);
					_mouseOverTimer.Start();
				}
			}
			else
			{
				if (null != _mouseOverTimer)
				{
					_mouseOverTimer.Tick -= new EventHandler(OnMouseOverTick);
					_mouseOverTimer.Stop();
					_mouseOverTimer = null;
				}
			}
		}
		#endregion //VerifyMouseOverTimer

		#endregion //Internal Methods

		#region Private Methods

		#region CalcResizeOrigin
		private static Point CalcResizeOrigin(Dock side)
		{
			switch (side)
			{
				default:
				case Dock.Top:
				case Dock.Left:
					return new Point();
				case Dock.Right:
					return new Point(1, 0);
				case Dock.Bottom:
					return new Point(0, 1);
			}
		}
		#endregion //CalcResizeOrigin

		// AS 6/15/12 TFS114790
		#region CoerceHeight
		private static object CoerceHeight(DependencyObject d, object newValue)
		{
			var flyout = d as UnpinnedTabFlyout;

			switch (flyout.Side)
			{
				case Dock.Top:
				case Dock.Bottom:
					var actual = (double)newValue;

					if (flyout.ConstrainFlyoutExtent(ref actual, flyout.MaxFlyoutHeight, false))
						return actual;
					break;
			}

			return newValue;
		}
		#endregion //CoerceHeight

		#region CoerceRenderTransform
		private static object CoerceRenderTransform(DependencyObject d, object newValue)
		{
			UnpinnedTabFlyout flyout = (UnpinnedTabFlyout)d;

			Debug.Assert(newValue == null || Transform.Identity.Equals(newValue));

			switch (flyout.FlyoutAnimation)
			{
				case PaneFlyoutAnimation.Slide:
					if (flyout._translateTransform == null)
						flyout._translateTransform = new TranslateTransform();

					return flyout._translateTransform;
				case PaneFlyoutAnimation.Resize:
					if (flyout._scaleTransform == null)
						flyout._scaleTransform = new ScaleTransform();

					return flyout._scaleTransform;
			}

			return newValue;
		} 
		#endregion //CoerceRenderTransform

		#region CoerceRenderTransformOrigin
		private static object CoerceRenderTransformOrigin(DependencyObject d, object newValue)
		{
			UnpinnedTabFlyout flyout = (UnpinnedTabFlyout)d;

			// we need to control the render origin for resize operations
			if (flyout.FlyoutAnimation == PaneFlyoutAnimation.Resize)
				return CalcResizeOrigin(flyout.Side);

			return newValue;
		} 
		#endregion //CoerceRenderTransformOrigin

		// AS 6/15/12 TFS114790
		#region CoerceWidth
		private static object CoerceWidth(DependencyObject d, object newValue)
		{
			var flyout = d as UnpinnedTabFlyout;

			switch (flyout.Side)
			{
				case Dock.Left:
				case Dock.Right:
					var actual = (double)newValue;

					if (flyout.ConstrainFlyoutExtent(ref actual, flyout.MaxFlyoutWidth, true))
						return actual;
					break;
			}

			return newValue;
		}
		#endregion //CoerceWidth

		// AS 6/15/12 TFS114790
		#region ConstrainFlyoutExtent
		private bool ConstrainFlyoutExtent(ref double value, double maxAvailable, bool isHorizontal)
		{
			if (double.IsNaN(value))
				return false;

			if (double.IsPositiveInfinity(maxAvailable))
				return false;

			// remove the required reserved space
			maxAvailable = Math.Max(maxAvailable - UnpinnedTabFlyoutSplitter.GetReservedSpace(isHorizontal), 0);

			if (CoreUtilities.LessThanOrClose(value, maxAvailable))
				return false;

			value = maxAvailable;
			return true;
		} 
		#endregion //ConstrainFlyoutExtent

		#region CreateStoryboard
		private Storyboard CreateStoryboard(bool show, bool allowAutoHideDelay)
		{
			return this.CreateStoryboard(show, allowAutoHideDelay, this._currentStoryboard != null);
		}

		private Storyboard CreateStoryboard(bool show, bool allowAutoHideDelay, bool startWithCurrentValue)
		{
			PaneFlyoutAnimation animation = this.FlyoutAnimation;

			double milliseconds = show ? ShowDurationTime : HideDurationTime;
			Duration duration = new Duration(TimeSpan.FromMilliseconds(milliseconds));





			// if we're hiding and we're not using the autohide delay then immediately process the hide.
			if (show == false && animation == PaneFlyoutAnimation.None && allowAutoHideDelay == false)
				return null;

			Storyboard storyboard = new Storyboard();
			storyboard.FillBehavior = FillBehavior.Stop;

			if (show == false)
				storyboard.BeginTime = TimeSpan.FromMilliseconds(AutoHideDelay);

			switch (animation)
			{
				case PaneFlyoutAnimation.None:
					{
						#region None

						// changed from zero to a minimal duration or else the animation won't run and
						// therefore the storyboard completed won't invoke and we won't hide the flyout
						// when the animation is none.
						DoubleAnimation noOpAnimation = new DoubleAnimation(this.Opacity, this.Opacity, new Duration(TimeSpan.FromMilliseconds(1)), FillBehavior.Stop);
						storyboard.Children.Add(noOpAnimation);
						Storyboard.SetTargetProperty(noOpAnimation, new PropertyPath(OpacityProperty));





						break;

						#endregion //None
					}
				case PaneFlyoutAnimation.Fade:
					{
						#region Fade

						// create a fade in/out storyboard
						double to = show ? 1d : 0d;
						double from = startWithCurrentValue ? this.Opacity : (show ? 0d : 1d);

						DoubleAnimation opacityAnimation = new DoubleAnimation(from, to, duration, FillBehavior.Stop);





						storyboard.Children.Add(opacityAnimation);
						Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));
						opacityAnimation.DecelerationRatio = .75d;
						break;

						#endregion //Fade
					}
				case PaneFlyoutAnimation.Resize:
					{
						#region Resize

						bool isLeftOrRight = this.IsLeftOrRightEdge;
						string scalePropertyPath = isLeftOrRight ? "(UIElement.RenderTransform).(ScaleTransform.ScaleX)" : "(UIElement.RenderTransform).(ScaleTransform.ScaleY)";
						DependencyProperty scaleProperty = isLeftOrRight ? ScaleTransform.ScaleXProperty : ScaleTransform.ScaleYProperty;

						Debug.Assert(null != this._scaleTransform);
						ScaleTransform scaleTransform = this._scaleTransform ?? new ScaleTransform();

						double scaleTo = show ? 1d : 0d;

						// technically this can seem incorrect because if its animated then we 
						// should adjust the duration or it will take the entire duration to 
						// animate the remainder. that being said, considering we could be 
						// using an acceleration/decelleration ration there really isn't a good
						// way to see how much time the animation should take
						double scaleFrom = startWithCurrentValue ? (double)scaleTransform.GetValue(scaleProperty) : (show ? 0d : 1d);





						// animate the scalex
						DoubleAnimation scaleAnimation = new DoubleAnimation(scaleFrom, scaleTo, duration, FillBehavior.Stop);
						Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(scalePropertyPath));

						// make sure the rendering origin has been set

						storyboard.Children.Add(scaleAnimation);
						break;

						#endregion //Resize
					}
				case PaneFlyoutAnimation.Slide:
					{
						#region Slide

						bool isLeftOrRight = this.IsLeftOrRightEdge;
						string translatePropertyPath = isLeftOrRight ? "(UIElement.RenderTransform).(TranslateTransform.X)" : "(UIElement.RenderTransform).(TranslateTransform.Y)";
						DependencyProperty translateProperty = isLeftOrRight ? TranslateTransform.XProperty : TranslateTransform.YProperty;

						Debug.Assert(null != this._translateTransform);
						TranslateTransform translateTransform = this._translateTransform ?? new TranslateTransform();

						double translateTo;
						double defaultTranslateFrom;

						// assume a show
						translateTo = 0d;

						switch (this.Side)
						{
							default:
							case Dock.Left:
								defaultTranslateFrom = -this.ActualWidth;
								break;
							case Dock.Right:
								defaultTranslateFrom = this.ActualWidth;
								break;
							case Dock.Top:
								defaultTranslateFrom = -this.ActualHeight;
								break;
							case Dock.Bottom:
								defaultTranslateFrom = this.ActualHeight;
								break;
						}

						if (false == show)
						{
							double temp = translateTo;
							translateTo = defaultTranslateFrom;
							defaultTranslateFrom = temp;
						}

						// technically this can seem incorrect because if its animated then we 
						// should adjust the duration or it will take the entire duration to 
						// animate the remainder. that being said, considering we could be 
						// using an acceleration/decelleration ration there really isn't a good
						// way to see how much time the animation should take
						double translateFrom = startWithCurrentValue ? (double)translateTransform.GetValue(translateProperty) : defaultTranslateFrom;





						// animate the scalex
						DoubleAnimation translateAnimation = new DoubleAnimation(translateFrom, translateTo, duration, FillBehavior.Stop);

						Storyboard.SetTargetProperty(translateAnimation, new PropertyPath(translatePropertyPath));

						storyboard.Children.Add(translateAnimation);
						break;

						#endregion //Slide
					}
			}

			return storyboard;
		}
		#endregion //CreateStoryboard

		#region OnDockManagerChanged
		private static void OnDockManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// find out when the animation type changes
			if (e.NewValue != null)
				BindingOperations.SetBinding(d, UnpinnedTabFlyout.FlyoutAnimationProperty, Utilities.CreateBindingObject(XamDockManager.FlyoutAnimationProperty, BindingMode.OneWay, e.NewValue));
			else
				BindingOperations.ClearBinding(d, UnpinnedTabFlyout.FlyoutAnimationProperty);
		} 
		#endregion //OnDockManagerChanged

		// AS 4/29/08 BR32006
		#region OnFlyoutPaneClosed
		private void OnFlyoutPaneClosed(object sender, RoutedEventArgs e)
		{
			if (this.Pane == sender)
				this.HideFlyout(this.Pane, false, false, true, false);
		} 
		#endregion //OnFlyoutPaneClosed

		// AS 9/11/09 TFS21329
		#region OnLoaded
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

			this.VerifyMouseOverTimer();
		}
		#endregion //OnLoaded

		// AS 9/11/09 TFS21329
		#region OnMouseOverTick
		private void OnMouseOverTick(object sender, EventArgs e)
		{
			switch (_currentState)
			{
				case UnpinnedFlyoutState.Closed:
				case UnpinnedFlyoutState.ClosingIgnoreMouse:
					break;

				case UnpinnedFlyoutState.Closing:
					// if we're in the process of closing and the mouse enters
					// the hwnd host then show the flyout again
					if (null != this.Pane && this.IsMouseOverEx)
						UnpinnedTabFlyout.ShowFlyoutOnMouseEnter(this.Pane);
					break;
				case UnpinnedFlyoutState.Showing:
				case UnpinnedFlyoutState.Shown:
					// if we're showing it or it has been shown then it may need 
					// to close up if the mouse isn't over the flyout or tab any more
					if (null != this.Pane && !this.IsMouseOverEx)
						UnpinnedTabFlyout.HideFlyoutOnMouseLeave(this.Pane);
					break;

				default:
					Debug.Fail("Unexpected state:" + _currentState.ToString());
					break;
			}
		}
		#endregion //OnMouseOverTick

		#region OnOpacityChanged






		#endregion //OnOpacityChanged

		#region OnShowDelayTimerTick
		private void OnShowDelayTimerTick(object sender, EventArgs e)
		{
			DispatcherTimer timer = (DispatcherTimer)sender;

			ContentPane pane = timer.Tag as ContentPane;

			this.StopShowTimer();

			if (null != pane)
				this.ShowFlyout(pane, false, true, true);
		} 
		#endregion // OnShowDelayTimerTick

		#region OnStoryboardCompleted
		private void OnStoryboardCompleted(object sender, EventArgs e)
		{




			this.StopCurrentStoryboard();

			if (this.IsClosing)
				this.CurrentState = UnpinnedFlyoutState.Closed;
			else if (this._currentState == UnpinnedFlyoutState.Showing)
				this.CurrentState = UnpinnedFlyoutState.Shown;
			else
			{
				Debug.Fail("Unexpected state");
			}
		}
		#endregion //OnStoryboardCompleted

		// AS 9/11/09 TFS21329
		#region OnUnloaded
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.Unloaded -= new RoutedEventHandler(OnUnloaded);
			this.Loaded += new RoutedEventHandler(OnLoaded);

			this.VerifyMouseOverTimer();
		}
		#endregion //OnUnloaded 

		#region OutputElementTree
		[Conditional("DEBUG")]
		private static void OutputElementTree()
		{
			DependencyObject element = Mouse.DirectlyOver as DependencyObject;
			StringBuilder sb = new StringBuilder();

			while (element != null)
			{
				sb.Append(element.GetType().Name);
				sb.Append("->");

				if (element is ContentElement)
					element = LogicalTreeHelper.GetParent(element);
				else
					element = VisualTreeHelper.GetParent(element);
			}

			Debug.WriteLine(sb.ToString(), DateTime.Now.ToString("hh:mm:ss:ffffff"));
		}
		#endregion //OutputElementTree

		#region StartShowTimer
		private void StartShowTimer(ContentPane pane)
		{
			if (this._showDelayTimer != null && this._showDelayTimer.Tag == pane)
				return;

			if (null == this._showDelayTimer)
			{
				this._showDelayTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500d), DispatcherPriority.Normal, new EventHandler(OnShowDelayTimerTick), this.Dispatcher);
			}

			this._showDelayTimer.Tag = pane;
			this._showDelayTimer.Start();
		} 
		#endregion // StartShowTimer

		#region StartStoryboard
		private void StartStoryboard(Storyboard sb)
		{
			if (null != sb)
			{
				sb.Completed += new EventHandler(OnStoryboardCompleted);
				sb.Begin(this, true);
			}

			// stop any current storyboard
			this.StopCurrentStoryboard();

			if (null != sb)
				this._currentStoryboard = sb;
			else if (this._currentState == UnpinnedFlyoutState.Showing)
				this.CurrentState = UnpinnedFlyoutState.Shown;
			else if (this.IsClosing)
				this.CurrentState = UnpinnedFlyoutState.Closed;
		}
		#endregion //StartStoryboard

		#region StopCurrentStoryboard
		private void StopCurrentStoryboard()
		{
			if (this._currentStoryboard != null)
			{
				this._currentStoryboard.Completed -= new EventHandler(OnStoryboardCompleted);
				this._currentStoryboard.Stop(this);
				this._currentStoryboard.Remove(this);
				this._currentStoryboard = null;





			}
		}
		#endregion //StopCurrentStoryboard

		#region StopShowTimer
		private void StopShowTimer()
		{
			if (null != this._showDelayTimer)
			{
				this._showDelayTimer.Tag = null;
				this._showDelayTimer.Stop();
			}
		} 
		#endregion // StopShowTimer

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