using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Infragistics.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Windows.DataPresenter.Events;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
    /// <see cref="ViewBase"/> derived class that defines settings and defaults for a view that arranges data along a user defined path.
	/// </summary>
    /// <remarks>
    /// <p class="body">The CarouselView object is used by <see cref="XamDataCarousel"/> and <see cref="XamDataPresenter"/> to provide 
    /// settings and defaults that <see cref="DataPresenterBase"/> (the base class for the <see cref="XamDataCarousel"/> and 
    /// <see cref="XamDataPresenter"/> controls) can query when it provides UI element generation and field layout generation services in support of the View.  While the
    /// CarouselView is not actually reponsible for arranging items, it does expose a property called <see cref="ItemsPanelType"/> that returns the 
    /// <see cref="System.Windows.Controls.Panel"/> derived type that should be used to provide layout functionality for <see cref="DataRecord"/>s displayed in the
    /// view.  <see cref="DataPresenterBase"/> will ensure that a panel of <see cref="ItemsPanelType"/> is generated for use by the embedded <see cref="RecordListControl"/> 
    /// (the <see cref="System.Windows.Controls.ListBox"/> derived class used to display <see cref="DataRecord"/>s).</p>
    /// <p class="body">The CarouselView object exposes a property called <see cref="ViewSettings"/> that returns a <see cref="Infragistics.Windows.Controls.CarouselViewSettings"/> 
    /// object.  (Note: This property is not found on the <see cref="ViewBase"/> class but is specific to the CarouselView).  <see cref="Infragistics.Windows.Controls.CarouselViewSettings"/>
    /// in turn exposes a number of properties that let you control all aspects of layout and visual effects supported by the CarouselView.  This is the same object that
    /// is returned from the <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/>.ViewSettings and <see cref="Infragistics.Windows.Controls.XamCarouselListBox"/>.ViewSettings properties.
    /// Refer to <see cref="Infragistics.Windows.Controls.CarouselViewSettings"/> object for detailed information on these properties.</p>
    /// <p class="note"><b>Note: </b>CarouselView is only used by the <see cref="XamDataPresenter"/> control (as described above) when the <see cref="XamDataPresenter"/> control's <see cref="XamDataPresenter.View"/>
    /// property is set to an instance of CarouselView.</p>
    /// <p class="body">The following ViewBase properties are overridden by the CarouselView:
    /// 
    /// <table class="igbluetablenowidth">
    ///     <thead>
    ///         <tr>
    ///             <th>Property</th>
    ///             <th>Description</th>
    ///             <th>Overridden Value</th>
    ///         </tr>
    ///     </thead>
    ///     <tbody>
    ///         <tr>
    ///             <td>CellPresentation</td>
    ///             <td>Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.</td>
    ///             <td><see cref="CellPresentation"/>.CardView</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultAutoArrangeCells</td>
    ///             <td>Returns the default value for <see cref="AutoArrangeCells"/> for field layout templates generated on behalf of the View.</td>
    ///             <td><see cref="AutoArrangeCells"/>.TopToBottom</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultAutoArrangeMaxColumns</td>
    ///             <td>Returns the default maximum number of columns of cells to auto-generate in the field layout templates.</td>
    ///             <td>1</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultAutoArrangeMaxRows</td>
    ///             <td>Returns the default maximum number of rows of cells to auto-generate in the field layout templates.</td>
    ///             <td>3</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultAutoFit</td>
    ///             <td>Returns the default value for the resolved value for the <see cref="DataPresenterBase.AutoFit"/> if that property is set to null (Nothing in VB), its default value.</td>
    ///             <td>true</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultCellClickAction</td>
    ///             <td>Returns the default <see cref="CellClickAction"/> for cells in the View.</td>
    ///             <td><see cref="CellClickAction"/>.SelectCell</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultLabelClickAction</td>
    ///             <td>Returns the default <see cref="LabelClickAction"/> for cells in the View.</td>
    ///             <td><see cref="LabelClickAction"/>.Nothing</td>
    ///         </tr>
    ///         <tr>
    ///             <td>DefaultLabelLocation</td>
    ///             <td>Returns the default <see cref="LabelLocation"/> for <see cref="Field"/> Labels in the View.</td>
    ///             <td><see cref="LabelLocation"/>.Hidden</td>
    ///         </tr>
    ///         <tr>
    ///             <td>HasLogicalOrientation</td>
    ///             <td>Returns a value that indicates whether this View arranges its descendants in a particular dimension.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>HorizontalScrollBarVisibility</td>
    ///             <td>Returns a value that indicates when the horizontal scrollbar should be shown in this view.</td>
    ///             <td><see cref="ScrollBarVisibility"/>.Hidden</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsAddNewRecordSupported</td>
    ///             <td>Determines if the <see cref="DataPresenterBase"/> allows an AddNew record to be displayed in the View.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsAutoFitHeightSupported</td>
    ///             <td>Returns true if the height of the cells within in each row should be adjusted so that all cells will fit within the vertical space available for the row.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsAutoFitWidthSupported</td>
    ///             <td>Returns true if the width of the cells within in each row should be adjusted so that all cells will fit within the horizontal space available for the row.</td>
    ///             <td>true</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsLogicalFieldHeightResizingAllowed</td>
    ///             <td>Returns true if the <see cref="DataPresenterBase"/> UI should allow logical field heights in this View to be resized.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsLogicalFieldWidthResizingAllowed</td>
    ///             <td>Returns true if the <see cref="DataPresenterBase"/> UI should allow fields in this View to be resized.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>IsNestedPanelsSupported</td>
    ///             <td>Returns true if the View supports nested panels to display hierarchical data.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>ItemsPanelType</td>
    ///             <td>Returns the type of <see cref="System.Windows.Controls.Panel"/> used by the view to layout items in the list.</td>
    ///             <td>typeof(<see cref="CarouselViewPanel"/>)</td>
    ///         </tr>
    ///         <tr>
    ///             <td>RecordPresenterContainerType</td>
    ///             <td>Returns the type used as the container (if any) for <see cref="RecordPresenter"/>s in the View.</td>
    ///             <td>typeof(<see cref="CarouselItem"/>)</td>
    ///         </tr>
    ///         <tr>
    ///             <td>ShouldDisplayRecordSelectors</td>
    ///             <td>Returns true if the <see cref="DataPresenterBase"/> should generate and display a record selector for each record in the View.</td>
    ///             <td>false</td>
    ///         </tr>
    ///         <tr>
    ///             <td>SupportedDataDisplayMode</td>
    ///             <td>Returns a value that indicates the <see cref="DataDisplayMode"/> supported by the View.</td>
    ///             <td><see cref="DataDisplayMode"/>.Hierarchical</td>
    ///         </tr>
    ///         <tr>
    ///             <td>VerticalScrollBarVisibility</td>
    ///             <td>Returns a value that indicates when the vertical scrollbar should be shown in this view.</td>
    ///             <td><see cref="ScrollBarVisibility"/>.Hidden</td>
    ///         </tr>
    ///     </tbody>
    /// </table>
    /// </p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
    /// <see cref="ViewBase"/>
    /// <see cref="XamDataCarousel"/>
    /// <see cref="XamDataPresenter"/>
    /// <see cref="ItemsPanelType"/>
    /// <see cref="DataRecord"/>
    /// <see cref="RecordListControl"/>
    /// <see cref="ViewSettings"/>
    /// <see cref="Infragistics.Windows.Controls.CarouselViewSettings"/>
    /// <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/>
    /// <see cref="Infragistics.Windows.Controls.XamCarouselListBox"/>
    public class CarouselView : ViewBase
	{
		#region Member Variables

		private CarouselViewSettings							_viewSettings = null;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CarouselView"/> class
		/// </summary>
		public CarouselView()
		{
			this.SetValue(CarouselView.ViewSettingsProperty, this.ViewSettings);
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region Properties

				#region CellPresentation

		/// <summary>
        /// Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.
		/// </summary>
        /// <seealso cref="CellPresentation"/>
        /// <seealso cref="FieldLayoutTemplateGenerator"/>
		internal protected override CellPresentation CellPresentation
		{
			get { return CellPresentation.CardView; }
		}

				#endregion //CellPresentation	

				#region DefaultAutoArrangeCells

        /// <summary>
        /// Returns the default value for <see cref="AutoArrangeCells"/> for field layout templates generated on behalf of the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="AutoArrangeCells"/>.LeftToRight.
        /// </p>
        /// </remarks>
        /// <seealso cref="AutoArrangeCells"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AutoArrangeCells"/>
        internal protected override AutoArrangeCells DefaultAutoArrangeCells
		{
			get { return AutoArrangeCells.TopToBottom; }
		}

				#endregion //DefaultAutoArrangeCells	

				#region DefaultAutoArrangeMaxColumns

		/// <summary>
		/// Returns the default maximum number of columns of cells to auto-generate in the field layout templates.
		/// </summary>
		/// <remarks>
		/// The base implementation returns 0 which causes as many rows as necessary to be generated.
		/// </remarks>
		/// <seealso cref="DefaultAutoArrangeCells"/>
		/// <seealso cref="DefaultAutoArrangeMaxColumns"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeMaxColumns"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeMaxRows"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeCells"/>
		internal protected override int DefaultAutoArrangeMaxColumns
		{
			get { return 1; }
		}

				#endregion //DefaultAutoArrangeMaxColumns

				#region DefaultAutoArrangeMaxRows

		/// <summary>
		/// Returns the default maximum number of rows of cells to auto-generate in the field layout templates.
		/// </summary>
		/// <remarks>
		/// The base implementation returns 0 which causes as many rows as necessary to be generated.
		/// </remarks>
		/// <seealso cref="DefaultAutoArrangeCells"/>
		/// <seealso cref="DefaultAutoArrangeMaxColumns"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeMaxColumns"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeMaxRows"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeCells"/>
		internal protected override int DefaultAutoArrangeMaxRows
		{
			get { return 3; }
		}

				#endregion //DefaultAutoArrangeMaxRows

				// AS 3/22/07 AutoFit
				#region DefaultAutoFit

		/// <summary>
		/// Returns the default value for the resolved value for the <see cref="DataPresenterBase.AutoFit"/> if that property is set to null (Nothing in VB), its default value.
		/// </summary>
		/// <remarks>
        /// <p class="body">
		/// Returns true since the items should size to fill the size of the item.
        /// </p>
		/// </remarks>
        /// <seealso cref="DataPresenterBase.AutoFit"/>
		internal protected override bool DefaultAutoFit
		{
			get { return true; }
		}

				#endregion // DefaultAutoFit

				#region DefaultCellClickAction

		/// <summary>
        /// Returns the default <see cref="CellClickAction"/> for cells in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="CellClickAction"/>.Default.  This will ultimately cause <see cref="Field.DefaultCellClickAction"/> to be used.
        /// </p>
        /// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.CellClickAction"/>
        /// <seealso cref="Field.DefaultCellClickAction"/>
		internal protected override CellClickAction DefaultCellClickAction
		{
			get { return CellClickAction.SelectCell; }
		}

				#endregion //DefaultCellClickAction	

				#region DefaultLabelClickAction

		/// <summary>
        /// Returns the default <see cref="LabelClickAction"/> for cells in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns LabelClickAction.Default.  This will ultimately cause <see cref="Field.DefaultLabelClickAction"/> to be used.
        /// </p>
        /// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.LabelClickAction"/>
        /// <seealso cref="Field.DefaultLabelClickAction"/>
		internal protected override LabelClickAction DefaultLabelClickAction
		{
			get { return LabelClickAction.Nothing; }
		}

				#endregion //DefaultLabelClickAction	

				#region DefaultLabelLocation

		/// <summary>
        /// Returns the default <see cref="LabelLocation"/> for <see cref="Field"/> Labels in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="LabelLocation"/>.Default.
        /// </p>
        /// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.LabelLocation"/>
        /// <seealso cref="LabelLocation"/>
		internal protected override LabelLocation DefaultLabelLocation
		{
			get { return LabelLocation.Hidden; }
		}

				#endregion //DefaultLabelLocation
   
				#region HasLogicalOrientation

		/// <summary>
		/// Returns a value that indicates whether this View arranges its descendants in a particular dimension.
		/// </summary>
		internal protected override bool HasLogicalOrientation
		{
			get { return false; }
		}

				#endregion //HasLogicalOrientation	
    
				#region HorizontalScrollBarVisibility

		/// <summary>
		/// Returns a value that indicates when the horizontal scrollbar should be shown in this view.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="System.Windows.Controls.ScrollBarVisibility"/>.Auto.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Windows.Controls.ScrollBarVisibility"/>
		internal protected override ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return ScrollBarVisibility.Hidden; }
		}

				#endregion //HorizontalScrollBarVisibility	
    
				#region IsAddNewRecordSupported

		/// <summary>
		/// Determines if the <see cref="DataPresenterBase"/> allows an AddNew record to be displayed in the View.
		/// </summary>
        /// <value><para class="body">Returns true if the <see cref="DataPresenterBase"/> should allow an AddNew record to be displayed in the View.</para></value>
		/// <remarks>
		/// <para class="body">Since this view does not support adding records it return false.</para>
		/// </remarks>
        /// <seealso cref="DataPresenterBase"/>
		internal protected override bool IsAddNewRecordSupported
		{
			get	{ return false; }
		}

   				#endregion //IsAddNewRecordSupported	

				#region IsAutoFitHeightSupported
    
		/// <summary>
		/// Returns true if the height of the cells within in each row should be adjusted so that all cells will fit within the vertical space available for the row.
		/// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default implementation returns false.
        /// </p>
        /// </remarks>
        internal protected override bool IsAutoFitHeightSupported
		{
			get	{ return false; }
		}

   				#endregion //IsAutoFitHeightSupported	

				#region IsAutoFitWidthSupported

        /// <summary>
        /// Returns true if the width of the cells within in each row should be adjusted so that all cells will fit within the horizontal space available for the row.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default implementation returns false.
        /// </p>
        /// </remarks>
        internal protected override bool IsAutoFitWidthSupported
		{
			get	{ return true; }
		}

   				#endregion //IsAutoFitWidthSupported	

				#region IsLogicalFieldHeightResizingAllowed

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> UI should allow logical field heights in this View to be resized.
		/// </summary>
		/// <remarks>
        /// <p class="body">
		/// The base implementation returns true.
        /// The <see cref="Field"/> height that is affected by this property is the logical <see cref="Field"/> height.  When the view's
        /// orientation is vertical the <see cref="Field"/>'s logical height is equivalent to the <see cref="Field"/>'s physical height.  When 
        /// the View's orientation is horizontal the <see cref="Field"/>'s logical height is equivalent to the <see cref="Field"/>'s physical width.
        /// </p>
		/// </remarks>
        /// <see cref="DataPresenterBase"/>
        /// <seealso cref="Field"/>
        internal protected override bool IsLogicalFieldHeightResizingAllowed
		{
			get { return false; }
		}

				#endregion //IsLogicalFieldHeightResizingAllowed

				#region IsLogicalFieldWidthResizingAllowed

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> UI should allow fields in this View to be resized.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// The <see cref="Field"/> width that is affected by this property is the logical <see cref="Field"/> width.  When the view's
        /// orientation is vertical the <see cref="Field"/>'s logical width is equivalent to the <see cref="Field"/>'s physical width.  When 
        /// the View's orientation is horizontal the <see cref="Field"/>'s logical width is equivalent to the <see cref="Field"/>'s physical height.
        /// </p>
        /// </remarks>
        /// <seealso cref="Field"/>
        /// <seealso cref="DataPresenterBase"/>
		internal protected override bool IsLogicalFieldWidthResizingAllowed
		{
			get { return false; }
		}

				#endregion //IsLogicalFieldWidthResizingAllowed	

				#region IsNestedPanelsSupported

		/// <summary>
		/// Returns true if the View supports nested panels to display hierarchical data.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// </p>
        /// </remarks>
		internal protected override bool IsNestedPanelsSupported
		{
			get { return false; }
		}

				#endregion //IsNestedPanelsSupported

				#region IsSummaryRecordSupported

		// SSP 5/15/08 - Summaries Feature
		// Added IsSummaryRecordSupported.
		// 
		/// <summary>
		/// Returns false since carousel view doesn't support displaying summary records.
		/// </summary>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		internal protected override bool IsSummaryRecordSupported
		{
			get { return false; }
		}

				#endregion // IsSummaryRecordSupported
    
				#region ItemsPanelType

		/// <summary>
        /// Returns the type of <see cref="System.Windows.Controls.Panel"/> used by the view to layout items in the list.
		/// </summary>
        /// <seealso cref="System.Windows.Controls.Panel"/>
		internal protected override Type ItemsPanelType
		{
			get { return typeof(CarouselViewPanel); }
		}

				#endregion //ItemsPanelType	

				#region RecordPresenterContainerType

		/// <summary>
        /// Returns the type used as the container (if any) for <see cref="RecordPresenter"/>s in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns null.
        /// Note: If this property is overridden then the <see cref="GetContainerForRecordPresenter"/> method should also be overridden to return a container of the same type.
        /// </p>
        /// </remarks>
        /// <seealso cref="RecordPresenter"/>
        /// <seealso cref="GetContainerForRecordPresenter"/>
		internal protected override Type RecordPresenterContainerType
		{
			get { return typeof(CarouselItem); }
		}

				#endregion //RecordPresenterContainerType	

				#region ShouldDisplayRecordSelectors

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should generate and display a record selector for each record in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
		internal protected override bool ShouldDisplayRecordSelectors
		{
			get { return false; }
		}

				#endregion //ShouldDisplayRecordSelectors	
    
				#region SupportedDataDisplayMode

		/// <summary>
        /// Returns a value that indicates the <see cref="DataDisplayMode"/> supported by the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="DataDisplayMode"/>.Flat.
        /// Note: Views that support the Hierarchical <see cref="DataDisplayMode"/> are responsible for managing
        /// the display of nested data.  Such views will typically return true for <see cref="IsNestedPanelsSupported"/>.
        /// When the <see cref="DataDisplayMode"/>.Flat enumeration is returned, the DataPresenter will still include child
        /// records in the <see cref="ViewableRecordCollection"/>s but it will cause expansion indicators to be hidden and
		/// prohibit records from being expanded.
        /// </p>
        /// </remarks>
		/// <seealso cref="DataDisplayMode"/>
		/// <seealso cref="IsNestedPanelsSupported"/>
        /// <seealso cref="ViewableRecordCollection"/>
		internal protected override DataDisplayMode SupportedDataDisplayMode
		{
			get { return DataDisplayMode.Hierarchical; }
		}

				#endregion //SupportedDataDisplayMode
    
				#region VerticalScrollBarVisibility

		/// <summary>
		/// Returns a value that indicates when the vertical scrollbar should be shown in this view.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="System.Windows.Controls.ScrollBarVisibility"/>.Auto.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Windows.Controls.ScrollBarVisibility"/>
		internal protected override ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return ScrollBarVisibility.Hidden; }
		}

				#endregion //VerticalScrollBarVisibility	

			#endregion //Properties

			#region Methods

				#region GetContainerForRecordPresenter

		// SSP 4/22/08 - Summaries Feature
		// Took out RecordPresenter parameter from GetContainerForRecordPresenter because in GetContainerForItemOverride
		// we don't have the item and thus don't know which record presenter to create, and thus we cannot pass that
		// along into this method. Instead we added PrepareContainerForRecordPresenter method that will allow the view
		// to associated its wrapper to the record presenter.
		// 
		/// <summary>
		/// Returns an element that will wrap a <see cref="RecordPresenter"/> inside it. If no wrapper is required, the method should return <b>null</b>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, along with the <see cref="PrepareContainerForRecordPresenter"/>, only need to be overridden if the 
		/// view requires a specialized wrapper element around each <see cref="RecordPresenter"/> element. You would create
		/// a wrapper element in this method and associate it with a record presenter in <i>PrepareContainerForRecordPresenter</i> method.
		/// </p>
		/// </remarks>
		/// <param name="panel">The <see cref="System.Windows.Controls.Panel"/> derived element that will contain the returned wrapper element.</param>
		/// <returns>The container object to use to wrap a <see cref="RecordPresenter"/>.</returns>
		/// <seealso cref="PrepareContainerForRecordPresenter"/>
		internal protected override DependencyObject GetContainerForRecordPresenter( Panel panel )
		{
			Debug.Assert( panel is XamCarouselPanel );

			if ( panel is XamCarouselPanel )
			{
				CarouselItem carouselItem = new CarouselItem( panel as XamCarouselPanel );
				return carouselItem;
			}

			return null;
		}
		
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetContainerForRecordPresenter	

                #region GetFieldLayoutTemplateGenerator

        /// <summary>
        /// Gets a <see cref="FieldLayoutTemplateGenerator"/> derived class for generating an appropriate template for the specified layout in the current View.
        /// </summary>
        /// <param name="fieldLayout">The specified layout</param>
        /// <seealso cref="FieldLayoutTemplateGenerator"/>
        /// <seealso cref="FieldLayout"/>
        internal protected override FieldLayoutTemplateGenerator GetFieldLayoutTemplateGenerator(FieldLayout fieldLayout)
        {
            return new CardViewFieldLayoutTemplateGenerator(fieldLayout);
        }

                #endregion //GetFieldLayoutTemplateGenerator

				#region OnPropertyChanged

		/// <summary>
		/// Called when the value of a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

            // AS 3/11/09 Optimization
			//switch (e.Property.Name)
			//{
			//	case "ViewSettings":
            if (e.Property == ViewSettingsProperty)
            {
					// Unhook from old
					if (this._viewSettings != null)
						this._viewSettings.PropertyChanged -= new PropertyChangedEventHandler(this.OnViewSettingsPropertyChanged);


					// Set our ViewSettings member variable.
					this._viewSettings = e.NewValue as CarouselViewSettings;


					// Hook new change event
					if (this._viewSettings != null)
						this._viewSettings.PropertyChanged += new PropertyChangedEventHandler(this.OnViewSettingsPropertyChanged);


					// Raise a ViewStateChanged event.
					this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateMeasure));

					//break;
			}
		}

				#endregion //OnPropertyChanged	

				#region PrepareContainerForRecordPresenter

		// SSP 4/22/08 - Summaries Feature
		// Took out RecordPresenter parameter from GetContainerForRecordPresenter because in GetContainerForItemOverride
		// we don't have the item and thus don't know which record presenter to create, and thus we cannot pass that
		// along into this method. Instead we added PrepareContainerForRecordPresenter method that will allow the view
		// to associated its wrapper to the record presenter.
		// 
		/// <summary>
		/// Associates the wrapper element returned from <see cref="GetContainerForRecordPresenter"/> with 
		/// the specified <see cref="RecordPresenter"/> element.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, along with the <see cref="GetContainerForRecordPresenter"/>, only need to be overridden if the 
		/// view requires a specialized wrapper element around each <see cref="RecordPresenter"/> element. You would create
		/// a wrapper element in <see cref="GetContainerForRecordPresenter"/> and associate it with a record presenter in 
		/// this method.
		/// </p>
		/// </remarks>
		/// <param name="panel">The <see cref="System.Windows.Controls.Panel"/> derived element that contains the wrapper element.</param>
		/// <param name="wrapper">The wrapper element that was returned from the <see cref="GetContainerForRecordPresenter"/>.</param>
		/// <param name="recordPresenter">The <see cref="RecordPresenter"/> to associate with the wrapper.</param>
		/// <seealso cref="GetContainerForRecordPresenter"/>
		internal protected override void PrepareContainerForRecordPresenter( Panel panel, DependencyObject wrapper, RecordPresenter recordPresenter )
		{
			Debug.Assert( panel is XamCarouselPanel && wrapper is CarouselItem );
			CarouselItem carouselItem = wrapper as CarouselItem;
			if ( null != carouselItem )
				carouselItem.Content = recordPresenter;
		}

				#endregion //PrepareContainerForRecordPresenter

			#endregion //Methods

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ViewSettingsProperty = DependencyProperty.Register("ViewSettings",
			typeof(CarouselViewSettings), typeof(CarouselView), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
			if (value == null)
				return new CarouselViewSettings();

			return value;
		}

		/// <summary>
        /// Returns/sets the <see cref="CarouselViewSettings"/> object for this CarouselView.
		/// </summary>
        /// <remarks>
        /// <p class="body"><see cref="Infragistics.Windows.Controls.CarouselViewSettings"/> exposes a number of properties that let you control all aspects of layout 
        /// and visual effects supported by the CarouselView.  This is the same object that is returned from the 
        /// <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/>.ViewSettings and <see cref="Infragistics.Windows.Controls.XamCarouselListBox"/>.ViewSettings properties.
        /// Refer to <see cref="Infragistics.Windows.Controls.CarouselViewSettings"/> object for detailed information on these properties.</p>
        /// </remarks>
		/// <seealso cref="ViewSettingsProperty"/>
		/// <seealso cref="CarouselViewSettings"/>
        /// <see cref="Infragistics.Windows.Controls.XamCarouselPanel"/>
        /// <see cref="Infragistics.Windows.Controls.XamCarouselListBox"/>
        //[Description("Returns/sets the CarouselViewSettings object for this CarouselView.")]
		//[Category("Appearance")]
		[Bindable(true)]
        public CarouselViewSettings ViewSettings
		{
			get
			{
				if (this._viewSettings == null)
					this._viewSettings = new CarouselViewSettings();

				return this._viewSettings;
			}
			set
			{
				this.SetValue(CarouselView.ViewSettingsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (CarouselViewSettings)CarouselView.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ClearValue(CarouselView.ViewSettingsProperty);
		}

				#endregion //ViewSettings

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnViewSettingsPropertyChanged

		private void OnViewSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaiseViewStateChanged(new ViewStateChangedEventArgs(ViewStateChangedAction.InvalidateMeasure));
		}

				#endregion //OnViewSettingsPropertyChanged

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