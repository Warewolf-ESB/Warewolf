using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UITesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses
{
    public partial class VariablesUIMap
    {
        public void EnterTextIntoScalarName(int indexNumber, string stringToEnter)
        {
            UITestControlCollection variableList = GetScalarVariableList();
            UITestControl theBox = variableList[indexNumber].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(theBox != null)
            {
                theBox.EnterText(stringToEnter);
            }
        }

        public void EnterTextIntoRecordsetName(int indexNumber, string stringToEnter)
        {
            UITestControlCollection variableList = GetRecordsetVariableList();
            UITestControl theBox = variableList[indexNumber].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(theBox != null)
            {
                theBox.EnterText(stringToEnter);
            }
        }

        public void EnterTextIntoRecordsetSubItemName(int recSetIndex, int valIndex, string stringToEnter)
        {
            UITestControl recordSetList = GetRecordSetList();
            List<UITestControl> recordsetTreeList = recordSetList.GetChildren().Where(c => c.ControlType == ControlType.TreeItem).ToList();
            List<UITestControl> subsetCollection = recordsetTreeList[recSetIndex].GetChildren().Where(c => c.ControlType == ControlType.TreeItem).ToList();
            UITestControl textBox = subsetCollection[valIndex].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textBox != null)
            {
                textBox.EnterText(stringToEnter);
            }
        }

        public void ClickScalarVariableName(int position)
        {
            // The actual box is item [1];
            UITestControlCollection variableList = GetScalarVariableList();
            UITestControl theBox = variableList[position].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            Mouse.Click(theBox, new Point(5, 5));
        }

        public bool CheckVariableNameIsValid(int position)
        {
            UITestControlCollection variableList = GetScalarVariableList();
            UITestControl theBox = variableList[position].GetChildren()[1];

            string helpText = theBox.GetProperty("HelpText").ToString();

            if(helpText == "Variable names can only contain letters.")
            {
                return false;
            }
            return true;
        }

        public string GetVariableName(int position)
        {
            UITestControlCollection variableList = GetScalarVariableList();
            UITestControl theBox = variableList[position].GetChildren()[1];

            string boxText = theBox.GetText();
            return boxText;
        }

        public void UpdateDataList()
        {
            UITestControl addRemoveButton = GetUpdateButton();
            Mouse.Click(addRemoveButton, new Point(5, 5));
            Playback.Wait(500);
        }

        public void ClickRecordSetName(int recSetIndex)
        {
            UITestControlCollection variableList = GetRecordsetVariableList();
            UITestControl theBox = variableList[recSetIndex].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(theBox != null)
            {
                Point p = new Point(theBox.BoundingRectangle.X, theBox.BoundingRectangle.Y);
                Mouse.Click(new Point(p.X + 25, p.Y + 5));
            }
        }

        public void ClickRecordSetSubItem(int recSetIndex, int valIndex)
        {
            UITestControl recordSetList = GetRecordSetList();
            List<UITestControl> recordsetTreeList = recordSetList.GetChildren().Where(c => c.ControlType == ControlType.TreeItem).ToList();
            List<UITestControl> subsetCollection = recordsetTreeList[recSetIndex].GetChildren().Where(c => c.ControlType == ControlType.TreeItem).ToList();
            UITestControl textBox = subsetCollection[valIndex].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textBox != null)
            {
                Point p = new Point(textBox.BoundingRectangle.X, textBox.BoundingRectangle.Y);
                Mouse.Click(new Point(p.X + 25, p.Y + 5));
            }
        }

        public void CheckScalarInputOrOuput(int position, Dev2MappingType mappingType)
        {
            UITestControlCollection variableList = GetScalarVariableList();
            UITestControlCollection collection = variableList[position].GetChildren();
            List<UITestControl> theBoxs = collection.Where(c => c.ControlType == ControlType.CheckBox).ToList();
            if(mappingType == Dev2MappingType.Input)
            {
                theBoxs[0].Check(!theBoxs[0].IsChecked());
            }
            else
            {
                theBoxs[1].Check(!theBoxs[1].IsChecked());
            }
        }

        public string GetRecordSetSubItemHelptext(int recSetIndex, int valIndex)
        {
            UITestControl recordSetList = GetRecordSetList();
            List<UITestControl> recordsetTreeList = recordSetList.GetChildren().Where(c => c.ControlType == ControlType.TreeItem).ToList();
            List<UITestControl> subsetCollection = recordsetTreeList[recSetIndex].GetChildren().Where(c => c.ControlType == ControlType.TreeItem).ToList();
            UITestControl textBox = subsetCollection[valIndex].GetChildren().FirstOrDefault(c => c.ControlType == ControlType.Edit);
            if(textBox != null)
            {
                WpfEdit theEdit = (WpfEdit)textBox.GetChildren()[1];
                string helpText = theEdit.HelpText;
                return helpText;
            }
            return string.Empty;
        }

        public bool CheckIfVariableIsUsed(int position)
        {
            bool result = false;

            UITestControlCollection variableList = GetScalarVariableList();
            var item = variableList[position];
            if(item != null)
            {
                var children = item.GetChildren();
                var button = children.Last(c => c.ControlType == ControlType.Button);
                if(button != null && button.Height == -1)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
