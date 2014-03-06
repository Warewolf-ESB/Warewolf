using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class XfElement : XLSXElementBase
    {
		#region XML Schema Fragment

		//<complexType name="CT_Xf">
		//  <sequence>
		//    <element name="alignment" type="CT_CellAlignment" minOccurs="0" maxOccurs="1"/>
		//    <element name="protection" type="CT_CellProtection" minOccurs="0" maxOccurs="1"/>
		//    <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//  </sequence>
		//  <attribute name="numFmtId" type="ST_NumFmtId" use="optional"/>
		//  <attribute name="fontId" type="ST_FontId" use="optional"/>
		//  <attribute name="fillId" type="ST_FillId" use="optional"/>
		//  <attribute name="borderId" type="ST_BorderId" use="optional"/>
		//  <attribute name="xfId" type="ST_CellStyleXfId" use="optional"/>
		//  <attribute name="quotePrefix" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="pivotButton" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="applyNumberFormat" type="xsd:boolean" use="optional"/>
		//  <attribute name="applyFont" type="xsd:boolean" use="optional"/>
		//  <attribute name="applyFill" type="xsd:boolean" use="optional"/>
		//  <attribute name="applyBorder" type="xsd:boolean" use="optional"/>
		//  <attribute name="applyAlignment" type="xsd:boolean" use="optional"/>
		//  <attribute name="applyProtection" type="xsd:boolean" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

        #region Constants

        public const string LocalName = "xf";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            XfElement.LocalName;

		private const string ApplyAlignmentAttributeName = "applyAlignment";
		private const string ApplyBorderAttributeName = "applyBorder";
		private const string ApplyFillAttributeName = "applyFill";
		private const string ApplyFontAttributeName = "applyFont";
		private const string ApplyNumberFormatAttributeName = "applyNumberFormat";
		private const string ApplyProtectionAttributeName = "applyProtection";
		private const string BorderIdAttributeName = "borderId";
		private const string FillIdAttributeName = "fillId";
		private const string FontIdAttributeName = "fontId";
		private const string NumFmtIdAttributeName = "numFmtId";
		private const string PivotButtonAttributeName = "pivotButton";
		private const string QuotePrefixAttributeName = "quotePrefix";
		private const string XfIdAttributeName = "xfId";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.xf; }
        }
        #endregion Type

		// MD 1/1/12 - 12.1 - Table Support
		// Rewrote the save and load methods to function more closely with the way Excel saves and loads XF records.
		#region Old Code

		//        #region Load

		//#if DEBUG
		//        /// <summary>
		//        /// Deserializes the state of this element from the specified Excel2007WorkbookSerializationManager.
		//        /// </summary>
		//        /// <param name="manager">The Excel2007WorkbookSerializationManager instance which handles serialization for the element.</param>
		//        /// <param name="element">The XmlElement associated with this element.</param>
		//        /// <param name="value">The text within the tags of the element.</param>
		//        /// <param name="isReaderOnNextNode">[Ref] Determines whether the reader has already advanced to the next node.</param>
		//#endif
		//        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		//        {
		//            ListContext<FormatInfo> listContext = (ListContext<FormatInfo>)manager.ContextStack[typeof(ListContext<FormatInfo>)];
		//            if (listContext == null)
		//            {
		//                Utilities.DebugFail("List for FormatInfo was not found on the ContextStack.");
		//                return;
		//            }

		//            FormatInfo formatInfo = new FormatInfo();

		//            foreach (ExcelXmlAttribute attribute in element.Attributes)
		//            {
		//                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

		//                switch (attributeName)
		//                {
		//                    case XfElement.ApplyAlignmentAttributeName:
		//                        formatInfo.ApplyAlignment = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.ApplyBorderAttributeName:
		//                        formatInfo.ApplyBorder = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.ApplyFillAttributeName:
		//                        formatInfo.ApplyFill = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.ApplyFontAttributeName:
		//                        formatInfo.ApplyFont = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.ApplyNumberFormatAttributeName:
		//                        formatInfo.ApplyNumberFormat = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.ApplyProtectionAttributeName:
		//                        formatInfo.ApplyProtection = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.BorderIdAttributeName:
		//                        formatInfo.BorderId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
		//                        break;
		//                    case XfElement.FillIdAttributeName:
		//                        formatInfo.FillId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
		//                        break;
		//                    case XfElement.FontIdAttributeName:
		//                        formatInfo.FontId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
		//                        break;
		//                    case XfElement.NumFmtIdAttributeName:
		//                        formatInfo.NumFmtId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
		//                        break;
		//                    case XfElement.PivotButtonAttributeName:
		//                        formatInfo.PivotButton = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.QuotePrefixAttributeName:
		//                        formatInfo.QuotePrefix = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
		//                        break;
		//                    case XfElement.XfIdAttributeName:
		//                        formatInfo.CellStyleXfId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
		//                        break;
		//                    default:
		//                        Utilities.DebugFail("Unknown attribute type in the xf element: " + attributeName);
		//                        break;
		//                }
		//            }

		//            listContext.AddItem(formatInfo);

		//            ChildDataItem childDataItem = new ChildDataItem(formatInfo);
		//            manager.ContextStack.Push(childDataItem);

		//        }

		//        #endregion Load

		//        #region Save

		//#if DEBUG
		//        /// <summary>
		//        /// Serializes the state of this Element to the specified <see cref="Excel2007WorkbookSerializationManager">Excel2007WorkbookSerializationManager</see>.
		//        /// </summary>
		//        /// <param name="manager">The <see cref="Excel2007WorkbookSerializationManager">Excel2007WorkbookSerializationManager</see> instance which handles serialization for the element.</param>
		//        /// <param name="element">The XmlElement associated with this element.</param>
		//        /// <param name="value">[ref] The text value of the element. This should be set to save out text between the element tags. If this is set, it is expected that the node has no child nodes.</param>  
		//#endif
		//        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		//        {
		//            FormatInfo formatInfo = null;

		//            ListContext<FormatInfo> listContext = manager.ContextStack[typeof(ListContext<FormatInfo>)] as ListContext<FormatInfo>;
		//            if (listContext != null)
		//                formatInfo = listContext.ConsumeCurrentItem() as FormatInfo;

		//            if (formatInfo == null)
		//                formatInfo = manager.ContextStack[typeof(FormatInfo)] as FormatInfo;

		//            if (formatInfo == null)
		//            {
		//                Utilities.DebugFail("FormatInfo object not found on stack.");
		//                return;
		//            }

		//            string trueString = XLSXElementBase.GetXmlString(true, DataType.Boolean);

		//            if (formatInfo.Alignment != null &&
		//                !formatInfo.Alignment.IsDefault)
		//            {
		//                manager.ContextStack.Push(formatInfo.Alignment);
		//                XLSXElementBase.AddElement(element, AlignmentElement.QualifiedName);
		//            }

		//            if (formatInfo.ApplyAlignment)
		//                XLSXElementBase.AddAttribute(element, XfElement.ApplyAlignmentAttributeName, trueString);

		//            if (formatInfo.ApplyBorder)
		//                XLSXElementBase.AddAttribute(element, XfElement.ApplyBorderAttributeName, trueString);

		//            if (formatInfo.ApplyFill)
		//                XLSXElementBase.AddAttribute(element, XfElement.ApplyFillAttributeName, trueString);

		//            if (formatInfo.ApplyFont)
		//                XLSXElementBase.AddAttribute(element, XfElement.ApplyFontAttributeName, trueString);

		//            if (formatInfo.ApplyNumberFormat)
		//                XLSXElementBase.AddAttribute(element, XfElement.ApplyNumberFormatAttributeName, trueString);

		//            if (formatInfo.ApplyProtection)
		//                XLSXElementBase.AddAttribute(element, XfElement.ApplyProtectionAttributeName, trueString);

		//            if (formatInfo.BorderId >= 0)
		//                XLSXElementBase.AddAttribute(element, XfElement.BorderIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.BorderId, DataType.Int32));

		//            if (formatInfo.CellStyleXfId >= 0)
		//                XLSXElementBase.AddAttribute(element, XfElement.XfIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.CellStyleXfId, DataType.Int32));

		//            if (formatInfo.FillId >= 0)
		//                XLSXElementBase.AddAttribute(element, XfElement.FillIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.FillId, DataType.Int32));

		//            if (formatInfo.FontId >= 0)
		//                XLSXElementBase.AddAttribute(element, XfElement.FontIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.FontId, DataType.Int32));

		//            if (formatInfo.NumFmtId >= 0)
		//                XLSXElementBase.AddAttribute(element, XfElement.NumFmtIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.NumFmtId, DataType.Int32));

		//            if (formatInfo.PivotButton)
		//                XLSXElementBase.AddAttribute(element, XfElement.PivotButtonAttributeName, trueString);

		//            if (formatInfo.QuotePrefix)
		//                XLSXElementBase.AddAttribute(element, XfElement.QuotePrefixAttributeName, trueString);

		//            if (formatInfo.Protection != null &&
		//                !formatInfo.Protection.IsDefault)
		//            {
		//                manager.ContextStack.Push(formatInfo.Protection);
		//                XLSXElementBase.AddElement(element, ProtectionElement.QualifiedName);
		//            }
		//        }

		//        #endregion Save

		//        // MD 1/17/11 - TFS62014
		//        // We cannot resolve the FormatDataObject instances immediately after the <xf> element is loaded because it may depend on indexed
		//        // colors from a custom color palette. Those custom colors are defined at the bottom of the styles.xml file, after all the <xf>
		//        // elements. So this step must be deferred until the entire file is loaded. It has now been moved to StyleSheetElement.ParseFormatInfos,
		//        // which is called from StyleSheetElement.OnAfterLoadChildElements, after the entire sheet is loaded.
		//        //#region OnAfterLoadChildElements
		//        //
		//        //protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		//        //{
		//        //    ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
		//        //
		//        //    if (dataItem == null ||
		//        //        dataItem.Data == null)
		//        //    {
		//        //        Utilities.DebugFail("The ChildDataItem not found on the stack, or its data is null.");
		//        //        return;
		//        //    }
		//        //
		//        //    FormatInfo formatInfo = dataItem.Data as FormatInfo;
		//        //
		//        //    if (formatInfo == null)
		//        //    {
		//        //        Utilities.DebugFail("The FormatInfo was removed from the context stack");
		//        //        return;
		//        //    }
		//        //
		//        //    formatInfo.FormatDataObject = formatInfo.CreateWorksheetCellFormatData(manager);
		//        //
		//        //    if (manager.Formats.Count == 0)
		//        //        manager.Workbook.CellFormats.DefaultElement = formatInfo.FormatDataObject;
		//        //
		//        //    manager.Formats.Add(formatInfo.FormatDataObject);
		//        //}
		//        //
		//        //#endregion OnAfterLoadChildElements

		#endregion // Old Code

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			ListContext<FormatInfo> listContext = (ListContext<FormatInfo>)manager.ContextStack[typeof(ListContext<FormatInfo>)];
			if (listContext == null)
			{
				Utilities.DebugFail("List for FormatInfo was not found on the ContextStack.");
				return;
			}

			bool isStyle = (listContext.List == manager.CellStyleXfs);

			// The attributes indicating which format options are set defaults to False for cell formats and True for style format.
			bool defaultFormatOptionsValue = isStyle;

			bool applyAlignment = defaultFormatOptionsValue;
			bool applyBorder = defaultFormatOptionsValue;
			bool applyFill = defaultFormatOptionsValue;
			bool applyFont = defaultFormatOptionsValue;
			bool applyNumberFormat = defaultFormatOptionsValue;
			bool applyProtection = defaultFormatOptionsValue;
			int borderId = default(int);
			int fillId = default(int);
			int fontId = default(int);
			int numFmtId = default(int);
			bool pivotButton = false;
			bool quotePrefix = false;
			int xfId = default(int);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case XfElement.ApplyAlignmentAttributeName:
						applyAlignment = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, applyAlignment);
						break;

					case XfElement.ApplyBorderAttributeName:
						applyBorder = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, applyBorder);
						break;

					case XfElement.ApplyFillAttributeName:
						applyFill = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, applyFill);
						break;

					case XfElement.ApplyFontAttributeName:
						applyFont = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, applyFont);
						break;

					case XfElement.ApplyNumberFormatAttributeName:
						applyNumberFormat = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, applyNumberFormat);
						break;

					case XfElement.ApplyProtectionAttributeName:
						applyProtection = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, applyProtection);
						break;

					case XfElement.BorderIdAttributeName:
						borderId = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Int32, borderId);
						break;

					case XfElement.FillIdAttributeName:
						fillId = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Int32, fillId);
						break;

					case XfElement.FontIdAttributeName:
						fontId = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Int32, fontId);
						break;

					case XfElement.NumFmtIdAttributeName:
						numFmtId = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Int32, numFmtId);
						break;

					case XfElement.PivotButtonAttributeName:
						pivotButton = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, pivotButton);
						break;

					case XfElement.QuotePrefixAttributeName:
						quotePrefix = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, quotePrefix);
						break;

					case XfElement.XfIdAttributeName:
						xfId = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Int32, xfId);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			FormatInfo formatInfo = new FormatInfo();

			WorksheetCellFormatOptions formatOptions = WorksheetCellFormatOptions.None;
			if (applyAlignment)
				formatOptions |= WorksheetCellFormatOptions.ApplyAlignmentFormatting;

			if (applyBorder)
				formatOptions |= WorksheetCellFormatOptions.ApplyBorderFormatting;

			if (applyFont)
				formatOptions |= WorksheetCellFormatOptions.ApplyFontFormatting;

			if (applyNumberFormat)
				formatOptions |= WorksheetCellFormatOptions.ApplyNumberFormatting;

			if (applyFill)
				formatOptions |= WorksheetCellFormatOptions.ApplyFillFormatting;

			if (applyProtection)
				formatOptions |= WorksheetCellFormatOptions.ApplyProtectionFormatting;

			formatInfo.FormatOptions = formatOptions;

			formatInfo.BorderId = borderId;
			formatInfo.FillId = fillId;
			formatInfo.FontId = fontId;
			formatInfo.NumFmtId = numFmtId;
			formatInfo.PivotButton = pivotButton;
			formatInfo.QuotePrefix = quotePrefix;
			formatInfo.CellStyleXfId = xfId;

			listContext.AddItem(formatInfo);

			ChildDataItem childDataItem = new ChildDataItem(formatInfo);
			manager.ContextStack.Push(childDataItem);
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			FormatInfo formatInfo = null;

			ListContext<FormatInfo> listContext = manager.ContextStack[typeof(ListContext<FormatInfo>)] as ListContext<FormatInfo>;
			if (listContext != null)
				formatInfo = listContext.ConsumeCurrentItem() as FormatInfo;

			if (formatInfo == null)
				formatInfo = manager.ContextStack[typeof(FormatInfo)] as FormatInfo;

			if (formatInfo == null)
			{
				Utilities.DebugFail("FormatInfo object not found on stack.");
				return;
			}

			bool applyAlignment = Utilities.TestFlag(formatInfo.FormatOptions, WorksheetCellFormatOptions.ApplyAlignmentFormatting);
			bool applyBorder = Utilities.TestFlag(formatInfo.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting);
			bool applyFill = Utilities.TestFlag(formatInfo.FormatOptions, WorksheetCellFormatOptions.ApplyFillFormatting);
			bool applyFont = Utilities.TestFlag(formatInfo.FormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting);
			bool applyNumberFormat = Utilities.TestFlag(formatInfo.FormatOptions, WorksheetCellFormatOptions.ApplyNumberFormatting);
			bool applyProtection = Utilities.TestFlag(formatInfo.FormatOptions, WorksheetCellFormatOptions.ApplyProtectionFormatting);

			bool isStyle = (listContext.List == manager.CellStyleXfs);

			// The attributes indicating which format options are set defaults to False for cell formats and True for style format.
			bool defaultFormatOptionsValue = isStyle;

			string attributeValue;

			attributeValue = XLSXElementBase.GetXmlString(applyAlignment, DataType.Boolean, defaultFormatOptionsValue, false);
			XLSXElementBase.AddAttribute(element, XfElement.ApplyAlignmentAttributeName, attributeValue);

			attributeValue = XLSXElementBase.GetXmlString(applyBorder, DataType.Boolean, defaultFormatOptionsValue, false);
			XLSXElementBase.AddAttribute(element, XfElement.ApplyBorderAttributeName, attributeValue);

			attributeValue = XLSXElementBase.GetXmlString(applyFill, DataType.Boolean, defaultFormatOptionsValue, false);
			XLSXElementBase.AddAttribute(element, XfElement.ApplyFillAttributeName, attributeValue);

			attributeValue = XLSXElementBase.GetXmlString(applyFont, DataType.Boolean, defaultFormatOptionsValue, false);
			XLSXElementBase.AddAttribute(element, XfElement.ApplyFontAttributeName, attributeValue);

			attributeValue = XLSXElementBase.GetXmlString(applyNumberFormat, DataType.Boolean, defaultFormatOptionsValue, false);
			XLSXElementBase.AddAttribute(element, XfElement.ApplyNumberFormatAttributeName, attributeValue);

			attributeValue = XLSXElementBase.GetXmlString(applyProtection, DataType.Boolean, defaultFormatOptionsValue, false);
			XLSXElementBase.AddAttribute(element, XfElement.ApplyProtectionAttributeName, attributeValue);

			if (formatInfo.BorderId >= 0)
				XLSXElementBase.AddAttribute(element, XfElement.BorderIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.BorderId, DataType.Int32));

			if (formatInfo.CellStyleXfId >= 0)
				XLSXElementBase.AddAttribute(element, XfElement.XfIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.CellStyleXfId, DataType.Int32));

			if (formatInfo.FillId >= 0)
				XLSXElementBase.AddAttribute(element, XfElement.FillIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.FillId, DataType.Int32));

			if (formatInfo.FontId >= 0)
				XLSXElementBase.AddAttribute(element, XfElement.FontIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.FontId, DataType.Int32));

			if (formatInfo.NumFmtId >= 0)
				XLSXElementBase.AddAttribute(element, XfElement.NumFmtIdAttributeName, XLSXElementBase.GetXmlString(formatInfo.NumFmtId, DataType.Int32));

			attributeValue = XLSXElementBase.GetXmlString(formatInfo.PivotButton, DataType.Boolean, false, false);
			XLSXElementBase.AddAttribute(element, XfElement.PivotButtonAttributeName, attributeValue);

			attributeValue = XLSXElementBase.GetXmlString(formatInfo.QuotePrefix, DataType.Boolean, false, false);
			XLSXElementBase.AddAttribute(element, XfElement.QuotePrefixAttributeName, attributeValue);

			if (formatInfo.Alignment != null &&
			!formatInfo.Alignment.IsDefault)
			{
				manager.ContextStack.Push(formatInfo.Alignment);
				XLSXElementBase.AddElement(element, AlignmentElement.QualifiedName);
			}

			if (formatInfo.Protection != null &&
				!formatInfo.Protection.IsDefault)
			{
				manager.ContextStack.Push(formatInfo.Protection);
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