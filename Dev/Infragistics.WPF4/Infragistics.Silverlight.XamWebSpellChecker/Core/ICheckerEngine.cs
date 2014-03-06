using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Infragistics.Controls.Interactions;

namespace Infragistics.SpellChecker
{
	///<summary>Defines the interface required for a class to be used as a checker engine</summary>
	///<remarks>This is the interface which any checker engine must have, by implementing this interface any object can be
	///used to parse text and identify 'bad words'.
	///It is not necessary to functionally implement all members, for this reason an abstract empty implementation class is provided (CheckerEngineAdapter) which can be extended and overridden for desired functionality.
	///</remarks>
	internal interface IInternalCheckerEngine{
		///<summary>Checks the text for errors.</summary>
		///<exception cref="ArgumentNullException">if text parameter is null.</exception>
		void Check(String text);

        /// <summary>
        /// Checks the text for errors starting at a givne position.
        /// </summary>
        /// <param name="text">The text to spellcheck</param>
        /// <param name="startPosition">The position to start spell checking.</param>
        /// <exception cref="ArgumentNullException">if text parameter is null.</exception>
        void Check(String text, int startPosition);


        /// <summary>
        /// Looks up a <c>word</c> in the dictionary and returns true if it is in there.
        /// </summary>
		bool LookUp(String word);

        /// <summary>Gets the next bad word in the list that was identified by Check.</summary>
        /// <remarks>Check must be called before this method.</remarks>
        /// <returns>The next BadWord object from the text.  This must return null (C#) or nothing (VB.NET) when no more bad words exist.</returns>
		BadWord NextBadWord();

		
		///<summary>The AdvancedTextBoundary class to use to parse words.</summary>
		AdvancedTextBoundary TextBoundary{get; set;}


        /// <summary>Returns an ArrayList of Strings that are suitable suggestions for the current bad word (that is, the one last returned by NextBadWord()).</summary>
        /// <remarks>If no suggestions can be found, this should return an empty ArrayList.  This method should be thread safe if operating with RapidSpell Desktop.</remarks>
        /// <exception cref="NoCurrentBadWordException">If NextBadWord() hasn't been run first AND found an erroneous word.</exception>
        List<String> FindSuggestions();
		
		///<summary>Sets whether to include the user dictionary in suggestions taken from this engine.</summary>
		void SetIncludeUserDictionaryInSuggestions(bool includeUserDictionaryInSuggestions);

		///<summary>Sets the UserDictionary object to be used as a user dictionary source.</summary>
		void SetUserDictionary(UserDictionary userDictionary);

		///<summary>Sets the UserDictionary file path to be used as a user dictionary source.</summary>
		void SetUserDictionary(Stream userDictionary);


		///<summary>Adds a word to the user dictionary, if it exists.</summary>
		///<remarks>Should return true if the word was added successfully, false otherwise.</remarks>
		bool AddWord(String word);

		/// <summary>
		/// Removes a word from the user dictionary, if it exists.
		/// </summary>
		/// <remarks>Should return true if the word was successfully removed, false otherise.</remarks>
		bool RemoveWord( string word );

		///<summary>Sets the suggestions method, where method is an integer identifier.</summary>
		void SetSuggestionsMethod(SuggestionsMethod suggestionsMethod);

		///<summary>Sets whether to ignore capitalized words.</summary>
		void SetAllowCapitalizedWords(bool ignore);

		///<summary>Whether to ignore XML tags in the text.</summary>
		bool AllowXML{
			get;
			set;
		}

		///<summary>Sets the breadth of the suggestions search.</summary>
		void SetConsiderationRange(int range);

		///<summary>Sets whether to separate words with hyphens in them into separate words.</summary>
		void SetSeparateHyphenWords(bool separate);

		///<summary>The user dictionary, this should be null (C#) or nothing (VB.NET) if no user dictionary is being used.</summary>
		UserDictionary userDictionary{
			get;
		}

        /// <summary>
        /// The Dictionary to be used.  If none is specified, then the English Dictionary will be used.
        /// </summary>
		DictFile DictFile
		{
			get;
			set;
		}


        /// <summary>The type of language parsing to use.</summary>
        /// <remarks>E.g. If the dictionary is set to French, you should use the French parser.</remarks>
        /// <returns>This implementation always returns ENGLISH.</returns>
		LanguageType LanguageParser{
			get;
			set;
		}

        /// <summary>Sets the pointer position for the <c>NextBadWord</c> iterator.</summary>
        /// <remarks>
        /// Further calls to <c>NextBadWord</c> will look for the next bad word from position <c>pos</c>.
        /// If position &gt; the text length is set to the text length.
        /// If position &lt; 0, it is set to zero.
        /// </remarks>
		void SetPosition(int pos);

        /// <summary>
        /// Gets whether the user dictionary should be used in finding suggestions for misspelt words.
        /// </summary>
		bool GetIncludeUserDictionaryInSuggestions();

        /// <summary>
        /// Gets whether to ignore words that start with capital letters.
        /// </summary>
		bool GetAllowCapitalizedWords();

        /// <summary>
        /// Gets the factor for words to consider for suggestions.
        /// </summary>
		int GetConsiderationRange();

		/// <summary>Changes the current bad word to <c>newWord</c> in the text.</summary>
		/// <param name="newWord">replaces the current misspelt word.</param>
		/// <returns>void</returns>
		/// <exception cref="NoCurrentBadWordException">if nextBadWord() hasn't been run first AND found an erroneous word.</exception>
		/// <exception cref="ArgumentNullException">if newWord parameter is null.</exception>
		void ChangeBadWord(String newWord);


        /// <summary>
        /// Marks <c>word</c> to be ignored in the rest of the text.
        /// </summary>
        /// <param name="word">to ignore</param>
        /// <returns>void</returns>
        /// <exception cref="ArgumentNullException">if word parameter is null.</exception>
		void IgnoreAll(String word);

        /// <summary>
        /// Whether to 'look into' text with hyphens (-), if the word has hyphens in it and CheckHyphenatedText is set true (default), the parts of the text around the hyphens will be checked individually.
        /// </summary>
        /// <remarks>E.g. "socio-economic" will be checked as "socio" and "economic".</remarks>
		bool CheckHyphenatedText{
			get;
			set;
		}


		///<summary>Whether to check for compound words, setting this to true is essential for languages such as German which 
		///allow for compound words.</summary>
		///<remarks>If this=false then Lookup("thesewordsarejoined") = false
		///<p>If this=true then Lookup("thesewordsarejoined") = true</p>
		///<p>If this=false then Lookup("abcdef") = false</p>
		///<p>If this=true then Lookup("abcdef") = false</p>
		///<p>Setting this to true will degrade lookup performance</p>
		///</remarks>
		bool CheckCompoundWords{
			get;
			set;
		}


		///<summary>Whether to look for connected words in suggestions.</summary>
		///<remarks>if this=true then suggestions may include joined words.
		///<p>Eg; suggestions for "myhouse" will include "my house"</p>
		///</remarks>
		bool SuggestSplitWords{
			get;
			set;
		}


		/// <summary>Whether to ignore words with numbers in them (eg. A10 or 7-11).</summary>
		bool AllowWordsWithDigits{
			get;
			set;
		}

		///<summary>Gets the current bad word (last returned by NextBadWord).</summary>
		BadWord CurrentBadWord{
			get;
		}


        /// <summary>
        /// Whether to allow words spelt with mixed case, e.g. "MIxEd."
        /// </summary>
		bool AllowMixedCase{
			get;
			set;
		}

		///<summary>Whether to allow suggestions for correctly spelled words with mix case.</summary>
		bool AllowCaseInsensitiveSuggestions
		{
			get;
			set;
		}

        /// <summary>
        /// Whether to allow words spelt with any case, e.g. "africa" instead of "Africa."  This is more relaxed than AllowMixedCase.
        /// </summary>
		bool AllowAnyCase{
			get;
			set;
		}


        /// <summary>
        /// The property coincides with SuggestSplitWords.  The value of the property determines the minimum size of each word in order for it to become a suggestion.  For example, "myhouse" has two words "my" and "house" in order for "my house" to be a suggestion, the SplitWordThreshold must be set to 2 or smaller. A lower threshold results in worse performance.
        /// </summary>
		int SplitWordThreshold
		{
			get;
			set;
		}


        
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