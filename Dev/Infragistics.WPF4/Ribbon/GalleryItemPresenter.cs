using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Displays a <see cref="GalleryItem"/> in <see cref="GalleryTool"/> preview area as well as the <see cref="GalleryTool"/> dropdown.
	/// </summary>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryTool"/>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,                GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,                  GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateHighlight,               GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,                GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,              GroupName = VisualStateUtilities.GroupSelection)]

    [TemplateVisualState(Name = VisualStateUtilities.StateDropdownArea,            GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StatePreviewArea,             GroupName = VisualStateUtilities.GroupLocation)]

    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class GalleryItemPresenter : Control
	{
		#region Member Variables

		private GalleryTool											_galleryTool;
		private GalleryItemGroup									_galleryItemGroup;

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="GalleryItemPresenter"/> class.
		/// </summary>
		internal GalleryItemPresenter(bool isInPreviewArea, GalleryTool galleryTool, GalleryItemGroup galleryItemGroup)
		{
			this.SetValue(GalleryItemPresenter.IsInPreviewAreaPropertyKey, isInPreviewArea);

			this._galleryTool		= galleryTool;
			this._galleryItemGroup	= galleryItemGroup;

			this.SetBinding(GalleryItemPresenter.GalleryItemSettingsVersionProperty, Utilities.CreateBindingObject(GalleryTool.ItemSettingsVersionProperty, BindingMode.OneWay, this._galleryTool));
			if (this._galleryItemGroup != null)
				this.SetBinding(GalleryItemPresenter.GroupItemSettingsVersionProperty, Utilities.CreateBindingObject(GalleryItemGroup.ItemSettingsVersionProperty, BindingMode.OneWay, this._galleryItemGroup));
		}

		static GalleryItemPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(typeof(GalleryItemPresenter)));

            // JJD 11/20/07 - BR27066
            // default the FocusVisualStyle to an empty style 
            FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(new Style()));
        }

		#endregion //Constructor

		#region Base Class Overrides

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GalleryItemPresenter"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.GalleryItemPresenterAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GalleryItemPresenterAutomationPeer(this);
        }
            #endregion
        
            #region OnInitialized

		///// <summary>
		///// Called when the element is initialized.
		///// </summary>
		///// <param name="e"></param>
		//protected override void OnInitialized(EventArgs e)
		//{
		//    base.OnInitialized(e);

		//    this.UpdateResolvedProperties()
		//}

			#endregion //OnInitialized	

            // JJD 11/20/07 - BR27066
            #region OnIsKeyboardFocusWithinChanged

        /// <summary>
        /// Invoked when the value of the <see cref="UIElement.IsKeyboardFocusWithin"/> property changes.
        /// </summary>
        /// <param name="e">Provides information about the property change</param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            // JJD 11/20/07 - BR27066
            // let the tool know when we get focus
            if (this.GalleryTool != null)
                this.GalleryTool.OnIsKeyboardFocusWithinItemChanged(this);

        }

            #endregion //OnIsKeyboardFocusWithinChanged	

            // JJD 11/20/07 - BR27066
            #region OnKeyDown

        /// <summary>
        /// Called when the element has input focus and a key is pressed.
        /// </summary>
        /// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || this._galleryTool == null || this.IsInPreviewArea)
                return;

            switch (e.Key)
            {
                case Key.Enter:
                    #region Process the Enter key
                    {
                        if (!this.IsSelected)
                        {
                            e.Handled = true;

                            this.SelectOrClickItem();
                        }
                        break;
                    }

                    #endregion //Process the Enter key

                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    #region Process the directional navigation keys

                    {
						// AS 5/5/10 TFS30526
						// Moved to helper method
						//
						//FocusNavigationDirection direction;
						//
						//#region Map key to direction enum
						//
						//switch (e.Key)
						//{
						//    default:
						//    case Key.Down:
						//        direction = FocusNavigationDirection.Down;
						//        break;
						//    case Key.Up:
						//        direction = FocusNavigationDirection.Up;
						//        break;
						//    case Key.Left:
						//        direction = FocusNavigationDirection.Left;
						//        break;
						//    case Key.Right:
						//        direction = FocusNavigationDirection.Right;
						//        break;
						//}
						//
						//#endregion //Map key to direction enum
                        FocusNavigationDirection direction = ToolMenuItem.GetDirection(e).Value;

                        IInputElement elementToFocus = this.PredictFocus(direction) as IInputElement;

                        if (elementToFocus != null)
                        {
                            // call OnMouseLeaveItem to stop the timer
                            this._galleryTool.OnMouseLeaveItem(this);

                            // set a flag to ignore mouse enters during thr focuds change
                            this._galleryTool.IgnoreMouseEnter = true;

                            //// see if the element to focus is a GalleryItemPresenter
                            //GalleryItemPresenter itemToFocus = elementToFocus as GalleryItemPresenter;

                            //// if not see if it is a descendant of a GalleryItemPresenter
                            //if (itemToFocus == null)
                            //    itemToFocus = Utilities.GetAncestorFromType(elementToFocus as DependencyObject, typeof(GalleryItemPresenter), false) as GalleryItemPresenter;

                            //// if not call OnMouseLeaveItemArea on the gallery tool to de-activate this item
                            //if (itemToFocus == null)
                            //{
                            //    GalleryToolDropDownPresenter ddp = Utilities.GetAncestorFromType(this, typeof(GalleryToolDropDownPresenter), false) as GalleryToolDropDownPresenter;

                            //    Debug.Assert(ddp != null);

                            //    if (ddp != null)
                            //        this._galleryTool.OnMouseLeaveItemArea(ddp);
                            //}
                            try
                            {
                                Keyboard.Focus(elementToFocus);
                            }
                            finally
                            {
                                // reset the flag asynchronously
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new XamRibbon.MethodInvoker(this._galleryTool.ResetIgnoreMouseEnter));
                            }

                            e.Handled = true;
                        }

                        break;
                    }

                    #endregion //Process the directional navigation keys

            }

        }

            #endregion //OnKeyDown	
    
			#region OnMouseEnter

		/// <summary>
		/// Raised when the mouse enters the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (this.GalleryTool != null)
				this.GalleryTool.OnMouseEnterItem(this);

            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

			#endregion //OnMouseEnter	
    
			#region OnMouseLeave

		/// <summary>
		/// Raised when the mouse leaves the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (this.GalleryTool != null)
				this.GalleryTool.OnMouseLeaveItem(this);

            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

		}

			#endregion //OnMouseLeave	
    
			#region OnMouseLeftButtonDown

		/// <summary>
		/// Raised when the left mouse button is pressed over the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

            // JJD 11/20/07 - BR27066
            // Moved logic into helper method
            #region Old code

            //GalleryTool galleryTool = this.GalleryTool;
            //GalleryToolItemBehavior itemBehavior = galleryTool.ItemBehavior;
            //if (itemBehavior == GalleryToolItemBehavior.StateButton)
            //    //galleryTool.SetSelectedItem(this.Item, this._galleryItemGroup);
            //    galleryTool.SetSelectedItem(this.Item, this._galleryItemGroup, this); // JM 11-12-07
            //else
            //    galleryTool.OnItemClicked(this.Item, this._galleryItemGroup);

            #endregion //Old code
			// JM 08-04-09 TFS20278 - Moved down to the OnMouseLeftButtonUp event handler below
			//this.SelectOrClickItem();
		}

			#endregion //OnMouseLeftButtonDown	

			#region OnMouseLeftButtonUp

		/// <summary>
		/// Raised when the left mouse button is released over the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			// JM 08-04-09 TFS20278 - Moved here from the OnMouseLeftButtonDown event handler above
			this.SelectOrClickItem();
		}

			#endregion //OnMouseLeftButtonUp

		#endregion Base Class Overrides

		#region Properties

		    #region Public Properties

		        #region HorizontalTextAlignmentResolved

		private static readonly DependencyPropertyKey HorizontalTextAlignmentResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("HorizontalTextAlignmentResolved",
			typeof(TextAlignment), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(TextAlignment.Center));

		/// <summary>
		/// Identifies the <see cref="HorizontalTextAlignmentResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalTextAlignmentResolvedProperty =
			HorizontalTextAlignmentResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved value of the <see cref="Infragistics.Windows.Ribbon.GalleryItemSettings.HorizontalTextAlignment"/> property on the <see cref="GalleryItemSettings"/> for the 
		/// <see cref="GalleryItem"/>. (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="GalleryItemSettings.HorizontalTextAlignment"/> property can be set on the <see cref="GalleryItem"/> itself and also on the
		/// containing <see cref="GalleryItemGroup"/> to serve as a default for all items in the <see cref="GalleryItemGroup"/>.  This 'resolved' version of the property
		/// returns the value set directly on the <see cref="GalleryItem"/> (if any).  If it is not set on the <see cref="GalleryItem"/> and the <see cref="GalleryItem"/>
		/// appears in the <see cref="GalleryTool"/> dropdown, the the value set in <see cref="Infragistics.Windows.Ribbon.GalleryItemGroup.ItemSettings"/> is used (if any).  If the value is not set 
		/// in either place, a value of 'Center' is returned.</p>
		/// </remarks>
		/// <seealso cref="HorizontalTextAlignmentResolvedProperty"/>
		/// <seealso cref="VerticalTextAlignmentResolved"/>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItemSettings"/>
		/// <seealso cref="GalleryItemSettings.HorizontalTextAlignment"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="IsInPreviewArea"/>
		//[Description("Returns the resolved value of the HorizontalTextAlignment property on the GalleryItemSettings for the GalleryItem. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public TextAlignment HorizontalTextAlignmentResolved
		{
			get
			{
				return (TextAlignment)this.GetValue(GalleryItemPresenter.HorizontalTextAlignmentResolvedProperty);
			}
		}

				#endregion //HorizontalTextAlignmentResolved

                // JJD 11/20/07 - BR27066
                // Added IsHighlighted property to support keyboard navigation
				#region IsHighlighted

		private static readonly DependencyPropertyKey IsHighlightedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsHighlighted",
			typeof(bool), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsHighlighted"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsHighlightedProperty =
			IsHighlightedPropertyKey.DependencyProperty;

		/// <summary>
        /// Returns true if the <see cref="GalleryItem"/> is currently highlighted inside the <see cref="GalleryToolDropDownPresenter"/>. (read-only)
		/// </summary>
		/// <seealso cref="IsHighlightedProperty"/>
		/// <seealso cref="SelectionDisplayModeResolved"/>
		/// <seealso cref="GalleryItemSettings.SelectionDisplayMode"/>
		/// <seealso cref="GalleryTool"/>
		//[Description("Returns true if the GalleryItem is currently highlighted inside the GalleryToolDropDownPresenter. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsHighlighted
		{
			get
			{
				return (bool)this.GetValue(GalleryItemPresenter.IsHighlightedProperty);
			}
		}

        internal void SetIsHighlighted(bool value)
        {
            if (value == true)
                this.SetValue(GalleryItemPresenter.IsHighlightedPropertyKey, KnownBoxes.TrueBox);
            else
                this.ClearValue(GalleryItemPresenter.IsHighlightedPropertyKey);
        }
				#endregion //IsHighlighted

				#region IsInPreviewArea

		private static readonly DependencyPropertyKey IsInPreviewAreaPropertyKey =
			DependencyProperty.RegisterReadOnly("IsInPreviewArea",
			typeof(bool), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsInPreviewArea"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInPreviewAreaProperty =
			IsInPreviewAreaPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="GalleryItem"/> is being displayed in the <see cref="GalleryTool"/> preview area. (read-only)
		/// </summary>
		/// <seealso cref="IsInPreviewAreaProperty"/>
		/// <seealso cref="GalleryTool"/>
		//[Description("Returns true if the GalleryItem is being displayed in the GalleryTool preview area. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsInPreviewArea
		{
			get
			{
				return (bool)this.GetValue(GalleryItemPresenter.IsInPreviewAreaProperty);
			}
		}

				#endregion //IsInPreviewArea

				#region IsSelected

		private static readonly DependencyPropertyKey IsSelectedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsSelected",
			typeof(bool), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty =
			IsSelectedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="GalleryItem"/> is the current selected item in the <see cref="GalleryTool"/>. (read-only)
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		/// <seealso cref="SelectionDisplayModeResolved"/>
		/// <seealso cref="GalleryItemSettings.SelectionDisplayMode"/>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.SelectedItem"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemBehavior"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemSelectedEvent"/>
		//[Description("Returns true if the GalleryItem is the current selected item in the GalleryTool. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsSelected
		{
			get
			{
				return (bool)this.GetValue(GalleryItemPresenter.IsSelectedProperty);
			}
		}

				#endregion //IsSelected

				#region Item

		internal static readonly DependencyPropertyKey ItemPropertyKey =
			DependencyProperty.RegisterReadOnly("Item",
			typeof(GalleryItem), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemChanged)));

		private static void OnItemChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryItemPresenter galleryItemPresenter = target as GalleryItemPresenter;

			// JM 10-24-07
			//if (galleryItemPresenter != null  &&  e.NewValue is GalleryItem)
			//    galleryItemPresenter.SetBinding(GalleryItemPresenter.ItemSettingsVersionProperty, Utilities.CreateBindingObject(GalleryItem.SettingsVersionProperty, BindingMode.OneWay, e.NewValue as GalleryItem));
			if (galleryItemPresenter != null)
			{
				if (e.NewValue is GalleryItem)
					galleryItemPresenter.SetBinding(GalleryItemPresenter.ItemSettingsVersionProperty, Utilities.CreateBindingObject(GalleryItem.SettingsVersionProperty, BindingMode.OneWay, e.NewValue as GalleryItem));

				galleryItemPresenter.UpdateResolvedProperties();
			}

		}

		/// <summary>
		/// Identifies the <see cref="Item"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemProperty =
			ItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="GalleryItem"/> associated with this GalleryItemPresenter. (read-only)
		/// </summary>
		/// <seealso cref="ItemProperty"/>
		/// <seealso cref="GalleryItem"/>
		//[Description("Returns the GalleryItem associated with this GalleryItemPresenter. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public GalleryItem Item
		{
			get
			{
				return (GalleryItem)this.GetValue(GalleryItemPresenter.ItemProperty);
			}
		}

				#endregion //Item

				#region SelectionDisplayModeResolved

		private static readonly DependencyPropertyKey SelectionDisplayModeResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("SelectionDisplayModeResolved",
			typeof(GalleryItemSelectionDisplayMode), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(GalleryItemSelectionDisplayMode.HighlightEntireItem));

		/// <summary>
		/// Identifies the <see cref="SelectionDisplayModeResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionDisplayModeResolvedProperty =
			SelectionDisplayModeResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value that determines what part of the <see cref="GalleryItem"/> (if any) is highlighted when the item is selected. (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="GalleryItemSettings.SelectionDisplayMode"/> property can be set on the <see cref="GalleryItem"/> itself and also on the
		/// containing <see cref="GalleryItemGroup"/> to serve as a default for all items in the <see cref="GalleryItemGroup"/>.  This 'resolved' version of the property
		/// returns the value set directly on the <see cref="GalleryItem"/> (if any).  If it is not set on the <see cref="GalleryItem"/> and the <see cref="GalleryItem"/>
		/// appears in the <see cref="GalleryTool"/> dropdown, the the value set in <see cref="Infragistics.Windows.Ribbon.GalleryItemGroup.ItemSettings"/> is used (if any).  If the value is not set 
		/// in either place, a value of 'HighlightEntireItem' is returned.</p>
		/// </remarks>
		/// <seealso cref="SelectionDisplayModeResolvedProperty"/>
		/// <seealso cref="IsSelected"/>
		/// <seealso cref="GalleryItemSettings.SelectionDisplayMode"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.SelectedItem"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemBehavior"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemSelectedEvent"/>
		/// <seealso cref="IsInPreviewArea"/>
		//[Description("Returns a value that determines what part of the GalleryItem (if any) is highlighted when the item is selected. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public GalleryItemSelectionDisplayMode SelectionDisplayModeResolved
		{
			get
			{
				return (GalleryItemSelectionDisplayMode)this.GetValue(GalleryItemPresenter.SelectionDisplayModeResolvedProperty);
			}
		}

				#endregion //SelectionDisplayModeResolved

				#region TextPlacementResolved

		private static readonly DependencyPropertyKey TextPlacementResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("TextPlacementResolved",
			typeof(TextPlacement), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(TextPlacement.BelowImage));

		/// <summary>
		/// Identifies the <see cref="TextPlacementResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextPlacementResolvedProperty =
			TextPlacementResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value that determines where the <see cref="GalleryItem"/> text is displayed with respect to the <see cref="GalleryItem"/> image. (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="GalleryItemSettings.TextPlacement"/> property can be set on the <see cref="GalleryItem"/> itself and also on the
		/// containing <see cref="GalleryItemGroup"/> to serve as a default for all items in the <see cref="GalleryItemGroup"/>.  This 'resolved' version of the property
		/// returns the value set directly on the <see cref="GalleryItem"/> (if any).  If it is not set on the <see cref="GalleryItem"/> and the <see cref="GalleryItem"/>
		/// appears in the <see cref="GalleryTool"/> dropdown, the the value set in <see cref="Infragistics.Windows.Ribbon.GalleryItemGroup.ItemSettings"/> is used (if any).  If the value is not set 
		/// in either place, a value of 'BelowImage' is returned.</p>
		/// </remarks>
		/// <seealso cref="TextPlacementResolvedProperty"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Text"/>
		/// <seealso cref="GalleryItem.Image"/>
		/// <seealso cref="GalleryItemSettings.TextPlacement"/>
		/// <seealso cref="IsInPreviewArea"/>
		//[Description("Returns a value that determines where the GalleryItem.Text is displayed with respect to the Gallerytem.Image. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public TextPlacement TextPlacementResolved
		{
			get
			{
				return (TextPlacement)this.GetValue(GalleryItemPresenter.TextPlacementResolvedProperty);
			}
		}

				#endregion //TextPlacementResolved

				#region TextVisibility

		private static readonly DependencyPropertyKey TextVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("TextVisibility",
			typeof(Visibility), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

		/// <summary>
		/// Identifies the <see cref="TextVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextVisibilityProperty =
			TextVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value that determines if the GalleryItem text is displayed. (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The returned value is based on the resolved setting of the <see cref="GalleryItemSettings.TextDisplayMode"/> property
		/// of the <see cref="GalleryItemSettings"/> object for the <see cref="GalleryItem"/> and the current location of the <see cref="GalleryItem"/>
		/// (i.e., in the <see cref="GalleryTool"/> preview area or the dropdown).  <see cref="GalleryItem"/>s that appear in the <see cref="GalleryTool"/>
		/// dropdown will display their text by default while <see cref="GalleryItem"/>s in the <see cref="GalleryTool"/> preview area will not display
		/// their text by default.</p>
		/// </remarks>
		/// <seealso cref="TextVisibilityProperty"/>
		/// <seealso cref="GalleryItemSettings.TextDisplayMode"/>
		/// <seealso cref="IsInPreviewArea"/>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="GalleryItem"/>
		//[Description("Returns a value that determines if the GalleryItem text is displayed. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility TextVisibility
		{
			get
			{
				return (Visibility)this.GetValue(GalleryItemPresenter.TextVisibilityProperty);
			}
		}

				#endregion //TextVisibility

				#region VerticalTextAlignmentResolved

		private static readonly DependencyPropertyKey VerticalTextAlignmentResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("VerticalTextAlignmentResolved",
			typeof(VerticalAlignment), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

		/// <summary>
		/// Identifies the <see cref="VerticalTextAlignmentResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalTextAlignmentResolvedProperty =
			VerticalTextAlignmentResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved value of the <see cref="Infragistics.Windows.Ribbon.GalleryItemSettings.VerticalTextAlignment"/> property on the <see cref="GalleryItemSettings"/> for the <see cref="GalleryItem"/> (read-only).
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="GalleryItemSettings.VerticalTextAlignment"/> property can be set on the <see cref="GalleryItem"/> itself and also on the
		/// containing <see cref="GalleryItemGroup"/> to serve as a default for all items in the <see cref="GalleryItemGroup"/>.  This 'resolved' version of the property
		/// returns the value set directly on the <see cref="GalleryItem"/> (if any).  If it is not set on the <see cref="GalleryItem"/> and the <see cref="GalleryItem"/>
		/// appears in the <see cref="GalleryTool"/> dropdown, the the value set in <see cref="Infragistics.Windows.Ribbon.GalleryItemGroup.ItemSettings"/> is used (if any).  If the value is not set 
		/// in either place, a value of 'Center' is returned.</p>
		/// </remarks>
		/// <seealso cref="VerticalTextAlignmentResolvedProperty"/>
		/// <seealso cref="HorizontalTextAlignmentResolved"/>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItemSettings"/>
		/// <seealso cref="GalleryItemSettings.VerticalTextAlignment"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="IsInPreviewArea"/>
		//[Description("Returns the resolved value of the VerticalTextAlignment property on the GalleryItemSettings for the GalleryItem. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public VerticalAlignment VerticalTextAlignmentResolved
		{
			get
			{
				return (VerticalAlignment)this.GetValue(GalleryItemPresenter.VerticalTextAlignmentResolvedProperty);
			}
		}

				#endregion //VerticalTextAlignmentResolved
	
			#endregion //Public Properties

			#region Internal Properties

				#region GalleryItemGroup

		internal GalleryItemGroup GalleryItemGroup
		{
			get { return this._galleryItemGroup; }
		}

				#endregion //GalleryItemGroup

				#region GalleryTool

		internal GalleryTool GalleryTool
		{
			get { return this._galleryTool; }
		}

				#endregion //GalleryTool

			#endregion //Internal properties

			#region Private Properties

				#region GalleryItemSettingsVersion

		private static readonly DependencyProperty GalleryItemSettingsVersionProperty = DependencyProperty.Register("GalleryItemSettingsVersion",
			typeof(int), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnItemSettingsVersionChanged)));

		private static void OnItemSettingsVersionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			 GalleryItemPresenter galleryItemPresenter = target as GalleryItemPresenter;
			 if (galleryItemPresenter != null)
				 galleryItemPresenter.UpdateResolvedProperties();
		}
				#endregion //GalleryItemSettingsVersion

				#region GroupItemSettingsVersion

		private static readonly DependencyProperty GroupItemSettingsVersionProperty = DependencyProperty.Register("GroupItemSettingsVersion",
			typeof(int), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnItemSettingsVersionChanged)));

				#endregion //GroupItemSettingsVersion

				#region ItemSettingsVersion

		private static readonly DependencyProperty ItemSettingsVersionProperty = DependencyProperty.Register("ItemSettingsVersion",
			typeof(int), typeof(GalleryItemPresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnItemSettingsVersionChanged)));

				#endregion //ItemSettingsVersion

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region ClearContainerForItem

		internal void ClearContainerForItem(GalleryItem galleryItem)
		{
			if (this.Item != null)
				this.Item.IsSelectedChangedInternal -= new EventHandler(ItemIsSelectedChanged);

			this.SetValue(GalleryItemPresenter.ItemPropertyKey, null);
		}

				#endregion //ClearContainerForItem

				#region PrepareContainerForItem

		internal void PrepareContainerForItem(GalleryItem galleryItem)
		{
			// JM 11-06-07 BR28257
			Debug.Assert(galleryItem != null, "GalleryItem is null in PrepareContainerForItem!");
			if (galleryItem == null)
				return;


			this.SetValue(GalleryItemPresenter.ItemPropertyKey, galleryItem);
			this.SetBinding(GalleryWrapPanel.ColumnSpanProperty, Utilities.CreateBindingObject(GalleryItem.ColumnSpanProperty, BindingMode.OneWay, galleryItem));

			if (this.Item != null)
			{
				// Initialize the value of our IsSelected property.
				this.SetValue(GalleryItemPresenter.IsSelectedPropertyKey, this.Item.IsSelected);

				// Listen to the IsSelectedChangedInternal event on the item so we can change sync IsSelected property with the
				// Item's IsSelected property.
				this.Item.IsSelectedChangedInternal += new EventHandler(ItemIsSelectedChanged);

				// JM 11-06-07 BR28257
				this.SetBinding(GalleryItemPresenter.ToolTipProperty, Utilities.CreateBindingObject(ToolTipService.ToolTipProperty, BindingMode.OneWay, galleryItem));

				// JM 03-09-09 TFS15046
				Style style = null;
				if (galleryItem.Settings									!= null &&
					galleryItem.Settings.GalleryItemPresenterStyleSelector	!= null)
					style = galleryItem.Settings.GalleryItemPresenterStyleSelector.SelectStyle(galleryItem ,this);

				if (style											== null &&
					galleryItem.Settings							!= null &&
					galleryItem.Settings.GalleryItemPresenterStyle	!= null)
					style = galleryItem.Settings.GalleryItemPresenterStyle;

				if (style																== null &&
					this._galleryTool.ItemSettings										!= null &&
					this._galleryTool.ItemSettings.GalleryItemPresenterStyleSelector	!= null)
					style = this._galleryTool.ItemSettings.GalleryItemPresenterStyleSelector.SelectStyle(galleryItem, this);

				if (style														== null &&
					this._galleryTool.ItemSettings								!= null &&
					this._galleryTool.ItemSettings.GalleryItemPresenterStyle	!= null)
					style = this._galleryTool.ItemSettings.GalleryItemPresenterStyle;

				if (style != null)
					this.SetValue(FrameworkElement.StyleProperty, style);
			}
		}

				#endregion //PrepareContainerForItem

			#endregion //Internal Methods

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            if (this.IsEnabled)
            {
                if (this.IsHighlighted)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateHighlight, VisualStateUtilities.StateNormal);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            }
            else
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDisabled, VisualStateUtilities.StateNormal);

            if (this.IsSelected)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);

            if (this.IsInPreviewArea)
                VisualStateManager.GoToState(this, VisualStateUtilities.StatePreviewArea, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDropdownArea, useTransitions);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            GalleryItemPresenter presenter = target as GalleryItemPresenter;

            if ( presenter != null )
                presenter.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

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

			#endregion //Protected methods

			#region Private Methods

				#region ItemIsSelectedChanged

		private void ItemIsSelectedChanged(object sender, EventArgs e)
		{
			GalleryItem item = sender as GalleryItem;
			if (item != null)
				this.SetValue(GalleryItemPresenter.IsSelectedPropertyKey, item.IsSelected);
		}

				#endregion //ItemIsSelectedChanged	

                // JJD 11/20/07 - BR27066
                #region SelectOrClickItem

        private void SelectOrClickItem()
        {
            GalleryTool galleryTool = this.GalleryTool;
            GalleryToolItemBehavior itemBehavior = galleryTool.ItemBehavior;
			if (itemBehavior == GalleryToolItemBehavior.StateButton)
				//galleryTool.SetSelectedItem(this.Item, this._galleryItemGroup);
				galleryTool.SetSelectedItem(this.Item, this._galleryItemGroup, this); // JM 11-12-07
			else
			{
				// JJD 12/9/11 - added GalleryItemPresenter parameter so we can expose it for TestAdvantage use
				//galleryTool.OnItemClicked(this.Item, this._galleryItemGroup);
				galleryTool.OnItemClicked(this.Item, this._galleryItemGroup, this);
			}

            // JJD 11/20/07 - BR27066
            // Make sure we clear the active item
            //galleryTool.ClearActiveItem();
			// JJD 12/9/11 - added GalleryItemPresenter so we can expose it for TestAdvantage use
			galleryTool.ClearActiveItem(this);
        }

                #endregion //SelectOrClickItem	
    
				#region UpdateResolvedProperties

		private void UpdateResolvedProperties()
		{
			// HorizontalTextAlignmentResolved
			TextAlignment horizontalTextAlignmentResolved = TextAlignment.Center;
			if (this.Item											!= null &&
				this.Item.Settings									!= null &&
				this.Item.Settings.HorizontalTextAlignment.HasValue			&&
				this.Item.Settings.HorizontalTextAlignment.Value	!= horizontalTextAlignmentResolved)
				horizontalTextAlignmentResolved = this.Item.Settings.HorizontalTextAlignment.Value;
			else
			if (this.GalleryItemGroup												!= null	&&
				this.GalleryItemGroup.ItemSettings									!= null &&
				this.GalleryItemGroup.ItemSettings.HorizontalTextAlignment.HasValue			&&
				this.GalleryItemGroup.ItemSettings.HorizontalTextAlignment.Value	!= horizontalTextAlignmentResolved)
				horizontalTextAlignmentResolved = this.GalleryItemGroup.ItemSettings.HorizontalTextAlignment.Value;
			else
			if (this.GalleryTool												!= null &&
				this.GalleryTool.ItemSettings									!= null &&
				this.GalleryTool.ItemSettings.HorizontalTextAlignment.HasValue			&&
				this.GalleryTool.ItemSettings.HorizontalTextAlignment.Value		!= horizontalTextAlignmentResolved)
				horizontalTextAlignmentResolved = this.GalleryTool.ItemSettings.HorizontalTextAlignment.Value;

			if (this.HorizontalTextAlignmentResolved != horizontalTextAlignmentResolved)
				this.SetValue(GalleryItemPresenter.HorizontalTextAlignmentResolvedPropertyKey, horizontalTextAlignmentResolved);


			// SelectionDisplayModeResolved
			GalleryItemSelectionDisplayMode selectionDisplayModeResolved = GalleryItemSelectionDisplayMode.HighlightEntireItem;
			if (this.Item								!= null										&&
				this.Item.Settings						!= null										&&
				this.Item.Settings.SelectionDisplayMode != GalleryItemSelectionDisplayMode.Default	&&
				this.Item.Settings.SelectionDisplayMode	!= selectionDisplayModeResolved)
				selectionDisplayModeResolved = this.Item.Settings.SelectionDisplayMode;
			else
			if (this.GalleryItemGroup										!= null										&&
				this.GalleryItemGroup.ItemSettings							!= null										&&
				this.GalleryItemGroup.ItemSettings.SelectionDisplayMode		!= GalleryItemSelectionDisplayMode.Default	&&
				this.GalleryItemGroup.ItemSettings.SelectionDisplayMode		!= selectionDisplayModeResolved)
				selectionDisplayModeResolved = this.GalleryItemGroup.ItemSettings.SelectionDisplayMode;
			else
			if (this.GalleryTool									!= null										&&
				this.GalleryTool.ItemSettings						!= null										&&
				this.GalleryTool.ItemSettings.SelectionDisplayMode	!= GalleryItemSelectionDisplayMode.Default	&&
				this.GalleryTool.ItemSettings.SelectionDisplayMode	!= selectionDisplayModeResolved)
				selectionDisplayModeResolved = this.GalleryTool.ItemSettings.SelectionDisplayMode;

			if (selectionDisplayModeResolved != this.SelectionDisplayModeResolved)
				this.SetValue(GalleryItemPresenter.SelectionDisplayModeResolvedPropertyKey, selectionDisplayModeResolved);


			// TextPlacementResolved
			TextPlacement textPlacementResolved = TextPlacement.BelowImage;
			if (this.Item							!= null						&&
				this.Item.Settings					!= null						&&
				this.Item.Settings.TextPlacement	!= TextPlacement.Default	&&
				this.Item.Settings.TextPlacement	!= textPlacementResolved)
				textPlacementResolved = this.Item.Settings.TextPlacement;
			else
			if (this.GalleryItemGroup								!= null						&&
				this.GalleryItemGroup.ItemSettings					!= null						&&
				this.GalleryItemGroup.ItemSettings.TextPlacement	!= TextPlacement.Default	&&
				this.GalleryItemGroup.ItemSettings.TextPlacement	!= textPlacementResolved)
				textPlacementResolved = this.GalleryItemGroup.ItemSettings.TextPlacement;
			else
			if (this.GalleryTool							!= null						&&
				this.GalleryTool.ItemSettings				!= null						&&
				this.GalleryTool.ItemSettings.TextPlacement != TextPlacement.Default	&&
				this.GalleryTool.ItemSettings.TextPlacement	!= textPlacementResolved)
				textPlacementResolved = this.GalleryTool.ItemSettings.TextPlacement;

			if (textPlacementResolved != this.TextPlacementResolved)
				this.SetValue(GalleryItemPresenter.TextPlacementResolvedPropertyKey, textPlacementResolved);


			// TextVisibility
			GalleryItemTextDisplayMode textDisplayModeResolved = GalleryItemTextDisplayMode.OnlyInDropDown;
			if (this.Item							!= null									&&
				this.Item.Settings					!= null									&&
				this.Item.Settings.TextDisplayMode	!= GalleryItemTextDisplayMode.Default	&&
				this.Item.Settings.TextDisplayMode	!= textDisplayModeResolved)
				textDisplayModeResolved = this.Item.Settings.TextDisplayMode;
			else
			if (this.GalleryItemGroup								!= null									&&
				this.GalleryItemGroup.ItemSettings					!= null									&&
				this.GalleryItemGroup.ItemSettings.TextDisplayMode	!= GalleryItemTextDisplayMode.Default	&&
				this.GalleryItemGroup.ItemSettings.TextDisplayMode	!= textDisplayModeResolved)
				textDisplayModeResolved = this.GalleryItemGroup.ItemSettings.TextDisplayMode;
			else
			if (this.GalleryTool								!= null									&&
				this.GalleryTool.ItemSettings					!= null									&&
				this.GalleryTool.ItemSettings.TextDisplayMode	!= GalleryItemTextDisplayMode.Default	&&
				this.GalleryTool.ItemSettings.TextDisplayMode	!= textDisplayModeResolved)
				textDisplayModeResolved = this.GalleryTool.ItemSettings.TextDisplayMode;

			if (textDisplayModeResolved		== GalleryItemTextDisplayMode.Always ||
				(textDisplayModeResolved	== GalleryItemTextDisplayMode.OnlyInDropDown && this.IsInPreviewArea == false))
				this.SetValue(GalleryItemPresenter.TextVisibilityPropertyKey, Visibility.Visible);
			else
			if (textDisplayModeResolved		== GalleryItemTextDisplayMode.Never ||
				(textDisplayModeResolved	== GalleryItemTextDisplayMode.OnlyInDropDown && this.IsInPreviewArea == true))
				this.SetValue(GalleryItemPresenter.TextVisibilityPropertyKey, Visibility.Collapsed);


			// VerticalTextAlignmentResolved
			VerticalAlignment verticalTextAlignmentResolved = VerticalAlignment.Center;
			if (this.Item											!= null &&
				this.Item.Settings									!= null &&
				this.Item.Settings.VerticalTextAlignment.HasValue			&&
				this.Item.Settings.VerticalTextAlignment.Value		!= verticalTextAlignmentResolved)
				verticalTextAlignmentResolved = this.Item.Settings.VerticalTextAlignment.Value;
			else
			if (this.GalleryItemGroup												!= null	&&
				this.GalleryItemGroup.ItemSettings									!= null &&
				this.GalleryItemGroup.ItemSettings.VerticalTextAlignment.HasValue			&&
				this.GalleryItemGroup.ItemSettings.VerticalTextAlignment.Value		!= verticalTextAlignmentResolved)
				verticalTextAlignmentResolved = this.GalleryItemGroup.ItemSettings.VerticalTextAlignment.Value;
			else
			if (this.GalleryTool												!= null &&
				this.GalleryTool.ItemSettings									!= null	&&
				this.GalleryTool.ItemSettings.VerticalTextAlignment.HasValue			&&
				this.GalleryTool.ItemSettings.VerticalTextAlignment.Value		!= verticalTextAlignmentResolved)
				verticalTextAlignmentResolved = this.GalleryTool.ItemSettings.VerticalTextAlignment.Value;

			if (this.VerticalTextAlignmentResolved != verticalTextAlignmentResolved)
				this.SetValue(GalleryItemPresenter.VerticalTextAlignmentResolvedPropertyKey, verticalTextAlignmentResolved);
		}

				#endregion UpdateResolvedProperties

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