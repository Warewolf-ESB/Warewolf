using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.Intellisense.Provider;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.InterfaceImplementors
{
    public class DateTimeIntellisenseProvider : IIntellisenseProvider
    {
        #region Class Members

        private List<IIntellisenseResult>  _intellisenseResults;

        #endregion Class Members

        #region Constructors

        public DateTimeIntellisenseProvider()
        {
            Optional = false;

            IDateTimeParser dateTimeParser = DateTimeConverterFactory.CreateParser();
            _intellisenseResults = dateTimeParser.DateTimeFormatParts.Select(p => 
                {
                    IIntellisenseResult intellisenseResult = IntellisenseFactory.CreateDateTimeResult(IntellisenseFactory.CreateDateTimePart(p.Value, p.Description));
                    return intellisenseResult;
                }).OrderBy(p => p.Option.DisplayValue).ToList();
        }

        public DateTimeIntellisenseProvider(List<IIntellisenseResult> intellisenseResults)
        {
            Optional = false;
            _intellisenseResults = intellisenseResults;
        }

        #endregion Constructors

        #region Override Methods
        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            throw new NotSupportedException();
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            IList<IIntellisenseResult> oldResults = GetIntellisenseResultsImpl(context);
            var results = new List<IntellisenseProviderResult>();

            if (oldResults != null)
            {
                foreach (IIntellisenseResult currentResult in oldResults)
                {
                    if (currentResult.ErrorCode != enIntellisenseErrorCode.None)
                    {
                        if (currentResult.Type == enIntellisenseResultType.Error && currentResult.IsClosedRegion)
                        {
                            results.Add(new IntellisenseProviderResult(this, currentResult.Option.DisplayValue, currentResult.Message, currentResult.Message, true));
                        }
                    }

                    if (currentResult.Type == enIntellisenseResultType.Selectable)
                    {
                        results.Add(new IntellisenseProviderResult(this, currentResult.Option.DisplayValue, currentResult.Option.Description, currentResult.Option.Description, false));
                    }
                }
            }

            return results;
        }

        public IList<IIntellisenseResult> GetIntellisenseResultsImpl(IntellisenseProviderContext context)
        {
            string searchText = context.FindTextToSearch();
            var results = new List<IIntellisenseResult>();

            if (context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
            {
                results.AddRange(IntellisenseResults);
            }
            else if (!InLiteralRegion(context.InputText, context.CaretPosition))
            {
                var filteredResults = IntellisenseResults.Where(i => i.Option.DisplayValue.ToLower().StartsWith(searchText.ToLower()));
                results.AddRange(filteredResults);
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
        /// Determines if the caret in a literal region
        /// </summary>
        public static bool InLiteralRegion(string inputText, int caretPosition)
        {
            bool inLiteralRegion = false;

            string text = inputText;
            if (caretPosition <= text.Length)
            {
                text = text.Replace("\\\\", "##").Replace("\\'", "##").Replace("'''", "###");

                inLiteralRegion = text.Substring(0, caretPosition).Count(s => s == '\'') % 2 != 0;
            }

            return inLiteralRegion;
        }

        #endregion Private Methods

        #region Properties

        public bool Optional { get; set; }
        public bool HandlesResultInsertion { get; set; }

        public List<IIntellisenseResult> IntellisenseResults
        {
            get { return _intellisenseResults; }
        }

        #endregion Properties
    }
}
