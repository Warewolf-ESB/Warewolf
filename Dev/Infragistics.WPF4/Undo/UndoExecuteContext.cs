using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Undo
{
	/// <summary>
	/// Used to provide information to the <see cref="UndoUnit"/> while the <see cref="UndoUnit.Execute(UndoExecuteContext)"/> method is being invoked.
	/// </summary>
	/// <seealso cref="UndoUnit.Execute(UndoExecuteContext)"/>
	/// <seealso cref="UndoExecuteReason"/>
	public class UndoExecuteContext
	{
		#region Member Variables

		private UndoManager _undoManager;
		private UndoExecuteReason _reason;
		private StackList<UndoUnit> _executeStack = new StackList<UndoUnit>();

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoExecuteContext"/>
		/// </summary>
		/// <param name="undoManager">The UndoManager for which the <see cref="UndoUnit"/> is being executed.</param>
		/// <param name="reason">Indicates what triggered the execute.</param>
		internal UndoExecuteContext(UndoManager undoManager, UndoExecuteReason reason)
		{
			CoreUtilities.ValidateNotNull(undoManager, "undoManager");

			_undoManager = undoManager;
			_reason = reason;
		} 
		#endregion //Constructor

		#region Properties

		#region ExecuteItemType
		/// <summary>
		/// Returns an enumeration used to indicate the type of operation being performed.
		/// </summary>
		public UndoHistoryItemType ExecuteItemType
		{
			get
			{
				switch (_reason)
				{
					case UndoExecuteReason.Undo:
					{
						return UndoHistoryItemType.Undo;
					}
					case UndoExecuteReason.Redo:
					{
						return UndoHistoryItemType.Redo;
					}
					case UndoExecuteReason.Rollback:
					{
						// if we're rolling back an undo then this is really like doing a redo
						if (_undoManager.IsPerformingUndo)
							return UndoHistoryItemType.Redo;

						// if this is a new operation or we were performing a redo then 
						// this is really akin to an undo
						return UndoHistoryItemType.Undo;
					}
					default:
					{
						Debug.Assert(false, "Unexpected reason:" + _reason.ToString());
						return UndoHistoryItemType.Undo;
					}
				}
			}
		}
		#endregion //ExecuteItemType

		#region Reason
		/// <summary>
		/// Returns an enumeration indicating what triggered the <see cref="UndoUnit.Execute(UndoExecuteContext)"/>
		/// </summary>
		public UndoExecuteReason Reason
		{
			get { return _reason; }
		} 
		#endregion //Reason

		#region UnitCount
		/// <summary>
		/// Returns the depth of the current UndoUnit being executed.
		/// </summary>
		/// <remarks>This will typically return 1 unless there are transactions being executed.</remarks>
		internal int UnitCount
		{
			get { return _executeStack.Count; }
		} 
		#endregion //UnitCount

		#region UndoManager
		/// <summary>
		/// Returns the <see cref="UndoManager"/> for which the <see cref="UndoUnit"/> is being executed.
		/// </summary>
		public UndoManager UndoManager
		{
			get { return _undoManager; }
		} 
		#endregion //UndoManager 

		#endregion //Properties

		#region Methods

		#region GetUnit
		/// <summary>
		/// Returns an undo unit in the current execution stack.
		/// </summary>
		/// <param name="offset">The offset into the undo units being executed. 0 returns the UndoUnit currently being executed and <see cref="UnitCount"/> - 1 would return the outermost unit being executed.</param>
		/// <returns></returns>
		internal UndoUnit GetUnit(int offset)
		{
			if (offset < 0 || offset > this.UnitCount)
				return null;

			return _executeStack.GetItem(offset);
		} 
		#endregion //GetUnit

		#region Execute
		/// <summary>
		/// Used to execute an UndoUnit with this instance as the context.
		/// </summary>
		/// <param name="unit">The unit to be executed.</param>
		/// <returns></returns>
		internal bool Execute(UndoUnit unit)
		{
			CoreUtilities.ValidateNotNull(unit, "unit");

			_executeStack.Push(unit);

			try
			{
				return unit.Execute(this);
			}
			finally
			{
				_executeStack.Pop();
			}
		} 
		#endregion //Execute

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