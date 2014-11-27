
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities.Presentation;
using System.Windows;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels
{
    public interface IWorkflowDesignerViewModel : IDesignerViewModel
    {
        bool HasErrors { get; set; }
        object SelectedModelItem { get; }
        string WorkflowName { get; }
        bool RequiredSignOff { get; }
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }
        void UpdateWorkflowLink(string newLink);
        void Dispose();
        bool NotifyItemSelected(object primarySelection);
        void BindToModel();
        void AddMissingWithNoPopUpAndFindUnusedDataListItems();
    }
}
