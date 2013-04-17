using System;

namespace Dev2.Data.Storage
{
    public static class IndexMemoryRanageAllocator
    {
        private const int TotalSlots = 256;
        private static readonly bool[] OpenSlots = new bool[TotalSlots];

        private const long Offset = 0; // 256 MBth/1024/1024
        private const long StandardLength = 2097152; // 2 MB ;)

        private static readonly object IdxLock = new object();

        static IndexMemoryRanageAllocator()
        {
            // populate the total number of index files in the system ;)
            for (int i = 0; i < TotalSlots; i++)
            {
                OpenSlots[i] = true;
            }    
        }

        /// <summary>
        /// Generates the view ranage.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>The rented slot of the index </returns>
        public static int GenerateViewRanage(out long offset, out long length)
        {
            int rentedSlot = FetchRentableIndex();

            offset = Offset+ (StandardLength*rentedSlot);
            length = StandardLength;

            return rentedSlot;
        }

        /// <summary>
        /// Returns the index slot.
        /// </summary>
        /// <param name="idx">The idx.</param>
        public static void ReturnIndexSlot(int idx)
        {

            if (idx < TotalSlots && idx > 0)
            {
                lock (IdxLock)
                {
                    OpenSlots[idx] = true;
                }
            }
        }


        /// <summary>
        /// Fetches the index of the rentable.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.OutOfMemoryException">Fatal Error : All index slots are in use</exception>
        private static int FetchRentableIndex()
        {
            int pos = 0;

            lock (IdxLock)
            {

                while (pos < TotalSlots)
                {
                    if (OpenSlots[pos])
                    {
                        OpenSlots[pos] = false;
                        return pos;
                    }
                    pos++;
                }
            }

            return -1;
        }
    }
}
