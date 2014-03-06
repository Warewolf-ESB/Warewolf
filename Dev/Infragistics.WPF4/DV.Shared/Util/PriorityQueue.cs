using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Infragistics
{
    /// <summary>
    /// Represents a strongly typed sorted list of objects accessed by their priority. Provides
    /// methods to add and remove items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PriorityQueue<T>
    {
        /// <summary>
        /// Initializes a new instance of the PriorityQueue class. 
        /// </summary>
        public PriorityQueue()
        {
            Count = 0;
            Capacity = 15;
            heap = new HeapItem[Capacity];
        }

        /// <summary>
        /// Removes and returns the highest priority object from the priority queue.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException();
            }

            T result = heap[0].Item;
            --Count;

            TrickleDown(0, heap[Count]);

            return result;
        }
        /// <summary>
        /// Inserts an item into the current priorty queue with the specified priority.
        /// </summary>
        /// <param name="item">The object to push onto the priority queue.</param>
        /// <param name="priority">The object's priority.</param>
        public void Enqueue(T item, double priority)
        {
            if (Count == Capacity)
            {
                GrowHeap();
            }

            ++Count;

            BubbleUp(Count - 1, new HeapItem(item, priority));
        }

        /// <summary>
        /// Gets the number of items in the current priority queue.
        /// 
        /// </summary>
        public int Count { get; private set; }
        private int Capacity { get; set; }

        private struct HeapItem
        {
            public HeapItem(T item, double priority)
                : this()
            {
                Item = item;
                Priority = priority;
            }
            public T Item { get; private set; }
            public double Priority { get; private set; }
        }
        private HeapItem[] heap;

        private void BubbleUp(int index, HeapItem he)
        {
            int parent = Parent(index);

            while (index > 0 && heap[parent].Priority < he.Priority)
            {
                heap[index] = heap[parent];
                index = parent;
                parent = Parent(index);
            }

            heap[index] = he;
        }
        private void TrickleDown(int index, HeapItem he)
        {
            int child = LeftChild(index);

            while (child < Count)
            {
                if (child + 1 < Count && heap[child].Priority < heap[child + 1].Priority)
                {
                    ++child;
                }

                heap[index] = heap[child];
                index = child;
                child = LeftChild(index);
            }

            BubbleUp(index, he);
        }
        private void GrowHeap()
        {
            Capacity = (Capacity * 2) + 1;
            HeapItem[] newHeap = new HeapItem[Capacity];
            System.Array.Copy(heap, 0, newHeap, 0, Count);
            heap = newHeap;
        }

        private static int Parent(int index)
        {
            return (index - 1) / 2;
        }
        private static int LeftChild(int index)
        {
            return (index * 2) + 1;
        }
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved