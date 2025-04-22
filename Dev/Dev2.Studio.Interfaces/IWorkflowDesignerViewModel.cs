#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
#if WINDOWS || NETFRAMEWORK
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
#endif
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface IWorkflowDesignerViewModel : IDesignerViewModel, IDisposable
    {
        object SelectedModelItem { get; }
        string WorkflowName { get; }
#if WINDOWS || NETFRAMEWORK
        WorkflowDesigner Designer { get; }
        UIElement DesignerView { get; }
#endif
        StringBuilder DesignerText { get; }
#if WINDOWS || NETFRAMEWORK
        Action<ModelItem> ItemSelectedAction { get; set; }
#endif
        bool IsTestView { get; set; }
#if WINDOWS || NETFRAMEWORK
        ModelItem SelectedItem { get; set; }
#endif
        bool WorkspaceSave { get; }
        Action WorkflowChanged { get; set; }
        bool CanViewWorkflowLink { get; set; }
        bool CanMerge { get; set; }

        bool NotifyItemSelected(object primarySelection);
        void BindToModel();
        void AddMissingWithNoPopUpAndFindUnusedDataListItems();
#if WINDOWS || NETFRAMEWORK
        ModelItem GetModelItem(Guid workSurfaceMappingId, Guid parentID);
#endif

        string GetWorkflowInputs(string field);
        void CreateBlankWorkflow();
        void UpdateWorkflowInputDataViewModel(IContextualResourceModel resourceModel);
        string GetAndUpdateWorkflowLinkWithWorkspaceID();
        List<NameValue> GetSelectableGates(string uniqueId);
    }

    public interface IMergePreviewWorkflowDesignerViewModel : IWorkflowDesignerViewModel
    {
        void RemoveItem(IToolConflictItem model);
        void AddItem(IToolConflictItem model);
        void RemoveStartNodeConnection();
        void LinkStartNode(IToolConflictItem model);
        void LinkActivities(Guid sourceUniqueId, Guid destinationUniqueId, string key);
        void DeLinkActivities(Guid sourceUniqueId, Guid destinationUniqueId, string key);
    }
}
