using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Undo
{
	/// <summary>
	/// Base class for an <see cref="UndoUnit"/> that changes the value of a single property.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class PropertyChangeUndoUnitBase : UndoUnit
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PropertyChangeUndoUnitBase"/>
		/// </summary>
		protected PropertyChangeUndoUnitBase()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetDescription
		/// <summary>
		/// Returns a string representation of the action based on whether this is for an undo or redo operation.
		/// </summary>
		/// <param name="itemType">The type of history for which the description is being requested.</param>
		/// <param name="detailed">A boolean indicating if a detailed description should be returned. For example, when false one may return "Typing" but for verbose one may return "Typing 'qwerty'".</param>
		public override string GetDescription(UndoHistoryItemType itemType, bool detailed)
		{
			return UndoManager.GetDefaultPropertyChangeDescription(this.TargetDisplayName ?? this.Target, this.OldValue, this.NewValue, this.PropertyDisplayName ?? this.PropertyName, itemType, detailed);
		}

		#endregion //GetDescription

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region NewValue
		/// <summary>
		/// Returns the new value that was set on the object.
		/// </summary>
		public abstract object NewValue
		{
			get;
		}
		#endregion //NewValue

		#region OldValue
		/// <summary>
		/// Returns the original value of the object that will be restored.
		/// </summary>
		public abstract object OldValue
		{
			get;
		}
		#endregion //OldValue

		#region PropertyName
		/// <summary>
		/// Returns the name of the property that will be changed.
		/// </summary>
		public abstract string PropertyName
		{
			get;
		}
		#endregion //PropertyName

		#endregion //Public Properties

		#region Protected Properties

		#region PropertyDisplayName
		/// <summary>
		/// Optional string representing the property name as it should be displayed to the end user.
		/// </summary>
		protected virtual string PropertyDisplayName
		{
			get { return null; }
		}
		#endregion //PropertyDisplayName

		#region TargetDisplayName
		/// <summary>
		/// Optional string representing the target as it should be displayed to the end user.
		/// </summary>
		protected virtual string TargetDisplayName
		{
			get { return null; }
		}
		#endregion //TargetDisplayName

		#endregion //Protected Properties

		#region Private Properties

		#region DebuggerDisplay
		private string DebuggerDisplay
		{
			get { return string.Format("PropertyChange: Target='{0}', Property='{1}', Old='{2}', New='{3}'", this.PropertyName, this.Target, this.OldValue, this.NewValue); }
		}
		#endregion //DebuggerDisplay

		#endregion //Private Properties

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