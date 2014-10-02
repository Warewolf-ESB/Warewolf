
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace System.Cryptography
{
    public sealed class BigInteger
    {
        #region Constants
        private const int MaxLength = 70;
        #endregion

        #region Readonly Fields
        private static readonly int[] _primesBelow2000 = new int[] 
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53,
            59, 61, 67, 71, 73, 79, 83, 89, 97,
            101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157,
            163, 167, 173, 179, 181, 191, 193, 197, 199,
            211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271,
            277, 281, 283, 293,
            307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373,
            379, 383, 389, 397,
            401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463,
            467, 479, 487, 491, 499,
            503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587,
            593, 599,
            601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661,
            673, 677, 683, 691,
            701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773,
            787, 797,
            809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877,
            881, 883, 887,
            907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983,
            991, 997,
            1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061,
            1063, 1069, 1087, 1091, 1093, 1097,
            1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181,
            1187, 1193,
            1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277,
            1279, 1283, 1289, 1291, 1297,
            1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381,
            1399,
            1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459,
            1471, 1481, 1483, 1487, 1489, 1493, 1499,
            1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579,
            1583, 1597,
            1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663,
            1667, 1669, 1693, 1697, 1699,
            1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783,
            1787, 1789,
            1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877,
            1879, 1889,
            1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987,
            1993, 1997, 1999
        };
        #endregion

        #region Static Members
        //***********************************************************************
        // Fast calculation of modular reduction using Barrett's reduction.
        // Requires x < b^(2k), where b is the base.  In this case, base is
        // 2^32 (uint).
        //
        // Reference [4]
        //***********************************************************************

        private static BigInteger BarrettReduction(BigInteger x, BigInteger n, BigInteger constant)
        {
            int k = n._length,
                kPlusOne = k + 1,
                kMinusOne = k - 1;

            BigInteger q1 = new BigInteger();

            // q1 = x / b^(k-1)
            for (int i = kMinusOne, j = 0; i < x._length; i++, j++)
                q1._data[j] = x._data[i];
            q1._length = x._length - kMinusOne;
            if (q1._length <= 0)
                q1._length = 1;


            BigInteger q2 = q1 * constant;
            BigInteger q3 = new BigInteger();

            // q3 = q2 / b^(k+1)
            for (int i = kPlusOne, j = 0; i < q2._length; i++, j++)
                q3._data[j] = q2._data[i];
            q3._length = q2._length - kPlusOne;
            if (q3._length <= 0)
                q3._length = 1;


            // r1 = x mod b^(k+1)
            // i.e. keep the lowest (k+1) words
            BigInteger r1 = new BigInteger();
            int lengthToCopy = (x._length > kPlusOne) ? kPlusOne : x._length;
            for (int i = 0; i < lengthToCopy; i++)
                r1._data[i] = x._data[i];
            r1._length = lengthToCopy;


            // r2 = (q3 * n) mod b^(k+1)
            // partial multiplication of q3 and n

            BigInteger r2 = new BigInteger();
            for (int i = 0; i < q3._length; i++)
            {
                if (q3._data[i] == 0) continue;

                ulong mcarry = 0;
                int t = i;
                for (int j = 0; j < n._length && t < kPlusOne; j++, t++)
                {
                    // t = i + j
                    ulong val = ((ulong)q3._data[i] * (ulong)n._data[j]) +
                                r2._data[t] + mcarry;

                    r2._data[t] = (uint)(val & 0xFFFFFFFF);
                    mcarry = (val >> 32);
                }

                if (t < kPlusOne)
                    r2._data[t] = (uint)mcarry;
            }
            r2._length = kPlusOne;
            while (r2._length > 1 && r2._data[r2._length - 1] == 0)
                r2._length--;

            r1 -= r2;
            if ((r1._data[MaxLength - 1] & 0x80000000) != 0) // negative
            {
                BigInteger val = new BigInteger();
                val._data[kPlusOne] = 0x00000001;
                val._length = kPlusOne + 1;
                r1 += val;
            }

            while (r1 >= n)
                r1 -= n;

            return r1;
        }

        private static bool LucasStrongTestHelper(BigInteger thisVal)
        {
            // Do the test (selects D based on Selfridge)
            // Let D be the first element of the sequence
            // 5, -7, 9, -11, 13, ... for which J(D,n) = -1
            // Let P = 1, Q = (1-D) / 4

            long D = 5, sign = -1, dCount = 0;
            bool done = false;

            while (!done)
            {
                int Jresult = Jacobi((BigInteger)D, (BigInteger)thisVal);

                if (Jresult == -1)
                    done = true; // J(D, this) = 1
                else
                {
                    if (Jresult == 0 && thisVal > Math.Abs(D)) // divisor found
                        return false;

                    if (dCount == 20)
                    {
                        // check for square
                        BigInteger root = thisVal.Sqrt();
                        if (root * root == thisVal)
                            return false;
                    }

                    D = (Math.Abs(D) + 2) * sign;
                    sign = -sign;
                }
                dCount++;
            }

            long Q = (1 - D) >> 2;

            BigInteger p_add1 = thisVal + 1;
            int s = 0;

            for (int index = 0; index < p_add1._length; index++)
            {
                uint mask = 0x01;

                for (int i = 0; i < 32; i++)
                {
                    if ((p_add1._data[index] & mask) != 0)
                    {
                        index = p_add1._length; // to break the outer loop
                        break;
                    }
                    mask <<= 1;
                    s++;
                }
            }

            BigInteger t = p_add1 >> s;

            // for Barrett Reduction
            BigInteger constant = new BigInteger();

            int nLen = thisVal._length << 1;
            constant._data[nLen] = 0x00000001;
            constant._length = nLen + 1;

            constant = constant / thisVal;

            BigInteger[] lucas =
                LucasSequenceHelper((BigInteger)1, (BigInteger)Q, (BigInteger)t, (BigInteger)thisVal,
                                    (BigInteger)constant, 0);
            bool isPrime = false;

            if ((lucas[0]._length == 1 && lucas[0]._data[0] == 0) ||
                (lucas[1]._length == 1 && lucas[1]._data[0] == 0))
            {
                isPrime = true;
            }

            for (int i = 1; i < s; i++)
            {
                if (!isPrime)
                {
                    // doubling of index
                    lucas[1] = BarrettReduction(lucas[1] * lucas[1], thisVal, constant);
                    lucas[1] = (lucas[1] - (lucas[2] << 1)) % thisVal;

                    if ((lucas[1]._length == 1 && lucas[1]._data[0] == 0))
                        isPrime = true;
                }

                lucas[2] = BarrettReduction(lucas[2] * lucas[2], thisVal, constant); //Q^k
            }


            if (isPrime) // additional checks for composite numbers
            {
                // If n is prime and gcd(n, Q) == 1, then
                // Q^((n+1)/2) = Q * Q^((n-1)/2) is congruent to (Q * J(Q, n)) mod n

                BigInteger g = thisVal.GCD((BigInteger)Q);
                if (g._length == 1 && g._data[0] == 1) // gcd(this, Q) == 1
                {
                    if ((lucas[2]._data[MaxLength - 1] & 0x80000000) != 0)
                        lucas[2] += thisVal;

                    BigInteger temp = (BigInteger)((Q * Jacobi((BigInteger)Q, thisVal)) % thisVal);
                    if ((temp._data[MaxLength - 1] & 0x80000000) != 0)
                        temp += thisVal;

                    if (lucas[2] != temp)
                        isPrime = false;
                }
            }

            return isPrime;
        }

        //***********************************************************************
        // Computes the Jacobi Symbol for a and b.
        // Algorithm adapted from [3] and [4] with some optimizations
        //***********************************************************************

        private static int Jacobi(BigInteger a, BigInteger b)
        {
            // Jacobi defined only for odd integers
            if ((b._data[0] & 0x1) == 0)
                throw (new ArgumentException("Jacobi defined only for odd integers."));

            if (a >= b) a %= b;
            if (a._length == 1 && a._data[0] == 0) return 0; // a == 0
            if (a._length == 1 && a._data[0] == 1) return 1; // a == 1

            if (a < 0)
            {
                if ((((b - 1)._data[0]) & 0x2) == 0) 
                    return Jacobi(-a, b);
                else
                    return -Jacobi(-a, b);
            }

            int e = 0;
            for (int index = 0; index < a._length; index++)
            {
                uint mask = 0x01;

                for (int i = 0; i < 32; i++)
                {
                    if ((a._data[index] & mask) != 0)
                    {
                        index = a._length; // to break the outer loop
                        break;
                    }
                    mask <<= 1;
                    e++;
                }
            }

            BigInteger a1 = a >> e;

            int s = 1;
            if ((e & 0x1) != 0 && ((b._data[0] & 0x7) == 3 || (b._data[0] & 0x7) == 5))
                s = -1;

            if ((b._data[0] & 0x3) == 3 && (a1._data[0] & 0x3) == 3)
                s = -s;

            if (a1._length == 1 && a1._data[0] == 1)
                return s;
            else
                return (s * Jacobi(b % a1, a1));
        }

        private static void Reverse<T>(T[] buffer, int length)
        {
            for (int i = 0; i < length / 2; i++)
            {
                T temp = buffer[i];
                buffer[i] = buffer[length - i - 1];
                buffer[length - i - 1] = temp;
            }
        }

        private static void Reverse<T>(T[] buffer)
        {
            Reverse(buffer, buffer.Length);
        }

        //***********************************************************************
        // Returns the k_th number in the Lucas Sequence reduced modulo n.
        //
        // Uses index doubling to speed up the process.  For example, to calculate V(k),
        // we maintain two numbers in the sequence V(n) and V(n+1).
        //
        // To obtain V(2n), we use the identity
        //      V(2n) = (V(n) * V(n)) - (2 * Q^n)
        // To obtain V(2n+1), we first write it as
        //      V(2n+1) = V((n+1) + n)
        // and use the identity
        //      V(m+n) = V(m) * V(n) - Q * V(m-n)
        // Hence,
        //      V((n+1) + n) = V(n+1) * V(n) - Q^n * V((n+1) - n)
        //                   = V(n+1) * V(n) - Q^n * V(1)
        //                   = V(n+1) * V(n) - Q^n * P
        //
        // We use k in its binary expansion and perform index doubling for each
        // bit position.  For each bit position that is set, we perform an
        // index doubling followed by an index addition.  This means that for V(n),
        // we need to update it to V(2n+1).  For V(n+1), we need to update it to
        // V((2n+1)+1) = V(2*(n+1))
        //
        // This function returns
        // [0] = U(k)
        // [1] = V(k)
        // [2] = Q^n
        //
        // Where U(0) = 0 % n, U(1) = 1 % n
        //       V(0) = 2 % n, V(1) = P % n
        //***********************************************************************

        private static BigInteger[] LucasSequence(BigInteger P, BigInteger Q,
                                                 BigInteger k, BigInteger n)
        {
            if (k._length == 1 && k._data[0] == 0)
            {
                BigInteger[] result = new BigInteger[3];

                result[0] = (BigInteger)0;
                result[1] = 2 % n;
                result[2] = 1 % n;
                return result;
            }

            // calculate constant = b^(2k) / m
            // for Barrett Reduction
            BigInteger constant = new BigInteger();

            int nLen = n._length << 1;
            constant._data[nLen] = 0x00000001;
            constant._length = nLen + 1;

            constant = constant / n;

            // calculate values of s and t
            int s = 0;

            for (int index = 0; index < k._length; index++)
            {
                uint mask = 0x01;

                for (int i = 0; i < 32; i++)
                {
                    if ((k._data[index] & mask) != 0)
                    {
                        index = k._length; // to break the outer loop
                        break;
                    }
                    mask <<= 1;
                    s++;
                }
            }

            BigInteger t = k >> s;

            return LucasSequenceHelper(P, Q, t, n, constant, s);
        }


        //***********************************************************************
        // Performs the calculation of the kth term in the Lucas Sequence.
        // For details of the algorithm, see reference [9].
        //
        // k must be odd.  i.e LSB == 1
        //***********************************************************************

        private static BigInteger[] LucasSequenceHelper(BigInteger P, BigInteger Q,
                                                        BigInteger k, BigInteger n,
                                                        BigInteger constant, int s)
        {
            BigInteger[] result = new BigInteger[3];

            if ((k._data[0] & 0x00000001) == 0)
                throw (new ArgumentException("Argument k must be odd."));

            int numbits = k.BitCount();
            uint mask = (uint)0x1 << ((numbits & 0x1F) - 1);

            // v = v0, v1 = v1, u1 = u1, Q_k = Q^0

            BigInteger v = 2 % n,
                       Q_k = 1 % n,
                       v1 = P % n,
                       u1 = Q_k;
            bool flag = true;

            for (int i = k._length - 1; i >= 0; i--) // iterate on the binary expansion of k
            {

                while (mask != 0)
                {
                    if (i == 0 && mask == 0x00000001) // last bit
                        break;

                    if ((k._data[i] & mask) != 0) // bit is set
                    {
                        // index doubling with addition

                        u1 = (u1 * v1) % n;

                        v = ((v * v1) - (P * Q_k)) % n;
                        v1 = BarrettReduction(v1 * v1, n, constant);
                        v1 = (v1 - ((Q_k * Q) << 1)) % n;

                        if (flag)
                            flag = false;
                        else
                            Q_k = BarrettReduction(Q_k * Q_k, n, constant);

                        Q_k = (Q_k * Q) % n;
                    }
                    else
                    {
                        // index doubling
                        u1 = ((u1 * v) - Q_k) % n;

                        v1 = ((v * v1) - (P * Q_k)) % n;
                        v = BarrettReduction(v * v, n, constant);
                        v = (v - (Q_k << 1)) % n;

                        if (flag)
                        {
                            Q_k = Q % n;
                            flag = false;
                        }
                        else
                            Q_k = BarrettReduction(Q_k * Q_k, n, constant);
                    }

                    mask >>= 1;
                }
                mask = 0x80000000;
            }

            // at this point u1 = u(n+1) and v = v(n)
            // since the last bit always 1, we need to transform u1 to u(2n+1) and v to v(2n+1)

            u1 = ((u1 * v) - Q_k) % n;
            v = ((v * v1) - (P * Q_k)) % n;
            if (flag)
                flag = false;
            else
                Q_k = BarrettReduction(Q_k * Q_k, n, constant);

            Q_k = (Q_k * Q) % n;


            for (int i = 0; i < s; i++)
            {
                // index doubling
                u1 = (u1 * v) % n;
                v = ((v * v) - (Q_k << 1)) % n;

                if (flag)
                {
                    Q_k = Q % n;
                    flag = false;
                }
                else
                    Q_k = BarrettReduction(Q_k * Q_k, n, constant);
            }

            result[0] = u1;
            result[1] = v;
            result[2] = Q_k;

            return result;
        }
        #endregion

        #region Instance Fields
        private uint[] _data;
        private int _length;
        #endregion

        #region Constructors
        public BigInteger()
        {
            _data = new uint[MaxLength];
            _length = 1;
        }

        public BigInteger(long value)
        {
            _data = new uint[MaxLength];
            long tempVal = value;
            _length = 0;

            while (value != 0 && _length < MaxLength)
            {
                _data[_length] = (uint)(value & 0xFFFFFFFF);
                value >>= 32;
                _length++;
            }

            if (tempVal > 0) // overflow check for +ve value
            {
                if (value != 0 || (_data[MaxLength - 1] & 0x80000000) != 0)
                    throw (new ArithmeticException("Positive overflow in constructor."));
            }
            else if (tempVal < 0) // underflow check for -ve value
            {
                if (value != -1 || (_data[_length - 1] & 0x80000000) == 0)
                    throw (new ArithmeticException("Negative underflow in constructor."));
            }

            if (_length == 0)
                _length = 1;
        }

        public BigInteger(ulong value)
        {
            _data = new uint[MaxLength];
            _length = 0;

            while (value != 0 && _length < MaxLength)
            {
                _data[_length] = (uint)(value & 0xFFFFFFFF);
                value >>= 32;
                _length++;
            }

            if (value != 0 || (_data[MaxLength - 1] & 0x80000000) != 0)
                throw (new ArithmeticException("Positive overflow in constructor."));

            if (_length == 0)
                _length = 1;
        }

        public BigInteger(BigInteger bi)
        {
            SetValue(bi);
        }

        public void SetValue(BigInteger bi)
        {
            _data = new uint[MaxLength];

            _length = bi._length;

            for (int i = 0; i < _length; i++)
                _data[i] = bi._data[i];
        }


        //***********************************************************************
        // Constructor (Default value provided by a string of digits of the
        //              specified base)
        //
        // Example (base 10)
        // -----------------
        // To initialize "a" with the default value of 1234 in base 10
        //      BigInteger a = new BigInteger("1234", 10)
        //
        // To initialize "a" with the default value of -1234
        //      BigInteger a = new BigInteger("-1234", 10)
        //
        // Example (base 16)
        // -----------------
        // To initialize "a" with the default value of 0x1D4F in base 16
        //      BigInteger a = new BigInteger("1D4F", 16)
        //
        // To initialize "a" with the default value of -0x1D4F
        //      BigInteger a = new BigInteger("-1D4F", 16)
        //
        // Note that string values are specified in the <sign><magnitude>
        // format.
        //
        //***********************************************************************

        public BigInteger(string value, int radix)
        {
            BigInteger multiplier = new BigInteger(1);
            BigInteger result = new BigInteger();
            value = (value.ToUpper()).Trim();
            int limit = 0;

            if (value[0] == '-')
                limit = 1;

            for (int i = value.Length - 1; i >= limit; i--)
            {
                int posVal = value[i];

                if (posVal >= '0' && posVal <= '9')
                    posVal -= '0';
                else if (posVal >= 'A' && posVal <= 'Z')
                    posVal = (posVal - 'A') + 10;
                else
                    posVal = 9999999; // arbitrary large


                if (posVal >= radix)
                    throw (new ArithmeticException("Invalid string in constructor."));
                else
                {
                    if (value[0] == '-')
                        posVal = -posVal;

                    result = result + (multiplier * posVal);

                    if ((i - 1) >= limit)
                        multiplier = multiplier * radix;
                }
            }

            if (value[0] == '-') // negative values
            {
                if ((result._data[MaxLength - 1] & 0x80000000) == 0)
                    throw (new ArithmeticException("Negative underflow in constructor."));
            }
            else // positive values
            {
                if ((result._data[MaxLength - 1] & 0x80000000) != 0)
                    throw (new ArithmeticException("Positive overflow in constructor."));
            }

            _data = new uint[MaxLength];
            for (int i = 0; i < result._length; i++)
                _data[i] = result._data[i];

            _length = result._length;
        }


        //***********************************************************************
        // Constructor (Default value provided by an array of bytes)
        //
        // The lowest index of the input byte array (i.e [0]) should contain the
        // most significant byte of the number, and the highest index should
        // contain the least significant byte.
        //
        // E.g.
        // To initialize "a" with the default value of 0x1D4F in base 16
        //      byte[] temp = { 0x1D, 0x4F };
        //      BigInteger a = new BigInteger(temp)
        //
        // Note that this method of initialization does not allow the
        // sign to be specified.
        //
        //***********************************************************************

        public BigInteger(byte[] inData)
        {
            // copy the array, or people will be confused about why their array is suddenly reversed
            inData = (byte[])inData.Clone();

            Reverse(inData);
            _length = inData.Length >> 2;

            int leftOver = inData.Length & 0x3;
            if (leftOver != 0) // length not multiples of 4
                _length++;


            if (_length > MaxLength)
                throw (new ArithmeticException("Byte overflow in constructor."));

            _data = new uint[MaxLength];

            for (int i = inData.Length - 1, j = 0; i >= 3; i -= 4, j++)
            {
                _data[j] = (uint)((inData[i - 3] << 24) + (inData[i - 2] << 16) +
                                  (inData[i - 1] << 8) + inData[i]);
            }

            switch (leftOver)
            {
                case 1:
                    _data[_length - 1] = inData[0];
                    break;
                case 2:
                    _data[_length - 1] = (uint)((inData[0] << 8) + inData[1]);
                    break;
                case 3:
                    _data[_length - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);
                    break;
            }


            while (_length > 1 && _data[_length - 1] == 0)
                _length--;

        }


        //***********************************************************************
        // Constructor (Default value provided by an array of bytes of the
        // specified length.)
        //***********************************************************************

        public BigInteger(byte[] inData, int inLen)
        {
            _length = inLen >> 2;

            int leftOver = inLen & 0x3;
            if (leftOver != 0) // length not multiples of 4
                _length++;

            if (_length > MaxLength || inLen > inData.Length)
                throw (new ArithmeticException("Byte overflow in constructor."));


            _data = new uint[MaxLength];

            for (int i = inLen - 1, j = 0; i >= 3; i -= 4, j++)
            {
                _data[j] = (uint)((inData[i - 3] << 24) + (inData[i - 2] << 16) +
                                  (inData[i - 1] << 8) + inData[i]);
            }

            if (leftOver == 1)
                _data[_length - 1] = inData[0];
            else if (leftOver == 2)
                _data[_length - 1] = (uint)((inData[0] << 8) + inData[1]);
            else if (leftOver == 3)
                _data[_length - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);


            if (_length == 0)
                _length = 1;

            while (_length > 1 && _data[_length - 1] == 0)
                _length--;

        }


        //***********************************************************************
        // Constructor (Default value provided by an array of unsigned integers)
        //*********************************************************************

        public BigInteger(uint[] inData)
        {
            _length = inData.Length;

            if (_length > MaxLength)
                throw (new ArithmeticException("Byte overflow in constructor."));

            _data = new uint[MaxLength];

            for (int i = _length - 1, j = 0; i >= 0; i--, j++)
                _data[j] = inData[i];

            while (_length > 1 && _data[_length - 1] == 0)
                _length--;
        }

        public BigInteger(Random rand, int bitLength)
        {
            _data = new uint[MaxLength];
            _length = 1;

            GenerateRandomBits(bitLength, rand);
        }

        #endregion

        #region Overrides
        public string ToString(int radix)
        {
            if (radix < 2 || radix > 36)
                throw (new ArgumentException("Radix must be >= 2 and <= 36"));

            string charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";

            BigInteger a = this;

            bool negative = false;
            if ((a._data[MaxLength - 1] & 0x80000000) != 0)
            {
                negative = true;
                try
                {
                    a = -a;
                }
                catch (Exception)
                {
                }
            }

            BigInteger quotient = new BigInteger();
            BigInteger remainder = new BigInteger();
            BigInteger biRadix = new BigInteger(radix);

            if (a._length == 1 && a._data[0] == 0)
                result = "0";
            else
            {
                while (a._length > 1 || (a._length == 1 && a._data[0] != 0))
                {
                    singleByteDivide(a, biRadix, quotient, remainder);

                    if (remainder._data[0] < 10)
                        result = remainder._data[0] + result;
                    else
                        result = charSet[(int)remainder._data[0] - 10] + result;

                    a = quotient;
                }
                if (negative)
                    result = "-" + result;
            }

            return result;
        }

        public string ToHexString()
        {
            string result = _data[_length - 1].ToString("X");

            for (int i = _length - 2; i >= 0; i--)
            {
                result += _data[i].ToString("X8");
            }

            return result;
        }

        public override string ToString()
        {
            return "0x" + ToString(16);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        #endregion

        #region Get(...) Handling
        public byte[] GetBytes()
        {
            int numBits = BitCount();
            int numBytes = numBits >> 3;
            if ((numBits & 0x7) != 0)
                numBytes++;

            return GetBytes(numBytes);
        }

        public byte[] GetBytesBE()
        {
            int numBits = BitCount();
            int numBytes = numBits >> 3;
            if ((numBits & 0x7) != 0)
                numBytes++;

            return GetBytesBE(numBytes);
        }

        //***********************************************************************
        // Returns the value of the BigInteger as a byte array.  The lowest
        // index contains the MSB.
        //***********************************************************************

        public byte[] GetBytes(int numBytes)
        {
            byte[] result = new byte[numBytes];

            int numBits = BitCount();
            int realNumBytes = numBits >> 3;
            if ((numBits & 0x7) != 0)
                realNumBytes++;

            for (int i = 0; i < realNumBytes; i++)
            {
                for (int b = 0; b < 4; b++)
                {
                    if (i * 4 + b >= realNumBytes)
                        return result;
                    result[i * 4 + b] = (byte)(_data[i] >> (b * 8) & 0xff);
                }
            }

            return result;
        }

        public byte[] GetBytesBE(int numBytes)
        {
            byte[] result = GetBytes(numBytes);
            Reverse(result);
            return result;
        }
        #endregion

        #region Conversion Handling
        public static explicit operator BigInteger(long value)
        {
            return (new BigInteger(value));
        }

        public static explicit operator BigInteger(ulong value)
        {
            return (new BigInteger(value));
        }

        public static explicit operator BigInteger(int value)
        {
            return (new BigInteger(value));
        }

        public static explicit operator BigInteger(uint value)
        {
            return (new BigInteger((ulong)value));
        }

        public static implicit operator BigInteger(byte[] value)
        {
            return (new BigInteger(value));
        }
        #endregion

        #region Operator Overloads
        public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
        {
            BigInteger result = new BigInteger();

            result._length = (bi1._length > bi2._length) ? bi1._length : bi2._length;

            long carry = 0;
            for (int i = 0; i < result._length; i++)
            {
                long sum = (long)bi1._data[i] + (long)bi2._data[i] + carry;
                carry = sum >> 32;
                result._data[i] = (uint)(sum & 0xFFFFFFFF);
            }

            if (carry != 0 && result._length < MaxLength)
            {
                result._data[result._length] = (uint)(carry);
                result._length++;
            }

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;


            // overflow check
            int lastPos = MaxLength - 1;
            if ((bi1._data[lastPos] & 0x80000000) == (bi2._data[lastPos] & 0x80000000) &&
                (result._data[lastPos] & 0x80000000) != (bi1._data[lastPos] & 0x80000000))
            {
                throw (new ArithmeticException());
            }

            return result;
        }

        public static BigInteger operator +(BigInteger bi1, long bi2)
        {
            return bi1 + (BigInteger)bi2;
        }

        public static BigInteger operator +(BigInteger bi1, ulong bi2)
        {
            return bi1 + (BigInteger)bi2;
        }

        public static BigInteger operator +(BigInteger bi1, int bi2)
        {
            return bi1 + (BigInteger)bi2;
        }

        public static BigInteger operator +(BigInteger bi1, uint bi2)
        {
            return bi1 + (BigInteger)bi2;
        }

        public static BigInteger operator ++(BigInteger bi1)
        {
            BigInteger result = new BigInteger(bi1);

            long carry = 1;
            int index = 0;

            while (carry != 0 && index < MaxLength)
            {
                long val;
                val = result._data[index];
                val++;

                result._data[index] = (uint)(val & 0xFFFFFFFF);
                carry = val >> 32;

                index++;
            }

            if (index > result._length)
                result._length = index;
            else
            {
                while (result._length > 1 && result._data[result._length - 1] == 0)
                    result._length--;
            }

            // overflow check
            int lastPos = MaxLength - 1;

            // overflow if initial value was +ve but ++ caused a sign
            // change to negative.

            if ((bi1._data[lastPos] & 0x80000000) == 0 &&
                (result._data[lastPos] & 0x80000000) != (bi1._data[lastPos] & 0x80000000))
            {
                throw (new ArithmeticException("Overflow in ++."));
            }
            return result;
        }

        public static BigInteger operator -(BigInteger bi1)
        {
            // handle neg of zero separately since it'll cause an overflow
            // if we proceed.

            if (bi1._length == 1 && bi1._data[0] == 0)
                return (new BigInteger());

            BigInteger result = new BigInteger(bi1);

            // 1's complement
            for (int i = 0; i < MaxLength; i++)
                result._data[i] = ~(bi1._data[i]);

            // add one to result of 1's complement
            long carry = 1;
            int index = 0;

            while (carry != 0 && index < MaxLength)
            {
                long val;
                val = result._data[index];
                val++;

                result._data[index] = (uint)(val & 0xFFFFFFFF);
                carry = val >> 32;

                index++;
            }

            if ((bi1._data[MaxLength - 1] & 0x80000000) == (result._data[MaxLength - 1] & 0x80000000))
                throw (new ArithmeticException("Overflow in negation.\n"));

            result._length = MaxLength;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;
            return result;
        }

        public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
        {
            BigInteger result = new BigInteger();

            result._length = (bi1._length > bi2._length) ? bi1._length : bi2._length;

            long carryIn = 0;
            for (int i = 0; i < result._length; i++)
            {
                long diff;

                diff = (long)bi1._data[i] - (long)bi2._data[i] - carryIn;
                result._data[i] = (uint)(diff & 0xFFFFFFFF);

                if (diff < 0)
                    carryIn = 1;
                else
                    carryIn = 0;
            }

            // roll over to negative
            if (carryIn != 0)
            {
                for (int i = result._length; i < MaxLength; i++)
                    result._data[i] = 0xFFFFFFFF;
                result._length = MaxLength;
            }

            // fixed in v1.03 to give correct datalength for a - (-b)
            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            // overflow check

            int lastPos = MaxLength - 1;
            if ((bi1._data[lastPos] & 0x80000000) != (bi2._data[lastPos] & 0x80000000) &&
                (result._data[lastPos] & 0x80000000) != (bi1._data[lastPos] & 0x80000000))
            {
                throw (new ArithmeticException());
            }

            return result;
        }

        public static BigInteger operator -(BigInteger bi1, long bi2)
        {
            return bi1 - (BigInteger)bi2;
        }

        public static BigInteger operator -(BigInteger bi1, ulong bi2)
        {
            return bi1 - (BigInteger)bi2;
        }

        public static BigInteger operator -(BigInteger bi1, int bi2)
        {
            return bi1 - (BigInteger)bi2;
        }

        public static BigInteger operator -(BigInteger bi1, uint bi2)
        {
            return bi1 - (BigInteger)bi2;
        }

        public static BigInteger operator --(BigInteger bi1)
        {
            BigInteger result = new BigInteger(bi1);

            bool carryIn = true;
            int index = 0;

            while (carryIn && index < MaxLength)
            {
                long val;
                val = result._data[index];
                val--;

                result._data[index] = (uint)(val & 0xFFFFFFFF);

                if (val >= 0)
                    carryIn = false;

                index++;
            }

            if (index > result._length)
                result._length = index;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            // overflow check
            int lastPos = MaxLength - 1;

            // overflow if initial value was -ve but -- caused a sign
            // change to positive.

            if ((bi1._data[lastPos] & 0x80000000) != 0 &&
                (result._data[lastPos] & 0x80000000) != (bi1._data[lastPos] & 0x80000000))
            {
                throw (new ArithmeticException("Underflow in --."));
            }

            return result;
        }

        public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
        {
            int lastPos = MaxLength - 1;
            bool bi1Neg = false, bi2Neg = false;

            // take the absolute value of the inputs
            try
            {
                if ((bi1._data[lastPos] & 0x80000000) != 0) // bi1 negative
                {
                    bi1Neg = true;
                    bi1 = -bi1;
                }
                if ((bi2._data[lastPos] & 0x80000000) != 0) // bi2 negative
                {
                    bi2Neg = true;
                    bi2 = -bi2;
                }
            }
            catch (Exception)
            {
            }

            BigInteger result = new BigInteger();

            // multiply the absolute values
            try
            {
                for (int i = 0; i < bi1._length; i++)
                {
                    if (bi1._data[i] == 0) continue;

                    ulong mcarry = 0;
                    for (int j = 0, k = i; j < bi2._length; j++, k++)
                    {
                        // k = i + j
                        ulong val = ((ulong)bi1._data[i] * (ulong)bi2._data[j]) +
                                    result._data[k] + mcarry;

                        result._data[k] = (uint)(val & 0xFFFFFFFF);
                        mcarry = (val >> 32);
                    }

                    if (mcarry != 0)
                        result._data[i + bi2._length] = (uint)mcarry;
                }
            }
            catch (Exception)
            {
                throw (new ArithmeticException("Multiplication overflow."));
            }


            result._length = bi1._length + bi2._length;
            if (result._length > MaxLength)
                result._length = MaxLength;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            // overflow check (result is -ve)
            if ((result._data[lastPos] & 0x80000000) != 0)
            {
                if (bi1Neg != bi2Neg && result._data[lastPos] == 0x80000000) // different sign
                {
                    // handle the special case where multiplication produces
                    // a max negative number in 2's complement.

                    if (result._length == 1)
                        return result;
                    else
                    {
                        bool isMaxNeg = true;
                        for (int i = 0; i < result._length - 1 && isMaxNeg; i++)
                        {
                            if (result._data[i] != 0)
                                isMaxNeg = false;
                        }

                        if (isMaxNeg)
                            return result;
                    }
                }

                throw (new ArithmeticException("Multiplication overflow."));
            }

            // if input has different signs, then result is -ve
            if (bi1Neg != bi2Neg)
                return -result;

            return result;
        }

        public static BigInteger operator *(BigInteger bi1, long bi2)
        {
            return bi1 * (BigInteger)bi2;
        }

        public static BigInteger operator *(BigInteger bi1, ulong bi2)
        {
            return bi1 * (BigInteger)bi2;
        }

        public static BigInteger operator *(BigInteger bi1, int bi2)
        {
            return bi1 * (BigInteger)bi2;
        }

        public static BigInteger operator *(BigInteger bi1, uint bi2)
        {
            return bi1 * (BigInteger)bi2;
        }

        private static void multiByteDivide(BigInteger bi1, BigInteger bi2,
                                            BigInteger outQuotient, BigInteger outRemainder)
        {
            uint[] result = new uint[MaxLength];

            int remainderLen = bi1._length + 1;
            uint[] remainder = new uint[remainderLen];

            uint mask = 0x80000000;
            uint val = bi2._data[bi2._length - 1];
            int shift = 0, resultPos = 0;

            while (mask != 0 && (val & mask) == 0)
            {
                shift++;
                mask >>= 1;
            }

            for (int i = 0; i < bi1._length; i++)
                remainder[i] = bi1._data[i];
            shiftLeft(remainder, shift);
            bi2 = bi2 << shift;

            int j = remainderLen - bi2._length;
            int pos = remainderLen - 1;

            ulong firstDivisorByte = bi2._data[bi2._length - 1];
            ulong secondDivisorByte = bi2._data[bi2._length - 2];

            int divisorLen = bi2._length + 1;
            uint[] dividendPart = new uint[divisorLen];

            while (j > 0)
            {
                ulong dividend = ((ulong)remainder[pos] << 32) + remainder[pos - 1];
                ulong q_hat = dividend / firstDivisorByte;
                ulong r_hat = dividend % firstDivisorByte;

                bool done = false;
                while (!done)
                {
                    done = true;

                    if (q_hat == 0x100000000 ||
                        (q_hat * secondDivisorByte) > ((r_hat << 32) + remainder[pos - 2]))
                    {
                        q_hat--;
                        r_hat += firstDivisorByte;

                        if (r_hat < 0x100000000)
                            done = false;
                    }
                }

                for (int h = 0; h < divisorLen; h++)
                    dividendPart[h] = remainder[pos - h];

                BigInteger kk = new BigInteger(dividendPart);
                BigInteger ss = bi2 * (long)q_hat;

                while (ss > kk)
                {
                    q_hat--;
                    ss -= bi2;
                }

                BigInteger yy = kk - ss;

                for (int h = 0; h < divisorLen; h++)
                    remainder[pos - h] = yy._data[bi2._length - h];

                result[resultPos++] = (uint)q_hat;

                pos--;
                j--;
            }

            outQuotient._length = resultPos;
            int y = 0;
            for (int x = outQuotient._length - 1; x >= 0; x--, y++)
                outQuotient._data[y] = result[x];
            for (; y < MaxLength; y++)
                outQuotient._data[y] = 0;

            while (outQuotient._length > 1 && outQuotient._data[outQuotient._length - 1] == 0)
                outQuotient._length--;

            if (outQuotient._length == 0)
                outQuotient._length = 1;

            outRemainder._length = shiftRight(remainder, shift);

            for (y = 0; y < outRemainder._length; y++)
                outRemainder._data[y] = remainder[y];
            for (; y < MaxLength; y++)
                outRemainder._data[y] = 0;
        }

        private static void singleByteDivide(BigInteger bi1, BigInteger bi2,
                                             BigInteger outQuotient, BigInteger outRemainder)
        {
            uint[] result = new uint[MaxLength];
            int resultPos = 0;

            // copy dividend to reminder
            for (int i = 0; i < MaxLength; i++)
                outRemainder._data[i] = bi1._data[i];
            outRemainder._length = bi1._length;

            while (outRemainder._length > 1 && outRemainder._data[outRemainder._length - 1] == 0)
                outRemainder._length--;

            ulong divisor = bi2._data[0];
            int pos = outRemainder._length - 1;
            ulong dividend = outRemainder._data[pos];

            if (dividend >= divisor)
            {
                ulong quotient = dividend / divisor;
                result[resultPos++] = (uint)quotient;

                outRemainder._data[pos] = (uint)(dividend % divisor);
            }
            pos--;

            while (pos >= 0)
            {
                dividend = ((ulong)outRemainder._data[pos + 1] << 32) + outRemainder._data[pos];
                ulong quotient = dividend / divisor;
                result[resultPos++] = (uint)quotient;

                outRemainder._data[pos + 1] = 0;
                outRemainder._data[pos--] = (uint)(dividend % divisor);
            }

            outQuotient._length = resultPos;
            int j = 0;
            for (int i = outQuotient._length - 1; i >= 0; i--, j++)
                outQuotient._data[j] = result[i];
            for (; j < MaxLength; j++)
                outQuotient._data[j] = 0;

            while (outQuotient._length > 1 && outQuotient._data[outQuotient._length - 1] == 0)
                outQuotient._length--;

            if (outQuotient._length == 0)
                outQuotient._length = 1;

            while (outRemainder._length > 1 && outRemainder._data[outRemainder._length - 1] == 0)
                outRemainder._length--;
        }

        public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
        {
            BigInteger quotient = new BigInteger();
            BigInteger remainder = new BigInteger();

            int lastPos = MaxLength - 1;
            bool divisorNeg = false, dividendNeg = false;

            if ((bi1._data[lastPos] & 0x80000000) != 0) // bi1 negative
            {
                bi1 = -bi1;
                dividendNeg = true;
            }
            if ((bi2._data[lastPos] & 0x80000000) != 0) // bi2 negative
            {
                bi2 = -bi2;
                divisorNeg = true;
            }

            if (bi1 < bi2)
            {
                return quotient;
            }

            else
            {
                if (bi2._length == 1)
                    singleByteDivide(bi1, bi2, quotient, remainder);
                else
                    multiByteDivide(bi1, bi2, quotient, remainder);

                if (dividendNeg != divisorNeg)
                    return -quotient;

                return quotient;
            }
        }

        public static BigInteger operator /(BigInteger bi1, long bi2)
        {
            return bi1 / (BigInteger)bi2;
        }

        public static BigInteger operator /(BigInteger bi1, ulong bi2)
        {
            return bi1 / (BigInteger)bi2;
        }

        public static BigInteger operator /(BigInteger bi1, int bi2)
        {
            return bi1 / (BigInteger)bi2;
        }

        public static BigInteger operator /(BigInteger bi1, uint bi2)
        {
            return bi1 / (BigInteger)bi2;
        }

        public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
        {
            BigInteger quotient = new BigInteger();
            BigInteger remainder = new BigInteger(bi1);

            int lastPos = MaxLength - 1;
            bool dividendNeg = false;

            if ((bi1._data[lastPos] & 0x80000000) != 0) // bi1 negative
            {
                bi1 = -bi1;
                dividendNeg = true;
            }
            if ((bi2._data[lastPos] & 0x80000000) != 0) // bi2 negative
                bi2 = -bi2;

            if (bi1 < bi2)
            {
                return remainder;
            }

            else
            {
                if (bi2._length == 1)
                    singleByteDivide(bi1, bi2, quotient, remainder);
                else
                    multiByteDivide(bi1, bi2, quotient, remainder);

                if (dividendNeg)
                    return -remainder;

                return remainder;
            }
        }

        public static BigInteger operator %(BigInteger bi1, long bi2)
        {
            return bi1 % (BigInteger)bi2;
        }

        public static BigInteger operator %(BigInteger bi1, ulong bi2)
        {
            return bi1 % (BigInteger)bi2;
        }

        public static BigInteger operator %(BigInteger bi1, int bi2)
        {
            return bi1 % (BigInteger)bi2;
        }

        public static BigInteger operator %(BigInteger bi1, uint bi2)
        {
            return bi1 % (BigInteger)bi2;
        }

        public static BigInteger operator %(long bi1, BigInteger bi2)
        {
            return (BigInteger)bi1 % bi2;
        }

        public static BigInteger operator %(ulong bi1, BigInteger bi2)
        {
            return (BigInteger)bi1 % bi2;
        }

        public static BigInteger operator %(int bi1, BigInteger bi2)
        {
            return (BigInteger)bi1 % bi2;
        }

        public static BigInteger operator %(uint bi1, BigInteger bi2)
        {
            return (BigInteger)bi1 % bi2;
        }

        public static BigInteger operator <<(BigInteger bi1, int shiftVal)
        {
            BigInteger result = new BigInteger(bi1);
            result._length = shiftLeft(result._data, shiftVal);

            return result;
        }

        private static int shiftLeft(uint[] buffer, int shiftVal)
        {
            int shiftAmount = 32;
            int bufLen = buffer.Length;

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            for (int count = shiftVal; count > 0; )
            {
                if (count < shiftAmount)
                    shiftAmount = count;

                ulong carry = 0;
                for (int i = 0; i < bufLen; i++)
                {
                    ulong val = ((ulong)buffer[i]) << shiftAmount;
                    val |= carry;

                    buffer[i] = (uint)(val & 0xFFFFFFFF);
                    carry = val >> 32;
                }

                if (carry != 0)
                {
                    if (bufLen + 1 <= buffer.Length)
                    {
                        buffer[bufLen] = (uint)carry;
                        bufLen++;
                    }
                }
                count -= shiftAmount;
            }
            return bufLen;
        }

        public static BigInteger operator >>(BigInteger bi1, int shiftVal)
        {
            BigInteger result = new BigInteger(bi1);
            result._length = shiftRight(result._data, shiftVal);


            if ((bi1._data[MaxLength - 1] & 0x80000000) != 0) // negative
            {
                for (int i = MaxLength - 1; i >= result._length; i--)
                    result._data[i] = 0xFFFFFFFF;

                uint mask = 0x80000000;
                for (int i = 0; i < 32; i++)
                {
                    if ((result._data[result._length - 1] & mask) != 0)
                        break;

                    result._data[result._length - 1] |= mask;
                    mask >>= 1;
                }
                result._length = MaxLength;
            }

            return result;
        }


        private static int shiftRight(uint[] buffer, int shiftVal)
        {
            int shiftAmount = 32;
            int invShift = 0;
            int bufLen = buffer.Length;

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            for (int count = shiftVal; count > 0; )
            {
                if (count < shiftAmount)
                {
                    shiftAmount = count;
                    invShift = 32 - shiftAmount;
                }

                ulong carry = 0;
                for (int i = bufLen - 1; i >= 0; i--)
                {
                    ulong val = ((ulong)buffer[i]) >> shiftAmount;
                    val |= carry;

                    carry = ((ulong)buffer[i]) << invShift;
                    buffer[i] = (uint)(val);
                }

                count -= shiftAmount;
            }

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            return bufLen;
        }

        public static BigInteger operator ~(BigInteger bi1)
        {
            BigInteger result = new BigInteger(bi1);

            for (int i = 0; i < MaxLength; i++)
                result._data[i] = ~(bi1._data[i]);

            result._length = MaxLength;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            return result;
        }

        public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
        {
            BigInteger result = new BigInteger();

            int len = (bi1._length > bi2._length) ? bi1._length : bi2._length;

            for (int i = 0; i < len; i++)
            {
                uint sum = bi1._data[i] & bi2._data[i];
                result._data[i] = sum;
            }

            result._length = MaxLength;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            return result;
        }

        public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
        {
            BigInteger result = new BigInteger();

            int len = (bi1._length > bi2._length) ? bi1._length : bi2._length;

            for (int i = 0; i < len; i++)
            {
                uint sum = bi1._data[i] | bi2._data[i];
                result._data[i] = sum;
            }

            result._length = MaxLength;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            return result;
        }

        public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
        {
            BigInteger result = new BigInteger();

            int len = (bi1._length > bi2._length) ? bi1._length : bi2._length;

            for (int i = 0; i < len; i++)
            {
                uint sum = bi1._data[i] ^ bi2._data[i];
                result._data[i] = sum;
            }

            result._length = MaxLength;

            while (result._length > 1 && result._data[result._length - 1] == 0)
                result._length--;

            return result;
        }

        public static bool operator ==(BigInteger bi1, BigInteger bi2)
        {
            if (bi1 as Object == null && bi2 as Object == null)
                return true;
            else if (bi1 as Object == null || bi2 as Object == null)
                return false;

            return bi1.Equals(bi2);
        }

        public static bool operator ==(BigInteger bi1, uint bi2)
        {
            return bi1 == (BigInteger)bi2;
        }

        public static bool operator ==(BigInteger bi1, int bi2)
        {
            return bi1 == (BigInteger)bi2;
        }

        public static bool operator ==(BigInteger bi1, long bi2)
        {
            return bi1 == (BigInteger)bi2;
        }

        public static bool operator ==(BigInteger bi1, ulong bi2)
        {
            return bi1 == (BigInteger)bi2;
        }

        public static bool operator !=(BigInteger bi1, BigInteger bi2)
        {
            if ((object)bi1 == null && (object)bi2 == null)
                return false;
            else if ((object)bi1 == null || (object)bi2 == null)
                return true;
            else
                return !(bi1.Equals(bi2));
        }

        public static bool operator !=(BigInteger bi1, uint bi2)
        {
            return bi1 != (BigInteger)bi2;
        }

        public static bool operator !=(BigInteger bi1, int bi2)
        {
            return bi1 != (BigInteger)bi2;
        }

        public static bool operator !=(BigInteger bi1, long bi2)
        {
            return bi1 != (BigInteger)bi2;
        }

        public static bool operator !=(BigInteger bi1, ulong bi2)
        {
            return bi1 != (BigInteger)bi2;
        }


        public override bool Equals(object o)
        {
            BigInteger bi = (BigInteger)o;

            if (_length != bi._length)
                return false;

            for (int i = 0; i < _length; i++)
            {
                if (_data[i] != bi._data[i])
                    return false;
            }
            return true;
        }

        public static bool operator >(BigInteger bi1, BigInteger bi2)
        {
            int pos = MaxLength - 1;

            // bi1 is negative, bi2 is positive
            if ((bi1._data[pos] & 0x80000000) != 0 && (bi2._data[pos] & 0x80000000) == 0)
                return false;

                // bi1 is positive, bi2 is negative
            else if ((bi1._data[pos] & 0x80000000) == 0 && (bi2._data[pos] & 0x80000000) != 0)
                return true;

            // same sign
            int len = (bi1._length > bi2._length) ? bi1._length : bi2._length;
            for (pos = len - 1; pos >= 0 && bi1._data[pos] == bi2._data[pos]; pos--) ;

            if (pos >= 0)
            {
                if (bi1._data[pos] > bi2._data[pos])
                    return true;
                return false;
            }
            return false;
        }

        public static bool operator >(BigInteger bi1, long bi2)
        {
            return bi1 > (BigInteger)bi2;
        }

        public static bool operator >(BigInteger bi1, ulong bi2)
        {
            return bi1 > (BigInteger)bi2;
        }

        public static bool operator >(BigInteger bi1, int bi2)
        {
            return bi1 > (BigInteger)bi2;
        }

        public static bool operator >(BigInteger bi1, uint bi2)
        {
            return bi1 > (BigInteger)bi2;
        }


        public static bool operator <(BigInteger bi1, BigInteger bi2)
        {
            int pos = MaxLength - 1;

            // bi1 is negative, bi2 is positive
            if ((bi1._data[pos] & 0x80000000) != 0 && (bi2._data[pos] & 0x80000000) == 0)
                return true;

                // bi1 is positive, bi2 is negative
            else if ((bi1._data[pos] & 0x80000000) == 0 && (bi2._data[pos] & 0x80000000) != 0)
                return false;

            // same sign
            int len = (bi1._length > bi2._length) ? bi1._length : bi2._length;
            for (pos = len - 1; pos >= 0 && bi1._data[pos] == bi2._data[pos]; pos--) ;

            if (pos >= 0)
            {
                if (bi1._data[pos] < bi2._data[pos])
                    return true;
                return false;
            }
            return false;
        }

        public static bool operator <(BigInteger bi1, long bi2)
        {
            return bi1 < (BigInteger)bi2;
        }

        public static bool operator <(BigInteger bi1, ulong bi2)
        {
            return bi1 < (BigInteger)bi2;
        }

        public static bool operator <(BigInteger bi1, int bi2)
        {
            return bi1 < (BigInteger)bi2;
        }

        public static bool operator <(BigInteger bi1, uint bi2)
        {
            return bi1 < (BigInteger)bi2;
        }

        public static bool operator >=(BigInteger bi1, BigInteger bi2)
        {
            return (bi1 == bi2 || bi1 > bi2);
        }

        public static bool operator >=(BigInteger bi1, long bi2)
        {
            return bi1 >= (BigInteger)bi2;
        }

        public static bool operator >=(BigInteger bi1, ulong bi2)
        {
            return bi1 >= (BigInteger)bi2;
        }

        public static bool operator >=(BigInteger bi1, int bi2)
        {
            return bi1 >= (BigInteger)bi2;
        }

        public static bool operator >=(BigInteger bi1, uint bi2)
        {
            return bi1 >= (BigInteger)bi2;
        }

        public static bool operator <=(BigInteger bi1, BigInteger bi2)
        {
            return (bi1 == bi2 || bi1 < bi2);
        }

        public static bool operator <=(BigInteger bi1, long bi2)
        {
            return bi1 <= (BigInteger)bi2;
        }

        public static bool operator <=(BigInteger bi1, ulong bi2)
        {
            return bi1 <= (BigInteger)bi2;
        }

        public static bool operator <=(BigInteger bi1, int bi2)
        {
            return bi1 <= (BigInteger)bi2;
        }

        public static bool operator <=(BigInteger bi1, uint bi2)
        {
            return bi1 <= (BigInteger)bi2;
        }
        #endregion

        #region [Max/Min/Abs] Handling
        public BigInteger Max(BigInteger bi)
        {
            if (this > bi)
                return (new BigInteger(this));
            else
                return (new BigInteger(bi));
        }

        public BigInteger Min(BigInteger bi)
        {
            if (this < bi)
                return (new BigInteger(this));
            else
                return (new BigInteger(bi));
        }

        public BigInteger Abs()
        {
            if ((_data[MaxLength - 1] & 0x80000000) != 0)
                return (-this);
            else
                return (new BigInteger(this));
        }
        #endregion

        #region Primality Testing

        //***********************************************************************
        // Probabilistic prime test based on Fermat's little theorem
        //
        // for any a < p (p does not divide a) if
        //      a^(p-1) mod p != 1 then p is not prime.
        //
        // Otherwise, p is probably prime (pseudoprime to the chosen base).
        //
        // Returns
        // -------
        // True if "this" is a pseudoprime to randomly chosen
        // bases.  The number of chosen bases is given by the "confidence"
        // parameter.
        //
        // False if "this" is definitely NOT prime.
        //
        // Note - this method is fast but fails for Carmichael numbers except
        // when the randomly chosen base is a factor of the number.
        //
        //***********************************************************************

        public bool FermatLittleTest(int confidence)
        {
            BigInteger thisVal;
            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                thisVal = -this;
            else
                thisVal = this;

            if (thisVal._length == 1)
            {
                // test small numbers
                if (thisVal._data[0] == 0 || thisVal._data[0] == 1)
                    return false;
                else if (thisVal._data[0] == 2 || thisVal._data[0] == 3)
                    return true;
            }

            if ((thisVal._data[0] & 0x1) == 0) // even numbers
                return false;

            int bits = thisVal.BitCount();
            BigInteger a = new BigInteger();
            BigInteger p_sub1 = thisVal - (new BigInteger(1));
            Random rand = new Random();

            for (int round = 0; round < confidence; round++)
            {
                bool done = false;

                while (!done) // generate a < n
                {
                    int testBits = 0;

                    // make sure "a" has at least 2 bits
                    while (testBits < 2)
                        testBits = (int)(rand.NextDouble() * bits);

                    a.GenerateRandomBits(testBits, rand);

                    int byteLen = a._length;

                    // make sure "a" is not 0
                    if (byteLen > 1 || (byteLen == 1 && a._data[0] != 1))
                        done = true;
                }

                // check whether a factor exists (fix for version 1.03)
                BigInteger gcdTest = a.GCD(thisVal);
                if (gcdTest._length == 1 && gcdTest._data[0] != 1)
                    return false;

                // calculate a^(p-1) mod p
                BigInteger expResult = a.ModPow(p_sub1, thisVal);

                int resultLen = expResult._length;

                // is NOT prime is a^(p-1) mod p != 1

                if (resultLen > 1 || (resultLen == 1 && expResult._data[0] != 1))
                {
                    return false;
                }
            }

            return true;
        }


        //***********************************************************************
        // Probabilistic prime test based on Rabin-Miller's
        //
        // for any p > 0 with p - 1 = 2^s * t
        //
        // p is probably prime (strong pseudoprime) if for any a < p,
        // 1) a^t mod p = 1 or
        // 2) a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
        //
        // Otherwise, p is composite.
        //
        // Returns
        // -------
        // True if "this" is a strong pseudoprime to randomly chosen
        // bases.  The number of chosen bases is given by the "confidence"
        // parameter.
        //
        // False if "this" is definitely NOT prime.
        //
        //***********************************************************************

        public bool RabinMillerTest(int confidence)
        {
            BigInteger thisVal;
            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                thisVal = -this;
            else
                thisVal = this;

            if (thisVal._length == 1)
            {
                // test small numbers
                if (thisVal._data[0] == 0 || thisVal._data[0] == 1)
                    return false;
                else if (thisVal._data[0] == 2 || thisVal._data[0] == 3)
                    return true;
            }

            if ((thisVal._data[0] & 0x1) == 0) // even numbers
                return false;


            // calculate values of s and t
            BigInteger p_sub1 = thisVal - (new BigInteger(1));
            int s = 0;

            for (int index = 0; index < p_sub1._length; index++)
            {
                uint mask = 0x01;

                for (int i = 0; i < 32; i++)
                {
                    if ((p_sub1._data[index] & mask) != 0)
                    {
                        index = p_sub1._length; // to break the outer loop
                        break;
                    }
                    mask <<= 1;
                    s++;
                }
            }

            BigInteger t = p_sub1 >> s;

            int bits = thisVal.BitCount();
            BigInteger a = new BigInteger();
            Random rand = new Random();

            for (int round = 0; round < confidence; round++)
            {
                bool done = false;

                while (!done) // generate a < n
                {
                    int testBits = 0;

                    // make sure "a" has at least 2 bits
                    while (testBits < 2)
                        testBits = (int)(rand.NextDouble() * bits);

                    a.GenerateRandomBits(testBits, rand);

                    int byteLen = a._length;

                    // make sure "a" is not 0
                    if (byteLen > 1 || (byteLen == 1 && a._data[0] != 1))
                        done = true;
                }

                // check whether a factor exists (fix for version 1.03)
                BigInteger gcdTest = a.GCD(thisVal);
                if (gcdTest._length == 1 && gcdTest._data[0] != 1)
                    return false;

                BigInteger b = a.ModPow(t, thisVal);

                bool result = false;

                if (b._length == 1 && b._data[0] == 1) // a^t mod p = 1
                    result = true;

                for (int j = 0; result == false && j < s; j++)
                {
                    if (b == p_sub1) // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
                    {
                        result = true;
                        break;
                    }

                    b = (b * b) % thisVal;
                }

                if (result == false)
                    return false;
            }
            return true;
        }


        //***********************************************************************
        // Probabilistic prime test based on Solovay-Strassen (Euler Criterion)
        //
        // p is probably prime if for any a < p (a is not multiple of p),
        // a^((p-1)/2) mod p = J(a, p)
        //
        // where J is the Jacobi symbol.
        //
        // Otherwise, p is composite.
        //
        // Returns
        // -------
        // True if "this" is a Euler pseudoprime to randomly chosen
        // bases.  The number of chosen bases is given by the "confidence"
        // parameter.
        //
        // False if "this" is definitely NOT prime.
        //
        //***********************************************************************

        public bool SolovayStrassenTest(int confidence)
        {
            BigInteger thisVal;
            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                thisVal = -this;
            else
                thisVal = this;

            if (thisVal._length == 1)
            {
                // test small numbers
                if (thisVal._data[0] == 0 || thisVal._data[0] == 1)
                    return false;
                else if (thisVal._data[0] == 2 || thisVal._data[0] == 3)
                    return true;
            }

            if ((thisVal._data[0] & 0x1) == 0) // even numbers
                return false;


            int bits = thisVal.BitCount();
            BigInteger a = new BigInteger();
            BigInteger p_sub1 = thisVal - 1;
            BigInteger p_sub1_shift = p_sub1 >> 1;

            Random rand = new Random();

            for (int round = 0; round < confidence; round++)
            {
                bool done = false;

                while (!done) // generate a < n
                {
                    int testBits = 0;

                    // make sure "a" has at least 2 bits
                    while (testBits < 2)
                        testBits = (int)(rand.NextDouble() * bits);

                    a.GenerateRandomBits(testBits, rand);

                    int byteLen = a._length;

                    // make sure "a" is not 0
                    if (byteLen > 1 || (byteLen == 1 && a._data[0] != 1))
                        done = true;
                }

                // check whether a factor exists (fix for version 1.03)
                BigInteger gcdTest = a.GCD(thisVal);
                if (gcdTest._length == 1 && gcdTest._data[0] != 1)
                    return false;

                // calculate a^((p-1)/2) mod p

                BigInteger expResult = a.ModPow(p_sub1_shift, thisVal);
                if (expResult == p_sub1)
                    expResult = (BigInteger)(-1);

                // calculate Jacobi symbol
                BigInteger jacob = (BigInteger)Jacobi((BigInteger)a, (BigInteger)thisVal);

                // if they are different then it is not prime
                if (expResult != jacob)
                    return false;
            }

            return true;
        }


        //***********************************************************************
        // Implementation of the Lucas Strong Pseudo Prime test.
        //
        // Let n be an odd number with gcd(n,D) = 1, and n - J(D, n) = 2^s * d
        // with d odd and s >= 0.
        //
        // If Ud mod n = 0 or V2^r*d mod n = 0 for some 0 <= r < s, then n
        // is a strong Lucas pseudoprime with parameters (P, Q).  We select
        // P and Q based on Selfridge.
        //
        // Returns True if number is a strong Lucus pseudo prime.
        // Otherwise, returns False indicating that number is composite.
        //***********************************************************************

        public bool LucasStrongTest()
        {
            BigInteger thisVal;
            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                thisVal = -this;
            else
                thisVal = this;

            if (thisVal._length == 1)
            {
                // test small numbers
                if (thisVal._data[0] == 0 || thisVal._data[0] == 1)
                    return false;
                else if (thisVal._data[0] == 2 || thisVal._data[0] == 3)
                    return true;
            }

            if ((thisVal._data[0] & 0x1) == 0) // even numbers
                return false;

            return LucasStrongTestHelper(thisVal);
        }

        //***********************************************************************
        // Determines whether a number is probably prime, using the Rabin-Miller's
        // test.  Before applying the test, the number is tested for divisibility
        // by primes < 2000
        //
        // Returns true if number is probably prime.
        //***********************************************************************

        public bool IsProbablePrime(int confidence)
        {
            BigInteger thisVal;
            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                thisVal = -this;
            else
                thisVal = this;


            // test for divisibility by primes < 2000
            for (int p = 0; p < _primesBelow2000.Length; p++)
            {
                BigInteger divisor = (BigInteger)_primesBelow2000[p];

                if (divisor >= thisVal)
                    break;

                BigInteger resultNum = thisVal % divisor;
                if (resultNum.IntValue() == 0)
                {
                    return false;
                }
            }

            if (thisVal.RabinMillerTest(confidence))
                return true;
            else
                return false;
        }


        //***********************************************************************
        // Determines whether this BigInteger is probably prime using a
        // combination of base 2 strong pseudoprime test and Lucas strong
        // pseudoprime test.
        //
        // The sequence of the primality test is as follows,
        //
        // 1) Trial divisions are carried out using prime numbers below 2000.
        //    if any of the primes divides this BigInteger, then it is not prime.
        //
        // 2) Perform base 2 strong pseudoprime test.  If this BigInteger is a
        //    base 2 strong pseudoprime, proceed on to the next step.
        //
        // 3) Perform strong Lucas pseudoprime test.
        //
        // Returns True if this BigInteger is both a base 2 strong pseudoprime
        // and a strong Lucas pseudoprime.
        //
        // For a detailed discussion of this primality test, see [6].
        //
        //***********************************************************************

        public bool IsProbablePrime()
        {
            BigInteger thisVal;
            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                thisVal = -this;
            else
                thisVal = this;

            if (thisVal._length == 1)
            {
                // test small numbers
                if (thisVal._data[0] == 0 || thisVal._data[0] == 1)
                    return false;
                else if (thisVal._data[0] == 2 || thisVal._data[0] == 3)
                    return true;
            }

            if ((thisVal._data[0] & 0x1) == 0) // even numbers
                return false;


            // test for divisibility by primes < 2000
            for (int p = 0; p < _primesBelow2000.Length; p++)
            {
                BigInteger divisor = (BigInteger)_primesBelow2000[p];

                if (divisor >= thisVal)
                    break;

                BigInteger resultNum = thisVal % divisor;
                if (resultNum.IntValue() == 0)
                {
                    return false;
                }
            }

            // Perform BASE 2 Rabin-Miller Test

            // calculate values of s and t
            BigInteger p_sub1 = thisVal - (new BigInteger(1));
            int s = 0;

            for (int index = 0; index < p_sub1._length; index++)
            {
                uint mask = 0x01;

                for (int i = 0; i < 32; i++)
                {
                    if ((p_sub1._data[index] & mask) != 0)
                    {
                        index = p_sub1._length; // to break the outer loop
                        break;
                    }
                    mask <<= 1;
                    s++;
                }
            }

            BigInteger t = p_sub1 >> s;

            BigInteger a = (BigInteger)2;

            // b = a^t mod p
            BigInteger b = a.ModPow(t, thisVal);
            bool result = false;

            if (b._length == 1 && b._data[0] == 1) // a^t mod p = 1
                result = true;

            for (int j = 0; result == false && j < s; j++)
            {
                if (b == p_sub1) // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
                {
                    result = true;
                    break;
                }

                b = (b * b) % thisVal;
            }

            // if number is strong pseudoprime to base 2, then do a strong lucas test
            if (result)
                result = LucasStrongTestHelper(thisVal);

            return result;
        }

        #endregion

        #region Primality Generation

        //***********************************************************************
        // Generates a positive BigInteger that is probably prime.
        //***********************************************************************

        public static BigInteger genPseudoPrime(int bits, int confidence, Random rand)
        {
            BigInteger result = new BigInteger();
            bool done = false;

            while (!done)
            {
                result.GenerateRandomBits(bits, rand);
                result._data[0] |= 0x01; // make it odd

                // prime test
                done = result.IsProbablePrime(confidence);
            }
            return result;
        }


        //***********************************************************************
        // Generates a random number with the specified number of bits such
        // that gcd(number, this) = 1
        //***********************************************************************

        public BigInteger genCoPrime(int bits, Random rand)
        {
            bool done = false;
            BigInteger result = new BigInteger();

            while (!done)
            {
                result.GenerateRandomBits(bits, rand);

                // gcd test
                BigInteger g = result.GCD(this);
                if (g._length == 1 && g._data[0] == 1)
                    done = true;
            }

            return result;
        }

        #endregion

        #region Bit Handling
        public void SetBit(uint bitNum)
        {
            uint bytePos = bitNum >> 5; // divide by 32
            byte bitPos = (byte)(bitNum & 0x1F); // get the lowest 5 bits

            uint mask = (uint)1 << bitPos;
            _data[bytePos] |= mask;

            if (bytePos >= _length)
                _length = (int)bytePos + 1;
        }

        public void UnsetBit(uint bitNum)
        {
            uint bytePos = bitNum >> 5;

            if (bytePos < _length)
            {
                byte bitPos = (byte)(bitNum & 0x1F);

                uint mask = (uint)1 << bitPos;
                uint mask2 = 0xFFFFFFFF ^ mask;

                _data[bytePos] &= mask2;

                if (_length > 1 && _data[_length - 1] == 0)
                    _length--;
            }
        }

        //***********************************************************************
        // Returns gcd(this, bi)
        //***********************************************************************

        public BigInteger GCD(BigInteger bi)
        {
            BigInteger x;
            BigInteger y;

            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative
                x = -this;
            else
                x = this;

            if ((bi._data[MaxLength - 1] & 0x80000000) != 0) // negative
                y = -bi;
            else
                y = bi;

            BigInteger g = y;

            while (x._length > 1 || (x._length == 1 && x._data[0] != 0))
            {
                g = x;
                x = y % x;
                y = g;
            }

            return g;
        }


        //***********************************************************************
        // Populates "this" with the specified amount of random bits
        //***********************************************************************

        public void GenerateRandomBits(int bits, Random rand)
        {
            int dwords = bits >> 5;
            int remBits = bits & 0x1F;

            if (remBits != 0)
                dwords++;

            if (dwords > MaxLength)
                throw (new ArithmeticException("Number of required bits > maxLength."));

            for (int i = 0; i < dwords; i++)
                _data[i] = (uint)(rand.NextDouble() * 0x100000000);

            for (int i = dwords; i < MaxLength; i++)
                _data[i] = 0;

            if (remBits != 0)
            {
                uint mask = (uint)(0x01 << (remBits - 1));
                _data[dwords - 1] |= mask;

                mask = 0xFFFFFFFF >> (32 - remBits);
                _data[dwords - 1] &= mask;
            }
            else
                _data[dwords - 1] |= 0x80000000;

            _length = dwords;

            if (_length == 0)
                _length = 1;
        }


        //***********************************************************************
        // Returns the position of the most significant bit in the BigInteger.
        //
        // Eg.  The result is 0, if the value of BigInteger is 0...0000 0000
        //      The result is 1, if the value of BigInteger is 0...0000 0001
        //      The result is 2, if the value of BigInteger is 0...0000 0010
        //      The result is 2, if the value of BigInteger is 0...0000 0011
        //
        //***********************************************************************

        public int BitCount()
        {
            while (_length > 1 && _data[_length - 1] == 0)
                _length--;

            uint value = _data[_length - 1];
            uint mask = 0x80000000;
            int bits = 32;

            while (bits > 0 && (value & mask) == 0)
            {
                bits--;
                mask >>= 1;
            }
            bits += ((_length - 1) << 5);

            return bits;
        }

        public byte LeastSignificantByte()
        {
            return ByteValue();
        }

        //***********************************************************************
        // Returns the lowest byte of the BigInteger as a byte.
        //***********************************************************************

        public byte ByteValue()
        {
            return (byte)_data[0];
        }

        //***********************************************************************
        // Returns the lowest 4 bytes of the BigInteger as an int.
        //***********************************************************************

        public int IntValue()
        {
            return (int)_data[0];
        }


        //***********************************************************************
        // Returns the lowest 8 bytes of the BigInteger as a long.
        //***********************************************************************

        public long LongValue()
        {
            long val;

            val = _data[0];
            try
            {
                // exception if maxLength = 1
                val |= (long)_data[1] << 32;
            }
            catch (Exception)
            {
                if ((_data[0] & 0x80000000) != 0) // negative
                    val = (int)_data[0];
            }

            return val;
        }
        #endregion

        #region [ModPow/ModInverse/Sqrt] Handling
        public BigInteger ModPow(BigInteger exp, BigInteger n)
        {
            if ((exp._data[MaxLength - 1] & 0x80000000) != 0)
                throw (new ArithmeticException("Positive exponents only."));

            BigInteger resultNum = (BigInteger)1;
            BigInteger tempNum;
            bool thisNegative = false;

            if ((_data[MaxLength - 1] & 0x80000000) != 0) // negative this
            {
                tempNum = -this % n;
                thisNegative = true;
            }
            else
                tempNum = this % n; // ensures (tempNum * tempNum) < b^(2k)

            if ((n._data[MaxLength - 1] & 0x80000000) != 0) // negative n
                n = -n;

            // calculate constant = b^(2k) / m
            BigInteger constant = new BigInteger();

            int i = n._length << 1;
            constant._data[i] = 0x00000001;
            constant._length = i + 1;

            constant = constant / n;
            int totalBits = exp.BitCount();
            int count = 0;

            // perform squaring and multiply exponentiation
            for (int pos = 0; pos < exp._length; pos++)
            {
                uint mask = 0x01;

                for (int index = 0; index < 32; index++)
                {
                    if ((exp._data[pos] & mask) != 0)
                        resultNum = BarrettReduction(resultNum * tempNum, n, constant);

                    mask <<= 1;

                    tempNum = BarrettReduction(tempNum * tempNum, n, constant);


                    if (tempNum._length == 1 && tempNum._data[0] == 1)
                    {
                        if (thisNegative && (exp._data[0] & 0x1) != 0) //odd exp
                            return -resultNum;
                        return resultNum;
                    }
                    count++;
                    if (count == totalBits)
                        break;
                }
            }

            if (thisNegative && (exp._data[0] & 0x1) != 0) //odd exp
                return -resultNum;

            return resultNum;
        }

        //***********************************************************************
        // Returns the modulo inverse of   Throws ArithmeticException if
        // the inverse does not exist.  (i.e. gcd(this, modulus) != 1)
        //***********************************************************************

        public BigInteger ModInverse(BigInteger modulus)
        {
            BigInteger[] p = { (BigInteger)0, (BigInteger)1 };
            BigInteger[] q = new BigInteger[2]; // quotients
            BigInteger[] r = { (BigInteger)0, (BigInteger)0 }; // remainders

            int step = 0;

            BigInteger a = modulus;
            BigInteger b = this;

            while (b._length > 1 || (b._length == 1 && b._data[0] != 0))
            {
                BigInteger quotient = new BigInteger();
                BigInteger remainder = new BigInteger();

                if (step > 1)
                {
                    BigInteger pval = (p[0] - (p[1] * q[0])) % modulus;
                    p[0] = p[1];
                    p[1] = pval;
                }

                if (b._length == 1)
                    singleByteDivide(a, b, quotient, remainder);
                else
                    multiByteDivide(a, b, quotient, remainder);

                q[0] = q[1];
                r[0] = r[1];
                q[1] = quotient;
                r[1] = remainder;

                a = b;
                b = remainder;

                step++;
            }

            if (r[0]._length > 1 || (r[0]._length == 1 && r[0]._data[0] != 1))
                throw (new ArithmeticException("No inverse!"));

            BigInteger result = ((p[0] - (p[1] * q[0])) % modulus);

            if ((result._data[MaxLength - 1] & 0x80000000) != 0)
                result += modulus; // get the least positive modulus

            return result;
        }
        
        //***********************************************************************
        // Returns a value that is equivalent to the integer square root
        // of the BigInteger.
        //
        // The integer square root of "this" is defined as the largest integer n
        // such that (n * n) <= this
        //
        //***********************************************************************

        public BigInteger Sqrt()
        {
            uint numBits = (uint)BitCount();

            if ((numBits & 0x1) != 0) // odd number of bits
                numBits = (numBits >> 1) + 1;
            else
                numBits = (numBits >> 1);

            uint bytePos = numBits >> 5;
            byte bitPos = (byte)(numBits & 0x1F);

            uint mask;

            BigInteger result = new BigInteger();
            if (bitPos == 0)
                mask = 0x80000000;
            else
            {
                mask = (uint)1 << bitPos;
                bytePos++;
            }
            result._length = (int)bytePos;

            for (int i = (int)bytePos - 1; i >= 0; i--)
            {
                while (mask != 0)
                {
                    // guess
                    result._data[i] ^= mask;

                    // undo the guess if its square is larger than this
                    if ((result * result) > this)
                        result._data[i] ^= mask;

                    mask >>= 1;
                }
                mask = 0x80000000;
            }
            return result;
        }
        #endregion
    }
}
