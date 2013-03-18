using System.Collections.Generic;
using System.Linq;

namespace Dev2.Diagnostics
{
    public class DebugItem : IDebugItem
    {
        private List<IDebugItemResult> _resultsList = new List<IDebugItemResult>();

        public const int MaxItemDispatchCount = 10;
        public const int MaxCharDispatchCount = 150;
        public const int ActCharDispatchCount = 100;

        public static IDebugItem[] EmptyList = new IDebugItem[0];

        #region CTOR

        public DebugItem()
        {
        }

        public DebugItem(IEnumerable<IDebugItemResult> results)           
        {
            AddRange(results.ToList());
        }

        #endregion

        #region Contains

        public bool Contains(string filterText)
        {
            return _resultsList.Any(r => r.Value.ContainsSafe(filterText) || r.GroupName.ContainsSafe(filterText));
        }

        #endregion

        #region Public Methods

        public void Add(IDebugItemResult itemToAdd)
        {
            _resultsList.Add(itemToAdd);
        }

        public void AddRange(IList<IDebugItemResult> itemsToAdd)
        {
            _resultsList.AddRange(itemsToAdd);
        }

        public IList<IDebugItemResult> FetchResultsList()
        {
            return _resultsList;
        }

        #endregion

    }
}
