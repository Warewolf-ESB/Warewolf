using System;
using System.Linq;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// Class used by Conditional Formatting to hold derived information about Percent formulated
	/// data and allow access to it.
	/// </summary>
	public class PercentData
	{
		#region Properties

		#region GeneratedMinimumValue
		/// <summary>
		/// Gets / sets the value determined to be the minimum from the data.
		/// </summary>
		protected internal double GeneratedMinimumValue { get; set; }
		#endregion // GeneratedMinimumValue

		#region GeneratedMaximumValue
		/// <summary>
		/// Gets / sets the value determined to be the maximum from the data.
		/// </summary>
		protected internal double GeneratedMaximumValue { get; set; }
		#endregion // GeneratedMaximumValue

		#region Range

		/// <summary>
		/// Gets the Range between the Minimum and Maximum values.
		/// </summary>
		public double Range { get; private set; }

		#endregion // Range

		#endregion // Properties

		#region Methods

		#region Static

		#region CreatePercentData

		/// <summary>
		/// Creates a <see cref="PercentData"/> object from the inputted values.
		/// </summary>
		/// <param name="objectDataType"></param>
		/// <param name="key"></param>
		/// <param name="query"></param>
		/// <returns></returns>
        public static PercentData CreatePercentData(CachedTypedInfo objectDataType, string key, IQueryable query)
		{
			PercentData data = new PercentData();

			SummaryContext minimumContext = SummaryContext.CreateGenericSummary(objectDataType, key, LinqSummaryOperator.Minimum);
			data.GeneratedMinimumValue = Convert.ToDouble(minimumContext.Execute(query), System.Globalization.CultureInfo.InvariantCulture);

			SummaryContext maximumContext = SummaryContext.CreateGenericSummary(objectDataType, key, LinqSummaryOperator.Maximum);
			data.GeneratedMaximumValue = Convert.ToDouble(maximumContext.Execute(query), System.Globalization.CultureInfo.InvariantCulture);

			data.Range = data.GeneratedMaximumValue - data.GeneratedMinimumValue;

			return data;
		}

		#endregion // CreatePercentData

		#endregion // Static

		#region GetPercentValue

		/// <summary>
		/// Determines the percentage that the value is displaced from the <see cref="GeneratedMinimumValue"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected internal double GetPercentValue(double value)
		{
			return ((value - this.GeneratedMinimumValue) / this.Range) * 100;
		}

		#endregion // GetPercentValue

		#region GetRealValue
		/// <summary>
		/// Determines the real value that would be associated with a given percentage in the given range.
		/// </summary>
		/// <param name="percentage"></param>
		/// <returns></returns>
        protected internal double GetRealValue(double percentage)
		{
			return ((percentage * this.Range) / 100) + this.GeneratedMinimumValue;
		}

		#endregion // GetRealValue

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