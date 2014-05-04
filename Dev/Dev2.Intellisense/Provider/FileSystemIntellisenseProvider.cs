using Dev2.Intellisense.Helper;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Intellisense.Provider
{
    public class FileSystemIntellisenseProvider : IIntellisenseProvider
    {
        #region Class Members

        List<IntellisenseProviderResult> _intellisenseResults;

        IFileSystemQuery _fileSystemQuery;

        #endregion Class Members

        #region Constructors

        public FileSystemIntellisenseProvider()
        {
            Optional = false;
            HandlesResultInsertion = true;
            _intellisenseResults = new List<IntellisenseProviderResult>();
            _fileSystemQuery = new FileSystemQuery();
        }

        #endregion Constructors

        #region Override Methods

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {

            var regions = context.InputText.Split(new[] { ' ' });
            var sum = 0;
            var regionsText = regions.Select(a => new { a, a.Length }).TakeWhile(a =>
            {
                sum = sum + context.CaretPosition;
                return sum >= context.CaretPosition;
            }).Select(a => a.a).ToList();
            regionsText[regionsText.Count - 1] = input;
            var prefix = regionsText.Aggregate("", (a, b) => a + " " + b).TrimStart(new[] { ' ' });
            context.CaretPositionOnPopup = prefix.Length;
            context.CaretPosition = prefix.Length;
            int i = 0;
            var inner = regions.SkipWhile(a =>
                {
                    i = i + 1;
                    return i < regionsText.Count + 1;
                }).Aggregate("", (a, b) => a + " " + b);
            return prefix + inner;

        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            var results = new List<IntellisenseProviderResult>();
            if(context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
            {
                FileSystemQuery.QueryList("");
                FileSystemQuery.QueryCollection.ForEach(s => _intellisenseResults.Add(new IntellisenseProviderResult(this, s, string.Empty, string.Empty, false)));
                results.AddRange(_intellisenseResults);
            }
            else
            {
                if(!InLiteralRegion(context.InputText, context.CaretPosition))
                {
                    _intellisenseResults.Clear();
                    var regions = context.InputText.Split(new[] { ' ' });
                    var sum = 0;
                    string searchText = regions.Select(a => new { a, a.Length }).TakeWhile(a =>
                        {
                            sum = sum + context.CaretPosition;
                            return sum >= context.CaretPosition;
                        }).Last().a;
                    FileSystemQuery.QueryList(searchText);
                    FileSystemQuery.QueryCollection.ForEach(s => _intellisenseResults.Add(new IntellisenseProviderResult(this, s, string.Empty, string.Empty, false)));
                    results.AddRange(_intellisenseResults);
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
            bool inLiteralResionRegion = false;

            if(caretPosition <= inputText.Length)
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



        public IFileSystemQuery FileSystemQuery
        {
            get
            {
                return _fileSystemQuery;
            }
            set
            {
                _fileSystemQuery = value;
            }
        }
    }
}