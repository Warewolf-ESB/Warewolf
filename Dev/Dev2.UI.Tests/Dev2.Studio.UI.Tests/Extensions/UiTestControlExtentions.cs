using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.Extensions
{
    public static class UiTestControlExtentions
    {
        public static UITestControl GetChildByAutomationIDPath(this UITestControl parent, params string[] automationIDs)
        {
            return new VisualTreeWalker().GetChildByAutomationIDPath(parent, automationIDs);
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

        public static UITestControl FindByAutomationId(this UITestControl container, string automationId)
        {
            List<UITestControl> parentCollection = container.GetChildren()
                                                            .Where(c => !(c is WpfListItem) && c is WpfControl)
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

            if(control == null)
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
            Point point;
            if(control.TryGetClickablePoint(out point))
            {
                point.Offset(control.Left, control.Top);
                Mouse.Move(point);
                if(control.State == ControlStates.Focused)
                {
                    Mouse.Click();
                }
                else
                {
                    try
                    {
                        Mouse.DoubleClick();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        // just to handle silly UI framework issues
                    }
                }
            }
            else
            {
                throw new Exception("Cannot get clickable point on control");
            }
        }

        public static void DoubleClick(this UITestControl control)
        {
            Point point = new Point(control.BoundingRectangle.X + 30, control.BoundingRectangle.Y + 5);
            Mouse.Move(point);
            Mouse.DoubleClick();
        }

        public static void EnterText(this UITestControl control, string text)
        {
            ////Clear Text
            control.ClearText();
            ////Enter text
            control.AppendText(text);
        }

        public static void AppendText(this UITestControl control, string text)
        {
            ////Enter text
            WpfEdit editControl = control as WpfEdit;

            if(editControl == null)
            {
                throw new Exception("Cannot enter text in a non editable control");
            }

            editControl.Text = text;
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

        public static bool IsEnabled(this UITestControl control)
        {
            var checkBox = control as WpfControl;

            if(checkBox == null)
            {
                throw new Exception("Control must be a valid WPF Control");
            }

            return checkBox.Enabled;
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
    }
}