using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SheetPrElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        // <complexType name="CT_SheetPr"> 
        //   <sequence> 
        //   <element name="tabColor" type="CT_Color" minOccurs="0" maxOccurs="1"/> 
        //   <element name="outlinePr" type="CT_OutlinePr" minOccurs="0" maxOccurs="1"/> 
        //   <element name="pageSetUpPr" type="CT_PageSetUpPr" minOccurs="0" maxOccurs="1"/> 
        //   </sequence> 
        //   <attribute name="syncHorizontal" type="xsd:boolean" use="optional" default="false"/> 
        //   <attribute name="syncVertical" type="xsd:boolean" use="optional" default="false"/> 
        //   <attribute name="syncRef" type="ST_Ref" use="optional"/> 
        //   <attribute name="transitionEvaluation" type="xsd:boolean" use="optional" default="false"/> 
        //   <attribute name="transitionEntry" type="xsd:boolean" use="optional" default="false"/> 
        //   <attribute name="published" type="xsd:boolean" use="optional" default="true"/> 
        //   <attribute name="codeName" type="xsd:string" use="optional"/> 
        //   <attribute name="filterMode" type="xsd:boolean" use="optional" default="false"/> 
        //   <attribute name="enableFormatConditionsCalculation" type="xsd:boolean" use="optional" 
        // default="true"/> 
        // </complexType> 
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>sheetPr</summary>
        public const string LocalName = "sheetPr";

        /// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/sheetPr</summary>
        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SheetPrElement.LocalName;

        private const string SyncHorizontalAttributeName = "syncHorizontal";
        private const string SyncVerticalAttributeName = "syncVertical";
        private const string SyncRefAttributeName = "syncRef";
        private const string TransitionEvaluationAttributeName = "transitionEvaluation";
        private const string TransitionEntryAttributeName = "transitionEntry";
        private const string PublishedAttributeName = "published";
        private const string CodeNameAttributeName = "codeName";
        private const string FilterModeAttributeName = "filterMode";
        private const string EnableFormatConditionsCalculationAttributeName = "enableFormatConditionsCalculation";

        #endregion Constants

        #region Base class overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheetPr; }
        }

        #endregion Type

        #region Load

        /// <summary>Loads the data for this element from the specified manager.</summary>
        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			// MD 10/1/08 - TFS8471
			// We now need a reference to the worksheet
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];
			if ( worksheet == null )
			{
				Utilities.DebugFail( "Could not get the worksheet off the context stack" );
				return;
			}

			Workbook workBook = manager.Workbook;
			object attributeValue = null;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					case SheetPrElement.SyncHorizontalAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

						// Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.SyncVerticalAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        // Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.SyncRefAttributeName:
					{
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Ref, String.Empty);

                        // Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.TransitionEvaluationAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        // Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.TransitionEntryAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        // Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.PublishedAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );

                        // Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.CodeNameAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.String, String.Empty );

						// MD 10/1/08 - TFS8471
						worksheet.VBAObjectName = (string)attributeValue;
					}
					break;

					case SheetPrElement.FilterModeAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        // Roundtrip - Page 2019
					}
					break;

					case SheetPrElement.EnableFormatConditionsCalculationAttributeName:
					{
						attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );

                        // Roundtrip - Page 2019
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
                Utilities.DebugFail("Could not get the worksheet off the context stack");
                return;
            }

            // Add the 'tabColor' element
			// MD 1/27/12 - 12.1 - Cell Format Updates
            //if(worksheet.DisplayOptions.TabColorIndex != WorkbookColorCollection.AutomaticColor)
			if (worksheet.DisplayOptions.TabColorInfo != null)
				XmlElementBase.AddElement(element, TabColorElement.QualifiedName);

            // Add the 'outlinePr' element
            XmlElementBase.AddElement(element, OutlinePrElement.QualifiedName);

            // Add the 'pageSetUpPr' element
            XmlElementBase.AddElement(element, PageSetUpPrElement.QualifiedName);

			string attributeValue = string.Empty;

            // Roundtrip - Save out previously loaded variables for all attributes below

            #region SyncHorizontal
            // Name = 'syncHorizontal', Type = Boolean, Default = False

            #endregion SyncHorizontal

            #region SyncVertical
            // Name = 'syncVertical', Type = Boolean, Default = False

            #endregion SyncVertical

            #region SyncRef
            // Name = 'syncRef', Type = ST_Ref, Default = 

            #endregion SyncRef

            #region TransitionEvaluation
            // Name = 'transitionEvaluation', Type = Boolean, Default = False

            #endregion TransitionEvaluation

            #region TransitionEntry
            // Name = 'transitionEntry', Type = Boolean, Default = False

            #endregion TransitionEntry

            #region Published
            // Name = 'published', Type = Boolean, Default = True

            #endregion Published

            #region CodeName

			// MD 10/1/08 - TFS8471
			if ( manager.Workbook.VBAData2007 != null )
			{
				// Add the 'codeName' element
				string vbaObjectName = worksheet.VBAObjectName;
				attributeValue = XmlElementBase.GetXmlString( vbaObjectName, DataType.String, null, false );
				if ( attributeValue != null )
					XmlElementBase.AddAttribute( element, SheetPrElement.CodeNameAttributeName, attributeValue );
			}

            #endregion CodeName

            #region FilterMode
            // Name = 'filterMode', Type = Boolean, Default = False

            #endregion FilterMode

            #region EnableFormatConditionsCalculation
            // Name = 'enableFormatConditionsCalculation', Type = Boolean, Default = True

            #endregion EnableFormatConditionsCalculation
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