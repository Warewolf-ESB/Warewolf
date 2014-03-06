using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Drawing
{
	internal class RElement : XmlElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_RegularTextRun">
		// <sequence>
		// <element name="rPr" type="CT_TextCharacterProperties" minOccurs="0" maxOccurs="1"/>
		// <element name="t" type="xsd:string" minOccurs="1" maxOccurs="1"/>
		// </sequence>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>r</summary>
		public const string LocalName = "r";
		
		/// <summary>http://schemas.openxmlformats.org/drawingml/2006/main/r</summary>
		public const string QualifiedName =
			DrawingsPart.MainNamespace +
			XmlElementBase.NamespaceSeparator +
			RElement.LocalName;


		#endregion Constants

		#region Base class overrides

			#region ElementName

		public override string ElementName
		{
			get { return RElement.QualifiedName; }
		}

			#endregion ElementName

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            //  Get the instance (which holds the value of the shape's Text property)
            //  off the context stack, and add a FormattedStringRun to it.
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
            //FormattedString fs = manager.ContextStack[typeof(FormattedString)] as FormattedString;
			// MD 11/8/11 - TFS85193
			// We now have new types to deal with formatted strings with paragraphs.
			//FormattedStringElement fs = manager.ContextStack[typeof(FormattedStringElement)] as FormattedStringElement;
			//
			//fs.FormattingRuns.Add( new FormattedStringRun(fs, 0, manager.Workbook) );
			FormattedTextParagraph paragraph = (FormattedTextParagraph)manager.ContextStack[typeof(FormattedTextParagraph)];
			if (paragraph == null)
			{
				Utilities.DebugFail("Cannot find a FormattedTextParagraph on the context stack.");
				return;
			}

			FormattedTextRun run = new FormattedTextRun(paragraph, paragraph.UnformattedString.Length);
			paragraph.AddFormattingRun(run);

			IWorkbookFontDefaultsResolver defaultsResolver = (IWorkbookFontDefaultsResolver)manager.ContextStack[typeof(IWorkbookFontDefaultsResolver)];

			WorkbookFontProxy fontProxy = run.GetFontInternal(manager.Workbook);

			// MD 7/5/12 - TFS115687
			// The default font color is based on the style/fontRef/... element, so try to load that before resolving defaults.
			RElement.InitializeFontColor(manager, manager.ContextStack.Get<WorksheetShape>(), fontProxy);

			WorkbookFontData font = fontProxy.Element.ResolvedFontData(defaultsResolver);
			fontProxy.SetFontFormatting(font);

			manager.ContextStack.Push(run);
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			Workbook workbook = manager.ContextStack[ typeof(Workbook) ] as Workbook;
			string attributeValue = string.Empty;

			

		}

			#endregion Save

		#endregion Base class overrides

		// MD 7/5/12 - TFS115687
		#region InitializeFontColor

		private static void InitializeFontColor(WorkbookSerializationManager manager, WorksheetShape shape, IWorkbookFont font)
		{
			if (shape == null || shape.HasExcel2007ShapeSerializationManager == false)
				return;

			foreach (ElementDataCache element in shape.Excel2007ShapeSerializationManager.Elements)
			{
				if (element.QualifiedElementName != "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing/style")
					continue;

				foreach (ElementDataCache element2 in element.Elements)
				{
					if (element2.QualifiedElementName != "http://schemas.openxmlformats.org/drawingml/2006/main/fontRef")
						continue;

					foreach (ElementDataCache element3 in element2.Elements)
					{
						if (element3.QualifiedElementName != "http://schemas.openxmlformats.org/drawingml/2006/main/schemeClr")
							continue;

						string val;
						if (element3.AttributeValues.TryGetValue("val", out val))
						{
							WorkbookThemeColorType themeType = (WorkbookThemeColorType)(ST_SchemeColorVal)XmlElementBase.GetValue(val, DataType.ST_SchemeColorVal, ST_SchemeColorVal.lt1);
							font.ColorInfo = new WorkbookColorInfo(themeType);
						}
					}
				}
			}
		}

		#endregion // InitializeFontColor
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