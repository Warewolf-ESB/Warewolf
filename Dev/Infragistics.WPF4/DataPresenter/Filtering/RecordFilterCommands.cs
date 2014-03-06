using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Infragistics.Collections;
using Infragistics.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;

// AS - NA 11.2 Excel Style Filtering
namespace Infragistics.Windows.DataPresenter
{
	
	/// <summary>
	/// Custom class that exposes the <see cref="Field"/> and <see cref="RecordManager"/> associated with a given <see cref="LabelPresenter"/>
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public class RecordFilterFieldInfo : PropertyChangeNotifier
	{
		#region Member Variables

		private Field _field;
		private RecordManager _recordManager; 

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RecordFilterFieldInfo"/>
		/// </summary>
		/// <param name="field">Field to be represented by the object</param>
		/// <param name="recordManager">RecordManager for the specified field</param>
		public RecordFilterFieldInfo(Field field, RecordManager recordManager)
		{
			_field = field;
			_recordManager = recordManager;
		} 
		#endregion //Constructor

		#region Properties

		#region Field
		/// <summary>
		/// Returns the associated Field
		/// </summary>
		public Field Field
		{
			get { return _field; }
			internal set
			{
				if (value != _field)
				{
					_field = value;
					this.OnPropertyChanged("Field");
				}
			}
		}
		#endregion //Field

		#region RecordManager
		/// <summary>
		/// Returns the associated RecordManager
		/// </summary>
		public RecordManager RecordManager
		{
			get { return _recordManager; }
			internal set
			{
				if (value != _recordManager)
				{
					_recordManager = value;
					this.OnPropertyChanged("RecordManager");
				}
			}
		}
		#endregion //RecordManager

		#endregion //Properties
	}


	/// <summary>
	/// Base class for a command that updates the <see cref="RecordFilter"/> of a given <see cref="Field"/> and <see cref="RecordManager"/>
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public abstract class RecordFilterCommandBase : PropertyChangeNotifier, ICommand
	{
		#region Member Variables

		private bool _showCustomFilterDialog;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RecordFilterCommandBase"/>
		/// </summary>
		protected RecordFilterCommandBase()
		{
		}
		#endregion //Constructor

		#region Properties

		#region ShowCustomFilterDialog
		/// <summary>
		/// Returns or sets a boolean indicating whether to show the custom dialog with the new condition.
		/// </summary>
		public bool ShowCustomFilterDialog
		{
			get { return _showCustomFilterDialog; }
			set { this.SetField(ref _showCustomFilterDialog, value, "ShowCustomFilterDialog"); }
		}
		#endregion //ShowCustomFilterDialog

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region AreSimilar
		private bool AreSimilar(ICondition condition1, ICondition condition2, bool compareOperands)
		{
			if (condition1 == condition2)
				return true;
			else if (condition1 == null || condition2 == null)
				return false;
			else if (condition1.GetType() != condition2.GetType())
				return false;

			if (condition1 is ConditionGroup)
			{
				var group1 = condition1 as ConditionGroup;
				var group2 = condition2 as ConditionGroup;

				if (group1.Count != group2.Count)
					return false;

				for (int i = 0, count = group1.Count; i < count; i++)
				{
					if (!AreSimilar(group1[i], group2[i], compareOperands))
						return false;
				}

				return true;
			}
			else if (condition1 is ComparisonCondition)
			{
				var compare1 = condition1 as ComparisonCondition;
				var compare2 = condition2 as ComparisonCondition;

				if (compare1.Operator != compare2.Operator)
					return false;

				if (compareOperands && !object.Equals(compare1.Value, compare2.Value))
					return false;

				return true;
			}
			else if (condition1 is ComplementCondition)
			{
				var complement1 = condition1 as ComplementCondition;
				var complement2 = condition2 as ComplementCondition;

				return AreSimilar(complement1.SourceCondition, complement2.SourceCondition, compareOperands);
			}
			else
			{
				Debug.Fail("Unrecognized condition type:" + condition1.GetType());
				return condition1.Equals(condition2);
			}
		}
		#endregion //AreSimilar

		#region CanExecuteImpl
		private bool CanExecuteImpl(object parameter, out Field field, out RecordManager rm)
		{
			var ffi = parameter as ResolvedRecordFilterCollection.FieldFilterInfo;

			if (ffi != null)
			{
				field = ffi.Field;
				rm = ffi.RecordManager;
			}
			else
			{
				var fi = parameter as RecordFilterFieldInfo;

				field = fi != null ? fi.Field : null;
				rm = fi != null ? fi.RecordManager : null;
			}

			return this.CanExecute(field, rm);
		}
		#endregion //CanExecuteImpl

		#region HasSameConditions
		private bool HasSameConditions(RecordFilter rfOriginal, ICondition condition, bool compareOperands)
		{
			ConditionGroup group = condition as ConditionGroup;

			if (group == null)
			{
				// create a temporary group to make comparisons easier
				group = new ConditionGroup();
				group.Add(condition);
			}

			return AreSimilar(rfOriginal.Conditions, group, compareOperands);
		}
		#endregion //HasSameConditions

		#region UpdateRecordFilter
		private void UpdateRecordFilter(RecordFilter rf, ICondition condition)
		{
			// if we're not applying this to the grid or it can't be because 
			// its not associated with a specific collection then just update 
			// the filter itself
			if (this.ShowCustomFilterDialog || rf.ParentCollection == null)
			{
				// always clear what we have
				rf.Conditions.Clear();

				ConditionGroup sourceGroup = condition as ConditionGroup;

				if (sourceGroup != null)
				{
					rf.Conditions.LogicalOperator = sourceGroup.LogicalOperator;
					foreach (var sourceItem in sourceGroup)
						rf.Conditions.Add(sourceItem);
				}
				else if (condition != null)
				{
					rf.Conditions.Add(condition);
				}
			}
			else
			{
				// otherwise we're updating the grid and so we want to 
				// raise any related events and save this to the undo 
				// history
				if (condition == null)
				{
					rf.Clear(true, true);
				}
				else
				{
					ComparisonCondition cc = condition as ComparisonCondition;
					ConditionGroup sourceGroup = condition as ConditionGroup;

					if (cc == null && sourceGroup != null && sourceGroup.Count == 1)
						cc = sourceGroup[0] as ComparisonCondition;

					if (cc != null)
					{
						rf.CurrentUIOperator = cc.Operator;
						rf.CurrentUIOperand = cc.Value;
					}
					else
					{
						rf.CurrentUIOperator = ComparisonOperator.Equals;
						rf.CurrentUIOperand = condition;
					}

					rf.ApplyPendingFilter(true, true);
				}
			}
		}
		#endregion //UpdateRecordFilter

		#endregion //Private Methods

		#region Protected methods

		#region CanExecute
		/// <summary>
		/// Used to determine if the commmand may be executed for the specified field and recordmanager.
		/// </summary>
		/// <param name="field">The field whose filter is to be changed</param>
		/// <param name="rm">The recordmanager whose filter is to be changed</param>
		/// <returns>Returns true if field and rm are non-null and the field is associated with a field layout.</returns>
		protected virtual bool CanExecute(Field field, RecordManager rm)
		{
			return field != null && rm != null && field.Owner != null;
		}
		#endregion //CanExecute

		#region Execute
		/// <summary>
		/// Used to invoke the command for the specified Field and RecordManager
		/// </summary>
		/// <param name="field">The field for which the command is being executed</param>
		/// <param name="recordManager">The recordmanager for which the command is being executed</param>
		protected virtual void Execute(Field field, RecordManager recordManager)
		{
			Debug.Assert(this.CanExecute(field, recordManager));

			ICondition condition = this.GetCondition(field, recordManager);
			FieldLayout fl = field.Owner;
			RecordFilterCollection rfc = recordManager.GetRecordFiltersResolved(fl);

			if (this.ShowCustomFilterDialog)
			{
				RecordFilter rfOriginal = rfc[field];
				RecordFilter rf;

				// if the condition is null we'll show the dialog for the current filter
				if (condition == null || HasSameConditions(rfOriginal, condition, false))
				{
					rf = rfOriginal;
				}
				else
				{
					// otherwise create a new one for the specified field
					rf = new RecordFilter(field);

					// and update it with the condition(s)
					UpdateRecordFilter(rf, condition);
				}

				fl.ShowCustomFilterSelectionControl(rf, recordManager);
			}
			else
			{
				RecordFilter rfSource = rfc[field];

				

				// copy over the condition information
				UpdateRecordFilter(rfSource, condition);
			}
		}
		#endregion //Execute

		#region GetCondition
		/// <summary>
		/// Used to obtain the condition that will be used to update the record filter for the field
		/// </summary>
		/// <param name="field">The field for which the command is being executed</param>
		/// <param name="recordManager">The recordmanager for which the command is being executed</param>
		protected abstract ICondition GetCondition(Field field, RecordManager recordManager);
		#endregion //GetCondition

		#region SetField
		/// <summary>
		/// Used to change the value of a field and raise the
		/// </summary>
		/// <typeparam name="T">The type of field being changed</typeparam>
		/// <param name="member">The field member passed by reference that will be updated/compared with the new value</param>
		/// <param name="newValue">The new value for the field</param>
		/// <param name="propertyName">The name of the property being changed</param>
		/// <returns>Returns false if the new value matches the existing member; otherwise true if the value is different</returns>
		protected bool SetField<T>(ref T member, T newValue, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(member, newValue))
				return false;

			Debug.Assert(this.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null, "Property name doesn't match current public property");
			member = newValue;
			this.OnPropertyChanged(propertyName);
			return true;
		}
		#endregion //SetField

		#endregion //Protected methods

		#region Internal Methods
		
		#region HasSameConditions
		internal bool HasSameConditions(RecordFilter filter, bool compareOperands, Field field, RecordManager rm)
		{
			ICondition condition = this.GetCondition(field, rm);
			return this.HasSameConditions(filter, condition, compareOperands);
		}
		#endregion //HasSameConditions

		#endregion //Internal Methods
		#endregion //Methods

		#region ICommand members
		bool ICommand.CanExecute(object parameter)
		{
			Field field;
			RecordManager rm;
			return CanExecuteImpl(parameter, out field, out rm);
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		void ICommand.Execute(object parameter)
		{
			Field field;
			RecordManager rm;

			if (!this.CanExecuteImpl(parameter, out field, out rm))
				return;

			this.Execute(field, rm);
		}
		#endregion //ICommand members
	}

	/// <summary>
	/// Custom record filter command that creates a <see cref="ComparisonCondition"/> for a given registered <see cref="SpecialFilterOperandBase"/>
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public class SpecialFilterOperandCommand : RecordFilterCommandBase
	{
		#region Member Variables

		private string _operandName;
		internal const ComparisonOperator DefaultOperator  = ComparisonOperator.Equals;
		private ComparisonOperator _operator = DefaultOperator;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="SpecialFilterOperandCommand"/>
		/// </summary>
		public SpecialFilterOperandCommand()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetCondition
		/// <summary>
		/// Used to obtain the condition that will be used to update the record filter for the field
		/// </summary>
		/// <param name="field">The field for which the command is being executed</param>
		/// <param name="recordManager">The recordmanager for which the command is being executed</param>
		protected override ICondition GetCondition(Field field, RecordManager recordManager)
		{
			var operand = SpecialFilterOperands.GetRegisteredOperand(this._operandName);
			return new ComparisonCondition(this.ComparisonOperator, operand);
		}
		#endregion //GetCondition

		#endregion //Base class overrides

		#region Properties

		#region ComparisonOperator
		/// <summary>
		/// Returns or sets the operator to be used when changing the condition.
		/// </summary>
		public ComparisonOperator ComparisonOperator
		{
			get { return _operator; }
			set { this.SetField(ref _operator, value, "ComparisonOperator"); }
		}
		#endregion //ComparisonOperator

		#region OperandName
		/// <summary>
		/// Returns or sets the name of the special filter operand.
		/// </summary>
		/// <seealso cref="SpecialFilterOperands"/>
		/// <seealso cref="SpecialFilterOperandBase"/>
		[TypeConverter(typeof(SpecialFilterOperandTypeConverter))]
		public string OperandName
		{
			get { return _operandName; }
			set { SetField(ref _operandName, value, "OperandName"); }
		}
		#endregion //OperandName

		#endregion //Properties
	}

	/// <summary>
	/// Custom record filter command that uses the specified <see cref="Condition"/> to update the record filter for a given <see cref="Field"/> and <see cref="RecordManager"/>
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public class ConditionFilterCommand : RecordFilterCommandBase
	{
		#region Member Variables

		private ICondition _condition;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initalizes a new <see cref="ConditionFilterCommand"/>
		/// </summary>
		public ConditionFilterCommand()
		{
			this.ShowCustomFilterDialog = true;
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanExecute
		/// <summary>
		/// Used to determine if the commmand may be executed for the specified field and recordmanager.
		/// </summary>
		/// <param name="field">The field whose filter is to be changed</param>
		/// <param name="rm">The recordmanager whose filter is to be changed</param>
		/// <returns>Returns true if field and rm are non-null and the field is associated with a field layout.</returns>
		protected override bool CanExecute(Field field, RecordManager rm)
		{
			if (!base.CanExecute(field, rm))
				return false;

			// having no condition and now showing the dialog is the 
			// same as clearing the filter. so we can only clear if there 
			// is a filter/conditions
			if (_condition == null && !this.ShowCustomFilterDialog)
			{
				RecordFilterCollection rfc = rm.GetRecordFiltersResolved(field.Owner);

				if (rfc == null)
					return false;

				var filter = rfc.GetItem(field, false);

				if (filter == null || !filter.HasConditions)
					return false;
			}

			return true;
		}
		#endregion //CanExecute

		#region GetCondition
		/// <summary>
		/// Used to obtain the condition that will be used to update the record filter for the field
		/// </summary>
		/// <param name="field">The field for which the command is being executed</param>
		/// <param name="recordManager">The recordmanager for which the command is being executed</param>
		protected override ICondition GetCondition(Field field, RecordManager recordManager)
		{
			if (_condition == null)
				return null;

			return _condition.Clone() as ICondition;
		}
		#endregion //GetCondition

		#endregion //Base class overrides

		#region Properties

		#region Condition
		/// <summary>
		/// Returns or sets the condition that will be used to update the record filter for the Field
		/// </summary>
		public ICondition Condition
		{
			get { return _condition; }
			set { this.SetField(ref _condition, value, "Condition"); }
		}
		#endregion //Condition

		#endregion //Properties
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