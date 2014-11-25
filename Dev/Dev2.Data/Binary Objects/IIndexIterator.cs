
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    public interface IIndexIterator
    {

        int Count { get; }

        bool IsEmpty { get;  }

        bool HasMore();

        int FetchNextIndex();

        int MaxIndex();

        int MinIndex();

        void AddGap(int idx);

        void RemoveGap(int idx);

        HashSet<int> FetchGaps();

        IIndexIterator Clone();
    }
}
