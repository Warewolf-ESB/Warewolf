using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class CellElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Cell"> 
        //  <sequence> 
        //      <element name="f" type="CT_CellFormula" minOccurs="0" maxOccurs="1"/> 
        //      <element name="v" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="is" type="CT_Rst" minOccurs="0" maxOccurs="1"/> 
        //      <element name="extLst" minOccurs="0" type="CT_ExtensionList"/> 
        //  </sequence> 
        //  <attribute name="r" type="ST_CellRef" use="optional"/> 
        //  <attribute name="s" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="t" type="ST_CellType" use="optional" default="n"/> 
        //  <attribute name="cm" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="vm" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="ph" type="xsd:boolean" use="optional" default="false"/> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "c";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            CellElement.LocalName;

        private const string RAttributeName = "r";
        private const string SAttributeName = "s";
        private const string TAttributeName = "t";
        private const string CmAttributeName = "cm";
        private const string VmAttributeName = "vm";
        private const string PhAttributeName = "ph";

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.c; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            WorksheetRow row = manager.ContextStack[typeof(WorksheetRow)] as WorksheetRow;
            if (row == null)
            {
                Utilities.DebugFail("Could not get the row off of the context stack");
                return;
            }

            short columnIndex = -1;
            int rowIndex = -1;
            ST_CellType cellType = ST_CellType.n;
            int styleIndex = -1;
            
            // Roundtrip - Variables for load
            //uint cellMetadata = 0;
            //uint valueMetadata = 0;
            //bool phonetic = false;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case CellElement.RAttributeName:
                        string cellRef = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, String.Empty);
						// MD 4/6/12 - TFS101506
                        //if (Utilities.ParseA1CellAddress(cellRef, WorkbookFormat.Excel2007, out columnIndex, out rowIndex) == false)
						if (Utilities.ParseA1CellAddress(cellRef, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out columnIndex, out rowIndex) == false)
                            Utilities.DebugFail("Could not parse address");                        

                        break;

                    case CellElement.SAttributeName:
                        styleIndex = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Integer, 0);
                        break;

                    case CellElement.TAttributeName:
                        cellType = (ST_CellType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellType, ST_CellType.n);
                        break;

                    case CellElement.CmAttributeName:
                        // Roundtrip - Page 1929
                        //cellMetadata = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);
                        break;

                    case CellElement.VmAttributeName:
                        // Roundtrip - Page 1930
                        //valueMetadata = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);
                        break;

                    case CellElement.PhAttributeName:
                        // Roundtrip - Page 1929
                        //phonetic = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;
                }
            }

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			#region Old Code

			//WorksheetCell cell = null;
			//WorksheetCellCollection cells = row.Cells;
			//
			//// Create the cell in the cells collection
			//if (columnIndex > -1)
			//{
			//    cell = cells[columnIndex];
			//}
			//else
			//{
			//    // This situation shouldn't occur, but in case we've somehow loaded a cell into an index where
			//    // we've already loaded a cell, we should raise a notification, but only once.  The while loop
			//    // will ensure that we put the data into a new cell, though we will end up mangling the data in terms
			//    // of order, which is better than losing it completely.  
			//    //
			//    // Note: the LastLoadedIndex property starts at -1, so we always want to start off preincrementing it
			//    // MD 10/20/10 - TFS36617
			//    // In the normal case, we were doing a full lookup in the collection twice: once to see if the cell was 
			//    // already created, which it shouldn't have been, and a second time to actually insert and get the item. 
			//    // I added a new method to get the item and determine if it was already in the collection so we can just 
			//    // do it once.
			//    //bool hasShownAssert = false;
			//    //while (cells.GetIfCreated(++cells.LastLoadedIndex) != null)
			//    //{
			//    //    if (hasShownAssert == false)
			//    //    {
			//    //        hasShownAssert = true;
			//    //        Utilities.DebugFail(String.Format("A cell has already been created at the specified index: {0}", cells.LastLoadedIndex));
			//    //    }
			//    //}
			//    //cell = cells[cells.LastLoadedIndex];
			//    bool hasShownAssert = false;
			//    while (true)
			//    {
			//        bool wasCreated;
			//        cell = cells.GetItem(++cells.LastLoadedIndex, out wasCreated);
			//
			//        if (wasCreated)
			//            break;
			//
			//        if (hasShownAssert == false)
			//        {
			//            hasShownAssert = true;
			//            Utilities.DebugFail(String.Format("A cell has already been created at the specified index: {0}", cells.LastLoadedIndex));
			//        }
			//    }
			//} 

			#endregion  // Old Code
			if (columnIndex < 0)
				columnIndex = (short)++row.Cells.LastLoadedIndex;

            if (styleIndex > -1)
            {
                if (styleIndex < manager.CellXfs.Count)
				{
					// MD 10/27/10 - TFS56976
					// Getting the CellFormat and then setting the formatting creates the format proxy with the default format and then 
					// changes it to hold the new format. This is unnecessary and slow, so create the proxy with the correct format
					// with the new SetCellFormat method.
                    //cell.CellFormat.SetFormatting(manager.CellXfs[styleIndex].FormatDataObject);
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//cell.SetCellFormatWhileLoading(manager.CellXfs[styleIndex].FormatDataObject);
					row.SetCellFormatWhileLoading(columnIndex, manager.CellXfs[styleIndex].FormatDataObject);
				}
                else
                    Utilities.DebugFail("We have a style index that is greater than the number of CellXfs on the manager");
            }

            // Push the cell onto the context stack so that the child elements can access it
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
            //manager.ContextStack.Push(cell);
			manager.ContextStack.Push((ColumnIndex)columnIndex);

            // Push the cell type onto the context stack so that the child value element can parse it
            manager.ContextStack.Push(cellType);
        }

        #endregion Load

        #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 4/28/11
			Utilities.DebugFail("We should have used the SaveDirect overload instead.");

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Since we should never get in here, I have commented out the code.
			#region Removed

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////ListContext<WorksheetCell> worksheetCells = manager.ContextStack[typeof(ListContext<WorksheetCell>)] as ListContext<WorksheetCell>;
			////if (worksheetCells == null)
			////{
			////    Utilities.DebugFail("Could not get the worksheet cells from the context stack.");
			////    return;
			////}
			////
			////WorksheetCell cell = worksheetCells.ConsumeCurrentItem() as WorksheetCell;
			////if (cell == null)
			////{
			////    Utilities.DebugFail("Could not get the worksheet cell");
			////    return;
			////}
			////
			////// Push the cell onto the context stack for the child elements
			////manager.ContextStack.Push(cell);
			//ListContext<ColumnIndex> columnIndexes = (ListContext<ColumnIndex>)manager.ContextStack[typeof(ListContext<ColumnIndex>)];
			//WorksheetRow row = (WorksheetRow)manager.ContextStack[typeof(WorksheetRow)];

			//// MD 4/18/11 - TFS62026
			//// We also need to row cache to save the cell.
			////if (columnIndexes == null || row == null)
			//WorksheetRowSerializationCache rowCache = (WorksheetRowSerializationCache)manager.ContextStack[typeof(WorksheetRowSerializationCache)];
			//if (columnIndexes == null || row == null || rowCache == null)
			//{
			//    Utilities.DebugFail("Could not get the column indexes or row from the context stack.");
			//    return;
			//}

			//object columnIndexValue = columnIndexes.ConsumeCurrentItem();
			//if (columnIndexValue == null)
			//{
			//    Utilities.DebugFail("Could not get the column index");
			//    return;
			//}

			//// Push the columnIndex onto the context stack for the child elements
			//manager.ContextStack.Push(columnIndexValue);
			//short columnIndex = (short)(ColumnIndex)columnIndexValue;

			//// MD 11/4/10 - TFS49093
			//// Most of this code has been moved to the GetSaveValues method
			//#region Refactored

			////object cellValue = WorkbookSerializationManager.GetSerializableCellValue(cell);
			////
			////// MD 7/2/09 - TFS18634
			////// Cache the formuals values because they are needed later
			////Formula formula = cellValue as Formula;
			////WorksheetDataTable associatedDataTable = cellValue as WorksheetDataTable;
			////
			////// MD 7/2/09 - TFS18634
			////// There is no need to call GetSerializableCellValue with the True parameter if the cell value is not a 
			////// formula value, becasue we will just get the same value back. 
			//////object nonFormulaCellValue = WorkbookSerializationManager.GetSerializableCellValue( cell, true );
			////object nonFormulaCellValue;
			////if ( formula != null || associatedDataTable != null )
			////    nonFormulaCellValue = WorkbookSerializationManager.GetSerializableCellValue( cell, true );
			////else
			////    nonFormulaCellValue = cellValue;
			////
			////string attributeValue = null;
			////
			////// Add the 'r' attribute. Though this is optional, since we can figure it out, we might
			////// as well write it out
			////attributeValue = CellAddress.GetCellReferenceString(cell.RowIndex, cell.ColumnIndex, true, true, manager.Workbook.CurrentFormat, cell, false, CellReferenceMode.A1);
			////XmlElementBase.AddAttribute(element, CellElement.RAttributeName, attributeValue);
			////
			////// MD 3/16/09 - TFS14252
			////// We should always get in this block and asking for the CellFormatInternal forces it to get lazily created.
			//////if (cell.CellFormatInternal != null)
			////{
			////    // MD 12/5/08 - TFS11227
			////    // The resolved format must be used because any default format values on the cell will be picked up from the
			////    // owing row or column. This resolved format has already been determined in the cell's InitSerializationCache,
			////    // so just use the already resolved format.
			////    //int styleIndex = -1;
			////    //// 08/28/08 CDS - Call AddFormat() instead of PopulateManagerLists(). This makes sure the font and format indexes are set.
			////    ////manager.PopulateManagerLists(cell.CellFormatInternal.Element, ref styleIndex);
			////    //manager.AddFormat(cell.CellFormatInternal.Element, true);
			////    //styleIndex = cell.CellFormatInternal.Element.IndexInXfsCollection;
			////    // MD 7/26/10 - TFS34398
			////    // Now the resolved formats are stored on the manager instead of the cell.
			////    //int styleIndex = cell.ResolvedFormat.IndexInXfsCollection;
			////    int styleIndex = manager.ResolvedCellFormatsByCell[cell].IndexInXfsCollection;
			////
			////    // MD 1/28/09 - TFS12701
			////    // If the value is 0, it is considered default and shouldn't be written out.
			////    //attributeValue = XmlElementBase.GetXmlString(styleIndex, DataType.Integer);
			////    attributeValue = XmlElementBase.GetXmlString( styleIndex, DataType.Integer, 0, false );
			////
			////    if (attributeValue != null)
			////        XmlElementBase.AddAttribute(element, CellElement.SAttributeName, attributeValue);                
			////}
			////// Add the 't' attribute            
			////ST_CellType cellType = ST_CellType.n;
			////if ( nonFormulaCellValue != null )
			////{
			////    if ( nonFormulaCellValue is bool )
			////        cellType = ST_CellType.b;
			////    // MD 11/3/10 - TFS49093
			////    // The formatted string data is now stored on the FormattedStringElement. Also, StringBuilders need to be handled manually.
			////    //else if ( nonFormulaCellValue is FormattedString )
			////    else if (nonFormulaCellValue is FormattedStringElement || nonFormulaCellValue is StringBuilder)
			////        cellType = ST_CellType.s;
			////    else if ( nonFormulaCellValue is ErrorValue )
			////        cellType = ST_CellType.e;
			////    else if ( Utilities.IsNumericType( nonFormulaCellValue.GetType() ) == false )
			////        cellType = ST_CellType.str;
			////}
			////attributeValue = XmlElementBase.GetXmlString(cellType, DataType.ST_CellType, ST_CellType.n, false);
			////if (attributeValue != null)
			////    XmlElementBase.AddAttribute(element, CellElement.TAttributeName, attributeValue);
			////
			////// Roundtrip - Write out the 'cm', 'vm', and 'ph' attributes
			////
			////// Add the 'f' element
			////// MD 7/2/09 - TFS18634
			////// The Formula and AssociatedDataTable getters are relatively expensive, so use the cached versions from above.
			//////if ((cell.Formula != null && cell.Formula.OwningCell == cell) ||
			//////     (cell.AssociatedDataTable != null && cell.AssociatedDataTable.InteriorCells.TopLeftCell == cell))
			////if ( ( formula != null && formula.OwningCell == cell ) ||
			////     ( associatedDataTable != null && associatedDataTable.InteriorCells.TopLeftCell == cell ) )
			////{
			////    // MD 11/1/10 - TFS56976
			////    // We usually add a lot of formula elements, so let's not parse through the fully qualified name each time. 
			////    // We know what the local name and prefix are always going to be anyway.
			////    //XmlElementBase.AddElement(element, FormulaElement.QualifiedName);
			////    XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, FormulaElement.LocalName, string.Empty);
			////}
			////
			////// There is no reason to serialize the CellValueElement or the type without a value
			////if ( nonFormulaCellValue != null )
			////{
			////    // Add the cell so that the child element can get the value
			////    if (cellType == ST_CellType.s)
			////        manager.ContextStack.Push( nonFormulaCellValue );
			////    // MD 5/27/09 - TFS17956
			////    // The numeric values must be written with the invariant culture so the cell values do not 
			////    // change based on the machine on which the file is opened.
			////    else if ( cellType == ST_CellType.n )
			////    {
			////        try
			////        {
			////            double doubleValue = Convert.ToDouble( nonFormulaCellValue );
			////            manager.ContextStack.Push( doubleValue.ToString( Workbook.InvariantFormatProvider ) );
			////        }
			////        catch ( InvalidCastException )
			////        {
			////            Debug.Fail( "The numeric type should convert to a double." );
			////            manager.ContextStack.Push( nonFormulaCellValue.ToString() );
			////        }
			////    }
			////    // MD 6/30/09 - TFS18957
			////    // This was a pretty big oversight. The boolean types have to be serialized out as an integer and not 
			////    // as a 'true' or 'false' string
			////    else if ( cellType == ST_CellType.b )
			////    {
			////        int numericValue = Convert.ToInt32( (bool)nonFormulaCellValue );
			////        manager.ContextStack.Push( numericValue.ToString() );
			////    }
			////    else
			////        manager.ContextStack.Push( nonFormulaCellValue.ToString() );
			////
			////    // Add the 'v' child element
			////    // MD 11/1/10 - TFS56976
			////    // We usually add a lot of cell value elements, so let's not parse through the fully qualified name each time. 
			////    // We know what the local name and prefix are always going to be anyway.
			////    //XmlElementBase.AddElement(element, CellValueElement.QualifiedName);
			////    XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, CellValueElement.LocalName, string.Empty);
			////} 

			//#endregion // Refactored
			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////string rowReferenceString = (cell.RowIndex + 1).ToString();
			//string rowReferenceString = (row.Index + 1).ToString();

			//Formula formula;
			//WorksheetDataTable associatedDataTable;
			//string rAttributeValue;
			//string sAttributeValue;
			//string tAttributeValue;
			//object valueToWrite;
			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////CellElement.GetSaveValues(manager, cell, rowReferenceString,
			////    out formula, out associatedDataTable, out rAttributeValue, out sAttributeValue, out tAttributeValue, out valueToWrite);
			//// MD 4/18/11 - TFS62026
			//// Pass along the row cache.
			////CellElement.GetSaveValues(manager, row, columnIndex, rowReferenceString,
			//CellElement.GetSaveValues(manager, row, columnIndex, rowCache, rowReferenceString,
			//    out formula, out associatedDataTable, out rAttributeValue, out sAttributeValue, out tAttributeValue, out valueToWrite);

			//// Add the 'r' attribute. Though this is optional, since we can figure it out, we might
			//// as well write it out
			//XmlElementBase.AddAttribute(element, CellElement.RAttributeName, rAttributeValue);

			//if (sAttributeValue != null)
			//    XmlElementBase.AddAttribute(element, CellElement.SAttributeName, sAttributeValue);

			//// Add the 't' attribute            
			//if (tAttributeValue != null)
			//    XmlElementBase.AddAttribute(element, CellElement.TAttributeName, tAttributeValue);

			//// Roundtrip - Write out the 'cm', 'vm', and 'ph' attributes

			//// Add the 'f' element
			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////if ((formula != null && formula.OwningCell == cell) ||
			////     (associatedDataTable != null && associatedDataTable.InteriorCells.TopLeftCell == cell))
			//if ((formula != null && formula.OwningCellRow == row && formula.OwningCellColumnIndex == columnIndex) ||
			//     (associatedDataTable != null && associatedDataTable.InteriorCells.FirstRow == row.Index && associatedDataTable.InteriorCells.FirstColumnInternal == columnIndex))
			//{
			//    XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, FormulaElement.LocalName, string.Empty);
			//}

			//// There is no reason to serialize the CellValueElement or the type without a value
			//if (valueToWrite != null)
			//{
			//    manager.ContextStack.Push(valueToWrite);
			//    XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, CellValueElement.LocalName, string.Empty);
			//}

			#endregion // Removed
        }
        #endregion Save

        #endregion Base class overrides

		#region Methods

		// MD 11/4/10 - TFS49093
		#region GetSaveValues

		private static void GetSaveValues(
			Excel2007WorkbookSerializationManager manager,
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell,
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Pass in the cell data context instead of just the column index, because we need more information now.
			//WorksheetRow row, short columnIndex,
			WorksheetRow row, CellDataContext cellDataContext,
			WorksheetRowSerializationCache rowCache,	// MD 4/18/11 - TFS62026
			string rowReferenceString,
			out Formula formula,
			out WorksheetDataTable associatedDataTable,
			out string rAttributeValue,
			out string sAttributeValue,
			out string tAttributeValue,
			// MD 7/10/12 - TFS116306
			//out object valueToWrite)
			out string valueToWrite)
		{
			// MD 1/10/12 - 12.1 - Cell Format Updates
			short columnIndex = cellDataContext.ColumnIndex;

			// MD 4/12/11 - TFS67084
			WorksheetCellBlock cellBlock;
			row.TryGetCellBlock(columnIndex, out cellBlock);

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//object cellValue = WorkbookSerializationManager.GetSerializableCellValue(cell);
			// MD 2/1/12 - TFS100573
			// This is now an instance method.
			//object cellValue = WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, cellBlock);
			object cellValue = manager.GetSerializableCellValue(row, columnIndex, cellBlock);

			formula = cellValue as Formula;
			associatedDataTable = cellValue as WorksheetDataTable;

			// There is no need to call GetSerializableCellValue with the True parameter if the cell value is not a 
			// formula value, because we will just get the same value back. 
			object nonFormulaCellValue;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if (cell.UsesCalculations)
			//    nonFormulaCellValue = WorkbookSerializationManager.GetSerializableCellValue(cell, true);
			if (cellBlock != null && cellBlock.DoesCellUseCalculations(columnIndex))
			{
				// MD 2/1/12 - TFS100573
				// This is now an instance method.
				//nonFormulaCellValue = WorkbookSerializationManager.GetSerializableCellValue(row, columnIndex, cellBlock, true);
				nonFormulaCellValue = manager.GetSerializableCellValue(row, columnIndex, cellBlock, true);
			}
			else
			{
				nonFormulaCellValue = cellValue;
			}

			// Add the 'r' attribute. Though this is optional, since we can figure it out, we might
			// as well write it out
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//rAttributeValue = CellAddress.GetColumnString(cell.ColumnIndex, true, manager.Workbook.CurrentFormat, cell, false, CellReferenceMode.A1) + rowReferenceString;
			//
			//int styleIndex = manager.ResolvedCellFormatsByCell[cell].IndexInXfsCollection;
			rAttributeValue = CellAddress.GetColumnString(columnIndex, true, manager.Workbook.CurrentFormat, columnIndex, false, CellReferenceMode.A1) + rowReferenceString;

			// MD 4/18/11 - TFS62026
			// The ResolvedCellFormatsByCell collection used too much memory.
			//int styleIndex = manager.ResolvedCellFormatsByCell[new WorksheetCellAddress(row, columnIndex)].IndexInXfsCollection;
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// We no longer cache format indexes because we can easily get them at save time.
			//int styleIndex = rowCache.cellFormatIndexValues[rowCache.nextCellFormatIndex++];
			int styleIndex = manager.GetCellFormatIndex(cellDataContext.CellFormatData);

			sAttributeValue = XmlElementBase.GetXmlString(styleIndex, DataType.Integer, 0, false);

			// MD 7/10/12 - TFS116306
			// Refactored this code into a helper method which could be called elsewhere.
			#region Refactored

			//// Add the 't' attribute            
			//ST_CellType cellType = ST_CellType.n;
			//if (nonFormulaCellValue != null)
			//{
			//    if (nonFormulaCellValue is bool)
			//        cellType = ST_CellType.b;
			//    // MD 2/1/12 - TFS100573
			//    // The save values for strings will now be their index instead of the StringElement itself.
			//    //else if (nonFormulaCellValue is StringElement || nonFormulaCellValue is StringBuilder)
			//    else if (nonFormulaCellValue is StringElementIndex)
			//        cellType = ST_CellType.s;
			//    else if (nonFormulaCellValue is ErrorValue)
			//        cellType = ST_CellType.e;
			//    // MD 1/19/11 - TFS37369
			//    // The IsNumericType method now takes a value rather than a type.
			//    //else if (Utilities.IsNumericType(nonFormulaCellValue.GetType()) == false)
			//    else if (Utilities.IsNumericType(nonFormulaCellValue) == false)
			//        cellType = ST_CellType.str;
			//}
			//tAttributeValue = XmlElementBase.GetXmlString(cellType, DataType.ST_CellType, ST_CellType.n, false);

			//// There is no reason to serialize the CellValueElement or the type without a value
			//// MD 11/22/11
			//// Found while writing Interop tests
			//// DBNull values should be saved out just like nulls.
			////if (nonFormulaCellValue != null)
			//bool valueIsNull = nonFormulaCellValue == null || nonFormulaCellValue is DBNull;
			//if (valueIsNull == false)
			//{
			//    // Add the cell so that the child element can get the value
			//    if (cellType == ST_CellType.s)
			//    {
			//        valueToWrite = nonFormulaCellValue;
			//    }
			//    else if (cellType == ST_CellType.n)
			//    {
			//        try
			//        {
			//            double doubleValue = Convert.ToDouble(nonFormulaCellValue);
			//            valueToWrite = doubleValue.ToString(Workbook.InvariantFormatProvider);
			//        }
			//        catch (InvalidCastException)
			//        {
			//            Utilities.DebugFail("The numeric type should convert to a double.");
			//            valueToWrite = nonFormulaCellValue.ToString();
			//        }
			//    }
			//    else if (cellType == ST_CellType.b)
			//    {
			//        int numericValue = Convert.ToInt32((bool)nonFormulaCellValue);
			//        valueToWrite = numericValue.ToString();
			//    }
			//    else
			//    {
			//        valueToWrite = nonFormulaCellValue.ToString();
			//    }
			//}
			//else
			//{
			//    valueToWrite = null;
			//}

			#endregion // Refactored
			ST_CellType cellType;
			Excel2007WorkbookSerializationManager.GetCellValueInfo(
				nonFormulaCellValue, 
				out cellType, 
				out valueToWrite);

			tAttributeValue = XmlElementBase.GetXmlString(cellType, DataType.ST_CellType, ST_CellType.n, false);
		}

		#endregion // GetSaveValues

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			Excel2007WorkbookSerializationManager manager,
			XmlWriter writer,
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell,
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Pass in the cell data context instead of just the column index, because we need more information now.
			//WorksheetRow row, short columnIndex,
			WorksheetRow row, CellDataContext cellDataContext,
			WorksheetRowSerializationCache rowCache,	// MD 4/18/11 - TFS62026
			string rowReferenceString)
		{
			writer.WriteStartElement(CellElement.LocalName);

			// MD 1/10/12 - 12.1 - Cell Format Updates
			short columnIndex = cellDataContext.ColumnIndex;

			Formula formula;
			WorksheetDataTable associatedDataTable;
			string rAttributeValue;
			string sAttributeValue;
			string tAttributeValue;

			// MD 7/10/12 - TFS116306
			//object valueToWrite;
			string valueToWrite;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//CellElement.GetSaveValues(manager, cell, rowReferenceString,
			//    out formula, out associatedDataTable, out rAttributeValue, out sAttributeValue, out tAttributeValue, out valueToWrite);
			// MD 4/18/11 - TFS62026
			// Pass along the row cache.
			//CellElement.GetSaveValues(manager, row, columnIndex, rowReferenceString,
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Pass in the cell data context instead of just the column index, because we need more information now.
			//CellElement.GetSaveValues(manager, row, columnIndex, rowCache, rowReferenceString,
			CellElement.GetSaveValues(manager, row, cellDataContext, rowCache, rowReferenceString,
				out formula, out associatedDataTable, out rAttributeValue, out sAttributeValue, out tAttributeValue, out valueToWrite);

			// Add the 'r' attribute. Though this is optional, since we can figure it out, we might
			// as well write it out
			writer.WriteAttributeString(CellElement.RAttributeName, rAttributeValue);

			if (sAttributeValue != null)
				writer.WriteAttributeString(CellElement.SAttributeName, sAttributeValue);

			// Add the 't' attribute            
			if (tAttributeValue != null)
				writer.WriteAttributeString(CellElement.TAttributeName, tAttributeValue);

			// Roundtrip - Write out the 'cm', 'vm', and 'ph' attributes

			// Add the 'f' element
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ((formula != null && formula.OwningCell == cell) ||
			//     (associatedDataTable != null && associatedDataTable.InteriorCells.TopLeftCell == cell))
			//{
			//    FormulaElement.SaveDirectHelper(manager, writer, cell);
			//}
			if ((formula != null && formula.OwningCellRow == row && formula.OwningCellColumnIndex == columnIndex) ||
				 (associatedDataTable != null && associatedDataTable.InteriorCells.FirstRow == row.Index && associatedDataTable.InteriorCells.FirstColumnInternal == columnIndex))
			{
				FormulaElement.SaveDirectHelper(manager, writer, row, columnIndex);
			}

			// There is no reason to serialize the CellValueElement or the type without a value
			if (valueToWrite != null)
				CellValueElement.SaveDirectHelper(manager, writer, valueToWrite);

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