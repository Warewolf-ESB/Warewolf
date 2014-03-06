using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// <see cref="CarouselPanelItem"/> derived class used as a container for items in a <see cref="XamCarouselListBox"/>.
	/// </summary>
	/// <remarks>
    /// <p class="body">When a <see cref="XamCarouselPanel"/> is used within a <see cref="XamCarouselListBox"/> to arrange items in the list, the <see cref="XamCarouselPanel"/> wraps each of its child items in a CarouselListBoxItem element.  The wrapper serves as a convenient
	/// place to store state (required by the <see cref="XamCarouselPanel"/>) for each of its child items.  You will not normally need to interact with this element but you should
	/// be aware of its existence in case you have code that needs to traverse the <see cref="XamCarouselPanel"/>'s parent tree.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselListBox"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselListBox"/>
	//[Description("Element used by the XamCarouselListBox to serve as a container (wrapper) for each item in the list")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CarouselListBoxItem : CarouselPanelItem
	{
		#region Constructor

		static CarouselListBoxItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselListBoxItem), new FrameworkPropertyMetadata(typeof(CarouselListBoxItem)));
		}

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselListBox"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselListBox"/>
		public CarouselListBoxItem()
		{
		}

		/// <summary>
		/// Creates an instance of a CarouselListBoxItem.
		/// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>This constructor is for Infragistics internal use only.</p>
        /// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselListBox"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
        /// </remarks>
        /// <seealso cref="XamCarouselListBox"/>
        /// <seealso cref="XamCarouselPanel"/>
        /// <param name="carouselPanel">The <see cref="XamCarouselPanel"/> that will contain the CarouselListBoxItem.</param>
		internal protected CarouselListBoxItem(XamCarouselPanel carouselPanel) : base(carouselPanel)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			//[JM 06-06-07 Since we are no longer derived from selector we need to process this message.
			#region OnMouseLeftButtonDown

		/// <summary>
		/// Fired when the left mouse button is pressed.
		/// </summary>
		/// <param name="e">An instance of MouseButtonEventArgs that contains information about the mouse state.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!e.Handled)
			{
				XamCarouselListBox xamCarouselListBox = ItemsControl.ItemsControlFromItemContainer(this) as XamCarouselListBox;
				if (xamCarouselListBox != null)
				{
					this.Focus();

					xamCarouselListBox.SelectedIndex = xamCarouselListBox.Items.IndexOf(this.Content);

					// If the developer added CarouselPanelItems directly to the list, then 'this' not 'this.Content'
					// will be in the list.
					if (xamCarouselListBox.SelectedIndex == -1)
						xamCarouselListBox.SelectedIndex = xamCarouselListBox.Items.IndexOf(this);

					e.Handled = true;
				}
				// [JM 06-14-07]
				//else
				//	base.OnMouseLeftButtonDown(e);
			}
		}

			#endregion //OnMouseLeftButtonDown

		#endregion //Base Class Overrides	
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