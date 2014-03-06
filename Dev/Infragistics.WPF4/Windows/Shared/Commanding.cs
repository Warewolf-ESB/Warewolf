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
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls
{
	/// <summary>
	/// An object that contains attached properties for hooking up commands to <see cref="FrameworkElement"/> objects.
	/// </summary>
	public static class Commanding
	{
		#region Properties

		#region Command

		/// <summary>
		/// An attached property that Gets/Sets the <see cref="CommandSource"/> that should be attached to a <see cref="FrameworkElement"/>
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(CommandSource), typeof(Commanding), new PropertyMetadata(null, OnCommandChanged));

		/// <summary>
		/// Gets the <see cref="CommandSource"/> attached to a specified <see cref="FrameworkElement"/>
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static CommandSource GetCommand(FrameworkElement element)
		{
			return (CommandSource)element.GetValue(CommandProperty);
		}
				
		/// <summary>
		/// Sets the <see cref="CommandSource"/> that should be attached to the specified <see cref="FrameworkElement"/>
		/// </summary>
		/// <param name="element"></param>
		/// <param name="command"></param>
		public static void SetCommand(FrameworkElement element, CommandSource command)
		{
			element.SetValue(CommandProperty, command);
		}

		static void OnCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement elem = obj as FrameworkElement;

			CommandSource oldCommand = e.OldValue as CommandSource;
			if (oldCommand != null)
				CommandSourceManager.UnregisterCommandSource(oldCommand);

			CommandSource newCommand = e.NewValue as CommandSource;
			if (newCommand != null)
				CommandSourceManager.RegisterCommandSource(newCommand, elem);
		}

		#endregion // Command

		#region Commands

		/// <summary>
		/// Gets a collection of <see cref="CommandSource"/> objects that should be attached to a specified <see cref="FrameworkElement"/>
		/// </summary>
        public static readonly DependencyProperty CommandsProperty = DependencyProperty.RegisterAttached("Commands", typeof(CommandSourceCollection), typeof(Commanding), new PropertyMetadata(null, OnCommandsChanged));

		/// <summary>
		/// Gets a collection of <see cref="CommandSource"/> objects that should be attached to a specified <see cref="FrameworkElement"/>
		/// </summary>
		public static CommandSourceCollection GetCommands(FrameworkElement element)
		{
            CommandSourceCollection commands = (CommandSourceCollection)element.GetValue(CommandsProperty);

            if (commands == null)
            {
                commands = new CommandSourceCollection();
                element.SetValue(CommandsProperty, commands);
            }

            if (commands.Element == null)
            {
                commands.Element = element;
            }

            return commands;
		}

        /// <summary>
        /// Sets a collection of <see cref="CommandSource"/> objects that should be attached to a specified <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="commands"></param>
        public static void SetCommands(FrameworkElement elem, CommandSourceCollection commands)
        {
            elem.SetValue(CommandsProperty, commands);
        }
        

        static void OnCommandsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
            CommandSourceCollection commands = (CommandSourceCollection)e.NewValue;
            if (commands != null)
            {
                commands.Element = obj as FrameworkElement;
            }
		}

		#endregion // Commands

        #region CommandTarget

        /// <summary>
        /// An attached property that Gets/Sets the CommandTarget that should be attached to a <see cref="FrameworkElement"/>
        /// </summary>
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.RegisterAttached("CommandTarget", typeof(FrameworkElement), typeof(Commanding), new PropertyMetadata(null, OnCommandTargetChanged));

        /// <summary>
        /// Gets the <see cref="ICommandTarget"/> that commands should be triggered off of. 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static FrameworkElement GetCommandTarget(FrameworkElement element)
        {
            return (FrameworkElement)element.GetValue(CommandTargetProperty);
        }

        /// <summary>
        /// Sets the <see cref="ICommandTarget"/> that commands should be triggered off of.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="commandTarget"></param>
        public static void SetCommandTarget(FrameworkElement element, FrameworkElement commandTarget)
        {
            element.SetValue(CommandTargetProperty, commandTarget);
        }

        static void OnCommandTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
                        
        }

        #endregion // CommandTarget

        #endregion // Properties
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