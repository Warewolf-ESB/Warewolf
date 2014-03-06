using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Commands;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// A derived <see cref="TabItem"/> class that provides additional functionality when used with a <see cref="XamTabControl"/>
	/// </summary>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateTop,                     GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateBottom,                  GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,                    GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,                   GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateTopMouseOverTab,         GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateBottomMouseOverTab,      GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateLeftMouseOverTab,        GroupName = VisualStateUtilities.GroupLocation )]
    [TemplateVisualState(Name = VisualStateUtilities.StateRightMouseOverTab,       GroupName = VisualStateUtilities.GroupLocation )]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!! (AS 5/27/10)
	public class TabItemEx : TabItem, ICommandHost
	{
        #region Private members

        private XamTabControl _bindingsSetWithTabControl;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Private members	
    
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TabItemEx"/>
		/// </summary>
		public TabItemEx()
		{
		} 

		static TabItemEx()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TabItemEx), new FrameworkPropertyMetadata(typeof(TabItemEx)));

			EventManager.RegisterClassHandler(typeof(TabItemEx), Mouse.GotMouseCaptureEvent, new MouseEventHandler(OnGotMouseCaptureWithin), true);
        }

		#endregion //Constructor
        
        #region Events

        #region ExecutingCommand

        /// <summary>
        /// Event ID for the <see cref="ExecutingCommand"/> routed event
        /// </summary>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="OnExecutingCommand"/>
        /// <seealso cref="ExecutingCommandEventArgs"/>
        public static readonly RoutedEvent ExecutingCommandEvent =
            EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(TabItemEx));

        /// <summary>
        /// Occurs before a command is executed.
        /// </summary>
        /// <remarks><para class="body">This event is cancellable.</para></remarks>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutingCommandEvent"/>
        /// <seealso cref="ExecutingCommandEventArgs"/>
        protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
        {
            args.RoutedEvent = TabItemEx.ExecutingCommandEvent;
            args.Source = this;
            this.OnExecutingCommand(args);

            return args.Cancel == false;
        }

        /// <summary>
        /// Occurs before a command is executed
        /// </summary>
        /// <remarks><para class="body">This event is cancellable.</para></remarks>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="OnExecutingCommand"/>
        /// <seealso cref="ExecutingCommandEvent"/>
        /// <seealso cref="ExecutingCommandEventArgs"/>
        //[Description("Occurs before a command is performed")]
        //[Category("DockManager Events")] // Action
        public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
        {
            add
            {
                base.AddHandler(TabItemEx.ExecutingCommandEvent, value);
            }
            remove
            {
                base.RemoveHandler(TabItemEx.ExecutingCommandEvent, value);
            }
        }

        #endregion //ExecutingCommand

        #region ExecutedCommand

        /// <summary>
        /// Event ID for the <see cref="ExecutedCommand"/> routed event
        /// </summary>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="OnExecutedCommand"/>
        /// <seealso cref="ExecutedCommandEventArgs"/>
        public static readonly RoutedEvent ExecutedCommandEvent =
            EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(TabItemEx));

        /// <summary>
        /// Occurs after a command is executed
        /// </summary>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="ExecutedCommandEvent"/>
        /// <seealso cref="ExecutedCommandEventArgs"/>
        protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
        {
            args.RoutedEvent = TabItemEx.ExecutedCommandEvent;
            args.Source = this;
            this.OnExecutedCommand(args);
        }

        /// <summary>
        /// Occurs after a command is executed
        /// </summary>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="OnExecutedCommand"/>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutedCommandEvent"/>
        /// <seealso cref="ExecutedCommandEventArgs"/>
        //[Description("Occurs after a command is performed")]
        //[Category("DockManager Events")] // Action
        public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
        {
            add
            {
                base.AddHandler(TabItemEx.ExecutedCommandEvent, value);
            }
            remove
            {
                base.RemoveHandler(TabItemEx.ExecutedCommandEvent, value);
            }
        }

        #endregion //ExecutedCommand

        #endregion //Events

        #region Base class overrides

        // JJD 8/6/08 - added
        #region MeasureOverride

        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = base.MeasureOverride(constraint);

            this.ValidateTabControlBindings();

            return size;
        }

        #endregion //MeasureOverride	
    
		#region OnAccessKey
		/// <summary>
		/// Invoked when the access key for the tab item has been pressed.
		/// </summary>
		/// <param name="e">Event arguments for the mnemonic that was pressed</param>
		protected override void OnAccessKey(AccessKeyEventArgs e)
		{
			// AS 2/11/08 NA 2008 Vol 1
			// select the tab if its not focusable
			if (this.Focusable == false && this.IsSelected == false)
			{
				// AS 5/10/12 TFS111333
				//this.IsSelected = true;
				DependencyPropertyUtilities.SetCurrentValue(this, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);

				if (this.IsSelected)
					this.BringIntoView();
			}
			else
				base.OnAccessKey(e);
		}
		#endregion //OnAccessKey

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

        // JJD 8/5/08 - added
        #region OnKeyDown

        /// <summary>
        /// Helper method for processing a keydown.
        /// </summary>
        /// <param name="e">Provides information about the keyboard event</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Pass this key along to our commands class which will check to see if a command
            // needs to be executed.  If so, the commands class will execute the command and
            // return true.
            if (e.Handled == false &&
                TabItemExCommands.Instance.ProcessKeyboardInput(e, this) == true)
                e.Handled = true;
        }

        #endregion //OnKeyDown	
    
		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse is moved within the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			this.VerifyIsMouseOverTab(e);
		}
		#endregion //OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse is moved outside the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			this.VerifyIsMouseOverTab(e);
		}
		#endregion //OnMouseLeave

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed on the tab item.
		/// </summary>
		/// <param name="e">Provides information about the mouse button pressed.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			// AS 2/11/08 NA 2008 Vol 1
			if (this.Focusable == false && this.IsSelected == false)
			{
				// AS 5/10/12 TFS111333
				//this.IsSelected = true;
				DependencyPropertyUtilities.SetCurrentValue(this, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);

				if (this.IsSelected)
					this.BringIntoView();

				e.Handled = true;
			}
			else
				base.OnMouseLeftButtonDown(e);
		}
		#endregion //OnMouseLeftButtonDown

        // JJD 8/6/08 - added
        #region OnMouseDown

        /// <summary>
        /// Called when any mouse button is pressed on the tab item
        /// </summary>
        /// <param name="e">Provides information about the mouse button pressed.</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.MiddleButton == MouseButtonState.Pressed &&
				// AS 1/17/11 TFS63134
                //this.AllowClosingResolved == true)
                this.AllowClosingResolved == true &&
				this.IsMouseOverTab)
            {
                XamTabControl tc = this.TabControl;

                if (tc != null &&
                    tc.CloseTab(this))
                {
                    e.Handled = true;
                }
            }
        }

        #endregion //OnMouseDown	
        
		#region OnMouseMove
		/// <summary>
		/// Invoked when the mouse is moved within the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			this.VerifyIsMouseOverTab(e);
		}
		#endregion //OnMouseMove

		#region OnPreviewGotKeyboardFocus
		/// <summary>
		/// Invoked before the element receives the keyboard focus.
		/// </summary>
		/// <param name="e">Provides information about the focus change.</param>
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			XamTabControl xamTab = this.TabControl;

			// the base implementation of the tab item class will select the tab when it gets
			// focus. to emulate the behavior of a minimized tab in office, we should not 
			// automatically select the tab item. instead we should wait until a key
			// is pressed 
			if (null != xamTab && xamTab.IsMinimized && xamTab.IsDropDownOpen == false)
				return;

			base.OnPreviewGotKeyboardFocus(e);
		} 
		#endregion //OnPreviewGotKeyboardFocus

        #region OnPropertyChanged

        /// <summary>
        /// Called when a property value changes.
        /// </summary>
        /// <param name="e">An instance of DependencyPropertyChangedEventArgs that contains information about the property that changed.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            if (e.Property == TabItem.TabStripPlacementProperty)
                this.UpdateVisualStates();

        }

        #endregion //OnPropertyChanged	

        // JJD 8/4/08 XamTabControl
        #region OnSelected

        /// <summary>
        /// Called when this tab item is selected
        /// </summary>
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);

            this.ComputeCloseButtonVisibility();
        }

        #endregion //OnSelected	

        // JJD 8/4/08 XamTabControl
        #region OnUnselected

        /// <summary>
        /// Called when this tab item is unselected
        /// </summary>
        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);

            this.ComputeCloseButtonVisibility();
        }

        #endregion //OnUnselected	
    
        // JJD 8/4/08 XamTabControl
        #region OnVisualParentChanged

        /// <summary>
        /// Called when the visual parent has changed
        /// </summary>
        /// <param name="oldParent">The old parent visual</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            this.ValidateTabControlBindings();
         }

        #endregion //OnVisualParentChanged	

		#endregion //Base class overrides

        #region Events

        #region Closing

        /// <summary>
        /// Event ID for the <see cref="Closing"/> routed event
        /// </summary>
        /// <seealso cref="Closing"/>
        /// <seealso cref="OnClosing"/>
        /// <seealso cref="TabClosingEventArgs"/>
        public static readonly RoutedEvent ClosingEvent =
            EventManager.RegisterRoutedEvent("Closing", RoutingStrategy.Bubble, typeof(EventHandler<TabClosingEventArgs>), typeof(TabItemEx));

        /// <summary>
        /// Occurs when the TabItem is about to be closed (cancellable)
        /// </summary>
        /// <seealso cref="Closing"/>
        /// <seealso cref="ClosingEvent"/>
        /// <seealso cref="TabClosingEventArgs"/>
        protected virtual void OnClosing(TabClosingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseClosing(TabClosingEventArgs args)
        {
            args.RoutedEvent = TabItemEx.ClosingEvent;
            args.Source = this;
            this.OnClosing(args);
        }

        /// <summary>
        /// Occurs when the TabItem is about to be closed (cancellable)
        /// </summary>
        /// <seealso cref="OnClosing"/>
        /// <seealso cref="ClosingEvent"/>
        /// <seealso cref="TabClosingEventArgs"/>
        //[Description("Occurs when the TabItem is about to be closed (cancellable)")]
        //[Category("Behavior")]
        public event EventHandler<TabClosingEventArgs> Closing
        {
            add
            {
                base.AddHandler(TabItemEx.ClosingEvent, value);
            }
            remove
            {
                base.RemoveHandler(TabItemEx.ClosingEvent, value);
            }
        }

        #endregion //Closing

        #region Closed

        /// <summary>
        /// Event ID for the <see cref="Closed"/> routed event
        /// </summary>
        /// <seealso cref="Closed"/>
        /// <seealso cref="OnClosed"/>
        /// <seealso cref="Closing"/>
        public static readonly RoutedEvent ClosedEvent =
            EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TabItemEx));

        /// <summary>
        /// Occurs after the tab has been closed.
        /// </summary>
        /// <seealso cref="Closed"/>
        /// <seealso cref="ClosedEvent"/>
        /// <seealso cref="Closing"/>
        protected virtual void OnClosed(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseClosed()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = TabItemEx.ClosedEvent;
            args.Source = this;
            this.OnClosed(args);
        }

        /// <summary>
        /// Occurs after the tab has been closed.
        /// </summary>
        /// <seealso cref="OnClosed"/>
        /// <seealso cref="Closing"/>
        //[Description("Occurs after the tab has been closed.")]
        //[Category("Behavior")]
        public event RoutedEventHandler Closed
        {
            add
            {
                base.AddHandler(TabItemEx.ClosedEvent, value);
            }
            remove
            {
                base.RemoveHandler(TabItemEx.ClosedEvent, value);
            }
        }

        #endregion //Closed

        #endregion //Events

        #region Properties

        #region Internal Properties

        // JJD 8/4/08 XamTabControl
        internal bool AllowClosingResolved
        {
            get
            {
                bool? allowClosing = this.AllowClosing;

                if (allowClosing.HasValue)
                    return allowClosing.Value;

                TabItemCloseButtonVisibility? cbv = this.CloseButtonVisibility;

                if (cbv.HasValue)
                    return cbv.Value != TabItemCloseButtonVisibility.Hidden;

                XamTabControl tc = this.TabControl;

                if (tc == null)
                    return false;

                allowClosing = tc.AllowTabClosing;

                if (allowClosing.HasValue)
                    return allowClosing.Value;

                if (tc.TabItemCloseButtonVisibility != TabItemCloseButtonVisibility.Hidden)
                    return true;

                return tc.ShowTabHeaderCloseButton;

            }
        }

        // JJD 8/4/08 XamTabControl
        #region TabControlVersion

        internal static readonly DependencyProperty TabControlVersionProperty = DependencyProperty.Register("TabControlVersion",
            typeof(int), typeof(TabItemEx), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnTabControlVersionChanged)));

        private static void OnTabControlVersionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemEx tabitem = target as TabItemEx;

            if (tabitem != null)
                tabitem.ComputeCloseButtonVisibility();
        }

        internal int TabControlVersion
        {
            get
            {
                return (int)this.GetValue(TabItemEx.TabControlVersionProperty);
            }
            set
            {
                this.SetValue(TabItemEx.TabControlVersionProperty, value);
            }
        }

        #endregion //TabControlVersion

        #endregion //Internal Properties	
    
        #region Private Properties
        
        #region TabControl
        private XamTabControl TabControl
		{
			get
			{
				return ItemsControl.ItemsControlFromItemContainer(this) as XamTabControl;
			}
		}
		#endregion //TabControl

		#endregion //Private Properties

		#region Public Properties

        // JJD 8/4/08 XamTabControl
        #region AllowClosing

        /// <summary>
        /// Identifies the <see cref="AllowClosing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowClosingProperty = DependencyProperty.Register("AllowClosing",
            typeof(bool?), typeof(TabItemEx), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAllowClosingChanged)));
        
        private static void OnAllowClosingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemEx tabitem = target as TabItemEx;

            if (tabitem != null)
                tabitem.ComputeCloseButtonVisibility();
        }
 
        /// <summary>
        /// Returns or sets a value that indicates whether the tab item is allowed to be closed.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> By default this value is null in which case the <see cref="XamTabControl.TabItemCloseButtonVisibility"/> and <see cref="XamTabControl.AllowTabClosing"/> of the containing <see cref="XamTabControl"/> will be used.</para>
        /// </remarks>
        /// <seealso cref="AllowClosingProperty"/>
        /// <seealso cref="CloseButtonVisibility"/>
        /// <seealso cref="ComputedCloseButtonVisibility"/>
        /// <seealso cref="XamTabControl.AllowTabClosingProperty"/>
        /// <seealso cref="XamTabControl.TabItemCloseButtonVisibility"/>
        //[Description("Returns or sets a value that indicates whether the tab item is allowed to be closed.")]
        //[Category("Behavior")]
        public bool? AllowClosing
        {
            get
            {
                return (bool?)this.GetValue(TabItemEx.AllowClosingProperty);
            }
            set
            {
                this.SetValue(TabItemEx.AllowClosingProperty, value);
            }
        }

        #endregion //AllowClosing

        // JJD 8/4/08 XamTabControl
        #region CloseButtonVisibility

        /// <summary>
        /// Identifies the <see cref="CloseButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility",
            typeof(TabItemCloseButtonVisibility?), typeof(TabItemEx), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCloseButtonVisibilityChanged)));

        private static void OnCloseButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemEx tabitem = target as TabItemEx;

            if (tabitem != null)
                tabitem.ComputeCloseButtonVisibility();
        }

        /// <summary>
        /// Returns or sets a value that indicates when the close button should be displayed within the tab item. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> By default this value is null in which case the state is based on that of the containing <see cref="XamTabControl"/> <see cref="XamTabControl.TabItemCloseButtonVisibility"/> property.</para>
        /// </remarks>
        /// <seealso cref="CloseButtonVisibilityProperty"/>
        /// <seealso cref="AllowClosing"/>
        /// <seealso cref="ComputedCloseButtonVisibility"/>
        /// <seealso cref="XamTabControl.AllowTabClosingProperty"/>
        /// <seealso cref="XamTabControl.TabItemCloseButtonVisibility"/>
        //[Description("Returns or sets a value that indicates when the close button should be displayed within the tab item. ")]
        //[Category("Behavior")]  
        public TabItemCloseButtonVisibility? CloseButtonVisibility
        {
            get
            {
                return (TabItemCloseButtonVisibility?)this.GetValue(TabItemEx.CloseButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(TabItemEx.CloseButtonVisibilityProperty, value);
            }
        }

        #endregion //CloseButtonVisibility

        // JJD 8/4/08 XamTabControl
        #region ComputedCloseButtonVisibility

        private static readonly DependencyPropertyKey ComputedCloseButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("ComputedCloseButtonVisibility",
            typeof(Visibility), typeof(TabItemEx), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

        /// <summary>
        /// Identifies the <see cref="ComputedCloseButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ComputedCloseButtonVisibilityProperty =
            ComputedCloseButtonVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the calculated visibility of the close button within the tab item based on its settings and those of the containing XamTabControl.
        /// </summary>
        /// <seealso cref="ComputedCloseButtonVisibilityProperty"/>
        /// <seealso cref="AllowClosing"/>
        /// <seealso cref="CloseButtonVisibility"/>
        /// <seealso cref="ComputedCloseButtonVisibility"/>
        /// <seealso cref="XamTabControl.AllowTabClosingProperty"/>
        /// <seealso cref="XamTabControl.TabItemCloseButtonVisibility"/>
        /// <seealso cref="XamTabControl.ShowTabHeaderCloseButton"/>
        //[Description("Returns the calculated visibility of the close button within the tab item based on its settings and those of the containing XamTabControl.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Visibility ComputedCloseButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(TabItemEx.ComputedCloseButtonVisibilityProperty);
            }
        }

        #endregion //ComputedCloseButtonVisibility

		#region IsMouseOverTab

		private static readonly DependencyPropertyKey IsMouseOverTabPropertyKey =
			DependencyProperty.RegisterReadOnly("IsMouseOverTab",
            typeof(bool), typeof(TabItemEx), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsMouseOverTabChanged)));

        // JJD 8/4/08 XamTabControl
        // Added callback
        private static void OnIsMouseOverTabChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemEx tabitem = target as TabItemEx;

            if (tabitem != null)
            {
                tabitem.ComputeCloseButtonVisibility();

                // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                tabitem.UpdateVisualStates();

            }
        }

		/// <summary>
		/// Identifies the <see cref="IsMouseOverTab"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMouseOverTabProperty =
			IsMouseOverTabPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the mouse is over the tab item header.
		/// </summary>
		/// <seealso cref="IsMouseOverTabProperty"/>
		//[Description("Returns a boolean indicating whether the mouse is over the tab item header.")]
		//[Category("Appearance")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsMouseOverTab
		{
			get
			{
				return (bool)this.GetValue(TabItemEx.IsMouseOverTabProperty);
			}
		}

		#endregion //IsMouseOverTab

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

        #region Public Methods

        #region ExecuteCommand

        /// <summary>
        /// Executes the specified RoutedCommand.
        /// </summary>
        /// <param name="command">The RoutedCommand to execute.</param>
        /// <returns>True if command was executed, false if canceled.</returns>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="TabItemExCommands"/>
        public bool ExecuteCommand(RoutedCommand command)
        {
            return this.ExecuteCommandImpl(new ExecuteCommandInfo(command));
        }

        #endregion //ExecuteCommand

        #endregion //Public Methods

        #region Private Methods

        // JJD 8/4/08 XamTabControl
        #region ComputeCloseButtonVisibility

        private void ComputeCloseButtonVisibility()
        {
            Visibility visibility = Visibility.Collapsed;

            if (this.AllowClosingResolved)
            {
                TabItemCloseButtonVisibility? tiv = this.CloseButtonVisibility;

                if (!tiv.HasValue)
                {
                    XamTabControl tc = this.TabControl;

                    if (tc != null)
                        tiv = this.TabControl.TabItemCloseButtonVisibility;
                }

                if (tiv.HasValue)
                {
                    switch (tiv.Value)
                    {
                        case TabItemCloseButtonVisibility.Visible:
                            visibility = Visibility.Visible;
                            break;
                        case TabItemCloseButtonVisibility.WhenSelected:
                            visibility = this.IsSelected ? Visibility.Visible : Visibility.Hidden;
                            break;
                        case TabItemCloseButtonVisibility.WhenSelectedOrHotTracked:
                            visibility = this.IsSelected || this.IsMouseOverTab ? Visibility.Visible : Visibility.Hidden;
                            break;
                    }
                }
            }

            this.SetValue(ComputedCloseButtonVisibilityPropertyKey, visibility);

        }

        #endregion //ComputeCloseButtonVisibility	

        #region ExecuteCommandImpl

        private bool ExecuteCommandImpl(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;

            // Make sure we have a command to execute.
            Utilities.ThrowIfNull(command, "command");

            // Make sure the minimal control state exists to execute the command.
            if (TabItemExCommands.IsMinimumStatePresentForCommand(this as ICommandHost, command) == false)
                return false;

            // make sure the command can be executed
            if (false == ((ICommandHost)this).CanExecute(commandInfo))
                return false;

            // Fire the 'before executed' cancelable event.
            ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(command);
            bool proceed = this.RaiseExecutingCommand(beforeArgs);

            if (proceed == false)
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }

            // =========================================================================================
            // Determine which of our supported commands should be executed and do the associated action.
            bool handled = false;

            XamTabControl tc = this.TabControl;

            if (tc != null)
            {
                if (command == TabItemExCommands.Close)
                    handled = tc.CloseTab(this);

                if (command == TabItemExCommands.CloseAllButThis)
                    handled = tc.CloseAllTabs(this);
            }

            // =========================================================================================

            //PostExecute:
            // If the command was executed, fire the 'after executed' event.
            if (handled == true)
                this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));


            return handled;
        }

        #endregion //ExecuteCommandImpl

		#region OnGotMouseCaptureWithin
		private static void OnGotMouseCaptureWithin(object sender, MouseEventArgs e)
		{
			((TabItemEx)sender).VerifyIsMouseOverTab(e);
		}
		#endregion //OnGotMouseCaptureWithin

        // JJD 8/6/08 - added
        #region ValidateTabControlBindings

        private void ValidateTabControlBindings()
        {
            XamTabControl tc = this.TabControl;

            if (tc != null)
            {
                if (this._bindingsSetWithTabControl != tc)
                {
                    this._bindingsSetWithTabControl = tc;
                    this.SetBinding(TabControlVersionProperty, Utilities.CreateBindingObject(XamTabControl.InternalVersionProperty, BindingMode.OneWay, tc));
                }
            }
            else
            {
                if (this._bindingsSetWithTabControl != null)
                {
                    this._bindingsSetWithTabControl = null;
                    BindingOperations.ClearBinding(this, TabControlVersionProperty);
                }
            }

        }

        #endregion //ValidateTabControlBindings	
    
		#region VerifyIsMouseOverTab
		private void VerifyIsMouseOverTab(MouseEventArgs e)
		{
			if (Mouse.Captured != null && this.IsMouseCaptureWithin == false)
				this.ClearValue(IsMouseOverTabPropertyKey);
			else if (this.IsMouseOver == false)
				this.ClearValue(IsMouseOverTabPropertyKey);
			else
			{
				HitTestResult hr = VisualTreeHelper.HitTest(this, e.GetPosition(this));

				if (null != hr && hr.VisualHit != null)
					this.SetValue(IsMouseOverTabPropertyKey, KnownBoxes.TrueBox);
				else
					this.ClearValue(IsMouseOverTabPropertyKey);
			}
		}
		#endregion //VerifyIsMouseOverTab

        #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            if (this.IsMouseOverTab)
            {
                switch (this.TabStripPlacement)
                {
                    case Dock.Bottom:
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateBottomMouseOverTab, VisualStateUtilities.StateBottom);
                        break;
                    case Dock.Left:
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateLeftMouseOverTab, VisualStateUtilities.StateLeft);
                        break;
                    case Dock.Right:
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateRightMouseOverTab, VisualStateUtilities.StateRight);
                        break;
                    case Dock.Top:
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateTopMouseOverTab, VisualStateUtilities.StateTop);
                        break;
                }
            }
            else
            {
                switch (this.TabStripPlacement)
                {
                    case Dock.Bottom:
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateBottom, useTransitions);
                        break;
                    case Dock.Left:
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateLeft, useTransitions);
                        break;
                    case Dock.Right:
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateRight, useTransitions);
                        break;
                    case Dock.Top:
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateTop, useTransitions);
                        break;
                }
            }
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemEx tabItem = target as TabItemEx;

            tabItem.UpdateVisualStates();
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

		#endregion //Private Methods

        #region Interal Methods

        // JJD 8/6/08 - added
        #region Close

        internal bool Close()
        {
            if (this.Visibility == Visibility.Collapsed)
                return false;

            if (!this.AllowClosingResolved)
                return false;

            TabClosingEventArgs args = new TabClosingEventArgs();

            this.RaiseClosing(args);

            if (args.Cancel == true)
                return false;

            this.Visibility = Visibility.Collapsed;

            this.RaiseClosed();

            return this.Visibility == Visibility.Collapsed;
        }

        #endregion //Close

        #endregion //Interal Methods	
        		
        #endregion //Methods

        #region ICommandHost Members

        bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;
            return null != command && this.ExecuteCommandImpl(commandInfo);
        }

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			TabItemExStates states = 0;

			if ( this.AllowClosingResolved )
				states |= TabItemExStates.AllowsClosing;

			if ( this.IsSelected )
				states |= TabItemExStates.SelectedTab;

			return (long)states & statesToQuery;
		}

        bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;

            if (null == command)
                return false;

            if (command == TabItemExCommands.Close)
                return this.AllowClosingResolved;

            if (command == TabItemExCommands.CloseAllButThis)
                return true;

            return false;
        }

        #endregion
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