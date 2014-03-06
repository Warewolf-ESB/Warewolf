using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class Top10Element : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_Top10">
		//  <attribute name="top" type="xsd:boolean" use="optional" default="true"/>
		//  <attribute name="percent" type="xsd:boolean" use="optional" default="false"/>
		//  <attribute name="val" type="xsd:double" use="required"/>
		//  <attribute name="filterVal" type="xsd:double" use="optional"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "top10";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			Top10Element.LocalName;

		private const string FilterValAttributeName = "filterVal";
		private const string PercentAttributeName = "percent";
		private const string TopAttributeName = "top";
		private const string ValAttributeName = "val";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.top10; }
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

			double filterVal = default(double);
			bool percent = false;
			bool top = true;
			double val = double.NaN;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case Top10Element.FilterValAttributeName:
						filterVal = (double)XmlElementBase.GetAttributeValue(attribute, DataType.Double, filterVal);
						break;

					case Top10Element.PercentAttributeName:
						percent = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, percent);
						break;

					case Top10Element.TopAttributeName:
						top = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, top);
						break;

					case Top10Element.ValAttributeName:
						val = (double)XmlElementBase.GetAttributeValue(attribute, DataType.Double, val);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			TopOrBottomFilterType filterType = TopOrBottomFilter.GetFilterType(percent, top);
			filterContext.Filter = new TopOrBottomFilter(null, filterType, (int)val, filterVal);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			TopOrBottomFilter filter = (TopOrBottomFilter)manager.ContextStack[typeof(TopOrBottomFilter)];
			if (filter == null)
			{
				Utilities.DebugFail("Cannot find the TopOrBottomFilter on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'filterVal' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.ReferenceValue, DataType.Double);
			XmlElementBase.AddAttribute(element, Top10Element.FilterValAttributeName, attributeValue);

			// Add the 'percent' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.IsPercent, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, Top10Element.PercentAttributeName, attributeValue);

			// Add the 'top' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.IsTop, DataType.Boolean, true, false);
			XmlElementBase.AddAttribute(element, Top10Element.TopAttributeName, attributeValue);

			// Add the 'val' attribute
			attributeValue = XmlElementBase.GetXmlString((double)filter.Value, DataType.Double, default(double), true);
			XmlElementBase.AddAttribute(element, Top10Element.ValAttributeName, attributeValue);
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