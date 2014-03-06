using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class CellValueElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "v";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            CellValueElement.LocalName;

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.v; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
            //WorksheetCell cell = manager.ContextStack[typeof(WorksheetCell)] as WorksheetCell;
			WorksheetRow row = (WorksheetRow)manager.ContextStack[typeof(WorksheetRow)];
			object columnIndexValue = manager.ContextStack[typeof(ColumnIndex)];

			// MD 3/30/11 - TFS69969
			// Added support for loading this element when it is a descendant of ExternalLink.
			//if (cell == null)
			//{
			//    // TODO: Roundtrip and Formula Solving - Page 2487
			//    // This element is also used as a descendant of ExternalLink, so we will
			//    // need to deserialize whatever we put in the context stack from the
			//    // 'sheetDataSet' element to deserialize this.
			//    Debug.Assert(manager.ContextStack[typeof(ExternalWorkbookReference)] != null, "Could not get the cell off of the context stack");
			//    return;
			//}
			WorksheetReferenceExternal.RowValues rowValues = manager.ContextStack.Get<WorksheetReferenceExternal.RowValues>();
			
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if (cell == null && rowValues == null)
			if (columnIndexValue == null || (row == null && rowValues == null))
			{
				Utilities.DebugFail("Cannot get the data needed for the cell value element.");
				return;
			}

			// MD 4/12/11 - TFS67084
			// Cache the casted column index.
			short columnIndex = (short)(ColumnIndex)columnIndexValue;

            ST_CellType cellType = (ST_CellType)manager.ContextStack[typeof(ST_CellType)];
			object cellValue = null;
            switch (cellType)
            {
                case ST_CellType.b:
					// MD 6/30/09 - TFS18957
					// The boolean types have to be serialized out as an integer and not as a 'true' or 'false' string
					//cellValue = bool.Parse( value );
					int numericValue;
					if ( int.TryParse( value, out numericValue ) )
					{
						cellValue = Convert.ToBoolean( numericValue );
					}
					else
					{
						// As a fallback, try to parse the value as a boolean
						bool boolValue;
						if ( bool.TryParse( value, out boolValue ) )
							cellValue = boolValue;
						else
							Utilities.DebugFail( "Cannot convert the value [" + value + "] to a boolean" );
					}
                    break;

                case ST_CellType.n:
					
					// MD 6/3/11 - TFS77777
					// Some 3rd party exporters may write out no type on the <c> element (which defaults to n) but blank <v> tags,
					// in which case, the value of the cell should remain null.
					if (value == null)
						break;

					// MD 5/27/09 - TFS17956
					// The numeric values must be written with the invariant culture so the cell values do not 
					// change based on the machine on which the file is opened.
					//cellValue = double.Parse( value );
					cellValue = double.Parse( value, Workbook.InvariantFormatProvider );
                    break;

                case ST_CellType.e:
					//cellValue = FormulaParser.ParseError( value );
					cellValue = FormulaParser.ParseError(value, CultureInfo.InvariantCulture);
                    break;

                case ST_CellType.s:

					// MD 10/26/10 - TFS56976
					// Apparently, a string cell can have a null value if its value was cleared.
					if (value == null)
						break;

                    int sharedStringIndex;
                    if (int.TryParse(value, out sharedStringIndex))
                    {
                        if (sharedStringIndex >= manager.SharedStringTable.Count)
                        {
                            Utilities.DebugFail("An index was specified that is greater than the number of strings in the SharedStringTable");
                            return;
                        }

						// MD 11/3/10 - TFS49093
						// The formatted string data is now stored on the FormattedStringElement.
						// Also, when the string has no formatting, we should return it as a string.
						//FormattedString sharedString = manager.SharedStringTable[sharedStringIndex].Value;
						//cellValue = sharedString.Clone();
						// MD 1/31/12 - TFS100573
						//FormattedStringElement sharedString = manager.SharedStringTable[sharedStringIndex];
						//
						//if (sharedString.HasFormatting)
						//    cellValue = new FormattedString(sharedString);
						StringElement sharedString = manager.SharedStringTable[sharedStringIndex];
						FormattedStringElement formattedStringElement = sharedString as FormattedStringElement;
						if (formattedStringElement != null && formattedStringElement.HasFormatting)
						{
							cellValue = new FormattedString(manager.Workbook, formattedStringElement);
						}
						else
						{
							// MD 4/12/11 - TFS67084
							// The regular strings will now be set with the element, not the proxy.
							//cellValue = new FormattedStringProxy(sharedString, manager.Workbook.SharedStringTable);
							cellValue = sharedString;
						}
                    }
                    else
                        Utilities.DebugFail("Expected an index to the shared string table");

                    break;

                default:
                case ST_CellType.inlineStr:
                case ST_CellType.str:
					cellValue = value;
                    break;
            }

			// MD 3/30/11 - TFS69969
			// If this element is under an ExternalLink, cache the value on the RowValues instance.
			if (rowValues != null)
			{
				// MD 4/12/11 - TFS67084
				// We now always get the column index at the top.
				//short columnIndex = (short)manager.ContextStack[typeof(short)];

				rowValues.SetCachedValue(columnIndex, cellValue);
				return;
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( cell.Formula != null || cell.AssociatedDataTable != null )
			//    cell.CalcReference.Value = FormulaUtilities.CalcEngine.CalcUtilities.CreateExcelCalcValue( cellValue );
			//else
			//{
			//    // MD 9/23/09 - TFS19150
			//    // The Value setter may get the row again, which will be slow, so pass in the row we already have.
			//    //cell.InternalSetValue( cellValue, false );
			//    WorksheetRow row = (WorksheetRow)manager.ContextStack[ typeof( WorksheetRow ) ];
			//    cell.InternalSetValue( cellValue, row, false );
			//}
			WorksheetCellBlock cellBlock = row.GetCellBlock(columnIndex);
			object valueRaw = cellBlock.GetCellValueRaw(row, columnIndex);

			if (valueRaw is Formula || valueRaw is WorksheetDataTable)
				row.GetCellCalcReference(columnIndex).Value = FormulaUtilities.CalcEngine.CalcUtilities.CreateExcelCalcValue(cellValue);
			else
			{
				// MD 1/31/12 - TFS100573
				// The cell block may get replaced by this operation. If it does, replace our reference to it (even though we don't 
				// currently do anything with the cell block after this, it will prevent bugs from being introduced if code is added
				// later which does use it).
				//cellBlock.SetCellValueInternal(row, columnIndex, cellValue, false);
				WorksheetCellBlock replacementBlock;
				cellBlock.SetCellValueRaw(row, columnIndex, cellValue, false, out replacementBlock);
				cellBlock = replacementBlock ?? cellBlock;
			}
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 11/4/10 - TFS49093
			// Most of this code has been moved to the GetSaveValue method
			#region Refactored

			//// MD 11/3/10 - TFS49093
			//// StringBuilders must be processed manually because they can't go into the shared string table.
			//StringBuilder sb = manager.ContextStack[typeof(StringBuilder)] as StringBuilder;
			//if (sb != null)
			//{
			//    int sbIndex;
			//    if (manager.AdditionalStringsInStringTable.TryGetValue(sb, out sbIndex) == false)
			//        Utilities.DebugFail("The string value is not in the shared string table");

			//    value = sbIndex.ToString();
			//    return;
			//}

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////FormattedString cellValue = manager.ContextStack[typeof(FormattedString)] as FormattedString;
			//FormattedStringElement cellValue = manager.ContextStack[typeof(FormattedStringElement)] as FormattedStringElement;

			//if (cellValue != null)
			//{
			//    // MD 11/3/10 - TFS49093
			//    // The element now stores it's location in the table, so we don't need to do a search.
			//    // Save the index of the value that's stored in the shared string table
			//    //int index = manager.SharedStringTable.BinarySearch(new WorkbookSerializationManager.FormattedStringHolder(cellValue));
			//    //if (index >= 0)
			//    //    value = index.ToString();
			//    //else
			//    //    Utilities.DebugFail("The cell's value is not in the shared string table");
			//    value = cellValue.IndexInStringTable.ToString();
			//}
			//else
			//{
			//    string cellString = manager.ContextStack[typeof(string)] as string;
			//    if (cellString == null)
			//    {
			//        Utilities.DebugFail("We shouldn't be serializing this element without a value");
			//        return;
			//    }

			//    value = cellString;
			//} 

			#endregion // Refactored
			// MD 7/10/12 - TFS116306
			// Now the item on the context stack is the actual element value which needs to be written out.
			//value = CellValueElement.GetSaveValue(manager, manager.ContextStack.Current);
			value = manager.ContextStack.Get<string>();
        }
        #endregion Save

        #endregion Base class overrides

		#region Methods

		// MD 7/10/12 - TFS116306
		// This is no longer needed.
		#region Removed

		//// MD 11/4/10 - TFS49093
		//#region GetSaveValue

		//public static string GetSaveValue(Excel2007WorkbookSerializationManager manager, object cellValue)
		//{
		//    // MD 2/1/12 - TFS100573
		//    // The save values for strings will now be their index instead of the StringElement itself.
		//    //StringBuilder sb = cellValue as StringBuilder;
		//    //if (sb != null)
		//    //{
		//    //    int sbIndex;
		//    //    if (manager.AdditionalStringsInStringTable.TryGetValue(sb, out sbIndex) == false)
		//    //        Utilities.DebugFail("The string value is not in the shared string table");
		//    //
		//    //    return sbIndex.ToString();
		//    //}
		//    //
		//    //StringElement formattedStringElement = cellValue as StringElement;
		//    //
		//    //if (formattedStringElement != null)
		//    //    return formattedStringElement.IndexInStringTable.ToString();
		//    StringElementIndex stringIndex = cellValue as StringElementIndex;
		//    if (stringIndex != null)
		//        return stringIndex.IndexInSharedStringTable.ToString();

		//    string cellString = cellValue as string;
		//    if (cellString == null)
		//    {
		//        Utilities.DebugFail("We shouldn't be serializing this element without a value");
		//        return null;
		//    }

		//    return cellString;
		//}

		//#endregion // GetSaveValue

		#endregion // Removed

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			Excel2007WorkbookSerializationManager manager,
			XmlWriter writer,
			// MD 7/10/12 - TFS116306
			//object cellValue)
			string cellValue)
		{
			writer.WriteStartElement(CellValueElement.LocalName);

			// MD 7/10/12 - TFS116306
			// Now the item specified is the actual element value which needs to be written out.
			//XmlElementBase.WriteString(writer, CellValueElement.GetSaveValue(manager, cellValue));
			XmlElementBase.WriteString(writer, cellValue);

			writer.WriteEndElement();
		}

		#endregion // SaveDirectHelper 

		#endregion // Methods
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