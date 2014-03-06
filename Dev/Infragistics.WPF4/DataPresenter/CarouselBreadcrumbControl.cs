using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;

using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
    /// A control used by the <see cref="XamDataCarousel"/> and <see cref="XamDataPresenter"/> to display a collection of <see cref="CarouselBreadcrumb"/> objects that represent the trail of parent DataRecords during hierarchy navigation.
	/// </summary>
    /// <remarks>
    /// <p class="body">The CarouselBreadcrumbControl maintains a collection of <see cref="CarouselBreadcrumb"/>s that represent a trail of visited parent
    /// <see cref="DataRecord"/>s in a <see cref="XamDataCarousel"/> control that is displaying hierarchical data.  Unlike the <see cref="XamDataGrid"/>, the <see cref="XamDataCarousel"/> cannot display multiple levels of hierarchical
    /// data concurrently.  When it is bound to hierarchical data the <see cref="XamDataCarousel"/> displays a single flat list of the current child <see cref="DataRecord"/>s.  This list changes as the user drills into the 
    /// data.</p>
    /// <p class="body">To provide a visual representation of the parent context for the current list of child records and to allow the user to navigate back up the parent chain, 
    /// the <see cref="XamDataCarousel"/> creates and displays an instance of a CarouselBreadcrumbControl in its adorner layer.  The CarouselBreadcrumbControl shows the trail of parent <see cref="DataRecord"/>s that led to the
    /// currently displayed list of child records.</p>
    /// <p class="body">The parent <see cref="DataRecord"/>s are represented by a list of one or more <see cref="CarouselBreadcrumb"/>s.  Clicking on a <see cref="CarouselBreadcrumb"/> navigates back up the parent 
    /// chain and displays the list of <see cref="DataRecord"/>s that contains the parent <see cref="DataRecord"/> associated with the <see cref="CarouselBreadcrumb"/> that was clicked.</p>
    /// <p class="note"><b>Note: </b>The functionality described above for <see cref="XamDataCarousel"/> also applies to the <see cref="XamDataPresenter"/> when it is using <see cref="CarouselView"/> 
    /// (i.e., when its <see cref="XamDataPresenter.View"/> property is set to an instance of a <see cref="CarouselView"/>)</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the CarouselBreadcrumbControl when needed.  You do not ordinarily need to create an instance of this class directly.</p>
    /// </remarks>
    /// <seealso cref="CarouselBreadcrumb"/>
    /// <seealso cref="CarouselBreadcrumbControl"/>
    /// <seealso cref="CarouselBreadcrumbControl.Breadcrumbs"/>
    /// <seealso cref="CarouselBreadcrumbCollection"/>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="DataRecord"/>
    /// <seealso cref="CarouselView"/>
    //[Description("A control used by the XamDataCarousel and XamDataPresenter to display a collection of CarouselBreadcrumb objects that represent the trail of parent DataRecords during hierarchy navigation.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CarouselBreadcrumbControl : Control
	{
		#region Member Variables

		#endregion //Member Variables	
    
		#region Constructor

		static CarouselBreadcrumbControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselBreadcrumbControl), new FrameworkPropertyMetadata(typeof(CarouselBreadcrumbControl)));
		}

        /// <summary>
        /// Constructor provided to allow creation in design tools for template and style editing.
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamDataCarousel"/> and <see cref="XamDataPresenter"/> controls when needed.  You do not ordinarily need to create an instance of this class directly.</p>
        /// </remarks>
        public CarouselBreadcrumbControl()
		{
			// Set the Breadcrumbs read-only property to an instance of the CarouselBreadcrumbCollection class.
			this.SetValue(CarouselBreadcrumbControl.BreadcrumbsPropertyKey, new CarouselBreadcrumbCollection(new ObservableCollection<CarouselBreadcrumb>(), this));


			// Listen to the CollectionChanged event on the CarouselBreadcrumbCollection class.
			((INotifyCollectionChanged)this.Breadcrumbs).CollectionChanged += new NotifyCollectionChangedEventHandler(OnBreadcrumbCollectionChanged);
		}

		#endregion //Constructor	
    
		#region Properties

			#region Public Properties

				#region Breadcrumbs

		private static readonly DependencyPropertyKey BreadcrumbsPropertyKey =
			DependencyProperty.RegisterReadOnly("Breadcrumbs",
			typeof(CarouselBreadcrumbCollection), typeof(CarouselBreadcrumbControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Breadcrumbs"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BreadcrumbsProperty =
			BreadcrumbsPropertyKey.DependencyProperty;

		/// <summary>
        /// Returns a read only collection of <see cref="CarouselBreadcrumb"/>s associated with this CarouselBreadcrumbControl.  (read only)
		/// </summary>
        /// <remarks>
        /// <p class="body">Each <see cref="CarouselBreadcrumb"/> in the collection represents a visited parent <see cref="DataRecord"/>.</p>
        /// <p class="body">The CarouselBreadcrumbControl maintains a collection of <see cref="CarouselBreadcrumb"/>s that represent a trail of visited parent
        /// <see cref="DataRecord"/>s in a <see cref="XamDataCarousel"/> control that is displaying hierarchical data.  Unlike the <see cref="XamDataGrid"/>, the <see cref="XamDataCarousel"/> cannot display multiple levels of hierarchical
        /// data concurrently.  When it is bound to hierarchical data the <see cref="XamDataCarousel"/> displays a single flat list of the current child <see cref="DataRecord"/>s.  This list changes as the user drills into the 
        /// data.</p>
        /// <p class="body">To provide a visual representation of the parent context for the current list of child records and to allow the user to navigate back up the parent chain, 
        /// the <see cref="XamDataCarousel"/> creates and displays an instance of a CarouselBreadcrumbControl in its adorner layer.  The CarouselBreadcrumbControl shows the trail of parent <see cref="DataRecord"/>s that led to the
        /// currently displayed list of child records.</p>
        /// <p class="body">The parent <see cref="DataRecord"/>s are represented by a list of one or more <see cref="CarouselBreadcrumb"/>s.  Clicking on a <see cref="CarouselBreadcrumb"/> navigates back up the parent 
        /// chain and displays the list of <see cref="DataRecord"/>s that contains the parent <see cref="DataRecord"/> associated with the <see cref="CarouselBreadcrumb"/> that was clicked.</p>
        /// <p class="note"><b>Note: </b>The functionality described above for <see cref="XamDataCarousel"/> also applies to the <see cref="XamDataPresenter"/> when it is using <see cref="CarouselView"/> 
        /// (i.e., when its <see cref="XamDataPresenter.View"/> property is set to an instance of a <see cref="CarouselView"/>)</p>
        /// </remarks>
        /// <seealso cref="BreadcrumbsProperty"/>
        //[Description("Returns a read only collection of CarouselBreadcrumbs associated with this CarouselBreadcrumbControl.  (read only)")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
        public CarouselBreadcrumbCollection Breadcrumbs
		{
			get
			{
				return (CarouselBreadcrumbCollection)this.GetValue(CarouselBreadcrumbControl.BreadcrumbsProperty);
			}
		}

				#endregion //Breadcrumbs

				#region HasCrumbs

		private static readonly DependencyPropertyKey HasCrumbsPropertyKey =
			DependencyProperty.RegisterReadOnly("HasCrumbs",
			typeof(bool), typeof(CarouselBreadcrumbControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasCrumbs"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasCrumbsProperty =
			HasCrumbsPropertyKey.DependencyProperty;

		/// <summary>
        /// Returns true if the <see cref="CarouselBreadcrumbControl"/> contains at least 1 <see cref="CarouselBreadcrumb"/>.  (read only)
		/// </summary>
		/// <seealso cref="HasCrumbsProperty"/>
        //[Description("Returns true if the CarouselBreadcrumbControl contains at least 1 CarouselBreadcrumb.  (read only)")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool HasCrumbs
		{
			get
			{
				return (bool)this.GetValue(CarouselBreadcrumbControl.HasCrumbsProperty);
			}
		}

				#endregion //HasCrumbs

				#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(CarouselBreadcrumbControl), new FrameworkPropertyMetadata(Orientation.Horizontal));

		/// <summary>
        /// Returns/sets the orientation of the <see cref="CarouselBreadcrumb"/>s in the list.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns/sets the orientation of the CarouselBreadcrumbs in the list.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(CarouselBreadcrumbControl.OrientationProperty);
			}
			set
			{
				this.SetValue(CarouselBreadcrumbControl.OrientationProperty, value);
			}
		}

				#endregion //Orientation

			#endregion //Public Properties	
    	    
		#endregion Properties

		#region Methods

			#region Public Methods

				#region PopBreadcrumb

		/// <summary>
		/// Removes and returns the <see cref="CarouselBreadcrumb"/> at end of the collection exposed by the <see cref="CarouselBreadcrumbControl.Breadcrumbs"/> property.
		/// </summary>
		/// <returns>The CarouselBreadcrumb that was removed from the end of the collection, or null if no such CarouselBreadcrumb existed.</returns>
		/// <seealso cref="CarouselBreadcrumb"/>
		/// <seealso cref="CarouselBreadcrumbCollection"/>
        /// <seealso cref="CarouselBreadcrumbControl.Breadcrumbs"/>
        public CarouselBreadcrumb PopBreadcrumb()
		{
			CarouselBreadcrumb breadcrumb = this.Breadcrumbs.Pop();

			// If there is a CarouselBreadcrumb remaining, set its IsLastBreadcrumb property to true.
			if (this.Breadcrumbs.Count > 0)
				this.Breadcrumbs[this.Breadcrumbs.Count - 1].SetValue(CarouselBreadcrumb.IsLastBreadcrumbPropertyKey, KnownBoxes.TrueBox);

			return breadcrumb;
		}

				#endregion //PopBreadcrumb

				#region PopBreadcrumbsUpTo

		/// <summary>
        /// Removes all <see cref="CarouselBreadcrumb"/>s from the end of the collection exposed by the <see cref="CarouselBreadcrumbControl.Breadcrumbs"/> 
        /// property up to but not including the specified <see cref="CarouselBreadcrumb"/>.
		/// </summary>
        /// <returns>The number of <see cref="CarouselBreadcrumb"/>s removed.</returns>
		/// <seealso cref="CarouselBreadcrumb"/>
		/// <seealso cref="CarouselBreadcrumbCollection"/>
        /// <seealso cref="CarouselBreadcrumbControl.Breadcrumbs"/>
        public int PopBreadcrumbsUpTo(CarouselBreadcrumb breadcrumb)
		{
			CarouselBreadcrumb	nextBreadcrumb		= this.Breadcrumbs.Peek();
			int			totalCrumbsPopped	= 0;

			while (nextBreadcrumb != breadcrumb)
			{
				this.PopBreadcrumb();
				totalCrumbsPopped++;

				nextBreadcrumb = this.Breadcrumbs.Peek();
			}

			return totalCrumbsPopped;
		}

				#endregion //PopBreadcrumbsUpTo

				#region PushBreadcrumb

		/// <summary>
        /// Adds a <see cref="CarouselBreadcrumb"/> at the end of the collection exposed by the <see cref="CarouselBreadcrumbControl.Breadcrumbs"/> property. 
		/// </summary>
		/// <param name="breadcrumb">The CarouselBreadcrumb to add to the collection</param>
		/// <seealso cref="CarouselBreadcrumb"/>
		/// <seealso cref="CarouselBreadcrumbCollection"/>
        /// <seealso cref="CarouselBreadcrumbControl.Breadcrumbs"/>
        public void PushBreadcrumb(CarouselBreadcrumb breadcrumb)
		{
			if (breadcrumb == null)
				throw new ArgumentNullException( "breadcrumb", DataPresenterBase.GetString( "LE_ArgumentNullException_1" ) );

			breadcrumb.ParentCollection = this.Breadcrumbs;
			this.Breadcrumbs.Push(breadcrumb);

			// If there is a previous CarouselBreadcrumb, set its IsLastBreadcrumb property to false.
			if (this.Breadcrumbs.Count > 1)
				this.Breadcrumbs[this.Breadcrumbs.Count - 2].SetValue(CarouselBreadcrumb.IsLastBreadcrumbPropertyKey, KnownBoxes.FalseBox);
		}

				#endregion //PushBreadcrumb

			#endregion //Public Methods	

			#region Private Methods

				#region OnBreadcrumbCollectionChanged

		private void OnBreadcrumbCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.Breadcrumbs.Count < 1)
			{
				if (this.HasCrumbs == true)
					this.SetValue(CarouselBreadcrumbControl.HasCrumbsPropertyKey, KnownBoxes.FalseBox);
			}
			else
			{
				if (this.HasCrumbs == false)
					this.SetValue(CarouselBreadcrumbControl.HasCrumbsPropertyKey, KnownBoxes.TrueBox);
			}
		}

				#endregion //OnBreadcrumbCollectionChanged	
    		
			#endregion //Private Methods

		#endregion //Methods

		#region Events

			#region CarouselBreadcrumbClick

		/// <summary>
		/// Event ID for the <see cref="CarouselBreadcrumbClick"/> routed event
		/// </summary>
		/// <seealso cref="CarouselBreadcrumbClick"/>
		/// <seealso cref="OnCarouselBreadcrumbClick"/>
		/// <seealso cref="CarouselBreadcrumbClickEventArgs"/>
		public static readonly RoutedEvent CarouselBreadcrumbClickEvent =
			EventManager.RegisterRoutedEvent("CarouselBreadcrumbClick", RoutingStrategy.Bubble, typeof(EventHandler<CarouselBreadcrumbClickEventArgs>), typeof(CarouselBreadcrumbControl));

		/// <summary>
		/// Occurs when a CarouselBreadcrumb is clicked.
		/// </summary>
		/// <seealso cref="CarouselBreadcrumbClick"/>
		/// <seealso cref="CarouselBreadcrumbClickEvent"/>
		/// <seealso cref="CarouselBreadcrumbClickEventArgs"/>
		protected virtual void OnCarouselBreadcrumbClick(CarouselBreadcrumbClickEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseCarouselBreadcrumbClick(CarouselBreadcrumbClickEventArgs args)
		{
			args.RoutedEvent	= CarouselBreadcrumbControl.CarouselBreadcrumbClickEvent;
			args.Source			= this;
			this.OnCarouselBreadcrumbClick(args);
		}

		/// <summary>
		/// Occurs when a CarouselBreadcrumb is clicked.
		/// </summary>
		/// <seealso cref="OnCarouselBreadcrumbClick"/>
		/// <seealso cref="CarouselBreadcrumbClickEvent"/>
		/// <seealso cref="CarouselBreadcrumbClickEventArgs"/>
		//[Description("Occurs when a CarouselBreadcrumb is clicked.")]
		//[Category("Behavior")]
		public event EventHandler<CarouselBreadcrumbClickEventArgs> CarouselBreadcrumbClick
		{
			add
			{
				base.AddHandler(CarouselBreadcrumbControl.CarouselBreadcrumbClickEvent, value);
			}
			remove
			{
				base.RemoveHandler(CarouselBreadcrumbControl.CarouselBreadcrumbClickEvent, value);
			}
		}

			#endregion //CarouselBreadcrumbClick

		#endregion //Events
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