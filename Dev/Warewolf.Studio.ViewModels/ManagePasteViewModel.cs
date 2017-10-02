using System;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ManagePasteViewModel : BindableBase
    {
        readonly string _originalText;
        readonly Action _action;
        DelegateCommand _saveCommand;
        DelegateCommand _cancelCommand;
        string _text;

        public ManagePasteViewModel(string text, Action action)
        {
            _originalText = text;
            _action = action;
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            Text = text;
        }

        void Cancel()
        {
            Text = _originalText;
            _action();
        }

        void Save()
        {
            _action();
        }

        public DelegateCommand CancelCommand
        {
            get
            {
                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
            }
        }

        public DelegateCommand SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                _saveCommand = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                OnPropertyChanged(() => Text);
            }
        }
    }
}