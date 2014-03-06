using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Arranges the buttons in the header of a menu tool.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class MenuButtonArea : Control
	{
		#region Member Variables

		private Size _lastArrangeSize;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="MenuButtonArea"/> class.
		/// </summary>
		public MenuButtonArea()
		{
		}

		static MenuButtonArea()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButtonArea), new FrameworkPropertyMetadata(typeof(MenuButtonArea)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="arrangeBounds">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			this._lastArrangeSize = arrangeBounds;

			return base.ArrangeOverride(arrangeBounds);
		} 
			#endregion //ArrangeOverride

			#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the mouse is pressed down on the menu button area.
		/// </summary>
		/// <param name="e">Provides information about the mouse event args.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			MenuTool menu = this.MenuTool;

			// if the button is segmented and the segmented button is disabled then eat the mouse message
			// when it gets here so that the menu doesn't open. if the click were in the dropdown area
			// then we wouldn't get to here. just to be safe, we'll only eat the message if its segmented
			// and the segment is disabled.
			if (menu != null && menu.ButtonType != MenuToolButtonType.DropDown && menu.CanExecuteSegmentedButton() == false)
				e.Handled = true;

			base.OnMouseLeftButtonDown(e);
		} 
			#endregion //OnMouseLeftButtonDown

			#region OnVisualParentChanged

		/// <summary>
		/// Invoked when the parent element of this element reports a change to its underlying visual parent.
		/// </summary>
		/// <param name="oldParent">The previous parent. This may be null if the element did not have a parent element previously.</param>
		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			base.OnVisualParentChanged(oldParent);

			MenuTool menuTool = Utilities.GetAncestorFromType(this, typeof(MenuTool),  true) as MenuTool;
			if (menuTool != null)
			{
				this.SetValue(MenuButtonArea.MenuToolPropertyKey, menuTool);
				this.SetBinding(MenuButtonArea.IsActiveInternalProperty, Utilities.CreateBindingObject(XamRibbon.IsActiveProperty, BindingMode.OneWay, menuTool));
				this.SetBinding(MenuButtonArea.SizingModeInternalProperty, Utilities.CreateBindingObject(RibbonToolHelper.SizingModeProperty, BindingMode.OneWay, menuTool));
				this.SetBinding(RibbonGroupPanel.SizingModeVersionProperty, Utilities.CreateBindingObject(RibbonGroupPanel.SizingModeVersionProperty, BindingMode.OneWay, menuTool));
				this.SetBinding(MenuButtonArea.LocationInternalProperty, Utilities.CreateBindingObject(XamRibbon.LocationProperty, BindingMode.OneWay, menuTool));
				this.SetBinding(MenuButtonArea.HasImageInternalProperty, Utilities.CreateBindingObject(RibbonToolHelper.HasImageProperty, BindingMode.OneWay, menuTool));

				// AS 10/3/07
				// We're not binding the IsCheckable of the MenuItem any more so bind to the ButtonType above.
				//
				Binding binding = Utilities.CreateBindingObject(MenuTool.ButtonTypeProperty, BindingMode.OneWay, menuTool);
				binding.Converter = MenuTool.ButtonTypeToIsCheckableConverter.Instance;
				this.SetBinding(MenuButtonArea.IsCheckableInternalProperty, binding);

				if (menuTool.Location == ToolLocation.QuickAccessToolbar)
				{
					XamRibbon ribbon = XamRibbon.GetRibbon(this);
					QuickAccessToolbar qat = ribbon != null ? ribbon.QuickAccessToolbar : null;

					if (null != qat && menuTool == qat.QuickCustomizeMenu)
						this.SetValue(MenuButtonArea.IsQuickCustomizeMenuPropertyKey, KnownBoxes.TrueBox);
				}
			}

			MenuToolPresenter menuToolPresenter = Utilities.GetAncestorFromType(this, typeof(MenuToolPresenter),  true) as MenuToolPresenter;
			if (menuToolPresenter != null)
			{
				this.SetValue(MenuButtonArea.MenuToolPresenterPropertyKey, menuToolPresenter);
				this.SetBinding(MenuButtonArea.IsSegmentedInternalProperty, Utilities.CreateBindingObject(MenuToolPresenter.IsSegmentedProperty, BindingMode.OneWay, menuToolPresenter));
				// AS 10/3/07
				// We're not binding the IsCheckable of the MenuItem any more so bind to the ButtonType above.
				//
				//this.SetBinding(MenuButtonArea.IsCheckableInternalProperty, Utilities.CreateBindingObject(MenuToolPresenter.IsCheckableProperty, BindingMode.OneWay, menuToolPresenter));
			}

		} 
			#endregion //OnVisualParentChanged

		#endregion //Base Class Overrides 

		#region Properties

			#region Public Properties

				#region HasImage

		private static readonly DependencyPropertyKey HasImagePropertyKey =
			DependencyProperty.RegisterReadOnly("HasImage",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasImageProperty =
			HasImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true is the associated MenuTool has a large or small image.
		/// </summary>
		/// <seealso cref="HasImageProperty"/>
		/// <see cref="MenuTool"/>
		/// <see cref="XamRibbon"/>
		/// <see cref="Infragistics.Windows.Ribbon.MenuTool.HasImageProperty"/>
		//[Description("Returns true is the associated MenuTool has a large or small image.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool HasImage
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.HasImageProperty);
			}
		}

				#endregion //HasImage

				#region IsActive

		private static readonly DependencyPropertyKey IsActivePropertyKey =
			DependencyProperty.RegisterReadOnly("IsActive",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty =
			IsActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true is the associated MenuTool is the ActiveItem
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		/// <see cref="MenuTool"/>
		/// <see cref="XamRibbon"/>
		/// <see cref="XamRibbon.ActiveItem"/>
		/// <see cref="XamRibbon.IsActiveProperty"/>
		/// <see cref="XamRibbon.GetIsActive"/>
		//[Description("Returns true is the associated MenuTool is the ActiveItem")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsActiveProperty);
			}
		}

				#endregion //IsActive

				#region IsCheckable

		private static readonly DependencyPropertyKey IsCheckablePropertyKey =
			DependencyProperty.RegisterReadOnly("IsCheckable",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsCheckable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCheckableProperty =
			IsCheckablePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the associated MenuTool is checkable.
		/// </summary>
		/// <seealso cref="IsCheckableProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		//[Description("Returns whether the associated MenuTool is checkable.")]
		//[Category("Ribbon Properties")]
		public bool IsCheckable
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsCheckableProperty);
			}
		}

				#endregion //IsCheckable

				#region IsQuickCustomizeMenu

		private static readonly DependencyPropertyKey IsQuickCustomizeMenuPropertyKey =
			DependencyProperty.RegisterReadOnly("IsQuickCustomizeMenu",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsQuickCustomizeMenu"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsQuickCustomizeMenuProperty =
			IsQuickCustomizeMenuPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the associated menu tool represents the quick customize menu of the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		/// <seealso cref="IsQuickCustomizeMenuProperty"/>
		//[Description("Returns a boolean indicating whether the associated menu tool represents the quick customize menu of the QuickAccessToolbar.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsQuickCustomizeMenu
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsQuickCustomizeMenuProperty);
			}
		}

				#endregion //IsQuickCustomizeMenu

				#region IsSegmented

		private static readonly DependencyPropertyKey IsSegmentedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsSegmented",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsSegmented"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSegmentedProperty =
			IsSegmentedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the MenuButtonArea is segmented.
		/// </summary>
		/// <seealso cref="IsSegmentedProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		//[Description("Returns whether the associated MenuTool is segmented.")]
		//[Category("Ribbon Properties")]
		public bool IsSegmented
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsSegmentedProperty);
			}
		}

				#endregion //IsSegmented

				#region Location

		private static readonly DependencyPropertyKey LocationPropertyKey =
			DependencyProperty.RegisterReadOnly("Location",
			typeof(ToolLocation), typeof(MenuButtonArea), new FrameworkPropertyMetadata(RibbonKnownBoxes.ToolLocationUnknownBox));

		/// <summary>
		/// Identifies the <see cref="Location"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LocationProperty =
			LocationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the current location for the associated menu tool.
		/// </summary>
		/// <seealso cref="LocationProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//[Description("Returns the current location for the associated menu tool.")]
		//[Category("Ribbon Properties")]
		public ToolLocation Location
		{
			get
			{
				return (ToolLocation)this.GetValue(MenuButtonArea.LocationProperty);
			}
		}

				#endregion //Location

				#region MenuTool

		private static readonly DependencyPropertyKey MenuToolPropertyKey =
			DependencyProperty.RegisterReadOnly("MenuTool",
			typeof(MenuTool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="MenuTool"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MenuToolProperty =
			MenuToolPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the MenuTool associated with this MenuButtonArea.
		/// </summary>
		/// <seealso cref="MenuToolProperty"/>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MenuTool MenuTool
		{
			get
			{
				return (MenuTool)this.GetValue(MenuButtonArea.MenuToolProperty);
			}
		}

				#endregion //MenuTool

				#region MenuToolPresenter

		private static readonly DependencyPropertyKey MenuToolPresenterPropertyKey =
			DependencyProperty.RegisterReadOnly("MenuToolPresenter",
			typeof(MenuToolPresenter), typeof(MenuButtonArea), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="MenuToolPresenter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MenuToolPresenterProperty =
			MenuToolPresenterPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated MenuToolPresenter.
		/// </summary>
		/// <seealso cref="MenuToolPresenterProperty"/>
		//[Description("Returns the associated MenuToolPresenter.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public MenuToolPresenter MenuToolPresenter
		{
			get
			{
				return (MenuToolPresenter)this.GetValue(MenuButtonArea.MenuToolPresenterProperty);
			}
		}

				#endregion //MenuToolPresenter

				#region SizingMode

		private static readonly DependencyPropertyKey SizingModePropertyKey =
			DependencyProperty.RegisterReadOnly("SizingMode",
			typeof(RibbonToolSizingMode), typeof(MenuButtonArea), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox));

		/// <summary>
		/// Identifies the <see cref="SizingMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SizingModeProperty =
			SizingModePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the current sizing mode for the associated menu tool.
		/// </summary>
		/// <seealso cref="SizingModeProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//[Description("Returns the current sizing mode for the associated menu tool.")]
		//[Category("Ribbon Properties")]
		public RibbonToolSizingMode SizingMode
		{
			get
			{
				return (RibbonToolSizingMode)this.GetValue(MenuButtonArea.SizingModeProperty);
			}
		}

				#endregion //SizingMode

			#endregion //Properties

			#region Internal Properties

				#region HasImageInternal

		private static readonly DependencyProperty HasImageInternalProperty = DependencyProperty.Register("HasImageInternal",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnHasImageInternalChanged)));

		private static void OnHasImageInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuButtonArea mba = target as MenuButtonArea;

			if (mba != null)
				mba.SetValue(HasImagePropertyKey, e.NewValue);
		}

				#endregion //HasImageInternal

				#region IsActiveInternal

		private static readonly DependencyProperty IsActiveInternalProperty = DependencyProperty.Register("IsActiveInternal",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveInternalChanged)));

		private static void OnIsActiveInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuButtonArea mba = target as MenuButtonArea;

			if (mba != null)
				mba.SetValue(IsActivePropertyKey, e.NewValue);
		}

		private bool IsActiveInternal
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsActiveInternalProperty);
			}
			set
			{
				this.SetValue(MenuButtonArea.IsActiveInternalProperty, value);
			}
		}

				#endregion //IsActiveInternal

				#region IsCheckableInternal

		private static readonly DependencyProperty IsCheckableInternalProperty = DependencyProperty.Register("IsCheckableInternal",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsCheckableInternalChanged)));

		private static void OnIsCheckableInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuButtonArea mba = target as MenuButtonArea;

			if (mba != null)
				mba.SetValue(IsCheckablePropertyKey, e.NewValue);
		}

		private bool IsCheckableInternal
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsCheckableInternalProperty);
			}
			set
			{
				this.SetValue(MenuButtonArea.IsCheckableInternalProperty, value);
			}
		}

				#endregion //IsCheckableInternal

				#region IsSegmentedInternal

		private static readonly DependencyProperty IsSegmentedInternalProperty = DependencyProperty.Register("IsSegmentedInternal",
			typeof(bool), typeof(MenuButtonArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSegmentedInternalChanged)));

		private static void OnIsSegmentedInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuButtonArea mba = target as MenuButtonArea;

			if (mba != null)
				mba.SetValue(IsSegmentedPropertyKey, e.NewValue);
		}

		private bool IsSegmentedInternal
		{
			get
			{
				return (bool)this.GetValue(MenuButtonArea.IsSegmentedInternalProperty);
			}
			set
			{
				this.SetValue(MenuButtonArea.IsSegmentedInternalProperty, value);
			}
		}

				#endregion //IsSegmentedInternal

				#region LastArrangeSize
		internal Size LastArrangeSize
		{
			get { return this._lastArrangeSize; }
		} 
				#endregion //LastArrangeSize

				#region LocationInternal

		private static readonly DependencyProperty LocationInternalProperty = DependencyProperty.Register("LocationInternal",
			typeof(ToolLocation), typeof(MenuButtonArea), new FrameworkPropertyMetadata(RibbonKnownBoxes.ToolLocationUnknownBox, new PropertyChangedCallback(OnLocationInternalChanged)));

		private static void OnLocationInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuButtonArea mba = target as MenuButtonArea;

			if (mba != null)
				mba.SetValue(LocationPropertyKey, e.NewValue);
		}

		private ToolLocation LocationInternal
		{
			get
			{
				return (ToolLocation)this.GetValue(MenuButtonArea.LocationInternalProperty);
			}
			set
			{
				this.SetValue(MenuButtonArea.LocationInternalProperty, value);
			}
		}

				#endregion //LocationInternal

				#region SizingModeInternal

		private static readonly DependencyProperty SizingModeInternalProperty = DependencyProperty.Register("SizingModeInternal",
			typeof(RibbonToolSizingMode), typeof(MenuButtonArea), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox, new PropertyChangedCallback(OnSizingModeInternalChanged)));

		private static void OnSizingModeInternalChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuButtonArea mba = target as MenuButtonArea;

			if (mba != null)
				mba.SetValue(SizingModePropertyKey, e.NewValue);
		}

		private RibbonToolSizingMode SizingModeInternal
		{
			get
			{
				return (RibbonToolSizingMode)this.GetValue(MenuButtonArea.SizingModeInternalProperty);
			}
			set
			{
				this.SetValue(MenuButtonArea.SizingModeInternalProperty, value);
			}
		}

				#endregion //SizingModeInternal

			#endregion //Internal Properties
		
		#endregion //Properties

		#region Methods

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