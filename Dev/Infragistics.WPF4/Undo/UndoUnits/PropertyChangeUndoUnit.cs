using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Undo
{
	/// <summary>
	/// Custom undo unit used to undo the result of a property change.
	/// </summary>
	/// <typeparam name="TTarget">The type of object whose property was changed</typeparam>
	/// <typeparam name="TProperty">The type of the property whose value was changed</typeparam>
	public class PropertyChangeUndoUnit<TTarget, TProperty> : PropertyChangeUndoUnitBase
		where TTarget : class
	{
		#region Member Variables

		private WeakReference _target;
		private TProperty _oldValue;
		private TProperty _newValue;
		private GetSetProxy<TTarget, TProperty> _getSetProxy;
		private string _propertyDisplayName;
		private string _targetDisplayName;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PropertyChangeUndoUnit&lt;TTarget,TProperty&gt;"/>
		/// </summary>
		/// <param name="target">Object whose property was changed</param>
		/// <param name="oldValue">The old value of the property that is to be restored when an undo is performed.</param>
		/// <param name="newValue">The new value of the property.</param>
		/// <param name="setter">The delegate used to perform the undo</param>
		/// <param name="memberName">The name of the associated property</param>
		/// <param name="propertyDisplayName">The name of the property as it should be displayed to the end user or null to use the <paramref name="memberName"/>.</param>
		/// <param name="targetDisplayName">A string representing the target as it should be displayed to the end user or null to obtain the string representation from the <paramref name="target"/> itself.</param>
		public PropertyChangeUndoUnit(TTarget target, TProperty oldValue, TProperty newValue, Action<TTarget, TProperty> setter, string memberName, string propertyDisplayName, string targetDisplayName)
			: this(target, oldValue, newValue, new GetSetProxy<TTarget, TProperty>(null, setter, memberName, null), propertyDisplayName, targetDisplayName)
		{
		}

		internal PropertyChangeUndoUnit(TTarget target, TProperty oldValue, TProperty newValue, GetSetProxy<TTarget, TProperty> getSetProxy, string propertyDisplayName, string targetDisplayName)
		{
			CoreUtilities.ValidateNotNull(target, "target");
			CoreUtilities.ValidateNotNull(getSetProxy, "getSetter");
			CoreUtilities.ValidateNotNull(getSetProxy.Setter, "setter");

			_target = new WeakReference(target);
			_oldValue = oldValue;
			_newValue = newValue;
			_getSetProxy = getSetProxy;
			_propertyDisplayName = propertyDisplayName;
			_targetDisplayName = targetDisplayName;
		}
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the associated action.
		/// </summary>
		/// <param name="executeInfo">Provides information about the undo/redo operation being executed.</param>
		protected internal override bool Execute(UndoExecuteContext executeInfo)
		{
			var target = this.ActualTarget;

			if (target == null)
				return false;

			_getSetProxy.Setter(target, _oldValue);
			return true;
		}

		#endregion //Execute

		#region Merge
		/// <summary>
		/// Used to allow multiple consecutive undo units to be merged into a single operation.
		/// </summary>
		/// <param name="mergeInfo">Provides information about the unit to evaluate for a merge operation</param>
		/// <returns>Returns an enumeration used to provide identify how the unit was merged.</returns>
		internal protected override UndoMergeAction Merge(UndoMergeContext mergeInfo)
		{
			var other = mergeInfo.UnitBeingAdded as PropertyChangeUndoUnit<TTarget, TProperty>;

			if (other != null)
			{
				TTarget otherTarget = other.ActualTarget;
				TTarget target = this.ActualTarget;

				if (otherTarget == target && otherTarget != null && _getSetProxy.Equals(other._getSetProxy))
				{
					if (!EqualityComparer<TProperty>.Default.Equals(_oldValue, other._newValue))
					{
						_newValue = other._newValue;
						return UndoMergeAction.Merged;
					}
				}
			}

			return base.Merge(mergeInfo);
		} 
		#endregion //Merge

		#region NewValue
		/// <summary>
		/// Returns the new value that was set on the object.
		/// </summary>
		public override object NewValue
		{
			get { return _newValue; }
		}
		#endregion //NewValue 

		#region OldValue
		/// <summary>
		/// Returns the original value of the object that will be restored.
		/// </summary>
		public override object OldValue
		{
			get { return _oldValue; }
		}
		#endregion //OldValue

		#region PropertyDisplayName
		/// <summary>
		/// Optional string representing the property name as it should be displayed to the end user.
		/// </summary>
		protected override string PropertyDisplayName
		{
			get 
			{ 
				return _propertyDisplayName ?? _getSetProxy.DisplayName; 
			}
		}
		#endregion //PropertyDisplayName

		#region PropertyName
		/// <summary>
		/// Returns the name of the property that will be changed.
		/// </summary>
		public override string PropertyName
		{
			get { return _getSetProxy.Name; }
		}
		#endregion //PropertyName

		#region Target
		/// <summary>
		/// Returns the target object that will be affected by the <see cref="UndoUnit"/>
		/// </summary>
		public override object Target
		{
			get { return this.ActualTarget; }
		} 
		#endregion //Target

		#region TargetDisplayName
		/// <summary>
		/// Optional string representing the target as it should be displayed to the end user.
		/// </summary>
		protected override string TargetDisplayName
		{
			get { return _targetDisplayName; }
		}
		#endregion //TargetDisplayName

		#endregion //Base class overrides

		#region Properties
		/// <summary>
		/// Returns the object whose property is to be restored when executed.
		/// </summary>
		private TTarget ActualTarget
		{
			get { return CoreUtilities.GetWeakReferenceTargetSafe(_target) as TTarget; }
		} 
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