using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter.Events;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Data;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{

	/// <summary>
    /// Abstract base class for all <see cref="DataPresenterBase"/> views that defines settings and defaults for the view .
	/// </summary>
    /// <remarks>
    /// <p class="body">ViewBase derived objects are used by the <see cref="XamDataCarousel"/>, <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> to provide 
    /// settings and defaults that <see cref="DataPresenterBase"/> (the base class for the <see cref="XamDataCarousel"/>, <see cref="XamDataGrid"/> and 
    /// <see cref="XamDataPresenter"/> controls) can query when it provides UI element generation and field layout generation services in support of the View.</p>
    /// <p class="body">While the ViewBase object is not actually reponsible for arranging items, it does expose a property called <see cref="ItemsPanelType"/> that returns the 
    /// <see cref="System.Windows.Controls.Panel"/> derived type that should be used to provide layout functionality for <see cref="DataRecord"/>s displayed in the
    /// view.  <see cref="DataPresenterBase"/> will ensure that a panel of <see cref="ItemsPanelType"/> is generated for use by the embedded <see cref="RecordListControl"/> 
    /// (the System.Windows.Controls.ListBox derived class used to display <see cref="DataRecord"/>s)</p>
    /// <p class="note"><b>Note: </b>Two ViewBase derived views are included in this version of the controls:
    ///		<ul>
    ///			<li><see cref="GridView"/> - Arranges items in a classic grid format.</li>
    ///			<li><see cref="CarouselView"/> - Arranges items along a user defined path.</li>
    ///		</ul>
	/// You can extend the ViewBase class to provide additional custom views.  Refer to the <a href="xamDataPresenter_Creating_Custom_Views.html">Creating Custom Views for XamDataPresenter</a> topic in the Developer's Guide for an explanation of how to create your own custom views.
    /// </p>
    /// </remarks>
    /// <seealso cref="GridView"/>
	/// <seealso cref="CarouselView"/>
    /// <seealso cref="DataRecord"/>
    /// <seealso cref="DataPresenterBase"/>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="XamDataGrid"/>
    /// <seealso cref="ItemsPanelType"/>
    /// <seealso cref="RecordListControl"/>






	public abstract class ViewBase : FrameworkContentElement
	{
		#region Member Variables

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewBase"/> class.
		/// </summary>
		protected ViewBase()
		{
		}

		#endregion //Constructor

		#region Properties

			#region Abstract Properties

				#region CellPresentation

        /// <summary>
        /// Returns the type of <see cref="CellPresentation"/> used by the view which determines the default manner in which the cells within each row are laid out by the <see cref="FieldLayoutTemplateGenerator"/>.
        /// </summary>
        /// <seealso cref="CellPresentation"/>
        /// <seealso cref="FieldLayoutTemplateGenerator"/>
        internal protected abstract CellPresentation CellPresentation
		{
			get;
		}

				#endregion //CellPresentation	
   
				#region HasLogicalOrientation

		/// <summary>
		/// Returns a value that indicates whether this View arranges its descendants in a particular dimension.
		/// </summary>
		internal protected abstract bool HasLogicalOrientation
		{
			get ;
		}

				#endregion //HasLogicalOrientation	
    
				#region ItemsPanelType

        /// <summary>
        /// Returns the type of <see cref="System.Windows.Controls.Panel"/> used by the view to layout items in the list.
        /// </summary>
        /// <seealso cref="System.Windows.Controls.Panel"/>
        internal protected abstract Type ItemsPanelType
		{
			get;
		}

				#endregion //ItemsPanelType	
    
			#endregion //Abstract Properties

			#region Protected Virtual Properties

				#region AutoFitToRecord

		/// <summary>
		/// Returns a boolean indicating whether the cell area of a <see cref="DataRecordPresenter"/> will 
		/// be auto sized to the <see cref="RecordPresenter"/> itself or based on the root <see cref="RecordListControl"/> when 
		/// <see cref="DataPresenterBase.AutoFitResolved"/> is true.
		/// </summary>
		/// <remarks>
		/// <p class="body">For <see cref="XamDataPresenter.View"/>s where the item size should dictate the size available to the <see cref="RecordPresenter"/> for 
		/// the cell area, this should return true. For views such as <see cref="GridView"/>, where all the records are to 
		/// be constrained by the size available within the control itself, this should return false.</p>
		/// </remarks>
        /// <seealso cref="DataRecordPresenter"/>
        /// <seealso cref="RecordListControl"/>
        /// <seealso cref="XamDataPresenter.View"/>
        /// <seealso cref="GridView"/>
		/// <seealso cref="ViewBase.DefaultAutoFit"/>
		/// <seealso cref="IsAutoFitHeightSupported"/>
		/// <seealso cref="IsAutoFitWidthSupported"/>
		internal protected virtual bool AutoFitToRecord
		{
		    get { return true; }
		}
 
				#endregion //AutoFitToRecord

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
        internal protected virtual AutoArrangeCells DefaultAutoArrangeCells
		{
			get { return AutoArrangeCells.LeftToRight; }
		}

				#endregion //DefaultAutoArrangeCells	

				#region DefaultAutoArrangeMaxColumns

		/// <summary>
		/// Returns the default maximum number of columns of cells to auto-generate in the field layout templates.
		/// </summary>
		/// <remarks>
		/// The base implementation returns 0 which causes as many columns as necessary to be generated.
		/// </remarks>
		/// <seealso cref="DefaultAutoArrangeCells"/>
		/// <seealso cref="DefaultAutoArrangeMaxRows"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeMaxColumns"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeMaxRows"/>
		/// <seealso cref="FieldLayoutSettings.AutoArrangeCells"/>
		internal protected virtual int DefaultAutoArrangeMaxColumns
		{
			get { return 0; }
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
		internal protected virtual int DefaultAutoArrangeMaxRows
		{
			get { return 0; }
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
        internal protected virtual bool DefaultAutoFit
		{
			get { return false; }
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
        internal protected virtual CellClickAction DefaultCellClickAction
		{
			get { return CellClickAction.Default; }
		}

				#endregion //DefaultCellClickAction	

				// JM NA 10.1 CardView 
				#region DefaultCellContentAlignment

        /// <summary>
        /// Returns the default <see cref="CellContentAlignment"/> for <see cref="Cell"/> content in the View.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="CellContentAlignment"/>.Default.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.CellContentAlignment"/>
        /// <seealso cref="CellContentAlignment"/>
		internal protected virtual CellContentAlignment DefaultCellContentAlignment
		{
			get { return CellContentAlignment.Default; }
		}

				#endregion //DefaultLabelLocation

				#region DefaultDataRecordSizingMode

		/// <summary>
		/// Returns the default <see cref="DataRecordSizingMode"/> for data records in the View.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns <see cref="DataRecordSizingMode"/>.Default.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataRecordSizingMode"/>
		/// <seealso cref="FieldLayoutSettings.DataRecordSizingMode"/>
		internal protected virtual DataRecordSizingMode DefaultDataRecordSizingMode
		{
			get { return DataRecordSizingMode.Default; }
		}

				#endregion //DefaultDataRecordSizingMode	

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
        internal protected virtual LabelClickAction DefaultLabelClickAction
		{
			get { return LabelClickAction.Default; }
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
        internal protected virtual LabelLocation DefaultLabelLocation
		{
			get { return LabelLocation.Default; }
		}

				#endregion //DefaultLabelLocation

				// JM NA 10.1 CardView 
				#region DefaultShouldCollapseCards

		/// <summary>
		/// Returns a default value for whether the View should collapse Cards.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns false.
		/// </p>
		/// </remarks>
		internal protected virtual bool DefaultShouldCollapseCards
		{
			get { return false; }
		}

				#endregion //DefaultShouldCollapseCards

				// JM NA 10.1 CardView 
				#region DefaultShouldCollapseEmptyCells

		/// <summary>
		/// Returns a default value for whether the View should collapse Cells with empty values.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns false.
		/// </p>
		/// </remarks>
		internal protected virtual bool DefaultShouldCollapseEmptyCells
		{
			get { return false; }
		}

				#endregion //DefaultShouldCollapseEmptyCells

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
        internal protected virtual ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return ScrollBarVisibility.Auto; }
		}

				#endregion //HorizontalScrollBarVisibility	
				
				// JJD 4/28/11 - TFS73523 - added 
				#region InterRecordSpacingX

		/// <summary>
		/// Returns the amount of space between records in the X dimension (used for resizing logic).
		/// </summary>
		public virtual double InterRecordSpacingX { get { return 0; } }
				
				#endregion //InterRecordSpacingX
				
				// JJD 4/28/11 - TFS73523 - added 
				#region InterRecordSpacingY

		/// <summary>
		/// Returns the amount of space between records in the Y dimension (used for resizing logic).
		/// </summary>
		public virtual double InterRecordSpacingY { get { return 0; } }
				
				#endregion //InterRecordSpacingY
    
				#region IsAddNewRecordSupported

        /// <summary>
        /// Determines if the <see cref="DataPresenterBase"/> allows an AddNew record to be displayed in the View.
        /// </summary>
        /// <value><para class="body">Returns true if the <see cref="DataPresenterBase"/> should allow an AddNew record to be displayed in the View.</para></value>
        /// <remarks>
        /// <para class="body">The base implementation returns true.</para>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
        internal protected virtual bool IsAddNewRecordSupported
		{
			get	{ return true; }
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
		internal protected virtual bool IsAutoFitHeightSupported
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
		internal protected virtual bool IsAutoFitWidthSupported
		{
			get	{ return false; }
		}

   				#endregion //IsAutoFitWidthSupported	

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				#region IsChildRecordsDisplayOrderSupported

		/// <summary>
		/// Returns true if the view supports the ability to control where the child records are ordered relative to the parent record.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns false.
		/// </p>
		/// </remarks>
		internal protected virtual bool IsChildRecordsDisplayOrderSupported
		{
			get { return false; }
		} 

				#endregion // IsChildRecordsDisplayOrderSupported

				// JM NA 10.1 CardView 
				#region IsEmptyCellCollapsingSupported

		/// <summary>
		/// Returns true if the View supports the collapsing of Cells with empty values.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The default implementation returns false.
		/// </p>
		/// </remarks>
		internal protected virtual bool IsEmptyCellCollapsingSupported
		{
			get { return false; }
		}

				#endregion //IsEmptyCellCollapsingSupported

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
        internal protected virtual bool IsLogicalFieldHeightResizingAllowed
		{
			get { return true; }
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
        internal protected virtual bool IsLogicalFieldWidthResizingAllowed
		{
			get { return true; }
		}

				#endregion //IsLogicalFieldWidthResizingAllowed	

				#region IsFilterRecordSupported

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// Added IsFilterRecordSupported property.
		// 
		/// <summary>
		/// Returns true if the <see cref="DataPresenterBase"/> should display filter records.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base implementation returns false.
		/// </p>
		/// </remarks>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
		internal protected virtual bool IsFilterRecordSupported
		{
			get { return false; }
		}

				#endregion // IsFilterRecordSupported

                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
				#region IsFixedFieldsSupported

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should allow fields to be fixed in the UI.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns the false.
        /// </p>
        /// <p class="note"><b>Note:</b> The view must also return true from <see cref="HasLogicalOrientation"/>.</p>
        /// </remarks>
        /// <seealso cref="Field.FixedLocation"/>
        /// <seealso cref="FieldSettings.AllowFixing"/>
		internal protected virtual bool IsFixedFieldsSupported
		{
			get { return false; }
		}

				#endregion //IsFixedFieldsSupported

				#region IsFixedRecordsSupported

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should allow root records to be fixed at the top or bottom of the UI.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns false.
        /// When a record is fixed it's position in the <see cref="ViewableRecordCollection"/> is changed so the the 
		/// record is positioned with other fixed records at the beginning or end of the list.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
        /// <seealso cref="ViewableRecordCollection"/>
		internal protected virtual bool IsFixedRecordsSupported
		{
			get { return false; }
		}

				#endregion //IsFixedRecordsSupported

				#region IsFixingSupportedForNestedRecords

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should allow records to be fixed at the top of the UI for nested (i.e. non-root) records.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns false.
        /// When a record is fixed it's position in the <see cref="ViewableRecordCollection"/> is changed so the the 
		/// record is positioned with other fixed records at the beginning or end of the list.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
        /// <seealso cref="ViewableRecordCollection"/>
        internal protected virtual bool IsFixingSupportedForNestedRecords
		{
			get { return false; }
		}

				#endregion //IsFixingSupportedForNestedRecords

				#region IsGroupBySupported

		/// <summary>
        /// Returns true if the <see cref="DataPresenterBase"/> should display the <see cref="GroupByArea"/> by default and allow programmatic grouping of records.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
        /// <seealso cref="GroupByArea"/>
		/// <seealso cref="DataPresenterBase.GroupByAreaLocation"/>
		internal protected virtual bool IsGroupBySupported
		{
			get { return true; }
		}

				#endregion //IsGroupBySupported

				#region IsNestedPanelsSupported

        /// <summary>
        /// Returns true if the View supports nested panels to display hierarchical data.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The base implementation returns true.
        /// </p>
        /// </remarks>
        internal protected virtual bool IsNestedPanelsSupported
		{
			get { return true; }
		}

				#endregion //IsNestedPanelsSupported

				// JJD 2/22/12 - TFS101199 - Touch support
				#region IsPanningModeSupported

		/// <summary>
        /// Returns true if the RecordListControl should coerce its ScrollViewer.PanningMode property to enable standard flick scrolling on touch enabled systems.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The base implementation returns false.
        /// </p>
        /// </remarks>
        /// <seealso cref="DataPresenterBase"/>
		internal protected virtual bool IsPanningModeSupported
		{
			get { return false; }
		}

				#endregion //IsPanningModeSupported



		#region IsSummaryRecordSupported

		// SSP 5/15/08 - Summaries Feature
		// Added IsSummaryRecordSupported.
		// 
		/// <summary>
		/// Returns true if the <see cref="DataPresenterBase"/> should display summary records.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base implementation returns true.
		/// </p>
		/// </remarks>
		/// <seealso cref="DataPresenterBase"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		internal protected virtual bool IsSummaryRecordSupported
		{
			get { return true; }
		}

				#endregion // IsSummaryRecordSupported
    
				#region LogicalOrientation

		/// <summary>
        /// The <see cref="System.Windows.Controls.Orientation"/> of the View, if the view only supports layout in a particular dimension.
		/// </summary>
		/// <remarks>
        /// <p class="body">
        /// The <see cref="HasLogicalOrientation"/> property returns whether the View only supports layout in a particular dimension.
        /// The base implementation returns <see cref="System.Windows.Controls.Orientation"/>.Vertical.
        /// </p>
        /// </remarks>
        /// <seealso cref="System.Windows.Controls.Orientation"/>
        /// <seealso cref="HasLogicalOrientation"/>
		internal protected virtual Orientation LogicalOrientation
		{
			get { return Orientation.Vertical; }
		}

				#endregion //LogicalOrientation	

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
        internal protected virtual Type RecordPresenterContainerType
		{
			get { return null; }
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
        internal protected virtual bool ShouldDisplayRecordSelectors
		{
			get { return true; }
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
        internal protected virtual DataDisplayMode SupportedDataDisplayMode
		{
			get { return DataDisplayMode.Flat; }
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
        internal protected virtual ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return ScrollBarVisibility.Auto; }
		}

				#endregion //VerticalScrollBarVisibility	
    
			#endregion //Protected Virtual Properties

        #endregion //Properties

        #region Methods

            #region Internal Methods

            #endregion //Internal Methods

            #region Protected Abstract methods

                #region GetFieldLayoutTemplateGenerator

        /// <summary>
        /// Gets a <see cref="FieldLayoutTemplateGenerator"/> derived class for generating an appropriate template for the specified layout in the current View.
        /// </summary>
        /// <param name="fieldLayout">The specified layout</param>
        /// <seealso cref="FieldLayoutTemplateGenerator"/>
        /// <seealso cref="FieldLayout"/>
        internal protected abstract FieldLayoutTemplateGenerator GetFieldLayoutTemplateGenerator(FieldLayout fieldLayout);

                #endregion //GetFieldLayoutTemplateGenerator

			#endregion //Protected Abstract Methods

			#region Protected Virtual Methods

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
		internal protected virtual DependencyObject GetContainerForRecordPresenter( Panel panel )
		{
			return null;
		}
		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetContainerForRecordPresenter	

                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
                #region GetFixedFieldInfo
        /// <summary>
        /// For a view that supports fixing fields, this method must return an object that provides the scroll offset for a given <see cref="RecordPresenter"/>
        /// </summary>
        /// <param name="recordPresenter">The record element for which the fixed field information is being requested.</param>
        internal virtual FixedFieldInfo GetFixedFieldInfo(RecordPresenter recordPresenter)
        {
            return null;
        } 
                #endregion //GetFixedFieldInfo

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
		internal protected virtual void PrepareContainerForRecordPresenter( Panel panel, DependencyObject wrapper, RecordPresenter recordPresenter )
		{
		}

				#endregion //PrepareContainerForRecordPresenter

			#endregion //Protected Virtual Methods

		#endregion //Methods

		#region Events

		#region ViewStateChanged

		/// <summary>
		/// Event ID for the <see cref="ViewStateChanged"/> routed event
		/// </summary>
		/// <seealso cref="ViewStateChanged"/>
		/// <seealso cref="OnViewStateChanged"/>
		/// <seealso cref="ViewStateChangedEventArgs"/>
		public static readonly RoutedEvent ViewStateChangedEvent =
			EventManager.RegisterRoutedEvent("ViewStateChanged", RoutingStrategy.Direct, typeof(EventHandler<ViewStateChangedEventArgs>), typeof(ViewBase));

		/// <summary>
		/// Occurs when the state of the View has changed, and an update to the UI is required.
		/// </summary>
		/// <seealso cref="ViewStateChanged"/>
		/// <seealso cref="ViewStateChangedEvent"/>
		/// <seealso cref="ViewStateChangedEventArgs"/>
		protected virtual void OnViewStateChanged(ViewStateChangedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseViewStateChanged(ViewStateChangedEventArgs args)
		{
			args.RoutedEvent = ViewBase.ViewStateChangedEvent;
			args.Source = this;
			this.OnViewStateChanged(args);
		}

		/// <summary>
		/// Occurs when the state of the View has changed, and an update to the UI is required.
		/// </summary>
		/// <seealso cref="OnViewStateChanged"/>
		/// <seealso cref="ViewStateChanged"/>
		/// <seealso cref="ViewStateChangedEvent"/>
		/// <seealso cref="ViewStateChangedEventArgs"/>
		//[Description("Occurs when the state of the View has changed, and an update to the UI is required.")]
		//[Category("Behavior")]
		public event EventHandler<ViewStateChangedEventArgs> ViewStateChanged
		{
			add
			{
				base.AddHandler(ViewBase.ViewStateChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(ViewBase.ViewStateChangedEvent, value);
			}
		}

			#endregion //ViewStateChanged

		#endregion //Events
	}

    // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
    /// <summary>
    /// Class used to provide information for fixed fields within a <see cref="ViewBase"/>
    /// </summary>
    internal class FixedFieldInfo : DependencyObject
    {
        #region Member Variables

        private TranslateTransform[]	_transforms;
		private FrameworkElement		_fixedAreaElement;

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        // To support paging, we need to know the amount of space that is
        // available.
        //
        private WeakList<VirtualizingDataRecordCellPanel> _cellPanels;

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="FixedFieldInfo"/>
        /// </summary>
        public FixedFieldInfo()
        {
        } 
        #endregion //Constructor

        #region Properties

        #region Public

        #region Extent

        /// <summary>
        /// Identifies the <see cref="Extent"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExtentProperty = DependencyProperty.Register("Extent",
            typeof(double), typeof(FixedFieldInfo), new FrameworkPropertyMetadata(0d));

        /// <summary>
        /// Returns or sets the total extent available for the elements.
        /// </summary>
        /// <seealso cref="ExtentProperty"/>
        //[Description("Returns or sets the total extent available for the elements.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public double Extent
        {
            get
            {
                return (double)this.GetValue(FixedFieldInfo.ExtentProperty);
            }
            set
            {
                this.SetValue(FixedFieldInfo.ExtentProperty, value);
            }
        }

        #endregion //Extent

		#region FixedAreaElement

		/// <summary>
		/// Returns/sets the outermost element that contains the Fixed area.
		/// </summary>
		public FrameworkElement FixedAreaElement
		{
			get	{ return this._fixedAreaElement; }
			set { this._fixedAreaElement = value; }
		}

		#endregion //FixedAreaElement

		#region Offset

		/// <summary>
        /// Identifies the <see cref="Offset"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register("Offset",
            typeof(double), typeof(FixedFieldInfo), new FrameworkPropertyMetadata(0d));

        /// <summary>
        /// Returns or sets the fixed scroll offset.
        /// </summary>
        /// <seealso cref="OffsetProperty"/>
        //[Description("Returns or sets the fixed scroll offset.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public double Offset
        {
            get
            {
                return (double)this.GetValue(FixedFieldInfo.OffsetProperty);
            }
            set
            {
                this.SetValue(FixedFieldInfo.OffsetProperty, value);
            }
        }

        #endregion //Offset

        #region ViewportExtent

        /// <summary>
        /// Identifies the <see cref="ViewportExtent"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ViewportExtentProperty = DependencyProperty.Register("ViewportExtent",
            typeof(double), typeof(FixedFieldInfo), new FrameworkPropertyMetadata(0d));

        /// <summary>
        /// Returns or sets the extent of the viewport of the fixed area.
        /// </summary>
        /// <seealso cref="ViewportExtentProperty"/>
        //[Description("Returns or sets the extent of the viewport of the fixed area.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public double ViewportExtent
        {
            get
            {
                return (double)this.GetValue(FixedFieldInfo.ViewportExtentProperty);
            }
            set
            {
                this.SetValue(FixedFieldInfo.ViewportExtentProperty, value);
            }
        }

        #endregion //ViewportExtent

        #endregion //Public
        
        #endregion //Properties

        #region Methods

        #region AddPanel
        internal void AddPanel(VirtualizingDataRecordCellPanel panel)
        {
            if (null == _cellPanels)
                _cellPanels = new WeakList<VirtualizingDataRecordCellPanel>();

            Debug.Assert(_cellPanels.Contains(panel) == false);
            _cellPanels.Add(panel);
        } 
        #endregion //AddPanel

        #region CreateTransform
        private TranslateTransform CreateTransform(FixedFieldLocation fixedLocation, bool isVertical)
        {
            TranslateTransform tt = new TranslateTransform();
            BindingBase binding;

            if (fixedLocation == FixedFieldLocation.FixedToFarEdge)
            {
                MultiBinding mb = new MultiBinding();
                mb.Bindings.Add(Utilities.CreateBindingObject(OffsetProperty, BindingMode.OneWay, this));
                mb.Bindings.Add(Utilities.CreateBindingObject(ViewportExtentProperty, BindingMode.OneWay, this));
                mb.Bindings.Add(Utilities.CreateBindingObject(ExtentProperty, BindingMode.OneWay, this));
                mb.Converter = FixedFarConverter.Instance;
                binding = mb;
            }
            else
            {
                Binding sb = Utilities.CreateBindingObject(OffsetProperty, BindingMode.OneWay, this);

                if (fixedLocation == FixedFieldLocation.Scrollable)
                    sb.Converter = NegateConverter.Instance;

                binding = sb;
            }

            DependencyProperty prop = isVertical ? TranslateTransform.YProperty : TranslateTransform.XProperty;
            BindingOperations.SetBinding(tt, prop, binding);
            return tt;
        } 
        #endregion //CreateTransform

        #region GetScrollableViewportExtent
        
        
        
        /// <summary>
        /// Returns the smallest extent for a page scroll operation based on the current viewport extent.
        /// </summary>
        internal double GetScrollableViewportExtent()
        {
            double viewport = this.ViewportExtent;
            double originalViewport = viewport;
            FrameworkElement fixedArea = this.FixedAreaElement;

            if (null != _cellPanels && null != fixedArea)
            {
                Window fixedAreaWindow = Window.GetWindow(fixedArea);

                for (int i = _cellPanels.Count - 1; i >= 0; i--)
                {
                    VirtualizingDataRecordCellPanel cellPanel = _cellPanels[i];

                    // if a panel was gc'd or has been removed but not gc'd yet remove it
                    if (null == cellPanel || fixedAreaWindow != Window.GetWindow(cellPanel))
                        _cellPanels.RemoveAt(i);
                    else
                        viewport = Math.Min(viewport, cellPanel.GetScrollableViewport(originalViewport, true));
                }
            }

            return viewport;
        }
        #endregion //GetScrollableViewportExtent

        #region GetTransform
        internal Transform GetTransform(FixedFieldLocation fixedLocation, bool isFixedAreaVertical)
        {
            if (null == _transforms)
                _transforms = new TranslateTransform[6];

            int index = (int)fixedLocation * 2;

            if (isFixedAreaVertical)
                index++;

            if (_transforms[index] == null)
                _transforms[index] = CreateTransform(fixedLocation, isFixedAreaVertical);

            return _transforms[index];
        }
        #endregion //GetTransform

        #region RemovePanel
        internal void RemovePanel(VirtualizingDataRecordCellPanel panel)
        {
            if (null != _cellPanels)
                _cellPanels.Remove(panel);
        } 
        #endregion //RemovePanel

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