/*
 * Parser.cs
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

#pragma warning disable 1570

using System;
using System.Collections;
using System.Text;

namespace PerCederberg.Grammatica.Parser {

    /**
     * A base parser class. This class provides the standard parser 
     * interface, as well as token handling.
     *
     * @author   Per Cederberg
     * @version  1.4
     */
    public abstract class Parser {
    
        /**
         * The parser initialization flag.
         */
        private bool initialized = false;
    
        /**
         * The tokenizer to use.
         */
        private Tokenizer tokenizer;
    
        /**
         * The analyzer to use for callbacks.
         */
        private Analyzer analyzer;
    
        /**
         * The list of production patterns. 
         */
        private ArrayList patterns = new ArrayList();
    
        /**
         * The map with production patterns and their id:s. This map 
         * contains the production patterns indexed by their id:s.
         */
        private Hashtable patternIds = new Hashtable();
    
        /**
         * The list of buffered tokens. This list will contain tokens that
         * have been read from the tokenizer, but not yet consumed.
         */
        private ArrayList tokens = new ArrayList();
        
        /**
         * The error log. All parse errors will be added to this log as
         * the parser attempts to recover from the error. If the error
         * count is higher than zero (0), this log will be thrown as the
         * result from the parse() method. 
         */
        private ParserLogException errorLog = new ParserLogException();
    
        /**
         * The error recovery counter. This counter is initially set to a
         * negative value to indicate that no error requiring recovery 
         * has been encountered. When a parse error is found, the counter
         * is set to three (3), and is then decreased by one for each 
         * correctly read token until it reaches zero (0).  
         */
        private int errorRecovery = -1;
    
        /**
         * Creates a new parser.
         * 
         * @param tokenizer       the tokenizer to use
         */
        internal Parser(Tokenizer tokenizer) 
            : this(tokenizer, null) {
        }

        /**
         * Creates a new parser.
         * 
         * @param tokenizer       the tokenizer to use
         * @param analyzer        the analyzer callback to use
         */
        internal Parser(Tokenizer tokenizer, Analyzer analyzer) {
            this.tokenizer = tokenizer;
            if (analyzer == null) {
                this.analyzer = new Analyzer();
            } else {
                this.analyzer = analyzer;
            }
        }

        /**
         * Returns the tokenizer in use by this parser.
         * 
         * @return the tokenizer in use by this parser
         * 
         * @since 1.4
         */
        public Tokenizer GetTokenizer() {
            return tokenizer;
        }

        /**
         * Returns the analyzer in use by this parser.
         * 
         * @return the analyzer in use by this parser
         * 
         * @since 1.4
         */
        public Analyzer GetAnalyzer() {
            return analyzer;
        }

        /**
         * Sets the parser initialized flag. Normally this flag is set by
         * the prepare() method, but this method allows further 
         * modifications to it.
         * 
         * @param initialized    the new initialized flag
         */
        internal void SetInitialized(bool initialized) {
            this.initialized = initialized;
        }
    
        /**
         * Adds a new production pattern to the parser. The first pattern 
         * added is assumed to be the starting point in the grammar. The 
         * patterns added may be validated to some extent.
         * 
         * @param pattern        the pattern to add
         * 
         * @throws ParserCreationException if the pattern couldn't be 
         *             added correctly to the parser
         */
        public virtual void AddPattern(ProductionPattern pattern) {
            if (pattern.GetAlternativeCount() <= 0) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.GetName(),
                    "no production alternatives are present (must have at " +
                    "least one)");
            }
            if (patternIds.ContainsKey(pattern.GetId())) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.GetName(),
                    "another pattern with the same id (" + pattern.GetId() + 
                    ") has already been added");
            }
            patterns.Add(pattern);
            patternIds.Add(pattern.GetId(), pattern);
            SetInitialized(false);
        }
    
        /**
         * Initializes the parser. All the added production patterns will
         * be analyzed for ambiguities and errors. This method also 
         * initializes internal data structures used during the parsing. 
         * 
         * @throws ParserCreationException if the parser couldn't be 
         *             initialized correctly 
         */
        public virtual void Prepare() {
            if (patterns.Count <= 0) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PARSER,
                    "no production patterns have been added");
            }
            for (int i = 0; i < patterns.Count; i++) {
                CheckPattern((ProductionPattern) patterns[i]);
            }
            SetInitialized(true);
        }
    
        /**
         * Checks a production pattern for completeness. If some rule
         * in the pattern referenced an production pattern not added
         * to this parser, a parser creation exception will be thrown.
         * 
         * @param pattern        the production pattern to check
         * 
         * @throws ParserCreationException if the pattern referenced a 
         *             pattern not added to this parser
         */
        private void CheckPattern(ProductionPattern pattern) {
            for (int i = 0; i < pattern.GetAlternativeCount(); i++) {
                CheckRule(pattern.GetName(), pattern.GetAlternative(i));     
            }
        }

        /**
         * Checks a production pattern rule for completeness. If some
         * element in the rule referenced an production pattern not
         * added to this parser, a parser creation exception will be
         * thrown.
         *
         * @param name           the name of the pattern being checked 
         * @param rule           the production pattern rule to check
         * 
         * @throws ParserCreationException if the rule referenced a 
         *             pattern not added to this parser
         */
        private void CheckRule(string name, 
                               ProductionPatternAlternative rule) {
            
            for (int i = 0; i < rule.GetElementCount(); i++) {
                CheckElement(name, rule.GetElement(i));
            }
        }

        /**
         * Checks a production pattern element for completeness. If
         * the element references a production pattern not added to
         * this parser, a parser creation exception will be thrown.
         * 
         * @param name           the name of the pattern being checked 
         * @param elem           the production pattern element to check
         * 
         * @throws ParserCreationException if the element referenced a
         *             pattern not added to this parser
         */
        private void CheckElement(string name, 
                                  ProductionPatternElement elem) {

            if (elem.IsProduction() && GetPattern(elem.GetId()) == null) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    name,
                    "an undefined production pattern id (" + elem.GetId() +
                    ") is referenced");
            }
        }

        /**
         * Parses the token stream and returns a parse tree. This method
         * will call prepare() if not previously called. In case of a 
         * parse error, the parser will attempt to recover and throw all
         * the errors found in a parser log exception in the end.
         * 
         * @return the parse tree
         * 
         * @throws ParserCreationException if the parser couldn't be
         *             initialized correctly
         * @throws ParserLogException if the input couldn't be parsed 
         *             correctly
         * 
         * @see #prepare
         */
        public Node Parse() {
            Node  root = null;
    
            // Initialize parser
            if (!initialized) {
                Prepare();
            }
    
            // Parse input
            try {
                root = ParseStart();
            } catch (ParseException e) {
                AddError(e, true);
            }
            
            // Check for errors
            if (errorLog.GetErrorCount() > 0) {
                throw errorLog;
            }
    
            return root;
        }
    
        /**
         * Parses the token stream and returns a parse tree.
         * 
         * @return the parse tree
         * 
         * @throws ParseException if the input couldn't be parsed 
         *             correctly
         */
        protected abstract Node ParseStart();
    
        /**
         * Adds an error to the error log. If the parser is in error 
         * recovery mode, the error will not be added to the log. If the
         * recovery flag is set, this method will set the error recovery 
         * counter thus enter error recovery mode. Only lexical or 
         * syntactical errors require recovery, so this flag shouldn't be
         * set otherwise.
         * 
         * @param e              the error to add
         * @param recovery       the recover flag 
         */
        internal void AddError(ParseException e, bool recovery) {
            if (errorRecovery <= 0) {
                errorLog.AddError(e);
            }
            if (recovery) {
                errorRecovery = 3;
            }
        }
    
        /**
         * Returns the production pattern with the specified id.
         *  
         * @param id             the production pattern id
         * 
         * @return the production pattern found, or
         *         null if non-existent
         */
        internal ProductionPattern GetPattern(int id) {
            return (ProductionPattern) patternIds[id];
        }

        /**
         * Returns the production pattern for the starting production.
         *  
         * @return the start production pattern, or
         *         null if no patterns have been added
         */
        internal ProductionPattern GetStartPattern() {
            if (patterns.Count <= 0) {
                return null;
            } else {
                return (ProductionPattern) patterns[0];
            }
        }
    
        /**
         * Returns the ordered set of production patterns.
         * 
         * @return the ordered set of production patterns
         */
        internal ICollection GetPatterns() {
            return patterns;
        }

        /**
         * Handles the parser entering a production. This method calls the
         * appropriate analyzer callback if the node is not hidden. Note
         * that this method will not call any callback if an error 
         * requiring recovery has ocurred.
         * 
         * @param node           the parse tree node
         */
        internal void EnterNode(Node node) {
            if (!node.IsHidden() && errorRecovery < 0) {
                try {
                    analyzer.Enter(node);
                } catch (ParseException e) {
                    AddError(e, false);
                }
            }
        }
        
        /**
         * Handles the parser leaving a production. This method calls the
         * appropriate analyzer callback if the node is not hidden, and 
         * returns the result. Note that this method will not call any 
         * callback if an error requiring recovery has ocurred.
         * 
         * @param node           the parse tree node
         * 
         * @return the parse tree node, or
         *         null if no parse tree should be created
         */
        internal Node ExitNode(Node node) {
            if (!node.IsHidden() && errorRecovery < 0) {
                try {
                    return analyzer.Exit(node);
                } catch (ParseException e) {
                    AddError(e, false);
                }
            }
            return node;
        }
    
        /**
         * Handles the parser adding a child node to a production. This 
         * method calls the appropriate analyzer callback. Note that this 
         * method will not call any callback if an error requiring 
         * recovery has ocurred.
         * 
         * @param node           the parent parse tree node
         * @param child          the child parse tree node, or null
         */
        internal void AddNode(Production node, Node child) {
            if (errorRecovery >= 0) {
                // Do nothing
            } else if (node.IsHidden()) {
                node.AddChild(child);
            } else if (child != null && child.IsHidden()) {
                for (int i = 0; i < child.GetChildCount(); i++) {
                    AddNode(node, child.GetChildAt(i));
                }
            } else {
                try {
                    analyzer.Child(node, child);
                } catch (ParseException e) {
                    AddError(e, false);
                }
            }
        }
    
        /**
         * Reads and consumes the next token in the queue. If no token
         * was available for consumation, a parse error will be
         * thrown.
         * 
         * @return the token consumed
         * 
         * @throws ParseException if the input stream couldn't be read or
         *             parsed correctly
         */
        internal Token NextToken() {
            Token  token = PeekToken(0);
        
            if (token != null) {
                tokens.RemoveAt(0);
                return token;
            } else {
                throw new ParseException(
                    ParseException.ErrorType.UNEXPECTED_EOF,
                    null,
                    tokenizer.GetCurrentLine(),
                    tokenizer.GetCurrentColumn());
            }
        }
    
        /**
         * Reads and consumes the next token in the queue. If no token was
         * available for consumation, a parse error will be thrown. A 
         * parse error will also be thrown if the token id didn't match 
         * the specified one. 
         *
         * @param id             the expected token id
         *  
         * @return the token consumed
         * 
         * @throws ParseException if the input stream couldn't be parsed
         *             correctly, or if the token wasn't expected
         */
        internal Token NextToken(int id) {
            Token      token = NextToken();
            ArrayList  list;
            
            if (token.GetId() == id) {
                if (errorRecovery > 0) {
                    errorRecovery--;
                }
                return token;
            } else {
                list = new ArrayList(1);
                list.Add(tokenizer.GetPatternDescription(id));
                throw new ParseException(
                    ParseException.ErrorType.UNEXPECTED_TOKEN,
                    token.ToShortString(),
                    list,
                    token.GetStartLine(),
                    token.GetStartColumn());
            }
        }
    
        /**
         * Returns a token from the queue. This method is used to check 
         * coming tokens before they have been consumed. Any number of 
         * tokens forward can be checked. 
         * 
         * @param steps          the token queue number, zero (0) for first
         * 
         * @return the token in the queue, or
         *         null if no more tokens in the queue
         */
        internal Token PeekToken(int steps) {
            Token  token;
    
            while (steps >= tokens.Count) {
                try {
                    token = tokenizer.Next();
                    if (token == null) {
                        return null;
                    } else {
                        tokens.Add(token);
                    }
                } catch (ParseException e) {
                    AddError(e, true);
                }
            }
            return (Token) tokens[steps];
        }
    
        /**
         * Returns a string representation of this parser. The string will
         * contain all the production definitions and various additional 
         * information.
         * 
         * @return a detailed string representation of this parser
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();
            
            for (int i = 0; i < patterns.Count; i++) {
                buffer.Append(ToString((ProductionPattern) patterns[i])); 
                buffer.Append("\n");
            }
            return buffer.ToString();
        }
    
        /**
         * Returns a string representation of a production pattern.
         * 
         * @param prod           the production pattern
         * 
         * @return a detailed string representation of the pattern
         */
        private string ToString(ProductionPattern prod) {
            StringBuilder  buffer = new StringBuilder();
            StringBuilder  indent = new StringBuilder();
            LookAheadSet   set;
            int            i;
    
            buffer.Append(prod.GetName());
            buffer.Append(" (");
            buffer.Append(prod.GetId());
            buffer.Append(") ");
            for (i = 0; i < buffer.Length; i++) {
                indent.Append(" ");
            }
            buffer.Append("= ");
            indent.Append("| ");
            for (i = 0; i < prod.GetAlternativeCount(); i++) {
                if (i > 0) {
                    buffer.Append(indent);
                }
                buffer.Append(ToString(prod.GetAlternative(i)));
                buffer.Append("\n");
            }
            for (i = 0; i < prod.GetAlternativeCount(); i++) {
                set = prod.GetAlternative(i).GetLookAhead();
                if (set.GetMaxLength() > 1) {
                    buffer.Append("Using ");
                    buffer.Append(set.GetMaxLength());
                    buffer.Append(" token look-ahead for alternative "); 
                    buffer.Append(i + 1);
                    buffer.Append(": ");
                    buffer.Append(set.ToString(tokenizer));
                    buffer.Append("\n");
                }
            }
            return buffer.ToString();
        }
        
        /**
         * Returns a string representation of a production pattern 
         * alternative.
         * 
         * @param alt            the production pattern alternative
         * 
         * @return a detailed string representation of the alternative
         */
        private string ToString(ProductionPatternAlternative alt) {
            StringBuilder  buffer = new StringBuilder();
    
            for (int i = 0; i < alt.GetElementCount(); i++) {
                if (i > 0) {
                    buffer.Append(" ");
                }
                buffer.Append(ToString(alt.GetElement(i)));
            }
            return buffer.ToString();
        }
    
        /**
         * Returns a string representation of a production pattern 
         * element.
         * 
         * @param elem           the production pattern element
         * 
         * @return a detailed string representation of the element
         */
        private string ToString(ProductionPatternElement elem) {
            StringBuilder  buffer = new StringBuilder();
            int            min = elem.GetMinCount();
            int            max = elem.GetMaxCount();
    
            if (min == 0 && max == 1) {
                buffer.Append("[");
            }
            if (elem.IsToken()) {
                buffer.Append(GetTokenDescription(elem.GetId()));
            } else {
                buffer.Append(GetPattern(elem.GetId()).GetName());
            }
            if (min == 0 && max == 1) {
                buffer.Append("]");
            } else if (min == 0 && max == Int32.MaxValue) {
                buffer.Append("*");
            } else if (min == 1 && max == Int32.MaxValue) {
                buffer.Append("+");
            } else if (min != 1 || max != 1) {
                buffer.Append("{");
                buffer.Append(min);
                buffer.Append(",");
                buffer.Append(max);
                buffer.Append("}");
            }
            return buffer.ToString();
        }
    
        /**
         * Returns a token description for a specified token.
         * 
         * @param token          the token to describe
         * 
         * @return the token description
         */
        internal string GetTokenDescription(int token) {
            if (tokenizer == null) {
                return "";
            } else {
                return tokenizer.GetPatternDescription(token);
            }
        }
    }
}
