
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using Dev2.Communication;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IContextualResourceModel : IResourceModel, INotifyPropertyChanged
    {
        IEnvironmentModel Environment { get; }
        Guid ServerID { get; set; }
        void UpdateIconPath(string iconPath);
        bool IsNewWorkflow { get; set; }
        event Action<IContextualResourceModel> OnResourceSaved;
        event Action OnDataListChanged;
        event EventHandler<DesignValidationMemo> OnDesignValidationReceived;
        void ClearErrors();
    }
}
