using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class TabColorElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        // <complexType name="CT_Color"> 
        //   <attribute name="auto" type="xsd:boolean" use="optional"/> 
        //   <attribute name="indexed" type="xsd:unsignedInt" use="optional"/> 
        //   <attribute name="rgb" type="ST_UnsignedIntHex" use="optional"/> 
        //   <attribute name="theme" type="xsd:unsignedInt" use="optional"/> 
        //   <attribute name="tint" type="xsd:double" use="optional" default="0.0"/> 
        // </complexType> 
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>tabColor</summary>
        public const string LocalName = "tabColor";

        /// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/tabColor</summary>
        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            TabColorElement.LocalName;

        private const string AutoAttributeName = "auto";
        private const string IndexedAttributeName = "indexed";
        private const string RgbAttributeName = "rgb";
        private const string ThemeAttributeName = "theme";
        private const string TintAttributeName = "tint";

        #endregion Constants

        #region Base class overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.tabColor; }
        }

        #endregion Type

        #region Load

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

			// MD 1/26/12 - 12.1 - Cell Format Updates
			#region Old Code

			//Color rgb = Utilities.ColorEmpty;
			//double tint = 0;
			//int tabColorIndex = -1;
			//int theme = -1;
			//bool auto = false;

			//object attributeValue = null;
			//foreach (ExcelXmlAttribute attribute in element.Attributes)
			//{
			//    string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
			//    switch (attributeName)
			//    {
			//        case TabColorElement.AutoAttributeName:
			//            {
			//                attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
			//                auto = (bool)attributeValue;
			//            }
			//            break;

			//        case TabColorElement.IndexedAttributeName:
			//            {
			//                attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
			//                tabColorIndex = (int)attributeValue;
			//            }
			//            break;

			//        case TabColorElement.RgbAttributeName:
			//            {
			//                //  BF 8/14/08
			//                //  I changed the GetValue and GetXmlString methods to handle conversion
			//                //  of the ST_UnsignedIntHex type, so I changed this code to use that.
			//                attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_UnsignedIntHex, 0);
			//                int intValue = Utilities.ToInteger( attributeValue );
			//                rgb = Utilities.ColorFromArgb(intValue);
			//            }
			//            break;

			//        case TabColorElement.ThemeAttributeName:
			//            {
			//                attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
			//                theme = (int)attributeValue;                            
			//            }
			//            break;

			//        case TabColorElement.TintAttributeName:
			//            {
			//                attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Double, 0);
			//                tint = (double)attributeValue;
			//            }
			//            break;
			//    }
			//}

			//WorksheetDisplayOptions options = worksheet.DisplayOptions;

			//// Going to assume that having the theme attribute set will override the rgb attribute.
			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////if (theme > 0 &&
			////    theme <= manager.ThemeColors.Count)
			////    // Retrieve the color from the ThemeColors list. Remember, its a 1-base index.
			////    rgb = manager.ThemeColors[theme - 1];
			//if (theme > 0 && theme <= manager.Workbook.ThemeColors.Length)
			//    rgb = manager.Workbook.ThemeColors[theme - 1];

			//Debug.Assert(!auto || tabColorIndex == -1, "Expected either 'auto' or a TabColorIndex, but not both");
			//if (tabColorIndex > -1)
			//    // Let the explicit index take priority, if both are specified
			//    options.TabColorIndex = tabColorIndex;
			//else if (auto)
			//    // Specify the automatic color for the index
			//    // MD 1/16/12 - 12.1 - Cell Format Updates
			//    //options.TabColorIndex = WorkbookColorCollection.AutomaticColor;
			//    options.TabColorIndex = WorkbookColorPalette.AutomaticColor;

			//if (rgb != Utilities.ColorEmpty)
			//{
			//    Debug.Assert(!auto && tabColorIndex == -1, "Expected to have an RGB without 'auto' or 'tabColorIndex'");
			//    if (tint != 0)
			//        options.TabColor = Utilities.ApplyTint(rgb, tint);
			//    else
			//        options.TabColor = rgb;
			//} 

			#endregion // Old Code
			bool auto = false;
			int? indexed = null;
			Color? rgb = null;
			int? theme = null;
			double? tint = null;

			object attributeValue = null;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
				switch (attributeName)
				{
					case TabColorElement.AutoAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
							auto = (bool)attributeValue;
						}
						break;

					case TabColorElement.IndexedAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
							indexed = (int)attributeValue;
						}
						break;

					case TabColorElement.RgbAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_UnsignedIntHex, 0);
							int intValue = Utilities.ToInteger(attributeValue);
							rgb = Utilities.ColorFromArgb(intValue);
						}
						break;

					case TabColorElement.ThemeAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
							theme = (int)attributeValue;
						}
						break;

					case TabColorElement.TintAttributeName:
						{
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Double, 0);
							tint = (double)attributeValue;
						}
						break;
				}
			}

			WorkbookColorInfo colorInfo;

			if (auto)
			{
				colorInfo = WorkbookColorInfo.Automatic;
			}
			else
			{
				if (rgb.HasValue)
				{
					colorInfo = new WorkbookColorInfo(rgb, null, tint, true);
				}
				else if (theme.HasValue)
				{
					colorInfo = new WorkbookColorInfo(null, (WorkbookThemeColorType)theme.Value, tint, true);
				}
				else if (indexed.HasValue)
				{
					colorInfo = new WorkbookColorInfo(manager.Workbook, indexed.Value);
				}
				else
				{
					Utilities.DebugFail("This is unexpected.");
					colorInfo = WorkbookColorInfo.Automatic;
				}
			}

			worksheet.DisplayOptions.TabColorInfo = colorInfo;
        }

        #endregion Load

        #region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
			if (worksheet == null)
			{
				Utilities.DebugFail("Could not get the worksheet from the context stack");
				return;
			}

			// MD 1/26/12 - 12.1 - Cell Format Updates
			#region Old Code

			//			WorksheetDisplayOptions options = worksheet.DisplayOptions;
			//			string attributeValue = String.Empty;
			//
			//            // Add the 'auto' attribute
			//            //
			//            // TODO: Figure out when this should actually be written out
			//            //
			//            //attributeValue = XmlElementBase.GetXmlString(options.TabColorIndex == WorkbookColorCollection.AutomaticColor, DataType.Boolean, false, false);
			//            //if (attributeValue != null)
			//            //    XmlElementBase.AddAttribute(element, TabColorElement.AutoAttributeName, attributeValue);

			//            // Add the 'rgb' attribute
			//            Color tabColor = options.TabColor;
			//            if (tabColor != Utilities.ColorEmpty)
			//            {
			//                // If the color is a system color, we need to serialize out the index instead 
			//                // of the actual color, since we're don't support themes (where the system
			//                // colors are stored when we read them in)
			//#if !SILVERLIGHT
			//                bool isSystemColor = tabColor.IsSystemColor;
			//#else
			//                bool isSystemColor = false;
			//#endif
			//                if (isSystemColor)
			//                {
			//                    attributeValue = XmlElementBase.GetXmlString(options.TabColorIndex, DataType.UInt);
			//                    XmlElementBase.AddAttribute(element, TabColorElement.IndexedAttributeName, attributeValue);
			//                }
			//                else
			//                {
			//                    //  BF 8/14/08
			//                    //  I changed the GetValue and GetXmlString methods to handle conversion
			//                    //  of the ST_UnsignedIntHex type, so I changed this code to use that.
			//                    attributeValue = XmlElementBase.GetXmlString(tabColor, DataType.ST_UnsignedIntHex);
			//                    XmlElementBase.AddAttribute(element, TabColorElement.RgbAttributeName, attributeValue);
			//                }
			//            }

			//            // Name = 'theme', Type = UInt32, Default = 
			//            // We do not write out this value since we do not support themes.  Instead,
			//            // the RGB value itself is written out.

			//            // Name = 'tint', Type = Double, Default = 0
			//            // We do not write out this value since we do not support themes.  Instead,
			//            // the RGB value itself is written out.

			#endregion // Old Code

			string attributeValue = String.Empty;
			WorkbookColorInfo colorInfo = worksheet.DisplayOptions.TabColorInfo;

			// Add the 'auto' attribute
			if (colorInfo.IsAutomatic)
			{
				attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean);
				if (attributeValue != null)
					XmlElementBase.AddAttribute(element, TabColorElement.AutoAttributeName, attributeValue);
			}
			else if (colorInfo.Color.HasValue)
			{
				Color color = colorInfo.Color.Value;
				bool isSystemColor = Utilities.ColorIsSystem(color);

				if (isSystemColor)
				{
					attributeValue = XmlElementBase.GetXmlString(colorInfo.GetIndex(manager.Workbook, ColorableItem.WorksheetTab), DataType.UInt);
					XmlElementBase.AddAttribute(element, TabColorElement.IndexedAttributeName, attributeValue);
				}
				else
				{
					attributeValue = XmlElementBase.GetXmlString(color, DataType.ST_UnsignedIntHex);
					XmlElementBase.AddAttribute(element, TabColorElement.RgbAttributeName, attributeValue);
				}
			}
			else if (colorInfo.ThemeColorType.HasValue)
			{
				attributeValue = XmlElementBase.GetXmlString((int)colorInfo.ThemeColorType.Value, DataType.Integer);
				XmlElementBase.AddAttribute(element, TabColorElement.ThemeAttributeName, attributeValue);
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
			}

			if (colorInfo.Tint.HasValue)
			{
				attributeValue = XmlElementBase.GetXmlString(colorInfo.Tint.Value, DataType.Double);
				XmlElementBase.AddAttribute(element, TabColorElement.TintAttributeName, attributeValue);
			}
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