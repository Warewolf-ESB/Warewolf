using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class TableStyleElementElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TableStyleElement">
		//  <attribute name="type" type="ST_TableStyleType" use="required"/>
		//  <attribute name="size" type="xsd:unsignedInt" use="optional" default="1"/>
		//  <attribute name="dxfId" type="ST_DxfId" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "tableStyleElement";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TableStyleElementElement.LocalName;

		private const string DxfIdAttributeName = "dxfId";
		private const string SizeAttributeName = "size";
		private const string TypeAttributeName = "type";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.tableStyleElement; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			uint dxfId = default(uint);
			uint size = 1;
			ST_TableStyleType type = default(ST_TableStyleType);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableStyleElementElement.DxfIdAttributeName:
						dxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, dxfId);
						break;

					case TableStyleElementElement.SizeAttributeName:
						size = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, size);
						break;

					case TableStyleElementElement.TypeAttributeName:
						type = (ST_TableStyleType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_TableStyleType, type);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			WorksheetTableStyleArea tableArea = (WorksheetTableStyleArea)type;
			if (Enum.IsDefined(typeof(WorksheetTableStyleArea), tableArea) == false)
			{
				return;
			}

			WorksheetTableStyle tableStyle = (WorksheetTableStyle)manager.ContextStack[typeof(WorksheetTableStyle)];
			if (tableStyle == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTableStyle on the context stack.");
				return;
			}

			if (size != 1)
			{
				switch (tableArea)
				{
					case WorksheetTableStyleArea.RowStripe:
						tableStyle.RowStripeHeight = (int)size;
						break;

					case WorksheetTableStyleArea.AlternateRowStripe:
						tableStyle.AlternateRowStripeHeight = (int)size;
						break;

					case WorksheetTableStyleArea.ColumnStripe:
						tableStyle.ColumnStripeWidth = (int)size;
						break;

					case WorksheetTableStyleArea.AlternateColumnStripe:
						tableStyle.AlternateColumnStripeWidth = (int)size;
						break;

					default:
						Utilities.DebugFail("This type should not have a non-default size: " + type);
						break;
				}
			}

			// For some reason, the table styles in the presets file use 1-based dxfIds (but the pivot table styles use 0-based dxfIds)
			if (manager.IsLoadingPresetTableStyles)
				dxfId--;

			WorksheetCellFormatData format = manager.Dxfs[(int)dxfId];
			if (format.IsEmpty == false || manager.IsLoadingPresetTableStyles)
			{
				Workbook workbook = manager.Workbook;

				// For the preset table style, we don't want to root the temporary workbook in the area formats, so pass in a null workbook.
				if (manager.IsLoadingPresetTableStyles)
					workbook = null;

				WorksheetCellFormatProxy areaFormat = tableStyle.AreaFormats.GetFormatProxy(workbook, tableArea);
				foreach (CellFormatValue formatValue in WorksheetCellFormatData.AllCellFormatValues)
				{
					if (WorksheetTableStyle.CanAreaFormatValueBeSet(formatValue) == false)
						continue;

					Utilities.CopyCellFormatValue(format, areaFormat, formatValue);
				}
			}
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			WorksheetTableStyle tableStyle = (WorksheetTableStyle)manager.ContextStack[typeof(WorksheetTableStyle)];
			if (tableStyle == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTableStyle on the context stack.");
				return;
			}

			DictionaryContext<WorksheetTableStyleArea, uint> dxfIdsByAreaDuringSave = 
				(DictionaryContext<WorksheetTableStyleArea, uint>)manager.ContextStack[typeof(DictionaryContext<WorksheetTableStyleArea, uint>)];
			if (dxfIdsByAreaDuringSave == null)
			{
				Utilities.DebugFail("Cannot find the DictionaryContext<WorksheetTableStyleArea, uint> on the context stack.");
				return;
			}

			KeyValuePair<WorksheetTableStyleArea, uint> pair = dxfIdsByAreaDuringSave.ConsumeCurrentItem();

			uint? size = tableStyle.GetAreaSize(pair.Key);

			string attributeValue = String.Empty;

			// Add the 'dxfId' attribute
			attributeValue = XmlElementBase.GetXmlString(pair.Value, DataType.ST_DxfId, default(uint), true);
			XmlElementBase.AddAttribute(element, TableStyleElementElement.DxfIdAttributeName, attributeValue);

			if (size.HasValue)
			{
				// Add the 'size' attribute
				attributeValue = XmlElementBase.GetXmlString(size.Value, DataType.UInt32, (uint)1, false);
				XmlElementBase.AddAttribute(element, TableStyleElementElement.SizeAttributeName, attributeValue);
			}

			// Add the 'type' attribute
			attributeValue = XmlElementBase.GetXmlString((ST_TableStyleType)pair.Key, DataType.ST_TableStyleType, default(ST_TableStyleType), true);
			XmlElementBase.AddAttribute(element, TableStyleElementElement.TypeAttributeName, attributeValue);
		}

		#endregion // Save

		#endregion // Base Class Overrides
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