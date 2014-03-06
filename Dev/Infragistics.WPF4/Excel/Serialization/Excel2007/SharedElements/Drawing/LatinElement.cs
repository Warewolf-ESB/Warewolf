using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	// MD 11/8/11 - TFS85193
	internal class LatinElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		//<complexType name="CT_TextFont">
		//    <attribute name="typeface" type="ST_TextTypeface"/>
		//    <attribute name="panose" type="ST_Panose" use="optional"/>
		//    <attribute name="pitchFamily" type="xsd:byte" use="optional" default="0"/>
		//    <attribute name="charset" type="xsd:byte" use="optional" default="1"/>
		//</complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>rPr</summary>
		public const string LocalName = "latin";

		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/latin</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			LatinElement.LocalName;

		internal const string TypefaceAttributeName = "typeface";

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return LatinElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			// MD 2/9/12 - TFS89375
			// Moved this below so we can load the font name first, then decide what to apply it to.
			//FormattedTextRun shapeRun = (FormattedTextRun)manager.ContextStack[typeof(FormattedTextRun)];
			//if (shapeRun == null)
			//{
			//    Utilities.DebugFail("Could not get a ShapeFormattingRun in RPrElement.Load.");
			//    return;
			//}
			//
			//WorkbookFontProxy font = shapeRun.GetFontInternal(manager.Workbook);
			string fontName = null;

			object attributeValue = null;
			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case LatinElement.TypefaceAttributeName:
						{
							// MD 1/3/12 - 12.1 - Cell Format Updates
							// The default font name is now exposed off the normal style.
							//attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.String, Workbook.DefaultFontName);
							// MD 2/9/12 - TFS89375
							// Just load the font name here, don't apply it yet.
							//attributeValue = XmlElementBase.GetAttributeValue(
							//    attribute, 
							//    DataType.String, 
							//    manager.Workbook.Styles.NormalStyle.StyleFormatInternal.FontNameResolved);
							//
							//font.Name = (string)attributeValue;
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.String, null);
							fontName = (string)attributeValue;
						}
						break;
				}
			}

			// MD 1/3/12 - 12.1 - Cell Format Updates
			FontCollection fontCollection = (FontCollection)manager.ContextStack[typeof(FontCollection)];
			if (fontCollection != null)
			{
				if (fontName != null)
					fontCollection.DefaultFontName = fontName;
				else
					Utilities.DebugFail("This is unexpected.");

				return;
			}

			FormattedTextRun shapeRun = (FormattedTextRun)manager.ContextStack[typeof(FormattedTextRun)];
			if (shapeRun == null)
			{
				Utilities.DebugFail("Could not get a ShapeFormattingRun in RPrElement.Load.");
				return;
			}

			WorkbookFontProxy font = shapeRun.GetFontInternal(manager.Workbook);

			if (fontName == null)
				fontName = manager.Workbook.Styles.NormalStyle.StyleFormatInternal.FontNameResolved;

			font.Name = fontName;
		}

		#endregion Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
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