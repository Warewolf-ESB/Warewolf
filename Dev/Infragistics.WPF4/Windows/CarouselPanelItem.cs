using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Virtualization;
using Infragistics.Collections;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// ListBoxItem derived class used as a container for items in a <see cref="XamCarouselPanel"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">When used as a standalone panel, the <see cref="XamCarouselPanel"/> wraps each of its child items in a CarouselPanelItem element.  The wrapper serves as a convenient
	/// place to store state (required by the <see cref="XamCarouselPanel"/>) for each of its child items.  You will not normally need to interact with this element but you should
	/// be aware of its existence in case you have code that needs to traverse the <see cref="XamCarouselPanel"/>'s parent tree.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselPanel"/>
	//[Description("Element used by the XamCarouselPanel to serve as a wrapper for each child element within the panel.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CarouselPanelItem : ListBoxItem
	{
		#region Member Variables

		private double									_pathLocationPercent = double.NegativeInfinity;
		private double									_pathTargetPercent;
		private double									_velocity = 0;
		private Point									_currentLocation = new Point(0, 0);
		private double									_currentZindexEffectStopValue;
		private XamCarouselPanel						_carouselPanel;

		private MatrixTransform							_matrixTransform = null;
		private ScaleTransform							_scaleTransform = null;
		private SkewTransform							_skewTransform = null;

		private bool									_needsArrange = true;

		#endregion //Member Variables

		#region Constructor

		static CarouselPanelItem()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselPanelItem), new FrameworkPropertyMetadata(typeof(CarouselPanelItem)));
		}

		/// <summary>
		/// Constructor provided to allow creation in design tools for template and style editing.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		public CarouselPanelItem()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CarouselPanelItem"/> class
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		internal protected CarouselPanelItem(XamCarouselPanel carouselPanel)
		{
			if (carouselPanel == null)
				throw new ArgumentException( SR.GetString( "LE_ArgumentNullException_1" ) );

			this._carouselPanel = carouselPanel;

			// Bind to XamCarouselPanel's ViewSettings.Version property.
			Binding binding = null;

			binding			= new Binding();
			binding.Mode	= BindingMode.OneWay;

			// [JM 04-23-07] Bind to property on xamCarouselPanel instead of CarouselViewSettings
			//binding.Source	= carouselPanel.ViewSettings;
			binding.Source	= carouselPanel;
			//binding.Path = new PropertyPath("Version");
			binding.Path = new PropertyPath("ViewSettingsVersion");

			this.SetBinding(CarouselPanelItem.ViewSettingsVersionProperty, binding);
		}

		#endregion //Constructor

		#region Base Class Overrides

            #region LogicalChildren
        /// <summary>
        /// Gets an enumerator that can iterate the logical child elements of this element.
        /// </summary>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                // AS 3/11/09 TFS11010
                // The CarouselView sets the RecordPresenter as the content. When that 
                // happens, the base ContentControl OnContentChanged implementation makes 
                // that a logical child of this element. However, when the ContentControl's 
                // PrepareContentControl is called by the PrepareContainerForItemOverride 
                // it sets its ContentIsNotLogical which causes its LogicalChildren to 
                // not return the content because it assumes that the DataContext and 
                // Content are the same which in the case of the CarouselItem it is not.
                //
                DependencyObject d = this.Content as DependencyObject;

                if (d != null && LogicalTreeHelper.GetParent(d) == this)
                    return new SingleItemEnumerator(d);

                return base.LogicalChildren;
            }
        } 
            #endregion //LogicalChildren

			// [JM 06-06-07] Let the base class (ListBoxItem) process this.
			#region OnMouseLeftButtonDown

		///// <summary>
		///// Fired when the left mouse button is pressed.
		///// </summary>
		///// <param name="e">An instance of MouseButtonEventArgs that contains information about the mouse state.</param>
		//protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		//{
		//	  if (!e.Handled)
		//    {
		//        // [JM 05-22-07] 
		//        //Selector selector = ItemsControl.ItemsControlFromItemContainer(this) as Selector;
		//        XamCarouselListBox xamCarouselListBox = ItemsControl.ItemsControlFromItemContainer(this) as XamCarouselListBox;
		//        if (xamCarouselListBox != null)
		//        {
		//            this.Focus();

		//            // Get the index of our content in the ItemsControl's Items list and set the Selector's
		//            // SelectedIndex property.  For some reason setting the Selector's SelectedItem property directly
		//            // does not work.  Sounds like a bug in Selector.
		//            xamCarouselListBox.SelectedIndex = xamCarouselListBox.Items.IndexOf(this.Content);

		//		      // [JM 05-01-07] If the developer added CarouselPanelItems directly to the list, then 'this' not 'this.Content'
		//		      // will be in the list.
		//	          if (xamCarouselListBox.SelectedIndex == -1)
		//		  	    xamCarouselListBox.SelectedIndex = xamCarouselListBox.Items.IndexOf(this);

		//		      e.Handled = true;
		//		 }
		//	}
		//}

			#endregion //OnMouseLeftButtonDown

		#endregion //Base Class Overrides	

		#region Properties

			#region Internal Properties

				#region CarouselPanel

		internal XamCarouselPanel CarouselPanel
		{
			get { return this._carouselPanel; }
		}

				#endregion //CarouselPanel	

				#region CurrentLocation

		internal Point CurrentLocation
		{
			get { return this._currentLocation; }
			set { this._currentLocation = value; }
		}

				#endregion //CurrentLocation	
    
				#region CurrentZindexEffectStopValue

		internal double CurrentZindexEffectStopValue
		{
			get { return this._currentZindexEffectStopValue; }
			set { this._currentZindexEffectStopValue = value; }
		}

				#endregion //CurrentZindexEffectStopValue	
    
				#region IsPathLocationUnset

		internal bool IsPathLocationUnset
		{
			get { return double.IsNegativeInfinity(this._pathLocationPercent); }
		}

				#endregion //IsPathLocationUnset

				#region NeedsArrange

		internal bool NeedsArrange
		{
			get { return this._needsArrange; }
			set { this._needsArrange = value; }
		}

				#endregion //NeedsArrange

				#region PathLocationPercent

		internal double PathLocationPercent
		{
			get { return this._pathLocationPercent; }
			set { this._pathLocationPercent = value; }
		}

				#endregion //PathLocationPercent	
    
				#region PathTargetPercent

		internal double PathTargetPercent
		{
			get { return this._pathTargetPercent; }
			set { this._pathTargetPercent = value; }
		}

			#endregion //PathTargetPercent	
    
				#region Velocity

		internal double Velocity
		{
			get { return this._velocity; }
			set { this._velocity = value; }
		}

			#endregion //Velocity	

				#region ViewSettingsVersion

		internal static readonly DependencyProperty ViewSettingsVersionProperty = DependencyProperty.Register("ViewSettingsVersion",
			typeof(int), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnViewSettingsVersionChanged)));

		private static void OnViewSettingsVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CarouselPanelItem carouselPanelItem = d as CarouselPanelItem;
			if (carouselPanelItem != null)
			{
				CarouselViewSettings carouselViewSettings = carouselPanelItem.CarouselPanel.ViewSettingsInternal;

				if (carouselViewSettings != null)
				{
					carouselPanelItem.SetValue(CarouselPanelItem.AutoScaleItemContentsToFitPropertyKey, KnownBoxes.FromValue(carouselViewSettings.AutoScaleItemContentsToFit));
					carouselPanelItem.SetValue(CarouselPanelItem.IsListContinuousPropertyKey, KnownBoxes.FromValue(carouselViewSettings.IsListContinuous));
					carouselPanelItem.SetValue(CarouselPanelItem.ItemHorizontalScrollBarVisibilityPropertyKey, KnownBoxes.FromValue(carouselViewSettings.ItemHorizontalScrollBarVisibility));
					carouselPanelItem.SetValue(CarouselPanelItem.ItemVerticalScrollBarVisibilityPropertyKey, KnownBoxes.FromValue(carouselViewSettings.ItemVerticalScrollBarVisibility));
				}
			}
		}

				#endregion //ViewSettingsVersion

			#endregion //Internal Properties

			#region Public Properties

				#region AutoScaleItemContentsToFit

		/// <summary>
		/// Identifies the <see cref="AutoScaleItemContentsToFit"/> dependency property
		/// </summary>
		private static readonly DependencyPropertyKey AutoScaleItemContentsToFitPropertyKey =
			DependencyProperty.RegisterReadOnly("AutoScaleItemContentsToFit",
			typeof(bool), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="AutoScaleItemContentsToFit"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoScaleItemContentsToFitProperty =
			AutoScaleItemContentsToFitPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="XamCarouselPanel"/> that contains this item is requesting that items autoscale their content to fit within their bounds.  (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">This read only property is useful if you need to create a Style that replaces the default Template for <see cref="CarouselPanelItem"/>.  You can 
        /// query this property from within the Template to determine whether to scale the item contents.</p>
		/// </remarks>
		/// <seealso cref="AutoScaleItemContentsToFitProperty"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns true if the XamCarouselPanel that contains this item is requesting that items autoscale their content to fit within their bounds.  (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool AutoScaleItemContentsToFit
		{
			get
			{
				return (bool)this.GetValue(CarouselPanelItem.AutoScaleItemContentsToFitProperty);
			}
		}

				#endregion //AutoScaleItemContentsToFit

				#region IsListContinuous

		private static readonly DependencyPropertyKey IsListContinuousPropertyKey =
			DependencyProperty.RegisterReadOnly("IsListContinuous",
			typeof(bool), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsListContinuous"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsListContinuousProperty =
			IsListContinuousPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="XamCarouselPanel"/> that contains this item is treating the item list as a continuous list.  (read only)
		/// </summary>
		/// <remarks>
        /// <p class="body">This read only property is useful if you need to create a Style that replaces the default Template for <see cref="CarouselPanelItem"/>.  You can 
        /// query this property from within the Template to determine whether to highlight the first and last items in the list by querying the <see cref="IsFirstItem"/> and <see cref="IsLastItem"/> properties.</p>
        /// </remarks>
		/// <seealso cref="IsListContinuousProperty"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns true if the XamCarouselPanel that contains this item is treating the item list a continuous list.  (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsListContinuous
		{
			get
			{
				return (bool)this.GetValue(CarouselPanelItem.IsListContinuousProperty);
			}
		}

				#endregion //IsListContinuous

				#region IsFirstItem

		// AS 4/18/07 BR21222
		// Changed to a DependencyPropertyKey since this should be readonly.
		//
		////// <summary>
		///// Identifies the <see cref="IsFirstItem"/> dependency property
		///// </summary>
		//public static readonly DependencyProperty IsFirstItemProperty =
		//	DependencyProperty.Register("IsFirstItem",
		//	typeof(bool), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal static readonly DependencyPropertyKey IsFirstItemPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFirstItem",
			typeof(bool), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsFirstItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFirstItemProperty = IsFirstItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the item is the first item in the list.  (read only)
		/// </summary>
		/// <remarks>
        /// <p class="body">This read only property is useful if you need to create a Style that replaces the default Template for <see cref="CarouselPanelItem"/>.  You can 
        /// query the IsListContinuous property from within the Template, and then look at this 
        /// property and the <see cref="IsLastItem"/> property to determine if the item needs to be highlighted.</p>
        /// </remarks>
		/// <seealso cref="IsFirstItemProperty"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns true if the this item is the first item in the list.  (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsFirstItem
		{
			get
			{
				return (bool)this.GetValue(CarouselPanelItem.IsFirstItemProperty);
			}
			// AS 4/18/07 BR21222
			//set
			//{
			//	this.SetValue(CarouselPanelItem.IsFirstItemProperty, KnownBoxes.FromValue(value));
			//}
		}

				#endregion //IsFirstItem

				#region IsLastItem

		// AS 4/18/07 BR21222
		// Changed to a DependencyPropertyKey since this should be readonly.
		//
		///// <summary>
		///// Identifies the <see cref="IsLastItem"/> dependency property
		///// </summary>
		//public static readonly DependencyProperty IsLastItemProperty =
		//    DependencyProperty.Register("IsLastItem",
		//    typeof(bool), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal static readonly DependencyPropertyKey IsLastItemPropertyKey =
		    DependencyProperty.RegisterReadOnly("IsLastItem",
		    typeof(bool), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsLastItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLastItemProperty = IsLastItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the item is the last item in the list.  (read only)
		/// </summary>
		/// <remarks>
        /// <p class="body">This read only property is useful if you need to create a Style that replaces the default Template for <see cref="CarouselPanelItem"/>.  You can 
        /// query the IsListContinuous property from within the Template, and then look at this 
        /// property and the <see cref="IsFirstItem"/> property to determine if the item needs to be highlighted.</p>
        /// </remarks>
		/// <seealso cref="IsLastItemProperty"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns true if the this item is the last item in the list.  (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsLastItem
		{
			get
			{
				return (bool)this.GetValue(CarouselPanelItem.IsLastItemProperty);
			}
			// AS 4/18/07 BR21222
			//set
			//{
			//	this.SetValue(CarouselPanelItem.IsLastItemProperty, KnownBoxes.FromValue(value));
			//}
		}

				#endregion //IsLastItem

				#region ItemHorizontalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="ItemHorizontalScrollBarVisibility"/> dependency property
		/// </summary>
		private static readonly DependencyPropertyKey ItemHorizontalScrollBarVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ItemHorizontalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityAutoBox));

		/// <summary>
		/// Identifies the <see cref="ItemHorizontalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemHorizontalScrollBarVisibilityProperty =
			ItemHorizontalScrollBarVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the item's horizontal scrollbar as determined by the <see cref="XamCarouselPanel"/> that contains this item.  (read only)
		/// </summary>
		/// <remarks>
        /// <p class="body">This read only property is useful if you need to create a Style that replaces the default Template for <see cref="CarouselPanelItem"/>.  You can 
        /// query this property from within the Template to determine whether to display a horizontal scrollbar.</p>
        /// </remarks>
		/// <seealso cref="ItemHorizontalScrollBarVisibilityProperty"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns the visibility of the item's horizontal scrollbar as determined by the XamCarouselPanel.  (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public ScrollBarVisibility ItemHorizontalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(CarouselPanelItem.ItemHorizontalScrollBarVisibilityProperty);
			}
		}

				#endregion //ItemHorizontalScrollBarVisibility

				#region ItemVerticalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="ItemVerticalScrollBarVisibility"/> dependency property
		/// </summary>
		private static readonly DependencyPropertyKey ItemVerticalScrollBarVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ItemVerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(CarouselPanelItem), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityAutoBox));

		/// <summary>
		/// Identifies the <see cref="ItemVerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemVerticalScrollBarVisibilityProperty =
			ItemVerticalScrollBarVisibilityPropertyKey.DependencyProperty;
		
		/// <summary>
		/// Returns the visibility of the item's vertical scrollbar as determined by the <see cref="XamCarouselPanel"/> that contains this item.
		/// </summary>
		/// <remarks>
        /// <p class="body">This read only property is useful if you need to create a Style that replaces the default Template for <see cref="CarouselPanelItem"/>.  You can 
        /// query this property from within the Template to determine whether to display a vertical scrollbar.</p>
        /// </remarks>
		/// <seealso cref="ItemVerticalScrollBarVisibilityProperty"/>
		/// <seealso cref="CarouselPanelItem"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings"/>
		//[Description("Returns the visibility of the item's vertical scrollbar as determined by the XamCarouselPanel.  (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public ScrollBarVisibility ItemVerticalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(CarouselPanelItem.ItemVerticalScrollBarVisibilityProperty);
			}
		}

				#endregion //ItemVerticalScrollBarVisibility

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region ClearContainerForItem

		/// <summary>
		/// Undoes any preparation done by the container to 'host' the item.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method is called automatically by the <see cref="XamCarouselPanel"/>.  You do not ordinarily need to call this method.</p>
		/// </remarks>
		/// <param name="item">The item being hosted by the container.</param>
		/// <seealso cref="XamCarouselPanel"/>
		public virtual void ClearContainerForItem(object item)
		{
			System.Windows.Data.BindingOperations.ClearAllBindings(this);
		}

				#endregion //ClearContainerForItem

				#region PrepareContainerForItem

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method is called automatically by the <see cref="XamCarouselPanel"/>.  You do not ordinarily need to call this method.</p>
		/// </remarks>
		/// <param name="item">The item being hosted by the container.</param>
		/// <seealso cref="XamCarouselPanel"/>
		public virtual void PrepareContainerForItem(object item)
		{
		}

				#endregion //PrepareContainerForItem

			#endregion //Public Methods

			#region Internal Methods

				#region UpdateTransforms

		internal bool UpdateTransforms(Matrix newMatrix, double newSkewAngleX, double newSkewAngleY, double newSkewCenterX, double newSkewCenterY)
		{
			this.EnsureTransforms();

			bool valueChanged = false;

			if (this._matrixTransform.Matrix != newMatrix)
			{
				this._matrixTransform.Matrix	= newMatrix;
				valueChanged					= true;
			}

			if (this._skewTransform.AngleX != newSkewAngleX)
			{
				this._skewTransform.AngleX		= newSkewAngleX;
				valueChanged					= true;
			}

			if (this._skewTransform.AngleY != newSkewAngleY)
			{
				this._skewTransform.AngleY		= newSkewAngleY;
				valueChanged					= true;
			}

			if (double.IsNegativeInfinity(newSkewCenterX) == false && this._skewTransform.CenterX != newSkewCenterX)
			{
				this._skewTransform.CenterX		= newSkewCenterX;
				valueChanged					= true;
			}

			if (double.IsNegativeInfinity(newSkewCenterY) == false && this._skewTransform.CenterY != newSkewCenterY)
			{
				this._skewTransform.CenterY		= newSkewCenterY;
				valueChanged					= true;
			}

			return valueChanged;
		}

				#endregion //UpdateTransforms

			#endregion //Internal Methods

			#region Private Methods

				#region EnsureTransforms

		private void EnsureTransforms()
		{
			// Ensure that the RenderTransform is actually a TransformGroup with the 
			// second entry in the group being a ScaleTransform (this is to satisfy a
			// loose contract we have agreed upon for CarouselPanelItem styles)
			TransformGroup transformGroup = this.RenderTransform as TransformGroup;

			if (transformGroup != null)
			{
				TransformCollection tc = transformGroup.Children;

				if (tc.Count				> 2							&&
					tc[0].GetType()			== typeof(SkewTransform)	&&
					tc[1].GetType()			== typeof(ScaleTransform)	&&
					tc[2].GetType()			== typeof(MatrixTransform)	&&
					this._skewTransform		== tc[0]					&&
					this._scaleTransform	== tc[1]					&&
					this._matrixTransform	== tc[2])
					return;
			}

			transformGroup			= new TransformGroup();

			this._skewTransform		= new SkewTransform();
			transformGroup.Children.Add(this._skewTransform);

			this._scaleTransform	= new ScaleTransform();
			transformGroup.Children.Add(this._scaleTransform);

			this._matrixTransform	= new MatrixTransform();
			transformGroup.Children.Add(this._matrixTransform);

			this.RenderTransform	= transformGroup;
		}

				#endregion //EnsureTransforms

			#endregion //Private Transforms

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