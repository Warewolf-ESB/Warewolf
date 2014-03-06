using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    class RgbColorElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "rgbColor";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            RgbColorElement.LocalName;

        public const string RgbAttributeName = "rgb";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.rgbColor; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ListContext<Color> listContext = (ListContext<Color>)manager.ContextStack[typeof(ListContext<Color>)];
            if (listContext == null)
            {
                Utilities.DebugFail("Failed to find the ListContext<Color> object on the stack.");
                return;
            }


            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case RgbColorElement.RgbAttributeName:
                        // p 2154 shows this attribute as optional. However, there is no color-specific information
                        // without it. So the color is only added to the list if the rgb attribute is set
                        // 8/28/08 CDS - should be processing as an int, not a string
                        //string val = (string)XLSXElementBase.GetAttributeValue(attribute, DataType.ST_UnsignedIntHex, string.Empty);
                        //if (val != string.Empty)
                        //{
                        //    try
                        //    {
                        //        Color color = ColorTranslator.FromHtml(string.Format("#{0}", val));
                        //        listContext.AddItem(color);
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Utilities.DebugFail("Unable to convert rgb attribute to a valid color :" + ex.Message);
                        //        return;
                        //    }
                        //}
                        try
                        {
                            int rgbVal = Utilities.ToInteger(XLSXElementBase.GetAttributeValue(attribute, DataType.ST_UnsignedIntHex, 0));
							rgbVal = (int)( (uint)rgbVal | 0xFF000000 );
                            Color color = Utilities.ColorFromArgb(rgbVal);
                            listContext.AddItem(color);
                        }
                        catch (Exception ex)
                        {
                            Utilities.DebugFail("Unable to convert rgb attribute to a valid color :" + ex.Message);
                            return;
                        }

                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the rgbColor element: " + attributeName);
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
            Color color = Utilities.ColorEmpty;

            ListContext<Color> listContext = manager.ContextStack[typeof(ListContext<Color>)] as ListContext<Color>;
            if (listContext != null)
                color = (Color)listContext.ConsumeCurrentItem();

            if (color ==  Utilities.ColorEmpty)
            {
                Object o = manager.ContextStack[typeof(Color)];
                color = (o != null) ? (Color)o :  Utilities.ColorEmpty;
            }

            if (color ==  Utilities.ColorEmpty)
            {
                Utilities.DebugFail("Color object not found on the context stack");
                return;
            }

            int hexValue = Utilities.ColorToArgb(color);
			hexValue = (int)( (uint)hexValue & 0x00FFFFFF );

			string attributeValue = XLSXElementBase.GetXmlString( hexValue, DataType.ST_UnsignedIntHex );
            XLSXElementBase.AddAttribute(element, RgbColorElement.RgbAttributeName, attributeValue);
        }

        #endregion Save

        #endregion Base Class Overrides
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