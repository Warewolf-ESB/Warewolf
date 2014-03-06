using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Represents a data validation rule which allows a value from a list of accepted values to be applied to a cell.
	/// </summary>
	/// <seealso cref="Worksheet.DataValidationRules"/>
	/// <seealso cref="DataValidationRuleCollection.Add(ListDataValidationRule,WorksheetCell)"/>
	/// <seealso cref="DataValidationRuleCollection.Add(ListDataValidationRule,WorksheetRegion)"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class ListDataValidationRule : LimitedValueDataValidationRule
	{
		#region Member Variables

		private bool showDropdown;
		private Formula valuesFormula;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="ListDataValidationRule"/> instance.
		/// </summary>
		public ListDataValidationRule()
		{
			this.showDropdown = true;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		// MD 12/21/11 - TFS97840
		// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
		internal override FormulaType FormulaType
		{
			get { return FormulaType.ListDataValidationFormula; }
		}

		internal override Formula GetFormula1(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetValuesFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode);
			return this.GetValuesFormulaInternal(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override Formula GetFormula2(string address)
		{
			return null;
		}

		// MD 12/21/11 - TFS97840
		// Added more DV formula restrictions based on the MSDN documentation.
		// MD 2/24/12 - 12.1 - Table Support
		//internal override bool IsTokenAllowedInFormula(FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1)
		internal override bool IsTokenAllowedInFormula(FormulaToken token, bool isRootNode, bool isOnlyToken, bool isFormula1, WorkbookFormat format)
		{
			// These restrictions can be found here: http://msdn.microsoft.com/en-us/library/dd953412(v=office.12).aspx

			// MD 2/24/12 - 12.1 - Table Support
			//if (base.IsTokenAllowedInFormula(token, isRootNode, isOnlyToken, isFormula1) == false)
			if (base.IsTokenAllowedInFormula(token, isRootNode, isOnlyToken, isFormula1, format) == false)
				return false;

			// The root node of the parse tree of this field MUST NOT be a VALUE_TYPE, as described in Rgce.
			if (isRootNode)
			{
				if (token.Token == Token.Str)
				{
					// Even though it doesn't say so in the docs, a single string constant is allowed in the formula.
					return true;
				}

				bool isValueType = token.TokenClass == TokenClass.Value || token.TokenClass == TokenClass.Array;

				if (isValueType)
					return false;
			}

			switch (token.Token)
			{
				// If rgce contains a PtgArea3d or a PtgAreaErr3d then the PtgArea3d or PtgAreaErr3d MUST be the only Ptg in rgce.
 				case Token.Area3DA:
				case Token.Area3DR:
				case Token.Area3DV:
				case Token.AreaErr3dA:
				case Token.AreaErr3dR:
				case Token.AreaErr3dV:
					return isOnlyToken;

				default:
					return true;
			}
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnAddedToCollection(DataValidationRuleCollection parentCollection)
		{
			base.OnAddedToCollection(parentCollection);

			if (this.valuesFormula != null)
			{
				FormulaContext context = new FormulaContext(this.Worksheet, -1, -1, this.Worksheet.CurrentFormat, this.valuesFormula);
				this.valuesFormula.ConnectReferences(context);
			}
		}

		// MD 6/16/12 - CalcEngineRefactor
		internal override void OnRemovedFromCollection()
		{
			if (this.valuesFormula != null)
				this.valuesFormula.DisconnectReferences();

			base.OnRemovedFromCollection();
		}

		internal override DataValidationOperatorType OperatorType
		{
			get { return (DataValidationOperatorType)0; }
		}

		internal override void SetFormula1(Formula formula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetValuesFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode);
			this.SetValuesFormulaInternal(formula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		internal override void SetFormula2(Formula formula, string address)
		{
			Debug.Assert(formula == null, "Formula2 is not valid on the ListDataValidationRule.");
		}

		internal override DataValidationType ValidationType
		{
			get { return DataValidationType.List; }
		}

		internal override void VerifyFormula(Formula formula, 
			DataValidationRuleCollection collection, 
			WorksheetReferenceCollection references, 
			string formulaPropertyName,
			bool isOriginalFormula,
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			bool isFormula1,
			// MD 2/24/12 - 12.1 - Table Support
			WorkbookFormat format)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//base.VerifyFormula(formula, collection, references, formulaPropertyName, isOriginalFormula);
			// MD 2/24/12 - 12.1 - Table Support
			//base.VerifyFormula(formula, collection, references, formulaPropertyName, isOriginalFormula, isFormula1);
			base.VerifyFormula(formula, collection, references, formulaPropertyName, isOriginalFormula, isFormula1, format);

			if (formula == null)
				return;

			Debug.Assert(formulaPropertyName == "valuesFormula", "The strings in this method must be updated to take the new property name.");

			// MD 12/9/11 - TFS97379
			// This check is incorrect. Now the correct validations for lists are contained in the IsTokenAllowedInFormula override.
			//if (isOriginalFormula && formula.PostfixTokenList.Count != 1)
			//    throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ListFormulaMustBeStringOrReference"));

			FormulaToken token = formula.PostfixTokenList[0];

			StrToken strToken = token as StrToken;
			ReferenceToken referenceToken = token as ReferenceToken;

			if (strToken != null)
			{
				if (strToken.Value.Length == 0)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ListFormulaCannotHaveEmptyString"));
			}
			else if (referenceToken != null)
			{
				// We can skip verifying this. When the base call to VerifyFormula hits a name token, it will call back into VerifyFormula with the named reference's formula,
				// so we will verify it's tokens then.
				if (referenceToken is NameToken)
					return;

				CellAddressRange range = Utilities.GetCellAddressRange(referenceToken);

				if (range != null &&
					range.TopLeftCellAddress.Row != range.BottomRightCellAddress.Row &&
					range.TopLeftCellAddress.Column != range.BottomRightCellAddress.Column)
				{
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ListFormulaReferenceMustBeOneDimensional"));
				}
			}
			// MD 12/9/11 - TFS97379
			// This check is incorrect. Now the correct validations for lists are contained in the IsTokenAllowedInFormula override.
			//else
			//{
			//    throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ListFormulaMustBeStringOrReference"));
			//}
		}

		internal override void VerifyState(DataValidationRuleCollection collection, WorksheetReferenceCollection references)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(this.valuesFormula, collection, references, "valuesFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(this.valuesFormula, collection, references, "valuesFormula", true, true);
			this.VerifyFormula(this.valuesFormula, collection, references, "valuesFormula", true, true, this.CurrentFormat);
		}

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region GetValuesFormula

		/// <summary>
		/// Gets the formula which specifies the accepted values.
		/// </summary>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The is a string containing the list of accepted values or a reference to a cell or region in the same Workbook which contains 
		/// the accepted values.
		/// </p>
		/// <p class="body">
		/// If the formula equals a string, it will be a list of accepted value, such as ="A,B,C". If one of the values must contain a double 
		/// quote ("), the character will be repeated in the list, like so: ="A,""B"",C". This will allow the values A, "B", and C. The 
		/// separator between values will be a comma (,), unless the decimal separator for the current culture is a comma, in which case the 
		/// separator will be a semicolon (;).
		/// </p>
		/// <p class="body">
		/// If the formula equals one or more references, it will be a reference to a single cell or region in the same Workbook. Union, 
		/// intersection, and range operators are not allowed. An formula might be something like =$A$1 or =Sheet2!$A$1:$A$5. In addition to a 
		/// single cell or region, a named reference can also be used, but only if it refers to a single cell or region. If a region is specified, 
		/// or a named reference that refers to a region, the region will consist of a single row or column. A formula that equals an error value 
		/// can also be returned, but will cause the cell to not accept any values and the drop down to be empty, so it is not very useful.
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
		/// <seealso cref="TryGetValues"/>
		/// <seealso cref="GetValuesFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetValuesFormula(string,string)"/>
		/// <seealso cref="SetValuesFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public string GetValuesFormula(string address)
		{
			// MD 4/9/12 - TFS101506
			//return this.GetValuesFormula(address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			return this.GetValuesFormula(address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Gets the formula which specifies the accepted values.
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
		/// The is a string containing the list of accepted values or a reference to a cell or region in the same Workbook which contains 
		/// the accepted values.
		/// </p>
		/// <p class="body">
		/// If the formula equals a string, it will be a list of accepted value, such as ="A,B,C". If one of the values must contain a double 
		/// quote ("), the character will be repeated in the list, like so: ="A,""B"",C". This will allow the values A, "B", and C. The 
		/// separator between values will be a comma (,), unless the decimal separator for the current culture is a comma, in which case the 
		/// separator will be a semicolon (;).
		/// </p>
		/// <p class="body">
		/// If the formula equals one or more references, it will be a reference to a single cell or region in the same Workbook. Union, 
		/// intersection, and range operators are not allowed. An formula might be something like =$A$1 or =Sheet2!$A$1:$A$5. In addition to a 
		/// single cell or region, a named reference can also be used, but only if it refers to a single cell or region. If a region is specified, 
		/// or a named reference that refers to a region, the region will consist of a single row or column. A formula that equals an error value 
		/// can also be returned, but will cause the cell to not accept any values and the drop down to be empty, so it is not very useful.
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
		/// <seealso cref="TryGetValues"/>
		/// <seealso cref="GetValuesFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetValuesFormula(string,string)"/>
		/// <seealso cref="SetValuesFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public string GetValuesFormula(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 4/9/12 - TFS101506
			//Formula formula = this.GetValuesFormulaInternal(address, format, cellReferenceMode);
			Formula formula = this.GetValuesFormulaInternal(address, format, cellReferenceMode, culture);

			if (formula == null)
				return null;

			// MD 4/9/12 - TFS101506
			//CultureInfo cultureResolved = culture ?? CultureInfo.CurrentCulture;
			CultureInfo cultureResolved = culture ?? this.Culture;

			if (formula.PostfixTokenList.Count == 1)
			{
				StrToken strToken = formula.PostfixTokenList[0] as StrToken;

				if (strToken != null)
				{
					string decimalSeparator = cultureResolved.NumberFormat.NumberDecimalSeparator;

					if (decimalSeparator == ",")
					{
						return ListDataValidationRule.CreateFormulaString(
							ListDataValidationRule.GetValuesHelper(strToken.Value, CultureInfo.InvariantCulture), cultureResolved);
					}
				}
			}

			return formula.ToString(cellReferenceMode, culture);
		}

		#endregion  // GetValuesFormula

		#region SetValues

		/// <summary>
		/// Sets the list of accepted values the cell can accept.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If <see cref="LimitedValueDataValidationRule.AllowNull"/> is True, null values are allowed in addition to the list of accepted values.
		/// </p>
		/// <p class="body">
		/// All values will have ToString called on them to covert the accepted values list to a formula.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> the formula of accepted values is created by separating each value with a function parameter separator and concatenating 
		/// them into a single string. So a list of 1, 2, and 3 would have the following formula created: ="1,2,3". However, if the decimal separator 
		/// of the current culture is a comma (,) then a semicolon (;) will be used to separate the values instead. Because of this, if the ToString 
		/// of a value returns a string which contains one of these separators, the value will be split into two or more allowed values.
		/// </p>
		/// </remarks>
		/// <param name="values">The list of accepted values.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="values"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="values"/> array is empty.
		/// </exception>
		/// <seealso cref="GetValuesFormula(string)"/>
		/// <seealso cref="GetValuesFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetValuesFormula(string,string)"/>
		/// <seealso cref="SetValuesFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public void SetValues(params object[] values)
		{
			if (values == null)
			{
				this.SetValuesFormula(null, null);
				return;
			}

			if (values.Length == 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_MustHaveOneAcceptedValue"));

			this.SetValuesFormula(ListDataValidationRule.CreateFormulaString(values, CultureInfo.InvariantCulture), null);
		}

		#endregion // SetValues

		#region SetValuesFormula

		/// <summary>
		/// Sets the formula which specifies the accepted values.
		/// </summary>
		/// <param name="valuesFormula">The formula which provides the accepted values for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// The formula must be a string containing the list of accepted values or a reference to a cell or region in the same Workbook 
		/// which contains the accepted values.
		/// </p>
		/// <p class="body">
		/// If a formula equaling a string is specified, it must be a string literal and it cannot be concatenated. For example, an 
		/// acceptable formula would be ="A,B,C". If one of the values must contain a double quote ("), the character should be repeated 
		/// in the list, like so: ="A,""B"",C". This will allow the values A, "B", and C. The separator between values must be a comma (,), 
		/// unless the decimal separator for the current culture is a comma, in which case the separator must be a semicolon (;).
		/// </p>
		/// <p class="body">
		/// If a formula equaling one or more references is specified, it must be a reference to a single cell or region in the same Workbook. 
		/// Union, intersection, and range operators are not allowed. An acceptable formula might be =$A$1 or =Sheet2!$A$1:$A$5. In addition 
		/// to a single cell or region, a named reference can also be used, but only if it refers to a single cell or region. If a region is 
		/// specified, or a named reference that refers to a region, the region must consist of a single row or column. A formula that equals 
		/// an error value is also allowed, but will cause the cell to not accept any values and the drop down to be empty, so it is not very 
		/// useful.
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
		/// Occurs when <paramref name="valuesFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="valuesFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value contains something other than a string or reference.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value contains a region reference which has more than one row and column.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="address"/> is not a valid cell or regions address.
		/// </exception>
		/// <seealso cref="SetValues"/>
		/// <seealso cref="GetValuesFormula(string)"/>
		/// <seealso cref="GetValuesFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetValuesFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public void SetValuesFormula(string valuesFormula, string address)
		{
			// MD 4/9/12 - TFS101506
			//this.SetValuesFormula(valuesFormula, address, this.CurrentFormat, this.CellReferenceMode, CultureInfo.CurrentCulture);
			this.SetValuesFormula(valuesFormula, address, this.CurrentFormat, this.CellReferenceMode, this.Culture);
		}

		/// <summary>
		/// Sets the formula which specifies the accepted values.
		/// </summary>
		/// <param name="valuesFormula">The formula which provides the accepted values for the rule.</param>
		/// <param name="address">
		/// The address of the cell or region that serves as the basis for relative references, or null to use the top-left cell of 
		/// the worksheet.
		/// </param>
		/// <param name="format">The workbook format with which to parse <paramref name="address"/>.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse <paramref name="address"/>.</param>
		/// <param name="culture">The culture to use when parsing the formula string.</param>
		/// <remarks>
		/// <p class="body">
		/// The formula must be a string containing the list of accepted values or a reference to a cell or region in the same Workbook 
		/// which contains the accepted values.
		/// </p>
		/// <p class="body">
		/// If a formula equaling a string is specified, it must be a string literal and it cannot be concatenated. For example, an 
		/// acceptable formula would be ="A,B,C". If one of the values must contain a double quote ("), the character should be repeated 
		/// in the list, like so: ="A,""B"",C". This will allow the values A, "B", and C. The separator between values must be a comma (,), 
		/// unless the decimal separator for the current culture is a comma, in which case the separator must be a semicolon (;).
		/// </p>
		/// <p class="body">
		/// If a formula equaling one or more references is specified, it must be a reference to a single cell or region in the same Workbook. 
		/// Union, intersection, and range operators are not allowed. An acceptable formula might be =$A$1 or =Sheet2!$A$1:$A$5. In addition 
		/// to a single cell or region, a named reference can also be used, but only if it refers to a single cell or region. If a region is 
		/// specified, or a named reference that refers to a region, the region must consist of a single row or column. A formula that equals 
		/// an error value is also allowed, but will cause the cell to not accept any values and the drop down to be empty, so it is not very 
		/// useful.
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
		/// Occurs when <paramref name="valuesFormula"/> is null and the rule is currently applied to a <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// Occurs when <paramref name="valuesFormula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value contains something other than a string or reference.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the specified value contains a region reference which has more than one row and column.
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
		/// <seealso cref="SetValues"/>
		/// <seealso cref="GetValuesFormula(string)"/>
		/// <seealso cref="GetValuesFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		/// <seealso cref="SetValuesFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public void SetValuesFormula(string valuesFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 4/9/12 - TFS101506
			//CultureInfo cultureResolved = culture ?? CultureInfo.CurrentCulture;
			CultureInfo cultureResolved = culture ?? this.Culture;

			// MD 12/21/11 - TFS97840
			// ParseFormula is no longer static because it need to access some instance members.
			//Formula parsedFormula = LimitedValueDataValidationRule.ParseFormula(valuesFormula, format, cellReferenceMode, cultureResolved);
			Formula parsedFormula = this.ParseFormula(valuesFormula, format, cellReferenceMode, cultureResolved);

			if (parsedFormula != null && parsedFormula.PostfixTokenList.Count == 1)
			{
				StrToken strToken = parsedFormula.PostfixTokenList[0] as StrToken;

				if (strToken != null)
				{
					string decimalSeparator = cultureResolved.NumberFormat.NumberDecimalSeparator;

					if (decimalSeparator == ",")
					{
						this.SetValues(ListDataValidationRule.GetValuesHelper(strToken.Value, cultureResolved));
						return;
					}
				}
			}

			// MD 4/9/12 - TFS101506
			//this.SetValuesFormulaInternal(parsedFormula, address, format, cellReferenceMode);
			this.SetValuesFormulaInternal(parsedFormula, address, format, cellReferenceMode, culture);
		}

		#endregion  // SetValuesFormula

		#region TryGetValues

		/// <summary>
		/// Tries to obtain the value of the constraint.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The list of values can only be obtained if it was set with the <see cref="SetValues"/> method or a formula is applied with one of 
		/// the SetValuesFormula methods and the formula equals a constant string with a list of values, such as ="A,B,C".
		/// </p>
		/// </remarks>
		/// <param name="values">When the method returns, will be an array of values or null if the list of values could not be obtained.</param>
		/// <returns>True if the list of values could be obtained; False otherwise.</returns>
		/// <seealso cref="SetValuesFormula(string,string)"/>
		/// <seealso cref="SetValuesFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
		public bool TryGetValues(out object[] values)
		{
			values = null;

			if (this.valuesFormula == null)
				return false;

			Debug.Assert(this.valuesFormula.PostfixTokenList.Count == 1, "Incorrect number of tokens.");

			StrToken strToken = this.valuesFormula.PostfixTokenList[0] as StrToken;
			if (strToken == null)
				return false;

			values = ListDataValidationRule.GetValuesHelper(strToken.Value, CultureInfo.InvariantCulture);
			return true;
		}

		#endregion  // TryGetValues

		#endregion  // Public Methods

		#region Internal Methods

		#region CreateFormulaString

		private static string CreateFormulaString(object[] values, CultureInfo culture)
		{
			StringBuilder formulaString = new StringBuilder("=\"");

			string decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
			string unionOperator = FormulaParser.GetUnionOperatorResolved(decimalSeparator);

			bool hasAtLeastOneValue = false;
			for (int i = 0; i < values.Length; i++)
			{
				object value = values[i];

				if (value == null)
					continue;

				hasAtLeastOneValue = true;

				string stringValue;

				IConvertible convertible = value as IConvertible;
				if (convertible != null)
					stringValue = convertible.ToString(culture);
				else
					stringValue = value.ToString() ?? string.Empty;

				if (value is bool)
					stringValue = stringValue.ToUpper();

				stringValue = stringValue.Replace("\"", "\"\"");

				if (ListDataValidationRule.ValueNeedsSingleQuoteInFormulaString(value, stringValue, culture))
					stringValue = "'" + stringValue;

				formulaString.Append(stringValue);

				if (i != values.Length - 1)
					formulaString.Append(unionOperator);
			}

			if (hasAtLeastOneValue == false)
				formulaString.Append(unionOperator);

			formulaString.Append("\"");
			return formulaString.ToString();
		}

		#endregion  // CreateFormulaString

		#region GetValuesFormulaInternal

		// MD 4/9/12 - TFS101506
		//internal Formula GetValuesFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		//{
		//    return LimitedValueDataValidationRule.ResolveFormula(this.valuesFormula, address, format, cellReferenceMode, true);
		//}
		internal Formula GetValuesFormulaInternal(string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			return this.ResolveFormula(this.valuesFormula, address, format, cellReferenceMode, culture, true);
		}

		#endregion  // GetValuesFormulaInternal

		#region GetValuesHelper

		private static object[] GetValuesHelper(string listOfValues, CultureInfo culture)
		{
			string unionOperator = FormulaParser.GetUnionOperatorResolved(culture.NumberFormat.NumberDecimalSeparator);

			string[] valueStrings = listOfValues.Split(new string[] { unionOperator }, StringSplitOptions.RemoveEmptyEntries);

			if (valueStrings.Length == 0)
				return new object[0];

			object[] values = new object[valueStrings.Length];

			for (int i = 0; i < values.Length; i++)
			{
				string valueString = valueStrings[i];

				double number;
				// MD 4/9/12 - TFS101506
				//if (FormulaParser.TryParseNumber(valueString, culture, out number))
				if (MathUtilities.DoubleTryParse(valueString, culture, out number))
				{
					values[i] = number;
					continue;
				}

				bool booleanValue;
				if (FormulaParser.TryParseBoolean(valueString, out booleanValue))
				{
					values[i] = booleanValue;
					continue;
				}

				// MD 4/9/12 - TFS101506
				//ErrorValue errorValue = FormulaParser.ParseError(valueString);
				ErrorValue errorValue = FormulaParser.ParseError(valueString, culture);

				if (errorValue != null)
				{
					values[i] = errorValue;
					continue;
				}

				valueString = valueString.Replace("\"\"", "\"");

				if (valueString.StartsWith("'"))
					valueString = valueString.Substring(1);

				values[i] = valueString;
			}

			return values;
		}

		#endregion  // GetValuesHelper

		#region SetValuesFormulaInternal

		// MD 4/9/12 - TFS101506
		//private void SetValuesFormulaInternal(Formula valuesFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		private void SetValuesFormulaInternal(Formula valuesFormula, string address, WorkbookFormat format, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 12/21/11 - TFS97840
			// Added a parameter to determine whether formula1 or formula2 is being validated.
			//this.VerifyFormula(valuesFormula, this.ParentCollection, null, "valuesFormula", true);
			// MD 2/24/12 - 12.1 - Table Support
			//this.VerifyFormula(valuesFormula, this.ParentCollection, null, "valuesFormula", true, true);
			this.VerifyFormula(valuesFormula, this.ParentCollection, null, "valuesFormula", true, true, format);

			this.valuesFormula = this.ResolveFormula(valuesFormula, address, format, cellReferenceMode, culture, false);
		}

		#endregion  // SetValuesFormulaInternal

		#region ValueNeedsSingleQuoteInFormulaString

		private static bool ValueNeedsSingleQuoteInFormulaString(object value, string stringValue, CultureInfo culture)
		{
			if (stringValue.Length == 0)
				return true;

			if (value is ErrorValue == false)
			{
				// MD 4/9/12 - TFS101506
				//if (FormulaParser.ParseError(stringValue) != null)
				if (FormulaParser.ParseError(stringValue, culture) != null)
					return true;
			}

			if (value is bool == false)
			{
				bool booleanValue;
				if (FormulaParser.TryParseBoolean(stringValue, out booleanValue))
					return true;
			}

			if (Utilities.IsNumericType(value) == false)
			{
				double number;
				// MD 4/9/12 - TFS101506
				//if (FormulaParser.TryParseNumber(stringValue, culture, out number))
				if (MathUtilities.DoubleTryParse(stringValue, culture, out number))
					return true;
			}

			return false;
		}

		#endregion  // ValueNeedsSingleQuoteInFormulaString

		#endregion  // Internal Methods

		#endregion // Methods

		#region Properties

		#region ShowDropdown

		/// <summary>
		/// Gets or sets the value which indicates whether a drop down should be displayed in Microsoft Excel with the list of accepted values.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is True, a drop down arrow will be displayed next to the cell when it is selected. When the user clicks the drop down arrow,
		/// a drop down will be displayed with the list of accepted values.
		/// </p>
		/// </remarks>
		public bool ShowDropdown
		{
			get { return this.showDropdown; }
			set { this.showDropdown = value; }
		}

		#endregion // ShowDropdown

		#endregion // Properties
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