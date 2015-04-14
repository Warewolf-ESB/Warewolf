using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Warewolf.Storage
{
    public class WarewolfListIterator : IWarewolfListIterator
    {
        readonly List<IWarewolfIterator> _variablesToIterateOn;

        public WarewolfListIterator()
        {
            _variablesToIterateOn = new List<IWarewolfIterator>();
        }

        public string FetchNextValue(IWarewolfIterator expression)
        {
            var warewolfEvalResult = _variablesToIterateOn[_variablesToIterateOn.IndexOf(expression)];
            if (warewolfEvalResult!=null)
            {
                return warewolfEvalResult.GetNextValue();
            }            
            return null;
        }

        public void AddVariableToIterateOn(IWarewolfIterator iterator)
        {
            _variablesToIterateOn.Add(iterator);            
        }

        public int GetMax()
        {
            return _variablesToIterateOn.Max(iterator => iterator.GetLength());
        }

        public bool HasMoreData()
        {
            return _variablesToIterateOn.Any(iterator => iterator.HasMoreData());
        }
    }
}
