// Decompiled with JetBrains decompiler
// Type: System.IEncryptionProvider
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public interface IEncryptionProvider
  {
    int NextKSRN();

    byte Encrypt(byte decrypted);

    void Encrypt(byte[] data, int offset, int length);

    void Encrypt(
      byte[] decrypted,
      int decryptOffset,
      byte[] encrypted,
      int encryptOffset,
      int count);
  }
}
