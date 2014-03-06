using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class RowElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Row"> 
        //  <sequence> 
        //  <element name="c" type="CT_Cell" minOccurs="0" maxOccurs="unbounded"/> 
        //  <element name="extLst" minOccurs="0" type="CT_ExtensionList"/> 
        //  </sequence> 
        //  <attribute name="r" type="xsd:unsignedInt" use="optional"/> 
        //  <attribute name="spans" type="ST_CellSpans" use="optional"/> 
        //  <attribute name="s" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="customFormat" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="ht" type="xsd:double" use="optional"/> 
        //  <attribute name="hidden" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="customHeight" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="outlineLevel" type="xsd:unsignedByte" use="optional" default="0"/> 
        //  <attribute name="collapsed" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="thickTop" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="thickBot" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="ph" type="xsd:boolean" use="optional" default="false"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "row";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            RowElement.LocalName;

        private const string RAttributeName = "r";
        private const string SpansAttributeName = "spans";
        private const string SAttributeName = "s";
        private const string CustomFormatAttributeName = "customFormat";
        private const string HtAttributeName = "ht";
        private const string HiddenAttributeName = "hidden";
        private const string CustomHeightAttributeName = "customHeight";
        private const string OutlineLevelAttributeName = "outlineLevel";
        private const string CollapsedAttributeName = "collapsed";
        private const string ThickTopAttributeName = "thickTop";
        private const string ThickBotAttributeName = "ThickBot";
        private const string PhAttributeName = "ph";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.row; }
        }
        #endregion //Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 3/30/11 - TFS69969
			// Moved from below
			int rowIndex = -1;

			// MD 3/30/11 - TFS69969
			// Added suport for loading this element when it is a descendant of ExternalLink.
			WorksheetReferenceExternal worksheetReference = (WorksheetReferenceExternal)manager.ContextStack[typeof(WorksheetReferenceExternal)];
			if (worksheetReference != null)
			{
				ExcelXmlAttribute rowIndexAttribute = element.Attributes[RAttributeName];
				if (rowIndexAttribute == null)
				{
					Utilities.DebugFail("Cannot find the \"r\" attribute.");
					return;
				}

				rowIndex = (int)XmlElementBase.GetAttributeValue(rowIndexAttribute, DataType.Int32, 0) - 1;
				WorksheetReferenceExternal.RowValues rowValues = worksheetReference.GetRowValues(rowIndex);
				manager.ContextStack.Push(rowValues);
				return;
			}

            ChildDataItem item = manager.ContextStack[typeof(ChildDataItem)] as ChildDataItem;
            if (item == null)
            {
                Utilities.DebugFail("Could not get the ChildDataItem from the ContextStack");
                return;
            }

            Worksheet worksheet = item.Data as Worksheet;
            if (worksheet == null)
            {
				// MD 3/30/11 - TFS69969
				// This no longer applies.
				//// TODO: Roundtrip and Formula Solving - Page 2487
				//// This element is also used as a descendant of ExternalLink, so we will
				//// need to deserialize whatever we put in the context stack from the
				//// 'sheetDataSet' element to deserialize this.
				//Debug.Assert(manager.ContextStack[typeof(ExternalWorkbookReference)] != null, "Could not get the worksheet from the ContextStack");
                return;
            }

			// MD 3/30/11 - TFS69969
			// Moved above
			//int rowIndex = -1;

            bool customFormat = false;
            double rowHeight = -1;
            bool hidden = false;
            bool customHeight = false;
            byte outlineLevel = 0;
            int styleIndex = -1;

            // Roundtrip = Variables for loading attributes
            //bool thickTop = false;
            //bool thickBot = false;
            //bool ph = false;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case RowElement.RAttributeName:     
                        // Technically this attribute is specified as a uint, but our object model
                        // expects an int, so there is no reason to parse this attribute multiple
                        // times (i.e. once as a uint, then again as an int).
                        rowIndex = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Integer, 0) - 1;
                        Debug.Assert(rowIndex > -1, "Expected the row index to be 1-based");
                        break;

                    case RowElement.SpansAttributeName:
                        // Ignore this element since we will not use it and will be writing it out ourselves later
                        // as an optimization
                        break;

                    case RowElement.SAttributeName:
                        styleIndex = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
                        break;

                    case RowElement.CustomFormatAttributeName:
                        customFormat = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;

                    case RowElement.HtAttributeName:
                        rowHeight = (double)XmlElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;

                    case RowElement.HiddenAttributeName:
                        hidden = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;

                    case RowElement.CustomHeightAttributeName:
                        customHeight = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;

                    case RowElement.OutlineLevelAttributeName:
                        outlineLevel = (byte)XmlElementBase.GetAttributeValue(attribute, DataType.Byte, 0);
                        break;

                    case RowElement.CollapsedAttributeName:
                        // We can ignore this attribute since we're going to re-calculate it anyway in the Save method
                        break;

                    case RowElement.ThickTopAttributeName:
                        // Roundtrip - Page 2010
                        //thickTop = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;

                    case RowElement.ThickBotAttributeName:
                        // Roundtrip - Page 2010
                        //thickBot = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;

                    case RowElement.PhAttributeName:
                        // Roundtrip - Page 2009
                        // ph = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;
                }
            }

            WorksheetRow row = null;
            WorksheetRowCollection rows = worksheet.Rows;

            // Create the row in the rows collection
            if (rowIndex > -1)
            {                
                row = rows[rowIndex];
            }
            else
            {
                // This situation shouldn't occur, but in case we've somehow loaded a row into an index where
                // we've already loaded a row, we should raise a notification, but only once.  The while loop
                // will ensure that we put the data into a new row, though we will end up mangling the data in terms
                // of order, which is better than losing it completely.  
                //
                // Note: the LastLoadedIndex property starts at -1, so we always want to start off preincrementing it
                bool hasShownAssert = false;
                while (rows.GetIfCreated(++rows.LastLoadedIndex) != null)
                {
                    if (hasShownAssert == false)
                    {
                        hasShownAssert = true;
                        Utilities.DebugFail(String.Format("A row has already been created at the specified index: {0}", rows.LastLoadedIndex));                        
                    }
                }
                row = rows[rows.LastLoadedIndex];
            }

            // Only apply the row's formatting if this attribute has been specified
            if (customFormat && styleIndex > -1)
            {
                if (styleIndex < manager.CellXfs.Count)
                    row.CellFormat.SetFormatting(manager.CellXfs[styleIndex].FormatDataObject);
                else
                    Utilities.DebugFail("We have a row style index greater than the number of styles we've loaded");
            }            

            // Only try to set the row's height if customHeight was specified and we've
            // loaded a rowHeight attribute
            if (customHeight && rowHeight > -1)
                // The attribute gives us the height in points, so convert it to twips,
                // which is what our object model uses.
                //
                // Roundtrip - We are probably losing information here, so we may need to 
                // store the original value provided
                row.Height = (int)(rowHeight * 20);

            row.Hidden = hidden;                   
            row.OutlineLevel = outlineLevel;     
       
            // Put the row on the context stack so that the cells can access it
            manager.ContextStack.Push(row);
        }
        #endregion //Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 7/10/12 - TFS116306
			// Implemented the saving of external data.
			EnumerableContext<WorksheetReferenceExternal.RowValues> rowValuesContext = 
				manager.ContextStack.Get<EnumerableContext<WorksheetReferenceExternal.RowValues>>();
			if (rowValuesContext != null)
			{
				WorksheetReferenceExternal.RowValues rowValues = rowValuesContext.ConsumeCurrentItem();
				string attrbuteValue = XmlElementBase.GetXmlString(rowValues.RowIndex + 1, DataType.Int32);
				XmlElementBase.AddAttribute(element, RowElement.RAttributeName, attrbuteValue);

				manager.ContextStack.Push(rowValues);

				if (rowValues.CachedValueCount != 0)
				{
					manager.ContextStack.Push(new EnumerableContext<KeyValuePair<short, object>>(rowValues.GetCachedValues()));
					XmlElementBase.AddElements(element, ExternalCellElement.QualifiedName, rowValues.CachedValueCount);
				}
				return;
			}

			// MD 4/28/11
			Utilities.DebugFail("We should have used the SaveDirect overload instead.");

			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Since we should never get in here, I have commented out the code.
			#region Removed

			//ListContext<WorksheetRow> worksheetRows = manager.ContextStack[typeof(ListContext<WorksheetRow>)] as ListContext<WorksheetRow>;
			//if (worksheetRows == null)
			//{
			//    Utilities.DebugFail("Could not get the worksheet rows from the context stack.");
			//    return;
			//}

			//Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
			//if (worksheet == null)
			//{
			//    Utilities.DebugFail("Could not get the worksheet off of the context stack");
			//    return;
			//}

			//string attributeValue = String.Empty;
			//WorksheetRow row = worksheetRows.ConsumeCurrentItem() as WorksheetRow;
			//if (row != null)
			//{
			//    // MD 7/26/10 - TFS34398
			//    WorksheetRowSerializationCache rowCache = manager.RowSerializationCaches[row];

			//    // MD 11/4/10 - TFS49093
			//    // Most of this code has been moved to the GetSaveValues method
			//    #region Refactored

			//    //// Add the 'r' attribute.  Though this is optional, since we have the index we 
			//    //// might as well serialize it
			//    ////
			//    //// NOTE: Excel uses a 1-based index while we used 0-based
			//    //attributeValue = XmlElementBase.GetXmlString(row.Index + 1, DataType.UInt);
			//    //XmlElementBase.AddAttribute(element, RowElement.RAttributeName, attributeValue);

			//    //// Add the 'spans' attribute
			//    ////
			//    //// TODO: Actually write this value out, as it is used for an optimization, but is not required
			//    //// <attribute name="spans" type="ST_CellSpans" use="optional"/>                 
			//    ////XmlElementBase.AddAttribute(element, RowElement.SpansAttributeName, attributeValue);

			//    //// Add the 's' attribute and the 'customFormat' attribute
			//    //// MD 4/24/09
			//    //// Found while fixing TFS16204 
			//    //// HasCellFormat only returns if the proxy has been lazily created. It itgnores the data within the element, 
			//    //// so we must check to see if it has to be serialized.
			//    ////if (row.HasCellFormat)
			//    //if ( row.HasCellFormat && row.CellFormatInternal.HasDefaultValue == false )
			//    //{
			//    //    // 09/18/08 CDS - The Format collection has both formats and styles, so we can't use 
			//    //    // IndexInFormatCollection, use the IndexInXfsCollection instead
			//    //    //int index = row.CellFormatInternal.Element.IndexInFormatCollection;
			//    //    int index = row.CellFormatInternal.Element.IndexInXfsCollection;
			//    //    attributeValue = XmlElementBase.GetXmlString(index, DataType.UInt);
			//    //    XmlElementBase.AddAttribute(element, RowElement.SAttributeName, attributeValue);

			//    //    // Add the 'customFormat' attribute since we're also specifying a style
			//    //    attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean);
			//    //    XmlElementBase.AddAttribute(element, RowElement.CustomFormatAttributeName, attributeValue);
			//    //}

			//    //// Add the 'ht' attribute
			//    //bool hasCustomHeight = false;
			//    //if (row.Height > -1)
			//    //{
			//    //    attributeValue = XmlElementBase.GetXmlString(row.Height / 20d, DataType.Double, worksheet.DefaultRowHeightResolved / 20d, false);
			//    //    if (attributeValue != null)
			//    //    {
			//    //        XmlElementBase.AddAttribute(element, RowElement.HtAttributeName, attributeValue);

			//    //        // Since we're specifying the height, we need to also specify that it is a custom height
			//    //        hasCustomHeight = true;
			//    //    }
			//    //}

			//    //// Add the 'hidden' attribute
			//    //attributeValue = XmlElementBase.GetXmlString(row.Hidden, DataType.Boolean, false, false);
			//    //if(attributeValue != null)
			//    //    XmlElementBase.AddAttribute(element, RowElement.HiddenAttributeName, attributeValue);

			//    //// Add the 'customHeight' property
			//    //attributeValue = XmlElementBase.GetXmlString(hasCustomHeight, DataType.Boolean, false, false);
			//    //if(attributeValue != null)
			//    //    XmlElementBase.AddAttribute(element, RowElement.CustomHeightAttributeName, attributeValue);

			//    //// Add the 'outlineLevel' attribute
			//    //// MD 11/19/08 - TFS10637
			//    //// The OutlineLevel property is returned as an int and cannot be unboxed to a byte. It has to be casted first.
			//    ////attributeValue = XmlElementBase.GetXmlString(row.OutlineLevel, DataType.Byte, 0, false);
			//    //// MD 1/28/09 - TFS12701
			//    //// Now that the value is casted to a byte, the default value must be casted as well.
			//    ////attributeValue = XmlElementBase.GetXmlString( (byte)row.OutlineLevel, DataType.Byte, 0, false );
			//    //attributeValue = XmlElementBase.GetXmlString( (byte)row.OutlineLevel, DataType.Byte, (byte)0, false );

			//    //if (attributeValue != null)
			//    //    XmlElementBase.AddAttribute(element, RowElement.OutlineLevelAttributeName, attributeValue);

			//    //// Add the 'collapsed' attribute
			//    //// MD 7/26/10 - TFS34398
			//    ////attributeValue = XmlElementBase.GetXmlString(row.HasCollapseIndicator, DataType.Boolean, false, false);
			//    //attributeValue = XmlElementBase.GetXmlString(rowCache.hasCollapseIndicator, DataType.Boolean, false, false);
			//    //if(attributeValue != null)
			//    //    XmlElementBase.AddAttribute(element, RowElement.CollapsedAttributeName, attributeValue);

			//    //// Roundtrip
			//    //// Add the 'thickTop' attribute
			//    ////attributeValue = XmlElementBase.GetXmlString(thickTop, DataType.Boolean, false, false);
			//    ////if (attributeValue != null)
			//    ////    XmlElementBase.AddAttribute(element, RowElement.ThickTopAttributeName, attributeValue);

			//    //// Roundtrip
			//    //// Add the 'thickBot' attribute
			//    ////attributeValue = XmlElementBase.GetXmlString(thickBot, DataType.Boolean, false, false);
			//    ////if (attributeValue != null)
			//    ////    XmlElementBase.AddAttribute(element, RowElement.ThickBotAttributeName, attributeValue);

			//    //// Roundtrip
			//    //// Add the 'ph' attribute
			//    ////attributeValue = XmlElementBase.GetXmlString(ph, DataType.Boolean, false, false);
			//    ////if (attributeValue != null)
			//    ////    XmlElementBase.AddAttribute(element, RowElement.PhAttributeName, attributeValue); 

			//    #endregion // Refactored
			//    string rAttributeValue;
			//    string sAttributeValue;
			//    string customFormatAttributeValue;
			//    string htAttributeValue;
			//    string hiddenAttributeValue;
			//    string customHeightAttributeValue;
			//    string outlineLevelAttributeValue;
			//    string collapsedAttributeValue;
			//    RowElement.GetSaveValues(worksheet, row, rowCache,
			//        out  rAttributeValue, out  sAttributeValue, out  customFormatAttributeValue, out  htAttributeValue, out  hiddenAttributeValue,
			//        out  customHeightAttributeValue, out  outlineLevelAttributeValue, out  collapsedAttributeValue);

			//    // Add the 'r' attribute.  Though this is optional, since we have the index we 
			//    // might as well serialize it
			//    //
			//    // NOTE: Excel uses a 1-based index while we used 0-based
			//    XmlElementBase.AddAttribute(element, RowElement.RAttributeName, rAttributeValue);

			//    // Add the 'spans' attribute
			//    //
			//    // TODO: Actually write this value out, as it is used for an optimization, but is not required
			//    // <attribute name="spans" type="ST_CellSpans" use="optional"/>                 
			//    //XmlElementBase.AddAttribute(element, RowElement.SpansAttributeName, attributeValue);

			//    // Add the 's' attribute and the 'customFormat' attribute
			//    if (sAttributeValue != null)
			//    {
			//        XmlElementBase.AddAttribute(element, RowElement.SAttributeName, sAttributeValue);

			//        // Add the 'customFormat' attribute since we're also specifying a style
			//        XmlElementBase.AddAttribute(element, RowElement.CustomFormatAttributeName, customFormatAttributeValue);
			//    }

			//    // Add the 'ht' attribute
			//    if (htAttributeValue != null)
			//        XmlElementBase.AddAttribute(element, RowElement.HtAttributeName, htAttributeValue);

			//    // Add the 'hidden' attribute
			//    if (hiddenAttributeValue != null)
			//        XmlElementBase.AddAttribute(element, RowElement.HiddenAttributeName, hiddenAttributeValue);

			//    // Add the 'customHeight' property
			//    if (customHeightAttributeValue != null)
			//        XmlElementBase.AddAttribute(element, RowElement.CustomHeightAttributeName, customHeightAttributeValue);

			//    if (outlineLevelAttributeValue != null)
			//        XmlElementBase.AddAttribute(element, RowElement.OutlineLevelAttributeName, outlineLevelAttributeValue);

			//    // Add the 'collapsed' attribute
			//    if (collapsedAttributeValue != null)
			//        XmlElementBase.AddAttribute(element, RowElement.CollapsedAttributeName, collapsedAttributeValue);

			//    // MD 4/12/11 - TFS67084
			//    // Moved away from using WorksheetCell objects.
			//    #region Old Code

			//    //// Add the cell elements that have data
			//    //List<WorksheetCell> cells = new List<WorksheetCell>();
			//    //foreach (WorksheetCell cell in row.Cells)
			//    //{
			//    //    if (cell.HasData)
			//    //    {
			//    //        // Create a new child element that will serialize the data that we add to the list
			//    //        // MD 11/1/10 - TFS56976
			//    //        // We usually add a lot of cell elements, so let's not parse through the fully qualified name each time. 
			//    //        // We know what the local name and prefix are always going to be anyway.
			//    //        //XmlElementBase.AddElement(element, CellElement.QualifiedName);
			//    //        XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, CellElement.LocalName, string.Empty);
			//    //        cells.Add(cell);
			//    //    }
			//    //}
			//    //
			//    //// Push the rows that have data onto the context stack so that each child row element
			//    //// can access the data.
			//    //ListContext<WorksheetCell> cellContext = new ListContext<WorksheetCell>(cells);
			//    //manager.ContextStack.Push(cellContext); 

			//    #endregion  // Old Code
			//    // Add the cell elements that have data
			//    List<ColumnIndex> columnIndexes = new List<ColumnIndex>();
			//    foreach (CellDataContext cellDataContext in row.GetCellsWithData())
			//    {
			//        XmlElementBase.AddElement(element, XLSXElementBase.DefaultXmlNamespace, CellElement.LocalName, string.Empty);
			//        columnIndexes.Add(cellDataContext.ColumnIndex);
			//    }

			//    manager.ContextStack.Push(row);

			//    // MD 4/18/11 - TFS62026
			//    // The cells need to row cache to get their format index values.
			//    manager.ContextStack.Push(rowCache);

			//    // Push the column indexes that have data onto the context stack so that each child cell element
			//    // can access the data.
			//    ListContext<ColumnIndex> cellContext = new ListContext<ColumnIndex>(columnIndexes);
			//    manager.ContextStack.Push(cellContext);
			//}
			//else
			//    Utilities.DebugFail("An item in the list context was not of the expected type");

			#endregion // Removed
        }
        #endregion //Save         

        #endregion //Base Class Overrides

		#region Methods

		// MD 11/4/10 - TFS49093
		#region GetSaveValues

		private static void GetSaveValues(
			WorkbookSerializationManager manager,		// MD 1/10/12 - 12.1 - Cell Format Updates
			Worksheet worksheet,
			WorksheetRow row,
			WorksheetRowSerializationCache rowCache,
			out string rAttributeValue,
			out string sAttributeValue,
			out string customFormatAttributeValue,
			out string htAttributeValue,
			out string hiddenAttributeValue,
			out string customHeightAttributeValue,
			out string outlineLevelAttributeValue,
			out string collapsedAttributeValue)
		{
			// Add the 'r' attribute.  Though this is optional, since we have the index we 
			// might as well serialize it
			//
			// NOTE: Excel uses a 1-based index while we used 0-based
			rAttributeValue = XmlElementBase.GetXmlString(row.Index + 1, DataType.UInt);

			// Add the 's' attribute and the 'customFormat' attribute
			sAttributeValue = null;
			customFormatAttributeValue = null;

			// MD 3/2/12 - 12.1 - Table Support
			//if (row.HasCellFormat && row.CellFormatInternal.HasDefaultValue == false)
			if (row.HasCellFormat && row.CellFormatInternal.IsEmpty == false)
			{
				// MD 1/10/12 - 12.1 - Cell Format Updates
				// We no longer cache format indexes because we can easily get them at save time.
				//int index = row.CellFormatInternal.Element.IndexInXfsCollection;
				int index = manager.GetCellFormatIndex(row.CellFormatInternal.Element);

				sAttributeValue = XmlElementBase.GetXmlString(index, DataType.UInt);

				// Add the 'customFormat' attribute since we're also specifying a style
				customFormatAttributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean);
			}

			// Add the 'ht' attribute
			htAttributeValue = null;
			bool hasCustomHeight = false;
			if (row.Height > -1)
			{
				htAttributeValue = XmlElementBase.GetXmlString(row.Height / 20d, DataType.Double, worksheet.DefaultRowHeightResolved / 20d, false);
				if (htAttributeValue != null)
				{
					// Since we're specifying the height, we need to also specify that it is a custom height
					hasCustomHeight = true;
				}
			}

			// Add the 'hidden' attribute
			hiddenAttributeValue = XmlElementBase.GetXmlString(row.Hidden, DataType.Boolean, false, false);

			// Add the 'customHeight' property
			customHeightAttributeValue = XmlElementBase.GetXmlString(hasCustomHeight, DataType.Boolean, false, false);

			outlineLevelAttributeValue = XmlElementBase.GetXmlString((byte)row.OutlineLevel, DataType.Byte, (byte)0, false);

			// Add the 'collapsed' attribute
			collapsedAttributeValue = XmlElementBase.GetXmlString(rowCache.hasCollapseIndicator, DataType.Boolean, false, false);
		}

		#endregion // GetSaveValues

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			Excel2007WorkbookSerializationManager manager,
			XmlWriter writer,
			Worksheet worksheet,
			WorksheetRow row,
			WorksheetRowSerializationCache rowCache)
		{
			writer.WriteStartElement(RowElement.LocalName);

			string rAttributeValue;
			string sAttributeValue;
			string customFormatAttributeValue;
			string htAttributeValue;
			string hiddenAttributeValue;
			string customHeightAttributeValue;
			string outlineLevelAttributeValue;
			string collapsedAttributeValue;
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Pass along the manager.
			//RowElement.GetSaveValues(worksheet, row, rowCache,
			RowElement.GetSaveValues(manager, worksheet, row, rowCache,
				out  rAttributeValue, out  sAttributeValue, out  customFormatAttributeValue, out  htAttributeValue, out  hiddenAttributeValue,
				out  customHeightAttributeValue, out  outlineLevelAttributeValue, out  collapsedAttributeValue);

			// Add the 'r' attribute.  Though this is optional, since we have the index we 
			// might as well serialize it
			writer.WriteAttributeString(RowElement.RAttributeName, rAttributeValue);

			// Add the 's' attribute and the 'customFormat' attribute
			if (sAttributeValue != null)
			{
				writer.WriteAttributeString(RowElement.SAttributeName, sAttributeValue);

				// Add the 'customFormat' attribute since we're also specifying a style
				writer.WriteAttributeString(RowElement.CustomFormatAttributeName, customFormatAttributeValue);
			}

			if (htAttributeValue != null)
				writer.WriteAttributeString(RowElement.HtAttributeName, htAttributeValue);

			// Add the 'hidden' attribute
			if (hiddenAttributeValue != null)
				writer.WriteAttributeString(RowElement.HiddenAttributeName, hiddenAttributeValue);

			// Add the 'customHeight' property
			if (customHeightAttributeValue != null)
				writer.WriteAttributeString(RowElement.CustomHeightAttributeName, customHeightAttributeValue);

			if (outlineLevelAttributeValue != null)
				writer.WriteAttributeString(RowElement.OutlineLevelAttributeName, outlineLevelAttributeValue);

			// Add the 'collapsed' attribute
			if (collapsedAttributeValue != null)
				writer.WriteAttributeString(RowElement.CollapsedAttributeName, collapsedAttributeValue);

			// MD 1/10/12 - 12.1 - Cell Format Updates
			WorksheetCellFormatData rowFormat = row.HasCellFormat ? row.CellFormatInternal.Element : manager.Workbook.CellFormats.DefaultElement;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//// Add the cell elements that have data
			//foreach (WorksheetCell cell in row.Cells)
			//{
			//    if (cell.HasData)
			//        CellElement.SaveDirectHelper(manager, writer, cell, rAttributeValue);
			//}
			foreach (CellDataContext cellDataContext in row.GetCellsWithData())
			{
				// MD 4/18/11 - TFS62026
				// Pass along the row cache.
				//CellElement.SaveDirectHelper(manager, writer, row, columnIndex, rAttributeValue);
				// MD 1/10/12 - 12.1 - Cell Format Updates
				// Make sure the CellFormatData is non null before passing it to CellElement.SaveDirectHelper
				// Also, pass in the cell data context instead of just the column index, because we need more information now.
				//CellElement.SaveDirectHelper(manager, writer, row, cellDataContext.ColumnIndex, rowCache, rAttributeValue);
				if (cellDataContext.CellFormatData == null)
					cellDataContext.CellFormatData = rowFormat;

				CellElement.SaveDirectHelper(manager, writer, row, cellDataContext, rowCache, rAttributeValue);
			}

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