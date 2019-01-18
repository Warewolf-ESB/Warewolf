/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System;
using System.Collections.Generic;

namespace Dev2.Common
{
    public interface IConflictTreeNode : IEquatable<IConflictTreeNode>
    {
        IDev2Activity Activity { get; }
        List<(string uniqueId, IConflictTreeNode node)> Children { get; }
        bool IsInConflict { get; set; }
        Point Location { get; }
        string UniqueId { get; set; }

        void AddChild(IConflictTreeNode node,string name);
    }
}