using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class DateGroupItemElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_DateGroupItem">
		//  <attribute name="year" type="xsd:unsignedShort" use="required"/>
		//  <attribute name="month" type="xsd:unsignedShort" use="optional"/>
		//  <attribute name="day" type="xsd:unsignedShort" use="optional"/>
		//  <attribute name="hour" type="xsd:unsignedShort" use="optional"/>
		//  <attribute name="minute" type="xsd:unsignedShort" use="optional"/>
		//  <attribute name="second" type="xsd:unsignedShort" use="optional"/>
		//  <attribute name="dateTimeGrouping" type="ST_DateTimeGrouping" use="required"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "dateGroupItem";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			DateGroupItemElement.LocalName;

		private const string DateTimeGroupingAttributeName = "dateTimeGrouping";
		private const string DayAttributeName = "day";
		private const string HourAttributeName = "hour";
		private const string MinuteAttributeName = "minute";
		private const string MonthAttributeName = "month";
		private const string SecondAttributeName = "second";
		private const string YearAttributeName = "year";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.dateGroupItem; }
		}

		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			FixedValuesFilter filter = (FixedValuesFilter)manager.ContextStack[typeof(FixedValuesFilter)];
			if (filter == null)
			{
				Utilities.DebugFail("Cannot find the FixedValuesFilter on the context stack.");
				return;
			}

			ST_DateTimeGrouping dateTimeGrouping = default(ST_DateTimeGrouping);
			ushort day = default(ushort);
			ushort hour = default(ushort);
			ushort minute = default(ushort);
			ushort month = default(ushort);
			ushort second = default(ushort);
			ushort year = default(ushort);

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case DateGroupItemElement.DateTimeGroupingAttributeName:
						dateTimeGrouping = (ST_DateTimeGrouping)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DateTimeGrouping, dateTimeGrouping);
						break;

					case DateGroupItemElement.DayAttributeName:
						day = (ushort)XmlElementBase.GetAttributeValue(attribute, DataType.UInt16, day);
						break;

					case DateGroupItemElement.HourAttributeName:
						hour = (ushort)XmlElementBase.GetAttributeValue(attribute, DataType.UInt16, hour);
						break;

					case DateGroupItemElement.MinuteAttributeName:
						minute = (ushort)XmlElementBase.GetAttributeValue(attribute, DataType.UInt16, minute);
						break;

					case DateGroupItemElement.MonthAttributeName:
						month = (ushort)XmlElementBase.GetAttributeValue(attribute, DataType.UInt16, month);
						break;

					case DateGroupItemElement.SecondAttributeName:
						second = (ushort)XmlElementBase.GetAttributeValue(attribute, DataType.UInt16, second);
						break;

					case DateGroupItemElement.YearAttributeName:
						year = (ushort)XmlElementBase.GetAttributeValue(attribute, DataType.UInt16, year);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			FixedDateGroup dateGroup = FixedDateGroup.CreateFixedDateGroup(dateTimeGrouping, year, month, day, hour, minute, second);

			if (dateGroup != null)
				filter.DateGroups.Add(dateGroup);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			EnumerableContext<FixedDateGroup> fixedDateGroups = (EnumerableContext<FixedDateGroup>)manager.ContextStack[typeof(EnumerableContext<FixedDateGroup>)];
			if (fixedDateGroups == null)
			{
				Utilities.DebugFail("Cannot find the EnumerableContext<FixedDateGroup> on the context stack.");
				return;
			}

			FixedDateGroup fixedDateGroup = fixedDateGroups.ConsumeCurrentItem();

			string attributeValue = String.Empty;

			// Add the 'dateTimeGrouping' attribute
			attributeValue = XmlElementBase.GetXmlString(fixedDateGroup.DateTimeGrouping, DataType.ST_DateTimeGrouping, default(ST_DateTimeGrouping), true);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.DateTimeGroupingAttributeName, attributeValue);

			// Add the 'year' attribute
			attributeValue = XmlElementBase.GetXmlString((ushort)fixedDateGroup.Value.Year, DataType.UInt16, default(ushort), true);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.YearAttributeName, attributeValue);

			if (fixedDateGroup.Type == FixedDateGroupType.Year)
				return;

			// Add the 'month' attribute
			attributeValue = XmlElementBase.GetXmlString((ushort)fixedDateGroup.Value.Month, DataType.UInt16, default(ushort), false);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.MonthAttributeName, attributeValue);

			if (fixedDateGroup.Type == FixedDateGroupType.Month)
				return;

			// Add the 'day' attribute
			attributeValue = XmlElementBase.GetXmlString((ushort)fixedDateGroup.Value.Day, DataType.UInt16, default(ushort), false);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.DayAttributeName, attributeValue);

			if (fixedDateGroup.Type == FixedDateGroupType.Day)
				return;

			// Add the 'hour' attribute
			attributeValue = XmlElementBase.GetXmlString((ushort)fixedDateGroup.Value.Hour, DataType.UInt16, default(ushort), false);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.HourAttributeName, attributeValue);

			if (fixedDateGroup.Type == FixedDateGroupType.Hour)
				return;

			// Add the 'minute' attribute
			attributeValue = XmlElementBase.GetXmlString((ushort)fixedDateGroup.Value.Minute, DataType.UInt16, default(ushort), false);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.MinuteAttributeName, attributeValue);

			if (fixedDateGroup.Type == FixedDateGroupType.Minute)
				return;

			// Add the 'second' attribute
			attributeValue = XmlElementBase.GetXmlString((ushort)fixedDateGroup.Value.Second, DataType.UInt16, default(ushort), false);
			XmlElementBase.AddAttribute(element, DateGroupItemElement.SecondAttributeName, attributeValue);

			if (fixedDateGroup.Type == FixedDateGroupType.Second)
				return;

			Utilities.DebugFail("Unknown FixedDateGroupType: " + fixedDateGroup.Type);
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