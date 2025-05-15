// Decompiled with JetBrains decompiler
// Type: System.Cryptography.SecureRemotePassword
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System.Cryptography
{
  public sealed class SecureRemotePassword
  {
    public const int MinPassLength = 3;
    public const int MaxPassLength = 16;
    public const int KeyLength = 32;
    private static readonly BigInteger _modulus = new BigInteger("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89", 16);
    private static readonly BigInteger _generator = new BigInteger(7L);
    private static readonly HashAlgorithm _hash = (HashAlgorithm) new SHA1Managed();
    private static RandomNumberGenerator _randomGenerator = (RandomNumberGenerator) new RNGCryptoServiceProvider();
    private string _username;
    private bool _isServer;
    private BigInteger _credentialsHash;
    private BigInteger _credentials;
    private BigInteger _salt;
    private BigInteger _rawSessionKey;
    private BigInteger _publicEphemeralValueA;
    private BigInteger _secretEphemeralValueA = SecureRemotePassword.RandomNumber();
    private BigInteger _publicEphemeralValueB;
    private BigInteger _secretEphemeralValueB;
    private BigInteger _verifier;

    public string Username => this._username;

    public bool IsServer => this._isServer;

    public BigInteger ClientSessionKeyProof => SecureRemotePassword.Hash((HashDataBroker) (SecureRemotePassword.Hash((HashDataBroker) SecureRemotePassword._modulus) ^ SecureRemotePassword.Hash((HashDataBroker) SecureRemotePassword._generator)), (HashDataBroker) SecureRemotePassword.Hash((HashDataBroker) this._username), (HashDataBroker) this.Salt, (HashDataBroker) this.PublicEphemeralValueA, (HashDataBroker) this.PublicEphemeralValueB, (HashDataBroker) this.SessionKey);

    public BigInteger ServerSessionKeyProof => SecureRemotePassword.Hash((HashDataBroker) this.PublicEphemeralValueA, (HashDataBroker) this.ClientSessionKeyProof, (HashDataBroker) this.SessionKey);

    public BigInteger Modulus => SecureRemotePassword._modulus;

    public BigInteger Generator => SecureRemotePassword._generator;

    public BigInteger Multiplier => (BigInteger) 3;

    public BigInteger CredentialsHash
    {
      get
      {
        BigInteger credentialsHash = this._credentialsHash;
        if ((object) credentialsHash != null)
          return credentialsHash;
        return this._credentialsHash = SecureRemotePassword.Hash((HashDataBroker) this.Salt, (HashDataBroker) this._credentials);
      }
    }

    public BigInteger Credentials
    {
      get => this._credentials;
      set => this._credentials = value;
    }

    public BigInteger Salt
    {
      get
      {
        BigInteger salt = this._salt;
        return (object) salt != null ? salt : (this._salt = SecureRemotePassword.RandomNumber());
      }
      set => this._salt = value;
    }

    public BigInteger ScramblingParameter => SecureRemotePassword.Hash((HashDataBroker) this.PublicEphemeralValueA, (HashDataBroker) this.PublicEphemeralValueB);

    public BigInteger PublicEphemeralValueA
    {
      get => this._publicEphemeralValueA == (BigInteger) null && !this._isServer ? (this._publicEphemeralValueA = SecureRemotePassword._generator.ModPow(this._secretEphemeralValueA, SecureRemotePassword._modulus)) : this._publicEphemeralValueA;
      set => this.SetPublicEphemeralValueA(value);
    }

    public BigInteger PublicEphemeralValueB
    {
      get => this.GetPublicEphemeralValueB();
      set => this.SetPublicEphemeralValueB(value);
    }

    public BigInteger SessionKey => this.GetSessionKey();

    public BigInteger Verifier
    {
      get
      {
        BigInteger verifier = this._verifier;
        return (object) verifier != null ? verifier : (this._verifier = (this._verifier = SecureRemotePassword._generator.ModPow(this.CredentialsHash, SecureRemotePassword._modulus)) < 0 ? this._verifier + SecureRemotePassword._modulus : this._verifier);
      }
      set => this._verifier = value;
    }

    private static BigInteger RandomNumber(int size)
    {
      byte[] numArray = new byte[size];
      SecureRemotePassword._randomGenerator.GetBytes(numArray);
      if (numArray[0] == (byte) 0)
        numArray[0] = (byte) 1;
      return new BigInteger(numArray);
    }

    private static BigInteger RandomNumber() => SecureRemotePassword.RandomNumber(32);

    private static BigInteger Hash(params HashDataBroker[] brokers) => CryptUtility.HashToBigInteger(SecureRemotePassword._hash, brokers);

    public static byte[] GenerateCredentialsHash(string username, string password)
    {
      byte[] hash = SecureRemotePassword._hash.ComputeHash(Encoding.ASCII.GetBytes(username.ToUpper() + ":" + password));
      if (hash.Length <= 20)
        return hash;
      byte[] destinationArray = new byte[20];
      Array.Copy((Array) hash, (Array) destinationArray, 20);
      return destinationArray;
    }

    public static byte[] GenerateCredentialsHash(byte[] source)
    {
      byte[] hash = SecureRemotePassword._hash.ComputeHash(source);
      if (hash.Length <= 20)
        return hash;
      byte[] destinationArray = new byte[20];
      Array.Copy((Array) hash, (Array) destinationArray, 20);
      return destinationArray;
    }

    public SecureRemotePassword(string username, BigInteger credentials, bool isServer)
    {
      this._username = username.ToUpper();
      this._isServer = isServer;
      this._credentials = credentials;
    }

    public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt)
    {
      this._username = username.ToUpper();
      this._isServer = true;
      this._verifier = verifier;
      this._salt = salt;
    }

    private BigInteger GetPublicEphemeralValueB()
    {
      if (this._isServer && this._publicEphemeralValueB == (BigInteger) null)
      {
        this._secretEphemeralValueB = SecureRemotePassword.RandomNumber();
        this._publicEphemeralValueB = this.Multiplier * this.Verifier + SecureRemotePassword._generator.ModPow(this._secretEphemeralValueB, SecureRemotePassword._modulus);
        this._publicEphemeralValueB %= SecureRemotePassword._modulus;
        if (this._publicEphemeralValueB < 0)
          this._publicEphemeralValueB += SecureRemotePassword._modulus;
      }
      return this._publicEphemeralValueB;
    }

    private BigInteger GetSessionKey()
    {
      if (this._rawSessionKey == (BigInteger) null)
      {
        BigInteger bigInteger;
        if (this._isServer)
        {
          if (this._publicEphemeralValueA == (BigInteger) null)
            return (BigInteger) null;
          bigInteger = (this.Verifier.ModPow(this.ScramblingParameter, SecureRemotePassword._modulus) * this.PublicEphemeralValueA % SecureRemotePassword._modulus).ModPow(this._secretEphemeralValueB, SecureRemotePassword._modulus);
        }
        else
        {
          bigInteger = (this.PublicEphemeralValueB - this.Multiplier * SecureRemotePassword._generator.ModPow(this.CredentialsHash, SecureRemotePassword._modulus)).ModPow(this._secretEphemeralValueA + this.ScramblingParameter * this.CredentialsHash, SecureRemotePassword._modulus);
          if (bigInteger < 0)
            bigInteger += SecureRemotePassword._modulus;
        }
        this._rawSessionKey = bigInteger;
      }
      byte[] bytes1 = this._rawSessionKey.GetBytes(32);
      byte[] numArray = new byte[16];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = bytes1[2 * index];
      byte[] bytes2 = SecureRemotePassword.Hash((HashDataBroker) numArray).GetBytes(20);
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = bytes1[2 * index + 1];
      byte[] bytes3 = SecureRemotePassword.Hash((HashDataBroker) numArray).GetBytes(20);
      byte[] inData = new byte[40];
      for (int index = 0; index < inData.Length; ++index)
        inData[index] = index % 2 == 0 ? bytes2[index / 2] : bytes3[index / 2];
      return new BigInteger(inData);
    }

    private void SetPublicEphemeralValueA(BigInteger value)
    {
      this._publicEphemeralValueA = value;
      this._publicEphemeralValueA %= SecureRemotePassword._modulus;
      if (this._publicEphemeralValueA < 0)
        this._publicEphemeralValueA += SecureRemotePassword._modulus;
      if (this._publicEphemeralValueA == 0)
        throw new InvalidDataException("A cannot be 0 mod N!");
    }

    private void SetPublicEphemeralValueB(BigInteger value)
    {
      this._publicEphemeralValueB = value;
      this._publicEphemeralValueB %= SecureRemotePassword._modulus;
      if (this._publicEphemeralValueB < 0)
        this._publicEphemeralValueB += SecureRemotePassword._modulus;
      if (this._publicEphemeralValueB == 0)
        throw new InvalidDataException("B cannot be 0 mod N!");
    }

    public bool IsClientProofValid(BigInteger clientProof) => this.ClientSessionKeyProof == clientProof;

    public bool IsServerProofValid(BigInteger serverProof) => serverProof == this.ServerSessionKeyProof;
  }
}
