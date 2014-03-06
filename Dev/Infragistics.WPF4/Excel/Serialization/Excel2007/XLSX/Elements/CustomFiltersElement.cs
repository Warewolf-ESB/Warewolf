using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Infragistics.Documents.Excel.Filtering;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal class CustomFiltersElement : XLSXElementBase
	{
		#region XML Schema Fragment

		//<complexType name="CT_CustomFilters">
		//  <sequence>
		//    <element name="customFilter" type="CT_CustomFilter" minOccurs="1" maxOccurs="2"/>
		//  </sequence>
		//  <attribute name="and" type="xsd:boolean" use="optional" default="false"/>
		//</complexType>

		#endregion // Xml Schema Fragment

		#region Constants






		public const string LocalName = "customFilters";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CustomFiltersElement.LocalName;

		private const string AndAttributeName = "and";

		#endregion // Constants

		#region Base Class Overrides

		#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.customFilters; }
		}
		
		#endregion // Type

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			bool and = false;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case CustomFiltersElement.AndAttributeName:
						and = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, and);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			manager.ContextStack.Push(new CustomFilterContext(and ? ConditionalOperator.And : ConditionalOperator.Or));
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

			CustomFilterContext customFilterContext = (CustomFilterContext)manager.ContextStack[typeof(CustomFilterContext)];
			if (customFilterContext == null)
			{
				Utilities.DebugFail("Cannot find the CustomFilterContext on the context stack.");
				return;
			}

			filterContext.Filter = customFilterContext.CreateFilter();
		}

		#endregion // OnAfterLoadChildElements

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			CustomFilter filter = (CustomFilter)manager.ContextStack[typeof(CustomFilter)];
			if (filter == null)
			{
				Utilities.DebugFail("Cannot find the CustomFilterContext on the context stack.");
				return;
			}

			string attributeValue = String.Empty;

			// Add the 'and' attribute
			attributeValue = XmlElementBase.GetXmlString(filter.ConditionalOperator == ConditionalOperator.And, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, CustomFiltersElement.AndAttributeName, attributeValue);

			List<CustomFilterCondition> conditions = new List<CustomFilterCondition>();
			conditions.Add(filter.Condition1);
			if (filter.Condition2 != null)
				conditions.Add(filter.Condition2);

			manager.ContextStack.Push(new ListContext<CustomFilterCondition>(conditions));

			// Add the 'customFilter' element
			XmlElementBase.AddElements(element, CustomFilterElement.QualifiedName, conditions.Count);
		}

		#endregion // Save

		#endregion // Base Class Overrides
	}

	internal class CustomFilterContext
	{
		private readonly ConditionalOperator conditionalOperator;
		private CustomFilterCondition condition1;
		private CustomFilterCondition condition2;

		public CustomFilterContext(ConditionalOperator conditionalOperator)
		{
			this.conditionalOperator = conditionalOperator;
		}

		public void AddCondition(CustomFilterCondition condition)
		{
			if (this.condition1 == null)
			{
				this.condition1 = condition;
				return;
			}

			if (this.condition2 == null)
			{
				this.condition2 = condition;
				return;
			}

			Utilities.DebugFail("There are more than 2 conditions.");
		}

		public CustomFilter CreateFilter()
		{
			if (this.condition1 == null && this.condition2 == null)
			{
				Utilities.DebugFail("There are no conditions.");
				return null;
			}

			return new CustomFilter(null, this.condition1, this.condition2, this.conditionalOperator);
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