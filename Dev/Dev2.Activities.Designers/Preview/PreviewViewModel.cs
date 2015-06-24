
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Windows;
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

        public RelayCommand PreviewCommand { get; private set; }

        public bool CanPreview
        {
            get
            {
                return _canPreview;
            }
            set
            {
                OnPropertyChanged(ref _canPreview, value);
                PreviewCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsPreviewFocused { get { return _isPreviewFocused; } set { OnPropertyChanged(ref _isPreviewFocused, value); } }

        public string Output { get { return _output; } set { OnPropertyChanged(ref _output, value); } }

        public ObservableCollection<ObservablePair<string, string>> Inputs { get; set; }

        public Visibility InputsVisibility { get { return _inputsVisibility; } set { OnPropertyChanged(ref _inputsVisibility, value); } }

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
