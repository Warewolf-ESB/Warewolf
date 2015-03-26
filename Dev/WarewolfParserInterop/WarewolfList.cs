using System;
using System.Collections.Generic;
using System.Linq;

namespace WarewolfParserInterop
{
  public class WarewolfAtomList<T>:IEnumerable<T>
    {
        private T[] _values;
        private int _count;
        private readonly T _defaultValue;
      IEnumerator<T> _currentEnumerator;

      public WarewolfAtomList(T defaultValue)
        {
           _count= 32;
            _values = new T[32];
            for (int a = 0; a < _count;a++) _values[a] = defaultValue;
            _count = 0;
            _defaultValue = defaultValue;
        }

        public WarewolfAtomList(T defaultValue,IEnumerable<T> values)
        {
            IEnumerable<T> enumerable = values as T[] ?? values.ToArray();
            if (enumerable.Any())
            {
                _values = enumerable.ToArray();
                _count = enumerable.Count();
                

            }
            else
            {
                _count = 32;
                _values = new T[32];
                for (int a = 0; a < _count; a++) _values[a] = defaultValue;
                _count = 0;
                _defaultValue = defaultValue;
            }
        }

      public WarewolfAtomList(T defaultValue, IEnumerable<T> values, int count)
      {
          IEnumerable<T> enumerable = values as T[] ?? values.ToArray();
          if (enumerable.Any())
            {
                _values = enumerable.ToArray();

                _count = count;
                _defaultValue = defaultValue;
            }
            else
            {
                count = 32;
                _values = new T[32];
                for (int a = 0; a < count; a++) _values[a] = defaultValue;
                _defaultValue = defaultValue;
                _count = 0;
            }
      }

      public void AddNothing()
        {
            ResizeToCount();
            _count++;
           
        }

        public void AddSomething(T value)
        {
            
            ResizeToCount();
            _values[_count] = value;
            _count++;
        }

        public T GetValue(int position)
        {
            if (position + 1 >= _count)
            {
                return _defaultValue;
            }
            return _values[position];
        }

      private void ResizeToCount()
        {
            if (_count >= _values.Length - 1)
            {
                Array.Resize(ref _values, _values.Length * 2);
                for (int a = _count+1; a < _values.Length; a++) 
                    _values[a] = _defaultValue;
            
            }
        }

      public T GetNextValue()
      {
          var x = GetCurrentEnumerator();
          x.MoveNext();
          return x.Current;
      }

      IEnumerator<T> GetCurrentEnumerator()
      {
          return _currentEnumerator ?? (_currentEnumerator = GetEnumerator());
      }

      public bool Apply (Func<T,T> action )
      {
          for(int i = 0; i < _count; i++)
          {
              _values[i] = action(_values[i]);
          }
          return true;
      }

      public IEnumerator<T> GetEnumerator()
        {
            return _values.Take(_count).GetEnumerator();
        }


      // ReSharper restore FunctionNeverReturns

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int i]
        {
            get
            {
                if (i < _count)
                    return _values[i];
                throw new Dev2.Common.Common.NullValueInVariableException("the recordset does not have the row"+i,"");
            }
            set {
                if (i < _count)
                    _values[i] = value;
                else if (i == Count)
                    AddSomething(value);
                else throw new Dev2.Common.Common.NullValueInVariableException("the recordset does not have the row" + i, "");
                
            }
        }

        public int Count { get { return _count; } }

        public int Length { get { return _values.Length; } }

      public WarewolfAtomList<T> DeletePosition(int position)
      {
          var lst = new WarewolfAtomList<T>(_defaultValue);
          for(int i = 0;i< Count;i++)
          {
              if(i!= position)
                  lst.AddSomething(this[i]);
          }

          return lst;
      }


      public IEnumerable<int> Where(Func<T, bool> func)
      {

          for (int i = 0; i < _count; i++)
          {
              if (func(_values[i]))
              {
                  yield return i;

              }
          }
      }
    }
}

