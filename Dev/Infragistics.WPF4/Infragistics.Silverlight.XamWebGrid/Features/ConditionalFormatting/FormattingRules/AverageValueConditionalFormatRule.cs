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
using System.Linq;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A conditional formatting rule that evaluates if the value equals the average value.
	/// </summary>
	/// <remarks>
	/// This format rule is intended to be used by numeric data types.
	/// </remarks>
	public class AverageValueConditionalFormatRule : DiscreetRuleBase
	{
		#region Properties

		#region FormattingTarget

		/// <summary>
		/// Identifies the <see cref="FormattingTarget"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FormattingTargetProperty = DependencyProperty.Register("FormattingTarget", typeof(AverageFormattingTarget), typeof(AverageValueConditionalFormatRule), new PropertyMetadata(new PropertyChangedCallback(FormattingTargetChanged)));

		/// <summary>
		/// Gets / sets what cells should be styled by this rule.
		/// </summary>
		public AverageFormattingTarget FormattingTarget
		{
			get { return (AverageFormattingTarget)this.GetValue(FormattingTargetProperty); }
			set { this.SetValue(FormattingTargetProperty, value); }
		}

		private static void FormattingTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			AverageValueConditionalFormatRule rule = (AverageValueConditionalFormatRule)obj;
			rule.OnPropertyChanged("FormattingTarget");
		}

		#endregion // FormattingTarget

		#endregion // Properties

		#region Overrides

		/// <summary>
		/// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override IConditionalFormattingRuleProxy CreateProxy()
		{
			return new AverageValueConditionalFormatRuleProxy();
		}

		#endregion // Overrides
	}

	/// <summary>
	/// The execution proxy for the <see cref="AverageValueConditionalFormatRule"/>.
	/// </summary>
	public class AverageValueConditionalFormatRuleProxy : DiscreetRuleBaseProxy
	{
		#region Members

		IComparable _value;

		#endregion // Members

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
			IComparable compareValue = sourceDataValue as IComparable;

			if (compareValue == null)
				return null;

			AverageValueConditionalFormatRule rule = (AverageValueConditionalFormatRule)this.Parent;

			if (rule.FormattingTarget == AverageFormattingTarget.Above)
			{
				if (compareValue.CompareTo(this._value) > 0)
				{
					return rule.StyleToApply;
				}
			}
			else if (rule.FormattingTarget == AverageFormattingTarget.EqualOrAbove)
			{
				if (compareValue.CompareTo(this._value) >= 0)
				{
					return rule.StyleToApply;
				}
			}
			else if (rule.FormattingTarget == AverageFormattingTarget.Below)
			{
				if (compareValue.CompareTo(this._value) < 0)
				{
					return rule.StyleToApply;
				}
			}
			else if (rule.FormattingTarget == AverageFormattingTarget.EqualOrBelow)
			{
				if (compareValue.CompareTo(this._value) <= 0)
				{
					return rule.StyleToApply;
				}
			}
			return null;
		}

		#endregion // EvaluateCondition

		#region GatherData
		/// <summary>
		/// Called at the stage provided by <see cref="IRule.RuleExecution"/> so that values can be derived for formatting purposes.
		/// </summary>
		/// <param name="query"></param>
		protected override void GatherData(IQueryable query)
		{
			SummaryContext sc = SummaryContext.CreateGenericSummary(this.Parent.Column.ColumnLayout.ObjectDataTypeInfo, this.Parent.Column.Key, LinqSummaryOperator.Average);

			this._value = this.Parent.Column.ResolveValue(sc.Execute(query)) as IComparable;
		}
		#endregion // GatherData

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