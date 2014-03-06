using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using Infragistics.Windows.DockManager.Dragging;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A customizable element used to represent the caption/header area of a <see cref="ContentPane"/>
	/// </summary>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,        GroupName = VisualStateUtilities.GroupCommon )]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,          GroupName = VisualStateUtilities.GroupCommon )]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,       GroupName = VisualStateUtilities.GroupCommon )]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,          GroupName = VisualStateUtilities.GroupActive )]
    [TemplateVisualState(Name = VisualStateUtilities.StateActiveDocument,  GroupName = VisualStateUtilities.GroupActive )]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,        GroupName = VisualStateUtilities.GroupActive )]


	[TemplatePart(Name = "PART_PositionMenuItem", Type = typeof(MenuItem))]
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PaneHeaderPresenter : ContentControl
	{
		#region Member Variables

		private MenuItem _positionMenuItem;
		private MenuItem _hiddenMenuItem;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneHeaderPresenter"/>
		/// </summary>
		public PaneHeaderPresenter()
		{
		}

		static PaneHeaderPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(typeof(PaneHeaderPresenter)));
			FrameworkElement.DataContextProperty.OverrideMetadata(typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDataContextChanged)));
			ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			ContentControl.VerticalContentAlignmentProperty.OverrideMetadata(typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));
			UIElement.FocusableProperty.OverrideMetadata(typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			// we need to show the panes context menu when right clicking on the header or a tab item
			EventManager.RegisterClassHandler(typeof(PaneHeaderPresenter), FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(ContentPane.OnPaneContextMenuOpening));

			// we need to register a handler for the MouseDown because we need to get handled
			// events because we want to activate the associated pane even if you click on a 
			// button within the caption
			EventManager.RegisterClassHandler(typeof(PaneHeaderPresenter), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), true);

			// we could have hooked the event on every tool but this wasn't really necessary since
			// every header needs to do this so we'll handle it in the class handler
			EventManager.RegisterClassHandler(typeof(PaneHeaderPresenter), MenuItem.SubmenuOpenedEvent, new RoutedEventHandler(OnSubmenuOpened));

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }
		#endregion //Constructor

		#region Resource Keys

		#region CloseButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the close <see cref="Button"/> within the header of a <see cref="ContentPane"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
		/// <seealso cref="ContentPane.AllowClose"/>
		public static readonly ResourceKey CloseButtonStyleKey = new StaticPropertyResourceKey(typeof(PaneHeaderPresenter), "CloseButtonStyleKey");

		#endregion //CloseButtonStyleKey

		// AS 6/24/11 FloatingWindowCaptionSource
		#region MaximizeButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the maximize <see cref="Button"/> within the header of a <see cref="ContentPane"/> when the pane's header is used to represent the caption area of the floating window.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
		/// <seealso cref="XamDockManager.FloatingWindowCaptionSource"/>
		public static readonly ResourceKey MaximizeButtonStyleKey = new StaticPropertyResourceKey(typeof(PaneHeaderPresenter), "MaximizeButtonStyleKey");

		#endregion //MaximizeButtonStyleKey

		// AS 6/24/11 FloatingWindowCaptionSource
		#region MinimizeButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the minimize <see cref="Button"/> within the header of a <see cref="ContentPane"/> when the pane's header is used to represent the caption area of the floating window.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
		/// <seealso cref="XamDockManager.FloatingWindowCaptionSource"/>
		public static readonly ResourceKey MinimizeButtonStyleKey = new StaticPropertyResourceKey(typeof(PaneHeaderPresenter), "MinimizeButtonStyleKey");

		#endregion //MinimizeButtonStyleKey

		#region PositionMenuItemStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> used for the <see cref="MenuItem"/> within the header area that displays the list of options where the associated pane can be docked.
		/// </summary>
		public static readonly ResourceKey PositionMenuItemStyleKey = new StaticPropertyResourceKey(typeof(PaneHeaderPresenter), "PositionMenuItemStyleKey");

		#endregion //PositionMenuItemStyleKey

		#region PinButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="FrameworkElement.Style"/> used for the unpin <see cref="Button"/> within the header of a <see cref="ContentPane"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">This field represents the <see cref="ResourceKey"/> that should be used as the key in a <see cref="ResourceDictionary"/> 
		/// for a <see cref="FrameworkElement.Style"/> targeting a <see cref="Button"/>.</p>
		/// </remarks>
		/// <seealso cref="ContentPane.AllowPinning"/>
		public static readonly ResourceKey PinButtonStyleKey = new StaticPropertyResourceKey(typeof(PaneHeaderPresenter), "PinButtonStyleKey");

		#endregion //PinButtonStyleKey

		#endregion //Resource Keys

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (null != this._positionMenuItem && null != this._hiddenMenuItem)
			{
				// AS 5/5/08
				//this._positionMenuItem.Items.Add(this._hiddenMenuItem);
				this._positionMenuItem.Items.Remove(this._hiddenMenuItem);
			}

			this._positionMenuItem = this.GetTemplateChild("PART_PositionMenuItem") as MenuItem;

			// put a default hidden menu item in the template so the menu item is considered
			// a header item and not a leaf item - otherwise we won't get the submenuopened event
			if (null != this._positionMenuItem)
				this._positionMenuItem.Items.Add(this.HiddenMenuItem);

			// AS 6/27/11 TFS79703
			this.InitializePaneProperty();


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
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

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

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnMouseLeave	
    
		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Pane

		private static readonly DependencyPropertyKey PanePropertyKey =
			DependencyProperty.RegisterReadOnly("Pane",
			typeof(ContentPane), typeof(PaneHeaderPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPaneChanged)));

        private static void OnPaneChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            PaneHeaderPresenter php = target as PaneHeaderPresenter;

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
            if (php != null)
            {
                ContentPane oldpane = e.OldValue as ContentPane;

                if (oldpane != null)
                    oldpane.ClearHeader(php);

                ContentPane newpane = e.NewValue as ContentPane;

                if (newpane != null)
                    newpane.InitializeHeader(php);
            }
        }
		/// <summary>
		/// Identifies the <see cref="Pane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneProperty =
			PanePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated ContentPane that the header represents.
		/// </summary>
		/// <seealso cref="PaneProperty"/>
		//[Description("Returns the associated ContentPane that the header represents.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public ContentPane Pane
		{
			get
			{
				return (ContentPane)this.GetValue(PaneHeaderPresenter.PaneProperty);
			}
		}

		#endregion //Pane

		#endregion //Public Properties

		#region Private Properties

		#region HiddenMenuItem
		private MenuItem HiddenMenuItem
		{
			get
			{
				if (null == this._hiddenMenuItem)
				{
					this._hiddenMenuItem = new MenuItem();
					this._hiddenMenuItem.Visibility = Visibility.Collapsed;
				}

				return this._hiddenMenuItem;
			}
		}
		#endregion //HiddenMenuItem 

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
            // set Common states
            if (this.IsEnabled)
            {
                if (this.IsMouseOver)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            }
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);

            ContentPane pane = this.Pane;

            // set active states
            if (pane != null)
            {
                if (pane.IsActivePane)
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
                else
                {
                    if (pane.IsActiveDocument)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateActiveDocument, VisualStateUtilities.StateInactive);
                    else
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);
                }
            }
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            PaneHeaderPresenter php = target as PaneHeaderPresenter;

            php.UpdateVisualStates();
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        internal protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        internal protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods

        #endregion //Protected Methods

		#region Private Methods

		// AS 6/27/11 TFS79703
		#region InitializePaneProperty
		private void InitializePaneProperty()
		{
			ContentPane cp = this.TemplatedParent as ContentPane ?? this.DataContext as ContentPane;
			this.SetValue(PanePropertyKey, cp);
		} 
		#endregion //InitializePaneProperty

		#region OnDataContextChanged
		private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 6/27/11 TFS79703
			//d.SetValue(PanePropertyKey, e.NewValue as ContentPane);
			((PaneHeaderPresenter)d).InitializePaneProperty();
		}
		#endregion //OnDataContextChanged

		#region OnMouseButtonDown
		private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			PaneHeaderPresenter paneHeader = sender as PaneHeaderPresenter;
			ContentPane pane = null != paneHeader ? paneHeader.Pane : null;

			if (null != pane)
			{
				bool wasHandled = e.Handled;

				if (e.ChangedButton == MouseButton.Left && wasHandled == false && e.ClickCount == 2)
				{
					XamDockManager dockManager = XamDockManager.GetDockManager(pane);

					if (null != dockManager && dockManager.ProcessPaneHeaderDoubleClick(pane))
					{
						e.Handled = true;
						return;
					}
				}

				if (pane.ActivateInternal())
				{
					e.Handled = true;
				}

				if (wasHandled == false)
					DragManager.ProcessMouseDown(paneHeader, e);
			}
		} 
		#endregion //OnMouseButtonDown

		#region OnSubmenuOpened
		private static void OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			PaneHeaderPresenter header = sender as PaneHeaderPresenter;

			if (null != header &&
				header._positionMenuItem == e.OriginalSource &&
				header.Pane != null)
			{
				header._positionMenuItem.Items.Clear();
				header._positionMenuItem.Items.Add(header.HiddenMenuItem);

				// have the pane centralize the logic for creating its menu items
				header.Pane.InitializeMenu(header._positionMenuItem.Items);
			}
		} 
		#endregion //OnSubmenuOpened

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