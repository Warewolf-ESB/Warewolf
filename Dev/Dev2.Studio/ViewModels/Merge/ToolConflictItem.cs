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
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;
using System.Activities.Statements;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Collections.Generic;
using Dev2.ViewModels.Merge.Utils;

namespace Dev2.ViewModels.Merge
{
    public class ToolConflictItem : ConflictItem, IToolConflictItem, ICheckable
    {
        readonly (ConflictRowList list, ConflictRowList.Column column) _context;
        public ToolConflictItem(ConflictRowList list, ConflictRowList.Column column)
        {
            OutboundConnectors = new List<IConnectorConflictItem>();
            _context.list = list;
            _context.column = column;
        }

        public Guid UniqueId { get; set; }
        public string MergeDescription { get; set; }
        public ModelItem ModelItem { get; set; }
        public object Activity { get; set; }
        public FlowNode FlowNode { get; set; }
        public Point NodeLocation { get; set; }
        [JsonIgnore]
        public object MergeIcon { get; set; }
        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }
        public override bool AllowSelection { get; set; }
        public bool IsInWorkflow => _context.list.ActivityIsInWorkflow(Activity as IDev2Activity);

        public bool ShowCheckbox => true;
        public bool IsAddedToWorkflow { get; set; }
        public List<IConnectorConflictItem> InboundConnectors { get; set; }
        public List<IConnectorConflictItem> OutboundConnectors { get; set; }

        public IToolConflictItem Clone()
        {
            var clonedItem = MemberwiseClone() as IToolConflictItem;            
            if (!(InboundConnectors is null))
            {
                var inboundConnectors = new List<IConnectorConflictItem>();
                foreach (var inboundConnector in InboundConnectors)
                {
                    inboundConnectors.Add(inboundConnector.Clone());
                }
                clonedItem.InboundConnectors = inboundConnectors;
            }
            if (!(OutboundConnectors is null))
            {
                var outboundConnectors = new List<IConnectorConflictItem>();
                foreach (var outboundConnector in OutboundConnectors)
                {
                    outboundConnectors.Add(outboundConnector.Clone());
                }

                clonedItem.OutboundConnectors = outboundConnectors;
            }
            return clonedItem;
        }

        public override bool Equals(object obj)
        {
            var item = obj as ToolConflictItem;
            if (item == null)
            {
                return false;
            }
            var equal = UniqueId.Equals(item.UniqueId);
            equal &= MergeDescription == item.MergeDescription;
            if (ModelItem is null || item.ModelItem is null)
            {
                equal &= ModelItem == item.ModelItem;
            }
            else
            {
                equal &= ModelItem.Attributes.Equals(item.ModelItem.Attributes);
                equal &= ModelItem.GetModelPath().Equals(item.ModelItem.GetModelPath());
            }
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

        public new class Empty : ConflictItem.Empty, IToolConflictItem
        {
            public object MergeIcon { get; set; }
            public string MergeDescription { get; set; }
            public Guid UniqueId { get; set; }
            public FlowNode FlowNode { get; set; }
            public object Activity { get; set; }
            public ModelItem ModelItem { get; set; }
            public Point NodeLocation { get; set; }
            List<IConnectorConflictItem> _inboundConnectors;
            public List<IConnectorConflictItem> InboundConnectors { get => null; set => _inboundConnectors = value; }
            public List<IConnectorConflictItem> OutboundConnectors { get => throw new NotImplementedException(); set=> throw new NotImplementedException(); }
            public bool IsInWorkflow => false;
            public bool ShowCheckbox => false;
            public bool IsAddedToWorkflow { get; set; }

            public IToolConflictItem Clone()
            {
                var clonedItem = MemberwiseClone() as IToolConflictItem;                
                return clonedItem;
            }
        }
    }
}
