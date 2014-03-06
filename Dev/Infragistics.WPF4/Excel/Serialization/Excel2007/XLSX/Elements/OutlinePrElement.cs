using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{



    internal class OutlinePrElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        // <complexType name="CT_OutlinePr"> 
        //   <attribute name="applyStyles" type="xsd:boolean" use="optional" default="false"/> 
        //   <attribute name="summaryBelow" type="xsd:boolean" use="optional" default="true"/> 
        //   <attribute name="summaryRight" type="xsd:boolean" use="optional" default="true"/> 
        //   <attribute name="showOutlineSymbols" type="xsd:boolean" use="optional" default="true"/> 
        // </complexType> 
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>outlinePr</summary>
        public const string LocalName = "outlinePr";

        /// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/outlinePr</summary>
        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            OutlinePrElement.LocalName;

        private const string ApplyStylesAttributeName = "applyStyles";
        private const string SummaryBelowAttributeName = "summaryBelow";
        private const string SummaryRightAttributeName = "summaryRight";
        private const string ShowOutlineSymbolsAttributeName = "showOutlineSymbols";

        #endregion Constants

        #region Base class overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.outlinePr; }
        }

        #endregion Type

        #region Load

        /// <summary>Loads the data for this element from the specified manager.</summary>
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

            object attributeValue = null;
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case OutlinePrElement.ApplyStylesAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                            // Roundtrip - Page 1985
                        }
                        break;

                    case OutlinePrElement.SummaryBelowAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

							// MD 11/19/08 - TFS10637
							// This attribute is currently supported so it shouldn't have been a round-trip property
							worksheet.ShowExpansionIndicatorBelowGroup = (bool)attributeValue;

							// MD 6/4/10 - ChildRecordsDisplayOrder feature
							worksheet.DisplayOptions.ShowExpansionIndicatorBelowGroupedRows = worksheet.ShowExpansionIndicatorBelowGroup
								? ExcelDefaultableBoolean.True 
								: ExcelDefaultableBoolean.False;
                        }
                        break;

                    case OutlinePrElement.SummaryRightAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

							// MD 6/4/10 - ChildRecordsDisplayOrder feature
							worksheet.DisplayOptions.ShowExpansionIndicatorToRightOfGroupedColumns = (bool)attributeValue
								? ExcelDefaultableBoolean.True
								: ExcelDefaultableBoolean.False;
                        }
                        break;

                    case OutlinePrElement.ShowOutlineSymbolsAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);
                            worksheet.DisplayOptions.ShowOutlineSymbols = (bool)attributeValue;
                        }
                        break;

                }
            }
        }

        #endregion Load

        #region Save

        /// <summary>Saves the data for this element to the specified manager.</summary>
        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {            
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the context stack");
                return;
            }

            string attributeValue = String.Empty;

            // Save the 'showOutlineSymbols' attribute
            attributeValue = XmlElementBase.GetXmlString(worksheet.DisplayOptions.ShowOutlineSymbols, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, OutlinePrElement.ShowOutlineSymbolsAttributeName, attributeValue);

            // Roundtrip - Save out the attributes below

            #region ApplyStyles
            // Name = 'applyStyles', Type = Boolean, Default = False

            #endregion ApplyStyles

            #region SummaryBelow
            // Name = 'summaryBelow', Type = Boolean, Default = True

			// MD 11/19/08 - TFS10637
			// This attribute is currently supported so it shouldn't have been a round-trip property
			// MD 6/4/10 - ChildRecordsDisplayOrder feature
			// Use the public property instead.
			//attributeValue = XmlElementBase.GetXmlString( worksheet.ShowExpansionIndicatorBelowGroup, DataType.Boolean, true, false );
			attributeValue = XmlElementBase.GetXmlString(worksheet.DisplayOptions.ShowExpansionIndicatorBelowGroupedRowsResolved, DataType.Boolean, true, false);

			if ( attributeValue != null )
				XmlElementBase.AddAttribute( element, OutlinePrElement.SummaryBelowAttributeName, attributeValue );

            #endregion SummaryBelow

            #region SummaryRight
            // Name = 'summaryRight', Type = Boolean, Default = True

			// MD 6/4/10 - ChildRecordsDisplayOrder feature
			attributeValue = XmlElementBase.GetXmlString(worksheet.DisplayOptions.ShowExpansionIndicatorToRightOfGroupedColumnsResolved, DataType.Boolean, true, false);
			if (attributeValue != null)
			{
				// MD 11/22/11
				// Found while writing Interop tests
				// We were writing out the wrong attribute here.
				//XmlElementBase.AddAttribute(element, OutlinePrElement.SummaryBelowAttributeName, attributeValue);
				XmlElementBase.AddAttribute(element, OutlinePrElement.SummaryRightAttributeName, attributeValue);
			}

            #endregion SummaryRight
        }

        #endregion Save

        #endregion Base class overrides

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