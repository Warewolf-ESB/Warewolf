using System;
using System.Text.RegularExpressions;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="CustomComparisonCondition"/> which will do a regular expression based contains search
    /// on various parts of a date field for a given text string.
    /// </summary>
    /// <remarks>This class is designed for internal use only.</remarks>
    public class DateFilterObjectStringFilter : CustomComparisonCondition
    {
        #region Properties
        /// <summary>
        /// The <see cref="DateFilterObjectType"/> which will limit the search.
        /// </summary>
        public DateFilterObjectType DateFilterObjectType
        {
            get;
            set;
        }
        #endregion // Properties

        #region Overrides
        /// <summary>
        /// Generates the current expression for this <see cref="ComparisonConditionBase"/>.
        /// </summary>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression GetCurrentExpression()
        {
            return base.GetCurrentExpression();
        }

        /// <summary>
        /// Generates the current expression for this <see cref="ComparisonConditionBase"/> using the inputted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression GetCurrentExpression(FilterContext context)
        {
            System.Linq.Expressions.Expression<Func<XamGridFilterDate, bool>> expr = null;

            string filterCellValue = (string)this.FilterValue;

            filterCellValue = Regex.Escape(filterCellValue);

            Regex regex = new Regex(filterCellValue, RegexOptions.IgnoreCase);

            switch (this.DateFilterObjectType)
            {
                case (DateFilterObjectType.All):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                                        regex.IsMatch(a.Date.Year.ToString()) ||
                                        regex.IsMatch(a.ContentStringMonth) ||
                                        regex.IsMatch(a.Date.Day.ToString()) ||
                                        regex.IsMatch(a.Date.Hour.ToString()) ||
                                        regex.IsMatch(a.Date.Minute.ToString()) ||
                                        regex.IsMatch(a.Date.Second.ToString())));
                    break;
                case (DateFilterObjectType.Year):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                        regex.IsMatch(a.Date.Year.ToString())));
                    break;
                case (DateFilterObjectType.Month):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                        regex.IsMatch(a.ContentStringMonth)));
                    break;
                case (DateFilterObjectType.Date):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                        regex.IsMatch(a.Date.Day.ToString())));
                    break;
                case (DateFilterObjectType.Hour):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                        regex.IsMatch(a.Date.Hour.ToString())));
                    break;
                case (DateFilterObjectType.Minute):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                        regex.IsMatch(a.Date.Minute.ToString())));
                    break;
                case (DateFilterObjectType.Second):
                    expr = a => (a.NullDate && regex.IsMatch(a.ContentString)) ||
                        (!a.NullDate && (
                        regex.IsMatch(a.Date.Second.ToString())));
                    break;
            }
            return expr;
        }
        #endregion // Overrides
    }

    /// <summary>
    /// A <see cref="CustomComparisonCondition"/> which will do a regular expression based contains search over
    /// a given field.
    /// </summary>
    public class FilterValueProxyStringFilter : CustomComparisonCondition
    {
        #region Overrides

        /// <summary>
        /// Generates the current expression for this <see cref="ComparisonConditionBase"/> using the inputted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override System.Linq.Expressions.Expression GetCurrentExpression(FilterContext context)
        {
            System.Linq.Expressions.Expression<Func<FilterValueProxy, bool>> expr = null;

            string filterCellValue = (string)this.FilterValue;

            filterCellValue = Regex.Escape(filterCellValue);

            Regex regex = new Regex(filterCellValue, RegexOptions.IgnoreCase);

            expr = a => a.ContentString != null && regex.IsMatch(a.ContentString);

            return expr;
        }
        #endregion // Overrides
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