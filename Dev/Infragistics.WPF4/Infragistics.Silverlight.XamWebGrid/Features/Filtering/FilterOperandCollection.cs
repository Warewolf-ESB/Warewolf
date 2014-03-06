using System.Collections.ObjectModel;
using System;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of <see cref="FilterOperand"/> objects.
	/// </summary>
	public class FilterOperandCollection : ObservableCollection  <FilterOperand>
	{
		#region Remove

		/// <summary>
		/// Removes an object based on the ComparisonOperator.
		/// </summary>
		/// <param name="condition"></param>
		public void Remove(ComparisonOperator condition)
		{
			int count = this.Count - 1;
			for (int i = count ; i >= 0; i--)
			{
				FilterOperand fo = this.Items[i];
				if ((ComparisonOperator)fo.ComparisonOperatorValue == condition)
				{
					this.Remove(this[i]);
				}
			}
		}

		#endregion // Remove

		#region Iterators

		/// <summary>
		/// Returns the first object in the collection that shares the <see cref="ComparisonOperator"/>.
		/// </summary>
		/// <param name="op"></param>
		/// <returns></returns>
		public FilterOperand this[ComparisonOperator op]
		{
			get
			{
				int count = this.Count - 1;

				for (int i = 0; i < count; i++)
				{
					FilterOperand fo = this.Items[i];

					if ((ComparisonOperator)fo.ComparisonOperatorValue == op)
					{
						return fo;						
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Returns the first object in the collection that shares the Type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public FilterOperand this[Type type]
		{
			get
			{
				int count = this.Count - 1;

				for (int i = 0; i < count; i++)
				{
					FilterOperand fo = this.Items[i];

					if (fo.GetType() == type)
					{
						return fo;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the FilterOperand in the collection at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public new FilterOperand this[int index]
		{
			get
			{
				return this.Items[index];
			}
		}

		#endregion // Iterators
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