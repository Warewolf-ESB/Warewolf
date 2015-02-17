using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Warewolf.Studio.Views
{
    public class InteractiveCommand : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null)
            {
                ICommand command = ResolveCommand();
                if ((command != null) && command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }

        private ICommand ResolveCommand()
        {
            ICommand command = null;
            if (Command != null)
            {
                return Command;
            }
            if (AssociatedObject != null)
            {
                foreach (PropertyInfo info in AssociatedObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (typeof(ICommand).IsAssignableFrom(info.PropertyType) && string.Equals(info.Name, CommandName, StringComparison.Ordinal))
                    {
                        command = (ICommand)info.GetValue(AssociatedObject, null);
                    }
                }
            }
            return command;
        }

        private string _commandName;
        // ReSharper disable MemberCanBePrivate.Global
        public string CommandName
            // ReSharper restore MemberCanBePrivate.Global
        {
            get
            {
                ReadPreamble();
                return _commandName;
            }
            // ReSharper disable UnusedMember.Global
            set
                // ReSharper restore UnusedMember.Global
            {
                if (CommandName != value)
                {
                    WritePreamble();
                    _commandName = value;
                    WritePostscript();
                }
            }
        }

        #region Command
        // ReSharper disable MemberCanBePrivate.Global
        public ICommand Command
            // ReSharper restore MemberCanBePrivate.Global
        {
            get { return (ICommand)GetValue(CommandProperty); }
            // ReSharper disable UnusedMember.Global
            set { SetValue(CommandProperty, value); }
            // ReSharper restore UnusedMember.Global
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(InteractiveCommand), new UIPropertyMetadata(null));
        #endregion
    }
}