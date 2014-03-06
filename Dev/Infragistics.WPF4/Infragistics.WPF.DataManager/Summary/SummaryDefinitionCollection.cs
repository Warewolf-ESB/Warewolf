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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics
{
	/// <summary>
	/// A collection of <see cref="SummaryDefinition"/> objects.
	/// </summary>
	public class SummaryDefinitionCollection : CollectionBase<SummaryDefinition>
	{
		#region GetDefinitionsByKey

		/// <summary>
		/// Returns a collection of <see cref="SummaryDefinition"/> which are applied to a given column based on it's key value.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ReadOnlyCollection<SummaryDefinition> GetDefinitionsByKey(string key)
		{
			List<SummaryDefinition> sums = new List<SummaryDefinition>();

			foreach (SummaryDefinition sd in this.Items)
			{
				if (sd.ColumnKey == key)
				{
					sums.Add(sd);
				}
			}

			return new ReadOnlyCollection<SummaryDefinition>(sums);
		}
		#endregion // GetDefinitionsByKey

		#region GetDefinitionsBySummaryExecution

		/// <summary>
		/// Returns a collection of <see cref="SummaryDefinition"/>  based on when the summary is applied during databind.
		/// </summary>
		/// <param name="summaryExecution"></param>
		/// <returns></returns>
		public ReadOnlyCollection<SummaryDefinition> GetDefinitionsBySummaryExecution(SummaryExecution summaryExecution)
		{
			return this.GetDefinitionsBySummaryExecution(summaryExecution, false);
		}

		/// <summary>
		/// Returns a collection of <see cref="SummaryDefinition"/>  based on when the summary is applied during databind.
		/// </summary>
		/// <param name="summaryExecution"></param>
		/// <param name="includeNulls"></param>
		/// <returns></returns>
		public ReadOnlyCollection<SummaryDefinition> GetDefinitionsBySummaryExecution(SummaryExecution summaryExecution, bool includeNulls)
		{
			List<SummaryDefinition> sums = new List<SummaryDefinition>();

			foreach (SummaryDefinition sd in this.Items)
			{
				if (sd.SummaryOperand != null &&
					sd.SummaryOperand.SummaryCalculator != null &&
					sd.SummaryOperand.SummaryCalculator.SummaryExecution == summaryExecution ||
						(includeNulls && sd.SummaryOperand.SummaryCalculator.SummaryExecution == null))
				{
					sums.Add(sd);
				}
			}

			return new ReadOnlyCollection<SummaryDefinition>(sums);
		}

		#endregion // GetDefinitionsBySummaryExecution

		#region AddItemSilently
		/// <summary>
		/// Adds an element to the collection without invoking any events.
		/// </summary>
		/// <param name="item"></param>
		protected internal void AddItemSilently(SummaryDefinition item)
		{
			base.AddItemSilently(this.Items.Count, item);
		}
		#endregion // AddItemSilently
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