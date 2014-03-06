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
	/// Represents a data validation rule which can validate the cell value against a single constraint value or formula.
	/// </summary>
	/// <seealso cref="Worksheet.DataValidationRules"/>
	/// <seealso cref="DataValidationRuleCollection.Add(OneConstraintDataValidationRule,WorksheetCell)"/>
	/// <seealso cref="DataValidationRuleCollection.Add(OneConstraintDataValidationRule,WorksheetRegion)"/>
	/// <seealso cref="OneConstraintDataValidationOperator"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class OneConstraintDataValidationRule : ValueConstraintDataValidationRule
	{
		#region Member Variables

		private Formula constraintFormula;
		private OneConstraintDataValidationOperator validationOperator = OneConstraintDataValidationOperator.EqualTo;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="OneConstraintDataValidationRule"/> instance.
		/// </summary>
		public OneConstraintDataValidationRule()
		{
		}

		/// <summary>
		/// Creates a new <see cref="OneConstraintDataValidationRule"/> instance.
		/// </summary>
		/// <param name="validationOperator">The operator to use when comparing the cell value to the constraint value.</param>
		/// <param name="validationCriteria">The criteria to use when validating the cell value.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when the <paramref name="validationCriteria"/> is not a member of the <see cref="DataValidationCriteria"/> enumeration.
		/// </exception>
		public OneConstraintDataValidationRule(OneConstraintDataValidationOperator validationOperator, DataValidationCriteria validationCriteria)
		{
			this.validationOperator = validationOperator;
			this.SetValidationCriteria(validationCriteria, "validationCriteria");
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal override Formula GetFormula1(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetConstraintFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode);
			return this.GetConstraintFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override Formula GetFormula2(string address)
		{
			return null;
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnAddedToCollection(DataValidationRuleCollection parentCollection)
		{
			base.OnAddedToCollection(parentCollection);

			if (this.constraintFormula != null)
			{
				FormulaContext context = new FormulaContext(this.Worksheet, -1, -1, this.Worksheet.CurrentFormat, this.constraintFormula);
				this.constraintFormula.ConnectReferences(context);
			}
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnRemovedFromCollection()
		{
			if (this.constraintFormula != null)
				this.constraintFormula.DisconnectReferences();

			base.OnRemovedFromCollection();
		}

		internal override DataValidationOperatorType OperatorType
		{
			get { return (DataValidationOperatorType)this.validationOperator; }
		}

		internal override void SetFormula1(Formula formula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetConstraintFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode);
			this.SetConstraintFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override void SetFormula2(Formula formula, string address)
		{
			Debug.Assert(formula == null, "Formula2 is not valid on the OneConstraintDataValidationRule.");
		}

		internal override void VerifyState(DataValidationRuleCollection collection, WorksheetReferenceCollection references)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(this.constraintFormula, collection, references, "constraintFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(this.constraintFormula, collection, references, "constraintFormula", true, true);
			this.VerifyFormula(this.constraintFormula, collection, references, "constraintFormula", true, true, this.CurrentFormat);
		}

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region GetConstraintFormula

		/// <summary>
		/// Gets the constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint formula's value.
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
		/// <seealso cref="TryGetConstraint(out double)"/>
		/// <seealso cref="TryGetConstraint(out DateTime)"/>
		/// <seealso cref="TryGetConstraint(out TimeSpan)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		/// <seealso cref="GetConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public string GetConstraintFormula(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetConstraintFormula(address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			return this.GetConstraintFormula(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the constraint formula used to validate the cell value.
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
		/// The way in which the cell value is compared to the constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint formula's value.
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
		/// <seealso cref="TryGetConstraint(out double)"/>
		/// <seealso cref="TryGetConstraint(out DateTime)"/>
		/// <seealso cref="TryGetConstraint(out TimeSpan)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		/// <seealso cref="GetConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public string GetConstraintFormula(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 4/9/12 - TFS101506
			//Formula formula = this.GetConstraintFormulaInternal(address, format, cellReferenceMode);
			Formula formula = this.GetConstraintFormulaInternal(address, format, cellReferenceMode, culture);

			if (formula == null)
				return null;

			return formula.ToString(cellReferenceMode, culture);
		}

		#endregion  // GetConstraintFormula

		#region SetConstraint

		/// <summary>
		/// Sets the constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the constraint value is determined by the <see cref="ValidationOperator"/> as well 
		/// as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint value.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is WholeNumber, Decimal, or TextLength. When the ValidationCriteria is Time, 
		/// the <see cref="SetConstraint(TimeSpan)"/> overload is preferred, and when the ValidationCriteria is Date, the 
		/// <see cref="SetConstraint(DateTime)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The constraint value used to compare against the cell value.</param>
		/// <seealso cref="SetConstraint(TimeSpan)"/>
		/// <seealso cref="SetConstraint(DateTime)"/>
		/// <seealso cref="TryGetConstraint(out double)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetConstraint(double value)
		{
			this.SetConstraintFormula("=" + value.ToString(), null);
		}

		/// <summary>
		/// Sets the constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the constraint value is determined by the <see cref="ValidationOperator"/> as well 
		/// as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint value.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is Date. When the ValidationCriteria is WholeNumber, Decimal, or TextLength, 
		/// the <see cref="SetConstraint(double)"/> overload is preferred, and when the ValidationCriteria is Time, the 
		/// <see cref="SetConstraint(TimeSpan)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The constraint value used to compare against the cell value.</param>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="value"/> cannot be represented as a date in Excel.
		/// </exception>
		/// <seealso cref="SetConstraint(double)"/>
		/// <seealso cref="SetConstraint(TimeSpan)"/>
		/// <seealso cref="TryGetConstraint(out DateTime)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetConstraint(DateTime value)
		{
			double? excelData = ExcelCalcValue.DateTimeToExcelDate(this.Workbook, value);

			if (excelData.HasValue == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_InvalidExcelDate"), "value");

			this.SetConstraint(excelData.Value);
		}

		/// <summary>
		/// Sets the constraint value used to validate the cell value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the constraint value is determined by the <see cref="ValidationOperator"/> as well 
		/// as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint value.
		/// </p>
		/// <p class="body">
		/// This overload is preferred when the ValidationCriteria is Time. When the ValidationCriteria is WholeNumber, Decimal, or TextLength, 
		/// the <see cref="SetConstraint(double)"/> overload is preferred, and when the ValidationCriteria is Date, the 
		/// <see cref="SetConstraint(DateTime)"/> overload is preferred.
		/// </p>
		/// </remarks>
		/// <param name="value">The constraint value used to compare against the cell value.</param>
		/// <seealso cref="SetConstraint(double)"/>
		/// <seealso cref="SetConstraint(DateTime)"/>
		/// <seealso cref="TryGetConstraint(out TimeSpan)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public void SetConstraint(TimeSpan value)
		{
			this.SetConstraint(ExcelCalcValue.TimeOfDayToExcelDate(value));
		}

		#endregion  // SetConstraint

		#region SetConstraintFormula

		/// <summary>
		/// Sets the constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="constraintFormula">The validation formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint formula's value.
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
		/// Occurs when <paramref name="constraintFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="constraintFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <seealso cref="SetConstraint(double)"/>
		/// <seealso cref="SetConstraint(TimeSpan)"/>
		/// <seealso cref="SetConstraint(DateTime)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		/// <seealso cref="GetConstraintFormula(string)"/>
		/// <seealso cref="GetConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public void SetConstraintFormula(string constraintFormula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetConstraintFormula(constraintFormula, address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			this.SetConstraintFormula(constraintFormula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Sets the constraint formula used to validate the cell value.
		/// </summary>
		/// <param name="constraintFormula">The validation formula to use for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <param name="format">The workbook format with which to parse <paramref name="address"/>.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse <paramref name="address"/>.</param>
		/// <param name="culture">The culture to use when parsing the formula string.</param>
		/// <remarks>
		/// <p class="body">
		/// The way in which the cell value is compared to the constraint formula's value is determined by the <see cref="ValidationOperator"/>
		/// as well as the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/>.
		/// </p>
		/// <p class="body">
		/// Depending on the ValidationCriteria of the rule, either the cell value itself or the length of the cell value's text equivalent is 
		/// compared to the constraint formula's value.
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
		/// Occurs when <paramref name="constraintFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="constraintFormula"/> is not a valid formula.
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
		/// <seealso cref="SetConstraint(double)"/>
		/// <seealso cref="SetConstraint(TimeSpan)"/>
		/// <seealso cref="SetConstraint(DateTime)"/>
		/// <seealso cref="ValidationOperator"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		/// <seealso cref="GetConstraintFormula(string)"/>
		/// <seealso cref="GetConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		public void SetConstraintFormula(string constraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// ParseFormula is no longer static because it need to access some instance members.
			//Formula parsedFormula = LimitedValueDataValidationRule.ParseFormula(constraintFormula, format, cellReferenceMode, culture);
			Formula parsedFormula = this.ParseFormula(constraintFormula, format, cellReferenceMode, culture);

			// MD 4/9/12 - TFS101506
			//this.SetConstraintFormulaInternal(parsedFormula, address, format, cellReferenceMode);
			this.SetConstraintFormulaInternal(parsedFormula, address, format, cellReferenceMode, culture);
		}

		#endregion  // SetConstraintFormula

		#region TryGetConstraint

		/// <summary>
		/// Tries to obtain the value of the constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with the one of the SetConstraint methods or a formula was set with one 
		/// of the SetConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or 0 if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetConstraint(double)"/>
		/// <seealso cref="TryGetConstraint(out DateTime)"/>
		/// <seealso cref="TryGetConstraint(out TimeSpan)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetConstraint(out double value)
		{
			value = 0;

			double? constraintValue = ValueConstraintDataValidationRule.GetConstraint(this.constraintFormula);

			if (constraintValue.HasValue == false)
				return false;

			value = constraintValue.Value;
			return true;
		}

		/// <summary>
		/// Tries to obtain the value of the constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with the one of the SetConstraint methods or a formula was set with one 
		/// of the SetConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or DateTime.MinValue if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetConstraint(DateTime)"/>
		/// <seealso cref="TryGetConstraint(out double)"/>
		/// <seealso cref="TryGetConstraint(out TimeSpan)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetConstraint(out DateTime value)
		{
			value = DateTime.MinValue;

			double excelDate;
			if (this.TryGetConstraint(out excelDate) == false)
				return false;

			DateTime? dateTime = ExcelCalcValue.ExcelDateToDateTime(this.Workbook, excelDate);
			if (dateTime.HasValue == false)
				return false;

			value = dateTime.Value;
			return true;
		}

		/// <summary>
		/// Tries to obtain the value of the constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The constraint value can only be obtained if it was set with the one of the SetConstraint methods or a formula was set with one 
		/// of the SetConstraintFormula methods and the formula equals a constant value, such as =5.
		/// </p>
		/// </remarks>
		/// <param name="value">When the method returns, will be the value of the constraint or TimeSpan.Zero if the value could not be obtained.</param>
		/// <returns>True if the constraint value could be obtained; False otherwise.</returns>
		/// <seealso cref="SetConstraint(TimeSpan)"/>
		/// <seealso cref="TryGetConstraint(out double)"/>
		/// <seealso cref="TryGetConstraint(out DateTime)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetConstraint(out TimeSpan value)
		{
			value = TimeSpan.Zero;

			double excelDate;
			if (this.TryGetConstraint(out excelDate) == false)
				return false;

			if (excelDate >= 1)
				return false;

			value = ExcelCalcValue.ExcelDateToTimeOfDay(excelDate);
			return true;
		}

		#endregion  // TryGetConstraint

		#endregion  // Public Methods

		#region Internal Methods

		#region GetConstraintFormulaInternal

		// MD 4/9/12 - TFS101506
		//internal Formula GetConstraintFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		//{
		//    return LimitedValueDataValidationRule.ResolveFormula(this.constraintFormula, address, format, cellReferenceMode, true);
		//}
		internal Formula GetConstraintFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			return this.ResolveFormula(this.constraintFormula, address, format, cellReferenceMode, culture, true);
		}

		#endregion  // GetConstraintFormulaInternal

		#region SetConstraintFormulaInternal

		// MD 4/9/12 - TFS101506
		//private void SetConstraintFormulaInternal(Formula constraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		private void SetConstraintFormulaInternal(Formula constraintFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(constraintFormula, this.ParentCollection, null, "constraintFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(constraintFormula, this.ParentCollection, null, "constraintFormula", true, true);
			this.VerifyFormula(constraintFormula, this.ParentCollection, null, "constraintFormula", true, true, format);

			// MD 4/9/12 - TFS101506
			//this.constraintFormula = LimitedValueDataValidationRule.ResolveFormula(constraintFormula, address, format, cellReferenceMode, false);
			this.constraintFormula = this.ResolveFormula(constraintFormula, address, format, cellReferenceMode, culture, false);
		}

		#endregion  // SetConstraintFormulaInternal

		#endregion  // Internal Methods

		#endregion  // Methods

		#region Properties

		#region ValidationOperator

		/// <summary>
		/// Gets or sets the validation operator to use when comparing the cell value against the constraint value or formula.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Depending on the <see cref="ValueConstraintDataValidationRule.ValidationCriteria"/> of the rule, either the cell value itself or the 
		/// length of the cell value's text equivalent is compared to the constraint value or formula.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when the value is not a member of the <see cref="OneConstraintDataValidationOperator"/> enumeration.
		/// </exception>
		/// <seealso cref="SetConstraint(double)"/>
		/// <seealso cref="SetConstraint(TimeSpan)"/>
		/// <seealso cref="SetConstraint(DateTime)"/>
		/// <seealso cref="GetConstraintFormula(string)"/>
		/// <seealso cref="GetConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetConstraintFormula(string,string)"/>
		/// <seealso cref="SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
		public OneConstraintDataValidationOperator ValidationOperator
		{
			get { return this.validationOperator; }
			set
			{
				if (this.validationOperator == value)
					return;

				if (Enum.IsDefined(typeof(OneConstraintDataValidationOperator), value) == false)
					throw new InvalidEnumArgumentException("value", (int)value, typeof(OneConstraintDataValidationOperator));

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