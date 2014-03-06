using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 5/13/11 - Data Validations / Page Breaks
	internal class DataValidationElement : XLSXElementBase
	{
		#region XML Schema fragment <docs>

		//<complexType name="CT_DataValidation">
		//<sequence>
		//<element name="formula1" type="ST_Formula" minOccurs="0" maxOccurs="1"/>
		//<element name="formula2" type="ST_Formula" minOccurs="0" maxOccurs="1"/>
		//</sequence>
		//<attribute name="type" type="ST_DataValidationType" use="optional" default="none"/>
		//<attribute name="errorStyle" type="ST_DataValidationErrorStyle" use="optional" default="stop"/>
		//<attribute name="imeMode" type="ST_DataValidationImeMode" use="optional" default="noControl"/>
		//<attribute name="operator" type="ST_DataValidationOperator" use="optional" default="between"/>
		//<attribute name="allowBlank" type="xsd:boolean" use="optional" default="false"/>
		//<attribute name="showDropDown" type="xsd:boolean" use="optional" default="false"/>
		//<attribute name="showInputMessage" type="xsd:boolean" use="optional" default="false"/>
		//<attribute name="showErrorMessage" type="xsd:boolean" use="optional" default="false"/>
		//<attribute name="errorTitle" type="ST_Xstring" use="optional"/>
		//<attribute name="error" type="ST_Xstring" use="optional"/>
		//<attribute name="promptTitle" type="ST_Xstring" use="optional"/>
		//<attribute name="prompt" type="ST_Xstring" use="optional"/>
		//<attribute name="sqref" type="ST_Sqref" use="required"/>
		//</complexType> 

		#endregion // XML Schema fragment <docs>

		#region Constants






		public const string LocalName = "dataValidation";






		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			DataValidationElement.LocalName;

		public const string AllowBlankAttributeName = "allowBlank";
		public const string ErrorAttributeName = "error";
		public const string ErrorStyleAttributeName = "errorStyle";
		public const string ErrorTitleAttributeName = "errorTitle";
		public const string IMEModeAttributeName = "imeMode";
		public const string OperatorAttributeName = "operator";
		public const string PromptAttributeName = "prompt";
		public const string PromptTitleAttributeName = "promptTitle";
		public const string ShowDropDownAttributeName = "showDropDown";
		public const string ShowErrorMessageAttributeName = "showErrorMessage";
		public const string ShowInputMessageAttributeName = "showInputMessage";
		public const string SqrefAttributeName = "sqref";
		public const string TypeAttributeName = "type";

		private const string DescriptionNewline = "_x000a_";

		#endregion //Constants

		#region Base class overrides

		#region Type
		/// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
		/// </summary>
		public override XLSXElementType Type
		{
			get { return XLSXElementType.dataValidation; }
		}
		#endregion Type

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			Worksheet worksheet = (Worksheet)manager.ContextStack[typeof(Worksheet)];

			if (worksheet == null)
			{
				Utilities.DebugFail("Could not get the worksheet off of the context stack");
				return;
			}

			bool allowBlank = false;
			ST_DataValidationErrorStyle errorStyle = ST_DataValidationErrorStyle.stop;
			string error = null;
			string errorTitle = null;
			ST_DataValidationOperator _operator = ST_DataValidationOperator.between;
			string prompt = null;
			string promptTitle = null;
			bool showDropDown = false;
			bool showErrorMessage = false;
			bool showInputMessage = false;
			string sqref = null;
			ST_DataValidationType type = ST_DataValidationType.none;

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case DataValidationElement.AllowBlankAttributeName:
						allowBlank = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
						break;

					case DataValidationElement.IMEModeAttributeName:
						// Roundtrip - page 1966
						//imeMode = (ST_DataValidationImeMode)XmlElementBase.GetValue(attribute.Value, DataType.ST_DataValidationImeMode, ST_DataValidationImeMode.noControl);
						break;

					case DataValidationElement.ErrorStyleAttributeName:
						errorStyle = (ST_DataValidationErrorStyle)XmlElementBase.GetValue(attribute.Value, DataType.ST_DataValidationErrorStyle, ST_DataValidationErrorStyle.stop);
						break;

					case DataValidationElement.ErrorAttributeName:
						error = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Xstring, string.Empty);
						break;

					case DataValidationElement.ErrorTitleAttributeName:
						errorTitle = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Xstring, string.Empty);
						break;

					case DataValidationElement.OperatorAttributeName:
						_operator = (ST_DataValidationOperator)XmlElementBase.GetValue(attribute.Value, DataType.ST_DataValidationOperator, ST_DataValidationOperator.between);
						break;

					case DataValidationElement.PromptAttributeName:
						prompt = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Xstring, string.Empty);
						break;

					case DataValidationElement.PromptTitleAttributeName:
						promptTitle = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Xstring, string.Empty);
						break;

					case DataValidationElement.ShowDropDownAttributeName:
						showDropDown = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
						break;

					case DataValidationElement.ShowErrorMessageAttributeName:
						showErrorMessage = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
						break;

					case DataValidationElement.ShowInputMessageAttributeName:
						showInputMessage = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
						break;

					case DataValidationElement.SqrefAttributeName:
						sqref = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Sqref, string.Empty);
						break;

					case DataValidationElement.TypeAttributeName:
						type = (ST_DataValidationType)XmlElementBase.GetValue(attribute.Value, DataType.ST_DataValidationType, ST_DataValidationType.none);
						break;

					default:
						Utilities.DebugFail("Unknown attribute");
						break;
				}
			}

			DataValidationRule rule;

			switch (type)
			{
				case ST_DataValidationType.none:
					rule = new AnyValueDataValidationRule();
					break;

				case ST_DataValidationType.list:
					ListDataValidationRule listRule = new ListDataValidationRule();

					// This value seems to be defined incorrectly by the XML schema. When the drop down should not be shown, the value is true. 
					// When it should be shown it is omitted because the default value is false.
					listRule.ShowDropdown = (showDropDown == false); 

					rule = listRule;
					break;

				case ST_DataValidationType.custom:
					rule = new CustomDataValidationRule();
					break;

				case ST_DataValidationType.date:
				case ST_DataValidationType._decimal:
				case ST_DataValidationType.textLength:
				case ST_DataValidationType.time:
				case ST_DataValidationType.whole:
					switch (_operator)
					{
						case ST_DataValidationOperator.between:
						case ST_DataValidationOperator.notBetween:
							rule = new TwoConstraintDataValidationRule((TwoConstraintDataValidationOperator)_operator, (DataValidationCriteria)type);
							break;

						case ST_DataValidationOperator.equal:
						case ST_DataValidationOperator.notEqual:
						case ST_DataValidationOperator.greaterThan:
						case ST_DataValidationOperator.lessThan:
						case ST_DataValidationOperator.greaterThanOrEqual:
						case ST_DataValidationOperator.lessThanOrEqual:
							rule = new OneConstraintDataValidationRule((OneConstraintDataValidationOperator)_operator, (DataValidationCriteria)type);
							break;

						default:
							Utilities.DebugFail("Unknown ST_DataValidationOperator: " + _operator);
							return;
					}
					break;

				default:
					Utilities.DebugFail("Unknown ST_DataValidationType: " + type);
					return;
			}

			// MD 1/5/12 - TFS98535
			// The newlines are escaped in these descriptions, so we should un-escape those newlines.
			error = DataValidationElement.UnescapeDescription(error);
			prompt = DataValidationElement.UnescapeDescription(prompt);

			rule.AllowNullInternal = allowBlank;
			rule.ErrorStyle = (DataValidationErrorStyle)errorStyle;
			rule.ShowInputMessage = showInputMessage;
			rule.ShowErrorMessageForInvalidValue = showErrorMessage;
			rule.InputMessageTitle = promptTitle;
			rule.InputMessageDescription = prompt;
			rule.ErrorMessageTitle = errorTitle;
			rule.ErrorMessageDescription = error;

			// MD 1/23/12
			// Found while fixing TFS99849
			// We need to always parse the sqref with the A1 mode, not the worksheet's mode.
			//WorksheetReferenceCollection references = new WorksheetReferenceCollection(worksheet, sqref);
			WorksheetReferenceCollection references = new WorksheetReferenceCollection(worksheet);
			references.Add(sqref, CellReferenceMode.A1);

			worksheet.DataValidationRules.AddInternal(rule, references);

			manager.ContextStack.Push(rule);

			WorksheetCell rootCell;
			if (references.Regions.Count == 1 && sqref.IndexOf(' ') < 0)
			{
				// MD 3/13/12 - 12.1 - Table Support
				//rootCell = references.Regions[0].TopLeftCell;
				rootCell = references.TopLeftCell;
			}
			else
			{
				// MD 8/6/12 - TFS118386
				// If we're going to specify the cell in the A1 mode, make sure we always parse in the A1 cell reference mode.
				//rootCell = worksheet.GetCell("A1");
				rootCell = worksheet.GetCell("A1", CellReferenceMode.A1);
			}

			manager.ContextStack.Push(rootCell);
		}

		#endregion Load

		#region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			DictionaryContext<DataValidationRule, WorksheetReferenceCollection> context =
				(DictionaryContext<DataValidationRule, WorksheetReferenceCollection>)manager.ContextStack[typeof(DictionaryContext<DataValidationRule, WorksheetReferenceCollection>)];

			if (context == null)
			{
				Utilities.DebugFail("Could not get the dictionary context off of the context stack.");
				return;
			}

			KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair = context.ConsumeCurrentItem();

			DataValidationRule rule = pair.Key;
			ListDataValidationRule listRule = rule as ListDataValidationRule;
			WorksheetReferenceCollection references = pair.Value;

			if (rule == null)
			{
				Utilities.DebugFail("The current item is null.");
				return;
			}

			string attributeValue = null;

			attributeValue = XmlElementBase.GetXmlString(rule.ValidationType, DataType.ST_DataValidationType, DataValidationType.AnyValue, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.TypeAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.ErrorStyle, DataType.ST_DataValidationErrorStyle, DataValidationErrorStyle.Stop, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.ErrorStyleAttributeName, attributeValue);

			

			attributeValue = XmlElementBase.GetXmlString(rule.OperatorType, DataType.ST_DataValidationOperator, DataValidationOperatorType.Between, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.OperatorAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.AllowNullInternal, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.AllowBlankAttributeName, attributeValue);

			if (listRule != null)
			{
				// This value seems to be defined incorrectly by the XML schema. When the drop down should not be shown, the value is true. 
				// When it should be shown it is omitted because the default value is false.
				bool showDropDown = (listRule.ShowDropdown == false);

				attributeValue = XmlElementBase.GetXmlString(showDropDown, DataType.Boolean, false, false);
				XmlElementBase.AddAttribute(element, DataValidationElement.ShowDropDownAttributeName, attributeValue);
			}

			attributeValue = XmlElementBase.GetXmlString(rule.ShowInputMessage, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.ShowInputMessageAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.ShowErrorMessageForInvalidValue, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.ShowErrorMessageAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.ErrorMessageTitle, DataType.ST_Xstring, string.Empty, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.ErrorTitleAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.ErrorMessageDescription, DataType.ST_Xstring, string.Empty, false);

			// MD 1/5/12 - TFS98535
			// The newlines must be escaped in these attributes.
			attributeValue = DataValidationElement.EscapeDescription(attributeValue);

			XmlElementBase.AddAttribute(element, DataValidationElement.ErrorAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.InputMessageTitle, DataType.ST_Xstring, string.Empty, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.PromptTitleAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(rule.InputMessageDescription, DataType.ST_Xstring, string.Empty, false);

			// MD 1/5/12 - TFS98535
			// The newlines must be escaped in these attributes.
			attributeValue = DataValidationElement.EscapeDescription(attributeValue);

			XmlElementBase.AddAttribute(element, DataValidationElement.PromptAttributeName, attributeValue);

			attributeValue = XmlElementBase.GetXmlString(references.ToString(CellReferenceMode.A1), DataType.ST_Sqref, string.Empty, false);
			XmlElementBase.AddAttribute(element, DataValidationElement.SqrefAttributeName, attributeValue);

			if (rule.GetFormula1(null) != null)
				XmlElementBase.AddElement(element, Formula1Element.QualifiedName);

			if (rule.GetFormula2(null) != null)
				XmlElementBase.AddElement(element, Formula2Element.QualifiedName);
		}
		#endregion Save

		#endregion Base class overrides

		// MD 1/5/12 - TFS98535
		#region EscapeDescription

		private static string EscapeDescription(string description)
		{
			if (description == null)
				return null;

			string value = description.Replace("\r\n", DataValidationElement.DescriptionNewline);
			value = value.Replace("\n", DataValidationElement.DescriptionNewline);
			return value;
		}

		#endregion  // EscapeDescription

		// MD 1/5/12 - TFS98535
		#region UnescapeDescription

		private static string UnescapeDescription(string description)
		{
			if (description == null)
				return null;

			return Regex.Replace(description, DataValidationElement.DescriptionNewline, "\n", RegexOptions.IgnoreCase);
		}

		#endregion  // UnescapeDescription
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