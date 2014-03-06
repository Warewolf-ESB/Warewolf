using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Represents a data validation rule which can validate the cell value against two constraint values or formulas.
	/// </summary>
	/// <seealso cref="Worksheet.DataValidationRules"/>
	/// <seealso cref="DataValidationRuleCollection.Add(TwoConstraintDataValidationRule,WorksheetCell)"/>
	/// <seealso cref="DataValidationRuleCollection.Add(TwoConstraintDataValidationRule,WorksheetRegion)"/>
	/// <seealso cref="TwoConstraintDataValidationOperator"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)]
	public

		sealed class TwoConstraintDataValidationRule : ValueConstraintDataValidationRule
	{
		#region Member Variables

		private Formula lowerConstraintFormula;
		private Formula upperConstraintFormula;
		private TwoConstraintDataValidationOperator validationOperator = TwoConstraintDataValidationOperator.Between;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="TwoConstraintDataValidationRule"/> instance.
		/// </summary>
		public TwoConstraintDataValidationRule()
		{
		}

		/// <summary>
		/// Creates a new <see cref="TwoConstraintDataValidationRule"/> instance.
		/// </summary>
		/// <param name="validationOperator">The operator to use when comparing the cell value to the constraint values.</param>
		/// <param name="validationCriteria">The criteria to use when validating the cell value.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when the <paramref name="validationCriteria"/> is not a member of the <see cref="DataValidationCriteria"/> enumeration.
		/// </exception>
		public TwoConstraintDataValidationRule(TwoConstraintDataValidationOperator validationOperator, DataValidationCriteria validationCriteria)
		{
			this.validationOperator = validationOperator;
			this.SetValidationCriteria(validationCriteria, "validationCriteria");
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal override Formula GetFormula1(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetLowerConstraintFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode);
			return this.GetLowerConstraintFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override Formula GetFormula2(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetUpperConstraintFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode);
			return this.GetUpperConstraintFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnAddedToCollection(DataValidationRuleCollection parentCollection)
		{
			base.OnAddedToCollection(parentCollection);

			if (this.lowerConstraintFormula != null)
			{
				FormulaContext context = new FormulaContext(this.Worksheet, -1, -1, this.Worksheet.CurrentFormat, this.lowerConstraintFormula);
				this.lowerConstraintFormula.ConnectReferences(context);
			}

			if (this.upperConstraintFormula != null)
			{
				FormulaContext context = new FormulaContext(this.Worksheet, -1, -1, this.Worksheet.CurrentFormat, this.upperConstraintFormula);
				this.upperConstraintFormula.ConnectReferences(context);
			}
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnRemovedFromCollection()
		{
			if (this.lowerConstraintFormula != null)
				this.lowerConstraintFormula.DisconnectReferences();

			if (this.upperConstraintFormula != null)
				this.upperConstraintFormula.DisconnectReferences();

			base.OnRemovedFromCollection();
		}

		internal override DataValidationOperatorType OperatorType
		{
			get { return (DataValidationOperatorType)this.validationOperator; }
		}

		internal override void SetFormula1(Formula formula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetLowerConstraintFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode);
			this.SetLowerConstraintFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override void SetFormula2(Formula formula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetUpperConstraintFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode);
			this.SetUpperConstraintFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override void VerifyState(DataValidationRuleCollection collection, WorksheetReferenceCollection references)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(this.lowerConstraintFormula, collection, references, "lowerConstraintFormula", true);
			//this.VerifyFormula(this.upperConstraintFormula, collection, references, "upperConstraintFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(this.lowerConstraintFormula, collection, references, "lowerConstraintFormula", true, true);
			//this.VerifyFormula(this.upperConstraintFormula, collection, references, "upperConstraintFormula", true, false);
			WorkbookFormat format = this.CurrentFormat;
			this.VerifyFormula(this.lowerConstraintFormula, collection, references, "lowerConstraintFormula", true, true, format);
			this.VerifyFormula(this.upperConstraintFormula, collection, references, "upperConstraintFormula", true, false, format);
		}

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region GetLowerConstraintFormula

		/// <summary>
		/// Gets the lower constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the lower constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// <seealso cref="TryGetLowerConstraint(out double)"/>
		/// <seealso cref="TryGetLowerConstraint(out DateTime)"/>
		/// <seealso cref="TryGetLowerConstraint(out TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="GetUpperConstraintFormula(string)"/>
		/// <seealso cref="GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public string GetLowerConstraintFormula(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetLowerConstraintFormula(address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			return this.GetLowerConstraintFormula(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the lower constraint formula used to validate the cell value.
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
		/// The way in which the cell value is compared to the lower constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// <seealso cref="TryGetLowerConstraint(out double)"/>
		/// <seealso cref="TryGetLowerConstraint(out DateTime)"/>
		/// <seealso cref="TryGetLowerConstraint(out TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string)"/>
		/// <seealso cref="GetUpperConstraintFormula(string)"/>
		/// <seealso cref="GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public string GetLowerConstraintFormula(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 4/9/12 - TFS101506
			//Formula formula = this.GetLowerConstraintFormulaInternal(address, format, cellReferenceMode);
			Formula formula = this.GetLowerConstraintFormulaInternal(address, format, cellReferenceMode, culture);

			if (formula == null)
				return null;

			return formula.ToString(cellReferenceMode, culture);
		}

		#endregion  // GetLowerConstraintFormula

		#region GetUpperConstraintFormula

		/// <summary>
		/// Gets the upper constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the upper constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// <seealso cref="TryGetUpperConstraint(out double)"/>
		/// <seealso cref="TryGetUpperConstraint(out DateTime)"/>
		/// <seealso cref="TryGetUpperConstraint(out TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string)"/>
		/// <seealso cref="GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public string GetUpperConstraintFormula(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetUpperConstraintFormula(address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			return this.GetUpperConstraintFormula(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the upper constraint formula used to validate the cell value.
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
		/// The way in which the cell value is compared to the upper constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// <seealso cref="TryGetUpperConstraint(out double)"/>
		/// <seealso cref="TryGetUpperConstraint(out DateTime)"/>
		/// <seealso cref="TryGetUpperConstraint(out TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string)"/>
		/// <seealso cref="GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="GetUpperConstraintFormula(string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public string GetUpperConstraintFormula(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 4/9/12 - TFS101506
			//Formula formula = this.GetUpperConstraintFormulaInternal(address, format, cellReferenceMode);
			Formula formula = this.GetUpperConstraintFormulaInternal(address, format, cellReferenceMode, culture);

			if (formula == null)
				return null;

			return formula.ToString(cellReferenceMode, culture);
		}

		#endregion  // GetUpperConstraintFormula

		#region SetLowerConstraint

		/// <summary>
		/// Sets the lower constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the lower constraint value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is WholeNumber, Decimal, or TextLength. When the ValidationCriteria is Time, 
		/// the <see cref="SetLowerConstraint(TimeSpan)"/> overload is preferred, and when the ValidationCriteria is Date, the 
		/// <see cref="SetLowerConstraint(DateTime)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The lower constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is greater than the upper constraint value. If the upper constraint formula does not equal 
		/// a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetLowerConstraint(double value)
		{
			this.SetLowerConstraintFormula("=" + value.ToString(), null);
		}

		/// <summary>
		/// Sets the lower constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the lower constraint value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is Date. When the ValidationCriteria is WholeNumber, Decimal, or TextLength, 
		/// the <see cref="SetLowerConstraint(double)"/> overload is preferred, and when the ValidationCriteria is Time, the 
		/// <see cref="SetLowerConstraint(TimeSpan)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The lower constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="value"/> cannot be represented as a date in Excel.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is greater than the upper constraint value. If the upper constraint formula does not equal 
		/// a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetLowerConstraint(DateTime value)
		{
			double? excelData = ExcelCalcValue.DateTimeToExcelDate(this.Workbook, value);

			if (excelData.HasValue == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_InvalidExcelDate"), "value");

			this.SetLowerConstraint(excelData.Value);
		}

		/// <summary>
		/// Sets the lower constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the lower constraint value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is Time. When the ValidationCriteria is WholeNumber, Decimal, or TextLength, 
		/// the <see cref="SetLowerConstraint(double)"/> overload is preferred, and when the ValidationCriteria is Date, the 
		/// <see cref="SetLowerConstraint(DateTime)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The lower constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is greater than the upper constraint value. If the upper constraint formula does not equal 
		/// a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetLowerConstraint(TimeSpan value)
		{
			this.SetLowerConstraint(ExcelCalcValue.TimeOfDayToExcelDate(value));
		}

		#endregion  // SetLowerConstraint

		#region SetLowerConstraintFormula

		/// <summary>
		/// Gets the lower constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="lowerConstraintFormula">The lower constraint formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the lower constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// Occurs when <paramref name="lowerConstraintFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="lowerConstraintFormula"/> is an <see cref="ArrayFormula"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the specified value is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="lowerConstraintFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value equals a constant, such as =5, and the constant value is greater than the upper constraint value.
		/// If the upper constraint formula does not equal a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string)"/>
		/// <seealso cref="GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetLowerConstraintFormula(string lowerConstraintFormula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetLowerConstraintFormula(lowerConstraintFormula, address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			this.SetLowerConstraintFormula(lowerConstraintFormula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the lower constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="lowerConstraintFormula">The lower constraint formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <param name="format">The workbook format with which to parse <paramref name="address"/>.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse <paramref name="address"/>.</param>
		/// <param name="culture">The culture to use when parsing the formula string.</param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the lower constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the lower constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// Occurs when <paramref name="lowerConstraintFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="lowerConstraintFormula"/> is an <see cref="ArrayFormula"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the specified value is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="lowerConstraintFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value equals a constant, such as =5, and the constant value is greater than the upper constraint value.
		/// If the upper constraint formula does not equal a constant, this verification is not performed.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="format"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string)"/>
		/// <seealso cref="GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetLowerConstraintFormula(string lowerConstraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// ParseFormula is no longer static because it need to access some instance members.
			//Formula parsedFormula = LimitedValueDataValidationRule.ParseFormula(lowerConstraintFormula, format, cellReferenceMode, culture);
			Formula parsedFormula = this.ParseFormula(lowerConstraintFormula, format, cellReferenceMode, culture);

			// MD 4/9/12 - TFS101506
			//this.SetLowerConstraintFormulaInternal(parsedFormula, address, format, cellReferenceMode);
			this.SetLowerConstraintFormulaInternal(parsedFormula, address, format, cellReferenceMode, culture);
		}

		#endregion  // SetLowerConstraintFormula

		#region SetUpperConstraint

		/// <summary>
		/// Sets the upper constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the upper constraint value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is WholeNumber, Decimal, or TextLength. When the ValidationCriteria is Time, 
		/// the <see cref="SetUpperConstraint(TimeSpan)"/> overload is preferred, and when the ValidationCriteria is Date, the 
		/// <see cref="SetUpperConstraint(DateTime)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The lower constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is less than the lower constraint value. If the lower constraint formula does not equal 
		/// a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetUpperConstraint(double value)
		{
			this.SetUpperConstraintFormula("=" + value.ToString(), null);
		}

		/// <summary>
		/// Sets the upper constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the upper constraint value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is Date. When the ValidationCriteria is WholeNumber, Decimal, or TextLength, 
		/// the <see cref="SetUpperConstraint(double)"/> overload is preferred, and when the ValidationCriteria is Time, the 
		/// <see cref="SetUpperConstraint(TimeSpan)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The lower constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="value"/> cannot be represented as a date in Excel.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is less than the lower constraint value. If the lower constraint formula does not equal 
		/// a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetUpperConstraint(DateTime value)
		{
			double? excelData = ExcelCalcValue.DateTimeToExcelDate(this.Workbook, value);

			if (excelData.HasValue == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_InvalidExcelDate"), "value");

			this.SetUpperConstraint(excelData.Value);
		}

		/// <summary>
		/// Sets the upper constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the upper constraint value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is Time. When the ValidationCriteria is WholeNumber, Decimal, or TextLength, 
		/// the <see cref="SetUpperConstraint(double)"/> overload is preferred, and when the ValidationCriteria is Date, the 
		/// <see cref="SetUpperConstraint(DateTime)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The lower constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value is less than the lower constraint value. If the lower constraint formula does not equal 
		/// a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetUpperConstraint(TimeSpan value)
		{
			this.SetUpperConstraint(ExcelCalcValue.TimeOfDayToExcelDate(value));
		}

		#endregion  // SetUpperConstraint

		#region SetUpperConstraintFormula

		/// <summary>
		/// Gets the upper constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="upperConstraintFormula">The upper constraint formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the upper constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// Occurs when <paramref name="upperConstraintFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="upperConstraintFormula"/> is an <see cref="ArrayFormula"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the specified value is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="upperConstraintFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value equals a constant, such as =5, and the constant value is greater than the upper constraint value.
		/// If the upper constraint formula does not equal a constant, this verification is not performed.
		/// </exception>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="GetUpperConstraintFormula(string)"/>
		/// <seealso cref="GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetUpperConstraintFormula(string upperConstraintFormula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetUpperConstraintFormula(upperConstraintFormula, address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			this.SetUpperConstraintFormula(upperConstraintFormula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the upper constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="upperConstraintFormula">The upper constraint formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <param name="format">The workbook format with which to parse <paramref name="address"/>.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse <paramref name="address"/>.</param>
		/// <param name="culture">The culture to use when parsing the formula string.</param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the upper constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent 
		/// is compared to the upper constraint formula's value.
		/// </p>
		/// <p class="body">
		/// When the <see cref="ValidationOperator"/> is Between, the value must be greater than or equal to the lower constraint and less 
		/// than or equal to the upper constraint. When the ValidationOperator is NotBetween, the value must be less than the lower constraint
		/// or greater than the upper constraint.
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
		/// Occurs when <paramref name="upperConstraintFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="upperConstraintFormula"/> is an <see cref="ArrayFormula"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the specified value is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="upperConstraintFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value equals a constant, such as =5, and the constant value is greater than the upper constraint value.
		/// If the upper constraint formula does not equal a constant, this verification is not performed.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="format"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="GetUpperConstraintFormula(string)"/>
		/// <seealso cref="GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetUpperConstraintFormula(string upperConstraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// ParseFormula is no longer static because it need to access some instance members.
			//Formula parsedFormula = LimitedValueDataValidationRule.ParseFormula(upperConstraintFormula, format, cellReferenceMode, culture);
			Formula parsedFormula = this.ParseFormula(upperConstraintFormula, format, cellReferenceMode, culture);

			// MD 4/9/12 - TFS101506
			//this.SetUpperConstraintFormulaInternal(parsedFormula, address, format, cellReferenceMode);
			this.SetUpperConstraintFormulaInternal(parsedFormula, address, format, cellReferenceMode, culture);
		}

		#endregion  // SetUpperConstraintFormula

		#region TryGetLowerConstraint

		/// <summary>
		/// Tries to obtain the value of the lower constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with one of the SetLowerConstraint methods or a formula was set with one of the 
		/// SetLowerConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or 0 if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetLowerConstraint(out double value)
		{
			value = 0;

			double? lowerConstraintValue = ValueConstraintDataValidationRule.GetConstraint(this.lowerConstraintFormula);

			if (lowerConstraintValue.HasValue == false)
				return false;

			value = lowerConstraintValue.Value;
			return true;
		}

		/// <summary>
		/// Tries to obtain the value of the lower constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with one of the SetLowerConstraint methods or a formula was set with one of the 
		/// SetLowerConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or DateTime.MinValue if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetLowerConstraint(out DateTime value)
		{
			value = DateTime.MinValue;

			double excelDate;
			if (this.TryGetLowerConstraint(out excelDate) == false)
				return false;

			DateTime? dateTime = ExcelCalcValue.ExcelDateToDateTime(this.Workbook, excelDate);
			if (dateTime.HasValue == false)
				return false;

			value = dateTime.Value;
			return true;
		}

		/// <summary>
		/// Tries to obtain the value of the lower constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with one of the SetLowerConstraint methods or a formula was set with one of the 
		/// SetLowerConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or TimeSpan.Zero if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetLowerConstraint(out TimeSpan value)
		{
			value = TimeSpan.Zero;

			double excelDate;
			if (this.TryGetLowerConstraint(out excelDate) == false)
				return false;

			if (excelDate >= 1)
				return false;

			value = ExcelCalcValue.ExcelDateToTimeOfDay(excelDate);
			return true;
		}

		#endregion  // TryGetLowerConstraint

		#region TryGetUpperConstraint

		/// <summary>
		/// Tries to obtain the value of the upper constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with one of the SetUpperConstraint methods or a formula was set with one of the 
		/// SetLowerConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or 0 if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetUpperConstraint(out double value)
		{
			value = 0;

			double? upperConstraintValue = ValueConstraintDataValidationRule.GetConstraint(this.upperConstraintFormula);

			if (upperConstraintValue.HasValue == false)
				return false;

			value = upperConstraintValue.Value;
			return true;
		}

		/// <summary>
		/// Tries to obtain the value of the upper constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with one of the SetUpperConstraint methods or a formula was set with one of the 
		/// SetLowerConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or DateTime.MinValue if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetUpperConstraint(out DateTime value)
		{
			value = DateTime.MinValue;

			double excelDate;
			if (this.TryGetUpperConstraint(out excelDate) == false)
				return false;

			DateTime? dateTime = ExcelCalcValue.ExcelDateToDateTime(this.Workbook, excelDate);
			if (dateTime.HasValue == false)
				return false;

			value = dateTime.Value;
			return true;
		}

		/// <summary>
		/// Tries to obtain the value of the upper constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with one of the SetUpperConstraint methods or a formula was set with one of the 
		/// SetLowerConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or TimeSpan.Zero if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetUpperConstraint(out TimeSpan value)
		{
			value = TimeSpan.Zero;

			double excelDate;
			if (this.TryGetUpperConstraint(out excelDate) == false)
				return false;

			if (excelDate >= 1)
				return false;

			value = ExcelCalcValue.ExcelDateToTimeOfDay(excelDate);
			return true;
		}

		#endregion  // TryGetUpperConstraint

		#endregion  // Public Methods

		#region Internal Methods

		#region GetLowerConstraintFormulaInternal

		// MD 4/9/12 - TFS101506
		//internal Formula GetLowerConstraintFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		//{
		//    return LimitedValueDataValidationRule.ResolveFormula(this.lowerConstraintFormula, address, format, cellReferenceMode, true);
		//}
		internal Formula GetLowerConstraintFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			return this.ResolveFormula(this.lowerConstraintFormula, address, format, cellReferenceMode, culture, true);
		}

		#endregion  // GetLowerConstraintFormulaInternal

		#region GetUpperConstraintFormulaInternal

		// MD 4/9/12 - TFS101506
		//internal Formula GetUpperConstraintFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		//{
		//    return LimitedValueDataValidationRule.ResolveFormula(this.upperConstraintFormula, address, format, cellReferenceMode, true);
		//}
		internal Formula GetUpperConstraintFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			return this.ResolveFormula(this.upperConstraintFormula, address, format, cellReferenceMode, culture, true);
		}

		#endregion  // GetUpperConstraintFormulaInternal

		#region SetLowerConstraintFormulaInternal

		// MD 4/9/12 - TFS101506
		//private void SetLowerConstraintFormulaInternal(Formula lowerConstraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		private void SetLowerConstraintFormulaInternal(Formula lowerConstraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(lowerConstraintFormula, this.ParentCollection, null, "lowerConstraintFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(lowerConstraintFormula, this.ParentCollection, null, "lowerConstraintFormula", true, true);
			this.VerifyFormula(lowerConstraintFormula, this.ParentCollection, null, "lowerConstraintFormula", true, true, format);

			double? lowerConstraintValue = ValueConstraintDataValidationRule.GetConstraint(lowerConstraintFormula);
			double? upperConstraintValue = ValueConstraintDataValidationRule.GetConstraint(this.upperConstraintFormula);

			if (lowerConstraintValue.HasValue && upperConstraintValue.HasValue)
			{
				if (upperConstraintValue.Value < lowerConstraintValue.Value)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_LowerGreaterThanUpperContraint"), "lowerConstraintFormula");
			}

			// MD 4/9/12 - TFS101506
			//this.lowerConstraintFormula = LimitedValueDataValidationRule.ResolveFormula(lowerConstraintFormula, address, format, cellReferenceMode, false);
			this.lowerConstraintFormula = this.ResolveFormula(lowerConstraintFormula, address, format, cellReferenceMode, culture, false);
		}

		#endregion  // SetLowerConstraintFormulaInternal

		#region SetUpperConstraintFormulaInternal

		// MD 4/9/12 - TFS101506
		//private void SetUpperConstraintFormulaInternal(Formula upperConstraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		private void SetUpperConstraintFormulaInternal(Formula upperConstraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(upperConstraintFormula, this.ParentCollection, null, "upperConstraintFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(upperConstraintFormula, this.ParentCollection, null, "upperConstraintFormula", true, false);
			this.VerifyFormula(upperConstraintFormula, this.ParentCollection, null, "upperConstraintFormula", true, false, format);

			double? lowerConstraintValue = ValueConstraintDataValidationRule.GetConstraint(this.lowerConstraintFormula);
			double? upperConstraintValue = ValueConstraintDataValidationRule.GetConstraint(upperConstraintFormula);

			if (lowerConstraintValue.HasValue && upperConstraintValue.HasValue)
			{
				if (upperConstraintValue.Value < lowerConstraintValue.Value)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_UpperLessThanLowerContraint"), "upperConstraintFormula");
			}

			// MD 4/9/12 - TFS101506
			//this.upperConstraintFormula = LimitedValueDataValidationRule.ResolveFormula(upperConstraintFormula, address, format, cellReferenceMode, false);
			this.upperConstraintFormula = this.ResolveFormula(upperConstraintFormula, address, format, cellReferenceMode, culture, false);
		}

		#endregion  // SetUpperConstraintFormulaInternal

		#endregion  // Internal Methods

		#endregion  // Methods

		#region Properties

		#region ValidationOperator

		/// <summary>
		/// Gets or sets the validation operator to use when comparing the cell value against the constraint values or formulas.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Depending on the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/> of the rule, either the cell value itself or the 
		/// length of the cell value's text equivalent is compared to the constraint values or formulas.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when the value is not a member of the <see cref="TwoConstraintDataValidationOperator"/> enumeration.
		/// </exception>
		/// <seealso cref="SetLowerConstraint(double)"/>
		/// <seealso cref="SetLowerConstraint(DateTime)"/>
		/// <seealso cref="SetLowerConstraint(TimeSpan)"/>
		/// <seealso cref="SetUpperConstraint(double)"/>
		/// <seealso cref="SetUpperConstraint(DateTime)"/>
		/// <seealso cref="SetUpperConstraint(TimeSpan)"/>
		/// <seealso cref="GetLowerConstraintFormula(string)"/>
		/// <seealso cref="GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="GetUpperConstraintFormula(string)"/>
		/// <seealso cref="GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string)"/>
		/// <seealso cref="SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string)"/>
		/// <seealso cref="SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public TwoConstraintDataValidationOperator ValidationOperator
		{
			get { return this.validationOperator; }
			set
			{
				if (this.validationOperator == value)
					return;

				if (Enum.IsDefined(typeof(TwoConstraintDataValidationOperator), value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(TwoConstraintDataValidationOperator));

				this.validationOperator = value;
			}
		}

		#endregion  // ValidationOperator

		#endregion  // Properties
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