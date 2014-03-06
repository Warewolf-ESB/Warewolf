using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Infragistics
{
	/// <summary>
	/// Represents a <see cref="ComparisonConditionBase"/> object that is based on a custom Expression object.
	/// </summary>
	public class CustomComparisonCondition : ComparisonConditionBase
	{
		#region Members

		Expression _expression;
		object _filterValue;

		#endregion // Members

		#region Properties

		#region Expression

		/// <summary>
		/// Gets / sets the Expression that will be used by the filtering statement.
		/// </summary>
		public Expression Expression
		{
			get
			{
				return this._expression;
			}
			set
			{
				if (this._expression != value)
				{
					this._expression = value;
					this.OnPropertyChanged("Expression");
				}
			}
		}

		#endregion // Expression

		#region FilterValue

		/// <summary>
		/// Gets / sets the value that will be used to build the filter.
		/// </summary>
		public object FilterValue
		{
			get
			{
				return this._filterValue;
			}
			set
			{
				if (this._filterValue != value)
				{
					this._filterValue = value;
					this.OnPropertyChanged("FilterValue");
				}
			}
		}

		#endregion //FilterValue

		#region FilterOperand

		/// <summary>
		/// Get / set the type of the FilterOperand (via string)
		/// </summary>
		protected internal Type FilterOperand { get; set; }

		#endregion // FilterOperand

		#endregion // Properties

		#region Overrides

		#region GetCurrentExpression

		/// <summary>
		/// Generates the current expression for this <see cref="ComparisonConditionBase"/>.
		/// </summary>
		/// <returns></returns>
		protected override Expression GetCurrentExpression()
		{
			return this.Expression;
		}

		/// <summary>
		/// Generates the current expression for this <see cref="ComparisonConditionBase"/> using the inputted context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected override Expression GetCurrentExpression(FilterContext context)
		{
			return this.Expression;
		}

		#endregion // GetCurrentExpression

		#endregion // Overrides
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