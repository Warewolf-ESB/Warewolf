using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Custom class used to store cell values in the clipboard
	/// </summary>
    [Serializable]
    internal class ClipboardData
    {
		#region Member Variables

		private object[] _cellValues;
		private string[] _displayTexts;
		private int _columnCount;
		private bool[] _hasOriginalValues;

		#endregion //Member Variables

		#region Constructor
		private ClipboardData()
		{
		} 
		#endregion //Constructor

        #region Methods

        #region Internal Methods

        #region Create
        internal static object Create(CellValueHolderCollection cellValues)
        {
			int count = cellValues.RecordCount * cellValues.FieldCount;
			object[] values = new object[count];
			string[] texts = new string[count];
			bool[] hasOriginalValues = new bool[count];
			int index = 0;
			int rCount = cellValues.RecordCount;
			int cCount = cellValues.FieldCount;

			for (int r = 0; r < rCount; r++)
			{
				DataRecord record = cellValues.GetRecord(r);

				for (int c = 0; c < cCount; c++)
				{
					Field field = cellValues.GetField(c);

					CellTextConverterInfo converter = CellTextConverterInfo.GetCachedConverter( field );

					object cellValue = record.GetCellValue(field, true);

					// don't try to save the original value if its not serializable
					if (GridUtilities.IsSerializable(cellValue))
					{
						values[index] = cellValue;
						// AS 8/25/09 TFS21354
						// We were only updating the first record's flag indicating we had original values.
						//
						//hasOriginalValues[c] = true;
						hasOriginalValues[index] = true;
					}

					texts[index] = converter.ConvertCellValue(cellValue);

					index++;
				}
			}

			ClipboardData cd = new ClipboardData();
			cd._cellValues = values;
			cd._displayTexts = texts;
			cd._columnCount = cellValues.FieldCount;
			cd._hasOriginalValues = hasOriginalValues;
			return cd;
        }
        #endregion //Create

		#region GetCellValues
		internal CellValueHolder[,] GetCellValues()
		{
			CellValueHolder[,] values = null;

			if (null != _cellValues && _columnCount > 0)
			{
				Debug.Assert(_cellValues.Length % _columnCount == 0);
				Debug.Assert(_displayTexts.Length == _cellValues.Length);
				Debug.Assert(_hasOriginalValues.Length == _hasOriginalValues.Length);

				int rowCount = _cellValues.Length / _columnCount;

				values = new CellValueHolder[rowCount, _columnCount];

				for (int i = 0; i < _cellValues.Length; i++)
				{
					int r = i / _columnCount;
					int c = i % _columnCount;

					CellValueHolder cvh;

					if (_hasOriginalValues[i])
						cvh = new ClipboardCellValueHolder(_displayTexts[i], true, _cellValues[i], _displayTexts[i]);
					else
						cvh = new CellValueHolder(_displayTexts[i], true);

					values[r, c] = cvh;
				}
			}

			return values;
		} 
		#endregion //GetCellValues

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