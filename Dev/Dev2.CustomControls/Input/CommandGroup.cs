/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - July 2008

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Markup;

namespace WPF.JoshSmith.Input
{
    /// <summary>
    ///     This is a command that simply aggregates other commands into a group.
    ///     This command's CanExecute logic delegates to the CanExecute logic of
    ///     all the child commands.  When executed, it calls the Execute method
    ///     on each child command sequentially.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/commandgroup.aspx
    /// </remarks>
    [ContentProperty("Commands")]
    public class CommandGroup : ICommand
    {
        #region Constructor

        /// <summary>
        ///     Initializes a new instance.
        /// </summary>
        // ReSharper disable EmptyConstructor
        public CommandGroup()
            // ReSharper restore EmptyConstructor
        {
            // Parameterless public ctor required for XAML instantiation.
        }

        #endregion // Constructor

        #region Commands

        private ObservableCollection<ICommand> _commands;

        /// <summary>
        ///     Returns the collection of child commands.  They are executed
        ///     in the order that they exist in this collection.
        /// </summary>
        public ObservableCollection<ICommand> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = new ObservableCollection<ICommand>();
                    _commands.CollectionChanged += OnCommandsCollectionChanged;
                }

                return _commands;
            }
        }

        private void OnCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We have a new child command so our ability to execute may have changed.
            OnCanExecuteChanged();

            if (e.NewItems != null && 0 < e.NewItems.Count)
            {
                foreach (ICommand cmd in e.NewItems)
                    cmd.CanExecuteChanged += OnChildCommandCanExecuteChanged;
            }

            if (e.OldItems != null && 0 < e.OldItems.Count)
            {
                foreach (ICommand cmd in e.OldItems)
                    cmd.CanExecuteChanged -= OnChildCommandCanExecuteChanged;
            }
        }

        private void OnChildCommandCanExecuteChanged(object sender, EventArgs e)
        {
            // Bubble up the child commands CanExecuteChanged event so that
            // it will be observed by WPF.
            OnCanExecuteChanged();
        }

        #endregion // Commands

        #region ICommand Members

        /// <summary>
        ///     Returns true if all of the commands in the group can execute.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (ICommand cmd in Commands)
                // ReSharper restore LoopCanBeConvertedToQuery
                if (!cmd.CanExecute(parameter))
                    return false;

            return true;
        }

        /// <summary>
        ///     Raised when something changes whether the command can or cannot execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     Executes each command in the group sequentially.
        /// </summary>
        public void Execute(object parameter)
        {
            foreach (ICommand cmd in Commands)
                cmd.Execute(parameter);
        }

        /// <summary>
        ///     Subclasses can invoke this method to raise the CanExecuteChanged event.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion // ICommand Members
    }
}