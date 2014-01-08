using Dev2.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
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
            HandlesResultInsertion = true;
            _intellisenseResults = new List<IntellisenseProviderResult>();
            FileSystemQuery = new FileSystemQuery();
        }

        #endregion Constructors

        #region Override Methods

        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            context.CaretPosition = input.Length;
            context.CaretPositionOnPopup = input.Length;
            return input;
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
                    string searchText = context.InputText; //context.InputText.Substring(context.CaretPositionOnPopup, (context.CaretPosition - context.CaretPositionOnPopup));
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

        public IFileSystemQuery FileSystemQuery { get; set; }
    }
}