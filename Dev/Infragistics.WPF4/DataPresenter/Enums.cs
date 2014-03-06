using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;

namespace Infragistics.Windows.DataPresenter
{

	#region AddNewRecordLocation

	/// <summary>
	/// Enum used for specifying the <see cref="FieldLayoutSettings"/>'s <see cref="FieldLayoutSettings.AddNewRecordLocation"/> property.
	/// </summary>
	/// <remarks>
	/// <seealso cref="FieldLayoutSettings.AddNewRecordLocation"/>
	/// </remarks>
	public enum AddNewRecordLocation
	{
		/// <summary>
		/// Default. The ultimate default is 'OnTopFixed'.
		/// </summary>
		Default = 0,

		/// <summary>
		/// An add record displays as the first record in a records collection. This record scrolls with its sibling records.
		/// </summary>
		OnTop = 1,

		/// <summary>
		/// An add record displays as the first record in a records collection. This record is fixed and does not scroll with its sibling records.
		/// </summary>
		OnTopFixed = 2,

		/// <summary>
		/// An add record displays as the last record in a records collection. This record scrolls with its sibling records.
		/// </summary>
		OnBottom = 3,

		/// <summary>
		/// An add record displays as the last record in a records collection. This record is fixed and does not scroll with its sibling records.
		/// </summary>
		OnBottomFixed = 4,
	};

	#endregion // AddNewRecordLocation

    // AS 4/8/09 NA 2009.2 ClipboardSupport
    #region AllowClipboardOperations
    /// <summary>
    /// Enumeration used to indicate which clipboard operations are allowed to be performed by the end user.
    /// </summary>
	/// <seealso cref="FieldLayoutSettings.AllowClipboardOperations"/>
    [Flags]
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public enum AllowClipboardOperations
    {
        /// <summary>
        /// No operations are allowed.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The values of cells may be removed and placed on the clipboard
        /// </summary>
        Cut = 0x1,

        /// <summary>
        /// The values of cells may be copied to the clipboard
        /// </summary>
        Copy = 0x2,

        /// <summary>
        /// Information on the clipboard may be used to change the values of one or more cells. Note, this functionality requires use of the Clipboard class which requires UIPermissions of AllClipboard in order to read from the clipboard.
        /// </summary>
        Paste = 0x4,

        /// <summary>
        /// The values of one or more cells can be removed
        /// </summary>
        ClearContents = 0x8,

        /// <summary>
        /// All operations are allowed.
        /// </summary>
        All = -1,
    }
    #endregion //AllowClipboardOperations

    // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
    #region AllowFieldFixing
    /// <summary>
    /// Indicates whether the <see cref="Field.FixedLocation"/> may be changed by the end user and if so, to which edges the field may be fixed.
    /// </summary>
    /// <seealso cref="FieldSettings.AllowFixing"/>
    public enum AllowFieldFixing
    {
        /// <summary>
        /// The default is resolved to 'No'
        /// </summary>
        Default,

        /// <summary>
        /// The field�s fixed state may not be changed.
        /// </summary>
        No,

        /// <summary>
        /// The field may be fixed to the near edge. In a horizontal layout, the field elements will be displayed on top when fixed and in a vertical layout, the field elements will be displayed on left when fixed.
        /// </summary>
        Near,

        /// <summary>
        /// The field may be fixed to the far edge. In a horizontal layout, the field elements will be displayed on bottom when fixed and in a vertical layout, the field elements will be displayed on right when fixed.
        /// </summary>
        Far,

        /// <summary>
        /// The field may be fixed to the near or far edge.
        /// </summary>
        NearOrFar,
    }
    #endregion //AllowFieldFixing

	#region AllowFieldHiding

	// SSP 8/21/09 - NAS9.2 Field Chooser
	// 
	/// <summary>
	/// Enum used for specifying the FieldSettings' <see cref="FieldSettings.AllowHiding"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public enum AllowFieldHiding
	{
		/// <summary>
		/// Default is resolved to <b>Always</b> if field chooser user interface is enabled. 
		/// If the field chooser UI is not enabled then the user is not allowed to
		/// change the visibility of the field.
		/// </summary>
		Default,

		/// <summary>
		/// The user cannot hide or show the field. The field is not displayed in the
		/// field chooser control.
		/// </summary>
		Never,

		/// <summary>
		/// The user can hide or show the field via field chooser, including by dragging 
		/// from the field chooser into the data presenter to show the field and by 
		/// dragging the field from the data presenter into the field chooser to hide it.
		/// </summary>
		ViaFieldChooserOnly,

		/// <summary>
		/// In addition to what's allowed by the <b>ViaFieldChooserOnly</b> option, the 
		/// user can hide the field by dragging it from the data presenter and dropping
		/// outside of the data presenter. A cursor indicating that the field is going
		/// to be removed from the data presenter is displayed and upon releasing the
		/// mouse, the field is removed from the data presenter. The user can show the
		/// field back into the data presenter via the field chooser control.
		/// </summary>
		Always
	}

	#endregion // AllowFieldHiding

	// JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
    #region AllowRecordFixing

    /// <summary>
    /// Indicates whether the <see cref="Record.FixedLocation"/> may be changed by the end user and if so, to which edges the record may be fixed.
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.AllowRecordFixing"/>
    /// <seealso cref="FieldLayout.AllowRecordFixingResolved"/>
    /// <seealso cref="Record.FixedLocation"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
    public enum AllowRecordFixing
    {
        /// <summary>
        /// The default is resolved to 'No'
        /// </summary>
        Default,

        /// <summary>
        /// The record�s fixed state may not be changed.
        /// </summary>
        No,

        /// <summary>
        /// The record may be fixed to the top edge. 
        /// </summary>
        Top,

        /// <summary>
        /// The record may be fixed to the bottom edge. 
        /// </summary>
        Bottom,

        /// <summary>
        /// The record may be fixed to the top or bottom edge.
        /// </summary>
        TopOrBottom,
    }
    #endregion //AllowRecordFixing

    #region AllowFieldMoving


    
	
	/// <summary>
	/// Enum used for specifying the <see cref="FieldLayoutSettings"/>'s <see cref="FieldLayoutSettings.AllowFieldMoving"/> property.
	/// </summary>
	/// <remarks>
	/// <seealso cref="FieldLayoutSettings.AllowFieldMoving"/>
	/// </remarks>
	public enum AllowFieldMoving
	{
		/// <summary>
		/// Default is resolved to <b>Yes</b>.
		/// </summary>
		Default				= 0,

		/// <summary>
		/// The user is allowed to move fields.
		/// </summary>
		Yes					= 1,

		/// <summary>
		/// The user is not allowed to move fields.
		/// </summary>
		No					= 2,

		/// <summary>
		/// The user is allowed to move fields within the same logical row.
		/// </summary>
		WithinLogicalRow	= 3,

		/// <summary>
		/// The user is allowed to move fields within the same logical column.
		/// </summary>
		WithinLogicalColumn	= 4
	}

    #endregion // AllowFieldMoving

    #region AutoArrangeCells

    /// <summary>
	/// Determines how the cells laid out when generating item and label styles
	/// </summary>
	public enum AutoArrangeCells
	{
		/// <summary>
		/// Either LeftToRight or TopToBottom based on the panel type
		/// </summary>
		Default = 0,
		/// <summary>
		/// Use each Field's explicit <see cref="Field.Row"/> and <see cref="Field.Column"/> settings only. Ignore <see cref="FieldLayoutSettings.AutoArrangeMaxRows"/>, <see cref="FieldLayoutSettings.AutoArrangeMaxColumns"/> and <see cref="FieldLayoutSettings.AutoArrangePrimaryFieldReservation"/> settings.
		/// </summary>
		Never = 1,
		/// <summary>
		/// Automatically flow the cells left to right first then top to bottom ignoring <see cref="Field.Row"/> and <see cref="Field.Column"/> settings.
		/// </summary>
		LeftToRight = 2,
		/// <summary>
		/// Automatically flow the cells top to bottom first then left to right ignoring <see cref="Field.Row"/> and <see cref="Field.Column"/> settings.
		/// </summary>
		TopToBottom = 3,
	}

	#endregion //AutoArrangeCells

	#region AutoArrangePrimaryFieldReservation

	/// <summary>
	/// Determines if the first row, column or cell is reserved for the primary field
	/// </summary>
	public enum AutoArrangePrimaryFieldReservation
	{
		/// <summary>
		/// Use the default specified at a higher level
		/// </summary>
		Default = 0,
		/// <summary>
		/// Don't reserve a slot for the primary field.
		/// </summary>
		None = 1,
		/// <summary>
		/// Reserve the entire first row for the primary field.
		/// </summary>
		ReserveFirstRow = 2,
		/// <summary>
		/// Reserve the entire first column for the primary field.
		/// </summary>
		ReserveFirstColumn = 3,
		/// <summary>
		/// Reserve the entire first row or column for the primary field based on the flow setting.
		/// </summary>
		ReserveFirstRowOrColumnBasedOnFlow = 4,
		/// <summary>
		/// Reserve the first cell for the primary field.
		/// </summary>
		ReserveFirstCell = 5,
	}

	#endregion //AutoArrangePrimaryFieldReservation

	// JM 10/20/09 NA 2010.1 CardView
	#region AutoFitCards
	/// <summary>
	/// Enumeration that determines whether and how Cards are auto-sized to used up all available horizontal/vertical space.
	/// </summary>
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
	public enum AutoFitCards
	{
		/// <summary>
		/// Cards are not automatically resized to use up all available space.
		/// </summary>
		None,

		/// <summary>
		/// Cards are automatically resized to use up all available horizontal space.
		/// </summary>
		Horizontally,

		/// <summary>
		/// Cards are automatically resized to use up all available vertical space.
		/// </summary>
		Vertically,

		/// <summary>
		/// Cards are automatically resized to use up all available horizontal and vertical space.
		/// </summary>
		HorizontallyAndVertically,
	}
	#endregion //AutoFitCards

	// AS 6/9/09 NA 2009.2 Field Sizing
	#region AutoFitMode
	/// <summary>
	/// Enumeration whether fields are resized to always fill the available area of the DataPresenter and if so, which fields are resized.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
	public enum AutoFitMode
	{
		/// <summary>
		/// Defaults to ResizeAllFields when <see cref="DataPresenterBase.AutoFit"/> is true, to ResizeStarFields if <see cref="DataPresenterBase.AutoFit"/> is null and to None if <see cref="DataPresenterBase.AutoFit"/> is false.
		/// </summary>
		Default,

		/// <summary>
		/// The fields are not automatically resized based on the viewable area of the <see cref="DataPresenterBase"/>
		/// </summary>
		Never,

		/// <summary>
		/// The field layout is always in autofit mode and all fields are resized.
		/// </summary>
		Always,

		/// <summary>
		/// The field layout participates in autofit mode as long as there is one visible field with an explicit star extent.
		/// </summary>
		OnlyWithVisibleStarFields,

		/// <summary>
		/// Only the fields in the last logical column are resized. For example, in a vertical orientation the right most fields are resized.
		/// </summary>
		ExtendLastField,
	} 
	#endregion //AutoFitMode

	// AS 6/22/09 NA 2009.2 Field Sizing
	#region AutoFitState
	[Flags]
	internal enum AutoFitState : byte
	{
		None,
		Width = 0x1,
		Height = 0x2,
		WidthAndHeight = Width | Height,
	} 
	#endregion //AutoFitState

	// AS 8/5/09 NA 2009.2 Field Sizing
	#region AutoSizeCalculationFlags enum
	[Flags()]
	internal enum AutoSizeCalculationFlags
	{
		None = 0x0,

		/// <summary>
		/// Ignore the edit mode cell when calculating
		/// </summary>
		SkipEditCell = 0x1,

		/// <summary>
		/// Always recalculate even if the cell/record version doesn't require it
		/// </summary>
		RecalculateRecords = 0x2,

		// AS 8/10/11 TFS83904
		/// <summary>
		/// When calculating the size for records in view we can use the template record as a fallback if there is no cell element to use.
		/// </summary>
		UseTemplateRecordAsFallback = 0x4,
	}
	#endregion //AutoSizeCalculationFlags enum

	#region BindingRetentionMode Enum

	// SSP 3/22/10 - Optimizations
	// 
	/// <summary>
	/// Used for specifying UnboundField's <see cref="UnboundField.BindingRetentionMode"/> property.
	/// </summary>
	public enum BindingRetentionMode
	{
		/// <summary>
		/// Default. Bindings are created as necessary when unbound field's value is accessed and then they are discarded to preserve memory.
		/// </summary>
		AutoRelease,

		/// <summary>
		/// Bindings are created as necessary when unbound field's value is accessed and they are retained so when the unbound 
		/// field's value is accessed again in the future, the previously created binding object will be reused and there won't 
		/// be a need for re-creating the binding object.
		/// </summary>
		Retain
	}

	#endregion // BindingRetentionMode Enum

	#region CalculationScope

	
	
	/// <summary>
	/// Used for specifying whether to include all records in unsorted order, all records in sorted order
	/// or only the filtered records for calculating summaries.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>CalculationScope</b> is used for specifying <see cref="FieldLayoutSettings.CalculationScope"/> 
	/// property. It specifies which records and in which order to traverse for summary calculation purposes.
	/// <b>FullUnsortedList</b> will calculate summaries based on all records and the field values will
	/// be aggregated into the summary calculation in the order in which it appears in the underlying data 
	/// list. <b>FullSortedList</b> will traverse field values in the order they are in the data grid. This
	/// only makes a difference if the calculation relies on the order of values. <b>FilteredSortedList</b>
	/// will only use field values of visible records.
	/// </para>
	/// <seealso cref="FieldLayoutSettings.CalculationScope"/>
	/// </remarks>
	public enum CalculationScope
	{
		/// <summary>
		/// Default is resolved to <b>FilteredSortedList</b>.
		/// </summary>
		Default				= 0,

		/// <summary>
		/// All records are used and the order in which the records are processed 
		/// corresponds to their order in which their associated data items appear 
		/// in the underlying data source list.
		/// </summary>
		FullUnsortedList	= 1,

		/// <summary>
		/// All records are used and they are processed in sorted order.
		/// </summary>
		FullSortedList		= 2,

		/// <summary>
		/// Only visible (viewable) records are processed and they are processed in 
		/// their sorted order.
		/// </summary>
		FilteredSortedList	= 3
	}

	#endregion // CalculationScope

	#region CellClickAction

	/// <summary>
	/// Determines what happens when the user clicks on a <see cref="Infragistics.Windows.DataPresenter.Field"/>'s cell
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.CellClickAction"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field.CellClickActionResolved"/>
	public enum CellClickAction
	{
		/// <summary>
		/// Use the default setting
		/// </summary>
		Default = 0,
		/// <summary>
		/// The cell is selected
		/// </summary>
		SelectCell = 1,
		/// <summary>
		/// The cell's record is slected
		/// </summary>
		SelectRecord = 2,
		/// <summary>
		/// If the cell allows editing then edit mode is entered otherwise the cell is selected
		/// </summary>
		EnterEditModeIfAllowed = 3,
	}

	#endregion CellClickAction

	// JJD 5/22/07 - Added
	#region CellContainerGenerationMode enum

	/// <summary>
	/// Determines how cell containers are generated and cached by the RecyclingItemsPanel.
	/// </summary>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="DataPresenterBase.CellContainerGenerationMode"/>
	public enum CellContainerGenerationMode
	{
		/// <summary>
		/// Reuses cell containers of cells that get scrolled out of view for the cells that get scrolled into view.
		/// </summary>
		Recycle = 0,
		/// <summary>
		/// Generates cell containers for cells as they are scrolled into view and does not clear them for cells that get scrolled out of view.
		/// </summary>
		LazyLoad = 2,
		/// <summary>
		/// Pre-generates and caches an cell container for each cell.
		/// </summary>
		PreLoad = 3
	}

	#endregion //CellContainerGenerationMode enum

	#region CellContentAlignment

	/// <summary>
	/// Determines the position of a label relative to its cell
	/// </summary>
	public enum CellContentAlignment
	{
		/// <summary>
		/// The default based on other settings
		/// </summary>
		Default = 0,
		/// <summary>
		/// Only show the label - hide the cell value
		/// </summary>
		LabelOnly = 1,
		/// <summary>
		/// Only show the cell value - hide the label
		/// </summary>
		ValueOnly = 2,
		/// <summary>
		/// Label above cell value on left
		/// </summary>
		LabelAboveValueAlignLeft = 3,
		/// <summary>
		/// Label above cell value in center
		/// </summary>
		LabelAboveValueAlignCenter = 4,
		/// <summary>
		/// Label above cell value on right
		/// </summary>
		LabelAboveValueAlignRight = 5,
		/// <summary>
		/// Label above cell value - stretch to same width
		/// </summary>
		LabelAboveValueStretch = 6,
		/// <summary>
		/// Label below cell value on left
		/// </summary>
		LabelBelowValueAlignLeft = 7,
		/// <summary>
		/// Label below cell value in center
		/// </summary>
		LabelBelowValueAlignCenter = 8,
		/// <summary>
		/// Label below cell value on right
		/// </summary>
		LabelBelowValueAlignRight = 9,
		/// <summary>
		/// Label below cell value - stretch to same width
		/// </summary>
		LabelBelowValueStretch = 10,
		/// <summary>
		/// Label left of cell value on top 
		/// </summary>
		LabelLeftOfValueAlignTop = 11,
		/// <summary>
		/// Label left of cell value in middle
		/// </summary>
		LabelLeftOfValueAlignMiddle = 12,
		/// <summary>
		/// Label left of cell value on bottom
		/// </summary>
		LabelLeftOfValueAlignBottom = 13,
		/// <summary>
		/// Label left of cell value - stretch to same height
		/// </summary>
		LabelLeftOfValueStretch = 14,
		/// <summary>
		/// Label right of cell value on top 
		/// </summary>
		LabelRightOfValueAlignTop = 15,
		/// <summary>
		/// Label right of cell value in middle
		/// </summary>
		LabelRightOfValueAlignMiddle = 16,
		/// <summary>
		/// Label right of cell value on bottom
		/// </summary>
		LabelRightOfValueAlignBottom = 17,
		/// <summary>
		/// Label right of cell value - stretch to same height
		/// </summary>
		LabelRightOfValueStretch = 18,
	}

	#endregion //CellContentAlignment
    
    // JJD 5/29/09 - TFS18063 - added
    #region CellValueType

    /// <summary>
    /// Specified what type of value to return
    /// </summary>
    /// <seealso cref="DataRecord.GetCellValue(Field, CellValueType)"/>
    /// <seealso cref="Field.Converter"/>
    public enum CellValueType
    {
        /// <summary>
        /// The raw (unconverted) value from the cell. This is equivalent to calling <see cref="DataRecord.GetCellValue(Field)"/>.
        /// </summary>
        Raw = 0,

        /// <summary>
        /// The cell's value after it has been converted by the <see cref="Field"/>'s <see cref="Field.Converter"/>.  This is equivalent to calling <see cref="DataRecord.GetCellValue(Field, bool)"/> with true.
        /// </summary>
        Converted = 1,

        /// <summary>
        /// The cell's converted value after it has then been converted to the specified <see cref="FieldSettings.EditAsType"/>
        /// </summary>
        EditAsType = 2
    }

    #endregion //CellValueType	
    
	#region CellPresentation

	/// <summary>
	/// CellPresentation
	/// </summary>
	public enum CellPresentation
	{
		/// <summary>
		/// A GridView presention
		/// </summary>
		GridView = 0,
		/// <summary>
		/// A card view presentation
		/// </summary>
		CardView = 1,
	}

	#endregion //CellPresentation

	#region CellTemplateType

	internal enum CellTemplateType
	{
		ValueOnly = 0,
		LabelOnly = 1,
		ValueAndLabel = 2,
	}

	#endregion CellTemplateType

	// AS 5/12/09 NA 2009.2 Undo/Redo
	#region CellValueProviderSource
	internal enum CellValueProviderSource
	{
		Unknown,
		SelectedRecords,
		SelectedCells,
		SelectedFields,
		ActiveRecord,
		ActiveCell,
	}
	#endregion //CellValueProviderSource

	// MD 5/26/10 - ChildRecordsDisplayOrder feature
	#region ChildRecordsDisplayOrder

	/// <summary>
	/// Specifies the order in which child records should be displayed relative to their parent record.
	/// </summary>
	public enum ChildRecordsDisplayOrder
	{
		/// <summary>
		/// The default based on other settings.
		/// </summary>
		Default,

		/// <summary>
		/// The child records should be displayed after their parent record when the parent is expanded.
		/// </summary>
		AfterParent,

		/// <summary>
		/// The child records should be displayed before their parent record when the parent is expanded.
		/// If the parent record has a header before it when collapsed, the header will appear before the child 
		/// records and their header.
		/// </summary>
		BeforeParent,

		// MD 7/30/10 - ChildRecordsDisplayOrder header placement
		/// <summary>
		/// The child records should be displayed before their parent record when the parent is expanded.
		/// If the parent record has a header before it when collapsed, the header will remain attached to the
		/// parent record and the child records will appear before the header. Note: When nested panels are used,
		/// the headers must go before the child records, so this will be the same as the BeforeParent value.
		/// </summary>
		BeforeParentHeadersAttached,
	} 

	#endregion // ChildRecordsDisplayOrder

	// AS 4/20/09 NA 2009.2 ClipboardSupport
    #region ClipboardError
    /// <summary>
    /// Enumeration used to identify the type of clipboard error that occurred.
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ClipboardOperationErrorEventArgs.Error"/>
    [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
    public enum ClipboardError
    {
        /// <summary>
        /// The <see cref="DataPresenterBase.SelectedItems"/> has a mixture of selected item types - e.g. has some cells selected and some fields.
        /// </summary>
        MixedSelection,

        /// <summary>
        /// The operation requires that the cells of the same fields are selected in all the records which contain selection.
        /// </summary>
        NonRectangularSelection,

		/// <summary>
		/// The information to be pasted has more column/field information than the amount of available from the origin of the paste operation.
		/// </summary>
		NotEnoughColumns,

		/// <summary>
		/// The information to be pasted has more row/record information than the amount of available from the origin of the paste operation.
		/// </summary>
		NotEnoughRows,

		/// <summary>
		/// The target cell does not allow editing and therefore the value cannot be changed.
		/// </summary>
		ReadOnlyCell,

		/// <summary>
		/// Unable to convert the display value to the target cell EditAsType.
		/// </summary>
		ConversionError,

		/// <summary>
		/// The value was determined to be invalid with the constraints of the editor.
		/// </summary>
		ValidationError,

		/// <summary>
		/// An error occurred while attempting to modify the value of a cell
		/// </summary>
		SetCellValueError,

		/// <summary>
		/// An error occurred while attempting to update a record
		/// </summary>
		UpdateRecordError,

		/// <summary>
		/// An error occurred while attempting to insert a record or the insertion was cancelled.
		/// </summary>
		InsertRecordError,

		/// <summary>
        /// An exception occurred during the operation.
        /// </summary>
        Exception,
    } 
    #endregion //ClipboardError

    // AS 4/14/09 NA 2009.2 ClipboardSupport
    #region ClipboardErrorAction

    /// <summary>
    /// Used for specifying the <see cref="Infragistics.Windows.DataPresenter.Events.ClipboardOperationErrorEventArgs.Action"/> property.
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ClipboardOperationErrorEventArgs.Action"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public enum ClipboardErrorAction
    {
        /// <summary>
        /// Continue the operation with remaining cells. This option leaves the value of the cell to
        /// its current value.
        /// </summary>
        Continue,

        /// <summary>
        /// Continue the operation with remaining cells. This option clears out the value of the cell.
        /// </summary>
        ClearCellAndContinue,

        /// <summary>
        /// Stop the operation.
        /// </summary>
        Stop,

        /// <summary>
        /// Stop the operation and revert the values of the cells already modified by the operation.
        /// </summary>
        Revert
    }

    #endregion // ClipboardErrorAction

    // AS 4/8/09 NA 2009.2 ClipboardSupport
    #region ClipboardOperation
    /// <summary>
    /// Enumeration used to identify a type of clipboard operation.
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ClipboardOperationErrorEventArgs.Operation"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public enum ClipboardOperation
    {
        /// <summary>
        /// The values from the cells will be removed and placed on the clipboard.
        /// </summary>
        Cut,

        /// <summary>
        /// The values from the cells will be placed on the clipboard.
        /// </summary>
        Copy,

        /// <summary>
        /// The information in the clipboard will be used to update the values of one or more cells.
        /// </summary>
        Paste,

        /// <summary>
        /// The values from the cell will be cleared.
        /// </summary>
        ClearContents,
    }
    #endregion //ClipboardOperation

    #region CustomizationType

	// SSP 8/27/08
	// 
	/// <summary>
	/// Specifies the type of user customization.
	/// </summary>
	/// <seealso cref="DataPresenterBase.SaveCustomizations()"/>
	/// <seealso cref="DataPresenterBase.LoadCustomizations(string)"/>
	/// <seealso cref="DataPresenterBase.ClearCustomizations(CustomizationType)"/>
	[Flags]
	public enum CustomizationType
	{
		/// <summary>
		/// None
		/// </summary>
		None				= 0x0,

		/// <summary>
		/// Grouping and sorting
		/// </summary>
		GroupingAndSorting	= 0x1,

		/// <summary>
		/// Width or height of the field depending on the logical orientation
		/// </summary>
		FieldExtent			= 0x2,

		/// <summary>
		/// Field position
		/// </summary>
		FieldPosition		= 0x4,

		// SSP 2/4/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Record filters
		/// </summary>
		RecordFilters		= 0x8,

		// SSP 9/8/09 TFS18172
		// Added support for saving and loading summaries.
		// 
		/// <summary>
		/// Summaries
		/// </summary>
		Summaries			= 0x10,

		// JM NA 10.1 CardView
		/// <summary>
		/// CardViewSettings
		/// </summary>
		CardViewSettings = 0x20,

		/// <summary>
		/// All
		/// </summary>
		All					= 0x7fffffff
	}

	#endregion // CustomizationType

	#region DataChangeType

	
	
	/// <summary>
	/// Enum used for passing along context information to RecordManager's OnDataChanged method.
	/// </summary>
	internal enum DataChangeType
	{
		Reset,
		CellDataChange,
		RecordDataChange,
		AddRecord,
		RemoveRecord,
		ItemMoved,
		Unknown
	}

	#endregion // DataChangeType

	#region DataDisplayMode

	/// <summary>
	/// The modes in which data can be displayed.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase.SupportedDataDisplayMode"/>
	public enum DataDisplayMode
	{
		/// <summary>
		/// Flat data only.
		/// </summary>
		Flat = 0,

		/// <summary>
		/// Hierarchical data.
		/// </summary>
		Hierarchical = 1,

        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        /// <summary>
        /// Hierachical data flattened into a single list of records
        /// </summary>
        FlattenedHierarchical = 2,
	}

	#endregion DataDisplayMode

	#region DataErrorOperation

	/// <summary>
	/// Identifies the operation that was attempted and which triggered a data error.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.DataErrorEventArgs"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataError"/>
	public enum DataErrorOperation
	{
		/// <summary>
		/// Attempted to get a cell's value
		/// </summary>
		CellValueGet = 0,
		/// <summary>
		/// Attempted to set a cell's value
		/// </summary>
		CellValueSet = 1,
		/// <summary>
		/// Attempted to commit pending changes on a record 
		/// </summary>
		RecordUpdate = 2,
		/// <summary>
		/// Attempted to add a new record
		/// </summary>
		RecordAdd = 3,
		/// <summary>
		/// Attempted to delete a record
		/// </summary>
		RecordDelete = 4,
		/// <summary>
		/// Another unspecified operation
		/// </summary>
		Other = 4,
	}

	#endregion //DataErrorOperation

	#region DataErrorDisplayMode

	// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> property.
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
	/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
	/// <seealso cref="SupportDataErrorInfo"/>
	public enum DataErrorDisplayMode
	{
		/// <summary>
		/// Default is resolved to <i>ErrorIcon</i>.
		/// </summary>
		Default,

		/// <summary>
		/// Data error ui will not be displayed. This is useful if you want to provide your own
		/// ui indication for data errors and prevent the data presenter from displaying ui errors.
		/// </summary>
		None,

		/// <summary>
		/// When a cell has data error, an error icon is displayed in the cell 
		/// and when the record has data error, an error icon is displayed in the record selector.
		/// </summary>
		ErrorIcon,
		
		/// <summary>
		/// When a cell has data error, the cell is highlighted with a color
		/// and when the record has data error, a vertical line is displayed on the right side 
        /// of the record selector.
		/// </summary>
		Highlight,

		/// <summary>
		/// Both the error icon and highlighting mechanism will be used to display data errors.
		/// </summary>
		ErrorIconAndHighlight
	}

	#endregion // DataErrorDisplayMode

	#region DataRecordSizingMode

	/// <summary>
	/// Enum associated with <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.DataRecordSizingMode"/> property.
	/// </summary>
	public enum DataRecordSizingMode
	{
		/// <summary>
		/// Default
		/// </summary>
		/// <remarks>The ultimate default is 'SizedToContentAndFixed'.</remarks>
		Default = 0,

		/// <summary>
		/// All <see cref="DataRecord"/>s are the same size and cannot be resized.
		/// </summary>
		Fixed = 1,

		/// <summary>
		/// All <see cref="DataRecord"/>s are initialized to the same size but can be resized individually.
		/// </summary>
		IndividuallySizable = 2,

		/// <summary>
		/// All <see cref="DataRecord"/>s are the sized to their content and cannot be resized.
		/// </summary>
		/// <remarks>This is the ultimate default and results in multiline text in cells of type string by default.</remarks>
		SizedToContentAndFixed = 3,

		/// <summary>
		/// All <see cref="DataRecord"/>s are the sized to their content and cannot be resized.
		/// </summary>
		/// <remarks>This results in multiline text in cells of type string by default.</remarks>
		SizedToContentAndIndividuallySizable = 4,

		/// <summary>
		/// All <see cref="DataRecord"/>s are initialized to the same size but can be resized individually.
		/// </summary>
		SizableSynchronized = 5,
	}

	#endregion // DataRecordSizingMode

	#region DataSourceResetBehavior Enum

	// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
	// 
	/// <summary>
	/// Used for specifying <see cref="DataPresenterBase.DataSourceResetBehavior"/> property.
	/// </summary>
	/// <seealso cref="DataPresenterBase.DataSourceResetBehavior"/>
	/// <seealso cref="DataPresenterBase.RecordLoadMode"/>
	public enum DataSourceResetBehavior
	{
		/// <summary>
		/// Reuse data records by checking to see if the underlying data item still exists in the data list
		/// by looping through the data list. Note that the data list is looped through only once for all
		/// data records.
		/// </summary>
		ReuseRecordsViaEnumerator = 0,
		
		/// <summary>
		/// Reuse data records by checking to see if the underlying data item still exists in the data list
		/// using IndexOf call on the data list.
		/// </summary>
		ReuseRecordsViaIndexOf = 1,

		/// <summary>
		/// Discards the existing records. <b>Note</b> that as a result any active or selected 
		/// records/cells will be discarded as well.
		/// </summary>
		DiscardExistingRecords = 2
	}

	#endregion // DataSourceResetBehavior Enum

	// JM 6/12/09 NA 2009.2 DataValueChangedEvent
	#region DataValueChangedScope
	/// <summary>
	/// Enumeration used to indicate the range of <see cref="DataRecord"/>s for which <see cref="DataPresenterBase.DataValueChanged"/> and <see cref="DataPresenterBase.InitializeCellValuePresenter"/> events will be raised.
	/// </summary>
	/// <seealso cref="FieldSettings.DataValueChangedScope"/>
	/// <seealso cref="DataPresenterBase.DataValueChanged"/>
	/// <seealso cref="DataPresenterBase.InitializeCellValuePresenter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
	public enum DataValueChangedScope
	{
		/// <summary>
		/// A default value will be resolved and used.
		/// </summary>
		Default = 0,

		/// <summary>
		/// All allocated <see cref="DataRecord"/>s will be included.
		/// </summary>
		AllAllocatedRecords = 1,

		/// <summary>
		/// Only <see cref="DataRecord"/>s that are in view will be included.
		/// </summary>
		OnlyRecordsInView = 2,
	}
	#endregion //DataValueChangedScope

	#region ExpandableFieldRecordExpansionMode

	/// <summary>
	/// Enum associated with <see cref="Infragistics.Windows.DataPresenter.FieldSettings.ExpandableFieldRecordExpansionMode"/> property.
	/// </summary>
	public enum ExpandableFieldRecordExpansionMode
	{
		/// <summary>
		/// Default
		/// </summary>
		/// <remarks>The ultimate default is 'ShowExpansionIndicatorIfSiblingsExist'.</remarks>
		Default = 0,
		/// <summary>
		/// When the parent record is expanded always show the expanded contents of the <see cref="ExpandableFieldRecord"/> without an expansion indicator.
		/// </summary>
		ExpandAlways = 1,
		/// <summary>
		/// When the parent record is expanded show the <see cref="ExpandableFieldRecord"/> with an expansion indicator.
		/// </summary>
		ShowExpansionIndicator = 2,
		/// <summary>
		/// When the parent record is expanded show the <see cref="ExpandableFieldRecord"/> with an expansion indicator if there is more than one sibling ExpandableFieldRecord. Otherwise show the expanded content without an expansion indicator 
		/// </summary>
		ShowExpansionIndicatorIfSiblingsExist = 3,
	}

	#endregion ExpandableFieldRecordExpansionMode

	#region ExpandableFieldRecordHeaderDisplayMode

	/// <summary>
	/// Enum associated with <see cref="Infragistics.Windows.DataPresenter.FieldSettings.ExpandableFieldRecordHeaderDisplayMode"/> property.
	/// </summary>
	public enum ExpandableFieldRecordHeaderDisplayMode
	{
		/// <summary>
		/// Default
		/// </summary>
		/// <remarks>The ultimate default is 'AlwaysDisplayHeader'.</remarks>
		Default = 0,
		/// <summary>
		/// Display the <see cref="ExpandableFieldRecord"/> header whether its expanded or collapsed.
		/// </summary>
		AlwaysDisplayHeader = 1,
		/// <summary>
		/// Never display the <see cref="ExpandableFieldRecord"/> header.
		/// </summary>
		NeverDisplayHeader = 2,
		/// <summary>
		/// Only display the <see cref="ExpandableFieldRecord"/> header when it is expanded.
		/// </summary>
		DisplayHeaderOnlyWhenExpanded = 3,
		/// <summary>
		/// Only display the <see cref="ExpandableFieldRecord"/> header when it is collapsed.
		/// </summary>
		DisplayHeaderOnlyWhenCollapsed = 4,
	}

	#endregion ExpandableFieldRecordHeaderDisplayMode

    // JJD 4/28/08 - BR31406 and BR31707 - added
    #region ExpansionIndicatorDisplayMode

    /// <summary>
    /// Enum used for specifying the FieldLayoutSettings' <see cref="FieldLayoutSettings.ExpansionIndicatorDisplayMode"/> property.
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.ExpansionIndicatorDisplayMode"/>
    /// <seealso cref="Record.ExpansionIndicatorVisibility"/>
    public enum ExpansionIndicatorDisplayMode
    {
        /// <summary>
        /// Use Default.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Always display an expansion indicator.
        /// </summary>
        Always = 1,

        /// <summary>
        /// Never display an expansion indicator
        /// </summary>
        Never = 2,

        /// <summary>
        /// When the record is first displayed check to see if there are any child records and only display an expansion indicator 
        /// if there are.
        /// </summary>
        CheckOnDisplay = 3,

        /// <summary>
        /// After the record is expanded, check to see if there are any child records.
        /// If not, the indicator is hidden.
        /// </summary>
        CheckOnExpand = 4
    };

    #endregion // ExpansionIndicatorDisplayMode

	// AS 6/9/09 NA 2009.2 Field Sizing
	#region FieldAutoSizeOptions
	/// <summary>
	/// Enumeration used to determine what values are considered when autosizing a field.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
	[Flags]
	public enum FieldAutoSizeOptions
	{
		/// <summary>
		/// The field does not support autosizing
		/// </summary>
		None = 0x0,

		/// <summary>
		/// The size of the LabelPresenter is included in the size calculation
		/// </summary>
		Label = 0x1,

		/// <summary>
		/// The cells of data records are included in the size calculation
		/// </summary>
		DataCells = 0x2,

		/// <summary>
		/// The summaries aligned with the field are included in the size calculation
		/// </summary>
		Summaries = 0x4,

		/// <summary>
		/// The filter cells are included in the size calculation
		/// </summary>
		FilterCells = 0x8,

		/// <summary>
		/// All elements associated with the field are considered in the size calculation
		/// </summary>
		All = -1,
	}
	#endregion //FieldAutoSizeOptions

	// AS 6/22/09 NA 2009.2 Field Sizing
	#region FieldAutoSizeScope
	/// <summary>
	/// Enumeration used to indicate what records are considered during an auto size operation for a field.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
	public enum FieldAutoSizeScope
	{
		/// <summary>
		/// By default only the records in view are considered.
		/// </summary>
		Default,

		/// <summary>
		/// Only the records currently in view are considered.
		/// </summary>
		RecordsInView,

		/// <summary>
		/// All records that may be displayed to the end user. For example, the children of a collapsed records and hidden records are ignored. This option will force allocation of records when an autosize operation is performed.
		/// </summary>
		ViewableRecords,

		/// <summary>
		/// All records are considered in the calculation. This option will force allocation of records when an autosize operation is to be performed.
		/// </summary>
		AllRecords,
	} 
	#endregion //FieldAutoSizeScope

	#region FieldChooserDisplayOrder

	// SSP 6/3/09 - NAS9.2 Field Chooser
	// 

	/// <summary>
	/// Used to specify <see cref="FieldChooser.FieldDisplayOrder"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public enum FieldChooserDisplayOrder
	{
		/// <summary>
		/// Fields are displayed in alphabetical order of the field captions.
		/// </summary>
		Alphabetical,

		/// <summary>
		/// Fields are displayed in the visible order as they appear in the data presenter.
		/// </summary>
		SameAsDataPresenter
	}

	#endregion // FieldChooserDisplayOrder

	#region FieldGroupByMode

	/// <summary>
	/// Enum used for specifying the <see cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByMode"/> property.
	/// </summary>
	public enum FieldGroupByMode
	{
		/// <summary>
		/// The default mode.
		/// </summary>
		Default,

		/// <summary>
		/// The groups are based on the date portion of DateTime values found in the column's cells.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Date,

		/// <summary>
		/// The groups are based on the first character of the cell text.
		/// This option is intended to be used when the column�s data type is String.
		/// </summary>
		FirstCharacter,

		/// <summary>
		/// The groups are based on the first two characters of the cell text.
		/// This option is intended to be used when the column�s data type is String.
		/// </summary>
		First2Characters,

		/// <summary>
		/// The groups are based on the first three characters of the cell text.
		/// This option is intended to be used when the column�s data type is String.
		/// </summary>
		First3Characters,

		/// <summary>
		/// The groups are based on the first four characters of the cell text.
		/// This option is intended to be used when the column�s data type is String.
		/// </summary>
		First4Characters,

		/// <summary>
		/// The groups are based on the date and hour portions of DateTime values found in the column's cells.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Hour,

		/// <summary>
		/// The groups are based on the date, hour, and minute portions of DateTime values found in the column's cells.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Minute,

		/// <summary>
		/// The groups are based on the year and month portions of DateTime values found in the column's cells.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Month,

		/// <summary>
		/// The groups are created in the same way that Outlook 2003 creates groups for dates.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		OutlookDate,

		/// <summary>
		/// The groups are based on the quarter and year that the cell values are in.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Quarter,

		/// <summary>
		/// The groups are based on the date, hour, minute, and second portions of DateTime values found in the column's cells.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Second,

		/// <summary>
		/// The groups are based on the text of the cells, not their values.
		/// </summary>
		Text,

		/// <summary>
		/// The groups are based on the value of the cells, not their text.
		/// Note, the description of each groupby row will use the text of the cell by default.
		/// </summary>
		Value,

		/// <summary>
		/// The groups are based on the year portion of DateTime values found in the column's cells.
		/// The column's <see cref="Infragistics.Windows.DataPresenter.Field.DataType"/> must be System.DateTime for this mode to work properly.
		/// </summary>
		Year
	}

	#endregion // FieldGroupByMode

	// AS 6/22/09 NA 2009.2 Field Sizing
	#region FieldLengthUnitType
	/// <summary>
	/// Indicates the type of <see cref="FieldLength.Value"/> represented by the <see cref="FieldLength"/>
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_FieldSizing)]
	public enum FieldLengthUnitType
	{
		/// <summary>
		/// The size is based on the contents indicated by the <see cref="FieldSettings.AutoSizeOptions"/> and increased as larger content becomes available.
		/// </summary>
		Auto,

		/// <summary>
		/// The size is an explicit pixel value.
		/// </summary>
		Pixel,

		/// <summary>
		/// The size is a weighted portion of the space remaining after arranging non-star fields.
		/// </summary>
		Star,

		/// <summary>
		/// The size is based on the content indicated by the <see cref="FieldSettings.AutoSizeOptions"/> at the time when the field is initialized.
		/// </summary>
		InitialAuto,
	} 
	#endregion //FieldLengthUnitType

    // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
    #region FieldPositionChangeReason
    /// <summary>
    /// Indicates what triggered the change in the field's position.
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Events.FieldPositionChangingEventArgs.ChangeReason"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Events.FieldPositionChangedEventArgs.ChangeReason"/>
    public enum FieldPositionChangeReason
    {
        /// <summary>
        /// The field has been dragged to a new location.
        /// </summary>
        Moved,

        /// <summary>
        /// The <see cref="Field.FixedLocation"/> has been changed to a fixed location.
        /// </summary>
        Fixed,

        /// <summary>
        /// The <see cref="Field.FixedLocation"/> has been changed from fixed to unfixed.
        /// </summary>
        Unfixed,

		// SSP 6/3/09 - NAS9.2 Field Chooser
		// Added Hidden and Displayed members.
		// 
		/// <summary>
		/// The user has hidden the field from the data presenter. The user can hide a field
		/// via <i>FieldChooser</i> or simply by dragging and dropping it outside
		/// of the data presenter when FieldChooser UI is enabled. See <see cref="FieldChooser"/>
		/// for more information.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		Hidden,

		// SSP 6/3/09 - NAS9.2 Field Chooser
		// Added Hidden and Displayed members.
		// 
		/// <summary>
		/// The user has displayed the field in the data presenter via a <see cref="FieldChooser"/>.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		Displayed
    } 
    #endregion //FieldPositionChangeReason

	#region FieldSortComparisonType

	/// <summary>
	/// Used by the <see cref="Infragistics.Windows.DataPresenter.FieldSettings.SortComparisonType"/> property.
	/// </summary>
	public enum FieldSortComparisonType
	{
		/// <summary>
		/// The default sort comparison type.
		/// </summary>
		Default,

		/// <summary>
		/// Sorting is performed without case sensitivity.
		/// </summary>
		CaseInsensitive,

		/// <summary>
		/// Sorting is performed with case sensitivity.
		/// </summary>
		CaseSensitive,
	}

	#endregion // FieldSortComparisonType

	#region FilterClearButtonLocation

	// SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldLayoutSettings.FilterClearButtonLocation"/> property.
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
	public enum FilterClearButtonLocation
	{
		/// <summary>
		/// Default is resolved to <i>RecordSelectAndFilterCell</i>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Filter clear button is not displayed.
		/// </summary>
		None = 1,

		/// <summary>
		/// Filter clear button is displayed in record selector of the filter record.
		/// </summary>
		RecordSelector = 2,

		/// <summary>
		/// Filter clear button is displayed in each filter cell.
		/// </summary>
		FilterCell = 3,

		/// <summary>
		/// Filter clear button is displayed in record selector as well as in each filter cell.
		/// </summary>
		RecordSelectorAndFilterCell = 4
	}

	#endregion // FilterClearButtonLocation

	// AS - NA 11.2 Excel Style Filtering
	#region FilterDropDownItemsType
	/// <summary>
	/// Enumeration used to indicate the type of items that will be displayed.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public enum FilterDropDownItemsType
	{
		/// <summary>
		/// The <see cref="Infragistics.Windows.DataPresenter.Events.RecordFilterDropDownPopulatingEventArgs.DropDownItems"/> and <see cref="Infragistics.Windows.DataPresenter.Events.RecordFilterDropDownOpeningEventArgs.DropDownItems"/> property will be used to provide the list of operands.
		/// </summary>
		DropDownItems,

		/// <summary>
		/// The <see cref="Infragistics.Windows.DataPresenter.Events.RecordFilterDropDownPopulatingEventArgs.MenuItems"/> and <see cref="Infragistics.Windows.DataPresenter.Events.RecordFilterDropDownOpeningEventArgs.MenuItems"/> property will be used to provide the list of operands.
		/// </summary>
		MenuItems,
	}
	#endregion //FilterDropDownItemsType

	#region FilterEvaluationTrigger

	// SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldSettings.FilterEvaluationTrigger"/> property.
	/// </summary>
	/// <seealso cref="FieldSettings.FilterEvaluationTrigger"/>
	public enum FilterEvaluationTrigger
	{
		/// <summary>
		/// Default is resolved to <i>OnCellValueChange</i>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Filters are evaluated as soon as a filter cell�s value is changed. This will 
		/// re-filter records as you type.
		/// </summary>
		OnCellValueChange = 1,

		/// <summary>
		/// Filters are evaluated when you leave the filter cell.
		/// </summary>
		OnLeaveCell = 2,

		/// <summary>
		/// Filters are evaluated when you leave the filter record.
		/// </summary>
		OnLeaveRecord = 3,

		/// <summary>
		/// Filters are evaluated when you hit Enter key while on the filter record. Until you do so, any 
		/// changes you make to the filter record will not get applied.
		/// </summary>
		OnEnterKey = 4,

		/// <summary>
		/// Filters are evaluated when you hit Enter key or leave the filter cell.
		/// </summary>
		OnEnterKeyOrLeaveCell = 5,

		/// <summary>
		/// Filters are evaluated when you hit Enter key or leave the filter record.
		/// </summary>
		OnEnterKeyOrLeaveRecord = 6
	}

	#endregion // FilterEvaluationTrigger

	// AS - NA 11.2 Excel Style Filtering
	#region FilterLabelIconDropDownType
	/// <summary>
	/// Enumeration used to determine the type of dropdown that is displayed when the <see cref="FieldLayoutSettings.FilterUIType"/> is resolved to <b>LabelIcons</b>
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
	/// <seealso cref="FilterButton"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public enum FilterLabelIconDropDownType
	{
		/// <summary>
		/// Default is resolved to <i>SingleSelect</i>
		/// </summary>
		Default,

		/// <summary>
		/// A dropdown that allows selection of a single item is displayed.
		/// </summary>
		SingleSelect,

		/// <summary>
		/// A Excel style menu dropdown that allows the end user to select multiple values to match.
		/// </summary>
		MultiSelectExcelStyle,
	} 
	#endregion //FilterLabelIconDropDownType

	#region FilterOperandUIType

	// SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldSettings.FilterOperandUIType"/> property.
	/// </summary>
	/// <seealso cref="FieldSettings.FilterOperandUIType"/>
	public enum FilterOperandUIType
	{
		/// <summary>
		/// Default is resolved to <i>Combo</i>.
		/// </summary>
		Default,

		/// <summary>
		/// Filter cell is not displayed, including the operand as well as the operator portion.
		/// </summary>
		None,

		/// <summary>
		/// Filter cell is disabled, including the operand as well as the operator portions.
		/// </summary>
		Disabled,
		
		/// <summary>
		/// Filter cell Operand portion is a text box.
		/// </summary>
		TextBox, 
		
		/// <summary>
		/// Operand portion is a combo with editable text portion.
		/// </summary>
		Combo, 
		
		/// <summary>
		/// Operand portion is a drop-down list (a combo with non-editable text portion).
		/// </summary>
		DropDownList, 
		
		/// <summary>
		/// Uses the same editor as other cells in the field.
		/// </summary>
		UseFieldEditor
	}

	#endregion // FilterOperandUIType

	#region FilterRecordLocation

	// SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldLayoutSettings.FilterRecordLocation"/> property.
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.FilterRecordLocation"/>
	public enum FilterRecordLocation
	{
		/// <summary>
		/// Default is resolved to OnTop.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Filter record is displayed as the first record in a records collection. This record scrolls 
		/// with its sibling records.
		/// </summary>
		OnTop = 1,

		/// <summary>
		/// Filter record is displayed as the first record in a records collection. This record is fixed 
		/// and does not scroll with its sibling records.
		/// </summary>
		OnTopFixed = 2,

		/// <summary>
		/// Filter record is displayed as the last record in a records collection. This record scrolls 
		/// with its sibling records.
		/// </summary>
		OnBottom = 3,

		/// <summary>
		/// Filter record is displayed as the last record in a records collection. This record is fixed 
		/// and does not scroll with its sibling records.
		/// </summary>
		OnBottomFixed = 4
	}

	#endregion // FilterRecordLocation

	#region FilterUIType

	// SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldLayoutSettings.FilterUIType"/> property.
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
	public enum FilterUIType
	{
		/// <summary>
		/// Default is resolved to <i>FilterRecord</i>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// A filter icon is displayed in each field label. When clicked upon, a drop-down list with various
		/// filtering options is displayed. It typically has field values along with other items like (All), 
		/// (Blanks), (NonBlanks), (Custom), etc. The type of dropdown that is displayed is based on the resolved 
		/// <see cref="FieldSettings.FilterLabelIconDropDownType"/> for each <see cref="Field"/>
		/// </summary>
		LabelIcons = 1,

		/// <summary>
		/// A filter record is displayed. Each filter cell in the record will let you select filter operator 
		/// as well as filter condition for the associated field. Each filter cell has UI for selecting applicable
		/// filter operators (see <see cref="Infragistics.Windows.Controls.ComparisonOperator"/>) and UI for entering the operand value. Certain
		/// common aspects of the filter cell can be controlled using <see cref="FieldSettings.FilterOperatorDropDownItems"/>,
		/// <see cref="FieldSettings.FilterOperatorDefaultValue"/>, <see cref="FieldSettings.FilterOperandUIType"/>,
		/// <see cref="FieldSettings.FilterEvaluationTrigger"/> and <see cref="FieldSettings.FilterClearButtonVisibility"/>.
		/// </summary>
		FilterRecord = 2
	}

	#endregion // FilterUIType

	// AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
    #region FixedFieldLocation
    /// <summary>
    /// Indicates whether a <see cref="Field"/> is fixed and if so to which edge.
    /// </summary>
    /// <seealso cref="Field.FixedLocation"/>
    public enum FixedFieldLocation
    {
        /// <summary>
        /// The field is not fixed and will move as the scrollbar is moved.
        /// </summary>
        Scrollable,

        /// <summary>
        /// The field is fixed and does not scroll. In a horizontal layout, the field elements will be positioned at the top and in a vertical layout, the field elements will be positioned on the left.
        /// </summary>
        FixedToNearEdge,

        /// <summary>
        /// The field is fixed and does not scroll. In a horizontal layout, the field elements will be positioned at the bottom and in a vertical layout, the field elements will be positioned on the right.
        /// </summary>
        FixedToFarEdge,
    } 
    #endregion //FixedFieldLocation

    #region FixedFieldSplitterType
    /// <summary>
    /// Indicates whether a <see cref="Field"/> is fixed and if so to which edge.
    /// </summary>
    /// <seealso cref="FixedFieldSplitter.SplitterType"/>
    public enum FixedFieldSplitterType
    {
        /// <summary>
        /// The thumb will be used to fix the element � top in horizontal and left in vertical layout.
        /// </summary>
        Near,

        /// <summary>
        /// The thumb will be used to fix field elements to the far edge � bottom in horizontal and right in vertical layout.
        /// </summary>
        Far,
    }
    #endregion //FixedFieldSplitterType

    #region FixedFieldUIType
    /// <summary>
    /// Indicates the type of UI provided to allow changing of the <see cref="Field.FixedLocation"/>
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.FixedFieldUIType"/>
    public enum FixedFieldUIType
    {
        /// <summary>
        /// The default is resolved based on whether there are fixable and/or fixed fields. If there are field's whose <see cref="Field.AllowFixingResolved"/> is not no, then the default is 'ButtonAndSplitter'. If there are fixed fields but the end user is not allowed to change their state, then this resolves to 'Splitter'.
        /// </summary>
        Default,

        /// <summary>
        /// The user cannot change the fixed state of the field(s).
        /// </summary>
        None,

        /// <summary>
        /// A button or dropdown is shown within the LabelPresenter of the field that may be used to toggle the fixed state.
        /// </summary>
        Button,

        /// <summary>
        /// A thumb is displayed within the HeaderPresenter that may be used to drag over fields to indicate a range of fields that should be fixed.
        /// </summary>
        Splitter,

        /// <summary>
        /// A dropdown appears within the LabelPresenter and a thumb is displayed within the HeaderPresenter.
        /// </summary>
        ButtonAndSplitter,
    }
    #endregion //FixedFieldUIType

    #region FixedRecordLocationInternal

    
	
	/// <summary>
	/// Enum used for indicating whether a record is fixed and if so whether it's fixed on top or bottom.
	/// </summary>
	internal enum FixedRecordLocationInternal
	{
		/// <summary>
		/// Default is resolved to None.
		/// </summary>
		Default,

		/// <summary>
		/// Record is not fixed.
		/// </summary>
		None,

		/// <summary>
		/// Record is fixed on top.
		/// </summary>
		Top,

		/// <summary>
		/// Record is fixed on bottom.
		/// </summary>
		Bottom,
	}

	#endregion // FixedRecordLocationInternal

    // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
    #region FixedRecordLocation
    /// <summary>
    /// Indicates whether a <see cref="Record"/> is fixed and if so to which edge.
    /// </summary>
    /// <seealso cref="Record.FixedLocation"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
    public enum FixedRecordLocation
    {
        /// <summary>
        /// The record is not fixed and will move as the scrollbar is moved.
        /// </summary>
        Scrollable,

        /// <summary>
        /// The record is fixed on top and does not scroll.
        /// </summary>
        FixedToTop,

        /// <summary>
        /// The record is fixed on bottom and does not scroll. 
        /// </summary>
        FixedToBottom,
    } 
    #endregion //FixedRecordLocation

    // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
    #region FixedRecordUIType
    /// <summary>
    /// Indicates the type of UI provided to allow changing of the <see cref="Record.FixedLocation"/>
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.FixedRecordUIType"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
    public enum FixedRecordUIType
    {
        /// <summary>
        /// The default resolves to None. 
        /// </summary>
        Default,

        /// <summary>
        /// The user cannot change the fixed state of the records(s).
        /// </summary>
        None,

        /// <summary>
        /// A button within the <see cref="RecordSelector"/> that may be used to toggle the fixed state.
        /// </summary>
        Button,
    }
    #endregion //FixedRecordUIType

    // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
    #region FixedRecordSortOrder
    /// <summary>
    /// Indicates the order of records that have been fixed thru the UI.
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.FixedRecordSortOrder"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
    public enum FixedRecordSortOrder
    {
        /// <summary>
        /// The default resolves to Sorted. 
        /// </summary>
        Default,

        /// <summary>
        /// Fixed records remain in the same order as they are fixed, even after the sorted fields have changed. Calling RefreshSort method on the FixedRowsCollection will resort the fixed records based on the sorted fields.
        /// </summary>
        FixOrder,

        /// <summary>
        /// Fixed records are sorted when the user sorts fields. Note that however when the user fixes a record it is always added at the end of the fixed records. To insert it into the correct sort position call <see cref="Record.RefreshSortPosition"/> method on the record in the <see cref="DataPresenterBase.RecordFixedLocationChanged"/> event handler.
        /// </summary>
        Sorted
    }
    #endregion //FixedRecordSortOrder

	#region GroupByAreaLocation

	/// <summary>
	/// Determines where the GroupByArea is displayed
	/// </summary>
	public enum GroupByAreaLocation
	{
		/// <summary>
		/// The GroupByArea is not displayed
		/// </summary>
		None = 0,
		/// <summary>
		/// The GroupByArea is displayed above the data area.
		/// </summary>
		AboveDataArea = 1,
		/// <summary>
		/// The GroupByArea is displayed below the data area.
		/// </summary>
		BelowDataArea = 2,
	}

	#endregion GroupByAreaLocation

    // JJD 4/07/09 - NA 2009 vol 2 - Cross band grouping
	#region GroupByAreaMode

	/// <summary>
	/// Determines how fields are grouped by the end-user.
	/// </summary>
	[InfragisticsFeature(FeatureName=FeatureInfo.FeatureName_CrossBandGrouping, Version=FeatureInfo.Version_9_2)]
    public enum GroupByAreaMode
	{
		/// <summary>
		/// The GroupByArea supports more than one field layout and the FieldLayouts are stacked vertically.
        /// In order to group by a field the user must drag its 
        /// associated LabelPresenter into the GroupByArea. This is the default setting.
		/// </summary>
		MultipleFieldLayoutsFull = 0,

		/// <summary>
		/// The GroupByArea supports more than one field layout and the FieldLayouts are stacked horizontally to
        /// save vertical space. In order to group by a field the user must drag its 
        /// associated LabelPresenter into the GroupByArea. 		
        /// </summary>
		MultipleFieldLayoutsCompact = 1,

		/// <summary>
		/// In this mode the GroupByArea displays 2 lists of fields. The first list contains fields that are currently being grouped 
        /// and the second list contains available fields from the DefaultFieldLayout. The user can drag fields between these lists 
        /// to change their groupby status. 
		/// </summary>
		DefaultFieldLayoutOnly = 2,
	}

	#endregion GroupByAreaMode

	#region GroupBySummaryDisplayMode

	
	
	/// <summary>
	/// Specifies how summaries are displayed inside each group-by record.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>GroupBySummaryDisplayMode</b> is used for specifying 
	/// <see cref="FieldLayoutSettings.GroupBySummaryDisplayMode"/> property.
	/// </para>
	/// <seealso cref="FieldLayoutSettings.GroupBySummaryDisplayMode"/>
	/// </remarks>
	public enum GroupBySummaryDisplayMode
	{
		/// <summary>
		/// Default is resolved to <b>Text</b>.
		/// </summary>
		Default				= 0,

		///// <summary>
		///// Each summary is displayed inside the group-by record as a cell aligned 
		///// with the associated field. If a summary cell overlaps with the group-by 
		///// record description, the summaries are displayed below the description 
		///// and the record height is increased as necessary.
		///// </summary>
		//SummaryCells		= 1,

		/// <summary>
		/// Same as <b>SummaryCells</b> however summaries are always displayed below 
		/// the group-by record description.
		/// </summary>
		SummaryCellsAlwaysBelowDescription = 1,

		/// <summary>
		/// Summaries are displayed as plain text appended to the group-by record 
		/// description. Each summary is displayed as a column name followed by the 
		/// summary�s value. Multiple summaries are separated by comma (�,�) character.
		/// </summary>
		Text				= 2
	}

	#endregion // GroupBySummaryDisplayMode

    // JJD 1/15/09 - NA 2009 vol 1 
    #region HeaderPlacement

    /// <summary>
    /// Determines where header's are placed
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.HeaderPlacement"/>
    /// <seealso cref="FieldLayoutSettings.HeaderPlacementInGroupBy"/>
    /// <seealso cref="HeaderPlacementInGroupBy"/>
    public enum HeaderPlacement
    {
        /// <summary>
        /// The default (resolves to 'OnTopOnly')
        /// </summary>
        Default = 0,

        /// <summary>
        /// A header will only appear at the top of each record island.
        /// </summary>
        OnTopOnly = 1,

        /// <summary>
        /// A header will appear at the top of each record island as well as on top of a record whose previous sibling record was expanded with child records.
        /// </summary>
        OnRecordBreak = 2,
    }

    #endregion //HeaderPlacement	

    // JJD 1/15/09 - NA 2009 vol 1 
    #region HeaderPlacementInGroupBy

    /// <summary>
    /// Determines where header's are placed when there are groupby records displayed
    /// </summary>
    /// <seealso cref="HeaderPlacement"/>
    /// <seealso cref="FieldLayoutSettings.HeaderPlacement"/>
    /// <seealso cref="FieldLayoutSettings.HeaderPlacementInGroupBy"/>
    /// <seealso cref="FieldLayoutSettings.FilterUIType"/>
    /// <seealso cref="FieldLayout.FilterUITypeResolved"/>
    public enum HeaderPlacementInGroupBy
    {
        /// <summary>
        /// The default (resolves to 'WithDataRecords' unless HeaderPlacement is explicitly set to 'OnTopOnly' or FilterUIType resolves to 'LabelIcons')
        /// </summary>
        Default = 0,

        /// <summary>
        /// A header will only appear at the top of the groupby records.
        /// </summary>
        OnTopOnly = 1,

        /// <summary>
        /// A header will appear at the top each island of DataRecords and will not appear on top of the groupby records unless there is an associated FilterRecord or SummaryRecord on top.
        /// </summary>
        WithDataRecords  = 2,
    }

    #endregion //HeaderPlacementInGroupBy	

	#region HeaderPrefixAreaDisplayMode

	// SSP 6/5/09 - NAS9.2 Field Chooser
	// 

	/// <summary>
	/// Used to specify <see cref="FieldLayoutSettings.HeaderPrefixAreaDisplayMode"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public enum HeaderPrefixAreaDisplayMode
	{
		/// <summary>
		/// Default is resolved to <b>None</b>.
		/// </summary>
		Default,

		/// <summary>
		/// Nothing is displayed.
		/// </summary>
		None,

		/// <summary>
		/// A button that shows the <see cref="FieldChooser"/> is displayed.
		/// </summary>
		FieldChooserButton
	}

	#endregion // HeaderPrefixAreaDisplayMode

	#region HighlightPrimaryField

	/// <summary>
	/// Determines if a separate template will be used to highlight the primary field
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.HighlightPrimaryField"/>
	/// <seealso cref="FieldLayout.HighlightPrimaryFieldResolved"/>
	/// <seealso cref="Field.IsPrimary"/>
	public enum HighlightPrimaryField
	{
		/// <summary>
		/// The default based on the type of generator used
		/// </summary>
		Default = 0,

		/// <summary>
		/// Highlight the primary field
		/// </summary>
		Highlight = 1,

		/// <summary>
		/// Don't highlight the primary field
		/// </summary>
		SameAsOtherFields = 2,
	}

	#endregion //HighlightPrimaryField

	// AS 7/30/09 NA 2009.2 Field Sizing
	#region ItemSizeType
	internal enum ItemSizeType : byte
	{
		Explicit = 0,
		AutoMode = 1,
		ExplicitAutoSize = 2
	} 
	#endregion //ItemSizeType

	#region LabelClickAction

	/// <summary>
	/// Determines what happens when the user clicks on a <see cref="Infragistics.Windows.DataPresenter.Field"/>'s label
	/// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> when using the SortByOneFieldOnlyTriState or SortByMultipleFieldsTriState settings, if the 'Ctrl' key is pressed during a click, or the <see cref="Field.IsGroupBy"/> property is true, then the Field will not cycle thru the unsorted state. Instead it will cycle thru the ascending and descending states only.</para>
    /// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.LabelClickAction"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field.LabelClickActionResolved"/>
	public enum LabelClickAction
	{
		/// <summary>
		/// Use the default setting
		/// </summary>
		Default = 0,
		/// <summary>
		/// Nothing happens
		/// </summary>
		Nothing = 1,
		/// <summary>
		/// The records are sorted exclusively by this field
		/// </summary>
		SortByOneFieldOnly = 2,
		/// <summary>
		/// The records are sorted by this field, the old sort criteria is only reset if the 'Ctrl' key is not depressed.
		/// </summary>
		SortByMultipleFields = 3,
		/// <summary>
		/// The field is selected
		/// </summary>
		SelectField = 4,
        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
		/// <summary>
		/// The records are sorted exclusively by this field. Sequential clicks will cycle thru the ascending, descending and unsorted states.
		/// </summary>
		SortByOneFieldOnlyTriState = 5,
		/// <summary>
        /// The records are sorted by this field, the old sort criteria is only reset if the 'Ctrl' key is not depressed. Sequential clicks will cycle thru the ascending, descending and unsorted states.
		/// </summary>
		SortByMultipleFieldsTriState = 6,
	}

	#endregion LabelClickAction

	#region LabelLocation

	/// <summary>
	/// Determines where the labels should appear
	/// </summary>
	public enum LabelLocation
	{
		/// <summary>
		/// Default based on CellPresentation
		/// </summary>
		Default = 0,
		/// <summary>
		/// Show labels inside the cells
		/// </summary>
		InCells = 1,
		/// <summary>
		/// Show labels in a separate header
		/// </summary>
		SeparateHeader = 2,
		/// <summary>
		/// Don't show labels
		/// </summary>
		Hidden = 3,
	}

	#endregion //LabelLocation

	// JJD 7/15/10 - TFS35815 - added
	#region NonSpecificNotificationBehavior

	/// <summary>
	/// Determines if values are refreshed when a notification is received that a change has occured for a DataRecord but the notification doesn't specify which field value has been changed.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note</b>: non-specific notifications can be received in one of 2 ways. The first is if the parent list implements <see cref="IBindingList"/> and raises
	///  a ListChanged event with a <see cref="ListChangedType"/> of 'ItemChanged' and a null PropertyDescriptor. The second way is if the data item implements <see cref="INotifyPropertyChanged"/> and 
	///  raises a PropertyChanged event with a null or empty 'PropertyName'.</para>
	/// </remarks>
	/// <seealso cref="FieldSettings"/>
	/// <seealso cref="FieldSettings.NonSpecificNotificationBehavior"/>
	public enum NonSpecificNotificationBehavior
	{
		/// <summary>
		/// Use the default value. This resolves to 'RefreshValue' unless set at a higher level.
		/// </summary>
		Default,
		
		/// <summary>
		/// Re-get the value
		/// </summary>
		RefreshValue,
		
		/// <summary>
		/// Only re-get the value for Fields whose data type doesn't implement the <see cref="IEnumerable"/> interface (excluding strings)
		/// </summary>
		BypassIfEnumerable,

		/// <summary>
		/// Only re-get the value for Fields whose data type doesn't implement the <see cref="IBindingList"/> interface
		/// </summary>
		BypassIfBindingList,
		
		/// <summary>
		/// Only re-get the value for Fields whose data type doesn't implement either the <see cref="INotifyCollectionChanged"/> or <see cref="IBindingList"/> interfaces
		/// </summary>
		BypassIfObservable,
	}

	#endregion //NonSpecificNotificationBehavior	
    
	#region PanelNavigationDirection

	/// <summary>
	/// Direction to navigate within a DataPresenterBase panel.
	/// </summary>
	public enum PanelNavigationDirection
	{
		/// <summary>
		/// Navigate to the item that is to the left of the item at the current position.
		/// </summary>
		Left,
		/// <summary>
		/// Navigate to the item that is to the right of the item at the current position.
		/// </summary>
		Right,
		/// <summary>
		/// Navigate to the item that is above the item at the current position.
		/// </summary>
		Above,
		/// <summary>
		/// Navigate to the item that is below the item at the current position.
		/// </summary>
		Below,
		/// <summary>
		/// Navigate to the item that is logically 'next' with respect to item at the current position.
		/// </summary>
		Next,
		/// <summary>
		/// Navigate to the item that is logically 'previous' with respect to item at the current position.
		/// </summary>
		Previous
	}

	#endregion //PanelNavigationDirection

	#region PanelLayoutStyle

	/// <summary>
	/// The layout style of a DataPresenterBase panel.
	/// </summary>
	public enum PanelLayoutStyle
	{
		/// <summary>
		/// The panel arranges records in a vertical grid view.
		/// </summary>
		GridViewVertical,
		/// <summary>
		/// The panel arranges records in a horizontal grid view.
		/// </summary>
		GridViewHorizontal,
		/// <summary>
		/// The panel arranges records using a custom layout.
		/// </summary>
		Custom
	}

	#endregion //PanelLayoutStyle

	#region PanelNavigationScrollType

	/// <summary>
	/// Direction in which to scroll the contents of a DataPresenterBase panel.
	/// </summary>
	public enum PanelNavigationScrollType
	{
		/// <summary>
		/// Scroll to the page above.
		/// </summary>
		PageAbove,
		/// <summary>
		/// Scroll to the page below.
		/// </summary>
		PageBelow,
		/// <summary>
		/// Scroll to the page on the left.
		/// </summary>
		PageLeft,
		/// <summary>
		/// Scroll to the page on the right.
		/// </summary>
		PageRight
	}

	#endregion //PanelNavigationScrollType	

	#region PanelSiblingNavigationStyle

	/// <summary>
	/// Types of navigation between sibling cells and records within a DataPresenterBase panel.
	/// </summary>
	public enum PanelSiblingNavigationStyle
	{
		/// <summary>
		/// Navigate across parents with no wrapping
		/// </summary>
		AcrossParentsNoWrap,
		/// <summary>
		/// Navigate across parents and wrap.
		/// </summary>
		AcrossParentsAndWrap,
		/// <summary>
		/// Navigate within the parent with no wrapping.
		/// </summary>
		StayWithinParentNoWrap,
		/// <summary>
		/// Navigate within the parent and wrap.
		/// </summary>
		StayWithinParentAndWrap
	}

	#endregion //PanelSiblingNavigationStyle

	// JJD 4/16/12 - TFS108549 - added
	#region RecordContainerRetentionMode

	/// <summary>
	/// Determines whether de-activated (i.e. unused) record containers will be retained in the visual tree for possible future use.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> if the property is set 'RetainDeactivatedContainers' or 'RetainDeactivatedContainersUntilResize' then any record containers that have already been hydrated but are not used 
	/// in the current display will be retained in the visual tree (collapsed) so that they can be recycled in the future. Depending on the scenario, e.g. if the mix of DataRecords, GroupByRecords and SummaryRecords in the display changes, this
	/// can improve scrolling performance. This property is ignored if <see cref="DataPresenterBase.RecordContainerGenerationMode"/> is set to any value other than 'Recycle'.</para>
	/// </remarks>
	/// <seealso cref="DataPresenterBase.RecordContainerRetentionMode"/>
	/// <seealso cref="DataPresenterBase.RecordContainerGenerationMode"/>
	public enum RecordContainerRetentionMode
	{
		/// <summary>
		/// (Default) All record containers that are not being used in the display (i.e. deactivated and collapsed) will be retained for later use until the control is resized smaller in the scrolling dimension. This can help optimize scrolling performance in cases where the mix of DataRecords, GroupByRecords and SummaryRecords in the display changes.
		/// </summary>
		RetainDeactivatedContainersUntilResize = 0,

		/// <summary>
		/// All record containers that are not being used in the display (i.e. deactivated and collapsed) will be retained for later use. This can help optimize scrolling performance in cases where the mix of DataRecords, GroupByRecords and SummaryRecords in the display changes.
		/// </summary>
		RetainDeactivatedContainers = 1,
		
		/// <summary>
		/// On layout updated any record containers that are not being used in the display (i.e. deactivated and collapsed) will be discarded.
		/// </summary>
		DiscardDeactivatedContainers = 2,
	}

	#endregion //RecordContainerRetentionMode	
    
	#region RecordExportCancellationReason
	/// <summary>
	/// Enumeration identifying the cause for the export operation being cancelled.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public enum RecordExportCancellationReason
	{
		/// <summary>
		/// The export was explicitly cancelled - e.g. by the <see cref="DataPresenterBase.CancelExport"/> method or using the <see cref="DataPresenterCommands.CancelExport"/> command.
		/// </summary>
		Cancelled,

		/// <summary>
		/// The <see cref="ProcessRecordParams.TerminateExport"/> property was set to true during the <see cref="IDataPresenterExporter.ProcessRecord"/> or <see cref="IDataPresenterExporter.InitializeRecord"/> calls.
		/// </summary>
		TerminateExport,

		/// <summary>
		/// An exception occured during the export process.
		/// </summary>
		Exception,

		/// <summary>
		/// The cancellation reason is unknown. This is primarily used when performing an synchronous export and the export operation was cancelled.
		/// </summary>
		Unknown,
	} 
	#endregion //RecordExportCancellationReason

	// AS 3/3/11 NA 2011.1 - Async Exporting
	#region RecordExportStatus
	/// <summary>
	/// Enumeration indicating the status of the export operation.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public enum RecordExportStatus
	{
		/// <summary>
		/// No export operations are in progress
		/// </summary>
		NotExporting,

		/// <summary>
		/// An asynchronous export operation has been queued. The control will remain in this state while other previously requested asynchronous export operations have been initiated for other DataPresenter instances.
		/// </summary>
		Pending,

		/// <summary>
		/// An asynchronous export operation has started and is in the process of being initialized for the export operation.
		/// </summary>
		Initializing,

		/// <summary>
		/// An asynchronous export operation is in progress.
		/// </summary>
		Exporting,
	} 
	#endregion //RecordExportStatus

    #region RecordFilterAction

    // SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldLayoutSettings.FilterAction"/> property.
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.FilterAction"/>
	public enum RecordFilterAction
	{
		/// <summary>
		/// Default is resolved to <i>Hide</i>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Hides filtered out records. 
		/// </summary>
		Hide	= 1,

		/// <summary>
		/// Disables filtered out records.
		/// </summary>
		Disable = 2,

		/// <summary>
		/// Record�s <see cref="DataRecord.IsFilteredOut"/> property will be updated however no other action will 
		/// be taken. The fitlered out records will remain visible. This is useful if you want to style records 
		/// differently when they are filtered out (for example, apply different background color to filtered or
		/// non-filtered records).
		/// </summary>
		None	= 3
	}

	#endregion // RecordFilterAction

	#region RecordFilterScope

	// SSP 12/9/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Used for specifying <see cref="FieldLayoutSettings.RecordFilterScope"/> property.
	/// </summary>
	/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
	public enum RecordFilterScope
	{
		/// <summary>
		/// Default is resolved to <i>AllRecords</i>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// FieldLayout�s <see cref="FieldLayout.RecordFilters"/> is used for filtering records.
		/// </summary>
		AllRecords = 1,

		/// <summary>
		/// RecordManager�s <see cref="RecordManager.RecordFilters"/> is used for filtering records. 
		/// This allows for independent filter criteria on records of different record managers.
		/// <b>Note</b> that this only has meaning for child RecordManagers. For the root RecordManager,
		/// the field layout's RecordFilters will be used always. Modifying the root RecordManager's
		/// RecordFilters will result in an exception.
		/// </summary>
		SiblingDataRecords = 2
	}

	#endregion // RecordFilterScope

	// AS - NA 11.2 Excel Style Filtering
	#region RecordFilterTreeSearchScope
	/// <summary>
	/// Enumeration used to identify the scope to use when searching for the <see cref="RecordFilterTreeControl.SearchText"/> in the list of available nodes.
	/// </summary>
	/// <seealso cref="RecordFilterTreeControl"/>
	/// <seealso cref="RecordFilterTreeControl.SearchText"/>
	/// <seealso cref="RecordFilterTreeControl.SearchScope"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public enum RecordFilterTreeSearchScope
	{
		// Note the values are specific and ordered

		/// <summary>
		/// The search text will be compared against the display of all the nodes
		/// </summary>
		All = 0,

		/// <summary>
		/// The search text will be compared against the display of the year nodes and root level nodes
		/// </summary>
		Year = 1,

		/// <summary>
		/// The search text will be compared against the display of the month nodes
		/// </summary>
		Month = 2,

		/// <summary>
		/// The search text will be compared against the display of the day nodes
		/// </summary>
		Day = 3,

		/// <summary>
		/// The search text will be compared against the display of the hour nodes
		/// </summary>
		Hour = 4,

		/// <summary>
		/// The search text will be compared against the display of the minute nodes
		/// </summary>
		Minute = 5,

		/// <summary>
		/// The search text will be compared against the display of the second nodes
		/// </summary>
		Second = 6
	}
	#endregion //RecordFilterTreeSearchScope

	#region RecordLoadMode

	/// <summary>
	/// Enum the specifies how <see cref="DataRecord"/>s are loaded.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordLoadMode"/>
	public enum RecordLoadMode
	{
		/// <summary>
		/// Pre-loads all records.
		/// </summary>
		PreloadRecords = 0,

		/// <summary>
		/// Loads records as they are needed. <b>NOTE:</b> Some operations, like sorting records, 
		/// will cause the all records to be loaded regardless of the RecordLoadMode setting because they 
		/// require access to all records. 
		/// </summary>
		LoadOnDemand = 1
	}

	#endregion // RecordLoadMode

	#region RecordSelectorLocation

	/// <summary>
	/// Determines the position of the <see cref="RecordSelector"/> relative to the <see cref="Record"/>'s cell area.
	/// </summary>
	public enum RecordSelectorLocation
	{
		/// <summary>
		/// Use the default setting
		/// </summary>
		Default = 0,
		/// <summary>
		/// Above cell area
		/// </summary>
		AboveCellArea = 1,
		/// <summary>
		/// Below cell area
		/// </summary>
		BelowCellArea = 2,
		/// <summary>
		/// Left on cell area 
		/// </summary>
		LeftOfCellArea = 3,
		/// <summary>
		/// Right on cell area 
		/// </summary>
		RightOfCellArea = 4,
		/// <summary>
		/// Don't show record selectors
		/// </summary>
		None = 5,

	}

	#endregion RecordSelectorLocation

	#region RecordSeparatorLocation

	
	
	/// <summary>
	/// Used for specifying if and for which special records a separator element is displayed.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// Used for specifying FieldLayoutSettings' <see cref="FieldLayoutSettings.RecordSeparatorLocation"/>
	/// property which controls which special records display a seprator element. Separator element
	/// looks like a thin 3d bar element (like a resizer bar) that separates the associated record from
	/// other records.
	/// </para>
	/// </remarks>
	/// <seealso cref="FieldLayoutSettings.RecordSeparatorLocation"/>
	[Flags]
	public enum RecordSeparatorLocation
	{
		/// <summary>
		/// Record separator is not displayed.
		/// </summary>
		None				= 0x0,

		/// <summary>
		/// Separator is displayed after template add-record.
		/// </summary>
		TemplateAddRecord	= 0x1,

		/// <summary>
		/// Separator is displayed after fixed records to separate them from the non-fixed records.
		/// </summary>
		FixedRecords		= 0x2,

		/// <summary>
		/// Separator is displayed after summary record.
		/// </summary>
		SummaryRecord		= 0x4,

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Separator is displayed after filter record.
		/// </summary>
		FilterRecord		= 0x8,
	}

	#endregion // RecordSeparatorLocation

	#region RecordSeparatorVisibility

	// SSP 5/6/08 - Summaries Feature
	// 
	/// <summary>
	/// Used for indicating whether a record displays separator before or after it.
	/// </summary>
	[Flags]
	internal enum RecordSeparatorVisibility
	{
		/// <summary>
		/// Record doesn't display separator.
		/// </summary>
		None	= 0,

		/// <summary>
		/// Record displays separator before it.
		/// </summary>
		Before	= 0x1,

		/// <summary>
		/// Record displays separator after it.
		/// </summary>
		After	= 0x2,
	}

	#endregion // RecordSeparatorVisibility

	#region RecordType

	/// <summary>
	/// Determines what a Record represents in a DataPresenterBase
	/// </summary>
	public enum RecordType
	{
		/// <summary>
		/// Represents a record in a DataPresenterBase
		/// </summary>
		DataRecord = 0,
		/// <summary>
		/// Represents records grouped by a specific Field in a DataPresenterBase
		/// </summary>
		GroupByField = 1,
		/// <summary>
		/// Represents records grouped by a their type in a DataPresenterBase
		/// </summary>
		GroupByFieldLayout = 2,
		/// <summary>
		/// Represents a nested record retrieved from an expandable field value
		/// </summary>
		ExpandableFieldRecord = 3,

		
		
		/// <summary>
		/// Represents a <see cref="SummaryRecord"/> where summary results are displayed.
		/// </summary>
		SummaryRecord = 4,

		
		
		/// <summary>
		/// Represents a <see cref="FilterRecord"/>.
		/// </summary>
		FilterRecord = 5,

		
		
		/// <summary>
		/// Represents a <see cref="HeaderRecord"/>.
		/// </summary>
		HeaderRecord = 6
	}

	#endregion RecordType

	#region RecordUpdatingAction

	/// <summary>
	/// Determines what action to take when processing the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdating"/> event.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.RecordUpdatingEventArgs"/>
	public enum RecordUpdatingAction
	{
		/// <summary>
		/// Update the data source with any modified cell values.
		/// </summary>
		ProceedWithUpdate = 0,
		/// <summary>
		/// Cancel the update and retain any modified cell values.
		/// </summary>
		CancelUpdateRetainChanges = 1,
		/// <summary>
		/// Cancel the update and discard any modified cell values. Revert all cells to their original values.
		/// </summary>
		CancelUpdateDiscardChanges = 2,
	}

	#endregion RecordUpdatingAction

	// JJD 11/30/10 - TFS31984 - added
	#region ScrollBehaviorOnListChange
	/// <summary>
	/// Enumeration used to determine the scroll position behavior when records are added or removed before the current records in view.
	/// </summary>
	/// <seealso cref="DataPresenterBase.ScrollBehaviorOnListChange"/>
	public enum ScrollBehaviorOnListChange
	{
		/// <summary>
		/// Default behavior based on the view. 
		/// </summary>
		Default,

		/// <summary>
		/// When records are added or removed before the current records in view attempt to preserve the scroll offset. This should cause the records in view to change.
		/// </summary>
		PreserveScrollOffset,

		/// <summary>
		/// When records are added or removed before the current records in view attempt to preserve the current records in view. This should cause the records in view to remain the same.
		/// </summary>
		PreserveRecordsInView,

	}
	#endregion //ScrollBehaviorOnListChange

	#region ScrollingMode
	/// <summary>
	/// Enumeration used to determine whether record scrolling is deferred or immediate when dragging the scroll thumb.
	/// </summary>
	/// <seealso cref="DataPresenterBase.ScrollingMode"/>
	public enum ScrollingMode
	{
		/// <summary>
		/// Scrolling is deferred until the scroll thumb is released. As the thumb is repositioned during the drag, a tooltip is displayed adjacent to the scroll thumb that provides information about the record that would be displayed as the new trecord if the mouse was released as that location.
		/// </summary>
		DeferredWithScrollTips,

		/// <summary>
		/// Scrolling is deferred until the scroll thumb is released.
		/// </summary>
		Deferred,

		/// <summary>
		/// Scrolling of records occurs while the thumb is being dragged.
		/// </summary>
		Immediate,
	} 
	#endregion //ScrollingMode

	#region States

	/// <summary>
	/// Represents the different states of the control.  Used to evaluate whether a specific command can be executed.
	/// </summary>
	[Flags]
	public enum States : long
	{
		/// <summary>
		/// Indicates that there is at least one item selected (Record, Field or Cell)
		/// </summary>
		ItemsSelected							= 0x00000001,

		/// <summary>
		/// There is at least 1 record.
		/// </summary>
		Records									= 0x00000002,

		/// <summary>
		/// Active Record is not null
		/// </summary>
		Record									= 0x00000004,

		/// <summary>
		/// Active Record is expandable
		/// </summary>
		RecordExpandable						= 0x00000008,

		/// <summary>
		/// Active Record is expanded
		/// </summary>
		RecordExpanded							= 0x00000010,

		/// <summary>
		/// Active Record is the first displayed record
		/// </summary>
		RecordFirstDisplayed					= 0x00000020,

		/// <summary>
		/// Active Record is the last displayed record
		/// </summary>
		RecordLastDisplayed						= 0x00000040,

		/// <summary>
		/// Active Record is the first overall record
		/// </summary>
		RecordFirstOverall						= 0x00000080,

		/// <summary>
		/// Active Record is the last overall record
		/// </summary>
		RecordLastOverall						= 0x00000100,

		/// <summary>
		/// ActiveRecord is a GroupByRecord.
		/// </summary>
		GroupByRecord							= 0x00000200,

		/// <summary>
		/// ActiveRecord is an ExpandableFieldRecord.
		/// </summary>
		ExpandableFieldRecord					= 0x00000400,

		/// <summary>
		/// Active Cell not null
		/// </summary>
		Cell									= 0x00000800,

		/// <summary>
		/// Active Cell is the first Cell in the ActiveRecord
		/// </summary>
		CellFirstInRecord						= 0x00001000,

		/// <summary>
		/// Active Cell is the last Cell in the ActiveRecord
		/// </summary>
		CellLastInRecord						= 0x00002000,

		/// <summary>
		/// Active Cell is the first displayed Cell
		/// </summary>
		CellFirstDisplayed						= 0x00004000,

		/// <summary>
		/// Active Cell is the last displayed Cell
		/// </summary>
		CellLastDisplayed						= 0x00008000,

		/// <summary>
		/// Active Cell is the first overall Cell
		/// </summary>
		CellFirstOverall						= 0x00010000,

		/// <summary>
		/// Active Cell is the last overall Cell
		/// </summary>
		CellLastOverall							= 0x00020000,

		/// <summary>
		/// Layout style is GridView vertical.
		/// </summary>
		NavigationLayoutStyleGridViewVertical	= 0x00040000,

		/// <summary>
		/// Layout style is GridView horizontal.
		/// </summary>
		NavigationLayoutStyleGridViewHorizontal	= 0x00080000,

		/// <summary>
		/// The active cell is in edit mode
		/// </summary>
		IsInEditMode							= 0x00100000,

		/// <summary>
		/// Indicates that there is at least one DataRecord selected
		/// </summary>
		DataRecordsSelected						= 0x00200000,

		/// <summary>
		/// Active Record has changes that are pending (i.e. one or more cell values have been changed)
		/// </summary>
		RecordHasPendingChanges					= 0x00400000,

        // JJD 12/30/08 NA 2009 Vol 1 - Record Filtering
        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
        /// <summary>
        /// The active cell is a filter cell.
        /// </summary>
        FilterCell                              = 0x00800000,

        /// <summary>
        /// The active cell is a filter cell and it has filter criteria specified.
        /// </summary>
        FilterCellHasActiveFilters              = 0x01000000,

        /// <summary>
        /// The active record is a filter record.
        /// </summary>
        FilterRecord                             = 0x02000000,

        /// <summary>
        /// The active record is a filter record and at least one of its cells has filter criteria specified.
        /// </summary>
        FilterRecordHasActiveFilters              = 0x04000000,

        // AS 4/8/09 NA 2009.2 ClipboardSupport
        /// <summary>
        /// Indicates that there is at least one <see cref="Infragistics.Windows.DataPresenter.Cell"/> selected
        /// </summary>
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
        CellsSelected                           = 0x08000000,

		// JM NA 2010.1 CardView.
		/// <summary>
		/// Layout style is CardView vertical.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_CardView)]
		NavigationLayoutStyleCardViewVertical	= 0x10000000,

		// JM NA 2010.1 CardView.
		/// <summary>
		/// Layout style is CardView horizontal.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_CardView)]
		NavigationLayoutStyleCardViewHorizontal = 0x20000000,

		// AS 3/3/11 NA 2011.1 - Async Exporting
		/// <summary>
		/// The <see cref="IsExporting"/> is true.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		IsExporting = 0x40000000,

		// AS 3/10/11 NA 2011.1 - Async Exporting
		/// <summary>
		/// An asynchronous export operation has been initiated but export of the records has not yet begun.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
		IsExportingInitializing = 0x40000000,

		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
	}

	#endregion //States enum

	#region SummaryDisplayAreas

	
	
	/// <summary>
	/// Used for specifying where summaries are displayed.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryDisplayAreas</b> is used for specifying <see cref="FieldSettings.SummaryDisplayArea"/> and
	/// <see cref="SummaryDefinition.DisplayArea"/> properties. It controls if and where to display summaries.
	/// </para>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
	/// <seealso cref="SummaryDefinition.DisplayArea"/>
	/// </remarks>
	[Flags]
	public enum SummaryDisplayAreas
	{
		/// <summary>
		/// The summary is hidden.
		/// </summary>
		None			= 0x0,

		/// <summary>
		/// Summary is displayed in the summary record at the top of each record collection.
		/// </summary>
		Top				= 0x1,

		/// <summary>
		/// Same as <b>Top</b> except the summary record is fixed (non-scrolling) so that 
		/// it remains in view when other data records are scrolled.
		/// </summary>
		TopFixed		= 0x2,

		/// <summary>
		/// Summary is displayed in the summary record at the bottom of each record collection.
		/// </summary>
		Bottom			= 0x4,

		/// <summary>
		/// Same as <b>Bottom</b> except the summary record is fixed (non-scrolling) so that
		/// it remains in view when other data reocrds are scrolled.
		/// </summary>
		BottomFixed		= 0x8,

		/// <summary>
		/// This option only makes a difference when there are group-by records. When there 
		/// are group-by records, by default the summary records are displayed at all levels. 
		/// With this option, the summary record is displayed only for top-level record 
		/// collection.
		/// </summary>
		TopLevelOnly	= 0x10,

		/// <summary>
		/// Summary records are displayed for data record collections only. When there 
		/// are group-by records, the summary records are not displayed for group-by record 
		/// collections.
		/// </summary>
		DataRecordsOnly	= 0x20,

		/// <summary>
		/// Summaries are displayed in each group-by record.
		/// </summary>
		InGroupByRecords = 0x40
	}

	#endregion // SummaryDisplayAreas

	#region SummaryPosition

	
	
	/// <summary>
	/// Used for specifying where to position the summary in the summary record.
	/// </summary>
	/// <remarks>
	/// <seealso cref="SummaryDefinition.Position"/>
	/// </remarks>
	public enum SummaryPosition
	{
		/// <summary>
		/// Default is resolved to <b>UseSummaryPositionField</b>.
		/// </summary>
		Default		= 0,

		/// <summary>
		/// Position the summary left in free-form area of the summary record.
		/// </summary>
		Left		= 1,

		/// <summary>
		/// Position the summary center in free-form area of the summary record.
		/// </summary>
		Center		= 2,

		/// <summary>
		/// Position the summary right in free-form area of the summary record.
		/// </summary>
		Right		= 3,

		/// <summary>
		/// Position the summary in the summary record aligned with the field 
		/// specified by the <see cref="SummaryDefinition.PositionFieldName"/> 
		/// property. If <i>SummaryPositionField</i> is not set then the summary 
		/// will be aligned with the summary�s <see cref="SummaryDefinition.SourceFieldName"/>.
		/// </summary>
		UseSummaryPositionField = 4
	}

	#endregion // SummaryPosition

	#region SummaryUIType

	
	
	/// <summary>
	/// Used for specifying what type of user interface will be presented for 
	/// selecting summary calculations to perform.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryUIType</b> is used for specifying <see cref="FieldSettings.SummaryUIType"/> property.
	/// To actually enable the user interface, set the <see cref="FieldSettings.AllowSummaries"/> property.
	/// </para>
	/// <seealso cref="FieldSettings.SummaryUIType"/>
	/// <seealso cref="FieldSettings.AllowSummaries"/>
	/// </remarks>
	public enum SummaryUIType
	{
		/// <summary>
		/// Default is resolved to <b>MultiSelectForNumericsOnly</b>.
		/// </summary>
		Default					= 0,

		/// <summary>
		/// User is allowed to add only one summary per field via the UI. 
		/// Note that this does not prevent adding multiple summaries per 
		/// field via code.
		/// </summary>
		SingleSelect			= 1,

		/// <summary>
		/// This option will enable the summaries UI for numeric columns 
		/// only. For data types such as String or DateTime only a limited 
		/// subset of summaries are valid like Minimum, Maximum etc. Summaries 
		/// such as Average, Sum etc� are not applicable to them. This option 
		/// is a way to only display summaries UI for fields that are numeric 
		/// types.
		/// </summary>
		SingleSelectForNumericsOnly = 2,

		/// <summary>
		/// User is allowed to add multiple summaries per field.
		/// </summary>
		MultiSelect				= 3,

		/// <summary>
		/// Same as <b>SingleSelectForNumericsOnly</b> except this option will 
		/// allow multiple summaries per field.
		/// </summary>
		MultiSelectForNumericsOnly = 4
	}

	#endregion // SummaryUIType

	#region SupportDataErrorInfo

	// SSP 4/9/09 - NAS9.2 IDataErrorInfo Support
	//
	/// <summary>
	/// Enum for specifying <see cref="FieldLayoutSettings.SupportDataErrorInfo"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName=FeatureInfo.FeatureName_IDataErrorInfo, Version=FeatureInfo.Version_9_2 )]
	public enum SupportDataErrorInfo
	{
		/// <summary>
		/// Default. Default is resolved to None.
		/// </summary>
		Default,

		/// <summary>
		/// None. IDataErrorInfo error information is not displayed.
		/// </summary>
		None,

		/// <summary>
		/// Records only. IDataErrorInfo's record Error is displayed. By default the data presenter
		/// displays this error information in the form of an error icon inside the record
		/// selector. Individual cell error information is not not displayed.
		/// When the mouse is hovered over the error icon, a tooltip
		/// with the error text is displayed.
		/// </summary>
		RecordsOnly,

		/// <summary>
		/// Cells only. IDataErrorInfo's cell error information is displayed. Record error 
		/// information is not displayed. Each cell's error information is displayed inside the cell
		/// in the form of an error icon. When the mouse is hovered over the error icon, a tooltip
		/// with the error text is displayed.
		/// </summary>
		CellsOnly,

		/// <summary>
		/// Both records and cells. IDataErrorInfo's record error as well as individual cell error
		/// information is displayed.
		/// </summary>
		RecordsAndCells
	}

	#endregion // SupportDataErrorInfo

    #region CellPageSpanStrategy

    /// <summary>
    /// Determines the strategy used during printing when a cell is encountered that will not fit on a page. 
    /// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> This also applies to field labels as well so that the header area will line up properly.</para>
    /// </remarks>
    public enum CellPageSpanStrategy
    {
        /// <summary>
        /// If a cell can't fit on a page it will be moved to the next page and the previous cell will be expanded to fill.
        /// </summary>
        NextPageFillWithPreviousCell = 1,

        /// <summary>
        /// If a cell can't fit on a page it will be moved to the next page.
        /// </summary>
        NextPage = 2,

        /// <summary>
        /// If a cell can't fit on a page it will be clipped and the portion that couldn't fit will be shown on the next page.
        /// </summary>
        Continue = 3,

    };

    #endregion // CellPageSpanStrategy

	// AS 3/10/11 NA 2011.1 - Async Exporting
	#region UIOperation
	internal enum UIOperation
	{
		ClearCellContents,
		Edit,
		FieldMoving,
		FieldFixing,
		FieldGrouping,
		FieldVisibility,
		FieldResizing,
		FieldAutoSizing,
		FieldSorting,
		Paste,
		FieldSummaries,
		RecordResizing,
		RecordFixing,
		RecordFiltering,
		RecordExpandCollapse,
		DeleteRecord,
		Undo,
	} 
	#endregion //UIOperation

    #region UpdateMode

    /// <summary>
	/// Used for specifying how the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/> is updated when any changes are 
	/// made to the data displayed in the <see cref="DataPresenterBase"/>. The default UpdateMode is OnRecordChangeOrLostFocus.
	/// </summary>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="DataPresenterBase.UpdateMode"/>
	/// <seealso cref="DataPresenterBase.RecordUpdating"/>
	/// <seealso cref="DataPresenterBase.RecordUpdated"/>
	/// <seealso cref="DataPresenterBase.RecordUpdateCanceling"/>
	/// <seealso cref="DataPresenterBase.RecordUpdateCanceled"/>
	/// <seealso cref="DataRecord.IsDataChanged"/>
	/// <seealso cref="DataRecord.Update()"/>
	/// <seealso cref="DataRecord.CancelUpdate"/>
	/// <seealso cref="DataPresenterCommands"/>
	/// <seealso cref="DataPresenterCommands.CommitChangesToAllRecords"/>
	/// <seealso cref="DataPresenterCommands.CommitChangesToActiveRecord"/>
	/// <seealso cref="DataPresenterCommands.DiscardChangesToAllRecords"/>
	/// <seealso cref="DataPresenterCommands.DiscardChangesToActiveRecord"/>
	/// <seealso cref="DataPresenterBase.ExecuteCommand(RoutedCommand)"/>
	public enum UpdateMode
	{
		/// <summary>
		/// The DataSource is updated when the user modifies cell(s) in a record
		/// and then activates a different record or the DataPresenterBase loses focus.
		/// </summary>
		OnRecordChangeOrLostFocus = 0,

		/// <summary>
		/// The DataSource is updated when the user modifies cell(s) in a record
		/// and then activates a different record
		/// </summary>
		OnRecordChange = 1,

		/// <summary>
		/// The DataSource is updated when the user modifies a cell and
		/// exits the edit mode or when the <see cref="DataPresenterBase"/> loses the focus.
		/// </summary>
		OnCellChangeOrLostFocus = 2,

		/// <summary>
		/// The DataSource is updated when the user modifies a cell and
		/// exits the edit mode
		/// </summary>
		OnCellChange = 3,

		/// <summary>
		/// The DataSource is updated only when the <see cref="DataPresenterCommands.CommitChangesToAllRecords"/> or <see cref="DataPresenterCommands.CommitChangesToActiveRecord"/> command is executed. 
		/// </summary>
		OnUpdate = 4
	};

	#endregion // UpdateMode

	#region ViewStateChangedAction

	/// <summary>
	/// A list of possible actions that should be taken when the state of a View changes.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase.ViewStateChangedEvent"/>
	public enum ViewStateChangedAction
	{
		/// <summary>
		/// The target(s) of the View state change should invalidate their FieldLayouts.
		/// </summary>
		/// <remarks>
		/// This action is normally directed at the DataPresenter ViewStateChangedTarget.
		/// </remarks>
		InvalidateFieldLayouts = 0,
		/// <summary>
		/// The target(s) of the View state change should invalidate their arrange.
		/// </summary>
		InvalidateArrange = 1,
		/// <summary>
		/// The target(s) of the View state change should invalidate their measure.
		/// </summary>
		InvalidateMeasure = 2,
		/// <summary>
		/// The target(s) of the View state change should invalidate render.
		/// </summary>
		InvalidateRender = 3,
	}

	#endregion ViewStateChangedAction
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