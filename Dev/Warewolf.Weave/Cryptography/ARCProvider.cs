// Decompiled with JetBrains decompiler
// Type: System.Cryptography.ARCProvider
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Security.Cryptography;

namespace System.Cryptography
{
  public sealed class ARCProvider : ICryptProvider, IDecryptionProvider, IEncryptionProvider
  {
    private Random _ksrn;
    private int _ksrnSeed;
    private byte[] _sessionKey;
    private ARCProvider.ARC4 _inboundRC4;
    private ARCProvider.ARC4 _outboundRC4;

    public byte[] Key => this._sessionKey;

    private static byte[] ExecuteHMAC(byte[] seed, byte[] key) => new HMACSHA1(seed).ComputeHash(key);

    public ARCProvider(int ksrnSeed, byte[] sessionKey, byte[] inboundSeed, byte[] outboundSeed)
    {
      this._ksrn = new Random(this._ksrnSeed = ksrnSeed);
      this._sessionKey = sessionKey;
      this._inboundRC4 = new ARCProvider.ARC4(ARCProvider.ExecuteHMAC(inboundSeed, this._sessionKey), 1024);
      this._outboundRC4 = new ARCProvider.ARC4(ARCProvider.ExecuteHMAC(outboundSeed, this._sessionKey), 1024);
    }

    public void Advance(bool outbound, int count) => (outbound ? this._outboundRC4 : this._inboundRC4).Advance(count);

    public void Retreat(bool outbound, int count) => (outbound ? this._outboundRC4 : this._inboundRC4).Retreat(count);

    public int NextKSRN() => this._ksrn.Next(256);

    public byte Encrypt(byte decrypted) => (byte) ((uint) decrypted ^ (uint) this._outboundRC4.Advance());

    public byte[] Encrypt(byte[] decrypted)
    {
      int length = decrypted.Length;
      byte[] encrypted = new byte[length];
      this.Encrypt(decrypted, 0, encrypted, 0, length);
      return encrypted;
    }

    public void Encrypt(byte[] data, int offset, int length) => this.Encrypt(data, offset, data, offset, length);

    public void Encrypt(byte[] decrypted, byte[] encrypted) => this.Encrypt(decrypted, 0, encrypted, 0, decrypted.Length);

    public void Encrypt(
      byte[] decrypted,
      int decryptOffset,
      byte[] encrypted,
      int encryptOffset,
      int count)
    {
      for (int index = 0; index < count; ++index)
        encrypted[index + encryptOffset] = (byte) ((uint) decrypted[index + decryptOffset] ^ (uint) this._outboundRC4.Advance());
    }

    public byte Decrypt(byte encrypted) => (byte) ((uint) encrypted ^ (uint) this._inboundRC4.Advance());

    public byte[] Decrypt(byte[] encrypted)
    {
      int length = encrypted.Length;
      byte[] decrypted = new byte[length];
      this.Decrypt(encrypted, 0, decrypted, 0, length);
      return decrypted;
    }

    public void Decrypt(byte[] data, int offset, int length) => this.Decrypt(data, offset, data, offset, length);

    public void Decrypt(byte[] encrypted, byte[] decrypted) => this.Decrypt(encrypted, 0, decrypted, 0, encrypted.Length);

    public void Decrypt(
      byte[] encrypted,
      int encryptOffset,
      byte[] decrypted,
      int decryptOffset,
      int count)
    {
      for (int index = 0; index < count; ++index)
        decrypted[index + decryptOffset] = (byte) ((uint) encrypted[index + encryptOffset] ^ (uint) this._inboundRC4.Advance());
    }

    private sealed class ARC4
    {
      private int _prgaIndex;
      private byte[] _prgaBuffer;
      private byte[] _s;
      private byte _i;
      private byte _j;

      public ARC4(byte[] key, int disgard)
      {
        int length = key.Length;
        this._s = new byte[256];
        for (int index = 0; index < this._s.Length; ++index)
          this._s[index] = (byte) index;
        int index1 = 0;
        int index2 = 0;
        for (; index1 < this._s.Length; ++index1)
        {
          index2 = (int) (byte) ((index2 + (int) key[index1 % length] + (int) this._s[index1]) % 256);
          byte num = this._s[index1];
          this._s[index1] = this._s[index2];
          this._s[index2] = num;
        }
        this._i = this._j = (byte) 0;
        for (int index3 = 0; index3 < disgard; ++index3)
        {
          int prga = (int) this.GeneratePRGA();
        }
        this._prgaBuffer = new byte[1024];
        for (int index4 = 512; index4 < this._prgaBuffer.Length; ++index4)
          this._prgaBuffer[index4] = this.GeneratePRGA();
        this._prgaIndex = 512;
      }

      public byte Advance()
      {
        if (this._prgaIndex == 1024)
        {
          Buffer.BlockCopy((Array) this._prgaBuffer, 512, (Array) this._prgaBuffer, 0, 512);
          for (int index = 512; index < this._prgaBuffer.Length; ++index)
            this._prgaBuffer[index] = this.GeneratePRGA();
          this._prgaIndex = 512;
        }
        return this._prgaBuffer[this._prgaIndex++];
      }

      public void Advance(int count) => this._prgaIndex += count;

      public void Retreat(int count) => this._prgaIndex -= count;

      private byte GeneratePRGA()
      {
        this._i = (byte) (((int) this._i + 1) % 256);
        this._j = (byte) (((int) this._j + (int) this._s[(int) this._i]) % 256);
        byte num = this._s[(int) this._i];
        this._s[(int) this._i] = this._s[(int) this._j];
        this._s[(int) this._j] = num;
        return this._s[((int) this._s[(int) this._i] + (int) this._s[(int) this._j]) % 256];
      }
    }
  }
}
