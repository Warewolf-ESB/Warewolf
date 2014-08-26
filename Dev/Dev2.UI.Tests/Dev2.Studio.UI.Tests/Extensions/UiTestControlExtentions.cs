using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
                var collectionToSearch = parentCollection
                                .SelectMany(c => c.GetChildren())
                                .Where(c => c is WpfControl)
                                .ToList();

                control = collectionToSearch
                    .FirstOrDefault(b => ((WpfControl)b).AutomationId.Equals(automationId));

                if(control == null)
                {
                    parentCollection = collectionToSearch;
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

        public static UITestControl FindByFriendlyName(this UITestControl container, string friendlyName, bool returnNullIfNotFound)
        {
            List<UITestControl> parentCollection = container.GetChildren()
                                                            .Where(c => !(c is WpfListItem) && c is WpfControl)
                                                            .ToList();

            var cleanName = friendlyName.Replace("*", "");
            var control = parentCollection.SingleOrDefault(b => b.FriendlyName.Equals(cleanName)
                                                             || b.FriendlyName.StartsWith(cleanName)
                                                             || ((WpfControl)b).AutomationId.Contains(cleanName));

            if(control != null)
            {
                return control;
            }

            while(parentCollection.Count > 0)
            {
                var uiTestControlCollection = parentCollection
                    .SelectMany(c => c.GetChildren())
                    .Where(c => c is WpfControl)
                    .ToList();

                control = uiTestControlCollection
                    .SingleOrDefault(b => b.FriendlyName.Equals(cleanName)
                                        || b.FriendlyName.StartsWith(cleanName)
                                        || ((WpfControl)b).AutomationId.Contains(cleanName));

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
                string message = string.Format("Control with friendly name : [{0}] was not found", cleanName);
                throw new Exception(message);
            }

            return control;
        }

        public static void Click(this UITestControl control)
        {
            try
            {
                switch(control.ControlType.ToString())
                {
                    case "Button":
                        {
                            Mouse.Click(control);
                            break;
                        }
                    case "Custom":
                        {
                            var point = GetClickablePoint(control);
                            Mouse.Click(point);
                            break;
                        }
                    case "CheckBox":
                    case "ComboBox":
                    case "List":
                    case "RadioButton":
                        {
                            Mouse.Click(control, new Point(5, 5));
                            break;
                        }
                    case "TreeItem":
                    case "ListItem":
                    case "MenuItem":
                        {
                            Mouse.Click(control.GetParent());
                            Mouse.Click(control, new Point(5, 5));
                            break;
                        }
                    default:
                        {
                            Mouse.Click(control.GetParent());
                            Mouse.Click(control, new Point(5, 5));
                            var clickablePoint = control.GetClickablePoint();
                            clickablePoint.Offset(5, 5);
                            Mouse.Click(control, clickablePoint);
                            Playback.Wait(100);
                            break;
                        }
                }
            }
            catch(Exception)
            {
                var point = GetClickablePoint(control);
                Mouse.Click(point);
            }
        }

        public static bool HasClickableParent(this UITestControl control)
        {
            return !(control is WpfCheckBox || control is WpfComboBox || control is WpfList || control is WpfListItem);
        }

        public static void RightClick(this UITestControl control)
        {
            try
            {
                string state = control.State.ToString().ToLower();
                if(state.Contains("invisible") || state.Contains("offscreen"))
                {
                    Mouse.Click(control);
                }

                var point = GetClickablePoint(control);
                Mouse.Click(point);
                Mouse.Click(MouseButtons.Right);
            }
            catch(Exception)
            {
                Mouse.Click(control, MouseButtons.Right);
            }
        }

        public static void DoubleClick(this UITestControl control)
        {
            string state = control.State.ToString().ToLower();
            if(state.Contains("invisible") || state.Contains("offscreen"))
            {
                Mouse.Click(control);
            }

            var point = GetClickablePoint(control);
            Mouse.DoubleClick(point);
        }

        static Point GetClickablePoint(UITestControl control)
        {
            var boundingRect = control.BoundingRectangle;
            if(boundingRect.Y == -1 || boundingRect.X == -1)
            {
                Mouse.Click(control.GetParent());
                boundingRect = control.BoundingRectangle;
            }
            var point = new Point(boundingRect.X + boundingRect.Width / 2, boundingRect.Top + 5);
            return point;
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