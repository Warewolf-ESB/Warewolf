using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class WorkbookPrElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_WorkbookPr"> 
        //  <attribute name="date1904" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="showObjects" type="ST_Objects" use="optional" default="all"/> 
        //  <attribute name="showBorderUnselectedTables" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="filterPrivacy" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="promptedSolutions" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="showInkAnnotation" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="backupFile" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="saveExternalLinkValues" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="updateLinks" type="ST_UpdateLinks" use="optional" default="userSet"/> 
        //  <attribute name="codeName" type="xsd:string" use="optional"/> 
        //  <attribute name="hidePivotFieldList" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="showPivotChartFilter" type="xsd:boolean" default="false"/> 
        //  <attribute name="allowRefreshQuery" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="publishItems" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="checkCompatibility" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="autoCompressPictures" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="refreshAllConnections" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="defaultThemeVersion" type="xsd:unsignedInt" use="optional"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "workbookPr";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            WorkbookPrElement.LocalName;

        private const string Date1904AttributeName = "date1904";
        private const string ShowObjectsAttributeName = "showObjects";
        private const string ShowBorderUnselectedTablesAttributeName = "showBorderUnselectedTables";
        private const string FilterPrivacyAttributeName = "filterPrivacy";
        private const string PromptedSolutionsAttributeName = "promptedSolutions";
        private const string ShowInkAnnotationAttributeName = "showInkAnnotation";
        private const string BackupFileAttributeName = "backupFile";
        private const string SaveExternalLinkValuesAttributeName = "saveExternalLinkValues";
        private const string UpdateLinksAttributeName = "updateLinks";
        private const string CodeNameAttributeName = "codeName";
        private const string HidePivotFieldListAttributeName = "hidePivotFieldList";
        private const string ShowPivotChartFilterAttributeName = "showPivotChartFilter";
        private const string AllowRefreshQueryAttributeName = "allowRefreshQuery";
        private const string PublishItemsAttributeName = "publishItems";
        private const string CheckCompatibilityAttributeName = "checkCompatibility";
        private const string AutoCompressPicturesAttributeName = "autoCompressPictures";
        private const string RefreshAllConnectionsAttributeName = "refreshAllConnections";
        private const string DefaultThemeVersionAttributeName = "defaultThemeVersion";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.workbookPr; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            Workbook workbook = manager.Workbook;
            object attributeValue = null;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case WorkbookPrElement.Date1904AttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        bool useDate1904 = (bool)attributeValue;                        
                        workbook.DateSystem = useDate1904 ? DateSystem.From1904 : DateSystem.From1900;
                    }
                    break;

                    case WorkbookPrElement.ShowObjectsAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Objects, ObjectDisplayStyle.ShowAll);
                        ObjectDisplayStyle displayStyle = (ObjectDisplayStyle)attributeValue;
                        workbook.WindowOptions.ObjectDisplayStyle = displayStyle;
                    }
                    break;

                    case WorkbookPrElement.ShowBorderUnselectedTablesAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - page 1914
                        //bool showBorderUnselectedTables = (bool)attributeValue;                        
                    }
                    break;

                    case WorkbookPrElement.FilterPrivacyAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - page 1912
                        //bool filterPrivacy = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.PromptedSolutionsAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - page 1913
                        //bool promptedSolutions = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.ShowInkAnnotationAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - page 1914
                        // bool showInkAnnotation = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.BackupFileAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1911
                        // bool backupFile = (bool)attributeName;
                    }
                    break;

                    case WorkbookPrElement.SaveExternalLinkValuesAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);
                        workbook.SaveExternalLinkedValues = (bool)attributeValue;                        
                    }
                    break;

                    case WorkbookPrElement.UpdateLinksAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Links, ST_Links.userSet);
                        
                        // Roundtrip - Page 1915
                        //ST_Links updateLinks = (ST_Links)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.CodeNameAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.String, String.Empty);

						// MD 10/1/08 - TFS8471
						manager.Workbook.VBAObjectName = (string)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.HidePivotFieldListAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1913
                        // bool hidePivotFieldList = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.ShowPivotChartFilterAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1914
                        // bool showPivotChartFilter = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.AllowRefreshQueryAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1911
                        // bool allowRefreshQuery = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.PublishItemsAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1913
                        // bool publishItems = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.CheckCompatibilityAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1912
                        
                        // bool checkCompatibility = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.AutoCompressPicturesAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                        // Roundtrip - Page 1911
                        // bool autoCompressPictures = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.RefreshAllConnectionsAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);

                        // Roundtrip - Page 1913
                        // bool refreshAllConnection = (bool)attributeValue;
                    }
                    break;

                    case WorkbookPrElement.DefaultThemeVersionAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt, 0);

                        // Roundtrip - Page 1912
                        // uint defaultThemeVersion = (uint)attributeValue;
                    }
                    break;

                    default:
                    {
                        Utilities.DebugFail("Unknown attribute " + attributeName);
                        break;
                    }
                }
            }
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Workbook workbook = manager.Workbook;
            string attributeValue = String.Empty;

            // Add the 'date1904' attribute
            bool date1904 = workbook.DateSystem == DateSystem.From1904;
            attributeValue = XmlElementBase.GetXmlString(date1904, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, WorkbookPrElement.Date1904AttributeName, attributeValue);

            // Add the 'showObjects' attribute
            ObjectDisplayStyle displayStyle = workbook.WindowOptions.ObjectDisplayStyle;
            attributeValue = XmlElementBase.GetXmlString(displayStyle, DataType.ST_Objects, ObjectDisplayStyle.ShowAll, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, WorkbookPrElement.ShowObjectsAttributeName, attributeValue);

            // Add the 'saveExternalLinkValues' attribute
            bool saveExternalLinkValues = workbook.SaveExternalLinkedValues;
            attributeValue = XmlElementBase.GetXmlString(saveExternalLinkValues, DataType.Boolean, true, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, WorkbookPrElement.SaveExternalLinkValuesAttributeName, attributeValue);

			// MD 10/1/08 - TFS8471
			if ( workbook.VBAData2007 != null )
			{
				// Add the 'codeName' attribute
				string vbaObjectName = workbook.VBAObjectName;
				attributeValue = XmlElementBase.GetXmlString( vbaObjectName, DataType.String, null, false );
				if ( attributeValue != null )
					XmlElementBase.AddAttribute( element, WorkbookPrElement.CodeNameAttributeName, attributeValue );
			}

            // Roundtrip - Add remaining attributes
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