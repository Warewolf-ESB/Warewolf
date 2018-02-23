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
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Dev2.Common.Interfaces
{
    public interface IToolConflictItem : IConflictItem
    {
        ImageSource MergeIcon { get; set; }
        string MergeDescription { get; set; }
        Guid UniqueId { get; set; }
        FlowNode FlowNode { get; set; }
        object Activity { get; set; }
        ModelItem ModelItem { get; set; }
        Point NodeLocation { get; set; }
        bool IsInWorkflow { get; }

        List<IConnectorConflictItem> InboundConnectors { get; set; }
        List<IConnectorConflictItem> OutboundConnectors { get; set; }

        IToolConflictItem Clone();
    }
}
