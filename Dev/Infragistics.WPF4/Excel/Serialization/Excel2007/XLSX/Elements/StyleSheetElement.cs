using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class StyleSheetElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "styleSheet";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            StyleSheetElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.styleSheet; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
        }

        #endregion Load

		// MD 1/17/11 - TFS62014
		// Because some of the cell formats depend on custom color palette data, which is defined after the <xf> elements,
		// we need to defer the resolving of FormatDataObject objects until after the entire sheet is loaded. When the sheet is
		// done loading, this method will be called, so we will resolve the formats and styles here.
		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			// MD 1/19/11 - TFS62268
			StyleSheetElement.ParseFontInfos(manager);

			StyleSheetElement.ParseFormatInfos(manager, manager.CellStyleXfs);

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved this below because we need the style to be loaded before we load the cell formats.
			//StyleSheetElement.ParseFormatInfos(manager, manager.CellXfs);

			foreach (StyleInfo styleInfo in manager.CellStyles)
			{
				if (styleInfo.CellStyleXfId > -1 &&
					styleInfo.CellStyleXfId < manager.CellStyleXfs.Count)
				{
					// MD 1/8/12 - 12.1 - Cell Format Updates
					#region Old Code

					//if (styleInfo.BuiltinId >= 0)
					//{
					//    WorkbookBuiltInStyle workbookStyle = new WorkbookBuiltInStyle(manager.Workbook, manager.CellStyleXfs[styleInfo.CellStyleXfId].FormatDataObject, (BuiltInStyleType)styleInfo.BuiltinId, Convert.ToByte(styleInfo.OutlineStyle));

					//    // MD 12/28/11 - 12.1 - Table Support
					//    workbookStyle.IsCustomized = styleInfo.CustomBuiltin;

					//    manager.Workbook.Styles.Add(workbookStyle);
					//}
					//else
					//{
					//    manager.Workbook.Styles.AddUserDefinedStyle(manager.CellStyleXfs[styleInfo.CellStyleXfId].FormatDataObject, styleInfo.Name);
					//}

					#endregion // Old Code
					WorksheetCellFormatData formatData = manager.CellStyleXfs[styleInfo.CellStyleXfId].FormatDataObject;

					// MD 2/17/12 - TFS102210
					// If the producer of the file failed to write out the BuiltinId, we should try to see if the name is a 
					// built in name and provide the BuiltinId if it is.
					WorkbookStyleCollection.BuiltInStyleInfo info;
					if (styleInfo.BuiltinId < 0 && WorkbookStyleCollection.StyleTypesByName.TryGetValue(styleInfo.Name, out info))
					{
						Utilities.DebugFail("The style should have been written out as a built in style.");
						styleInfo.BuiltinId = (int)info.Type;
						styleInfo.OutlineStyle = info.OutlineLevel;
					}

					WorkbookStyle style;
					if (styleInfo.BuiltinId >= 0)
					{
						WorkbookBuiltInStyle workbookStyle = new WorkbookBuiltInStyle(manager.Workbook, formatData, (BuiltInStyleType)styleInfo.BuiltinId, Convert.ToByte(styleInfo.OutlineStyle));
						workbookStyle.IsCustomized = styleInfo.CustomBuiltin;

						if (styleInfo.Hidden)
							style = manager.Workbook.Styles.AddHiddenStyle(workbookStyle);
						else
							style = manager.Workbook.Styles.Add(workbookStyle);
					}
					else
					{
						Debug.Assert(styleInfo.Hidden == false, "We were not expecting user defined styles to be hidden.");
						style = manager.Workbook.Styles.AddUserDefinedStyle(formatData, styleInfo.Name);
					}

					// MD 4/4/12
					// Found while fixing TFS100966 
					// Apparently, two styles can share an xfId and when they do, cells will use the first style.
					//Debug.Assert(manager.StylesByCellStyleXfId.ContainsKey(styleInfo.CellStyleXfId) == false, "A style with this xfid should not exist already.");
					//manager.StylesByCellStyleXfId[styleInfo.CellStyleXfId] = style;
					if (manager.StylesByCellStyleXfId.ContainsKey(styleInfo.CellStyleXfId) == false)
						manager.StylesByCellStyleXfId.Add(styleInfo.CellStyleXfId, style);
				}
			}

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved this from above because we need the style to be loaded before we load the cell formats.
			StyleSheetElement.ParseFormatInfos(manager, manager.CellXfs);
		}

		#endregion // OnAfterLoadChildElements

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 1/23/09 - TFS12619
			#region numFmts element

			if ( manager.NumberFormats.Count > 0 )
				XLSXElementBase.AddElement( element, NumFmtsElement.QualifiedName );

			#endregion numFmts element

            #region fonts element

            XLSXElementBase.AddElement(element, FontsElement.QualifiedName);

            #endregion fonts element

            #region fills element

            XLSXElementBase.AddElement(element, FillsElement.QualifiedName);

            #endregion fills element

            #region borders element

            XLSXElementBase.AddElement(element, BordersElement.QualifiedName);

            #endregion borders element

            #region cellStyleXfs element

            XLSXElementBase.AddElement(element, CellStyleXfsElement.QualifiedName);

            #endregion cellStyleXfs element

            #region cellXfs element

            XLSXElementBase.AddElement(element, CellXfsElement.QualifiedName);

            #endregion cellXfs element

            #region cellStyles element

            XLSXElementBase.AddElement(element, CellStylesElement.QualifiedName);

            #endregion cellStyles element

            #region dxfs element

			// MD 12/30/11 - 12.1 - Table Support
            XLSXElementBase.AddElement(element, DxfsElement.QualifiedName);

            #endregion dxfs element

            #region tableStyles element

			// MD 12/30/11 - 12.1 - Table Support
            XLSXElementBase.AddElement(element, TableStylesElement.QualifiedName);

            #endregion tableStyles element

            #region colors element

			// MD 1/23/12 - 12.1 - Cell Format Updates
            //if (manager.IndexedColors.Count > 0)
			if (manager.Workbook.Palette.IsCustom)
                XLSXElementBase.AddElement(element, ColorsElement.QualifiedName);

            #endregion colors element
        }

        #endregion Save

        #endregion Base Class Overrides

		// MD 1/19/11 - TFS62268
		#region ParseFontInfos

		private static void ParseFontInfos(Excel2007WorkbookSerializationManager manager)
		{
			for (int i = 0; i < manager.FontColorInfos.Count; i++)
			{
				ColorInfo fontColorInfo = manager.FontColorInfos[i];
				WorkbookFontData fontData = fontColorInfo.FontDataObject;

				// MD 1/17/12 - 12.1 - Cell Format Updates
				////if (colorInfo.Auto != null)
				////    //set something
				//if (fontColorInfo.Indexed != null)
				//    fontData.ColorIndex = (int)fontColorInfo.Indexed;

				// MD 4/29/11 - TFS73906
				// There is no longer an overload which takes an isForFont parameter.
				//fontData.Color = fontColorInfo.ResolveColor(manager, true);
				// MD 1/17/12 - 12.1 - Cell Format Updates
				//fontData.Color = fontColorInfo.ResolveColor(manager);
				fontData.ColorInfo = fontColorInfo.ResolveColorInfo(manager);

				if (manager.Fonts.Count == 0)
					manager.Workbook.Fonts.DefaultElement = fontData;

				manager.Fonts.Add(fontData);

				// MD 9/15/08 - Excel 2007 Format
				// All properties should be resolved when loading
				// MD 1/17/12 - 12.1 - Cell Format Updates
				// We will now resolve these when we apply them to formats
				//fontData.SetFontFormatting(fontData.ResolvedFontData());
			}
		} 

		#endregion // ParseFontInfos

		// MD 1/17/11 - TFS62014
		#region ParseFormatInfo

		private static void ParseFormatInfos(Excel2007WorkbookSerializationManager manager, List<FormatInfo> formatInfos)
		{
			// MD 1/9/12 - 12.1 - Cell Format Updates
			// Keep track of whether we are creating a style or cell format.
			bool isStylesCollection = (formatInfos == manager.CellStyleXfs);

			for (int i = 0; i < formatInfos.Count; i++)
			{
				FormatInfo formatInfo = formatInfos[i];

				// MD 1/9/12 - 12.1 - Cell Format Updates
				//formatInfo.FormatDataObject = formatInfo.CreateWorksheetCellFormatData(manager);
				formatInfo.FormatDataObject = formatInfo.CreateWorksheetCellFormatData(manager, isStylesCollection);

				// MD 2/27/12 - 12.1 - Table Support
				// This is set when the format object is created.
				//formatInfo.FormatDataObject.IsStyle = isStylesCollection;

				// MD 1/9/12 - 12.1 - Cell Format Updates
				// The first cell format is the default element.
				//if (manager.Formats.Count == 0)
				if (isStylesCollection == false && i == 0)
					manager.Workbook.CellFormats.DefaultElement = formatInfo.FormatDataObject;

				// MD 1/10/12 - 12.1 - Cell Format Updates
				// We don't use the Formats collection when writing out Excel 2007 files.
				//manager.Formats.Add(formatInfo.FormatDataObject);
			}
		}

		#endregion // ParseFormatInfo
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