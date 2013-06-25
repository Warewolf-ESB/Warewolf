using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dev2.DataList.Contract;

namespace Dev2.Studio.InterfaceImplementors
{
    public class DateTimeIntellisenseProvider : IIntellisenseProvider
    {
        #region Class Members

        List<IIntellisenseResult> _intellisenseResults;

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

        #endregion Constructors

        #region Override Methods
        public string PerformResultInsertion(string input, IntellisenseProviderContext context)
        {
            throw new NotSupportedException();
        }

        public IList<IntellisenseProviderResult> GetIntellisenseResults(IntellisenseProviderContext context)
        {
            IList<IIntellisenseResult> oldResults = GetIntellisenseResultsImpl(context);
            List<IntellisenseProviderResult> results = new List<IntellisenseProviderResult>();

            if (oldResults != null)
            {
                for (int i = 0; i < oldResults.Count; i++)
                {
                    IIntellisenseResult currentResult = oldResults[i];

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

        private IList<IIntellisenseResult> GetIntellisenseResultsImpl(IntellisenseProviderContext context)
        {
            List<IIntellisenseResult> results = new List<IIntellisenseResult>();

            if (context.DesiredResultSet == IntellisenseDesiredResultSet.EntireSet)
            {
                results.AddRange(_intellisenseResults);
            }
            else if (!InLiteralRegion(context.InputText, context.CaretPosition))
            {
                if (context.CaretPosition > context.CaretPositionOnPopup)
                {
                    string searchText = context.InputText.Substring(context.CaretPositionOnPopup, (context.CaretPosition - context.CaretPositionOnPopup));
                    var filteredResults = _intellisenseResults.Where(i =>
                        {
                            bool forwardMatch = i.Option.DisplayValue.ToLower().StartsWith(searchText.ToLower()) && searchText != i.Option.DisplayValue;

                            bool backwardMatch = false;
                            int index = i.Option.DisplayValue.IndexOf(searchText.ToLower());
                            if (index >= 0)
                            {
                                int startIndex = (context.CaretPositionOnPopup - index);
                                int length = (context.CaretPosition - startIndex);
                                
                                if (startIndex >= 0 && (startIndex + length) <=context.InputText.Length)
                                {
                                    string tmp = context.InputText.Substring(startIndex, length);
                                    backwardMatch = i.Option.DisplayValue.ToLower().StartsWith(tmp.ToLower()) && tmp != i.Option.DisplayValue;
                                }
                            }

                            return forwardMatch || backwardMatch;
                        });

                    results.AddRange(filteredResults);
                }
                else if (context.CaretPositionOnPopup >= context.CaretPosition && context.CaretPosition > 0)
                {
                    string searchText = context.InputText.Substring(context.CaretPosition - 1, 1);
                    var filteredResults = _intellisenseResults.Where(i =>
                    {
                        bool backwardMatch = false;
                        int index = i.Option.DisplayValue.ToLower().IndexOf(searchText);
                        if (index >= 0)
                        {
                            int startIndex = context.CaretPosition - index - 1;
                            int length = context.CaretPosition - startIndex;

                            if (startIndex >= 0 && (startIndex + length) <= context.InputText.Length)
                            {
                                string tmp = context.InputText.Substring(startIndex, length);
                                backwardMatch = i.Option.DisplayValue.ToLower().StartsWith(tmp.ToLower()) && tmp != i.Option.DisplayValue;
                            }
                        }

                        return backwardMatch;
                    });

                    results.AddRange(filteredResults);                   
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
        /// Determines if the caret in a literal region
        /// </summary>
        private bool InLiteralRegion(string inputText, int caretPosition)
        {
            bool inLiteralResionRegion = false;

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
    }
}
