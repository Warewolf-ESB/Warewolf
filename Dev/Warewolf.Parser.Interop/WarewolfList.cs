#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Common;
using Warewolf.Resource.Errors;

namespace WarewolfParserInterop
{
    public class WarewolfAtomList<T>:IEnumerable<T>
    {
        T[] _values;
        int _count;
        readonly T _defaultValue;
        IEnumerator<T> _currentEnumerator;
      T _currentValue;

      public WarewolfAtomList(T defaultValue)
        {
           _count= 32;
            _values = new T[32];
            for (int a = 0; a < _count;a++)
            {
                _values[a] = defaultValue;
            }

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
                for (int a = 0; a < _count; a++)
                {
                    _values[a] = defaultValue;
                }

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
                for (int a = 0; a < count; a++)
                {
                    _values[a] = defaultValue;
                }

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

        void ResizeToCount()
        {
            if (_count >= _values.Length - 1)
            {
                Array.Resize(ref _values, _values.Length * 2);
                for (int a = _count + 1; a < _values.Length; a++)
                {
                    _values[a] = _defaultValue;
                }
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

        IEnumerator<T> GetCurrentEnumerator() => _currentEnumerator ?? (_currentEnumerator = GetEnumerator());

        public void ResetCurrentEnumerator()
      {
          _currentEnumerator?.Reset();
      }

  
      public bool Apply (Func<T,T> action )
      {
          for(int i = 0; i < _count; i++)
            {
                _values[i] = action(_values[i]);
            }
          return true;
      }

        public IEnumerator<T> GetEnumerator() => _values.Take(_count).ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int i]
        {
            get
            {
                if (i < _count)
                {
                    return _values[i];
                }

                throw new NullValueInVariableException("the recordset does not have the row"+i,"");
            }
            set {
                if (i < _count)
                {
                    _values[i] = value;
                }
                else if (i == Count)
                {
                    AddSomething(value);
                }
                else
                {
                    throw new NullValueInVariableException(ErrorResource.RecordsetDoesNotHaveRow + i, "");
                }
            }
        }

        public int Count => _count;

      public int Length => _values.Length;

    
        public WarewolfAtomList<T> DeletePosition(int position)
      {
          var lst = new WarewolfAtomList<T>(_defaultValue);
          for(int i = 0;i< Count;i++)
          {
              if(i!= position)
                {
                    lst.AddSomething(this[i]);
                }
            }

          return lst;
      }


    
        public IEnumerable<int> Where(Func<T, bool> func)
      {

          for (int i = 0; i < _count; i++)
          {
              if (func?.Invoke(_values[i]) ?? default(bool))
              {
                  yield return i;

              }
          }
      }
    }
}

