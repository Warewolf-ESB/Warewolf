using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.Sorting;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/6/11 - 12.1 - Table Support
	internal class TableElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Table">
		//  <sequence>
		//    <element name="autoFilter" type="CT_AutoFilter" minOccurs="0" maxOccurs="1"/>
		//    <element name="sortState" type="CT_SortState" minOccurs="0" maxOccurs="1"/>
		//    <element name="tableColumns" type="CT_TableColumns" minOccurs="1" maxOccurs="1"/>
		//    <element name="tableStyleInfo" type="CT_TableStyleInfo" minOccurs="0" maxOccurs="1"/>
		//    <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//  </sequence>
		//  <attribute name="id" type="xsd:unsignedInt" use="required"/>
		//  <attribute name="name" type="ST_Xstring" use="optional"/>
		//  <attribute name="displayName" type="ST_Xstring" use="required"/>
		//  <attribute name="comment" type="ST_Xstring" use="optional"/>
		//  <attribute name="ref" type="ST_Ref" use="required"/>
		//  <attribute name="tableType" type="ST_TableType" use="optional" default="worksheet"/>
		//  <attribute name="headerRowCount" type="xsd:unsignedInt" use="optional" default="1"/>
		//  <attribute name="insertRow" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="insertRowShift" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="totalsRowCount" type="xsd:unsignedInt" use="optional" default="0"/>
		//  <attribute name="totalsRowShown" type="xsd:boolean" use="optional" default="true"/>
		//  <attribute name="published" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="headerRowDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="dataDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="totalsRowDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="headerRowBorderDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="tableBorderDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="totalsRowBorderDxfId" type="ST_DxfId" use="optional"/>
		//  <attribute name="headerRowCellStyle" type="ST_Xstring" use="optional"/>
		//  <attribute name="dataCellStyle" type="ST_Xstring" use="optional"/>
		//  <attribute name="totalsRowCellStyle" type="ST_Xstring" use="optional"/>
		//  <attribute name="connectionId" type="xsd:unsignedInt" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "table";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TableElement.LocalName;

		private const string CommentAttributeName = "comment";
		private const string ConnectionIdAttributeName = "connectionId";
		private const string DataCellStyleAttributeName = "dataCellStyle";
		private const string DataDxfIdAttributeName = "dataDxfId";
		private const string DisplayNameAttributeName = "displayName";
		private const string HeaderRowBorderDxfIdAttributeName = "headerRowBorderDxfId";
		private const string HeaderRowCellStyleAttributeName = "headerRowCellStyle";
		private const string HeaderRowCountAttributeName = "headerRowCount";
		private const string HeaderRowDxfIdAttributeName = "headerRowDxfId";
		private const string InsertRowAttributeName = "insertRow";
		private const string InsertRowShiftAttributeName = "insertRowShift";
		private const string NameAttributeName = "name";
		private const string PublishedAttributeName = "published";
		private const string RefAttributeName = "ref";
		private const string TableBorderDxfIdAttributeName = "tableBorderDxfId";
		private const string TableTypeAttributeName = "tableType";
		private const string TotalsRowBorderDxfIdAttributeName = "totalsRowBorderDxfId";
		private const string TotalsRowCellStyleAttributeName = "totalsRowCellStyle";
		private const string TotalsRowCountAttributeName = "totalsRowCount";
		private const string TotalsRowDxfIdAttributeName = "totalsRowDxfId";
		private const string TotalsRowShownAttributeName = "totalsRowShown";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.table; }
		}

		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			string comment = default(string);
			uint connectionId = default(uint);
			string dataCellStyle = default(string);
			uint? dataDxfId = null;
			string displayName = default(string);
			uint? headerRowBorderDxfId = null;
			string headerRowCellStyle = default(string);
			uint headerRowCount = 1;
			uint? headerRowDxfId = null;
			uint id = default(uint);
			bool insertRow = false;
			bool insertRowShift = false;
			string name = default(string);
			bool published = false;
			string refValue = default(string);
			uint? tableBorderDxfId = null;
			ST_TableType tableType = ST_TableType.worksheet;
			uint? totalsRowBorderDxfId = null;
			string totalsRowCellStyle = default(string);
			uint totalsRowCount = 0;
			uint? totalsRowDxfId = null;
			bool totalsRowShown = true;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableElement.CommentAttributeName:
						comment = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, comment);
						break;

					case TableElement.ConnectionIdAttributeName:
						connectionId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, connectionId);
						break;

					case TableElement.DataCellStyleAttributeName:
						dataCellStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, dataCellStyle);
						break;

					case TableElement.DataDxfIdAttributeName:
						dataDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, dataDxfId);
						break;

					case TableElement.DisplayNameAttributeName:
						displayName = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, displayName);
						break;

					case TableElement.HeaderRowBorderDxfIdAttributeName:
						headerRowBorderDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, headerRowBorderDxfId);
						break;

					case TableElement.HeaderRowCellStyleAttributeName:
						headerRowCellStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, headerRowCellStyle);
						break;

					case TableElement.HeaderRowCountAttributeName:
						headerRowCount = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, headerRowCount);
						break;

					case TableElement.HeaderRowDxfIdAttributeName:
						headerRowDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, headerRowDxfId);
						break;

					case TableElement.IdAttributeName:
						id = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, id);
						break;

					case TableElement.InsertRowAttributeName:
						insertRow = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, insertRow);
						break;

					case TableElement.InsertRowShiftAttributeName:
						insertRowShift = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, insertRowShift);
						break;

					case TableElement.NameAttributeName:
						name = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, name);
						break;

					case TableElement.PublishedAttributeName:
						published = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, published);
						break;

					case TableElement.RefAttributeName:
						refValue = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Ref, refValue);
						break;

					case TableElement.TableBorderDxfIdAttributeName:
						tableBorderDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, tableBorderDxfId);
						break;

					case TableElement.TableTypeAttributeName:
						tableType = (ST_TableType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_TableType, tableType);
						break;

					case TableElement.TotalsRowBorderDxfIdAttributeName:
						totalsRowBorderDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, totalsRowBorderDxfId);
						break;

					case TableElement.TotalsRowCellStyleAttributeName:
						totalsRowCellStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, totalsRowCellStyle);
						break;

					case TableElement.TotalsRowCountAttributeName:
						totalsRowCount = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, totalsRowCount);
						break;

					case TableElement.TotalsRowDxfIdAttributeName:
						totalsRowDxfId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DxfId, totalsRowDxfId);
						break;

					case TableElement.TotalsRowShownAttributeName:
						totalsRowShown = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, true);
						break;


					case "http://www.w3.org/2000/xmlns/xmlns":
						break;

					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			
			
			Debug.Assert(name == displayName, "This is unexpected.");
			Debug.Assert(headerRowCount == 0 || headerRowCount == 1, "This is unexpected.");
			Debug.Assert(totalsRowCount == 0 || totalsRowCount == 1, "This is unexpected.");
			Debug.Assert(published == false, "Not sure how to handle a published table.");

			if (String.IsNullOrEmpty(refValue))
			{
				Utilities.DebugFail("There was no ref attribute in the table element.");
				return;
			}

			if (tableType != ST_TableType.worksheet)
			{
				Utilities.DebugFail("Not sure how to handle these types of tables.");
				return;
			}

			int firstRowIndex;
			short firstColumnIndex;
			int lastRowIndex;
			short lastColumnIndex;
			// MD 4/9/12 - TFS101506
			//Utilities.ParseRegionAddress(refValue, manager.Workbook.CurrentFormat, CellReferenceMode.A1, null, -1,
			Utilities.ParseRegionAddress(refValue, manager.Workbook.CurrentFormat, CellReferenceMode.A1, CultureInfo.InvariantCulture, null, -1,
				out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			WorksheetTable table = new WorksheetTable(name, id, firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
			ChildDataItem item = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			if (item != null)
				item.Data = table;

			table.Comment = comment;
			table.IsHeaderRowVisible = (headerRowCount == 1);
			table.IsInsertRowVisible = insertRow;
			table.IsTotalsRowVisible = (totalsRowCount == 1);
			table.HasTotalsRowEverBeenVisible = totalsRowShown;
			table.Name = name;
			table.Published = published;
			// Initialize this to False. We will se tit back to True if there is an autoFilter element.
			table.IsFilterUIVisible = false;
			table.WereCellsShiftedToShowInsertRow = insertRowShift;

			TableElement.LoadAreaFormat(manager, table.AreaFormats, WorksheetTableArea.DataArea, WorksheetTable.CanAreaFormatValueBeSet, 
				dataCellStyle, dataDxfId);
			TableElement.LoadAreaFormat(manager, table.AreaFormats, WorksheetTableArea.HeaderRow, WorksheetTable.CanAreaFormatValueBeSet, 
				headerRowCellStyle, headerRowDxfId, headerRowBorderDxfId,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);
			TableElement.LoadAreaFormat(manager, table.AreaFormats, WorksheetTableArea.TotalsRow, WorksheetTable.CanAreaFormatValueBeSet, 
				totalsRowCellStyle, totalsRowDxfId, totalsRowBorderDxfId,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle);
			TableElement.LoadAreaFormat(manager, table.AreaFormats, WorksheetTableArea.WholeTable, WorksheetTable.CanAreaFormatValueBeSet, 
				null, null, tableBorderDxfId,
				CellFormatValue.LeftBorderColorInfo, CellFormatValue.LeftBorderStyle,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle,
				CellFormatValue.RightBorderColorInfo, CellFormatValue.RightBorderStyle,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);

			manager.ContextStack.Push(new TableContext(table));
			manager.ContextStack.Push(table);
		}

		#endregion // Load

		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			TableContext tableContext = (TableContext)manager.ContextStack[typeof(TableContext)];
			if (tableContext == null)
			{
				Utilities.DebugFail("Cannot find the TableContext on the context stack.");
				return;
			}

			tableContext.OnLoaded();
		}

		#endregion // OnAfterLoadChildElements

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if(table == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTable on the context stack.");
				return;
			}

			string attributeValue = default(string);

			// Add the 'comment' attribute
			attributeValue = XmlElementBase.GetXmlString(table.Comment, DataType.ST_Xstring, default(string), false);
			XmlElementBase.AddAttribute(element, TableElement.CommentAttributeName, attributeValue);

			// Add the 'connectionId' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.UInt32, default(uint), false);
			//XmlElementBase.AddAttribute(element, TableElement.ConnectionIdAttributeName, attributeValue);

			TableElement.SaveAreaFormat(manager, table.AreaFormats, WorksheetTableArea.DataArea, element, 
				TableElement.DataCellStyleAttributeName, TableElement.DataDxfIdAttributeName);

			// Add the 'displayName' attribute
			attributeValue = XmlElementBase.GetXmlString(table.Name, DataType.ST_Xstring, default(string), true);
			XmlElementBase.AddAttribute(element, TableElement.DisplayNameAttributeName, attributeValue);

			TableElement.SaveAreaFormat(manager, table.AreaFormats, WorksheetTableArea.HeaderRow, element,
				TableElement.HeaderRowCellStyleAttributeName, TableElement.HeaderRowDxfIdAttributeName, TableElement.HeaderRowBorderDxfIdAttributeName, 
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);

			// Add the 'headerRowCount' attribute
			attributeValue = XmlElementBase.GetXmlString(table.IsHeaderRowVisible ? 1 : 0, DataType.UInt32, 1, false);
			XmlElementBase.AddAttribute(element, TableElement.HeaderRowCountAttributeName, attributeValue);

			// Add the 'id' attribute
			attributeValue = XmlElementBase.GetXmlString(table.Id, DataType.UInt32, default(uint), true);
			XmlElementBase.AddAttribute(element, TableElement.IdAttributeName, attributeValue);

			// Add the 'insertRow' attribute
			attributeValue = XmlElementBase.GetXmlString(table.IsInsertRowVisible, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, TableElement.InsertRowAttributeName, attributeValue);

			// Add the 'insertRowShift' attribute
			attributeValue = XmlElementBase.GetXmlString(table.WereCellsShiftedToShowInsertRow, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, TableElement.InsertRowShiftAttributeName, attributeValue);

			// Add the 'name' attribute
			attributeValue = XmlElementBase.GetXmlString(table.Name, DataType.ST_Xstring, default(string), false);
			XmlElementBase.AddAttribute(element, TableElement.NameAttributeName, attributeValue);

			// Add the 'published' attribute
			attributeValue = XmlElementBase.GetXmlString(table.Published, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, TableElement.PublishedAttributeName, attributeValue);

			// Add the 'ref' attribute
			attributeValue = XmlElementBase.GetXmlString(table.WholeTableRegion.ToString(CellReferenceMode.A1, false, true, true), DataType.ST_Ref, default(string), true);
			XmlElementBase.AddAttribute(element, TableElement.RefAttributeName, attributeValue);

			TableElement.SaveAreaFormat(manager, table.AreaFormats, WorksheetTableArea.WholeTable, element,
				null, null, TableElement.TableBorderDxfIdAttributeName,
				CellFormatValue.LeftBorderColorInfo, CellFormatValue.LeftBorderStyle,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle,
				CellFormatValue.RightBorderColorInfo, CellFormatValue.RightBorderStyle,
				CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);

			// Add the 'tableType' attribute
			attributeValue = XmlElementBase.GetXmlString(ST_TableType.worksheet, DataType.ST_TableType, ST_TableType.worksheet, false);
			XmlElementBase.AddAttribute(element, TableElement.TableTypeAttributeName, attributeValue);

			TableElement.SaveAreaFormat(manager, table.AreaFormats, WorksheetTableArea.TotalsRow, element,
				TableElement.TotalsRowCellStyleAttributeName, TableElement.TotalsRowDxfIdAttributeName, TableElement.TotalsRowBorderDxfIdAttributeName,
				CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle);

			// Add the 'totalsRowCount' attribute
			attributeValue = XmlElementBase.GetXmlString(table.IsTotalsRowVisible ? 1 : 0, DataType.UInt32, 0, false);
			XmlElementBase.AddAttribute(element, TableElement.TotalsRowCountAttributeName, attributeValue);

			// Add the 'totalsRowShown' attribute
			attributeValue = XmlElementBase.GetXmlString(table.HasTotalsRowEverBeenVisible, DataType.Boolean, true, false);
			XmlElementBase.AddAttribute(element, TableElement.TotalsRowShownAttributeName, attributeValue);



			if (table.IsFilterUIVisible)
			{
				// Add the 'autoFilter' element
				XmlElementBase.AddElement(element, AutoFilterElement.QualifiedName);
			}

			if (table.SortSettings.IsDirty)
			{
				// Add the 'sortState' element
				XmlElementBase.AddElement(element, SortStateElement.QualifiedName);
			}

			// Add the 'tableColumns' element
			XmlElementBase.AddElement(element, TableColumnsElement.QualifiedName);

			// Add the 'tableStyleInfo' element
			XmlElementBase.AddElement(element, TableStyleInfoElement.QualifiedName);

			// Add the 'extLst' element
			//XmlElementBase.AddElement(element, ExtLstElement.QualifiedName);
		}

		#endregion // Save

		#endregion // Base Class Overrides

		#region LoadAreaFormat

		internal static void LoadAreaFormat<T>(
			Excel2007WorkbookSerializationManager manager,
			WorksheetTableAreaFormatsCollection<T> areaFormats,
			T area,
			CanAreaFormatValueBeSetCallback<T> callback,
			string styleName,
			uint? dxfId)
		{
			TableElement.LoadAreaFormat(manager, areaFormats, area, callback, styleName, dxfId, null);
		}

		internal static void LoadAreaFormat<T>(
			Excel2007WorkbookSerializationManager manager,
			WorksheetTableAreaFormatsCollection<T> areaFormats,
			T area,
			CanAreaFormatValueBeSetCallback<T> callback,
			string styleName,
			uint? dxfId,
			uint? borderDxfId,
			params CellFormatValue[] borderValues)
		{
			Workbook workbook = manager.Workbook;

			WorkbookStyle style = null;
			if (String.IsNullOrEmpty(styleName) == false)
				style = workbook.Styles[styleName];

			WorksheetCellFormatData dxf = null;
			if (dxfId.HasValue)
				dxf = manager.Dxfs[(int)dxfId.Value].CloneInternal();

			WorksheetCellFormatData dxfBorders = null;
			if (borderDxfId.HasValue)
				dxfBorders = manager.Dxfs[(int)borderDxfId.Value].CloneInternal();

			manager.CombineDxfInfo(areaFormats, area, callback, style, dxf, dxfBorders, borderValues);
		}

		#endregion // LoadAreaFormat

		#region SaveAreaFormat

		internal static void SaveAreaFormat<T>(Excel2007WorkbookSerializationManager manager, WorksheetTableAreaFormatsCollection<T> areaFormats, T area, ExcelXmlElement element, string styleAttributeName, string dxfIdAttributeName)
		{
			TableElement.SaveAreaFormat(manager, areaFormats, area, element, styleAttributeName, dxfIdAttributeName, null);
		}

		internal static void SaveAreaFormat<T>(Excel2007WorkbookSerializationManager manager, WorksheetTableAreaFormatsCollection<T> areaFormats, T area, ExcelXmlElement element, string styleAttributeName, string dxfIdAttributeName, string borderDxfIdAttributeName, params CellFormatValue[] borderValues)
		{
			WorkbookStyle style;
			WorksheetCellFormatData dxf;
			WorksheetCellFormatData dxfBorder;
			manager.ExtractDxfInfo(areaFormats, area, out style, out dxf, out dxfBorder, borderValues);

			string attributeValue;
			if (styleAttributeName != null && style != null)
			{
				attributeValue = XmlElementBase.GetXmlString(style.Name, DataType.ST_Xstring, default(string), false);
				XmlElementBase.AddAttribute(element, styleAttributeName, attributeValue);
			}

			if (dxfIdAttributeName != null && dxf != null)
			{
				attributeValue = XmlElementBase.GetXmlString((uint)manager.AddDxf(dxf), DataType.ST_DxfId);
				XmlElementBase.AddAttribute(element, dxfIdAttributeName, attributeValue);
			}

			if (borderDxfIdAttributeName != null && dxfBorder != null)
			{
				attributeValue = XmlElementBase.GetXmlString((uint)manager.AddDxf(dxfBorder), DataType.ST_DxfId);
				XmlElementBase.AddAttribute(element, borderDxfIdAttributeName, attributeValue);
			}
		}

		#endregion // SaveAreaFormat
	}

	internal class TableContext
	{
		private Dictionary<int, Filter> _columnFilters;
		private List<KeyValuePair<short, SortCondition>> _columnSortConditions;
		private readonly WorksheetTable _table;

		public TableContext(WorksheetTable table)
		{
			_table = table;
		}

		public void AddFilter(int columnIdex, Filter filter)
		{
			if (filter == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			if (_columnFilters == null)
				_columnFilters = new Dictionary<int, Filter>();

			Debug.Assert(_columnFilters.ContainsKey(columnIdex) == false, "We shouldn't have two filters for the same column.");
			_columnFilters[columnIdex] = filter;
		}

		public void AddSortCondition(short columnIndex, SortCondition sortCondition)
		{
			if (_columnSortConditions == null)
				_columnSortConditions = new List<KeyValuePair<short, SortCondition>>();

			_columnSortConditions.Add(new KeyValuePair<short, SortCondition>(columnIndex, sortCondition));
		}

		public void OnLoaded()
		{
			if (_columnFilters != null)
			{
				foreach (KeyValuePair<int, Filter> pair in _columnFilters)
				{
					int relativeColumnIndex = pair.Key;
					if (relativeColumnIndex < 0 || _table.Columns.Count <= relativeColumnIndex)
					{
						Utilities.DebugFail("The column index is out of range..");
						continue;
					}

					WorksheetTableColumn column = _table.Columns[relativeColumnIndex];
					if (column == null)
					{
						Utilities.DebugFail("Cannot find a column with the specified id.");
						continue;
					}

					Filter filter = pair.Value;
					filter.SetOwner(column);
					column.Filter = filter;
				}
			}

			if (_columnSortConditions != null)
			{
				foreach (KeyValuePair<short, SortCondition> pair in _columnSortConditions)
				{
					int relativeColumnIndex = pair.Key - _table.WholeTableAddress.FirstColumnIndex;
					if (relativeColumnIndex < 0 || _table.Columns.Count <= relativeColumnIndex)
					{
						Utilities.DebugFail("The column index is out of range..");
						continue;
					}

					WorksheetTableColumn column = _table.Columns[relativeColumnIndex];
					Debug.Assert(column.SortCondition == null, "The column should not have a sort condition already.");
					_table.SortSettings.SortConditions.Add(column, pair.Value);
				}
			}
		}

		public WorksheetTable Table
		{
			get { return _table; }
		}
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