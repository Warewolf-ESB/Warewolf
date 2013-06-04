using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Dev2.DataList
{
    /// <summary>
    /// Static class for returning the search option that was selected by the user
    /// </summary>
    public static class FindRecsetOptions
    {

        private static readonly Dictionary<string, IFindRecsetOptions> _options = new Dictionary<string, IFindRecsetOptions>();

        /// <summary>
        /// Private method for intitailizing the list of options
        /// </summary>
        static FindRecsetOptions()
        {
            var type = typeof(IFindRecsetOptions);

            List<Type> types = typeof(IFindRecsetOptions).Assembly.GetTypes()
                   .Where(t => (type.IsAssignableFrom(t))).ToList();

            foreach (Type t in types)
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    IFindRecsetOptions item = Activator.CreateInstance(t, true) as IFindRecsetOptions;
                    if (item != null)
                    {
                        _options.Add(item.HandlesType(), item);
                    }
                }
            }
        }

        /// <summary>
        /// Find the matching search object
        /// </summary>
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public static IFindRecsetOptions FindMatch(string expressionType)
        {
            IFindRecsetOptions result;
            if (!_options.TryGetValue(expressionType, out result))
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Find all AbstractRecsetSearchValidation objects
        /// </summary>
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public static IList<IFindRecsetOptions> FindAll()
        {
            return _options.Values.ToList();
        }
    }
}
