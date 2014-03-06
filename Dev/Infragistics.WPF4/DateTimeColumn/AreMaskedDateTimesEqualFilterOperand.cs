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
using System.Linq.Expressions;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{

    /// <summary>
    /// A <see cref="FilterOperand"/> that will apply a filter based on the selected Mask of the DateTime Column.
    /// </summary>
    public class AreMaskedDateTimesEqualFilterOperand : FilterOperand
    {
        #region Properties

        #region DefaultDisplayName

        /// <summary>
        /// Gets the string that will be displayed, when the DisplayName is not set. 
        /// </summary>
        protected override string DefaultDisplayName
        {
            get
            {
                return SRColumn.GetString("AreMaskedDateTimesEqualFilterOperandString");
            }
        }

        #endregion // DefaultDisplayName

        #region DateTimeColumn

        /// <summary>
        /// The DateTimeColumn
        /// </summary>
        protected internal DateTimeColumn DateTimeColumn
        {
            get;
            set;
        }

        #endregion

        #endregion // Properties

        #region Overrides

        #region FilteringExpression

        /// <summary>
        /// Returns a Linq Expression which will be used for filtering.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression FilteringExpression(object value)
        {
            if ((value == null) || value.GetType() != typeof(DateTime))
                return null;

            return CreateAreMaskedDateTimesEqual(this.DateTimeColumn.Key, value);
        }

        #endregion

        #endregion

        #region Private Methods

        #region CreateAreMaskedDateTimesEqual

        /// <summary>
        /// Generates the expression to compare two date time objects with the specified mask.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private System.Linq.Expressions.Expression CreateAreMaskedDateTimesEqual(string fieldName, object value)
        {
            System.Linq.Expressions.Expression dateTimeExpression = null;
            if (this.DateTimeColumn.DataType == typeof(DateTime))
            {
                Expression<Func<DateTime, bool>> datetimeStringExpression = (x) => AreDateObjectsSameWithMask(x, (DateTime)value);
                dateTimeExpression = datetimeStringExpression;
            }
            else
            {
                Expression<Func<DateTime?, bool>> datetimeStringExpression = (x) => x != null && AreDateObjectsSameWithMask(x.Value, (DateTime)value);
                dateTimeExpression = datetimeStringExpression;
            }
            ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(this.DateTimeColumn.ColumnLayout.ObjectDataType, "parameter");
            System.Linq.Expressions.Expression left = DataManagerBase.BuildPropertyExpressionFromPropertyName(fieldName, parameterExpression, this.DateTimeColumn.ColumnLayout.ObjectDataTypeInfo, this.DateTimeColumn.DataType, GetDefaultValue());
            InvocationExpression invokeExpression = System.Linq.Expressions.Expression.Invoke(dateTimeExpression, left);
            System.Linq.Expressions.Expression equalExpression = System.Linq.Expressions.Expression.Equal(parameterExpression, System.Linq.Expressions.Expression.Constant(null, this.DateTimeColumn.ColumnLayout.ObjectDataType));
            ConditionalExpression cExpr = System.Linq.Expressions.Expression.Condition(equalExpression, System.Linq.Expressions.Expression.Constant(false), invokeExpression);
            return System.Linq.Expressions.Expression.Lambda(cExpr, parameterExpression);
        }

        #endregion

        #region GetDefaultValue

        /// <summary>
        /// Gets the Default Value of a DateTime Instance
        /// </summary>
        /// <returns></returns>
        private object GetDefaultValue()
        {
            if (!this.DateTimeColumn.DataType.IsValueType)
            {
                return null;
            }
            else
            {
                return Activator.CreateInstance(this.DateTimeColumn.DataType);
            }
        }

        #endregion

        #region AreDateObjectsSameWithMask

        /// <summary>
        /// Analyzes the mask and compares two date time objects.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private bool AreDateObjectsSameWithMask(DateTime original, DateTime filter)
        {
            if (filter == null)
                return true;

            switch (DateTimeColumn.SelectedDateMask.ToLower())
            {
                case "{date}":
                    return (original.ToShortDateString() == filter.ToShortDateString());
                case "{time}":
                    return (original.ToShortTimeString() == filter.ToShortTimeString());
                case "{longtime}":
                    return (original.ToLongTimeString() == filter.ToLongTimeString());
                default:
                    return (original == filter);
            }
        }

        #endregion

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