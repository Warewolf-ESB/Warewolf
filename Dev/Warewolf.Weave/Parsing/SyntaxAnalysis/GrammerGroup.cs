// Decompiled with JetBrains decompiler
// Type: System.Parsing.SyntaxAnalysis.GrammerGroup
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.SyntaxAnalysis
{
  public sealed class GrammerGroup
  {
    public static readonly GrammerGroup BooleanLiteralGrammers = new GrammerGroup("Boolean Literal");
    public static readonly GrammerGroup CharLiteralGrammers = new GrammerGroup("Character Literal");
    public static readonly GrammerGroup StringLiteralGrammers = new GrammerGroup("String Literal");
    public static readonly GrammerGroup NumericLiteralGrammers = new GrammerGroup("Numeric Literal");
    public static readonly GrammerGroup NullLiteralGrammers = new GrammerGroup("Null Literal");
    private string _displayName;

    public string DisplayName => this._displayName;

    public GrammerGroup(string displayName) => this._displayName = !string.IsNullOrEmpty(displayName) ? displayName : throw new ArgumentNullException(nameof (displayName));

    public override string ToString() => "GrammerGroup [" + this._displayName + "]";
  }
}
