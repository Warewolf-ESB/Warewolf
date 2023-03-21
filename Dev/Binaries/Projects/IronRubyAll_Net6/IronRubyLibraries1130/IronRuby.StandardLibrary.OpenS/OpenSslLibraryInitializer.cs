using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.OpenSsl
{
	public sealed class OpenSslLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(object));
			RubyClass class2 = GetClass(typeof(SystemException));
			RubyClass class3 = GetClass(typeof(ExternalException));
			RubyModule module = DefineGlobalModule("OpenSSL", typeof(OpenSsl), 8, null, null, LoadOpenSSL_Constants, RubyModule.EmptyArray);
			RubyClass value = DefineClass("OpenSSL::BN", typeof(OpenSsl.BN), 8, @class, null, LoadOpenSSL__BN_Class, null, RubyModule.EmptyArray);
			RubyModule rubyModule = DefineModule("OpenSSL::Digest", typeof(OpenSsl.DigestFactory), 8, null, null, null, RubyModule.EmptyArray);
			RubyClass value2 = DefineClass("OpenSSL::Digest::Digest", typeof(OpenSsl.DigestFactory.Digest), 8, @class, LoadOpenSSL__Digest__Digest_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, OpenSsl.DigestFactory.Digest>(OpenSsl.DigestFactory.Digest.CreateDigest));
			RubyClass value3 = DefineClass("OpenSSL::HMAC", typeof(OpenSsl.HMAC), 8, @class, null, LoadOpenSSL__HMAC_Class, null, RubyModule.EmptyArray);
			RubyClass rubyClass = DefineClass("OpenSSL::OpenSSLError", typeof(OpenSsl.OpenSSLError), 8, class2, null, null, null, RubyModule.EmptyArray);
			RubyModule rubyModule2 = DefineModule("OpenSSL::PKey", typeof(OpenSsl.PKey), 8, null, null, null, RubyModule.EmptyArray);
			RubyClass value4 = DefineClass("OpenSSL::PKey::RSA", typeof(OpenSsl.PKey.RSA), 8, @class, null, null, null, RubyModule.EmptyArray);
			RubyModule value5 = DefineModule("OpenSSL::Random", typeof(OpenSsl.RandomModule), 8, null, LoadOpenSSL__Random_Class, null, RubyModule.EmptyArray);
			RubyModule rubyModule3 = DefineModule("OpenSSL::SSL", typeof(OpenSsl.SSL), 8, null, null, null, RubyModule.EmptyArray);
			RubyModule rubyModule4 = DefineModule("OpenSSL::X509", typeof(OpenSsl.X509), 8, null, null, null, RubyModule.EmptyArray);
			RubyClass value6 = DefineClass("OpenSSL::X509::Certificate", typeof(OpenSsl.X509.Certificate), 8, @class, LoadOpenSSL__X509__Certificate_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, OpenSsl.X509.Certificate>(OpenSsl.X509.Certificate.CreateCertificate), new Func<RubyClass, MutableString, OpenSsl.X509.Certificate>(OpenSsl.X509.Certificate.CreateCertificate));
			RubyClass value7 = DefineClass("OpenSSL::X509::CertificateError", typeof(CryptographicException), 0, class3, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, CryptographicException>(OpenSsl.X509.CryptographicExceptionOps.Create));
			RubyClass value8 = DefineClass("OpenSSL::X509::Name", typeof(OpenSsl.X509.Name), 8, @class, null, null, null, RubyModule.EmptyArray);
			RubyClass value9 = DefineClass("OpenSSL::SSL::SSLError", typeof(OpenSsl.SSL.SSLError), 8, rubyClass, null, null, null, RubyModule.EmptyArray);
			LibraryInitializer.SetConstant(module, "BN", value);
			LibraryInitializer.SetConstant(module, "Digest", rubyModule);
			LibraryInitializer.SetConstant(rubyModule, "Digest", value2);
			LibraryInitializer.SetConstant(module, "HMAC", value3);
			LibraryInitializer.SetConstant(module, "OpenSSLError", rubyClass);
			LibraryInitializer.SetConstant(module, "PKey", rubyModule2);
			LibraryInitializer.SetConstant(rubyModule2, "RSA", value4);
			LibraryInitializer.SetConstant(module, "Random", value5);
			LibraryInitializer.SetConstant(module, "SSL", rubyModule3);
			LibraryInitializer.SetConstant(module, "X509", rubyModule4);
			LibraryInitializer.SetConstant(rubyModule4, "Certificate", value6);
			LibraryInitializer.SetConstant(rubyModule4, "CertificateError", value7);
			LibraryInitializer.SetConstant(rubyModule4, "Name", value8);
			LibraryInitializer.SetConstant(rubyModule3, "SSLError", value9);
		}

		private static void LoadOpenSSL_Constants(RubyModule module)
		{
			LibraryInitializer.SetConstant(module, "OPENSSL_VERSION", "OpenSSL 0.9.8d 28 Sep 2006");
			LibraryInitializer.SetConstant(module, "OPENSSL_VERSION_NUMBER", 9470031.0);
			LibraryInitializer.SetConstant(module, "VERSION", "1.0.0");
		}

		private static void LoadOpenSSL__BN_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "rand", 33, 196608u, new Func<RubyClass, int, int, bool, BigInteger>(OpenSsl.BN.Rand));
		}

		private static void LoadOpenSSL__Digest__Digest_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "digest", 17, 0u, new Func<OpenSsl.DigestFactory.Digest, MutableString>(OpenSsl.DigestFactory.Digest.BlankDigest));
			LibraryInitializer.DefineLibraryMethod(module, "digest_size", 17, 0u, new Func<OpenSsl.DigestFactory.Digest, int>(OpenSsl.DigestFactory.Digest.Seed));
			LibraryInitializer.DefineLibraryMethod(module, "hexdigest", 17, 0u, new Func<OpenSsl.DigestFactory.Digest, MutableString>(OpenSsl.DigestFactory.Digest.BlankHexDigest));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 2u, new Func<OpenSsl.DigestFactory.Digest, MutableString, OpenSsl.DigestFactory.Digest>(OpenSsl.DigestFactory.Digest.Initialize));
			LibraryInitializer.DefineLibraryMethod(module, "name", 17, 0u, new Func<OpenSsl.DigestFactory.Digest, MutableString>(OpenSsl.DigestFactory.Digest.Name));
			LibraryInitializer.DefineLibraryMethod(module, "reset", 17, 0u, new Func<OpenSsl.DigestFactory.Digest, OpenSsl.DigestFactory.Digest>(OpenSsl.DigestFactory.Digest.Reset));
		}

		private static void LoadOpenSSL__HMAC_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "digest", 33, 14u, new Func<RubyClass, OpenSsl.DigestFactory.Digest, MutableString, MutableString, MutableString>(OpenSsl.HMAC.Digest));
			LibraryInitializer.DefineLibraryMethod(module, "hexdigest", 33, 14u, new Func<RubyClass, OpenSsl.DigestFactory.Digest, MutableString, MutableString, MutableString>(OpenSsl.HMAC.HexDigest));
		}

		private static void LoadOpenSSL__Random_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "pseudo_bytes", 33, 65536u, new Func<RubyModule, int, MutableString>(OpenSsl.RandomModule.RandomBytes));
			LibraryInitializer.DefineLibraryMethod(module, "random_bytes", 33, 65536u, new Func<RubyModule, int, MutableString>(OpenSsl.RandomModule.RandomBytes));
			LibraryInitializer.DefineLibraryMethod(module, "seed", 33, 65538u, new Func<RubyModule, MutableString, MutableString>(OpenSsl.RandomModule.Seed));
		}

		private static void LoadOpenSSL__X509__Certificate_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 0u, new Func<OpenSsl.X509.Certificate, MutableString, OpenSsl.X509.Certificate>(OpenSsl.X509.Certificate.Initialize));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 17, 0u, new Func<RubyContext, OpenSsl.X509.Certificate, MutableString>(OpenSsl.X509.Certificate.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "issuer", 17, 0u, new Func<OpenSsl.X509.Certificate, MutableString>(OpenSsl.X509.Certificate.Issuer));
			LibraryInitializer.DefineLibraryMethod(module, "public_key", 17, 0u, new Func<OpenSsl.X509.Certificate, MutableString>(OpenSsl.X509.Certificate.PublicKey));
			LibraryInitializer.DefineLibraryMethod(module, "serial", 17, 0u, new Func<OpenSsl.X509.Certificate, int>(OpenSsl.X509.Certificate.Serial));
			LibraryInitializer.DefineLibraryMethod(module, "subject", 17, 0u, new Func<OpenSsl.X509.Certificate, MutableString>(OpenSsl.X509.Certificate.Subject));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 17, 0u, new Func<RubyContext, OpenSsl.X509.Certificate, MutableString>(OpenSsl.X509.Certificate.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "version", 17, 0u, new Func<OpenSsl.X509.Certificate, int>(OpenSsl.X509.Certificate.Version));
		}
	}
}
