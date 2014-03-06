using System;
using System.Collections ;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace Infragistics.Windows.Commands
{
	#region Class CommandWrapper

	/// <summary>
	/// Class that wraps a RoutedCommand or a RoutedCommand derived class (e.g., RoutedUICommand) and
	/// stores information about the control states that are Disallowed and Required
	/// for the command to execute.
	/// </summary>
	/// <seealso cref="ICommandHost"/>
	public class CommandWrapper
	{
		#region Member Variables

		private RoutedCommand			_command ;
		private Int64					_stateDisallowed; // = 0;
		private Int64					_stateRequired; // = 0;
		private bool					_isEnabled = true;
		private InputGestureCollection	_inputGestures;
		private ModifierKeys			_modifierKeysDisallowed = ModifierKeys.None;

		#endregion Member Variables

		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		public CommandWrapper(RoutedCommand command)
            // AS 9/8/08 TFS7171
            : this(command, 0, 0)
		{
            // AS 9/8/08 TFS7171
            //this._command	= command;
		}

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="stateDisallowed">Bit flags specifying the state that would prevent this command from being active.</param>
		/// <param name="stateRequired">Bit flags specifying the state required for this command to be active.</param>
		public CommandWrapper(RoutedCommand command, Int64 stateDisallowed, Int64 stateRequired)
            // AS 9/8/08 TFS7171
            : this(command, stateDisallowed, stateRequired, (InputGestureCollection)null)
        {
            // AS 9/8/08 TFS7171
            //this._command			= command;
			//this._stateDisallowed	= stateDisallowed;
			//this._stateRequired		= stateRequired;
		}

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="stateDisallowed">Bit flags specifying the state that would prevent this command from being active.</param>
		/// <param name="stateRequired">Bit flags specifying the state required for this command to be active.</param>
		/// <param name="inputGestures">The input gesture to trigger this command</param>
		public CommandWrapper( RoutedCommand command, Int64 stateDisallowed, Int64 stateRequired, InputGestureCollection inputGestures )
			: this( command, stateDisallowed, stateRequired, inputGestures, ModifierKeys.None )
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to wrap.</param>
		/// <param name="stateDisallowed">Bit flags specifying the state that would prevent this command from being active.</param>
		/// <param name="stateRequired">Bit flags specifying the state required for this command to be active.</param>
		/// <param name="inputGesture">The input gesture to trigger this command</param>
		public CommandWrapper( RoutedCommand command, Int64 stateDisallowed, Int64 stateRequired, InputGesture inputGesture )
			: this( command, stateDisallowed, stateRequired, inputGesture, ModifierKeys.None )
		{
		}		

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="stateDisallowed">Bit flags specifying the state that would prevent this command from being active.</param>
		/// <param name="stateRequired">Bit flags specifying the state required for this command to be active.</param>
		/// <param name="inputGesture">The input gesture to trigger this command</param>
		/// <param name="modifierKeysDisallowed">Disallowed modifier keys</param>
		public CommandWrapper( RoutedCommand command, Int64 stateDisallowed, Int64 stateRequired,
			InputGesture inputGesture, ModifierKeys modifierKeysDisallowed )
			: this( command, stateDisallowed, stateRequired,
					inputGesture == null ? null : new InputGesture[] { inputGesture }, modifierKeysDisallowed )
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="stateDisallowed">Bit flags specifying the state that would prevent this command from being active.</param>
		/// <param name="stateRequired">Bit flags specifying the state required for this command to be active.</param>
		/// <param name="inputGestures">The input gesture to trigger this command</param>
		/// <param name="modifierKeysDisallowed">Disallowed modifier keys</param>
		public CommandWrapper( RoutedCommand command, Int64 stateDisallowed, Int64 stateRequired,
			InputGesture[] inputGestures, ModifierKeys modifierKeysDisallowed )
			: this( command, stateDisallowed, stateRequired,
					inputGestures == null ? null : new InputGestureCollection( inputGestures ), modifierKeysDisallowed )
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="stateDisallowed">Bit flags specifying the state that would prevent this command from being active.</param>
		/// <param name="stateRequired">Bit flags specifying the state required for this command to be active.</param>
		/// <param name="inputGestures">The input gesture to trigger this command</param>
		/// <param name="modifierKeysDisallowed">Disallowed modifier keys</param>
		public CommandWrapper( RoutedCommand command, Int64 stateDisallowed, Int64 stateRequired, 
			InputGestureCollection inputGestures, ModifierKeys modifierKeysDisallowed )
		{
			this._command = command;
			this._stateDisallowed = stateDisallowed;
			this._stateRequired = stateRequired;
			this._inputGestures = inputGestures;
			this._modifierKeysDisallowed = modifierKeysDisallowed;

            
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

            ICommandEx commandEx = command as ICommandEx;

            if (null != commandEx)
            {
                this._stateDisallowed |= commandEx.MinimumStateDisallowed;
                this._stateRequired |= commandEx.MinimumStateRequired;
            }
        }

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="inputGestures">A collection of input gestures that should be associated with the command</param>
		public CommandWrapper(IGRoutedCommand command, InputGestureCollection inputGestures) : 
			this(command, command.MinimumStateDisallowed, command.MinimumStateRequired, inputGestures)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CommandWrapper"/>
		/// </summary>
		/// <param name="command">The command to be represented by the CommandWrapper</param>
		/// <param name="inputGestures">A collection of input gestures that should be associated with the command</param>
		public CommandWrapper(IGRoutedUICommand command, InputGestureCollection inputGestures)
			: this(command, command.MinimumStateDisallowed, command.MinimumStateRequired, inputGestures)
		{
		}

		#endregion //Constructors

		#region Properties

			#region Command

		/// <summary>
		/// The RoutedCommand or RoutedCommand derived class (e.g., RoutedUICommand) being wrapped by
		/// the CommandWrapper.
		/// </summary>
		public RoutedCommand Command
		{
			get { return this._command; }
		}
	
			#endregion //Command

		/// <summary>
		/// Indicates if the <see cref="InputGestures"/> for the class has been allocated and has gestures in it.
		/// </summary>
		public bool HasInputGestures
		{
			get { return this._inputGestures != null && this._inputGestures.Count > 0; }
		}

			#region InputGestures

		/// <summary>
		/// A collection of InputGestures associated with the command being wrapped.
		/// </summary>
		public InputGestureCollection InputGestures
		{
			get
			{
				if (this._inputGestures == null)
					this._inputGestures = new InputGestureCollection();

				return this._inputGestures;
			}
		}

			#endregion //InputGestures

			#region IsEnabled

		/// <summary>
		/// Returns/sets whether the command is enabled.  If set to false, the wrapped command cannot be executed.
		/// </summary>
		public bool IsEnabled
		{
			get { return this._isEnabled; }
			set { this._isEnabled = value; }
		}

			#endregion //IsEnabled

			#region ModifierKeysDisallowed

		/// <summary>
		/// Returns the disallowed state. These are bit flags that specify
		/// the state that the control MUST NOT be in for this command to be
		/// executed. If the current state of the control has any of these 
		/// bits turned on this command will be ignored.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> Use <b>InputGestures</b> to associate this command wrapper with
		/// the required keys, including required modifier keys.
		/// </para>
		/// </remarks>
		/// <seealso cref="InputGestures"/>
		/// <seealso cref="StateRequired"/>
		/// <seealso cref="StateDisallowed"/>
		/// <seealso cref="IsEnabled"/>
		public ModifierKeys ModifierKeysDisallowed
		{
			get { return this._modifierKeysDisallowed; }
		}

			#endregion //ModifierKeysDisallowed

			#region StateDisallowed

		/// <summary>
		/// Returns the disallowed state. These are bit flags that specify
		/// the state that the control MUST NOT be in for this command to be
		/// executed. If the current state of the control has any of these 
		/// bits turned on this command will be ignored.
		/// </summary>
		/// <seealso cref="StateRequired"/>
		/// <seealso cref="IsEnabled"/>
		public Int64 StateDisallowed
		{
			get { return this._stateDisallowed; }
		}

			#endregion //StateDisallowed

			#region StateRequired

		/// <summary>
		/// Returns the required state. These are bit flags that specify
		/// the state that the control MUST be in for this command to be
		/// executed.
		/// </summary>
		/// <seealso cref="StateDisallowed"/>
		/// <seealso cref="IsEnabled"/>
		public Int64 StateRequired
		{
			get { return this._stateRequired; }
		}

			#endregion //StateRequired

		#endregion //Properties

		#region Methods

			#region IsAllowed

		/// <summary>
		/// Returns true if this command is allowed for the passed in state
		/// </summary>
		/// <param name="state">Bit flags indicating the current state of the control</param>
		/// <returns>True if this command is allowed.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsAllowed(Int64 state)
		{
			// Make sure the command is enabled.
			if (this._isEnabled == false)
				return false;


			// Check for the required state bits.
			if ((state & this.StateRequired) != this.StateRequired)
				return false;


			// Check for the state bits that are disallowed
			if (0 != (state & this.StateDisallowed))
				return false;

			return true;
		}

			#endregion IsAllowed

			#region IsMinimumStatePresent

		/// <summary>
		/// Returns true if there is the minimum amount of state required for execution.  Compares the
		/// wrapped command's MinimumStateDisallowed and MinimumStateRequired properties to the specified
		/// control state to determine if the minimum state is present.
		/// </summary>
		/// <param name="state">Bit flags indicating the current state of the control</param>
		/// <returns>True if this command is allowed.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsMinimumStatePresent(Int64 state)
		{
			// Make sure the command is enabled.
			if (this._isEnabled == false)
				return false;

			return CommandUtilities.IsMinimumStatePresent(state, this.Command);
		}

			#endregion //IsMinimumStatePresent

		#endregion //Methods
	}

	#endregion //Class CommandWrapper
	
	#region ICommandHost Interface

	/// <summary>
	/// Interface implemented by control that support CommandWrappers as a technique for controlling
	/// the execution of a RoutedCommand based on control state.
	/// </summary>
	/// <seealso cref="CommandWrapper"/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICommandHost
	{
		// AS 2/5/08 ExecuteCommandInfo
		//void Execute(ExecutedRoutedEventArgs args);
		/// <summary>
		/// Executes the specified command.
		/// </summary>
		/// <param name="commandInfo">Contains information about the command being executed and the command parameter.</param>
		/// <returns>Returns a boolean indicating whether the command execution was handled.</returns>
		bool Execute(ExecuteCommandInfo commandInfo);

		// SSP 3/18/10 TFS29783 - Optimizations
		// When we need to check if a command is executable, instead of querying the command host to return 
		// the all of the current state (all the state flags), query only the state flags that we are 
		// interested in in order to determine if the command in question can be executable. The flags of
		// interest for a command constitute of the command's disallowed and required states.
		// 
		/// <summary>
		/// Returns the current state of the command host (control).
		/// </summary>
		/// <param name="statesToQuery">States to query.</param>
		/// <returns>Current state of the control.</returns>
		Int64 GetCurrentState( Int64 statesToQuery );
		///// <summary>
		///// Returns the current state of the command host (control).
		///// </summary>
		//Int64	CurrentState { get; }		

		// AS 2/5/08 ExecuteCommandInfo
		///// <param name="command">The command whose CanExecute state is being queried</param>
		///// <param name="commandParameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		//bool CanExecute(RoutedCommand command, object commandParameter);
		/// <summary>
		/// Returns whether the specified standard command is allowed by the Command Host.
		/// </summary>
		/// <param name="commandInfo">Provides information about the command whose CanExecute state is being queried.</param>
		/// <returns>True if the command can be executed or false if the specified command cannot be executed.</returns>
		bool CanExecute(ExecuteCommandInfo commandInfo);
	}

	#endregion //ICommandHost Interface

	#region Commands<T> class

	/// <summary>
	/// An abstract class for defining and managing the global list of available RoutedCommands for a specific component type.
	/// </summary>
	/// <seealso cref="CommandWrapper"/>
	/// <seealso cref="ICommandHost"/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class CommandsBase
	{
		#region ProcessKeyboardInput

		/// <summary>
		/// Processes the keyboard input represented by e against the state of the commandHost
		/// to determine if a command should be executed.  If so, the command is executed.
		/// </summary>
		/// <param name="e">The key event arguments</param>
		/// <param name="commandHost">An object that implements ICoomandHost</param>
		/// <returns>True if an action was performed.</returns>
		public abstract bool ProcessKeyboardInput(KeyEventArgs e, ICommandHost commandHost);

		#endregion //ProcessKeyboardInput
	}

	/// <summary>
	/// An abstract class for defining and managing the global list of available RoutedCommands for a specific component type.
	/// </summary>
	/// <seealso cref="CommandWrapper"/>
	/// <seealso cref="ICommandHost"/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class Commands<T> : CommandsBase
		where T: ICommandHost
	{
		#region Member Variables

		private static	CommandWrapper[]	CommandWrappersInternal;

		[ThreadStatic] // AS 1/12/10 TFS59619
		private	static	bool				_lastCommandExecuted = false;

		#endregion //Member Variables

		#region Methods

			#region Public Methods

				#region FindCommandWrappers

		/// <summary>
		/// Finds the CommandWrapper(s) associated with the specified RoutedCommand.
		/// </summary>
		/// <param name="commandToFind">The command for which to find wrappers</param>
		/// <returns>An array of CommandWrappers that wrap the specified command</returns>
		/// <seealso cref="CommandWrapper"/>
		public static CommandWrapper[] FindCommandWrappers(RoutedCommand commandToFind)
		{
			List<CommandWrapper> commandWrappers = new List<CommandWrapper>(3);

			foreach (CommandWrapper commandWrapper in Commands<T>.CommandWrappersInternal)
			{
				if (commandWrapper.Command == commandToFind)
					commandWrappers.Add(commandWrapper);
			}

			if (commandWrappers.Count > 0)
				return commandWrappers.ToArray();
			else
				return null;
		}

				#endregion //FindCommandWrappers

				#region Initialize

		/// <summary>
		/// Initializes command bindings for the commands represented by the supplied array of CommandWrappers
		/// Also saves a reference to the array of CommandWrappers
		/// </summary>
		/// <param name="commandWrappers"></param>
		[EditorBrowsable( EditorBrowsableState.Never)]
		public static void Initialize(CommandWrapper[] commandWrappers)
		{
			Commands<T>.CommandWrappersInternal = commandWrappers;


			// Register command bindings for each CommandWrapper's Command.
			foreach (CommandWrapper commandWrapper in commandWrappers)
			{
				CommandManager.RegisterClassCommandBinding(typeof(T),
					new CommandBinding(commandWrapper.Command,
									   new ExecutedRoutedEventHandler(OnCommand),
									   new CanExecuteRoutedEventHandler(OnQueryCommand)));
			}
		}

				#endregion //Initialize

				#region IsCommandAllowed

		/// <summary>
		/// Return true of the specified command can be executed given the state of the specified command host.
		/// </summary>
		/// <param name="commandHost">The CommandHost whose state will be evaluated to determine if the command can be executed</param>
		/// <param name="command">The command to evalue.</param>
		/// <returns>True if command can be executed, false if not.</returns>
		public static bool IsCommandAllowed(ICommandHost commandHost, RoutedCommand command)
		{
			return IsCommandAllowed(commandHost, command, null);
		}

		/// <summary>
		/// Return true of the specified command can be executed given the state of the specified command host.
		/// </summary>
		/// <param name="commandHost">The CommandHost whose state will be evaluated to determine if the command can be executed</param>
		/// <param name="command">The command to evalue.</param>
		/// <param name="commandParameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>True if command can be executed, false if not.</returns>
		public static bool IsCommandAllowed(ICommandHost commandHost, RoutedCommand command, object commandParameter)
		{
			// AS 2/5/08 ExecuteCommandInfo
			return IsCommandAllowed(commandHost, command, commandParameter, null);
		}

		// AS 2/5/08 ExecuteCommandInfo
		/// <summary>
		/// Return true of the specified command can be executed given the state of the specified command host.
		/// </summary>
		/// <param name="commandHost">The CommandHost whose state will be evaluated to determine if the command can be executed</param>
		/// <param name="command">The command to evalue.</param>
		/// <param name="commandParameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		/// <param name="originalSource">The original source of the request. This parameter may be null</param>
		/// <returns>True if command can be executed, false if not.</returns>
		public static bool IsCommandAllowed(ICommandHost commandHost, RoutedCommand command, object commandParameter, object originalSource)
		{
			// JM 12-02-08 TFS11061 - Call the new overload instead.
			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			bool continueRouting;
			return IsCommandAllowed(commandHost, command, commandParameter, originalSource, out continueRouting);
		}

		// JM 12-02-08 TFS11061 - Added overload.
		/// <summary>
		/// Return true of the specified command can be executed given the state of the specified command host.
		/// </summary>
		/// <param name="commandHost">The CommandHost whose state will be evaluated to determine if the command can be executed</param>
		/// <param name="command">The command to evalue.</param>
		/// <param name="commandParameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		/// <param name="originalSource">The original source of the request. This parameter may be null</param>
		/// <param name="continueRouting">Indicates whether to continue routing the original input event that triggered the command.</param>
		/// <returns>True if command can be executed, false if not.</returns>
		public static bool IsCommandAllowed(ICommandHost commandHost, RoutedCommand command, object commandParameter, object originalSource, out bool continueRouting)
		{
            // AS 2/12/09 TFS12819
            bool forceHandled;
            return IsCommandAllowed(commandHost, command, commandParameter, originalSource, out continueRouting, out forceHandled);
        }

        // AS 2/12/09 TFS12819
        /// <summary>
        /// Return true of the specified command can be executed given the state of the specified command host.
        /// </summary>
        /// <param name="commandHost">The CommandHost whose state will be evaluated to determine if the command can be executed</param>
        /// <param name="command">The command to evalue.</param>
        /// <param name="commandParameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <param name="originalSource">The original source of the request. This parameter may be null</param>
        /// <param name="continueRouting">Indicates whether to continue routing the original input event that triggered the command.</param>
        /// <param name="forceHandled">Out parameter that indicates whether the CanExecute should be considered handled regardless of whether it may be executed.</param>
        /// <returns>True if command can be executed, false if not.</returns>
        internal static bool IsCommandAllowed(ICommandHost commandHost, RoutedCommand command, object commandParameter, object originalSource, out bool continueRouting, out bool forceHandled)
        {
            continueRouting = false;

            // AS 2/12/09 TFS12819
            forceHandled = false;

			if (command == null)
				throw new ArgumentNullException("command");

			if (commandHost == null)
				throw new ArgumentNullException("commandHost");

            // AS 2/12/09 TFS12819
            // Created a common interface to avoid checking for the different types.
            //
            //if (command is IGRoutedCommand || command is IGRoutedUICommand)
            if (command is ICommandEx)
			{
                // AS 2/12/09 TFS12819
                // Call a new overload that returns the ForceHandled state of the command.
                //
                //if (false == Commands<T>.IsMinimumStatePresentForCommand(commandHost, command))
				// SSP 3/18/10 TFS29783 - Optimizations
				// Pass along command host instead of state so the method can query the command host for only the states
				// necessary to determine whether the command can be executed.
				// 
				//if (false == CommandUtilities.IsMinimumStatePresent(commandHost.CurrentState, command, out forceHandled))
				if ( false == CommandUtilities.IsMinimumStatePresent( commandHost, command, out forceHandled ) )
					return false;
			}

			// AS 2/5/08 ExecuteCommandInfo
			//return commandHost.CanExecute(command, commandParameter);
			ExecuteCommandInfo	eci			= new ExecuteCommandInfo(command, commandParameter, originalSource, continueRouting);
			bool				canExecute	= commandHost.CanExecute(eci);
			continueRouting					= eci.ContinueRouting;

            // AS 2/12/09 TFS12819
            forceHandled                    = eci.ForceHandled;

			return canExecute;
		}

				#endregion //IsCommandAllowed

				#region IsMinimumStatePresentForCommand

		/// <summary>
		/// Returns true if there is the minimum amount of state required in the host for execution of the command.
		/// </summary>
		/// <param name="commandHost">The CommandHost whose state will be evaluated to determine if the command can be executed</param>
		/// <param name="command">The command to evaluate.</param>
		/// <returns>True if the minimum state exists, false if not.</returns>
		public static bool IsMinimumStatePresentForCommand(ICommandHost commandHost, RoutedCommand command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			if (commandHost == null)
				throw new ArgumentNullException("commandHost");

			// SSP 3/18/10 TFS29783 - Optimizations
			// 
			//return CommandUtilities.IsMinimumStatePresent(commandHost.CurrentState, command);
			return CommandUtilities.IsMinimumStatePresent( commandHost, command );
		}

				#endregion //IsMinimumStatePresentForCommand

				#region ProcessKeyboardInput

		/// <summary>
		/// Processes the keyboard input represented by e against the state of the commandHost
		/// to determine if a command should be executed.  If so, the command is executed.
		/// </summary>
		/// <param name="e">The key event arguments</param>
		/// <param name="commandHost">An object that implements ICoomandHost</param>
		/// <returns>True if an action was performed.</returns>
		public override bool ProcessKeyboardInput(KeyEventArgs e, ICommandHost commandHost)
		{
			switch (e.Key)
			{
				case Key.LeftAlt:
				case Key.LeftCtrl:
				case Key.LeftShift:
				case Key.LWin:
				case Key.RightAlt:
				case Key.RightCtrl:
				case Key.RightShift:
				case Key.RWin:
					return false;
				default:
					break;
			}

			ModifierKeys currentModifierKeys = Keyboard.Modifiers;
			bool actionPerformed = false;
			
			// SSP 3/18/10 TFS29783 - Optimizations
			// Instead of forcing the command host to evaluate all potential states, only query for states that
			// are required to evaluate commands that match the input gestures.
			// 
			// ------------------------------------------------------------------------------------------------
			List<CommandWrapper> list = new List<CommandWrapper>( );

			CommandWrapper[] commandWrappers = Commands<T>.CommandWrappersInternal;
			if ( commandWrappers != null )
			{
				for ( int i = 0; i < commandWrappers.Length; i++ )
				{
					CommandWrapper commandWrapper = commandWrappers[i];
				
					// AS 3/21/08 Optimization
					// Do not force the input gestures to be allocated and also don't bother
					// creating the enumerator if there is nothing in the collection.
					//
					//if (commandWrapper != null)
					if ( commandWrapper != null && commandWrapper.HasInputGestures )
					{
						foreach ( InputGesture inputGesture in commandWrapper.InputGestures )
						{
							if ( inputGesture.Matches( commandHost, e ) )
							{
								if ( 0 == ( commandWrapper.ModifierKeysDisallowed & currentModifierKeys ) )
									list.Add( commandWrapper );

								// AS 3/21/08 Optimization
								// There could be multiple gestures but there is no point in checking any
								// more for this command wrapper.
								//
								break;
							}
						}
					}
				}
			}

			int count = null != list ? list.Count : 0;
			if ( count > 0 )
			{
				long statesToQuery = 0;

				for ( int i = 0; i < count; i++ )
				{
					CommandWrapper commandWrapper = list[i];
					statesToQuery |= commandWrapper.StateRequired;
					statesToQuery |= commandWrapper.StateDisallowed;
				}

				long commandHostCurrentState = commandHost.GetCurrentState( statesToQuery );

				for ( int i = 0; i < count; i++ )
				{
					CommandWrapper commandWrapper = list[i];

					if ( commandWrapper.IsAllowed( commandHostCurrentState ) )
					{
						
						
						
						
						// AS 9/10/09 TFS19267
						//commandWrapper.Command.Execute(null, commandHost as IInputElement);
						RoutedCommand command = commandWrapper.Command;

						object parameter = this.GetKeyboardParameter( commandHost, command, e  );

						// AS 1/12/10 TFS59619
						// Since the _lastCommandExecuted is only set in the OnCommand and that may not be 
						// invoked if the CanExecute doesn't return true, we could end up returning true because 
						// the last command that was executed successfully processed. Note this is also an 
						// unlikely chance that we could return true should another instance of this T (or the host 
						// being processed here) could invoke another command in which case we'll return true but 
						// after some discussion we decided to leave that aspect alone for now.
						//
						bool wasLastExecuted = _lastCommandExecuted;
						_lastCommandExecuted = false;

						try
						{
							command.Execute(parameter, commandHost as IInputElement);

							if (_lastCommandExecuted == true)
								actionPerformed = true;
						}
						finally
						{
							_lastCommandExecuted = wasLastExecuted;
						}
					}
				}
			}

			//long commandHostCurrentState = commandHost.CurrentState;

			//if (Commands<T>.CommandWrappersInternal != null)
			//{
			//    foreach (CommandWrapper commandWrapper in Commands<T>.CommandWrappersInternal)
			//    {
			//        // AS 3/21/08 Optimization
			//        // Do not force the input gestures to be allocated and also don't bother
			//        // creating the enumerator if there is nothing in the collection.
			//        //
			//        //if (commandWrapper != null)
			//        if (commandWrapper != null && commandWrapper.HasInputGestures)
			//        {
			//            foreach (InputGesture inputGesture in commandWrapper.InputGestures)
			//            {
			//                if (inputGesture.Matches(commandHost, e))
			//                {
			//                    if (commandWrapper.IsAllowed(commandHostCurrentState)
			//                        && 0 == (commandWrapper.ModifierKeysDisallowed & currentModifierKeys))
			//                    {
			//                        //~ TODO: Should we be calling CanExecute on the command or on the ICommandHost?
			//                        //~ By not doing that we will mark a key as handled as long as the state of the 
			//                        //~ command and host is such that it seems like it could be handled.
			//                        //~
			//                        // AS 9/10/09 TFS19267
			//                        //commandWrapper.Command.Execute(null, commandHost as IInputElement);
			//                        RoutedCommand command = commandWrapper.Command;

			//                        object parameter = this.GetKeyboardParameter(commandHost, command, e /* AS 10/15/09 TFS23867 */ );

			//                        command.Execute(parameter, commandHost as IInputElement);

			//                        if (_lastCommandExecuted == true)
			//                            actionPerformed = true;
			//                    }

			//                    // AS 3/21/08 Optimization
			//                    // There could be multiple gestures but there is no point in checking any
			//                    // more for this command wrapper.
			//                    //
			//                    break;
			//                }
			//            }
			//        }
			//    }
			//}
			// ------------------------------------------------------------------------------------------------

			return actionPerformed;
		}

				#endregion //ProcessKeyboardInput

			#endregion //Public Methods

			#region Private methods

				#region OnCommand

		private static void OnCommand(object target, ExecutedRoutedEventArgs args)
		{
			ICommandHost commandHost = target as ICommandHost;
			if (commandHost != null)
            {
				// AS 2/5/08 ExecuteCommandInfo
				//commandHost.Execute(args);
                // AS 2/12/09 TFS12819
                // Also consider it handled if Forcehandled was true.
                //
                //args.Handled = commandHost.Execute(new ExecuteCommandInfo(args.Command, args.Parameter, args.OriginalSource));
                ExecuteCommandInfo eci = new ExecuteCommandInfo(args.Command, args.Parameter, args.OriginalSource);
                args.Handled = commandHost.Execute(eci) || eci.ForceHandled;
            }

			_lastCommandExecuted = args.Handled;
		}

				#endregion //OnCommand

				#region OnQueryCommand

		private static void OnQueryCommand(object target, CanExecuteRoutedEventArgs args)
		{
			ICommandHost		commandHost		= target as ICommandHost;
			RoutedCommand		command			= args.Command as RoutedCommand;

			if (commandHost	!= null &&
				command		!= null)
			{
				// JM 12-02-08 TFS11061
				//// AS 2/5/08 ExecuteCommandInfo
				////if (IsCommandAllowed(commandHost, command, args.Parameter))
				//if (IsCommandAllowed(commandHost, command, args.Parameter, args.OriginalSource))
                // AS 2/12/09 TFS12819
                // Added a new overload that returns a forceHandled parameter so we can 
                // make the event handled even if the object cannot execute the command. 
                // This is needed for nested command usage scenarios such as nesting a 
                // dockmanager within a dockmanager.
                //
                //bool continueRouting;
                //if (IsCommandAllowed(commandHost, command, args.Parameter, args.OriginalSource, out continueRouting))
                bool continueRouting, forceHandled;
                if (IsCommandAllowed(commandHost, command, args.Parameter, args.OriginalSource, out continueRouting, out forceHandled))
                {
                    args.CanExecute = true;
                    args.Handled = true;

					// AS 8/20/09
					// I found this while debugging TFS20920. Essentially the xamNumericEditor had keyboard focus.
					// So its input binding for the undo command was being triggered. However, it didn't need to 
					// undo so it left CanExecute false. This bubbled up to the XDP which indicated it could process 
					// it. The XDP got the executed for this command and correctly marked it handled but as part of 
					// the xamnumericeditor's keydown handling (where the base uielement class had called off to 
					// CommandManager.TranslateInput). This method only marks the input event args as handled if 
					// ContinueRouting of the CanExecute was false. We were only setting the ContinueRouting when 
					// IsCommandAllowed returned false - probably assuming this was already false but it now would 
					// be true because the editor set it as such.
					//
					args.ContinueRouting = continueRouting;
                }
                else
                {
                    args.ContinueRouting = continueRouting;

                    // AS 2/12/09 TFS12819
                    if (forceHandled)
                        args.Handled = true;
                }

				return;
			}

			args.CanExecute = false;
		}

				#endregion //OnQueryCommand

			#endregion //Private Methods

			#region Protected methods

				// AS 9/10/09 TFS19267
				#region GetKeyboardParameter
		/// <summary>
		/// Returns the parameter that should be used for a given command that is being processed by the <see cref="ProcessKeyboardInput"/> method.
		/// </summary>
		/// <param name="commandHost">The ICommandHost instance for which the keyboard input is being processed.</param>
		/// <param name="command">The command to be executed.</param>
		/// <param name="keyArgs">The key event arguments for which the command is being invoked</param>
		/// <returns>By default null is returned.</returns>
		// AS 10/15/09 TFS23867
		// Added keyArgs so the instance could know what key is causing the action.
		//
		//protected virtual object GetKeyboardParameter(ICommandHost commandHost, RoutedCommand command)
		protected virtual object GetKeyboardParameter(ICommandHost commandHost, RoutedCommand command, KeyEventArgs keyArgs)
		{
			return null;
		}
				#endregion //GetKeyboardParameter

			#endregion //Protected methods

		#endregion //Methods
	}

	#endregion //Commands<T> class

	#region ExecuteCommandInfo
	/// <summary>
	/// Class used to provide information about a <see cref="RoutedCommand"/> related event.
	/// </summary>
	public class ExecuteCommandInfo
	{
		#region Member Variables
		private ICommand _command;
		private object _parameter;
		private object _originalSource;

		// JM 12-02-08 TFS11061
		private bool	_continueRouting;

        // AS 2/12/09 TFS12819
        private bool _forceHandled;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ExecuteCommandInfo"/>
		/// </summary>
		/// <param name="command">Associated command</param>
		public ExecuteCommandInfo(ICommand command)
			: this(command, null, null)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="ExecuteCommandInfo"/>
		/// </summary>
		/// <param name="command">Associated command</param>
		/// <param name="commandParameter">The parameter for the command or null (Nothing in VB) if there is no parameter</param>
		/// <param name="originalSource">The object that is the source of the command event or null.</param>
		public ExecuteCommandInfo(ICommand command, object commandParameter, object originalSource)
			: this(command, commandParameter, originalSource, false)	// JM 12-02-08 TFS11061
		{
			// JM 12-02-08 TFS11061 - Call the new overload instead.
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		}

		// JM 12-02-08 TFS11061 - Added overload.
		/// <summary>
		/// Initializes a new <see cref="ExecuteCommandInfo"/>
		/// </summary>
		/// <param name="command">Associated command</param>
		/// <param name="commandParameter">The parameter for the command or null (Nothing in VB) if there is no parameter</param>
		/// <param name="originalSource">The object that is the source of the command event or null.</param>
		/// <param name="continueRouting">Indicates whether to continue routing the original input event that triggered the command.</param>
		public ExecuteCommandInfo(ICommand command, object commandParameter, object originalSource, bool continueRouting)
		{
			if (null == command)
				throw new ArgumentNullException("command");

			this._command = command;
			this._parameter = commandParameter;
			this._originalSource = originalSource;
			this._continueRouting	= continueRouting;
		}

		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the associated command
		/// </summary>
		public ICommand Command
		{
			get { return this._command; }
		}

		/// <summary>
		/// Returns the <see cref="Command"/> as a <see cref="RoutedCommand"/>
		/// </summary>
		public RoutedCommand RoutedCommand
		{
			get { return this._command as RoutedCommand; }
		}

		/// <summary>
		/// Returns the parameter for the command or null if there is no parameter.
		/// </summary>
		public object Parameter
		{
			get { return this._parameter; }
		}

		/// <summary>
		/// Returns the object that initiated the command related event or null if one was not available.
		/// </summary>
		public object OriginalSource
		{
			get { return this._originalSource; }
		}

		/// <summary>
		/// Returns/sets whether to continue routing the original input event that triggered the command.
		/// </summary>
		public bool ContinueRouting
		{
			get { return this._continueRouting; }
			set { this._continueRouting = value; }
		}

        // AS 2/12/09 TFS12819
        /// <summary>
        /// Returns/sets whether the command event should be marked as handled even if the command cannot be executed.
        /// </summary>
        public bool ForceHandled
        {
            get { return _forceHandled; }
            set { _forceHandled = value; }
        }
		#endregion //Properties
	} 
	#endregion //ExecuteCommandInfo

	#region CommandUtilities Class

	internal static class CommandUtilities
	{
		#region IsMinimumStatePresent

		// SSP 3/18/10 TFS29783 - Optimizations
		// 
		internal static bool IsMinimumStatePresent( ICommandHost host, RoutedCommand command )
		{
			bool forceHandled;

			return IsMinimumStatePresent( host, command, out forceHandled );
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// 
		internal static bool IsMinimumStatePresent( ICommandHost host, RoutedCommand command, out bool forceHandled )
		{
			return IsMinimumStatePresent( host, null, command, out forceHandled );
		}

        internal static bool IsMinimumStatePresent(Int64 state, RoutedCommand command)
        {
            bool forceHandled;
            return IsMinimumStatePresent(state, command, out forceHandled);
        }

        // AS 2/12/09 TFS12819
        // Added overload to check the ForceHandled of the command.
        //
        internal static bool IsMinimumStatePresent(Int64 state, RoutedCommand command, out bool forceHandled)
		{
			return IsMinimumStatePresent( null, state, command, out forceHandled );
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// Added an overload of IsMinimumStatePresent that takes in host. If hostState is null then it's queried
		// from the host.
		// 
		private static bool IsMinimumStatePresent( ICommandHost host, Int64? hostState, RoutedCommand command, out bool forceHandled )
		{
			// Get the minimum state requirements from the command.
			Int64			minimumStateRequired	= -1;
			Int64			minimumStateDisallowed	= -1;

            
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

            ICommandEx commandEx = command as ICommandEx;

            if (null == commandEx)
            {
                forceHandled = false;
                return true;
            }

			minimumStateRequired	= commandEx.MinimumStateRequired;
			minimumStateDisallowed	= commandEx.MinimumStateDisallowed;
            forceHandled            = commandEx.ForceHandled;

			// SSP 3/18/10 TFS29783 - Optimizations
			// Added host parameter and changed state parameter to a nullable hostState parameter. If hostState is 
			// null then query the state from the host.
			// 
			Debug.Assert( null != host || hostState.HasValue );
			Int64 state = hostState.HasValue ? hostState.Value 
				: ( null != host ? host.GetCurrentState( minimumStateRequired | minimumStateDisallowed ) : 0L );

            // Check for the required state bits
			if ((state & minimumStateRequired) != minimumStateRequired)
				return false;


			// Check for the state bits that are disallowed
			if (0 != (state & minimumStateDisallowed))
				return false;

			return true;
		}

		#endregion //IsMinimumStatePresent
	}

	#endregion //CommandUtilities Class

    // AS 2/12/09 TFS12819
    // To avoid checking for IGRoutedCommand vs IGRoutedUICommand, I
    // created an internal interface we can use to get the common 
    // state.
    //
    #region ICommandEx
    internal interface ICommandEx
    {
        long MinimumStateDisallowed { get; }
        long MinimumStateRequired { get; }
        bool ForceHandled { get; }
    } 
    #endregion //ICommandEx

	#region IGRoutedCommand Class

	/// <summary>
	/// Wrapper for WPF RoutedCommand
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class IGRoutedCommand : RoutedCommand
        // AS 2/12/09 TFS12819
        , ICommandEx
	{
		#region Member Variables

		private Int64				_minimumStateDisallowed = 0;
		private Int64				_minimumStateRequired = 0;

        // AS 2/12/09 TFS12819
        private bool                _forceHandled;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="IGRoutedCommand"/> class
		/// </summary>
		/// <param name="name">Declared name for serialization</param>
		/// <param name="ownerType">The type that is registering the command</param>
		/// <param name="minimumStateDisallowed">The minimum state would be used to consider the command as disabled.</param>
		/// <param name="minimumStateRequired">The minimum state required to consider the command enabled.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IGRoutedCommand(string name, Type ownerType, Int64 minimumStateDisallowed, Int64 minimumStateRequired)
			: base(name, ownerType)
		{
			this._minimumStateDisallowed	= minimumStateDisallowed;
			this._minimumStateRequired		= minimumStateRequired;
		}

        // AS 2/12/09 TFS12819
        /// <summary>
		/// Initializes a new instance of the <see cref="IGRoutedCommand"/> class
		/// </summary>
		/// <param name="name">Declared name for serialization</param>
		/// <param name="ownerType">The type that is registering the command</param>
		/// <param name="minimumStateDisallowed">The minimum state would be used to consider the command as disabled.</param>
		/// <param name="minimumStateRequired">The minimum state required to consider the command enabled.</param>
        /// <param name="forceHandled">True if the CanExecute should always be considered handled even when the minimum state required for the command is not present.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IGRoutedCommand(string name, Type ownerType, Int64 minimumStateDisallowed, Int64 minimumStateRequired, bool forceHandled)
			: this(name, ownerType, minimumStateDisallowed, minimumStateRequired)
		{
            _forceHandled = forceHandled;
		}
		#endregion //Constructors

		#region Properties

            // AS 2/12/09 TFS12819
            #region ForceHandled
        /// <summary>
        /// Returns a boolean indicating whether the CanExecute/Execute events should be considered handled for this command when 
        /// the minimum state required/disallowed would prevent the execution of the command.
        /// </summary>
        public bool ForceHandled
        {
            get { return _forceHandled; }
        } 
            #endregion //ForceHandled

			#region MinimumStateDisallowed

		/// <summary>
		/// Minimum state that would disallow the command.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Int64 MinimumStateDisallowed
		{
			get { return this._minimumStateDisallowed; }
		}

			#endregion //MinimumStateDisallowed

			#region MinimumStateRequired

		/// <summary>
		/// Minimum state required to execute the command.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Int64 MinimumStateRequired
		{
			get { return this._minimumStateRequired; }
		}

			#endregion //MinimumStateRequired

		#endregion Properties
	}

	#endregion //IGRoutedCommand Class

	#region IGRoutedUICommand Class

	/// <summary>
	/// Wrapper for WPF RoutedUICommand
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class IGRoutedUICommand : RoutedUICommand
        // AS 2/12/09 TFS12819
        , ICommandEx
	{
		#region Member Variables

		private Int64			_minimumStateDisallowed = 0;
		private Int64			_minimumStateRequired	= 0;

        // AS 2/12/09 TFS12819
        private bool            _forceHandled;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="text">Descriptive text for the command.</param>
		/// <param name="name">Declared name for serialization</param>
		/// <param name="ownerType">The type that is registering the command</param>
		/// <param name="minimumStateDisallowed">The minimum state would be used to consider the command as disabled.</param>
		/// <param name="minimumStateRequired">The minimum state required to consider the command enabled.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IGRoutedUICommand(string text, string name, Type ownerType, Int64 minimumStateDisallowed, Int64 minimumStateRequired)
			: base(text, name, ownerType)
		{
			this._minimumStateDisallowed	= minimumStateDisallowed;
			this._minimumStateRequired		= minimumStateRequired;
		}

        // AS 2/12/09 TFS12819
        /// <summary>
		/// Initializes a new <see cref="IGRoutedUICommand"/>
		/// </summary>
		/// <param name="text">Descriptive text for the command.</param>
		/// <param name="name">Declared name for serialization</param>
		/// <param name="ownerType">The type that is registering the command</param>
		/// <param name="minimumStateDisallowed">The minimum state would be used to consider the command as disabled.</param>
		/// <param name="minimumStateRequired">The minimum state required to consider the command enabled.</param>
        /// <param name="forceHandled">True if the CanExecute should always be considered handled even when the minimum state required for the command is not present.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
		public IGRoutedUICommand(string text, string name, Type ownerType, Int64 minimumStateDisallowed, Int64 minimumStateRequired, bool forceHandled)
			: this(text, name, ownerType, minimumStateDisallowed, minimumStateRequired)
		{
            _forceHandled = forceHandled;
		}

		#endregion //Constructors

		#region Properties

            // AS 2/12/09 TFS12819
            #region ForceHandled
        /// <summary>
        /// Returns a boolean indicating whether the CanExecute/Execute events should be considered handled for this command when 
        /// the minimum state required/disallowed would prevent the execution of the command.
        /// </summary>
        public bool ForceHandled
        {
            get { return _forceHandled; }
        } 
            #endregion //ForceHandled

			#region MinimumStateDisallowed

		/// <summary>
		/// Minimum state that would disallow the command.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Int64 MinimumStateDisallowed
		{
			get { return this._minimumStateDisallowed; }
		}

			#endregion //MinimumStateDisallowed

			#region MinimumStateRequired

		/// <summary>
		/// Minimum state required to execute the command.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Int64 MinimumStateRequired
		{
			get { return this._minimumStateRequired; }
		}

			#endregion //MinimumStateRequired

		#endregion Properties
	}

	#endregion //IGRoutedUICommand Class
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