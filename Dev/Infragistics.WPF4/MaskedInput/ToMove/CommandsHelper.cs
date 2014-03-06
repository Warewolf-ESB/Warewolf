using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Linq;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Provides the list of RoutedCommands supported by the XamMaskedInput. 
	/// </summary>
	internal abstract class CommandsHelper<TCommandId>
		where TCommandId : struct
	{
		#region CommandDefinition class
		internal class CommandDefinition
		{
			internal readonly TCommandId _command;
			internal readonly Int64 _disallowedState;
			internal readonly Int64 _requiredState;

			internal CommandDefinition(TCommandId command, Int64 disallowedState, Int64 requiredState)
			{
				_command = command;
				_disallowedState = disallowedState;
				_requiredState = requiredState;
			}
		} 
		#endregion //CommandDefinition class

		#region KeyGesture class
		internal struct KeyGesture
		{
			internal readonly Key _key;
			internal readonly ModifierKeys _modifierKeys;

			internal KeyGesture(Key key, ModifierKeys modifierKeys)
			{
				_key = key;
				_modifierKeys = modifierKeys;
			}
		} 
		#endregion //KeyGesture class

		#region CommandWrapper class
		internal class CommandWrapper
		{
			internal readonly TCommandId _command;
			internal CommandDefinition _minimumRequiredState;
			internal readonly Int64 _disallowedState;
			internal readonly Int64 _requiredState;
			internal readonly KeyGesture[] _keyGestures;
			internal readonly ModifierKeys _disallowedModifierKeys;

			internal CommandWrapper(TCommandId command, Int64 disallowedState, Int64 requiredState, KeyGesture keyGesture, ModifierKeys disallowedModifierKeys = ModifierKeys.None)
				: this(command, disallowedState, requiredState, new KeyGesture[] { keyGesture }, disallowedModifierKeys)
			{
			}

			internal CommandWrapper(TCommandId command, Int64 disallowedState, Int64 requiredState, KeyGesture[] keyGestures, ModifierKeys disallowedModifierKeys = ModifierKeys.None)
			{
				_command = command;
				_disallowedState = disallowedState;
				_requiredState = requiredState;
				_keyGestures = keyGestures;
				_disallowedModifierKeys = disallowedModifierKeys;
			}

			internal bool DoesKeyMatch(Key key, ModifierKeys modifierKeys)
			{
				if (null != _keyGestures)
				{
					for (int i = 0; i < _keyGestures.Length; i++)
					{
						KeyGesture ii = _keyGestures[i];
						if (ii._key == key && ii._modifierKeys == (ii._modifierKeys & modifierKeys) && 0 == (_disallowedModifierKeys & modifierKeys))
							return true;
					}
				}

				return false;
			}

			internal static bool StateMatchHelper(Int64 controlState, Int64 disallowedState, Int64 requiredState)
			{
				return 0 == (disallowedState & controlState)
					&& requiredState == (requiredState & controlState);
			}

			internal bool DoesStateMatch(Int64 controlState)
			{
				return StateMatchHelper(controlState, _disallowedState, _requiredState)
					&& (null == _minimumRequiredState || StateMatchHelper(controlState, _minimumRequiredState._disallowedState, _minimumRequiredState._requiredState));
			}

			public Int64 StatesConsidered
			{
				get
				{
					Int64 state = _disallowedState | _requiredState;

					if (null != _minimumRequiredState)
						state |= _minimumRequiredState._disallowedState | _minimumRequiredState._requiredState;

					return state;
				}
			}
		} 
		#endregion //CommandWrapper class

		#region Member Variables

		private CommandWrapper[] _commands;
		private Dictionary<TCommandId, CommandDefinition> _minimumRequiredStates;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CommandsHelper&lt;TCommandId&gt;"/>
		/// </summary>
		/// <param name="commands">An array of <see cref="CommandWrapper"/> instances</param>
		/// <param name="minimumRequiredStates">An array of definitions for the command dictating their required and minimum states</param>
		protected CommandsHelper(CommandWrapper[] commands, CommandDefinition[] minimumRequiredStates = null)
		{
			CoreUtilities.ValidateNotNull(commands, "commands");

			_commands = commands;

			if (null != minimumRequiredStates)
			{
				var map = minimumRequiredStates.ToDictionary(ii => ii._command);
				foreach (var ww in commands)
				{
					CommandDefinition minimumRequiredState;
					if (map.TryGetValue(ww._command, out minimumRequiredState))
						ww._minimumRequiredState = minimumRequiredState;
				}

				_minimumRequiredStates = map;
			}
		} 
		#endregion //Constructor

		#region Methods

		#region Public Methods

		#region DoesMinimumStateMatch
		/// <summary>
		/// Returns a nullable boolean indicating whether the specified command meets the required and disallowed states of the requirements for the specified command.
		/// </summary>
		/// <param name="commandId">The id of the command being evaluated</param>
		/// <param name="controlState">The current state of the control</param>
		/// <returns>If a minimum required state was not provided for the specified command when the object was constructed then null is returned. If there is a minimum required state for the specified command then true is returned if the requirements are satisfied based on the specified <paramref name="controlState"/>; otherwise false is returned.</returns>
		public bool? DoesMinimumStateMatch(TCommandId commandId, Int64 controlState)
		{
			if (null != _minimumRequiredStates)
			{
				CommandDefinition ii;
				if (_minimumRequiredStates.TryGetValue(commandId, out ii))
					return CommandWrapper.StateMatchHelper(controlState, ii._disallowedState, ii._requiredState);
			}

			return null;
		}

		/// <summary>
		/// Returns a nullable boolean indicating whether the specified command meets the required and disallowed states of the requirements for the specified command.
		/// </summary>
		/// <param name="commandId">The id of the command being evaluated</param>
		/// <param name="getCurrentStates">Method used to obtain the current state of the control given a set of states for the command based on the minimum requirements provided for the specified command when the helper was constructed.</param>
		/// <returns>If a minimum required state was not provided for the specified command when the object was constructed then null is returned. If there is a minimum required state for the specified command then true is returned if the requirements are satisfied based on the specified current control state returned from the <paramref name="getCurrentStates"/>; otherwise false is returned.</returns>
		public bool? DoesMinimumStateMatch(TCommandId commandId, Func<Int64, Int64> getCurrentStates)
		{
			CoreUtilities.ValidateNotNull(getCurrentStates);

			if (_minimumRequiredStates != null)
			{
				CommandDefinition cd;
				if (_minimumRequiredStates.TryGetValue(commandId, out cd))
				{
					Int64 controlState = getCurrentStates(cd._disallowedState | cd._requiredState);
					return CommandWrapper.StateMatchHelper(controlState, cd._disallowedState, cd._requiredState);
				}
			}

			return null;
		}
		#endregion //DoesMinimumStateMatch

		#region GetMatchingCommands
		/// <summary>
		/// Returns an enumerable of commands based on the specified key and modifier keys for the specified current control state.
		/// </summary>
		/// <param name="key">The key being evaluated</param>
		/// <param name="modifierKeys">The current state of the modifier keys</param>
		/// <param name="controlState">The current state of the control</param>
		/// <returns>An enumerable of command ids for the commands mapped to the specified key for the specified modifier keys and control state</returns>
		public IEnumerable<TCommandId> GetMatchingCommands(Key key, ModifierKeys modifierKeys, Int64 controlState)
		{
			return from ww in _commands
				   where ww.DoesKeyMatch(key, modifierKeys) && ww.DoesStateMatch((long)controlState)
				   select ww._command;
		}

		/// <summary>
		/// Returns an enumerable of commands based on the specified key and modifier keys for the specified current control state.
		/// </summary>
		/// <param name="key">The key being evaluated</param>
		/// <param name="modifierKeys">The current state of the modifier keys</param>
		/// <param name="getCurrentStates">Method used to obtain the current state of the control given a set of states for the commands matching the specified key and modifiers</param>
		/// <returns>An enumerable of command ids for the commands mapped to the specified key for the specified modifier keys and control state</returns>
		public IEnumerable<TCommandId> GetMatchingCommands(Key key, ModifierKeys modifierKeys, Func<Int64, Int64> getCurrentStates)
		{
			CoreUtilities.ValidateNotNull(getCurrentStates);

			var list = new List<CommandWrapper>();
			long statesUsed = 0;

			foreach (var ww in _commands)
			{
				if (ww.DoesKeyMatch(key, modifierKeys))
				{
					list.Add(ww);
					statesUsed |= ww.StatesConsidered;
				}
			}

			Int64 controlState = getCurrentStates(statesUsed);

			return from ww in list
				   where ww.DoesStateMatch(controlState)
				   select ww._command;
		}
		#endregion //GetMatchingCommands

		#endregion //Public Methods

		#region Internal Methods
		
		#region GetDefinition
		internal CommandDefinition GetDefinition(TCommandId commandId)
		{
			CommandDefinition ii;
			_minimumRequiredStates.TryGetValue(commandId, out ii);
			return ii;
		}
		#endregion //GetDefinition

		#endregion //Internal Methods
		
		#region Private Methods

		#region GetCommandWrappers
		private IEnumerable<CommandWrapper> GetCommandWrappers(TCommandId commandId)
		{
			return from ii in _commands where EqualityComparer<TCommandId>.Default.Equals(ii._command, commandId) select ii;
		}
		#endregion //GetCommandWrappers

		#endregion //Private Methods

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