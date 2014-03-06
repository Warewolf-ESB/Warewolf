using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class PageMarginsElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_PageMargins"> 
        //  <attribute name="left" type="xsd:double" use="required"/> 
        //  <attribute name="right" type="xsd:double" use="required"/> 
        //  <attribute name="top" type="xsd:double" use="required"/> 
        //  <attribute name="bottom" type="xsd:double" use="required"/> 
        //  <attribute name="header" type="xsd:double" use="required"/> 
        //  <attribute name="footer" type="xsd:double" use="required"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "pageMargins";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            PageMarginsElement.LocalName;

        private const string LeftAttributeName = "left";
        private const string RightAttributeName = "right";
        private const string TopAttributeName = "top";
        private const string BottomAttributeName = "bottom";
        private const string HeaderAttributeName = "header";
        private const string FooterAttributeName = "footer";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.pageMargins; }
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
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;
            if ( options == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
            
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                
                //  BF 8/8/08 (see above)
                //PrintOptions options = worksheet.PrintOptions;
                
                double attributeValue = (double)XmlElementBase.GetAttributeValue(attribute, DataType.Double, 0);
                
                switch (attributeName)
                {
                    case PageMarginsElement.LeftAttributeName:
                        options.LeftMargin = attributeValue;
                        break;

                    case PageMarginsElement.RightAttributeName:
                        options.RightMargin = attributeValue;
                        break;

                    case PageMarginsElement.TopAttributeName:
                        options.TopMargin = attributeValue;
                        break;

                    case PageMarginsElement.BottomAttributeName:
                        options.BottomMargin = attributeValue;
                        break;

                    case PageMarginsElement.HeaderAttributeName:
                        options.HeaderMargin = attributeValue;
                        break;

                    case PageMarginsElement.FooterAttributeName:
                        options.FooterMargin = attributeValue;
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
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");
                return;
            }

            //  BF 8/8/08
            //PrintOptions options = worksheet.PrintOptions;
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;
            if ( options == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
            
            string attributeValue = String.Empty;
            
            // Add the left margin
            attributeValue = XmlElementBase.GetXmlString(options.LeftMargin, DataType.Double);
            XmlElementBase.AddAttribute(element, PageMarginsElement.LeftAttributeName, attributeValue);

            // Add the right margin
            attributeValue = XmlElementBase.GetXmlString(options.RightMargin, DataType.Double);
            XmlElementBase.AddAttribute(element, PageMarginsElement.RightAttributeName, attributeValue);

            // Add the top margin
            attributeValue = XmlElementBase.GetXmlString(options.TopMargin, DataType.Double);
            XmlElementBase.AddAttribute(element, PageMarginsElement.TopAttributeName, attributeValue);

            // Add the bottom margin
            attributeValue = XmlElementBase.GetXmlString(options.BottomMargin, DataType.Double);
            XmlElementBase.AddAttribute(element, PageMarginsElement.BottomAttributeName, attributeValue);

            // Add the header margin
            attributeValue = XmlElementBase.GetXmlString(options.HeaderMargin, DataType.Double);
            XmlElementBase.AddAttribute(element, PageMarginsElement.HeaderAttributeName, attributeValue);

            // Add the footer margin
            attributeValue = XmlElementBase.GetXmlString(options.FooterMargin, DataType.Double);
            XmlElementBase.AddAttribute(element, PageMarginsElement.FooterAttributeName, attributeValue);
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