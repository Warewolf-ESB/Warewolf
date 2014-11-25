
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Dev2.Activities.Utils
{
    /// <summary>
    /// Utility class used for the foreach activity designer backing logic
    /// </summary>
    public class DropEnabledActivityDesignerUtils
    {
        public bool LimitDragDropOptions(IDataObject data)
        {
            var formats = data.GetFormats();
            if(!formats.Any())
            {
                return true;
            }


            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemsFormat", StringComparison.Ordinal) >= 0);
            if(!String.IsNullOrEmpty(modelItemString))
            {
                var innnerObjectData = data.GetData(modelItemString);
                var modelList = innnerObjectData as List<ModelItem>;
                if(modelList != null && modelList.Count > 1)
                {
                    if(modelList.FirstOrDefault(c => c.ItemType == typeof(FlowDecision) || c.ItemType == typeof(FlowSwitch<string>)) != null)
                    {
                        return false;
                    }
                }
            }

            modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemFormat", StringComparison.Ordinal) >= 0);
            if(String.IsNullOrEmpty(modelItemString))
            {
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);
                if(String.IsNullOrEmpty(modelItemString))
                {
                    return true;
                }
            }
            var objectData = data.GetData(modelItemString);
            return DropPointOnDragEnter(objectData);
        }


        #region DropPointOnDragEnter

        /// <summary>
        /// Used to decide if the dragged activity can be dropped onto the foreach activity 
        /// </summary>
        /// <param name="objectData">The ModelItem of the dragged activity</param>
        /// <returns>If the activity is drop-able into a foreach</returns>
        bool DropPointOnDragEnter(object objectData)
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
