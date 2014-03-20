using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;

namespace Dev2.DataList
{
    /// <summary>
    /// Static class for returning the search option that was selected by the user
    /// </summary>
    public static class FindRecsetOptions
    {

        private static Dictionary<string, IFindRecsetOptions> _options = new Dictionary<string, IFindRecsetOptions>();

        /// <summary>
        /// Private method for initializing the list of options
        /// </summary>
        static FindRecsetOptions()
        {
            var type = typeof(IFindRecsetOptions);

            List<Type> types = typeof(IFindRecsetOptions).Assembly.GetTypes()
                   .Where(t => (type.IsAssignableFrom(t))).ToList();

            foreach(Type t in types)
            {
                if(!t.IsAbstract && !t.IsInterface)
                {
                    IFindRecsetOptions item = Activator.CreateInstance(t, true) as IFindRecsetOptions;
                    if(item != null)
                    {
                        _options.Add(item.HandlesType(), item);
                    }
                }
            }
            SortRecordsetOptions();
        }

        private static void SortRecordsetOptions()
        {
            Dictionary<string, IFindRecsetOptions> tmpDictionary = new Dictionary<string, IFindRecsetOptions>();
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(string findRecordsOperation in GlobalConstants.FindRecordsOperations)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                KeyValuePair<string, IFindRecsetOptions> firstOrDefault = _options.FirstOrDefault(c => c.Value.HandlesType() == findRecordsOperation);
                if(!string.IsNullOrEmpty(firstOrDefault.Key))
                {
                    tmpDictionary.Add(firstOrDefault.Key, firstOrDefault.Value);
                }
            }

            _options = tmpDictionary;
        }

        /// <summary>
        /// Find the matching search object
        /// </summary>
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public static IFindRecsetOptions FindMatch(string expressionType)
        {
            IFindRecsetOptions result;
            if(!_options.TryGetValue(expressionType, out result))
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Find all AbstractRecsetSearchValidation objects
        /// </summary>
        /// <returns></returns>
        public static IList<IFindRecsetOptions> FindAll()
        {
            return _options.Values.ToList();
        }
    }
}
