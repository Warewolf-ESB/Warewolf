using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Microsoft.VisualBasic.Activities;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public abstract class DsfFlowNodeActivity<TResult> : DsfActivityAbstract<TResult>, IActivityTemplateFactory
    {
        // Changing the ExpressionText property of a VisualBasicValue during runtime has no effect. 
        // The expression text is only evaluated and converted to an expression tree when CacheMetadata() is called.
        readonly VisualBasicValue<TResult> _expression;
        TResult _theResult;

        #region Ctor

        protected DsfFlowNodeActivity(string displayName)
            : base(displayName, true)
        {
            _expression = new VisualBasicValue<TResult>();
        }

        #endregion

        #region ExpressionText

        public string ExpressionText
        {
            get
            {
                return _expression.ExpressionText;
            }
            set
            {
                _expression.ExpressionText = value;
            }
        }

        #endregion

        #region CacheMetadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            //
            // Must use AddChild (which adds children as 'public') otherwise you will get the following exception:
            //
            // The private implementation of activity Decision has the following validation error:
            // Compiler error(s) encountered processing expression t.Eq(d.Get("FirstName",AmbientDataList),"Trevor").
            // 't' is not declared. It may be inaccessible due to its protection level
            // 'd' is not declared. It may be inaccessible due to its protection level
            //
            metadata.AddChild(_expression);
        }

        #endregion

        #region OnExecute

        protected override void OnExecute(NativeActivityContext context)
        {
            context.ScheduleActivity(_expression, OnCompleted, OnFaulted);
        }

        #endregion

        #region OnCompleted

        void OnCompleted(NativeActivityContext activityContext, ActivityInstance completedInstance, TResult result)
        {
            Result.Set(activityContext, result);
            _theResult = result;
            OnExecutedCompleted(activityContext, false, false);
        }

        #endregion

        #region OnFaulted

        void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            OnExecutedCompleted(faultContext, true, false);
        }

        #endregion

        #region IActivityTemplateFactory

        // Called when toolbox item is dropped onto design surface
        public new Activity Create(DependencyObject target)
        {
            var flowNode = CreateFlowNode();
            var designer = target as ActivityDesigner;
            ModelItem modelItem = null;
            if (designer != null)
            {
                var modelProperty = designer.ModelItem.Properties["Nodes"];
                if (modelProperty != null)
                {
                    if (modelProperty.Collection != null)
                    {
                        modelItem = modelProperty.Collection.Add(flowNode);
                    }
                }
            }

            //WorkflowViewElement view;
            //ModelItem action = null;
            //action = modelItem.Properties["Action"].Value;
            //view = action.View as WorkflowViewElement;
            //object o = view;

            //if (modelItem != null)
            //{
            //    EditingContext context = modelItem.GetEditingContext();
            //    if (context != null)
            //    {
            //        ViewService virtualizedContainerService = context.Services.GetService<ViewService>();
            //        modelItem.View
            //        DependencyObject container = virtualizedContainerService.GetView(modelItem);
            //        object o = container;
            //    }
            //}

            return null;
        }


        //#region Attached Properties

        //#region Location

        //public static Point GetLocation(DependencyObject obj)
        //{
        //    return (Point)obj.GetValue(LocationProperty);
        //}

        //public static void SetLocation(DependencyObject obj, Point value)
        //{
        //    obj.SetValue(LocationProperty, value);
        //}

        //// Using a DependencyProperty as the backing store for Location.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty LocationProperty =
        //    DependencyProperty.RegisterAttached("Location", typeof(Point), typeof(WorkflowDesignerViewModel), new PropertyMetadata(new Point()));


        //#endregion Location

        //#endregion Attached Properties

        #endregion

        #region Get Debug Inputs/Outputs

        // Travis.Frisinger - 28.01.2013 : Amended for Debug
        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> result = new List<IDebugItem>();
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();

            string val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText);

            try
            {

                Dev2DecisionStack dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                DebugItem itemToAdd = new DebugItem();
                string userModel = dds.GenerateUserFriendlyModel();

                foreach (Dev2Decision dev2Decision in dds.TheStack)
                {
                    if (DataListUtil.IsEvaluated(dev2Decision.Col1))
                    {
                        itemToAdd.AddRange(CreateDebugItems(dev2Decision.Col1, dataList));
                        ErrorResultTO errors = new ErrorResultTO();

                        userModel = userModel.Replace(dev2Decision.Col1, EvaluateExpressiomToStringValue(dev2Decision.Col1, dataList));
                    }
                    if (DataListUtil.IsEvaluated(dev2Decision.Col2))
                    {
                        itemToAdd.AddRange(CreateDebugItems(dev2Decision.Col2, dataList));
                        ErrorResultTO errors = new ErrorResultTO();

                        userModel = userModel.Replace(dev2Decision.Col2, EvaluateExpressiomToStringValue(dev2Decision.Col2, dataList));
                    }
                    if (DataListUtil.IsEvaluated(dev2Decision.Col3))
                    {
                        itemToAdd.AddRange(CreateDebugItems(dev2Decision.Col3, dataList));
                        ErrorResultTO errors = new ErrorResultTO();

                        userModel = userModel.Replace(dev2Decision.Col3, EvaluateExpressiomToStringValue(dev2Decision.Col3, dataList));
                    }
                }
                result.Add(itemToAdd);

                itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Statement" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = userModel });
                result.Add(itemToAdd);

            }
            catch (JsonSerializationException)
            {
                Dev2Switch ds = new Dev2Switch() { SwitchVariable = val };

                string userModel = ds.GenerateUserFriendlyModel();

                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Switch" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = userModel });
                result.Add(itemToAdd);

            }
            catch (Exception e)
            {
                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Error" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = e.Message });
                result.Add(itemToAdd);
            }

            return result;

            //return GetDebugItems(dataList, StateType.Before, ExpressionText);
        }


        // Travis.Frisinger - 28.01.2013 : Amended for Debug
        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> result = new List<IDebugItem>();
            string resultString = _theResult.ToString();
            DebugItem itemToAdd = new DebugItem();
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            string val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText);

            try
            {
            Dev2DecisionStack dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(val);

            if (_theResult.ToString() == "True")
            {
                resultString = dds.TrueArmText;
            }
            else if (_theResult.ToString() == "False")
            {
                resultString = dds.FalseArmText;
            }
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = resultString });
            result.Add(itemToAdd);
            }
            catch(Exception)
            {
                //2013.02.11: Ashley lewis - Bug 8725: Task 8730 - This means it is a swith, not a decision

                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = resultString });
                result.Add(itemToAdd);
            }

            return result;

            //return GetDebugItems(dataList, StateType.After, _theResult.ToString());
        }

        #endregion

        #region Private Debug Methods

        private string EvaluateExpressiomToStringValue(string Expression, IBinaryDataList dataList)
        {
            string result = string.Empty;
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors = new ErrorResultTO();
            var dlEntry = c.Evaluate(dataList.UID, enActionType.User, Expression, true, out errors);
            if (dlEntry.IsRecordset)
            {
                if (DataListUtil.GetRecordsetIndexType(Expression) == enRecordsetIndexType.Numeric)
                {
                    int index;
                    if (int.TryParse(DataListUtil.ExtractIndexRegionFromRecordset(Expression), out index))
                    {
                        string error;
                        IList<IBinaryDataListItem> listOfCols = dlEntry.FetchRecordAt(index, out error);
                        foreach (IBinaryDataListItem binaryDataListItem in listOfCols)
                        {
                            result = binaryDataListItem.TheValue;
                        }
                    }
                }
                else
                {
                    //TODO: When recordset star notation is viable in the desicion then please implement this.The bug for this fix is 
                    result = string.Empty;
                }
            }
            else
            {
                IBinaryDataListItem scalarItem = dlEntry.FetchScalar();
                result = scalarItem.TheValue;
            }


            return result;
        }

        #endregion

        protected abstract FlowNode CreateFlowNode();

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, ExpressionText);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, _theResult.ToString());
        }

        #endregion
    }
}
