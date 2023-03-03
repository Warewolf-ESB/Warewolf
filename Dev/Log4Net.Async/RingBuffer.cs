using System;

namespace Log4Net.Async
{
    public sealed class RingBuffer<T> : IQueue<T>
    {
        private readonly object lockObject = new object();
        private readonly T[] buffer;
        private readonly int size;
        private int readIndex = 0;
        private int writeIndex = 0;
        private bool bufferFull = false;

        public int Size { get { return size; } }

        public event Action<object, EventArgs> BufferOverflow;

        public RingBuffer(int size)
        {
            this.size = size;
            buffer = new T[size];
        }

        public void Enqueue(T item)
        {
            var bufferWasFull = false;
            lock (lockObject)
            {
                buffer[writeIndex] = item;
                writeIndex = (++writeIndex) % size;
                if (bufferFull)
                {
                    bufferWasFull = true;
                    readIndex = writeIndex;
                }
                else if (writeIndex == readIndex)
                {
                    bufferFull = true;
                }
            }

            if (bufferWasFull)
            {
                if (BufferOverflow != null)
                {
                    BufferOverflow(this, EventArgs.Empty);
                }
            }
        }

        public bool TryDequeue(out T ret)
        {
            if (readIndex == writeIndex && !bufferFull)
            {
                ret = default(T);
                return false;
            }
            lock (lockObject)
            {
                if (readIndex == writeIndex && !bufferFull)
                {
                    ret = default(T);
                    return false;
                }

                ret = buffer[readIndex];
                buffer[readIndex] = default(T);
                readIndex = (++readIndex) % size;
                bufferFull = false;
                return true;
            }
        }
    }
}