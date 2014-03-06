using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class RichTextRunPropertiesElement : XLSXElementBase 
    {
        #region XML Schema Fragment

        //<complexType name="CT_RPrElt"> 
        //  <choice maxOccurs="unbounded"> 
            //  <element name="rFont" type="CT_FontName" minOccurs="0" maxOccurs="1"/> 
            //  <element name="charset" type="CT_IntProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="family" type="CT_IntProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="b" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="i" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="strike" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="outline" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="shadow" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="condense" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="extend" type="CT_BooleanProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="color" type="CT_Color" minOccurs="0" maxOccurs="1"/> 
            //  <element name="sz" type="CT_FontSize" minOccurs="0" maxOccurs="1"/> 
            //  <element name="u" type="CT_UnderlineProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="vertAlign" type="CT_VerticalAlignFontProperty" minOccurs="0" maxOccurs="1"/> 
            //  <element name="scheme" type="CT_FontScheme" minOccurs="0" maxOccurs="1"/> 
        //  </choice> 
        //</complexType>  

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "rPr";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            RichTextRunPropertiesElement.LocalName;

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.rPr; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/29/12 - 12.1 - Cell Format Updates
			// Move this from RichTextRunElement so we only create runs when we need to.
			FormattedStringElement formattedStringElement = manager.ContextStack[typeof(FormattedStringElement)] as FormattedStringElement;
			if (formattedStringElement == null)
			{
				Utilities.DebugFail("Could not get the formatted string element from the context stack");
				return;
			}

			int startPosition = 0;
			string formattedValue = formattedStringElement.UnformattedString;
			if (formattedValue != null)
				startPosition = formattedStringElement.UnformattedString.Length;

			FormattedStringRun run = new FormattedStringRun(formattedStringElement, startPosition);
			formattedStringElement.FormattingRuns.Add(run);
			manager.ContextStack.Push(run.GetFontInternal(manager.Workbook));

			// Push an empty ColorInfo onto the context stack so it can be populated later
			ColorInfo info = new ColorInfo();
			manager.ContextStack.Push(info);
        }

        #endregion Load

		// MD 1/29/12 - 12.1 - Cell Format Updates
		// Move this from RichTextRunElement so we only create runs when we need to.
		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			ColorInfo colorInfo = (ColorInfo)manager.ContextStack[typeof(ColorInfo)];
			if (colorInfo == null)
			{
				Utilities.DebugFail("For some reason, the ColorInfo was removed from the context stack");
				return;
			}

			WorkbookFontProxy font = (WorkbookFontProxy)manager.ContextStack[typeof(WorkbookFontProxy)];

			if (font == null)
			{
				Utilities.DebugFail("Could not get the font data from the context stack");
				return;
			}

			if (colorInfo.IsDefault == false)
				font.ColorInfo = colorInfo.ResolveColorInfo(manager);

			IWorkbookFontDefaultsResolver fontDefaultsResolver = (IWorkbookFontDefaultsResolver)manager.ContextStack[typeof(IWorkbookFontDefaultsResolver)];
			font.SetFontFormatting(font.Element.ResolvedFontData(fontDefaultsResolver));
		}

		#endregion //OnAfterLoadChildElements

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
            //FormattedString formattedString = manager.ContextStack[typeof(FormattedString)] as FormattedString;
			StringElement formattedString = manager.ContextStack[typeof(StringElement)] as StringElement;
            if (formattedString == null)
            {   
                Utilities.DebugFail("Could not get the FormattedString from the context stack");
                return;
            }

            IWorkbookFont fontData = (IWorkbookFont)manager.ContextStack[typeof(IWorkbookFont)];
            if (fontData == null)
            {
                Utilities.DebugFail("Could not get the font off of the context stack");
                return;
            }

            // Add the 'rFont' element
            if (fontData.Name != null && fontData.Name.Length > 0)
                XmlElementBase.AddElement(element, FontNameElement.QualifiedName);

            // Add the 'family' element
            XmlElementBase.AddElement(element, FamilyElement.QualifiedName);

            // Add the 'b' element
            if (fontData.Bold == ExcelDefaultableBoolean.True)
                XmlElementBase.AddElement(element, BoldElement.QualifiedName);

            // Add the 'i' element
            if (fontData.Italic == ExcelDefaultableBoolean.True)
                XmlElementBase.AddElement(element, ItalicElement.QualifiedName);

            // Add the 'strike' element
            if (fontData.Strikeout == ExcelDefaultableBoolean.True)
                XmlElementBase.AddElement(element, StrikeThroughElement.QualifiedName);

            // Add the 'color' element
            ColorInfo colorInfo = (ColorInfo)manager.ContextStack[typeof(ColorInfo)];
            if (colorInfo != null && colorInfo.IsDefault == false)
                XmlElementBase.AddElement(element, ColorElement.QualifiedName);

            // Add the 'sz' element            
            XmlElementBase.AddElement(element, FontSizeElement.QualifiedName);

            // Add the 'u' element
            if (fontData.UnderlineStyle != FontUnderlineStyle.None && fontData.UnderlineStyle != FontUnderlineStyle.Default)
                XmlElementBase.AddElement(element, UnderlineElement.QualifiedName);

            // Add the 'vertAlign' element
            if (fontData.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.None &&
                fontData.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default)
            {
                XmlElementBase.AddElement(element, VertAlignElement.QualifiedName);
            }

            
            // We don't currently use this element since we do not support themes, but should we do so, we will
            // need to properly handle this element, since serializing 'minor' or 'major' will cause the font to 
            // be pulled from the themes.xml file instead of what we have serialized.
            //
            //XmlElementBase.AddElement(element, SchemeElement.QualifiedName);
        }
        #endregion Save

        #endregion Base class overrides

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			XmlWriter writer,
			WorkbookFontData fontData)
		{
			writer.WriteStartElement(RichTextRunPropertiesElement.LocalName);

			// Add the 'rFont' element
			if (fontData.Name != null && fontData.Name.Length > 0)
				FontNameElement.SaveDirectHelper(writer, fontData.Name);

			// Add the 'family' element
			FamilyElement.SaveDirectHelper(writer);

			// Add the 'b' element
			if (fontData.Bold == ExcelDefaultableBoolean.True)
				BoldElement.SaveDirectHelper(writer);

			// Add the 'i' element
			if (fontData.Italic == ExcelDefaultableBoolean.True)
				ItalicElement.SaveDirectHelper(writer);

			// Add the 'strike' element
			if (fontData.Strikeout == ExcelDefaultableBoolean.True)
				StrikeThroughElement.SaveDirectHelper(writer);

			// Add the 'color' element
			// MD 1/17/12 - 12.1 - Cell Format Updates
			//if (Utilities.ColorIsEmpty(fontData.Color) == false)
			if (fontData.ColorInfo != null)
				ColorElement.SaveDirectHelper(writer, fontData);

			// Add the 'sz' element       
			FontSizeElement.SaveDirectHelper(writer, fontData);

			// Add the 'u' element
			if (fontData.UnderlineStyle != FontUnderlineStyle.None && fontData.UnderlineStyle != FontUnderlineStyle.Default)
				UnderlineElement.SaveDirectHelper(writer, fontData.UnderlineStyle);

			// Add the 'vertAlign' element
			if (fontData.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.None && fontData.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default)
				VertAlignElement.SaveDirectHelper(writer, fontData.SuperscriptSubscriptStyle);

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