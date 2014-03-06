using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class PrintOptionsElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_PrintOptions"> 
        //  <attribute name="horizontalCentered" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="verticalCentered" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="headings" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="gridLines" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="gridLinesSet" type="xsd:boolean" use="optional" default="true"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "printOptions";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            PrintOptionsElement.LocalName;

        private const string HorizontalCenteredAttributeName = "horizontalCentered";
        private const string VerticalCenteredAttributeName = "verticalCentered";
        private const string HeadingsAttributeName = "headings";
        private const string GridLinesAttributeName = "gridLines";
        private const string GridLinesSetAttributeName = "gridLinesSet";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.printOptions; }
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

            bool gridLines = false, gridLinesSet = true;
            
            //  BF 8/8/08
            //  Get the reference off the context stack
            //PrintOptions options = worksheet.PrintOptions;
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;
            
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);                                                

                // All of the attributes have a default value of false except gridLinesSet
                bool attributeValue = false;
                switch (attributeName)
                {
                    case PrintOptionsElement.HorizontalCenteredAttributeName:
                    case PrintOptionsElement.VerticalCenteredAttributeName:
                    case PrintOptionsElement.HeadingsAttributeName:
                    case PrintOptionsElement.GridLinesAttributeName:
                        attributeValue = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;

                    case PrintOptionsElement.GridLinesSetAttributeName:
                        attributeValue = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);
                        break;
                }
                                
                switch (attributeName)
                {
                    case PrintOptionsElement.HorizontalCenteredAttributeName:
                        options.CenterHorizontally = attributeValue;
                        break;

                    case PrintOptionsElement.VerticalCenteredAttributeName:
                        options.CenterVertically = attributeValue;
                        break;

                    case PrintOptionsElement.HeadingsAttributeName:
                        options.PrintRowAndColumnHeaders = attributeValue;
                        break;

                    case PrintOptionsElement.GridLinesAttributeName:
                        gridLines = attributeValue;
                        break;

                    case PrintOptionsElement.GridLinesSetAttributeName:
                        // Roundtrip - Possibly store this in another variable since even
                        // though 'gridLines' and 'gridLinesSet' are documented to do 
                        // basically the same thing, they may not
                        gridLinesSet = attributeValue;
                        break;
                }
            }

            // It is documented that grid lines shall print only if both gridLines and gridLinesSet are true,
            // so we keep track of that in a variable and set it now after reading all attributes
            options.PrintGridlines = gridLines && gridLinesSet;
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
            //  Get the reference off the context stack
            //PrintOptions options = worksheet.PrintOptions;
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;

            string attributeValue = String.Empty;

            // Add the 'horizontalCentered' attribute
            attributeValue = XmlElementBase.GetXmlString(options.CenterHorizontally, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, PrintOptionsElement.HorizontalCenteredAttributeName, attributeValue);

            // Add the 'verticalCentered' attribute
            attributeValue = XmlElementBase.GetXmlString(options.CenterVertically, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, PrintOptionsElement.VerticalCenteredAttributeName, attributeValue);

            // Add the 'headings' attribute
            attributeValue = XmlElementBase.GetXmlString(options.PrintRowAndColumnHeaders, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, PrintOptionsElement.HeadingsAttributeName, attributeValue);

            // Roundtrip - 'gridLines' and 'gridLinesSet' have different defaults, so we may need to store the variable
            // in the Load() method as to what they originally were
            //
            // Add the 'gridLines' attribute
            attributeValue = XmlElementBase.GetXmlString(options.PrintGridlines, DataType.Boolean, false, false);
            if (attributeValue != null)
            {
                XmlElementBase.AddAttribute(element, PrintOptionsElement.GridLinesAttributeName, attributeValue);

                // Add the 'gridLinesSet' attribute, for now only doing so when we're also adding the 'gridLines' attribute
                attributeValue = XmlElementBase.GetXmlString(options.PrintGridlines, DataType.Boolean);
                XmlElementBase.AddAttribute(element, PrintOptionsElement.GridLinesSetAttributeName, attributeValue);
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