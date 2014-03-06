using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 12/21/11 - 12.1 - Table Support
	internal abstract class TableColumnFormulaElementBase : XLSXElementBase
	{
		#region Constants

		private const string ArrayAttributeName = "array";

		#endregion // Constants

		#region Base Class Overrides

		#region Load

		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			WorksheetTableColumn column = (WorksheetTableColumn)manager.ContextStack[typeof(WorksheetTableColumn)];
			if (column == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTableColumn on the context stack.");
				return;
			}

			bool array = false;

			#region Load Attribute Values

			foreach (ExcelXmlAttribute attribute in element.Attributes)
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);

				switch (attributeName)
				{
					case TableColumnFormulaElementBase.ArrayAttributeName:
						array = (bool)XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, array);
						break;


					default:
						Utilities.DebugFail("Unknown attribute " + attributeName);
						break;
				}
			}

			#endregion // Load Attribute Values

			Formula formula;
			if (array)
			{
				formula = ArrayFormula.Parse("=" + value,
					CellReferenceMode.A1,
					manager.Workbook.CurrentFormat,
					CultureInfo.InvariantCulture);
			}
			else
			{
				formula = Formula.Parse("=" + value,
					CellReferenceMode.A1,
					manager.Workbook.CurrentFormat,
					CultureInfo.InvariantCulture);
			}

			this.SetFormula(column, formula);
		}

		#endregion // Load

		#region Save

		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			WorksheetTableColumn column = (WorksheetTableColumn)manager.ContextStack[typeof(WorksheetTableColumn)];
			if (column == null)
			{
				Utilities.DebugFail("Cannot find the WorksheetTableColumn on the context stack.");
				return;
			}

			Formula formula = this.GetFormula(column);
			value = Utilities.BuildSavingFormulaReferenceString(formula, manager);

			// Add the 'array' attribute
			string attributeValue = XmlElementBase.GetXmlString(formula is ArrayFormula, DataType.Boolean, false, false);
			XmlElementBase.AddAttribute(element, TableColumnFormulaElementBase.ArrayAttributeName, attributeValue);
		}

		#endregion // Save

		#endregion // Base Class Overrides

		protected abstract Formula GetFormula(WorksheetTableColumn column);
		protected abstract void SetFormula(WorksheetTableColumn column, Formula formula);
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