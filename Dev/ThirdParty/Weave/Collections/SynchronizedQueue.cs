
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Collections.Generic
{
    public sealed class SynchronizedQueue<T> : IEnumerable<T>
    {
        #region Instance Fields
        private SingleLinkNode _head;
        private SingleLinkNode _tail;
        private int _count;
        #endregion

        #region Internal Properties
        internal int UnsafeCount { get { return _count; } }
        #endregion

        #region Public Properties
        public int Count { get { return Thread.VolatileRead(ref _count); } }
        #endregion

        #region Constructors
        public SynchronizedQueue()
        {
            _head = new SingleLinkNode();
            _tail = _head;
        }

        public SynchronizedQueue(IEnumerable<T> items)
            : this()
        {
            foreach (T item in items) Enqueue(item);
        }
        #endregion

        #region Enqueue Handling
        public void Enqueue(SynchronizedQueue<T> queue)
        {
            if (queue._count == 0) return;
            SingleLinkNode oldTail = null;
            SingleLinkNode oldTailNext;
            
            SingleLinkNode newNode = queue._head;
            while (newNode.Item == null) newNode = newNode.Next;
            bool newNodeWasAdded = false;

            while (!newNodeWasAdded)
            {
                oldTail = _tail;
                oldTailNext = oldTail.Next;

                if (_tail == oldTail)
                    if (oldTailNext == null) newNodeWasAdded = Interlocked.CompareExchange<SingleLinkNode>(ref _tail.Next, newNode, null) == null;
                    else Interlocked.CompareExchange<SingleLinkNode>(ref _tail, oldTailNext, oldTail);
            }

            Interlocked.CompareExchange<SingleLinkNode>(ref _tail, newNode, oldTail);
            Interlocked.Add(ref _count, queue._count);
            queue._count = 0;
            queue._head = new SynchronizedQueue<T>.SingleLinkNode();
            queue._tail = queue._head;
        }

        /// <summary>
        /// Adds an object to the end of the queue.
        /// </summary>
        /// <param name="item">the object to add to the queue</param>
        public void Enqueue(T item)
        {
            SingleLinkNode oldTail = null;
            SingleLinkNode oldTailNext;

            SingleLinkNode newNode = new SingleLinkNode { Item = item };
            bool newNodeWasAdded = false;

            while (!newNodeWasAdded)
            {
                oldTail = _tail;
                oldTailNext = oldTail.Next;

                if (_tail == oldTail)
                    if (oldTailNext == null) newNodeWasAdded = Interlocked.CompareExchange<SingleLinkNode>(ref _tail.Next, newNode, null) == null;
                    else Interlocked.CompareExchange<SingleLinkNode>(ref _tail, oldTailNext, oldTail);
            }

            Interlocked.CompareExchange<SingleLinkNode>(ref _tail, newNode, oldTail);
            Interlocked.Increment(ref _count);
        }
        #endregion

        #region Dequeue Handling
        /// <summary>
        /// Removes and returns the object at the beginning of the queue.
        /// </summary>
        /// <returns>the object that is removed from the beginning of the queue</returns>
        public T Dequeue()
        {
            T result;
            if (!TryDequeue(out result)) throw new InvalidOperationException("the queue is empty");
            return result;
        }

        public T TryDequeue()
        {
            T item;
            TryDequeue(out item);
            return item;
        }

        public T TryDequeueUnsafe()
        {
            T item;
            TryDequeueUnsafe(out item);
            return item;
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the queue.
        /// </summary>
        /// <param name="item">
        /// when the method returns, contains the object removed from the beginning of the queue, 
        /// if the queue is not empty; otherwise it is the default value for the element type
        /// </param>
        /// <returns>
        /// true if an object from removed from the beginning of the queue; 
        /// false if the queue is empty
        /// </returns>
        public bool TryDequeue(out T item)
        {
            item = default(T);
            SingleLinkNode oldHead = null;
            bool haveAdvancedHead = false;

            while (!haveAdvancedHead)
            {
                oldHead = _head;
                SingleLinkNode oldTail = _tail;
                SingleLinkNode oldHeadNext = oldHead.Next;

                if (oldHead == _head)
                    if (oldHead == oldTail)
                    {
                        if (oldHeadNext == null) return false;
                        Interlocked.CompareExchange<SingleLinkNode>(ref _tail, oldHeadNext, oldTail);
                    }
                    else
                    {
                        item = oldHeadNext.Item;
                        haveAdvancedHead = Interlocked.CompareExchange<SingleLinkNode>(ref _head, oldHeadNext, oldHead) == oldHead;
                    }
            }

            Interlocked.Decrement(ref _count);
            return true;
        }

        public bool TryDequeueUnsafe(out T item)
        {
            item = default(T);
            SingleLinkNode oldHead = null;
            bool haveAdvancedHead = false;

            while (!haveAdvancedHead)
            {
                oldHead = _head;
                SingleLinkNode oldTail = _tail;
                SingleLinkNode oldHeadNext = oldHead.Next;

                if (oldHead == _head)
                    if (oldHead == oldTail)
                    {
                        if (oldHeadNext == null) return false;
                        if (_tail == oldTail) _tail = oldHeadNext;
                    }
                    else
                    {
                        item = oldHeadNext.Item;
                        if (haveAdvancedHead = (_head == oldHead)) _head = oldHeadNext;
                    }
            }

            --_count;
            return true;
        }
        #endregion

        #region Enumeration Handling
        /// <summary>
        /// Returns an enumerator that iterates through the queue.
        /// </summary>
        /// <returns>an enumerator for the queue</returns>
        public IEnumerator<T> GetEnumerator()
        {
            SingleLinkNode currentNode = _head;

            do
            {
                if (currentNode.Item == null)
                    yield break;
                else
                    yield return currentNode.Item;
            }
            while ((currentNode = currentNode.Next) != null);

            yield break;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the queue.
        /// </summary>
        /// <returns>an enumerator for the queue</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Clear Handling
        /// <summary>
        /// Clears the queue.
        /// </summary>
        /// <remarks>This method is not thread-safe.</remarks>
        public void Clear()
        {
            SingleLinkNode tempNode;
            SingleLinkNode currentNode = _head;

            while (currentNode != null)
            {
                tempNode = currentNode;
                currentNode = currentNode.Next;

                tempNode.Item = default(T);
                tempNode.Next = null;
            }

            _head = new SingleLinkNode();
            _tail = _head;
            _count = 0;
        }
        #endregion

        #region SingleLinkNode
        private class SingleLinkNode
        {
            public SingleLinkNode Next;
            public T Item;
        }
        #endregion
    }
}
