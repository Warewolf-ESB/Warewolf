using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class CustomFilterElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_CustomFilter">
		//  <attribute name="operator" type="ST_FilterOperator" default="equal" use="optional"/>
		//  <attribute name="val" type="ST_Xstring"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "customFilter";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CustomFilterElement.LocalName;

		private const string OperatorAttributeName = "operator";
		private const string ValAttributeName = "val";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.customFilter; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			CustomFilterContext customFilterContext = (CustomFilterContext)manager.ContextStack[typeof(CustomFilterContext)];
			if (customFilterContext == null)
			{
				Utilities.DebugFail("Cannot find the CustomFilterContext on the context stack.");
				return;
			}

			ST_FilterOperator operatorValue = ST_FilterOperator.equal;
			string val = string.Empty;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case CustomFilterElement.OperatorAttributeName:
						operatorValue = (ST_FilterOperator)XmlElementBase.GetAttributeValue(attribute, DataType.ST_FilterOperator, operatorValue);
						break;

					case CustomFilterElement.ValAttributeName:
						val = (string)XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, val);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			object conditionValue;

			double numericValue;
			if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out numericValue))
				conditionValue = numericValue;
			else
				conditionValue = val;

			CustomFilterCondition filterCondition = CustomFilterCondition.CreateCustomFilterCondition(operatorValue, conditionValue);
			if (filterCondition != null)
				customFilterContext.AddCondition(filterCondition);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			ListContext<CustomFilterCondition> conditions = (ListContext<CustomFilterCondition>)manager.ContextStack[typeof(ListContext<CustomFilterCondition>)];
			if (conditions == null)
			{
				Utilities.DebugFail("Cannot find the ListContext<CustomFilterCondition> on the context stack.");
				return;
			}

			CustomFilterCondition condition = conditions.ConsumeCurrentItem();

			ST_FilterOperator operatorValue;
			object val;
			condition.GetSaveValues(manager, out operatorValue, out val);

			string attributeValue = String.Empty;

			// Add the 'operator' attribute
			attributeValue = XmlElementBase.GetXmlString(operatorValue, DataType.ST_FilterOperator, ST_FilterOperator.equal, false);
			XmlElementBase.AddAttribute(element, CustomFilterElement.OperatorAttributeName, attributeValue);

			// Add the 'val' attribute
			attributeValue = XmlElementBase.GetXmlString(val.ToString(), DataType.ST_Xstring, default(string), false);
			XmlElementBase.AddAttribute(element, CustomFilterElement.ValAttributeName, attributeValue);
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