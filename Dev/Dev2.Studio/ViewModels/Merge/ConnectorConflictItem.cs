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

namespace Dev2.ViewModels.Merge
{
    public class ConnectorConflictItem : ConflictItem, IConnectorConflictItem, ICheckable
    {
        public ConnectorConflictItem(Guid Grouping, string armDescription, Guid sourceUniqueId, Guid destinationUniqueId, string key)
        {
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

        public override bool Equals(object obj)
        {
            var item = obj as ConnectorConflictItem;
            return item != null &&
                   SourceUniqueId == item.SourceUniqueId &&
                   DestinationUniqueId.Equals(item.DestinationUniqueId) &&
                   Key.Equals(item.Key);
        }

        public override int GetHashCode()
        {
            var hashCode = 1531919647;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(SourceUniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(DestinationUniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            return hashCode;
        }
    }
}
