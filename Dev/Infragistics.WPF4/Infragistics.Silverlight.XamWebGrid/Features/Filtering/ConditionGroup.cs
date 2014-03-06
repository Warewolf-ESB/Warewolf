using System.ComponentModel;
using System.Linq.Expressions;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A derived <see cref="CustomComparisonCondition"/> which groups other <see cref="IFilterCondition"/> objects together under a single logical operator.
    /// </summary>
    public class ConditionGroup : CustomComparisonCondition
    {
        #region Members

        ConditionCollection _conditions;

        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionGroup"/> class.
        /// </summary>
        public ConditionGroup(IRecordFilter parent)
        {
            this.Conditions.Parent = parent;
        }
        #endregion // Constructor

        #region Conditions
        /// <summary>
        /// Gets the collection of filter conditions which will be applied to this column
        /// </summary>
        [Browsable(false)]
        public ConditionCollection Conditions
        {
            get
            {
                if (this._conditions == null)
                {
                    this._conditions = new ConditionCollection();
                    //this._conditions.CollectionItemChanged += new EventHandler<EventArgs>(Conditions_CollectionItemChanged);
                    //this._conditions.CollectionChanged += new NotifyCollectionChangedEventHandler(Conditions_CollectionChanged);
                    //this._conditions.PropertyChanged += new PropertyChangedEventHandler(Conditions_PropertyChanged);
                }

                //if (this._conditions.Parent != this)
                //    this._conditions.Parent = this;

                return this._conditions;
            }
        }
        #endregion // Conditions

        #region GetCurrentExpression
        /// <summary>
        /// Generates the current expression for this <see cref="ComparisonConditionBase"/> using the inputted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression GetCurrentExpression(FilterContext context)
        {
            return context.CreateExpression(this.Conditions);
        }

        /// <summary>
        /// Generates the current expression for this <see cref="ComparisonConditionBase"/>.
        /// </summary>
        /// <returns></returns>
        protected override Expression GetCurrentExpression()
        {
            return base.GetCurrentExpression();
        }
        #endregion // GetCurrentExpression
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