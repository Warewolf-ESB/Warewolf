using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;





namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class ColorElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "color";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ColorElement.LocalName;

        public const string AutoAttributeName = "auto";
        public const string IndexedAttributeName = "indexed";
        public const string RGBAttributeName = "rgb";
        public const string ThemeAttributeName = "theme";
        public const string TintAttributeName = "tint";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.color; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ColorInfo colorInfo = (ColorInfo)manager.ContextStack[ typeof(ColorInfo) ];

            if (colorInfo == null)
            {
                Utilities.DebugFail("ColorInfo object not found on the ContextStack.");
                return;
            }

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case ColorElement.AutoAttributeName:
                        bool val = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, true);
                        if (val)
                            colorInfo.Auto = ExcelDefaultableBoolean.True;
                        else
                            colorInfo.Auto = ExcelDefaultableBoolean.False;
                        break;
                    case ColorElement.IndexedAttributeName:
                        colorInfo.Indexed = (uint)XLSXElementBase.GetAttributeValue(attribute, DataType.UInt, 0);
                        break;
                    case ColorElement.RGBAttributeName:

                        //  BF 8/14/08
                        //  TryParse without NumberStyles.HexNumber will fail on ST_UnsignedIntHex strings.
                        #region Obsolete code
                        //string rgbString = (string)XLSXElementBase.GetAttributeValue(attribute, DataType.String, string.Empty);
                        //if (rgbString != string.Empty)
                        //{
                        //    int rgbInt = -1;
                        //    if (int.TryParse(rgbString, out rgbInt))
                        //        colorInfo.RGB = System.Drawing.Color.FromArgb(rgbInt);
                        //}
                        #endregion Obsolete code

                        int rgbVal =
                            Utilities.ToInteger(
                                XLSXElementBase.GetAttributeValue(attribute, DataType.ST_UnsignedIntHex, 0));

                        colorInfo.RGB = Utilities.ColorFromArgb(rgbVal);
                        break;
                    case ColorElement.ThemeAttributeName:
                        colorInfo.Theme = (uint)XLSXElementBase.GetAttributeValue(attribute, DataType.UInt, 0);
                        break;
                    case ColorElement.TintAttributeName:
                        colorInfo.Tint = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, 0.0);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the color element: " + attributeName);
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
            ColorInfo colorInfo = (ColorInfo)manager.ContextStack[typeof(ColorInfo)];
            if (colorInfo == null)
            {
                Utilities.DebugFail("ColorInfo not found on the ContextStack.");
                return;
            }

            #region 09/17/08 CDS - Moved to SaveColorElement() so the functionality can be called from derived elements while passing the ColorInfo object
            //if (colorInfo.Auto == ExcelDefaultableBoolean.False)
            //    XLSXElementBase.AddAttribute(element, ColorElement.AutoAttributeName, XLSXElementBase.GetXmlString(false, DataType.Boolean));
            
            //// want either Theme, Indexed or RGB attribute, not a combination
            //// setting the resolution order to Theme, Indexed, then RGB as none was specified.
            //if (colorInfo.Theme != null)
            //    XLSXElementBase.AddAttribute(element, ColorElement.ThemeAttributeName, XLSXElementBase.GetXmlString(colorInfo.Theme, DataType.Int32));
            //else if (colorInfo.Indexed != null)
            //    XLSXElementBase.AddAttribute(element, ColorElement.IndexedAttributeName, XLSXElementBase.GetXmlString(colorInfo.Indexed, DataType.UInt));
            //else if (colorInfo.RGB != System.Drawing.Color.Empty)
            //{
            //    //  BF 8/14/08
            //    //  The ST_UnsignedIntHex data type has to be an 8-character
            //    //  string representation of the color.
            //    //XLSXElementBase.AddAttribute(element, ColorElement.RGBAttributeName, XLSXElementBase.GetXmlString(colorInfo.RGB.ToArgb(), DataType.Int32));
            //    string attributeValue = XLSXElementBase.GetXmlString(colorInfo.RGB.ToArgb(), DataType.ST_UnsignedIntHex);
            //    XLSXElementBase.AddAttribute(element, ColorElement.RGBAttributeName, attributeValue);
            //}
            //else
			//    Utilities.DebugFail("We shouldn't be serializing the Color element if all attributes are defaults");

            //if (colorInfo.Tint != 0.0)
            //    XLSXElementBase.AddAttribute(element, ColorElement.TintAttributeName, XLSXElementBase.GetXmlString(colorInfo.Tint, DataType.Double));
            #endregion Commented out

            this.SaveColorElement(colorInfo, manager, element, ref value);
        }

        #endregion Save

        #endregion Base Class Overrides

        #region Protected Methods

        #region SaveColorElement

        /// <summary>
        /// Handles the writing of the Color attributes based on the provided ColorInfo object
        /// </summary>
        /// <param name="colorInfo">The ColorInfo object for the owning element (ColorElement, BgColorElement, FgColorElement)</param>
        /// <param name="manager">The owning Excel2007WorkbookSerializationManager</param>
        /// <param name="element">The owning XML element</param>
        /// <param name="value">The value parameter from the Save() method, passed in case its needed in the future</param>
        protected void SaveColorElement(ColorInfo colorInfo, Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 1/16/12 - 12.1 - Cell Format Updates
			// This is wrong. We should be writing out the value if it is True, not False.
			//if (colorInfo.Auto == ExcelDefaultableBoolean.False)
			//    XLSXElementBase.AddAttribute(element, ColorElement.AutoAttributeName, XLSXElementBase.GetXmlString(false, DataType.Boolean));
			if (colorInfo.Auto == ExcelDefaultableBoolean.True)
			{
				XLSXElementBase.AddAttribute(element, ColorElement.AutoAttributeName, XLSXElementBase.GetXmlString(true, DataType.Boolean));
				return;
			}

            // want either Theme, Indexed or RGB attribute, not a combination
            // setting the resolution order to Theme, Indexed, then RGB as none was specified.
            if (colorInfo.Theme != null)
                XLSXElementBase.AddAttribute(element, ColorElement.ThemeAttributeName, XLSXElementBase.GetXmlString((int)colorInfo.Theme, DataType.Int32));
            else if (colorInfo.Indexed != null)
                XLSXElementBase.AddAttribute(element, ColorElement.IndexedAttributeName, XLSXElementBase.GetXmlString(colorInfo.Indexed, DataType.UInt));
            else if (colorInfo.RGB != Utilities.ColorEmpty)
            {
                //  BF 8/14/08
                //  The ST_UnsignedIntHex data type has to be an 8-character
                //  string representation of the color.
                //XLSXElementBase.AddAttribute(element, ColorElement.RGBAttributeName, XLSXElementBase.GetXmlString(colorInfo.RGB.ToArgb(), DataType.Int32));
                string attributeValue = XLSXElementBase.GetXmlString(Utilities.ColorToArgb(colorInfo.RGB), DataType.ST_UnsignedIntHex);
                XLSXElementBase.AddAttribute(element, ColorElement.RGBAttributeName, attributeValue);
            }
            else
                Utilities.DebugFail("We shouldn't be serializing the Color element if all attributes are defaults");

            if (colorInfo.Tint != 0.0)
                XLSXElementBase.AddAttribute(element, ColorElement.TintAttributeName, XLSXElementBase.GetXmlString(colorInfo.Tint, DataType.Double));

        }

        #endregion SaveColorElement

        #endregion Protected Methods

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			XmlWriter writer,
			WorkbookFontData fontData)
		{
			writer.WriteStartElement(ColorElement.LocalName);

			// MD 1/17/12 - 12.1 - Cell Format Updates
			//writer.WriteAttributeString(ColorElement.RGBAttributeName, XLSXElementBase.GetXmlString(Utilities.ColorToArgb(fontData.Color), DataType.ST_UnsignedIntHex));
			WorkbookColorInfo colorInfo = fontData.ColorInfo;
			if (colorInfo.IsAutomatic)
			{
				writer.WriteAttributeString(ColorElement.AutoAttributeName, XLSXElementBase.GetXmlString(colorInfo.IsAutomatic, DataType.Boolean));
			}
			else
			{
				if (colorInfo.ThemeColorType.HasValue)
				{
					writer.WriteAttributeString(ColorElement.ThemeAttributeName, XLSXElementBase.GetXmlString((int)colorInfo.ThemeColorType.Value, DataType.Int32));
				}
				else if (colorInfo.Color.HasValue)
				{
					writer.WriteAttributeString(ColorElement.RGBAttributeName, XLSXElementBase.GetXmlString(Utilities.ColorToArgb(colorInfo.Color.Value), DataType.ST_UnsignedIntHex));
				}
				else
				{
					Utilities.DebugFail("We shouldn't be serializing the Color element if all attributes are defaults");
					writer.WriteAttributeString(ColorElement.IndexedAttributeName, XLSXElementBase.GetXmlString(WorkbookColorPalette.AutomaticColor, DataType.UInt));
				}

				if (colorInfo.Tint.HasValue)
					writer.WriteAttributeString(ColorElement.TintAttributeName, XLSXElementBase.GetXmlString(colorInfo.Tint.Value, DataType.Double));
			}

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