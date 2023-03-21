using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Digest
{
	[RubyModule("Digest")]
	public static class Digest
	{
		[Includes(new Type[] { typeof(Instance) })]
		[RubyClass("Class")]
		public class Class
		{
			[RubyMethod("digest", RubyMethodAttributes.PublicSingleton)]
			public static MutableString Digest(CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, CallSiteStorage<Func<CallSite, object, MutableString, object>> digestStorage, RubyClass self, [DefaultProtocol][NotNull] MutableString str)
			{
				CallSite<Func<CallSite, RubyClass, object>> callSite = allocateStorage.GetCallSite("allocate", 0);
				object arg = callSite.Target(callSite, self);
				CallSite<Func<CallSite, object, MutableString, object>> callSite2 = digestStorage.GetCallSite("digest", 1);
				return (MutableString)callSite2.Target(callSite2, arg, str);
			}

			[RubyMethod("digest", RubyMethodAttributes.PublicSingleton)]
			public static MutableString Digest(RubyClass self)
			{
				throw RubyExceptions.CreateArgumentError("no data given");
			}

			[RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
			public static MutableString HexDigest(CallSiteStorage<Func<CallSite, object, MutableString, object>> storage, RubyClass self, [DefaultProtocol][NotNull] MutableString str)
			{
				CallSite<Func<CallSite, object, MutableString, object>> callSite = storage.GetCallSite("digest", 1);
				MutableString str2 = (MutableString)callSite.Target(callSite, self, str);
				return HexEncode(str2);
			}

			[RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
			public static MutableString HexDigest(RubyClass self)
			{
				throw RubyExceptions.CreateArgumentError("no data given");
			}

			internal static MutableString Bytes2Hex(byte[] bytes)
			{
				return MutableString.CreateAscii(BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant());
			}

			internal static MutableString HexEncode(MutableString str)
			{
				return Bytes2Hex(str.ConvertToBytes());
			}
		}

		[RubyClass("Base")]
		public class Base : Class
		{
			private readonly HashAlgorithm _algorithm;

			private MutableString _buffer;

			protected Base(HashAlgorithm algorithm)
			{
				_algorithm = algorithm;
				_buffer = MutableString.CreateBinary();
			}

			[RubyMethod("update")]
			[RubyMethod("<<")]
			public static Base Update(RubyContext context, Base self, MutableString str)
			{
				self._buffer.Append(str);
				return self;
			}

			[RubyMethod("finish", RubyMethodAttributes.PrivateInstance)]
			public static MutableString Finish(RubyContext context, Base self)
			{
				byte[] buffer = self._buffer.ConvertToBytes();
				byte[] bytes = self._algorithm.ComputeHash(buffer);
				return MutableString.CreateBinary(bytes);
			}

			[RubyMethod("reset")]
			public static Base Reset(RubyContext context, Base self)
			{
				self._buffer = MutableString.CreateBinary();
				self._algorithm.Initialize();
				return self;
			}
		}

		[RubyClass("MD5", BuildConfig = "!SILVERLIGHT")]
		public class MD5 : Base
		{
			public MD5()
				: base(System.Security.Cryptography.MD5.Create())
			{
			}
		}

		[RubyClass("SHA1", BuildConfig = "!SILVERLIGHT")]
		public class SHA1 : Base
		{
			public SHA1()
				: base(System.Security.Cryptography.SHA1.Create())
			{
			}
		}

		[RubyClass("SHA256", BuildConfig = "!SILVERLIGHT")]
		public class SHA256 : Base
		{
			public SHA256()
				: base(System.Security.Cryptography.SHA256.Create())
			{
			}
		}

		[RubyClass("SHA384", BuildConfig = "!SILVERLIGHT")]
		public class SHA384 : Base
		{
			public SHA384()
				: base(System.Security.Cryptography.SHA384.Create())
			{
			}
		}

		[RubyClass("SHA512", BuildConfig = "!SILVERLIGHT")]
		public class SHA512 : Base
		{
			public SHA512()
				: base(System.Security.Cryptography.SHA512.Create())
			{
			}
		}

		[RubyModule("Instance")]
		public class Instance
		{
			[RubyMethod("digest")]
			public static MutableString Digest(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, CallSiteStorage<Func<CallSite, object, object>> finishStorage, object self)
			{
				object copy;
				if (!RubyUtils.TryDuplicateObject(initializeCopyStorage, allocateStorage, self, true, out copy))
				{
					throw RubyExceptions.CreateArgumentError("unable to copy object");
				}
				CallSite<Func<CallSite, object, object>> callSite = finishStorage.GetCallSite("finish", 0);
				return (MutableString)callSite.Target(callSite, copy);
			}

			[RubyMethod("digest")]
			public static MutableString Digest(CallSiteStorage<Func<CallSite, object, MutableString, object>> updateStorage, CallSiteStorage<Func<CallSite, object, object>> finishStorage, CallSiteStorage<Func<CallSite, object, object>> resetStorage, object self, [NotNull][DefaultProtocol] MutableString str)
			{
				CallSite<Func<CallSite, object, MutableString, object>> callSite = updateStorage.GetCallSite("update", 1);
				callSite.Target(callSite, self, str);
				CallSite<Func<CallSite, object, object>> callSite2 = finishStorage.GetCallSite("finish", 0);
				object obj = callSite2.Target(callSite2, self);
				CallSite<Func<CallSite, object, object>> callSite3 = resetStorage.GetCallSite("reset", 0);
				callSite3.Target(callSite3, self);
				return (MutableString)obj;
			}

			[RubyMethod("digest!")]
			public static MutableString DigestNew(CallSiteStorage<Func<CallSite, object, object>> finishStorage, CallSiteStorage<Func<CallSite, object, object>> resetStorage, object self)
			{
				CallSite<Func<CallSite, object, object>> callSite = finishStorage.GetCallSite("finish", 0);
				object obj = callSite.Target(callSite, self);
				CallSite<Func<CallSite, object, object>> callSite2 = resetStorage.GetCallSite("reset", 0);
				callSite2.Target(callSite2, self);
				return (MutableString)obj;
			}

			[RubyMethod("hexdigest")]
			public static MutableString HexDigest(CallSiteStorage<Func<CallSite, object, object, object>> initializeCopyStorage, CallSiteStorage<Func<CallSite, RubyClass, object>> allocateStorage, CallSiteStorage<Func<CallSite, object, object>> finishStorage, object self)
			{
				return Class.HexEncode(Digest(initializeCopyStorage, allocateStorage, finishStorage, self));
			}

			[RubyMethod("hexdigest")]
			public static MutableString HexDigest(CallSiteStorage<Func<CallSite, object, MutableString, object>> updateStorage, CallSiteStorage<Func<CallSite, object, object>> finishStorage, CallSiteStorage<Func<CallSite, object, object>> resetStorage, object self, [DefaultProtocol][NotNull] MutableString str)
			{
				return Class.HexEncode(Digest(updateStorage, finishStorage, resetStorage, self, str));
			}

			[RubyMethod("hexdigest!")]
			public static MutableString HexDigestNew(CallSiteStorage<Func<CallSite, object, object>> finishStorage, CallSiteStorage<Func<CallSite, object, object>> resetStorage, object self)
			{
				return Class.HexEncode(DigestNew(finishStorage, resetStorage, self));
			}
		}

		[RubyMethod("const_missing", RubyMethodAttributes.PublicSingleton)]
		public static object ConstantMissing(RubyModule self, [NotNull][DefaultProtocol] string name)
		{
			throw new NotImplementedException();
		}

		[RubyMethod("hexencode", RubyMethodAttributes.PublicSingleton)]
		public static MutableString HexEncode(RubyModule self, [NotNull] MutableString str)
		{
			throw new NotImplementedException();
		}
	}
}
