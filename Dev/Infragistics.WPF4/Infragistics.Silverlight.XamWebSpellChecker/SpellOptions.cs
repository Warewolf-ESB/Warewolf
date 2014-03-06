using System.Windows;

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// The <see cref="SpellOptions"/> class lists properties which determine how the spell check operation will proceed and react to issues found 
    /// in the text.
    /// </summary>
    public class SpellOptions : DependencyObject
    {
        #region Members
        
        private ICheckerEngine _checker;
        private bool _allowAnyCase;
        private bool _allowCaseInsensitiveSuggestions;
        private bool _allowMixedCase;
        private bool _allowWordsWithDigits;
        private bool _allowXML;
        private bool _checkHyphenatedText;
        private bool _includeUserDictionaryInSuggestions;
        private LanguageType _languageParser;
        private bool _separateHyphenWords;
        private SuggestionsMethod _suggestionsMethod;

        #endregion //Members        

        #region Properties
        
        #region Public

        #region AllowAnyCase

        /// <summary>
        ///  Gets/Sets whether to allow words spelt with any case, e.g. "africa" instead of "Africa."  This is more relaxed than AllowMixedCase.
        /// </summary>
        public bool AllowAnyCase
        {
            get { return this._allowAnyCase; }
            set
            {
                this._allowAnyCase = value;
                if(Checker!=null)
                {
                    Checker.AllowAnyCase=value;
                }
            }
        }

        #endregion // AllowAnyCase 
          
        #region AllowCaseInsensitiveSuggestions
        /// <summary>
        /// Gets/Sets whether to allow suggestions for correctly spelled words with mixed case. 
        /// </summary>
        /// <remarks>   
        /// For example: if AllowMixedCase is false and the word "tHE" is passed to the WebSpellChecker, 
        /// when set to "True" you will recieve suggestions, when set to "False" you will not. 
        /// </remarks>
        public bool AllowCaseInsensitiveSuggestions
        {
            get { return this._allowCaseInsensitiveSuggestions; }
            set
            {
                this._allowCaseInsensitiveSuggestions = value;
                if (Checker != null)
                {
                    Checker.AllowCaseInsensitiveSuggestions = value;
                }

            }
        }

        #endregion // AllowAnyCase 

        #region AllowMixedCase
        /// <summary>
        /// Gets/Sets whether to allow words spelt with mixed case, e.g. "MIxEd." 
        /// </summary>
        public bool AllowMixedCase
        {
            get { return this._allowMixedCase; }
            set
            {
                this._allowMixedCase = value;
                if (Checker != null)
                {
                    Checker.AllowMixedCase = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #region AllowWordsWithDigits
        /// <summary>
        /// Gets/Sets whether to Allow words with numbers in them (eg. A10 or 7-11). 
        /// </summary>
        public bool AllowWordsWithDigits
        {
            get { return this._allowWordsWithDigits; }
            set
            {
                this._allowWordsWithDigits = value;
                if (Checker != null)
                {
                    Checker.AllowWordsWithDigits = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #region AllowXML
        /// <summary>
        /// Gets/Sets whether to Allow XML/HTML tags, false by default. 
        /// </summary>
        /// <remarks>
        /// This identifies XML tags as anything in between &lt; and &gt; characters.  Therefore it will 
        /// cause undesirable effects in text such as &amp;quot; the sign for less than is &lt; and the 
        /// sign for greater than is &gt; &amp;quot;.  Of course in HTML that should have been written as 
        /// &amp;quot; the sign for less than is &amp;lt; and the sign for greater than is &amp;gt; &amp;quot;;
        ///  anyway, which would be fine.
        /// </remarks>
        public bool AllowXml
        {
            get { return this._allowXML; }
            set
            {
                this._allowXML = value;
                if (Checker != null)
                {
                    Checker.AllowXML = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #region CheckHyphenatedText
        /// <summary>
        /// Gets/Sets whether to check text with hyphenate.
        /// </summary>
        /// <remarks>
        /// When set to "True" the hyphenated text is checked as separate words. When set to "False" the 
        /// hyphenated text is checked as one word. If that word is in the dictionary it will be marked as 
        /// correct. True: The word "after-effect" would be treated as "after" and "effect". False: The word
        /// "after-effect" would be treated as "after-effect". For both "true" and "false", if misspelled 
        /// the whole word will be marked as misspelled. 
        /// </remarks>
        public bool CheckHyphenatedText
        {
            get { return this._checkHyphenatedText; }
            set
            {
                this._checkHyphenatedText = value;
                if (Checker != null)
                {
                    Checker.CheckHyphenatedText = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #region IncludeUserDictionaryInSuggestions
        /// <summary>
        /// Gets/Sets whether the user dictionary should be used in finding suggestions for misspelt words.
        /// </summary>
        public bool IncludeUserDictionaryInSuggestions
        {
            get { return this._includeUserDictionaryInSuggestions; }
            set
            {
                this._includeUserDictionaryInSuggestions = value;
                if (Checker != null)
                {
                    Checker.IncludeUserDictionaryInSuggestions = value;
                }
            }
        }

        #endregion // AllowAnyCase 
    
        #region LanguageParser
        /// <summary>
        /// Gets/Sets the language parser to use. 
        /// </summary>
        /// <remarks>
        /// This should match the language of the main dictionary where possible. 
        /// This is used to give hints to the parser about how punctuation is used (e.g. apostrophe is used differently in English and French).
        /// </remarks>
        public LanguageType LanguageParser
        {
            get { return this._languageParser; }
            set
            {
                this._languageParser = value;
                if (Checker != null)
                {
                    Checker.LanguageParser = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #region SeparateHyphenWords
        /// <summary>
        /// Gets/Sets whether the hyphenated text is treated as separate words. 
        /// </summary>
        /// <remarks>
        /// When set to "True" the hyphenated text is treated as separate words. For e.g. if this is true 
        /// text like "cheap-deals" will be treated as two words "cheap" and "deals". If one of the words
        /// was misspelled only that word would be marked as misspelled. 
        /// </remarks>
        public bool SeparateHyphenWords
        {
            get { return this._separateHyphenWords; }
            set
            {
                this._separateHyphenWords = value;
                if (Checker != null)
                {
                    Checker.SeparateHyphenWords = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #region SuggestionsMethod
        /// <summary>
        /// Gets/Sets the method by which suggestions are determined. 
        /// </summary>
        public SuggestionsMethod SuggestionsMethod
        {
            get { return this._suggestionsMethod; }
            set
            {
                this._suggestionsMethod = value;
                if (Checker != null)
                {
                    Checker.SuggestionsMethod = value;
                }
            }
        }

        #endregion // AllowAnyCase 

        #endregion //Public

        #region Internal
        internal ICheckerEngine Checker
        {
            get { return _checker; }
            set
            {
                if (value == null)
                    return;
                _checker = value;
                _checker.AllowAnyCase = this.AllowAnyCase;
                _checker.AllowCaseInsensitiveSuggestions = this.AllowCaseInsensitiveSuggestions;
                _checker.AllowMixedCase = this.AllowMixedCase;
                _checker.AllowWordsWithDigits = this.AllowWordsWithDigits;
                _checker.AllowXML = this.AllowXml;
                _checker.CheckHyphenatedText = this.CheckHyphenatedText;
                _checker.IncludeUserDictionaryInSuggestions = this.IncludeUserDictionaryInSuggestions;
                _checker.LanguageParser = this.LanguageParser;
                _checker.SeparateHyphenWords = this.SeparateHyphenWords;
                _checker.SuggestionsMethod = this.SuggestionsMethod;
            }
        }
        #endregion //Internal

        #endregion //Properties
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved