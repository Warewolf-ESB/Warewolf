using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{

    internal class IndexedColorsElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "indexedColors";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            IndexedColorsElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.indexedColors; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/23/12 - 12.1 - Cell Format Updates
            //ListContext<Color> listContext = new ListContext<Color>(manager.IndexedColors);
			List<Color> indexedColors = new List<Color>();
			ListContext<Color> listContext = new ListContext<Color>(indexedColors);
            manager.ContextStack.Push(listContext);
        }

        #endregion Load

		// MD 1/23/12 - 12.1 - Cell Format Updates
		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			ListContext<Color> listContext = (ListContext<Color>)manager.ContextStack[typeof(ListContext<Color>)];
			IList<Color> indexedColors = listContext.List;
			Debug.Assert(indexedColors.Count == WorkbookColorPalette.UserPaletteSize + WorkbookColorPalette.UserPaletteStart, "Incorrect number of indexed colors.");

			WorkbookColorPalette palette = manager.Workbook.Palette;
			int count = Math.Min(indexedColors.Count - WorkbookColorPalette.UserPaletteStart, WorkbookColorPalette.UserPaletteSize);
			for (int i = WorkbookColorPalette.UserPaletteStart; i < indexedColors.Count; i++)
			{
				palette[i - WorkbookColorPalette.UserPaletteStart] = indexedColors[i];
			}
		}

		#endregion // OnAfterLoadChildElements

		#region Save

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
        {
			// MD 1/23/12 - 12.1 - Cell Format Updates
			//List<Color> indexedColors = manager.IndexedColors;
			//
			//manager.ContextStack.Push(new ListContext<Color>(indexedColors));
			//XLSXElementBase.AddElements( element, RgbColorElement.QualifiedName, indexedColors.Count );
			WorkbookColorPalette palette = manager.Workbook.Palette;
			List<Color> indexedColors = new List<Color>();
			for (int i = 0; i < WorkbookColorPalette.UserPaletteStart + WorkbookColorPalette.UserPaletteSize; i++)
				indexedColors.Add(palette.GetColorAtAbsoluteIndex(i));

			manager.ContextStack.Push(new ListContext<Color>(indexedColors));
			XLSXElementBase.AddElements(element, RgbColorElement.QualifiedName, indexedColors.Count);
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