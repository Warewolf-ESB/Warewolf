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
        private MessageBoxResult _result = MessageBoxResult.None;
        FontAwesomeIcon _icon;
        bool _isError;
        bool _isInfo;
        bool _isQuestion;
        private List<string> _urlsFound;

        public MessageBoxViewModel(string message, string title, MessageBoxButton buttons, FontAwesomeIcon icon, bool isDependenciesButtonVisible,
            bool isError, bool isInfo, bool isQuestion, List<string> urlsFound)
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

        public bool IsNoButtonVisible => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

        public bool IsYesButtonVisible => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

        public bool IsCancelButtonVisible => _buttons == MessageBoxButton.OKCancel || _buttons == MessageBoxButton.YesNoCancel;

        public bool IsDependenciesButtonVisible { get; set; }

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

        public MessageBoxResult Result => _result;

        public void No()
        {
            _result = MessageBoxResult.No;
            TryClose(false);
        }

        public void Yes()
        {
            _result = MessageBoxResult.Yes;
            TryClose(true);
        }

        public void Cancel()
        {
            _result = MessageBoxResult.Cancel;
            TryClose(false);
        }

        public void Ok()
        {
            _result = MessageBoxResult.OK;
            TryClose(true);
        }
    }
}
