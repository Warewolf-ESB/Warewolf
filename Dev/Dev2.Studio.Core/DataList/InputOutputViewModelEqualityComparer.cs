using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DataList
{
    /// <summary>
    /// Used to compare IO mapping details ;)
    /// </summary>
    public class InputOutputViewModelEqualityComparer : IEqualityComparer<IInputOutputViewModel>
    {
        public bool Equals(IInputOutputViewModel x, IInputOutputViewModel y)
        {
            if (x.DisplayName == y.DisplayName)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(IInputOutputViewModel obj)
        {
            return obj.DisplayName.GetHashCode();
        }
    }
}
