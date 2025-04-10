// Decompiled with JetBrains decompiler
// Type: System.Emission.AttributesToAvoidReplicating
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Emission
{
  internal static class AttributesToAvoidReplicating
  {
    private static readonly IList<Type> attributes = (IList<Type>) new List<Type>();

    static AttributesToAvoidReplicating()
    {
      AttributesToAvoidReplicating.Add<ComImportAttribute>();
      AttributesToAvoidReplicating.Add<SecurityPermissionAttribute>();
      AttributesToAvoidReplicating.Add<TypeIdentifierAttribute>();
    }

    public static void Add(Type attribute)
    {
      if (AttributesToAvoidReplicating.attributes.Contains(attribute))
        return;
      AttributesToAvoidReplicating.attributes.Add(attribute);
    }

    public static void Add<T>() => AttributesToAvoidReplicating.Add(typeof (T));

    public static bool Contains(Type type) => AttributesToAvoidReplicating.attributes.Contains(type);
  }
}
