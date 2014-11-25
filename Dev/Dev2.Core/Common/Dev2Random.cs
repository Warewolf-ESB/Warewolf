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
using System.Globalization;
using System.Text;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Common
{
    public class Dev2Random
    {
        public string GetRandom(enRandomType type, int length, int from, int to)
        {
            int seed = DateTime.Now.Millisecond;
            if ((length < 0 && type != enRandomType.Guid && type != enRandomType.Numbers))
            {
                throw new ArgumentException();
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

        private string GenerateNumbers(int from, int to, ref int seed)
        {
            //Added for BUG 9506 to account for when the from is larger thean the to.
            if (from > to)
            {
                int tmpTo = to;
                to = from;
                from = tmpTo;
            }
            Random rand = GetRandom(ref seed);
            string result = rand.Next(from, to > 0 ? to + 1 : to).ToString(CultureInfo.InvariantCulture);
            return result;
        }

        private string GenerateLetters(int length, ref int seed)
        {
            int charStart = EnvironmentVariables.CharacterMap.LettersStartNumber;
            int charEnd = charStart + EnvironmentVariables.CharacterMap.LettersLength;
            var result = new StringBuilder();


            for (int i = 0; i < length; i++)
            {
                Random rand = GetRandom(ref seed);
                result.Append((char) (rand.Next(charStart, charEnd)));
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
            return new Random((seed += r.Next(1, 100000)));
        }
    }
}