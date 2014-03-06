using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SheetViewElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_SheetView"> 
        //  <sequence> 
        //    <element name="pane" type="CT_Pane" minOccurs="0" maxOccurs="1"/> 
        //    <element name="selection" type="CT_Selection" minOccurs="0" maxOccurs="4"/> 
        //    <element name="pivotSelection" type="CT_PivotSelection" minOccurs="0" maxOccurs="4"/> 
        //    <element name="extLst" minOccurs="0" maxOccurs="1" type="CT_ExtensionList"/> 
        //  </sequence> 
        //  <attribute name="windowProtection" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="showFormulas" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="showGridLines" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="showRowColHeaders" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="showZeros" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="rightToLeft" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="tabSelected" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="showRuler" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="showOutlineSymbols" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="defaultGridColor" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="showWhiteSpace" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="view" type="ST_SheetViewType" use="optional" default="normal"/> 
        //  <attribute name="topLeftCell" type="ST_CellRef" use="optional"/> 
        //  <attribute name="colorId" type="xsd:unsignedInt" use="optional" default="64"/> 
        //  <attribute name="zoomScale" type="xsd:unsignedInt" use="optional" default="100"/> 
        //  <attribute name="zoomScaleNormal" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="zoomScaleSheetLayoutView" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="zoomScalePageLayoutView" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="workbookViewId" type="xsd:unsignedInt" use="required"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "sheetView";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SheetViewElement.LocalName;

        private const string WindowProtectionAttributeName = "windowProtection";
        private const string ShowFormulasAttributeName = "showFormulas";
        private const string ShowGridLinesAttributeName = "showGridLines";
        private const string ShowRowColHeadersAttributeName = "showRowColHeaders";
        private const string ShowZerosAttributeName = "showZeros";
        private const string RightToLeftAttributeName = "rightToLeft";
        private const string TabSelectedAttributeName = "tabSelected";
        private const string ShowRulerAttributeName = "showRuler";
        private const string ShowOutlineSymbolsAttributeName = "showOutlineSymbols";
        private const string DefaultGridColorAttributeName = "defaultGridColor";
        private const string ShowWhiteSpaceAttributeName = "showWhiteSpace";
        private const string ViewAttributeName = "view";
        private const string TopLeftCellAttributeName = "topLeftCell";
        private const string ColorIdAttributeName = "colorId";
        private const string ZoomScaleAttributeName = "zoomScale";
        private const string ZoomScaleNormalAttributeName = "zoomScaleNormal";
        private const string ZoomScaleSheetLayoutViewAttributeName = "zoomScaleSheetLayoutView";
        private const string ZoomScalePageLayoutViewAttributeName = "zoomScalePageLayoutView";
        private const string WorkbookViewIdAttributeName = "workbookViewId";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheetView; }
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

            Worksheet worksheet = item.Data as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");
                return;
            }

            //  BF 8/8/08
            //  Push the DisplayOptions onto the context stack; this is so
            //  the PaneElement class can support parent elements of type
            //  SheetViewElement (this one) and CustomSheetViewElement.
            WorksheetDisplayOptions displayOptions = worksheet.DisplayOptions;
            manager.ContextStack.Push( displayOptions );

            object attributeValue = null;

            // We only support a single SheetView, so just load the first one that we find
            bool hasLoaded = worksheet.HasLoadedSheetView;
            if (!hasLoaded)
                worksheet.HasLoadedSheetView = true;

			// MD 7/23/12 - TFS117431
			uint? currentZoomScale = null;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case SheetViewElement.ColorIdAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, 64);

                        // If we've deserialized a color id, then we know that we're not using an automatic color
                        int colorId = (int)attributeValue;
						// MD 1/16/12 - 12.1 - Cell Format Updates
                        //if (colorId != WorkbookColorCollection.AutomaticColor)
						if (colorId != WorkbookColorPalette.AutomaticColor)
                            displayOptions.UseAutomaticGridlineColor = false;

                        displayOptions.GridlineColorIndex = colorId;
                    }
                    break;

                    case SheetViewElement.DefaultGridColorAttributeName:
                    {
                        // Don't do anything here because we will serialize this attribute based on whether 
                        // we actually have a color set
                    }
                    break;

                    case SheetViewElement.RightToLeftAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool rightToLeft = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.OrderColumnsRightToLeft = rightToLeft;
                    }
                    break;

                    case SheetViewElement.ShowFormulasAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showFormulas = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowFormulasInCells = showFormulas;
                    }
                    break;

                    case SheetViewElement.ShowGridLinesAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showGridLines = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowGridlines = showGridLines;
                    }
                    break;

                    case SheetViewElement.ShowOutlineSymbolsAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showOutlineSymbols = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowOutlineSymbols = showOutlineSymbols;
                    }
                    break;

                    case SheetViewElement.ShowRowColHeadersAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showRowColHeaders = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowRowAndColumnHeaders = showRowColHeaders;
                    }
                    break;

                    case SheetViewElement.ShowRulerAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showRuler = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowRulerInPageLayoutView = showRuler;
                    }
                    break;

                    case SheetViewElement.ShowWhiteSpaceAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showWhiteSpace = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowWhitespaceInPageLayoutView = showWhiteSpace;
                    }
                    break;

                    case SheetViewElement.ShowZerosAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        bool showZeros = (bool)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.ShowZeroValues = showZeros;
                    }
                    break;

                    case SheetViewElement.TabSelectedAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2026
                        //bool tabSelected = (bool)attributeValue;
                    }
                    break;

                    case SheetViewElement.TopLeftCellAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, String.Empty);

						// MD 11/22/11
						// Found while writing Interop tests
						// We should have been processing this value instead of ignoring it.
						string topLeftCell = (string)attributeValue;
						short columnIndex = -1;
						int rowIndex = -1;
						if (topLeftCell.Length > 0)
						{
							// MD 4/9/12 - TFS101506
							//if (Utilities.ParseA1CellAddress(topLeftCell, WorkbookFormat.Excel2007, out columnIndex, out rowIndex) == false)
							if (Utilities.ParseA1CellAddress(topLeftCell, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out columnIndex, out rowIndex) == false)
								Utilities.DebugFail("Could not parse address");
						}

						displayOptions.UnfrozenPaneSettings.FirstColumnInLeftPane = columnIndex;
						displayOptions.UnfrozenPaneSettings.FirstRowInTopPane = rowIndex;
                    }
                    break;

                    case SheetViewElement.ViewAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_SheetViewType, ST_SheetViewType.normal);

                        // Roundtrip - Page 2026
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        WorksheetView view = (WorksheetView)attributeValue;
                        if (hasLoaded == false)
                            worksheet.DisplayOptions.View = view;
                    }
                    break;

                    case SheetViewElement.WindowProtectionAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2027
                        //bool windowProtection = (bool)attributeValue;
                    }
                    break;

                    case SheetViewElement.WorkbookViewIdAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);

                        // Roundtrip - Page 2027
                        //uint workbookViewId = (uint)attributeValue;
                    }
                    break;

                    case SheetViewElement.ZoomScaleAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 100);

						// MD 7/23/12 - TFS117431
						currentZoomScale = (uint)attributeValue;
                    }
                    break;

                    case SheetViewElement.ZoomScaleNormalAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);

                        // Roundtrip - Page 2027
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        uint zoomScaleNormal = (uint)attributeValue;
                        if (hasLoaded == false)
						{
							// MD 11/11/09 - TFS24618
							// Make sure setting the magnification will not throw an exception.
							//worksheet.DisplayOptions.MagnificationInNormalView = (int)zoomScaleNormal;
							int magnification = (int)zoomScaleNormal;
							Utilities.EnsureMagnificationIsValid( ref magnification );
							worksheet.DisplayOptions.MagnificationInNormalView = magnification;
						}
                    }
                    break;

                    case SheetViewElement.ZoomScalePageLayoutViewAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);

                        // Roundtrip - Page 2028
                        // Copy the attributes for the views that we don't support (i.e. more than 1).
                        uint zoomScalePageLayoutView = (uint)attributeValue;
                        if (hasLoaded == false)
						{
							// MD 11/11/09 - TFS24618
							// Make sure setting the magnification will not throw an exception.
							//worksheet.DisplayOptions.MagnificationInPageLayoutView = (int)zoomScalePageLayoutView;
							int magnification = (int)zoomScalePageLayoutView;
							Utilities.EnsureMagnificationIsValid( ref magnification );
							worksheet.DisplayOptions.MagnificationInPageLayoutView = magnification;
						}
                    }
                    break;

                    case SheetViewElement.ZoomScaleSheetLayoutViewAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);

                        // Roundtrip - Page 2028
                        uint zoomScaleSheetLayoutView = (uint)attributeValue;
                        if (hasLoaded == false)
						{
							// MD 11/11/09 - TFS24618
							// Make sure setting the magnification will not throw an exception.
							//worksheet.DisplayOptions.MagnificationInPageBreakView = (int)zoomScaleSheetLayoutView;
							int magnification = (int)zoomScaleSheetLayoutView;
							Utilities.EnsureMagnificationIsValid( ref magnification );
							worksheet.DisplayOptions.MagnificationInPageBreakView = magnification;
						}
                    }
                    break;
                }
            }

			// MD 7/23/12 - TFS117431
			if (currentZoomScale != null)
				worksheet.DisplayOptions.CurrentMagnificationLevel = (int)currentZoomScale.Value;
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
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");
                return;
            }
            
            string attributeValue = String.Empty;
            WorksheetDisplayOptions options = worksheet.DisplayOptions;

			// MD 11/22/11
			// Found while writing Interop tests
			// We should be saving out the scroll position of the top/left panes.
			int firstColumnInLeftPane = options.UnfrozenPaneSettings.FirstColumnInLeftPane;
			int firstRowInTopPane = options.UnfrozenPaneSettings.FirstRowInTopPane;
			if (firstColumnInLeftPane > 0 || firstRowInTopPane > 0)
			{
				attributeValue = CellAddress.GetCellReferenceString(firstRowInTopPane,
					firstColumnInLeftPane,
					true,
					true,
					manager.Workbook.CurrentFormat,
					// MD 2/20/12 - 12.1 - Table Support
					//worksheet.Rows[firstRowInTopPane], (short)firstColumnInLeftPane,
					firstRowInTopPane, (short)firstColumnInLeftPane,
					false,
					CellReferenceMode.A1);

				XmlElementBase.AddAttribute(element, SheetViewElement.TopLeftCellAttributeName, attributeValue);
			}
            
            // Add the 'showFormulas' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowFormulasInCells, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowFormulasAttributeName, attributeValue);

            // Add the 'showGridLines' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowGridlines, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowGridLinesAttributeName, attributeValue);

            // Add the 'showRowColHeaders' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowRowAndColumnHeaders, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowRowColHeadersAttributeName, attributeValue);

            // Add the 'showZeros' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowZeroValues, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowZerosAttributeName, attributeValue);

            // Add the 'rightToLeft' attribute
            attributeValue = XmlElementBase.GetXmlString(options.OrderColumnsRightToLeft, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.RightToLeftAttributeName, attributeValue);

            // Add the 'showRuler' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowRulerInPageLayoutView, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowRulerAttributeName, attributeValue);

            // Add the 'showOutlineSymbols' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowOutlineSymbols, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowOutlineSymbolsAttributeName, attributeValue);

            // Add the 'defaultGridColor' attribute, but only if we're using the default color
			// MD 1/16/12 - 12.1 - Cell Format Updates
            //if (options.GridlineColorIndex != WorkbookColorCollection.AutomaticColor)
			if (options.GridlineColorIndex != WorkbookColorPalette.AutomaticColor)
            {
                attributeValue = XmlElementBase.GetXmlString(false, DataType.Boolean);
                XmlElementBase.AddAttribute(element, SheetViewElement.DefaultGridColorAttributeName, attributeValue);
            }

            // Add the 'showWhiteSpace' attribute
            attributeValue = XmlElementBase.GetXmlString(options.ShowWhitespaceInPageLayoutView, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ShowWhiteSpaceAttributeName, attributeValue);

            // Add the 'view' attribute
            attributeValue = XmlElementBase.GetXmlString(options.View, DataType.ST_SheetViewType, WorksheetView.Normal, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ViewAttributeName, attributeValue);

            // Add the 'colorId' attribute
			// MD 1/16/12 - 12.1 - Cell Format Updates
            //if (options.GridlineColorIndex != WorkbookColorCollection.AutomaticColor)
			if (options.GridlineColorIndex != WorkbookColorPalette.AutomaticColor)
            {
                attributeValue = XmlElementBase.GetXmlString(options.GridlineColorIndex, DataType.UInt);
                XmlElementBase.AddAttribute(element, SheetViewElement.ColorIdAttributeName, attributeValue);
            }

            // Add the 'zoomScale' attribute

			// MD 7/23/12 - TFS117431
			// The default zoom is always 100, regardless of the view type.
			#region Old Code

			//int zoom = 0;
			//int defaultZoom = 0;
			//switch (options.View)
			//{
			//    default:
			//    case WorksheetView.Normal:
			//        zoom = options.MagnificationInNormalView;
			//        defaultZoom = 100;
			//        break;

			//    case WorksheetView.PageBreakPreview:
			//        zoom = options.MagnificationInPageBreakView;
			//        defaultZoom = 60;
			//        break;

			//    case WorksheetView.PageLayout:
			//        zoom = options.MagnificationInPageLayoutView;
			//        defaultZoom = 100;
			//        break;
			//}

			#endregion // Old Code
			int zoom = options.CurrentMagnificationLevel;
			int defaultZoom = 100;

            attributeValue = XmlElementBase.GetXmlString(zoom, DataType.UInt, defaultZoom, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ZoomScaleAttributeName, attributeValue);

            // Add the 'zoomScaleNormal' attribute
            attributeValue = XmlElementBase.GetXmlString(options.MagnificationInNormalView, DataType.UInt, 100, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ZoomScaleNormalAttributeName, attributeValue);

            // Add the 'zoomScaleSheetLayoutView' attribute
            attributeValue = XmlElementBase.GetXmlString(options.MagnificationInPageBreakView, DataType.UInt, 60, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ZoomScaleSheetLayoutViewAttributeName, attributeValue);

            // Add the 'zoomScalePageLayoutView' attribute
            attributeValue = XmlElementBase.GetXmlString(options.MagnificationInPageLayoutView, DataType.UInt, 100, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, SheetViewElement.ZoomScalePageLayoutViewAttributeName, attributeValue);

            // Write out the default required attribute until we properly round-trip everything
            XmlElementBase.AddAttribute(element, SheetViewElement.WorkbookViewIdAttributeName, "0");

            // Add the 'pane' element, but only if we actually have panes
            FrozenPaneSettings fSettings = worksheet.DisplayOptions.FrozenPaneSettings;
            UnfrozenPaneSettings uSettings = worksheet.DisplayOptions.UnfrozenPaneSettings;            
            if ((worksheet.DisplayOptions.PanesAreFrozen && (fSettings.HasHorizontalSplit || fSettings.HasVerticalSplit)) ||
               (!worksheet.DisplayOptions.PanesAreFrozen && (uSettings.HasHorizontalSplit || uSettings.HasVerticalSplit)))
            {
                XmlElementBase.AddElement(element, PaneElement.QualifiedName);
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