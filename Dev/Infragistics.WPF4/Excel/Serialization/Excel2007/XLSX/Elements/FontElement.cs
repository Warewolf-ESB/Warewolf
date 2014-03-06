using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class FontElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "font";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            FontElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.font; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/19/11 - TFS62268
			ListContext<ColorInfo> fontColorInfos = (ListContext<ColorInfo>)manager.ContextStack[typeof(ListContext<ColorInfo>)];

			// MD 12/21/11 - 12.1 - Table Support
			// This may be null now.
			//Debug.Assert(fontColorInfos != null, "Couldn't find the FontColorInfos collection on the context stack");

            WorkbookFontData fontData = new WorkbookFontData(manager.Workbook);
            manager.ContextStack.Push(fontData);

            // considering dxf elements as unsupported
            //// handles when the parent element is a dxf
            //ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
            //if (childDataItem != null)
            //{
            //    // handle dxf elements
            //    DxfInfo dxfInfo = childDataItem.Data as DxfInfo;
            //    if (dxfInfo != null)
            //        dxfInfo.Font = fontData;
            //}

            ColorInfo colorInfo = new ColorInfo();

			// MD 1/19/11 - TFS62268
			// Store the font data on the color info for now and add the color info to the collection.
			colorInfo.FontDataObject = fontData;
			if (fontColorInfos != null)
				fontColorInfos.AddItem(colorInfo);

            manager.ContextStack.Push(colorInfo);

			// MD 12/21/11 - 12.1 - Table Support
			ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			if (childDataItem != null)
			{
				DxfInfo dxfInfo = childDataItem.Data as DxfInfo;
				if (dxfInfo != null)
				{
					dxfInfo.FontColorInfo = colorInfo;
					dxfInfo.Font = fontData;
					return;
				}
			}
        }

        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            IWorkbookFont fontData = null;

            ListContext<WorkbookFontData> listContext = manager.ContextStack[typeof(ListContext<WorkbookFontData>)] as ListContext<WorkbookFontData>;
            if (listContext != null)
                fontData = listContext.ConsumeCurrentItem() as IWorkbookFont;

            if (fontData == null)
                fontData = (IWorkbookFont)manager.ContextStack[typeof(IWorkbookFont)];

            if (fontData != null)
            {
                if (fontData.Bold == ExcelDefaultableBoolean.True)
                    XLSXElementBase.AddElement(element, BoldElement.QualifiedName);

                if (fontData.UnderlineStyle != FontUnderlineStyle.Default &&
                    fontData.UnderlineStyle != FontUnderlineStyle.None)
                    XLSXElementBase.AddElement(element, UnderlineElement.QualifiedName);

                if (fontData.Italic == ExcelDefaultableBoolean.True)
                    XLSXElementBase.AddElement(element, ItalicElement.QualifiedName);

                if (fontData.Strikeout == ExcelDefaultableBoolean.True)
                    XLSXElementBase.AddElement(element, StrikeThroughElement.QualifiedName);

                if (fontData.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.Default &&
                    fontData.SuperscriptSubscriptStyle != FontSuperscriptSubscriptStyle.None)
                    XLSXElementBase.AddElement(element, VertAlignElement.QualifiedName);

                XLSXElementBase.AddElement(element, FontSizeElement.QualifiedName);

				// MD 1/17/12 - 12.1 - Cell Format Updates
				#region Old Code

				////create ColorInfo object and throw it on the ContextStack
				//ColorInfo colorInfo = new ColorInfo();
				//WorkbookFontData workbookFontData = fontData as WorkbookFontData;
				//if (workbookFontData != null)
				//{
				//    // MD 1/19/11 - TFS62268
				//    // We shouldn't use negative values for the font color index.
				//    //if (workbookFontData.ColorIndex != 0x7FFF)
				//    if (workbookFontData.ColorIndex != 0x7FFF && workbookFontData.ColorIndex >= 0)
				//        colorInfo.Indexed = Convert.ToUInt32(workbookFontData.ColorIndex);
				//    else
				//        colorInfo.Indexed = null;
				//}
				////if (fontData.Color == System.Drawing.SystemColors.WindowText)
				////    fontData.Color = System.Drawing.Color.Empty;

				//// MD 1/8/12 - 12.1 - Cell Format Updates
				//// Rewrote this to work the way excel does.
				////if (fontData.Color != Utilities.ColorEmpty)
				////    colorInfo.RGB = fontData.Color;
				////
				////manager.ContextStack.Push(fontData);
				////manager.ContextStack.Push(colorInfo);
				////XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
				//manager.ContextStack.Push(fontData);

				//if (fontData.Color != Utilities.ColorEmpty)
				//{
				//    colorInfo.RGB = fontData.Color;
				//    manager.ContextStack.Push(colorInfo);
				//    XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
				//}

				//XLSXElementBase.AddElement(element, NameElement.QualifiedName);
				//XLSXElementBase.AddElement(element, FamilyElement.QualifiedName);

				//// TODO: Roundtrip / Theme Support
				//// We don't currently use this element since we do not support themes, but should we do so, we will
				//// need to properly handle this element, since serializing 'minor' or 'major' will cause the font to 
				//// be pulled from the themes.xml file instead of what we have serialized.
				////
				////XLSXElementBase.AddElement(element, SchemeElement.QualifiedName);

				#endregion // Old Code
				manager.ContextStack.Push(fontData);
				if (fontData.ColorInfo != null)
				{
					ColorInfo colorInfo = ColorInfo.CreateColorInfo(manager, fontData.ColorInfo, ColorableItem.CellFont);
					manager.ContextStack.Push(colorInfo);
					XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
				}

				XLSXElementBase.AddElement(element, NameElement.QualifiedName);
				XLSXElementBase.AddElement(element, FamilyElement.QualifiedName);

				// MD 2/5/12 - TFS100840
				if (fontData.Name == WorkbookFontData.DefaultMinorFontName || fontData.Name == WorkbookFontData.DefaultMajorFontName)
					XLSXElementBase.AddElement(element, SchemeElement.QualifiedName);
            }
        }

        #endregion Save

		// MD 1/19/11 - TFS62268
		// We cannot resolve the color of the WorkbookFontData instances immediately after the <font> element is loaded because it may depend 
		// on indexed colors from a custom color palette. Those custom colors are defined at the bottom of the styles.xml file, after all the 
		// <font> elements. So this step must be deferred until the entire file is loaded. It has now been moved to StyleSheetElement.ParseFontInfos,
		// which is called from StyleSheetElement.OnAfterLoadChildElements, after the entire sheet is loaded.
		//#region OnAfterLoadChildElements
		//
		//protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		//{
		//    WorkbookFontData fontData = (WorkbookFontData)manager.ContextStack[typeof(WorkbookFontData)];
		//    if (fontData == null)
		//    {
		//        Utilities.DebugFail("For some reason, the WorkbookFontData was removed from the context stack");
		//        return;
		//    }
		//
		//    ColorInfo colorInfo = (ColorInfo)manager.ContextStack[ typeof(ColorInfo) ];
		//
		//    if (colorInfo == null)
		//    {
		//        Utilities.DebugFail("For some reason, the ColorInfo was removed from the context stack");
		//        return;
		//    }
		//
		//    //TODO: what do these related to?
		//    // ROUNDTRIP p 1950
		//
		//    //if (colorInfo.Auto != null)
		//    //    //set something
		//    if (colorInfo.Indexed != null)
		//        fontData.ColorIndex = (int)colorInfo.Indexed;
		//    fontData.Color = colorInfo.ResolveColor( manager, true );
		//
		//    if (manager.Fonts.Count == 0)
		//        manager.Workbook.Fonts.DefaultElement = fontData;
		//
		//    //TODO: don't know if the font for a dxf should be added to the manager.Fonts collection; if not we can process like the FillElement
		//    manager.Fonts.Add(fontData);
		//
		//    // MD 9/15/08 - Excel 2007 Format
		//    // All properties should be resolved when loading
		//    fontData.SetFontFormatting( fontData.ResolvedFontData() );
		//}
		//
		//#endregion OnAfterLoadChildElements

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