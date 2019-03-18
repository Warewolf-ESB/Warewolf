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

using System.Collections.Generic;
using System.Linq;
using Dev2.Intellisense.Helper;
using Dev2.Studio.Interfaces;

namespace Dev2.Intellisense.Provider
{
    public class FileSystemIntellisenseProvider : IIntellisenseProvider
    {
        #region Class Members

        List<IntellisenseProviderResult> _intellisenseResults;

        #endregion Class Members

        #region Constructors

        public FileSystemIntellisenseProvider()
        {
            Optional = false;
            IntellisenseProviderType = IntellisenseProviderType.Default;
            HandlesResultInsertion = true;
            _intellisenseResults = new List<IntellisenseProviderResult>();
            FileSystemQuery = new FileSystemQuery(); 
        }

        #endregion Constructors

        #region Override Methods

        public IntellisenseProviderType IntellisenseProviderType { get; private set; }

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            VerifyArgument.IsNotNull("Context",context);
           
            var inputText = context.InputText ?? string.Empty;
            var caretPosition = context.CaretPosition;

            if (caretPosition < 0 || caretPosition>inputText.Length)
            {
                return string.Empty;
            }

            var regions = inputText.Split(' '); // we can safely do this because the default provider handles the language features

            var sum = 0;
            var items = 0;
            var regionsText = regions.Select(a => new { a, a.Length }).TakeWhile(a =>
            {
                sum = sum + a.Length;
                items++;
                return sum <= caretPosition || items==1;
            }).Select(a => a.a).ToList();
            regionsText[regionsText.Count - 1] = input;// set caret region to replacement text

            var prefix = regionsText.Aggregate("", (a, b) => a + " " + b).TrimStart(' '); // fold back together
            context.CaretPositionOnPopup = prefix.Length;
            context.CaretPosition = prefix.Length;
            var i = 0;
            var inner = regions.SkipWhile(a =>
                {
                    i = i + 1;
                    return i < regionsText.Count + 1;
                }).Aggregate("", (a, b) => a + " " + b);
            return (prefix + inner).TrimEnd();

        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            if (context == null)
            {
                return new List<IntellisenseProviderResult>();
            }

            var results = new List<IntellisenseProviderResult>();
            if(context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
            {
                IntellisenseResults.Clear();
                FileSystemQuery.QueryList("");
                FileSystemQuery.QueryCollection.ForEach(s => IntellisenseResults.Add(new IntellisenseProviderResult(this, s, string.Empty, string.Empty, false)));
                results.AddRange(IntellisenseResults);
            }
            else
            {
                if(!InLiteralRegion(context.InputText, context.CaretPosition))
                {
                    IntellisenseResults.Clear();
                    var regions = context.InputText.Split(' ');
                    var sum = 0;
                    var searchText = regions.Select(a => new { a, a.Length }).TakeWhile(a =>
                        {
                            sum = sum + context.CaretPosition;
                            return sum >= context.CaretPosition;
                        }).Last().a;
                    FileSystemQuery.QueryList(searchText);
                    FileSystemQuery.QueryCollection.ForEach(s => IntellisenseResults.Add(new IntellisenseProviderResult(this, s, string.Empty, string.Empty, false)));
                    results.AddRange(IntellisenseResults);
                }
            }
            return results;
        }

        public void Dispose()
        {
            _intellisenseResults = null;
        }

        #endregion Override Methods

        #region Private Methods

        /// <summary>
        ///     Determines if the caret in a literal region
        /// </summary>
        bool InLiteralRegion(string inputText, int caretPosition)
        {
            var inLiteralResionRegion = false;

            if (caretPosition <= inputText.Length)
            {
                inputText = inputText.Replace("\\\\", "##");
                inputText = inputText.Replace("\\'", "##");
                inputText = inputText.Replace("'''", "###");

                inLiteralResionRegion = inputText.Substring(0, caretPosition).Count(s => s == '\'') % 2 != 0;
            }

            return inLiteralResionRegion;
        }

        #endregion Private Methods

        #region Properties

        public bool Optional { get; set; }
        public bool HandlesResultInsertion { get; set; }

        #endregion Properties

        public IFileSystemQuery FileSystemQuery { get; set; }

        public List<IntellisenseProviderResult> IntellisenseResults => _intellisenseResults;
    }
}
