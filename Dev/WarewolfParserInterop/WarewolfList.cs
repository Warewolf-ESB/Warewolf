using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Common;
using Warewolf.Resource.Errors;

namespace WarewolfParserInterop
{
  public class WarewolfAtomList<T>:IEnumerable<T>
    {
        private T[] _values;
        private int _count;
        private readonly T _defaultValue;
      IEnumerator<T> _currentEnumerator;
      T _currentValue;

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
          if (x.Current != null)
          {
              _currentValue = x.Current;
          }
          x.MoveNext();
          if (x.Current == null)
          {
              return _currentValue;
          }
          return x.Current;
      }

      IEnumerator<T> GetCurrentEnumerator()
      {
          return _currentEnumerator ?? (_currentEnumerator = GetEnumerator());
      }

      public void ResetCurrentEnumerator()
      {
          _currentEnumerator?.Reset();
      }

      [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            return _values.Take(_count).ToList().GetEnumerator();
        }


      // ReSharper restore FunctionNeverReturns

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int i]
        {
            get
            {
                if (i < _count)
                    return _values[i];
                throw new NullValueInVariableException("the recordset does not have the row"+i,"");
            }
            set {
                if (i < _count)
                    _values[i] = value;
                else if (i == Count)
                    AddSomething(value);
                else throw new NullValueInVariableException(ErrorResource.RecordsetDoesNotHaveRow + i, "");
                
            }
        }

        public int Count => _count;

      public int Length => _values.Length;

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

