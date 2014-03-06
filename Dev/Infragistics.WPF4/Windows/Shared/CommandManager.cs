using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using Infragistics.Controls;

namespace Infragistics
{
	/// <summary>
	/// A static class that manages all registered <see cref="ICommandTarget"/> and <see cref="CommandSource"/> objects of an application.
	/// </summary>
	public static class CommandSourceManager
	{
		#region Members
		// AS 6/7/11 TFS78383
		// Added ThreadStatic to these static lists since we cannot be dealing with DispatcherObjects on 
		// different threads or else an InvalidOperationException will be generated. As part of this change 
		// I added 2 static properties to allocate the collection instances and changed the code to deal 
		// with these properties instead of the fields.
		//
		[ThreadStatic]
		static List<WeakReference> _commandTargets = new List<WeakReference>();
		[ThreadStatic]
		static Dictionary<Type, List<WeakReference>> _commands = new Dictionary<Type, List<WeakReference>>();
		#endregion // Members

		#region Properties

		#region Private Properties

		// AS 6/7/11 TFS78383
		#region Commands
		private static Dictionary<Type, List<WeakReference>> Commands
		{
			get
			{
				if (_commands == null)
					_commands = new Dictionary<Type, List<WeakReference>>();

				return _commands;
			}
		}
		#endregion //Commands

		// AS 6/7/11 TFS78383
		#region CommandTargets
		private static List<WeakReference> CommandTargets
		{
			get
			{
				if (_commandTargets == null)
					_commandTargets = new List<WeakReference>();

				return _commandTargets;
			}
		}
		#endregion //CommandTargets

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Public

		#region RegisterCommandTarget
		/// <summary>
		/// Registers the specified <see cref="ICommandTarget"/> with the <see cref="CommandSourceManager"/>
		/// </summary>
		/// <param name="target"></param>
		/// <remarks>
		/// Once registered, if a <see cref="CommandSource"/> is unable to resolve a target from walking up the VisualTree,
		/// it will turn to the <see cref="CommandSourceManager"/> to find all <see cref="ICommandTarget"/> objects that support the 
		/// given <see cref="ICommand"/>
		/// </remarks>
		public static void RegisterCommandTarget(ICommandTarget target)
		{
			CommandTargets.Add(new WeakReference(target));
		}
		#endregion // RegisterCommandTarget

		#region UnregisterCommandTarget
		/// <summary>
		/// Unregisters the specified <see cref="ICommandTarget"/> with the <see cref="CommandSourceManager"/>
		/// </summary>
		/// <param name="target"></param>
		public static void UnregisterCommandTarget(ICommandTarget target)
		{
			var commandTargets = CommandTargets;
			for(int i = commandTargets.Count - 1; i >= 0; i--)
			{
				WeakReference wr = commandTargets[i];
				if (wr.Target == target)
				{
					commandTargets.RemoveAt(i);
				}
			}
		}
		#endregion // UnregisterCommandTarget

		#region GetTargets

		/// <summary>
		/// Gets a list of registered <see cref="ICommandTarget"/> objects that support the specified command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static Collection<ICommandTarget> GetTargets(ICommand command)
		{
			Collection<ICommandTarget> targets = new Collection<ICommandTarget>();
			foreach (WeakReference wr in CommandTargets)
			{
				if (wr.IsAlive)
				{
					ICommandTarget target = (ICommandTarget)wr.Target;
					if (target.SupportsCommand(command))
						targets.Add(target);
				}
			}
			return targets;
		}
		#endregion // GetTargets

		#region NotifyCanExecuteChanged
		
		/// <summary>
		/// An <see cref="ICommandTarget"/> will generally invoke this method to notify all 
		/// registered commands that have the specific Target to check and see if it still can execute.
		/// </summary>
		/// <param name="commandType"></param>
		public static void NotifyCanExecuteChanged(Type commandType)
		{
			// AS 6/7/11 TFS78383
			// In addition to using the static property now there is no need to index into the 
			// dictionary multiple times.
			//
			//if (CommandSourceManager._commands.ContainsKey(commandType))
			//{
			//    List<WeakReference> refs = CommandSourceManager._commands[commandType];
			List<WeakReference> refs;

			if (CommandSourceManager.Commands.TryGetValue(commandType, out refs))
			{
				foreach (WeakReference wr in refs)
				{
					if (wr.IsAlive)
					{
						CommandSource source = wr.Target as CommandSource;
						if (source != null)
						{
							if (source.Command.GetType() == commandType)
								source.InvokeCommand(false);
						}
					}
				}
			}
		}
		#endregion // NotifyCanExecuteChanged

		#endregion // Public

		#region Internal

		#region RegisterCommandSource

		/// <summary>
		/// Registers the specified CommandSource with the <see cref="CommandSourceManager"/>
		/// </summary>
		/// <param name="source"></param>
		/// <param name="elem"></param>
		/// <remarks>
		/// This method actually associates the source with a given element and resolves and attaches the event to the element.
		/// </remarks>
		internal static void RegisterCommandSource(CommandSource source, FrameworkElement elem)
		{
			ICommand command = source.Command;
			if (command != null && !String.IsNullOrEmpty(source.EventName))
			{
				// Associate CommandSource with Element
				source.SourceElement = elem;

				// Add Event Handler
				EventInfo info = elem.GetType().GetEvent(source.EventName);
				if (info != null)
				{
					Action<object, EventArgs> handler = new Action<object, EventArgs>(source.EventFired);
					source.EventHandler = Delegate.CreateDelegate(info.EventHandlerType, source, handler.Method);
					info.AddEventHandler(elem, source.EventHandler);
				}
				else
				{
					throw new InvalidEventNameException(source.EventName, elem.GetType());
				}

				// Register the Command
				Type t = command.GetType();
				List<WeakReference> refs;
				// AS 6/7/11 TFS78383
				// In addition to using the static property now there is no need to index into the 
				// dictionary multiple times.
				//
				//if (!CommandSourceManager._commands.ContainsKey(t))
				if (!CommandSourceManager.Commands.TryGetValue(t, out refs))
				{
					refs = new List<WeakReference>();
					CommandSourceManager.Commands.Add(t, refs);
				}
				//else
				//    refs = CommandSourceManager._commands[t];

				bool found = false;
				foreach (WeakReference wr in refs)
				{
					if (wr.IsAlive && wr.Target == source)
					{
						found = true;
						break;
					}
				}

				if (!found)
					refs.Add(new WeakReference(source));
			}
		}
		
		#endregion // RegisterCommandSource

		#region UnregisterCommandSource

		/// <summary>
		/// Removes the source from the CommandSourceManager, and also removes the attached event handler.
		/// </summary>
		/// <param name="source"></param>
		internal static void UnregisterCommandSource(CommandSource source)
		{
			//Remove Event Handler
			EventInfo info = source.SourceElement.GetType().GetEvent(source.EventName);
			if (info != null)
				info.RemoveEventHandler(source.SourceElement, source.EventHandler);

			// Unregister the Command
			Type t = source.Command.GetType();
			// AS 6/7/11 TFS78383
			// In addition to using the static property now there is no need to index into the 
			// dictionary multiple times.
			//
			//if (CommandSourceManager._commands.ContainsKey(t))
			//{
			//    List<WeakReference> refs = CommandSourceManager._commands[t];
			List<WeakReference> refs;
			if (CommandSourceManager.Commands.TryGetValue(t, out refs))
			{
				int index = -1;
				foreach (WeakReference wr in refs)
				{
					index++;
					if (wr.IsAlive && wr.Target == source)
						break;
				}
			
				if (index < refs.Count)
					refs.RemoveAt(index);
			}

			source.EventHandler = null;
			source.SourceElement = null;
		}
		#endregion // UnregisterCommandSource

		#endregion // Internal

		#endregion // Methods
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