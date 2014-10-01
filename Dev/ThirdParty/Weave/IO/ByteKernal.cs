
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public sealed class ByteKernal
    {
        public byte[] Buffer;
        public int Length;
        public int Capacity;
        public int Position;

        public void SetCapacity(int value)
        {
            if (value == Capacity) return;

            if (value > 0)
            {
                byte[] dst = new byte[value];
                if (Length > 0) System.Buffer.BlockCopy(Buffer, 0, dst, 0, Length);
                Buffer = dst;
            }
            else Buffer = null;

            Capacity = value;
        }

        public void SetLength(int value)
        {
            int end = value;
            if (!EnsureCapacity(end) && end > Length) Array.Clear(Buffer, Length, end - Length);
            Length = end;
            if (Position > end) Position = end;
        }

        public bool EnsureRead(int amount)
        {
            return Position + amount <= Length;
        }

        public void EnsureWrite(int end)
        {
            bool clear = Position > Length;
            if (end > Capacity && EnsureCapacity(end)) clear = false;
            if (clear) Array.Clear(Buffer, Length, end - Length);
            Length = end;
        }

        public bool EnsureCapacity(int value)
        {
            if (value <= Capacity) return false;
            if (value < 32) value = 32;
            if (value < Capacity * 2) value = Capacity * 2;
            SetCapacity(value);
            return true;
        }
    }
}
