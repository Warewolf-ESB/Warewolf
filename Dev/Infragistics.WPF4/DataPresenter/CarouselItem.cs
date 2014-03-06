using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
    /// <see cref="Infragistics.Windows.Controls.CarouselListBoxItem"/> derived class used by the <see cref="XamDataCarousel"/> (and <see cref="XamDataPresenter"/>'s <see cref="CarouselView"/>) to serve as a container (wrapper) for each item in the list.
	/// </summary>
    /// <remarks>
    /// <p class="body">A <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/> is used within a <see cref="XamDataCarousel"/>'s (and <see cref="XamDataPresenter"/>'s <see cref="CarouselView"/>) to arrange items in the list. 
    /// The <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/> wraps each of its child items in a CarouselItem element.  The wrapper serves as a convenient
    /// place to store state (required by the <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/>) for each of its child items.  You will not normally need to interact with this element but you should
    /// be aware of its existence in case you have code that needs to traverse the control's visual tree.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamDataCarousel"/> (or <see cref="XamDataPresenter"/>'s <see cref="CarouselView"/> 
    /// when needed.  You do not ordinarily need to create an instance of this class directly.</p>
    /// </remarks>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="XamDataPresenter.View"/>
    /// <seealso cref="CarouselView"/>
    //[Description("CarouselListBixItem derived class used by the XamDataCarousel (and XamDataPresenter's CarouselView) to serve as a container (wrapper) for each item in the list.")]
	// JJD 5/22/07
	// Implemented IRecordPresenterContainer interface
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public sealed class CarouselItem : CarouselListBoxItem,
									   ISelectableElement,
										IRecordPresenterContainer
	{
		#region Member Variables

		private RecordPresenter						_recordPresenter = null;

		#endregion //Member Variables

		#region Constructor

		static CarouselItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselItem), new FrameworkPropertyMetadata(typeof(CarouselItem)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(CarouselItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
        /// <remarks>
        /// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamDataCarousel"/> (or <see cref="XamDataPresenter"/>'s <see cref="CarouselView"/> when needed.  
        /// You do not ordinarily need to create an instance of this class directly.</p>
        /// </remarks>
        /// <seealso cref="XamDataCarousel"/>
        /// <seealso cref="XamDataPresenter"/>
        /// <seealso cref="XamDataPresenter.View"/>
        /// <seealso cref="CarouselView"/>
        public CarouselItem()
		{
		}

		internal CarouselItem(XamCarouselPanel carouselPanel) : base(carouselPanel)
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

            // AS 3/11/09 Optimization
			//switch (e.Property.Name)
			//{
			//	case "Content":
            if (e.Property == ContentProperty)
            {
					if (e.OldValue is RecordPresenter)
					    BindingOperations.ClearBinding(this, CarouselItem.RecordProperty);

					this._recordPresenter = e.NewValue as RecordPresenter;
					if (this.RecordPresenter != null)
						this.SetBinding(CarouselItem.RecordProperty, Utilities.CreateBindingObject(RecordPresenter.RecordProperty, BindingMode.OneWay, this._recordPresenter));

					//break;
			}
		}

			#endregion //OnPropertyChanged

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region DataPresenterBase

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
		/// </summary>
		public DataPresenterBase DataPresenter
		{
			get
			{
				Record rcd = this.Record;
				if (rcd != null)
					return rcd.DataPresenter;

				return null;
			}
		}

				#endregion //DataPresenterBase

				#region ExpansionIndicatorVisibility

		private static readonly DependencyPropertyKey ExpansionIndicatorVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ExpansionIndicatorVisibility",
			typeof(Visibility), typeof(CarouselItem), new FrameworkPropertyMetadata(KnownBoxes.VisibilityHiddenBox));

		/// <summary>
		/// Identifies the <see cref="ExpansionIndicatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorVisibilityProperty =
			ExpansionIndicatorVisibilityPropertyKey.DependencyProperty;

		/// <summary>
        /// Returns <see cref="System.Windows.Visibility"/>.Visible if the record associated with this CarouselItem has child records, otherwise returns <see cref="System.Windows.Visibility"/>.Hidden.
		/// </summary>
		/// <seealso cref="ExpansionIndicatorVisibilityProperty"/>
		//[Description("Returns Visbility.Visible if the record associated with this CarouselItem has child records to display, otherwise returns Visibility.Hidden.")]
		//[Category("Behavior")]
		public Visibility ExpansionIndicatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CarouselItem.ExpansionIndicatorVisibilityProperty);
			}
		}

				#endregion //ExpansionIndicatorVisibility

				#region IsExpanded

		/// <summary>
		/// Identifies the 'IsExpanded' dependency property
		/// </summary>
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
				typeof(bool), typeof(CarouselItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, null, new CoerceValueCallback(CoerceIsExpanded)));

		private static object CoerceIsExpanded(DependencyObject target, object value)
		{
			if ((bool)value == true)
			{
				CarouselItem carouselItem = target as CarouselItem;
				if (carouselItem != null && carouselItem.CarouselViewPanel != null)
				{
					// [JM 04-18-07 BR21391]
					//carouselItem.OnExpanded();
					if (carouselItem.Record != null)
						carouselItem.Record.IsExpanded = (bool)value;
				}
			}

			return value;
		}

		/// <summary>
		/// Determines if the wrapper is expanded
		/// </summary>
		//[Description("Determines if the CarouselItem is expanded")]
		//[Category("Behavior")]
		public bool IsExpanded
		{
			get
			{
				return (bool)this.GetValue(CarouselItem.IsExpandedProperty);
			}
			set
			{
				this.SetValue(CarouselItem.IsExpandedProperty, KnownBoxes.FromValue(value));
			}
		}

				#endregion //IsExpanded

				#region ItemDisappearingStoryboard

		/// <summary>
		/// Identifies the <see cref="ItemDisappearingStoryboard"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemDisappearingStoryboardProperty = DependencyProperty.Register("ItemDisappearingStoryboard",
			typeof(Storyboard), typeof(CarouselItem), new FrameworkPropertyMetadata((Storyboard)null));

		/// <summary>
		/// Returns/sets the storyboard used to animate items before they disappear.
		/// </summary>
        /// <remarks>
        /// <p class="body">The storyboard is run when the user clicks on the expansion indicator to drill down into the child records of the <see cref="DataRecord"/>
        /// associated with the CarouselItem.  The storyboard can be used to animate the disappearance of CarouselItems in the current list before the list of
        /// child records is shown.</p>
        /// <p class="note"><b>Note: </b>If this property is null a default storyboard is provided which animates the Opacity of each item to 0 and size of each item
        /// to twice its normal size.</p>
        /// </remarks>
        /// <seealso cref="ItemDisappearingStoryboardProperty"/>
		//[Description("Returns/sets the storyboard used to animate items before they disappear.")]
		//[Category("Behavior")]
		[Bindable(true)]
        public Storyboard ItemDisappearingStoryboard
		{
			get
			{
				return (Storyboard)this.GetValue(CarouselItem.ItemDisappearingStoryboardProperty);
			}
			set
			{
				this.SetValue(CarouselItem.ItemDisappearingStoryboardProperty, value);
			}
		}

				#endregion //ItemDisappearingStoryboard

			#endregion //Public Properties

			#region Internal Properties

				#region ExpansionIndicatorVisibilityInternal

		private static readonly DependencyProperty ExpansionIndicatorVisibilityInternalProperty = DependencyProperty.Register("ExpansionIndicatorVisibilityInternal",
			typeof(Visibility), typeof(CarouselItem), new FrameworkPropertyMetadata(ExpansionIndicatorVisibilityProperty.DefaultMetadata.DefaultValue, new PropertyChangedCallback(OnExpansionIndicatorVisibilityInternalChanged)));

		private static void OnExpansionIndicatorVisibilityInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselItem carouselItem = d as CarouselItem;
			if (carouselItem != null)
				carouselItem.SetValue(ExpansionIndicatorVisibilityPropertyKey, (Visibility)e.NewValue);
		}

		private Visibility ExpansionIndicatorVisibilityInternal
		{
			get
			{
				return (Visibility)this.GetValue(CarouselItem.ExpansionIndicatorVisibilityInternalProperty);
			}
			set
			{
				this.SetValue(CarouselItem.ExpansionIndicatorVisibilityInternalProperty, KnownBoxes.FromValue(value));
			}
		}

				#endregion //ExpansionIndicatorVisibilityInternal

				#region CarouselViewPanel

		internal CarouselViewPanel CarouselViewPanel
		{
			get { return Utilities.GetAncestorFromType(this, typeof(CarouselViewPanel), true) as CarouselViewPanel; }
		}

				#endregion //CarouselViewPanel

				// [JM 04-18-07 BR21391]
				#region IsRecordExpanded

		private static readonly DependencyProperty IsRecordExpandedProperty = DependencyProperty.Register("IsRecordExpanded",
			typeof(bool), typeof(CarouselItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsRecordExpandedChanged)));

		private static void OnIsRecordExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselItem carouselItem = d as CarouselItem;
			if (carouselItem												!= null  &&  
				(bool)e.NewValue											== true &&
				carouselItem.RecordPresenter								!= null  &&
				carouselItem.RecordPresenter.IsPreparingItemForContainer	== false)
				carouselItem.OnExpanded();
		}

				#endregion //IsRecordExpanded

				#region Record

		internal static readonly DependencyProperty RecordProperty =
			DependencyProperty.Register("Record",
			typeof(Record), typeof(CarouselItem), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRecordPropertyChanged)));

		private static void OnRecordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselItem carouselItem = d as CarouselItem;
			if (carouselItem != null)
			{
				// Bind the record's ExpansionIndicatorVisibility to our internal property
				Binding binding = new Binding();
				binding.Path	= new PropertyPath("ExpansionIndicatorVisibility");
				binding.Mode	= BindingMode.OneWay;
				binding.Source	= e.NewValue as Record;
				carouselItem.SetBinding(CarouselItem.ExpansionIndicatorVisibilityInternalProperty, binding);

				// [JM 04-18-07 BR21391]
				// Bind the record's ExpansionIndicatorVisibility to our internal property
				binding			= new Binding();
				binding.Path	= new PropertyPath("IsExpanded");
				binding.Mode	= BindingMode.OneWay;
				binding.Source	= e.NewValue as Record;
				carouselItem.SetBinding(CarouselItem.IsRecordExpandedProperty, binding);
			}
		}

		internal Record Record
		{
			get
			{
				return (Record)this.GetValue(CarouselItem.RecordProperty);
			}
			set
			{
				this.SetValue(CarouselItem.RecordProperty, value);
			}
		}

				#endregion //Record

				#region RecordPresenter

		internal RecordPresenter RecordPresenter
		{
			get { return this._recordPresenter; }
		}

				#endregion //RecordPresenter

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnExpanded

		private void OnExpanded()
		{
			// [JM 04-18-07 BR21391] Check for nulls.
			if (this.CarouselViewPanel != null  &&  this.Record != null)
				this.CarouselViewPanel.OnRecordExpanded(this.Record);
		}

				#endregion //OnExpanded	
    
			#endregion //Private Methods

		#endregion //Methods

		#region ISelectableElement Members

		ISelectableItem ISelectableElement.SelectableItem
		{
			get
			{
				if (this.Content is RecordPresenter)
					return ((ISelectableElement)this.Content).SelectableItem;
				else
					return null;
			}
		}

		#endregion

		// JJD 5/22/07
		// Implemented IRecordPresenterContainer interface
		#region IRecordPresenterContainer Members

		RecordPresenter IRecordPresenterContainer.RecordPresenter
		{
			get { return this.Content as RecordPresenter; }
		}

		#endregion
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