using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class SheetNameElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "sheetName";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SheetNameElement.LocalName;

        private const string ValAttributeName = "val";

        #endregion //Constants

        #region Base Class Overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.sheetName; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ExternalWorkbookReference workbook = manager.ContextStack[typeof(ExternalWorkbookReference)] as ExternalWorkbookReference;
            if (workbook == null)
            {
                Utilities.DebugFail("Could not find the external workbook reference on the context stack");
                return;
            }

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                switch (attributeName)
                {
                    case SheetNameElement.ValAttributeName:
                        string sheetName = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, String.Empty);

						// MD 3/30/11
						// Found while fixing TFS69969
						// This assumption is incorrect. When this element is under an ExternalLink element, it may have an empty worksheet name.
						//if (sheetName.Length > 0)
						//    workbook.WorksheetNames.Add(sheetName);
						//else
						//    Utilities.DebugFail("Was not expecting an empty string for a worksheet name");
						workbook.WorksheetNames.Add(sheetName);

                        break;
                }
            }
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            ListContext<string> sheetNames = manager.ContextStack[typeof(ListContext<string>)] as ListContext<string>;
            if (sheetNames == null)
            {
                Utilities.DebugFail("Could not get the sheet names from the context stack");
                return;
            }

            // Add the 'val' attribute
            string sheetName = (string)sheetNames.ConsumeCurrentItem();
            string attributeValue = XmlElementBase.GetXmlString(sheetName, DataType.ST_Xstring);
            XmlElementBase.AddAttribute(element, SheetNameElement.ValAttributeName, attributeValue);
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