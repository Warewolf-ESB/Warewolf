using System.Collections.Generic;

namespace Dev2.Data
{
    internal class WarewolfAtomComparer : IEqualityComparer<DataStorage.WarewolfAtom>
    {
        public bool Equals(DataStorage.WarewolfAtom x, DataStorage.WarewolfAtom y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(DataStorage.WarewolfAtom obj)
        {
            return obj.GetHashCode();
        }
    }
}
