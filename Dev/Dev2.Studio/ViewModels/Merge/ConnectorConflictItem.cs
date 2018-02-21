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
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.ViewModels.Merge.Utils;

namespace Dev2.ViewModels.Merge
{
    public class ConnectorConflictItem : ConflictItem, IConnectorConflictItem, ICheckable
    {
        readonly (ConflictRowList list, ConflictRowList.Column Column) context;
        public ConnectorConflictItem(ConflictRowList rowList, ConflictRowList.Column column, Guid Grouping, string armDescription, Guid sourceUniqueId, Guid destinationUniqueId, string key)
        {
            context.list = rowList;
            context.Column= column;

            ArmDescription = armDescription;
            if (!string.IsNullOrWhiteSpace(armDescription))
            {
                var description = armDescription.Split(new[] { "->" }, StringSplitOptions.None);
                LeftArmDescription = description[0].Trim();
                RightArmDescription = description[1].Trim();
            }
            SourceUniqueId = sourceUniqueId;
            DestinationUniqueId = destinationUniqueId;
            Key = key;
            this.Grouping = Grouping;
        }

        public string ArmDescription { get; set; }
        public string LeftArmDescription { get; set; }
        public string RightArmDescription { get; set; }
        public Guid SourceUniqueId { get; set; }
        public Guid DestinationUniqueId { get; set; }
        public string Key { get; set; }
        public Guid Grouping { get; private set; }

        public bool IsArmConnectorVisible => !string.IsNullOrWhiteSpace(ArmDescription);

        internal static IConnectorConflictItem EmptyConflictItem() => new Empty();

        private bool _allowSelection;
        public override bool AllowSelection
        {
            get => _allowSelection;
            set => SetProperty(ref _allowSelection, value);
        }
        public IToolConflictItem SourceConflictItem() {
            if (!context.list.Ready) {
                throw new Exception("ConflictRowList not ready");
            }
            return context.Column == ConflictRowList.Column.Current
                            ? context.list.GetToolItemFromIdCurrent(SourceUniqueId)
                            : context.list.GetToolItemFromIdDifferent(SourceUniqueId);
        }
        public IToolConflictItem DestinationConflictItem() {
            if (!context.list.Ready)
            {
                throw new Exception("ConflictRowList not ready");
            }
            return context.Column == ConflictRowList.Column.Current
                            ? context.list.GetToolItemFromIdCurrent(DestinationUniqueId)
                            : context.list.GetToolItemFromIdDifferent(DestinationUniqueId);
        }

        public override bool Equals(object obj)
        {
            if (obj is ConnectorConflictItem item)
            {
                var sourceEqual = SourceUniqueId.Equals(item.SourceUniqueId);
                var destinationEqual = DestinationUniqueId.Equals(item.DestinationUniqueId);
                var keyEqual = (Key != null && Key.Equals(item.Key));
                return sourceEqual && destinationEqual && keyEqual;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 1531919647;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(SourceUniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(DestinationUniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            return hashCode;
        }

        public new class Empty : ConflictItem.Empty, IConnectorConflictItem
        {
            public string ArmDescription { get; set; }
            public Guid SourceUniqueId { get; set; }
            public Guid DestinationUniqueId { get; set; }
            public string Key { get; set; }
            public bool IsArmConnectorVisible => false;

            public IToolConflictItem SourceConflictItem() { throw new NotImplementedException(); }
            public IToolConflictItem DestinationConflictItem() { throw new NotImplementedException(); }
        }
    }
}
