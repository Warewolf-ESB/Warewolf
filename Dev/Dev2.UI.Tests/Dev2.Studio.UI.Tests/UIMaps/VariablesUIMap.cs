using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses
{
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

        public void ClickRecordSetName(int recSetIndex)
        {
            UITestControl recordSetList = GetRecordSetList();
            UITestControl recList = new UITestControl(recordSetList);
            recList.SearchProperties.Add("ControlType", "TreeItem");
            UITestControlCollection recordsetTreeList = recList.FindMatchingControls();
            Point p = new Point(recordsetTreeList[recSetIndex].BoundingRectangle.X, recordsetTreeList[recSetIndex].BoundingRectangle.Y);
            Mouse.Click(new Point(p.X + 25, p.Y + 5));
        }

        public void ClickRecordSetSubItem(int recSetIndex, int valIndex)
        {
            UITestControl recordSetList = GetRecordSetList();
            UITestControl recList = new UITestControl(recordSetList);
            recList.SearchProperties.Add("ControlType", "TreeItem");
            UITestControlCollection recordsetTreeList = recList.FindMatchingControls();
            UITestControl recSetVals = new UITestControl(recordsetTreeList[recSetIndex]);
            recSetVals.SearchProperties.Add("ControlType", "TreeItem");
            UITestControlCollection subsetCollection = recSetVals.FindMatchingControls();
            Point p = new Point(subsetCollection[valIndex].BoundingRectangle.X, subsetCollection[valIndex].BoundingRectangle.Y);
            Mouse.Click(new Point(p.X + 25, p.Y + 5));
        }

        public string GetRecordSetSubItemHelptext(int recSetIndex, int valIndex)
        {

            UITestControl recordSetList = GetRecordSetList();
            UITestControl recList = new UITestControl(recordSetList);
            recList.SearchProperties.Add("ControlType", "TreeItem");
            UITestControlCollection recordsetTreeList = recList.FindMatchingControls();
            UITestControl recSetVals = new UITestControl(recordsetTreeList[recSetIndex]);
            recSetVals.SearchProperties.Add("ControlType", "TreeItem");
            UITestControlCollection subsetCollection = recSetVals.FindMatchingControls();
            WpfEdit theEdit = (WpfEdit)subsetCollection[valIndex].GetChildren()[1];
            string helpText = theEdit.HelpText;
            return helpText;
        }

        public bool CheckIfVariableIsUsed(int position)
        {
            bool result = false;

            UITestControlCollection variableList = getVariableList();
            var item = variableList[position];
            if (item != null)
            {
                var children = item.GetChildren();
                var button = children.Last(c => c.ClassName == "Uia.Button");
                if (button != null && button.Height == -1)
                {
                    result = true;
                }                
            }

            return result;
        }
    }
}
