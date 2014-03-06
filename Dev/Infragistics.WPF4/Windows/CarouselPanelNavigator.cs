using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Control that provides a UI for scrolling items in a <see cref="XamCarouselPanel"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">The CarouselPanelNavigator displays buttons that execute <see cref="XamCarouselPanelCommands"/> when clicked, in order to scroll items.  
	/// If you want to provide your own UI for scrollig items, you can hide the CarouselPanelNavigator by setting the CarouselViewSettings' <see cref="CarouselViewSettings.IsNavigatorVisible"/> 
    /// property to hidden and then creating a UI that executes the same scrolling commands used by the navigator.</p>
    /// <p class="body">The <see cref="CarouselViewSettings"/> object exposes a <see cref="CarouselViewSettings.CarouselPanelNavigatorStyle"/> property that can be used to style the control.  
    /// Alternatively, you can create a style that targets the CarouselPanelNavigator and place it in a resource dictionary somewhere in the ancestor chain of the control.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
    /// <p class="note"><b>Note: </b>The <see cref="CarouselViewSettings"/> object is available via the <see cref="XamCarouselPanel"/>'s <see cref="XamCarouselPanel.ViewSettings"/> and <see cref="XamCarouselListBox"/>'s <see cref="XamCarouselListBox.ViewSettings"/> properties.</p>
    /// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
	/// </remarks>
    /// <seealso cref="XamCarouselPanelCommands"/>
	/// <seealso cref="XamCarouselPanelCommands.NavigateToPreviousPage"/>
	/// <seealso cref="XamCarouselPanelCommands.NavigateToNextPage"/>
	/// <seealso cref="XamCarouselPanelCommands.NavigateToPreviousItem"/>
	/// <seealso cref="XamCarouselPanelCommands.NavigateToNextItem"/>
    /// <seealso cref="CarouselViewSettings"/>
    /// <seealso cref="CarouselViewSettings.CarouselPanelNavigatorStyle"/>
    /// <seealso cref="XamCarouselPanel"/>
	//[Description("Control used by the XamCarouselPanel to provide a UI for navigating (scrolling) among the child elements of the panel.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CarouselPanelNavigator : Control
	{
		#region Member Variables

		#endregion //Member Variables	
    
		#region Constructors

		static CarouselPanelNavigator()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselPanelNavigator), new FrameworkPropertyMetadata(typeof(CarouselPanelNavigator)));
		}

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
        /// </remarks>
        public CarouselPanelNavigator()
		{
		}

		/// <summary>
		/// Constructor used by the <see cref="XamCarouselPanel"/> to create an instance of the CarouselPanelNavigator that works with the <see cref="XamCarouselPanel"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>Instances of this class are automatically created by the <see cref="XamCarouselPanel"/>.  You do not ordinarily need to create and instance of this class directly.</p>
		/// </remarks>
		/// <param name="carouselPanel">The associated <see cref="XamCarouselPanel"/></param>
		public CarouselPanelNavigator(XamCarouselPanel carouselPanel)
		{
			this.CarouselPanel = carouselPanel;
		}

		#endregion //Constructors	

		#region Properties

			#region Public Properties

				#region CarouselPanel

		private static readonly DependencyProperty CarouselPanelProperty =
	DependencyProperty.Register("CarouselPanel",
	typeof(XamCarouselPanel), typeof(CarouselPanelNavigator), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the associated <see cref="XamCarouselPanel"/> that will be affected by the navigator.
		/// </summary>
		/// <seealso cref="XamCarouselPanel"/>
		//[Description("The 'XamCarouselPanel' that the navigator should affect.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public XamCarouselPanel CarouselPanel
		{
			get
			{
				return (XamCarouselPanel)this.GetValue(CarouselPanelNavigator.CarouselPanelProperty);
			}
			set
			{
				this.SetValue(CarouselPanelNavigator.CarouselPanelProperty, value);
			}
		} 
				#endregion //CarouselPanel

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