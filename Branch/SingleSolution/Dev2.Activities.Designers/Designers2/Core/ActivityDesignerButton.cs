using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Activities.Designers2.Core
{
    public class ActivityDesignerButton : Button
    {
        static ICommand _underlyingCommand;

        public ActivityDesignerButton()
        {
            IsValid = true;
            Command = new RelayCommand(CommandAction);
        }

        public bool IsValid { get; private set; }

        public bool IsValidatedBefore
        {
            get { return (bool)GetValue(IsValidatedBeforeProperty); }
            set { SetValue(IsValidatedBeforeProperty, value); }
        }

        public static readonly DependencyProperty IsValidatedBeforeProperty =
            DependencyProperty.Register("IsValidatedBefore", typeof(bool), typeof(ActivityDesignerButton), new PropertyMetadata(false));

        public bool IsClosedAfter
        {
            get { return (bool)GetValue(IsClosedAfterProperty); }
            set { SetValue(IsClosedAfterProperty, value); }
        }

        public static readonly DependencyProperty IsClosedAfterProperty =
            DependencyProperty.Register("IsClosedAfter", typeof(bool), typeof(ActivityDesignerButton), new PropertyMetadata(false));

        public ICommand CustomCommand
        {
            get { return (ICommand)GetValue(CustomCommandProperty); }
            set { SetValue(CustomCommandProperty, value); }
        }

        public static readonly DependencyProperty CustomCommandProperty =
            DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(ActivityDesignerButton), new PropertyMetadata(OnCustomCommandPropertyChanged));

        static void OnCustomCommandPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            _underlyingCommand = args.NewValue as ICommand;
        }

        void CommandAction(object o)
        {
            if(IsValidatedBefore)
            {
                DoValidate();
            }

            if(IsValid)
            {
                if(_underlyingCommand != null)
                {
                    _underlyingCommand.Execute(null);
                }

                if(IsClosedAfter)
                {
                    DoClose();
                }
            }
        }

        void DoValidate()
        {
            var validator = DataContext as IValidator;
            if(validator != null)
            {
                validator.Validate();
                IsValid = validator.IsValid;
            }
        }

        void DoClose()
        {
            var closable = DataContext as IClosable;
            if(closable != null)
            {
                closable.IsClosed = true;
            }
        }

    }
}