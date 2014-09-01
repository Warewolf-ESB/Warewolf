using System;
using System.Collections.Generic;

namespace Dev2
{
    public class ObservablePair<TKey, TValue> : ObservableObject, IEquatable<ObservablePair<TKey, TValue>>
    {
        TKey _key;
        TValue _value;

        public ObservablePair()
        {
        }

        public ObservablePair(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }

        public TKey Key { get { return _key; } set { OnPropertyChanged(ref _key, value); } }

        public TValue Value { get { return _value; } set { OnPropertyChanged(ref _value, value); } }

        public bool Equals(ObservablePair<TKey, TValue> other)
        {
            return other != null && EqualityComparer<TKey>.Default.Equals(Key, other.Key);
        }

        public override bool Equals(Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            var other = obj as ObservablePair<TKey, TValue>;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }


        public static bool operator ==(ObservablePair<TKey, TValue> pair1, ObservablePair<TKey, TValue> pair2)
        {
            if((object)pair1 == null || ((object)pair2) == null)
            {
                return Equals(pair1, pair2);
            }
            return pair1.Equals(pair2);
        }

        public static bool operator !=(ObservablePair<TKey, TValue> pair1, ObservablePair<TKey, TValue> pair2)
        {
            if(pair1 == null || pair2 == null)
            {
                return !Equals(pair1, pair2);
            }
            return !(pair1.Equals(pair2));
        }
    }
}
