using System.ComponentModel;
using System.Windows;

namespace Infragistics.Controls.Grids
{
	#region IConditionalFormattingRule

	/// <summary>
	/// An interface that is used by the conditional formatting framework to identify a conditional rule.
	/// </summary>
	public interface IConditionalFormattingRule : INotifyPropertyChanged
	{
		#region IsTerminalRule

		/// <summary>
		/// Gets if this rule should terminate evaluation
		/// of any later rules in the evaluation tree
		/// that might apply to the cell.
		/// </summary>	
		bool IsTerminalRule { get; }

		#endregion // IsTerminalRule

		#region Column

		/// <summary>
		/// Gets the <see cref="Column"/> that the rule will be evaluated against.
		/// </summary>
		Column Column { get; set; }

		#endregion // Column

		#region StyleScope

		/// <summary>
		/// Gets which style will be affected by the rule.
		/// </summary>
		StyleScope StyleScope
		{
			get;
		}

		#endregion // StyleScope

		#region ShouldRefreshOnDataChange

		/// <summary>
		/// Gets whether the rule needs to invalidate itself if a <see cref="Cell"/> in the <see cref="Column"/> changes value.
		/// </summary>
		bool ShouldRefreshOnDataChange
		{
			get;
		}

		#endregion // ShouldRefreshOnDataChange

		#region CellValueVisibility

		/// <summary>
		/// Gets whether the text of the cell should be shown during conditional formatting.
		/// </summary>
		/// <remarks>This is only applied when the <see cref="StyleScope"/> is <see cref="StyleScope"/>.Cell</remarks>
		Visibility CellValueVisibility
		{
			get;
		}

		#endregion // CellValueVisibility

		#region RuleExecution

		/// <summary>
		/// Gets which stage of the data limiting the <see cref="IConditionalFormattingRule"/> will gather it's data.
		/// </summary>
		EvaluationStage RuleExecution { get; }

		#endregion // RuleExecution

		#region CreateProxy

		/// <summary>
		/// Creates a new instance of the class which will execute the logic to populate and execute the conditional formatting logic for this <see cref="IConditionalFormattingRule"/>.
		/// </summary>
		/// <returns></returns>
		IConditionalFormattingRuleProxy CreateProxy();

		#endregion // CreateProxy
	}

	#endregion // IConditionalFormattingRule

	#region IConditionalFormattingRuleProxy

	/// <summary>
	/// An interface that is used to executed conditional formatting logic.
	/// </summary>
	public interface IConditionalFormattingRuleProxy : IRule
	{
		#region Parent

		/// <summary>
		/// Gets / sets the <see cref="IConditionalFormattingRule"/> which this proxy is associated with.
		/// </summary>
		IConditionalFormattingRule Parent { get; set; }

		#endregion // Parent

		#region EvaluateCondition

		/// <summary>
		/// Evaluates the rule against the inputted cell and returns true if the rule was applied
		/// </summary>
		/// <param name="sourceDataObject"></param>
		/// <param name="sourceDataValue"></param>
		/// <returns></returns>
		Style EvaluateCondition(object sourceDataObject, object sourceDataValue);

		#endregion // EvaluateCondition
	}

	#endregion // IConditionalFormattingRuleProxy
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