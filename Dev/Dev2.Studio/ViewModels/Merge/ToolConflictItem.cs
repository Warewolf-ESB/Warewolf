/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;
using System.Activities.Statements;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Collections.Generic;
using Dev2.Common.ExtMethods;

namespace Dev2.ViewModels.Merge
{
    public class ToolConflictItem : ConflictItem, IToolConflictItem, ICheckable
    {
        protected ToolConflictItem()
        {
        }

        public Guid UniqueId { get; set; }
        public string MergeDescription { get; set; }
        public ModelItem ModelItem { get; set; }
        public FlowNode FlowNode { get; set; }
        public Point NodeLocation { get; set; }
        [JsonIgnore]
        public ImageSource MergeIcon { get; set; }
        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as ToolConflictItem;
            return item != null &&
                   MergeDescription == item.MergeDescription &&
                   UniqueId.Equals(item.UniqueId) &&
                   EqualityComparer<ModelItem>.Default.Equals(ModelItem, item.ModelItem);
        }

        public override int GetHashCode()
        {
            var hashCode = 1531919647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MergeDescription);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(UniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<ModelItem>.Default.GetHashCode(ModelItem);
            return hashCode;
        }

        internal static ToolConflictItem NewStartConflictItem() => new ToolConflictItem
        {
            MergeDescription = "Start",
            MergeIcon = Application.Current.TryFindResource("System-StartNode") as ImageSource
        };

        internal static ToolConflictItem NewFromActivity(IDev2Activity activity, ModelItem modelItem, Point location) => new ToolConflictItem
        {
            UniqueId = activity.UniqueID.ToGuid(),
            MergeDescription = activity.GetDisplayName(),
            FlowNode = activity.GetFlowNode(),
            ModelItem = modelItem,
            NodeLocation = location
        };

        public void SetUserInterface(ImageSource mergeIcon, ActivityDesignerViewModel instance)
        {
            MergeIcon = mergeIcon;
            ActivityDesignerViewModel = instance;
        }
    }
}
