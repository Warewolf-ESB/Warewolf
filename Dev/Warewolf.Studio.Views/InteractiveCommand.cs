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
            if (base.AssociatedObject != null)
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
            if (base.AssociatedObject != null)
            {
                foreach (PropertyInfo info in base.AssociatedObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (typeof(ICommand).IsAssignableFrom(info.PropertyType) && string.Equals(info.Name, CommandName, StringComparison.Ordinal))
                    {
                        command = (ICommand)info.GetValue(base.AssociatedObject, null);
                    }
                }
            }
            return command;
        }

        private string _commandName;
        public string CommandName
        {
            get
            {
                base.ReadPreamble();
                return _commandName;
            }
            set
            {
                if (CommandName != value)
                {
                    base.WritePreamble();
                    _commandName = value;
                    base.WritePostscript();
                }
            }
        }

        #region Command
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(InteractiveCommand), new UIPropertyMetadata(null));
        #endregion
    }
}