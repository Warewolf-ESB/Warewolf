using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Collections;

namespace Infragistics.Calculations
{
	/// <summary>
	/// A collection of <see cref="ItemCalculation"/>s or <see cref="ListCalculation"/>s.
	/// </summary>
	/// <typeparam name="T">a <see cref="ItemCalculationBase"/> derived type</typeparam>
	/// <seealso cref="ItemCalculator.Calculations"/>
	/// <seealso cref="ListCalculator.ItemCalculations"/>
	/// <seealso cref="ListCalculator.ListCalculations"/>
	public class ItemCalculationBaseCollection<T> : ObservableCollectionExtended<T> where T : ItemCalculationBase
	{
		#region Constructor

		internal ItemCalculationBaseCollection()
		{
		}

		#endregion //Constructor	
    
		#region Methods

		#region Internal Methods

		#region InvalidateReferences

		internal void InvalidateReferences()
		{
			foreach (T calculation in this)
				calculation.InvalidateReference();
		}

		#endregion //InvalidateReferences	
    
		#endregion //Internal Methods	
    
		#endregion //Methods	
 	}

	/// <summary>
	/// A collection of <see cref="ItemCalculation"/>s that is exposed by an <see cref="ItemCalculator"/> or a <see cref="ListCalculator"/>
	/// </summary>
	/// <see cref="ItemCalculation"/>
	/// <see cref="ItemCalculator"/>
	/// <see cref="ListCalculator"/>
	public sealed class ItemCalculationCollection : ItemCalculationBaseCollection<ItemCalculation>
	{
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