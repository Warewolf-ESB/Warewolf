// Decompiled with JetBrains decompiler
// Type: System.Emission.AbstractCodeBuilder
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
  internal abstract class AbstractCodeBuilder
  {
    private ILGenerator _generator;
    private List<Reference> _ilMarkers;
    private List<Statement> _statements;
    private bool _isEmpty;

    internal bool IsEmpty => this._isEmpty;

    public ILGenerator Generator => this._generator;

    protected AbstractCodeBuilder(ILGenerator generator)
    {
      this._generator = generator;
      this._statements = new List<Statement>();
      this._ilMarkers = new List<Reference>();
      this._isEmpty = true;
    }

    public void SetNonEmpty() => this._isEmpty = false;

    public AbstractCodeBuilder AddExpression(Expression expression) => this.AddStatement((Statement) new ExpressionStatement(expression));

    public AbstractCodeBuilder AddStatement(Statement stmt)
    {
      this.SetNonEmpty();
      this._statements.Add(stmt);
      return this;
    }

    public LocalReference DeclareLocal(Type type)
    {
      LocalReference localReference = new LocalReference(type);
      this._ilMarkers.Add((Reference) localReference);
      return localReference;
    }

    internal void Generate(IEmissionMemberEmitter member, ILGenerator il)
    {
      foreach (Reference ilMarker in this._ilMarkers)
        ilMarker.Generate(il);
      foreach (Statement statement in this._statements)
        statement.Emit(member, il);
    }
  }
}
