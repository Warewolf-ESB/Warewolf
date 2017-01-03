using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using FontAwesome.WPF;
using Microsoft.Practices.Prism.Commands;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Warewolf.Studio.ViewModels
{
    public class MessageBoxViewModel : Screen
    {
        private MessageBoxButton _buttons = MessageBoxButton.OK;
        private string _message;
        private string _title;
        FontAwesomeIcon _icon;
        bool _isError;
        bool _isInfo;
        bool _isQuestion;
        private List<string> _urlsFound;
        private bool _isDuplicatesVisible;

        public MessageBoxViewModel(string message, string title, MessageBoxButton buttons, FontAwesomeIcon icon, bool isDependenciesButtonVisible,
            bool isError, bool isInfo, bool isQuestion, List<string> urlsFound, bool isDeleteAnywayButtonVisible, bool applyToAll)
        {
            Title = title;
            IsError = isError;
            IsInfo = isInfo;
            IsQuestion = isQuestion;
            Message = message;
            UrlsFound = urlsFound;
            Buttons = buttons;
            Icon = icon;
            YesCommand = new DelegateCommand(Yes);
            NoCommand = new DelegateCommand(No);
            CancelCommand = new DelegateCommand(Cancel);
            OkCommand = new DelegateCommand(Ok);
            IsDependenciesButtonVisible = isDependenciesButtonVisible;
            IsDeleteAnywayButtonVisible = isDeleteAnywayButtonVisible;
            ApplyToAll = applyToAll;
        }

        public List<string> UrlsFound
        {
            get { return _urlsFound; }
            set
            {
                _urlsFound = value;
                NotifyOfPropertyChange(() => UrlsFound);
            }
        }

        FontAwesomeIcon Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public bool IsError
        {
            get { return _isError; }
            set
            {
                _isError = value;
                NotifyOfPropertyChange(() => IsError);
            }
        }
        public bool IsInfo
        {
            get { return _isInfo; }
            set
            {
                _isInfo = value;
                NotifyOfPropertyChange(() => IsInfo);
            }
        }
        public bool IsQuestion
        {
            get { return _isQuestion; }
            set
            {
                _isQuestion = value;
                NotifyOfPropertyChange(() => IsQuestion);
            }
        }
        public bool IsDuplicatesVisible
        {
            get { return _isDuplicatesVisible; }
            set
            {
                _isDuplicatesVisible = value;
                NotifyOfPropertyChange(() => IsDuplicatesVisible);
            }
        }

        public bool IsNoButtonVisible => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

        public bool IsYesButtonVisible => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

        public bool IsCancelButtonVisible => _buttons == MessageBoxButton.OKCancel || _buttons == MessageBoxButton.YesNoCancel;

        public bool IsDependenciesButtonVisible { get; set; }

        public bool IsDeleteAnywayButtonVisible { get; set; }

        public bool IsDeleteAnywaySelected { get; set; }

        public bool ApplyToAll { get; set; }

        public bool IsOkButtonVisible => _buttons == MessageBoxButton.OK || _buttons == MessageBoxButton.OKCancel;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        ICommand YesCommand { get; set; }
        ICommand NoCommand { get; set; }
        ICommand CancelCommand { get; set; }
        ICommand OkCommand { get; set; }

        public MessageBoxButton Buttons
        {
            get { return _buttons; }
            set
            {
                _buttons = value;
                NotifyOfPropertyChange(() => IsNoButtonVisible);
                NotifyOfPropertyChange(() => IsYesButtonVisible);
                NotifyOfPropertyChange(() => IsCancelButtonVisible);
                NotifyOfPropertyChange(() => IsOkButtonVisible);
            }
        }

        public MessageBoxResult Result { get; set; } = MessageBoxResult.None;

        public void No()
        {
            Result = MessageBoxResult.No;
            TryClose(false);
        }

        public void Yes()
        {
            Result = MessageBoxResult.Yes;
            TryClose(true);
        }

        public void Cancel()
        {
            Result = MessageBoxResult.Cancel;
            TryClose(false);
        }

        public void Ok()
        {
            Result = MessageBoxResult.OK;
            TryClose(true);
        }
    }
}
