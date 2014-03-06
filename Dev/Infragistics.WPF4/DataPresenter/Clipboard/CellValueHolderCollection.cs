using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Globalization;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// Collection of <see cref="CellValueHolder"/> instances
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Events.ClipboardOperationEventArgs.Values"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
    public class CellValueHolderCollection : ICollection, IEnumerable<CellValueHolder>
    {
        #region Member Variables

		ICellValueProvider _valueProvider;

        #endregion //Member Variables

        #region Constructor
		internal CellValueHolderCollection(ICellValueProvider valueProvider)
        {
            GridUtilities.ValidateNotNull(valueProvider, "valueProvider");
            _valueProvider = valueProvider;

            Debug.Assert(valueProvider.FieldCount > 0);
        }
        #endregion //Constructor

        #region Properties

        #region Internal Properties

		#region ValueProvider
		internal ICellValueProvider ValueProvider
		{
			get { return _valueProvider; }
		} 
		#endregion //ValueProvider

        #endregion //Internal Properties

        #region Public Properties

        #region Indexer
        /// <summary>
        /// Returns the <see cref="CellValueHolder"/> for the cell at the specified position.
        /// </summary>
        /// <param name="recordIndex">The 0 based index of the row whose cell value is to be returned.</param>
        /// <param name="fieldIndex">The 0 based index of the column whose cell value is to be returned.</param>
        /// <returns>The <see cref="CellValueHolder"/> associated with the cell at the specified position or null if there is no cell at that position.</returns>
        public CellValueHolder this[int recordIndex, int fieldIndex]
        {
            get
            {
                return _valueProvider.GetValue(recordIndex, fieldIndex);
            }
        }

        /// <summary>
        /// Returns the <see cref="CellValueHolder"/> for the specified cell.
        /// </summary>
        /// <param name="cell">The <see cref="Cell"/> whose associated value is to be returned.</param>
        /// <returns>A <see cref="CellValueHolder"/> instance for the specified cell or null if there is no value associated with the specified cell</returns>
        public CellValueHolder this[Cell cell]
        {
            get
            {
                return _valueProvider.GetValue(cell);
            }
        } 
        #endregion //Indexer

		#region FieldCount
		/// <summary>
        /// Returns the number of fields represented in the collection.
        /// </summary>
        public int FieldCount
        {
            get
            {
                return this._valueProvider.FieldCount;
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
                return this._valueProvider.RecordCount;
            }
		}
		#endregion //RecordCount

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#endregion //Public Methods

		#region Internal Methods

		#region ConvertToDisplayText
		/// <summary>
		/// Ensures that the values represent display text.
		/// </summary>
		internal void ConvertToDisplayText()
		{
			ClipboardOperationInfo.ConvertToDisplayText(_valueProvider);
		}
		#endregion //ConvertToDisplayText

		#region GetCell
		/// <summary>
		/// The <see cref="Cell"/> at the specified index.
		/// </summary>
		/// <param name="row">The 0 based index of the row whose cell is to be returned</param>
		/// <param name="column">The 0 based index of the column whose cell is to be returned</param>
		/// <returns>The <see cref="Cell"/> at the specified index or null if there is no cell in the specified position</returns>
		/// <exception cref="ArgumentOutOfRangeException">The row/column must be at least 0 and less than the respective RowCount/ColumnCount count values.</exception>
		internal Cell GetCell(int row, int column)
		{
			return this._valueProvider.GetCell(row, column);
		}

		#endregion //GetCell

		#region GetField

		/// <summary>
		/// Returns the <see cref="Field"/> associated with the specified column in the collection
		/// </summary>
		/// <param name="fieldIndex">The 0 based index of the column whose associated Field is to be returned.</param>
		/// <returns>The <see cref="Field"/> at the specified column index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The column must be at least 0 and less than the ColumnCount.</exception>
		internal Field GetField(int fieldIndex)
		{
			return this._valueProvider.GetField(fieldIndex);
		}

		#endregion //GetField

		#region GetRecord
		/// <summary>
		/// Returns the record associated with the specified row in the collection
		/// </summary>
		/// <param name="row">The 0 based index of the row whose associated record is to be returned.</param>
		/// <returns>The <see cref="DataRecord"/> at the specified row index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The row must be at least 0 and less than the RowCount.</exception>
		internal DataRecord GetRecord(int row)
		{
			return this._valueProvider.GetRecord(row);
		}

		#endregion //GetRecord

		#region IndexOf
		internal int IndexOf(Field field)
		{
			for (int i = 0, count = this.FieldCount; i < count; i++)
			{
				if (this.GetField(i) == field)
					return i;
			}

			return -1;
		}

		internal int IndexOf(DataRecord record)
		{
			for (int i = 0, count = this.RecordCount; i < count; i++)
			{
				if (this.GetRecord(i) == record)
					return i;
			}

			return -1;
		}

		#endregion //IndexOf

        #endregion //Internal Methods

        #endregion //Methods

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            _valueProvider.CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return _valueProvider.ValueCount; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return _valueProvider; }
        }

        #endregion // ICollection Members

        #region IEnumerable<CellValueHolder> Members

        IEnumerator<CellValueHolder> IEnumerable<CellValueHolder>.GetEnumerator()
        {
            return _valueProvider.GetValueEnumerator();
        }

        #endregion //IEnumerable<CellValueHolder> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _valueProvider.GetValueEnumerator();
        }

        #endregion //IEnumerable Members
	}

	internal class ClipboardCellValueHolderCollection : CellValueHolderCollection
	{
		#region Member Variables

		private IDictionary<Field, CellValueHolder> _labels;

		#endregion //Member Variables

        #region Constructor
		internal ClipboardCellValueHolderCollection(ICellValueProvider valueProvider, IDictionary<Field, CellValueHolder> labels) : base(valueProvider)
        {
			Debug.Assert(null == labels || labels.Count == 0 || labels.Count == valueProvider.FieldCount);
			_labels = labels;
		}
        #endregion //Constructor

		#region Methods

		#region ConvertLabelsToText
		internal void ConvertLabelsToText()
		{
			if (null != _labels)
			{
				foreach (KeyValuePair<Field, CellValueHolder> item in _labels)
				{
					CellValueHolder cvh = item.Value;

					if (!cvh.IsDisplayText)
					{
						Field f = item.Key;

						CultureInfo ci = f.DataPresenter.DefaultConverterCulture;
						string text = string.Empty;

						if (null != cvh.Value)
						{
							text = Utilities.ConvertDataValue(cvh.Value, typeof(string), ci, null) as string;
						}

						cvh.Initialize(text, true);
					}
				}
			}
		}
		#endregion //ConvertLabelsToText

		#region GetLabel
		internal CellValueHolder GetLabel(Field field)
		{
			CellValueHolder cvh;

			if (null == _labels || !_labels.TryGetValue(field, out cvh))
				return null;

			return cvh;
		}

		internal CellValueHolder GetLabel(int fieldIndex)
		{
			Field f = GetField(fieldIndex);
			return GetLabel(f);
		} 
		#endregion //GetLabel

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