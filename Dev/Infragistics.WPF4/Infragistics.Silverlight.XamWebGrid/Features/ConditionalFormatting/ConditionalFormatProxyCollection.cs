using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of <see cref="IConditionalFormattingRuleProxy"/> objects.
	/// </summary>
	public class ConditionalFormatProxyCollection : FormattingRuleCollection<IConditionalFormattingRuleProxy>
	{
		Dictionary<string, ReadOnlyCollection<IConditionalFormattingRuleProxy>> _proxys = new Dictionary<string, ReadOnlyCollection<IConditionalFormattingRuleProxy>>();

		#region GenerateIRuleCollection

		/// <summary>
		/// Creates a collection of <see cref="IRule"/> objects from the items in this collection.
		/// </summary>
		/// <returns></returns>
		public FormattingRuleCollection<IRule> GenerateIRuleCollection()
		{
			FormattingRuleCollection<IRule> rules = new FormattingRuleCollection<IRule>();
			foreach (IConditionalFormattingRuleProxy rule in this.Items)
			{
				rules.Add(rule);
			}
			return rules;
		}

		#endregion // GenerateIRuleCollection

		#region GetCellScopedConditionsForKey

		/// <summary>
		/// Creates a collection of <see cref="IConditionalFormattingRuleProxy"/> objects for a <see cref="Column"/> with the given key value.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ReadOnlyCollection<IConditionalFormattingRuleProxy> GetCellScopedConditionsForKey(string key)
		{
			if (!this._proxys.ContainsKey(key))
			{
				this._proxys.Add(key, new ReadOnlyCollection<IConditionalFormattingRuleProxy>(new List<IConditionalFormattingRuleProxy>() ));
			}

			return this._proxys[key];
		}

		#endregion // GetCellScopedConditionsForKey

		#region GetRowScopedConditions

		/// <summary>
		/// Creates a collection of <see cref="IConditionalFormattingRuleProxy"/> which are set to <see cref="StyleScope"/>.Row.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyCollection<IConditionalFormattingRuleProxy> GetRowScopedConditions()
		{
			List<IConditionalFormattingRuleProxy> rules = new List<IConditionalFormattingRuleProxy>();
			foreach (IConditionalFormattingRuleProxy rule in this.Items)
			{
				if (rule.Parent.StyleScope == StyleScope.Row)
					rules.Add(rule);
			}
			return new ReadOnlyCollection<IConditionalFormattingRuleProxy>(rules);
		}

		#endregion // GetRowScopedConditions

		#region OnItemAdded
		/// <summary>
		/// Invoked when a <see cref="IConditionalFormattingRuleProxy"/> is added at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemAdded(int index, IConditionalFormattingRuleProxy item)
		{
			base.OnItemAdded(index, item);

            if (item.Parent.Column != null)
            {
                string key = item.Parent.Column.Key;

                if (!this._proxys.ContainsKey(key))
                {
                    this._proxys.Add(key, null);
                }

                List<IConditionalFormattingRuleProxy> rules = new List<IConditionalFormattingRuleProxy>();

                foreach (IConditionalFormattingRuleProxy rule in this.Items)
                {
                    if (rule.Parent.Column.Key == key && rule.Parent.StyleScope == StyleScope.Cell)
                        rules.Add(rule);
                }

                this._proxys[key] = new ReadOnlyCollection<IConditionalFormattingRuleProxy>(rules);
            }
		}
		#endregion // OnItemAdded

		#region OnItemRemoved

		/// <summary>
		/// Invoked when a <see cref="IConditionalFormattingRuleProxy"/> is removed at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemRemoved(int index, IConditionalFormattingRuleProxy item)
		{
			base.OnItemRemoved(index, item);

			string key = item.Parent.Column.Key;

			if (this._proxys.ContainsKey(key))
			{
				this._proxys[key] = null;
			}
		}

		#endregion // OnItemRemoved

		#region Clear

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public override void Clear()
		{
			base.Clear();
			
			this._proxys = new Dictionary<string, ReadOnlyCollection<IConditionalFormattingRuleProxy>>(); 
		}

		#endregion // Clear
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