using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class ColorsElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "colors";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ColorsElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.colors; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            // nothing to load. indexedColors element will place the list on the contextStack
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 1/23/12 - 12.1 - Cell Format Updates
			// We already did this check when determining whether to write out the colors element
			//if (manager.IndexedColors.Count > 0)
			//    XLSXElementBase.AddElement(element, IndexedColorsElement.QualifiedName);
			XLSXElementBase.AddElement(element, IndexedColorsElement.QualifiedName);
        }

        #endregion Save

		// MD 1/23/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//#region OnAfterLoadChildElements

		//protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		//{
		//    if ( manager.IndexedColors.Count > 0 )
		//    {
		//        manager.Workbook.Palette.PaletteMode = WorkbookPaletteMode.CustomPalette;
		//        for ( int i = WorkbookColorCollection.UserPaletteStart; i < manager.IndexedColors.Count; i++ )
		//        {
		//            Color color = manager.IndexedColors[ i ];
		//            manager.Workbook.Palette.AddInternal( i - WorkbookColorCollection.UserPaletteStart, color );
		//        }
		//    }
		//}

		//#endregion OnAfterLoadChildElements

		#endregion // Removed

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