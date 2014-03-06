using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Infragistics
{
	/// <summary>
	/// A base class for ComparisonConditions for filtering.
	/// </summary>
	public abstract class ComparisonConditionBase : IFilterCondition, INotifyPropertyChanged
	{
		#region Members
		private IRecordFilter _parent;
		#endregion

		#region Properties

		#region Public

		#region Parent
		/// <summary>
		/// The <see cref="IRecordFilter"/> object that ultimately is the parent of this object.
		/// </summary>
		public IRecordFilter Parent
		{
			get
			{
				return this._parent;
			}
			protected internal set
			{
				this._parent = value;
			}
		}

		#endregion // Parent

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
		protected virtual Expression GetCurrentExpression(FilterContext context)
		{
			return null;
		}

		/// <summary>
		/// Generates the current expression for this <see cref="ComparisonConditionBase"/>.
		/// </summary>
		/// <returns></returns>
		protected virtual Expression GetCurrentExpression()
		{
			return null;
		}

		#endregion // GetCurrentExpression

		#endregion // Protected

		#endregion // Methods

		#region IFilterCondition Members

		IRecordFilter IFilterCondition.Parent
		{
			get
			{
				return this.Parent;
			}
			set
			{
				this.Parent = value;
			}
		}

		Expression IExpressConditions.GetCurrentExpression(FilterContext context)
		{
			return this.GetCurrentExpression(context);
		}

		Expression IExpressConditions.GetCurrentExpression()
		{
			return this.GetCurrentExpression();
		}

		#endregion

		#region INotifyPropertyChanged Members
		/// <summary>
		/// Event raised when a property on this object changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="name"></param>
		protected virtual void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion
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