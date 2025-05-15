// Decompiled with JetBrains decompiler
// Type: System.SerializableEntity
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public abstract class SerializableEntity
  {
    protected SerializableEntity()
    {
    }

    public SerializableEntity(IByteReaderBase reader)
    {
    }

    public void Serialize(IByteWriterBase writer, bool attachHeader)
    {
      Type type = this.GetType();
      object[] customAttributes = type.GetCustomAttributes(typeof (SerializationExclusionAttribute), false);
      if (customAttributes != null && customAttributes.Length != 0)
        return;
      if (attachHeader)
        writer.Write(Deserializer.GetTypeHeader(type));
      this.Serialize(writer);
    }

    protected abstract void Serialize(IByteWriterBase writer);
  }
}
