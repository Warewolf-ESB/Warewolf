// Decompiled with JetBrains decompiler
// Type: System.Threading.AtomicComposition
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Threading
{
  public class AtomicComposition : IDisposable
  {
    private readonly AtomicComposition _outerComposition;
    private KeyValuePair<object, object>[] _values;
    private List<Action> _rollbackActions;
    private List<Action> _completeActions;
    private int _totalValues;
    private bool _containsInnerComposition;
    private bool _disposed;
    private bool _completed;

    public AtomicComposition()
      : this((AtomicComposition) null)
    {
    }

    public AtomicComposition(AtomicComposition outerComposition)
    {
      if (outerComposition == null)
        return;
      this._outerComposition = outerComposition;
      this._outerComposition.SetContainsInnerComposition(true);
    }

    public bool TryGetValue<T>(object key, out T value)
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
      if (this._completed)
        throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
      if (key == null)
        throw new ArgumentNullException(nameof (key));
      object obj;
      if (!this.TryGetValueInternal(key, false, out obj))
      {
        value = default (T);
        return false;
      }
      value = (T) obj;
      return true;
    }

    public bool TryGetValue<T>(object key, bool localCompositionOnly, out T value)
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
      if (this._completed)
        throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
      if (key == null)
        throw new ArgumentNullException(nameof (key));
      object obj;
      if (!this.TryGetValueInternal(key, localCompositionOnly, out obj))
      {
        value = default (T);
        return false;
      }
      value = (T) obj;
      return true;
    }

    private bool TryGetValueInternal(object key, bool localCompositionOnly, out object value)
    {
      for (int index = 0; index < this._totalValues; ++index)
      {
        if (this._values[index].Key == key)
        {
          value = this._values[index].Value;
          return true;
        }
      }
      if (!localCompositionOnly && this._outerComposition != null)
        return this._outerComposition.TryGetValueInternal(key, localCompositionOnly, out value);
      value = (object) null;
      return false;
    }

    public void SetValue(object key, object value)
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
      if (this._completed)
        throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
      if (this._containsInnerComposition)
        throw new InvalidOperationException("The atomicComposition contains another inner atomicComposition and cannot be changed until the that inner atomicComposition has been completed.");
      if (key == null)
        throw new ArgumentNullException(nameof (key));
      this.SetValueInternal(key, value);
    }

    private void SetValueInternal(object key, object value)
    {
      for (int index = 0; index < this._totalValues; ++index)
      {
        if (this._values[index].Key == key)
        {
          this._values[index] = new KeyValuePair<object, object>(key, value);
          return;
        }
      }
      if (this._values == null || this._totalValues == this._values.Length)
      {
        KeyValuePair<object, object>[] destinationArray = new KeyValuePair<object, object>[this._totalValues == 0 ? 5 : this._totalValues * 2];
        if (this._values != null)
          Array.Copy((Array) this._values, (Array) destinationArray, this._totalValues);
        this._values = destinationArray;
      }
      this._values[this._totalValues++] = new KeyValuePair<object, object>(key, value);
    }

    private void SetContainsInnerComposition(bool value) => this._containsInnerComposition = !value || !this._containsInnerComposition ? value : throw new InvalidOperationException("The atomicComposition already contains an inner atomicComposition and cannot contain more than one atomicComposition at a time.");

    public void AddCompleteAction(Action action)
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
      if (this._completed)
        throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
      if (this._containsInnerComposition)
        throw new InvalidOperationException("The atomicComposition contains another inner atomicComposition and cannot be changed until the that inner atomicComposition has been completed.");
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      if (this._completeActions == null)
        this._completeActions = new List<Action>();
      this._completeActions.Add(action);
    }

    public void AddRollbackAction(Action action)
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
      if (this._completed)
        throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
      if (this._containsInnerComposition)
        throw new InvalidOperationException("The atomicComposition contains another inner atomicComposition and cannot be changed until the that inner atomicComposition has been completed.");
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      if (this._rollbackActions == null)
        this._rollbackActions = new List<Action>();
      this._rollbackActions.Add(action);
    }

    public void Complete()
    {
      if (this._disposed)
        throw new ObjectDisposedException(this.GetType().ToString());
      if (this._completed)
        throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
      if (this._outerComposition == null)
      {
        if (this._completeActions != null)
        {
          foreach (Action completeAction in this._completeActions)
            completeAction();
          this._completeActions = (List<Action>) null;
        }
      }
      else
      {
        this._outerComposition.SetContainsInnerComposition(false);
        if (this._completeActions != null)
        {
          foreach (Action completeAction in this._completeActions)
            this._outerComposition.AddCompleteAction(completeAction);
        }
        if (this._rollbackActions != null)
        {
          foreach (Action rollbackAction in this._rollbackActions)
            this._outerComposition.AddRollbackAction(rollbackAction);
        }
        for (int index = 0; index < this._totalValues; ++index)
          this._outerComposition.SetValueInternal(this._values[index].Key, this._values[index].Value);
      }
      this._completed = true;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      this._disposed = !this._disposed ? true : throw new ObjectDisposedException(this.GetType().ToString());
      if (this._outerComposition != null)
        this._outerComposition.SetContainsInnerComposition(false);
      if (this._completed || this._rollbackActions == null)
        return;
      for (int index = this._rollbackActions.Count - 1; index >= 0; --index)
        this._rollbackActions[index]();
      this._rollbackActions = (List<Action>) null;
    }
  }
}
