// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.SynchronizedQueue`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Threading;

namespace System.Collections.Generic
{
  public sealed class SynchronizedQueue<T> : IEnumerable<T>, IEnumerable
  {
    private SynchronizedQueue<T>.SingleLinkNode _head;
    private SynchronizedQueue<T>.SingleLinkNode _tail;
    private int _count;

    internal int UnsafeCount => this._count;

    public int Count => Thread.VolatileRead(ref this._count);

    public SynchronizedQueue()
    {
      this._head = new SynchronizedQueue<T>.SingleLinkNode();
      this._tail = this._head;
    }

    public SynchronizedQueue(IEnumerable<T> items)
      : this()
    {
      foreach (T obj in items)
        this.Enqueue(obj);
    }

    public void Enqueue(SynchronizedQueue<T> queue)
    {
      if (queue._count == 0)
        return;
      SynchronizedQueue<T>.SingleLinkNode comparand = (SynchronizedQueue<T>.SingleLinkNode) null;
      SynchronizedQueue<T>.SingleLinkNode singleLinkNode = queue._head;
      while ((object) singleLinkNode.Item == null)
        singleLinkNode = singleLinkNode.Next;
      bool flag = false;
      while (!flag)
      {
        comparand = this._tail;
        SynchronizedQueue<T>.SingleLinkNode next = comparand.Next;
        if (this._tail == comparand)
        {
          if (next == null)
            flag = Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail.Next, singleLinkNode, (SynchronizedQueue<T>.SingleLinkNode) null) == null;
          else
            Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail, next, comparand);
        }
      }
      Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail, singleLinkNode, comparand);
      Interlocked.Add(ref this._count, queue._count);
      queue._count = 0;
      queue._head = new SynchronizedQueue<T>.SingleLinkNode();
      queue._tail = queue._head;
    }

    public void Enqueue(T item)
    {
      SynchronizedQueue<T>.SingleLinkNode comparand = (SynchronizedQueue<T>.SingleLinkNode) null;
      SynchronizedQueue<T>.SingleLinkNode singleLinkNode = new SynchronizedQueue<T>.SingleLinkNode()
      {
        Item = item
      };
      bool flag = false;
      while (!flag)
      {
        comparand = this._tail;
        SynchronizedQueue<T>.SingleLinkNode next = comparand.Next;
        if (this._tail == comparand)
        {
          if (next == null)
            flag = Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail.Next, singleLinkNode, (SynchronizedQueue<T>.SingleLinkNode) null) == null;
          else
            Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail, next, comparand);
        }
      }
      Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail, singleLinkNode, comparand);
      Interlocked.Increment(ref this._count);
    }

    public T Dequeue()
    {
      T obj;
      if (!this.TryDequeue(out obj))
        throw new InvalidOperationException("the queue is empty");
      return obj;
    }

    public T TryDequeue()
    {
      T obj;
      this.TryDequeue(out obj);
      return obj;
    }

    public T TryDequeueUnsafe()
    {
      T obj;
      this.TryDequeueUnsafe(out obj);
      return obj;
    }

    public bool TryDequeue(out T item)
    {
      item = default (T);
      bool flag = false;
      while (!flag)
      {
        SynchronizedQueue<T>.SingleLinkNode head = this._head;
        SynchronizedQueue<T>.SingleLinkNode tail = this._tail;
        SynchronizedQueue<T>.SingleLinkNode next = head.Next;
        if (head == this._head)
        {
          if (head == tail)
          {
            if (next == null)
              return false;
            Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._tail, next, tail);
          }
          else
          {
            item = next.Item;
            flag = Interlocked.CompareExchange<SynchronizedQueue<T>.SingleLinkNode>(ref this._head, next, head) == head;
          }
        }
      }
      Interlocked.Decrement(ref this._count);
      return true;
    }

    public bool TryDequeueUnsafe(out T item)
    {
      item = default (T);
      bool flag = false;
      while (!flag)
      {
        SynchronizedQueue<T>.SingleLinkNode head = this._head;
        SynchronizedQueue<T>.SingleLinkNode tail = this._tail;
        SynchronizedQueue<T>.SingleLinkNode next = head.Next;
        if (head == this._head)
        {
          if (head == tail)
          {
            if (next == null)
              return false;
            if (this._tail == tail)
              this._tail = next;
          }
          else
          {
            item = next.Item;
            if (flag = this._head == head)
              this._head = next;
          }
        }
      }
      --this._count;
      return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
      SynchronizedQueue<T>.SingleLinkNode currentNode = this._head;
      while ((object) currentNode.Item != null)
      {
        yield return currentNode.Item;
        if ((currentNode = currentNode.Next) == null)
          break;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public void Clear()
    {
      SynchronizedQueue<T>.SingleLinkNode singleLinkNode1 = this._head;
      while (singleLinkNode1 != null)
      {
        SynchronizedQueue<T>.SingleLinkNode singleLinkNode2 = singleLinkNode1;
        singleLinkNode1 = singleLinkNode1.Next;
        singleLinkNode2.Item = default (T);
        singleLinkNode2.Next = (SynchronizedQueue<T>.SingleLinkNode) null;
      }
      this._head = new SynchronizedQueue<T>.SingleLinkNode();
      this._tail = this._head;
      this._count = 0;
    }

    private class SingleLinkNode
    {
      public SynchronizedQueue<T>.SingleLinkNode Next;
      public T Item;
    }
  }
}
