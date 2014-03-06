using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    
    // We don't currently use this element since we do not support themes, but should we do so, we will
    // need to properly handle this element, since serializing 'minor' or 'major' will cause the font to 
    // be pulled from the themes.xml file instead of what we have serialized.
    class SchemeElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "scheme";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            SchemeElement.LocalName;

        public const string ValAttributeName = "val";

        #endregion Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.scheme; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/3/12 - 12.1 - Cell Format Updates
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
                    case SchemeElement.ValAttributeName:
                        ST_FontScheme val = (ST_FontScheme)XmlElementBase.GetAttributeValue(attribute, DataType.ST_FontScheme, ST_FontScheme.none);

						FontCollection fontCollection = null;
						if (val == ST_FontScheme.major)
							fontCollection = manager.MajorFonts;
						else if (val == ST_FontScheme.minor)
							fontCollection = manager.MinorFonts;

						if (fontCollection != null)
						{
							string fontName = fontCollection.GetCurrentFont();

							if (fontName != null)
								font.Name = fontName;
						}

                        break;

                    default:
                        Utilities.DebugFail("Unknown attribute type in the scheme element: " + attributeName);
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
			// MD 2/5/12 - TFS100840
            //XLSXElementBase.AddAttribute(element, SchemeElement.ValAttributeName, XLSXElementBase.GetXmlString("minor", DataType.String));
			IWorkbookFont font = (IWorkbookFont)manager.ContextStack[typeof(IWorkbookFont)];
			if (font != null)
			{
				if (font.Name == WorkbookFontData.DefaultMinorFontName)
					XLSXElementBase.AddAttribute(element, SchemeElement.ValAttributeName, XLSXElementBase.GetXmlString("minor", DataType.String));
				else if (font.Name == WorkbookFontData.DefaultMajorFontName)
					XLSXElementBase.AddAttribute(element, SchemeElement.ValAttributeName, XLSXElementBase.GetXmlString("major", DataType.String));
				else
					XLSXElementBase.AddAttribute(element, SchemeElement.ValAttributeName, XLSXElementBase.GetXmlString("none", DataType.String));
			}
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