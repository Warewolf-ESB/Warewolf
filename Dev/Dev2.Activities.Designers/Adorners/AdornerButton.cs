using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Activities.Adorners
{
    public class AdornerButton : Button
    {
        private static ICommand _underlyingCommand;
        public  bool IsValid { get; private set; }

        public AdornerButton()
        {
            IsValid = true;
            Command = new RelayCommand(CommandAction);
        }

        private void CommandAction(object o)
        {
            if (IsValidatedBefore)
            {
                DoValidate();
            }

            if (IsValid)
            {
                if (_underlyingCommand != null)
                {
                    _underlyingCommand.Execute(null);
                }

                if (IsClosedAfter)
                {
                    DoClose();
                }
            }
        }

        private void DoValidate()
        {
            var datacontext = DataContext as IValidator;
            if (datacontext != null)
            {
                IsValid = !datacontext.ValidationErrors().Any();
            }
        }

        private void DoClose()
        {
            var datacontext = DataContext as IOverlayManager;
            if (datacontext != null)
            {
                datacontext.HideContent();
            }
        }

        #region Dependency Properties

        // Using a DependencyProperty as the backing store for IsValidatedBefore.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidatedBeforeProperty =
            DependencyProperty.Register("IsValidatedBefore", typeof (bool), typeof (AdornerButton),
                                        new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for IsClosedAfter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsClosedAfterProperty =
            DependencyProperty.Register("IsClosedAfter", typeof (bool), typeof (AdornerButton),
                                        new PropertyMetadata(false));


        // Using a DependencyProperty as the backing store for CustomCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomCommandProperty =
            DependencyProperty.Register("CustomCommand", typeof (ICommand), typeof (AdornerButton),
                                        new PropertyMetadata(OnCustomCommandPropertyChanged));

        public bool IsValidatedBefore
        {
            get { return (bool) GetValue(IsValidatedBeforeProperty); }
            set { SetValue(IsValidatedBeforeProperty, value); }
        }

        public bool IsClosedAfter
        {
            get { return (bool) GetValue(IsClosedAfterProperty); }
            set { SetValue(IsClosedAfterProperty, value); }
        }

        public ICommand CustomCommand
        {
            get { return (ICommand) GetValue(CustomCommandProperty); }
            set { SetValue(CustomCommandProperty, value); }
        }

        #endregion

        #region OnCustomCommandPropertyChanged

        private static void OnCustomCommandPropertyChanged(DependencyObject dependencyObject,
                                                           DependencyPropertyChangedEventArgs args)
        {
            _underlyingCommand = args.NewValue as ICommand;
        }

        #endregion
    }
}