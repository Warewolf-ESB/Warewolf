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
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IToolConflict : IConflict
    {
        IMergeToolModel CurrentViewModel { get; set; }
        IMergeToolModel DiffViewModel { get; set; }
        LinkedList<IToolConflict> Children { get; set; }
        IToolConflict Parent { get; set; }
        bool IsMergeExpanded { get; set; }
        bool IsContainerTool { get; set; }
        IToolConflict GetNextConflict();
        LinkedListNode<IToolConflict> Find(IToolConflict itemToFind);
        bool All(Func<IToolConflict, bool> check);
    }
}
