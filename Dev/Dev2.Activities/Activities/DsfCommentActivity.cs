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
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities

{
    [ToolDescriptorInfo("Utility-Comment", "Comment", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Comment")]
    public class DsfCommentActivity : DsfActivityAbstract<string>,IEquatable<DsfCommentActivity>
    {
        string _text;

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


        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            var result = new List<DebugItem>();
            var itemToAdd = new DebugItem();
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

        public override IList<DsfForEachItem> GetForEachInputs() => DsfForEachItem.EmptyList;

        public override IList<DsfForEachItem> GetForEachOutputs() => new List<DsfForEachItem>
            {
                new DsfForEachItem
                {
                    Value = Text,
                    Name = Text
                }
            };

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            InitializeDebug(dataObject);
            DispatchDebugState(dataObject, StateType.Before, 0);

            DispatchDebugState(dataObject, StateType.After, 0);
        }

        #endregion

        public override List<string> GetOutputs() => new List<string>();

        public bool Equals(DsfCommentActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) 
                && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfCommentActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Text != null ? Text.GetHashCode() : 0);
            }
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] 
            {
                new StateVariable
                {
                    Name = "Text",
                    Type = StateVariable.StateType.InputOutput,
                    Value = Text
                }
            };
        }
    }
}
