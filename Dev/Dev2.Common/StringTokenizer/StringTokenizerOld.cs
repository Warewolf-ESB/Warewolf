using System;
using System.Collections;
using System.Text;


namespace Dev2.Common {
    /// <summary>
    /// A String Tokenizer that accepts Strings as source and delimiter. Only 1 delimiter is supported (either String or char[]).
    /// </summary>
    public class StringTokenizerOld {
        private int CurrIndex;
        private int NumTokens;
        private ArrayList tokens;
        private string OldSource;
        private string StrSource;
        private string StrDelimiter;
        private bool _IncludeDelimter;

        /// <summary>
        /// Constructor for StringTokenizerOld Class. 
        /// </summary>
        /// <param name="source">The Source String.</param>
        /// <param name="delimiter">The Delimiter String. If a 0 length delimiter is given, " " (space) is used by default.</param>
        public StringTokenizerOld(string source, string delimiter) {
            this.tokens = new ArrayList(10);
            this.StrSource = source;
            this.StrDelimiter = delimiter;

            if (delimiter.Length == 0) {
                this.StrDelimiter = " ";
            }
            this.Tokenize();
        }

        /// <summary>
        /// Constructor for StringTokenizerOld Class.
        /// </summary>
        /// <param name="source">The Source String.</param>
        /// <param name="delimiter">The Delimiter String. If a 0 length delimiter is given, " " (space) is used by default.</param>
        /// <param name="includeDelimter" value="false">The includeDelimter value.</param>
        public StringTokenizerOld(string source, string delimiter, bool includeDelimter = false) {
            this.tokens = new ArrayList(10);
            this.StrSource = source;
            this.OldSource = source;
            this.StrDelimiter = delimiter;
            this._IncludeDelimter = includeDelimter;

            if (delimiter.Length == 0) {
                this.StrDelimiter = " ";
            }
            this.Tokenize();
        }

        /// <summary>
        /// Constructor for StringTokenizerOld Class.
        /// </summary>
        /// <param name="source">The Source String.</param>
        /// <param name="delimiter">The Delimiter String as a char[].  Note that this is converted into a single String and
        /// expects Unicode encoded chars.</param>
        public StringTokenizerOld(string source, char[] delimiter)
            : this(source, new string(delimiter)) {
        }

        /// <summary>
        /// Constructor for StringTokenizerOld Class.  The default delimiter of " " (space) is used.
        /// </summary>
        /// <param name="source">The Source String.</param>
        public StringTokenizerOld(string source)
            : this(source, "") {
        }

        /// <summary>
        /// Empty Constructor.  Will create an empty StringTokenizerOld with no source, no delimiter, and no tokens.
        /// If you want to use this StringTokenizerOld you will have to call the NewSource(string s) method.  You may
        /// optionally call the NewDelim(string d) or NewDelim(char[] d) methods if you don't with to use the default
        /// delimiter of " " (space).
        /// </summary>
        public StringTokenizerOld()
            : this("", "") {
        }

        private void Tokenize() {
            string TempSource = this.StrSource;
            string Tok = "";
            this.NumTokens = 0;
            this.tokens.Clear();
            this.CurrIndex = 0;

            if (TempSource.IndexOf(this.StrDelimiter) < 0 && TempSource.Length > 0) {
                this.NumTokens = 1;
                this.CurrIndex = 0;
                this.tokens.Add(TempSource);
                this.tokens.TrimToSize();
                TempSource = "";
            }
            else if (TempSource.IndexOf(this.StrDelimiter) < 0 && TempSource.Length <= 0) {
                this.NumTokens = 0;
                this.CurrIndex = 0;
                this.tokens.TrimToSize();
            }
            while (TempSource.IndexOf(this.StrDelimiter) >= 0) {
                //Delimiter at beginning of source String.
                //if (TempSource.IndexOf(this.StrDelimiter) == 0) {
                //    if (TempSource.Length > this.StrDelimiter.Length) {
                //        //TempSource = TempSource.Substring(this.StrDelimiter.Length);
                //    }
                //    else {
                //        TempSource = "";
                //    }
                //}
                //else {
                if (_IncludeDelimter) {
                    Tok = TempSource.Substring(0, TempSource.IndexOf(this.StrDelimiter));
                    AddToken(Tok);
                    AddToken(this.StrDelimiter);
                    if (TempSource.Length > (this.StrDelimiter.Length + Tok.Length)) {
                        TempSource = TempSource.Substring(this.StrDelimiter.Length + Tok.Length);
                    }
                    else {
                        TempSource = "";
                    }
                }
                else {
                    Tok = TempSource.Substring(0, TempSource.IndexOf(this.StrDelimiter));
                    AddToken(Tok);
                    if (TempSource.Length > (this.StrDelimiter.Length + Tok.Length)) {
                        TempSource = TempSource.Substring(this.StrDelimiter.Length + Tok.Length);
                    }
                    else {
                        TempSource = "";
                    }
                    //}
                }
            }
            //we may have a string leftover.
            if (TempSource.Length > 0) {
                AddToken(TempSource);
            }
            this.tokens.TrimToSize();
            this.NumTokens = this.tokens.Count;
        }

        /// <summary>
        /// Method to add or change this Instance's Source string.  The delimiter will
        /// remain the same (either default of " " (space) or whatever you constructed 
        /// this StringTokenizerOld with or added with NewDelim(string d) or NewDelim(char[] d) ).
        /// </summary>
        /// <param name="newSrc">The new Source String.</param>
        public void NewSource(string newSrc) {
            this.StrSource = newSrc;
            this.Tokenize();
        }

        /// <summary>
        /// Method to change this Instance's IncudeDelimiter value.  The delimiter will
        /// remain the same (either default of " " (space) or whatever you constructed 
        /// this StringTokenizerOld with or added with NewDelim(string d) or NewDelim(char[] d) ).
        /// </summary>
        /// <param name="includeDel">The new IncludeDelimiter value.</param>
        public void IncludeDelimiter(bool includeDel) {
            this._IncludeDelimter = includeDel;
            this.Tokenize();
        }

        public void AddToken(string token) {
            if (!string.IsNullOrEmpty(token)) {
                this.tokens.Add(token);
            }
        }

        /// <summary>
        /// Method to add or change this Instance's Delimiter string.  The source string
        /// will remain the same (either empty if you used Empty Constructor, or the 
        /// previous value of source from the call to a parameterized constructor or
        /// NewSource(string s)).
        /// </summary>
        /// <param name="newDel">The new Delimiter String.</param>
        public void NewDelim(string newDel) {
            if (newDel.Length == 0) {
                this.StrDelimiter = " ";
            }
            else {
                this.StrDelimiter = newDel;
            }
            this.Tokenize();
        }

        /// <summary>
        /// Method to add or change this Instance's Delimiter string.  The source string
        /// will remain the same (either empty if you used Empty Constructor, or the 
        /// previous value of source from the call to a parameterized constructor or
        /// NewSource(string s)).
        /// </summary>
        /// <param name="newDel">The new Delimiter as a char[].  Note that this is converted into a single String and
        /// expects Unicode encoded chars.</param>
        public void NewDelim(char[] newDel) {
            string temp = new String(newDel);
            if (temp.Length == 0) {
                this.StrDelimiter = " ";
            }
            else {
                this.StrDelimiter = temp;
            }
            this.Tokenize();
        }

        /// <summary>
        /// Method to get the number of tokens in this StringTokenizerOld.
        /// </summary>
        /// <returns>The number of Tokens in the internal ArrayList.</returns>
        public int CountTokens() {
            return this.tokens.Count;
        }

        /// <summary>
        /// Method to probe for more tokens.
        /// </summary>
        /// <returns>true if there are more tokens; false otherwise.</returns>
        public bool HasMoreTokens() {
            if (this.CurrIndex <= (this.tokens.Count - 1)) {
                return true;
            }
            else {
                return false;
            }
        }

        public void RemoveWhiteSpace() {
            for (int i = 0; i < this.tokens.Count; i++) {
                this.tokens[i] = ((string)this.tokens[i]).Replace(" ", ""); ;
            }
        }

        public void RemoveTabes() {
            for (int i = 0; i < this.tokens.Count; i++) {
                this.tokens[i] = ((string)this.tokens[i]).Replace("\t", ""); ;
            }
        }

        public string UseIndexOf(int index) {
            string outputString = string.Empty;
            try {
                outputString = StrSource.Remove(index);
                StrSource = StrSource.Substring(index);
            }
            catch (Exception) {
                outputString = StrSource;
            }
            return outputString;
        }

        /// <summary>
        /// Method to get the next (string)token of this StringTokenizerOld.
        /// </summary>
        /// <returns>A string representing the next token; null if no tokens or no more tokens.</returns>
        public string NextToken() {
            String RetString = "";
            if (this.tokens.Count > 0) {
                RetString = (string)tokens[0];
                StrSource = StrSource.Substring(StrSource.IndexOf(RetString) + RetString.Length);
                Tokenize();
                return RetString;
            }
            else {
                Tokenize();
                return null;
            }
        }

        /// <summary>
        /// Gets the Source string of this Stringtokenizer.
        /// </summary>
        /// <returns>A string representing the current Source.</returns>
        public string Source {
            get {
                return this.StrSource;
            }
        }

        /// <summary>
        /// Gets the Delimiter string of this StringTokenizerOld.
        /// </summary>
        /// <returns>A string representing the current Delimiter.</returns>
        public string Delim {
            get {
                return this.StrDelimiter;
            }
        }

    }
}
