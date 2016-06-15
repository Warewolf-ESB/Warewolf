/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - July 2008

using System;
using System.Windows;
using System.Windows.Input;
using Warewolf.Resource.Errors;

namespace WPF.JoshSmith.Input
{
    /// <summary>
    ///     This abstract class is a RoutedCommand which allows its
    ///     subclasses to provide default logic for determining if
    ///     they can execute and how to execute.  To enable the default
    ///     logic to be used, set the IsCommandSink attached property
    ///     to true on the root element of the element tree which uses
    ///     one or more SmartRoutedCommand subclasses.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/SmartRoutedCommandsInWPF.aspx
    /// </remarks>
    public abstract class SmartRoutedCommand : RoutedCommand
    {
        #region IsCommandSink

        /// <summary>
        ///     Represents the IsCommandSink attached property.  This field is readonly.
        /// </summary>
        public static readonly DependencyProperty IsCommandSinkProperty =
            DependencyProperty.RegisterAttached(
                "IsCommandSink",
                typeof (bool),
                typeof (SmartRoutedCommand),
                new UIPropertyMetadata(false, OnIsCommandSinkChanged));

        /// <summary>
        ///     Invoked when the IsCommandSink attached property is set on an element.
        /// </summary>
        /// <param name="depObj">The element on which the property was set.</param>
        /// <param name="e">Information about the property setting.</param>
        private static void OnIsCommandSinkChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var isCommandSink = (bool) e.NewValue;

            var sinkElem = depObj as UIElement;
            if (sinkElem == null)
                throw new ArgumentException(ErrorResource.TargetobjectMustBeUIElement);

            if (isCommandSink)
            {
                CommandManager.AddCanExecuteHandler(sinkElem, OnCanExecute);
                CommandManager.AddExecutedHandler(sinkElem, OnExecuted);
            }
            else
            {
                CommandManager.RemoveCanExecuteHandler(sinkElem, OnCanExecute);
                CommandManager.RemoveExecutedHandler(sinkElem, OnExecuted);
            }
        }

        #endregion // IsCommandSink

        #region Static Callbacks

        private static void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var cmd = e.Command as SmartRoutedCommand;
            if (cmd != null)
            {
                e.CanExecute = cmd.CanExecuteCore(e.Parameter);
            }
        }

        private static void OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var cmd = e.Command as SmartRoutedCommand;
            if (cmd != null)
            {
                cmd.ExecuteCore(e.Parameter);
                e.Handled = true;
            }
        }

        #endregion // Static Callbacks

        #region Abstract Methods

        /// <summary>
        ///     Child classes override this method to provide logic which
        ///     determines if the command can execute.  This method will
        ///     only be invoked if no element in the tree indicated that
        ///     it can execute the command.
        /// </summary>
        /// <param name="parameter">The command parameter (optional).</param>
        /// <returns>True if the command can be executed, else false.</returns>
        protected abstract bool CanExecuteCore(object parameter);

        /// <summary>
        ///     Child classes override this method to provide default
        ///     execution logic.  This method will only be invoked if
        ///     CanExecuteCore returns true.
        /// </summary>
        /// <param name="parameter">The command parameter (optional).</param>
        protected abstract void ExecuteCore(object parameter);

        #endregion // Abstract Methods
    }
}