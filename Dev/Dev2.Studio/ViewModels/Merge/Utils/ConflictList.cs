using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using System.Collections;

namespace Dev2.ViewModels.Merge.Utils
{
    public class ConflictList : IEnumerable<IConflict>
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

        public IConflict GetNextConflict(IConflict conflict)
        {
            var idx = conflicts.IndexOf(conflict);
            var nextConflict = MoveNext(idx);
            while (nextConflict != null)
            {
                idx = idx + 1;
                nextConflict = MoveNext(idx);
            }
            return nextConflict;
        }

        public IConflict MoveNext(int index)
        {
            var nextIndex = index + 1;
            if (nextIndex >= conflicts.Count)
            {
                return null;
            }
            var nextConflict = conflicts[nextIndex];
            return nextConflict;
        }

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
