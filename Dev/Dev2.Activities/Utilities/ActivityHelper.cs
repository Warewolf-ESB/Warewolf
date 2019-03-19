#pragma warning disable
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.SystemTemplates.Models;

namespace Dev2.Utilities
{
    public static class ActivityHelper
    {
        public static string InjectExpression(Dev2Switch ds, ModelProperty activityExpression)
        {
            if(ds == null)
            {
                return null;
            }
            var expressionToInject = String.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                    "(\"", ds.SwitchVariable, "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable,
                                                    ")");
            if (activityExpression != null)
            {
                activityExpression.SetValue(expressionToInject);
            }
            return expressionToInject;
        }

        public static string InjectExpression(Dev2DecisionStack ds, ModelProperty activityExpression)
        {
            if(ds == null)
            {
                return null;
            }

            var modelData = ds.ToVBPersistableModel();
            var expressionToInject = String.Join("", GlobalConstants.InjectedDecisionHandler, "(\"",
                                                    modelData, "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable, ")");

            if (activityExpression != null)
            {
                activityExpression.SetValue(expressionToInject);
            }
            return expressionToInject;
        }

        public static string ExtractData(string val)
        {
            if(val.IndexOf(GlobalConstants.InjectedSwitchDataFetch, StringComparison.Ordinal) >= 0)
            {
                var start = val.IndexOf("(", StringComparison.Ordinal);
                if (start > 0)
                {
                    var end = val.IndexOf(@""",AmbientData", StringComparison.Ordinal);

                    if (end > start)
                    {
                        start += 2;
                        val = val.Substring(start, end - start);
                        val = Dev2DecisionStack.FromVBPersitableModelToJSON(val);
                    }
                }
            }
            return val;
        }

        public static void SetSwitchKeyProperty(Dev2Switch ds, ModelItem switchCase)
        {
            if(ds != null)
            {
                var keyProperty = switchCase.Properties["Key"];

                if (keyProperty != null)
                {
                    keyProperty.SetValue(ds.SwitchExpression);
                }
            }
        }

        public static void SetArmTextDefaults(Dev2DecisionStack dds)
        {
            if(String.IsNullOrEmpty(dds.TrueArmText?.Trim()))
            {
                dds.TrueArmText = GlobalConstants.DefaultTrueArmText;
            }

            if (String.IsNullOrEmpty(dds.FalseArmText?.Trim()))
            {
                dds.FalseArmText = GlobalConstants.DefaultFalseArmText;
            }
        }

        public static void SetArmText(ModelItem decisionActivity, Dev2DecisionStack dds)
        {
            SetArmText(decisionActivity, GlobalConstants.TrueArmPropertyText, dds.TrueArmText);
            SetArmText(decisionActivity, GlobalConstants.FalseArmPropertyText, dds.FalseArmText);
        }

        public static void SetArmText(ModelItem decisionActivity, string armType, string val)
        {
            var tArm = decisionActivity.Properties[armType];

            if (tArm != null)
            {
                tArm.SetValue(val);
            }
        }
      
        public static void SetDisplayName(ModelItem modelItem, IDev2FlowModel flow)
        {
            var displayName = modelItem.Properties[GlobalConstants.DisplayNamePropertyText];
            if(displayName != null)
            {
                displayName.SetValue(flow.DisplayText);
            }
        }

        [ExcludeFromCodeCoverage]
        public static void HandleDragEnter(DragEventArgs e)
        {
            //This is to ensure nothing can be dragged onto a Activity Designer
        }

        public static string GetSerializedStateValueFromCollection<T>(IList<T> collection)
        {
            var dev2JsonSerializer = new Dev2JsonSerializer();
            return dev2JsonSerializer.Serialize(collection);
        }
    }
}
