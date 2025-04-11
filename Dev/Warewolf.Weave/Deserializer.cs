// Decompiled with JetBrains decompiler
// Type: System.Deserializer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System
{
  public static class Deserializer
  {
    private static Type _serializableEntityType = typeof (SerializableEntity);
    private static List<Type> _typeStore = new List<Type>();
    private static string[] _typeDefinitions = new string[0];
    private static uint _tdCount = 0;

    internal static Exception PrepareLoad(string path)
    {
      Exception exception = (Exception) null;
      try
      {
        if (path != null)
        {
          using (BinaryReader binaryReader = new BinaryReader((Stream) File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)))
          {
            Deserializer._tdCount = binaryReader.ReadUInt32();
            Deserializer._typeDefinitions = new string[(int) Deserializer._tdCount];
            Deserializer._typeStore = new List<Type>((int) Deserializer._tdCount);
            for (uint index = 0; index < Deserializer._tdCount; ++index)
            {
              Deserializer._typeDefinitions[(int) index] = binaryReader.ReadString();
              Deserializer._typeStore.Add(Type.GetType(Deserializer._typeDefinitions[(int) index]));
            }
          }
        }
      }
      catch (Exception ex)
      {
        Deserializer._tdCount = 0U;
        Deserializer._typeDefinitions = new string[0];
        Deserializer._typeStore = new List<Type>();
        exception = ex;
      }
      return exception;
    }

    internal static void ConfirmLoad(Assembly[] definedAssemblies)
    {
      List<Type> typeList = new List<Type>();
      for (int index = 0; index < definedAssemblies.Length; ++index)
        typeList.AddRange((IEnumerable<Type>) definedAssemblies[index].GetTypes());
      Type[] array = typeList.ToArray();
      List<string> stringList = new List<string>();
      for (int index = 0; index < array.Length; ++index)
      {
        if (array[index].IsSubclassOf(Deserializer._serializableEntityType))
          stringList.Add(array[index].AssemblyQualifiedName);
      }
      Deserializer._tdCount = (uint) stringList.Count;
      Deserializer._typeDefinitions = stringList.ToArray();
      Deserializer._typeStore = new List<Type>((int) Deserializer._tdCount);
      for (uint index = 0; index < Deserializer._tdCount; ++index)
        Deserializer._typeStore.Add(Type.GetType(Deserializer._typeDefinitions[(int) index]));
    }

    internal static Exception Save(string path)
    {
      Exception exception = (Exception) null;
      try
      {
        using (BinaryWriter binaryWriter = new BinaryWriter((Stream) File.Create(path)))
        {
          binaryWriter.Write(Deserializer._tdCount);
          for (uint index = 0; index < Deserializer._tdCount; ++index)
            binaryWriter.Write(Deserializer._typeDefinitions[(int) index]);
        }
      }
      catch (Exception ex)
      {
        exception = ex;
      }
      return exception;
    }

    public static uint GetTypeHeader(Type headerFor)
    {
      int num = Deserializer._typeStore.IndexOf(headerFor);
      return num == -1 ? uint.MaxValue : (uint) num;
    }

    public static Type GetTypeFromHeader(uint header) => Deserializer._typeStore[(int) header];

    public static T ByHeader<T>(IByteReaderBase reader) where T : class => (T) Activator.CreateInstance(Deserializer.GetTypeFromHeader(reader.ReadUInt32()), (object) reader);
  }
}
