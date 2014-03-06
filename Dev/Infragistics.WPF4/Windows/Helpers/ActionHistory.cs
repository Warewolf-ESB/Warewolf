





using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Windows.Helpers
{
	/// <summary>
	/// Maintains an undo/redo history.
	/// </summary>
	public abstract class ActionHistory
	{
		#region Member Variables

		private ActionsCollection undoActions = null;
		private ActionsCollection redoActions = null;
		private bool isPerformingAction = false;

		/// <summary>
		/// Represents an undo action for an action that cannot be undone.
		/// </summary>
		/// <remarks>
		/// <p class="body">Returning this action from the <see cref="ActionBase.Perform(object)"/> method 
		/// will clear the undo/redo history but return true to indicate the operation was successful.</p>
		/// </remarks>
		public static readonly ActionBase NoUndoAction = NoOpAction.Instance;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="ActionHistory"/>
		/// </summary>
		protected ActionHistory()
		{
		}

		#endregion // Constructor

		#region Properties

        #region Public Properties

        #region IsPerformingAction
        // AS 4/14/09 NA 2009.2 ClipboardSupport
        /// <summary>
        /// Returns a boolean indicating if the history is currently performing an action.
        /// </summary>
        public bool IsPerformingAction
        {
            get { return isPerformingAction; }
        }
        #endregion //IsPerformingAction

        #endregion //Public Properties

		#region Private Properties

		#region HasRedoActions

		private bool HasRedoActions
		{
			get
			{
				return null != this.redoActions && this.redoActions.Count > 0;
			}
		}

		#endregion // HasRedoActions

		#region HasUndoActions

		private bool HasUndoActions
		{
			get
			{
				return null != this.undoActions && this.undoActions.Count > 0;
			}
		}

		#endregion // HasUndoActions

		#region RedoActionOnQueue

		private ActionBase RedoActionOnQueue
		{
			get
			{
				return this.HasRedoActions ? this.RedoActions.Peek() : null;
			}
		}

		#endregion // RedoActionOnQueue

		#region RedoActions

		private ActionsCollection RedoActions
		{
			get
			{
				if (null == this.redoActions)
					this.redoActions = new ActionsCollection(this.MaxUndoDepth);

				return this.redoActions;
			}
		}

		#endregion // RedoActions

		#region UndoActions

		private ActionsCollection UndoActions
		{
			get
			{
				if (null == this.undoActions)
					this.undoActions = new ActionsCollection(this.MaxUndoDepth);

				return this.undoActions;
			}
		}

		#endregion // UndoActions

		#region UndoActionOnQueue

		private ActionBase UndoActionOnQueue
		{
			get
			{
				return this.HasUndoActions ? this.UndoActions.Peek() : null;
			}
		}

		#endregion // UndoActionOnQueue

		#endregion //Private Properties

		#region Protected Properties
		/// <summary>
		/// The content to use when performing an undo action.
		/// </summary>
		protected virtual object UndoContext
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// The context to use when performing a redo action.
		/// </summary>
		protected virtual object RedoContext
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// The maximum number of undo items to maintain.
		/// </summary>
		protected virtual int MaxUndoDepth
		{
			get
			{
				return 100;
			}
		}
		#endregion //Protected Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region CanRedo

		/// <summary>
		/// Determines if there is an entry in the redo stack that can be performed.
		/// </summary>
		/// <returns>Returns true if there is an redo action on the undo stack and it can be performed.</returns>
		public bool CanRedo()
		{
			// AS 5/14/09 NA 2009.2 Undo/Redo
			//ActionBase action = this.RedoActionOnQueue;
			//return null != action && action.CanPerform(this.RedoContext);
			return this.CanPerform(redoActions, RedoContext);
		}

		#endregion // CanRedo

		#region CanUndo

		/// <summary>
		/// Determines if there is an entry in the undo stack that can be performed.
		/// </summary>
		/// <returns>Returns true if there is an undo action on the undo stack and it can be performed.</returns>
		public bool CanUndo()
		{
			// AS 5/14/09 NA 2009.2 Undo/Redo
			//ActionBase action = this.UndoActionOnQueue;
			//return null != action && action.CanPerform(this.UndoContext);
			return this.CanPerform(undoActions, UndoContext);
		}

		#endregion // CanUndo

		#region Clear

		/// <summary>
		/// Clears all the undo and redo actions.
		/// </summary>
		public void Clear()
		{
			this.ClearUndo();
			this.ClearRedo();
		}

		#endregion // Clear

		#region ClearRedo

		/// <summary>
		/// Removes any redo actions.
		/// </summary>
		public void ClearRedo()
		{
			if (this.HasRedoActions)
			{
				this.RedoActions.Clear();
				// AS 7/31/09
				// Do not clear the member. If we were within the performing action then 
				// the list handed to the PerformAction will change instead of adding it 
				// to the actual undo action.
				//
				//this.redoActions = null;
			}
		}

		#endregion // ClearRedo

		#region ClearUndo

		/// <summary>
		/// Removes all undo actions.
		/// </summary>
		internal void ClearUndo()
		{
			if (this.HasUndoActions)
			{
				this.UndoActions.Clear();
				// AS 7/31/09
				// Do not clear the member. If we were within the performing action then 
				// the list handed to the PerformAction will change instead of adding it 
				// to the actual undo action.
				//
				//this.undoActions = null;
			}
		}

		#endregion // ClearUndo

		#region PerformAction

		/// <summary>
		/// Performs the specified action and adds the resulting undo action into the undo history.
		/// </summary>
		/// <param name="action">The action to perform</param>
		/// <param name="actionContext">The content to provide to the action when it is performed.</param>
		/// <returns>True if the action was performed; otherwise false is returned.</returns>
		public bool PerformAction(ActionBase action, object actionContext)
		{
			return this.PerformActionHelper(actionContext, action, this.UndoActions);
		}

		#endregion // PerformAction

		#region Redo

		/// <summary>
		/// Performs a single redo operation.
		/// </summary>
		/// <returns>Returns true if a redo action was performed</returns>
		public bool Redo()
		{
			return this.HasRedoActions
				&& this.UndoRedoHelper(
						this.RedoContext,
						this.RedoActions,
						this.UndoActions);
		}

		#endregion // Redo

		#region Undo

		/// <summary>
		/// Performs a single undo operation.
		/// </summary>
		/// <returns>True if an undo action was performed.</returns>
		public bool Undo()
		{
			return this.HasUndoActions 
                && this.UndoRedoHelper(this.UndoContext, this.UndoActions, this.RedoActions);
		}

		#endregion // Undo

		#endregion //Public Methods

		#region Protected Methods
		/// <summary>
		/// Adds an action to the undo history.
		/// </summary>
		/// <param name="action">Action to add to the undo stack</param>
		protected void AddUndoAction(ActionBase action)
		{
			if (action is NoOpAction == false)
				this.UndoActions.Push(action);
		}

		/// <summary>
		/// Adds an action to the redo history.
		/// </summary>
		/// <param name="action">Action to add to the redo stack</param>
		protected void AddRedoAction(ActionBase action)
		{
			if (action is NoOpAction == false)
				this.RedoActions.Push(action);
		}

		/// <summary>
		/// Returns an enumerator used to iterate the actions on the undo stack.
		/// </summary>
		protected IEnumerator<ActionBase> GetRedoEnumerator()
		{
			return this.RedoActions.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator used to iterate the actions on the undo stack.
		/// </summary>
		protected IEnumerator<ActionBase> GetUndoEnumerator()
		{
			return this.UndoActions.GetEnumerator();
		}

        // AS 4/8/09 NA 2009.2 ClipboardSupport
        #region OnMaxUndoDepthChanged
        /// <summary>
        /// Invoked when the <see cref="MaxUndoDepth"/> has been changed.
        /// </summary>
        protected void OnMaxUndoDepthChanged()
        {
            if (null != undoActions)
                undoActions.SetMaxItems(this.MaxUndoDepth);

            if (null != redoActions)
                redoActions.SetMaxItems(this.MaxUndoDepth);
        } 
        #endregion //OnMaxUndoDepthChanged

		/// <summary>
		/// Returns the action that will be performed if the <see cref="Redo"/> method is invoked.
		/// </summary>
		/// <returns>The next action to redo or null if there are no actions to redo.</returns>
		protected ActionBase PeekRedo()
		{
			return this.RedoActionOnQueue;
		}

		/// <summary>
		/// Returns the action that will be performed if the <see cref="Undo"/> method is invoked.
		/// </summary>
		/// <returns>The next action to undo or null if there are no actions to undo.</returns>
		protected ActionBase PeekUndo()
		{
			return this.UndoActionOnQueue;
		}
		#endregion //Protected Methods

		#region Private Methods

		// AS 5/14/09 NA 2009.2 Undo/Redo
		// While the Undo/Redo methods would pop items off the stack, the actual
		// canundo/redo methods only evaluated the current item on the respective 
		// queues. I made the ActionsCollection enumerable and changed the can
		// methods to use this.
		// 
		#region CanPerform
		private bool CanPerform(ActionsCollection stack, object context)
		{
			if (null == stack)
				return false;

			foreach (ActionBase action in stack)
			{
				if (action.CanPerform(context))
					return true;
			}

			return false;
		}
		#endregion //CanPerform

		#region Output
		[Conditional("DEBUG_HISTORY")]
		private static void Output(string message, string category)
		{
			Debug.WriteLine(message, category);
		} 
		#endregion //Output

		#region PerformAction



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private bool PerformActionHelper(object actionContext, ActionBase action, ActionsCollection undoList)
		{
			Debug.Assert(!isPerformingAction, "We are attempting to perform an action while in the process of performing an action. The undo history order may be compromised.");

			// Set the isPerformingAction flag so we can ignore the ItemChanged notifications
			// that result from the action. This is because in response to ItemChanged we end 
			// up clearing the undo history.
			// 
			bool origIsPerformingAction = this.isPerformingAction;
			this.isPerformingAction = true;

			try
			{
				#region Debug
				Output(string.Format("Action:{0}, Context:{1}", action, actionContext == UndoContext ? "Undo"
					: actionContext == RedoContext ? "Redo" : actionContext), "PerformAction[Start]"); 
				#endregion //Debug

				ActionBase undoAction = action.Perform(actionContext);
				bool success = undoAction != null;

				#region Debug
				Output(string.Format("Action:{0}, Context:{1}, Success:{2}, UndoAction:{3}", action,
					actionContext == UndoContext ? "Undo"
						: actionContext == RedoContext ? "Redo" : actionContext,
						success,
						undoAction == NoOpAction.Instance ? "NoUndo" : (object)undoAction
					), "PerformAction Result"); 
				#endregion //Debug

				if (success)
				{
					// Otherwise clear the current undo/redo history since the action
					// was performed however it didn't returned an undo action and thus
					// the action can not be undone.
					//
                    if (undoAction is NoOpAction)
                        this.Clear();
                    else
                    {
                        // AS 4/14/09 NA 2009.2 ClipboardSupport
                        // If a new action is being performed and we had performed
                        // some undos then we should be clearing the redo stack.
                        //
                        if (!object.Equals(actionContext, this.UndoContext) &&
                            !object.Equals(actionContext, this.RedoContext) &&
                            undoList == this.undoActions)
                        {
                            this.ClearRedo();
                        }

                        undoList.Push(undoAction);
                    }
				}

				return success;
			}
			finally
			{
				#region Debug
				Output(string.Format("Action:{0}, Context:{1}", action,
					actionContext == UndoContext ? "Undo"
						: actionContext == RedoContext ? "Redo" : actionContext),
						"PerformAction[End]"); 
				#endregion //Debug

				this.isPerformingAction = origIsPerformingAction;
			}
		}

		#endregion // PerformAction

		#region UndoRedoHelper



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private bool UndoRedoHelper(object actionContext, ActionsCollection xxStack, ActionsCollection yyStack)
		{
			ActionBase action = xxStack.Pop();
			while (!action.CanPerform(actionContext) && xxStack.Count > 0)
				action = xxStack.Pop();

			return this.PerformActionHelper(actionContext, action, yyStack);
		}

		#endregion // UndoRedoHelper

		#endregion //Private Methods

		#endregion //Methods

		#region ActionBase Class

		/// <summary>
		/// Base class that represents a single action to perform.
		/// </summary>
		public abstract class ActionBase
		{
			#region Member Variables

			#endregion //Member Variables

			#region Constructor
			/// <summary>
			/// Initializes a new <see cref="ActionBase"/>
			/// </summary>
			protected ActionBase()
			{
			} 
			#endregion //Constructor

			#region Properties
			#endregion //Properties

			#region Methods

			/// <summary>
			/// Returns true if this action can be performed. Note that even if this method returns true, the Perform could fail.
			/// </summary>
			/// <param name="context">This is an object that is used to provides additional information to the action</param>
			/// <returns>Returns true if the action can be performed; ptherwise false is returned.</returns>
			internal protected abstract bool CanPerform(object context);

			/// <summary>
			/// Performs the action. If successful in performing the action then returns an action 
			/// that can undo the action performed by this action. If failure occurs then returns
			/// null.
			/// </summary>
			/// <param name="context">Opaque piece of data that the derived action class can use to determine what is acting upon. Note that the
			/// derived classes may require this parameter to be an instance of a specific class.</param>
			/// <returns>Returns an action that can be used to undo the results of this action. If the action was performed but cannot be undone then <see cref="ActionHistory.NoUndoAction"/> should be returned. If the action could not be performed then null should be returned.</returns>
			internal protected abstract ActionBase Perform(object context);

			#endregion //Methods
		}

		#endregion // ActionBase Class

		#region NoOpAction class
		internal class NoOpAction : ActionBase
		{
			internal static readonly ActionBase Instance = new NoOpAction();

			private NoOpAction()
			{
			}

			protected internal override bool CanPerform(object context)
			{
				return false;
			}

			protected internal override ActionBase Perform(object context)
			{
				return this;
			}
		}
		#endregion //NoOpAction class

		#region ActionsCollection Class

		private class ActionsCollection : IEnumerable<ActionBase>
		{
			#region Member Variables

            // AS 4/8/09 NA 2009.2 ClipboardSupport
			//private readonly int _maxItems;
			private int _maxItems;
			private List<ActionBase> stack;
			private int _version;

			#endregion //Member Variables

			#region Constructor
			internal ActionsCollection(int maxItems)
			{
                
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                this.SetMaxItems(maxItems);

				stack = new List<ActionBase>();
			} 
			#endregion //Constructor

			#region Properties
			internal int Count
			{
				get
				{
					return this.stack.Count;
				}
			} 
			#endregion //Properties

			#region Methods
			internal void Clear()
			{
				this.stack.Clear();
				_version++;
			}

            // AS 5/14/09 NA 2009.2 ClipboardSupport
			public IEnumerator<ActionBase> GetEnumerator()
			{
				return new Enumerator(this);
			}

			internal ActionBase Peek()
			{
				return this.stack[this.Count - 1];
			}

			internal ActionBase Pop()
			{
				ActionBase action = this.Peek();
				this.stack.RemoveAt(this.Count - 1);

				_version++;

				return action;
			}

			internal void Push(ActionBase action)
			{
				this.stack.Add(action);

				if (this.Count > _maxItems)
					this.stack.RemoveAt(0);

				_version++;
			}

            // AS 4/8/09 NA 2009.2 ClipboardSupport
            #region SetMaxItems
            internal void SetMaxItems(int maxItems)
            {
                Debug.Assert(maxItems >= 0, "Cannot have negative max item!");
                if (maxItems < 0)
                    throw new ArgumentOutOfRangeException();

                this._maxItems = maxItems > 0 ? maxItems : int.MaxValue;

				if (null != stack && stack.Count > _maxItems)
				{
					stack.RemoveRange(0, stack.Count - _maxItems);
					_version++;
				}
            }
            #endregion //SetMaxItems

            #endregion //Methods

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion //IEnumerable

			#region Enumerator
			private class Enumerator : IEnumerator<ActionBase>
			{
				private ActionsCollection _actions;
				private int _version;
				private int _index;
				private ActionBase _current;

				internal Enumerator(ActionsCollection actions)
				{
					_actions = actions;
					this.Reset();
				}

				public bool MoveNext()
				{
					if (_version != _actions._version)
						throw new InvalidOperationException();

					if (_index == -1)
						return false;

					if (_index == -2)
						_index = _actions.Count;

					_index--;

					if (_index >= 0)
					{
						_current = _actions.stack[_index];
						return true;
					}

					_current = null;
					return false;
				}

				public ActionBase Current
				{
					get
					{
						if (this._index == -2)
							throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_3"));
						else if (this._index == -1)
							throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_4"));

						return _current;
					}
				}

				public void Reset()
				{
					_version = _actions._version;
					_index = -2;
					_current = null;
				}

				#region IDisposable

				void IDisposable.Dispose()
				{
				}

				#endregion //IDisposable

				#region IEnumerator

				object System.Collections.IEnumerator.Current
				{
					get { return this.Current; }
				}

				#endregion //IEnumerator
			} 
			#endregion //Enumerator
		}

		#endregion // ActionsCollection Class
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