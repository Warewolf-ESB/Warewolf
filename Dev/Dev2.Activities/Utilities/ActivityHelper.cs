using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;

namespace Dev2.Utilities
{
    public static class ActivityHelper
    {
        public static void InjectExpression(Dev2Switch ds, ModelProperty activityExpression)
        {
            if(ds == null) return;

            // FetchSwitchData
            string expressionToInject = String.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                    "(\"", ds.SwitchVariable, "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable,
                                                    ")");
            if(activityExpression != null)
            {
                activityExpression.SetValue(expressionToInject);
            }
        }

        public static void InjectExpression(Dev2DecisionStack ds, ModelProperty activityExpression)
        {
            if(ds == null) return;

            string modelData = ds.ToVBPersistableModel();
            string expressionToInject = String.Join("", GlobalConstants.InjectedDecisionHandler, "(\"",
                                                    modelData, "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable, ")");

            if(activityExpression != null)
            {
                activityExpression.SetValue(expressionToInject);
            }
        }

        public static string ExtractData(string val)
        {
            if(val.IndexOf(GlobalConstants.InjectedSwitchDataFetch, StringComparison.Ordinal) >= 0)
            {
                // Time to extract the data
                int start = val.IndexOf("(", StringComparison.Ordinal);
                if(start > 0)
                {
                    int end = val.IndexOf(@""",AmbientData", StringComparison.Ordinal);

                    if(end > start)
                    {
                        start += 2;
                        val = val.Substring(start, (end - start));

                        // Convert back for usage ;)
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
                ModelProperty keyProperty = switchCase.Properties["Key"];

                if(keyProperty != null)
                {
                    keyProperty.SetValue(ds.SwitchVariable);

                }
            }
        }

        public static void SetArmTextDefaults(Dev2DecisionStack dds)
        {
            if(String.IsNullOrEmpty(dds.TrueArmText.Trim()))
            {
                dds.TrueArmText = GlobalConstants.DefaultTrueArmText;
            }

            if(String.IsNullOrEmpty(dds.FalseArmText.Trim()))
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
            ModelProperty tArm = decisionActivity.Properties[armType];

            if(tArm != null)
            {
                tArm.SetValue(val);
            }
        }

        #region SetDisplayName

        // PBI 9220 - 2013.04.29 - TWR

        public static void SetDisplayName(ModelItem modelItem, IDev2FlowModel flow)
        {
            var displayName = modelItem.Properties[GlobalConstants.DisplayNamePropertyText];
            if(displayName != null)
            {
                displayName.SetValue(flow.DisplayText);
            }
        }

        #endregion

        public static void HandleMouseDoubleClick(MouseButtonEventArgs e)
        {
            if(!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
        }

        public static void HandleDragEnter(DragEventArgs e)
        {
            //This is to ensure nothing can be dragged onto a Activity Designer
        }
    }
}
