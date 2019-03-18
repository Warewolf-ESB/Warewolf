#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Warewolf.Resource.Errors;

namespace Dev2.Common
{
    public class Dev2Random
    {
        public string GetRandom(enRandomType type, int length, double from, double to)
        {
            var seed = DateTime.Now.Millisecond;
            if (length < 0 && type != enRandomType.Guid && type != enRandomType.Numbers)
            {
                throw new ArgumentException(ErrorResource.InvalidLength);
            }

            switch (type)
            {
                case enRandomType.Letters:
                    return GenerateLetters(length, ref seed);
                case enRandomType.Numbers:
                    return GenerateNumbers(from, to, ref seed);
                case enRandomType.LetterAndNumbers:
                    return GenerateMixed(length, ref seed);
                case enRandomType.Guid:
                    return Guid.NewGuid().ToString();
                default:
                    return null;
            }
        }

        string GenerateNumbers(double from, double to, ref int seed)
        {
            //Added for BUG 9506 to account for when the from is larger thean the to.
            if (from > to)
            {
                var tmpTo = to;
                to = from;
                from = tmpTo;
            }
            var powerOfTen = (int)Math.Pow(10, GetDecimalPlaces(@from, to));
            var rand = GetRandom(ref seed);
            string result;
            result = powerOfTen != 1 ? (rand.NextDouble() * (to - from) + from).ToString(CultureInfo.InvariantCulture) : IsInIntRange(from) && IsInIntRange(to) ? rand.Next((int)from, (int)(to > 0 ? to + 1 : to)).ToString(CultureInfo.InvariantCulture) : Math.Round(rand.NextDouble() * (to - @from) + @from).ToString(CultureInfo.InvariantCulture);
            if (result == double.PositiveInfinity.ToString(CultureInfo.InvariantCulture))
            {
                return double.MaxValue.ToString(CultureInfo.InvariantCulture);
            }
            if (result == double.NegativeInfinity.ToString(CultureInfo.InvariantCulture))
            {
                return double.MinValue.ToString(CultureInfo.InvariantCulture);
            }
            return result;
        }

        bool IsInIntRange(double x) => x >= int.MinValue && x <= int.MaxValue;

        uint GetDecimalPlaces(double from, double to)
        {
            var smallest = Math.Min(
                Math.Abs(from),
                Math.Abs(to)
                );
            var largest = Math.Max(
                Math.Abs(from),
                Math.Abs(to)
                );
            return Math.Max(DecimalPlaces(smallest), DecimalPlaces(largest));
        }

        uint DecimalPlaces(double x)
        {
            uint places = 0;
            while (!((x * Math.Pow(10, places) % 1).Equals(0)))
            {
                places++;
            }
            return places;
        }
        string GenerateLetters(int length, ref int seed)
        {
            var charStart = EnvironmentVariables.CharacterMap.LettersStartNumber;
            var charEnd = charStart + EnvironmentVariables.CharacterMap.LettersLength;
            var result = new StringBuilder();


            for (int i = 0; i < length; i++)
            {
                var rand = GetRandom(ref seed);
                result.Append((char)rand.Next(charStart, charEnd));
            }

            return result.ToString();
        }

        string GenerateMixed(int length, ref int seed)
        {
            var result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                var rand = GetRandom(ref seed);

                var coinFlip = rand.Next(1, 10);


                if (coinFlip < 5)
                {
                    // do chars 
                    result.Append(GenerateLetters(1, ref seed));
                }
                else
                {
                    // do numbers
                    result.Append(rand.Next(0, 9));
                }
            }

            return result.ToString();
        }

        Random GetRandom(ref int seed)
        {
            var r = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
            seed += r.Next(1, 100000);
            return new Random(seed);
        }
    }
}