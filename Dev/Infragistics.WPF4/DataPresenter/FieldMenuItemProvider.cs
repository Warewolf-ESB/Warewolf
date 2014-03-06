using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Collections;
using Infragistics.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace Infragistics.Windows.DataPresenter
{
	internal class FieldMenuItemProvider
	{
		#region Member Variables

		private ResolvedRecordFilterCollection.FieldFilterInfo _fieldFilterInfo;
		private FieldMenuDataItem _rootMenuItem;
		private Type _editAsTypeResolved;
		private ComparisonOperatorFlags _compatibleOperators;
		private ComparisonOperator _defaultUiOperator;

		#endregion //Member Variables

		#region Constructor
		internal FieldMenuItemProvider(FieldMenuDataItem rootMenuItem, ResolvedRecordFilterCollection.FieldFilterInfo fieldFilterInfo)
		{
			_rootMenuItem = rootMenuItem;
			_fieldFilterInfo = fieldFilterInfo;
			_editAsTypeResolved = _fieldFilterInfo.Field.EditAsTypeResolved;
			_compatibleOperators = ComparisonCondition.GetCompatibleComparisonOperators(_editAsTypeResolved, out _defaultUiOperator);
		} 
		#endregion //Constructor

		#region Properties

		#region Internal Properties

		#region FilterCommandParameter
		internal object FilterCommandParameter
		{
			get { return _fieldFilterInfo; }
		}
		#endregion //FilterCommandParameter 

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddClearFilterMenuItem
		internal FieldMenuDataItem AddClearFilterMenuItem(FieldMenuDataItem parent)
		{
			if (_fieldFilterInfo == null || _fieldFilterInfo.Field == null)
				return null;

			Field field = _fieldFilterInfo.Field;
			var command = new ConditionFilterCommand { ShowCustomFilterDialog = false };
			string header = DataPresenterBase.GetString("FilterMenuCaption_ClearFieldFilter", field.Label);
			var newItem = AddMenuItem(parent, header, command, this.FilterCommandParameter);

			Uri uri = Utilities.BuildEmbeddedResourceUri(this.GetType().Assembly, "/Images/FilterClearMenu.png");
			var image = new System.Windows.Media.Imaging.BitmapImage(uri);
			newItem.ImageSource = image;

			return newItem;
		}
		#endregion //AddClearFilterMenuItem

		#region AddSeparator
		internal static FieldMenuDataItem AddSeparator(FieldMenuDataItem parent)
		{
			int parentItemCount = parent.Items.Count;

			if (parentItemCount < 1 || parent.Items[parentItemCount - 1].IsSeparator)
				return null;

			return AddMenuItem(parent.Items, new FieldMenuDataItem { IsSeparator = true });
		}
		#endregion //AddSeparator

		#region CreateDataTypeFilterMenuItem
		internal FieldMenuDataItem CreateDataTypeFilterMenuItem(Type fieldType)
		{
			Type underlyingType = CoreUtilities.GetUnderlyingType(fieldType);
			FieldMenuDataItem menuItem = null;

			if (underlyingType == typeof(DateTime))
				menuItem = this.CreateDateFilterMenuItem();
			else if (Utilities.IsNumericType(underlyingType) || underlyingType == typeof(Boolean))
				menuItem = this.CreateNumericFilterMenuItem();
			else
				menuItem = this.CreateTextFilterMenuItem();

			// just in case remove any trailing separator
			if (null != menuItem)
			{
				int childCount = menuItem.Items.Count;

				if (childCount > 0 && menuItem.Items[childCount - 1].IsSeparator)
					menuItem.Items.RemoveAt(childCount - 1);
			}

			return menuItem;
		}
		#endregion //CreateDataTypeFilterMenuItem

		#endregion //Internal Methods

		#region Private Methods

		#region AddCustomFilterMenuItem
		private FieldMenuDataItem AddCustomFilterMenuItem(FieldMenuDataItem parent)
		{
			var mi = AddMenuItem(parent, DataPresenterBase.GetString("FilterMenuCaption_CustomFilter"), new ConditionFilterCommand(), this.FilterCommandParameter);
			mi.IsCheckable = true;
			return mi;
		}
		#endregion //AddCustomFilterMenuItem

		#region AddFilterMenuItem
		private FieldMenuDataItem AddFilterMenuItem(FieldMenuDataItem parent, string headerResourceName, ComparisonOperator comparisonOperator)
		{
			if (!IsCompatibleOperator(comparisonOperator))
				return null;

			return this.AddFilterMenuItem(parent, headerResourceName, new ComparisonCondition { Operator = comparisonOperator });
		}

		private FieldMenuDataItem AddFilterMenuItem(FieldMenuDataItem parent, string headerResourceName, params ICondition[] conditions)
		{
			var command = new ConditionFilterCommand();

			if (conditions != null)
			{
				if (conditions.Length > 1)
				{
					ConditionGroup conditionGroup = new ConditionGroup();

					foreach (var condition in conditions)
						conditionGroup.Add(condition);

					command.Condition = conditionGroup;
				}
				else
				{
					command.Condition = conditions[0];
				}
			}

			var mi = AddMenuItem(parent, DataPresenterBase.GetString(headerResourceName), command, this.FilterCommandParameter);
			mi.IsCheckable = true;
			return mi;
		}
		#endregion //AddFilterMenuItem

		#region AddFilterOperandMenuItem
		private FieldMenuDataItem AddFilterOperandMenuItem(FieldMenuDataItem parent, string specialOperandName)
		{
			var operand = SpecialFilterOperands.GetRegisteredOperand(specialOperandName);

			Debug.Assert(operand != null, "No special operand registered for the name:" + specialOperandName);

			if (operand == null)
				return null;

			// only add it if the operand supports the data type
			if (!operand.SupportsOperator(SpecialFilterOperandCommand.DefaultOperator))
				return null;

			if (!operand.SupportsDataType(_editAsTypeResolved))
				return null;

			string resourceName = string.Format("FilterMenuCaption_{0}_Operand", specialOperandName);
			var mi = AddMenuItem(parent, DataPresenterBase.GetString(resourceName), new SpecialFilterOperandCommand { OperandName = specialOperandName }, this.FilterCommandParameter);
			mi.IsCheckable = true;
			return mi;
		}
		#endregion //AddFilterOperandMenuItem

		#region AddMenuItem
		private static FieldMenuDataItem AddMenuItem(FieldMenuDataItem parent, object header, ICommand command, object commandParameter)
		{
			return AddMenuItem(parent.Items, new FieldMenuDataItem { Header = header, Command = command, CommandParameter = commandParameter });
		}

		private static FieldMenuDataItem AddMenuItem(IList<FieldMenuDataItem> parentItems, FieldMenuDataItem child)
		{
			parentItems.Add(child);
			return child;
		}
		#endregion //AddMenuItem

		#region CreateDateFilterMenuItem
		private FieldMenuDataItem CreateDateFilterMenuItem()
		{
			FieldMenuDataItem root = new FieldMenuDataItem { Header = DataPresenterBase.GetString("FilterMenuCaption_DateFilters") };

			this.AddFilterMenuItem(root, "FilterMenuCaption_Equals", ComparisonOperator.Equals);
			AddSeparator(root);

			this.AddFilterMenuItem(root, "FilterMenuCaption_Before", ComparisonOperator.LessThan);
			this.AddFilterMenuItem(root, "FilterMenuCaption_After", ComparisonOperator.GreaterThan);

			if (this.IsCompatibleOperator(ComparisonOperator.GreaterThanOrEqualTo) && this.IsCompatibleOperator(ComparisonOperator.LessThanOrEqualTo))
				this.AddFilterMenuItem(root, "FilterMenuCaption_Between", new ComparisonCondition { Operator = ComparisonOperator.GreaterThanOrEqualTo }, new ComparisonCondition { Operator = ComparisonOperator.LessThanOrEqualTo });

			AddSeparator(root);

			this.AddFilterOperandMenuItem(root, "Tomorrow");
			this.AddFilterOperandMenuItem(root, "Today");
			this.AddFilterOperandMenuItem(root, "Yesterday");
			AddSeparator(root);

			this.AddFilterOperandMenuItem(root, "NextWeek");
			this.AddFilterOperandMenuItem(root, "ThisWeek");
			this.AddFilterOperandMenuItem(root, "LastWeek");
			AddSeparator(root);

			this.AddFilterOperandMenuItem(root, "NextMonth");
			this.AddFilterOperandMenuItem(root, "ThisMonth");
			this.AddFilterOperandMenuItem(root, "LastMonth");
			AddSeparator(root);

			this.AddFilterOperandMenuItem(root, "NextQuarter");
			this.AddFilterOperandMenuItem(root, "ThisQuarter");
			this.AddFilterOperandMenuItem(root, "LastQuarter");
			AddSeparator(root);

			this.AddFilterOperandMenuItem(root, "NextYear");
			this.AddFilterOperandMenuItem(root, "ThisYear");
			this.AddFilterOperandMenuItem(root, "LastYear");
			AddSeparator(root);

			this.AddFilterOperandMenuItem(root, "YearToDate");
			AddSeparator(root);

			var subMenu = new FieldMenuDataItem { Header = DataPresenterBase.GetString("FilterMenuCaption_AllDates") };

			#region Year Agnostic Sub Menu Items

			this.AddFilterOperandMenuItem(subMenu, "Quarter1");
			this.AddFilterOperandMenuItem(subMenu, "Quarter2");
			this.AddFilterOperandMenuItem(subMenu, "Quarter3");
			this.AddFilterOperandMenuItem(subMenu, "Quarter4");
			AddSeparator(subMenu);

			this.AddFilterOperandMenuItem(subMenu, "January");
			this.AddFilterOperandMenuItem(subMenu, "February");
			this.AddFilterOperandMenuItem(subMenu, "March");
			this.AddFilterOperandMenuItem(subMenu, "April");
			this.AddFilterOperandMenuItem(subMenu, "May");
			this.AddFilterOperandMenuItem(subMenu, "June");
			this.AddFilterOperandMenuItem(subMenu, "July");
			this.AddFilterOperandMenuItem(subMenu, "August");
			this.AddFilterOperandMenuItem(subMenu, "September");
			this.AddFilterOperandMenuItem(subMenu, "October");
			this.AddFilterOperandMenuItem(subMenu, "November");
			this.AddFilterOperandMenuItem(subMenu, "December");

			#endregion //Year Agnostic Sub Menu Items

			if (subMenu.Items.Count > 0)
			{
				AddMenuItem(root.Items, subMenu);
				AddSeparator(root);
			}

			this.AddCustomFilterMenuItem(root);
			return root;
		}
		#endregion //CreateDateFilterMenuItem

		#region CreateNumericFilterMenuItem
		private FieldMenuDataItem CreateNumericFilterMenuItem()
		{
			FieldMenuDataItem root = new FieldMenuDataItem { Header = DataPresenterBase.GetString("FilterMenuCaption_NumberFilters") };

			this.AddFilterMenuItem(root, "FilterMenuCaption_Equals", ComparisonOperator.Equals);
			this.AddFilterMenuItem(root, "FilterMenuCaption_NotEquals", ComparisonOperator.NotEquals);
			AddSeparator(root);

			this.AddFilterMenuItem(root, "FilterMenuCaption_GreaterThan", ComparisonOperator.GreaterThan);
			this.AddFilterMenuItem(root, "FilterMenuCaption_GreaterThanOrEqual", ComparisonOperator.GreaterThanOrEqualTo);
			this.AddFilterMenuItem(root, "FilterMenuCaption_LessThan", ComparisonOperator.LessThan);
			this.AddFilterMenuItem(root, "FilterMenuCaption_LessThanOrEqual", ComparisonOperator.LessThanOrEqualTo);

			if (this.IsCompatibleOperator(ComparisonOperator.GreaterThanOrEqualTo) && this.IsCompatibleOperator(ComparisonOperator.LessThanOrEqualTo))
				this.AddFilterMenuItem(root, "FilterMenuCaption_Between", new ComparisonCondition { Operator = ComparisonOperator.GreaterThanOrEqualTo }, new ComparisonCondition { Operator = ComparisonOperator.LessThanOrEqualTo });

			AddSeparator(root);

			if (this.IsCompatibleOperator(ComparisonOperator.Top))
				this.AddFilterMenuItem(root, "FilterMenuCaption_Top10", new ComparisonCondition(ComparisonOperator.Top, 10));

			this.AddFilterOperandMenuItem(root, "AboveAverage");
			this.AddFilterOperandMenuItem(root, "BelowAverage");
			AddSeparator(root);

			this.AddCustomFilterMenuItem(root);

			return root;
		}
		#endregion //CreateNumericFilterMenuItem

		#region CreateTextFilterMenuItem
		private FieldMenuDataItem CreateTextFilterMenuItem()
		{
			FieldMenuDataItem root = new FieldMenuDataItem { Header = DataPresenterBase.GetString("FilterMenuCaption_TextFilters") };

			this.AddFilterMenuItem(root, "FilterMenuCaption_Equals", ComparisonOperator.Equals);
			this.AddFilterMenuItem(root, "FilterMenuCaption_NotEquals", ComparisonOperator.NotEquals);
			AddSeparator(root);

			this.AddFilterMenuItem(root, "FilterMenuCaption_BeginsWith", ComparisonOperator.StartsWith);
			this.AddFilterMenuItem(root, "FilterMenuCaption_EndsWith", ComparisonOperator.EndsWith);
			AddSeparator(root);

			this.AddFilterMenuItem(root, "FilterMenuCaption_Contains", ComparisonOperator.Contains);
			this.AddFilterMenuItem(root, "FilterMenuCaption_NotContains", ComparisonOperator.DoesNotContain);
			AddSeparator(root);

			this.AddCustomFilterMenuItem(root);

			return root;
		}
		#endregion //CreateTextFilterMenuItem

		#region IsCompatibleOperator
		private bool IsCompatibleOperator(ComparisonOperator comparisonOperator)
		{
			ComparisonOperatorFlags operatorFlag = ComparisonCondition.GetComparisonOperatorFlag(comparisonOperator);
			return operatorFlag == (operatorFlag & _compatibleOperators);
		}
		#endregion //IsCompatibleOperator

		#endregion //Private Methods 

		#endregion //Methods
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