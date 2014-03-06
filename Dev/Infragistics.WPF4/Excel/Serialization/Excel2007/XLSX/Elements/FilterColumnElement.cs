using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class FilterColumnElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_FilterColumn">
		//  <choice minOccurs="0" maxOccurs="1">
		//    <element name="filters" type="CT_Filters" minOccurs="0" maxOccurs="1"/>
		//    <element name="top10" type="CT_Top10" minOccurs="0" maxOccurs="1"/>
		//    <element name="customFilters" type="CT_CustomFilters" minOccurs="0" maxOccurs="1"/>
		//    <element name="dynamicFilter" type="CT_DynamicFilter" minOccurs="0" maxOccurs="1"/>
		//    <element name="colorFilter" type="CT_ColorFilter" minOccurs="0" maxOccurs="1"/>
		//    <element name="iconFilter" minOccurs="0" maxOccurs="1" type="CT_IconFilter"/>
		//    <element name="extLst" type="CT_ExtensionList" minOccurs="0" maxOccurs="1"/>
		//  </choice>
		//  <attribute name="colId" type="xsd:unsignedInt" use="required"/>
		//  <attribute name="hiddenButton" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="showButton" type="xsd:boolean" use="optional" default="true"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "filterColumn";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			FilterColumnElement.LocalName;

		private const string ColIdAttributeName = "colId";
		private const string HiddenButtonAttributeName = "hiddenButton";
		private const string ShowButtonAttributeName = "showButton";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.filterColumn; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			uint colId = default(uint);
			bool hiddenButton = false;
			bool showButton = true;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case FilterColumnElement.ColIdAttributeName:
						colId = (uint)XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, colId);
						break;

					case FilterColumnElement.HiddenButtonAttributeName:
						hiddenButton = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, hiddenButton);
						break;

					case FilterColumnElement.ShowButtonAttributeName:
						showButton = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, showButton);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			Debug.Assert(hiddenButton == false && showButton, "Implement the loading of these values.");
			manager.ContextStack.Push(new FilterColumnElementContext((int)colId));
		}

		#endregion // Load

		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			FilterColumnElementContext filterContext = (FilterColumnElementContext)manager.ContextStack[typeof(FilterColumnElementContext)];
			if (filterContext == null)
			{
				Utilities.DebugFail("Cannot find the FilterColumnElementContext on the context stack.");
				return;
			}

			TableContext tableContext = (TableContext)manager.ContextStack[typeof(TableContext)];
			if (tableContext == null)
			{
				Utilities.DebugFail("Cannot find the TableContext on the context stack.");
				return;
			}

			tableContext.AddFilter(filterContext.ColumnIndex, filterContext.Filter);
		}

		#endregion // OnAfterLoadChildElements

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			ListContext<WorksheetTableColumn> filteredColumns = (ListContext<WorksheetTableColumn>)manager.ContextStack[typeof(ListContext<WorksheetTableColumn>)];
			if (filteredColumns == null)
			{
				Utilities.DebugFail("Cannot find the ListContext<WorksheetTableColumn> on the context stack.");
				return;
			}

			WorksheetTableColumn column = filteredColumns.ConsumeCurrentItem();
			if (column.Filter == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTableColumn on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'colId' attribute
			attributeValue = XmlElementBase.GetXmlString((uint)column.Index, DataType.UInt32, default(uint), true);
			XmlElementBase.AddAttribute(element, FilterColumnElement.ColIdAttributeName, attributeValue);

			// Add the 'hiddenButton' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.Boolean, false, false);
			//XmlElementBase.AddAttribute(element, FilterColumnElement.HiddenButtonAttributeName, attributeValue);

			// Add the 'showButton' attribute
			//attributeValue = XmlElementBase.GetXmlString(, DataType.Boolean, true, false);
			//XmlElementBase.AddAttribute(element, FilterColumnElement.ShowButtonAttributeName, attributeValue);

			manager.ContextStack.Push(column.Filter);
			if (column.Filter is FixedValuesFilter)
			{
				// Add the 'filters' element
				XmlElementBase.AddElement(element, FiltersElement.QualifiedName);
			}
			else if (column.Filter is TopOrBottomFilter)
			{
				// Add the 'top10' element
				XmlElementBase.AddElement(element, Top10Element.QualifiedName);
			}
			else if (column.Filter is CustomFilter)
			{
				// Add the 'customFilters' element
				XmlElementBase.AddElement(element, CustomFiltersElement.QualifiedName);
			}
			else if (column.Filter is DynamicValuesFilter)
			{
				// Add the 'dynamicFilter' element
				XmlElementBase.AddElement(element, DynamicFilterElement.QualifiedName);
			}
			else if (column.Filter is IColorFilter)
			{
				// Add the 'colorFilter' element
				XmlElementBase.AddElement(element, ColorFilterElement.QualifiedName);
			}
			else if (column.Filter is IconFilter)
			{
				// Add the 'iconFilter' element
				XmlElementBase.AddElement(element, IconFilterElement.QualifiedName);
			}
			//else if(...)
			//{
			//    // Add the 'extLst' element
			//    XmlElementBase.AddElement(element, ExtLstElement.QualifiedName);
			//}
			else
			{
				Utilities.DebugFail("Not sure which element to write out.");
			}
		}

		#endregion // Save

		#endregion // Base Class Overrides
	}

	internal class FilterColumnElementContext
	{
		private readonly int columnIndex;
		private Filter filter;

		public FilterColumnElementContext(int columnIndex)
		{
			this.columnIndex = columnIndex;
		}

		public int ColumnIndex
		{
			get { return this.columnIndex; }
		}

		public Filter Filter
		{
			get { return this.filter; }
			set
			{
				Debug.Assert(this.filter == null, "We shouldn't load two filters for the same column.");
				this.filter = value;
			}
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