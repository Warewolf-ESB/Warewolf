// Copyright (C) Josh Smith - July 2008
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Markup;

namespace WPF.JoshSmith.Input
{
    /// <summary>
    /// This is a command that simply aggregates other commands into a group.
    /// This command's CanExecute logic delegates to the CanExecute logic of 
    /// all the child commands.  When executed, it calls the Execute method
    /// on each child command sequentially.
    /// </summary>
    /// <remarks>
    /// Documentation: http://www.codeproject.com/KB/WPF/commandgroup.aspx
    /// </remarks>
    [ContentProperty("Commands")]
    public class CommandGroup : ICommand
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance.
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
        /// Returns the collection of child commands.  They are executed
        /// in the order that they exist in this collection.
        /// </summary>
        public ObservableCollection<ICommand> Commands
        {
            get
            {
                if(_commands == null)
                {
                    _commands = new ObservableCollection<ICommand>();
                    _commands.CollectionChanged += this.OnCommandsCollectionChanged;
                }

                return _commands;
            }
        }

        void OnCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We have a new child command so our ability to execute may have changed.
            this.OnCanExecuteChanged();

            if(e.NewItems != null && 0 < e.NewItems.Count)
            {
                foreach(ICommand cmd in e.NewItems)
                    cmd.CanExecuteChanged += this.OnChildCommandCanExecuteChanged;
            }

            if(e.OldItems != null && 0 < e.OldItems.Count)
            {
                foreach(ICommand cmd in e.OldItems)
                    cmd.CanExecuteChanged -= this.OnChildCommandCanExecuteChanged;
            }
        }

        void OnChildCommandCanExecuteChanged(object sender, EventArgs e)
        {
            // Bubble up the child commands CanExecuteChanged event so that
            // it will be observed by WPF.
            this.OnCanExecuteChanged();
        }

        #endregion // Commands

        #region ICommand Members

        /// <summary>
        /// Returns true if all of the commands in the group can execute.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            foreach(ICommand cmd in this.Commands)
                if(!cmd.CanExecute(parameter))
                    return false;

            return true;
        }

        /// <summary>
        /// Raised when something changes whether the command can or cannot execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Subclasses can invoke this method to raise the CanExecuteChanged event.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            if(this.CanExecuteChanged != null)
                this.CanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Executes each command in the group sequentially.
        /// </summary>
        public void Execute(object parameter)
        {
            foreach(ICommand cmd in this.Commands)
                cmd.Execute(parameter);
        }

        #endregion // ICommand Members
    }
}