using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Undo
{
	#region UndoHistoryItemType
	/// <summary>
	/// Used to indicate the type of operation that <see cref="UndoHistoryItem"/> represents.
	/// </summary>
	public enum UndoHistoryItemType : byte
	{
		/// <summary>
		/// The item represents an undo operation.
		/// </summary>
		Undo,

		/// <summary>
		/// The item represents a redo operation.
		/// </summary>
		Redo
	} 
	#endregion //UndoHistoryItemType

	#region UndoExecuteReason
	/// <summary>
	/// Enumeration used to identify the cause of the <see cref="UndoUnit.Execute(UndoExecuteContext)"/>
	/// </summary>
	/// <seealso cref="UndoExecuteContext.Reason"/>
	public enum UndoExecuteReason
	{
		/// <summary>
		/// The execute is being performed because an Undo was requested.
		/// </summary>
		Undo,

		/// <summary>
		/// The execute is being performed because a Redo was requested
		/// </summary>
		Redo,

		/// <summary>
		/// The execute is being performed because an <see cref="UndoTransaction"/> was being rolled back.
		/// </summary>
		Rollback,
	} 
	#endregion //UndoExecuteReason

	#region UndoTransactionCloseAction
	/// <summary>
	/// Enumeration used to indicate what should happen to the <see cref="UndoTransaction"/> when it is closed.
	/// </summary>
	internal enum UndoTransactionCloseAction
	{
		/// <summary>
		/// The group is being committed/completed.
		/// </summary>
		Commit,

		/// <summary>
		/// The group is being cancelled and the object should be undone and removed.
		/// </summary>
		Rollback,

		/// <summary>
		/// The group is being cancelled and the object should be released.
		/// </summary>
		Cancel,
	}
	#endregion //UndoTransactionCloseAction

	#region UndoMergeAction
	/// <summary>
	/// Used to identify the result of a call to <see cref="UndoUnit.Merge(UndoMergeContext)"/>
	/// </summary>
	public enum UndoMergeAction
	{
		/// <summary>
		/// The information from the specified unit could not be merged and should be added to the undo stack.
		/// </summary>
		NotMerged,

		/// <summary>
		/// The information from the specified unit was merged in and doesn't need to be added to the undo stack.
		/// </summary>
		Merged,

		/// <summary>
		/// The information from the specified unit was merged and the original unit should be removed from the undo stack.
		/// </summary>
		MergedRemoveUnit,
	} 
	#endregion //UndoMergeAction
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