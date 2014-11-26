
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IWebResourceViewModel
    {
        string Name { get; set; }
        bool IsFolder { get; set; }
        string Uri { get; set; }
        string Base64Data { get; set; }
        bool IsRoot { get; set; }
        IWebResourceViewModel Parent { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        ObservableCollection<IWebResourceViewModel> Children { get; }
        ICommand CopyCommand { get; }
        void AddChild(IWebResourceViewModel d);
        void SetParent(IWebResourceViewModel parent);
    }
}
