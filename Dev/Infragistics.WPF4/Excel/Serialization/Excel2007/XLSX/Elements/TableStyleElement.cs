using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	// Rewrote this class because it is now supported.
	internal class TableStyleElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TableStyle">
		//  <sequence>
		//    <element name="tableStyleElement" type="CT_TableStyleElement" minOccurs="0" maxOccurs="unbounded"/>
		//  </sequence>
		//  <attribute name="name" type="xsd:string" use="required"/>
		//  <attribute name="pivot" type="xsd:boolean" use="optional" default="true"/>
		//  <attribute name="table" type="xsd:boolean" use="optional" default="true"/>
		//  <attribute name="count" type="xsd:unsignedInt" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "tableStyle";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TableStyleElement.LocalName;

		private const string CountAttributeName = "count";
		private const string NameAttributeName = "name";
		private const string PivotAttributeName = "pivot";
		private const string TableAttributeName = "table";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.tableStyle; }
		}

		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			uint count = default(uint);
			string name = string.Empty;
			bool pivot = true;
			bool table = true;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableStyleElement.CountAttributeName:
						count = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, count);
						break;

					case TableStyleElement.NameAttributeName:
						name = (string)XmlElementBase.GetAttributeValue(attribute, DataType.String, name);
						break;

					case TableStyleElement.PivotAttributeName:
						pivot = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, pivot);
						break;

					case TableStyleElement.TableAttributeName:
						table = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, table);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			if (manager.IsLoadingPresetTableStyles == false)
			{
				if (table)
				{
					WorksheetTableStyle tableStyle = new WorksheetTableStyle(name, true);
					manager.Workbook.CustomTableStyles.Add(tableStyle);
					manager.ContextStack.Push(tableStyle);
				}
			}
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			EnumerableContext<WorksheetTableStyle> tableStyles = (EnumerableContext<WorksheetTableStyle>)manager.ContextStack[typeof(EnumerableContext<WorksheetTableStyle>)];
			if (tableStyles == null)
			{
				Utilities.DebugFail("Cannot find the EnumerableContext<WorksheetTableStyle> on the context stack.");
				return;
			}

			WorksheetTableStyle tableStyle = tableStyles.ConsumeCurrentItem();
			manager.ContextStack.Push(tableStyle);

			string attributeValue = String.Empty;

			// Add the 'count' attribute
			attributeValue = XmlElementBase.GetXmlString((uint)tableStyle.DxfIdsByAreaDuringSave.Count, DataType.UInt32, default(uint), false);
			XmlElementBase.AddAttribute(element, TableStyleElement.CountAttributeName, attributeValue);

			// Add the 'name' attribute
			attributeValue = XmlElementBase.GetXmlString(tableStyle.Name, DataType.String, default(string), true);
			XmlElementBase.AddAttribute(element, TableStyleElement.NameAttributeName, attributeValue);

			// Add the 'pivot' attribute
			attributeValue = XmlElementBase.GetXmlString(false, DataType.Boolean, true, false);
			XmlElementBase.AddAttribute(element, TableStyleElement.PivotAttributeName, attributeValue);

			// Add the 'table' attribute
			attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean, true, false);
			XmlElementBase.AddAttribute(element, TableStyleElement.TableAttributeName, attributeValue);


			manager.ContextStack.Push(new DictionaryContext<WorksheetTableStyleArea, uint>(tableStyle.DxfIdsByAreaDuringSave));

			// Add the 'tableStyleElement' element
			XmlElementBase.AddElements(element, TableStyleElementElement.QualifiedName, tableStyle.DxfIdsByAreaDuringSave.Count);
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