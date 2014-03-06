using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class WorksheetElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Worksheet"> 
        //    <sequence> 
        //        <element name="sheetPr" type="CT_SheetPr" minOccurs="0" maxOccurs="1"/> 
        //        <element name="dimension" type="CT_SheetDimension" minOccurs="0" maxOccurs="1"/> 
        //        <element name="sheetViews" type="CT_SheetViews" minOccurs="0" maxOccurs="1"/> 
        //        <element name="sheetFormatPr" type="CT_SheetFormatPr" minOccurs="0" maxOccurs="1"/> 
        //        <element name="cols" type="CT_Cols" minOccurs="0" maxOccurs="unbounded"/> 
        //        <element name="sheetData" type="CT_SheetData" minOccurs="1" maxOccurs="1"/> 
        //        <element name="sheetCalcPr" type="CT_SheetCalcPr" minOccurs="0" maxOccurs="1"/> 
        //        <element name="sheetProtection" type="CT_SheetProtection" minOccurs="0" maxOccurs="1"/> 
        //        <element name="protectedRanges" type="CT_ProtectedRanges" minOccurs="0" maxOccurs="1"/> 
        //        <element name="scenarios" type="CT_Scenarios" minOccurs="0" maxOccurs="1"/> 
        //        <element name="autoFilter" type="CT_AutoFilter" minOccurs="0" maxOccurs="1"/> 
        //        <element name="sortState" type="CT_SortState" minOccurs="0" maxOccurs="1"/> 
        //        <element name="dataConsolidate" type="CT_DataConsolidate" minOccurs="0" maxOccurs="1"/> 
        //        <element name="customSheetViews" type="CT_CustomSheetViews" minOccurs="0" maxOccurs="1"/> 
        //        <element name="mergeCells" type="CT_MergeCells" minOccurs="0" maxOccurs="1"/> 
        //        <element name="phoneticPr" type="CT_PhoneticPr" minOccurs="0" maxOccurs="1"/> 
        //        <element name="conditionalFormatting" type="CT_ConditionalFormatting" minOccurs="0" maxOccurs="unbounded"/> 
        //        <element name="dataValidations" type="CT_DataValidations" minOccurs="0" maxOccurs="1"/> 
        //        <element name="hyperlinks" type="CT_Hyperlinks" minOccurs="0" maxOccurs="1"/> 
        //        <element name="printOptions" type="CT_PrintOptions" minOccurs="0" maxOccurs="1"/> 
        //        <element name="pageMargins" type="CT_PageMargins" minOccurs="0" maxOccurs="1"/> 
        //        <element name="pageSetup" type="CT_PageSetup" minOccurs="0" maxOccurs="1"/> 
        //        <element name="headerFooter" type="CT_HeaderFooter" minOccurs="0" maxOccurs="1"/> 
        //        <element name="rowBreaks" type="CT_PageBreak" minOccurs="0" maxOccurs="1"/> 
        //        <element name="colBreaks" type="CT_PageBreak" minOccurs="0" maxOccurs="1"/> 
        //        <element name="customProperties" type="CT_CustomProperties" minOccurs="0" maxOccurs="1"/> 
        //        <element name="cellWatches" type="CT_CellWatches" minOccurs="0" maxOccurs="1"/> 
        //        <element name="ignoredErrors" type="CT_IgnoredErrors" minOccurs="0" maxOccurs="1"/> 
        //        <element name="smartTags" type="CT_SmartTags" minOccurs="0" maxOccurs="1"/> 
        //        <element name="drawing" type="CT_Drawing" minOccurs="0" maxOccurs="1"/> 
        //        <element name="legacyDrawing" type="CT_LegacyDrawing" minOccurs="0" maxOccurs="1"/> 
        //        <element name="legacyDrawingHF" type="CT_LegacyDrawing" minOccurs="0" maxOccurs="1"/> 
        //        <element name="picture" type="CT_SheetBackgroundPicture" minOccurs="0" maxOccurs="1"/> 
        //        <element name="oleObjects" type="CT_OleObjects" minOccurs="0" maxOccurs="1"/> 
        //        <element name="controls" type="CT_Controls" minOccurs="0" maxOccurs="1"/> 
        //        <element name="webPublishItems" type="CT_WebPublishItems" minOccurs="0" maxOccurs="1"/> 
        //        <element name="tableParts" type="CT_TableParts" minOccurs="0" maxOccurs="1"/> 
        //        <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/> 
        //    </sequence> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "worksheet";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            WorksheetElement.LocalName;

		// MD 10/12/10
		// Found while fixing TFS49853
		// Moved multiply defined constants to a single location.
        //internal const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        //private const string RelationshipsNamespacePrefix = "r";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.worksheet; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            // Create a new, blank worksheet, since we don't know the name of the worksheet at this point.
			// MD 4/12/11
			// Found while fixing TFS67084
			// We use the Workbook more often than the parent collection, so we will store that instead.
            //Worksheet worksheet = new Worksheet(null, manager.Workbook.Worksheets);
			// MD 4/6/12 - TFS102169
			// The workbook part is now loaded before the worksheet part, so the Worksheet instances will be created already.
			//Worksheet worksheet = new Worksheet(null, manager.Workbook);
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];
			if (worksheet == null)
			{
				Utilities.DebugFail("Cannot find the Worksheet on the context stack.");
				return;
			}

			// MD 11/19/08 - TFS10637/BR23998
			// For existing worksheets that are loaded in, default the ShowExpansionIndicatorBelowGroup to true,
			// the default for Excel.
			worksheet.ShowExpansionIndicatorBelowGroup = true;

            // Get the item off of the context stack and set its data to the worksheet so 
            // that we can later add this worksheet to the workbook's collection
            ChildDataItem item = manager.ContextStack[typeof(ChildDataItem)] as ChildDataItem;
            if (item != null)            
                item.Data = worksheet;

			manager.ContextStack.Push( worksheet );

            //  BF 8/8/08
            //  Push a reference to the PrintOptions onto the context stack
            manager.ContextStack.Push( worksheet.PrintOptions );
        }
        #endregion //Load
		
		// MD 9/9/08 - Excel 2007 Format
		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		{
			base.OnAfterLoadChildElements( manager, elementCache );

			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "Could not find a worksheet on the context stack." );
				return;
			}

			worksheet.Shapes.RemoveInvalidShapes();
		} 

		#endregion OnAfterLoadChildElements

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
				Utilities.DebugFail("Could not get the worksheet from the context stack");
                return;
            }
            
            XmlElementBase.AddNamespaceDeclaration(
                element,
                WorksheetElement.RelationshipsNamespacePrefix,
                WorksheetElement.RelationshipsNamespace);

			// MD 5/25/11 - Data Validations / Page Breaks
			// The print options are already in the context stack at this point.
			////  BF 8/8/08
			////  Push the worksheet's PrintOptions onto the stack so the
			////  PrintOptionsElement doesn't have to know who its parent is.
			//PrintOptions printOptions = worksheet.PrintOptions;
			//manager.ContextStack.Push( printOptions );

            
            // if it doesn't have any attributes/elements and didn't exist in the original XML, don't save it)

            // Add the 'sheetPr' element
            XmlElementBase.AddElement(element, SheetPrElement.QualifiedName);

            // Add the 'dimension' element
            XmlElementBase.AddElement(element, DimensionElement.QualifiedName);

            // Add the 'sheetViews' element
            XmlElementBase.AddElement(element, SheetViewsElement.QualifiedName);

            // Add the 'sheetFormatPr' element
            XmlElementBase.AddElement(element, SheetFormatPrElement.QualifiedName);

            // Add the 'cols' element
			// MD 3/15/12 - TFS104581
            //if (worksheet.HasColumns)
            {
				// MD 1/10/12 - 12.1 - Cell Format Updates
				// The GetColumnBlocks method is no longer static.
                //List<WorkbookSerializationManager.ColumnBlockInfo> columnInfo = WorkbookSerializationManager.GetColumnBlocks(worksheet);
				// MD 3/15/12 - TFS104581
				//List<WorkbookSerializationManager.ColumnBlockInfo> columnInfo = manager.GetColumnBlocks(worksheet);
				//
                //if (columnInfo != null && columnInfo.Count > 0)
				List<WorksheetColumnBlock> columnInfo = new List<WorksheetColumnBlock>(worksheet.ColumnBlocks.Values);
				if (columnInfo.Count > 1 || columnInfo[0].IsEmpty == false)
                {
                    // We only want to add the 'cols' element if we're actually going to have data to serialize
                    XmlElementBase.AddElement(element, ColumnsElement.QualifiedName);

                    // Push the list onto the context stack so that the child elements can properly serialize                    
                    manager.ContextStack.Push(columnInfo);
                }
            }

            // Add the 'sheetData' element
            XmlElementBase.AddElement(element, SheetDataElement.QualifiedName);

            // Add the 'sheetCalcPr' element
            XmlElementBase.AddElement(element, SheetCalcPrElement.QualifiedName);

			// MD 3/22/11 - TFS66776
			// Add the 'sheetProtection' element
			if (worksheet.Protected)
				XmlElementBase.AddElement(element, SheetProtectionElement.QualifiedName);

            //  BF 8/7/08
            // Add the 'customSheetViews' element            
            if ( manager.Workbook.HasCustomViews )
                XmlElementBase.AddElement(element, CustomSheetViewsElement.QualifiedName);

            // Add the 'mergeCells' element
            if (worksheet.MergedCellsRegions.Count > 0)
                XmlElementBase.AddElement(element, MergeCellsElement.QualifiedName);

			// Add the 'dataValidations' element
			// MD 2/1/11 - Data Validation support
			//if (worksheet.DataValidationInfo2007 != null)
			if (worksheet.HasDataValidationRules)
				XmlElementBase.AddElement(element, DataValidationsElement.QualifiedName);

            // Add the 'printOptions' element
            XmlElementBase.AddElement(element, PrintOptionsElement.QualifiedName);

            // Add the 'pageMargins' element
            XmlElementBase.AddElement(element, PageMarginsElement.QualifiedName);       
    
            // Add the 'pageSetup' element
            XmlElementBase.AddElement(element, PageSetupElement.QualifiedName);

            // Add the 'headerFooter' element
            XmlElementBase.AddElement(element, HeaderFooterElement.QualifiedName);

			// MD 2/1/11 - Page Break support
			// Add the 'rowBreaks' element
			if (worksheet.PrintOptions.HasHorizontalPageBreaks)
				XmlElementBase.AddElement(element, RowBreaksElement.QualifiedName);

			// MD 2/1/11 - Page Break support
			// Add the 'colBreaks' element
			if (worksheet.PrintOptions.HasVerticalPageBreaks)
				XmlElementBase.AddElement(element, ColBreaksElement.QualifiedName);

            //  BF 8/28/08
            //  Add the 'drawing' element if the Shapes collection is not empty.
			// MD 4/28/11 - TFS62775
			// Some shapes could be invalid in the collection. Instead, check CurrentWorksheetHasShapes.
            //if ( worksheet.Shapes.Count > 0 )
			if (manager.CurrentWorksheetHasShapes)
                XmlElementBase.AddElement(element, DrawingElement.QualifiedName);

            // Add the 'picture' element
            if (worksheet.ImageBackground != null)
                XmlElementBase.AddElement(element, PictureElement.QualifiedName);

			// Add the 'legacyDrawing' element
			if ( worksheet.CommentShapes.Count != 0 )
				XmlElementBase.AddElement( element, LegacyDrawingElement.QualifiedName );

			// MD 12/30/11 - 12.1 - Table Support
			if (worksheet.Tables.Count != 0)
				XmlElementBase.AddElement(element, TablePartsElement.QualifiedName);
        }
        #endregion //Save

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