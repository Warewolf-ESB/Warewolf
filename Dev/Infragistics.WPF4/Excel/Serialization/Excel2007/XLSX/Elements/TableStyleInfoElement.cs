using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/6/11 - 12.1 - Table Support
	internal class TableStyleInfoElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TableStyleInfo">
		//  <attribute name="name" type="ST_Xstring" use="optional"/>
		//  <attribute name="showFirstColumn" type="xsd:boolean" use="optional"/>
		//  <attribute name="showLastColumn" type="xsd:boolean" use="optional"/>
		//  <attribute name="showRowStripes" type="xsd:boolean" use="optional"/>
		//  <attribute name="showColumnStripes" type="xsd:boolean" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "tableStyleInfo";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TableStyleInfoElement.LocalName;

		private const string NameAttributeName = "name";
		private const string ShowColumnStripesAttributeName = "showColumnStripes";
		private const string ShowFirstColumnAttributeName = "showFirstColumn";
		private const string ShowLastColumnAttributeName = "showLastColumn";
		private const string ShowRowStripesAttributeName = "showRowStripes";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.tableStyleInfo; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if (table == null)
			{
				Utilities.DebugFail("Could not find the WorksheetTable on the context stack.");
				return;
			}

			string name = default(string);
			bool showColumnStripes = default(bool);
			bool showFirstColumn = default(bool);
			bool showLastColumn = default(bool);
			bool showRowStripes = default(bool);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableStyleInfoElement.NameAttributeName:
						name = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, name);
						break;

					case TableStyleInfoElement.ShowColumnStripesAttributeName:
						showColumnStripes = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, showColumnStripes);
						break;

					case TableStyleInfoElement.ShowFirstColumnAttributeName:
						showFirstColumn = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, showFirstColumn);
						break;

					case TableStyleInfoElement.ShowLastColumnAttributeName:
						showLastColumn = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, showLastColumn);
						break;

					case TableStyleInfoElement.ShowRowStripesAttributeName:
						showRowStripes = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, showRowStripes);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			WorksheetTableStyle style = manager.Workbook.GetTableStyle(name);
			table.Style = style;
			table.DisplayBandedColumns = showColumnStripes;
			table.DisplayBandedRows = showRowStripes;
			table.DisplayFirstColumnFormatting = showFirstColumn;
			table.DisplayLastColumnFormatting = showLastColumn;
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if (table == null)
			{
				Utilities.DebugFail("Could not find the WorksheetTable on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'name' attribute
			attributeValue = XmlElementBase.GetXmlString(table.Style.Name, DataType.ST_Xstring, default(string), false);
			XmlElementBase.AddAttribute(element, TableStyleInfoElement.NameAttributeName, attributeValue);

			// Add the 'showColumnStripes' attribute
			attributeValue = XmlElementBase.GetXmlString(table.DisplayBandedColumns, DataType.Boolean, default(bool), false);
			XmlElementBase.AddAttribute(element, TableStyleInfoElement.ShowColumnStripesAttributeName, attributeValue);

			// Add the 'showFirstColumn' attribute
			attributeValue = XmlElementBase.GetXmlString(table.DisplayFirstColumnFormatting, DataType.Boolean, default(bool), false);
			XmlElementBase.AddAttribute(element, TableStyleInfoElement.ShowFirstColumnAttributeName, attributeValue);

			// Add the 'showLastColumn' attribute
			attributeValue = XmlElementBase.GetXmlString(table.DisplayLastColumnFormatting, DataType.Boolean, default(bool), false);
			XmlElementBase.AddAttribute(element, TableStyleInfoElement.ShowLastColumnAttributeName, attributeValue);

			// Add the 'showRowStripes' attribute
			attributeValue = XmlElementBase.GetXmlString(table.DisplayBandedRows, DataType.Boolean, default(bool), false);
			XmlElementBase.AddAttribute(element, TableStyleInfoElement.ShowRowStripesAttributeName, attributeValue);
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