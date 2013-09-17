using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;

namespace Dev2.Activities.Preview
{
    public class PreviewViewModel : ObservableObject
    {
        string _output = string.Empty;
        bool _canPreview;
        Visibility _inputsVisibility;
        bool _isPreviewFocused;

        public PreviewViewModel()
        {
            Inputs = new ObservableCollection<ObservablePair<string, string>>();
            PreviewCommand = new RelayCommand(OnPreviewRequested, obj => CanPreview);
        }

        public event EventHandler<PreviewRequestedEventArgs> PreviewRequested;

        public ICommand PreviewCommand { get; private set; }

        public bool CanPreview { get { return _canPreview; } set { OnPropertyChanged("CanPreview", ref _canPreview, value); } }

        public bool IsPreviewFocused { get { return _isPreviewFocused; } set { OnPropertyChanged("IsPreviewFocused", ref _isPreviewFocused, value); } }

        public string Output { get { return _output; } set { OnPropertyChanged("Output", ref _output, value); } }

        public ObservableCollection<ObservablePair<string, string>> Inputs { get; set; }

        public Visibility InputsVisibility { get { return _inputsVisibility; } set { OnPropertyChanged("InputsVisibility", ref _inputsVisibility, value); } }

        void OnPreviewRequested(object obj)
        {
            var handler = PreviewRequested;
            if(handler != null)
            {
                var args = new PreviewRequestedEventArgs();
                handler(this, args);
            }
        }
    }
}