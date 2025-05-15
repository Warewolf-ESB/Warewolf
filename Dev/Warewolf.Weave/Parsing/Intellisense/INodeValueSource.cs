// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.INodeValueSource
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Intellisense
{
  public interface INodeValueSource
  {
    string GetEvaluatedValue(Node node);

    string GetRepresentationForEvaluation(Node node);
  }
}
