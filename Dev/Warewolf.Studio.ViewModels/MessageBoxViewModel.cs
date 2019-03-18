#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using FontAwesome.WPF;
using Microsoft.Practices.Prism.Commands;


namespace Warewolf.Studio.ViewModels
{
    public class MessageBoxViewModel : Screen
    {
        MessageBoxButton _buttons = MessageBoxButton.OK;
        string _message;
        string _title;
        FontAwesomeIcon _icon;
        bool _isError;
        bool _isInfo;
        bool _isQuestion;
        List<string> _urlsFound;
        bool _isDuplicatesVisible;

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
