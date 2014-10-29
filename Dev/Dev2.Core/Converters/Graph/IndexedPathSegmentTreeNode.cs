/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections;
using System.Collections.Generic;

namespace Unlimited.Framework.Converters.Graph
{
    public class IndexedPathSegmentTreeNode<T> : Dictionary<T, IndexedPathSegmentTreeNode<T>>
    {
        #region Indexers

        public IndexedPathSegmentTreeNode<T> this[List<T> complexKey]
        {
            get
            {
                IndexedPathSegmentTreeNode<T> returnNode = this;

                int count = 0;
                while (count < complexKey.Count && returnNode != null)
                {
                    returnNode.TryGetValue(complexKey[count], out returnNode);
                    count++;
                }

                return returnNode;
            }
        }

        #endregion Indexers

        #region Properties

        public object CurrentValue { get; set; }
        public IEnumerable EnumerableValue { get; set; }
        public new IEnumerator Enumerator { get; set; }
        public bool EnumerationComplete { get; set; }

        #endregion Properties
    }
}