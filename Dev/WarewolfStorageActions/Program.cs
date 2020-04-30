/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using CommandLine;
using Warewolf.Security.Encryption;

namespace WarewolfStorageActions
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Implementation(options).Run(),
                _ => 1);
        }

        private class Implementation
        {
            private readonly Args _options;

            public Implementation(Args options)
            {
                _options = options;
            }

            public int Run()
            {
                var sourceFileName = _options.FileName;
                var sourceData = File.ReadAllText(sourceFileName);
                var destinationFileName = _options.OutputFileName;
                if (_options.ShouldEncrypt)
                {
                    var data = DpapiWrapper.Encrypt(sourceData);
                    File.WriteAllText(destinationFileName, data);
                }
                else
                {
                    var data = DpapiWrapper.Decrypt(sourceData);
                    File.WriteAllText(destinationFileName, data);
                }

                return 0;
            }
        }
    }
}
