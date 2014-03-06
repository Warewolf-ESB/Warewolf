using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class DynamicFilterElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_DynamicFilter">
		//  <attribute name="type" type="ST_DynamicFilterType" use="required"/>
		//  <attribute name="val" type="xsd:double" use="optional"/>
		//  <attribute name="maxVal" type="xsd:double" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "dynamicFilter";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			DynamicFilterElement.LocalName;

		private const string MaxValAttributeName = "maxVal";
		private const string TypeAttributeName = "type";
		private const string ValAttributeName = "val";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.dynamicFilter; }
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

			double? maxVal = null;
			ST_DynamicFilterType type = default(ST_DynamicFilterType);
			double? val = null;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case DynamicFilterElement.MaxValAttributeName:
						maxVal = (double)XmlElementBase.GetAttributeValue(attribute, DataType.Double, maxVal);
						break;

					case DynamicFilterElement.TypeAttributeName:
						type = (ST_DynamicFilterType)XmlElementBase.GetAttributeValue(attribute, DataType.ST_DynamicFilterType, type);
						break;

					case DynamicFilterElement.ValAttributeName:
						val = (double)XmlElementBase.GetAttributeValue(attribute, DataType.Double, val);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			filterContext.Filter = DynamicValuesFilter.CreateDynamicValuesFilter(manager, null, type, val, maxVal);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			DynamicValuesFilter filter = (DynamicValuesFilter)manager.ContextStack[typeof(DynamicValuesFilter)];
			if (filter == null)
			{
				Utilities.DebugFail("Cannot find the FixedValuesFilter on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'type' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.Type2007, DataType.ST_DynamicFilterType, default(ST_DynamicFilterType), true);
			XmlElementBase.AddAttribute(element, DynamicFilterElement.TypeAttributeName, attributeValue);

			DateRangeFilter dateRangeFilter = filter as DateRangeFilter;
			if (dateRangeFilter != null)
			{
				double? startValue = ExcelCalcValue.DateTimeToExcelDate(manager.Workbook, dateRangeFilter.Start);
				double? endValue = ExcelCalcValue.DateTimeToExcelDate(manager.Workbook, dateRangeFilter.End);
				if (startValue.HasValue && endValue.HasValue)
				{
					// Add the 'maxVal' attribute
					attributeValue = XmlElementBase.GetXmlString(endValue.Value, DataType.Double, default(double), false);
					XmlElementBase.AddAttribute(element, DynamicFilterElement.MaxValAttributeName, attributeValue);

					// Add the 'val' attribute
					attributeValue = XmlElementBase.GetXmlString(startValue.Value, DataType.Double, default(double), false);
					XmlElementBase.AddAttribute(element, DynamicFilterElement.ValAttributeName, attributeValue);
				}
				else
				{
					Utilities.DebugFail("Something is wrong here.");
				}
			}
			else
			{
				AverageFilter averageFilter = filter as AverageFilter;
				if (averageFilter != null)
				{
					// Add the 'val' attribute
					attributeValue = XmlElementBase.GetXmlString(averageFilter.Average, DataType.Double);
					XmlElementBase.AddAttribute(element, DynamicFilterElement.ValAttributeName, attributeValue);
				}
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