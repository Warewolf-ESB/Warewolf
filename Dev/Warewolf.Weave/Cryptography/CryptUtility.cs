// Decompiled with JetBrains decompiler
// Type: System.Cryptography.CryptUtility
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System.Cryptography
{
  public static class CryptUtility
  {
    private static object _syncSHA1 = new object();
    private static byte[] _sha1Buffer = new byte[1024];
    private static SHA1CryptoServiceProvider _sha1Provider = new SHA1CryptoServiceProvider();
    private static MD5 _md5 = MD5.Create();
    internal static Encoding Encoding = Encoding.ASCII;

    public static string GetHashSHA1String(string phrase) => BitConverter.ToString(CryptUtility.GetHashSHA1(phrase));

    public static byte[] GetHashSHA1(string phrase)
    {
      int length = phrase.Length;
      lock (CryptUtility._syncSHA1)
      {
        if (length > CryptUtility._sha1Buffer.Length)
          CryptUtility._sha1Buffer = new byte[length];
        int bytes = CryptUtility.Encoding.GetBytes(phrase, 0, length, CryptUtility._sha1Buffer, 0);
        return CryptUtility._sha1Provider.ComputeHash(CryptUtility._sha1Buffer, 0, bytes);
      }
    }

    public static byte[] CalculateMD5(Stream buffer)
    {
      lock (CryptUtility._md5)
        return CryptUtility._md5.ComputeHash(buffer);
    }

    public static byte[] CalculateMD5(byte[] buffer)
    {
      lock (CryptUtility._md5)
        return CryptUtility._md5.ComputeHash(buffer);
    }

    public static byte[] CalculateMD5(byte[] buffer, int offset, int count)
    {
      lock (CryptUtility._md5)
        return CryptUtility._md5.ComputeHash(buffer, offset, count);
    }

    public static byte[] FinalizeHash(HashAlgorithm algorithm, params HashDataBroker[] brokers)
    {
      MemoryStream inputStream = new MemoryStream();
      foreach (HashDataBroker broker in brokers)
        inputStream.Write(broker.RawData, 0, broker.Length);
      inputStream.Position = 0L;
      return algorithm.ComputeHash((Stream) inputStream);
    }

    public static BigInteger HashToBigInteger(
      HashAlgorithm algorithm,
      params HashDataBroker[] brokers)
    {
      return new BigInteger(CryptUtility.FinalizeHash(algorithm, brokers));
    }
  }
}
