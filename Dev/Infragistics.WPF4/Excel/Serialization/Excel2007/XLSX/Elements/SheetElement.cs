using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SheetElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Sheet">
        //    <attribute name="name" type="ST_Xstring" use="required"/>
        //    <attribute name="sheetId" type="xsd:unsignedInt" use="required"/>
        //    <attribute name="state" type="ST_SheetState" use="optional" default="visible"/>
        //    <attribute ref="r:id" use="required"/>
        //</complexType>

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "sheet";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SheetElement.LocalName;

        private const string NameAttributeName = "name";
        private const string SheetIdAttributeName = "sheetId";
        private const string StateAttributeName = "state";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheet; }
        }
        #endregion //Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            string name = String.Empty;
            string relationshipID = String.Empty;
            int sheedId = -1;
            WorksheetVisibility visibility = WorksheetVisibility.Visible;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);                

                switch (attributeName)
                {
                    case SheetElement.NameAttributeName:
                        name = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, 0);
                        break;

                    case SheetElement.SheetIdAttributeName:
                        sheedId = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
                        break;

                    case SheetElement.StateAttributeName:
                        visibility = (WorksheetVisibility)XmlElementBase.GetAttributeValue(attribute, DataType.ST_SheetState, WorksheetVisibility.Visible);
                        break;

					case XmlElementBase.RelationshipIdAttributeName:
                        relationshipID = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_RelationshipID, 0);
                        break;
                }
            }

			// MD 4/6/12 - TFS102169
			// The workbook part is now loaded before the worksheet parts, so this needs to be rewritten.
			#region Old Code

			//Worksheet worksheet = manager.GetRelationshipDataFromActivePart(relationshipID) as Worksheet;
			//if(worksheet != null)
			//{
			//    // Now that we've loaded the direct attributes of the sheet, we can add it to the
			//    // collection on the Workbook
			//    manager.Workbook.Worksheets.InternalAdd(worksheet);

			//    worksheet.Name = name;
			//    worksheet.DisplayOptions.Visibility = visibility;

			//    if (sheedId > -1)
			//        manager.WorksheetIndices.Add(sheedId, worksheet.Index);
			//}
			//else
			//    Utilities.DebugFail("Could not retrieve the relationship for the associated worksheet");

			#endregion // Old Code
			Worksheet worksheet = manager.Workbook.Worksheets.Add(name);

			// MD 7/3/12 - TFS115690
			// By default, we shouldn't write out the column width on a loaded workbook, unless we load that attribute.
			worksheet.ShouldSaveDefaultColumnWidths256th = false;

			worksheet.DisplayOptions.Visibility = visibility;

			string absolutePartPath = manager.GetRelationshipPathFromActivePart(relationshipID);
			manager.SetPartData(absolutePartPath, worksheet);

			if (sheedId > -1)
			    manager.WorksheetIndices.Add(sheedId, worksheet.Index);
        }
        #endregion //Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            ListContext<Worksheet> listContext = manager.ContextStack[typeof(ListContext<Worksheet>)] as ListContext<Worksheet>;
            if (listContext != null)
            {
                Worksheet currentSheet = listContext.ConsumeCurrentItem() as Worksheet;
                if (currentSheet != null)
                {
                    string attributeValue = String.Empty;

                    // Add the 'name' attribute
                    attributeValue = XmlElementBase.GetXmlString(currentSheet.Name, DataType.ST_Xstring);
                    XmlElementBase.AddAttribute(element, SheetElement.NameAttributeName, attributeValue);

                    // Add the 'sheetId' attribute
                    // Roundtrip - Possibly store the same value that is used from the Load method
                    //
                    //XmlElementBase.AddAttribute(element, SheetElement.SheetIdAttributeName, id);
                    XmlElementBase.AddAttribute(element, SheetElement.SheetIdAttributeName, currentSheet.SheetId.ToString());

                    // Add the 'state' attribute
                    attributeValue = XmlElementBase.GetXmlString(currentSheet.DisplayOptions.Visibility, DataType.ST_SheetState, WorksheetVisibility.Visible, false);
                    if(attributeValue != null)
                        XmlElementBase.AddAttribute(element, SheetElement.StateAttributeName, attributeValue);

                    // Add the relationship
                    string absolutePath = manager.GetPartPath(currentSheet);
                    string relationshipId = manager.GetRelationshipId(absolutePath);
                    attributeValue = XmlElementBase.GetXmlString(relationshipId, DataType.String);
					XmlElementBase.AddAttribute(element, XmlElementBase.RelationshipIdAttributeName, attributeValue);
                }
            }
            else
                Utilities.DebugFail("Could not get the ListContext containing the worksheets");
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