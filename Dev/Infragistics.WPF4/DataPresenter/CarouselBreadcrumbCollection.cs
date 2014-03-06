using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A read only collection of <see cref="CarouselBreadcrumb"/>s contained in a <see cref="CarouselBreadcrumbControl"/>.
	/// </summary>
    /// <remarks>
    /// <p class="body">The <see cref="CarouselBreadcrumbControl"/> maintains a collection of CarouselBreadcrumbs that represent a trail of visited parent
    /// <see cref="DataRecord"/>s in a <see cref="XamDataCarousel"/> control that is displaying hierarchical data.  Unlike the <see cref="XamDataGrid"/>, the <see cref="XamDataCarousel"/> cannot display multiple levels of hierarchical
    /// data concurrently.  When it is bound to hierarchical data the <see cref="XamDataCarousel"/> displays a single flat list of the current child <see cref="DataRecord"/>s.  This list changes as the user drills into the 
    /// data.</p>
    /// <p class="body">To provide a visual representation of the parent context for the current list of child records and to allow the user to navigate back up the parent chain, 
    /// the <see cref="XamDataCarousel"/> creates and displays an instance of a <see cref="CarouselBreadcrumbControl"/> in its adorner layer.  The <see cref="CarouselBreadcrumbControl"/> shows the trail of parent <see cref="DataRecord"/>s that led to the
    /// currently displayed list of child records.</p>
    /// <p class="body">The parent <see cref="DataRecord"/>s are represented by a list of one or more CarouselBreadcrumbs.  Clicking on a CarouselBreadcrumb navigates back up the parent 
    /// chain and displays the list of <see cref="DataRecord"/>s that contains the parent <see cref="DataRecord"/> associated with the CarouselBreadcrumb that was clicked.</p>
    /// <p class="note"><b>Note: </b>The functionality described above for <see cref="XamDataCarousel"/> also applies to the <see cref="XamDataPresenter"/> when it is using <see cref="CarouselView"/> 
    /// (i.e., when its <see cref="XamDataPresenter.View"/> property is set to an instance of a <see cref="CarouselView"/>)</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="CarouselBreadcrumbControl"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
    /// </remarks>
    /// <seealso cref="CarouselBreadcrumb"/>
    /// <seealso cref="CarouselBreadcrumbControl"/>
    /// <seealso cref="CarouselBreadcrumbControl.Breadcrumbs"/>
    /// <seealso cref="CarouselBreadcrumbCollection"/>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="DataRecord"/>
    /// <seealso cref="CarouselView"/>
    public class CarouselBreadcrumbCollection : ReadOnlyObservableCollection<CarouselBreadcrumb>
	{
		#region Member Variables

		private CarouselBreadcrumbControl						_breadcrumbControl = null;

		#endregion //Member Variables

		#region Constructor

		internal CarouselBreadcrumbCollection(ObservableCollection<CarouselBreadcrumb> list, CarouselBreadcrumbControl breadcrumbControl) : base(list)
		{
			this._breadcrumbControl = breadcrumbControl;
		}

		#endregion //Constructor	
    
		#region Properties

			#region Internal Properties

				#region BreadcrumbControl

		internal CarouselBreadcrumbControl BreadcrumbControl
		{
			get { return this._breadcrumbControl; }
		}

				#endregion //BreadcrumbControl

			#endregion //Internal Properties	
        
		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region Peek

		internal CarouselBreadcrumb Peek()
		{
			if (this.Items.Count < 1)
				return null;

			return this.Items[this.Items.Count - 1];
		}

				#endregion //Peek

				#region Pop

		internal CarouselBreadcrumb Pop()
		{
			if (this.Items.Count < 1)
				return null;

			CarouselBreadcrumb breadcrumb = this.Items[this.Items.Count - 1];
			this.Items.RemoveAt(this.Items.Count - 1);

			return breadcrumb;
		}

				#endregion //Pop

				#region Push

		internal void Push(CarouselBreadcrumb breadcrumb)
		{
			this.Items.Add(breadcrumb);
		}

				#endregion //Push

			#endregion //Internal Methods	
        
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