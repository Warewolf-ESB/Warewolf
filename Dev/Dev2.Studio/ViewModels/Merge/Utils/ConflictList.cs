using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using System.Collections;

namespace Dev2.ViewModels.Merge.Utils
{
    class ConflictList : IEnumerable<IConflict>
    {
        private List<IConflict> conflicts;
        public List<IConflict> Conflicts
        {
            get { return conflicts; }
            set { conflicts = value; }
        }

        public IEnumerator<IConflict> GetEnumerator() => conflicts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => conflicts.GetEnumerator();

        public int Count => conflicts.Count;
        public int IndexOf(IConflict conflict) => conflicts.IndexOf(conflict);

        public IConflict GetNextConlictToUpdate(IConflict container)
        {
            var index = conflicts.IndexOf(container) + 1;
            if (index < conflicts.Count)
            {
                var nextConflict = conflicts.ElementAt(index);
                return nextConflict;
            }
            return null;
        }
    }
}
