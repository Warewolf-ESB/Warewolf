using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dev2.Studio.UI.Tests.Extensions
{
    public static class UiTestControlExtentions
    {
        public static UITestControl GetChildByAutomationIDPath(this UITestControl parent, params string[] automationIDs)
        {
            return VisualTreeWalker.GetChildByAutomationIdPath(parent, automationIDs);
        }

        public static List<UITestControl> FindControlsControlByAutomationID(this UITestControl container, string automationId)
        {
            List<UITestControl> parentCollection = container.GetChildren()
                                                            .Where(c => !(c is WpfListItem) && c is WpfControl)
                                                            .ToList();

            var control = parentCollection.Where(b => ((WpfControl)b).AutomationId.Equals(automationId)).ToList();

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if(control != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                return control;
            }

            return control;
        }

        public static UITestControl FindByAutomationId(this UITestControl container, string automationId, bool returnNullIfNotFound)
        {
            List<UITestControl> parentCollection = container.GetChildren()
                                                            .Where(c => c is WpfControl)
                                                            .ToList();

            var control = parentCollection.FirstOrDefault(b => ((WpfControl)b).AutomationId.Equals(automationId));

            if(control != null)
            {
                return control;
            }

            while(parentCollection.Count > 0)
            {
                var uiTestControlCollection = parentCollection
                    .SelectMany(c => c.GetChildren())
                    .ToList();

                control = uiTestControlCollection
                    .FirstOrDefault(b => ((WpfControl)b).AutomationId.Equals(automationId));

                if(control == null)
                {
                    parentCollection = uiTestControlCollection;
                }
                else
                {
                    break;
                }
            }

            if(control == null && !returnNullIfNotFound)
            {
                string message = string.Format("Control with automation id : [{0}] was not found", automationId);
                throw new Exception(message);
            }

            return control;
        }

        public static UITestControl FindByFriendlyName(this UITestControl container, string friendlyName)
        {
            List<UITestControl> parentCollection = container.GetChildren()
                                                            .Where(c => !(c is WpfListItem) && c is WpfControl)
                                                            .ToList();

            var control = parentCollection.SingleOrDefault(b => b.FriendlyName.Equals(friendlyName));

            if(control != null)
            {
                return control;
            }

            while(parentCollection.Count > 0)
            {
                var uiTestControlCollection = parentCollection
                    .SelectMany(c => c.GetChildren())
                    .ToList();

                control = uiTestControlCollection
                    .SingleOrDefault(b => b.FriendlyName.Equals(friendlyName));

                if(control == null)
                {
                    parentCollection = uiTestControlCollection;
                }
                else
                {
                    break;
                }
            }

            if(control == null)
            {
                string message = string.Format("Control with friendly name : [{0}] was not found", friendlyName);
                throw new Exception(message);
            }

            return control;
        }

        public static void Click(this UITestControl control)
        {
            if(control is WpfListItem)
            {
                var uiTestControl = control.GetParent();
                Mouse.Click(uiTestControl, new Point(5, 5));
            }

            Mouse.Click(control, new Point(5, 5));
            var checkBox = control as WpfCheckBox;
            var combobox = control as WpfComboBox;
            var wpfListItem = control as WpfListItem;
            if(checkBox == null && combobox == null && wpfListItem == null)
            {
                var clickablePoint = control.GetClickablePoint();
                clickablePoint.Offset(5, 5);
                Mouse.Click(control, clickablePoint);
                Playback.Wait(100);
            }
        }

        public static void DoubleClick(this UITestControl control)
        {
            Mouse.Click(control, new Point(5, 5));
            var clickablePoint = control.GetClickablePoint();
            clickablePoint.Offset(5, 5);
            Mouse.DoubleClick(control, clickablePoint);
            Playback.Wait(100);
        }

        public static void EnterText(this UITestControl control, string text)
        {
            WpfEdit editControl = control as WpfEdit;
            if(editControl == null)
            {
                throw new Exception("Cannot enter text in a non editable control");
            }
            if(editControl.Text != text)
            {
                editControl.Text = text;
                control.WaitForControlReady();
            }
        }

        public static void AppendText(this UITestControl control, string text)
        {
            ////Enter text
            WpfEdit editControl = control as WpfEdit;

            if(editControl == null)
            {
                throw new Exception("Cannot enter text in a non editable control");
            }

            editControl.Text += text;
            control.WaitForControlReady();
        }

        public static void ClearText(this UITestControl control)
        {
            WpfEdit editControl = control as WpfEdit;
            if(editControl == null)
            {
                throw new Exception("Cannot enter text in a non editable control");
            }

            editControl.Text = string.Empty;
            control.WaitForControlReady();
        }

        public static void Check(this UITestControl control, bool isChecked)
        {
            var checkBox = control as WpfCheckBox;

            if(checkBox == null)
            {
                throw new Exception("Control must be a check box");
            }

            checkBox.Checked = isChecked;
        }

        public static bool IsChecked(this UITestControl control)
        {
            var checkBox = control as WpfCheckBox;

            if(checkBox == null)
            {
                throw new Exception("Control must be a check box");
            }

            return checkBox.Checked;
        }

        public static bool IsSelected(this UITestControl control)
        {
            var radioButton = control as WpfRadioButton;
            if(radioButton == null)
            {
                throw new Exception("Control must be a radio button");
            }
            return radioButton.Selected;
        }

        public static bool IsEnabled(this UITestControl control)
        {
            return control.Enabled;
        }

        public static string GetText(this UITestControl control)
        {
            var editControl = control as WpfEdit;

            if(editControl == null)
            {
                throw new Exception("This is not an editable control");
            }

            return editControl.Text;
        }

        public static void Select(this UITestControl control, int selectedIndex)
        {
            var editControl = control as WpfComboBox;

            if(editControl == null)
            {
                throw new Exception("This is not an editable control");
            }

            editControl.SelectedIndex = selectedIndex;
        }
    }
}