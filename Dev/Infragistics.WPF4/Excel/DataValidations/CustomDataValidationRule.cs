using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization;





namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Represents a data validation rule which allows any formula to be used to validate the value applied to a cell.
	/// </summary>
	/// <seealso cref="Worksheet.DataValidationRules"/>
	/// <seealso cref="DataValidationRuleCollection.Add(CustomDataValidationRule,WorksheetCell)"/>
	/// <seealso cref="DataValidationRuleCollection.Add(CustomDataValidationRule,WorksheetRegion)"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class CustomDataValidationRule : LimitedValueDataValidationRule
	{
		#region Member Variables

		private Formula formula;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="CustomDataValidationRule"/> instance.
		/// </summary>
		public CustomDataValidationRule()
		{
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal override Formula GetFormula1(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode);
			return this.GetFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override Formula GetFormula2(string address)
		{
			return null;
		}

		// MD 12/21/11 - TFS97840
		// Added more DV formula restrictions based on the MSDN documentation.
		// MD 2/24/12 - 12.1 - Table Support
		//internal override bool IsTokenAllowedInFormula(FormulaUtilities.Tokens.FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1)
		//{
		//    if (base.IsTokenAllowedInFormula(token, isRootNode, isOnlyToken, isFormula1) == false)
		//        return false;
		//
		//    return LimitedValueDataValidationRule.IsTokenAllowedInNonListFormula(token, isRootNode, isOnlyToken, isFormula1);
		//}
		internal override bool IsTokenAllowedInFormula(FormulaUtilities.Tokens.FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1, WorkbookFormat format)
		{
			if (base.IsTokenAllowedInFormula(token, isRootNode, isOnlyToken, isFormula1, format) == false)
				return false;

			return LimitedValueDataValidationRule.IsTokenAllowedInNonListFormula(token, isRootNode, isOnlyToken, isFormula1);
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnAddedToCollection(DataValidationRuleCollection parentCollection)
		{
			base.OnAddedToCollection(parentCollection);

			if (this.formula != null)
			{
				FormulaContext context = new FormulaContext(this.Worksheet, -1, -1, parentCollection.Worksheet.CurrentFormat, this.formula);
				this.formula.ConnectReferences(context);
			}
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnRemovedFromCollection()
		{
			if (this.formula != null)
				this.formula.DisconnectReferences();

			base.OnRemovedFromCollection();
		}

		internal override DataValidationOperatorType OperatorType
		{
			get { return (DataValidationOperatorType)0; }
		}

		internal override void SetFormula1(Formula formula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode);
			this.SetFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override void SetFormula2(Formula formula, string address)
		{
			Debug.Assert(formula == null, "Formula2 is not valid on the CustomDataValidationRule.");
		}

		internal override DataValidationType ValidationType
		{
			get { return DataValidationType.Formula; }
		}

		internal override void VerifyState(DataValidationRuleCollection collection, WorksheetReferenceCollection references)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(this.formula, collection, references, "formula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(this.formula, collection, references, "formula", true, true);
			this.VerifyFormula(this.formula, collection, references, "formula", true, true, this.CurrentFormat);
		}

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region GetFormula

		/// <summary>
		/// Gets the formula used to validate the value applied to a cell.
		/// </summary>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The formula will indicate the value is invalid by evaluating to False, 0, any <see cref="ErrorValue"/> (such as #VALUE!), 
		/// or any string other than "True" (case-insensitive). 
		/// True, "True" (case-insensitive), null, and non-zero numeric values will indicate a valid value has been applied.
		/// </p>
		/// <p class="body">
		/// The address passed in is only needed if relative addresses are used in the the formula. For example, consider the formula 
		/// applied is =B1, and the data validation rule is applied to the region A1:A5. If you get the formula for A1, the formula
		/// =B1 will be returned. If you get the formula for A2, =B2 will be returned. Similarly, for cell A5, =B5 will be returned.
		/// However, if the formula contains no references or all absolute references, the <paramref name="address"/> is ignored. So
		/// in the previous example, if the original formula was =$B$1, the same formula will be returned regardless of the specified 
		/// address.
		/// </p>
		/// <p class="body">
		/// <paramref name="address"/> can be any valid cell or region reference on a worksheet. If a region address is specified, the
		/// top-left cell or the region is used. The cell or region specified does not need to have the data validation rule applied to it. 
		/// Any reference is allowed.
		/// </p>
		/// <p class="body">
		/// The cell reference mode with which to parse <paramref name="address"/> will be assumed to be A1, unless the data validation 
		/// rule is applied to a worksheet which is in a workbook, in which case the <see cref="Workbook.CellReferenceMode"/> will be used.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <returns>A formula used to validate the value applied to a cell.</returns>
		/// <seealso cref="GetFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetFormula(string,string)"/>
		/// <seealso cref="SetFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public string GetFormula(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetFormula(address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			return this.GetFormula(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the formula used to validate the value applied to a cell.
		/// </summary>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <param name="format">The workbook format with which to parse <paramref name="address"/>.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse <paramref name="address"/>.</param>
		/// <param name="culture">The culture to use when generating the formula string.</param>
		/// <remarks>
		/// <p class="body">
		/// The formula will indicate the value is invalid by evaluating to False, 0, any <see cref="ErrorValue"/> (such as #VALUE!), 
		/// or any string other than "True" (case-insensitive). 
		/// True, "True" (case-insensitive), null, and non-zero numeric values will indicate a valid value has been applied.
		/// </p>
		/// <p class="body">
		/// The address passed in is only needed if relative addresses are used in the the formula. For example, consider the formula 
		/// applied is =B1, and the data validation rule is applied to the region A1:A5. If you get the formula for A1, the formula
		/// =B1 will be returned. If you get the formula for A2, =B2 will be returned. Similarly, for cell A5, =B5 will be returned.
		/// However, if the formula contains no references or all absolute references, the <paramref name="address"/> is ignored. So
		/// in the previous example, if the original formula was =$B$1, the same formula will be returned regardless of the specified 
		/// address.
		/// </p>
		/// <p class="body">
		/// <paramref name="address"/> can be any valid cell or region reference on a worksheet. If a region address is specified, the
		/// top-left cell or the region is used. The cell or region specified does not need to have the data validation rule applied to it. 
		/// Any reference is allowed.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="format"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>A formula used to validate the value applied to a cell.</returns>
		/// <seealso cref="GetFormula(string)"/>
		/// <seealso cref="SetFormula(string,string)"/>
		/// <seealso cref="SetFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public string GetFormula(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 4/9/12 - TFS101506
			//Formula formula = this.GetFormulaInternal(address, format, cellReferenceMode);
			Formula formula = this.GetFormulaInternal(address, format, cellReferenceMode, culture);

			if (formula == null)
				return null;

			return formula.ToString(cellReferenceMode, culture);
		}

		#endregion  // GetFormula

		#region SetFormula

		/// <summary>
		/// Sets the formula used to validate the value applied to a cell.
		/// </summary>
		/// <param name="formula">The validation formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The formula will indicate the value is invalid by evaluating to False, 0, any <see cref="ErrorValue"/> (such as #VALUE!), 
		/// or any string other than "True" (case-insensitive). 
		/// True, "True" (case-insensitive), null, and non-zero numeric values will indicate a valid value has been applied.
		/// </p>
		/// <p class="body">
		/// The address passed in is only needed if relative addresses are used in the the formula. When the data validation rule is 
		/// applied to cells or regions, the references in the formula used by each individual cell will be shifted by the offset of
		/// the cell to the passed in <paramref name="address"/>. For example, consider the formula specified is =B1 and the specified 
		/// address is A1. If the data validation rule is then applied to the A5 cell, the formula is will use is =B5. However, if the
		/// references in the formula are absolute, such as =$B$1, the same formula will be applied regardless of the specified address.
		/// </p>
		/// <p class="body">
		/// <paramref name="address"/> can be any valid cell or region reference on a worksheet. If a region address is specified, the
		/// top-left cell or the region is used. The cell or region specified does not need to have the data validation rule applied to it. 
		/// Any reference is allowed.
		/// </p>
		/// <p class="body">
		/// The cell reference mode with which to parse <paramref name="address"/> will be assumed to be A1, unless the data validation 
		/// rule is applied to a worksheet which is in a workbook, in which case the <see cref="Workbook.CellReferenceMode"/> will be used.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="formula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="formula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <seealso cref="GetFormula(string)"/>
		/// <seealso cref="GetFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public void SetFormula(string formula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetFormula(formula, address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			this.SetFormula(formula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Sets the formula used to validate the value applied to a cell.
		/// </summary>
		/// <param name="formula">The validation formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <param name="format">The workbook format with which to parse <paramref name="address"/>.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse <paramref name="address"/>.</param>
		/// <param name="culture">The culture to use when parsing the formula string.</param>
		/// <remarks>
		/// <p class="body">
		/// The formula will indicate the value is invalid by evaluating to False, 0, any <see cref="ErrorValue"/> (such as #VALUE!), 
		/// or any string other than "True" (case-insensitive). 
		/// True, "True" (case-insensitive), null, and non-zero numeric values will indicate a valid value has been applied.
		/// </p>
		/// <p class="body">
		/// The address passed in is only needed if relative addresses are used in the the formula. When the data validation rule is 
		/// applied to cells or regions, the references in the formula used by each individual cell will be shifted by the offset of
		/// the cell to the passed in <paramref name="address"/>. For example, consider the formula specified is =B1 and the specified 
		/// address is A1. If the data validation rule is then applied to the A5 cell, the formula is will use is =B5. However, if the
		/// references in the formula are absolute, such as =$B$1, the same formula will be applied regardless of the specified address.
		/// </p>
		/// <p class="body">
		/// <paramref name="address"/> can be any valid cell or region reference on a worksheet. If a region address is specified, the
		/// top-left cell or the region is used. The cell or region specified does not need to have the data validation rule applied to it. 
		/// Any reference is allowed.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="formula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="formula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="format"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <seealso cref="GetFormula(string)"/>
		/// <seealso cref="GetFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public void SetFormula(string formula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// ParseFormula is no longer static because it need to access some instance members.
			//Formula parsedFormula = LimitedValueDataValidationRule.ParseFormula(formula, format, cellReferenceMode, culture);
			Formula parsedFormula = this.ParseFormula(formula, format, cellReferenceMode, culture);

			// MD 4/9/12 - TFS101506
			//this.SetFormulaInternal(parsedFormula, address, format, cellReferenceMode);
			this.SetFormulaInternal(parsedFormula, address, format, cellReferenceMode, culture);
		}

		#endregion  // SetFormula

		#endregion  // Public Methods

		#region Internal Methods

		#region GetFormulaInternal

		// MD 4/9/12 - TFS101506
		//internal Formula GetFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		//{
		//    return LimitedValueDataValidationRule.ResolveFormula(this.formula, address, format, cellReferenceMode, true);
		//}
		internal Formula GetFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			return this.ResolveFormula(this.formula, address, format, cellReferenceMode, culture, true);
		}

		#endregion  // GetFormulaInternal

		#region SetFormulaInternal

		// MD 4/9/12 - TFS101506
		//private void SetFormulaInternal(Formula formula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		private void SetFormulaInternal(Formula formula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(formula, this.ParentCollection, null, "formula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(formula, this.ParentCollection, null, "formula", true, true);
			this.VerifyFormula(formula, this.ParentCollection, null, "formula", true, true, format);

			// MD 4/9/12 - TFS101506
			//this.formula = LimitedValueDataValidationRule.ResolveFormula(formula, address, format, cellReferenceMode, false);
			this.formula = this.ResolveFormula(formula, address, format, cellReferenceMode, culture, false);
		}

		#endregion  // SetFormulaInternal

		#endregion  // Internal Methods

		#endregion  // Methods
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