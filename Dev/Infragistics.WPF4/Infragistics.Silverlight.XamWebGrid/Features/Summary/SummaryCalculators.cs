
namespace Infragistics.Controls.Grids
{
	#region MaximumSummaryCalculator

	/// <summary>
	/// A <see cref="SummaryCalculatorBase"/> base which will execute the LINQ Maximum summary.
	/// </summary>
	public class MaximumSummaryCalculator : SummaryCalculatorBase, ISupportLinqSummaries
	{
		#region ISupportLinqSummaries Members

		#region SummaryType

		/// <summary>
		/// Gets the <see cref="LinqSummaryOperator"/> which designates which LINQ summary to use.
		/// </summary>
		LinqSummaryOperator ISupportLinqSummaries.SummaryType
		{
			get { return LinqSummaryOperator.Maximum; }
		}

		#endregion // SummaryType

		#region SummaryContext

		/// <summary>
		/// Gets / sets the <see cref="SummaryContext"/> that will be used by the summary framework to build the summary.
		/// </summary>
		SummaryContext ISupportLinqSummaries.SummaryContext
		{
			get; set;
		}

		#endregion // SummaryContext

		#endregion // ISupportLinqSummaries Members
	}

	#endregion // MaximumSummaryCalculator

	#region MinimumSummaryCalculator

	/// <summary>
	/// A <see cref="SummaryCalculatorBase"/> base which will execute the LINQ Minimum summary.
	/// </summary>
	public class MinimumSummaryCalculator : SummaryCalculatorBase, ISupportLinqSummaries
	{
		#region ISupportLinqSummaries Members

		#region SummaryType

		/// <summary>
		/// Gets the <see cref="LinqSummaryOperator"/> which designates which LINQ summary to use.
		/// </summary>
		LinqSummaryOperator ISupportLinqSummaries.SummaryType
		{
			get { return LinqSummaryOperator.Minimum; }
		}

		#endregion // SummaryType

		#region SummaryContext

		/// <summary>
		/// Gets / sets the <see cref="SummaryContext"/> that will be used by the summary framework to build the summary.
		/// </summary>
		SummaryContext ISupportLinqSummaries.SummaryContext
		{
			get;
			set;
		}

		#endregion // SummaryContext

		#endregion // ISupportLinqSummaries Members
	}

	#endregion // MinimumSummaryCalculator

	#region CountSummaryCalculator

	/// <summary>
	/// A <see cref="SummaryCalculatorBase"/> base which will execute the LINQ Count summary.
	/// </summary>
	public class CountSummaryCalculator : SummaryCalculatorBase, ISupportLinqSummaries
	{
		#region ISupportLinqSummaries Members

		#region SummaryType

		/// <summary>
		/// Gets the <see cref="LinqSummaryOperator"/> which designates which LINQ summary to use.
		/// </summary>
		LinqSummaryOperator ISupportLinqSummaries.SummaryType
		{
			get { return LinqSummaryOperator.Count; }
		}

		#endregion // SummaryType


		#region SummaryContext

		/// <summary>
		/// Gets / sets the <see cref="SummaryContext"/> that will be used by the summary framework to build the summary.
		/// </summary>
		SummaryContext ISupportLinqSummaries.SummaryContext
		{
			get;
			set;
		}

		#endregion // SummaryContext

		#endregion // ISupportLinqSummaries Members
	}

	#endregion // CountSummaryCalculator

	#region SumSummaryCalculator

	/// <summary>
	/// A <see cref="SummaryCalculatorBase"/> base which will execute the LINQ Sum summary.
	/// </summary>
	public class SumSummaryCalculator : SummaryCalculatorBase, ISupportLinqSummaries
	{
		#region ISupportLinqSummaries Members

		#region SummaryType

		/// <summary>
		/// Gets the <see cref="LinqSummaryOperator"/> which designates which LINQ summary to use.
		/// </summary>
		LinqSummaryOperator ISupportLinqSummaries.SummaryType
		{
			get { return LinqSummaryOperator.Sum; }
		}

		#endregion // SummaryType


		#region SummaryContext

		/// <summary>
		/// Gets / sets the <see cref="SummaryContext"/> that will be used by the summary framework to build the summary.
		/// </summary>
		SummaryContext ISupportLinqSummaries.SummaryContext
		{
			get;
			set;
		}

		#endregion // SummaryContext

		#endregion // ISupportLinqSummaries Members
	}

	#endregion // SumSummaryCalculator

	#region AverageSummaryCalculator

	/// <summary>
	/// A <see cref="SummaryCalculatorBase"/> base which will execute the LINQ Average summary.
	/// </summary>
	public class AverageSummaryCalculator : SummaryCalculatorBase, ISupportLinqSummaries
	{
		#region ISupportLinqSummaries Members

		#region SummaryType

		/// <summary>
		/// Gets the <see cref="LinqSummaryOperator"/> which designates which LINQ summary to use.
		/// </summary>
		LinqSummaryOperator ISupportLinqSummaries.SummaryType
		{
			get { return LinqSummaryOperator.Average; }
		}

		#endregion // SummaryType


		#region SummaryContext

		/// <summary>
		/// Gets / sets the <see cref="SummaryContext"/> that will be used by the summary framework to build the summary.
		/// </summary>
		SummaryContext ISupportLinqSummaries.SummaryContext
		{
			get;
			set;
		}

		#endregion // SummaryContext

		#endregion // ISupportLinqSummaries Members
	}

	#endregion // AverageSummaryCalculator
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