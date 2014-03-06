using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SheetDataElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_SheetData"> 
        //  <sequence> 
        //  <element name="row" type="CT_Row" minOccurs="0" maxOccurs="unbounded"/> 
        //  </sequence> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "sheetData";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SheetDataElement.LocalName;

		// MD 3/30/11 - TFS69969
		private const string SheetIdAttributeName = "sheetId";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheetData; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// This element is shared to be a descendent of both a worksheet element as well as an external book element.

			// MD 3/30/11 - TFS69969
			// Added support for loading this element when it is a descendant of ExternalLink.
			ExternalWorkbookReference externalWorkbookReference = (ExternalWorkbookReference)manager.ContextStack[typeof(ExternalWorkbookReference)];
			if (externalWorkbookReference == null)
				return;

			ExcelXmlAttribute sheetIdAttribute = element.Attributes[SheetIdAttributeName];
			if (sheetIdAttribute == null)
				return;

			int sheetId = (int)XmlElementBase.GetAttributeValue(sheetIdAttribute, DataType.Int32, 0);
			WorksheetReference worksheetReference = externalWorkbookReference.GetWorksheetReference(sheetId);

			manager.ContextStack.Push(worksheetReference);
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 7/10/12 - TFS116306
			// Implemented the saving of external data.
			ExternalWorkbookReference externalWorkbook = manager.ContextStack.Get<ExternalWorkbookReference>();
			if (externalWorkbook != null)
			{
				ExternalSheetIndex sheetIndex = manager.ContextStack.Get<ExternalSheetIndex>();
				if (sheetIndex == null)
				{
					Utilities.DebugFail("Could not get the ExternalSheetIndex from the ContextStack");
					return;
				}

				int currentSheetIndex = sheetIndex.Index++;
				WorksheetReferenceExternal worksheetReference =
					externalWorkbook.GetWorksheetReference(currentSheetIndex) as WorksheetReferenceExternal;

				if (worksheetReference == null)
					return;

				string attrbuteValue = XmlElementBase.GetXmlString(currentSheetIndex, DataType.Int32);
				XmlElementBase.AddAttribute(element, SheetDataElement.SheetIdAttributeName, attrbuteValue);

				if (worksheetReference.CachedRowValuesCount != 0)
				{
					manager.ContextStack.Push(new EnumerableContext<WorksheetReferenceExternal.RowValues>(worksheetReference.CachedRowValues));
					XmlElementBase.AddElements(element, RowElement.QualifiedName, worksheetReference.CachedRowValuesCount);
				}
				return;
			}

			// MD 4/28/11
			Utilities.DebugFail("We should have used the SaveDirect overload instead.");

            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");
                return;
            }

            // Create a list of all of the rows that have data, since the child elements
            // will have to serialized these values.
            List<WorksheetRow> worksheetRows = new List<WorksheetRow>();
            foreach (WorksheetRow row in worksheet.Rows)
            {
				// MD 7/26/10 - TFS34398
				// The serialization cache isn't stored on the row anymore.
                //if (row.HasData)
				WorksheetRowSerializationCache rowCache;
				// MD 4/18/11 - TFS62026
				// Only rows which need to be written out will have a row cache now.
				//if (manager.RowSerializationCaches.TryGetValue(row, out rowCache) && rowCache.hasData)
				//{
				if (manager.RowSerializationCaches.TryGetValue(row, out rowCache))
				{
					Debug.Assert(rowCache.hasData, "Only rows with data should have a row cache");

                    // Create a new child element that will serialize the data that we add to the list
                    XmlElementBase.AddElement(element, RowElement.QualifiedName);
                    worksheetRows.Add(row);
                }
            }

            // Push the rows that have data onto the context stack so that each child row element
            // can access the data.
            ListContext<WorksheetRow> worksheetContext = new ListContext<WorksheetRow>(worksheetRows);
            manager.ContextStack.Push(worksheetContext);
        }
        #endregion //Save

		// MD 11/4/10 - TFS49093
		#region SaveDirect

		protected override bool SaveDirect(Excel2007WorkbookSerializationManager manager, ExcelXmlDocument document)
		{
			// MD 7/10/12 - TFS116306
			// If this is for external data, let the normal Save method be called instead.
			if (manager.ContextStack.Get<ExternalWorkbookReference>() != null)
				return false;

			Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
			if (worksheet == null)
			{
				Utilities.DebugFail("Could not get the worksheet from the ContextStack");
				return false;
			}

			XmlWriter writer = document.Writer;

			writer.WriteStartElement(string.Empty, SheetDataElement.LocalName, XLSXElementBase.DefaultXmlNamespace);

			foreach (WorksheetRow row in worksheet.Rows)
			{
				WorksheetRowSerializationCache rowCache;
				// MD 4/18/11 - TFS62026
				// Only rows which need to be written out will have a row cache now.
				//if (manager.RowSerializationCaches.TryGetValue(row, out rowCache) && rowCache.hasData)
				//    RowElement.SaveDirectHelper(manager, writer, worksheet, row, rowCache);
				if (manager.RowSerializationCaches.TryGetValue(row, out rowCache) == false)
					continue;
				
				Debug.Assert(rowCache.hasData, "Only rows with data should have a row cache");
				RowElement.SaveDirectHelper(manager, writer, worksheet, row, rowCache);
			}

			writer.WriteEndElement();
			return true;
		} 

		#endregion // SaveDirect

        #endregion //Base Class Overrides
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