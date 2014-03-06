using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// Represents a data validation rule which allows any value to be set on the target cells.
	/// This would be used to provide an input message to the user when the cell was selected.
	/// </summary>
	/// <seealso cref="Worksheet.DataValidationRules"/>
	/// <seealso cref="DataValidationRuleCollection.Add(AnyValueDataValidationRule,WorksheetCell)"/>
	/// <seealso cref="DataValidationRuleCollection.Add(AnyValueDataValidationRule,WorksheetRegion)"/>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 sealed class AnyValueDataValidationRule : DataValidationRule
	{
		#region Constructor

		/// <summary>
		/// Creates a new <see cref="AnyValueDataValidationRule"/> instance.
		/// </summary>
		public AnyValueDataValidationRule()
		{
		}

		#endregion  // Constructor

		#region Base Class Overrides

		internal override bool AllowNullInternal
		{
			get { return true; }
			set { Debug.Assert(value, "AllowNullInternal is not valid on the AnyValueDataValidationRule."); }
		}

		internal override Formula GetFormula1(string address)
		{
			return null;
		}

		internal override Formula GetFormula2(string address)
		{
			return null;
		}

		internal override DataValidationOperatorType OperatorType
		{
			get { return (DataValidationOperatorType)0; }
		}

		internal override void SetFormula1(Formula formula, string address)
		{
			Debug.Assert(formula == null, "Formula1 is not valid on the AnyValueDataValidationRule.");
		}

		internal override void SetFormula2(Formula formula, string address)
		{
			Debug.Assert(formula == null, "Formula2 is not valid on the AnyValueDataValidationRule."); 
		}

		internal override DataValidationType ValidationType
		{
			get { return DataValidationType.AnyValue; }
		}

		internal override void VerifyState(DataValidationRuleCollection collection, WorksheetReferenceCollection references)
		{
			// This rule is always in a valid state.
		}

		#endregion  // Base Class Overrides
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