using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class FileChooserMessage : ObservableObject, IMessage
    {
        IEnumerable<string> _selectedFiles;

        public FileChooserMessage(IEnumerable<string> selectedFiles = null)
        {
            _selectedFiles = selectedFiles;
        }

        public IEnumerable<string> SelectedFiles { get { return _selectedFiles; } set { OnPropertyChanged(ref _selectedFiles, value); } }
    }
}