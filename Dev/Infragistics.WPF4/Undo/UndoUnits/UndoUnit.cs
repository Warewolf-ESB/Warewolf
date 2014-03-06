using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Undo
{
	/// <summary>
	/// Base class for an undo/redo operation.
	/// </summary>
	public abstract class UndoUnit
	{
		/// <summary>
		/// Returns the target object that will be affected by the <see cref="UndoUnit"/>
		/// </summary>
		public abstract object Target { get; }

		/// <summary>
		/// Used to perform the associated action.
		/// </summary>
		/// <param name="executeInfo">Provides information about the undo/redo operation being executed.</param>
		/// <returns>Returns true if some action was taken. Otherwise false is returned. In either case the object was removed from the undo stack.</returns>
		internal protected abstract bool Execute(UndoExecuteContext executeInfo);

		/// <summary>
		/// Used to invoke an action on the descendants.
		/// </summary>
		/// <param name="action">The action to invoke on descendants</param>
		internal virtual void ForEach(Action<UndoUnit> action)
		{
		}

		/// <summary>
		/// Returns a string representation of the action based on whether this is for an undo or redo operation.
		/// </summary>
		/// <param name="itemType">The type of history for which the description is being requested.</param>
		/// <param name="detailed">A boolean indicating if a detailed description should be returned. For example, when false one may return "Typing" but for verbose one may return "Typing 'qwerty'".</param>
		public abstract string GetDescription(UndoHistoryItemType itemType, bool detailed);

		/// <summary>
		/// Used to allow multiple consecutive undo units to be merged into a single operation.
		/// </summary>
		/// <param name="mergeInfo">Provides information about the unit to evaluate for a merge operation</param>
		/// <returns>Returns an enumeration used to provide identify how the unit was merged.</returns>
		internal protected virtual UndoMergeAction Merge(UndoMergeContext mergeInfo)
		{
			return UndoMergeAction.NotMerged;
		}

		/// <summary>
		/// Used to remove any matching descendants and indicate if the unit should be removed.
		/// </summary>
		/// <param name="match">The routine to evaluate any descendant units. This should not be executed against the UndoUnit whose RemoveAll is being called.</param>
		/// <returns>Returns true if the item should be removed; otherwise returns false.</returns>
		internal virtual bool RemoveAll(Func<UndoUnit, bool> match)
		{
			return false;
		}
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