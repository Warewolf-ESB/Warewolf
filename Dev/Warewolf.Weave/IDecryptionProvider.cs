// Decompiled with JetBrains decompiler
// Type: System.IDecryptionProvider
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public interface IDecryptionProvider
  {
    int NextKSRN();

    byte Decrypt(byte encrypted);

    void Decrypt(byte[] data, int offset, int length);

    void Decrypt(
      byte[] encrypted,
      int encryptOffset,
      byte[] decrypted,
      int decryptOffset,
      int count);
  }
}
