using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{



    internal class PageSetUpPrElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        // <complexType name="CT_PageSetUpPr"> 
        //   <attribute name="autoPageBreaks" type="xsd:boolean" use="optional" default="true"/> 
        //   <attribute name="fitToPage" type="xsd:boolean" use="optional" default="false"/> 
        // </complexType> 
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>pageSetUpPr</summary>
        public const string LocalName = "pageSetUpPr";

        /// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/pageSetUpPr</summary>
        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            PageSetUpPrElement.LocalName;

        private const string AutoPageBreaksAttributeName = "autoPageBreaks";
        private const string FitToPageAttributeName = "fitToPage";

        #endregion Constants

        #region Base class overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.pageSetUpPr; }
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

                    case PageSetUpPrElement.AutoPageBreaksAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);

                            // Roundtrip - Page 1994
                        }
                        break;

                    case PageSetUpPrElement.FitToPageAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                            if ((bool)attributeValue)
                                worksheet.PrintOptions.ScalingType = ScalingType.FitToPages;
                            else
                                worksheet.PrintOptions.ScalingType = ScalingType.UseScalingFactor;
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

            // Roundtrip - Store this value
            // Name = 'autoPageBreaks', Type = Boolean, Default = True

            // Add the 'fitToPage' attribute
            bool isFitToPages = worksheet.PrintOptions.ScalingType == ScalingType.FitToPages;
            attributeValue = XmlElementBase.GetXmlString(isFitToPages, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, PageSetUpPrElement.FitToPageAttributeName, attributeValue);
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