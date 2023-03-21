using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime
{
	public class EqualityComparer : IEqualityComparer<object>
	{
		private readonly CallSite<Func<CallSite, object, object>> _hashSite;

		private readonly CallSite<Func<CallSite, object, object, object>> _eqlSite;

		internal EqualityComparer(RubyContext context)
			: this(CallSite<Func<CallSite, object, object>>.Create(RubyCallAction.Make(context, "hash", RubyCallSignature.WithImplicitSelf(0))), CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(context, "eql?", RubyCallSignature.WithImplicitSelf(1))))
		{
		}

		public EqualityComparer(UnaryOpStorage hashStorage, BinaryOpStorage eqlStorage)
			: this(hashStorage.GetCallSite("hash"), eqlStorage.GetCallSite("eql?"))
		{
		}

		public EqualityComparer(CallSite<Func<CallSite, object, object>> hashSite, CallSite<Func<CallSite, object, object, object>> eqlSite)
		{
			ContractUtils.RequiresNotNull(hashSite, "hashSite");
			ContractUtils.RequiresNotNull(eqlSite, "eqlSite");
			_hashSite = hashSite;
			_eqlSite = eqlSite;
		}

		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			if (x == y)
			{
				return true;
			}
			if (x is int)
			{
				if (y is int)
				{
					return (int)x == (int)y;
				}
				return false;
			}
			return RubyOps.IsTrue(_eqlSite.Target(_eqlSite, x, y));
		}

		int IEqualityComparer<object>.GetHashCode(object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}
			return Protocols.ToHashCode(_hashSite.Target(_hashSite, obj));
		}
	}
}
