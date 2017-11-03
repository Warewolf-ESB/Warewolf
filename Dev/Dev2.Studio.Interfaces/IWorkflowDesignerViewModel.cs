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
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Text;
using System.Windows;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface IWorkflowDesignerViewModel : IDesignerViewModel, IDisposable
    {
        object SelectedModelItem { get; }
        string WorkflowName { get; }
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }
        StringBuilder DesignerText { get; }
        Action<ModelItem> ItemSelectedAction { get; set; }
        bool IsTestView { get; set; }
        ModelItem SelectedItem { get; set; }
        bool WorkspaceSave { get; }
        Action WorkflowChanged { get; set; }
        bool CanViewWorkflowLink { get; set; }

        void UpdateWorkflowLink(string newLink);
        bool NotifyItemSelected(object primarySelection);
        void BindToModel();
        void AddMissingWithNoPopUpAndFindUnusedDataListItems();
        ModelItem GetModelItem(Guid workSurfaceMappingId, Guid parentID);

        string GetWorkflowInputs(string field);
        void CreateBlankWorkflow();
        void RemoveItem(IMergeToolModel model);
        void AddItem(IMergeToolModel model);
        void RemoveStartNodeConnection();
        void LinkTools(string sourceUniqueId, string destionationUniqueId, string key);
    }
}
