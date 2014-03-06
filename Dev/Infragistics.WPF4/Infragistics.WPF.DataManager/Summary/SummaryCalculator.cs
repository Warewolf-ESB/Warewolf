using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;

namespace Infragistics
{
	#region SummaryCalculatorBase
	/// <summary>
	/// Provides a base class for SummaryCalculators for the Summary framework.
	/// </summary>
	public abstract class SummaryCalculatorBase
	{
		#region Properties

		/// <summary>
		/// Gets the <see cref="SummaryExecution"/>, indicating when the summary will be applied.  
		/// </summary>
		/// <remarks>
		/// When overridden, this can be used to indicate when an individual summary should be evaluated.   Depending 
		/// on when the summary is executed the final result of the evaluation can change.
		/// </remarks>
		public virtual SummaryExecution? SummaryExecution
		{
			get
			{
				return null;
			}
		}

		#endregion // Properties
	}
	#endregion // SummaryCalculatorBase

	#region SynchoronousSummaryCalculator

	/// <summary>
	/// A summary that will be executed during the normal databinding in process.
	/// </summary>
	public abstract class SynchronousSummaryCalculator : SummaryCalculatorBase
	{
		#region Summarize
		/// <summary>
		/// Calculates the summary information from the records provided by the query.
		/// </summary>
		/// <param name="data">The LINQ that provides the data which is currently available.</param>
		/// <param name="fieldKey">The name of the field being acted on.</param>
		/// <returns></returns>
		public abstract object Summarize(IQueryable data, string fieldKey);
		#endregion // Summarize
	}

	#endregion // SynchoronousSummaryCalculator

	#region AsynchoronousSummaryCalculator

	internal abstract class AsynchoronousSummaryCalculator : SummaryCalculatorBase
	{
		public virtual void OnResultCalculating(string fieldKey, object parentRowDataObject)
		{
		}

		public virtual void OnResultCalculated()
		{
		}
	}

	#endregion // AsynchoronousSummaryCalculator
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