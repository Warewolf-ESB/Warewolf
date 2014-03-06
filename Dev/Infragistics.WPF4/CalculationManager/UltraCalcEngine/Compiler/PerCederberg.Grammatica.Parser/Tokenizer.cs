/*
 * Tokenizer.cs
 *
 * This work is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published
 * by the Free Software Foundation; either version 2 of the License,
 * or (at your option) any later version.
 *
 * This work is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software 
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307
 * USA
 *
 * As a special exception, the copyright holders of this library give
 * you permission to link this library with independent modules to
 * produce an executable, regardless of the license terms of these
 * independent modules, and to copy and distribute the resulting
 * executable under terms of your choice, provided that you also meet,
 * for each linked independent module, the terms and conditions of the
 * license of that module. An independent module is a module which is
 * not derived from or based on this library. If you modify this
 * library, you may extend this exception to your version of the
 * library, but you are not obligated to do so. If you do not wish to
 * do so, delete this exception statement from your version.
 *
 * Copyright (c) 2003 Per Cederberg. All rights reserved.
 */

using System.Collections;
using System.IO;
using System.Text;
using PerCederberg.Grammatica.Parser.RE;

#pragma warning disable 1570

namespace PerCederberg.Grammatica.Parser {

    /**
     * A character stream tokenizer. This class groups the characters read 
     * from the stream together into tokens ("words"). The grouping is
     * controlled by token patterns that contain either a fixed string to
     * search for, or a regular expression. If the stream of characters 
     * don't match any of the token patterns, a parse exception is thrown. 
     *
     * @author   Per Cederberg
     * @version  1.4
     */
    public class Tokenizer {
    
        /**
         * The token list feature flag.
         */
        private bool useTokenList = false;

        /**
         * The string token matcher. This token matcher is used for all
         * string token patterns. This matcher implements a DFA to 
         * provide maximum performance.
         */
        private StringTokenMatcher stringMatcher = new StringTokenMatcher();

        /**
         * The list of all regular expression token matchers. These 
         * matchers each test matches for a single regular expression.
         */
        private ArrayList regexpMatchers = new ArrayList();

        /**
         * The input stream to read from. When this is set to null, no
         * further input is available.
         */
        private TextReader input = null;

        /**
         * The buffer with previously read characters. Normally characters
         * are appended in blocks to this buffer, and for every token that
         * is found, its characters are removed from the buffer.
         */
        private StringBuilder buffer = new StringBuilder();

        /**
         * The current position in the string buffer.
         */
        private int position = 0;

        /**
         * The line number of the first character in the buffer. This 
         * value will be incremented when reading past line breaks. 
         */
        private int line = 1;

        /**
         * The column number of the first character in the buffer. This 
         * value will be updated for every character read. 
         */    
        private int column = 1;
    
        /**
         * The end of buffer read flag. This flag is set if the end of
         * the buffer was encountered while matching token patterns.
         */
        private bool endOfBuffer = false;

        /**
         * The previous token in the token list.
         */
        private Token previousToken = null; 

        /**
         * Creates a new tokenizer for the specified input stream.
         * 
         * @param input          the input stream to read
         */
        public Tokenizer(TextReader input) {
            this.input = input;
        }

        /**
         * Checks if the token list feature is used. The token list 
         * feature makes all tokens (including ignored tokens) link to 
         * each other in a linked list. By default the token list feature 
         * is not used.
         *
         * @return true if the token list feature is used, or 
         *         false otherwise
         *
         * @see #setUseTokenList
         * @see Token#getPreviousToken
         * @see Token#getNextToken
         *
         * @since 1.4
         */
        public bool GetUseTokenList() {
            return useTokenList;
        }

        /**
         * Sets the token list feature flag. The token list feature makes
         * all tokens (including ignored tokens) link to each other in a 
         * linked list when active. By default the token list feature is
         * not used.
         *
         * @param useTokenList   the token list feature flag
         *
         * @see #getUseTokenList
         * @see Token#getPreviousToken
         * @see Token#getNextToken
         *
         * @since 1.4
         */
        public void SetUseTokenList(bool useTokenList) {
            this.useTokenList = useTokenList;
        }

        /**
         * Returns a description of the token pattern with the
         * specified id.
         * 
         * @param id             the token pattern id
         * 
         * @return the token pattern description, or
         *         null if not present
         */
        public string GetPatternDescription(int id) {
            TokenPattern        pattern;
            RegExpTokenMatcher  re;
        
            pattern = stringMatcher.GetPattern(id);
            if (pattern != null) {
                return pattern.ToShortString();
            }
            for (int i = 0; i < regexpMatchers.Count; i++) {
                re = (RegExpTokenMatcher) regexpMatchers[i];
                if (re.GetPattern().GetId() == id) {
                    return re.GetPattern().ToShortString();
                }
            }
            return null;
        }

        /**
         * Returns the current line number. This number will be the line
         * number of the next token returned.
         * 
         * @return the current line number
         */
        public int GetCurrentLine() {
            return line;
        }
    
        /**
         * Returns the current column number. This number will be the 
         * column number of the next token returned.
         * 
         * @return the current column number
         */
        public int GetCurrentColumn() {
            return column;
        }

        /**
         * Adds a new token pattern to the tokenizer. The pattern will be
         * added last in the list, choosing a previous token pattern in 
         * case two matches the same string.
         * 
         * @param pattern        the pattern to add
         * 
         * @throws ParserCreationException if the pattern couldn't be 
         *             added to the tokenizer
         */
        public void AddPattern(TokenPattern pattern) {
            switch (pattern.GetPatternType()) {
            case TokenPattern.PatternType.STRING:
                stringMatcher.AddPattern(pattern);
                break;
            case TokenPattern.PatternType.REGEXP:
                try {
                    regexpMatchers.Add(new RegExpTokenMatcher(pattern));
                } catch (RegExpException e) {
                    throw new ParserCreationException(
                        ParserCreationException.ErrorType.INVALID_TOKEN,
                        pattern.GetName(),
                        "regular expression contains error(s): " + 
                        e.Message);
                }
                break;
            default:
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_TOKEN,
                    pattern.GetName(),
                    "pattern type " + pattern.GetPatternType() + 
                    " is undefined");
            }
        }

        /**
         * Finds the next token on the stream. This method will return
         * null when end of file has been reached. It will return a
         * parse exception if no token matched the input stream, or if
         * a token pattern with the error flag set matched. Any tokens
         * matching a token pattern with the ignore flag set will be
         * silently ignored and the next token will be returned.
         * 
         * @return the next token found, or 
         *         null if end of file was encountered
         *
         * @throws ParseException if the input stream couldn't be read or
         *             parsed correctly
         */
        public Token Next() {
            Token  token = null;
            
            do {
                token = NextToken();
                if (useTokenList && token != null) {
                    token.SetPreviousToken(previousToken);
                    previousToken = token;
                }
                if (token == null) {
                    return null;
                } else if (token.GetPattern().IsError()) {
                    throw new ParseException(
                        ParseException.ErrorType.INVALID_TOKEN,
                        token.GetPattern().GetErrorMessage(),
                        token.GetStartLine(),
                        token.GetStartColumn());
                } else if (token.GetPattern().IsIgnore()) {
                    token = null;
                }
            } while (token == null);
            
            return token;
        }

        /**
         * Finds the next token on the stream. This method will return
         * null when end of file has been reached. It will return a
         * parse exception if no token matched the input stream.
         * 
         * @return the next token found, or 
         *         null if end of file was encountered
         *
         * @throws ParseException if the input stream couldn't be read or
         *             parsed correctly
         */
        private Token NextToken() {
            TokenMatcher    m;
            Token           token;
            string          str;
            ParseException  e;
            
            // Find longest matching string 
            do {
                if (endOfBuffer) {
                    ReadInput();
                    endOfBuffer = false;
                }
                m = FindMatch();
            } while (endOfBuffer && input != null);
            
            // Return token results
            if (m != null) {
                str = buffer.ToString();
                str = str.Substring(position, m.GetMatchedLength());
                token = new Token(m.GetMatchedPattern(), str, line, column); 
                position += m.GetMatchedLength();
                line = token.GetEndLine();
                column = token.GetEndColumn() + 1;
                return token;
            } else if (position >= buffer.Length) {
                return null;
            } else {
                e = new ParseException(
                    ParseException.ErrorType.UNEXPECTED_CHAR,
                    buffer[position].ToString(),
                    line,
                    column); 
                if (buffer[position] == '\n') {
                    line++;
                    column = 1;
                } else {
                    column++;
                }
                position++;
                throw e;
            }
        }

        /**
         * Reads characters from the input stream and appends them to
         * the input buffer. This method is safe to call even though
         * the end of file has been reached. As a side effect, this
         * method may also remove
         * 
         * @throws ParseException if an error was encountered while 
         *             reading the input stream
         */
        private void ReadInput() {
            char[]  chars = new char[4096];
            int     length;
            
            // Check for end of file
            if (input == null) {
                return;
            }
            
            // Remove old characters from buffer
            if (position > 1024) {
                buffer.Remove(0, position);
                position = 0;
            }
            
            // Read characters
            try {
                length = input.Read(chars, 0, chars.Length);
            } catch (IOException e) {
                input = null;
                throw new ParseException(ParseException.ErrorType.IO,
                                         e.Message,
                                         -1,
                                         -1);
            }
            
            // Append characters to buffer
            if (length > 0) {
                buffer.Append(chars, 0, length);
            }
            if (length < chars.Length) {
                input.Close();
                input = null;
            }
        }

        /**
         * Finds the longest token match from the current buffer
         * position. This method will return the token matcher for the
         * best match, or null if no match was found. As a side
         * effect, this method will also set the end of buffer flag.
         *  
         * @return the token mathcher with the longest match, or
         *         null if no match was found
         */
        private TokenMatcher FindMatch() {
            TokenMatcher        bestMatch = null;
            int                 bestLength = 0;
            RegExpTokenMatcher  re;
            string              str = buffer.ToString();

            // Check string matches
            if (stringMatcher.MatchFrom(str, position)) {
                bestMatch = stringMatcher;
                bestLength = bestMatch.GetMatchedLength();
            }
            if (stringMatcher.HasReadEndOfString()) {
                endOfBuffer = true;
            }
        
            // Check regular expression matches
            for (int i = 0; i < regexpMatchers.Count; i++) {
                re = (RegExpTokenMatcher) regexpMatchers[i];
                if (re.MatchFrom(str, position)
                 && re.GetMatchedLength() > bestLength) {

                    bestMatch = re;
                    bestLength = bestMatch.GetMatchedLength();
                }
                if (re.HasReadEndOfString()) {
                    endOfBuffer = true;
                }
            }
            return bestMatch;
        }

        /**
         * Returns a string representation of this object. The returned
         * string will contain the details of all the token patterns 
         * contained in this tokenizer.
         * 
         * @return a detailed string representation 
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();

            buffer.Append(stringMatcher);
            for (int i = 0; i < regexpMatchers.Count; i++) {
                buffer.Append(regexpMatchers[i]);
            }
            return buffer.ToString();
        }
    }


    /**
     * A token pattern matcher. This class is the base class for the
     * two types of token matchers that exist. The token matcher
     * checks for matches with the tokenizer buffer, and maintains the
     * state of the last match.
     */
    internal abstract class TokenMatcher {

        /**
         * Returns the latest matched token pattern.
         * 
         * @return the latest matched token pattern, or
         *         null if no match found
         */
        public abstract TokenPattern GetMatchedPattern();
        
        /**
         * Returns the length of the latest match.
         * 
         * @return the length of the latest match, or 
         *         zero (0) if no match found
         */
        public abstract int GetMatchedLength();

        /**
         * Checks if the end of string was encountered during the last
         * match. 
         * 
         * @return true if the end of string was reached, or
         *         false otherwise
         */
        public abstract bool HasReadEndOfString();
    }


    /**
     * A regular expression token pattern matcher. This class is used
     * to match a single regular expression with the tokenizer 
     * buffer. This class also maintains the state of the last match. 
     */
    internal class RegExpTokenMatcher : TokenMatcher {

        /**
         * The token pattern to match with.
         */
        private TokenPattern pattern;
        
        /**
         * The regular expression to use.
         */
        private RegExp regExp;

        /**
         * The regular expression matcher to use.
         */
        private Matcher matcher = null;

        /**
         * Creates a new regular expression token matcher.
         * 
         * @param pattern        the pattern to match
         * 
         * @throws RegExpException if the regular expression couldn't
         *             be created properly
         */
        public RegExpTokenMatcher(TokenPattern pattern) {
            this.pattern = pattern;
            this.regExp = new RegExp(pattern.GetPattern());
        }

        /**
         * Returns the token pattern.
         * 
         * @return the token pattern
         */
        public TokenPattern GetPattern() {
            return pattern;
        }

        /**
         * Returns the start position of the latest match.
         * 
         * @return the start position of the last match, or
         *         zero (0) if none found
         */
        public int Start() {
            if (matcher == null || matcher.Length() <= 0) {
                return 0;
            } else {
                return matcher.Start();
            }
        }
        
        /**
         * Returns the latest matched token pattern.
         * 
         * @return the latest matched token pattern, or
         *         null if no match found
         */
        public override TokenPattern GetMatchedPattern() {
            if (matcher == null || matcher.Length() <= 0) {
                return null;
            } else {
                return pattern;
            }
        }
        
        /**
         * Returns the length of the latest match.
         * 
         * @return the length of the latest match, or 
         *         zero (0) if no match found
         */
        public override int GetMatchedLength() {
            return (matcher == null) ? 0 : matcher.Length();
        }
        
        /**
         * Checks if the end of string was encountered during the last
         * match. 
         * 
         * @return true if the end of string was reached, or
         *         false otherwise
         */
        public override bool HasReadEndOfString() {
            return (matcher == null) ? false : matcher.HasReadEndOfString();
        }

        /**
         * Checks if the token pattern matches the tokenizer buffer  
         * from the specified position. This method will also reset 
         * all flags in this matcher.
         * 
         * @param str            the string to match
         * @param pos            the starting position
         * 
         * @return true if a match was found, or
         *         false otherwise
         */
        public bool MatchFrom(string str, int pos) {
            matcher = regExp.Matcher(str);
            return matcher.MatchFrom(pos);
        }
        
        /**
         * Returns a string representation of this token matcher.
         * 
         * @return a detailed string representation of this matcher
         */
        public override string ToString() {
            return pattern.ToString() + "\n" + 
                   regExp.ToString() + "\n";
        }
    }


    /**
     * A string token pattern matcher. This class is used to match a 
     * set of strings with the tokenizer buffer. This class 
     * internally uses a DFA for maximum performance. It also 
     * maintains the state of the last match. 
     */
    internal class StringTokenMatcher : TokenMatcher {

        /**
         * The list of string token patterns. 
         */
        private ArrayList patterns = new ArrayList();

        /**
         * The finite automaton to use for matching.
         */
        private Automaton start = new Automaton();

        /**
         * The last token pattern match found.
         */
        private TokenPattern match = null;

        /**
         * The end of string read flag.
         */
        private bool endOfString = false;

        /**
         * Creates a new string token matcher.
         */
        public StringTokenMatcher() {
        }

        /**
         * Resets the matcher state. This will clear the results of 
         * the last match. 
         */
        public void Reset() {
            match = null;
            endOfString = false;
        }

        /**
         * Returns the latest matched token pattern.
         * 
         * @return the latest matched token pattern, or
         *         null if no match found
         */
        public override TokenPattern GetMatchedPattern() {
            return match;
        }
        
        /**
         * Returns the length of the latest match.
         * 
         * @return the length of the latest match, or 
         *         zero (0) if no match found
         */
        public override int GetMatchedLength() {
            if (match == null) {
                return 0;
            } else {
                return match.GetPattern().Length;
            }
        }

        /**
         * Checks if the end of string was encountered during the last
         * match. 
         * 
         * @return true if the end of string was reached, or
         *         false otherwise
         */
        public override bool HasReadEndOfString() {
            return endOfString; 
        }

        /**
         * Sets the end of string encountered flag.
         */
        public void SetReadEndOfString() {
            endOfString = true;
        }

        /**
         * Returns the token pattern with the specified id. Only 
         * token patterns handled by this matcher can be returned. 
         * 
         * @param id         the token pattern id
         * 
         * @return the token pattern found, or
         *         null if not found
         */
        public TokenPattern GetPattern(int id) {
            TokenPattern  pattern;

            for (int i = 0; i < patterns.Count; i++) {
                pattern = (TokenPattern) patterns[i];
                if (pattern.GetId() == id) {
                    return pattern;
                }
            }
            return null;
        }

        /**
         * Adds a string token pattern to this matcher. 
         * 
         * @param pattern        the pattern to add
         */
        public void AddPattern(TokenPattern pattern) {
            patterns.Add(pattern);
            start.AddMatch(pattern.GetPattern(), pattern);
        }

        /**
         * Checks if the token pattern matches the tokenizer buffer  
         * from the specified position. This method will also reset 
         * all flags in this matcher.
         * 
         * @param str            the string to match
         * @param pos            the starting position
         * 
         * @return true if a match was found, or
         *         false otherwise
         */
        public bool MatchFrom(string str, int pos) {
            Reset();
            match = (TokenPattern) start.MatchFrom(this, str, pos);
            return match != null;
        }
        
        /**
         * Returns a string representation of this matcher. This will
         * contain all the token patterns.
         * 
         * @return a detailed string representation of this matcher 
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();
            
            for (int i = 0; i < patterns.Count; i++) {
                buffer.Append(patterns[i]);
                buffer.Append("\n\n");
            }
            return buffer.ToString();
        }
    }
    

    /**
     * A deterministic finite automaton. This is a simple automaton
     * for character sequences. It cannot handle character set state
     * transitions, but only supports single character transitions.
     */
    internal class Automaton {

        /**
         * The state value.
         */
        private object value = null;
        
        /**
         * The automaton state transition tree. Each transition from
         * this state to another state is added to this tree with the
         * corresponding character. 
         */
        private AutomatonTree tree = new AutomatonTree();
        
        /**
         * Creates a new empty automaton. 
         */
        public Automaton() {
        }

        /**
         * Adds a string match to this automaton. New states and 
         * transitions will be added to extend this automaton to 
         * support the specified string.
         * 
         * @param str            the string to match
         * @param value          the match value
         */
        public void AddMatch(string str, object value) {
            Automaton  state;

            if (str.Equals("")) {
                this.value = value;
            } else {
                state = tree.Find(str[0]);
                if (state == null) {
                    state = new Automaton();
                    state.AddMatch(str.Substring(1), value);
                    tree.Add(str[0], state);
                } else {
                    state.AddMatch(str.Substring(1), value);
                }
            }
        }

        /**
         * Checks if the automaton matches the tokenizer buffer from 
         * the specified position. This method will set the end of
         * buffer flag in the specified token matcher if the end of
         * buffer is reached.
         *
         * @param m              the string token matcher 
         * @param str            the string to match
         * @param pos            the starting position
         * 
         * @return the match value, or
         *         null if no match is found
         */
        public object MatchFrom(StringTokenMatcher m, string str, int pos) {
            object     result = null;
            Automaton  state;

            if (pos >= str.Length) {
                m.SetReadEndOfString();
            } else if (tree != null) {
                state = tree.Find(str[pos]);
                if (state != null) {
                    result = state.MatchFrom(m, str, pos + 1);
                }
            }
            return (result == null) ? value : result;
        }
    }
    
    
    /**
     * An automaton state transition tree. This class contains a 
     * binary search tree for the automaton transitions from one 
     * state to another. All transitions are linked to a single 
     * character.
     */
    internal class AutomatonTree {

        /**
         * The transition character. If this value is set to the zero 
         * ('\0') character, this tree is empty.
         */
        private char value = '\0';
        
        /**
         * The transition state.
         */
        private Automaton state = null;
        
        /**
         * The left subtree.
         */
        private AutomatonTree left = null;
        
        /**
         * The right subtree.
         */
        private AutomatonTree right = null;
        
        /**
         * Creates a new empty automaton transition tree.
         */
        public AutomatonTree() {
        }
        
        /**
         * Finds an automaton state from the specified transition 
         * character. This method searches this transition tree for
         * a matching transition.
         * 
         * @param c              the character to search for
         * 
         * @return the automaton state found, or
         *         null if no transition exists
         */
        public Automaton Find(char c) {
            if (value == '\0' || value == c) {
                return state;
            } else if (value > c) {
                return left.Find(c); 
            } else {
                return right.Find(c);
            }
        }
        
        /**
         * Adds a transition to this tree. 
         * 
         * @param c              the character to transition for
         * @param state          the state to transition to
         */
        public void Add(char c, Automaton state) {
            if (value == '\0') {
                this.value = c;
                this.state = state;
                this.left = new AutomatonTree();
                this.right = new AutomatonTree();
            } else if (value > c) {
                left.Add(c, state);
            } else {
                right.Add(c, state);
            }
        }
    }
}
