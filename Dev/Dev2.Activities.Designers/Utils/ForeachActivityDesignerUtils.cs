using System;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Linq;
using System.Windows;

namespace Dev2.Activities.Utils
{
    /// <summary>
    /// Utility class used for the foreach activity designer backing logic
    /// </summary>
    public class ForeachActivityDesignerUtils
    {
        #region Ctor

        #endregion


        public bool LimitDragDropOptions(IDataObject data)
        {
            var formats = data.GetFormats();
            if(!formats.Any())
            {
                return true;
            }

            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemFormat", StringComparison.Ordinal) >= 0);
            if(String.IsNullOrEmpty(modelItemString))
            {
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);
                if(String.IsNullOrEmpty(modelItemString))
                {
                    return true;
                }
            }
            var objectData = data.GetData(modelItemString);
            return ForeachDropPointOnDragEnter(objectData);

        }


        #region DropPointOnDragEnter

        /// <summary>
        /// Used to decide if the dragged activity can be dropped onto the foreach activity 
        /// </summary>
        /// <param name="objectData">The ModelItem of the dragged activity</param>
        /// <returns>If the activity is dropable into a foreach</returns>
        bool ForeachDropPointOnDragEnter(object objectData)
        {
            bool dropEnabled = true;

            var data = objectData as ModelItem;

            if(data != null && (data.ItemType == typeof(FlowDecision) || data.ItemType == typeof(FlowSwitch<string>)))
            {
                dropEnabled = false;

            }
            else
            {
                var stringValue = objectData as string;
                if(stringValue != null && (stringValue.Contains("Decision") || stringValue.Contains("Switch")))
                {
                    dropEnabled = false;
                }

            }
            return dropEnabled;
        }

        #endregion
    }
}
