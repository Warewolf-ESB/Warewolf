using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infragistics
{
	#region IExpressConditions

	/// <summary>
	/// An interface which will be used by filtering to generate the expression to be applied.
	/// </summary>
	public interface IExpressConditions
	{
		/// <summary>
		/// Creates an expression based on a given <see cref="FilterContext"/>
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		Expression GetCurrentExpression(FilterContext context);

		/// <summary>
		/// Creates an expression. 
		/// </summary>
		/// <returns></returns>
		Expression GetCurrentExpression();
	}

	#endregion // IExpressConditions

	#region IFilterCondition

	/// <summary>
	/// Interface that defines an object that can participate in filtering.
	/// </summary>
	public interface IFilterCondition : IExpressConditions, INotifyPropertyChanged
	{
		/// <summary>
		/// The <see cref="IRecordFilter"/> object that ultimately is the parent of this object.
		/// </summary>
		IRecordFilter Parent { get; set; }
	}

	#endregion // IFilterCondition

	#region IGroupFilterConditions

	/// <summary>
	/// Interface that describes and object that can contain other IFilterCondition objects and will generate an expression for all of them.
	/// </summary>
	public interface IGroupFilterConditions : IExpressConditions
	{
		/// <summary>
		/// The <see cref="LogicalOperator"/> which will be used to combine all the Conditions listed by this group.
		/// </summary>
		LogicalOperator LogicalOperator { get; }

		/// <summary>
		/// Event raised when an Item in the Collection is changed.
		/// </summary>
		event EventHandler<EventArgs> CollectionItemChanged;
	}

	#endregion // IGroupFilterConditions
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