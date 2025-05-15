// Decompiled with JetBrains decompiler
// Type: System.NullCryptProvider
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public sealed class NullCryptProvider : ICryptProvider, IDecryptionProvider, IEncryptionProvider
  {
    public static readonly NullCryptProvider Singleton = new NullCryptProvider();

    private NullCryptProvider()
    {
    }

    public int NextKSRN() => 0;

    public byte Decrypt(byte encrypted) => encrypted;

    public void Decrypt(byte[] data, int offset, int length)
    {
    }

    public void Decrypt(
      byte[] encrypted,
      int encryptOffset,
      byte[] decrypted,
      int decryptOffset,
      int count)
    {
      Buffer.BlockCopy((Array) encrypted, encryptOffset, (Array) decrypted, decryptOffset, count);
    }

    public byte Encrypt(byte decrypted) => decrypted;

    public void Encrypt(byte[] data, int offset, int length)
    {
    }

    public void Encrypt(
      byte[] decrypted,
      int decryptOffset,
      byte[] encrypted,
      int encryptOffset,
      int count)
    {
      Buffer.BlockCopy((Array) decrypted, decryptOffset, (Array) encrypted, encryptOffset, count);
    }
  }
}
