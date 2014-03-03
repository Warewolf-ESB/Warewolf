using Dev2.Data.SystemTemplates;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList.Contract.Value_Objects
{
    public static class Dev2ValueObjectFactory
    {

        /// <summary>
        /// Creates the evaluate iterator.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static IDev2DataListEvaluateIterator CreateEvaluateIterator(IBinaryDataListEntry entry)
        {
            return new Dev2DataListEvaluateIterator(entry ?? DataListConstants.baseEntry);
        }

        /// <summary>
        /// Creates the iterator collection.
        /// </summary>
        /// <returns></returns>
        public static IDev2IteratorCollection CreateIteratorCollection()
        {
            return new Dev2IteratorCollection();
        }

    }
}
