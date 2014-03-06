using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class FiltersElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Filters">
		//  <sequence>
		//    <element name="filter" type="CT_Filter" minOccurs="0" maxOccurs="unbounded"/>
		//    <element name="dateGroupItem" type="CT_DateGroupItem" minOccurs="0" maxOccurs="unbounded"/>
		//  </sequence>
		//  <attribute name="blank" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="calendarType" type="ST_CalendarType" use="optional" default="none"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "filters";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			FiltersElement.LocalName;

		private const string BlankAttributeName = "blank";
		private const string CalendarTypeAttributeName = "calendarType";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.filters; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			FilterColumnElementContext filterContext = (FilterColumnElementContext)manager.ContextStack[typeof(FilterColumnElementContext)];
			if (filterContext == null)
			{
				Utilities.DebugFail("Cannot find the FilterColumnElementContext on the context stack.");
				return;
			}

			bool blank = false;
			ST_CalendarType calendarType = ST_CalendarType.none;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case FiltersElement.BlankAttributeName:
						blank = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, blank);
						break;

					case FiltersElement.CalendarTypeAttributeName:
						calendarType = (ST_CalendarType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_CalendarType, calendarType);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			FixedValuesFilter filter = new FixedValuesFilter(null);
			filter.CalendarType = (CalendarType)calendarType;
			filter.IncludeBlanks = blank;

			filterContext.Filter = filter;
			manager.ContextStack.Push(filter);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			FixedValuesFilter filter = (FixedValuesFilter)manager.ContextStack[typeof(FixedValuesFilter)];
			if (filter == null)
			{
				Utilities.DebugFail("Cannot find the FixedValuesFilter on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'blank' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.IncludeBlanks, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, FiltersElement.BlankAttributeName, attributeValue);

			// Add the 'calendarType' attribute
			attributeValue = XmlElementBase.GetXmlString((ST_CalendarType)filter.CalendarType, DataType.ST_CalendarType, ST_CalendarType.none, false);
			XmlElementBase.AddAttribute(element, FiltersElement.CalendarTypeAttributeName, attributeValue);

			if (filter.DisplayValues.Count != 0)
			{
				manager.ContextStack.Push(new EnumerableContext<string>(filter.DisplayValues));

				// Add the 'filter' element
				XmlElementBase.AddElements(element, FilterElement.QualifiedName, filter.DisplayValues.Count);
			}

			if (filter.DateGroups.Count != 0)
			{
				manager.ContextStack.Push(new EnumerableContext<FixedDateGroup>(filter.DateGroups));

				// Add the 'dateGroupItem' element
				XmlElementBase.AddElements(element, DateGroupItemElement.QualifiedName, filter.DateGroups.Count);
			}
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