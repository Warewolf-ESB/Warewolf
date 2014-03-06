using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SheetFormatPrElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_SheetFormatPr"> 
        //  <attribute name="baseColWidth" type="xsd:unsignedInt" use="optional" default="8"/> 
        //  <attribute name="defaultColWidth" type="xsd:double" use="optional"/> 
        //  <attribute name="defaultRowHeight" type="xsd:double" use="required"/> 
        //  <attribute name="customHeight" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="zeroHeight" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="thickTop" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="thickBottom" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="outlineLevelRow" type="xsd:unsignedByte" use="optional" default="0"/> 
        //  <attribute name="outlineLevelCol" type="xsd:unsignedByte" use="optional" default="0"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "sheetFormatPr";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SheetFormatPrElement.LocalName;

        private const string BaseColWidthAttributeName = "baseColWidth";
        private const string DefaultColWidthAttributeName = "defaultColWidth";
        private const string DefaultRowHeightAttributeName = "defaultRowHeight";
        private const string CustomHeightAttributeName = "customHeight";
        private const string ZeroHeightAttributeName = "zeroHeight";
        private const string ThickTopAttributeName = "thickTop";
        private const string ThickBottomAttributeName = "thickBottom";
        private const string OutlineLevelRowAttributeName = "outlineLevelRow";
        private const string OutlineLevelColAttributeName = "outlineLevelCol";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheetFormatPr; }
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

            uint baseColWidth = 0;

			// MD 2/10/12 - TFS97827
            //int defaultColWidth = -1;
			double? defaultColWidth = null;

			// MD 12/21/11 - TFS97948
			int defaultRowHeight = -1;
			bool customHeight = false;

            object attributeValue = null;
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case SheetFormatPrElement.BaseColWidthAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 8);

                        // Roundtrip - Page 2017
                        // Store this value to save later
                        //
                        // We may need this later to calculate the defaultColWidth
                        baseColWidth = (uint)attributeValue;
                    }
                    break;

                    case SheetFormatPrElement.DefaultColWidthAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Double, -1);

                        // Roundtrip - Page 2017
                        // Store this value to save later
                        //
                        // We will examine this later since if it is not specified we need to
                        // calculate it based on the 'baseColWidth' attribute
						// MD 2/10/12 - TFS97827
                        //defaultColWidth = (int)((double)attributeValue * 256); 
						defaultColWidth = (double)attributeValue;

						// MD 10/24/11 - TFS89375
						worksheet.hasExplicitlyLoadedDefaultColumnWidth = true;

						// MD 7/3/12 - TFS115690
						// If the default column width is loaded, make sure we write it out on save.
						worksheet.ShouldSaveDefaultColumnWidths256th = true;
                    }
                    break;

                    case SheetFormatPrElement.DefaultRowHeightAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Double, -1);

                        // Roundtrip - Page 2017
                        // The DefaultRowHeight on the worksheet will return the resolved value
                        // if we didn't have an initial default, so we shouldn't use that value
                        // to store as the round-trip value.
                        //                        
                        // The value is stored in points, so convert it to twips.
						// MD 12/21/11 - TFS97948
						// Don't apply this yet. We should only apply it is customHeight is True.
                        //worksheet.DefaultRowHeight = (int)((double)attributeValue * 20);
						defaultRowHeight = (int)((double)attributeValue * 20);
                    }
                    break;

                    case SheetFormatPrElement.CustomHeightAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

						// MD 12/21/11 - TFS97948
						customHeight = (bool)attributeValue;
                    }
                    break;

                    case SheetFormatPrElement.ZeroHeightAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2018
                        //bool zeroHeight = (bool)attributeValue;
                    }
                    break;

                    case SheetFormatPrElement.ThickTopAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2018
                        //bool thickTop = (bool)attributeValue;
                    }
                    break;

                    case SheetFormatPrElement.ThickBottomAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 2018
                        //bool thickBottom = (bool)attributeValue;
                    }
                    break;

                    case SheetFormatPrElement.OutlineLevelRowAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Byte, 0);
                        
                        // Roundtrip - Page 2018
                        //byte outlineLevelRow = (bool)attributeValue;
                    }
                    break;

                    case SheetFormatPrElement.OutlineLevelColAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Byte, 0);

                        // Roundtrip - Page 2017
                        //byte outlineLevelCol = (bool)attributeValue;
                    }
                    break;
                }
            }

			// MD 2/10/12 - TFS97827
			// Rewrote this code using the new SetDefaultColumnWidth methods.
			//if(defaultColWidth == -1 && baseColWidth > 0)
			//    worksheet.DefaultMinimumCharsPerColumn = (int)baseColWidth;
			//else if(defaultColWidth != -1)
			//    worksheet.DefaultColumnWidthResolved = defaultColWidth;
			if (defaultColWidth.HasValue)
				worksheet.SetDefaultColumnWidth(defaultColWidth.Value, WorksheetColumnWidthUnit.Character);
			else if (baseColWidth > 0)
				worksheet.SetDefaultColumnWidth(baseColWidth, WorksheetColumnWidthUnit.CharacterPaddingExcluded, true);

			// MD 12/21/11 - TFS97948
			// Only apply the defaultRowHeight if customHeight is True.
			if (customHeight && defaultRowHeight > 0)
				worksheet.DefaultRowHeight = defaultRowHeight;
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];
            if (worksheet == null)
            {
                Utilities.DebugFail("There is no worksheet in the context stack.");
                return;
            }

            string attributeValue = String.Empty;

            // Add the 'baseColWidth' attribute
			//if (worksheet.DefaultMinimumCharsPerColumn > 0)
			//{
			//    //attributeValue = XmlElementBase.GetXmlString(worksheet.DefaultMinimumCharsPerColumn, DataType.UInt, 8, false);
			//    uint baseColWidth = (uint)Math.Truncate(worksheet.DefaultMinimumCharsPerColumn);
			//    attributeValue = XmlElementBase.GetXmlString(baseColWidth, DataType.UInt, (uint)8, false);
			//
			//    if (attributeValue != null)
			//        XmlElementBase.AddAttribute(element, SheetFormatPrElement.BaseColWidthAttributeName, attributeValue);
			//}
			uint baseColWidth = (uint)MathUtilities.Truncate(worksheet.GetDefaultColumnWidth(WorksheetColumnWidthUnit.CharacterPaddingExcluded));
			if (baseColWidth > 0)
			{
				attributeValue = XmlElementBase.GetXmlString(baseColWidth, DataType.UInt, (uint)8, false);
				if (attributeValue != null)
					XmlElementBase.AddAttribute(element, SheetFormatPrElement.BaseColWidthAttributeName, attributeValue);
			}

			// MD 7/3/12 - TFS115690
			if (worksheet.ShouldSaveDefaultColumnWidths256th)
			{
            // Add the 'defaultColWidth' attribute
			//double defaultColWidth = worksheet.DefaultColumnWidthResolved / 256d;
			//attributeValue = XmlElementBase.GetXmlString(defaultColWidth, DataType.Double);
			double defaultColWidth = worksheet.GetDefaultColumnWidth(WorksheetColumnWidthUnit.Character);
			attributeValue = XmlElementBase.GetXmlString(defaultColWidth, DataType.Double);
            XmlElementBase.AddAttribute(element, SheetFormatPrElement.DefaultColWidthAttributeName, attributeValue);
			}

            // Write the 'defaultRowHeight' attribute
            attributeValue = XmlElementBase.GetXmlString(worksheet.DefaultRowHeight / 20d, DataType.Double);
            XmlElementBase.AddAttribute(element, SheetFormatPrElement.DefaultRowHeightAttributeName, attributeValue);

			// MD 12/21/11 - TFS97948
			// If we have a custom height, write out True for the customHeight attribute.
			if (worksheet.DefaultRowHeight != worksheet.DefaultRowHeightResolved)
			{
				attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean);
				XmlElementBase.AddAttribute(element, SheetFormatPrElement.CustomHeightAttributeName, attributeValue);
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