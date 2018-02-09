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

namespace Dev2.Common.Interfaces
{
    public interface IToolConflictRow : IConflictRow
    {
        IToolModelConflictItem CurrentViewModel { get; set; }
        IToolModelConflictItem DiffViewModel { get; set; }
        LinkedList<IToolConflictRow> Children { get; set; }
        IToolConflictRow Parent { get; set; }
        bool IsMergeExpanded { get; set; }
        bool IsContainerTool { get; set; }
        bool IsStartNode { get; set; }
        IToolConflictRow GetNext();
        LinkedListNode<IToolConflictRow> Find(IToolConflictRow itemToFind);
        bool All(Func<IToolConflictRow, bool> check);
    }
}
