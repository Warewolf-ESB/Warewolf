using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Controls
{
	// SSP 2/29/12 TFS89053
	// 

	/// <summary>
	/// Interface for providing custom logic for evaluating filter comparison operators. It also lets you provide custom
	/// logic for converting left-hand-side and right-hand-side values before the default built-in logic evaluates
	/// the comparison operator.
	/// </summary>
	public interface IFilterEvaluator
	{
		/// <summary>
		/// Called to evaluate the specified comparison operator. This method can also be used to convert the comparison 
		/// values using custom logic and let the default built-in evaluation logic handle the actual comparison.
		/// </summary>
		/// <param name="comparisonOperator">Comparison operator.</param>
		/// <param name="lhs">Value on the left hand side of the operator. You can set this by-ref parameter to its converted form
		/// and return null from the method to have the default built-in logic perform the evaluation of the operator using the converted value.</param>
		/// <param name="rhs">Value on the right hand side of the operator. Similarly to 'lhs' parameter, you can also set this by-ref parameter to its converted form.</param>
		/// <param name="context">Evaluation context. You can use the context's <see cref="ConditionEvaluationContext.SetUserCache"/>
		/// method to store any cached value for retrival in successive calls via the context's 
		/// <see cref="ConditionEvaluationContext.GetUserCache"/> method.
		/// </param>
		/// <returns>Returns true if the comparison of the specified values using specified operator evaluates to true.
		/// Returns false if the comparison fails. Returns null if you want to let the default built-in logic to handle
		/// evaluation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method lets you evaluate the comparison operator. You can choose to evaluate specific operators
		/// only and let the default logic handle the evaluation of other operators by returning null.
		/// Another use for this method is to provide custom conversion logic for converting the <paramref name="lhs"/> 
		/// and <paramref name="rhs"/> before the default built-in implementation evaluates the comparison. To do that
		/// you would set the 'lhs' and 'rhs' ref parameters to their converted representations and return null from
		/// the method. The default built-in evaluation logic will use the converted values.
		/// </para>
		/// </remarks>
		bool? Evaluate( ComparisonOperator comparisonOperator, ref object lhs, ref object rhs, ConditionEvaluationContext context );
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