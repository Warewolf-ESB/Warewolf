
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
