
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.UI.Tests.Extensions;

namespace Dev2.Studio.UI.Tests.UIMaps.FormatNumberUIMapClasses
{
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    
    
    public partial class FormatNumberUIMap
    {
        UITestControl _numberFormatTool;
        public void InputAllFormatNumberValues(UITestControl numberFormatTool, string number, string roundingType, string roundingValue, string decimalsToShow, string result)
        {
            _numberFormatTool = numberFormatTool;
            InputNumber(number);
            SelectRoundingType(roundingType);
            if(roundingType != "None")
            {
            InputRounding(roundingValue);
            InputDecimalsToShow(decimalsToShow);
            }
            InputResult(result);
        }

        public void SetFormatNumberControl(UITestControl control)
        {
            _numberFormatTool = control;
        }

        public void InputNumber(string numberToInput)
        {
            UITestControl numberInput = GetInput("UI__Number_Inputtxt_AutoID");
            numberInput.EnterText(numberToInput);
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
            roundingInputBox.EnterText(rounding);
        }

        public void InputDecimalsToShow(string decimalsToShow)
        {
            UITestControl decimalsToShowInput = GetInput("UI__DecimalsToShow_Inputtxt_AutoID");
            decimalsToShowInput.EnterText(decimalsToShow);
        }

        public void InputResult(string result)
        {
            UITestControl decimalsToShowInput = GetInput("UI__Result_Inputtxt_AutoID");
            decimalsToShowInput.EnterText(result);
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
                if(child.GetProperty("AutomationId").ToString() == AutomationId)
                {
                    return child;
                }
            }
            return null;
        }

        private UITestControl GetComboBox()
        {
            UITestControlCollection smallViewChildren = GetSmallView().GetChildren();
            foreach(UITestControl child in smallViewChildren)
            {
                if(child.ClassName == "Uia.ComboBox")
                {
                    return child;
                }
            }
            throw new UITestControlNotFoundException("Cannot find rounding type combo box on format number activity");
        }

        private UITestControl GetSmallView()
        {
            UITestControlCollection coll = _numberFormatTool.GetChildren();
            foreach(UITestControl ctrl in coll)
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
