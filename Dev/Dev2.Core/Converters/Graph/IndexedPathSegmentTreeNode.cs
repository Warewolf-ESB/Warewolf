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
                while(count < complexKey.Count && returnNode != null)
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
