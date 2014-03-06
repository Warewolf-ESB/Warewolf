using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class PaneElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Pane"> 
        //  <attribute name="xSplit" type="xsd:double" use="optional" default="0"/> 
        //  <attribute name="ySplit" type="xsd:double" use="optional" default="0"/> 
        //  <attribute name="topLeftCell" type="ST_CellRef" use="optional"/> 
        //  <attribute name="activePane" type="ST_Pane" use="optional" default="topLeft"/> 
        //  <attribute name="state" type="ST_PaneState" use="optional" default="split"/> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "pane";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            PaneElement.LocalName;

        private const string ActivePaneAttributeName = "activePane";
        private const string StateAttributeName = "state";
        private const string TopLeftCellAttributeName = "topLeftCell";
        private const string XSplitAttributeName = "xSplit";
        private const string YSplitAttributeName = "ySplit";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.pane; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ChildDataItem item = manager.ContextStack[typeof(ChildDataItem)] as ChildDataItem;
            if (item == null)
            {
                Utilities.DebugFail("Could not get the ChildDataItem from the ContextStack");
                return;
            }
            
            ST_PaneState paneState = ST_PaneState.split;
            string topLeftCell = String.Empty;
            double xSplit = 0, ySplit = 0;

            // Roundtrip
            //ST_Pane activePane = ST_Pane.topLeft;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case PaneElement.ActivePaneAttributeName:
                        // Roundtrip - Page 1995
                        //activePane = (ST_Pane)XmlElementBase.GetValue(attribute.Value, DataType.ST_Pane, ST_Pane.topLeft);
                        break;

                    case PaneElement.StateAttributeName:
                        paneState = (ST_PaneState)XmlElementBase.GetValue(attribute.Value, DataType.ST_PaneState, ST_PaneState.split);
                        break;

                    case PaneElement.TopLeftCellAttributeName:
                        topLeftCell = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_CellRef, String.Empty);
                        break;

                    case PaneElement.XSplitAttributeName:
                        xSplit = (double)XmlElementBase.GetValue(attribute.Value, DataType.Double, 0);
                        break;

                    case PaneElement.YSplitAttributeName:
                        ySplit = (double)XmlElementBase.GetValue(attribute.Value, DataType.Double, 0);
                        break;

                    default:
                        Utilities.DebugFail("Unknown attribute");
                        break;
                }
            }

            //  BF 8/8/08
            //  I had to modify this so that this element deals directly with a DisplayOptions
            //  instance. This is so it can support a parent element of type SheetViewElement
            //  and also CustomSheetViewElement.
            DisplayOptions displayOptions = manager.ContextStack[typeof(DisplayOptions)] as DisplayOptions;
            if ( displayOptions == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
            
            short columnIndex = -1;
            int rowIndex = -1;
            if (topLeftCell.Length > 0)
            {
				// MD 4/6/12 - TFS101506
                //if (Utilities.ParseA1CellAddress(topLeftCell, WorkbookFormat.Excel2007, out columnIndex, out rowIndex) == false)
				if (Utilities.ParseA1CellAddress(topLeftCell, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out columnIndex, out rowIndex) == false)
                    Utilities.DebugFail("Could not parse address");
            }

            switch (paneState)
            {
                case ST_PaneState.split:
                    UnfrozenPaneSettings uPaneSettings = displayOptions.UnfrozenPaneSettings;
                    if (columnIndex > -1 && rowIndex > -1)
                    {                                             
                        uPaneSettings.FirstColumnInRightPane = columnIndex;
                        uPaneSettings.FirstRowInBottomPane = rowIndex;
                    }
                    uPaneSettings.LeftPaneWidth = (int)xSplit;
                    uPaneSettings.TopPaneHeight = (int)ySplit;

                    break;

                case ST_PaneState.frozen:
                    displayOptions.PanesAreFrozen = true;

                    FrozenPaneSettings fPaneSettings = displayOptions.FrozenPaneSettings;
                    if (columnIndex > -1 && rowIndex > -1)
                    {
                        fPaneSettings.FirstColumnInRightPane = columnIndex;
                        fPaneSettings.FirstRowInBottomPane = rowIndex;
                    }
                    fPaneSettings.FrozenColumns = (int)xSplit;
                    fPaneSettings.FrozenRows = (int)ySplit;                    

                    break;

                default:
                    Utilities.DebugFail("Unsupported pane state");
                    goto case ST_PaneState.frozen;
            }
        }
        #endregion //Load

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

            //  BF 8/8/08
            //  I had to modify this so that this element deals directly with a DisplayOptions
            //  instance. This is so it can support a parent element of type SheetViewElement
            //  and also CustomSheetViewElement.
            DisplayOptions displayOptions = manager.ContextStack[typeof(DisplayOptions)] as DisplayOptions;
            if ( displayOptions == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
            
            string attributeValue = String.Empty;
            if (displayOptions.PanesAreFrozen)
            {
                FrozenPaneSettings fPaneSettings = displayOptions.FrozenPaneSettings;

                // Add the 'xSplit' attribute
                attributeValue = XmlElementBase.GetXmlString(fPaneSettings.FrozenColumns, DataType.Integer, 0, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PaneElement.XSplitAttributeName, attributeValue);

                // Add the 'ySplit' attribute
                attributeValue = XmlElementBase.GetXmlString(fPaneSettings.FrozenRows, DataType.Integer, 0, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PaneElement.YSplitAttributeName, attributeValue);

                // Add the 'topLeftCell' attribute.  Though this is optional, since we can calculate it, we may as well
				// MD 7/9/10 - TFS34788
				// The top-left cell must be at least after the frozen row and column boundary.
				//if (fPaneSettings.FirstColumnInRightPane > 0 && fPaneSettings.FirstRowInBottomPane > 0)
				//{
				//    attributeValue = CellAddress.GetCellReferenceString(fPaneSettings.FirstRowInBottomPane, 
				//        fPaneSettings.FirstColumnInRightPane, 
				//        true, 
				//        true, 
				//        manager.Workbook.CurrentFormat, 
				//        worksheet.Rows[fPaneSettings.FirstRowInBottomPane].Cells[fPaneSettings.FirstColumnInRightPane], 
				//        false, 
				//        CellReferenceMode.A1);
				//
				//    XmlElementBase.AddAttribute(element, PaneElement.TopLeftCellAttributeName, attributeValue);
				//}
				int firstColumnInRightPane = Math.Max(fPaneSettings.FrozenColumns, fPaneSettings.FirstColumnInRightPane);
				int firstRowInBottomPane = Math.Max(fPaneSettings.FrozenRows, fPaneSettings.FirstRowInBottomPane);
				if (firstColumnInRightPane > 0 || firstRowInBottomPane > 0)
                {
					attributeValue = CellAddress.GetCellReferenceString(firstRowInBottomPane,
						firstColumnInRightPane,
						true,
						true,
						manager.Workbook.CurrentFormat,
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//worksheet.Rows[firstRowInBottomPane].Cells[firstColumnInRightPane],
						// MD 2/20/12 - 12.1 - Table Support
						//worksheet.Rows[firstRowInBottomPane], (short)firstColumnInRightPane,
						firstRowInBottomPane, (short)firstColumnInRightPane,
						false,
						CellReferenceMode.A1);

					XmlElementBase.AddAttribute(element, PaneElement.TopLeftCellAttributeName, attributeValue);
				}

                // Roundtrip - Add the 'activePane' attribute

                // Add the 'state' attribute
                attributeValue = XmlElementBase.GetXmlString(ST_PaneState.frozen, DataType.ST_PaneState);
                XmlElementBase.AddAttribute(element, PaneElement.StateAttributeName, attributeValue);
            }
            else
            {
                UnfrozenPaneSettings uPaneSettings = displayOptions.UnfrozenPaneSettings;

                // Add the 'xSplit' attribute
                attributeValue = XmlElementBase.GetXmlString(uPaneSettings.LeftPaneWidth, DataType.Integer, 0, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PaneElement.XSplitAttributeName, attributeValue);

                // Add the 'ySplit' attribute
                attributeValue = XmlElementBase.GetXmlString(uPaneSettings.TopPaneHeight, DataType.Integer, 0, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PaneElement.YSplitAttributeName, attributeValue);

                // Add the 'topLeftCell' attribute
                if (uPaneSettings.FirstColumnInRightPane > 0 && uPaneSettings.FirstRowInBottomPane > 0)
                {
                    attributeValue = CellAddress.GetCellReferenceString(uPaneSettings.FirstRowInBottomPane,
                        uPaneSettings.FirstColumnInRightPane,
                        true,
                        true,
                        manager.Workbook.CurrentFormat,
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
                        //worksheet.Rows[uPaneSettings.FirstRowInBottomPane].Cells[uPaneSettings.FirstColumnInRightPane],
						// MD 2/20/12 - 12.1 - Table Support
						//worksheet.Rows[uPaneSettings.FirstRowInBottomPane], (short)uPaneSettings.FirstColumnInRightPane,
						uPaneSettings.FirstRowInBottomPane, (short)uPaneSettings.FirstColumnInRightPane,
                        false,
                        CellReferenceMode.A1);

                    XmlElementBase.AddAttribute(element, PaneElement.TopLeftCellAttributeName, attributeValue);
                }

                // Roundtrip - Add the 'activePane' attribute

                // We don't need to add the 'state' attribute here since the default is 'split'
            }
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