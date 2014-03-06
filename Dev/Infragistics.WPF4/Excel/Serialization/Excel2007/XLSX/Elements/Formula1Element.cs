using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
	// MD 5/13/11 - Data Validations / Page Breaks



	internal class Formula1Element : DVFormulaElementBase
	{
		#region Constants

		/// <summary>formula1</summary>
		public const string LocalName = "formula1";
		
		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/formula1</summary>
		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			Formula1Element.LocalName;

		#endregion Constants

		#region Base class overrides

		protected override Formula GetFormula(DataValidationRule rule, string rootCellAddress)
		{
			return rule.GetFormula1(rootCellAddress);
		}

		#region Type

	    public override XLSXElementType Type
	    {
		    get { return XLSXElementType.formula1; }
	    }

        #endregion Type

		protected override void SetFormula(DataValidationRule rule, string rootCellAddress, Formula formula)
		{
			rule.SetFormula1(formula, rootCellAddress);
		}

		#endregion Base class overrides
	}

	internal abstract class DVFormulaElementBase : XLSXElementBase
	{
		protected abstract Formula GetFormula(DataValidationRule rule, string rootCellAddress);
		protected abstract void SetFormula(DataValidationRule rule, string rootCellAddress, Formula formula);

		#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
		{
			DataValidationRule rule = (DataValidationRule)manager.ContextStack[typeof(DataValidationRule)];
			WorksheetCell rootCell = (WorksheetCell)manager.ContextStack[typeof(WorksheetCell)];
			if (rule == null || rootCell == null)
			{
				Utilities.DebugFail("Could not get the DataValidationRule or root cell off of the context stack");
				return;
			}

			// MD 2/29/12 - 12.1 - Table Support
			if (rootCell.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			value = Utilities.BuildLoadingFormulaReferenceString(value);
			this.SetFormula(
				rule, 
				rootCell.ToString(rootCell.Worksheet.CellReferenceMode, false),
				// MD 12/21/11 - TFS97840
				// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
				//Formula.Parse(value, CellReferenceMode.A1, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture));
				// MD 2/23/12 - TFS101504
				// Pass along the OrderedExternalReferences collection as the indexedReferencesDuringLoad parameter.
				//Formula.Parse(value, CellReferenceMode.A1, rule.FormulaType, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture));
				//Formula.Parse(value, CellReferenceMode.A1, rule.FormulaType, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, manager.OrderedExternalReferences));
				Formula.Parse(value, CellReferenceMode.A1, rule.FormulaType, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture, manager.OrderedExternalReferences));
		}
		#endregion Load

		#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
		{
			DictionaryContext<DataValidationRule, WorksheetReferenceCollection> context =
				 (DictionaryContext<DataValidationRule, WorksheetReferenceCollection>)manager.ContextStack[typeof(DictionaryContext<DataValidationRule, WorksheetReferenceCollection>)];

			if (context == null)
			{
				Utilities.DebugFail("Could not get the dictionary context off of the context stack.");
				return;
			}

			KeyValuePair<DataValidationRule, WorksheetReferenceCollection> pair = context.CurrentItem;

			DataValidationRule rule = pair.Key;

			if (rule == null)
			{
				Utilities.DebugFail("The current item is null.");
				return;
			}

			string rootCellAddress;
			if (pair.Value.Regions.Count == 1)
			{
				// MD 3/13/12 - 12.1 - Table Support
				//WorksheetCell rootCell = pair.Value.Regions[0].TopLeftCell;
				WorksheetCell rootCell = pair.Value.TopLeftCell;

				// MD 2/29/12 - 12.1 - Table Support
				// The TopLeftCell can now be null.
				//rootCellAddress = rootCell.ToString(rootCell.Worksheet.CellReferenceMode, false);
				if (rootCell == null || rootCell.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected");
					rootCellAddress = "A1";
				}
				else
				{
					rootCellAddress = rootCell.ToString(rootCell.Worksheet.CellReferenceMode, false);
				}
			}
			else
			{
				rootCellAddress = "A1";
			}

			Formula formula = this.GetFormula(rule, rootCellAddress);
			if (formula == null)
			{
				Utilities.DebugFail("The formula is null.");
				return;
			}

			value = Utilities.BuildSavingFormulaReferenceString(formula, manager);
		}
		#endregion Save

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