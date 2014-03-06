using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Controls.Interactions;

namespace Infragistics.SpellChecker
{

	#region "API Docs"
	 /// <summary>Spell checker engine, checks a text string for spelling errors and makes suggested corrections.</summary>
	 /// <remarks>  This is a non-GUI component, suitable for the business/logic layer of your application.
	 ///   You would use this component in web server based applications.
	 ///   This component can be used in 2 different ways.  The most common usage, in an iterative fashion is detailed below, but
	 /// it may also be used on a query by query basis.
	 ///   To use this component in an iterative way you must first call <c>Check</c> to set the text to check and 
	 /// then iterate through 
	 /// <c>NextBadWord()</c> to identify misspelt words.  <c>NextBadWord</c> will return
	 /// a BadWord object and internally locate the
	 /// the misspelt word in the text, then you can call <c>FindSuggestions</c>
	 /// which will return a ArrayList of suggestions as String objects.  You may also call <c>ChangeBadWord(replacement)</c>
	 /// to replace that current misspelt word in the text.
	 ///  <c>NextBadWord()</c> will return null when the text has been checked. <c>GetAmendedText</c> returns the current
	 /// state of the text with corrections.
	 ///  
	 /// Calling <c>ChangeBadWord</c> or <c>FindSuggestions</c> before <c>NextBadWord</c> has been called
	 /// and returned a BadWord (i.e. not null) will result in a <see cref="NoCurrentBadWordException"/>.
	 ///</remarks>
	 /// <example>
	 ///   Check some text, find suggestions and replace mis-spelt words with "replacement". 
	 ///	
	 ///  <code>
	 ///	webSpellChecker c = new webSpellChecker(); 
	 ///	BadWord badWord; 
	 ///	ArrayList suggestions; 
	 ///
	 ///	 //check some text.  
	 ///	c.Check("This is sume text."); 
	 /// 
	 ///	//iterate through all bad words in the text.  
	 ///	while((badWord = c.NextBadWord())!=null){ 
	 /// 
	 ///		Console.WriteLine(badWord.GetWord() + "- is not spelt correctly. Suggestions:"); 
	 /// 
	 ///		try{ 
	 ///			//get suggestions for the current bad word.  
	 ///			suggestions = c.FindSuggestions(); 
	 /// 
	 ///			//display all suggestions.  
	 ///			for(int i=0; i&lt;suggestions.Count; i++){
	 ///				Console.WriteLine(suggestions[i]);
	 ///			}
	 /// 
	 ///			//change the bad word in the text with "replacement".  
	 ///			c.ChangeBadWord("replacement"); 
	 /// 
	 ///		} catch (NoCurrentBadWordException e){ 
	 ///			Console.WriteLine(e); 
	 ///		} 
	 /// 
	 ///	} 
	 ///	Console.WriteLine(c.GetAmendedText());  
	 ///	</code>
	 ///   </example>
	 ///
	 ///<remarks><para>
	 /// The second way to use this component is simply to query words yourself using <c>Lookup(word)</c> to check if <c>word</c>
	 /// is in the lexicon/dictionary and <c>FindSuggestions(word)</c> to find spelling suggestions for <c>word</c>.
	 ///</para>
	 ///</remarks>
	#endregion
	


	internal class SpellChecker : IInternalCheckerEngine{

		#region "Fields"

		// pointers to current word being parsed
		///<summary>The start index of the current word being checked/parsed.</summary>
		protected int wordStart; 
		///<summary>The end index of the current word being checked/parsed.</summary>
		protected int wordEnd;

		
		internal int WordStart{ get{ return wordStart; } }
		internal int WordEnd{ get{ return wordEnd; } }

		/// ArrayList of words to be ignored.
		private List<String> ignoreList;


		/// ArrayList of mis-spelt words in the text.
        private List<String> badWordList;
		

		/// a copy of the text being checked.
		private StringBuilder theText;

		/// word iterator
		private AdvancedTextBoundary wordIterator = new AdvancedTextBoundary();

        private List<String> mainDictionary = new List<String>(100000);
        private List<String> mainDicListPhonetic;
 
        // MD 11/13/07 - BR27454
        // These's no reason to keep a separate copied list of the words if we have a 
        // reference to the user dictionary
        ///// user dictionary word list
        //private List<String> userDicList = new List<String>(5000);

		private int splitWordThreshold = 3; 

		#region Swears
		/// <summary>Don't suggest words in this array (swear words).</summary>
		protected String[] dontSuggest = new String[] {"fart", "farted", "farting", "farts", "fart's", "fuck", "fucker's", "fuck's", "fuck-all", "fucked", "fucker", "fuckers", "fucking", "fucks", "fucked","fucked-up", "shit's", "shit", "shits", "shitted", "shitting", "piss", "pissed", "pisses", "pissing", "wank", "wanked", "wanking", "wanks", "arse", "arsehole", "arseholes", "arses", "ass-head", "asshole's", "asshole", "assholes", "ass's", "orgy", "orgies","slut", "shithead", "shithole", "fucker", "fucking", "mother-fucker", "motherfucker", "cock-sucker", "cocksucker", "cunt", "clit", "clits", "cocksuckers", "cock-suckers", "cunts", "shitheads", "twat", "twats", "twatsucker", "twatface"};	
		#endregion

        // MD 11/13/07
        // Found while fixing BR27454
        // These are never set to true, we don't need them
        ////whether to share the dic
        //bool shareDictionary = false;
        //
        //#if DESKTOP
        //bool findCapitalizedSuggestions = false;
        //#else
        //internal bool findCapitalizedSuggestions = false;
        //#endif

		///  <summary>The UserDictionary being used.</summary>
		UserDictionary _userDictionary;
		
		

        // MD 11/13/07
        // Found while fixing BR27454
        // These lists are never used
        ////singleton copy of the dictionary, used when shareDictionary =true
        //static ArrayList sharedMainDictionary=null;
        //static ArrayList sharedReverseDictionary=null;

		// # of words in list
		private int numWords=0;

        //culture invariant comparer for sorts and searches
        // MD 11/13/07
        // Found while fixing BR27454
        // Don't store an instance of this with each spell checker.
        // Moved this to be a static readonly field on the comparer itself
        //InvariantComparer invariantComparer = new InvariantComparer();


		
		private CompareL compare ;


		/// <summary>Indicator for optimization (OPTIMIZE_FOR_SPEED only works with PhoneticSuggestions method).</summary>
		private static int OPTIMIZE_FOR_SPEED = 1, OPTIMIZE_FOR_MEMORY = 2;
		private int optimization = OPTIMIZE_FOR_SPEED;

		//MD 5/25/06
		// Changed values of SuggestionsMethod to follow our naming convention
		private SuggestionsMethod suggestionsMethod = SuggestionsMethod.HashingSuggestions;

		
		bool ignoreRtfCodes = false;

		//whether to allow words spelt with mixed case, eg. 'MIxEd'
		bool allowMixedCase = false;

		//Whether to allow suggestions for correctly spelled words with mix case.
		bool allowCaseInsensitiveSuggestions = true;
		//whether to allow words spelt with incorrect case, eg. 'canada'
		bool allowAnyCase = false;	
		//whether to ignore words with digits in them
		bool allowWordsWithDigits = true;

		//the bad word currently being iterated on
		BadWord currentBadWord = null;


		String loadedQryWrd = "";

		//whether to find suggestions from the user dictionary aswell (slight speed penalty)
		private bool includeUserDictionaryInSuggestions = false;

		//whether to ignore words that are CAPITALIZED (all-caps)
		private bool allowCapitalizedWords = false;

		//whether to allow words that are hyphenated when all constituant parts are valid words
		private bool checkHyphenatedText = true;



		//whether a dictionary is loaded
		bool dictionaryIsLoaded = false;

		//the reverse sorted dictionary list
        private List<String> reverseSortedDictionary = null;

		//the consideration range for suggestions
		int radius= 80;

		//used to decode the wordlist from the dictionary
		private ResourceDecoder resourceDecoder;
		

		//whether to check for compound substrings
		bool checkCompoundWords = false;

		//look for connected words in suggestions
		bool suggestSplitWords = true;

		
		//the previous word the was returned by GetNextWord
		string previousWord = "";

		//the language parser to use
		LanguageType languageParser = LanguageType.English;

		///used by GUI to override the change word disability
		internal bool overrideEval = false;
			#endregion

		#region "Events"

		/// <summary>Change word event, fired when a word is changed.</summary>
        internal event ChangeWordEventHandler ChangeWord;
		/// <summary>Change word event handler delegate.</summary>
		internal delegate void ChangeWordEventHandler(object sender, ChangeWordEventArgs e);

		#endregion

		#region "Properties"

        /// <summary>
        /// The property coincides with SuggestSplitWords.  The value of the property determines the minimum size of each word in order for it to become a suggestion.  For example, "myhouse" has two words, "my" and "house."  In order for "my house" to be a suggestion, the SplitWordThreshold must be set to 2 or smaller.  A lower threshold results in worse performance.
        /// </summary>
		public int SplitWordThreshold
		{
			get{return splitWordThreshold;}
			set{splitWordThreshold = value;}
		}

		///<summary>The AdvancedTextBoundary class to use to parse words.</summary>
		public AdvancedTextBoundary TextBoundary
		{
			get{ return wordIterator; }
			set{ wordIterator = value;}
		}

        /// <summary>
        /// Whether to check for compound words.  Setting this to true is essential for languages such as German which allow for compound words.
        /// </summary>
        /// <remarks>
        /// If this=false then Lookup("thesewordsarejoined") = false
        /// <p>If this=true then Lookup("thesewordsarejoined") = true</p>
        /// 	<p>If this=false then Lookup("abcdef") = false</p>
        /// 	<p>If this=true then Lookup("abcdef") = false</p>
        /// 	<p>Setting this to true will degrade lookup performance by a maximum of approximately 100[1-1/n] percent (where n is the number of letters in the word) for misspelt words.  There is no preformance degredation for correctly spelt words.  On average for texts with mostly correctly spelt words the performance degredation for the Check method is roughly 25%.</p>
        /// </remarks>
		public bool CheckCompoundWords
		{
			get { return checkCompoundWords;  }
			set { checkCompoundWords = value; }
		}


        /// <summary>Whether to look for connected words in suggestions.</summary>
        /// <remarks>
        /// If this=true then suggestions may include joined words.
        /// <p>E.g. suggestions for "myhouse" will include "my house"</p>
        /// Setting this to true (default) has a slight performance impact.
        /// </remarks>
		public bool SuggestSplitWords
		{
			get{ return suggestSplitWords; }
			set{ suggestSplitWords = value; }
		}

        /// <summary>
        /// Whether to ignore XML/HTML tags.  This should be set to true for RichTextBox support, but is false by default.
        /// </summary>
		public bool AllowXML
		{
			get{ return wordIterator.AllowXML; }
			set{ wordIterator.AllowXML = value;}
		}

        /// <summary>
        /// Whether to 'look into' text with hyphens (-).  If the word has hyphens in it and CheckHyphenatedText is set to true (default), the parts of the text around the hyphens will be checked individually.
        /// </summary>
        /// <remarks>E.g. "socio-economic" will be checked as "socio" and "economic".</remarks>
		public bool CheckHyphenatedText
		{
			get{ return checkHyphenatedText; }
			set{ checkHyphenatedText = value;}
		}

        /// <summary>The type of language parsing to use.</summary>
        /// <remarks>E.g. if the dictionary is set to French, you should use the French parser.</remarks>
		public LanguageType LanguageParser
		{
			get 
			{ 
				return languageParser;
			}
			set 
			{ 
				if( value == LanguageType.English ) 
				{
					wordIterator.LanguageParsing = SimpleTextBoundary.ENGLISH ;
				}
				if( value == LanguageType.French ) 
				{
					wordIterator.LanguageParsing = SimpleTextBoundary.FRENCH ;
				}
				if( value == LanguageType.Dutch ) 
				{
					wordIterator.LanguageParsing = SimpleTextBoundary.FRENCH ;
				}
				languageParser = value;
			}
		}

        // MD 11/13/07
        // Found while fixing BR27454
        // This is never set to true, we don't need it
        /////<summary>
        ///// Whether to share the dictionary in memory accross all instances of webSpellChecker (false by default).
        /////</summary>
        /////<remarks>Setting this to true will lower memory usage if multipe instances of webSpellChecker are in use at once,
        /////but will mean that only one dictionary may be used at one time.  This does not affect user dictionaries.
        /////<p>Note that all instances with this set to true should use the same dictionary.  This property must be set before any
        /////spell checking begins.
        /////</p></remarks>
        //public bool ShareDictionary
        //{
        //    get{ return shareDictionary;}
        //    set{ shareDictionary = value;}
        //}
        /// <summary>
        /// Whether to allow words spelt with mixed case, e.g. "MIxEd" - also see AllowAnyCase.
        /// </summary>
		public bool AllowMixedCase
		{
			get{ return allowMixedCase ; }
			set{ allowMixedCase = value; }
		}

        /// <summary>
        /// Whether to allow suggestions for correctly spelled words with mixed case.
        /// </summary>
		public bool AllowCaseInsensitiveSuggestions
		{
			get{ return allowCaseInsensitiveSuggestions ; }
			set{ allowCaseInsensitiveSuggestions = value; }
		}

        /// <summary>
        /// Whether to allow words spelt with incorrect case, e.g. "africa", instead of "Africa."  This is more relaxed than AllowMixedCase.
        /// </summary>
		public bool AllowAnyCase
		{
			get{ return allowAnyCase ; }
			set{ allowAnyCase = value; }
		}

        ///// <summary>
        ///// Whether to look for capitalized suggestions.  Note this will slow FindSuggestions down by about 7 times.
        ///// </summary>
        //public bool FindCapitalizedSuggestions
        //{
        //    get{ return findCapitalizedSuggestions;} set{ findCapitalizedSuggestions = value;}
        //}
		///  <summary>The UserDictionary being used, may be null if no user dictionary is being used.</summary>
		public UserDictionary userDictionary
		{
			get { return _userDictionary; }
			set { _userDictionary = value; }
		}

        /// <summary>
        /// The words to ignore.  Note this list is changed by IgnoreAll.
        /// </summary>
        public List<String> IgnoreList
		{
			get { return ignoreList; }
			set { ignoreList = value; }
		}

		/// <summary>Whether to ignore words with numbers in them (eg. A10 or 7-11).</summary>
		public bool AllowWordsWithDigits
		{
			get{ return allowWordsWithDigits;}
			set{ allowWordsWithDigits = value;}
		}

		///<summary>Gets the current bad word (last returned by NextBadWord).</summary>
        public BadWord CurrentBadWord
		{
			get{ return currentBadWord; }
		}

		/// <summary>Sets whether this should ignore Rtf (or any escaped) codes in the text.</summary>
		public bool IgnoreRtfCodes
		{
			get { return ignoreRtfCodes; }
			set {ignoreRtfCodes = value; }
		}

		#endregion

		#region "Methods"
		

#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


		
			public SpellChecker() 
			{
				Init();
			}
		

        // MD 8/21/07 - BR25671
        // Removed the finalizer because it will just cause the object to be kept around 
        // for one more collection, and we only null out other objects in while disposing.
        /////<summary></summary>
        
        //{
        //    mainDictionary = null;
        //    mainDicListPhonetic = null;
        //    userDicList = null;
        //    reverseSortedDictionary = null;
        //    Dispose(false);
        //}

        /// <summary>
        /// Disposes of resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // MD 8/21/07 - BR25671
            // This code will never be hit, it has been removed
            //#if DESKTOP
            ////must destroy license
            //if (license != null) 
            //{
            //    license.Dispose();
            //    license = null;
            //}
            //#endif
            // Suppress finalization of this disposed instance
            if (disposing)
            {
                // MD 8/21/07 - BR25671
                // We don't have a finalizer anymore
                //GC.SuppressFinalize(this);

                mainDictionary = null;
                mainDicListPhonetic = null;

                // MD 11/13/07 - BR27454
                // We don't keep a separate list of the words anymore
                //userDicList = null;

                reverseSortedDictionary = null;
            }

		}

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        // MD 8/21/07 - BR25671
        // This should not be virtual
        //public virtual void Dispose()
        public void Dispose()
        {
            Dispose(true);
        }



        /// <summary>
        /// Sets the suggestions method to use.  Either PhoneticSuggestions or HashingSuggestions.
        /// </summary>
		public void SetSuggestionsMethod(SuggestionsMethod suggestionsMethod)
		{
			this.suggestionsMethod = suggestionsMethod;
		}
        /// <summary>
        /// Gets the suggestions method to use.  Either PhoneticSuggestions or HashingSuggestions.
        /// </summary>
		public virtual SuggestionsMethod GetSuggestionsMethod(){
			return suggestionsMethod;
		}


        /// <summary>
        /// Sets whether the user dictionary should be used in finding suggestions for misspelt words.
        /// </summary>
		public void SetIncludeUserDictionaryInSuggestions(bool v){
			includeUserDictionaryInSuggestions = v;
		}
        /// <summary>
        /// Gets whether the user dictionary should be used in finding suggestions for misspelt words.
        /// </summary>
		public bool GetIncludeUserDictionaryInSuggestions(){
			return includeUserDictionaryInSuggestions;
		}

        /// <summary>
        /// Sets whether to ignore words with capital letters.
        /// </summary>
		public void SetAllowCapitalizedWords(bool v){
			allowCapitalizedWords = v;
		}
        /// <summary>
        /// Gets whether to ignore words with capital letters.
        /// </summary>
		public bool GetAllowCapitalizedWords(){
			return allowCapitalizedWords;
		}


        /// <summary>
        /// Sets the factor for words to consider for suggestions.  Lower values are faster but consider less words for suggestions.  The default is 80.  If w &lt;= 1 this method won't do anything.
        /// </summary>
		public void SetConsiderationRange( int w ){
			if(w>1)	radius  = w;
		}

        /// <summary>
        /// Gets the factor for words to consider for suggestions.
        /// </summary>
		public int GetConsiderationRange(  ){
			return radius;
		}

        /// <summary>
        /// Sets whether to treat hyphenated (-) words as separate words.  The default is false.
        /// </summary>
        /// <remarks>For e.g. if this is true text like "cheap-deals" will be treated as two words, "cheap" and "deals."  Otherwise this will be treated as one word, "cheap-deals".  Also see CheckHyphenatedText.</remarks>
		public void SetSeparateHyphenWords(bool f){
			wordIterator.SeparateHyphenWords = f;
		}

        /// <summary>
        /// Gets whether to treat hyphenated (-) words as separate words.  The default is false.
        /// </summary>
        /// <remarks>For e.g. if this is true text like "cheap-deals" will be treated as two words "cheap" and "deals", otherwise this will be treated as one word, "cheap-deals".</remarks>
		public bool GetSeparateHyphenWords(){
			return wordIterator.SeparateHyphenWords;
		}

		



		// initialises vectors
		private void Init(){
			
			resourceDecoder = new ResourceDecoder();
			compare = new CompareL(this);

			//list of misspelt words in doc.
            badWordList = new List<String>(100);

			//ignore word list
            ignoreList = new List<String>(10);
		}


		private DictFile dictFile = null;
		public DictFile DictFile
		{
			get{return dictFile;}
			set
			{
				if ( this.dictFile != value )
				{
					this.dictFile = value;

                    //MD 5/8/2006 - BR11823
                    // Need to reset the loaded dictionaries if the dictionary file is set to a new value.
                    //
                    this.dictionaryIsLoaded = false;
                    this.reverseSortedDictionary = null;

                    // MD 11/13/07
                    // Found while fixing BR27454
                    // These are never used, they were removed
                    //SpellChecker.sharedMainDictionary = null;
                    //SpellChecker.sharedReverseDictionary = null;
                }
            }
        }

		public bool autoCloseReader=true;
        /// <summary>
        /// Reads list from the stream.
        /// </summary>
        public int ReadWordListStream(List<String> list, int type, DictFile df, String encoding)
		{
			Stream fs;

			// MD 10/5/11 - TFS91391/TFS84743/TFS33353
			// Added support for .txt dictionary files
			// We check if its a .txt file we read from the specified filepath and store it in a Stream
			// Otherwise we proceed with decoding the .dict file as we did before
			//if (df.dictFilePath != null)
			//    fs = df.GetDecodedFileStream();
			//else
			//    fs = new DecodedMemoryStream(df.dictFileBytes);
			StreamReader reader = null;
			if (df.dictFileExtension != null &&
				string.Equals(".txt", df.dictFileExtension, StringComparison.CurrentCultureIgnoreCase))
			{
				fs = df.dictFile;

				reader = new StreamReader(fs, Encoding.Default);

			}
			else if (df.dictFilePath != null)
			{
				fs = df.GetDecodedFileStream();
			}
			else
			{
				fs = new DecodedMemoryStream(df.dictFileBytes);
			}

			// MD 10/10/11 - TFS84743/TFS33353
			// This is now declared above.
			//StreamReader reader;

			String line;

			int[] pos = df.GetStreamDimensions(type);
			fs.Seek(pos[0], SeekOrigin.Begin);

			// MD 10/10/11 - TFS84743/TFS33353
			// Only reader the reader if it hasn't already been created.
			//reader = new StreamReader(fs);
			if (reader == null)
				reader = new StreamReader(fs);

			int numWords=0;
			while(numWords < pos[1] && (line=reader.ReadLine())!=null)
			{
				list.Add(line);
				++numWords;
			}

			if (autoCloseReader) reader.Close();

            // MD 5/23/07 - BR22891
            // We do binary searches on the list, but its not sorted, so the search might not find a 
            // word in the list
            // MD 11/13/07
            // Found while fixing BR27454
            // This was moved to be a static readonly field on the comparer itself
            //list.Sort( invariantComparer );
            list.Sort(InvariantComparer.Instance);

			return numWords;
			
		}

		//loads the lexicon  from the DLL if no dict file spec'obj.
		private void LoadLexicon()
		{
			lock(this)
			{
				if(!dictionaryIsLoaded)
				{
					mainDictionary.Clear();
					badWordList.Clear();

					DictFile df = this.dictFile;
					if(df == null)
						df = this.GetEmbeddedResource();

                    // MD 11/13/07
                    // Found while fixing BR27454
                    // shareDictionary and sharedMainDictionary are never used and shareDictionary was always false
                    // They were all removed and the only code that would be hit was preserved.
                    //if(this.shareDictionary && sharedMainDictionary == null)
                    //        this.numWords = this.ReadWordListStream(sharedMainDictionary,1,df,"UTF8");
                    //else if(!this.shareDictionary)
                    //        this.numWords = this.ReadWordListStream(this.mainDictionary,1,df,"UTF8");
                    //else
                    //    this.numWords = sharedMainDictionary.Count;
                    this.numWords = this.ReadWordListStream(this.mainDictionary, 1, df, "UTF8");

                    dictionaryIsLoaded = true;

                    // MD 11/13/07
                    // Found while fixing BR27454
                    // sharedMainDictionary is never used and shareDictionary was always false.
                    //if (shareDictionary) mainDictionary = sharedMainDictionary;
                }
            }
        }

		//Loads the reverse sorted word list, from DLL
		private void LoadReverseList(){

            lock (this)
            {
                if (reverseSortedDictionary == null)
                {
                    // MD 11/13/07
                    // Found while fixing BR27454
                    // sharedReverseDictionary is never used and shareDictionary was always false
                    // They were all removed and the only code that would be hit was preserved.
                    //if (!shareDictionary) 
                    //	reverseSortedDictionary = new ArrayList(100000);
                    //
                    //if (shareDictionary && sharedReverseDictionary==null)
                    //{
                    //    sharedReverseDictionary = ArrayList.Synchronized( new ArrayList(100000) );
                    //    sharedReverseDictionary =(ArrayList)this.mainDictionary.Clone();
                    //    sharedReverseDictionary.Reverse();
                    //}
                    //else if (!shareDictionary) 
                    //{
                    //	reverseSortedDictionary =(ArrayList)this.mainDictionary.Clone();
                    //	reverseSortedDictionary.Reverse();
                    //} 
                    //	
                    //if (shareDictionary) 
                    //    reverseSortedDictionary = sharedReverseDictionary;
                    this.reverseSortedDictionary = new List<string>(this.mainDictionary);
                    this.reverseSortedDictionary.Reverse();
                }

            }

        }

		//Clears the reverse sorted word list
		private void ClearReverseList(){
			lock(this){
				reverseSortedDictionary = null;
				dictionaryIsLoaded = false;
			}
		}


        /// <summary>
        /// Adds a <c>word</c> to the user dictionary (if it has been specified).
        /// </summary>
        /// <param name="word">String to add to user dictionary, if it is being used.</param>
        /// <returns>Boolean, true if the word is now in the user dictionary, false if the word could not be added to the user dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">If word parameter is null.</exception>
		public virtual bool AddWord(String word)
		{
			if(word == null) throw new ArgumentNullException("Null String passed to AddWord() - ensure word parameter is not null.");

            //add word to ignore list
            IgnoreAll(word);
            //if a user dictionary is not installed, return false.
            if (userDictionary == null || !userDictionary.IsValid()) return false;
            else
            {
                //check if word is already in user dictionary
                if (LookUpUserDictionary(word)) return true;
                else
                {
                    // MD 11/13/07 - BR27454
                    // We don't keep a separate list of the words anymore
                    //userDicList.Add(word);
                    return userDictionary.AddWord(word);
                }
            }
        }

		public virtual bool RemoveWord( String word )
		{
			if ( word == null )
				throw new ArgumentNullException( "word", "Null String passed to RemoveWord() - ensure word parameter is not null." );
	
			this.ignoreList.Remove( word );

			if ( this.userDictionary == null || !this.userDictionary.IsValid() )
				return false;

            // MD 11/13/07 - BR27454
            // We don't keep a separate list of the words anymore
            //this.userDicList.Remove( word );
            return this.userDictionary.RemoveWord(word);
        }

        /// <summary>
        /// Returns the next mis-spelt word in the text as a BadWord object.
        /// </summary>
        /// <returns>Keyoti.RapidSpell.BadWord or null if there are no more bad words.</returns>
        /// @see Keyoti.rapidSpell.BadWord
        public virtual BadWord NextBadWord() 
		{
			string nextWord = GetNextWord();
	
			//invalidate current bad word
			int currentBadWordStart = -1;
			int currentBadWordEnd = -1;
			currentBadWord = null;

			//no next word exists so return null
			if (nextWord==null) return null;
			else {
				//see if next word is bad or if we are to ignore this word, if not scan for next bad word 
				while((!Flagged(nextWord) || ignoreList.Contains(nextWord)) ){
					previousWord = nextWord;
					nextWord = GetNextWord();


					//ran out of words
					if (nextWord==null) return null;
					
				}
				previousWord = nextWord;	//set previous word when we found a misspelling

				//set current bad word
				currentBadWordStart = wordStart;
				currentBadWordEnd = wordEnd;
			}

			//must remove formatting
			if(AllowXML){
				int openPos, closePos;
				while( (openPos=nextWord.IndexOf('<')) > -1 && (closePos=nextWord.IndexOf('>', openPos)) > -1){
					//remove everything between < and > inclusively
					nextWord = nextWord.Substring(0, openPos) + nextWord.Substring(closePos+1);
				}
			}

            currentBadWord = new BadWord(nextWord, currentBadWordStart, currentBadWordEnd);	//spelling error
				
			return currentBadWord;
		}

        /// <summary>
        /// Changes the current bad word to the <c>newWord</c> in the text.
        /// </summary>
        /// <param name="newWord">replaces the current misspelt word.</param>
        /// <returns>void</returns>
        /// <exception cref="NoCurrentBadWordException">if nextBadWord() hasn't been run first AND found an erroneous word.</exception>
        /// <exception cref="ArgumentNullException">if newWord parameter is null.</exception>
		public virtual void ChangeBadWord(String newWord)
		{

			if(newWord == null) throw new ArgumentNullException("Null String passed to changeBadWord() - ensure newWord parameter is not null");

			if(ChangeWord != null) ChangeWord(this, new ChangeWordEventArgs(theText.ToString().Substring(CurrentBadWord.StartPosition,CurrentBadWord.EndPosition - CurrentBadWord.StartPosition), newWord, CurrentBadWord.StartPosition));

			//check we currently have a word selected
			if(CurrentBadWord.StartPosition==-1 || CurrentBadWord.EndPosition==-1){
				throw new NoCurrentBadWordException("No word currently selected, use nextBadWord() first.", null);
			} else {
				//delete the old word
				theText.Remove(CurrentBadWord.StartPosition, CurrentBadWord.EndPosition - CurrentBadWord.StartPosition);
				//insert new one
				theText.Insert(CurrentBadWord.StartPosition, newWord);
				//reset the break iterator text.
				wordIterator.SetText(theText.ToString());

				//adjust wordEnd, so that next word will be search for from end of this new word
				wordEnd = CurrentBadWord.StartPosition + newWord.Length;

				previousWord = newWord;
			}

		}

		DictFile GetEmbeddedResource()
		{
            List<String> list = new List<String>();
			Stream stream = null;
			System.Reflection.Assembly assembly = this.GetType().Assembly;

            //stream = assembly.GetManifestResourceStream("Infragistics.Silverlight.XamSpellChecker.Dictionaries.us-english-v2-whole.dict");

            //BinaryReader r = new BinaryReader(stream);
            //byte[] bytes = r.ReadBytes((int)stream.Length);
            //r.Close();

			//stream = assembly.GetManifestResourceStream("Infragistics.Silverlight.Controls.Dictionaries.us-english-v2-whole.dict");

			// MD 10/10/11 - TFS84743/TFS33353
			// Use the new DictFile constructor overload to specify the file name so we know what extension the file has.
			//stream = assembly.GetManifestResourceStream("Infragistics.Controls.Interactions.Dictionaries.us-english-v2-whole.dict");
			//
			//return new DictFile(stream);//, bytes);
			const string resourceName = "Infragistics.Controls.Interactions.Dictionaries.us-english-v2-whole.dict";
			stream = assembly.GetManifestResourceStream(resourceName);
			return new DictFile(stream, resourceName);
		}


        /// <summary>
        /// Checks text for spelling correctness.  From startPosition in text, mis-spelt words can be accessed through <c>nextBadWord</c>.
        /// </summary>
        /// <remarks>The property startPosition should be &gt;=0 but if it is &lt; 0 it is set = 0.</remarks>
        /// <param name="text">the text to be checked</param>
        /// <param name="startPosition">the position in the text to begin checking from</param>
        /// <returns>void</returns>
        /// <exception cref="ArgumentNullException">if text parameter is null.</exception>
		public virtual void Check(String text, int startPosition)
		{
			if (text == null) throw new ArgumentNullException("Null String passed to check() - ensure text parameter is not null.");
			LoadLexicon();

			previousWord = "";

			if (startPosition < 0) startPosition = 0;
			//set the current word to start of the text
			wordStart = startPosition;
			wordEnd = wordStart;


			//keep a copy of the text as a StringBuffer
			theText = new StringBuilder(text);
			//set the text for wordIterator to work on
			wordIterator.SetText(text);
			//find all bad words and put them in badWordList.
			PreCheck();
		}

		#region "HTML Entity processing"
		//--Used by Web control only to properly convert entities
		static string[] entities = new string[]{"nbsp","iexcl","cent","pound","curren","yen","brvbar","sect","uml","copy","ordf","laquo","not","shy","reg","macr","deg","plusmn","sup2","sup3","acute","micro","para","middot","cedil","sup1","ordm","raquo","frac14","frac12","frac34","iquest","Agrave","Aacute","Acirc","Atilde","Auml","Aring","AElig","Ccedil","Egrave","Eacute","Ecirc","Euml","Igrave","Iacute","Icirc","Iuml","ETH","Ntilde","Ograve","Oacute","Ocirc","Otilde","Ouml","times","Oslash","Ugrave","Uacute","Ucirc","Uuml","Yacute","THORN","szlig","agrave","aacute","acirc","atilde","auml","aring","aelig","ccedil","egrave","eacute","ecirc","euml","igrave","iacute","icirc","iuml","eth","ntilde","ograve","oacute","ocirc","otilde","ouml","divide","oslash","ugrave","uacute","ucirc","uuml","yacute","thorn","yuml","quot","amp","lt","gt","OElig","oelig","Scaron","scaron","Yuml","circ","tilde","ensp","emsp","thinsp","zwnj","zwj","lrm","rlm","ndash","mdash","lsquo","rsquo","sbquo","ldquo","rdquo","bdquo","dagger","Dagger","permil","lsaquo","rsaquo","euro","fnof","Alpha","Beta","Gamma","Delta","Epsilon","Zeta","Eta","Theta","Iota","Kappa","Lambda","Mu","Nu","Xi","Omicron","Pi","Rho","Sigma","Tau","Upsilon","Phi","Chi","Psi","Omega","alpha","beta","gamma","delta","epsilon","zeta","eta","theta","iota","kappa","lambda","mu","nu","xi","omicron","pi","rho","sigmaf","sigma","tau","upsilon","phi","chi","psi","omega","thetasym","upsih","piv","bull","hellip","prime","Prime","oline","frasl","weierp","image","real","trade","alefsym","larr","uarr","rarr","darr","harr","crarr","lArr","uArr","rArr","dArr","hArr","forall","part","exist","empty","nabla","isin","notin","ni","prod","sum","minus","lowast","radic","prop","infin","ang","and","or","cap","cup","int","there4","sim","cong","asymp","ne","equiv","le","ge","sub","sup","nsub","sube","supe","oplus","otimes","perp","sdot","lceil","rceil","lfloor","rfloor","lang","rang","loz","spades","clubs","hearts","diams"};
		static int[] decimalCodes = new int[]{160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255,34,38,60,62,338,339,352,353,376,710,732,8194,8195,8201,8204,8205,8206,8207,8211,8212,8216,8217,8218,8220,8221,8222,8224,8225,8240,8249,8250,8364,402,913,914,915,916,917,918,919,920,921,922,923,924,925,926,927,928,929,931,932,933,934,935,936,937,945,946,947,948,949,950,951,952,953,954,955,956,957,958,959,960,961,962,963,964,965,966,967,968,969,977,978,982,8226,8230,8242,8243,8254,8260,8472,8465,8476,8482,8501,8592,8593,8594,8595,8596,8629,8656,8657,8658,8659,8660,8704,8706,8707,8709,8711,8712,8713,8715,8719,8721,8722,8727,8730,8733,8734,8736,8743,8744,8745,8746,8747,8756,8764,8773,8776,8800,8801,8804,8805,8834,8835,8836,8838,8839,8853,8855,8869,8901,8968,8969,8970,8971,9001,9002,9674,9824,9827,9829,9830};
		static Dictionary<String,int> entityDecimalCodes = null;

        /// <summary>
        /// Converts HTML entity codes to their Unicode characters.
        /// </summary>
		static public string convertHtmlEntities(string text){
			if(entityDecimalCodes==null){
                entityDecimalCodes = new Dictionary<String, int>();
				for(int i=0; i<entities.Length; i++)
					entityDecimalCodes.Add(entities[i], decimalCodes[i]);
				decimalCodes = null;
				entities=null;
			}
			int startPos=-1, endPos=-1;
			object edc;
			while( (startPos=text.IndexOf("&", startPos+1))>-1){
				endPos = text.IndexOf(";", startPos);
				if(endPos>-1 && endPos<startPos+10 && endPos>startPos){
					if(text[startPos+1]=='#' && endPos>startPos+1)
						edc = Int32.Parse(text.Substring(startPos+2, endPos - (startPos+1)-1));
					else
						edc = entityDecimalCodes[text.Substring(startPos+1, endPos - (startPos+1))];

					if(edc!=null)
						text = text.Substring(0, startPos) + (char)(int)edc + text.Substring(endPos+1);
					else 
						text = text.Substring(0, startPos) + text.Substring(endPos+1);
				}
			}
			return text;
		}
			#endregion


        /// <summary>
        /// Checks text for spelling correctness.  From the beginning of text, mis-spelt words can be accessed through <c>nextBadWord</c>.
        /// </summary>
        /// <param name="text">to spell check.</param>
        /// <returns>void</returns>
        /// <exception cref="ArgumentNullException">if text parameter is null.</exception>
		public virtual void Check(String text)
		{
			if (text == null) throw new Exception("Null String passed to check() - ensure text parameter is not null.");
			//check from start
			Check(text, 0);
		}

        /// <summary>
        /// Gets an enumeration of String suggestions for spellings of the current bad word.
        /// </summary>
        /// <returns>ArrayList of String suggestions for the current bad word.</returns>
        /// <exception cref="NoCurrentBadWordException">If NextBadWord() hasn't been run first AND found an erroneous word.</exception>
		public virtual List<String> FindSuggestions() 
		{
			lock(this){
				//check we currently have a word selected
				if(CurrentBadWord.StartPosition==-1 || CurrentBadWord.EndPosition==-1){
					throw new NoCurrentBadWordException("No word currently selected, use nextBadWord() first.", null);
				} else {
					return FindSuggestions(theText.ToString().Substring(CurrentBadWord.StartPosition, CurrentBadWord.EndPosition - CurrentBadWord.StartPosition));
				}
			}
		}

        /// <summary>
        /// Gets an enumeration of String suggestions for the word.
        /// </summary>
        /// <param name="word">the word to check the spelling of.</param>
        /// <returns>ArrayList of String suggestions for the word.</returns>
        /// <exception cref="ArgumentNullException">if word parameter is null.</exception>
        public virtual List<String> FindSuggestions(String word)
        {
			return FindSuggestions(word, true);
		}

        /// <summary>
        /// Gets an enumeration of String suggestions for the word.
        /// </summary>
        /// <param name="word">the word to check the spelling of.</param>
        /// <param name="searchLowerCase">whether to return suggestions for word in a lower case form (if it's capitalized).</param>
        /// <returns>ArrayList of String suggestions for the word.</returns>
        /// <exception cref="ArgumentNullException">if word parameter is null.</exception>
        protected virtual List<String> FindSuggestions(String word, bool searchLowerCase)
        {
            //the level at which the likeness (determined by suggestionScore2) is high enough for the
            //word to be considered a suggestion
            short threshold = 700;
            //3 letter words return a likeness of 666 (two thirds) when only one letter is wrong, so lower threshold
            if (word.Length < 4) threshold = 600;



            lock (this)
            {
                if (word == null) throw new ArgumentNullException("Null String passed to findSuggestions() - ensure word parameter is not null.");
                LoadLexicon();
                List<String> suggestions = new List<String>(12);
                //if (string.IsNullOrEmpty(word).Equals("")) return suggestions;
                if (string.IsNullOrEmpty(word)) return suggestions;

                //must remove formatting
                if (AllowXML)
                {
                    int openPos, closePos;
                    while ((openPos = word.IndexOf('<')) > -1 && (closePos = word.IndexOf('>', openPos)) > -1)
                    {
                        //remove everything between < and > inclusively
                        word = word.Substring(0, openPos) + word.Substring(closePos + 1);
                    }
                }

                bool dontSuggestWord = false;
                String listWord, listWordPhonetic;


                #region "Handling for capitalized words"
                //if the word starts with a cap or is all cap we want to find suggestions in various cases
                bool cap = true, allCap = true;
                if (Char.IsLower(word[0])) cap = false;
                bool startsWithLetter = Char.IsLetter(word[0]);

                if (word.Length > 1 && cap && startsWithLetter)
                {	//see if all caps
                    for (int ci = 0; ci < word.Length; ci++)
                        allCap = allCap && Char.IsUpper(word, ci);
                }

                if (cap && searchLowerCase && startsWithLetter)
                {

                    // MD 8/28/06 - BR15286
                    // We don't need to search with lower case anymore, 
                    // the word is already going to be lower case on the next pass
                    //suggestions = FindSuggestions(word.ToLower());
                    suggestions = FindSuggestions(word.ToLower(), false);

                    for (int i = 0; i < suggestions.Count; i++)
                    {
                        if (!allCap) suggestions[i] = Char.ToUpper(((String)suggestions[i])[0]) + ((String)suggestions[i]).Substring(1);
                        else suggestions[i] = ((String)suggestions[i]).ToUpper();
                    }
                    if (SuggestSplitWords)
                    {
                        List<String> joined = new List<String>(3);
                        StringBuilder joinedWords = new StringBuilder("");
                        if (FindSplitWords(word, joined))
                        {
                            for (int ptr = joined.Count - 1; ptr >= 0; ptr--)
                            {
                                joinedWords.Append((String)joined[ptr]).Append(" ");
                            }
                            if (joinedWords.Length > 1)
                            {
                                joinedWords.Remove(joinedWords.Length - 1, 1);
                                if (!suggestions.Contains(joinedWords.ToString()))
                                    suggestions.Add(joinedWords.ToString());
                            }
                        }
                    }
                    // MD 11/13/07
                    // Found while fixing BR27454
                    // findCapitalizedSuggestions is always false so we would never get in here
                    //} else if (!cap && findCapitalizedSuggestions && startsWithLetter){
                    //    suggestions = FindSuggestions(Char.ToUpper(word[0])+word.Substring(1), false);
                }
                #endregion


                //MD 5/25/06
                // Changed values of SuggestionsMethod to follow our naming convention for Win

                if (suggestionsMethod == SuggestionsMethod.HashingSuggestions)
                {
                    #region "HASHING suggestions"
                    //this method finds a list of words from the forward and reverse sorted dictionaries
                    //using a binary search, and then analyses only words from those lists to see if they're
                    //suitable matches.
                    //eg.
                    //word = "tablee"

                    //binary search in forward list might return words
                    //...,...,tablature,table,tableau,tableaus,tableau's,tableaux,...
                    //the length of the list is dependent on the ConsiderationRange (or radius field)\

                    //binary search in reverse list might return words
                    //...,teepee,Tennessee,tepee,thee,...
                    //the length of the list is dependent on the ConsiderationRange (or radius field)

                    //searching reverse list is useful when the word to get suggestions for has an error
                    //at the beginning, eg. "ttable"

                    //the forward and reverse lists are then merged and the words considered more closely.

                    List<String> subSet = null;

                    //find position in forward and reverse dics

                    if (optimization == OPTIMIZE_FOR_MEMORY)
                    {
                        //Optimized for memory means go through whole dictionary, and dont use the reverse list
                        subSet = mainDictionary;
                    }
                    else
                    {
                        if (reverseSortedDictionary == null)
                            LoadReverseList();

                        //Optimized for speed means can use reverse list to find short subSet
                        // MD 11/13/07
                        // Found while fixing BR27454
                        // This was moved to be a static readonly field on the comparer itself
                        //int forwardPos = mainDictionary.BinarySearch(word, invariantComparer);
                        int forwardPos = mainDictionary.BinarySearch(word, InvariantComparer.Instance);

                        if (forwardPos < 0) forwardPos = forwardPos * -1;

                        // MD 11/13/07
                        // Found while fixing BR27454
                        // This was moved to be a static readonly field on the comparer itself
                        //int backwardPos = reverseSortedDictionary.BinarySearch(word, new ReverseSorter());
                        int backwardPos = reverseSortedDictionary.BinarySearch(word, ReverseSorter.Instance);

                        if (backwardPos < 0) backwardPos = backwardPos * -1;
                        //make word list to run through

                        if (forwardPos > numWords - radius) forwardPos = numWords - radius;
                        if (forwardPos < radius) forwardPos = radius;
                        int count = radius * 2;
                        if (count > mainDictionary.Count) count = mainDictionary.Count - (forwardPos - radius);

                        // MD 11/13/07 - BR27454
                        // Changed this to a generic list for performance
                        //subSet = new ArrayList(mainDictionary.GetRange(forwardPos - radius, count));
                        subSet = new List<String>(mainDictionary.GetRange(forwardPos - radius, count));

                        if (backwardPos < radius) backwardPos = radius;
                        else if (backwardPos > reverseSortedDictionary.Count - radius) backwardPos = reverseSortedDictionary.Count - radius;
                        if (count > reverseSortedDictionary.Count) count = reverseSortedDictionary.Count - (backwardPos - radius);
                        subSet.AddRange(reverseSortedDictionary.GetRange(backwardPos - radius, count));

                    }

                    //subSet is list of merged words from forward and reverse sublists

                    for (int i = 0; i < subSet.Count; i++)
                    {
                        //if optimize for memory is set, the reverse list isnt loaded
                        if (optimization == OPTIMIZE_FOR_MEMORY) listWord = (String)mainDictionary[i];
                        else listWord = (String)subSet[i];



                        if (IsSuggestion(listWord, word, threshold))
                        {
                            if (cap) listWord = MakeCap(listWord);	//cap first letter

                            string listWordLower = listWord.ToLower();
                            //see if word is in dontSuggest array
                            for (int k = 0; k < dontSuggest.Length; k++)
                                if (listWordLower.Equals(dontSuggest[k])) dontSuggestWord = true;


                            //put word in suggestions if it wasnt in dontSuggest or already suggested.
                            if (!dontSuggestWord && !suggestions.Contains(listWord))
                            {
                                suggestions.Add(listWord);
                            }
                            else dontSuggestWord = false;

                        }

                    }


                    //if we should use the user dictionary for suggestions
                    // MD 11/13/07 - BR27454
                    // We don't keep a separate list of the words anymore
                    // Use the SortedWords collection on the user dictionary
                    //if(includeUserDictionaryInSuggestions){
                    //    for(int i=0; i<userDicList.Count; i++){	
                    //        listWord = (String) userDicList[i];
                    if (includeUserDictionaryInSuggestions && this.userDictionary != null)
                    {
                        for (int i = 0; i < this.userDictionary.SortedWords.Count; i++)
                        {
                            listWord = this.userDictionary.SortedWords[i];

                            if (IsSuggestion(listWord, word, threshold))
                            {
                                if (cap) listWord = MakeCap(listWord);	//cap first letter

                                string listWordLower = listWord.ToLower();

                                //see if word is in dontSuggest array
                                for (int k = 0; k < dontSuggest.Length; k++)
                                    if (listWordLower.Equals(dontSuggest[k])) dontSuggestWord = true;

                                //put word in suggestions if it wasnt in dontSuggest.
                                if (!dontSuggestWord)
                                {
                                    suggestions.Add(listWord);
                                }
                                else dontSuggestWord = false;

                            }
                        }
                    }


                    #endregion
                }
                else
                {
                    #region "PHONETIC suggestions"
                    String phonetic = PhoneticsProcessor.MetaPhone(word);
                    if (optimization == OPTIMIZE_FOR_MEMORY)
                    {

                        for (int i = 0; i < numWords; i++)
                        {


                            listWord = (String)mainDictionary[i];
                            if (PhoneticsProcessor.MetaPhone(listWord).Equals(phonetic))
                            {
                                if (cap) listWord = MakeCap(listWord);	//cap first letter

                                string listWordLower = listWord.ToLower();

                                //see if word is in dontSuggest array
                                for (int k = 0; k < dontSuggest.Length; k++)
                                    if (listWordLower.Equals(dontSuggest[k])) dontSuggestWord = true;

                                //put word in suggestions if it wasnt in dontSuggest.
                                if (!dontSuggestWord)
                                {
                                    suggestions.Add(listWord);
                                }
                                else dontSuggestWord = false;


                            }
                        }




                    }
                    else
                    {	//OPTIMIZE FOR SPEED, uses phonetic lookup and more memory

                        for (int i = 0; i < numWords; i++)
                        {
                            if (mainDicListPhonetic == null) GeneratePhoneticList();
                            listWordPhonetic = (String)mainDicListPhonetic[i];

                            if (listWordPhonetic.Equals(phonetic))
                            {

                                listWord = (String)mainDictionary[i];
                                if (cap) listWord = MakeCap(listWord);	//cap first letter

                                string listWordLower = listWord.ToLower();

                                //see if word is in dontSuggest array
                                for (int k = 0; k < dontSuggest.Length; k++)
                                    if (listWordLower.Equals(dontSuggest[k])) dontSuggestWord = true;

                                //put word in suggestions if it wasnt in dontSuggest.
                                if (!dontSuggestWord)
                                {
                                    suggestions.Add(listWord);
                                }
                                else dontSuggestWord = false;


                            }
                        }

                    }

                    // MD 11/13/07 - BR27454
                    // We don't keep a separate list of the words anymore
                    // Use the SortedWords collection on the user dictionary
                    //if(includeUserDictionaryInSuggestions){
                    //    for(int i=0; i<userDicList.Count; i++){	
                    //        listWord = (String) userDicList[i];
                    if (includeUserDictionaryInSuggestions && this.userDictionary != null)
                    {
                        for (int i = 0; i < this.userDictionary.SortedWords.Count; i++)
                        {
                            listWord = this.userDictionary.SortedWords[i];

                            if (PhoneticsProcessor.MetaPhone(listWord).Equals(phonetic))
                            {
                                if (cap) listWord = MakeCap(listWord);	//cap first letter

                                string listWordLower = listWord.ToLower();

                                //see if word is in dontSuggest array
                                for (int k = 0; k < dontSuggest.Length; k++)
                                    if (listWordLower.Equals(dontSuggest[k])) dontSuggestWord = true;

                                //put word in suggestions if it wasnt in dontSuggest.
                                if (!dontSuggestWord)
                                {
                                    suggestions.Add(listWord);
                                }
                                else dontSuggestWord = false;

                            }
                        }
                    }
                    #endregion
                }


                #region "Split word suggestions"
                //in addition to above searches, we can also see if the error word was just 
                //two words joined togther, eg. wherewere
                if (SuggestSplitWords)
                {
                    //look for joined words
                    List<String> joined = new List<String>(3);
                    StringBuilder joinedWords = new StringBuilder("");
                    //get compound words in 'word', but only if !cap because will have been done once
                    if (FindSplitWords(word, joined) && !cap)
                    {
                        for (int ptr = joined.Count - 1; ptr >= 0; ptr--)
                        {
                            joinedWords.Append((String)joined[ptr]).Append(" ");
                        }
                        if (joinedWords.Length > 1)
                        {
                            joinedWords.Remove(joinedWords.Length - 1, 1);
                            suggestions.Add(joinedWords.ToString());
                        }
                    }
                }
                #endregion

                #region "Anagrams"
                //if short word get anagrams aswell (good for typos)
                if (word.Length <= 5) FindAnagrams(word, suggestions);
                #endregion

                //sort list of suggestions by their score
                compare.with(word);
                suggestions.Sort(compare);

                return suggestions;
            }
        }
            



		//Wraps the currently used suggestions method, mostly here for testing.
		private bool IsSuggestion(String dictionaryWord, String queryWord, short tolerance){
			return suggestionScore2(dictionaryWord, queryWord) >= tolerance;
		}



		#region "Two stage suggestions scoring"

		#region "suggestionsScore2 members"

			//Keep these variables outside the method, to prevent reinitialisation every method call. 
			//although technically these declarations belong in the method scope because suggestionScore2 
			//is called hundreds to thousands of times, it seems wasteful.
		int[] taken = new int[15];
		int p;
		int maxScore;
		int tail;
		int smallestDistance, distance;
		int agreement;
		bool isTaken;
		int qryLenI;
		float qryLen;
		int chosenCharPos;
		int tally;
			#endregion
		//step 1, ignore words that are too different in length or have many different letters
		int suggestionScore2(String dictionaryWord, String queryWord)
		{
			if(this.AllowCaseInsensitiveSuggestions)
			{
				dictionaryWord = dictionaryWord.ToLower();
				queryWord = queryWord.ToLower();
			}
			//lazy loading
			if(loadedQryWrd==null || !loadedQryWrd.Equals(queryWord)){

				loadedQryWrd = queryWord;
				//used in suggestions
				qryLenI = queryWord.Length;
				qryLen = qryLenI;
				queryWordA = queryWord.ToCharArray();
			}

			maxScore = dictionaryWord.Length;
			tail = maxScore;
					

			//---------------------------------------------------------| if the lengths are too different then ignore

			if(maxScore >= qryLenI){
				if (maxScore - qryLenI > qryLen * .50) return 0;
			}else{
				if (qryLenI - maxScore > qryLen * .50) return 0;		
			}


			//---------------------------------------------------------| if they dont share the same letters then ignore
			tally=0;
			for(int posInQryWrd=0; posInQryWrd<qryLenI; posInQryWrd++)
				if ( dictionaryWord.IndexOf(queryWordA[posInQryWrd]) >-1 ) tally++;
			if (tally < qryLenI - (qryLenI*.38)) return 0;


			//word fairly close so figure it's score more precisely
			return suggestionScore2b(dictionaryWord, queryWord);

		}

		char[] dictionaryWordA;
		char[] queryWordA;

		//step 2, do the hash to get a score for 2 words
		int suggestionScore2b(String dictionaryWord, String queryWord)
		{
			if(this.AllowCaseInsensitiveSuggestions)
			{
				dictionaryWord = dictionaryWord.ToLower();
				queryWord = queryWord.ToLower();
			}

			dictionaryWordA = dictionaryWord.ToCharArray();
			//lazy loading
			if(loadedQryWrd==null || !loadedQryWrd.Equals(queryWord)){
				loadedQryWrd = queryWord;
				//used in suggestions
				qryLenI = (short)queryWord.Length;
				qryLen = qryLenI;
				queryWordA = queryWord.ToCharArray();
			}

			maxScore = dictionaryWord.Length;
			tail = maxScore;
			agreement = 0;
			

			//taken.clear();
			taken = new int[maxScore];
			p=0;

			//---------------------------------------------------------| calculate the hashing score
			//go through each letter in queryWord and tally matched letters and matched patterns
			for(int posInDicWrd=0; posInDicWrd<maxScore; posInDicWrd++){
				smallestDistance=tail;

				//find distance of occurance in query word
				for(int posInQryWrd=0; posInQryWrd<qryLenI; posInQryWrd++){

                    isTaken = false;
                    if (
                        dictionaryWordA[posInDicWrd] == queryWordA[posInQryWrd]

                        // MD 11/13/07
                        // Found while fixing BR27454
                        // findCapitalizedSuggestions ws always false. It was remvoed and this condition will never be needed
                        //||
                        //(findCapitalizedSuggestions &&
                        //Char.IsLower(dictionaryWordA[posInDicWrd]) == Char.IsLower(queryWordA[posInQryWrd]) &&
                        //String.Compare(dictionaryWord, posInDicWrd, queryWord, posInQryWrd, 1, true)==0
                        //)

                    )
                    {
                        for (int i = 0; i < p; i++) if (taken[i] == posInQryWrd) isTaken = true;
                        if (!isTaken)
                        {

                            if (posInQryWrd > posInDicWrd)
                            {
                                distance = posInQryWrd - posInDicWrd;
                            }
                            else
                            {
                                distance = posInDicWrd - posInQryWrd;
                            }
                            if (distance < smallestDistance)
                            {
                                smallestDistance = distance;
                                chosenCharPos = posInQryWrd;
                            }

						}
					}

				}

				if(smallestDistance != tail){
					taken[p] = chosenCharPos;
					if(p<15)p++;
					agreement += ( (1000*(tail - smallestDistance)) / tail ) ;
				} 
			}

			return agreement/qryLenI;
		}
		#endregion



        /// <summary>
        /// Returns the original text sent to check() but with any alterations made through <c>change</c>.
        /// </summary>
        /// <returns>String of the text after corrections, null if there is no text.</returns>
		public virtual String GetAmendedText() 
		{
			if(theText != null)	return theText.ToString();
			else return null;
		}


        /// <summary>
        /// Marks <c>word</c> to be ignored in the rest of the text.
        /// </summary>
        /// <param name="word">to ignore</param>
        /// <returns>void</returns>
        /// <exception cref="ArgumentNullException">if word parameter is null.</exception>
		public virtual void IgnoreAll(String word)
		{
			if(word == null) throw new ArgumentNullException("Null String passed to ignoreAll() - ensure word parameter is not null.");
			ignoreList.Add(word);
		}

        /// <summary>
        /// Checks if the query is in the main dictionary or user.
        /// </summary>
        /// <remarks>This method is inconsiderate of the words in IgnoreList.</remarks>
        /// <param name="query">the word to check spelling of</param>
        /// <returns>True if spelt correctly, false if it is not in the dictionary.</returns>
        /// <exception cref="ArgumentNullException"> if query parameter is null.</exception>
		public virtual bool LookUp(String query){
			if(query == null) throw new ArgumentNullException("Null String passed to LookUp() - ensure query parameter is not null.");
			lock(this){
				if (LookUpMainDictionary(query)) return true;
				else if (LookUpUserDictionary(query)) return true;
				else {
					//word not found, but it could be a compound word
					if (checkCompoundWords){
						//return IsCompoundFormed(query);
						return FindCompoundWords(query, null);
					} else return false;
				}
			}
		}

        // MD 6/8/06 - BR12977
        // Created a function to see if the word is in the dont
        // suggest list because this operation was being in many places
        private bool IsWordInDontSuggestList(string word)
        {
            for (int i = 0; i < dontSuggest.Length; i++)
            {
                if (word.Equals(dontSuggest[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the compound words in the <c>text</c> and puts them in <c>subwords</c>.  Set <c>subwords</c> to null/nothing to just find if the <c>text</c> has a compound word formation.
        /// </summary>
        /// <remarks>
        /// Only adds words to the list if all are valid.  In other words, this will not find all words that are part of the <c>text</c>, but only the words that all make up the <c>text</c>.
        /// <p>E.g. "catchmentarea" has words "catchment" and "area" but not "cat", "me" etc.</p>
        /// The order of the words in the list is the reverse of their order in the <c>text</c>.
        /// </remarks>
        /// <param name="text">The text to find compound words in</param>
        /// <param name="subwords">A list that formative sub-words will be added to, can be null/nothing.</param>
        /// <returns>True if the <c>text</c> is entirely formed from compound words, False if not.</returns>
        /// <exception cref="ArgumentNullException">if text is null</exception>
        public virtual bool FindCompoundWords(String text, IList subwords)
        {
            String remainder;
            int minCompoundLength = 2;
            if (LanguageParser == LanguageType.Dutch) minCompoundLength = 3;
            //find longest substring that is in dictionary
            for (int p = text.Length; p > minCompoundLength; p--)
            {
                // MD 6/8/06 - BR12977
                // Cache first word for performance
                string firstWord = text.Substring(0, p);

                // MD 6/8/06 - BR12977
                // Changed if to used cahced first word and to make sure it is 
                // not a curse word.
                //see if substring is in dictionary
                //if (LookUpMainDictionary(text.Substring(0, p)) || LookUpUserDictionary(text.Substring(0, p)) )
                if (!this.IsWordInDontSuggestList(firstWord) && (LookUpMainDictionary(firstWord) || LookUpUserDictionary(firstWord)))
                {
                    //this one is, so check if remainder is in dictionary.
                    remainder = text.Substring(p);
                    //in German compound words are made of nouns, and nouns are always capitalized in the dic.
                    //so capitalize the remainder, otherwise nouns wont be passed as OK
                    if (LanguageParser == LanguageType.German) remainder = Char.ToUpper(remainder[0]) + remainder.Substring(1);

                    // MD 6/8/06 - BR12977
                    // Added check to make sure the remainder is not in the dont suggest list.
                    //if (text.Length - p > minCompoundLength && ( LookUpMainDictionary(remainder) || LookUpUserDictionary(remainder)) )
                    if (text.Length - p > minCompoundLength && !this.IsWordInDontSuggestList(remainder) &&
                        (LookUpMainDictionary(remainder) || LookUpUserDictionary(remainder)))
                    {
                        //whole remainder is in dictionary, so add both to subwords
                        if (subwords != null)
                        {
                            subwords.Add(remainder);
                            subwords.Add(text.Substring(0, p));
                        }
                        return true;
                    }
                    else if (FindCompoundWords(remainder, subwords))
                    {
                        //remainder is  compound itself, so add main part
                        if (subwords != null) subwords.Add(text.Substring(0, p));
                        return true;

					}
				}
			}
			//no substrings found with valid remainders, return false.
			return false;

		}

        /// <summary>
        /// Finds correctly spelled words in the <c>text</c> and puts them in <c>subwords</c>.  Set <c>subwords</c> to null/nothing to just find if the <c>text</c> has compound word formation.
        /// </summary>
        /// <remarks>
        /// Only adds words to the list if all are valid.  In other words, this will not find all words that are part of the <c>text</c>, but only the words that all make up the <c>text</c>.
        /// <p>E.g. "catchmentarea" has words "catchment" and "area" but not "cat", "me" etc.</p>
        /// The order of the words in the list is the reverse of their order in the <c>text</c>.
        /// </remarks>
        /// <param name="text">The text to find compound words in</param>
        /// <param name="subwords">A list that formative sub-words will be added to, can be null/nothing.</param>
        /// <returns>True if the <c>text</c> is entirely formed from compound words, False if not.</returns>
        /// <exception cref="ArgumentNullException">if text is null</exception>
		public virtual bool FindSplitWords(String text, IList subwords)
		{
			String remainder;
			int minCompoundLength = this.SplitWordThreshold;
			
			//find longest substring that is in dictionary
			for(int p=text.Length; p>=minCompoundLength; p--)
			{
				//MD 5/25/06 - BR12977
				string firstWord = text.Substring( 0, p );

                //see if substring is in dictionary
                //MD 5/25/06 - BR12977
                //if (LookUpMainDictionary(text.Substring(0, p)) || LookUpUserDictionary(text.Substring(0, p)) )
                if (LookUpMainDictionary(firstWord) || LookUpUserDictionary(firstWord))
                {

                    // MD 6/8/06 - BR12977
                    // Moved code in the following section to a method call
                    #region Old Code
                    //MD 5/25/06 - BR12977
                    // Added the following check below because split word suggestions 
                    // containing explitives were being provided.  Now if part of the 
                    // split word is contained in the don't suggest list, the suggestion
                    // is not provided.  The changes above are optimizations which aided
                    // in the check below.
                    // 
                    //bool dontSuggestWordFound = false;
                    //
                    //for ( int k = 0; k < dontSuggest.Length; k++ )
                    //{
                    //    if ( firstWord.Equals( dontSuggest[ k ] ) )
                    //    {
                    //        dontSuggestWordFound = true;
                    //        break;
                    //    }
                    //}
                    //
                    //if ( dontSuggestWordFound )
                    //    continue;
                    #endregion Old Code

                    if (this.IsWordInDontSuggestList(firstWord))
                        continue;

                    //this one is, so check if remainder is in dictionary.
                    remainder = text.Substring(p);
                    //in German compound words are made of nouns, and nouns are always capitalized in the dic.
                    //so capitalize the remainder, otherwise nouns wont be passed as OK
                    // MD 6/17/08 - BR33967
                    // The remainder could be an empty string if this is the first iteration of the loop.
                    // In that case, we don't need to make the first character upper-case.
                    //if(LanguageParser == LanguageType.German) remainder = Char.ToUpper(remainder[0]) + remainder.Substring(1);
                    if (String.IsNullOrEmpty(remainder) == false && this.LanguageParser == LanguageType.German)
                        remainder = Char.ToUpper(remainder[0]) + remainder.Substring(1);

                    // MD 6/8/06 - BR12977
                    // Moved code in the following section to a method call
                    //// MD 6/2/06 - BR12977
                    //// Added check for a dont suggest word in the remainder
                    //dontSuggestWordFound = false;
                    //for ( int k = 0; k < dontSuggest.Length; k++ )
                    //{
                    //    if ( remainder.Equals( dontSuggest[ k ] ) )
                    //    {
                    //        dontSuggestWordFound = true;
                    //        break;
                    //    }
                    //}

                    // MD 6/8/06 - BR12977
                    // Moved check for word in dont suggest list into method call
                    //// MD 6/2/06 - BR12977
                    //// Added check for a dont suggest word in the remainder
                    ////if (text.Length - p >= minCompoundLength && ( LookUpMainDictionary(remainder) || LookUpUserDictionary(remainder)) )
                    //if ( !dontSuggestWordFound && text.Length - p >= minCompoundLength && ( LookUpMainDictionary( remainder ) || LookUpUserDictionary( remainder ) ) )
                    if (!this.IsWordInDontSuggestList(remainder) && text.Length - p >= minCompoundLength &&
                        (LookUpMainDictionary(remainder) || LookUpUserDictionary(remainder)))
                    {
                        //whole remainder is in dictionary, so add both to subwords
                        if (subwords != null)
                        {
                            subwords.Add(remainder);
                            subwords.Add(text.Substring(0, p));
                        }
                        return true;
                    }
                    else if (FindCompoundWords(remainder, subwords))
                    {
                        //remainder is  compound itself, so add main part
                        if (subwords != null) subwords.Add(text.Substring(0, p));
                        return true;

					}
				}
			}
			//no substrings found with valid remainders, return false.
			return false;

		}

		/// <summary>Finds anagrams of <c>word</c> and puts them in <c>anagrams</c> if not already present.</summary>
		/// <remarks></remarks>
		/// <param name="word">The word to find anagrams of</param>
		/// <param name="anagrams">A list that anagrams will be added to.</param>
		/// <exception cref="ArgumentNullException">if word is null/nothing</exception>
		public virtual void FindAnagrams(String word, IList anagrams){
			//initialize position array of charater pointers, which point to characters in string.
			//eg. if word = 'a', 'b', 'c'
			//loop through pos perms.  eg pos[0] = 0..2 pos[1] = 0..2 pos[2] = 0..2
			//anagram 1 => pos[0]=1 pos[1]=0 pos[2]=2 => "bac"
			int[] pos = new int[word.Length];
			
			Permuter p = new Permuter(word.Length);
			StringBuilder anagram = new StringBuilder(word,word.Length);
			for(int i=0; i<p.GetNumberOfPermutations(); i++){
				for(int c=0; c<p.GetPermutation(i).Length; ++c){
					anagram[c] = word[ p.GetPermutation(i)[c] - 1 ];
				}

				//check if anagram is a word
				if (LookUp(anagram.ToString()) && !anagrams.Contains(anagram.ToString())){
					//see if word is in dontSuggest array
					bool dontSuggestWord = false;
					string anagramLower = anagram.ToString().ToLower();
					for(int k=0; k<dontSuggest.Length; k++)
						if(anagramLower.ToString().Equals(dontSuggest[k])) dontSuggestWord = true;

					if(!dontSuggestWord)			
						anagrams.Add(anagram.ToString());
				}
			}
		}

        internal void WriteUserDictionary(string fileName)
        {
            this._userDictionary.WriteDict(fileName);
        }
        /// <summary>
        /// Set the user dictionary to be used.
        /// </summary>
        /// <param name="userDictionaryFile">A file to be used as a user dictionary.</param>
        /// <exception cref="ArgumentNullException">If userDictionaryFile is null</exception>
		public void SetUserDictionary(Stream userDictionaryFile)
		{
			//MD 5/18/06
			// If the user dictionary file is changed at run-time, stop
			// from ignoring the words in the old user dictionary
			// MD 11/13/07 - BR27454
            // We don't keep a separate list of the words anymore
            // Use the SortedWords collection on the user dictionary
            //foreach ( string word in this.userDicList )
            //    this.ignoreList.Remove( word );
            if (this.userDictionary != null)
            {
                foreach (string word in this.userDictionary.SortedWords)
                    this.ignoreList.Remove(word);
            }

            if (userDictionaryFile == null || userDictionaryFile.Equals(""))
            {//throw new ArgumentNullException("Null user dictionary file passed to SetUserDictionary().");
                this.userDictionary = null;
            }
            else
            {
                this.userDictionary = new UserDictionary(userDictionaryFile);
                // MD 11/13/07 - BR27454
                // We don't keep a separate list of the words anymore
                // We will just use the SortedWords collection on the user dictionary
                //this.userDictionary.ReadAll(userDicList);
            }
        }

        /// <summary>
        /// Set the user dictionary to be used.
        /// </summary>
        /// <remarks>If the userDictionary can not be created/read it will be ignored.</remarks>
        /// <param name="userDictionary">a UserDictionary object representing a user dictionary</param>
        /// <exception cref="ArgumentNullException">if userDictionary is null.</exception>
        public void SetUserDictionary(UserDictionary userDictionary)
        {
            //MD 5/18/06
            // If the user dictionary file is changed at run-time, stop
            // from ignoring the words in the old user dictionary
            // MD 11/13/07 - BR27454
            // We don't keep a separate list of the words anymore
            // Use the SortedWords collection on the user dictionary
            //foreach ( string word in this.userDicList )
            //    this.ignoreList.Remove( word );
            if (this.userDictionary != null)
            {
                foreach (string word in this.userDictionary.SortedWords)
                    this.ignoreList.Remove(word);
            }

            if (userDictionary == null) throw new ArgumentNullException("userDictionary");
            this.userDictionary = userDictionary;
            // MD 11/13/07 - BR27454
            // We don't keep a separate list of the words anymore
            // We will just use the SortedWords collection on the user dictionary
            //this.userDictionary.ReadAll(userDicList);
        }

		/// Capitalises first letter of word
		private String MakeCap(String word)
		{
			if(word.Length > 1)
				word = word.ToUpper()[0] + word.Substring(1);
			else 
				word = word.ToUpper();
			return word;
		}

		//whether all chars in s are in upper case
		bool isAllCaps(string s){
			for(int i=0; i<s.Length; i++)
				if(Char.IsLower(s[i])) return false;
			return true;
		}

		//whether word is in mixed caps.
		bool isMixedCase(string s){
			bool hasUpper = false;
			bool hasLower = false;
			int lookFrom = Char.IsUpper(s[0])?1:0;
			
			for(int i=lookFrom; i<s.Length; i++){
				if(Char.IsLower(s[i])) {
					hasLower = true;
					if(hasUpper) return true;
				} else if(Char.IsUpper(s[i])) {
					hasUpper = true;
					if(hasLower) return true;
				}
			}
			return false;
		}

        /// <summary>
        /// Checks if the query is in the word dictionary.  Returns true if it doesnt start with a letter or apostrophe.
        /// </summary>
        /// <param name="query">the word to check spelling of</param>
        /// <returns>Boolean true if spelt correctly, false if not in the dictionary.</returns>
		protected virtual bool LookUpMainDictionary(String query) 
		{

			LoadLexicon();

			//smart quotes must be converted to regular apostrophes
			query = query.Replace('\u2019', '\'');

			//avoid trying to lookup anything that is empty or one char
			if(query.Length <= 1) return true;
			else {
				//must remove formatting
				if(AllowXML){
					int openPos, closePos;
					while( (openPos=query.IndexOf('<')) > -1 && (closePos=query.IndexOf('>', openPos)) > -1){
						//remove everything between < and > inclusively
						query = query.Substring(0, openPos) + query.Substring(closePos+1);
					}
				}

				//Rules for caps.
				//1. MIxEd case, is valid if dictionary matches exactly
				//2. Capitalized is valid if dictionary matches in upper or lower case, but could appear in dic as upper first
				//char only
				//3. CAPITAL is valid if word is in dic in any case, but could appear in dic as upper first
				//char only
				//4. lower is valid if word matches in lower case

				//Process, 
				//P1. if word is in "CAPS", drop it to "Caps"
				//P2. try word.
				//P3. if word is in "Caps" then drop it to "caps" and try


				//if we should allow any case then make word ALL caps and try to find match in dic
				if(allowAnyCase) query = query.ToUpper();

                // -- CHECK CAPITAL ONLY WORDS P1
                //--------------------------------
                //drop it to Capitals in case its a CAPITALIZED proper name
                if (isAllCaps(query))
                {
                    if (allowCapitalizedWords) return true;
                    //try the word as-is, incase its an ACRONYM

                    // MD 11/13/07
                    // Found while fixing BR27454
                    // Don't store an instance of this with each spell checker.
                    // Moved this to be a static readonly field on the comparer itself
                    //if (mainDictionary.BinarySearch(query, invariantComparer) >= 0) return true;		//found it
                    if (mainDictionary.BinarySearch(query, InvariantComparer.Instance) >= 0)
                        return true;		//found it

                    //this is CAPITALIZED word, so drop it to Capitalized
                    query = query[0] + query.Substring(1).ToLower();
                }

                // -- Try word (CAPITALS will now be Capitals) P2
                //-----------------------------------------------
                // MD 11/13/07
                // Found while fixing BR27454
                // Don't store an instance of this with each spell checker.
                // Moved this to be a static readonly field on the comparer itself
                //if (mainDictionary.BinarySearch(query, invariantComparer) >= 0) return true;			//found it
                if (mainDictionary.BinarySearch(query, InvariantComparer.Instance) >= 0)
                    return true;			//found it

                // -- CHECK lower WORDS P3
                //-------------------------
                //check case, see if any capitalization, must check in lower case also
                //ie. words at start of sentance are capitalized, but lower case in dic
                if (!isMixedCase(query) || allowMixedCase)
                {
                    // MD 11/13/07
                    // Found while fixing BR27454
                    // Don't store an instance of this with each spell checker.
                    // Moved this to be a static readonly field on the comparer itself
                    //if (mainDictionary.BinarySearch(query.ToLower(), invariantComparer) >= 0) return true;	//found it
                    if (mainDictionary.BinarySearch(query.ToLower(), InvariantComparer.Instance) >= 0)
                        return true;	//found it
                }


					if (checkHyphenatedText) {
						//couldn't find word, check if it is hyphenated, and look at components if it is.
						int hyphenPos;
						if ( (hyphenPos = query.IndexOf('-')) > -1) //if hyphen exits, lookup word before and after hyphen
							return LookUpMainDictionary(query.Substring(0, hyphenPos)) && LookUpMainDictionary(query.Substring(hyphenPos + 1));					
					}
	
			}
			return false;
		}		


		/// Look up the query in the user dictionary if it exists 
		protected virtual bool LookUpUserDictionary(String query)
		{

			if(query==null) return false;



			//avoid trying to lookup anything that is empty or one char
			if(query.Length <= 1) return true;
			else {
				//must remove formatting
				if(AllowXML){
					int openPos, closePos;
					while( (openPos=query.IndexOf('<')) > -1 && (closePos=query.IndexOf('>', openPos)) > -1){
						//remove everything between < and > inclusively
						query = query.Substring(0, openPos) + query.Substring(closePos+1);
					}
				}


                // MD 11/13/07 - BR27454
                // Don't do a linear search over the list, it could be very large.
                // Do a binary search with the new SrotedWords collection on the user dictionary
                //if(userDictionary!=null && userDictionary.IsValid()){
                //    for(int i=0; i<userDicList.Count; i++){
                //        if(((String)userDicList[i]).Equals(query)  || (!query.Equals(query.ToLower()) && ((String)userDicList[i]).Equals(query.ToLower())) )
                //            return true;
                //    }
                //}
                if (this.userDictionary != null && userDictionary.IsValid())
                {
                    int index = this.userDictionary.SortedWords.BinarySearch(query, InvariantComparer.Instance);

                    if (index < 0)
                    {
                        string lowerQuery = query.ToLower(CultureInfo.CurrentCulture);

                        if (String.Equals(lowerQuery, query) == false)
                            index = this.userDictionary.SortedWords.BinarySearch(lowerQuery, InvariantComparer.Instance);
                    }

                    return index >= 0;
                }

			}
			return false;
		}


		/// Returns the previous word in the text, and moves word pointers in process.
		/// <returns>String the next word in the text</returns>
		internal String GetPreviousWord() 
		{
			wordEnd = wordStart;
			if(wordStart > 0) 
			{
				wordStart = wordIterator.Preceding( wordStart );
				if(wordStart<0)return null;	//happens when we've moved past the beginning

				//exclude anything with length less than or equal to 0 and anything else with white space.
				//because the word instance of BreakIterator groups any white space together (spaces, tabs, newlines, carriage
				//returns etc) they must be ignored. Eg: a space followed by a tab is a 2 character 'word' which must be ignored.
				if(wordEnd-wordStart <= 0 
					|| !Char.IsLetterOrDigit( theText[wordStart] ) 
					|| 
					(
					ignoreRtfCodes &&
					wordStart > 0 &&
					theText.ToString()[wordStart-1] == '\\'
					)					//RTF CODES
					)
				{

					//ignore and move on to next word
					return GetPreviousWord();
				}

				//return as a word
				return theText.ToString().Substring(wordStart, wordEnd - wordStart);
			} 
			else 
			{
				return null;
			}
		}

        /// <summary>Returns the next word in the text.</summary>
        /// <returns>String of the next word in the text.</returns>
		protected virtual internal String GetNextWord() {

			wordStart = wordEnd;


			if(wordEnd < wordIterator.Last()) {

				wordEnd = wordIterator.Following( wordEnd );



				//exclude anything with length less than or equal to 0 and anything else with white space.
				//because the word instance of BreakIterator groups any white space together (spaces, tabs, newlines, carriage
				//returns etc) they must be ignored. Eg: a space followed by a tab is a 2 character 'word' which must be ignored.
				if(wordEnd-wordStart <= 0 
					|| !Char.IsLetterOrDigit( theText[wordStart] ) 
					|| 
					(
						ignoreRtfCodes &&
						wordStart > 0 &&
						theText.ToString()[wordStart-1] == '\\'
					)					//RTF CODES
					){

					//ignore and move on to next word
					return GetNextWord();
				}

				//return as a word
				return theText.ToString().Substring(wordStart, wordEnd - wordStart);
			} else {
				return null;
			}
		}


		
		internal bool Flagged(String word){
			if(badWordList.Contains(word)) return true;
			else return false;
		}

		//check if a word has numbers in it
		private bool WordHasDigits(string w){
			for(int i=0; i<w.Length; i++) if(Char.IsDigit(w[i])) return true;
			return false;
		}

		//check if a 'word' is actually a number
		private bool WordIsDigits(string w){
			for(int i=0; i<w.Length; i++) if(!Char.IsDigit(w[i])) return false;
			return true;
		}

		/// Scan theText and record all mis-spelt words in badWordList and return the word pointers to the start.
		private void PreCheck(){
			String w;
			badWordList.Clear();
			//start by adding all words not in dictionary to a list
			while((w = GetNextWord())!=null){

				if (!badWordList.Contains(w)){
					if (!LookUpMainDictionary(w)){
						//check if we're ignoring words with digits	
						if(!(AllowWordsWithDigits && WordHasDigits(w))){
							//check if just a number
							if(!WordIsDigits(w))
								badWordList.Add(w);
						}
					}
				}

			}

			Reset();


			//check all bad word list words against user dictionary and compound forms
			for(IEnumerator e=(new List<String> (badWordList)).GetEnumerator(); e.MoveNext();){
				//if the bad word is in the user dictionary, remove it from the list
				w = (String) e.Current;
				if (LookUpUserDictionary(w)) badWordList.Remove(w);
				else {
					//word not found, but it could be a compound word
					if (checkCompoundWords){
						if(FindCompoundWords(w, null)){
							badWordList.Remove(w);
						}
					}
				}
			}
		}


		private void Reset()
		{
			SetPosition(0);
		}


		///	<summary>Sets the pointer position for the <c>NextBadWord</c> iterator.</summary>
		///	<remarks>Further calls to <c>NextBadWord</c> will look for the next bad word from position <c>pos</c>.
		///	If position &gt; the text length it is set to the text length.
		///	If position &lt; 0, it is set to zero.</remarks>
		public virtual void SetPosition(int pos)
		{
			if(pos < 0) pos = 0;
			if(pos > theText.Length) pos = theText.Length;
			wordEnd = pos;
			previousWord = "";
		}


		/// Generate a big array of phonetic codes for the dictionary, this will save reprocessing redundently,
		/// at the expense of memory.
		private void GeneratePhoneticList(){
			mainDicListPhonetic = new List<String>(160000);
			for(int i=0; i<numWords; i++)
				mainDicListPhonetic.Add( PhoneticsProcessor.MetaPhone((String) mainDictionary[i]) );
			
		}


        // MD 11/13/07 - BR27454
        // Changed to a generic comparer for performance
        //internal class InvariantComparer : IComparer 
        internal class InvariantComparer : IComparer<String>
        {
            // MD 11/13/07
            // Found while fixing BR27454
            // We don't want to store an instance of this with each spell checker.
            // Instead use a static readon;y field on the class
            internal static readonly InvariantComparer Instance = new InvariantComparer();

            private System.Globalization.CompareInfo m_compareInfo;

            // MD 11/13/07
            // Found while fixing BR27454
            // Since this will be a singleton now, use a protected contructor
            //internal InvariantComparer() 
            protected InvariantComparer()
            {
                m_compareInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            }

            public virtual int Compare(String a, String b)
			{
				String sa = a as String;
				String sb = b as String;
				if (sa != null && sb != null)
					return m_compareInfo.Compare(sa, sb);
				else
					return Comparer<String>.Default.Compare(a,b);
			}
		}

		class CompareL : IComparer<String>{

			
			private String topWord;
			private SpellChecker rsc;

			public CompareL(SpellChecker r){
				this.rsc = r;
			}

            public int Compare(string a, string b)
			{
				return (int)((rsc.suggestionScore2b(topWord, (String)b) - rsc.suggestionScore2b(topWord, (String)a)));
			}

			public void with(String w){ topWord = w; }


			//calcs number of chars in word1 but not in word2
			private int numberDifferentChars(String word1, String word2) 
			{
				int end1 = word1.Length, end2 = word2.Length;
				int difference=0;
				bool found;

				for(int c=0; c<end1; c++){
					found = false;
					for(int d=0; d<end2; d++){
						if(word1[c] == word2[d]){
							found = true;
						}
					}

					if(!found){
						difference++;
					}

				}

				return difference;
			}

		}


        /////Sort by length of string.
        //class SizeSorter : IComparer{

        //    public int Compare(object a, object b)
        //    {
        //        return ((String)a).Length - ((String)b).Length;
        //    }
        //}

		///Sort by length of string.
        class ReverseSorter : InvariantComparer
        {

            // MD 11/13/07
            // Found while fixing BR27454
            // We don't want to store an instance of this with each spell checker.
            // Instead use a static readon;y field on the class
            // Also added protected contructor so the class couldn't be created externally
            internal new static readonly ReverseSorter Instance = new ReverseSorter();
            protected ReverseSorter() { }

            public override int Compare(string a, string b)
			{
				return base.Compare(Reverse((String)a), Reverse((String)b));
			}

			public static String Reverse(String s){
				char[] cs = new char[s.Length];
				for(int i=s.Length; i>0; i--)	cs[s.Length - i] = s[i-1];
				return new String(cs);
			}

		}

		/// The resource decoder, which takes the byte array from wordlist and decodes it.
		internal class ResourceDecoder{
			
			Decoder uniDecoder = null;
			String decoderType = "";


			/// Decode byte array in to list, returns number of words in list, is thread safe
            public int DecodeWordList(List<String> list, byte[] bytes)
            {
				return DecodeWordList(list,bytes,"Unicode");
			}

			/// Decode byte array in to list, returns number of words in list, is thread safe
			/// encoding is either "UTF8" or "Unicode"
            public int DecodeWordList(List<String> list, byte[] bytes, String encoding)
            {
				int charCount=0;
				if(!decoderType.Equals(encoding)){
					//if(encoding.Equals("UTF8")) uniDecoder = utf8Encoding.GetDecoder();
					if(encoding.Equals("UTF8")) uniDecoder = Encoding.UTF8.GetDecoder();
					if(encoding.Equals("Unicode")) uniDecoder = Encoding.Unicode.GetDecoder();
					decoderType = encoding;
				}

				list.Clear();
				list.Capacity = 300000;

				for(int i=0; i<bytes.Length; i++){
					//bytes[i] = decodeByte(bytes[i]);
					bytes[i] = (byte)(  (((int)bytes[i]) - 50) % 256);
				}
				if(encoding.Equals("Unicode")) charCount = bytes.Length/2;
				if(encoding.Equals("UTF8")) charCount = Encoding.UTF8.GetCharCount(bytes);

				char[] chars = new char[charCount];
				uniDecoder.GetChars(bytes, 0, bytes.Length, chars, 0);
				String wordlist = new String(chars);


				int pos=-1, ppos=0;
				while( (pos = wordlist.IndexOf('\n', ppos)) > -1 ){
					list.Add( wordlist.Substring(ppos, pos-ppos) );	
					ppos = pos + 1;
				}
				list.Add( wordlist.Substring(ppos) );

				return list.Count;
			}

			byte decodeByte(int b){
				return (byte)( (b - 50) % 256  );
			}

		}

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