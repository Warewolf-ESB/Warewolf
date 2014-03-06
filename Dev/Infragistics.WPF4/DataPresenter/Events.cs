using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
//using Infragistics.Windows.Input;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Editors;
using Infragistics.Windows.DataPresenter;
using System.Collections;
using System.Collections.ObjectModel;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Internal;
using System.Diagnostics;
using System.Collections.Generic;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.DataPresenter.Events
{
	#region AssigningFieldLayoutToItemEventArgs

    /// <summary>
    /// Event arguments for routed event AssigningFieldLayoutToItem
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.AssigningFieldLayoutToItem"/>
    public class AssigningFieldLayoutToItemEventArgs : RoutedEventArgs
	{
		#region Private Members

		private object _item;
		private IEnumerable _containingList;
		private FieldLayout _fieldLayout = null;
		private bool _isAddRecord;
        private int _listIndex;

		// JJD 8/1/07 - Optimization
		// Added support for lazing searching for field layout
		private DataPresenterBase _dataPresenter;
		private RecordCollectionBase _parentCollection;
		private PropertyDescriptorProvider _propertyDescProvider;
		private bool _isFieldLayoutExplicitlySet;

		#endregion //Private Members

		/// <summary>
		/// Initializes a new instance of the <see cref="AssigningFieldLayoutToItemEventArgs"/> class
		/// </summary>
		/// <param name="item">The item for which the field layout is being associated.</param>
		/// <param name="containingList">The list containing the <paramref name="item"/></param>
		/// <param name="fieldLayout">The <b>FieldLayout</b> that is being assigned to the item</param>
		/// <param name="isAddRecord">True if the field layout is for the template add record.</param>
		public AssigningFieldLayoutToItemEventArgs(object item, IEnumerable containingList, FieldLayout fieldLayout, bool isAddRecord)
		{
			this._containingList	= containingList;
			this._item				= item;
			this._fieldLayout		= fieldLayout;
			this._isAddRecord		= isAddRecord;
		}
		// JJD 8/1/07 - Optimization
		// Added support for lazing searching for field layout
        // JJD 10/02/08 added index param to support printing so we can default to
        // the associated record's FieldLayout  
        //internal AssigningFieldLayoutToItemEventArgs(object item, IEnumerable containingList, FieldLayout fieldLayout, bool isAddRecord, DataPresenterBase dataPresenter, RecordCollectionBase parentCollection, PropertyDescriptorProvider propertyDescProvider) 
		internal AssigningFieldLayoutToItemEventArgs(object item, IEnumerable containingList, FieldLayout fieldLayout, bool isAddRecord, DataPresenterBase dataPresenter, RecordCollectionBase parentCollection, PropertyDescriptorProvider propertyDescProvider, int listIndex) 
			: this(item, containingList, fieldLayout, isAddRecord)
		{
			this._dataPresenter			= dataPresenter;
			this._parentCollection		= parentCollection;
			this._propertyDescProvider	= propertyDescProvider;
            // JJD 10/02/08 added index param to support printing so we can default to
            // the associated record's FieldLayout  
            this._listIndex = listIndex;
		}

		/// <summary>
		/// The container of the item (read-only)
		/// </summary>
		public IEnumerable ContainingList { get { return this._containingList; } }

		/// <summary>
		/// The item (read-only)
		/// </summary>
		/// <remarks>This will return null if <see cref="IsAddRecord"/> is true.</remarks>
		public Object Item { get { return this._item; } }

		/// <summary>
        /// Gets/sets the FieldLayout assigned to this item
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> The search through the existing <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayouts"/> collection for a compatible <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/> is now done lazily in the get of this property. 
		/// Therefore, if this property is explicitly set in an event handler for the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.AssigningFieldLayoutToItem"/> event then the search is bypassed completely.</para>
		/// <para class="note"></para>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout
		{
			// JJD 8/1/07 - Optimization
			// Added support for lazing searching for field layout
			//get	{ return this._fieldLayout;	}
			get	
			{
				if (this._fieldLayout == null && this._dataPresenter != null)
				{

                    // JJD 10/02/08
                    // If we know the index and we are in a DataPresenterReportControl then
                    // get the fieldlayout associated with the corresponding record's FieldLayout
                    if (this._listIndex >= 0)
                    {
                        // MBS 7/31/09 - NA9.2 Excel Exporting
                        //DataPresenterReportControl dprc = this._dataPresenter as DataPresenterReportControl;
                        DataPresenterExportControlBase dprc = this._dataPresenter as DataPresenterExportControlBase;

                        if (dprc != null && this._parentCollection != null)
                        {
                            RecordManager rm = this._parentCollection.ParentRecordManager.AssociatedRecordManager;

                            if (rm != null && this._listIndex < rm.Unsorted.Count)
                            {
                                DataRecord dr = rm.Unsorted[this._listIndex];

                                if (dr != null)
                                {
                                    // Get the object for record comparison
                                    object dataItemForComparison = DataRecord.GetObjectForRecordComparision(this._item);

                                    bool isSameItem = dataItemForComparison == dr.DataItemForComparison;

                                    Debug.Assert(isSameItem == true, "Data items don't match");

                                    if ( isSameItem )
                                        this._fieldLayout = dprc.GetClonedFieldLayout(dr.FieldLayout);
                                }
                            }
                        }
                    }

                    if (this._fieldLayout == null)
                    {
                        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                        // Added parentCollection parameter
                        //this._fieldLayout = this._dataPresenter.FieldLayouts.GetDefaultLayoutForItem(this._item, this._containingList, this._propertyDescProvider);
                        this._fieldLayout = this._dataPresenter.FieldLayouts.GetDefaultLayoutForItem(this._item, this._containingList, this._parentCollection, this._propertyDescProvider);
                    }

					// JJD 2/22/07 - BR20439
					// for add records use the collection's fl if necessary
					if (this._fieldLayout == null && this._isAddRecord && this._parentCollection != null)
						this._fieldLayout = this._parentCollection.FieldLayout;
				}

				return this._fieldLayout;	
			}

			set	
			{
				if (value != this._fieldLayout)
				{
					this._fieldLayout = value;

					// JJD 8/1/07 - Optimization
					// Added support for lazing searching for field layout
					this._isFieldLayoutExplicitlySet = this._fieldLayout != null;
				}
			}
		}

		/// <summary>
		/// True if this is a template add record  about to be initialized (read-only)
		/// </summary>
		public bool IsAddRecord { get { return this._isAddRecord; } }

        // JJD 6/1/09 - NA 2009 vol 2 - Cross band grouping
        /// <summary>
        /// Returns the parent <see cref="ExpandableFieldRecord"/> or null (read-only).
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public ExpandableFieldRecord ParentExpandableFieldRecord
        {
            get
            {
                Record parentRecord = (this._parentCollection != null) ? this._parentCollection.ParentRecord : null;

                // walk up the parent chain until we get to the ExpandableFieldRecord.
                // This will bypass any GroupByRecords
                while (parentRecord != null)
                {
                    if (parentRecord is ExpandableFieldRecord)
                        return parentRecord as ExpandableFieldRecord;

                    parentRecord = parentRecord.ParentRecord;
                }

                return null;
            }
        }

		// JJD 8/1/07 - Optimization
		// Added support for lazing searching for field layout
		internal bool IsFieldLayoutExplicitlySet { get { return this._isFieldLayoutExplicitlySet; } }
	}

	#endregion //AssigningFieldLayoutToItemEventArgs

	#region CarouselBreadcrumbClickEventArgs class

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl.CarouselBreadcrumbClick"/>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumb"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl.CarouselBreadcrumbClick"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl.CarouselBreadcrumbClickEvent"/>
	public class CarouselBreadcrumbClickEventArgs : RoutedEventArgs
	{
		private CarouselBreadcrumb _breadcrumb;

		internal CarouselBreadcrumbClickEventArgs(CarouselBreadcrumb breadcrumb)
		{
			this._breadcrumb = breadcrumb;
		}

		/// <summary>
		/// Returns the CarouselBreadcrumb that has just been clicked (read-only).
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumbControl"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.CarouselBreadcrumb"/>
		public CarouselBreadcrumb Breadcrumb { get { return this._breadcrumb; } }
	}

	#endregion //CarouselBreadcrumbClickEventArgs class
    
    #region CellActivatedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivated"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivated"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivatedEvent"/>
    public class CellActivatedEventArgs : RoutedEventArgs
    {
        private Cell _cell;

        internal CellActivatedEventArgs(Cell cell)
        {
            this._cell = cell;
        }

        /// <summary>
        /// Returns the cell that has just been activated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
        public Cell Cell { get { return this._cell; } }
    }

    #endregion //CellActivatedEventArgs

    #region CellActivatingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivating"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivating"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellActivatingEvent"/>
	public class CellActivatingEventArgs : CancelableRoutedEventArgs
    {
        private Cell _cell;

        internal CellActivatingEventArgs(Cell cell)
        {
            this._cell = cell;
        }

        /// <summary>
        /// Returns the cell to be activated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
        public Cell Cell { get { return this._cell; } }
    }

    #endregion //CellActivatingEventArgs
    
    #region CellChangedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellChanged"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellChanged"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellChangedEvent"/>
    public class CellChangedEventArgs : RoutedEventArgs
    {
        private Cell _cell;

		// SSP 5/23/07
		// Added Editor property so the user can access the new value.
		// 
		private ValueEditor _editor;


		// SSP 5/23/07
		// Added Editor property so the user can access the new value.
		// 
        //internal CellChangedEventArgs(Cell cell)
		internal CellChangedEventArgs( Cell cell, ValueEditor editor )
        {
            this._cell = cell;

			// SSP 5/23/07
			// 
			this._editor = editor;
        }

        /// <summary>
        /// Returns the cell that has just been changed (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
        public Cell Cell { get { return this._cell; } }

		// SSP 5/23/07
		// Added Editor property so the user can access the new value.
		// 
		/// <summary>
		/// Returns the editor the cell is using to edit the value. The new value
		/// associated with this event can be accessed using the returned editor's 
		/// Value property.
		/// </summary>
		public ValueEditor Editor
		{
			get
			{
				return this._editor;
			}
		}
    }

    #endregion //CellChangedEventArgs

    #region CellDeactivatingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellDeactivating"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellDeactivating"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellDeactivatingEvent"/>
	public class CellDeactivatingEventArgs : CancelableRoutedEventArgs
    {
        private Cell _cell;

        internal CellDeactivatingEventArgs(Cell cell)
        {
            this._cell = cell;
        }

        /// <summary>
        /// Returns the cell to be de-activated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
        public Cell Cell { get { return this._cell; } }
    }

    #endregion //CellDeactivatingEventArgs

    #region CellUpdatedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellUpdated"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellUpdated"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellUpdatedEvent"/>
    public class CellUpdatedEventArgs : RoutedEventArgs
    {
		private DataRecord _record;
		private Field _field;

		internal CellUpdatedEventArgs(DataRecord record, Field field)
        {
            this._record = record;
            this._field = field;
        }

        /// <summary>
		/// Returns the cell that has just been updated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        public Cell Cell { get { return this._record.Cells[this._field]; } }

        /// <summary>
		/// Returns the Field of the cell that has just been updated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
        public Field Field { get { return this._field; } }

        /// <summary>
		/// Returns the record of the cell that has just been updated (read-only).
        /// </summary>
        /// <seealso cref="DataRecord"/>
        public DataRecord Record { get { return this._record; } }
   }

    #endregion //CellUpdatedEventArgs

    #region CellUpdatingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellUpdating"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellUpdating"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CellUpdatingEvent"/>
	public class CellUpdatingEventArgs : CancelableRoutedEventArgs
    {
        private DataRecord _record;
        private Field _field;

        internal CellUpdatingEventArgs(DataRecord record, Field field)
        {
            this._record = record;
            this._field = field;
        }

        /// <summary>
        /// Returns the cell to be updated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        public Cell Cell { get { return this._record.Cells[this._field]; } }

        /// <summary>
        /// Returns the Field of the cell to be updated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
        public Field Field { get { return this._field; } }

        /// <summary>
        /// Returns the record of the cell to be updated (read-only).
        /// </summary>
        /// <seealso cref="DataRecord"/>
        public DataRecord Record { get { return this._record; } }
    }

    #endregion //CellUpdatingEventArgs

    // AS 4/14/09 NA 2009.2 ClipboardSupport
    #region ClipboardOperationErrorEventArgs
    /// <summary>
    /// Arguments for the <see cref="DataPresenterBase.ClipboardOperationError"/> event
    /// </summary>
    /// <seealso cref="DataPresenterBase.ClipboardOperationError"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public class ClipboardOperationErrorEventArgs : RoutedEventArgs
    {
        #region Member Variables

        private ClipboardErrorAction _action = ClipboardErrorAction.ClearCellAndContinue;
		private DataRecord _record;
		private Field _field;
        private Exception _exception;
        private ClipboardOperation _operation;
        private bool _canContinueWithRemainingCells;
        private string _message;
        private ClipboardError _error;
		private bool _displayErrorMessage = true;

        #endregion //Member Variables

        #region Constructor
        internal ClipboardOperationErrorEventArgs(ClipboardOperation operation, ClipboardError error,
            DataRecord record, Field field, string errorMessage, Exception exception, bool canContinueWithRemainingCells)
        {
			_record = record;
			_field = field;
            _error = error;
            _exception = exception;
            _operation = operation;
            _canContinueWithRemainingCells = canContinueWithRemainingCells;
            _message = errorMessage;
        }
        #endregion //Constructor

        #region Properties

        #region Action
        /// <summary>
        /// Returns or sets a value indicating whether and how to proceed with the specified <see cref="Operation"/>.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default, this will return Continue for read-only cells and ClearCellAndContinue for all others.</p>
        /// <p class="note"><b>Note:</b> This property has no effect if the <see cref="Cell"/> property is null (i.e. when 
        /// the whole operation had failed. For example, if when pasting the number of cells available for 
        /// pasting is smaller than the number of cells being pasted.</p>
        /// </remarks>
        public ClipboardErrorAction Action
        {
            get { return _action; }
            set { _action = value; }
        } 
        #endregion //Action

        #region CanContinueWithRemainingCells

        /// <summary>
        /// Specifies whether the operation can continue with remaining cells.
        /// </summary>
        public bool CanContinueWithRemainingCells
        {
            get
            {
                return _canContinueWithRemainingCells;
            }
        }

        #endregion // CanContinueWithRemainingCells

        #region Cell
        /// <summary>
        /// Returns the cell for which the specified <see cref="Operation"/> could not be performed or null 
        /// if the entire operation could not be performed.
        /// </summary>
        public Cell Cell
        {
            get 
			{
				if (null != _record && null != _field)
					return _record.Cells[_field];

				return null; 
			}
        } 
        #endregion //Cell

		#region DisplayErrorMessage
		/// <summary>
		/// Returns or sets whether an error message should be displayed to the end user.
		/// </summary>
		public bool DisplayErrorMessage
		{
			get { return _displayErrorMessage; }
			set { _displayErrorMessage = value; }
		} 
		#endregion //DisplayErrorMessage

        #region Error
        /// <summary>
        /// Returns an enumeration used to identify the type of error that occurred.
        /// </summary>
        public ClipboardError Error
        {
            get { return _error; }
        } 
        #endregion //Error

        #region Exception
        /// <summary>
        /// Returns the exception, if any, that resulted in the error.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        } 
        #endregion //Exception

		#region Field
		/// <summary>
		/// Returns the <see cref="Field"/> associated with the error.
		/// </summary>
		public Field Field
		{
			get { return _field; }
		} 
		#endregion //Field

        #region Message
        /// <summary>
        /// Gets/sets the message that will be displayed to the user.
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        } 
        #endregion //Message

        #region Operation
        /// <summary>
        /// Returns the type of operation that was being performed when the error occurred.
        /// </summary>
        public ClipboardOperation Operation
        {
            get { return _operation; }
        } 
        #endregion //Operation

		#region Record
		/// <summary>
		/// Returns the record associated with the error.
		/// </summary>
		public DataRecord Record
		{
			get { return _record; }
		} 
		#endregion //Record

        #endregion //Properties
    }
    #endregion //ClipboardOperationErrorEventArgs

    // AS 4/15/09 NA 2009.2 ClipboardSupport
    #region ClipboardCopyingEventArgs
    /// <summary>
    /// Event arguments for the <see cref="DataPresenterBase.ClipboardCopying"/> event
    /// </summary>
    /// <seealso cref="DataPresenterBase.ClipboardCopying"/>
	/// <seealso cref="DataPresenterBase.ClipboardPasting"/>
	/// <seealso cref="ClipboardPastingEventArgs"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowClipboardOperations"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public class ClipboardCopyingEventArgs : ClipboardOperationEventArgs
    {
        #region Constructor
		internal ClipboardCopyingEventArgs(ClipboardCellValueHolderCollection values)
            : base(values)
        {
        }
        #endregion //Constructor

		#region Methods

		#region GetLabelValueHolder
		/// <summary>
		/// Returns the <see cref="CellValueHolder"/> whose value is used when putting the field labels into the clipboard.
		/// </summary>
		/// <param name="column">The 0 based index of the field whose <see cref="CellValueHolder"/> is being requested</param>
		/// <returns>A CellValueHolder or null if the field labels are not going to be included in the clipboard output.</returns>
		/// <remarks>
		/// <p class="body">When cells are copied to the clipboard, the field labels are optionally included in the 
		/// output based on the value of the <see cref="FieldLayoutSettings.CopyFieldLabelsToClipboard"/>. Using this 
		/// method, you can provide the text that will be output to the clipboard for a given field. This is useful when the 
		/// <see cref="Field.Label"/> is set to a non-string value.</p>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.CopyFieldLabelsToClipboard"/>
		/// <seealso cref="GetLabelValueHolder(Field)"/>
		public CellValueHolder GetLabelValueHolder(int column)
		{
			Field f = GetSourceField(column);
			return GetLabelValueHolder(f);
		}

		/// <summary>
		/// Returns the <see cref="CellValueHolder"/> whose value is used when putting the field labels into the clipboard.
		/// </summary>
		/// <param name="field">The field whose <see cref="CellValueHolder"/> is being requested</param>
		/// <returns>A CellValueHolder or null if the field labels are not going to be included in the clipboard output.</returns>
		/// <remarks>
		/// <p class="body">When cells are copied to the clipboard, the field labels are optionally included in the 
		/// output based on the value of the <see cref="FieldLayoutSettings.CopyFieldLabelsToClipboard"/>. Using this 
		/// method, you can provide the text that will be output to the clipboard for a given field. This is useful when the 
		/// <see cref="Field.Label"/> is set to a non-string value.</p>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.CopyFieldLabelsToClipboard"/>
		/// <seealso cref="GetLabelValueHolder(int)"/>
		public CellValueHolder GetLabelValueHolder(Field field)
		{
			return ((ClipboardCellValueHolderCollection)this.Values).GetLabel(field);
		} 
		#endregion //GetLabelValueHolder

		#region GetSourceCell
		/// <summary>
		/// The <see cref="Cell"/> at the specified index.
		/// </summary>
		/// <param name="row">The 0 based index of the row whose cell is to be returned</param>
		/// <param name="column">The 0 based index of the column whose cell is to be returned</param>
		/// <returns>The <see cref="Cell"/> at the specified index or null if there is no cell in the specified position</returns>
		/// <exception cref="ArgumentOutOfRangeException">The row/column must be at least 0 and less than the respective RowCount/ColumnCount count values.</exception>
		public Cell GetSourceCell(int row, int column)
		{
			return this.Values.GetCell(row, column);
		}

		#endregion //GetSourceCell

		#region GetSourceField

		/// <summary>
		/// Returns the <see cref="Field"/> associated with the specified column in the collection
		/// </summary>
		/// <param name="column">The 0 based index of the column whose associated Field is to be returned.</param>
		/// <returns>The <see cref="Field"/> at the specified column index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The column must be at least 0 and less than the ColumnCount.</exception>
		public Field GetSourceField(int column)
		{
			return this.Values.GetField(column);
		}

		#endregion //GetSourceField

		#region GetSourceFieldIndex
		/// <summary>
		/// Helper method to return the index of the specified field or -1 if the field is not a source for the copy operation.
		/// </summary>
		/// <param name="field">The field to locate</param>
		/// <returns>An integer used to identify the offset at which the specified Field exists within the copy operation or -1 if the field is not a source for the copy operation.</returns>
		public int GetSourceFieldIndex(Field field)
		{
			return this.Values.IndexOf(field);
		}
		#endregion //GetSourceFieldIndex

		#region GetSourceRecordIndex
		/// <summary>
		/// Helper method to return the index of the specified record or -1 if the record is not a source for the copy operation.
		/// </summary>
		/// <param name="record">The record to locate</param>
		/// <returns>An integer used to identify the offset at which the specified record exists within the copy operation or -1 if the record is not a source for the copy operation.</returns>
		public int GetSourceRecordIndex(DataRecord record)
		{
			return this.Values.IndexOf(record);
		}
		#endregion //GetSourceRecordIndex

		#region GetSourceRecord
		/// <summary>
		/// Returns the record associated with the specified row in the collection
		/// </summary>
		/// <param name="row">The 0 based index of the row whose associated record is to be returned.</param>
		/// <returns>The <see cref="DataRecord"/> at the specified row index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The row must be at least 0 and less than the RowCount.</exception>
		public DataRecord GetSourceRecord(int row)
		{
			return this.Values.GetRecord(row);
		}

		#endregion //GetSourceRecord

		#endregion //Methods
	}
    #endregion //ClipboardCopyingEventArgs

    // AS 4/15/09 NA 2009.2 ClipboardSupport
    #region ClipboardOperationEventArgs
    /// <summary>
    /// Event arguments for an operation involving copying to or pasting from the clipboard.
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowClipboardOperations"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public abstract class ClipboardOperationEventArgs : CancelableRoutedEventArgs
    {
        #region Member Variables

        private CellValueHolderCollection _values;

        #endregion //Member Variables

        #region Constructor
        internal ClipboardOperationEventArgs(CellValueHolderCollection values)
        {
            _values = values;
        }
        #endregion //Constructor

        #region Properties

		#region FieldCount
		/// <summary>
		/// Returns the number of fields represented in the collection.
		/// </summary>
		public int FieldCount
		{
			get
			{
				return this.Values.FieldCount;
			}
		}
		#endregion //FieldCount

		#region RecordCount
		/// <summary>
		/// Returns the number of records represented in the collection.
		/// </summary>
		public int RecordCount
		{
			get
			{
				return this.Values.RecordCount;
			}
		}
		#endregion //RecordCount

		#region Values
		/// <summary>
        /// Returns a collection of the unconverted values to be used as the source for the clipboard operation.
        /// </summary>
        /// <remarks>
        /// <p class="body">During a cut/copy operation, this will be populated with the current values of the 
        /// cells. To control the text that will be put onto the clipboard for a given cell, you can modify the 
        /// associated <see cref="CellValueHolder"/> setting the <see cref="CellValueHolder.Value"/> to the 
        /// text representation and the <see cref="CellValueHolder.IsDisplayText"/> to true.</p>
        /// <p class="body">During a paste operation, this will be populated with the display text values from 
        /// the information in the clipboard. To control the parsed value that will be used to update the Cell's 
        /// <see cref="Cell.Value"/>, you can modify the associated <b>CellValueHolder</b> setting the 
        /// Value to the value you want to have set on the Cell and the IsDisplayText to false to indicate that the 
        /// value provided is the parsed cell value.</p>
        /// </remarks>
        public CellValueHolderCollection Values
        {
            get { return _values; }
        } 
        #endregion //Values

        #endregion //Properties
    } 
    #endregion //ClipboardOperationEventArgs

    // AS 4/15/09 NA 2009.2 ClipboardSupport
    #region ClipboardPastingEventArgs
    /// <summary>
    /// Event arguments for the <see cref="DataPresenterBase.ClipboardPasting"/> event
    /// </summary>
    /// <seealso cref="DataPresenterBase.ClipboardPasting"/>
	/// <seealso cref="ClipboardCopyingEventArgs"/>
	/// <seealso cref="DataPresenterBase.ClipboardCopying"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowClipboardOperations"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public class ClipboardPastingEventArgs : ClipboardOperationEventArgs
    {
		#region Member Variables

		private IDataObject _dataObject;

		#endregion //Member Variables

        #region Constructor
        internal ClipboardPastingEventArgs(CellValueHolderCollection values, IDataObject dataObject)
            : base(values)
        {
			_dataObject = dataObject;
        }
        #endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the <see cref="IDataObject"/> from which the cell values were obtained.
		/// </summary>
		public IDataObject DataObject
		{
			get { return _dataObject; }
		} 
		#endregion //Properties

		#region Methods

		#region GetTargetCell
		/// <summary>
		/// The <see cref="Cell"/> at the specified index.
		/// </summary>
		/// <param name="row">The 0 based index of the row whose cell is to be returned</param>
		/// <param name="column">The 0 based index of the column whose cell is to be returned</param>
		/// <returns>The <see cref="Cell"/> at the specified index or null if there is no cell in the specified position</returns>
		/// <exception cref="ArgumentOutOfRangeException">The row/column must be at least 0 and less than the respective RowCount/ColumnCount count values.</exception>
		public Cell GetTargetCell(int row, int column)
		{
			return this.Values.GetCell(row, column);
		}

		#endregion //GetTargetCell

		#region GetTargetField

		/// <summary>
		/// Returns the <see cref="Field"/> associated with the specified column in the collection
		/// </summary>
		/// <param name="column">The 0 based index of the column whose associated Field is to be returned.</param>
		/// <returns>The <see cref="Field"/> at the specified column index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The column must be at least 0 and less than the ColumnCount.</exception>
		public Field GetTargetField(int column)
		{
			return this.Values.GetField(column);
		}

		#endregion //GetTargetField

		#region GetTargetFieldIndex
		/// <summary>
		/// Helper method to return the index of the specified field or -1 if the field is not a target of the paste operation.
		/// </summary>
		/// <param name="field">The field to locate</param>
		/// <returns>An integer used to identify the offset at which the specified Field exists within the paste operation or -1 if the field is not a target of the paste operation.</returns>
		public int GetTargetFieldIndex(Field field)
		{
			return this.Values.IndexOf(field);
		} 
		#endregion //GetTargetFieldIndex

		#region GetTargetRecord
		/// <summary>
		/// Returns the record associated with the specified row in the collection
		/// </summary>
		/// <param name="row">The 0 based index of the row whose associated record is to be returned.</param>
		/// <returns>The <see cref="DataRecord"/> at the specified row index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The row must be at least 0 and less than the RowCount.</exception>
		public DataRecord GetTargetRecord(int row)
		{
			return this.Values.GetRecord(row);
		}

		#endregion //GetTargetRecord

		#region GetTargetRecordIndex
		/// <summary>
		/// Helper method to return the index of the specified record or -1 if the record is not a target of the paste operation.
		/// </summary>
		/// <param name="record">The record to locate</param>
		/// <returns>An integer used to identify the offset at which the specified record exists within the paste operation or -1 if the record is not a target of the paste operation.</returns>
		public int GetTargetRecordIndex(DataRecord record)
		{
			return this.Values.IndexOf(record);
		}
		#endregion //GetTargetRecordIndex

		#endregion //Methods
	} 
    #endregion //ClipboardPastingEventArgs

    #region CustomFilterSelectionControlOpeningEventArgs

    // SSP 12/12/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CustomFilterSelectionControlOpening"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CustomFilterSelectionControlOpening"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.CustomFilterSelectionControlOpeningEvent"/>
	public class CustomFilterSelectionControlOpeningEventArgs : CancelableRoutedEventArgs
	{
		private RecordFilter _recordFilter;
		private CustomFilterSelectionControl _control;

		internal CustomFilterSelectionControlOpeningEventArgs( RecordFilter recordFilter, CustomFilterSelectionControl control )
		{
			_recordFilter = recordFilter;
			_control = control;
		}

		/// <summary>
		/// Returns the control being displayed in the UI for selecting custom filter criteria for a field (read-only).
		/// </summary>
		public CustomFilterSelectionControl Control
		{
			get
			{
				return _control;
			}
		}

		/// <summary>
		/// Returns the RecordFilter instance that will be modified by the filter selection control.
		/// </summary>
		public RecordFilter RecordFilter
		{
			get
			{
				return _recordFilter;
			}
		}
	}

	#endregion // CustomFilterSelectionControlOpeningEventArgs

    #region DataErrorEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataError"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataError"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataErrorEvent"/>
    public class DataErrorEventArgs : CancelableRoutedEventArgs
    {
		// AS 5/5/09 NA 2009.2 ClipboardSupport
		//private DataRecord _record;
		//private Field _field;
		//private Exception _exception;
		//private DataErrorOperation _operation;
		//private string _message;
		private DataErrorInfo _info;

        /// <summary>
        /// Constructor
        /// </summary>
		// AS 5/5/09 NA 2009.2 ClipboardSupport
		//internal DataErrorEventArgs(DataRecord record, Field field, Exception exception, DataErrorOperation operation, string message)
		//{
		//    this._record = record;
		//    this._field = field;
		//    this._exception = exception;
		//    this._operation = operation;

		//    if (this._message == null || this._message.Length == 0)
		//        this._message = this._exception.Message;
		//}
		internal DataErrorEventArgs(DataErrorInfo info)
		{
			GridUtilities.ValidateNotNull(info);
			_info = info;
		}

        /// <summary>
        /// Returns the associated <see cref="Infragistics.Windows.DataPresenter.Cell"/> (read-pnly)
        /// </summary>
        public Cell Cell 
		{ 
			get 
			{
				return _info.Cell; 
			} 
		}

        /// <summary>
        /// Returns the associated <see cref="DataRecord"/> (read-pnly)
        /// </summary>
        public DataRecord Record { get { return this._info.Record; } }

        /// <summary>
        /// Returns the exception that was thrown (read-pnly)
        /// </summary>
        public Exception Exception { get { return this._info.Exception; } }

        /// <summary>
        /// Returns the associated <see cref="Infragistics.Windows.DataPresenter.Field"/> (read-pnly)
        /// </summary>
        public Field Field { get { return this._info.Field; } }

        /// <summary>
        /// Gets/sets the message that will be displayed to the user.
        /// </summary>
        public string Message 
		{ 
			get 
			{ 
				return this._info.Message; 
			}
			set
			{
				this._info.Message = value;
			}
		}

        /// <summary>
		/// Returns the operation that was attempted and which triggered a data error (read-pnly)
        /// </summary>
        public DataErrorOperation Operation { get { return this._info.Operation; } }

    }

    #endregion //DataErrorEventArgs

	#region CellsInViewChangedEventArgs Class

	// SSP 2/2/10
	// Added CellsInViewChanged event to the DataPresenterBase.
	// 
	/// <summary>
	/// Event args for <see cref="DataPresenterBase.CellsInViewChanged"/> event.
	/// </summary>
	public class CellsInViewChangedEventArgs : EventArgs
	{
		#region Constructor

		private List<VisibleDataBlock> _currentCellsInView;

		#endregion // Constructor

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="CellsInViewChangedEventArgs"/> class.
		/// </summary>
		/// <param name="currentCellsInView">Current data that's in view.</param>
		public CellsInViewChangedEventArgs( List<VisibleDataBlock> currentCellsInView )
		{
			GridUtilities.ValidateNotNull( currentCellsInView );

			_currentCellsInView = currentCellsInView;
		}

		#endregion // Constructor

		#region CurrentCellsInView

		/// <summary>
		/// Current data that's in view.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>CurrentCellsInView</b> property returns the current cells that are in view. It's
		/// a collection of <see cref="VisibleDataBlock"/> objects where each VisibleDataBlock
		/// represents data records and fields from a RecordManager that are currently in view.
		/// </para>
		/// </remarks>
		/// <seealso cref="VisibleDataBlock"/>
		public List<VisibleDataBlock> CurrentCellsInView
		{
			get
			{
				return _currentCellsInView;
			}
		}

		#endregion // CurrentCellsInView
	}

	#endregion // CellsInViewChangedEventArgs Class

	// JM 6/12/09 NA 2009.2 DataValueChangedEvent
	#region DataValueChangedEventArgs
	/// <summary>
	/// Arguments for the <see cref="DataPresenterBase.DataValueChanged"/> event
	/// </summary>
	/// <seealso cref="DataPresenterBase.DataValueChanged"/>
	/// <seealso cref="DataPresenterBase.InitializeCellValuePresenter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
	public class DataValueChangedEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private DataRecord				_record;
		private Field					_field;
		private CellValuePresenter		_cellValuePresenter;
		private IList<DataValueInfo>	_valueHistory;

		#endregion //Member Variables

		#region Constructor
		internal DataValueChangedEventArgs(DataRecord record, Field field, CellValuePresenter cellValuePresenter, IList<DataValueInfo> valueHistory)
		{
			_record				= record;
			_field				= field;
			_cellValuePresenter = cellValuePresenter;
			_valueHistory		= valueHistory;
		}
		#endregion //Constructor

		#region Properties

			#region CellValuePresenter
			/// <summary>
			/// Returns the <see cref="CellValuePresenter"/> associated with the value change, or null if a <see cref="CellValuePresenter"/> has not yet been allocated.
			/// </summary>
			public CellValuePresenter CellValuePresenter
			{
				get { return _cellValuePresenter; }
			}
			#endregion //CellValuePresenter

			#region Field
			/// <summary>
			/// Returns the <see cref="Field"/> associated with the value change.
			/// </summary>
			public Field Field
			{
				get { return _field; }
			}
			#endregion //Field

			#region Record
			/// <summary>
			/// Returns the <see cref="DataRecord"/> associated with the value change.
			/// </summary>
			public DataRecord Record
			{
				get { return _record; }
			}
			#endregion //Record

			#region ValueHistory
			/// <summary>
			/// Returns an IList containing <see cref="DataValueInfo"/> instances that contain the history of value changes for the <see cref="Cell"/> associated with the value change.
			/// This property will return null if the <see cref="Infragistics.Windows.DataPresenter.FieldSettings.DataValueChangedHistoryLimit"/> has been set to zero.
			/// </summary>
			/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.DataValueChangedHistoryLimit"/>
			public IList<DataValueInfo> ValueHistory
			{
				get { return _valueHistory; }
			}
			#endregion //ValueHistory

		#endregion //Properties
	}
	#endregion //DataValueChangedEventArgs

	#region EditModeEventArgs

	/// <summary>
	/// Abtract base class for <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>'s edit mode events 
	/// </summary>
	/// <seealso cref="EditModeStartingEventArgs"/>
	/// <seealso cref="EditModeStartedEventArgs"/>
	/// <seealso cref="EditModeEndingEventArgs"/>
	/// <seealso cref="EditModeEndedEventArgs"/>
	public abstract class EditModeEventArgs : RoutedEventArgs
	{
		private Cell _cell;
		private ValueEditor _editor;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeEventArgs"/> class
		/// </summary>
		/// <param name="cell">Cell associated with the edit mode event</param>
		/// <param name="editor">Editor associated with the edit mode event</param>
		protected EditModeEventArgs( Cell cell, ValueEditor editor )
		{
			this._cell = cell;
			this._editor = editor;
		}

		/// <summary>
		/// Returns the <see cref="Infragistics.Windows.DataPresenter.Cell"/> object (read-only)
		/// </summary>
		public Cell Cell
		{
			get { return this._cell; }
		}

		/// <summary>
		/// Returns the <see cref="ValueEditor"/> (read-only)
		/// </summary>
		public ValueEditor Editor
		{
			get { return this._editor; }
		}

	}

	#endregion // EditModeEventArgs

	#region EditModeStartingEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarting"/>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarting"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarted"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnding"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnded"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStartingEvent"/>
	public class EditModeStartingEventArgs : EditModeEventArgs
	{
		private bool _cancel;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeStartingEventArgs"/> class
		/// </summary>
		/// <param name="cell">Cell that is about to enter edit mode</param>
		/// <param name="editor">The editor that will be used for the cell</param>
		public EditModeStartingEventArgs(Cell cell, ValueEditor editor) : base(cell, editor)
		{
		}

		/// <summary>
		/// If set to true will cancel the operation
		/// </summary>
		public bool Cancel
		{
			get { return this._cancel; }
			set { this._cancel = value; }
		}

	}

	#endregion //EditModeStartingEventArgs

	#region EditModeStartedEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarted"/>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarting"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarted"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnding"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnded"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStartedEvent"/>
	public class EditModeStartedEventArgs : EditModeEventArgs
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeStartedEventArgs"/> class
		/// </summary>
		/// <param name="cell">The cell that has entered edit mode</param>
		/// <param name="editor">The editor being used for the cell in edit mode</param>
		public EditModeStartedEventArgs(Cell cell, ValueEditor editor) : base(cell, editor)
		{
		}
	}

	#endregion //EditModeStartedEventArgs

	#region EditModeEndingEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnding"/>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarting"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarted"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnding"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnded"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEndingEvent"/>
	public class EditModeEndingEventArgs : EditModeEventArgs
	{
		private bool _cancel;
		private bool _force;
		private bool _acceptChanges;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeEndingEventArgs"/> class
		/// </summary>
		/// <param name="cell">The cell that is about to exit edit mode</param>
		/// <param name="editor">The editor that is being used for the cell</param>
		/// <param name="acceptChanges">True if the changes will be accepted; otherwise false.</param>
		/// <param name="force">True if the edit mode is being forced to end</param>
		public EditModeEndingEventArgs(Cell cell, ValueEditor editor, bool acceptChanges, bool force)
			: base(cell, editor)
		{
			this._acceptChanges = acceptChanges;
			this._force = force;
		}

		/// <summary>
		/// Gets/sets whether any changes will be accepted.
		/// </summary>
		public bool AcceptChanges
		{
			get { return this._acceptChanges; }
			set { this._acceptChanges = value; }
		}

		/// <summary>
		/// If set to true will cancel the operation
		/// </summary>
		public bool Cancel
		{
			get { return this._cancel; }
			set
			{
				if (value == true &&
					 this._force == true)
					throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_5" ) );

				this._cancel = value;
			}
		}

		/// <summary>
		/// Indicates a forced exit of edit mode (read-only)
		/// </summary>
		public bool Force
		{
			get { return this._force; }
		}
	}

	#endregion //EditModeEndingEventArgs

	#region EditModeEndedEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnded"/>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarting"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarted"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnding"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnded"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEndedEvent"/>
	public class EditModeEndedEventArgs : EditModeEventArgs
	{
		private bool _changesAccepted;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeEndedEventArgs"/> class
		/// </summary>
		/// <param name="cell">The cell whose edit mode has ended</param>
		/// <param name="editor">The editor for which the cell has ended edit mode</param>
		/// <param name="changesAccepted">True if the changes were accepted</param>
		public EditModeEndedEventArgs(Cell cell, ValueEditor editor, bool changesAccepted) : base(cell, editor)
		{
			this._changesAccepted = changesAccepted;
		}

		/// <summary>
		/// Returns whether the changes were accepted (read-only).
		/// </summary>
		public bool ChangesAccepted
		{
			get { return this._changesAccepted; }
		}
	}

	#endregion //EditModeEndedEventArgs

	#region EditModeValidationErrorEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeValidationError"/>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeValidationError"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeValidationErrorEvent"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarting"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeStarted"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnding"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.EditModeEnded"/>
	public class EditModeValidationErrorEventArgs : EditModeEventArgs
	{
		private bool _forceExitEditMode;
		private Exception _exception;
		private string _errorMessage;
		private InvalidValueBehavior _invalidValueBehavior;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditModeValidationErrorEventArgs"/> class
		/// </summary>
		/// <param name="cell">The cell whose validation has resulted in an error</param>
		/// <param name="editor">The editor associated with the edit mode</param>
		/// <param name="errorMessage">The error message for the validation error</param>
		/// <param name="exception">The exception for the validation error</param>
		/// <param name="forceExitEditMode">True if edit mode is being forced to end</param>
		public EditModeValidationErrorEventArgs( Cell cell, ValueEditor editor,
			bool forceExitEditMode, Exception exception, string errorMessage )
			: base( cell, editor )
		{
			_forceExitEditMode = forceExitEditMode;
			_exception = exception;
			_errorMessage = errorMessage;
			_invalidValueBehavior = cell.Field.InvalidValueBehaviorResolved;
		}

		/// <summary>
		/// Indicates if the edit mode is being exitted forcefully. For example, when the
		/// application is being closed.
		/// </summary>
		public bool ForceExitEditMode
		{
			get { return _forceExitEditMode; }
		}

		/// <summary>
		/// Gets or sets the invalid value behavior.
		/// </summary>
		public InvalidValueBehavior InvalidValueBehavior
		{
			get
			{
				return _invalidValueBehavior;
			}
			set
			{
				_invalidValueBehavior = value;
			}
		}

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
			set
			{
				_errorMessage = value;
			}
		}

		/// <summary>
		/// Gets any exception associated with the validation error.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return _exception;
			}
		}
	}

	#endregion //EditModeValidationErrorEventArgs

	#region FieldLayoutInitializingEventArgs

    /// <summary>
    /// Event arguments for routed event <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutInitializing"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutInitializing"/>
    public class FieldLayoutInitializingEventArgs : RoutedEventArgs
	{
        private FieldLayout _layout;
	
        /// <summary>
		/// Initializes a new instance of the <see cref="FieldLayoutInitializingEventArgs"/> class
		/// </summary>
		/// <param name="layout">The associated <see cref="FieldLayout"/> that is being initialized</param>
        public FieldLayoutInitializingEventArgs(FieldLayout layout)
		{
            this._layout = layout;
		}

        /// <summary>
        /// Returns the <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/> being initialized (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout { get { return this._layout; } }

	}

	#endregion //FieldLayoutInitializingEventArgs

	#region FieldLayoutInitializedEventArgs

    /// <summary>
    /// Event arguments for routed event <b>DataPresenterBase.FieldLayoutInitialized</b>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutInitialized"/>
    public class FieldLayoutInitializedEventArgs : RoutedEventArgs
	{
        private FieldLayout _layout;
	
        /// <summary>
		/// Initializes a new instance of the <see cref="FieldLayoutInitializedEventArgs"/> class
		/// </summary>
		/// <param name="layout">The <see cref="FieldLayout"/> that has been initialized</param>
        public FieldLayoutInitializedEventArgs(FieldLayout layout)
		{
            this._layout = layout;
		}

        /// <summary>
        /// Returns the <b>FieldLayout</b> being initialized (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout { get { return this._layout; } }

	}

	#endregion //FieldLayoutInitializedEventArgs

	#region FieldPositionChangingEventArgs

	
	
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldPositionChanging"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldPositionChanging"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldPositionChanged"/>
	public class FieldPositionChangingEventArgs : CancelableRoutedEventArgs
	{
		private Field _field;

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        private FieldPositionChangeReason _changeReason = FieldPositionChangeReason.Moved;

		internal FieldPositionChangingEventArgs( Field field )
		{
			_field = field;
		}

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        internal FieldPositionChangingEventArgs(Field field, FieldPositionChangeReason changeReason)
            : this(field)
		{
            this._changeReason = changeReason;
		}

		/// <summary>
		/// Returns the field that's being repositioned.
		/// </summary>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        /// <summary>
        /// Returns a value indicating what caused the change in position.
        /// </summary>
        public FieldPositionChangeReason ChangeReason
        {
            get { return this._changeReason; }
        }
	}

	#endregion // FieldPositionChangingEventArgs

	#region FieldPositionChangedEventArgs

	
	
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldPositionChanged"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldPositionChanged"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldPositionChanging"/>
	public class FieldPositionChangedEventArgs : RoutedEventArgs
	{
		private Field _field;

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        private FieldPositionChangeReason _changeReason = FieldPositionChangeReason.Moved;

		internal FieldPositionChangedEventArgs( Field field )
		{
			_field = field;
		}

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        internal FieldPositionChangedEventArgs(Field field, FieldPositionChangeReason changeReason)
            : this(field)
		{
            this._changeReason = changeReason;
		}

		/// <summary>
		/// Returns the field that has been repositioned.
		/// </summary>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        /// <summary>
        /// Returns a value indicating what caused the change in position.
        /// </summary>
        public FieldPositionChangeReason ChangeReason
        {
            get { return this._changeReason; }
        }
    }

	#endregion // FieldPositionChangedEventArgs

	#region FieldChooserOpeningEventArgs

	// SSP 6/3/09 - NAS9.2 Field Chooser
	// 	
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldChooserOpening"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldChooserOpening"/>
	[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
	public class FieldChooserOpeningEventArgs : CancelableRoutedEventArgs
	{
		private FieldChooser _fieldChooser;
		private ToolWindow _toolWindow;

		internal FieldChooserOpeningEventArgs( ToolWindow toolWindow, FieldChooser fieldChooser )
		{
			_toolWindow = toolWindow;
			_fieldChooser = fieldChooser;
		}

		/// <summary>
		/// The <see cref="FieldChooser"/> that the data presenter is about to display.
		/// </summary>
		public FieldChooser FieldChooser
		{
			get
			{
				return _fieldChooser;
			}
		}

		/// <summary>
		/// Gets the ToolWindow being used to display the FieldChooser.
		/// </summary>
		public ToolWindow ToolWindow
		{
			get
			{
				return _toolWindow;
			}
		}
	}

	#endregion // FieldChooserOpeningEventArgs

	#region GroupingEventArgs

	/// <summary>
    /// Event arguments for routed event <b>DataPresenterBase.Grouping</b>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Grouping"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Grouped"/>
    public class GroupingEventArgs : CancelableRoutedEventArgs
	{
		private FieldLayout _layout;
		private FieldSortDescription[] _groups;
	
        /// <summary>
		/// Initializes a new instance of the <see cref="GroupingEventArgs"/> class
		/// </summary>
		/// <param name="layout">The associated <see cref="FieldLayout"/></param>
		/// <param name="groups">The set of <see cref="FieldSortDescription"/> instances that will determine the new grouping</param>
        public GroupingEventArgs(FieldLayout layout, FieldSortDescription[] groups)
		{
			this._groups			= groups;
            this._layout			= layout;
		}

        /// <summary>
		/// Returns the associated <b>FieldLayout</b> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout { get { return this._layout; } }

        /// <summary>
        /// Returns the set of <b>FieldSortDecription</b>s that will determine the new grouping (read-only)
        /// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
        public FieldSortDescription[] Groups { get { return this._groups; } }

	}

	#endregion //GroupingEventArgs

	#region GroupedEventArgs

    /// <summary>
    /// Event arguments for routed event <b>DataPresenterBase.Grouped</b>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Grouping"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Grouped"/>
    public class GroupedEventArgs : RoutedEventArgs
	{
		private FieldLayout _layout;
		private FieldSortDescription[] _groups;
	
        /// <summary>
		/// Initializes a new instance of the <see cref="GroupedEventArgs"/> class
		/// </summary>
		/// <param name="layout">The associated <see cref="FieldLayout"/></param>
		/// <param name="groups">The set of <see cref="FieldSortDescription"/> instances that defined the new grouping</param>
        public GroupedEventArgs(FieldLayout layout, FieldSortDescription[] groups)
		{
			this._layout	= layout;
            this._groups	= groups;
		}

        /// <summary>
		/// Returns the associated <b>FieldLayout</b> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout { get { return this._layout; } }

		/// <summary>
		/// Returns the set of <b>FieldSortDecription</b>s that defines the new grouping (read-only)
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
		public FieldSortDescription[] Groups { get { return this._groups; } }

	}

	#endregion //GroupedEventArgs

    #region HeaderTemplateInitializingEventArgs
	
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

    #endregion //HeaderTemplateInitializingEventArgs

    #region HeaderTemplateInitializedEventArgs
	
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

    #endregion //HeaderTemplateInitializedEventArgs

	// JM 6/12/09 NA 2009.2 DataValueChangedEvent
	#region InitializeCellValuePresenterEventArgs
	/// <summary>
	/// Arguments for the <see cref="DataPresenterBase.InitializeCellValuePresenter"/> event
	/// </summary>
	/// <seealso cref="DataPresenterBase.InitializeCellValuePresenter"/>
	/// <seealso cref="DataPresenterBase.DataValueChanged"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
	public class InitializeCellValuePresenterEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private CellValuePresenter	_cellValuePresenter;
		private bool				_isNew;

		#endregion //Member Variables

		#region Constructor
		internal InitializeCellValuePresenterEventArgs(CellValuePresenter cellValuePresenter, bool isNew)
		{
			_cellValuePresenter = cellValuePresenter;
			_isNew				= isNew;
		}
		#endregion //Constructor

		#region Properties

			#region CellValuePresenter
			/// <summary>
			/// Returns the <see cref="CellValuePresenter"/> associated with the value change, or null if a <see cref="CellValuePresenter"/> has not yet been allocated.
			/// </summary>
			public CellValuePresenter CellValuePresenter
			{
				get { return _cellValuePresenter; }
			}
			#endregion //CellValuePresenter

			#region IsNew
			/// <summary>
			/// Returns true if the <see cref="CellValuePresenter"/> is newly allocated, false if it is recycled.
			/// </summary>
			public bool IsNew
			{
				get { return _isNew; }
			}
			#endregion //IsNew

		#endregion //Properties
	}
	#endregion //InitializeCellValuePresenterEventArgs

    #region InitializeRecordEventArgs

    /// <summary>
    /// Event arguments for routed event <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.InitializeRecord"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.InitializeRecord"/>
    public class InitializeRecordEventArgs : RoutedEventArgs
    {
		#region Private Members

		private Record _record;

		// SSP 3/3/09 TFS11407
		// 
		private bool _reInitialize;

		// JJD 11/17/11 - TFS78651 - Added
		private bool _sortValueChanged;

		#endregion //Private Members	
    
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="InitializeRecordEventArgs"/> class
		/// </summary>
		/// <param name="record">The record being initialized</param>
		/// <param name="reInitialize">Indicates if the record is being re-initialized (for example when a cell's value is changed).</param>
		// SSP 3/3/09 TFS11407
		// Added reInitialize parameter. Added code to raise InitializeRecord event 
		// whenever a cell value changes.
		// 
		// JJD 11/17/11 - TFS78651 
		// Added internal ctor and moved initialization logic into it
		//public InitializeRecordEventArgs(Record record)
		public InitializeRecordEventArgs(Record record, bool reInitialize) : this(record, reInitialize, false) { }
		
		// JJD 11/17/11 - TFS78651 
		// Added sortValueChanged
		internal InitializeRecordEventArgs(Record record, bool reInitialize, bool sortValueChanged)
		{
			this._record = record;

			
			
			_reInitialize = reInitialize;

			_sortValueChanged = sortValueChanged;
		}

		#endregion //Constructors	
    
		#region Public Properties

		#region Record

		/// <summary>
		/// Returns the <see cref="Record"/> (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record Record { get { return this._record; } }

		#endregion //Record

		#region IsPreparingForSort

		// JJD 12/03/08 - added
		/// <summary>
		/// Returns whether this record is being created and initialized in preparation for a sort operation (read-only)
		/// </summary>
		/// <value>Returns true if the record is being created as the result of reset notification from the data source and their are sort criteria specified for the <see cref="FieldLayout"/></value>
		/// <seealso cref="FieldLayout.HasSortedFields"/>
		public bool IsPreparingForSort
		{
			get
			{
				FieldLayout fl = this._record.FieldLayout;

				if (fl == null ||
					 fl.HasSortedFields == false)
					return false;

				RecordManager rm = this._record.ParentCollection.ParentRecordManager;

				// JM 02-20-09 
				//return rm != null && rm.IsInReset;
				return rm != null && (rm.IsInReset || rm.IsInVerifySort);
			}
		}

		#endregion //IsPreparingForSort

		#region ReInitialize

		// SSP 3/3/09 TFS11407
		// Added reInitialize parameter. Added code to raise InitializeRecord event 
		// whenever a cell value changes.
		// 
		/// <summary>
		/// Indicates if the record is being re-initialized.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// InitializeRecord is raised for a record when it's created. It's also raised whenever 
		/// the value of one of its cells is changed. In the later case, the <b>ReInitialize</b> will be 
		/// set to <b>true</b> to indicate that this record is being re-initialized.
		/// </para>
		/// </remarks>
		/// <seealso cref="Record"/>
		public bool ReInitialize
		{
			get
			{
				return _reInitialize;
			}
		}

		#endregion //ReInitialize

		// JJD 11/17/11 - TFS78651 - Added
		#region SortValueChanged

		/// <summary>
		/// Indicates whether the event was triggered as a result of a cell value change associated with a Field that is sorted. (read-only)
		/// </summary>
		/// <value>True if a value change occurred from a sorted field, otherwise false.</value>
		/// <remarks>
		/// <para class="body">This property is useful to check before calling the <see cref="Infragistics.Windows.DataPresenter.Record"/>'s <see cref="Infragistics.Windows.DataPresenter.Record.RefreshSortPosition()"/> method.</para>
		/// <para class="note"><b>Note:</b> this property will always return false unless <see cref="ReInitialize"/> is true.</para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.RefreshSortPosition()"/>
		/// <seealso cref="ReInitialize"/>
		public bool SortValueChanged { get { return _sortValueChanged; } }

   		#endregion //SortValueChanged	
    
		#endregion //Public Properties
	}

    #endregion //InitializeRecordEventArgs

    #region InitializeTemplateAddRecordEventArgs

    /// <summary>
    /// Event arguments for routed event <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.InitializeTemplateAddRecord"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.InitializeTemplateAddRecord"/>
    public class InitializeTemplateAddRecordEventArgs : RoutedEventArgs
    {
        private DataRecord _record;

        /// <summary>
		/// Initializes a new instance of the <see cref="InitializeTemplateAddRecordEventArgs"/> class
        /// </summary>
		/// <param name="record">The template add record being initialized</param>
        public InitializeTemplateAddRecordEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the <see cref="DataRecord"/> to be use as an add template. (read-only)
        /// </summary>
        /// <seealso cref="DataRecord"/>
        public DataRecord TemplateAddRecord { get { return this._record; } }
    }

    #endregion //InitializeTemplateAddRecordEventArgs

	#region QuerySummaryResultEventArgs Class

	/// <summary>
	/// Event arguments for routed event <b>DataPresenterBase.QuerySummaryResult</b>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.QuerySummaryResult"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummaryResultChanged"/>
	[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
	public class QuerySummaryResultEventArgs : RoutedEventArgs
	{
		private SummaryResult _summary;
		private object _providedValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="QuerySummaryResultEventArgs"/> class.
		/// </summary>
		/// <param name="summary"></param>		
		public QuerySummaryResultEventArgs( SummaryResult summary )
		{
			_summary = summary;
		}

		/// <summary>
		/// Returns the <see cref="SummaryResult"/> instance that is being recalculated.
		/// </summary>
		public SummaryResult Summary
		{
			get
			{
				return _summary;
			}
		}

		/// <summary>
		/// Returns any value provided by the user via the <see cref="SetSummaryValue"/> method.
		/// </summary>
		internal object ProvidedValue
		{
			get
			{
				return _providedValue;
			}
		}

		/// <summary>
		/// Specifies the summary calculation value. If specified via this method, the data presenter will skip performing
		/// its own calculation.
		/// </summary>
		/// <param name="value">The calcualted summary result value.</param>
		public void SetSummaryValue( object value )
		{
			_providedValue = value;
		}
	}

	#endregion // QuerySummaryResultEventArgs Class

    #region RecordActivatedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivated"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivated"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivatedEvent"/>
    public class RecordActivatedEventArgs : RoutedEventArgs
    {
        private Record _record;

        internal RecordActivatedEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record that has just been activated (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record Record { get { return this._record; } }
    }

    #endregion //RecordActivatedEventArgs

    #region RecordActivatingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivating"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivating"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordActivatingEvent"/>
	public class RecordActivatingEventArgs : CancelableRoutedEventArgs
    {
        private Record _record;

        internal RecordActivatingEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record to be activated (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record Record { get { return this._record; } }
    }

    #endregion //RecordActivatingEventArgs

    #region RecordAddedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAdded"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAdded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAddedEvent"/>
    public class RecordAddedEventArgs : RoutedEventArgs
    {
		private DataRecord _record;

		internal RecordAddedEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record that has just been added (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		public DataRecord Record { get { return this._record; } }
    }

    #endregion //RecordAddedEventArgs

    #region RecordAddingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAdding"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAdding"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAddingEvent"/>
	public class RecordAddingEventArgs : CancelableRoutedEventArgs
    {
		private DataRecord _record;

		internal RecordAddingEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record to be added (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		public DataRecord Record { get { return this._record; } }
    }

    #endregion //RecordAddingEventArgs

    #region RecordCollapsedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordCollapsed"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.RecordPresenter.IsExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordCollapsed"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordCollapsedEvent"/>
    public class RecordCollapsedEventArgs : RoutedEventArgs
    {
        private Record _record;

        internal RecordCollapsedEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record that has just been collapsed (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record Record { get { return this._record; } }
    }

    #endregion //RecordCollapsedEventArgs

    #region RecordCollapsingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordCollapsing"/>
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.RecordPresenter.IsExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordCollapsing"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordCollapsingEvent"/>
	public class RecordCollapsingEventArgs : CancelableRoutedEventArgs
    {
        private Record _record;

        internal RecordCollapsingEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record to be collapsed (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
		public Record Record { get { return this._record; } }
    }

    #endregion //RecordCollapsingEventArgs

    #region RecordContentAreaTemplateInitializingEventArgs
	
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

    #endregion //RecordContentAreaTemplateInitializingEventArgs

    #region RecordContentAreaTemplateInitializedEventArgs
	
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

    #endregion //RecordContentAreaTemplateInitializedEventArgs

    #region RecordDeactivatingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordDeactivating"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordDeactivating"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordDeactivatingEvent"/>
	public class RecordDeactivatingEventArgs : CancelableRoutedEventArgs
    {
        private Record _record;

        internal RecordDeactivatingEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record to be de-activated (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveRecord"/>
        public Record Record { get { return this._record; } }
    }

    #endregion //RecordDeactivatingEventArgs

    #region RecordExpandedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordExpanded"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.RecordPresenter.IsExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordExpandedEvent"/>
    public class RecordExpandedEventArgs : RoutedEventArgs
    {
        private Record _record;

        internal RecordExpandedEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record that has just been expanded (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
        public Record Record { get { return this._record; } }
    }

    #endregion //RecordExpandedEventArgs

    #region RecordExpandingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordExpanding"/>
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.RecordPresenter.IsExpanded"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordExpanding"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordExpandingEvent"/>
	public class RecordExpandingEventArgs : CancelableRoutedEventArgs
    {
        private Record _record;

        internal RecordExpandingEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record to be expanded (read-only).
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.IsExpanded"/>
		public Record Record { get { return this._record; } }
    }

    #endregion //RecordExpandingEventArgs

	#region RecordFilterChangedEventArgs

	// SSP 12/12/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterChanged"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterChanged"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterChanging"/>
	public class RecordFilterChangedEventArgs : RoutedEventArgs
	{
		private RecordFilter _recordFilter;

		internal RecordFilterChangedEventArgs( RecordFilter recordFilter )
		{
			_recordFilter = recordFilter;
		}

		/// <summary>
		/// Returns the modified record filters.
		/// </summary>
		public RecordFilter RecordFilter
		{
			get
			{
				return _recordFilter;
			}
		}
	}

	#endregion // RecordFilterChangedEventArgs

	#region RecordFilterChangingEventArgs

	// SSP 12/12/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterChanging"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterChanging"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterChanged"/>
	public class RecordFilterChangingEventArgs : CancelableRoutedEventArgs
	{
		private RecordFilter _newRecordFilter;

		internal RecordFilterChangingEventArgs( RecordFilter newRecordFilter )
		{
			_newRecordFilter = newRecordFilter;
		}

		/// <summary>
		/// Returns the modified record filters.
		/// </summary>
		public RecordFilter NewRecordFilter
		{
			get
			{
				return _newRecordFilter;
			}
		}
	}

	#endregion // RecordFilterChangingEventArgs

	// JJD 02/21/12 - TFS99332 - added
	#region RecordFilterDropDownItemInitializingEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownItemInitializing"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownItemInitializing"/>
	public class RecordFilterDropDownItemInitializingEventArgs : RoutedEventArgs
	{
        private FilterDropDownItem _dropDownItem;
        private Field _field;
        private RecordManager _recordManager;

        internal RecordFilterDropDownItemInitializingEventArgs(Field field, RecordManager recordManager, FilterDropDownItem dropDownItem)
		{
			_field = field;
            _recordManager = recordManager;
            _dropDownItem = dropDownItem;
		}

		/// <summary>
		/// Returns the <see cref="FilterDropDownItem"/> item being initialized (read-only).
		/// </summary>
        public FilterDropDownItem DropDownItem
		{
			get
			{
				return _dropDownItem;
			}
		}

		/// <summary>
		/// Returns the associated Field (read-only).
		/// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

		/// <summary>
		/// Returns the associated RecordManager (read-only).
		/// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.RecordManager"/>
		public RecordManager RecordManager
		{
			get
			{
				return _recordManager;
			}
		}
	}

	#endregion // RecordFilterDropDownItemInitializingEventArgs

	#region RecordFilterDropDownOpeningEventArgs

	// SSP 12/12/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownOpening"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownOpening"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownPopulating"/>
	public class RecordFilterDropDownOpeningEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private ObservableCollection<FilterDropDownItem> _dropDownItems;
		private bool _raisedForCustomFilterSelectionControl;
		private Field _field;
		private RecordManager _recordManager;

		// AS - NA 11.2 Excel Style Filtering
		private FilterDropDownItemsType _itemsType;
		private IList<FieldMenuDataItem> _menuItems; 

		#endregion //Member Variables

		#region Constructor
		// AS - NA 11.2 Excel Style Filtering
		// Refactored ctor so the common portion is in a private ctor.
		//
		private RecordFilterDropDownOpeningEventArgs(Field field, RecordManager recordManager, bool raisedForCustomFilterSelectionControl)
		{
			_field = field;
			_recordManager = recordManager;
			_raisedForCustomFilterSelectionControl = raisedForCustomFilterSelectionControl;
		}

		internal RecordFilterDropDownOpeningEventArgs(Field field, RecordManager recordManager, ObservableCollection<FilterDropDownItem> dropDownItems, bool raisedForCustomFilterSelectionControl)
			: this(field, recordManager, raisedForCustomFilterSelectionControl)
		{
			_dropDownItems = dropDownItems;
			_itemsType = FilterDropDownItemsType.DropDownItems;
		}

		internal RecordFilterDropDownOpeningEventArgs(Field field, RecordManager recordManager, IList<FieldMenuDataItem> menuItems, bool raisedForCustomFilterSelectionControl)
			: this(field, recordManager, raisedForCustomFilterSelectionControl)
		{
			_menuItems = menuItems;
			_itemsType = FilterDropDownItemsType.MenuItems;
		} 
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the items that will be displayed in the filter drop-down. You can modify the list
		/// to add new entries or remove existing entries from the filter drop-down.
		/// </summary>
		public ObservableCollection<FilterDropDownItem> DropDownItems
		{
			get
			{
				return _dropDownItems;
			}
		}

		/// <summary>
		/// Returns the associated Field (read-only).
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

		#region ItemsType
		/// <summary>
		/// Returns an enumeration indicating whether the <see cref="DropDownItems"/> or <see cref="MenuItems"/> are used to populate the drop down.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public FilterDropDownItemsType ItemsType
		{
			get { return _itemsType; }
		}
		#endregion //ItemsType

		#region MenuItems

		/// <summary>
		/// Returns the menu items that will be displayed in the filter drop-down. You can modify the list
		/// to add new entries or remove existing entries from the filter drop-down.
		/// </summary>
		/// <remarks>
		/// <p class="note">This property will return null when the <see cref="ItemsType"/> returns <b>DropDownItems</b></p>
		/// </remarks>
		/// <seealso cref="ItemsType"/>
		/// <seealso cref="DropDownItems"/>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public IList<FieldMenuDataItem> MenuItems
		{
			get
			{
				return _menuItems;
			}
		}

		#endregion //MenuItems

		/// <summary>
		/// Returns the associated RecordManager (read-only).
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordManager"/>
		public RecordManager RecordManager
		{
			get
			{
				return _recordManager;
			}
		}

		/// <summary>
		/// Returns true if this event is raised when the user drops down the filter 
		/// drop-down in the custom filter selection control.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Useful in case you want to display different set of items in the filter drop-down 
		/// of a field and the filter drop-down of a custom filter selection control.
		/// </para>
		/// </remarks>
		public bool RaisedForCustomFilterSelectionControl
		{
			get
			{
				return _raisedForCustomFilterSelectionControl;
			}
		} 
		#endregion //Properties
	}

	#endregion // RecordFilterDropDownOpeningEventArgs

	#region RecordFilterDropDownPopulatingEventArgs

	// SSP 12/12/08 - NAS9.1 Record Filtering
	// 
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownPopulating"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownPopulating"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFilterDropDownOpening"/>
	public class RecordFilterDropDownPopulatingEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private List<FilterDropDownItem> _dropDownItems;
		private bool _raisedForCustomFilterSelectionControl;
		private bool _includeUniqueValues;
        private Field _field;
		// JJD 06/29/10 - TFS32174 - added
		private int _asynchLoadDuration		= DefaultAsynchLoadDuration;
		private int _asynchLoadInterval		= DefaultAsynchLoadInterval;
		private int _asynchLoadThreshhold	= DefaultAsynchLoadThreshhold;

		// AS - NA 11.2 Excel Style Filtering
		private FilterDropDownItemsType _itemsType;
		private IList<FieldMenuDataItem> _menuItems;

        private RecordManager _recordManager;

		internal const int DefaultAsynchLoadDuration	= 50;
		internal const int DefaultAsynchLoadInterval	= 50;
		internal const int DefaultAsynchLoadThreshhold	= 1000;

		#endregion //Member Variables

		#region Constructor
		// AS - NA 11.2 Excel Style Filtering
		// Refactored ctor so the common portion is in a private ctor.
		//
		private RecordFilterDropDownPopulatingEventArgs(Field field, RecordManager recordManager, bool includeUniqueValues, bool raisedForCustomFilterSelectionControl)
		{
			_field = field;
			_recordManager = recordManager;
			_includeUniqueValues = includeUniqueValues;
			_raisedForCustomFilterSelectionControl = raisedForCustomFilterSelectionControl;
		}

		internal RecordFilterDropDownPopulatingEventArgs(Field field, RecordManager recordManager, List<FilterDropDownItem> dropDownItems, bool includeUniqueValues, bool raisedForCustomFilterSelectionControl)
			: this(field, recordManager, includeUniqueValues, raisedForCustomFilterSelectionControl)
		{
			_dropDownItems = dropDownItems;
			_itemsType = FilterDropDownItemsType.DropDownItems;
		}

		internal RecordFilterDropDownPopulatingEventArgs(Field field, RecordManager recordManager, IList<FieldMenuDataItem> menuItems, bool includeUniqueValues, bool raisedForCustomFilterSelectionControl)
			: this(field, recordManager, includeUniqueValues, raisedForCustomFilterSelectionControl)
		{
			_menuItems = menuItems;
			_itemsType = FilterDropDownItemsType.MenuItems;
		} 
		#endregion //Constructor

		// JJD 06/29/10 - TFS32174 - added
		#region AsynchLoadDuration

		/// <summary>
		/// Specifies the maximum number of milliseconds to load unique values before stopping to wait for another <see cref="AsynchLoadInterval"/>.
		/// </summary>
		/// <value>The number of milliseconds to synchronously load unique values before stopping.</value>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if the <see cref="IncludeUniqueValues"/> property is set to false then this property has no effect.</para>
		/// </remarks>
		/// <seealso cref="IncludeUniqueValues"/>
		/// <seealso cref="AsynchLoadInterval"/>
		/// <seealso cref="AsynchLoadThreshhold"/>
		public int AsynchLoadDuration
		{
			get
			{
				return _asynchLoadDuration;
			}
			set
			{
				_asynchLoadDuration = value;
			}
		}

		#endregion //AsynchLoadDuration	

		// JJD 06/29/10 - TFS32174 - added
		#region AsynchLoadInterval

		/// <summary>
		/// Specifies the timer interval used to trigger asynchronous load processing.
		/// </summary>
		/// <value>The number of milliseconds to wait between asynchronous load processing.</value>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if the <see cref="IncludeUniqueValues"/> property is set to false then this property has no effect.</para>
		/// </remarks>
		/// <seealso cref="IncludeUniqueValues"/>
		/// <seealso cref="AsynchLoadDuration"/>
		/// <seealso cref="AsynchLoadThreshhold"/>
		public int AsynchLoadInterval
		{
			get
			{
				return _asynchLoadInterval;
			}
			set
			{
				_asynchLoadInterval = value;
			}
		}

		#endregion //AsynchLoadInterval	

		// JJD 06/29/10 - TFS32174 - added
		#region AsynchLoadThreshhold

		/// <summary>
		/// Specifies the maximum number of milliseconds to use to load unique values before continuing the loading process asynchronously.
		/// </summary>
		/// <value>The number of milliseconds to synchronously load unique values.</value>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if the <see cref="IncludeUniqueValues"/> property is set to false then this property has no effect.</para>
		/// </remarks>
		/// <seealso cref="IncludeUniqueValues"/>
		/// <seealso cref="AsynchLoadDuration"/>
		/// <seealso cref="AsynchLoadInterval"/>
		public int AsynchLoadThreshhold
		{
			get
			{
				return _asynchLoadThreshhold;
			}
			set
			{
				_asynchLoadThreshhold = value;
			}
		}

		#endregion //AsynchLoadThreshhold	
    
		#region DropDownItems

		/// <summary>
		/// Returns the items that will be displayed in the filter drop-down. You can modify the list
		/// to add new entries or remove existing entries from the filter drop-down.
		/// </summary>
		/// <remarks>
		/// <p class="note">This property will return null when the <see cref="ItemsType"/> returns <b>MenuItems</b></p>
		/// </remarks>
		/// <seealso cref="ItemsType"/>
		/// <seealso cref="MenuItems"/>
		public List<FilterDropDownItem> DropDownItems
		{
			get
			{
				return _dropDownItems;
			}
		}

		#endregion //DropDownItems	
    
		#region IncludeUniqueValues

		/// <summary>
		/// Specifies whether the data presenter should include unique field values from the data source
		/// in the filter drop-down list.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can set this property to false to prevent the data presenter from including field values in
		/// the filter drop-down list.
		/// </para>
		/// </remarks>
		public bool IncludeUniqueValues
		{
			get
			{
				return _includeUniqueValues;
			}
			set
			{
				_includeUniqueValues = value;
			}
		}

		#endregion //IncludeUniqueValues	
    
		#region ItemsType
		/// <summary>
		/// Returns an enumeration indicating whether the <see cref="DropDownItems"/> or <see cref="MenuItems"/> are used to populate the drop down.
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public FilterDropDownItemsType ItemsType
		{
			get { return _itemsType; }
		} 
		#endregion //ItemsType

		#region Field

		/// <summary>
		/// Returns the associated Field (read-only).
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

		#endregion //Field	
    
		#region MenuItems

		/// <summary>
		/// Returns the menu items that will be displayed in the filter drop-down. You can modify the list
		/// to add new entries or remove existing entries from the filter drop-down.
		/// </summary>
		/// <remarks>
		/// <p class="note">This property will return null when the <see cref="ItemsType"/> returns <b>DropDownItems</b></p>
		/// </remarks>
		/// <seealso cref="ItemsType"/>
		/// <seealso cref="DropDownItems"/>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public IList<FieldMenuDataItem> MenuItems
		{
			get
			{
				return _menuItems;
			}
		}

		#endregion //MenuItems

		#region RecordManager

		/// <summary>
		/// Returns the associated RecordManager (read-only).
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.RecordManager"/>
		public RecordManager RecordManager
		{
			get
			{
				return _recordManager;
			}
		}

		#endregion //RecordManager	
    
		#region RaisedForCustomFilterSelectionControl

		/// <summary>
		/// Returns true if this event is raised when the user drops down the filter 
		/// drop-down in the custom filter selection control.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Useful in case you want to display different set of items in the filter drop-down 
		/// of a field and the filter drop-down of a custom filter selection control.
		/// </para>
		/// </remarks>
		public bool RaisedForCustomFilterSelectionControl
		{
			get
			{
				return _raisedForCustomFilterSelectionControl;
			}
		}

		#endregion //RaisedForCustomFilterSelectionControl
	}

	#endregion // RecordFilterDropDownPopulatingEventArgs

    // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
    #region RecordFixedLocationChangedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFixedLocationChanged"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecord"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.Record.FixedLocation"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFixedLocationChanged"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFixedLocationChangedEvent"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
    public class RecordFixedLocationChangedEventArgs : RoutedEventArgs
    {
        private Record _record;

        internal RecordFixedLocationChangedEventArgs(Record record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record that has just been fixed/unfixed (read-only).
        /// </summary>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.FixedLocation"/>
        public Record Record { get { return this._record; } }
    }

    #endregion //RecordFixedLocationChangedEventArgs

    // JJD 6/8/09 NA 2009 Vol 2 - Fixed Records
    #region RecordFixedLocationChangingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFixedLocationChanging"/>
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.GroupByRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ExpandableFieldRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Record.FixedLocation"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFixedLocationChanging"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordFixedLocationChangingEvent"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_RecordFixing, Version = FeatureInfo.Version_9_2)]
    public class RecordFixedLocationChangingEventArgs : CancelableRoutedEventArgs
    {
        private Record _record;
        private FixedRecordLocation _newLocation;


        internal RecordFixedLocationChangingEventArgs(Record record, FixedRecordLocation newLocation)
        {
            this._record = record;
            this._newLocation = newLocation;
        }

        /// <summary>
        /// Returns the new fixed location for the record (read-only).
        /// </summary>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.FixedLocation"/>
		public FixedRecordLocation NewLocation { get { return this._newLocation; } }

        /// <summary>
        /// Returns the record to be fixed/unfixed (read-only).
        /// </summary>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Record.FixedLocation"/>
		public Record Record { get { return this._record; } }
    }

    #endregion //RecordFixedLocationChangingEventArgs

    #region RecordsDeletedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeleted"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeleted"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeletedEvent"/>
    public class RecordsDeletedEventArgs : RoutedEventArgs
    {
        internal RecordsDeletedEventArgs()
        {
        }
	}

    #endregion //RecordDeletedEventArgs

    #region RecordsDeletingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeleting"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeleting"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeletingEvent"/>
    public class RecordsDeletingEventArgs : CancelableRoutedEventArgs
    {
        private DataRecord[] _records;
		private bool _displayPromptMessage;
		private ReadOnlyCollection<Record> _readOnlyRecords;

        // AS 4/14/09 NA 2009.2 ClipboardSupport
        private UndeleteRecordsStrategy _undeleteStrategy;

        internal RecordsDeletingEventArgs(DataRecord[] records, bool displayPromptMessage)
        {
            this._records = records;
			this._displayPromptMessage = displayPromptMessage;
        }

        /// <summary>
        /// Returns the records that are about to be deleted (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        
		
		
		//public DataRecord[] Records { get { return this._records; } }
		public ReadOnlyCollection<Record> Records 
		{ 
			get 
			{
				if (this._readOnlyRecords == null)
					this._readOnlyRecords = new ReadOnlyCollection<Record>(this._records);

				return this._readOnlyRecords; 
			} 
		}

		/// <summary>
		/// Dertermines whether a prompt message is displayed to the user to confirm the record deletion.
		/// </summary>
		public bool DisplayPromptMessage
		{
			get
			{
				return this._displayPromptMessage;
			}
			set
			{
				this._displayPromptMessage = value;
			}
		}

        // AS 4/14/09 NA 2009.2 ClipboardSupport
        #region UndeleteStrategy
        /// <summary>
        /// Returns or sets an instance of a class that can be used to undo the deletion of the <see cref="Records"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">If <see cref="DataPresenterBase.IsUndoEnabled"/> is true and records are deleted, an instance of 
        /// <see cref="UndeleteRecordsStrategy"/> must be provided in order to be able to undo the deletion. It is the 
        /// responsibility of the implementer of this class to track any required state necessary and be able to 
        /// perform the undeletion of the records.</p>
        /// </remarks>
        [InfragisticsFeature(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
        public UndeleteRecordsStrategy UndeleteStrategy
        {
            get { return _undeleteStrategy; }
            set { _undeleteStrategy = value; }
        } 
        #endregion //UndeleteStrategy
	}

    #endregion //RecordDeletingEventArgs

	#region RecordsInViewChangedEventArgs

    /// <summary>
    /// Event arguments for routed event <b>DataPresenterBase.RecordsInViewChanged</b>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Grouping"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsInViewChanged"/>
    public class RecordsInViewChangedEventArgs : RoutedEventArgs
	{
	    /// <summary>
		/// Initializes a new instance of the <see cref="RecordsInViewChangedEventArgs"/> class
		/// </summary>
        public RecordsInViewChangedEventArgs()
		{
		}

	}

	#endregion //RecordsInViewChangedEventArgs

    #region RecordUpdateCanceledEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdateCanceled"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdateCanceled"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdateCanceledEvent"/>
    public class RecordUpdateCanceledEventArgs : RoutedEventArgs
    {
        private DataRecord _record;

        internal RecordUpdateCanceledEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record whose update has just been canceled (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        public DataRecord Record { get { return this._record; } }
    }

    #endregion //RecordUpdateCanceledEventArgs

    #region RecordUpdateCancelingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdateCanceling"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdateCanceling"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdateCancelingEvent"/>
	public class RecordUpdateCancelingEventArgs : CancelableRoutedEventArgs
    {
        private DataRecord _record;

        internal RecordUpdateCancelingEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record whose update is about to be canceled (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        public DataRecord Record { get { return this._record; } }
    }

    #endregion //RecordUpdateCancelingEventArgs

    #region RecordUpdatedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdated"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdated"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdatedEvent"/>
    public class RecordUpdatedEventArgs : RoutedEventArgs
    {
        private DataRecord _record;

        internal RecordUpdatedEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Returns the record that has just been updated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        public DataRecord Record { get { return this._record; } }
    }

    #endregion //RecordUpdatedEventArgs

    #region RecordUpdatingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdating"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdating"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordUpdatingEvent"/>
	public class RecordUpdatingEventArgs : RoutedEventArgs
    {
        private DataRecord _record;
		private RecordUpdatingAction _action = RecordUpdatingAction.ProceedWithUpdate;

        internal RecordUpdatingEventArgs(DataRecord record)
        {
            this._record = record;
        }

        /// <summary>
        /// Gets/sets the action to take.
        /// </summary>
		/// <seealso cref="RecordUpdatingAction"/>
		public RecordUpdatingAction Action 
		{ 
			get { return this._action; } 
			set { this._action = value; } 
		}

        /// <summary>
        /// Returns the record to be updated (read-only).
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        public DataRecord Record { get { return this._record; } }
    }

    #endregion //RecordUpdatingEventArgs

    #region SelectedItemsChangedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChanged"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChanged"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChangedEvent"/>
    public class SelectedItemsChangedEventArgs : RoutedEventArgs
    {
        private System.Type _type;

        internal SelectedItemsChangedEventArgs(System.Type type)
        {
            this._type = type;
        }

        /// <summary>
        /// Returns the type of object that triggered the selection change (read-only).
        /// </summary>
        public System.Type Type { get { return this._type; } }

    }

    #endregion //SelectedItemsChangedEventArgs

    #region SelectedItemsChangingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChanging"/>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChanging"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChangingEvent"/>
	public class SelectedItemsChangingEventArgs : CancelableRoutedEventArgs
    {
		private Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder _newSelection;
        private System.Type _type;

		internal SelectedItemsChangingEventArgs(System.Type type, Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder newSelection)
        {
            this._type = type;
            this._newSelection = newSelection;
        }

        /// <summary>
        /// Returns the new selection (read-only).
        /// </summary>
		public Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder NewSelection { get { return this._newSelection; } }

        /// <summary>
        /// Returns the type of object that triggered the selection change (read-only).
        /// </summary>
        public System.Type Type { get { return this._type; } }


    }

    #endregion //SelectedItemsChangingEventArgs

	#region SortingEventArgs

    /// <summary>
    /// Event arguments for routed event <b>DataPresenterBase.Sorting</b>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Sorting"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Sorted"/>
    public class SortingEventArgs : CancelableRoutedEventArgs
	{
		private FieldLayout _layout;
		private FieldSortDescription _sortDescription;
		private bool _replaceExistingSortCriteria;
        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
        private Field _field;
	
        /// <summary>
		/// Initializes a new instance of the <see cref="SortingEventArgs"/> class
		/// </summary>
		/// <param name="layout">The associated <see cref="FieldLayout"/></param>
		/// <param name="sortDescription">Defines the field being sorted and the sort direction</param>
		/// <param name="replaceExistingSortCriteria">Indicates if this will replace the current set of <see cref="FieldSortDescription"/> instances</param>
        public SortingEventArgs(FieldLayout layout, FieldSortDescription sortDescription, bool replaceExistingSortCriteria)
		{
			this._layout						= layout;
            this._sortDescription				= sortDescription;
			this._replaceExistingSortCriteria	= replaceExistingSortCriteria;

            // JJD 12/03/08 
            // initialze the new field member
            this._field                         = sortDescription.Field;
		}

        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
        // Needed ctor that takes a field when we are unsorting
        internal SortingEventArgs(FieldLayout layout, Field field)
		{
			this._layout						= layout;
            this._field				            = field;
			this._replaceExistingSortCriteria	= false;
		}

        /// <summary>
		/// Returns the associated <b>FieldLayout</b> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout { get { return this._layout; } }

        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
        /// <summary>
		/// Returns the associated <b>Infragistics.Windows.DataPresenter.Field</b> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
        public Field Field { get { return this._field; } }

        /// <summary>
        /// Returns the <b>SortDecription</b> that will be applied to the new group (read-only)
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property can return null if the <see cref="Field"/> is being unsorted, i.e. if the <see cref="FieldSettings.LabelClickAction"/> property is set to 'SortByOneFieldOnlyTriState' or 'SortByMultipleFieldsTriState'.
        /// </para>
        /// </remarks>
        /// <seealso cref="FieldSettings.LabelClickAction"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
        public FieldSortDescription SortDescription { get { return this._sortDescription; } }

        /// <summary>
        /// Determines if this will replace the existing set of the <b>SortDecription</b>s that determine the sort order
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this setting is ignored if the field is being unsorted</para>
        /// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
		public bool ReplaceExistingSortCriteria 
		{ 
			get { return this._replaceExistingSortCriteria; } 
			set { this._replaceExistingSortCriteria = value; } 
		}

	}

	#endregion //SortingEventArgs

	#region SortedEventArgs

    /// <summary>
    /// Event arguments for routed event <b>DataPresenterBase.Sorted</b>
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Sorting"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Sorted"/>
    public class SortedEventArgs : RoutedEventArgs
	{
		private FieldLayout _layout;
		private FieldSortDescription _sortDescription;
        
        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
        private Field _field;
	
        /// <summary>
		/// Initializes a new instance of the <see cref="SortedEventArgs"/> class
		/// </summary>
		/// <param name="layout">The associated <see cref="FieldLayout"/></param>
		/// <param name="sortDescription">The <see cref="FieldSortDescription"/> that was applied</param>
        public SortedEventArgs(FieldLayout layout, FieldSortDescription sortDescription)
		{
			this._layout			= layout;
            this._sortDescription	= sortDescription;

            // JJD 12/03/08 
            // initialze the new field member
            this._field             = sortDescription.Field;
        }

        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
        // Needed ctor that takes a field when we are unsorting
        internal SortedEventArgs(FieldLayout layout, Field field)
		{
			this._layout			= layout;
            this._field             = field;
        }

        /// <summary>
		/// Returns the associated <b>FieldLayout</b> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
        public FieldLayout FieldLayout { get { return this._layout; } }

        // JJD 12/03/08 
        // Added support for cycling thru ascending/descending/unsorted
        /// <summary>
        /// Returns the associated <b>Infragistics.Windows.DataPresenter.Field</b> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
        public Field Field { get { return this._field; } }

        /// <summary>
        /// Returns the <b>SortDecription</b> that was just applied (read-only)
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property can return null if the <see cref="Field"/> is being unsorted, i.e. if the <see cref="FieldSettings.LabelClickAction"/> property is set to 'SortByOneFieldOnlyTriState' or 'SortByMultipleFieldsTriState'.
        /// </para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
        public FieldSortDescription SortDescription { get { return this._sortDescription; } }

	}

	#endregion //SortedEventArgs

	#region SummaryResultChangedEventArgs

	/// <summary>
	/// Event arguments for routed event <b>DataPresenterBase.Sorted</b>
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Sorting"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Sorted"/>
	public class SummaryResultChangedEventArgs : RoutedEventArgs
	{
		private SummaryResult _summaryResult;

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryResultChangedEventArgs"/> class
		/// </summary>
		/// <param name="summaryResult"></param>		
		public SummaryResultChangedEventArgs( SummaryResult summaryResult )
		{
			_summaryResult = summaryResult;
		}

		/// <summary>
		/// Returns the <see cref="SummaryResult"/> instance that was recalculated.
		/// </summary>
		public SummaryResult SummaryResult
		{
			get
			{
				return _summaryResult;
			}
		}
	}

	#endregion // SummaryResultChangedEventArgs

	#region SummarySelectionControlClosedEventArgs

	
	
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummarySelectionControlClosed"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummarySelectionControlClosed"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummarySelectionControlClosedEvent"/>
	public class SummarySelectionControlClosedEventArgs : RoutedEventArgs
	{
		private Field _field;
		private bool _summariesChanged;

		internal SummarySelectionControlClosedEventArgs( Field field, bool summariesChanged )
		{
			_field = field;
			_summariesChanged = summariesChanged;
		}

		/// <summary>
		/// Returns the field for which summary selection UI is being displayed (read-only).
		/// </summary>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

		/// <summary>
		/// Indicates whether the user actually changed summaries.
		/// </summary>
		public bool SummariesChanged
		{
			get
			{
				return _summariesChanged;
			}
		}

	}

	#endregion // SummarySelectionControlClosedEventArgs

	#region SummarySelectionControlOpeningEventArgs

	
	
	/// <summary>
	/// Event arguments for routed event <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummarySelectionControlOpening"/>.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummarySelectionControlOpening"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SummarySelectionControlOpeningEvent"/>
	public class SummarySelectionControlOpeningEventArgs : CancelableRoutedEventArgs
	{
		private Field _field;
		private SummaryCalculatorSelectionControl _control;

		internal SummarySelectionControlOpeningEventArgs( Field field, SummaryCalculatorSelectionControl control )
		{
			_field = field;
			_control = control;
		}

		/// <summary>
		/// Returns the control being displayed in the UI for selecting summaries for a field (read-only).
		/// </summary>
		public SummaryCalculatorSelectionControl Control
		{
			get
			{
				return _control;
			}
		}

		/// <summary>
		/// Returns the field for which summary selection UI is being displayed (read-only).
		/// </summary>
		public Field Field
		{
			get
			{
				return _field;
			}
		}

	}

	#endregion // SummarySelectionControlOpeningEventArgs

	#region ViewStateChangedEventArgs

	/// <summary>
	/// EventArgs for <see cref="Infragistics.Windows.DataPresenter.ViewBase"/>'s ViewStateChanged event.
	/// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase.ViewStateChangedEvent"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.ViewStateChangedAction"/>
	public class ViewStateChangedEventArgs : RoutedEventArgs
	{
		private ViewStateChangedAction					_action;

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewStateChangedEventArgs"/> class
		/// </summary>
		/// <param name="action">The type of action that should be taken.</param>
		public ViewStateChangedEventArgs(ViewStateChangedAction action)
		{
			this._action	= action;
		}

		/// <summary>
		/// Returns an enumeration that specifies the action to be taken as a result of the View state change.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.ViewBase.ViewStateChangedEvent"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ViewStateChangedEventArgs"/>
		public ViewStateChangedAction Action
		{
			get { return this._action; }
		}
	}

	#endregion // ViewStateChangedEventArgs

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