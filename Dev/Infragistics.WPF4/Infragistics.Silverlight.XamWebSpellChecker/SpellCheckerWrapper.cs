using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.IO;
using Infragistics.Controls.Interactions;
using System.Windows;
using Infragistics.SpellChecker;

namespace Infragistics
{
    /// <summary>
    /// Provides the ability to spell-check a string.
    /// </summary>
    internal class SpellCheckerWrapper : ICheckerEngine
    {
        #region Members
        Infragistics.SpellChecker.SpellChecker _checker;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellChecker"/> class.
        /// </summary>
        public SpellCheckerWrapper()
        {
            this._checker = new Infragistics.SpellChecker.SpellChecker();
        }

        #endregion  // Constructor      

        #region IInternalCheckerEngine Members

        #region Properties

        #region Public

        #region DictionaryUri

        /// <summary>
        /// Gets or sets the Uri that the dictionary is at.
        /// </summary>
        public DictFile DictionaryUri
        {
            get { return this._checker.DictFile; }
            set { this._checker.DictFile = value; }
        }

        #endregion //DictionaryUri

        #region UserDictionary

        /// <summary>
        /// Sets the stream that the dictionary is at.
        /// </summary>
        public Stream UserDictionary
        {

            set { this._checker.SetUserDictionary(value); }
        }

        internal void WriteUserDictionary(string fileName)
        {
            this._checker.WriteUserDictionary(fileName);
        }
        #endregion //DictionaryUri

        /// <summary>
        /// Gets or sets a value indicating whether to allow words spelt with any case, e.g. "africa" instead of "Africa."  This is more relaxed than AllowMixedCase.
        /// </summary>
        public bool AllowAnyCase
        {
            get { return this._checker.AllowAnyCase; }
            set { this._checker.AllowAnyCase = value; }
        }

        /// <summary>
        /// Gets/Sets a value indicating whether to ignore words that start with capital letters.
        /// </summary> 
        public bool AllowCapitalizedWords
        {
            get { return this._checker.GetAllowCapitalizedWords(); }
            set { this._checker.SetAllowCapitalizedWords(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow suggestions for correctly spelled words with mix case.
        /// </summary>
        public bool AllowCaseInsensitiveSuggestions
        {
            get { return this._checker.AllowCaseInsensitiveSuggestions; }
            set { this._checker.AllowCaseInsensitiveSuggestions = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to allow words spelt with mixed case, e.g. "MIxEd."
        /// </summary>
        public bool AllowMixedCase
        {
            get { return this._checker.AllowMixedCase; }
            set { this._checker.AllowMixedCase = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore words with numbers in them (eg. A10 or 7-11).
        /// </summary>
        public bool AllowWordsWithDigits
        {
            get { return this._checker.AllowWordsWithDigits; }
            set { this._checker.AllowWordsWithDigits = value; }
        }

        /// <summary>
        /// Gets/Sets a value indicating whether to ignore XML tags in the text.
        /// </summary>
        public bool AllowXML
        {
            get { return this._checker.AllowXML; }
            set { this._checker.AllowXML = value; }
        }

        /// <summary>Gets or sets a value indicating whether to check for compound words, setting this to true is essential for languages such as German which 
        /// allow for compound words.</summary>
        /// <remarks>If this=false then Lookup("thesewordsarejoined") = false
        /// <p>If this=true then Lookup("thesewordsarejoined") = true</p>
        /// <p>If this=false then Lookup("abcdef") = false</p>
        /// <p>If this=true then Lookup("abcdef") = false</p>
        /// <p>Setting this to true will degrade lookup performance</p>
        /// </remarks>
        public bool CheckCompoundWords
        {
            get { return this._checker.CheckCompoundWords; }
            set { this._checker.CheckCompoundWords = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to 'look into' text with hyphens (-), if the word has hyphens in it and CheckHyphenatedText is set true (default), the parts of the text around the hyphens will be checked individually.
        /// </summary>
        /// <remarks>E.g. "socio-economic" will be checked as "socio" and "economic".</remarks>
        public bool CheckHyphenatedText
        {
            get { return this._checker.CheckHyphenatedText; }
            set { this._checker.CheckHyphenatedText = value; }
        }

        /// <summary>
        /// Gets/Sets the factor for how close words need to be to be suggested.
        /// </summary>
        public int ConsiderationRange
        {
            get { return this._checker.GetConsiderationRange(); }
            set { this._checker.SetConsiderationRange(value); }
        }      

        /// <summary>
        /// Gets/Sets a value indicating whether  the user dictionary should be used in finding suggestions for misspelt words.
        /// </summary>
        public bool IncludeUserDictionaryInSuggestions//(bool includeUserDictionaryInSuggestions)
        {
            get { return this._checker.GetIncludeUserDictionaryInSuggestions(); }
            set { this._checker.SetIncludeUserDictionaryInSuggestions(value); }
        }

        /// <summary>Gets or sets the type of language parsing to use.</summary>
        /// <remarks>E.g. If the dictionary is set to French, you should use the French parser.</remarks>        
        public LanguageType LanguageParser
        {
            get { return this._checker.LanguageParser; }
            set { this._checker.LanguageParser = value; }
        }

        /// <summary>
        /// Gets/Sets whether to separate words with hyphens in them into separate words.
        /// </summary>
        public bool SeparateHyphenWords
        {
            get { return this._checker.GetSeparateHyphenWords(); }
            set { this._checker.SetSeparateHyphenWords(value); }
        }

        /// <summary>
        /// Gets or sets the minimum size of each word in order for it to become a suggestion.  For example, "myhouse" has two words "my" and "house" in order for "my house" to be a suggestion, the SplitWordThreshold must be set to 2 or smaller. A lower threshold results in worse performance.
        /// </summary>
        public int SplitWordThreshold
        {
            get { return this._checker.SplitWordThreshold; }
            set { this._checker.SplitWordThreshold = value; }
        }

        /// <summary>
        /// Gets/Sets the suggestions should be done using hashing or phonetically.
        /// </summary>
        public SuggestionsMethod SuggestionsMethod
        {
            get { return this._checker.GetSuggestionsMethod(); }
            set { this._checker.SetSuggestionsMethod(value); }
        }

        /// <summary>Gets or sets a value indicating whether to look for connected words in suggestions.</summary>
        /// <remarks>if this=true then suggestions may include joined words.
        /// <p>Eg; suggestions for "myhouse" will include "my house"</p>
        /// </remarks>
        public bool SuggestSplitWords
        {
            get { return this._checker.SuggestSplitWords; }
            set { this._checker.SuggestSplitWords = value; }
        }

        #endregion //Public

        #endregion

        #region Methods

        #region Public
        /// <summary>Checks the text for errors.</summary>
        /// <param name="text">The text that is spellchecked.</param>
        /// <exception cref="ArgumentNullException">if text parameter is null.</exception>
        public void Check(string text)
        {
            this._checker.Check(text);
        }       

        

        /// <summary>Gets the next bad word in the list that was identified by Check.</summary>
        /// <remarks>Check must be called before this method.</remarks>
		/// <returns>The next BadWord object from the text.  Only one instance of the BadWord class is used.  
        /// This must return a empty string "" as the word when no more bad words exist.</returns>        
		public BadWord NextBadWord()
        {
           

            BadWord badword = this._checker.NextBadWord();

            
            return badword;
        }


        /// <summary>Returns an List of Strings that are suitable suggestions for the current bad word (that is, the one last returned by NextBadWord()).</summary>
        /// <remarks>If no suggestions can be found, this should return an empty List.</remarks>
        /// <exception cref="NoCurrentBadWordException">If NextBadWord() hasn't been run first AND found an erroneous word.</exception>
        /// <returns>A list of the suggested words.</returns>
        public IList<string> FindSuggestions()
        {
            return this._checker.FindSuggestions();
        }

        
        /// <summary>Adds a word to the user dictionary, if it exists.</summary>
        /// <param name="word">The word that is added to the user dictionary.</param>
        /// <remarks>Should return true if the word was added successfully, false otherwise.</remarks>
        /// <returns>Returns true if the word was added.</returns>
        public bool AddWord(string word)
        {
            return this._checker.AddWord(word);
        }

        /// <summary>
        /// Removes a word from the user dictionary, if it exists.
        /// </summary>
        /// <param name="word">The word to remove from the user dictionary.</param>
        /// <returns>Returns true if the word was successfully removed, false otherwise</returns>
        public bool RemoveWord(string word)
        {
            return this._checker.RemoveWord(word);
        }
        
        /// <summary>Sets the pointer position for the <c>NextBadWord</c> iterator.</summary>
        /// <remarks>
        /// Further calls to <c>NextBadWord</c> will look for the next bad word from position <c>pos</c>.
        /// If position &gt; the text length is set to the text length.
        /// If position &lt; 0, it is set to zero.
        /// </remarks> 
        /// <param name="pos">The position that it will start looking for bad words from.</param>
        public void SetPosition(int pos)
        {
            this._checker.SetPosition(pos);
        }
        
               
#endregion //Public

        #endregion //Methods

        #endregion
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