using System;

namespace Warewolf.DistributedStore
{

    public class ListNotification
    {
        public Type Type { get; set; }

        public ListNotification()
        {
        }

        protected ListNotification(Type type)
        {
            Type = type;
        }
    }
    public class Add<T> : ListNotification {
        public Add(T value)
            :base(typeof(Add<T>))
        {
            Value = value;
        }
        public T Value { get; set; }
    }

    public class Remove<T> : ListNotification {
        public Remove(T value)
            :base(typeof(Remove<T>))
        {
            Value = value;
        }
        public T Value { get; set; }
    }

    public class Clear : ListNotification {
        public Clear()
            :base(typeof(Clear))
        {
        }
    }
}
