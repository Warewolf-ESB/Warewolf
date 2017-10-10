/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;


namespace Dev2.Studio.Core.Messages
{
    public class FileChooserMessage : ObservableObject, IMessage
    {
        IEnumerable<string> _selectedFiles;

        public FileChooserMessage()
            : this(null)
        {
        }

        public FileChooserMessage(IEnumerable<string> selectedFiles)
        {
            _selectedFiles = selectedFiles;
        }

        public IEnumerable<string> SelectedFiles { get => _selectedFiles; set => OnPropertyChanged(ref _selectedFiles, value); }
        public string Filter { get; set; }
    }
}
