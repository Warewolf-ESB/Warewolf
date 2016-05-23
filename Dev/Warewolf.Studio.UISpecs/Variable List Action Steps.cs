using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using TechTalk.SpecFlow;
using Warewolf.Studio.UISpecs.OutsideWorkflowDesignSurfaceUIMapClasses;
using System.Drawing;

namespace Warewolf.Studio.UISpecs
{
    [Binding]
    public sealed class Variable_List_Action_Steps
    {
        [When(@"I click ""(.*)"" in the variable list")]
        public void WhenIClickTheItemInTheVariableList(string variableName)
        {
            UITestControl getVariableListRow = GetTreeItemByName(variableName);
            var textbox = GetTextboxFromVariableListRow(getVariableListRow);
            Mouse.Click(textbox, new Point(50, 12));
        }

        [When(@"I click delete ""(.*)"" in the variable list")]
        public void WhenIClickDeleteTheItemInTheVariableList(string variableName)
        {
            UITestControl getVariableListRow = GetTreeItemByName(variableName);
            var deleteButton = GetDeleteButtonFromVariableListRow(getVariableListRow);
            Mouse.Click(deleteButton, new Point(12, 12));
        }

        [When(@"I toggle ""(.*)"" Input checkbox in the variable list")]
        public void WhenIToggleInputCheckboxTheItemInTheVariableList(string variableName)
        {
            UITestControl getVariableListRow = GetTreeItemByName(variableName);
            var inputCheckbox = GetInputCheckboxFromVariableListRow(getVariableListRow);
            Mouse.Click(inputCheckbox, new Point(12, 12));
        }

        [When(@"I toggle ""(.*)"" Output checkbox in the variable list")]
        public void WhenIToggleOutputCheckboxTheItemInTheVariableList(string variableName)
        {
            UITestControl getVariableListRow = GetTreeItemByName(variableName);
            var outputCheckbox = GetOutputCheckboxFromVariableListRow(getVariableListRow);
            Mouse.Click(outputCheckbox, new Point(12, 12));
        }

        private UITestControl GetTreeItemByName(string name)
        {
            var Variables = Uimap.MainStudioWindow.DockManager.SplitPaneRight.Variables.VariablesControl.VariableTreeView.VariableTreeItem.GetChildren();
            var Recordsets = Uimap.MainStudioWindow.DockManager.SplitPaneRight.Variables.VariablesControl.VariableTreeView.RecordsetTreeItem.GetChildren();

            var getVariablesTreeItems = Variables.Where(control => control.ControlType == ControlType.TreeItem);
            var CurrentTreeItem = getVariablesTreeItems.FirstOrDefault(treeitem =>
            {
                var textbox = GetTextboxFromVariableListRow(treeitem);
                return textbox.Text == name;
            });

            if (CurrentTreeItem == null)
            {
                var getRecordsetsTreeItems = Recordsets.Where(control => control.ControlType == ControlType.TreeItem);
                CurrentTreeItem = getRecordsetsTreeItems.FirstOrDefault(treeitem =>
                {
                    var textbox = GetTextboxFromVariableListRow(treeitem);
                    return textbox.Text == name;
                });

                if (CurrentTreeItem == null)
                {
                    foreach(var recordset in getRecordsetsTreeItems)
                    {
                        var field = recordset.GetChildren().FirstOrDefault(control => control.ControlType == ControlType.TreeItem);
                        CurrentTreeItem = getRecordsetsTreeItems.FirstOrDefault(treeitem =>
                        {
                            var textbox = GetTextboxFromVariableListRow(treeitem);
                            return textbox.Text == name;
                        });
                    }
                }
            }
            return CurrentTreeItem;
        }

        private static WpfEdit GetTextboxFromVariableListRow(UITestControl treeitem)
        {
            var GetPaneFromListRow = treeitem.GetChildren();
            var pane = (GetPaneFromListRow.FirstOrDefault(control => control.ControlType == ControlType.Pane) as WpfPane);
            var GetTextboxFromPane = pane.GetChildren();
            var textbox = (GetTextboxFromPane.FirstOrDefault(control => control.ControlType == ControlType.Edit) as WpfEdit);
            return textbox;
        }

        private static WpfButton GetDeleteButtonFromVariableListRow(UITestControl treeitem)
        {
            var GetButtonFromListRow = treeitem.GetChildren();
            var button = (GetButtonFromListRow.FirstOrDefault(control => control.ControlType == ControlType.Button) as WpfButton);
            return button;
        }

        private static WpfCheckBox GetInputCheckboxFromVariableListRow(UITestControl treeitem)
        {
            var GetCheckboxFromListRow = treeitem.GetChildren();
            var checkbox = (GetCheckboxFromListRow.FirstOrDefault(control => control.ControlType == ControlType.CheckBox && control.FriendlyName == "UI_IsInputCheckbox_AutoID") as WpfCheckBox);
            return checkbox;
        }

        private static WpfCheckBox GetOutputCheckboxFromVariableListRow(UITestControl treeitem)
        {
            var GetCheckboxFromListRow = treeitem.GetChildren();
            var checkbox = (GetCheckboxFromListRow.FirstOrDefault(control => control.ControlType == ControlType.CheckBox && control.FriendlyName == "UI_IsOutputCheckbox_AutoID") as WpfCheckBox);
            return checkbox;
        }

        #region Properties and Fields

        OutsideWorkflowDesignSurfaceUIMap Uimap
        {
            get
            {
                if ((_uiMap == null))
                {
                    _uiMap = new OutsideWorkflowDesignSurfaceUIMap();
                }

                return _uiMap;
            }
        }

        private OutsideWorkflowDesignSurfaceUIMap _uiMap;

        #endregion
    }
}
