using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class AlignmentElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "alignment";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            AlignmentElement.LocalName;

        public const string HorizontalAttributeName = "horizontal";
        public const string IndentAttributeName = "indent";
        public const string JustifyLastLineAttributeName = "justifyLastLine";
        public const string ReadingOrderAttributeName = "readingOrder";
        public const string RelativeIndentAttributeName = "relativeIndent";
        public const string ShrinkToFitAttributeName = "shrinkToFit";
        public const string TextRotationAttributeName = "textRotation";
        public const string VerticalAttributeName = "vertical";
        public const string WrapTextAttributeName = "wrapText";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.alignment; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
            if (childDataItem == null)
            {
                Utilities.DebugFail("Failed to find the ChildDataItem object on the stack.");
                return;
            }

            AlignmentInfo alignmentInfo = new AlignmentInfo();

            // handle xf elements
            FormatInfo formatInfo = childDataItem.Data as FormatInfo;
            if (formatInfo != null)
                formatInfo.Alignment = alignmentInfo;

            // handle dxf elements
            DxfInfo dxfInfo = childDataItem.Data as DxfInfo;
            if (dxfInfo != null)
                dxfInfo.Alignment = alignmentInfo;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case AlignmentElement.HorizontalAttributeName:
                        alignmentInfo.Horizontal = (HorizontalCellAlignment)XLSXElementBase.GetAttributeValue(attribute, DataType.ST_HorizontalAlignment, AlignmentInfo.DEFAULT_HORIZONTAL);
                        break;
                    case AlignmentElement.IndentAttributeName:
                        alignmentInfo.Indent = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, AlignmentInfo.DEFAULT_INDENT);                      
                        break;
                    case AlignmentElement.JustifyLastLineAttributeName:
                        alignmentInfo.JustifyLastLine = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, AlignmentInfo.DEFAULT_JUSTIFYLASTLINE);
                        break;
                    case AlignmentElement.ReadingOrderAttributeName:
                        alignmentInfo.ReadingOrder = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, AlignmentInfo.DEFAULT_READINGORDER);
                        break;
                    case AlignmentElement.RelativeIndentAttributeName:
                        alignmentInfo.RelativeIndent = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, AlignmentInfo.DEFAULT_RELATIVEINDENT);
                        break;
                    case AlignmentElement.ShrinkToFitAttributeName:
                        alignmentInfo.ShrinkToFit = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, AlignmentInfo.DEFAULT_SHRINKTOFIT);
                        break;
                    case AlignmentElement.TextRotationAttributeName:
                        alignmentInfo.TextRotation = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, AlignmentInfo.DEFAULT_TEXTROTATION);
                        break;
                    case AlignmentElement.VerticalAttributeName:
                        alignmentInfo.Vertical = (VerticalCellAlignment)XLSXElementBase.GetAttributeValue(attribute, DataType.ST_VerticalAlignment, AlignmentInfo.DEFAULT_VERTICAL);
                        break;
                    case AlignmentElement.WrapTextAttributeName:
                        alignmentInfo.WrapText = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, AlignmentInfo.DEFAULT_WRAPTEXT);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the alignment element: " + attributeName);
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
            AlignmentInfo alignmentInfo = (AlignmentInfo)manager.ContextStack[typeof(AlignmentInfo)];
            if (alignmentInfo != null)
            {
                if (alignmentInfo.Horizontal != HorizontalCellAlignment.Default &&
                    alignmentInfo.Horizontal != AlignmentInfo.DEFAULT_HORIZONTAL)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.HorizontalAttributeName, XLSXElementBase.GetXmlString(alignmentInfo.Horizontal, DataType.ST_HorizontalAlignment));
                if (alignmentInfo.Indent != AlignmentInfo.DEFAULT_INDENT)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.IndentAttributeName, XLSXElementBase.GetXmlString(alignmentInfo.Indent, DataType.Int32));
                if (alignmentInfo.JustifyLastLine != AlignmentInfo.DEFAULT_JUSTIFYLASTLINE)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.JustifyLastLineAttributeName, XLSXElementBase.GetXmlString(true, DataType.Boolean));
                if (alignmentInfo.ReadingOrder != AlignmentInfo.DEFAULT_READINGORDER)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.ReadingOrderAttributeName, XLSXElementBase.GetXmlString(alignmentInfo.ReadingOrder, DataType.Int32));
                if (alignmentInfo.RelativeIndent != AlignmentInfo.DEFAULT_RELATIVEINDENT)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.RelativeIndentAttributeName, XLSXElementBase.GetXmlString(alignmentInfo.RelativeIndent, DataType.Int32));
                if (alignmentInfo.ShrinkToFit != AlignmentInfo.DEFAULT_SHRINKTOFIT)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.ShrinkToFitAttributeName, XLSXElementBase.GetXmlString(true, DataType.Boolean));
                if (alignmentInfo.TextRotation != AlignmentInfo.DEFAULT_TEXTROTATION)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.TextRotationAttributeName, XLSXElementBase.GetXmlString(alignmentInfo.TextRotation, DataType.Int32));
                if (alignmentInfo.Vertical != VerticalCellAlignment.Default &&
                    alignmentInfo.Vertical != AlignmentInfo.DEFAULT_VERTICAL)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.VerticalAttributeName, XLSXElementBase.GetXmlString(alignmentInfo.Vertical, DataType.ST_VerticalAlignment));
                if (alignmentInfo.WrapText != AlignmentInfo.DEFAULT_WRAPTEXT)
                    XLSXElementBase.AddAttribute(element, AlignmentElement.WrapTextAttributeName, XLSXElementBase.GetXmlString(true, DataType.Boolean));
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