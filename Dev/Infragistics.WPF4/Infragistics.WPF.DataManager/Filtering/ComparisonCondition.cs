using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;
namespace Infragistics
{
	/// <summary>
	/// Represents a <see cref="ComparisonConditionBase"/> object that is based on a <see cref="ComparisonOperator"/> operator.
	/// </summary>
	public class ComparisonCondition : ComparisonConditionBase
	{
		#region Members
		ComparisonOperator _operator = ComparisonOperator.Equals;
		object _filterValue;
		bool? _caseSensitive = null;
		#endregion

		#region Properties

		#region Public

		#region Operator

		/// <summary>
		/// Gets / sets the <see cref="ComparisonCondition"/> that should be applied.
		/// </summary>
		public ComparisonOperator Operator
		{
			get
			{
				return this._operator;
			}
			set
			{
				if (this._operator != value)
				{
					this._operator = value;
					this.OnPropertyChanged("Operator");
				}
			}
		}

		#endregion // Operator

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

		#region CaseSensitive

		/// <summary>
		/// Gets / sets if the filter that will be built from this term will be case sensitive.		
		/// </summary>
		/// <remarks >
		/// This value is only used for string columns.
		/// </remarks>
		public bool? CaseSensitive
		{
			get
			{
				return this._caseSensitive;
			}
			set
			{
				if (this._caseSensitive != value)
				{
					this._caseSensitive = value;
					this.OnPropertyChanged("CaseSensitive");
				}
			}
		}

		#endregion // CaseSensitive

		#endregion // Public

		#endregion // Properties

		#region Methods

		#region Protected

		#region GetCurrentExpression

		/// <summary>
		/// Generates the current expression for this <see cref="ComparisonConditionBase"/> using the inputted context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected override Expression GetCurrentExpression(FilterContext context)
		{
			if (this.Parent != null)
			{
                if (context.FieldDataType != this.Parent.FieldType)
                    return GetCurrentExpression();

				if (this.CaseSensitive == null)
				{
					return context.CreateExpression(this.Parent.FieldName, this.Operator, this.FilterValue);
				}
				else
				{
					return context.CreateExpression(this.Parent.FieldName, this.Operator, this.FilterValue, (bool)this.CaseSensitive);
				}
			}
			return null;
		}

		/// <summary>
		/// Generates the current expression for this <see cref="ComparisonConditionBase"/>.
		/// </summary>
		/// <returns></returns>
		protected override Expression GetCurrentExpression()
		{
			if (this.Parent != null)
			{
				bool resolvedCaseSensitive = false;
				if (this.CaseSensitive != null)
					resolvedCaseSensitive = (bool)this.CaseSensitive;

				FilterContext context = FilterContext.CreateGenericFilter(this.Parent.ObjectTypedInfo, this.Parent.FieldType, resolvedCaseSensitive, this.Parent.FieldType == typeof(DateTime)  );

				return GetCurrentExpression(context);
			}
			return null;
		}

		#endregion // GetCurrentExpression

		#endregion // Protected

		#endregion // Methods
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