/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Warewolf.Core;
using Warewolf.Storage;
// ReSharper disable ConvertToAutoProperty

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("Utility-Comment", "Comment", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Comment")]
    public class DsfCommentActivity : DsfActivityAbstract<string>
    {
        private string _text;

        public DsfCommentActivity()
        {
            DisplayName = "Comment";
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        #region Get Debug Inputs/Outputs


        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            List<DebugItem> result = new List<DebugItem>();
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = Text });
            result.Add(itemToAdd);
            return result;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            InitializeDebug(context.GetExtension<IDSFDataObject>());
            DispatchDebugState(dataObject, StateType.Before, 0);

            DispatchDebugState(dataObject, StateType.After, 0);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                Text = updates[0].Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return DsfForEachItem.EmptyList;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return new List<DsfForEachItem>
            {
                new DsfForEachItem
                {
                    Value = Text,
                    Name = Text
                }
            };
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            InitializeDebug(dataObject);
            DispatchDebugState(dataObject, StateType.Before, 0);

            DispatchDebugState(dataObject, StateType.After, 0);
        }

        #endregion

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }
    }
}
