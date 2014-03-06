using System;
using System.Windows;
using System.ComponentModel;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A conditional formatting rule that evaluates if the value is between two values
    /// </summary>
    public class BetweenXandYConditionalFormatRule : DiscreetRuleBase
    {
        #region Overrides
        /// <summary>
        /// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override IConditionalFormattingRuleProxy CreateProxy()
        {
            return new BetweenXandYConditionalFormatRuleProxy();
        }
        #endregion // Overrides

        #region Properties

        #region LowerBound

        /// <summary>
        /// Identifies the <see cref="LowerBound"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty LowerBoundProperty = DependencyProperty.Register("LowerBound", typeof(IComparable), typeof(BetweenXandYConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(LowerBoundChanged)));

        /// <summary>
        /// Gets / sets the <see cref="IComparable"/> that will act as the lower limit for the range.
        /// </summary>
    [TypeConverter(typeof(IComparableTypeConverter))]
        public IComparable LowerBound
        {
            get { return (IComparable)this.GetValue(LowerBoundProperty); }
            set { this.SetValue(LowerBoundProperty, value); }
        }

        private static void LowerBoundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            BetweenXandYConditionalFormatRule rule = (BetweenXandYConditionalFormatRule)obj;
            rule.OnPropertyChanged("LowerBound");
        }

        #endregion // LowerBound

        #region LowerBoundResolved

        /// <summary>
        /// Gets the value of the <see cref="LowerBound"/> converted to it's <see cref="Column"/> datatype and then converted to an <see cref="IComparable"/>.
        /// </summary>
        protected internal IComparable LowerBoundResolved
        {
            get
            {
                IComparable value = this.LowerBound;

                if (this.Column != null)
                {
                    try
                    {
                        value = this.Column.ResolveValue(value) as IComparable;
                    }
                    catch
                    {
                    }
                }

                return value;
            }
        }
        #endregion // LowerBoundResolved

        #region UpperBound

        /// <summary>
        /// Identifies the <see cref="UpperBound"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty UpperBoundProperty = DependencyProperty.Register("UpperBound", typeof(IComparable), typeof(BetweenXandYConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(UpperBoundChanged)));

        /// <summary>
        /// Gets / sets the <see cref="IComparable"/> that will act as the upper limit for the range.
        /// </summary>
      [TypeConverter(typeof(IComparableTypeConverter))]
        public IComparable UpperBound
        {
            get { return (IComparable)this.GetValue(UpperBoundProperty); }
            set { this.SetValue(UpperBoundProperty, value); }
        }

        private static void UpperBoundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            BetweenXandYConditionalFormatRule rule = (BetweenXandYConditionalFormatRule)obj;
            rule.OnPropertyChanged("UpperBound");
        }

        #endregion // UpperBound

        #region UpperBoundResolved
        /// <summary>
        /// Gets the value of the <see cref="UpperBound"/> converted to it's <see cref="Column"/> datatype and then converted to an <see cref="IComparable"/>.
        /// </summary>
        protected internal IComparable UpperBoundResolved
        {
            get
            {
                IComparable value = this.UpperBound;

                if (this.Column != null)
                {
                    try
                    {
                        value = this.Column.ResolveValue(value) as IComparable;
                    }
                    catch
                    {
                    }
                }

                return value;
            }
        }
        #endregion // UpperBoundResolved

        #region IsInclusive

        /// <summary>
        /// Identifies the <see cref="IsInclusive"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsInclusiveProperty = DependencyProperty.Register("IsInclusive", typeof(bool), typeof(BetweenXandYConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(IsInclusiveChanged)));

        /// <summary>
        /// Gets / sets if the evaluation should include the <see cref="LowerBound"/> and <see cref="UpperBound"/> in the range.
        /// </summary>
        public bool IsInclusive
        {
            get { return (bool)this.GetValue(IsInclusiveProperty); }
            set { this.SetValue(IsInclusiveProperty, value); }
        }

        private static void IsInclusiveChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            BetweenXandYConditionalFormatRule rule = (BetweenXandYConditionalFormatRule)obj;
            rule.OnPropertyChanged("IsInclusive");
        }

        #endregion // IsInclusive

        #endregion // Properties
    }

    /// <summary>
    /// The execution proxy for the <see cref="BetweenXandYConditionalFormatRule"/>.
    /// </summary>
    public class BetweenXandYConditionalFormatRuleProxy : DiscreetRuleBaseProxy
    {
        #region Overrides

        #region EvaluateCondition

        /// <summary>
        /// Determines whether the inputted value meets the condition of <see cref="ConditionalFormattingRuleBase"/> and returns the style 
        /// that should be applied.
        /// </summary>
        /// <param name="sourceDataObject"></param>
        /// <param name="sourceDataValue"></param>
        /// <returns></returns>
        protected override Style EvaluateCondition(object sourceDataObject, object sourceDataValue)
        {
            IComparable obj1 = sourceDataValue as IComparable;
            
            BetweenXandYConditionalFormatRule ruleBase = this.Parent as BetweenXandYConditionalFormatRule;
            IComparable lowerBound = ruleBase.LowerBoundResolved;
            IComparable upperBound = ruleBase.UpperBoundResolved;

            if (lowerBound != null && upperBound != null && obj1 != null)
            {
                return this.EvaluateConditionHelper(lowerBound, upperBound, obj1, ruleBase.StyleToApply, ruleBase.IsInclusive);
            }

            return null;
        }

        #endregion // EvaluateCondition

        #endregion // Overrides

        #region EvaluateConditionHelper

        /// <summary>
        /// Evaluates the conditions to determine the style applied.
        /// </summary>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="obj1"></param>
        /// <param name="styleToApply"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        protected virtual Style EvaluateConditionHelper(IComparable lowerBound, IComparable upperBound, IComparable obj1, Style styleToApply, bool inclusive)
        {
            if (inclusive)
            {
                return (lowerBound.CompareTo(obj1) < 1 && upperBound.CompareTo(obj1) >= 0) ? styleToApply : null;
            }
            return (lowerBound.CompareTo(obj1) < 0 && upperBound.CompareTo(obj1) > 0) ? styleToApply : null;
        }

        #endregion // EvaluateConditionHelper
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