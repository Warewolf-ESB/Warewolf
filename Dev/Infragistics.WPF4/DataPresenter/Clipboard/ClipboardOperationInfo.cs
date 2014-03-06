using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;
using System.Globalization;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// Contains nested classes and other infrastructure for performing clipboard operations.
    /// </summary>
    internal class ClipboardOperationInfo
    {
        #region Member Variables

        private DataPresenterBase _owner;

		private static readonly object PasteOperationContext = ClipboardOperation.Paste;
		private static readonly object ClearCellsOperationContext = ClipboardOperation.ClearContents;

        #endregion //Member Variables

        #region Constructor
        internal ClipboardOperationInfo(DataPresenterBase owner)
        {
            GridUtilities.ValidateNotNull(owner);
            _owner = owner;
        } 
        #endregion //Constructor

		#region Properties
		public DataPresenterBase DataPresenter
		{
			get { return _owner; }
		} 
		#endregion //Properties

        #region Methods

        #region Internal Methods

		#region CanPerformOperation
		/// <summary>
        /// Returns a boolean indicating if the specified operation can be performed.
        /// </summary>
        /// <param name="operation">The operation to evaluate</param>
        /// <returns>A boolean indicating if the specified operation can be performed.</returns>
        internal bool CanPerformOperation(ClipboardOperation operation)
        {
			FieldLayout fl = GetTargetFieldLayout(operation);

			if (fl == null)
				return false;

			#region Check FieldLayout.AllowClipboardOperations

			AllowClipboardOperations operationToCheck;

			switch (operation)
			{
				case ClipboardOperation.Copy:
					operationToCheck = AllowClipboardOperations.Copy;
					break;
				case ClipboardOperation.Paste:
					operationToCheck = AllowClipboardOperations.Paste;
					break;
				case ClipboardOperation.Cut:
					operationToCheck = AllowClipboardOperations.Cut;
					break;
				case ClipboardOperation.ClearContents:
					operationToCheck = AllowClipboardOperations.ClearContents;
					break;
				default:
					Debug.Fail("Unexpected operation:" + operation.ToString());
					return false;
			}

			AllowClipboardOperations allowedOperations = fl.AllowClipboardOperationsResolved;
			if (operationToCheck != (operationToCheck & allowedOperations))
				return false; 

			#endregion //Check FieldLayout.AllowClipboardOperations

            return true;
        }
        #endregion //CanPerformOperation

		#region ClearCellContents
		internal bool ClearCellContents()
		{
			CellValueProviderSource source;
			ICellValueProvider provider = GetCellValueProvider(ClipboardOperation.ClearContents, out source);
			return ClearCellContents(provider, source);
		}

		private bool ClearCellContents(ICellValueProvider provider, CellValueProviderSource source)
		{
			if (provider == null || provider.FieldCount < 1 || provider.RecordCount < 1)
				return false;

			// if all of the fields associated with the operation have their 
			// DisallowModificationViaClipboard set to true then consider this
			// a no-op and bail out.
			//
			if (!ContainsModifiableFields(provider))
				return false;

			// reset all the values to null
			ClearValues(provider);

			// note like the CommitEditValueHelper on CellValuePresenter, the coerced null
			// value is considered a converted value so we still pass in true for the 
			// use converter
			MultipleCellValuesAction action = new MultipleCellValuesAction(provider, true, GetSelectionTarget(source));

			return _owner.History.PerformAction(action, ClearCellsOperationContext);
		}

		#endregion //ClearCellContents

		#region ClearValues
		/// <summary>
		/// Helper method to reinitialize all the values to their null representation.
		/// </summary>
		/// <param name="provider">The provider whose values are to be reset</param>
		private static void ClearValues(ICellValueProvider provider)
		{
			if (null != provider)
			{
				int cCount = provider.FieldCount;
				Field[] fields = new Field[cCount];

				for (int i = 0; i < cCount; i++)
					fields[i] = provider.GetField(i);

				for (int r = 0, rCount = provider.RecordCount; r < rCount; r++)
				{
					DataRecord record = provider.GetRecord(r);
					Debug.Assert(null != record);

					if (null != record)
					{
						for (int c = 0; c < cCount; c++)
						{
							CellValueHolder cvh = provider.GetValue(r, c);

							if (cvh == null)
								continue;

							// use the same helper method used by the CellValuePresenter
							// to convert a null edit value to the appropriate null
							// representation for the field
							object nullValue = null;
							record.CoerceNullEditValue(fields[c], ref nullValue);

							cvh.Initialize(nullValue, false);
						}
					}
				}
			}
		}
		#endregion //ClearValues

		#region ContainsModifiableFields
		internal static bool ContainsModifiableFields(ICellValueProvider provider)
		{
			for (int i = 0, count = provider.FieldCount; i < count; i++)
			{
				Field f = provider.GetField(i);

				if (!ShouldSkipFieldModification(f))
					return true;
			}

			return false;
		}
		#endregion //ContainsModifiableFields

		#region ConvertToCellValue
		/// <summary>
		/// Helper method to ensure that all values in the collection represent raw cell values.
		/// </summary>
		internal static bool ConvertToCellValue(ICellValueProvider provider, IList<object> originalDataItems, bool[] skipFields, bool checkEditableCells, out RowColumnIndex stopAtCell, ClipboardOperation? operation, RecordManager defaultRecordManager, out bool hadConversionErrors)
		{
			// assume we can process all cells
			stopAtCell = null;
			hadConversionErrors = false;

			int rowCount = provider.RecordCount;
			int colCount = provider.FieldCount;
			Debug.Assert(skipFields != null && skipFields.Length == colCount);
			Dictionary<Field, bool> readOnlyFields = new Dictionary<Field,bool>();
			FieldLayout fl = provider.FieldLayout;
			Debug.Assert(null != fl);

			if (null == fl)
				return false;

			DataPresenterBase dp = fl.DataPresenter;
			ClipboardOperationInfo clipInfo = dp.ClipboardOperationInfo;
			Exception cellException = null;
			ClipboardError? error = null;
			bool displayErrorDialog = false;

			for (int r = 0; r < rowCount; r++)
			{
				DataRecord record = provider.GetRecord(r);

				// if we have a record and its data item is different then we
				// need to skip it since the record was changed but not fixed up
				bool isRecordValid = null == record || IsSameDataItem(record.DataItem, originalDataItems[r]);

				if (record == null && null != defaultRecordManager)
					record = defaultRecordManager.CurrentAddRecord;

				// if it was valid and we have a record then make sure its not deleted
				if (isRecordValid && null != record)
					isRecordValid = !ClipboardOperationInfo.IsDeleted(record);

				Debug.Assert(null != record);

				for (int c = 0; c < colCount; c++)
				{
					CellValueHolder cvh = provider.GetValue(r, c);

					if (null == cvh)
						continue;

					// ignore any cells where the record is deleted
					if (!isRecordValid)
						cvh.Ignore = true;

					// if the operation never occurred - e.g. this is an undo for an operation
					// that was stopped/continued - then skip the conversion
					if (cvh.Ignore)
						continue;

					// if a field has DisallowModificationViaClipboard then ignore
					// those cells
					if (null != skipFields && skipFields[c])
					{
						Debug.Assert(null != operation);
						cvh.Ignore = true;
						continue;
					}

					Field field = provider.GetField(c);

					#region ReadOnly cells

					// if we're checking editable fields and this cell isn't editable...
					if (checkEditableCells)
					{
						Debug.Assert(null != record);

						if (null != record && !record.GetIsEditingAllowed(field))
						{
							error = ClipboardError.ReadOnlyCell;

							bool isReadOnly;

							// only show the readonly error message box once per field
							if (!readOnlyFields.TryGetValue(field, out isReadOnly))
							{
								isReadOnly = field.AllowEditResolved;
								readOnlyFields[field] = isReadOnly;
								displayErrorDialog = true;
							}
						}
					} 
					#endregion //ReadOnly cells

					#region Convert DisplayText to Value
					if (error == null && cvh.IsDisplayText)
					{
						CellTextConverterInfo converter = CellTextConverterInfo.GetCachedConverter( field );

						// since the display text may lose precision and because it may
						// not be possible to support parsing a given format, if the original
						// value converted to a display text with the target field results
						// in the original display text then we can safely assume we can use
						// the original value
						ClipboardCellValueHolder clipCvh = cvh as ClipboardCellValueHolder;

						if (null != clipCvh)
						{
							string newDisplayText = converter.ConvertCellValue(clipCvh.OriginalValue, out cellException);

							// if we were able to convert it without an exception...
							if (null == cellException && string.Equals(newDisplayText, clipCvh.OriginalDisplayText))
							{
								// validate the value with the target field editor
								if (converter.ValidateValue(clipCvh.OriginalValue, out cellException))
								{
									Debug.Assert(null == cellException);

									cvh.Initialize(clipCvh.OriginalValue, false);
									continue;
								}

								// ignore this exception and try to parse the display text
								cellException = null;
							}
						}

						if (null == error)
						{
							// SSP 5/5/09
							// Made ConvertDisplayText take in a string instead of object.
							// 
							//object convertedValue = converter.ConvertDisplayText( cvh.Value, out cellException );
							object cvhValue = cvh.Value;
							object convertedValue = converter.ConvertDisplayText(null != cvhValue ? cvhValue.ToString() : null, out cellException);

							if (null != cellException)
							{
								error = ClipboardError.ConversionError;
								displayErrorDialog = true;
							}
							else
							{
								if (converter.ValidateValue(convertedValue, out cellException))
								{
									Debug.Assert(null == cellException);

									// if we got back a null value then we want to ensure its a null
									// value that the provider can accept
									if (null != record)
										record.CoerceNullEditValue(field, ref convertedValue);

									cvh.Initialize(convertedValue, false);
								}
								else
								{
									error = ClipboardError.ValidationError;
									displayErrorDialog = true;
								}
							}
						}
					} 
					#endregion //Convert DisplayText to Value

					#region Handle Errors
					if (null != error)
					{
						hadConversionErrors = true;

						// no matter what the user chooses we cannot allow the cell to 
						// be modified so mark the cvh as such
						cvh.Ignore = true;

						Debug.Assert(null != operation);
						if (null != operation)
						{
							ClipboardErrorAction action = clipInfo.RaiseClipboardError(operation.Value, error.Value,
								ClipboardErrorAction.Continue, true, cellException, displayErrorDialog, record, field);

							switch (action)
							{
								case ClipboardErrorAction.Revert:
									return false;
								case ClipboardErrorAction.Stop:
									stopAtCell = new RowColumnIndex(r, c);
									return true;
								case ClipboardErrorAction.ClearCellAndContinue:
									{
										if (null != record)
										{
											// if we got back a null value then we want to ensure its a null
											// value that the provider can accept
											object nullValue = null;

											record.CoerceNullEditValue(field, ref nullValue);

											// do not ignore the cell. instead
											cvh.Ignore = false;

											cvh.Value = nullValue;
										}
										break;
									}
							}
						}

						// reset the state for the next possible error
						cellException = null;
						error = null;
						displayErrorDialog = false;
					} 
					#endregion //Handle Errors
				}
			}

			return true;
		}
		#endregion //ConvertToCellValue

		#region ConvertToDisplayText
		/// <summary>
		/// Helper method to ensure that all values in the collection represent display text.
		/// </summary>
		/// <param name="provider">Provider whose values are to be converted</param>
		internal static void ConvertToDisplayText(ICellValueProvider provider)
		{
			int rowCount = provider.RecordCount;
			int colCount = provider.FieldCount;

			for (int r = 0; r < rowCount; r++)
			{
				for (int c = 0; c < colCount; c++)
				{
					CellValueHolder cvh = provider.GetValue(r, c);

					if (null != cvh && !cvh.IsDisplayText)
					{
						Field field = provider.GetField(c);

						CellTextConverterInfo converter = CellTextConverterInfo.GetCachedConverter( field );

						string displayText = converter.ConvertCellValue(cvh.Value);
						cvh.Initialize(displayText, true);
					}
				}
			}

		}
		#endregion //ConvertToDisplayText

		#region CopyToClipboard
		internal bool CopyToClipboard()
	    {
			CellValueProviderSource source;
			ICellValueProvider provider = this.GetCellValueProvider(ClipboardOperation.Copy, out source);
			return CopyToClipboard(provider);
		}

		private bool CopyToClipboard(ICellValueProvider provider)
        {
			IDataObject dataObject = CreateDataObject(provider, false);

			if (null == dataObject)
				return false;

			// AS 12/2/09 TFS25338
			// The (Get|Set)DataObjects may fail.
			//
			//return ClipboardHelper.SetDataObject(dataObject, true);
			try
			{
				return ClipboardHelper.SetDataObject(dataObject, true);
			}
			catch (Exception ex)
			{
				this.RaiseClipboardError(ClipboardOperation.Copy, ClipboardError.Exception, ClipboardErrorAction.Stop, false, ex);
				return false;
			}
		}

        #endregion //CopyToClipboard

		#region CreateDataObject
		private IDataObject CreateDataObject(ICellValueProvider provider, bool isDragDrop)
		{
			if (null == provider || provider.FieldCount < 1 || provider.RecordCount < 1)
				return null;

			FieldLayout fl = provider.FieldLayout;
			Debug.Assert(null != fl);
			bool includeHeaders = fl != null ? fl.CopyFieldLabelsToClipboardResolved : false;

			IDictionary<Field, CellValueHolder> labels = includeHeaders ? new Dictionary<Field, CellValueHolder>() : null;

			#region Initialize the CellValueHolders

			// initialize all the values with those of the associated cells
			int cCount = provider.FieldCount;
			Field[] fields = new Field[cCount];

			for (int i = 0; i < cCount; i++)
			{
				Field f = provider.GetField(i);
				fields[i] = f;

				if (null != labels)
					labels[f] = new CellValueHolder(f.Label, false);
			}

			Debug.Assert(!GridUtilities.Contains(fields, null), "There shouldn't be any null field entries");

			for (int r = 0, rCount = provider.RecordCount; r < rCount; r++)
			{
				DataRecord record = provider.GetRecord(r);
				Debug.Assert(null != record);

				if (null != record)
				{
					for (int c = 0; c < cCount; c++)
					{
						// note we're using the converted value here since the conversion to text
						// will just use the valuetodisplay converter
						object convertedValue = record.GetCellValue(fields[c], true);
						CellValueHolder cvh = provider.GetValue(r, c);

						Debug.Assert(null != cvh);

						cvh.Initialize(convertedValue, false);
					}
				}
			}

			#endregion //Initialize the CellValueHolders

			// raise ClipboardCopying
			ClipboardCellValueHolderCollection cellValues = new ClipboardCellValueHolderCollection(provider, labels);
			ClipboardCopyingEventArgs copyingArgs = new ClipboardCopyingEventArgs(cellValues);

			_owner.RaiseClipboardCopying(copyingArgs);

			if (copyingArgs.Cancel)
				return null;

			IDataObject dataObject = ClipboardHelper.CreateDataObject();

			if (null == dataObject)
				return null;

			// make sure everything is display text we can put on the clipboard
			cellValues.ConvertToDisplayText();
			cellValues.ConvertLabelsToText();

			Dictionary<ClipboardEncoding, object> encodings = new Dictionary<ClipboardEncoding, object>();

			string[] formats = new string[] {
				GetClipboardDataFormat(typeof(ClipboardData)),
				DataFormats.Html,
				DataFormats.CommaSeparatedValue,
				DataFormats.Text,
				DataFormats.UnicodeText
				};

			foreach (string format in formats)
				AddClipboardData(dataObject, format, cellValues, includeHeaders, encodings);

			DataObjectCopyingEventArgs dataObjectCopyingArgs = new DataObjectCopyingEventArgs(dataObject, isDragDrop);
			_owner.RaiseEvent(dataObjectCopyingArgs);

			if (dataObjectCopyingArgs.CommandCancelled)
				return null;

			return dataObject;
		} 
		#endregion //CreateDataObject

		#region GetClipboardDataFormat
		internal static string GetClipboardDataFormat(Type type)
		{
			return type.FullName;
		} 
		#endregion //GetClipboardDataFormat

		#region GetClipboardOperation
		internal static ClipboardOperation? GetClipboardOperation(object context)
		{
			if (context == PasteOperationContext)
				return ClipboardOperation.Paste;

			if (context == ClearCellsOperationContext)
				return ClipboardOperation.ClearContents;

			return null;
		}
		#endregion //GetClipboardOperation

		#region GetRowColumnCount
		internal static void GetRowColumnCount(CellValueHolder[,] cellValues, out int rowCount, out int columnCount)
		{
			if (cellValues == null)
				rowCount = columnCount = 0;
			else
			{
				rowCount = cellValues.GetLength(0);
				columnCount = cellValues.GetLength(1);
			}
		}
		#endregion //GetRowColumnCount

		#region GetSelectionTarget
		private MultipleCellValuesAction.SelectionTarget GetSelectionTarget(CellValueProviderSource source)
		{
			switch (source)
			{
				case CellValueProviderSource.SelectedRecords:
				case CellValueProviderSource.ActiveRecord:
					return MultipleCellValuesAction.SelectionTarget.Records;
				case CellValueProviderSource.SelectedFields:
					return MultipleCellValuesAction.SelectionTarget.Fields;
				default:
					return MultipleCellValuesAction.SelectionTarget.Cells;
			}
		} 
		#endregion //GetSelectionTarget

		#region GetText
		internal static string GetText(CellValueHolder cvh)
		{
			GridUtilities.ValidateNotNull(cvh);
			return ClipboardOperationInfo.GetText(cvh.Value);
		}

		internal static string GetText(object value)
		{
			string text = null;

			if (null != value)
				text = Utilities.ConvertDataValue(value, typeof(string), null, null) as string;
			if (null == text)
				text = string.Empty;

			return text;
		}
		#endregion //GetText

		#region InitializeIgnoreState
		internal static void InitializeIgnoreState(ICellValueProvider provider, bool ignore)
		{
			IEnumerator<CellValueHolder> cvh = provider.GetValueEnumerator();

			if (null != cvh)
			{
				while (cvh.MoveNext())
					cvh.Current.Ignore = ignore;
			}
		}
		#endregion //InitializeIgnoreState

		#region IsDeleted
		internal static bool IsDeleted(Cell cell)
		{
			Debug.Assert(cell != null);

			if (null == cell)
				return true;

			return IsDeleted(cell.Record);
		}

		internal static bool IsDeleted(DataRecord record)
		{
			if (null == record)
				return true;

			// AS 5/20/09
			// Changed to use the new IsStillValid property on record rather than
			// trying to calculate it.
			//
			return !record.IsStillValid;
		}
		#endregion //IsDeleted

		#region IsSameDataItem
		internal static bool IsSameDataItem(object item1, object item2)
		{
			if (object.ReferenceEquals(item1, item2))
				return true;

			item1 = DataBindingUtilities.GetObjectForComparison(item1);
			item2 = DataBindingUtilities.GetObjectForComparison(item2);

			// to be consistent with comparisons elsewhere we should compare references
			// and not equality. multiple data items could be equal but be different 
			// objects. through the DP and wingrid we compared by reference
			//return object.Equals(item1, item2);
			return item1 == item2;
		} 
		#endregion //IsSameDataItem

        #region PerformOperation
        internal bool PerformOperation(ICommand command)
        {
            ClipboardOperation operation;

            if (command == DataPresenterCommands.Cut)
                operation = ClipboardOperation.Cut;
            else if (command == DataPresenterCommands.Copy)
                operation = ClipboardOperation.Copy;
            else if (command == DataPresenterCommands.Paste)
                operation = ClipboardOperation.Paste;
            else if (command == DataPresenterCommands.ClearCellContents)
                operation = ClipboardOperation.ClearContents;
            else
                return false;

            return this.PerformOperation(operation);
        }

        internal bool PerformOperation(ClipboardOperation operation)
        {
			if (!this.CanPerformOperation(operation))
				return false;

            switch (operation)
            {
				case ClipboardOperation.Copy:
					return this.CopyToClipboard();
				case ClipboardOperation.Paste:
					return this.PerformPaste();
				case ClipboardOperation.ClearContents:
					// AS 3/10/11 NA 2011.1 - Async Exporting
					if ( !_owner.VerifyOperationIsAllowed(UIOperation.ClearCellContents) )
						return false;

					return this.ClearCellContents();
				case ClipboardOperation.Cut:
					{
						// AS 3/10/11 NA 2011.1 - Async Exporting
						if ( !_owner.VerifyOperationIsAllowed(UIOperation.ClearCellContents) )
							return false;

						CellValueProviderSource source;
						ICellValueProvider provider = GetCellValueProvider(ClipboardOperation.Cut, out source);

						if (!this.CopyToClipboard(provider))
							return false;

						this.ClearCellContents(provider, source);
						return true;
					}
				default:
					Debug.Fail("Unexpected operation:" + operation.ToString());
					return false;
            }
        }

        #endregion //PerformOperation

		#region RaiseClipboardError
		internal ClipboardErrorAction RaiseClipboardError(ClipboardOperation operation,
			ClipboardError error, ClipboardErrorAction defaultAction,
			bool canContinue, Exception exception)
		{
			return RaiseClipboardError(operation, error, defaultAction, canContinue, exception, true, null, null);
		}

		internal ClipboardErrorAction RaiseClipboardError(ClipboardOperation operation,
			ClipboardError error, ClipboardErrorAction defaultAction,
			bool canContinue, Exception exception, bool defaultDisplayErrorMessage, DataRecord record, Field field)
		{
			string errorMessage = GetErrorMessage(operation, error, exception, canContinue, record, field);

			Debug.Assert(!string.IsNullOrEmpty(errorMessage), "There should be a default error message");

			Debug.Assert(!canContinue || !defaultDisplayErrorMessage ||
				(defaultAction == ClipboardErrorAction.Continue || defaultAction == ClipboardErrorAction.ClearCellAndContinue),
				"The error can be recovered but we're not defaulting to that action so this routine will not return (ClearCellAnd)Continue even if the user chooses Ok.");

			ClipboardOperationErrorEventArgs args = new ClipboardOperationErrorEventArgs(
				operation, error, record, field, errorMessage, exception, canContinue);

			args.Action = defaultAction;
			args.DisplayErrorMessage = defaultDisplayErrorMessage;

			_owner.RaiseClipboardOperationError(args);

			ClipboardErrorAction action = args.Action;

			// if the operation is not allowed to continue and the developer sets it to continue/clearcontinue
			// then use the default action
			if (canContinue == false && action == ClipboardErrorAction.ClearCellAndContinue || action == ClipboardErrorAction.Continue)
				action = defaultAction;

			if (args.DisplayErrorMessage && !string.IsNullOrEmpty(args.Message))
			{
				string message = args.Message;
				string caption = GetErrorCaption(operation);
				MessageBoxButton button = args.CanContinueWithRemainingCells
					? MessageBoxButton.OKCancel
					: MessageBoxButton.OK;

				MessageBoxResult result = Utilities.ShowMessageBox(_owner, message, caption, button, MessageBoxImage.Exclamation);

				if (result == MessageBoxResult.Cancel)
					action = ClipboardErrorAction.Stop;
			}

			return action;
		}

		#endregion //RaiseClipboardError

		#region ShouldSkipFieldModification
		internal static bool ShouldSkipFieldModification(Field f)
		{
			if (null == f || f.DisallowModificationViaClipboard)
				return true;

			return false;
		}
		#endregion //ShouldSkipFieldModification

		#endregion //Internal Methods

        #region Private Methods

		#region AddClipboardData
		private bool AddClipboardData(IDataObject dataObject, string format, ClipboardCellValueHolderCollection cellValues, bool includeHeader, Dictionary<ClipboardEncoding, object> encodings)
		{
			if (!this.ShouldAddFormat(dataObject, format))
				return false;

			Debug.Assert(null != encodings);
			ClipboardEncoding encoding = ClipboardEncodingFactory.CreateEncoding(_owner, dataObject, format);

			if (null != encoding)
			{
				object data;

				// optimization. if we've already used an encoding for a different format
				// then we can reuse the data from that encoding
				if (encodings == null || !encodings.TryGetValue(encoding, out data))
				{
					data = encoding.GetClipboardData(cellValues, includeHeader);

					if (null != encodings)
						encodings.Add(encoding, data);
				}

				if (null != data)
				{
					dataObject.SetData(format, data);
					return true;
				}
			}

			return false;
		} 
		#endregion //AddClipboardData

        #region CreateRectangularArray
        private Cell[] CreateRectangularArray(Cell[] cells, bool allowSparse, out int fieldCount, out bool isSparse)
        {
            Dictionary<Field, object> fields = new Dictionary<Field, object>();
            int recordCount = 0;
            Record previousRecord = null;

            for (int i = 0; i < cells.Length; i++)
            {
                Cell cell = cells[i];

                if (cell.Record != previousRecord)
                {
                    previousRecord = cell.Record;
                    recordCount++;
                }

                Field f = cells[i].Field;

                if (!fields.ContainsKey(f))
                {
                    Debug.Assert(f.NavigationIndex >= 0, "Why are there cells associated with fields that are not in the navigation order?");

                    fields.Add(f, null);
                }
            }

            // if its non-sparse...
            fieldCount = fields.Count;

            // if its rectangular then just return it
            if (recordCount * fieldCount == cells.Length)
            {
                isSparse = false;
                return cells;
            }

            if (recordCount == 0 || fieldCount == 0)
            {
                Debug.Fail("There are no cells");
                isSparse = false;
                return new Cell[0];
            }

            isSparse = true;

            if (allowSparse == false)
                return null;

            // the list is sparse and sparse is supported...
            Cell[] sparseCells = new Cell[0];

            FieldLayout fl = cells[0].Record.FieldLayout;
            Debug.Assert(null != fl);

            if (null != fl)
            {
				Field[] navigationOrderFields = GetNavigationOrderFields(fl);
                Debug.Assert(null != navigationOrderFields);

                if (null != navigationOrderFields)
                {
                    // first build a sorted list of the fields so we can map into
                    // their sparse position
                    int[] columnIndexes = new int[navigationOrderFields.Length];
                    int usedIndex = 0;

                    for (int i = 0; i < navigationOrderFields.Length; i++)
                    {
                        Field field = navigationOrderFields[i];

                        int index;

                        if (fields.ContainsKey(field))
                            index = usedIndex++;
                        else
                            index = -1;

                        columnIndexes[i] = index;
                    }

                    sparseCells = new Cell[recordCount * fieldCount];
                    previousRecord = null;
                    int recordIndex = -1;

                    for (int i = 0; i < cells.Length; i++)
                    {
                        Cell cell = cells[i];

						if (cell.Record != previousRecord)
						{
							previousRecord = cell.Record;
							recordIndex++;
						}

                        Field field = cell.Field;
                        sparseCells[recordIndex * fieldCount + columnIndexes[field.NavigationIndex]] = cell;
                    }
                }
            }

            return sparseCells;
        }
        #endregion //CreateRectangularArray

        #region GetCellValueProvider
		private ICellValueProvider GetCellValueProvider(ClipboardOperation operation, out CellValueProviderSource source)
        {
			source = CellValueProviderSource.Unknown;
            DataPresenterBase.SelectedItemHolder selectedItems = _owner.SelectedItems;
			Debug.Assert(null != selectedItems);

            if (selectedItems == null)
                return null;

            switch (operation)
            {
                case ClipboardOperation.Cut:
                case ClipboardOperation.Copy:
                case ClipboardOperation.Paste:
                    if (selectedItems.HasMixedSelection)
                    {
						this.RaiseClipboardError(operation, ClipboardError.MixedSelection, ClipboardErrorAction.Stop, false, null);
						return null;
                    }
                    break;
            }

			if (selectedItems.HasFields)
			{
				FieldLayout fl = selectedItems.Fields[0].Owner;
				DataRecord[] dataRecords = GetSelectedFieldRecords(fl, false);

				if (dataRecords == null || dataRecords.Length == 0)
					return null;

				Field[] fields = selectedItems.Fields.ToArray();

				// sort the fields based on the navigation order
				Utilities.SortMergeGeneric(fields, FieldNavigationIndexComparer.Instance);

				source = CellValueProviderSource.SelectedFields;
				return new RecordCellRangeValueProvider(dataRecords, fields);
			}

            if (selectedItems.HasCells)
            {
                Cell[] cells = selectedItems.Cells.ToArray();
                Utilities.SortMergeGeneric<Cell>(cells, CellVisiblePositionComparer.Instance);

                int fieldCount;
                bool allowSparse = operation == ClipboardOperation.ClearContents;
                bool isSparse;

                cells = CreateRectangularArray(cells, allowSparse, out fieldCount, out isSparse);

                if (!allowSparse && isSparse)
                {
					// if its sparse for a paste we just won't use the selection but will
					// follow the previous behavior of using the anchor point
					if (operation == ClipboardOperation.Paste)
						return null;

                    this.RaiseClipboardError(operation, ClipboardError.NonRectangularSelection, ClipboardErrorAction.Stop, false, null);
                    return null;
                }

				bool isInSameFieldLayout = IsInSameFieldLayout(cells);

				if (!isInSameFieldLayout)
				{
					Debug.Fail("This shouldn't be allowed. If it is then we need to account for it in the cellvalueprovider and only ever deal with cells.");
					return null;
				}

				source = CellValueProviderSource.SelectedCells;

                Debug.Assert(null != cells && cells.Length > 0);
                return new GridCellRangeValueProvider(cells, fieldCount);
            }

            if (selectedItems.HasRecords)
            {
                foreach (Record record in selectedItems.Records)
                {
                    if (record is DataRecord == false)
                        return null;
                }

                Record[] records = selectedItems.Records.ToArray();
                Utilities.SortMergeGeneric<Record>(records, RecordVisiblePositionComparer.Instance);

                DataRecord[] dataRecords = new DataRecord[records.Length];
                records.CopyTo(dataRecords, 0);

				bool isInSameFieldLayout = this.IsInSameFieldLayout(dataRecords);

				if (!isInSameFieldLayout)
				{
					Debug.Fail("This shouldn't be allowed. If it is then we need to account for it in the cellvalueprovider and only ever deal with cells.");
					return null;
				}

				source = CellValueProviderSource.SelectedRecords;

                return new RecordCellRangeValueProvider(dataRecords, GetNavigationOrderFields(records[0].FieldLayout));

            }

			// for paste we only care about the selected collections. if there is no
			// selection then it will provide its own anchor logic
			if (operation == ClipboardOperation.Paste)
				return null;

            if (!selectedItems.HasSelection)
            {
				if (null != _owner.ActiveCell)
				{
					source = CellValueProviderSource.ActiveCell;
					return new GridCellRangeValueProvider(new Cell[] { _owner.ActiveCell }, 1);
				}

                DataRecord activeDataRecord = _owner.ActiveRecord as DataRecord;

                if (activeDataRecord != null)
                {
					source = CellValueProviderSource.ActiveRecord;

					return new RecordCellRangeValueProvider(
                        new DataRecord[] { activeDataRecord }, 
						GetNavigationOrderFields(activeDataRecord.FieldLayout)
                        );
                }
            }

            return null;
        }

        #endregion //GetCellValueProvider

        #region GetErrorCaption
        private string GetErrorCaption(ClipboardOperation operation)
        {
            string resourceId;

            switch (operation)
            {
                case ClipboardOperation.ClearContents:
                    resourceId = "ClipboardError_ClearContentsTitle";
                    break;
                case ClipboardOperation.Cut:
                    resourceId = "ClipboardError_CutTitle";
                    break;
                case ClipboardOperation.Copy:
                    resourceId = "ClipboardError_CopyTitle";
                    break;
                case ClipboardOperation.Paste:
                    resourceId = "ClipboardError_PasteTitle";
                    break;
                default:
                    resourceId = null;
                    break;
            }

            Debug.Assert(null != resourceId, "No resource specified for operation:" + operation.ToString());

            return null != resourceId
                ? DataPresenterBase.GetString(resourceId)
                : string.Empty;
        }
        #endregion //GetErrorCaption

        #region GetErrorMessage
        private string GetErrorMessage(ClipboardOperation operation, ClipboardError error, Exception exception, bool canContinue, DataRecord record, Field field)
        {
			string message = null;
			string exceptionMessage = exception != null ? exception.Message : string.Empty;
			string fieldLabel = null != field ? GetText(field.Label ?? field.Name) : string.Empty;

			switch (error)
			{
				default:
					Debug.Fail("Cannot provide message for unrecognized error:" + error.ToString());
					message = string.Empty;
					break;

				case ClipboardError.MixedSelection:
					Debug.Assert(canContinue == false);
					message = DataPresenterBase.GetString("ClipboardError_MixedSelectionMessage");
					break;
				case ClipboardError.NonRectangularSelection:
					Debug.Assert(canContinue == false);
					message = DataPresenterBase.GetString("ClipboardError_NonRectangularSelectionMessage");
					break;
				case ClipboardError.NotEnoughColumns:
					Debug.Assert(canContinue);
					message = DataPresenterBase.GetString("ClipboardError_NotEnoughColumnsMessage");
					break;
				case ClipboardError.NotEnoughRows:
					Debug.Assert(canContinue);
					message = DataPresenterBase.GetString("ClipboardError_NotEnoughRowsMessage");
					break;
				case ClipboardError.ConversionError:
					Debug.Assert(canContinue);
					message = DataPresenterBase.GetString("ClipboardError_ConversionErrorMessage", fieldLabel, exceptionMessage);
					break;
				case ClipboardError.ValidationError:
					Debug.Assert(canContinue);
					message = DataPresenterBase.GetString("ClipboardError_ValidationErrorMessage", exceptionMessage);
					break;
				case ClipboardError.SetCellValueError:
					Debug.Assert(canContinue);
					message = DataPresenterBase.GetString("ClipboardError_SetCellValueErrorMessage", exceptionMessage);
					break;
				case ClipboardError.UpdateRecordError:
					Debug.Assert(canContinue);
					message = DataPresenterBase.GetString("ClipboardError_UpdateRecordErrorMessage");
					break;
				case ClipboardError.InsertRecordError:
					Debug.Assert(!canContinue);
					message = DataPresenterBase.GetString("ClipboardError_InsertRecordErrorMessage");
					break;
				case ClipboardError.ReadOnlyCell:
					Debug.Assert(canContinue);
					Debug.Assert(null != field);
					message = DataPresenterBase.GetString("ClipboardError_ReadOnlyCellMessage", fieldLabel);
					break;
				case ClipboardError.Exception:
					Debug.Assert(null != exception);
					// AS 12/2/09 TFS25338
					//Debug.Assert(null != record);
					//Debug.Assert(null != field);
					string resourceId;
					if (canContinue)
						resourceId = "ClipboardError_ExceptionMessageCanContinue";
					else
						resourceId = "ClipboardError_ExceptionMessageCannotContinue";

					message = DataPresenterBase.GetString(resourceId, exceptionMessage);
					break;
			}

			return message;
        }
        #endregion //GetErrorMessage

		#region GetNavigationOrderFields
		private static Field[] GetNavigationOrderFields(FieldLayout fl)
		{
			FieldLayoutTemplateGenerator generator = fl.StyleGenerator;
			Debug.Assert(null != generator);

			return null != generator ? generator.GridFieldMap.GetNavigationOrderFields() : new Field[0];
		} 
		#endregion //GetNavigationOrderFields

		#region GetPasteCellValueProvider
		private ICellValueProvider GetPasteCellValueProvider(CellValueHolder[,] cellValues, Cell targetCell, out CellValueProviderSource source)
		{
			source = CellValueProviderSource.Unknown;

			if (null == cellValues || cellValues.Length == 0)
				return null;

			if (null != targetCell)
			{
				if (IsDeleted(targetCell))
					return null;

				return GetPasteCellValueProvider(cellValues, targetCell);
			}

			#region Replicate Values

			ICellValueProvider selectionBasedProvider = this.GetCellValueProvider(ClipboardOperation.Paste, out source);

			// if we got back a valid provider...
			if (null != selectionBasedProvider)
			{
				int rowCount, columnCount;
				GetRowColumnCount(cellValues, out rowCount, out columnCount);

				int selectionFieldCount = selectionBasedProvider.FieldCount;
				int selectionRecordCount = selectionBasedProvider.RecordCount;

				// if the values are an even multiple of the selection target
				// then we can replicate the values as excel does and apply
				// those to the selection
				if (selectionFieldCount > 0 &&
					selectionRecordCount > 0 &&
					selectionFieldCount >= columnCount &&
					selectionRecordCount >= rowCount &&
					selectionFieldCount % columnCount == 0 &&
					selectionRecordCount % rowCount == 0)
				{
					Debug.Assert(selectionBasedProvider.GetRecord(selectionRecordCount - 1) != null);

					for (int r = 0; r < selectionRecordCount; r++)
					{
						for (int c = 0; c < selectionFieldCount; c++)
							selectionBasedProvider.SetValue(r, c, cellValues[r % rowCount, c % columnCount].Clone());
					}

					return selectionBasedProvider;
				}
			} 
			#endregion //Replicate Values

			DataPresenterBase.SelectedItemHolder selectedItems = _owner.SelectedItems;
			Debug.Assert(null != selectedItems);

			if (selectedItems == null)
				return null;

			// like excel, we do not allow a clipboard operation to be performed when there 
			// are multiple types of things selected (e.g. cells and columns). we used to do
			// the check here but the GetCellValueProvider above should do this check for us
			Debug.Assert(!selectedItems.HasMixedSelection);

			// we prefer using the selected cells
			if (selectedItems.HasCells)
			{
				// find the anchor cell (i.e the top/left most cell) from which 
				// we will start the paste operation. we will expand out the selection
				// based on the amount of information in the clipboard
				Cell topMostCell = selectedItems.Cells[0];

				foreach(Cell selectedCell in selectedItems.Cells)
				{
					if (CellVisiblePositionComparer.Instance.Compare(selectedCell, topMostCell) < 0)
						topMostCell = selectedCell;
				}

				source = CellValueProviderSource.SelectedCells;

				return GetPasteCellValueProvider(cellValues, topMostCell);
			}

			if (selectedItems.HasRecords)
			{
				// use the top most record as the anchor record
				Record topMostRecord = selectedItems.Records[0];

				foreach (Record selectedRecord in selectedItems.Records)
				{
					if (RecordVisiblePositionComparer.Instance.Compare(selectedRecord, topMostRecord) < 0)
						topMostRecord = selectedRecord;
				}

				source = CellValueProviderSource.SelectedRecords;

				return GetPasteCellValueProvider(cellValues, topMostRecord);
			}

			if (selectedItems.HasFields)
			{
				FieldLayout fl = GetTargetFieldLayout(ClipboardOperation.Paste);

				DataRecord[] anchorRecords = GetSelectedFieldRecords(fl, true);

				if (anchorRecords == null || anchorRecords.Length == 0)
					return null;

				DataRecord anchorRecord = anchorRecords[0];

				// sort the fields and use the first as the anchor
				Field[] fields = selectedItems.Fields.ToArray();
				Utilities.SortMergeGeneric(fields, FieldNavigationIndexComparer.Instance);

				source = CellValueProviderSource.SelectedFields;
				return GetPasteCellValueProvider(cellValues, anchorRecord, fields[0].NavigationIndex);
			}

			if (!selectedItems.HasSelection)
			{
				// use the active cell as the anchor cell
				if (null != _owner.ActiveCell)
				{
					source = CellValueProviderSource.ActiveCell;
					return GetPasteCellValueProvider(cellValues, _owner.ActiveCell);
				}

				if (null != _owner.ActiveRecord)
				{
					source = CellValueProviderSource.ActiveRecord;
					return GetPasteCellValueProvider(cellValues, _owner.ActiveRecord);
				}
			}

			return null;
		}

		private ICellValueProvider GetPasteCellValueProvider(CellValueHolder[,] cellValues, Cell anchorCell)
		{
			Debug.Assert(null != anchorCell);
			Debug.Assert(null != cellValues);

			Field f = anchorCell.Field;
			int navIndex = f.NavigationIndex;
			Debug.Assert(navIndex >= 0);

			if (navIndex < 0)
				return null;

			return GetPasteCellValueProvider(cellValues, anchorCell.Record, navIndex);
		}

		private ICellValueProvider GetPasteCellValueProvider(CellValueHolder[,] cellValues, Record anchorRecord)
		{
			return GetPasteCellValueProvider(cellValues, anchorRecord, null);
		}

		private ICellValueProvider GetPasteCellValueProvider(CellValueHolder[,] cellValues, Record anchorRecord, int? firstFieldIndex)
		{
			DataRecord dr = anchorRecord as DataRecord;

			if (dr == null)
				return null;

			int navIndex = firstFieldIndex == null ? 0 : firstFieldIndex.Value;

			FieldLayout fl = anchorRecord.FieldLayout;

			if (null == fl)
				return null;

			// first check to see if we have enough fields to accomodate the paste
			Field[] navigationFields = GetNavigationOrderFields(fl);
			int rowCount, columnCount;
			GetRowColumnCount(cellValues, out rowCount, out columnCount);

			if (navigationFields.Length - navIndex < columnCount)
			{
				// if based on the anchor cell there are not enough "columns" then 
				// we need to raise the clipboard error. this however is recoverable so 
				// give the programmer the option of just filling as many cells as 
				// possible
				ClipboardErrorAction result = this.RaiseClipboardError(ClipboardOperation.Paste, ClipboardError.NotEnoughColumns, ClipboardErrorAction.Continue, true, null);

				if (result != ClipboardErrorAction.Continue)
					return null;
			}

			// next get the records that will be affected
			DataRecord[] records = null;
			bool keepExtraRowData = false;

			// we want to special case the filter record. if the target is the filter record
			// we want to ignore all 
			if (dr is FilterRecord)
			{
				// if the target is the filter record and we have more than 1 record worth of data then
				// raise an error but allow the operation to continue ignoring all but the filter record
				if (rowCount > 1)
				{
					ClipboardErrorAction result = this.RaiseClipboardError(ClipboardOperation.Paste, ClipboardError.NotEnoughRows, ClipboardErrorAction.Continue, true, null, false, dr, null);

					if (result != ClipboardErrorAction.Continue)
						return null;
				}

				records = new DataRecord[] { dr };
				rowCount = 1;
			}
			else if (dr.IsAddRecord)
			{
				// when pasting into the add record we will create new records
				// based on the number of rows of data
				records = new DataRecord[] { dr };
				keepExtraRowData = true;
			}
			else
			{
				Debug.Assert(!dr.IsSpecialRecord);

				if (dr.IsSpecialRecord)
					return null;

				// for all other cases just get the regular data rows as the 
				// target records based on the anchor record
				records = GetRecords(dr, rowCount, true, true, false);
			}

			Debug.Assert(null != records, "Should we be raising an error for this condition?");

			if (records == null)
				return null;

			if (records.Length < rowCount && !keepExtraRowData)
			{
				// if based on the anchor cell there are not enough "rows" then 
				// we need to raise the clipboard error. this however is recoverable so 
				// give the programmer the option of just filling as many cells as 
				// possible
				bool canContinue = records.Length > 0;
				ClipboardErrorAction result = this.RaiseClipboardError(ClipboardOperation.Paste, ClipboardError.NotEnoughRows, ClipboardErrorAction.Continue, canContinue, null);

				if (!canContinue || result != ClipboardErrorAction.Continue)
					return null;
			}

			// get the fields involved in the operation
			int fieldCount = Math.Min(navigationFields.Length - navIndex, columnCount);
			Field[] fields = new Field[fieldCount];

			for (int i = 0; i < fieldCount; i++)
				fields[i] = navigationFields[i + navIndex];

			return new RecordCellRangeValueProvider(records, fields, cellValues, keepExtraRowData);
		}
		#endregion //GetPasteCellValueProvider

		#region GetPreferredPasteFormat
		private string GetPreferredPasteFormat(IDataObject dataObject)
		{
			Debug.Assert(null != dataObject);

			if (dataObject.GetDataPresent(typeof(ClipboardData)))
				return GetClipboardDataFormat(typeof(ClipboardData));

			// AS 8/21/09 TFS21203
			// The CSV that excel puts into the clipboard seems to contain ascii charaters and 
			// not any unicode/dbcs characters so such characters appear as ?'s. The UnicodeText 
			// format has the correct values so we're going to give more preference to that 
			// format.
			//
			//if (dataObject.GetDataPresent(DataFormats.CommaSeparatedValue))
			//	return DataFormats.CommaSeparatedValue;

			if (dataObject.GetDataPresent(DataFormats.UnicodeText))
				return DataFormats.UnicodeText;

			if (dataObject.GetDataPresent(DataFormats.Text))
				return DataFormats.Text;

			// AS 8/21/09 TFS21203
			if (dataObject.GetDataPresent(DataFormats.CommaSeparatedValue))
				return DataFormats.CommaSeparatedValue;

			return string.Empty;
		} 
		#endregion //GetPreferredPasteFormat 

		#region GetRecords
		private static DataRecord[] GetRecords(DataRecord anchorRecord, int recordCount, 
			bool ignoreSpecialRecords, bool ignoreAddRecordTemplate, bool alwaysIncludeAnchor)
		{
			List<DataRecord> records = new List<DataRecord>();

			int startingIndex = anchorRecord.VisibleIndex;

			if (startingIndex >= 0)
			{
				FieldLayout fl = anchorRecord.FieldLayout;
				ViewableRecordCollection viewableRecords = anchorRecord.ParentCollection.ViewableRecords;

				int maxCount = viewableRecords.Count - startingIndex;

				for (int i = 0; i < maxCount; i++)
				{
					DataRecord dr = viewableRecords[i + startingIndex] as DataRecord;

					if (null != dr && dr.FieldLayout == fl)
					{
						if (dr != anchorRecord || !alwaysIncludeAnchor)
						{
							if (ignoreAddRecordTemplate && dr.IsAddRecordTemplate)
								continue;

							if (ignoreSpecialRecords && dr.IsSpecialRecord)
								continue;
						}

						// always allow the anchor record but if a limiting record type is specified 
						// then skip all other types of records
						records.Add(dr);

						if (records.Count == recordCount)
							break;
					}
				}
			}

			return records.ToArray();
		} 
		#endregion //GetRecords

		#region GetSelectedFieldRecords
		private DataRecord[] GetSelectedFieldRecords(FieldLayout fieldLayout, bool firstOnly)
		{
			DataRecord activeRecord = _owner.ActiveRecord as DataRecord;
			RecordManager rm = null != activeRecord 
				? activeRecord.RecordManager 
				: _owner.RecordManager;

			if (null == rm)
				return null;

			ViewableRecordCollection viewableRecords = rm.Sorted.ViewableRecords;
			List<DataRecord> records = new List<DataRecord>();

			foreach (Record record in viewableRecords)
			{
				DataRecord dataRecord = record as DataRecord;

				if (null != dataRecord &&
					dataRecord.IsDataRecord &&
					dataRecord.FieldLayout == fieldLayout)
				{
					records.Add(dataRecord);

					if (firstOnly)
						break;
				}
			}

			return records.ToArray();
		}
		#endregion //GetSelectedFieldRecords

		#region GetTargetFieldLayout
		private FieldLayout GetTargetFieldLayout(ClipboardOperation operation)
		{
			DataPresenterBase.SelectedItemHolder selectionHolder = _owner.SelectedItems;

			if (selectionHolder.HasCells)
				return selectionHolder.Cells[0].Field.Owner;

			if (selectionHolder.HasRecords)
				return selectionHolder.Records[0].FieldLayout;

			if (_owner.ActiveCell != null)
				return _owner.ActiveCell.Field.Owner;

			if (_owner.ActiveRecord != null)
				return _owner.ActiveRecord.FieldLayout;

			if (selectionHolder.HasFields)
				return selectionHolder.Fields[0].Owner;

			return null;
		}
		#endregion //GetTargetFieldLayout

		// AS 5/6/09 Selection across field layouts
		#region IsInSameFieldLayout
		private bool IsInSameFieldLayout(DataRecord[] dataRecords)
		{
			if (dataRecords != null && dataRecords.Length > 1)
			{
				FieldLayout fl = dataRecords[0].FieldLayout;

				for (int i = 1; i < dataRecords.Length; i++)
				{
					if (dataRecords[i].FieldLayout != fl)
						return false;
				}
			}

			return true;
		}

		private static bool IsInSameFieldLayout(Cell[] cells)
		{
			if (cells != null && cells.Length > 1)
			{
				// AS 8/19/09 TFS19133
				// Since the cell array could be sparse, the first cell could be null.
				//
				//FieldLayout fl = cells[0].Record.FieldLayout;
				//
				//for (int i = 1; i < cells.Length; i++)
				//{
				//    Cell cell = cells[i];
				//
				//    if (null != cell && cell.Record.FieldLayout != fl)
				//        return false;
				//}
				FieldLayout fl = null;

				for (int i = 0; i < cells.Length; i++)
				{
					Cell cell = cells[i];

					if (null != cell)
					{
						FieldLayout flTemp = cell.Record.FieldLayout;

						if (fl == null)
							fl = flTemp;
						else if (fl != flTemp)
							return false;
					}
				}
			}

			return true;
		}
		#endregion //IsInSameFieldLayout

		#region PerformPaste
		private bool PerformPaste()
		{
			// AS 12/2/09 TFS25338
			// The (Get|Set)DataObjects may fail.
			//
			//IDataObject dataObject = ClipboardHelper.GetDataObject();
			IDataObject dataObject = null;
			try
			{
				dataObject = ClipboardHelper.GetDataObject();
			}
			catch (Exception ex)
			{
				this.RaiseClipboardError(ClipboardOperation.Paste, ClipboardError.Exception, ClipboardErrorAction.Stop, false, ex);
				return false;
			}

			return PerformPaste(dataObject, false, null);
		}

		private bool PerformPaste(IDataObject dataObject, bool isDragDrop, Cell targetCell)
		{
			if (null == dataObject)
				return false;

			// get the format that we would like to use to provide the data for the cells
			string pasteFormat = GetPreferredPasteFormat(dataObject);

			// AS 10/16/09 TFS23241
			// If there are no formats that we can use then we cannot perform the paste operation.
			//
			if (string.IsNullOrEmpty(pasteFormat))
				return false;

			// give the programmer the option to alter which format we will use (or even
			// cancel the paste operation
			DataObjectPastingEventArgs dataPastingArgs = new DataObjectPastingEventArgs(dataObject, isDragDrop, pasteFormat);
			_owner.RaiseEvent(dataPastingArgs);

			if (dataPastingArgs.CommandCancelled)
				return false;

			// get the dataobject provided in case the programmer switches it out
			dataObject = dataPastingArgs.DataObject;

			if (dataObject == null)
				return false;

			// get the the format again in case the programmer changed it
			pasteFormat = dataPastingArgs.FormatToApply;

			// if the format they specified doesn't exit then bail out
			if (!dataObject.GetDataPresent(pasteFormat))
				return false;

			// get the clipboard encoding associated with the specified format if any
			ClipboardEncoding encoding = ClipboardEncodingFactory.CreateEncoding(_owner, dataObject, pasteFormat);

			if (null == encoding)
				return false;

			// if we got one then get the values that we will ultimately apply to the cells
			CellValueHolder[,] values = encoding.GetCellValues(pasteFormat, dataObject);

			// assuming we got values to paste we need to create an IProvideCellValueHolder
			// that will map those cellvalueholders to the appropriate cells
			CellValueProviderSource source;
			ICellValueProvider pasteValueHolder = this.GetPasteCellValueProvider(values, targetCell, out source);

			if (null == pasteValueHolder)
				return false;

			// if all of the fields associated with the operation have their 
			// DisallowModificationViaClipboard set to true then consider this
			// a no-op and bail out.
			//
			if (!ContainsModifiableFields(pasteValueHolder))
				return false;

			// get the cell value holder to use for the pasting event
			CellValueHolderCollection cellValues = new CellValueHolderCollection(pasteValueHolder);

			if (null == cellValues)
				return false;

			// now we can raise our event which gives the programmer the option of parsing the 
			// values themselves or adjusting them in some fashion before we apply them to the 
			// cells
			ClipboardPastingEventArgs pastingArgs = new ClipboardPastingEventArgs(cellValues, dataObject);
			_owner.RaiseClipboardPasting(pastingArgs);

			if (pastingArgs.Cancel)
				return false;

			// actually update the cells
			MultipleCellValuesAction action = new MultipleCellValuesAction(cellValues.ValueProvider, true, GetSelectionTarget(source));
			return _owner.History.PerformAction(action, PasteOperationContext);
		}

		#endregion //PerformPaste

        #region ShouldAddFormat
        private bool ShouldAddFormat(IDataObject dataObject, string clipboardFormat)
        {
            DataObjectSettingDataEventArgs args = new DataObjectSettingDataEventArgs(dataObject, clipboardFormat);

            _owner.RaiseEvent(args);

            return !args.CommandCancelled;
        }
        #endregion //ShouldAddFormat

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