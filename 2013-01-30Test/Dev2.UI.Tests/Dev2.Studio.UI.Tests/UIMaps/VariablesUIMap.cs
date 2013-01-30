namespace Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses
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
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using System.Windows.Forms;
    
    
    public partial class VariablesUIMap
    {
        public void ClickVariableName(int position)
        {
            // The actual box is item [1];
            UITestControlCollection variableList = getVariableList();
            UITestControl theBox = variableList[position].GetChildren()[1];
            Mouse.Click(theBox, new Point(5, 5));
        }

        public bool CheckVariableIsValid(int position)
        {
            UITestControlCollection variableList = getVariableList();
            UITestControl theBox = variableList[position].GetChildren()[1];
           
            string helpText = theBox.GetProperty("HelpText").ToString();

            if (helpText == "You have entered invalid characters")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetVariableName(int position)
        {
            UITestControlCollection variableList = getVariableList();
            UITestControl theBox = variableList[position].GetChildren()[1];

            string boxText = theBox.GetProperty("Text").ToString();
            return boxText;
        }

        public void UpdateDataList()
        {
            UITestControl refreshButton = getUpdateButton();
            Mouse.Click(refreshButton, new Point(5, 5));
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{ENTER}");
        }
    }
}
