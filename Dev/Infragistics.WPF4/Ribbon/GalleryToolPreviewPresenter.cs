using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Ribbon;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Displays <see cref="GalleryItem"/>s in the gallery preview area.
	/// </summary>
	[TemplatePart(Name="PART_DropDownButton", Type=typeof(FrameworkElement))]
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class GalleryToolPreviewPresenter : ItemsControl
	{
		#region Member Variables

		private GalleryTool									_galleryTool;
		private MenuToolBase								_menuTool;

		// AS 10/18/07
		// When the menu tool is active and we are showing a gallery preview, the dropdown button
		// needs to render as active.
		//
		private FrameworkElement							_dropDownButton;

		#endregion //Member Variables

		#region Constructor

		internal GalleryToolPreviewPresenter(GalleryTool galleryTool, MenuToolBase menuTool)
		{
			this._galleryTool	= galleryTool;
			this._menuTool		= menuTool;

			this.SetValue(GalleryToolPropertyKey, galleryTool);
			this.SetBinding(MaxColumnsInternalProperty, Utilities.CreateBindingObject(GalleryTool.MaxPreviewColumnsProperty, System.Windows.Data.BindingMode.OneWay, galleryTool));
			this.SetBinding(MinColumnsInternalProperty, Utilities.CreateBindingObject(GalleryTool.MinPreviewColumnsProperty, System.Windows.Data.BindingMode.OneWay, galleryTool));
			this.SetBinding(MaxColumnSpanInternalProperty, Utilities.CreateBindingObject(GalleryTool.MaxColumnSpanProperty, System.Windows.Data.BindingMode.OneWay, galleryTool));
		}

		static GalleryToolPreviewPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(typeof(GalleryToolPreviewPresenter)));
			FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			FrameworkElement.VerticalAlignmentProperty.OverrideMetadata(typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
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
			return new GalleryItemPresenter(true, this._galleryTool, null);
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
			return item is GalleryItemPresenter;
		}

			#endregion //IsItemItsOwnContainerOverride	

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// AS 10/18/07
			this._dropDownButton = this.GetTemplateChild("PART_DropDownButton") as FrameworkElement;

			// synchronize the IsActive property
			this.OnIsActiveChanged();
		}

			#endregion //OnApplyTemplate	

            #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="GalleryToolPreviewPresenter"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Ribbon.GalleryToolPreviewPresenterAutomationPeer"/></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GalleryToolPreviewPresenterAutomationPeer(this);
        }
            #endregion

			#region OnMouseEnter

		/// <summary>
		/// Raised when the mouse enters the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (this._galleryTool != null)
				this._galleryTool.OnMouseEnterItemArea(this);
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

			if (this.IsMouseOver == false && this._galleryTool != null)
				this._galleryTool.OnMouseLeaveItemArea(this);
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

			#region Public Properties

				#region GalleryTool

		private static readonly DependencyPropertyKey GalleryToolPropertyKey =
			DependencyProperty.RegisterReadOnly("GalleryTool",
			typeof(GalleryTool), typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="GalleryTool"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GalleryToolProperty =
			GalleryToolPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated <see cref="GalleryTool"/> whose <see cref="GalleryItem"/> instances will be displayed in the preview.
		/// </summary>
		/// <seealso cref="GalleryToolProperty"/>
		//[Description("Returns the associated 'GalleryTool' whose 'GalleryItem' instances will be displayed in the preview.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public GalleryTool GalleryTool
		{
			get
			{
				return (GalleryTool)this.GetValue(GalleryToolPreviewPresenter.GalleryToolProperty);
			}
		}

					#endregion //GalleryTool

				#region MaxColumns

		private static readonly DependencyPropertyKey MaxColumnsPropertyKey =
			DependencyProperty.RegisterReadOnly("MaxColumns",
			typeof(int), typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		/// <summary>
		/// Identifies the <see cref="MaxColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxColumnsProperty =
			MaxColumnsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the maximum number of columns to be displayed within the preview.
		/// </summary>
		/// <seealso cref="MaxColumnsProperty"/>
		//[Description("Returns the maximum number of columns to be displayed within the preview.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public int MaxColumns
		{
			get
			{
				return (int)this.GetValue(GalleryToolPreviewPresenter.MaxColumnsProperty);
			}
		}

				#endregion //MaxColumns

				#region MinColumns

		private static readonly DependencyPropertyKey MinColumnsPropertyKey =
			DependencyProperty.RegisterReadOnly("MinColumns",
			typeof(int), typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		/// <summary>
		/// Identifies the <see cref="MinColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinColumnsProperty =
			MinColumnsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the minimum number of columns to be displayed within the preview.
		/// </summary>
		/// <seealso cref="MinColumnsProperty"/>
		//[Description("Returns the minimum number of columns to be displayed within the preview.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public int MinColumns
		{
			get
			{
				return (int)this.GetValue(GalleryToolPreviewPresenter.MinColumnsProperty);
			}
		}

					#endregion //MinColumns

			#endregion //Public Properties

			#region Internal Properties

				#region ResizedColumnCount

		/// <summary>
		/// Used by the ribbon group resize logic to control the number of columns that should be displayed within the preview.
		/// </summary>
		internal static readonly DependencyProperty ResizedColumnCountProperty = MenuToolPresenter.ResizedColumnCountProperty.AddOwner(
			typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnResizedColumnCountChanged)));

		private static void OnResizedColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(MinColumnsInternalProperty);
			d.CoerceValue(MaxColumnsInternalProperty);
		}
				#endregion //ResizedColumnCount

				#region MenuTool

		internal MenuToolBase MenuTool
		{
			get { return this._menuTool; }
		}

				#endregion //MenuTool

			#endregion //Internal Properties

			#region Private Properties

				#region MaxColumnsInternal

		/// <summary>
		/// Identifies the maximum number of columns that should be displayed within the preview.
		/// </summary>
		private static readonly DependencyProperty MaxColumnsInternalProperty = DependencyProperty.Register("MaxColumnsInternal",
			typeof(int), typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnMaxColumnsInternalChanged), new CoerceValueCallback(CoerceMaxColumnsInternal)));

		private static object CoerceMaxColumnsInternal(DependencyObject d, object newValue)
		{
			int value = (int)newValue;

			// should not exceed the resize count
			int resizeColumnSpan = (int)d.GetValue(ResizedColumnCountProperty);

			if (resizeColumnSpan > 0 && (value == 0 || value > resizeColumnSpan))
				value = resizeColumnSpan;

			if (value > 0)
			{
				// also cannot be less than the minimum
				int minCols = (int)d.GetValue(MinColumnsInternalProperty);
				if (value < minCols)
					value = minCols;

				// must be at least the max column span
				int maxColSpan = (int)d.GetValue(MaxColumnSpanInternalProperty);

				if (value < maxColSpan)
					value = maxColSpan;

				newValue = value;
			}

			return newValue;
		}

		private static void OnMaxColumnsInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.SetValue(MaxColumnsPropertyKey, e.NewValue);

			// the scroll up/down buttons may be enabled/disabled
			CommandManager.InvalidateRequerySuggested();
		}
				#endregion //MaxColumnsInternal

				#region MaxColumnSpanInternal

		/// <summary>
		/// Used to determine the largest column span of the items in the associated GalleryTool
		/// </summary>
		private static readonly DependencyProperty MaxColumnSpanInternalProperty = DependencyProperty.Register("MaxColumnSpanInternal",
			typeof(int), typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnMaxColumnSpanInternalChanged)));

		private static void OnMaxColumnSpanInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(MinColumnsInternalProperty);
			d.CoerceValue(MaxColumnsInternalProperty);
		}
				#endregion //MaxColumnSpanInternal

				#region MinColumnsInternal

		/// <summary>
		/// Used to determine the minimum number of columns that should be displayed within the preview.
		/// </summary>
		private static readonly DependencyProperty MinColumnsInternalProperty = DependencyProperty.Register("MinColumnsInternal",
			typeof(int), typeof(GalleryToolPreviewPresenter), new FrameworkPropertyMetadata(1, new PropertyChangedCallback(OnMinColumnsInternalChanged), new CoerceValueCallback(CoerceMinColumnsInternal)));

		private static object CoerceMinColumnsInternal(DependencyObject d, object newValue)
		{
			int value = (int)newValue;
			int maxColSpan = (int)d.GetValue(MaxColumnSpanInternalProperty);

			if (value < maxColSpan)
				newValue = maxColSpan;

			return newValue;
		}

		private static void OnMinColumnsInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.SetValue(MinColumnsPropertyKey, e.NewValue);
			d.CoerceValue(MaxColumnsInternalProperty);
		}
				#endregion //MinColumnsInternal

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region OnIsActiveChanged
		// AS 10/18/07
		internal void OnIsActiveChanged()
		{
			// synchronize the IsActive property
			if (this._dropDownButton != null && this.MenuTool != null)
				this._dropDownButton.SetValue(XamRibbon.IsActivePropertyKey, this.MenuTool.GetValue(XamRibbon.IsActiveProperty));
		}

				#endregion //OnIsActiveChanged

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