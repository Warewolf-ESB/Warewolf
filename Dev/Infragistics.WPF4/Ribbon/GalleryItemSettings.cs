using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Contains settings that are applied to a <see cref="GalleryItem"/>.  <see cref="GalleryItemSettings"/> can be set directly on a 
	/// <see cref="GalleryItem"/> via its <see cref="GalleryItem.Settings"/> property	or on the <see cref="GalleryTool.ItemSettings"/> property to 
	/// serve as a default for all <see cref="GalleryItem"/>s.
	/// </summary>
	/// <remarks>
	/// <p class="body">The various property values in the GalleryItemSettings specified at the <see cref="GalleryTool"/> level 
	/// (via the <see cref="GalleryTool.ItemSettings"/> property) serve as the ultimate defaults for all <see cref="GalleryItem"/>s.  These values 
	/// can be overridden at two lower levels:
	/// <ul>
	/// <li>at the <see cref="GalleryItemGroup"/> level via the <see cref="GalleryItemGroup.ItemSettings"/> property.  The values specified there
	/// will override corresponding values set at the <see cref="GalleryTool"/> level, but could be further overridden at the <see cref="GalleryItem"/>
	/// level (see next bullet)</li>
	/// <li>at the <see cref="GalleryItem"/> level via the <see cref="GalleryItem.Settings"/> property.  The values specified here will override corresponding 
	/// values set at the <see cref="GalleryTool"/> and <see cref="GalleryItemGroup"/> levels</li>
	/// </ul>
	/// </p>
	/// </remarks>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryItem.Settings"/>
	/// <seealso cref="GalleryTool"/>
	/// <seealso cref="GalleryTool.ItemSettings"/>
	/// <seealso cref="GalleryItemGroup.ItemSettings"/>
	[StyleTypedProperty(Property = "GalleryItemPresenterStyle", StyleTargetType = typeof(GalleryItemPresenter))]	// JJD 9/4/07
	public class GalleryItemSettings : DependencyObjectNotifier
	{
		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="GalleryItemSettings"/> class.
		/// </summary>
		public GalleryItemSettings()
		{
		}

		#endregion //Constructor

        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed.
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region GalleryItemPresenterStyle

		/// <summary>
		/// Identifies the <see cref="GalleryItemPresenterStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GalleryItemPresenterStyleProperty = DependencyProperty.Register("GalleryItemPresenterStyle",
			typeof(Style), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the Style for the GalleryItemPresenter that is used to display the GalleryItem.
		/// </summary>
		/// <seealso cref="GalleryItemPresenterStyleProperty"/>
		/// <seealso cref="GalleryItemPresenter"/>
		/// <seealso cref="GalleryItem"/>
		//[Description("Returns/sets the Style for the GalleryItemPresenter that is used to display the GalleryItem.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public Style GalleryItemPresenterStyle
		{
			get
			{
				return (Style)this.GetValue(GalleryItemSettings.GalleryItemPresenterStyleProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.GalleryItemPresenterStyleProperty, value);
			}
		}

				#endregion //GalleryItemPresenterStyle

				#region GalleryItemPresenterStyleSelector

		/// <summary>
		/// Identifies the <see cref="GalleryItemPresenterStyleSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GalleryItemPresenterStyleSelectorProperty = DependencyProperty.Register("GalleryItemPresenterStyleSelector",
			typeof(StyleSelector), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets a Style Selector that can be used to dynamically assign a style for the <see cref="GalleryItemPresenter"/> used to display the 
		/// <see cref="GalleryItem"/>.
		/// </summary>
		/// <seealso cref="GalleryItemPresenterStyleSelectorProperty"/>
		/// <seealso cref="GalleryItemPresenterStyle"/>
		/// <seealso cref="GalleryItemPresenter"/>
		/// <seealso cref="GalleryItem"/>
		//[Description("Returns/sets a Style Selector that can be used to dynamically assign a style for the GalleryItemPresenter used to display the GalleryItem.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public StyleSelector GalleryItemPresenterStyleSelector
		{
			get
			{
				return (StyleSelector)this.GetValue(GalleryItemSettings.GalleryItemPresenterStyleSelectorProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.GalleryItemPresenterStyleSelectorProperty, value);
			}
		}

				#endregion //GalleryItemPresenterStyleSelector

				#region HorizontalTextAlignment

		/// <summary>
		/// Identifies the <see cref="HorizontalTextAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyProperty.Register("HorizontalTextAlignment",
			typeof(TextAlignment?), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the horizontal alignment of the <see cref="GalleryItem"/> text.
		/// </summary>
		/// <seealso cref="HorizontalTextAlignmentProperty"/>
		/// <seealso cref="VerticalTextAlignment"/>
		/// <seealso cref="GalleryItem.Text"/>
		//[Description("Returns/sets the horizontal alignment of the GalleryItem text.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<TextAlignment>))] // AS 5/15/08 BR32816
		public TextAlignment? HorizontalTextAlignment
		{
			get
			{
				return (TextAlignment?)this.GetValue(GalleryItemSettings.HorizontalTextAlignmentProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.HorizontalTextAlignmentProperty, value);
			}
		}

				#endregion //HorizontalTextAlignment

				#region SelectionDisplayMode

		/// <summary>
		/// Identifies the <see cref="SelectionDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionDisplayModeProperty = DependencyProperty.Register("SelectionDisplayMode",
			typeof(GalleryItemSelectionDisplayMode), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(GalleryItemSelectionDisplayMode.Default));

		/// <summary>
		/// Returns/sets a value that determines what area (if any) of the <see cref="GalleryItem"/> is highlighted when the item is selected.
		/// </summary>
		/// <seealso cref="SelectionDisplayModeProperty"/>
		/// <seealso cref="GalleryItemSelectionDisplayMode"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Image"/>
		/// <seealso cref="GalleryItem.Text"/>
		/// <seealso cref="GalleryTool.ItemSelected"/>
		/// <seealso cref="GalleryTool.SelectedItem"/>
		//[Description("Returns/sets a value that determines what area (if any) of the GalleryItem is highlighted when the item is selected.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryItemSelectionDisplayMode SelectionDisplayMode
		{
			get
			{
				return (GalleryItemSelectionDisplayMode)this.GetValue(GalleryItemSettings.SelectionDisplayModeProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.SelectionDisplayModeProperty, value);
			}
		}

				#endregion //SelectionDisplayMode

				#region TextPlacement

		/// <summary>
		/// Identifies the <see cref="TextPlacement"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextPlacementProperty = DependencyProperty.Register("TextPlacement",
			typeof(TextPlacement), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(TextPlacement.Default));

		/// <summary>
		/// Returns/sets a value that determines the placement of a <see cref="GalleryItem"/>'s text with respect to its image.
		/// </summary>
		/// <seealso cref="TextPlacementProperty"/>
		/// <seealso cref="TextDisplayMode"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Image"/>
		/// <seealso cref="GalleryItem.Text"/>
		//[Description("Returns/sets a value that determines the placement of a GalleryItem's text with respect to its image.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public TextPlacement TextPlacement
		{
			get
			{
				return (TextPlacement)this.GetValue(GalleryItemSettings.TextPlacementProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.TextPlacementProperty, value);
			}
		}

				#endregion //TextPlacement

				#region TextDisplayMode

		/// <summary>
		/// Identifies the <see cref="TextDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextDisplayModeProperty = DependencyProperty.Register("TextDisplayMode",
			typeof(GalleryItemTextDisplayMode), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(GalleryItemTextDisplayMode.Default));

		/// <summary>
		/// Returns/sets a value that determines when the GalleryItem's text is displayed.
		/// </summary>
		/// <seealso cref="TextDisplayModeProperty"/>
		/// <seealso cref="TextPlacement"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItem.Text"/>
		//[Description("Returns/sets a value that determines when the GalleryItem's text is displayed.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryItemTextDisplayMode TextDisplayMode
		{
			get
			{
				return (GalleryItemTextDisplayMode)this.GetValue(GalleryItemSettings.TextDisplayModeProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.TextDisplayModeProperty, value);
			}
		}

				#endregion //TextDisplayMode

				#region VerticalTextAlignment

		/// <summary>
		/// Identifies the <see cref="VerticalTextAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalTextAlignmentProperty = DependencyProperty.Register("VerticalTextAlignment",
			typeof(VerticalAlignment?), typeof(GalleryItemSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the vertical alignment of the <see cref="GalleryItem"/> text.
		/// </summary>
		/// <seealso cref="VerticalTextAlignmentProperty"/>
		/// <seealso cref="HorizontalTextAlignment"/>
		/// <seealso cref="GalleryItem.Text"/>
		//[Description("Returns/sets the vertical alignment of the GalleryItem text.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<VerticalAlignment>))] // AS 5/15/08 BR32816
		public VerticalAlignment? VerticalTextAlignment
		{
			get
			{
				return (VerticalAlignment?)this.GetValue(GalleryItemSettings.VerticalTextAlignmentProperty);
			}
			set
			{
				this.SetValue(GalleryItemSettings.VerticalTextAlignmentProperty, value);
			}
		}

				#endregion //VerticalTextAlignment

			#endregion //Public Properties

		#endregion //Properties
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