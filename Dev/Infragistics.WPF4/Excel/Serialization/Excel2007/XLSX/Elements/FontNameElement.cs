using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class FontNameElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_FontName"> 
        //  <attribute name="val" type="ST_Xstring" use="required"/> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "rFont";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            FontNameElement.LocalName;

        private const string ValAttributeName = "val";

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.rFont; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            IWorkbookFont font = manager.ContextStack[typeof(IWorkbookFont)] as IWorkbookFont;
            if (font == null)
            {
                Utilities.DebugFail("Could not get the font from the context stack");
                return;
            }

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case FontNameElement.ValAttributeName:
                        font.Name = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Xstring, String.Empty);
                        break;
                }
            }
        }

        #endregion Load

        #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            IWorkbookFont font = manager.ContextStack[typeof(IWorkbookFont)] as IWorkbookFont;
            if (font == null)
            {
                Utilities.DebugFail("Could not get the font from the context stack");
                return;
            }

            // Add the 'val' attribute
            string attributeVal = XmlElementBase.GetXmlString(font.Name, DataType.ST_Xstring);
            XmlElementBase.AddAttribute(element, FontNameElement.ValAttributeName, attributeVal);
        }
        #endregion Save

        #endregion Base class overrides

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			XmlWriter writer,
			string fontName)
		{
			writer.WriteStartElement(FontNameElement.LocalName);
			writer.WriteAttributeString(FontNameElement.ValAttributeName, XmlElementBase.GetXmlString(fontName, DataType.ST_Xstring));
			writer.WriteEndElement();
		} 

		#endregion // SaveDirectHelper
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