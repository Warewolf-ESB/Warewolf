using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/6/11 - 12.1 - Table Support
	internal class TableColumnElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TableColumn">
		//  <sequence>
		//    <element name="calculatedColumnFormula" type="CT_TableFormula" minOccurs="0" maxOccurs="1"/>
		//    <element name="totalsRowFormula" type="CT_TableFormula" minOccurs="0" maxOccurs="1"/>
		//    <element name="xmlColumnPr" type="CT_XmlColumnPr" minOccurs="0" maxOccurs="1"/>
		//    <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//  </sequence>
		//  <attribute name="id" type="xsd:unsignedInt" use="required"/>
		//  <attribute name="uniqueName" type="ST_Xstring" use="optional"/>
		//  <attribute name="name" type="ST_Xstring" use="required"/>
		//  <attribute name="totalsRowFunction" type="ST_TotalsRowFunction" use="optional" default="none"/>
		//  <attribute name="totalsRowLabel" type="ST_Xstring" use="optional"/>
		//  <attribute name="queryTableFieldId" type="xsd:unsignedInt" use="optional"/>
		//  <attribute name="headerRowDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="dataDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="totalsRowDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="headerRowCellStyle" type="ST_Xstring" use="optional"/>
		//  <attribute name="dataCellStyle" type="ST_Xstring" use="optional"/>
		//  <attribute name="totalsRowCellStyle" type="ST_Xstring" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "tableColumn";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TableColumnElement.LocalName;

		private const string DataCellStyleAttributeName = "dataCellStyle";
		private const string DataDxfIdAttributeName = "dataDxfId";
		private const string HeaderRowCellStyleAttributeName = "headerRowCellStyle";
		private const string HeaderRowDxfIdAttributeName = "headerRowDxfId";
		private const string NameAttributeName = "name";
		private const string QueryTableFieldIdAttributeName = "queryTableFieldId";
		private const string TotalsRowCellStyleAttributeName = "totalsRowCellStyle";
		private const string TotalsRowDxfIdAttributeName = "totalsRowDxfId";
		private const string TotalsRowFunctionAttributeName = "totalsRowFunction";
		private const string TotalsRowLabelAttributeName = "totalsRowLabel";
		private const string UniqueNameAttributeName = "uniqueName";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.tableColumn; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if (table == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTable on the context stack.");
				return;
			}

			string dataCellStyle = default(string);
			uint? dataDxfId = null;
			string headerRowCellStyle = default(string);
			uint? headerRowDxfId = null;
			uint id = default(uint);
			string name = default(string);
			uint queryTableFieldId = default(uint);
			string totalsRowCellStyle = default(string);
			uint? totalsRowDxfId = null;
			ST_TotalsRowFunction totalsRowFunction = ST_TotalsRowFunction.none;
			string totalsRowLabel = default(string);
			string uniqueName = default(string);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableColumnElement.DataCellStyleAttributeName:
						dataCellStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, dataCellStyle);
						break;

					case TableColumnElement.DataDxfIdAttributeName:
						dataDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, dataDxfId);
						break;

					case TableColumnElement.HeaderRowCellStyleAttributeName:
						headerRowCellStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, headerRowCellStyle);
						break;

					case TableColumnElement.HeaderRowDxfIdAttributeName:
						headerRowDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, headerRowDxfId);
						break;

					case TableColumnElement.IdAttributeName:
						id = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, id);
						break;

					case TableColumnElement.NameAttributeName:
						name = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, name);
						break;

					case TableColumnElement.QueryTableFieldIdAttributeName:
						queryTableFieldId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, queryTableFieldId);
						break;

					case TableColumnElement.TotalsRowCellStyleAttributeName:
						totalsRowCellStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, totalsRowCellStyle);
						break;

					case TableColumnElement.TotalsRowDxfIdAttributeName:
						totalsRowDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, totalsRowDxfId);
						break;

					case TableColumnElement.TotalsRowFunctionAttributeName:
						totalsRowFunction = (ST_TotalsRowFunction)XmlElementBase.GetAttributeValue(attribute, DataType.ST_TotalsRowFunction, totalsRowFunction);
						break;

					case TableColumnElement.TotalsRowLabelAttributeName:
						totalsRowLabel = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, totalsRowLabel);
						break;

					case TableColumnElement.UniqueNameAttributeName:
						uniqueName = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, uniqueName);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			
			if (element.Index < 0)
			{
				Utilities.DebugFail("The tableColumn element index is out of range.");
				return;
			}

			WorksheetTableColumn column = table.InsertColumn(id);

			column.Name = name;
			column.TotalLabel = totalsRowLabel;
			column.TotalFormula = manager.Workbook.GetTotalFormula(column, totalsRowFunction);

			TableElement.LoadAreaFormat(manager, column.AreaFormats, WorksheetTableColumnArea.DataArea, WorksheetTableColumn.CanAreaFormatValueBeSet,
				dataCellStyle, dataDxfId);
			TableElement.LoadAreaFormat(manager, column.AreaFormats, WorksheetTableColumnArea.HeaderCell, WorksheetTableColumn.CanAreaFormatValueBeSet,
				headerRowCellStyle, headerRowDxfId);
			TableElement.LoadAreaFormat(manager, column.AreaFormats, WorksheetTableColumnArea.TotalCell, WorksheetTableColumn.CanAreaFormatValueBeSet,
				totalsRowCellStyle, totalsRowDxfId);

			manager.ContextStack.Push(column);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			EnumerableContext<WorksheetTableColumn> tableColumns = (EnumerableContext<WorksheetTableColumn>)manager.ContextStack[typeof(EnumerableContext<WorksheetTableColumn>)];
			if (tableColumns == null)
			{
				Utilities.DebugFail("Cannot find the EnumerableContext<WorksheetTableColumn> on the context stack.");
				return;
			}

			WorksheetTableColumn column = tableColumns.ConsumeCurrentItem();
			manager.ContextStack.Push(column);

			string attributeValue = default(string);

			TableElement.SaveAreaFormat(manager, column.AreaFormats, WorksheetTableColumnArea.DataArea, element,
				TableColumnElement.DataCellStyleAttributeName, TableColumnElement.DataDxfIdAttributeName);

			if (column.Table.IsHeaderRowVisible == false)
			{
				TableElement.SaveAreaFormat(manager, column.AreaFormats, WorksheetTableColumnArea.HeaderCell, element,
					TableColumnElement.HeaderRowCellStyleAttributeName, TableColumnElement.HeaderRowDxfIdAttributeName);
			}

			// Add the 'id' attribute
			attributeValue = XmlElementBase.GetXmlString(column.Id, DataType.UInt32, default(uint), true);
			XmlElementBase.AddAttribute(element, TableColumnElement.IdAttributeName, attributeValue);

			// Add the 'name' attribute
			attributeValue = XmlElementBase.GetXmlString(column.Name, DataType.ST_Xstring, default(string), true);
			XmlElementBase.AddAttribute(element, TableColumnElement.NameAttributeName, attributeValue);

			// Add the 'queryTableFieldId' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.UInt32, default(uint), false);
			//XmlElementBase.AddAttribute(element, TableColumnElement.QueryTableFieldIdAttributeName, attributeValue);

			TableElement.SaveAreaFormat(manager, column.AreaFormats, WorksheetTableColumnArea.TotalCell, element,
				TableColumnElement.TotalsRowCellStyleAttributeName, TableColumnElement.TotalsRowDxfIdAttributeName);

			ST_TotalsRowFunction totalsRowFunction = manager.Workbook.GetTotalRowFunction(column);

			// Add the 'totalsRowFunction' attribute
			attributeValue = XmlElementBase.GetXmlString(totalsRowFunction, DataType.ST_TotalsRowFunction, ST_TotalsRowFunction.none, false);
			XmlElementBase.AddAttribute(element, TableColumnElement.TotalsRowFunctionAttributeName, attributeValue);

			// Add the 'totalsRowLabel' attribute
			string totalLabel = column.TotalLabel;
			if (string.IsNullOrEmpty(totalLabel) == false)
			{
				attributeValue = XmlElementBase.GetXmlString(totalLabel, DataType.ST_Xstring);
				XmlElementBase.AddAttribute(element, TableColumnElement.TotalsRowLabelAttributeName, attributeValue);
			}

			// Add the 'uniqueName' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.ST_Xstring, default(string), false);
			//XmlElementBase.AddAttribute(element, TableColumnElement.UniqueNameAttributeName, attributeValue);

			if (column.ColumnFormula != null)
			{
				// Add the 'calculatedColumnFormula' element
				XmlElementBase.AddElement(element, CalculatedColumnFormulaElement.QualifiedName);
			}

			if (totalsRowFunction == ST_TotalsRowFunction.custom)
			{
				// Add the 'totalsRowFormula' element
				XmlElementBase.AddElement(element, TotalsRowFormulaElement.QualifiedName);
			}

			// Add the 'xmlColumnPr' element
			//XmlElementBase.AddElement(element, XmlColumnPrElement.QualifiedName);

			// Add the 'extLst' element
			//XmlElementBase.AddElement(element, ExtLstElement.QualifiedName);
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