using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class DxfElement : XLSXElementBase
    {
		#region XML Schema Fragment

		//<complexType name="CT_Dxf">
		//    <sequence>
		//        <element name="font" type="CT_Font" minOccurs="0" maxOccurs="1"/>
		//        <element name="numFmt" type="CT_NumFmt" minOccurs="0" maxOccurs="1"/>
		//        <element name="fill" type="CT_Fill" minOccurs="0" maxOccurs="1"/>
		//        <element name="alignment" type="CT_CellAlignment" minOccurs="0" maxOccurs="1"/>
		//        <element name="border" type="CT_Border" minOccurs="0" maxOccurs="1"/>
		//        <element name="protection" type="CT_CellProtection" minOccurs="0" maxOccurs="1"/>
		//        <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//    </sequence>
		//</complexType>

		#endregion // XML Schema Fragment

        #region Constants

        public const string LocalName = "dxf";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            DxfElement.LocalName;

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.dxf; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            DxfInfo dxfInfo = new DxfInfo();
            ChildDataItem childDataItem = new ChildDataItem(dxfInfo);
            manager.ContextStack.Push(childDataItem);

            ListContext<DxfInfo> listContext = (ListContext<DxfInfo>)manager.ContextStack[typeof(ListContext<DxfInfo>)];
            if (listContext != null)
            {
                listContext.AddItem(dxfInfo);
            }

            
        }

        #endregion Load

        #region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			ListContext<WorksheetCellFormatData> dxfs = (ListContext<WorksheetCellFormatData>)manager.ContextStack[typeof(ListContext<WorksheetCellFormatData>)];
			if (dxfs == null)
			{
				Utilities.DebugFail("Cannot find the ListContext<WorksheetCellFormatData> on the context stack.");
				return;
			}

			WorksheetCellFormatData dxf = dxfs.ConsumeCurrentItem();
			DxfInfo dxfInfo = new DxfInfo(manager, dxf);

			manager.ContextStack.Push(dxfInfo);

			if (dxfInfo.Font != null)
			{
				manager.ContextStack.Push(dxfInfo.Font);
				XLSXElementBase.AddElement(element, FontElement.QualifiedName);
			}
			if (dxfInfo.NumberFormat != null)
			{
				manager.ContextStack.Push(dxfInfo.NumberFormat);
				XLSXElementBase.AddElement(element, NumFmtElement.QualifiedName);
			}
			if (dxfInfo.Fill != null)
			{
				manager.ContextStack.Push(dxfInfo.Fill);
				XLSXElementBase.AddElement(element, FillElement.QualifiedName);
			}
			if (dxfInfo.Alignment != null)
			{
				manager.ContextStack.Push(dxfInfo.Alignment);
				XLSXElementBase.AddElement(element, AlignmentElement.QualifiedName);
			}
			if (dxfInfo.Border != null)
			{
				manager.ContextStack.Push(dxfInfo.Border);
				XLSXElementBase.AddElement(element, BorderElement.QualifiedName);
			}
			if (dxfInfo.Protection != null)
			{
				manager.ContextStack.Push(dxfInfo.Protection);
				XLSXElementBase.AddElement(element, ProtectionElement.QualifiedName);
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