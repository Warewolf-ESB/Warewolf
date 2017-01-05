/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
            int seed = DateTime.Now.Millisecond;
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

        private string GenerateNumbers(double from, double to, ref int seed)
        {
            //Added for BUG 9506 to account for when the from is larger thean the to.
            if (from > to)
            {
                double tmpTo = to;
                to = from;
                from = tmpTo;
            }
            int powerOfTen = (int)Math.Pow(10, GetDecimalPlaces(@from, to));
            Random rand = GetRandom(ref seed);
            string result;
            if (powerOfTen != 1)
                result = (rand.NextDouble() * (to - from) + from).ToString(CultureInfo.InvariantCulture);
            //result = ((double)(rand.Next((int)(from * powerOfTen), (int)((to * powerOfTen) > 0 ? (to * powerOfTen + 1) : (to * powerOfTen)))) / (double)powerOfTen).ToString(CultureInfo.InvariantCulture);
            else
            {
                if (IsInIntRange(from) && IsInIntRange(to))
                    result = rand.Next((int)from, (int)(to > 0 ? to + 1 : to)).ToString(CultureInfo.InvariantCulture);
                else
                    result = Math.Round(rand.NextDouble() * (to - @from) + @from).ToString(CultureInfo.InvariantCulture);

            }
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

        private bool IsInIntRange(double x)
        {
            return x >= int.MinValue && x <= int.MaxValue;
        }

        private uint GetDecimalPlaces(double from, double to)
        {
            double smallest = Math.Min(
                Math.Abs(from),
                Math.Abs(to)
                );
            double largest = Math.Max(
                Math.Abs(from),
                Math.Abs(to)
                );
            return Math.Max(DecimalPlaces(smallest), DecimalPlaces(largest));
        }

        private uint DecimalPlaces(double x)
        {
            uint places = 0;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            // ReSharper disable once EmptyEmbeddedStatement
            for (; x * Math.Pow(10, places) % 1 != 0; places++) ;
            return places;
        }
        private string GenerateLetters(int length, ref int seed)
        {
            int charStart = EnvironmentVariables.CharacterMap.LettersStartNumber;
            int charEnd = charStart + EnvironmentVariables.CharacterMap.LettersLength;
            var result = new StringBuilder();


            for (int i = 0; i < length; i++)
            {
                Random rand = GetRandom(ref seed);
                result.Append((char)rand.Next(charStart, charEnd));
            }

            return result.ToString();
        }

        private string GenerateMixed(int length, ref int seed)
        {
            var result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                Random rand = GetRandom(ref seed);

                int coinFlip = rand.Next(1, 10);


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

        private Random GetRandom(ref int seed)
        {
            var r = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
            return new Random(seed += r.Next(1, 100000));
        }
    }
}