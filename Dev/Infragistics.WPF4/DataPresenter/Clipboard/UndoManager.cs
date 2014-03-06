using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using System.Diagnostics;

namespace Infragistics.Windows
{
	internal class UndoManager : ActionHistory
	{
		#region Constants
		private static object UndoContextId = new object();

		private static object RedoContextId = new object();

		#endregion //Constants

		#region Member Variables

		private int _undoLimit;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManager"/>
		/// </summary>
		public UndoManager()
			: this(0)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="UndoManager"/> with the specified undo limit.
		/// </summary>
		/// <param name="undoLimit">The initial number of items to allow within the undo/redo history.</param>
		public UndoManager(int undoLimit)
		{
			_undoLimit = undoLimit;
		}
		#endregion //Constructor

		#region Base class overrides
		protected sealed override object RedoContext
		{
			get
			{
				return RedoContextId;
			}
		}

		protected sealed override object UndoContext
		{
			get
			{
				return UndoContextId;
			}
		}

		protected override int MaxUndoDepth
		{
			get
			{
				return this.UndoLimit;
			}
		}
		#endregion //Base class overrides

		#region Properties

		#region Public

		#region UndoLimit
		/// <summary>
		/// Returns or sets the maximum number of items within the undo/redo history.
		/// </summary>
		public int UndoLimit
		{
			get { return _undoLimit; }
			set
			{
				_undoLimit = value;
				this.OnMaxUndoDepthChanged();
			}
		}
		#endregion //UndoLimit

		#endregion //Public

		#endregion //Properties

		#region Methods

		#region Public

		#region AddToUndo
		/// <summary>
		/// Adds an action to the undo history.
		/// </summary>
		/// <param name="action">The action to be added</param>
		/// <remarks>
		/// <p class="note"><b>Note:</b> The redo history will be cleared as part of the operation.</p>
		/// </remarks>
		public void AddToUndo(ActionHistory.ActionBase action)
		{
			GridUtilities.ValidateNotNull(action, "action");
			Debug.Assert(!this.IsPerformingAction);

			this.AddUndoAction(action);
			this.ClearRedo();
		}
		#endregion //AddToUndo

		#region Filter

		/// <summary>
		/// Used to enumerate and adjust/remove actions from the undo/redo history.
		/// </summary>
		/// <param name="filter">The delegate to invoke with each item from the undo/redo history.</param>
		public void Filter(ActionFilter filter)
		{
			GridUtilities.ValidateNotNull(filter, "filter");

			Stack<ActionBase> undoActions = this.Filter(this.GetUndoEnumerator(), filter);
			Stack<ActionBase> redoActions = this.Filter(this.GetRedoEnumerator(), filter);

			this.Clear();

			foreach (ActionBase action in undoActions)
				this.AddUndoAction(action);

			foreach (ActionBase action in redoActions)
				this.AddRedoAction(action);
		}

		private Stack<ActionBase> Filter(IEnumerator<ActionBase> actionEnumerator, ActionFilter filter)
		{
			Stack<ActionBase> actions = new Stack<ActionBase>();

			while (actionEnumerator.MoveNext())
			{
				ActionBase action = actionEnumerator.Current;

				action = filter(action);

				if (null == action)
					continue;

				actions.Push(action);
			}

			return actions;
		}
		#endregion //Filter

		#region ForEach
		/// <summary>
		/// Used to enumerate the undo/redo history and perform an action on each.
		/// </summary>
		/// <param name="action">The delegate to invoke with each item from the undo/redo history.</param>
		public void ForEach(Action<ActionBase> action)
		{
			this.ForEach(action, true, true);
		}

		/// <summary>
		/// Used to enumerate the undo/redo history and perform an action on each.
		/// </summary>
		/// <param name="action">The delegate to invoke with each item from the undo/redo history.</param>
		/// <param name="includeRedo">True to enumerate the undo items</param>
		/// <param name="includeUndo">True to enumerate the redo items</param>
		public void ForEach(Action<ActionBase> action, bool includeUndo, bool includeRedo)
		{
			GridUtilities.ValidateNotNull(action, "action");

			for (int i = 0; i < 2; i++)
			{
				bool isUndo = i == 0;

				if (isUndo && !includeUndo)
					continue;

				if (!isUndo && !includeRedo)
					continue;

				IEnumerator<ActionBase> enumerator = isUndo ? this.GetUndoEnumerator() : this.GetRedoEnumerator();

				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		} 
		#endregion //ForEach

		#region IsUndoRedoContext
		/// <summary>
		/// Indicates if an undo/redo operation is being performed based on the specified context.
		/// </summary>
		/// <param name="context">The context provided to an Action when its Perform method is invoked.</param>
		/// <returns>True if the context represents an undo/redo operation; otherwise false is returned.</returns>
		public static bool IsUndoRedoContext(object context)
		{
			return context == UndoContextId ||
				context == RedoContextId;
		}

		/// <summary>
		/// Indicates if an undo operation is being performed based on the specified context.
		/// </summary>
		/// <param name="context">The context provided to an Action when its Perform method is invoked.</param>
		/// <returns>True if the context represents an undo operation; otherwise false is returned.</returns>
		public static bool IsUndoContext(object context)
		{
			return context == UndoContextId;
		}

		/// <summary>
		/// Indicates if an redo operation is being performed based on the specified context.
		/// </summary>
		/// <param name="context">The context provided to an Action when its Perform method is invoked.</param>
		/// <returns>True if the context represents an redo operation; otherwise false is returned.</returns>
		public static bool IsRedoContext(object context)
		{
			return context == RedoContextId;
		}
		#endregion //IsUndoRedoContext

		#endregion //Public

		#endregion //Methods

		#region ActionFilter

		/// <summary>
		/// Custom callback used to be called with one or more actions from the undo history.
		/// </summary>
		/// <param name="action">An action from the history</param>
		/// <returns>The action to be added to the undo or redo history.</returns>
		public delegate ActionBase ActionFilter(ActionBase action);

		#endregion //ActionFilter
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