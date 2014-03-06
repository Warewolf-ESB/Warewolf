using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class HeaderFooterElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_HeaderFooter"> 
        //  <sequence> 
        //      <element name="oddHeader" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="oddFooter" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="evenHeader" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="evenFooter" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="firstHeader" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //      <element name="firstFooter" type="ST_Xstring" minOccurs="0" maxOccurs="1"/> 
        //  </sequence> 
        //  <attribute name="differentOddEven" type="xsd:boolean" default="false"/> 
        //  <attribute name="differentFirst" type="xsd:boolean" default="false"/> 
        //  <attribute name="scaleWithDoc" type="xsd:boolean" default="true"/> 
        //  <attribute name="alignWithMargins" type="xsd:boolean" default="true"/> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "headerFooter";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            HeaderFooterElement.LocalName;

        private const string DifferentOddEvenAttributeName = "differentOddEven";
        private const string DifferentFirstAttributeName = "differentFirst";
        private const string ScaleWithDocAttributeName = "scaleWithDoc";
        private const string AlignWithMarginsAttributeName = "alignWithMargins";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.headerFooter; }
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
            
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case HeaderFooterElement.DifferentOddEvenAttributeName:
                        // Roundtrip - Page 1972
                        //bool differentOddEven = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        break;

                    case HeaderFooterElement.DifferentFirstAttributeName:
                        // Roundtrip - Page 1972
                        //bool differentFirst = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        break;

                    case HeaderFooterElement.ScaleWithDocAttributeName:
                        // Roundtrip - Page 1972
                        //bool scaleWithDoc = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, true);
                        break;

                    case HeaderFooterElement.AlignWithMarginsAttributeName:
                        // Roundtrip - Page 1972
                        //bool alignWithMargins = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, true);
                        break;

                    default:
                        Utilities.DebugFail("Unknown attribute");
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
                Utilities.DebugFail("Could not get the worksheet from the context stack");
                return;
            }

            // We don't have a concept of odd or event headers/footers in our model, so if the user specified a 
            // header/footer through the object model, we set it on the OddHeader/OddFooter elements
            
            //  BF 8/8/08
            //  Get the PrintOptions reference off the context stack
            //PrintOptions options = worksheet.PrintOptions;
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;
            if ( options == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
            
            if (options.Header != null && options.Header.Length > 0)
                // Only add the odd header if we've specified header text
                XmlElementBase.AddElement(element, OddHeaderElement.QualifiedName);

            if(options.Footer != null && options.Footer.Length > 0)
                // Only add the odd footer if we've specified header text
                XmlElementBase.AddElement(element, OddFooterElement.QualifiedName);
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