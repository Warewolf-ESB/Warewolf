#pragma warning disable
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
