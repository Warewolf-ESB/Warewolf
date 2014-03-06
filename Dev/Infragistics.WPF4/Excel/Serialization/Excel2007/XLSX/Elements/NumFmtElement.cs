using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class NumFmtElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "numFmt";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            NumFmtElement.LocalName;

        public const string FormatCodeAttributeName = "formatCode";
        public const string NumFmtIdAttributeName = "numFmtId";

        #endregion Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.numFmt; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            NumberFormatInfo numberFormatInfo = new NumberFormatInfo();

            //// handle parent of numfmts
            //ListContext<NumberFormatInfo> listContext = (ListContext<NumberFormatInfo>)manager.ContextStack[typeof(ListContext<NumberFormatInfo>)];
            //if (listContext != null)
            //    listContext.AddItem(numberFormatInfo);

			// MD 2/7/12 - 12.1 - Table Support
			// Cache this above the block below because we may have to translate it.
			int formatId = (int)XLSXElementBase.GetAttributeValue(element.Attributes[NumFmtElement.NumFmtIdAttributeName], DataType.Int32, -1);

            ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
            if (childDataItem != null)
            {
                // handle parent of dxf elements
                DxfInfo dxfInfo = childDataItem.Data as DxfInfo;
				if (dxfInfo != null)
				{
					dxfInfo.NumberFormat = numberFormatInfo;

					// MD 2/7/12 - 12.1 - Table Support
					// There seems to be a bug in Excel where it writes out the wrong number format ids, so translate it before loading.
					formatId = WorkbookFormatCollection.FromDxfIndex(formatId);
				}
            }

            numberFormatInfo.FormatCode = (string)XLSXElementBase.GetAttributeValue(element.Attributes[NumFmtElement.FormatCodeAttributeName], DataType.String, string.Empty);

			// MD 2/7/12 - 12.1 - Table Support
			// Moved this above to we can translate the formatId if this is a dxf format.
            //numberFormatInfo.NumberFormatId = (int)XLSXElementBase.GetAttributeValue(element.Attributes[NumFmtElement.NumFmtIdAttributeName], DataType.Int32, -1);
			numberFormatInfo.NumberFormatId = formatId;

			// MD 11/19/08 - TFS10611
			// We have to allow for a duplicate format to be written in the file. This is similar to TFS6778, which affected the 2003 format.
            //manager.Workbook.Formats.AddFormat(numberFormatInfo.NumberFormatId, numberFormatInfo.FormatCode);
			manager.Workbook.Formats.AddFormat( numberFormatInfo.NumberFormatId, numberFormatInfo.FormatCode, true );
        }

        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 3/4/12 - 12.1 - Table Support
			DxfInfo dxfInfo = (DxfInfo)manager.ContextStack[typeof(DxfInfo)];
			if (dxfInfo != null)
			{
				int formatId = dxfInfo.NumberFormat.NumberFormatId;

				// There seems to be a bug in Excel where it writes out the wrong number format ids, so translate it before saving.
				formatId = WorkbookFormatCollection.ToDxfIndex(formatId);

				XLSXElementBase.AddAttribute(element, NumFmtElement.FormatCodeAttributeName, XLSXElementBase.GetXmlString(dxfInfo.NumberFormat.FormatCode, DataType.String));
				XLSXElementBase.AddAttribute(element, NumFmtElement.NumFmtIdAttributeName, XLSXElementBase.GetXmlString(formatId, DataType.Int32));
				return;
			}

			// MD 1/23/09 - TFS12619
			//.This context might be a collection of a single format, so account for both cases.
            //NumberFormatInfo numberFormatInfo = (NumberFormatInfo)manager.ContextStack[typeof(NumberFormatInfo)];
			ListContext<NumberFormatInfo> listContext = (ListContext<NumberFormatInfo>)manager.ContextStack[ typeof( ListContext<NumberFormatInfo> ) ];
			NumberFormatInfo numberFormatInfo = listContext != null
				? (NumberFormatInfo)listContext.ConsumeCurrentItem()
				: (NumberFormatInfo)manager.ContextStack[ typeof( NumberFormatInfo ) ];

			if (numberFormatInfo != null)
			{
				XLSXElementBase.AddAttribute(element, NumFmtElement.FormatCodeAttributeName, XLSXElementBase.GetXmlString(numberFormatInfo.FormatCode, DataType.String));
				XLSXElementBase.AddAttribute(element, NumFmtElement.NumFmtIdAttributeName, XLSXElementBase.GetXmlString(numberFormatInfo.NumberFormatId, DataType.Int32));
			}
			// MD 2/7/12 - 12.1 - Table Support
			else
			{
				Utilities.DebugFail("Cannot find a NumberFormatInfo on the context stack.");
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