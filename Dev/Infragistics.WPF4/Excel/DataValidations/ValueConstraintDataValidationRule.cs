using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Base class for all data validations rules which compare the cell value against one or more constraint when determining
	/// the validity of the cell value.
	/// </summary>
	/// <seealso cref="OneConstraintDataValidationRule"/>
	/// <seealso cref="TwoConstraintDataValidationRule"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 abstract class ValueConstraintDataValidationRule : LimitedValueDataValidationRule
	{
		#region Member Variables

		private DataValidationCriteria validationCriteria = DataValidationCriteria.Decimal;

		#endregion  // Member Variables

		#region Constructor

		internal ValueConstraintDataValidationRule()
		{
		}

		#endregion  // Constructor

		#region Base Class Overrides

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

		internal override DataValidationType ValidationType
		{
			get { return (DataValidationType)this.validationCriteria; }
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

			// Complex formulas cannot be verified to be numbers or references.
			if (formula.PostfixTokenList.Count != 1)
				return;

			FormulaToken token = formula.PostfixTokenList[0];

			NumberToken numberToken = token as NumberToken;
			IntToken intToken = token as IntToken;
			ReferenceToken referenceToken = token as ReferenceToken;

			if (referenceToken != null)
			{
				// We can skip verifying this. When the base call to VerifyFormula hits a name token, it will call back into VerifyFormula with the named reference's formula,
				// so we will verify it's tokens then.
				if (referenceToken is NameToken)
					return;

				if (referenceToken is Area3DNToken ||
					referenceToken is Area3DToken ||
					referenceToken is AreaErr3DToken ||
					referenceToken is AreaErrToken ||
					referenceToken is AreaNToken ||
					referenceToken is AreaToken)
				{
					throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ConstraintFormulaMustBeNumberOrReference", formulaPropertyName));
				}
			}
			else if (numberToken == null && intToken == null)
			{
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DV_ConstraintFormulaMustBeNumberOrReference", formulaPropertyName));
			}
		}

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region SetValidationCriteria

		internal void SetValidationCriteria(DataValidationCriteria value, string paramName)
		{
			if (Enum.IsDefined(typeof(DataValidationCriteria), value) == false)
				throw new InvalidEnumArgumentException(paramName, (int)value, typeof(DataValidationCriteria));

			this.validationCriteria = value;
		}

		#endregion  // SetValidationCriteria

		#endregion  // Public Methods

		#region Internal Methods

		#region GetConstraint

		internal static double? GetConstraint(Formula formula)
		{
			if (formula == null)
				return null;

			if (formula.PostfixTokenList.Count != 1)
				return null;

			FormulaToken token = formula.PostfixTokenList[0];

			IntToken intToken = token as IntToken;
			if (intToken != null)
				return intToken.Value;

			NumberToken numberToken = token as NumberToken;
			if (numberToken != null)
				return numberToken.Value;

			return null;
		}

		#endregion  // GetConstraint

		#endregion  // Internal Methods

		#endregion  // Methods

		#region Properties

		#region ValidationCriteria

		/// <summary>
		/// Gets or sets the criteria to use when validating the cell value against the constraint(s).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Depending on the value specified, either then cell value or the length of its text equivalent will be compared against the 
		/// constraint(s). In addition, certain ValidationCriteria values may disallow a cell value even if it is valid when compared to
		/// the constraint. For example, the ValidationCriteria.WholeNumber value will not allow any number with a fractional portion.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when the value is not a member of the <see cref="DataValidationCriteria"/> enumeration.
		/// </exception>
		public DataValidationCriteria ValidationCriteria
		{
			get { return this.validationCriteria; }
			set { this.SetValidationCriteria(value, "value"); }
		}

		#endregion  // ValidationCriteria

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