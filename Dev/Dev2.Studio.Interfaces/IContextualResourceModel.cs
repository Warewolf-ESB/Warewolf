/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using Microsoft.Practices.Prism.Mvvm;

// ReSharper disable InconsistentNaming


namespace Dev2.Studio.Interfaces
{
    public interface IContextualResourceModel : IResourceModel, INotifyPropertyChanged
    {
        IServer Environment { get; }
        Guid ServerID { get; set; }
        bool IsNewWorkflow { get; set; }
        event Action<IContextualResourceModel> OnResourceSaved;
        event Action OnDataListChanged;
        IView GetView(Func<IView> view);

        void ClearErrors();        
    }
}
