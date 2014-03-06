using System;
using System.Net;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Shapes;
using Infragistics.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Infragistics.Undo
{
	/// <summary>
	/// Class used to manage an undo/redo history.
	/// </summary>
	/// <remarks>
	/// <p class="body">The UndoManager manages the undo and redo history. Operations are added to the undo/redo history by adding 
	/// <see cref="UndoUnit"/> instances to it using one of the available methods. One can create an instance of a custom UndoUnit 
	/// derived class or one of the classes included in this assembly and use the <see cref="AddChange(UndoUnit)"/> method. The 
	/// UndoManager also exposes a number of helper methods that will create an UndoUnit and add it to the history. The 
	/// AddPropertyChange overloads can be used to record the change of a property of some object. The AddCollectionChange overloads 
	/// can be used to record a change to an collection implementing ICollection&gt;T&lt;. The remaining AddChange overloads may be 
	/// used to provide a set of delegates representing the Undo and Redo actions that should be taken when the manager is performing 
	/// an undo or redo operation. These methods using the <see cref="UndoUnitFactoryResolved"/> so if you want to change the UndoUnit 
	/// type that is created by these methods you may derive a class from <see cref="Infragistics.Undo.UndoUnitFactory"/> and set the 
	/// <see cref="UndoUnitFactory"/> property to an instance of that class.</p>
	/// <p class="body">The <see cref="UndoHistory"/> and <see cref="RedoHistory"/> are read-only collections of <see cref="UndoHistoryItem"/> 
	/// instances. UndoHistoryItem exposes the root <see cref="UndoUnit"/> that represents the operation to be performed as well as 
	/// short and long descriptions of the operation. These collections implement INotifyCollectionChanged and therefore can be used 
	/// in your UI as the source for menu, etc. that would display the history to the end user.</p>
	/// <p class="body">The <see cref="Undo(int)"/> and <see cref="Redo(int)"/> methods are used to perform an undo or redo operation. When 
	/// these methods are invoked a <see cref="UndoHistoryItem"/> is removed from the associated history (e.g. UndoHistory) and then the 
	/// <see cref="UndoUnit.Execute(UndoExecuteContext)"/> method of the <see cref="UndoHistoryItem.Unit"/> is invoked. It is important 
	/// to realize that by default UndoUnits are not automatically moved to the opposite history. For example, while undoing a property change 
	/// it is expected that the property change will result in a new call to the AddPropertyChange overload. That would create a new undo unit 
	/// representing that change and add it to the UndoManager's history. In that case, since the UndoManager was in the process of performing 
	/// an undo, the new UndoUnit would be added to the redo history.</p>
	/// <p class="body">The UndoManager exposes a number of read-only boolean properties which indicate the current state of the object. The 
	/// <see cref="IsPerformingUndo"/> and <see cref="IsPerformingRedo"/> are true while performing an undo or redo respectively. The 
	/// <see cref="CanUndo"/> and <see cref="CanRedo"/> properties indicate if there are any items in the undo/redo history.</p>
	/// <p class="body">When performing discrete initialization, one can use the <see cref="Suspend"/> method to temporarily disable 
	/// logging of the UndoUnits in the history. Any calls to AddChange are ignored until the <see cref="Resume"/> method is invoked. The 
	/// Resume method must be invoked once for each time that the Suspend method is invoked. The <see cref="IsSuspended"/> property will 
	/// return true between the calls to Suspend and Resume.</p>
	/// <p class="body">The UndoManager provides support for merging of a new UndoUnit with the most recent UndoUnit that was stored. This is 
	/// useful in cases where the same action is occuring multiple times. For example while dragging the thumb of a Slider and therefore the 
	/// property that is bound to the Slider's Value. Merging may be disabled indefinitely by setting the <see cref="AllowMerging"/> to false. 
	/// One can also suppress the merging of the current item by invoking the <see cref="PreventMerge"/> method.</p>
	/// <p class="body">When multiple operations should be treated as a single undoable action, one can use transactions. The UndoManager 
	/// provides support for transactions using the <see cref="StartTransaction(string, string)"/> method. This returns a <see cref="UndoTransaction"/> 
	/// instance. Any subsequent units that are added to the UndoManager are stored within that transaction until the transaction has been committed 
	/// (see <see cref="UndoTransaction.Commit"/>) or rolled back (see <see cref="UndoTransaction.Rollback"/>. Nested transactions are supported so 
	/// calling StartTransaction while a transaction is progress will add that to the <see cref="UndoTransaction.Units"/> of the 
	/// <see cref="CurrentTransaction"/>. One can get access to the current open transaction, if there is one, using the <see cref="RootTransaction"/> 
	/// property. Also, if there is a block of code that you want to execute within a transaction, you may use the 
	/// <see cref="ExecuteInTransaction(string, string, Action)"/> method. This method will take care of calling the Rollback method of the transaction 
	/// it creates if an exception occurs (without preventing the exception from bubbling up). Otherwise it will invoke the Commit of the transaction. 
	/// When the <see cref="RootTransaction"/> has been committed it is then added to the undo or redo history depending on the current state of the 
	/// UndoManager.</p>
	/// </remarks>
	public partial class UndoManager : PropertyChangeNotifier
		, IUndoTransactionOwner
	{
		#region Member Variables

		// static members
		[ThreadStatic]
		private static WeakDictionary<object, UndoManager> _registeredUndoManagers;

		[ThreadStatic]
		private static UndoManager _current;

		// instance members
		private int _suspendCount;
		private StackList<UndoHistoryItem> _undoStack;
		private StackList<UndoHistoryItem> _redoStack;
		private IList<UndoHistoryItem> _undoHistory;
		private IList<UndoHistoryItem> _redoHistory;
		private UndoUnitFactory _undoUnitFactory;
		private TimeSpan _mergeTimeout = TimeSpan.Zero;
		private DateTime _lastNewOperation = DateTime.MinValue;
		private Stack<UndoHistoryItem> _cachedRedoStack;
		private BoolProperties _boolFlags = BoolProperties.AllowMerging;
		private UndoTransaction _rootTransaction;
		private UndoTransaction _currentTransaction;
		private UndoHistoryItem _topUndoHistoryItem;
		private UndoHistoryItem _topRedoHistoryItem;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UndoManager"/>
		/// </summary>
		public UndoManager()
		{
			_undoStack = new StackList<UndoHistoryItem>();
			_redoStack = new StackList<UndoHistoryItem>();
			_undoHistory = new ReadOnlyNotifyCollection<UndoHistoryItem>(_undoStack);
			_redoHistory = new ReadOnlyNotifyCollection<UndoHistoryItem>(_redoStack);

			_undoStack.CollectionChanged += new NotifyCollectionChangedEventHandler(OnHistoryStackChanged);
			_redoStack.CollectionChanged += new NotifyCollectionChangedEventHandler(OnHistoryStackChanged);
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region AllowMerging
		/// <summary>
		/// Returns or sets a boolean indicating if the <see cref="UndoUnit.Merge(UndoMergeContext)"/> method may be used to merge changes.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> The <see cref="UndoUnit.Merge(UndoMergeContext)"/> method is only invoked when 2 or more 
		/// new UndoUnits are added to the history. If an Undo or Redo operation is performed, the method will not be 
		/// invoked until after the next non-undo/redo operation is performed.</p>
		/// </remarks>
		[DefaultValue(true)]
		public bool AllowMerging
		{
			get { return this.GetBool(BoolProperties.AllowMerging); }
			set { this.SetBool(BoolProperties.AllowMerging, value); }
		} 
		#endregion //AllowMerging

		#region CanRedo
		/// <summary>
		/// Returns a boolean indicating if there is an operation that can be redone.
		/// </summary>
		public bool CanRedo
		{
			get { return this.GetBool(BoolProperties.CanRedo); }
			private set { this.SetBool(BoolProperties.CanRedo, value); }
		} 
		#endregion //CanRedo

		#region CanUndo
		/// <summary>
		/// Returns a boolean indicating if there is an operation that can be undone.
		/// </summary>
		public bool CanUndo
		{
			get { return this.GetBool(BoolProperties.CanUndo); }
			private set { this.SetBool(BoolProperties.CanUndo, value); }
		} 
		#endregion //CanUndo

		#region Current
		/// <summary>
		/// Returns a thread static singleton instance of an UndoManager.
		/// </summary>
		public static UndoManager Current
		{
			get
			{
				if (_current == null)
					_current = new UndoManager();

				return _current;
			}
		} 
		#endregion //Current

		#region CurrentTransaction
		/// <summary>
		/// Returns the current open leaf <see cref="UndoTransaction"/>
		/// </summary>
		public UndoTransaction CurrentTransaction
		{
			get
			{
				if (_currentTransaction == null)
				{
					if (_rootTransaction != null)
						_currentTransaction = UndoTransaction.GetLeafOpenGroup(_rootTransaction);
				}

				Debug.Assert(_currentTransaction == null || _rootTransaction != null, "We still have a current transaction even though we don't have a root one?");

				return _currentTransaction;
			}
		} 
		#endregion //CurrentTransaction

		#region IsPerformingRedo
		/// <summary>
		/// Returns a boolean indicating if the manager is performing a redo operation.
		/// </summary>
		public bool IsPerformingRedo
		{
			get { return this.GetBool(BoolProperties.IsPerformingRedo); }
		}
		#endregion //IsPerformingRedo

		#region IsPerformingRollback
		/// <summary>
		/// Returns a boolean indicating if the <see cref="UndoManager"/> is performing a Rollback of a <see cref="UndoTransaction"/>
		/// </summary>
		public bool IsPerformingRollback
		{
			get { return this.GetBool(BoolProperties.IsPerformingRollback); }
			private set { this.SetBool(BoolProperties.IsPerformingRollback, value, true); }
		} 
		#endregion //IsPerformingRollback

		#region IsPerformingUndo
		/// <summary>
		/// Returns a boolean indicating if the manager is performing an undo operation.
		/// </summary>
		public bool IsPerformingUndo
		{
			get { return this.GetBool(BoolProperties.IsPerformingUndo); }
		}
		#endregion //IsPerformingUndo

		#region IsSuspended
		/// <summary>
		/// Returns a boolean indicating if the recording of <see cref="UndoUnit"/> instances in the history has been suspended using the <see cref="Suspend"/> method.
		/// </summary>
		/// <seealso cref="Suspend"/>
		/// <seealso cref="Resume"/>
		public bool IsSuspended
		{
			get { return _suspendCount > 0; }
		}
		#endregion //IsSuspended

		#region MergeTimeout
		/// <summary>
		/// Returns the amount of time since the last operation that a <see cref="UndoUnit.Merge(UndoMergeContext)"/> will be allowed.
		/// </summary>
		[DefaultValue(typeof(TimeSpan), "0")]
		public TimeSpan MergeTimeout
		{
			get { return _mergeTimeout; }
			set
			{
				CoreUtilities.ValidateIsNotNegative(value.Ticks, "value");

				this.SetField(ref _mergeTimeout, value, "MergeTimeout");
			}
		} 
		#endregion //MergeTimeout

		#region RedoHistory
		/// <summary>
		/// Returns a read-only collection of the items in the redo history.
		/// </summary>
		/// <seealso cref="TopRedoHistoryItem"/>
		/// <seealso cref="CanRedo"/>
		/// <seealso cref="UndoHistory"/>
		/// <seealso cref="UndoHistoryItem"/>
		public IList<UndoHistoryItem> RedoHistory
		{
			get { return _redoHistory; }
		}
		#endregion //UndoHistory

		#region RootTransaction
		/// <summary>
		/// Returns the outermost current open transaction or null if there is currently no transaction open.
		/// </summary>
		public UndoTransaction RootTransaction
		{
			get { return _rootTransaction; }
			private set
			{
				if (value != _rootTransaction)
				{
					_rootTransaction = value;
					this.OnPropertyChanged("RootTransaction");
				}
			}
		}
		#endregion //RootTransaction

		#region TopRedoHistoryItem
		/// <summary>
		/// Returns the next item on the top of the <see cref="RedoHistory"/>
		/// </summary>
		/// <remarks>
		/// <p>This property returns the <see cref="UndoHistoryItem"/> at the top of the <see cref="RedoHistory"/> 
		/// stack and is therefore equivalent to accessing RedoHistory[0]. The primary use case for this property 
		/// is for binding such as when displaying the <see cref="UndoHistoryItem.ShortDescription"/> for the tooltip 
		/// of an Redo button in the UI to avoid the exception information in the output window.</p>
		/// </remarks>
		/// <seealso cref="TopUndoHistoryItem"/>
		/// <seealso cref="RedoHistory"/>
		public UndoHistoryItem TopRedoHistoryItem
		{
			get { return _topRedoHistoryItem; }
			private set { this.SetField(ref _topRedoHistoryItem, value, "TopRedoHistoryItem"); }
		}
		#endregion //TopRedoHistoryItem

		#region TopUndoHistoryItem
		/// <summary>
		/// Returns the next item on the top of the <see cref="UndoHistory"/>
		/// </summary>
		/// <remarks>
		/// <p>This property returns the <see cref="UndoHistoryItem"/> at the top of the <see cref="UndoHistory"/> 
		/// stack and is therefore equivalent to accessing UndoHistory[0]. The primary use case for this property 
		/// is for binding such as when displaying the <see cref="UndoHistoryItem.ShortDescription"/> for the tooltip 
		/// of an Undo button in the UI to avoid the exception information in the output window.</p>
		/// </remarks>
		/// <seealso cref="TopRedoHistoryItem"/>
		/// <seealso cref="UndoHistory"/>
		public UndoHistoryItem TopUndoHistoryItem
		{
			get { return _topUndoHistoryItem; }
			private set { this.SetField(ref _topUndoHistoryItem, value, "TopUndoHistoryItem"); }
		}
		#endregion //TopUndoHistoryItem

		#region UndoLimit
		/// <summary>
		/// Returns or sets the maximum number of items to maintain within the undo/redo history.
		/// </summary>
		[DefaultValue(StackList<UndoHistoryItem>.DefaultMaxCapacity)]
		public int UndoLimit
		{
			get { return _undoStack.MaxCapacity; }
			set
			{
				if (value != this.UndoLimit)
				{
					Debug.Assert(_undoStack.Count == 0 || _redoStack.Count == 0, "The sum of the histories will exceed the max. Do we want to make an arbitrary decision about whether to keep the redo items and some of the undo?");
					_undoStack.MaxCapacity = _redoStack.MaxCapacity = value;
					this.OnPropertyChanged("UndoLimit");
				}
			}
		} 
		#endregion //UndoLimit

		#region UndoHistory
		/// <summary>
		/// Returns a read-only collection of the items in the undo history.
		/// </summary>
		/// <seealso cref="TopUndoHistoryItem"/>
		/// <seealso cref="CanUndo"/>
		/// <seealso cref="RedoHistory"/>
		/// <seealso cref="UndoHistoryItem"/>
		public IList<UndoHistoryItem> UndoHistory
		{
			get { return _undoHistory; }
		} 
		#endregion //UndoHistory

		#region UndoUnitFactory
		/// <summary>
		/// Returns or sets an object that is used to create the default <see cref="UndoUnit"/> instances for this <see cref="UndoManager"/>
		/// </summary>
		/// <see cref="Infragistics.Undo.UndoUnitFactory"/>
		/// <see cref="UndoUnitFactoryResolved"/>
		[DefaultValue(null)]
		public UndoUnitFactory UndoUnitFactory
		{
			get { return _undoUnitFactory; }
			set 
			{
				if (this.SetField(ref _undoUnitFactory, value, "UndoUnitFactory"))
					this.OnPropertyChanged("UndoUnitFactoryResolved");
			}
		}

		/// <summary>
		/// Read-Only property that returns the <see cref="UndoUnitFactory"/> that the UndoManager uses to create the UndoUnit units.
		/// </summary>
		/// <see cref="Infragistics.Undo.UndoUnitFactory"/>
		/// <see cref="UndoUnitFactory"/>
		public UndoUnitFactory UndoUnitFactoryResolved
		{
			get { return _undoUnitFactory ?? UndoUnitFactory.Current; }
		}
		#endregion //UndoUnitFactory

		#endregion //Public Properties

		#region Private Properties

		#region AllowMergingResolved
		private bool AllowMergingResolved
		{
			get { return this.AllowMerging && !this.ShouldPreventMerge; }
		}
		#endregion //AllowMergingResolved

		#region IsMerging
		private bool IsMerging
		{
			get { return this.GetBool(BoolProperties.IsMerging); }
			set { this.SetBool(BoolProperties.IsMerging, value, false); }
		} 
		#endregion //IsMerging

		#region IsPerformingRemoveAll
		private bool IsPerformingRemoveAll
		{
			get { return this.GetBool(BoolProperties.IsPerformingRemoveAll); }
			set { this.SetBool(BoolProperties.IsPerformingRemoveAll, value, false); }
		}
		#endregion //IsPerformingRemoveAll

		#region ShouldPreventMerge
		private bool ShouldPreventMerge
		{
			get { return this.GetBool(BoolProperties.ShouldPreventMerge); }
			set 
			{
				if (this.SetBool(BoolProperties.ShouldPreventMerge, value, false) && value)
				{
					// when merging is disabled we should release the cached redo cached right away
					this.ClearCachedRedoStack();
				}
			}
		}
		#endregion //ShouldPreventMerge

		#region RegisteredUndoManagers
		private static WeakDictionary<object, UndoManager> RegisteredUndoManagers
		{
			get
			{
				if (_registeredUndoManagers == null)
					_registeredUndoManagers = new WeakDictionary<object, UndoManager>(true, false);

				return _registeredUndoManagers;
			}
		}
		#endregion //RegisteredUndoManagers

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region AddChange
		/// <summary>
		/// Adds an <see cref="UndoUnit"/> to the history.
		/// </summary>
		/// <param name="unit">The unit to add to the history.</param>
		/// <exception cref="ArgumentNullException">The 'unit' cannot be null.</exception>
		/// <remarks>
		/// <p class="body">This overload takes the provided <paramref name="unit"/> and adds it to either 
		/// the undo or redo history stack based on the current state of the UndoManager. For example while 
		/// executing an Undo (i.e. <see cref="IsPerformingUndo"/> is true), change units will be added to 
		/// the redo stack.</p>
		/// <p class="note"><b>Note:</b> An undo unit is not automatically moved from the undo stack to the 
		/// redo stack or vice versa. It is expected that either the change that the UndoUnit makes (e.g. a 
		/// property change) will result in a new change being added to the UndoManager or that the unit itself 
		/// will add a unit (even itself) to the UndoManager. The <see cref="AddChange(string, string, Action,Action, object)"/> is one 
		/// such example of the latter. The UndoUnit created by that overload will add itself back into the 
		/// UndoManager if the associated undo or redo delegate is successful.</p>
		/// </remarks>
		public void AddChange(UndoUnit unit)
		{
			CoreUtilities.ValidateNotNull(unit, "unit");

			this.AddChangeHelper(unit);
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> to the history.
		/// </summary>
		/// <param name="description">The general description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <param name="undoMethod">The method to invoke when an Undo is being performed</param>
		/// <param name="redoMethod">The method to invoke when an Redo is being performed</param>
		/// <param name="target">An optional parameter that represents the object that will be affected by the operation.</param>
		/// <remarks>
		/// <p class="body">This overload creates an undo unit that will execute the <paramref name="undoMethod"/> when an Undo operation 
		/// is being performed and will execute the <paramref name="redoMethod"/> when a Redo operation is being performed. If the operation 
		/// is successful (i.e. the method returns true) the UndoUnit will add itself back into the history.</p>
		/// <p class="note"><b>Note:</b> Since an explicit description is passed in, the string should be applicable to both undo and redo operations. For example 
		/// one might use "Bold" or "Toggle Bold" instead of "Enable Bold"/"Disable Bold" since the latter might not be correct if the change is being created for the redo stack. If you do want to 
		/// use a string specific to the context then you might decide which description to provide by evaluating the <see cref="IsPerformingRedo"/> property.</p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The 'undoMethod' and 'redoMethod' parameters cannot be null.</exception>
		public UndoUnit AddChange(string description, string detailedDescription, Func<UndoExecuteContext, bool> undoMethod, Func<UndoExecuteContext, bool> redoMethod, object target = null)
		{
			if (this.IsSuspended)
				return null;

			CoreUtilities.ValidateNotNull(undoMethod, "undoMethod");
			CoreUtilities.ValidateNotNull(redoMethod, "redoMethod");

			var unit = this.UndoUnitFactoryResolved.CreateChange(description, detailedDescription, undoMethod, redoMethod, target);
			return this.AddChangeHelper(unit);
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> to the history.
		/// </summary>
		/// <param name="description">The general description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <param name="undoMethod">The method to invoke when an Undo is being performed</param>
		/// <param name="redoMethod">The method to invoke when an Redo is being performed</param>
		/// <param name="target">An optional parameter that represents the object that will be affected by the operation.</param>
		/// <remarks>
		/// <p class="body">This overload creates an undo unit that will execute the <paramref name="undoMethod"/> when an Undo operation 
		/// is being performed and will execute the <paramref name="redoMethod"/> when a Redo operation is being performed. If the operation 
		/// is successful (i.e. the method returns true) the UndoUnit will add itself back into the history.</p>
		/// <p class="note"><b>Note:</b> Since an explicit description is passed in, the string should be applicable to both undo and redo operatoins. For example 
		/// one might use "Bold" or "Toggle Bold" instead of "Enable Bold"/"Disable Bold" since the latter might not be correct if the change is being created for the redo stack. If you do want to 
		/// use a string specific to the context then you might decide which description to provide by evaluating the <see cref="IsPerformingRedo"/> property.</p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The 'undoMethod' and 'redoMethod' parameters cannot be null.</exception>
		public UndoUnit AddChange(string description, string detailedDescription, Func<bool> undoMethod, Func<bool> redoMethod, object target = null)
		{
			if (this.IsSuspended)
				return null;

			CoreUtilities.ValidateNotNull(undoMethod, "undoMethod");
			CoreUtilities.ValidateNotNull(redoMethod, "redoMethod");

			return this.AddChange(description, detailedDescription, (c) => undoMethod(), (c) => redoMethod(), target);
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> to the history.
		/// </summary>
		/// <param name="description">The general description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <param name="undoMethod">The method to invoke when an Undo is being performed</param>
		/// <param name="redoMethod">The method to invoke when an Redo is being performed</param>
		/// <param name="target">An optional parameter that represents the object that will be affected by the operation.</param>
		/// <remarks>
		/// <p class="body">This overload creates an undo unit that will execute the <paramref name="undoMethod"/> when an Undo operation 
		/// is being performed and will execute the <paramref name="redoMethod"/> when a Redo operation is being performed.</p>
		/// <p class="note"><b>Note:</b> Since an explicit description is passed in, the string should be applicable to both undo and redo operatoins. For example 
		/// one might use "Bold" or "Toggle Bold" instead of "Enable Bold"/"Disable Bold" since the latter might not be correct if the change is being created for the redo stack. If you do want to 
		/// use a string specific to the context then you might decide which description to provide by evaluating the <see cref="IsPerformingRedo"/> property.</p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The 'undoMethod' and 'redoMethod' parameters cannot be null.</exception>
		public UndoUnit AddChange(string description, string detailedDescription, Action undoMethod, Action redoMethod, object target = null)
		{
			if (this.IsSuspended)
				return null;

			CoreUtilities.ValidateNotNull(undoMethod, "undoMethod");
			CoreUtilities.ValidateNotNull(redoMethod, "redoMethod");

			return this.AddChange(description, detailedDescription, (c) => { undoMethod(); return true; }, (c) => { redoMethod(); return true; }, target);
		}

		#endregion //AddChange

		#region AddCollectionChange
		/// <summary>
		/// Adds an <see cref="UndoUnit"/> for the specified collection change to the undo history.
		/// </summary>
		/// <typeparam name="TItem">The type of the item </typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="changeArgs">An object that describes the changes made to the collection</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddCollectionChange<TItem>(ICollection<TItem> collection, NotifyCollectionChangedEventArgs changeArgs, string itemTypeDisplayName = null)
		{
			// don't bother creating the undo entity if we're suspended
			if (this.IsSuspended)
				return null;

			return this.AddChangeHelper(this.UndoUnitFactoryResolved.CreateCollectionChange(collection, changeArgs, itemTypeDisplayName));
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> for the specified add/remove collection change to the undo history.
		/// </summary>
		/// <typeparam name="TItem">The type of the item </typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="action">Either 'Add' or 'Remove' to indicate the type of operation that occurred</param>
		/// <param name="changedItem">The item that was added or removed</param>
		/// <param name="index">The index of the new item for an Add operation or the index where the item existed for a Remove operation.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddCollectionChange<TItem>(ICollection<TItem> collection, NotifyCollectionChangedAction action, TItem changedItem, int index, string itemTypeDisplayName = null)
		{
			if (this.IsSuspended)
				return null;

			return this.AddChangeHelper(this.UndoUnitFactoryResolved.CreateCollectionChange(collection, action, changedItem, index, itemTypeDisplayName));
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> for the specified add/remove collection change to the undo history.
		/// </summary>
		/// <typeparam name="TItem">The type of the item </typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="action">Either 'Add' or 'Remove' to indicate the type of operation that occurred</param>
		/// <param name="items">The items that were added or removed</param>
		/// <param name="index">The index of the new item for an Add operation or the index where the item existed for a Remove operation.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddCollectionChange<TItem>(ICollection<TItem> collection, NotifyCollectionChangedAction action, TItem[] items, int index, string itemTypeDisplayName = null)
		{
			if (this.IsSuspended)
				return null;

			return this.AddChangeHelper(this.UndoUnitFactoryResolved.CreateCollectionChange(collection, action, items, index, itemTypeDisplayName));
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> to the undo history for a collection change that replaces the contents of the collection.
		/// </summary>
		/// <typeparam name="TItem">The type of the item </typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="items">The items to restore when the operation is undone</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddCollectionChange<TItem>(ICollection<TItem> collection, TItem[] items, string itemTypeDisplayName = null)
		{
			if (this.IsSuspended)
				return null;

			return this.AddChangeHelper(this.UndoUnitFactoryResolved.CreateCollectionChange(collection, items, itemTypeDisplayName));
		}

		/// <summary>
		/// Adds an <see cref="UndoUnit"/> for the specified replace collection change to the undo history.
		/// </summary>
		/// <typeparam name="TItem">The type of the item </typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="oldItem">The item that was replaced</param>
		/// <param name="newItem">The item that was the replacement for <paramref name="oldItem"/></param>
		/// <param name="index">The index of the item that was replaced.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddCollectionChange<TItem>(ICollection<TItem> collection, TItem oldItem, TItem newItem, int index, string itemTypeDisplayName = null)
		{
			if (this.IsSuspended)
				return null;

			return this.AddChangeHelper(this.UndoUnitFactoryResolved.CreateCollectionChange(collection, oldItem, newItem, index, itemTypeDisplayName));
		}


		/// <summary>
		/// Adds an <see cref="UndoUnit"/> for the specified replace collection change to the undo history.
		/// </summary>
		/// <typeparam name="TItem">The type of the item </typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="item">The item that was moved.</param>
		/// <param name="oldIndex">The previous index of the item.</param>
		/// <param name="newIndex">The new index of the item.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddCollectionChange<TItem>(ICollection<TItem> collection, TItem item, int oldIndex, int newIndex, string itemTypeDisplayName = null)
		{
			if (this.IsSuspended)
				return null;

			return this.AddChangeHelper(this.UndoUnitFactoryResolved.CreateCollectionChange(collection, item, oldIndex, newIndex, itemTypeDisplayName));
		}

		#endregion //AddCollectionChange

		#region AddPropertyChange

		/// <summary>
		/// Adds an <see cref="PropertyChangeUndoUnitBase"/> for the specified property value change to the undo history.
		/// </summary>
		/// <typeparam name="TOwner">The type of class whose value was changed</typeparam>
		/// <typeparam name="TProperty">The type of the property that was changed</typeparam>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="getter">An expression for the property of the <typeparamref name="TOwner"/> that was changed</param>
		/// <param name="oldValue">The old value of the property that should be restored when the action is undone.</param>
		/// <param name="newValue">The new value of the property</param>
		/// <param name="preventMerge">Used to determine if the property change should be prevented from being merged with the top entry on the undo stack when merging is allowed.</param>
		/// <param name="propertyDisplayName">The name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <seealso cref="Infragistics.Undo.UndoUnitFactory"/>
		/// <seealso cref="UndoUnitFactory"/>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddPropertyChange<TOwner, TProperty>(TOwner owner, Expression<Func<TProperty>> getter, TProperty oldValue, TProperty newValue, bool? preventMerge = null, string propertyDisplayName = null, string typeDisplayName = null)
			where TOwner : class
		{
			if (!ShouldAddPropertyChange(oldValue, newValue))
				return null;

			return this.AddPropertyChangeHelper<TProperty>(this.UndoUnitFactoryResolved.CreatePropertyChange(owner, getter, oldValue, newValue, propertyDisplayName, typeDisplayName), preventMerge);
		}

		/// <summary>
		/// Adds an <see cref="PropertyChangeUndoUnitBase"/> for the specified property value change to the undo history.
		/// </summary>
		/// <typeparam name="TOwner">The type of class whose value was changed</typeparam>
		/// <typeparam name="TProperty">The type of the property that was changed</typeparam>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="getter">An expression for the property that was changed</param>
		/// <param name="oldValue">The old value of the property that should be restored when the action is undone.</param>
		/// <param name="newValue">The new value of the property</param>
		/// <param name="preventMerge">Used to determine if the property change should be prevented from being merged with the top entry on the undo stack when merging is allowed.</param>
		/// <param name="propertyDisplayName">The name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <seealso cref="Infragistics.Undo.UndoUnitFactory"/>
		/// <seealso cref="UndoUnitFactory"/>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddPropertyChange<TOwner, TProperty>(TOwner owner, Expression<Func<TOwner, TProperty>> getter, TProperty oldValue, TProperty newValue, bool? preventMerge = null, string propertyDisplayName = null, string typeDisplayName = null)
			where TOwner : class
		{
			if (!ShouldAddPropertyChange(oldValue, newValue))
				return null;

			return this.AddPropertyChangeHelper<TProperty>(this.UndoUnitFactoryResolved.CreatePropertyChange(owner, getter, oldValue, newValue, propertyDisplayName, typeDisplayName), preventMerge);
		}

		/// <summary>
		/// Adds an <see cref="PropertyChangeUndoUnitBase"/> for the specified property value change to the undo history.
		/// </summary>
		/// <typeparam name="TOwner">The type of class whose value was changed</typeparam>
		/// <typeparam name="TProperty">The type of the property that was changed</typeparam>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="propertyName">The string name of the public property that was changed. This is used to find the PropertyInfo for the property to be affected when the operation is undone.</param>
		/// <param name="oldValue">The old value of the property that should be restored when the action is undone.</param>
		/// <param name="newValue">The new value of the property</param>
		/// <param name="preventMerge">Used to determine if the property change should be prevented from being merged with the top entry on the undo stack when merging is allowed.</param>
		/// <param name="propertyDisplayName">The name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <seealso cref="Infragistics.Undo.UndoUnitFactory"/>
		/// <seealso cref="UndoUnitFactory"/>
		/// <returns>Returns the UndoUnit that was added or null if one was not added</returns>
		public UndoUnit AddPropertyChange<TOwner, TProperty>(TOwner owner, string propertyName, TProperty oldValue, TProperty newValue, bool? preventMerge = null, string propertyDisplayName = null, string typeDisplayName = null)
			where TOwner : class
		{
			if (!ShouldAddPropertyChange(oldValue, newValue))
				return null;

			return this.AddPropertyChangeHelper<TProperty>(this.UndoUnitFactoryResolved.CreatePropertyChange(owner, propertyName, oldValue, newValue, propertyDisplayName, typeDisplayName), preventMerge);
		}
		#endregion //AddPropertyChange

		#region ClearHistory
		/// <summary>
		/// Clears the undo and redo history.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This method does not affect the current open <see cref="RootTransaction"/> if there is one. If you 
		/// want to stop that transaction you would invoke its <see cref="UndoTransaction.Commit"/> or <see cref="UndoTransaction.Rollback"/> 
		/// method.</p>
		/// </remarks>
		public void ClearHistory()
		{
			this.VerifyCanAffectHistory();

			this.ClearCachedRedoStack();

			_undoStack.Clear();
			_redoStack.Clear();
		}
		#endregion //ClearHistory

		#region ExecuteInTransaction
		/// <summary>
		/// Executes the specified action within an <see cref="UndoTransaction"/>
		/// </summary>
		/// <param name="description">The description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <param name="action">The action to be invoked within the transaction</param>
		public void ExecuteInTransaction(string description, string detailedDescription, Action action)
		{
			CoreUtilities.ValidateNotNull(action, "action");

			bool wasCompleted = false;

			var transaction = this.StartTransaction(description, detailedDescription);
			try
			{
				action();

				// if we got here then the action completed and we don't need to call rollback
				wasCompleted = true;
				transaction.Commit();
			}
			finally
			{
				if (!wasCompleted && !transaction.IsClosed)
					transaction.Rollback();
			}
		}
		#endregion //ExecuteInTransaction

		#region ExecuteWhileSuspended
		
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		#endregion //ExecuteWhileSuspended

		#region ForEach
		/// <summary>
		/// Performs the specified action on each <see cref="UndoUnit"/> within the undo and redo history.
		/// </summary>
		/// <param name="action">The action to invoke for each item.</param>
		/// <remarks>
		/// <p class="body">The ForEach method will invoke the specified action on all items in the <see cref="UndoHistory"/>, 
		/// <see cref="RedoHistory"/> as well as <see cref="RootTransaction"/> if there is one currently open.</p>
		/// </remarks>
		public void ForEach(Action<UndoUnit> action)
		{
			CoreUtilities.ValidateNotNull(action, "action");

			ForEach(action, this.UndoHistory);
			ForEach(action, this.RedoHistory);

			// we could have a cached redo stack in which case we should 
			// enumerate that as well since we could end up restoring it
			if (null != _cachedRedoStack)
				ForEach(action, _cachedRedoStack);

			// also invoke the method on the transaction's descendants
			var rootTransaction = _rootTransaction;

			if (null != rootTransaction)
			{
				action(rootTransaction);
				rootTransaction.ForEach(action);
			}
		}

		private static void ForEach(Action<UndoUnit> action, IEnumerable<UndoHistoryItem> history)
		{
			foreach (var historyUnit in history)
			{
				var unit = historyUnit.Unit;

				// call the original action on the unit
				action(unit);

				// pass along the recursive func so we do 
				// that on all descendants as well
				unit.ForEach(action);
			}
		}
		#endregion //ForEach

		#region FromReference
		/// <summary>
		/// Used to obtain the UndoManager instance that has been registered with a given object.
		/// </summary>
		/// <param name="reference">The object that was registered with a given UndoManager using the <seealso cref="RegisterReference(object)"/> method.</param>
		/// <returns>The <see cref="UndoManager"/> that was registered with the specified reference.</returns>
		/// <see cref="UnregisterReference(object)"/>
		/// <see cref="RegisterReference(object)"/>
		public static UndoManager FromReference(object reference)
		{
			return FromReference(reference, false);
		}

		internal static UndoManager FromReference(object reference, bool validate)
		{
			if (reference == null)
				return UndoManager.Current;
			else
			{
				UndoManager manager;

				if (!UndoManager.RegisteredUndoManagers.TryGetValue(reference, out manager) && validate)
					throw new ArgumentException(Utils.GetString("LE_ReferenceNotRegistered", reference));

				return manager;
			}
		}
		#endregion //FromReference

		#region PreventMerge
		/// <summary>
		/// Used to ensure that the next <see cref="UndoUnit"/> that is recorded is not allowed to merge with the last operation on the <see cref="UndoHistory"/>.
		/// </summary>
		public void PreventMerge()
		{
			this.ShouldPreventMerge = true;
		} 
		#endregion //PreventMerge

		#region Redo
		/// <summary>
		/// Performs one or more redo operations from the current history.
		/// </summary>
		/// <param name="redoCount">The number of operations in the redo history to execute</param>
		/// <exception cref="ArgumentOutOfRangeException">The <paramref name="redoCount"/> must be 1 or greater</exception>
		public void Redo(int redoCount = 1)
		{
			PerformUndoRedo(redoCount, false);
		}
		#endregion //Redo

		#region RegisterReference
		/// <summary>
		/// Used to associate an object with a given <see cref="UndoManager"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">Registering an object with an UndoManager allows a reference to the object to be passed to static 
		/// methods that are used to obtain the UndoManager with which an undo operation should be registered. This allows an 
		/// object to not have to store a reference to the UndoManager directly.</p>
		/// </remarks>
		/// <param name="reference">The object with which to associate this UndoManager instance</param>
		/// <see cref="UnregisterReference(object)"/>
		/// <see cref="FromReference(object)"/>
		public void RegisterReference(object reference)
		{
			if (reference is UndoManager)
				throw new ArgumentException(Utils.GetString("LE_UndoManagerAsReference"), "reference");

			var registered = UndoManager.RegisteredUndoManagers;

			UndoManager oldManager;

			if (registered.TryGetValue(reference, out oldManager))
			{
				if (oldManager == this)
					return;

				throw new ArgumentException(Utils.GetString("LE_ReferenceRegisteredToOther", reference), "reference");
			}

			registered[reference] = this;
		}
		#endregion //RegisterReference

		#region RemoveAll
		/// <summary>
		/// Removes all the matching <see cref="UndoUnit"/> instances from the <see cref="UndoHistory"/> and <see cref="RedoHistory"/> collections.
		/// </summary>
		/// <param name="match">The delegate to invoke with each <see cref="UndoUnit"/> to determine if it should be removed.</param>
		/// <param name="includeRootTransaction">Indicates whether the method should affect the current open <see cref="RootTransaction"/> if there is one.</param>
		public void RemoveAll(Func<UndoUnit, bool> match, bool includeRootTransaction = true)
		{
			CoreUtilities.ValidateNotNull(match, "match");

			this.VerifyCanAffectHistory();

			// break up the declaration and assignment to allow recursion
			Func<UndoUnit, bool> unitMatch = null;
			unitMatch = (u) =>
			 {
				 // remove the unit if it matches the original match criteria
				 if (match(u))
					 return true;

				 // also remove it if the removal of the descendants warrants it
				 return u.RemoveAll(unitMatch);
			 };

			Func<UndoHistoryItem, bool> historyItemMatch = (o) =>
			{
				// the history is a collection of UndoHistoryItems but we're evaluating the units (recursively)
				return unitMatch(o.Unit);
			};

			bool wasChanging = this.IsPerformingRemoveAll;
			this.IsPerformingRemoveAll = true;

			try
			{
				if (includeRootTransaction && _rootTransaction != null)
					unitMatch(_rootTransaction);

				var topItem = _undoStack.Count == 0 ? null : _undoStack.Peek();

				_undoStack.RemoveAll(historyItemMatch);

				// if the undo stack is manipulated such that the most recent item is removed then 
				// we should remove the cached redo stack
				if (topItem != null && (_undoStack.Count == 0 || _undoStack.Peek() != topItem))
					this.ClearCachedRedoStack();

				// clean up the cached redo stack as well
				var cachedRedoStack = _cachedRedoStack;

				if (cachedRedoStack != null)
				{
					var redo = cachedRedoStack.ToArray();

					// enumerate it backwards (most recent) like we would 
					// have from the stack even though the order is not 
					// documented/required
					int removeCount = 0;
					for (int i = redo.Length - 1; i >= 0; i--)
					{
						if (unitMatch(redo[i].Unit))
						{
							redo[i] = null;
							removeCount++;
						}
					}

					Debug.Assert(_cachedRedoStack == cachedRedoStack, "Stack changed while removing?");

					if (_cachedRedoStack == cachedRedoStack)
					{
						if (removeCount == redo.Length)
							this.ClearCachedRedoStack();
						else if (removeCount > 0)
						{
							cachedRedoStack = new Stack<UndoHistoryItem>();

							// keep the non-null (i.e. non-removed items)
							for (int i = redo.Length - 1; i >= 0; i--)
							{
								var item = redo[i];

								if (item != null)
									cachedRedoStack.Push(item);
							}

							_cachedRedoStack = cachedRedoStack;
						}
					}
				}

				_redoStack.RemoveAll(historyItemMatch);

			}
			finally
			{
				this.IsPerformingRemoveAll = wasChanging;
			}
		}
		#endregion //RemoveAll

		#region Resume
		/// <summary>
		/// Resumes the recording of <see cref="UndoUnit"/> instances in the history.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="Suspend"/> method is meant to be used to temporarily prevent recording of undo operations. This 
		/// can be helpful when initializing objects/collections before they will be manipulated by the end user. The Resume method must 
		/// be called to remove the suspension and must be called an equal number of times that the Suspend method is invoked.</p>
		/// <p class="note">While suspended, calls to <see cref="AddChange(UndoUnit)"/> will be ignored. Also any calls to <see cref="Undo(int)"/> 
		/// or <see cref="Redo(int)"/> will result in an exception.</p>
		/// </remarks>
		/// <seealso cref="Suspend"/>
		/// <seealso cref="IsSuspended"/>
		public void Resume()
		{
			_suspendCount--;

			if (_suspendCount == 0)
				this.OnPropertyChanged("IsSuspended");
		} 
		#endregion //Resume

		#region StartTransaction
		/// <summary>
		/// Used to start a new transaction that will group one or more <see cref="UndoUnit"/> actions.
		/// </summary>
		/// <param name="description">The general description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <returns>The transaction that was created.</returns>
		/// <remarks>
		/// <p class="body">A transaction is used to group one or more <see cref="UndoUnit"/> instances into a single 
		/// item in the undo/redo history. The StartTranaction method is used to start a new transaction. If no 
		/// transactions have been started or all tranasactions are closed, this will create a new root level 
		/// transaction that is exposed via the <see cref="RootTransaction"/>. If a transaction has already been 
		/// started, and therefore RootTransaction returns non-null, the new transaction will be a nested transaction 
		/// of the leaf open transaction within that <see cref="UndoTransaction"/>. The method returns the newly 
		/// started transaction.</p>
		/// <p class="note"><b>Note:</b> The <see cref="Undo(int)"/> and <see cref="Redo(int)"/> methods cannot be 
		/// invoked while a transaction is open but one can create transactions while performing an undo/redo operation 
		/// to gather the operations into a single unit.</p>
		/// </remarks>
		/// <seealso cref="RootTransaction"/>
		/// <seealso cref="UndoTransaction.OpenTransaction"/>
		/// <seealso cref="Infragistics.Undo.UndoUnitFactory.CreateTransaction(string, string)"/>
		/// <seealso cref="UndoTransaction"/>
		public UndoTransaction StartTransaction(string description, string detailedDescription)
		{
			if (this.IsSuspended)
				throw new InvalidOperationException(Utils.GetString("LE_NewTransactionWhileSuspended"));

			var group = this.UndoUnitFactoryResolved.CreateTransaction(description, detailedDescription);

			if (group == null)
				throw new InvalidOperationException(Utils.GetString("LE_FactoryNullTransaction"));

			var leafGroup = this.CurrentTransaction;

			if (leafGroup != null)
				leafGroup.Add(group);

			var parent = leafGroup == null ? (IUndoTransactionOwner)this : (IUndoTransactionOwner)leafGroup;
			group.Open(parent);
			return group;
		}
		#endregion //StartTransaction

		#region Suspend
		/// <summary>
		/// Suspends the recording of <see cref="UndoUnit"/> instances in the history.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Suspend method is meant to be used to temporarily prevent recording of undo operations. This 
		/// can be helpful when initializing objects/collections before they will be manipulated by the end user. The <see cref="Resume"/> 
		/// method must be called to remove the suspension and must be called an equal number of times that the Suspend method is invoked.</p>
		/// <p class="note">While suspended, calls to <see cref="AddChange(UndoUnit)"/> will be ignored. Also any calls to <see cref="Undo(int)"/> 
		/// or <see cref="Redo(int)"/> will result in an exception.</p>
		/// </remarks>
		/// <seealso cref="Resume()"/>
		/// <seealso cref="IsSuspended"/>
		public void Suspend()
		{
			_suspendCount++;

			if (_suspendCount == 1)
				this.OnPropertyChanged("IsSuspended");
		} 
		#endregion //Suspend

		#region UnregisterReference
		/// <summary>
		/// Used to remove an registration for an object that was registered with this UndoManager instance via the <see cref="RegisterReference(object)"/> method.
		/// </summary>
		/// <param name="reference">The object with that was previously registered with this UndoManager instance</param>
		/// <see cref="RegisterReference(object)"/>
		/// <see cref="FromReference(object)"/>
		public void UnregisterReference(object reference)
		{
			var registered = UndoManager.RegisteredUndoManagers;

			UndoManager oldManager;

			if (!registered.TryGetValue(reference, out oldManager))
				throw new ArgumentException(Utils.GetString("LE_ReferenceNotRegistered"), "reference");

			if (oldManager != this)
				throw new ArgumentException(Utils.GetString("LE_ReferenceRegisteredToOther"), "reference");

			registered.Remove(reference);
		}
		#endregion //UnregisterReference

		#region Undo
		/// <summary>
		/// Performs one or more undo operations from the current history.
		/// </summary>
		/// <param name="undoCount">The number of operations in the undo history to execute</param>
		/// <exception cref="ArgumentOutOfRangeException">The <paramref name="undoCount"/> must be 1 or greater</exception>
		public void Undo(int undoCount = 1)
		{
			PerformUndoRedo(undoCount, true);
		}
		#endregion //Undo

		#endregion //Public Methods

		#region Internal Methods

		#region GetDefaultPropertyChangeDescription
		internal static string GetDefaultPropertyChangeDescription<TProperty>(object owner, TProperty oldValue, TProperty newValue, string memberName, UndoHistoryItemType itemType, bool detailed)
		{
			string description = null;

			if (!string.IsNullOrEmpty(memberName))
			{
				string resourceName = detailed ? "PropertyChangeDescriptionDetailed" : "PropertyChangeDescription";
				TProperty from = itemType == UndoHistoryItemType.Redo ? newValue : oldValue;
				TProperty to = itemType == UndoHistoryItemType.Redo ? oldValue : newValue;

				description = Utils.GetString(resourceName, memberName, owner, from, to);
			}

			return description;
		}
		#endregion //GetDefaultPropertyChangeDescription

		#region OnTransactionOpenedOrClosed
		internal void OnTransactionOpenedOrClosed(UndoTransaction child)
		{
			_currentTransaction = null;
			this.OnPropertyChanged("CurrentTransaction");
		}
		#endregion //OnTransactionOpenedOrClosed

		#region PerformUndoRedo
		internal void PerformUndoRedo(int count, bool isUndo)
		{
			if (count <= 0)
				throw new ArgumentOutOfRangeException("count", Utils.GetString("LE_ArgumentIsNegative", "count", count));

			if (this.IsPerformingRedo || this.IsPerformingUndo)
				throw new InvalidOperationException(Utils.GetString("LE_UndoRedoInUndoRedo"));

			if (this.IsPerformingRollback)
				throw new InvalidOperationException(Utils.GetString("LE_UndoRedoInRollback"));

			if (IsSuspended)
				throw new InvalidOperationException(Utils.GetString("LE_UndoRedoWhileSuspended"));

			if (_rootTransaction != null)
				throw new InvalidOperationException(Utils.GetString("LE_UndoRedoInTransaction"));

			this.VerifyCanAffectHistory();

			// if there is nothing to undo/redo then bail
			if (!this.GetBool(isUndo ? BoolProperties.CanUndo : BoolProperties.CanRedo))
				return;

			var isPerformingFlag = isUndo ? BoolProperties.IsPerformingUndo : BoolProperties.IsPerformingRedo;
			var reason = isUndo ? UndoExecuteReason.Undo : UndoExecuteReason.Redo;

			try
			{
				this.SetBool(isPerformingFlag, true);

				this.ClearCachedRedoStack();

				// get the appropriate stack
				var history = isUndo ? _undoStack : _redoStack;
				var executeInfo = new UndoExecuteContext(this, reason);

				// once we do an undo/redo operation we should not allow merging of undo units
				if (history.Count > 0)
					this.ShouldPreventMerge = true;

				for (int i = 0, max = Math.Min(count, history.Count); i < max; i++)
				{
					var item = history.Pop();

					executeInfo.Execute(item.Unit);
				}
			}
			finally
			{
				this.SetBool(isPerformingFlag, false);
			}
		}
		#endregion //PerformUndoRedo

		#region Rollback
		internal void Rollback(UndoUnit unit)
		{
			bool wasRollingBack = this.IsPerformingRollback;

			try
			{
				this.IsPerformingRollback = true;

				var executeInfo = new UndoExecuteContext(this, UndoExecuteReason.Rollback);

				executeInfo.Execute(unit);
			}
			finally
			{
				this.IsPerformingRollback = wasRollingBack;
			}
		}
		#endregion //Rollback

		#region VerifyCanClose
		internal void VerifyCanClose(UndoTransaction undoTransaction)
		{
			if (this.IsSuspended)
				throw new InvalidOperationException(Utils.GetString("LE_EndTransactionWhileSuspended"));
		}
		#endregion //VerifyCanClose

		#endregion //Internal Methods
		
		#region Private Methods

		#region AddChangeHelper
		private UndoUnit AddChangeHelper(UndoUnit unit, bool allowTransaction = false)
		{
			#region Setup

			if (unit == null)
				return null;

			// any changes recorded as part of a rollback should be ignored
			if (this.IsPerformingRollback)
				return null;

			if (IsSuspended)
			{
				Debug.Assert(!this.IsPerformingRedo && !this.IsPerformingUndo, "Changes were suspended while an Undo/Redo was in progress?");
				return null;
			}

			this.VerifyCanAffectHistory();

			var group = unit as UndoTransaction;

			if (group != null)
			{
				if (group.OpenState != false)
					throw new ArgumentException(Utils.GetString("LE_AddOpenTransaction"));

				if (!allowTransaction)
					throw new ArgumentException(Utils.GetString("LE_AddTransactionDirect"));
			} 
			#endregion //Setup

			// while a transaction is open we just store the units in the leaf transaction
			if (_rootTransaction != null)
			{
				var leaf = this.CurrentTransaction;
				var isUndoOrRedo = this.IsPerformingRedo || this.IsPerformingUndo;

				if (!isUndoOrRedo)
				{
					var mergeResult = this.Merge(leaf, unit);

					if (mergeResult != UndoMergeAction.NotMerged)
					{
						if (mergeResult == UndoMergeAction.MergedRemoveUnit)
						{
							Debug.Assert(leaf.IsClosed == false, "The transaction was closed?");

							// we don't return this from a transaction but if something 
							// does then we should probably honor it. since it was 
							// merged we shouldn't roll it back. we'll just cancel it 
							// so it is removed 
							leaf.Cancel();
						}

						return null;
					}

					// as occurs with a non-transactional add after we add a unit we 
					// should clear the prevent merge flag since we're successfully 
					// adding to the transaction
					this.ShouldPreventMerge = false;
				}

				leaf.Add(unit);

				if (!isUndoOrRedo)
					this.ResetLastNewOperationTime();
			}
			else
			{
				if (this.IsPerformingUndo)
				{
					// if we try to perform an undo and we had a cached redo stack then release it now
					this.ClearCachedRedoStack();

					_redoStack.Push(new UndoHistoryItem(unit, UndoHistoryItemType.Redo, this));
				}
				else
				{
					bool addToUndo = true;
					bool isRedoing = this.IsPerformingRedo;

					if (!isRedoing && _undoStack.Count > 0)
					{
						var item = _undoStack.Peek();
						var mergeResult = this.Merge(item.Unit, unit);

						#region Process Merge
						if (mergeResult != UndoMergeAction.NotMerged)
						{
							// the mergedelay will be the time between mergings and not the 
							// time that the history item was originally created
							this.ResetLastNewOperationTime();

							addToUndo = false;

							if (mergeResult == UndoMergeAction.Merged)
							{
								item.ResetDescription();
							}
							else
							{
								Debug.Assert(mergeResult == UndoMergeAction.MergedRemoveUnit, "Unexpected result");

								var oldItem = _undoStack.Pop();

								Debug.Assert(oldItem == item, "Different item popped off the stack?");

								// restore the redo stack to what it was when the unit was added
								this.RestoreCachedRedoStack();

								this.PreventMerge();
							}
						}
						#endregion //Process Merge
					}

					// we don't need a cached redo stack any more
					this.ClearCachedRedoStack();

					if (addToUndo)
					{
						// if this is a new operation then we can allow subsequent merging
						if (!isRedoing)
						{
							// temporarily cache the redo stack
							this.CacheRedoStack();

							// as typical undo implementations do, we lose the redo stack when 
							// a new operation is added to the history
							_redoStack.Clear();

							this.ShouldPreventMerge = false;
						}

						// then add a new item to the history
						var historyItem = new UndoHistoryItem(unit, UndoHistoryItemType.Undo, this);

						_undoStack.Push(historyItem);

						if (!isRedoing)
							this.ResetLastNewOperationTime();
					}
				}
			}

			return unit;
		} 
		#endregion //AddChangeHelper

		#region AddPropertyChangeHelper
		private UndoUnit AddPropertyChangeHelper<TProperty>(UndoUnit unit, bool? preventMerge)
		{
			if (unit == null)
				return null;

			if (preventMerge != false)
				this.PreventPropertyMerge(preventMerge, typeof(TProperty));

			return this.AddChangeHelper(unit);
		}
		#endregion //AddPropertyChangeHelper

		#region CacheRedoStack
		private void CacheRedoStack()
		{
			_cachedRedoStack = new Stack<UndoHistoryItem>();

			foreach (var item in _redoStack)
				_cachedRedoStack.Push(item);
		}
		#endregion //CacheRedoStack

		#region ClearCachedRedoStack
		private void ClearCachedRedoStack()
		{
			_cachedRedoStack = null;
		}
		#endregion //ClearCachedRedoStack

		#region GetBool
		private bool GetBool(BoolProperties property)
		{
			return (_boolFlags & property) == property;
		}
		#endregion //GetBool

		#region Merge
		private UndoMergeAction Merge(UndoUnit previousUnit, UndoUnit unitToMerge)
		{
			var result = UndoMergeAction.NotMerged;

			// cannot attempt merge if there is nothing to merge with or if we are 
			// not supposed to merge items at this time
			if (this.AllowMergingResolved)
			{
				if (_mergeTimeout == TimeSpan.Zero || DateTime.UtcNow.Subtract(_lastNewOperation) <= _mergeTimeout)
				{
					try
					{
						Debug.Assert(this.IsMerging == false, "We should not be a merging state while performing a merge");

						this.IsMerging = true;
						var mergeInfo = new UndoMergeContext(this, unitToMerge);
						result = previousUnit.Merge(mergeInfo);
					}
					finally
					{
						this.IsMerging = false;
					}

					if (result != UndoMergeAction.NotMerged)
					{
						// the mergedelay will be the time between mergings and not the 
						// time that the history item was originally created
						this.ResetLastNewOperationTime();
					}
				}
			}

			return result;
		}
		#endregion //Merge

		#region OnHistoryStackChanged
		private void OnHistoryStackChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.CanUndo = _undoStack.Count > 0;
			this.CanRedo = _redoStack.Count > 0;

			this.TopUndoHistoryItem = this.CanUndo ? _undoStack.Peek() : null;
			this.TopRedoHistoryItem = this.CanRedo ? _redoStack.Peek() : null;
		}
		#endregion //OnHistoryStackChanged 

		#region PreventPropertyMerge
		private void PreventPropertyMerge(bool? preventMerge, Type propertyType)
		{
			if (preventMerge ?? ShouldPreventPropertyMerge(propertyType))
				this.PreventMerge();
		} 
		#endregion //PreventPropertyMerge

		#region ResetLastNewOperationTime
		private void ResetLastNewOperationTime()
		{
			_lastNewOperation = DateTime.UtcNow;
		}
		#endregion //ResetLastNewOperationTime

		#region RestoreCachedRedoStack
		private void RestoreCachedRedoStack()
		{
			if (_cachedRedoStack == null)
				return;

			Debug.Assert(_redoStack.Count == 0, "RedoHistory already has items");

			var redoStack = _cachedRedoStack;
			this.ClearCachedRedoStack();

			foreach (var item in redoStack)
				_redoStack.Push(item);
		}
		#endregion //RestoreCachedRedoStack

		#region SetBool
		private bool SetBool(BoolProperties property, bool value, bool raisePropertyChange = true)
		{
			if (value == GetBool(property))
				return false;

			_boolFlags ^= property;
			this.OnPropertyChanged(property.ToString());
			return true;
		}
		#endregion //SetBool

		#region SetField
		private bool SetField<T>(ref T member, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(member, value))
				return false;

			member = value;
			this.OnPropertyChanged(propertyName);
			return true;
		}
		#endregion //SetField

		#region ShouldAddPropertyChange
		private bool ShouldAddPropertyChange<TProperty>(TProperty oldValue, TProperty newValue)
		{
			// make sure there is a change in the value
			if (EqualityComparer<TProperty>.Default.Equals(oldValue, newValue))
				return false;

			if (this.IsSuspended)
				return false;

			return true;
		}
		#endregion //ShouldAddPropertyChange

		#region ShouldPreventPropertyMerge
		// we can do something like this in the future if we need to. then if we get above
		// a couple of types we can switch to a dictionary if needed
		//private static readonly ICollection<Type> PreventMergePropertyTypes = new Type[] { typeof(bool) };

		private static bool ShouldPreventPropertyMerge(Type propertyType)
		{
			propertyType = CoreUtilities.GetUnderlyingType(propertyType);

			// we could check a collection but right now we only have 1 type
			//return PreventMergePropertyTypes.Contains(propertyType);
			return propertyType == typeof(bool);
		}
		#endregion //ShouldPreventPropertyMerge

		#region VerifyCanAffectHistory
		private void VerifyCanAffectHistory()
		{
			if (this.IsMerging)
				throw new InvalidOperationException(Utils.GetString("LE_ChangeHistoryInMerge"));

			if (this.IsPerformingRemoveAll)
				throw new InvalidOperationException(Utils.GetString("LE_ChangeHistoryInRemoveAll"));
		}
		#endregion //VerifyCanAffectHistory

		#endregion //Private Methods

		#endregion //Methods

		#region IUndoTransactionOwner Members
		UndoManager IUndoTransactionOwner.UndoManager
		{
			get { return this; }
		}

		bool IUndoTransactionOwner.OnChildOpened(UndoTransaction child)
		{
			CoreUtilities.ValidateNotNull(child, "child");

			// throw if another child is opened
			if (_rootTransaction != null)
				throw new InvalidOperationException(Utils.GetString("LE_HasOpenTransaction"));

			if (child.OpenState != true)
				throw new ArgumentException(Utils.GetString("LE_TransactionNotOpened", child));

			if (child.Owner != this)
				throw new ArgumentException(Utils.GetString("LE_InvalidTransactionOwner"));

			this.RootTransaction = child;
			return true;
		}

		void IUndoTransactionOwner.OnChildClosed(UndoTransaction child, UndoTransactionCloseAction closeAction)
		{
			CoreUtilities.ValidateNotNull(child, "child");

			if (child != _rootTransaction)
				throw new ArgumentException(Utils.GetString("LE_ClosingOtherTransaction", child, _rootTransaction));

			this.RootTransaction = null;

			if (closeAction == UndoTransactionCloseAction.Commit && child.ContainsNonTransactionUnits())
				this.AddChangeHelper(child, true);
		}
		#endregion //IUndoTransactionOwner Members

		#region BoolProperties enum
		[Flags]
		private enum BoolProperties
		{
			// Note: The enum names must match the name of public boolean properties
			CanUndo = 1 << 0,
			CanRedo = 1 << 1,
			IsPerformingRedo = 1 << 2,
			IsPerformingUndo = 1 << 3,
			ShouldPreventMerge = 1 << 4,
			AllowMerging = 1 << 5,
			IsMerging = 1 << 6,
			IsPerformingRollback = 1 << 7,
			IsPerformingRemoveAll = 1 << 8,
		} 
		#endregion //BoolProperties enum
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