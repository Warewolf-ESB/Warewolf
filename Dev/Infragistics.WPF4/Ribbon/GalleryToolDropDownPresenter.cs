using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Displays <see cref="GalleryItem"/>s in the dropdown portion of the <see cref="MenuTool"/> containing the <see cref="GalleryTool"/>.
	/// </summary>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class GalleryToolDropDownPresenter : ItemsControl
	{
		#region Member Variables

		private GalleryTool									_galleryTool;
		private ItemsType									_itemsType = ItemsType.None;

		#endregion //Member Variables

		#region Constructor

		internal GalleryToolDropDownPresenter(GalleryTool galleryTool)
		{
			this._galleryTool = galleryTool;

			// JJD 10/24/07
			// Bind the AllowResizeProperty to the tool's AllowResizeDropDownProperty
			if (this._galleryTool != null)
				this.SetBinding(AllowResizeProperty, Utilities.CreateBindingObject(GalleryTool.AllowResizeDropDownProperty, BindingMode.OneWay, galleryTool));
		}

		static GalleryToolDropDownPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GalleryToolDropDownPresenter), new FrameworkPropertyMetadata(typeof(GalleryToolDropDownPresenter)));

            // JJD 11/20/07 - BR27066
            // Default the focusable property to false so it doesn't interfere with the normal focus flow
            FrameworkElement.FocusableProperty.OverrideMetadata(typeof(GalleryToolDropDownPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ClearContainerForItemOverride

		/// <summary>
		/// Undoes any preparation done by the container to 'host' the item.
		/// </summary>
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);

			if (element is GalleryItemPresenter)
				((GalleryItemPresenter)element).ClearContainerForItem(item as GalleryItem);
		}

			#endregion //ClearContainerForItemOverride	
    
			#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item in the list.
		/// </summary>
		/// <returns>The newly created container.</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			if (this.DropDownItemsType == ItemsType.Items)
				return new GalleryItemPresenter(false, this._galleryTool, null);

			// GetContainerForItemOverride shouldn't get called when the items type is Groups since IsItemItsOwnContainer returns true for GalleryItemGroups,
			// so complain if we are here.
			Debug.Fail("GetContainerForItemOverride called unexpectedly in GalleryToolDropDownPresenter!");
			return null;
		}

			#endregion //GetContainerForItemOverride	

			#region HitTestCore

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="hitTestParameters"></param>
		/// <returns></returns>
		/// <remarks>
		/// <p class="body">
		/// This method is overridden on this class to make sure the element gets mouse messages
		/// regardless of whether its background is transparent.
		/// </p>
		/// </remarks>
		protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
		{
			Rect rect = new Rect( new Point( ), this.RenderSize );
			if ( rect.Contains( hitTestParameters.HitPoint ) )
				return new PointHitTestResult( this, hitTestParameters.HitPoint );

			return base.HitTestCore( hitTestParameters );
		}

			#endregion // HitTestCore
    
			#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item requires a separate container.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>True if the item does not require a wrapper</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			if (this.DropDownItemsType == ItemsType.Items)
				return item is GalleryItemPresenter;

			if (this.DropDownItemsType == ItemsType.Groups)
				return item is GalleryItemGroup;

			Debug.Fail("Unknown ItemsType in GalleryToolDropDownPresenter!");
			return false;
		}

			#endregion //IsItemItsOwnContainerOverride	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GalleryToolDropDownPresenter"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.GalleryToolDropDownPresenterAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GalleryToolDropDownPresenterAutomationPeer(this);
        }
            #endregion

            // JJD 11/20/07 - BR27066
            #region OnLostKeyboardFocus

        /// <summary>
        /// Invoked when an unhandled System.Windows.Input.Keyboard.LostKeyboardFocus�attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The System.Windows.Input.KeyboardFocusChangedEventArgs that contains the event data.</param>
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (this._galleryTool == null || e.Source != this)
                return;

            DependencyObject newfocus = e.NewFocus as DependencyObject;

            // JJD 11/20/07 - BR27066
            // If focus is moving out of the this area then let the gallery tool know
            if (newfocus == null ||
                !this.IsAncestorOf(newfocus))
                this._galleryTool.OnMouseLeaveItemArea(this);
        }

            #endregion //OnLostKeyboardFocus	
    
			#region OnMouseEnter

		/// <summary>
		/// Raised when the mouse enters the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (this.GalleryTool != null)
				this.GalleryTool.OnMouseEnterItemArea(this);
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
				this.GalleryTool.OnMouseLeaveItemArea(this);
		}

			#endregion //OnMouseLeave	
    
			#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);

			if (element is GalleryItemPresenter)
				((GalleryItemPresenter)element).PrepareContainerForItem(item as GalleryItem);
		}

			#endregion //PrepareContainerForItemOverride	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Public properties

                // JJD 11/07/07 - added property to support triggering of separators
                #region IsFirstInMenu

        internal static readonly DependencyPropertyKey IsFirstInMenuPropertyKey =
            DependencyProperty.RegisterReadOnly("IsFirstInMenu",
            typeof(bool), typeof(GalleryToolDropDownPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsFirstInMenu"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsFirstInMenuProperty =
            IsFirstInMenuPropertyKey.DependencyProperty;

        /// <summary>
        /// Determines if the gallery is the first visible item in the menu (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is set by the <see cref="MenuTool"/> if the gallery is the first visible item in the menu. It is intended for use within the style of the tool to trigger the display of separators.</para>
        /// </remarks>
        /// <value>Returns true if the gallery is the first visible item in the menu.</value>
        /// <seealso cref="IsFirstInMenuProperty"/>
        /// <seealso cref="IsLastInMenuProperty"/>
        /// <seealso cref="IsLastInMenu"/>
       //[Description("Returns true if the gallery is the first visible item in the menu")]
        //[Category("Ribbon Properties")]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsFirstInMenu
        {
            get
            {
                return (bool)this.GetValue(GalleryToolDropDownPresenter.IsFirstInMenuProperty);
            }
        }

                #endregion //IsFirstInMenu

                // JJD 11/07/07 - added property to support triggering of separators
                #region IsLastInMenu

        internal static readonly DependencyPropertyKey IsLastInMenuPropertyKey =
            DependencyProperty.RegisterReadOnly("IsLastInMenu",
            typeof(bool), typeof(GalleryToolDropDownPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsLastInMenu"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsLastInMenuProperty =
            IsLastInMenuPropertyKey.DependencyProperty;

        /// <summary>
        /// Determines if the gallery is the last visible item in the menu (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is set by the <see cref="MenuTool"/> if the gallery is the last visible item in the menu. It is intended for use within the style of the tool to trigger the display of separators.</para>
        /// </remarks>
        /// <value>Returns true if the gallery is the last visible item in the menu.</value>
        /// <seealso cref="IsLastInMenuProperty"/>
        /// <seealso cref="IsFirstInMenuProperty"/>
        /// <seealso cref="IsFirstInMenu"/>
        //[Description("Returns true if the gallery is the last visible item in the menu")]
        //[Category("Ribbon Properties")]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsLastInMenu
        {
            get
            {
                return (bool)this.GetValue(GalleryToolDropDownPresenter.IsLastInMenuProperty);
            }
        }

                #endregion //IsLastInMenu

				// JJD 10/24/07 added VerticalScrollBarVisibility
				#region VerticalScrollBarVisibility

		private static readonly DependencyPropertyKey VerticalScrollBarVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("VerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(GalleryToolDropDownPresenter), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityVisibleBox));

		/// <summary>
		/// Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
			VerticalScrollBarVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the vertical scrollbar (read-only)
		/// </summary>
		/// <value>
		/// 'Visible' if the associated <see cref="GalleryTool"/>'s <see cref="Infragistics.Windows.Ribbon.GalleryTool.AllowResizeDropDown"/> is true, otherwise 'Auto'.
		/// </value>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.AllowResizeDropDown"/>
		/// <seealso cref="VerticalScrollBarVisibilityProperty"/>
		//[Description("Returns the visibility of the vertical scrollbar (read-only)")]
		//[Category("Ribbon Properties")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(true)]
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(GalleryToolDropDownPresenter.VerticalScrollBarVisibilityProperty);
			}
		}

				#endregion //VerticalScrollBarVisibility

			#endregion //Public properties	
    
			#region Internal Properties

				#region DropDownItemsType

		internal ItemsType DropDownItemsType
		{
			get { return this._itemsType; }
			set 
			{
				if (value != this._itemsType)
				{
					this._itemsType = value;
					this.SetItemsPanel();
				}
			}
		}

				#endregion //DropDownItemsType

				#region GalleryTool

		internal GalleryTool GalleryTool
		{
			get	{ return this._galleryTool;	}
		}

				#endregion //GalleryTool

			#endregion //Internal Properties
		
			#region Private Properties

				// JJD 10/24/07 added AllowResize
				#region AllowResize

		private static readonly DependencyProperty AllowResizeProperty = DependencyProperty.Register("AllowResize",
			typeof(bool), typeof(GalleryToolDropDownPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAllowResizeChanged)));

		private static void OnAllowResizeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryToolDropDownPresenter gp = target as GalleryToolDropDownPresenter;

			if (gp != null)
			{
				if (true == (bool)e.NewValue)
					gp.ClearValue(VerticalScrollBarVisibilityPropertyKey);
				else
					gp.SetValue(VerticalScrollBarVisibilityPropertyKey, KnownBoxes.ScrollBarVisibilityAutoBox);
			}
		}
				#endregion //AllowResize

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

            #region Internal Methods

                // JJD 11/21/07 - BR27066
                #region GetFirstPresenterForItem

        internal GalleryItemPresenter GetFirstPresenterForItem(GalleryItem item)
        {
            Utilities.DependencyObjectSearchCallback<GalleryItemPresenter> callback = new Utilities.DependencyObjectSearchCallback<GalleryItemPresenter>(delegate(GalleryItemPresenter presenter)
            {
                return item == presenter.Item;
            });

            return Utilities.GetDescendantFromType<GalleryItemPresenter>(this, true, callback);
        }

                #endregion //GetFirstPresenterForItem

            #endregion //Internal Methods	
    
		    #region Private Methods

		        #region SetItemsPanel

		private void SetItemsPanel()
		{
			FrameworkElementFactory fefPanel = new FrameworkElementFactory(typeof(StackPanel));
			switch (this.DropDownItemsType)
			{
				case ItemsType.Groups:
					{
						fefPanel = new FrameworkElementFactory(typeof(StackPanel));
						break;
					}
				case ItemsType.Items:
					{
						// AS 10/2/07 BR26913
						//fefPanel = new FrameworkElementFactory(typeof(WrapPanel));
						fefPanel = new FrameworkElementFactory(typeof(GalleryWrapPanel));
						fefPanel.SetBinding(GalleryWrapPanel.MinColumnsProperty, Utilities.CreateBindingObject(GalleryTool.MinDropDownColumnsProperty, BindingMode.OneWay, this._galleryTool));
						fefPanel.SetBinding(GalleryWrapPanel.MaxColumnsProperty, Utilities.CreateBindingObject(GalleryTool.MaxDropDownColumnsProperty, BindingMode.OneWay, this._galleryTool));
						fefPanel.SetBinding(GalleryWrapPanel.PreferredColumnsProperty, Utilities.CreateBindingObject(GalleryTool.PreferredDropDownColumnsProperty, BindingMode.OneWay, this._galleryTool));
						break;
					}
			}

			ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate();

			itemsPanelTemplate.VisualTree = fefPanel;
			// do not bind the width/height of the inner element to that of the outer element
			// when i set the MinColumns of the gallery tool to some value (e.g. 8), the gallerywrappanel
			// was getting a constrained width (of about 222). i think we need to let the children
			// dictate the width of the panel
			//fefPanel.SetBinding(FrameworkElement.WidthProperty, Utilities.CreateBindingObject(FrameworkElement.ActualWidthProperty, BindingMode.OneWay, this));

			this.ItemsPanel = itemsPanelTemplate;
		}

				#endregion //SetItemsPanel

			#endregion //Private Methods

		#endregion Methods

		#region Nested Enumeration ItemsType (internal)

		internal enum ItemsType
		{
			None,
			Groups,
			Items
		}

		#endregion //Nested Enumeration ItemsType (internal)
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