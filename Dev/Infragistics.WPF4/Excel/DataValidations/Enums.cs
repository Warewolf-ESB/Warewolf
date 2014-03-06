using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;

// MD 5/13/11 - Data Validations / Page Breaks
namespace Infragistics.Documents.Excel
{
	#region DataValidationCriteria

	/// <summary>
	/// Determines what types of cell values are allowed and how the cell value is validated against the constraint(s).
	/// </summary>
	/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>
	/// <seealso cref="OneConstraintDataValidationRule"/>
	/// <seealso cref="TwoConstraintDataValidationRule"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 enum DataValidationCriteria
	{
		/// <summary>
		/// Only numbers are allowed and they cannot have a fractional part.
		/// When comparing against constraints, the cell value is used.
		/// </summary>
		WholeNumber = DataValidationType.WholeNumber,

		/// <summary>
		/// Only numbers are allowed.
		/// When comparing against constraints, the cell value is used.
		/// </summary>
		Decimal = DataValidationType.Decimal,

		/// <summary>
		/// Dates with or without time portions are allowed as well as numbers equivalent to valid dates.
		/// When comparing against constraints, the cell value is used.
		/// </summary>
		Date = DataValidationType.Date,

		/// <summary>
		/// Times are allowed are well as numbers equivalent to valid times without a date portion.
		/// When comparing against constraints, the cell value is used.
		/// </summary>
		Time = DataValidationType.Time,

		/// <summary>
		/// Non-error values are allowed.
		/// When comparing against constraints, the length of the cell value's text equivalent is used.
		/// </summary>
		TextLength = DataValidationType.TextLength,
	}

	#endregion  // DataValidationCriteria

	#region DataValidationErrorStyle

	/// <summary>
	/// Represents the various styles in which invalid values are handled by Microsoft Excel. When error messages are not 
	/// shown for invalid values, the error style is ignored and all invalid values are allowed to be set on cells.
	/// </summary>
	/// <seealso cref="DataValidationRule.ErrorStyle"/>
	/// <seealso cref="DataValidationRule.ShowErrorMessageForInvalidValue"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 enum DataValidationErrorStyle
	{
		/// <summary>
		/// Invalid values are not allowed. The error dialog shown in Microsoft Excel displays an error icon and retry and cancel 
		/// buttons. The retry button will put focus back in the cell and allow the user to try to enter a new value. The cancel 
		/// button will cancel the edit and revert the cell back to the previous value it contained before the edit operation.
		/// </summary>
		Stop = ErrorAlertStyle.Stop,

		/// <summary>
		/// Invalid values are allowed. The error dialog shown in Microsoft Excel displays a warning icon, asks the user if they 
		/// want to continue, and has yes, no and cancel buttons. The yes button commits the value to the cell. The no button will
		/// put focus back in the cell and allow the user to try to enter a new value. And the cancel button will cancel the edit 
		/// and revert the cell back to the previous value it contained before the edit operation.
		/// </summary>
		Warning = ErrorAlertStyle.Warning,

		/// <summary>
		/// Invalid values are allowed. The error dialog shown in Microsoft Excel displays an information icon and ok and cancel 
		/// buttons. The ok button commits the value to the cell. The cancel button will cancel the edit and revert the cell back 
		/// to the previous value it contained before the edit operation.
		/// </summary>
		Information = ErrorAlertStyle.Infromation,
	}

	#endregion  // DataValidationErrorStyle

	#region OneConstraintDataValidationOperator

	/// <summary>
	/// Represents the various operators which can be used when validating the cell value against a constraint.
	/// </summary>
	/// <seealso cref="OneConstraintDataValidationRule"/>
	/// <seealso cref="OneConstraintDataValidationRule.SetConstraint(double)"/>
	/// <seealso cref="OneConstraintDataValidationRule.SetConstraint(TimeSpan)"/>
	/// <seealso cref="OneConstraintDataValidationRule.SetConstraint(DateTime)"/>
	/// <seealso cref="OneConstraintDataValidationRule.GetConstraintFormula(string)"/>
	/// <seealso cref="OneConstraintDataValidationRule.GetConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
	/// <seealso cref="OneConstraintDataValidationRule.SetConstraintFormula(string,string)"/>
	/// <seealso cref="OneConstraintDataValidationRule.SetConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
	/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 enum OneConstraintDataValidationOperator
	{
		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is equal to the 
		/// constraint applied to the validation rule.
		/// </summary>
		EqualTo = DataValidationOperatorType.Equal,

		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is not equal to the 
		/// constraint applied to the validation rule.
		/// </summary>
		NotEqualTo = DataValidationOperatorType.NotEqual,

		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is greater than the 
		/// constraint applied to the validation rule.
		/// </summary>
		GreaterThan = DataValidationOperatorType.GreaterThan,

		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is greater than or equal to the 
		/// constraint applied to the validation rule.
		/// </summary>
		GreaterThanOrEqualTo = DataValidationOperatorType.GreaterThanOrEqual,

		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is less than the 
		/// constraint applied to the validation rule.
		/// </summary>
		LessThan = DataValidationOperatorType.LessThan,

		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is less than or equal to the 
		/// constraint applied to the validation rule.
		/// </summary>
		LessThanOrEqualTo = DataValidationOperatorType.LessThanOrEqual,
	}

	#endregion  // OneConstraintDataValidationOperator

	#region TwoConstraintDataValidationOperator

	/// <summary>
	/// Represents the various operators which can be used when validating the cell value against two constraints.
	/// </summary>
	/// <seealso cref="TwoConstraintDataValidationRule"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetLowerConstraint(double)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetLowerConstraint(DateTime)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetLowerConstraint(TimeSpan)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetUpperConstraint(double)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetUpperConstraint(DateTime)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetUpperConstraint(TimeSpan)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.GetLowerConstraintFormula(string)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.GetLowerConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetLowerConstraintFormula(string,string)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetLowerConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.GetUpperConstraintFormula(string)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.GetUpperConstraintFormula(string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetUpperConstraintFormula(string,string)"/>
	/// <seealso cref="TwoConstraintDataValidationRule.SetUpperConstraintFormula(string,string,WorkbookFormat,CellReferenceMode,CultureInfo)"/>
	/// <seealso cref="ValueConstraintDataValidationRule.ValidationCriteria"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 enum TwoConstraintDataValidationOperator
	{
		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is between the 
		/// constraints applied to the validation rule. The constraints are inclusive.
		/// </summary>
		Between = DataValidationOperatorType.Between,

		/// <summary>
		/// Only allows the cell value if it or its text length, depending on the validation criteria, is not between the 
		/// constraints applied to the validation rule. The constraints are exclusive.
		/// </summary>
		NotBetween = DataValidationOperatorType.NotBetween,
	}

	#endregion  // TwoConstraintDataValidationOperator
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