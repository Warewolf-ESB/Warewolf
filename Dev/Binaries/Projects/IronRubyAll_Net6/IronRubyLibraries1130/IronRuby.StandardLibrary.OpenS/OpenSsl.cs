using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.OpenSsl
{
	[RubyModule("OpenSSL")]
	public static class OpenSsl
	{
		[RubyModule("Digest")]
		public static class DigestFactory
		{
			[RubyClass("Digest")]
			public class Digest
			{
				private System.Security.Cryptography.HMAC _algorithm;

				public System.Security.Cryptography.HMAC Algorithm
				{
					get
					{
						return _algorithm;
					}
				}

				protected Digest()
				{
				}

				[RubyConstructor]
				public static Digest CreateDigest(RubyClass self, [NotNull] MutableString algorithmName)
				{
					return Initialize(new Digest(), algorithmName);
				}

				[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
				public static Digest Initialize(Digest self, [NotNull] MutableString algorithmName)
				{
					System.Security.Cryptography.HMAC hMAC = System.Security.Cryptography.HMAC.Create("HMAC" + algorithmName.ConvertToString());
					if (hMAC == null)
					{
						throw RubyExceptions.CreateRuntimeError("Unsupported digest algorithm ({0}).", algorithmName);
					}
					self._algorithm = hMAC;
					return self;
				}

				[RubyMethod("reset")]
				public static Digest Reset(Digest self)
				{
					self._algorithm.Clear();
					return self;
				}

				[RubyMethod("name")]
				public static MutableString Name(Digest self)
				{
					return MutableString.CreateAscii(self._algorithm.HashName);
				}

				[RubyMethod("digest_size")]
				public static int Seed(Digest self)
				{
					return self._algorithm.OutputBlockSize;
				}

				[RubyMethod("digest")]
				public static MutableString BlankDigest(Digest self)
				{
					byte[] bytes = Encoding.UTF8.GetBytes("");
					byte[] bytes2 = new SHA1CryptoServiceProvider().ComputeHash(bytes);
					return MutableString.CreateBinary(bytes2);
				}

				[RubyMethod("hexdigest")]
				public static MutableString BlankHexDigest(Digest self)
				{
					byte[] bytes = Encoding.UTF8.GetBytes("");
					byte[] array = new SHA1CryptoServiceProvider().ComputeHash(bytes);
					return MutableString.CreateAscii(BitConverter.ToString(array).Replace("-", "").ToLowerInvariant());
				}
			}
		}

		[RubyClass("HMAC")]
		public class HMAC
		{
			internal static byte[] Digest(DigestFactory.Digest digest, MutableString key, MutableString data)
			{
				digest.Algorithm.Key = key.ConvertToBytes();
				return digest.Algorithm.ComputeHash(data.ConvertToBytes());
			}

			[RubyMethod("hexdigest", RubyMethodAttributes.PublicSingleton)]
			public static MutableString HexDigest(RubyClass self, [NotNull] DigestFactory.Digest digest, [NotNull] MutableString key, [NotNull] MutableString data)
			{
				byte[] array = Digest(digest, key, data);
				return MutableString.CreateAscii(BitConverter.ToString(array).Replace("-", "").ToLowerInvariant());
			}

			[RubyMethod("digest", RubyMethodAttributes.PublicSingleton)]
			public static MutableString Digest(RubyClass self, [NotNull] DigestFactory.Digest digest, [NotNull] MutableString key, [NotNull] MutableString data)
			{
				byte[] bytes = Digest(digest, key, data);
				return MutableString.CreateBinary(bytes);
			}
		}

		[RubyModule("Random")]
		public static class RandomModule
		{
			[RubyMethod("seed", RubyMethodAttributes.PublicSingleton)]
			public static MutableString Seed(RubyModule self, [DefaultProtocol][NotNull] MutableString seed)
			{
				return seed;
			}

			[RubyMethod("pseudo_bytes", RubyMethodAttributes.PublicSingleton)]
			[RubyMethod("random_bytes", RubyMethodAttributes.PublicSingleton)]
			public static MutableString RandomBytes(RubyModule self, [DefaultProtocol] int length)
			{
				if (length < 0)
				{
					throw RubyExceptions.CreateArgumentError("negative string size");
				}
				if (length == 0)
				{
					return MutableString.CreateEmpty();
				}
				byte[] array = new byte[length];
				RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
				rNGCryptoServiceProvider.GetBytes(array);
				return MutableString.CreateBinary(array);
			}
		}

		[RubyClass("BN")]
		public class BN
		{
			[RubyMethod("rand", RubyMethodAttributes.PublicSingleton)]
			public static BigInteger Rand(RubyClass self, [DefaultProtocol] int bits, [Optional][DefaultProtocol] int someFlag, [Optional] bool otherFlag)
			{
				byte[] array = new byte[bits >> 3];
				RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
				rNGCryptoServiceProvider.GetBytes(array);
				uint[] array2 = new uint[array.Length >> 2];
				int num = 0;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = (uint)(array[num] + (array[num + 1] << 8) + (array[num + 2] << 16) + (array[num + 3] << 24));
					num += 4;
				}
				return new BigInteger(1, array2);
			}
		}

		[RubyModule("X509")]
		public static class X509
		{
			[RubyClass("CertificateError", Extends = typeof(CryptographicException), Inherits = typeof(ExternalException))]
			public class CryptographicExceptionOps
			{
				[RubyConstructor]
				public static CryptographicException Create(RubyClass self, [DefaultProtocol] MutableString message)
				{
					CryptographicException ex = new CryptographicException(RubyExceptions.MakeMessage(ref message, "Not enought data."));
					RubyExceptionData.InitializeException(ex, message);
					return ex;
				}
			}

			[RubyClass("Certificate")]
			public class Certificate
			{
				private X509Certificate _certificate;

				private int SerailNumber
				{
					get
					{
						if (_certificate.Handle == IntPtr.Zero)
						{
							return 0;
						}
						return int.Parse(_certificate.GetSerialNumberString(), CultureInfo.InvariantCulture);
					}
				}

				[RubyConstructor]
				public static Certificate CreateCertificate(RubyClass self)
				{
					return Initialize(new Certificate(), null);
				}

				[RubyConstructor]
				public static Certificate CreateCertificate(RubyClass self, MutableString data)
				{
					return Initialize(new Certificate(), data);
				}

				[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
				public static Certificate Initialize(Certificate self, MutableString data)
				{
					if (data == null)
					{
						self._certificate = new X509Certificate();
					}
					else
					{
						self._certificate = new X509Certificate(data.ToByteArray());
					}
					return self;
				}

				private static string OpenSSLFormat(string x509String)
				{
					string[] array = x509String.Split(',');
					Array.Sort(array);
					StringBuilder stringBuilder = new StringBuilder();
					string[] array2 = array;
					foreach (string text in array2)
					{
						stringBuilder.AppendFormat("/{0}", text.Trim());
					}
					return stringBuilder.ToString();
				}

				[RubyMethod("issuer")]
				public static MutableString Issuer(Certificate self)
				{
					if (self._certificate.Handle == IntPtr.Zero)
					{
						return null;
					}
					return MutableString.CreateAscii(OpenSSLFormat(self._certificate.Issuer));
				}

				[RubyMethod("public_key")]
				public static MutableString PublicKey(Certificate self)
				{
					if (self._certificate.Handle == IntPtr.Zero)
					{
						return MutableString.CreateEmpty();
					}
					return MutableString.CreateAscii(self._certificate.GetPublicKeyString());
				}

				[RubyMethod("serial")]
				public static int Serial(Certificate self)
				{
					return self.SerailNumber;
				}

				[RubyMethod("subject")]
				public static MutableString Subject(Certificate self)
				{
					if (self._certificate.Handle == IntPtr.Zero)
					{
						return null;
					}
					return MutableString.CreateAscii(OpenSSLFormat(self._certificate.Subject));
				}

				[RubyMethod("to_s")]
				[RubyMethod("inspect")]
				public static MutableString ToString(RubyContext context, Certificate self)
				{
					using (IDisposable disposable = RubyUtils.InfiniteInspectTracker.TrackObject(self))
					{
						MutableString mutableString = MutableString.CreateEmpty();
						mutableString.Append("#<");
						mutableString.Append(context.Inspect(context.GetClassOf(self)));
						if (disposable == null)
						{
							return mutableString.Append(":...>");
						}
						bool flag = self._certificate.Handle == IntPtr.Zero;
						mutableString.AppendFormat(" subject={0}, issuer={1}, serial={2}, not_before=nil, not_after=nil>", flag ? "" : OpenSSLFormat(self._certificate.Subject), flag ? "" : OpenSSLFormat(self._certificate.Issuer), (!flag) ? self.SerailNumber : 0);
						return mutableString;
					}
				}

				[RubyMethod("version")]
				public static int Version(Certificate self)
				{
					if (self._certificate.Handle == IntPtr.Zero)
					{
						return 0;
					}
					return 2;
				}
			}

			[RubyClass("Name")]
			public class Name
			{
			}
		}

		[RubyModule("PKey")]
		public static class PKey
		{
			[RubyClass("RSA")]
			public class RSA
			{
			}
		}

		[Serializable]
		[RubyClass("OpenSSLError")]
		public class OpenSSLError : SystemException
		{
			private const string M = "OpenSSL error";

			public OpenSSLError()
				: this(null, null)
			{
			}

			public OpenSSLError(string message)
				: this(message, null)
			{
			}

			public OpenSSLError(string message, Exception inner)
				: base(RubyExceptions.MakeMessage(message, "OpenSSL error"), inner)
			{
			}

			public OpenSSLError(MutableString message)
				: base(RubyExceptions.MakeMessage(ref message, "OpenSSL error"))
			{
				RubyExceptionData.InitializeException(this, message);
			}

			protected OpenSSLError(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		[RubyModule("SSL")]
		public static class SSL
		{
			[Serializable]
			[RubyClass("SSLError")]
			public class SSLError : OpenSSLError
			{
				private const string M = "SSL error";

				public SSLError()
					: this(null, null)
				{
				}

				public SSLError(string message)
					: this(message, null)
				{
				}

				public SSLError(string message, Exception inner)
					: base(RubyExceptions.MakeMessage(message, "SSL error"), inner)
				{
				}

				public SSLError(MutableString message)
					: base(RubyExceptions.MakeMessage(ref message, "SSL error"))
				{
					RubyExceptionData.InitializeException(this, message);
				}

				protected SSLError(SerializationInfo info, StreamingContext context)
					: base(info, context)
				{
				}
			}
		}

		[RubyConstant]
		public const string OPENSSL_VERSION = "OpenSSL 0.9.8d 28 Sep 2006";

		[RubyConstant]
		public const double OPENSSL_VERSION_NUMBER = 9470031.0;

		[RubyConstant]
		public const string VERSION = "1.0.0";
	}
}
