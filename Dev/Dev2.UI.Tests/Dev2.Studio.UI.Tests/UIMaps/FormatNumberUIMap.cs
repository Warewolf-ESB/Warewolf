namespace Dev2.Studio.UI.Tests.UIMaps.FormatNumberUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    
    
    public partial class FormatNumberUIMap
    {
        UITestControl _numberFormatTool;
        public void InputAllFormatNumberValues(UITestControl numberFormatTool, string number, string roundingType, string roundingValue, string decimalsToShow, string result)
        {
            _numberFormatTool = numberFormatTool;
            InputNumber(number);
            SelectRoundingType(roundingType);
            InputRounding(roundingValue);
            InputDecimalsToShow(decimalsToShow);
            InputResult(result);
        }

        public void InputNumber(string numberToInput)
        {
            UITestControl numberInput = GetInput("UI__Number_Inputtxt_AutoID");
            Mouse.Click(numberInput, new Point(5,5));
            SendKeys.SendWait(numberToInput);
        }

        public void SelectRoundingType(string roundingType)
        {
            UITestControl roundingTypeDropDown = GetComboBox();
            WpfComboBox comboBox = (WpfComboBox)roundingTypeDropDown;
            comboBox.SelectedItem = roundingType;
        }

        public void InputRounding(string rounding)
        {
            UITestControl roundingInputBox = GetInput("UI__Rounding_Inputtxt_AutoID");
            Mouse.Click(roundingInputBox, new Point(5, 5));
            SendKeys.SendWait(rounding);
        }

        public void InputDecimalsToShow(string decimalsToShow)
        {
            UITestControl decimalsToShowInput = GetInput("UI__DecimalsToShow_Inputtxt_AutoID");
            Mouse.Click(decimalsToShowInput, new Point(5, 5));
            SendKeys.SendWait(decimalsToShow);
        }

        public void InputResult(string result)
        {
            UITestControl decimalsToShowInput = GetInput("UI__Result_Inputtxt_AutoID");
            Mouse.Click(decimalsToShowInput, new Point(5, 5));
            SendKeys.SendWait(result);
        }

        public bool IsRoundingInputEnabled()
        {
            UITestControl roundingInputBox = GetInput("UI__Rounding_Inputtxt_AutoID");
            return roundingInputBox.Enabled;
        }

        public WpfEdit GetRoudingInputBoxControl()
        {
            return (WpfEdit)GetInput("UI__Rounding_Inputtxt_AutoID");
        }

        private UITestControl GetInput(string AutomationId)
        {
            UITestControlCollection smallViewChildren = GetSmallView().GetChildren();
            foreach(UITestControl child in smallViewChildren)
            {
                if (child.GetProperty("AutomationId").ToString() == AutomationId)
                {
                    return child;
                }
            }
            return null;
        }

        private UITestControl GetComboBox()
        {
            UITestControlCollection smallViewChildren = GetSmallView().GetChildren();
            foreach (UITestControl child in smallViewChildren)
            {
                if (child.ClassName == "Uia.ComboBox")
                {
                    return child;
                }
            }
            throw new UITestControlNotFoundException("Cannot find rounding type combo box on format number activity");
        }

        private UITestControl GetSmallView()
        {
            UITestControlCollection coll = _numberFormatTool.GetChildren();
            foreach (UITestControl ctrl in coll)
            {
                if(ctrl.GetProperty("AutomationId").ToString() == "SmallViewContent")
                {
                    return ctrl;
                }
            }
            throw new UITestControlNotFoundException("Cannot find format number activity small view");
        }
    }
}
