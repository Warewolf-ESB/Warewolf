using System.Activities.Presentation.Model;
using System.Activities.Statements;

namespace Dev2.Studio.Utils.ActivityDesignerUtils
{
    /// <summary>
    /// Utility class used for the foreach activity designer backing logic
    /// </summary>
    public class ForeachActivityDesignerUtils
    {
        #region Ctor

        public ForeachActivityDesignerUtils()
        {

        }

        #endregion

        #region DropPointOnDragEnter

        /// <summary>
        /// Used to decide if the dragged activity can be dropped onto the foreach activity 
        /// </summary>
        /// <param name="objectData">The ModelItem of the dragged activity</param>
        /// <returns>If the activity is dropable into a foreach</returns>
        public bool ForeachDropPointOnDragEnter(object objectData)
        {
            bool dropEnabled = true;            

            var data = objectData as ModelItem;

            if (data != null && (data.ItemType == typeof(FlowDecision) || data.ItemType == typeof(FlowSwitch<string>)))
            {
                dropEnabled = false;

            }
            else
            {
                var stringValue = objectData as string;
                if (stringValue != null && (stringValue.Contains("Decision") || stringValue.Contains("Switch")))
                {
                    dropEnabled = false;
                }

            }        
            return dropEnabled;
        }

        #endregion
    }
}
