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
using System.IO;
using System.Linq;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidFileNameRule : IsValidCollectionRule
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        public IsValidFileNameRule(Func<string> getValue, char splitToken = ';')
            : base(getValue, "file name", splitToken)
        {
        }

        protected override bool IsValid(string item)
        {
            try
            {
                string extension = Path.GetExtension(item);
                if (string.IsNullOrEmpty(extension))
                {
                    return false;
                }

                string dir = Path.GetDirectoryName(item);
                if (string.IsNullOrEmpty(dir) || HasIllegalCharacters(InvalidPathChars, dir))
                {
                    return false;
                }

                if (!dir.EndsWith("\\"))
                {
                    dir += "\\";
                }
                // NOTE: Path.GetFileName(@"c:\myfile:name.txt") returns "name.txt" instead of "myfile:name.txt"
                string fileName = item.Replace(dir, "").Replace(extension, "");

                if (string.IsNullOrEmpty(fileName) || HasIllegalCharacters(InvalidFileNameChars, fileName))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                // Illegal characters in path
                return false;
            }

            return true;
        }

        private static bool HasIllegalCharacters(IEnumerable<char> invalidChars, string path)
        {
            return path.Any(pathChar => invalidChars.Any(invalidChar => pathChar == invalidChar));
        }
    }
}