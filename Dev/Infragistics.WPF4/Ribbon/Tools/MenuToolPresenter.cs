using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a MenuToolBase instance on a ribbon.
	/// </summary>
	/// <seealso cref="MenuToolBase"/>
	/// <seealso cref="MenuToolBase.CreateMenuToolPresenter"/>
	/// <seealso cref="XamRibbon"/>
	// AS 11/14/07 BR28450
	// Changed the part from a contentpresenter to a contentcontrol so its contents are in the logical tree.
	//
	[TemplatePart(Name = "PART_GalleryToolPreviewSite", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_MenuButtonArea", Type = typeof(MenuButtonArea))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class MenuToolPresenter : ToolMenuItem
	{
		#region Member Variables

		private ContentControl									_galleryToolPreviewSite;
		private GalleryTool										_galleryToolForPreview;
		private MenuButtonArea									_menuButtonArea;

		#endregion //Member Variables

		#region Constructor

		static MenuToolPresenter()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuToolPresenter), new FrameworkPropertyMetadata(typeof(MenuToolPresenter)));

			// AS 10/9/07
			// This defaults to None for menu items but we need to be able to arrow into/out of the menu item.
			//
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(MenuToolPresenter), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

			// AS 10/9/07
			// Put focus in the menu tool presenter instead of the menu tool.
			//
			//FrameworkElement.FocusableProperty.OverrideMetadata(typeof(MenuToolPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			// AS 10/18/07
			// When the menu tool is disabled because it has no items then we want to prevent the 
			// tool from receiving focus. Otherwise it will get the input focus and if someone presses
			// enter, etc. then the Click of the menu item will fire as if it were a leaf menu but 
			// our menu tool doesn't work that way.
			//
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(MenuToolPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceFocusable)));

			// AS 11/13/07 BR27990
			// This also needs to be done for menu tools within a menu so I'm moving this to ToolMenuItem.
			//
			//// AS 11/8/07 BR27990
			//ToolMenuItem.IsSegmentedInternalProperty.OverrideMetadata(typeof(MenuToolPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSegmentedInternalChanged)));

			// AS 6/3/08 BR32772
			// Several overriden methods on MenuItem have been decorated with a uipermission attribute
			// so we cannot override them and still be able to run in an xbap. Register a separate handler
			// instead.
			//
			EventManager.RegisterClassHandler(typeof(MenuToolPresenter), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnClassMouseLeftButtonDown));
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="MenuToolPresenter"/>
		/// </summary>
		public MenuToolPresenter()
		{
		}

		#endregion //Constructor	
    
		#region Base Class Overrides

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._galleryToolPreviewSite = this.GetTemplateChild("PART_GalleryToolPreviewSite") as ContentControl;

			if (this._menuButtonArea != null)
				BindingOperations.ClearBinding( this._menuButtonArea, IsEnabledProperty );

			this._menuButtonArea = this.GetTemplateChild("PART_MenuButtonArea") as MenuButtonArea;

			if (this._menuButtonArea != null)
			{
				this.CoerceValue(IsMenuButtonAreaEnabledProperty);
				this._menuButtonArea.SetBinding(IsEnabledProperty, Utilities.CreateBindingObject(IsMenuButtonAreaEnabledProperty, BindingMode.OneWay, this));
			}

			// synchronize the IsActive property
			this.OnIsActiveChanged();

			this.InitializeToolPreviewPresenter();
		}

			#endregion //OnApplyTemplate	

			// AS 6/29/11 TFS80311
			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns null since the elements are exposed as child elements of the popup within the menu.
		/// </summary>
		/// <returns>Returns null</returns>
		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return null;
		}
			#endregion //OnCreateAutomationPeer

			#region OnKeyDown
		
#region Infragistics Source Cleanup (Region)
















































































#endregion // Infragistics Source Cleanup (Region)

			#endregion //OnKeyDown

		    #region OnMouseLeftButtonDown

		
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnMouseLeftButtonDown

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region MenuTool

		/// <summary>
        /// Gets the associated MenuToolBase object (read-only)
        /// </summary>
        /// <seealso cref="MenuToolBase"/>
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ReadOnly(true)]
		public MenuToolBase MenuTool
		{
			// AS 6/9/08 BR32242
			// We should not assume that the tool is the datacontext - use the
			// previously existing RibbonTool property instead.
			//
			//get { return this.DataContext as MenuToolBase; }
			get { return this.RibbonTool as MenuToolBase; }
		}

				#endregion //MenuTool

				#region PreviewGalleryVisibility

		internal static readonly DependencyPropertyKey PreviewGalleryVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("PreviewGalleryVisibility",
			typeof(Visibility), typeof(MenuToolPresenter), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		/// <summary>
		/// Identifies the <see cref="PreviewGalleryVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreviewGalleryVisibilityProperty =
			PreviewGalleryVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if a <see cref="GalleryTool"/> preview is visible in the <see cref="MenuToolBase"/> button area.  To display a <see cref="GalleryTool"/>
		/// preview in the <see cref="MenuToolBase"/>, add the <see cref="GalleryTool"/> to the Menu's Items collection and set the Menu's <see cref="Infragistics.Windows.Ribbon.MenuTool.ShouldDisplayGalleryPreview"/>
		/// property to true.
		/// </summary>
		/// <seealso cref="PreviewGalleryVisibilityProperty"/>
		/// <seealso cref="Infragistics.Windows.Ribbon.MenuTool.ShouldDisplayGalleryPreview"/>
		/// <seealso cref="GalleryTool"/>
		//[Description("Returns true if a GalleryTool preview is visible in the MenuToolBase button area.  To display a GalleryTool preview in the MenuToolBase, add the GalleryTool to the Menu's Items collection and set the Menu's PreviewGalleryToolId to the XamRibbon.Id property of the GalleryTool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility PreviewGalleryVisibility
		{
			get
			{
				return (Visibility)this.GetValue(MenuToolPresenter.PreviewGalleryVisibilityProperty);
			}
		}

				#endregion //PreviewGalleryVisibility
    
			#endregion //Public Properties

			#region Internal Properties

				#region GalleryToolForPreview

		internal GalleryTool GalleryToolForPreview
		{
			get { return this._galleryToolForPreview; }
			set 
			{
				if (value != this._galleryToolForPreview)
				{
					this._galleryToolForPreview = value;
					this.InitializeToolPreviewPresenter();
				}
			}
		}

				#endregion //GalleryToolForPreview	

				#region GalleryToolPreviewPresenter
		internal GalleryToolPreviewPresenter GalleryToolPreviewPresenter
		{
			get
			{
				if (this._galleryToolPreviewSite != null)
					return this._galleryToolPreviewSite.Content as GalleryToolPreviewPresenter;

				return null;
			}
		} 
				#endregion //GalleryToolPreviewPresenter

				#region IsMenuButtonAreaEnabled

		internal static readonly DependencyProperty IsMenuButtonAreaEnabledProperty =
			DependencyProperty.Register("IsMenuButtonAreaEnabled",
			// AS 10/18/07
			// The menu should be considered enabled by default since we only coerce the value to false.
			//
			//typeof(bool), typeof(MenuToolPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, null, new CoerceValueCallback(CoerceIsMenuButtonAreaEnabled)));
			typeof(bool), typeof(MenuToolPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsMenuButtonAreaEnabledChanged), new CoerceValueCallback(CoerceIsMenuButtonAreaEnabled)));

		private static void OnIsMenuButtonAreaEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// AS 10/18/07
			// The Focusable state of a root level menu is affected by whether the menu has 
			// items (i.e. whether its treated as enabled).
			//
			d.CoerceValue(FocusableProperty);

			// AS 2/22/08 BR30230
			// If the menu is segmented then the state of the segmented button may not get updated.
			// However, its value is based on this state so we need to force the command infrastructure
			// to reevaluate the command.
			//
			MenuToolPresenter mtp = d as MenuToolPresenter;

			if (null != mtp && mtp.IsSegmented)
				CommandManager.InvalidateRequerySuggested();
		}

		private static object CoerceIsMenuButtonAreaEnabled(DependencyObject target, object value)
		{
			MenuToolPresenter mtp = target as MenuToolPresenter;

			if (mtp != null)
			{
				if ( (bool)value == true)
				{
					MenuToolBase menuToolBase = mtp.MenuTool;

					if (menuToolBase != null)
					{
						if (!menuToolBase.IsEnabled)
							return KnownBoxes.FalseBox;

						MenuTool mt = menuToolBase as MenuTool;

						if ( mt != null && 
							 mt.TotalEnabledChildren < 1 )
							// JM 06-20-10 TFS26088
							//return KnownBoxes.FalseBox;
						{
							MenuTool menu = mt as MenuTool;

							if (menu == null || menu.ButtonType == MenuToolButtonType.DropDown || !menu.CanExecuteSegmentedButton())
								return KnownBoxes.FalseBox;
						}
					}
				}
			}

			return value;
		}
				#endregion //IsMenuButtonAreaEnabled

                #region IsMenuButtonAreaEnabled

        internal bool IsMenuButtonAreaEnabled
        {
            get
            {
                return (bool)this.GetValue(IsMenuButtonAreaEnabledProperty);
            }
        }

                #endregion //IsMenuButtonAreaEnabled	
 
				#region ResizedColumnCount

		/// <summary>
		/// Used by the ribbon group resize logic to control the number of columns that should be displayed within the preview.
		/// </summary>
		internal static readonly DependencyProperty ResizedColumnCountProperty = DependencyProperty.Register("ResizedColumnCount",
			typeof(int), typeof(MenuToolPresenter), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

				#endregion //ResizedColumnCount

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region OnIsActiveChanged

		internal void OnIsActiveChanged()
		{
			// synchronize the IsActive property
			if (this._menuButtonArea != null && this.MenuTool != null)
				this._menuButtonArea.SetValue(XamRibbon.IsActivePropertyKey, this.MenuTool.GetValue(XamRibbon.IsActiveProperty));

			// AS 10/18/07
			GalleryToolPreviewPresenter galleryToolPreviewPresenter = this.GalleryToolPreviewPresenter;

			if (null != galleryToolPreviewPresenter)
				galleryToolPreviewPresenter.OnIsActiveChanged();
		}

				#endregion //OnIsActiveChanged

				#region OnMenuTool_IsEnabledChanged

		internal void OnMenuTool_IsEnabledChanged()
		{
			// synchronize the IsActive property
			if (this._menuButtonArea != null && this.MenuTool != null)
				this.CoerceValue(IsMenuButtonAreaEnabledProperty);
		}

				#endregion //OnMenuTool_IsEnabledChanged

			#endregion //Internal Methods	
    
			#region Private Methods

				// AS 10/18/07
				#region CoerceFocusable
		private static object CoerceFocusable(DependencyObject d, object newValue)
		{
			MenuToolPresenter mtp = (MenuToolPresenter)d;

			if (mtp.IsMenuButtonAreaEnabled == false)
				return KnownBoxes.FalseBox;

			return newValue;
		} 
				#endregion //CoerceFocusable

				#region InitializeToolPreviewPresenter
		private void InitializeToolPreviewPresenter()
		{
			// if we have a preview site...
			if (this._galleryToolPreviewSite != null)
			{
				GalleryToolPreviewPresenter galleryToolPreviewPresenter = this._galleryToolPreviewSite.Content as GalleryToolPreviewPresenter;

				// if its already hooked up to the correct tool then exit
				if (null != galleryToolPreviewPresenter)
				{
					if (galleryToolPreviewPresenter.GalleryTool == this._galleryToolForPreview)
						return;
				}

				if (this._galleryToolForPreview != null)
				{
					// Create a GalleryToolPreviewPresenter
					// AS 6/9/08 BR32242
					// We should not assume that the tool is the datacontext - use the
					// previously existing RibbonTool property instead.
					//
					//galleryToolPreviewPresenter = new GalleryToolPreviewPresenter(this.GalleryToolForPreview, this.DataContext as MenuToolBase);
					galleryToolPreviewPresenter = new GalleryToolPreviewPresenter(this.GalleryToolForPreview, this.RibbonTool as MenuToolBase);
					galleryToolPreviewPresenter.ItemsSource = this.GalleryToolForPreview.PreviewItems;
					galleryToolPreviewPresenter.SetBinding(ResizedColumnCountProperty, Utilities.CreateBindingObject(ResizedColumnCountProperty, BindingMode.OneWay, this));
					this._galleryToolPreviewSite.Content = galleryToolPreviewPresenter;

					// AS 10/18/07
					galleryToolPreviewPresenter.OnIsActiveChanged();
				}
				else
					this._galleryToolPreviewSite.Content = null;
			}
		}
				#endregion //InitializeToolPreviewPresenter 

				// AS 6/3/08 BR32772
				#region OnClassMouseLeftButtonDown
		private static void OnClassMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			MenuToolPresenter presenter = (MenuToolPresenter)sender;
			MenuTool menuTool = presenter.MenuTool as MenuTool;

			// JJD 11/29/07 - BR28786
			// Only eat the mouse messages when the menu is closed.
			// Otherwise when the menu is open and you click on the menutoolpresenter
			// in the ribbon it won't close up.
			//if (menuTool != null)
			if (menuTool != null && menuTool.IsOpen == false)
			{
				MenuToolPresenter mtp = menuTool.MenuToolPresenter;

				if (mtp != null)
				{
					if (mtp.PreviewGalleryVisibility == Visibility.Visible)
					{
						e.Handled = true;
						return;
					}
				}
			}
		} 
				#endregion //OnClassMouseLeftButtonDown

				// AS 11/8/07 BR27990
				#region OnIsSegmentedInternalChanged
		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnIsSegmentedInternalChanged

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