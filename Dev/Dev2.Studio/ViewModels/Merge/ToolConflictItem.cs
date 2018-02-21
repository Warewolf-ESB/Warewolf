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
using Dev2.ViewModels.Merge.Utils;

namespace Dev2.ViewModels.Merge
{
    public class ToolConflictItem : ConflictItem, IToolConflictItem, ICheckable
    {
        private bool _allowSelection;
        readonly (ConflictRowList list, ConflictRowList.Column column) context;
        public ToolConflictItem(ConflictRowList list, ConflictRowList.Column column)
        {
            OutboundConnectors = new List<IConnectorConflictItem>();
            context.list = list;
            context.column = column;
        }

        public Guid UniqueId { get; set; }
        public string MergeDescription { get; set; }
        public ModelItem ModelItem { get; set; }
        public object Activity { get; set; }
        public FlowNode FlowNode { get; set; }
        public Point NodeLocation { get; set; }
        [JsonIgnore]
        public ImageSource MergeIcon { get; set; }
        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }
        public override bool AllowSelection
        {
            get => _allowSelection;
            set => SetProperty(ref _allowSelection, value);
        }
        public List<IConnectorConflictItem> InboundConnectors { get; set; }
        public List<IConnectorConflictItem> OutboundConnectors { get; private set; }

        public override bool Equals(object obj)
        {
            var item = obj as ToolConflictItem;
            if (item == null)
            {
                return false;
            }
            var equal = UniqueId.Equals(item.UniqueId);
            equal &= MergeDescription == item.MergeDescription;
            equal &= ModelItem.Attributes.Equals(item.ModelItem.Attributes);
            equal &= ModelItem.GetModelPath().Equals(item.ModelItem.GetModelPath());
            if (Activity != null)
            {
                equal &= Activity.Equals(item.Activity);
            }
            return equal;
        }

        public override int GetHashCode()
        {
            var hashCode = 1531919647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MergeDescription);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(UniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<ModelItem>.Default.GetHashCode(ModelItem);
            return hashCode;
        }

        internal static IToolConflictItem EmptyConflictItem() => new Empty();
        internal static ToolConflictItem NewStartConflictItem(ConflictRowList list, ConflictRowList.Column column, ImageSource mergeIcon) => new ToolConflictItem(list, column)
        {
            MergeDescription = "Start",
            MergeIcon = mergeIcon
        };

        internal void InitializeFromActivity(IDev2Activity activity, ModelItem modelItem, Point location)
        {
            UniqueId = activity.UniqueID.ToGuid();
            MergeDescription = activity.GetDisplayName();
            FlowNode = activity.GetFlowNode();
            ModelItem = modelItem;
            NodeLocation = location;
        }

        public void SetUserInterface(ImageSource mergeIcon, ActivityDesignerViewModel instance)
        {
            MergeIcon = mergeIcon;
            ActivityDesignerViewModel = instance;
        }

        public new class Empty : ConflictItem.Empty, IToolConflictItem
        {
            public ImageSource MergeIcon { get; set; }
            public string MergeDescription { get; set; }
            public Guid UniqueId { get; set; }
            public FlowNode FlowNode { get; set; }
            public object Activity { get; set; }
            public ModelItem ModelItem { get; set; }
            public Point NodeLocation { get; set; }
            List<IConnectorConflictItem> _inboundConnectors;
            public List<IConnectorConflictItem> InboundConnectors { get => null; set => _inboundConnectors = value; }
            public List<IConnectorConflictItem> OutboundConnectors { get => throw new NotImplementedException(); }
        }
    }


}
