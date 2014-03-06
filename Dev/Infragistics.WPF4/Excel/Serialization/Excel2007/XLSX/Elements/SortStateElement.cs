using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Sorting;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class SortStateElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_SortState">
		//  <sequence>
		//    <element name="sortCondition" minOccurs="0" maxOccurs="64" type="CT_SortCondition"/>
		//    <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//  </sequence>
		//  <attribute name="columnSort" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="caseSensitive" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="sortMethod" type="ST_SortMethod" use="optional" default="none"/>
		//  <attribute name="ref" type="ST_Ref" use="required"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "sortState";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			SortStateElement.LocalName;

		private const string CaseSensitiveAttributeName = "caseSensitive";
		private const string ColumnSortAttributeName = "columnSort";
		private const string RefAttributeName = "ref";
		private const string SortMethodAttributeName = "sortMethod";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.sortState; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			bool caseSensitive = false;
			bool columnSort = false;
			string refValue = string.Empty;
			ST_SortMethod sortMethod = ST_SortMethod.none;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case SortStateElement.CaseSensitiveAttributeName:
						caseSensitive = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, caseSensitive);
						break;

					case SortStateElement.ColumnSortAttributeName:
						columnSort = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, columnSort);
						break;

					case SortStateElement.RefAttributeName:
						refValue = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Ref, refValue);
						break;

					case SortStateElement.SortMethodAttributeName:
						sortMethod = (ST_SortMethod)XmlElementBase.GetAttributeValue(attribute, DataType.ST_SortMethod, sortMethod);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			if (String.IsNullOrEmpty(refValue))
			{
				Utilities.DebugFail("There was no ref attribute in the sortState element.");
				return;
			}

			

			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if (table == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTable on the context stack.");
				return;
			}
			else
			{
				Debug.Assert(columnSort == false, "This is unexpected.");

				table.SortSettings.CaseSensitive = caseSensitive;
				table.SortSettings.SortMethod = (SortMethod)sortMethod;
			}
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			WorksheetTable table = (WorksheetTable)manager.ContextStack[typeof(WorksheetTable)];
			if (table == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTable on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'caseSensitive' attribute
			attributeValue = XmlElementBase.GetXmlString(table.SortSettings.CaseSensitive, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, SortStateElement.CaseSensitiveAttributeName, attributeValue);

			// Add the 'columnSort' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.Boolean, false, false);
			//XmlElementBase.AddAttribute(element, SortStateElement.ColumnSortAttributeName, attributeValue);

			// Add the 'ref' attribute
			attributeValue = XmlElementBase.GetXmlString(table.SortAndHeadersRegion.ToString(CellReferenceMode.A1, false, true, true), DataType.ST_Ref, default(string), true);
			XmlElementBase.AddAttribute(element, SortStateElement.RefAttributeName, attributeValue);

			// Add the 'sortMethod' attribute
			// This seems to be a bug with MS Excel. If we write out the sortMethod attribute with the pinYin method value,
			// Excel uses stroke instead. It seems the presence of the attribute tells Excel to use the alternate sort method.
			if (table.SortSettings.HasAlternateSortMethod)
			{
				attributeValue = XmlElementBase.GetXmlString((ST_SortMethod)table.SortSettings.SortMethod, DataType.ST_SortMethod, ST_SortMethod.none, false);
				XmlElementBase.AddAttribute(element, SortStateElement.SortMethodAttributeName, attributeValue);
			}

			// Add the 'sortCondition' elements
			manager.ContextStack.Push(new EnumerableContext<KeyValuePair<WorksheetTableColumn, SortCondition>>(table.SortSettings.SortConditions));
			XmlElementBase.AddElements(element, SortConditionElement.QualifiedName, table.SortSettings.SortConditions.Count);

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