using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	// Rewrote this class because it is now supported.
	internal class TableStylesElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_TableStyles">
		//  <sequence>
		//    <element name="tableStyle" type="CT_TableStyle" minOccurs="0" maxOccurs="unbounded"/>
		//  </sequence>
		//  <attribute name="count" type="xsd:unsignedInt" use="optional"/>
		//  <attribute name="defaultTableStyle" type="xsd:string" use="optional"/>
		//  <attribute name="defaultPivotStyle" type="xsd:string" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "tableStyles";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			TableStylesElement.LocalName;

		private const string CountAttributeName = "count";
		private const string DefaultPivotStyleAttributeName = "defaultPivotStyle";
		private const string DefaultTableStyleAttributeName = "defaultTableStyle";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.tableStyles; }
		}

		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			uint count = default(uint);
			string defaultPivotStyle = default(string);
			string defaultTableStyle = default(string);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableStylesElement.CountAttributeName:
						count = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, count);
						break;

					case TableStylesElement.DefaultPivotStyleAttributeName:
						defaultPivotStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.String, defaultPivotStyle);
						break;

					case TableStylesElement.DefaultTableStyleAttributeName:
						defaultTableStyle = (string)XmlElementBase.GetAttributeValue(attribute, DataType.String, defaultTableStyle);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			manager.ContextStack.Push(defaultTableStyle);
		}

		#endregion // Load

		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			base.OnAfterLoadChildElements(manager, elementCache);

			if (manager.IsLoadingPresetTableStyles)
				return;

			string defaultTableStyle = (string)manager.ContextStack[typeof(string)];
			if (defaultTableStyle == null)
				return;

			WorksheetTableStyle tableStyle = manager.Workbook.GetTableStyle(defaultTableStyle);
			if (tableStyle == null)
			{
				Utilities.DebugFail("Cannot find the default table style in the workbook.");
				return;
			}

			manager.Workbook.DefaultTableStyle = tableStyle;
		}

		#endregion // OnAfterLoadChildElements

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			string attributeValue = String.Empty;

			CustomTableStyleCollection customTableStyles = manager.Workbook.CustomTableStyles;

			// Add the 'count' attribute
			attributeValue = XmlElementBase.GetXmlString((uint)customTableStyles.Count, DataType.UInt32);
			XmlElementBase.AddAttribute(element, TableStylesElement.CountAttributeName, attributeValue);

			// Add the 'defaultPivotStyle' attribute
			attributeValue = XmlElementBase.GetXmlString("PivotStyleLight16", DataType.String, default(string), false);
			XmlElementBase.AddAttribute(element, TableStylesElement.DefaultPivotStyleAttributeName, attributeValue);

			// Add the 'defaultTableStyle' attribute
			attributeValue = XmlElementBase.GetXmlString(manager.Workbook.DefaultTableStyle.Name, DataType.String, default(string), false);
			XmlElementBase.AddAttribute(element, TableStylesElement.DefaultTableStyleAttributeName, attributeValue);

			manager.ContextStack.Push(new EnumerableContext<WorksheetTableStyle>(customTableStyles));

			// Add the 'tableStyle' element
			XmlElementBase.AddElements(element, TableStyleElement.QualifiedName, customTableStyles.Count);
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