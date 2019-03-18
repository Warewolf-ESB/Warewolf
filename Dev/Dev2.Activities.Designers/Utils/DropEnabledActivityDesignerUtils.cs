#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Interfaces;
using Warewolf.Studio.Core.Popup;

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
            if(!string.IsNullOrEmpty(modelItemString))
            {
                var innnerObjectData = data.GetData(modelItemString);
                if (innnerObjectData is List<ModelItem> modelList && modelList.Count > 1 && modelList.FirstOrDefault(c => c.ItemType == typeof(FlowDecision) || c.ItemType == typeof(FlowSwitch<string>)) != null)
                {
                    return false;
                }

            }

            modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemFormat", StringComparison.Ordinal) >= 0);
            if(string.IsNullOrEmpty(modelItemString))
            {
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat", StringComparison.Ordinal) >= 0);
                if(string.IsNullOrEmpty(modelItemString))
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
            var data = objectData as ModelItem;

            if (!ValidateDecision(objectData, data, true))
            {
                return false;
            }
            if (!ValidateSwitch(objectData, data, true))
            {
                return false;
            }
            if (!ValidateSelectAndApply(objectData, data, true))
            {
                return false;
            }

            return true;
        }

        bool ValidateDecision(object objectData, ModelItem data, bool dropEnabled)
        {
            var stringValue = objectData as string;
            if ((data != null && data.ItemType.Name == "FlowDecision") || (stringValue != null && stringValue.Contains("Decision")))
            {
                dropEnabled = false;
            }
            if (!dropEnabled)
            {
                ShowErrorMessage(Warewolf.Studio.Resources.Languages.Core.DecisionDropNotAllowedMessage,
                    Warewolf.Studio.Resources.Languages.Core.ExplorerDropNotAllowedHeader);
            }
            return dropEnabled;
        }

        bool ValidateSwitch(object objectData, ModelItem data, bool dropEnabled)
        {
            var stringValue = objectData as string;
            if ((data != null && data.ItemType.Name == "FlowSwitch`1") || (stringValue != null && stringValue.Contains("Switch")))
            {
                dropEnabled = false;
            }

            if (!dropEnabled)
            {
                ShowErrorMessage(Warewolf.Studio.Resources.Languages.Core.SwitchDropNotAllowedMessage,
                    Warewolf.Studio.Resources.Languages.Core.ExplorerDropNotAllowedHeader);
            }
            return dropEnabled;
        }

        bool ValidateSelectAndApply(object objectData, ModelItem data, bool dropEnabled)
        {
            var stringValue = objectData as string;
            if ((data != null && data.ItemType.Name == "DsfSelectAndApplyActivity") || (stringValue != null && stringValue.Contains("SelectAndApply")))
            {
                dropEnabled = false;
            }

            if (!dropEnabled)
            {
                ShowErrorMessage(Warewolf.Studio.Resources.Languages.Core.SelectAndApplyDropNotAllowedMessage,
                    Warewolf.Studio.Resources.Languages.Core.ExplorerDropNotAllowedHeader);
            }
            return dropEnabled;
        }

        #endregion

        void ShowErrorMessage(string errorMessage, string header)
        {
            var a = new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Description = errorMessage,
                Header = header,
                Image = MessageBoxImage.Error
            };
            var popup = CustomContainer.Get<IShellViewModel>();
            popup.ShowPopup(a);
        }
    }
}
