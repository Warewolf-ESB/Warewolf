using System;

namespace IronRuby.Runtime
{
	public static class Key
	{
		public static Key<T0, T1> Create<T0, T1>(T0 first, T1 second)
		{
			return new Key<T0, T1>(first, second);
		}

		public static Key<T0, T1, T2> Create<T0, T1, T2>(T0 first, T1 second, T2 third)
		{
			return new Key<T0, T1, T2>(first, second, third);
		}
	}
	[Serializable]
	public class Key<T0, T1> : IEquatable<Key<T0, T1>>
	{
		public readonly T0 First;

		public readonly T1 Second;

		public Key(T0 first, T1 second)
		{
			First = first;
			Second = second;
		}

		public override int GetHashCode()
		{
			T0 first = First;
			int hashCode = first.GetHashCode();
			T1 second = Second;
			return hashCode ^ second.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Key<T0, T1> key = obj as Key<T0, T1>;
			if (key != null)
			{
				return Equals(key);
			}
			return false;
		}

		public bool Equals(Key<T0, T1> other)
		{
			if ((object)this != other)
			{
				T0 first = First;
				if (first.Equals(other.First))
				{
					T1 second = Second;
					return second.Equals(other.Second);
				}
				return false;
			}
			return true;
		}

		public static bool operator ==(Key<T0, T1> s, Key<T0, T1> t)
		{
			return s.Equals(t);
		}

		public static bool operator !=(Key<T0, T1> s, Key<T0, T1> t)
		{
			return !s.Equals(t);
		}
	}
	[Serializable]
	public sealed class Key<T0, T1, T2> : Key<T0, T1>, IEquatable<Key<T0, T1, T2>>
	{
		public readonly T2 Third;

		public Key(T0 first, T1 second, T2 third)
			: base(first, second)
		{
			Third = third;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			T2 third = Third;
			return hashCode ^ third.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Key<T0, T1, T2> key = obj as Key<T0, T1, T2>;
			if (key != null)
			{
				return Equals(key);
			}
			return false;
		}

		public bool Equals(Key<T0, T1, T2> other)
		{
			if ((object)this != other)
			{
				if (Equals((Key<T0, T1>)other))
				{
					T2 third = Third;
					return third.Equals(other.Third);
				}
				return false;
			}
			return true;
		}

		public static bool operator ==(Key<T0, T1, T2> s, Key<T0, T1, T2> t)
		{
			return s.Equals(t);
		}

		public static bool operator !=(Key<T0, T1, T2> s, Key<T0, T1, T2> t)
		{
			return !s.Equals(t);
		}
	}
}
