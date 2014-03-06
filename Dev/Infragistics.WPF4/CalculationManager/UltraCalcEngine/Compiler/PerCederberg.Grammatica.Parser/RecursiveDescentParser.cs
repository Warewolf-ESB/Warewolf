/*
 * RecursiveDescentParser.cs
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

namespace PerCederberg.Grammatica.Parser {

    /**
     * A recursive descent parser. This parser handles LL(n) grammars, 
     * selecting the appropriate pattern to parse based on the next few
     * tokens. The parser is more efficient the fewer look-ahead tokens
     * that is has to consider.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    public class RecursiveDescentParser : Parser {

        /**
         * The map of pattern look-ahead sets. The map is indexed by
         * the production pattern object.
         */
        private Hashtable lookAheads = new Hashtable();

        /**
         * Creates a new parser.
         * 
         * @param tokenizer      the tokenizer to use
         */
        public RecursiveDescentParser(Tokenizer tokenizer) 
            : base(tokenizer) {
        }

        /**
         * Creates a new parser.
         * 
         * @param tokenizer      the tokenizer to use
         * @param analyzer       the analyzer callback to use
         */
        public RecursiveDescentParser(Tokenizer tokenizer, 
                                      Analyzer analyzer) 
            : base(tokenizer, analyzer) {
        }

        /**
         * Adds a new production pattern to the parser. The pattern
         * will be added last in the list. The first pattern added is
         * assumed to be the starting point in the grammar. The
         * pattern will be validated against the grammar type to some
         * extent.
         * 
         * @param pattern        the pattern to add
         * 
         * @throws ParserCreationException if the pattern couldn't be 
         *             added correctly to the parser
         */
        public override void AddPattern(ProductionPattern pattern) {

            // Check for empty matches
            if (pattern.IsMatchingEmpty()) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.GetName(),
                    "zero elements can be matched (minimum is one)");
            }
                
            // Check for left-recusive patterns
            if (pattern.IsLeftRecursive()) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.GetName(),
                    "left recursive patterns are not allowed");
            }

            // Add pattern 
            base.AddPattern(pattern);
        }

        /**
         * Initializes the parser. All the added production patterns
         * will be analyzed for ambiguities and errors. This method
         * also initializes the internal data structures used during
         * the parsing.
         * 
         * @throws ParserCreationException if the parser couldn't be 
         *             initialized correctly 
         */
        public override void Prepare() {
            IEnumerator  e;
            
            // Performs production pattern checks
            base.Prepare();
	        SetInitialized(false);
            
            // Calculate production look-ahead sets
            e = GetPatterns().GetEnumerator();
            while (e.MoveNext()) {
                CalculateLookAhead((ProductionPattern) e.Current);
            }
            
            // Set initialized flag
	        SetInitialized(true);
        }

        /**
         * Parses the input stream and creates a parse tree.
         * 
         * @return the parse tree
         * 
         * @throws ParseException if the input couldn't be parsed 
         *             correctly
         */
        protected override Node ParseStart() {
            Token      token;
            Node       node;
            ArrayList  list;
        
            node = ParsePattern(GetStartPattern());
            token = PeekToken(0); 
            if (token != null) {
                list = new ArrayList(1);
                list.Add("<EOF>");
                throw new ParseException(
                    ParseException.ErrorType.UNEXPECTED_TOKEN,
                    token.ToShortString(),
                    list,
                    token.GetStartLine(),
                    token.GetStartColumn());
            }
            return node;
        }

        /**
         * Parses a production pattern. A parse tree node may or may
         * not be created depending on the analyzer callbacks.
         * 
         * @param pattern        the production pattern to parse
         * 
         * @return the parse tree node created, or null
         * 
         * @throws ParseException if the input couldn't be parsed 
         *             correctly
         */
        private Node ParsePattern(ProductionPattern pattern) {
            ProductionPatternAlternative  alt;
            ProductionPatternAlternative  defaultAlt;
            
            defaultAlt = pattern.GetDefaultAlternative(); 
            for (int i = 0; i < pattern.GetAlternativeCount(); i++) {
                alt = pattern.GetAlternative(i);
                if (defaultAlt != alt && IsNext(alt)) {
                    return ParseAlternative(alt);
                }
            }
            if (defaultAlt == null || !IsNext(defaultAlt)) {
                ThrowParseException(FindUnion(pattern)); 
            }
            return ParseAlternative(defaultAlt);
        }
    
        /**
         * Parses a production pattern alternative. A parse tree node
         * may or may not be created depending on the analyzer
         * callbacks.
         * 
         * @param alt            the production pattern alternative
         * 
         * @return the parse tree node created, or null
         * 
         * @throws ParseException if the input couldn't be parsed 
         *             correctly
         */
        private Node ParseAlternative(ProductionPatternAlternative alt) {
            Production  node;
        
            node = new Production(alt.GetPattern());
            EnterNode(node);
            for (int i = 0; i < alt.GetElementCount(); i++) {
                try {
                    ParseElement(node, alt.GetElement(i));
                } catch (ParseException e) {
                    AddError(e, true);
                    NextToken();
                    i--;
                }
            }
            return ExitNode(node);
        }

        /**
         * Parses a production pattern element. All nodes parsed may
         * or may not be added to the parse tree node specified,
         * depending on the analyzer callbacks.
         *
         * @param node           the production parse tree node  
         * @param elem           the production pattern element to parse
         * 
         * @throws ParseException if the input couldn't be parsed 
         *             correctly
         */
        private void ParseElement(Production node, 
                                  ProductionPatternElement elem) {

            Node  child;

            for (int i = 0; i < elem.GetMaxCount(); i++) {
                if (i < elem.GetMinCount() || IsNext(elem)) {
                    if (elem.IsToken()) {
                        child = NextToken(elem.GetId());
                        EnterNode(child);
                        AddNode(node, ExitNode(child));
                    } else {
                        child = ParsePattern(GetPattern(elem.GetId()));
                        AddNode(node, child);
                    }
                } else {
                    break;
                }
            }
        }
    
        /**
         * Checks if the next tokens match a production pattern. The
         * pattern look-ahead set will be used if existing, otherwise
         * this method returns false.
         * 
         * @param pattern        the pattern to check
         * 
         * @return true if the next tokens match, or
         *         false otherwise
         */
        private bool IsNext(ProductionPattern pattern) {
            LookAheadSet  set = pattern.GetLookAhead();

            if (set == null) {
                return false;
            } else {
                return set.IsNext(this);
            }
        }

        /**
         * Checks if the next tokens match a production pattern
         * alternative. The pattern alternative look-ahead set will be
         * used if existing, otherwise this method returns false.
         *
         * @param alt            the pattern alternative to check
         *  
         * @return true if the next tokens match, or
         *         false otherwise
         */
        private bool IsNext(ProductionPatternAlternative alt) {
            LookAheadSet  set = alt.GetLookAhead();

            if (set == null) {
                return false;
            } else {
                return set.IsNext(this);
            }
        }
    
        /**
         * Checks if the next tokens match a production pattern
         * element. If the element has a look-ahead set it will be
         * used, otherwise the look-ahead set of the referenced
         * production or token will be used.
         *
         * @param elem           the pattern element to check
         *  
         * @return true if the next tokens match, or
         *         false otherwise
         */
        private bool IsNext(ProductionPatternElement elem) {
            LookAheadSet  set = elem.GetLookAhead();
        
            if (set != null) {
                return set.IsNext(this);
            } else if (elem.IsToken()) {
                return elem.IsMatch(PeekToken(0));
            } else {
                return IsNext(GetPattern(elem.GetId()));
            }
        }

        /**
         * Calculates the look-ahead needed for the specified production 
         * pattern. This method attempts to resolve any conflicts and 
         * stores the results in the pattern look-ahead object. 
         * 
         * @param pattern        the production pattern
         * 
         * @throws ParserCreationException if the look-ahead set couldn't
         *             be determined due to inherent ambiguities
         */
        private void CalculateLookAhead(ProductionPattern pattern) {
            ProductionPatternAlternative  alt;
            LookAheadSet                  result;
            LookAheadSet[]                alternatives;
            LookAheadSet                  conflicts;
            LookAheadSet                  previous = new LookAheadSet(0);
            int                           length = 1;
            int                           i;
            CallStack                     stack = new CallStack();
            
            // Calculate simple look-ahead
            stack.Push(pattern.GetName(), 1);
            result = new LookAheadSet(1);
            alternatives = new LookAheadSet[pattern.GetAlternativeCount()];
            for (i = 0; i < pattern.GetAlternativeCount(); i++) {
                alt = pattern.GetAlternative(i);
                alternatives[i] = FindLookAhead(alt, 1, 0, stack, null);
                alt.SetLookAhead(alternatives[i]);
                result.AddAll(alternatives[i]);
            }
            if (pattern.GetLookAhead() == null) {
                pattern.SetLookAhead(result);
            }
            conflicts = FindConflicts(pattern, 1);
    
            // Resolve conflicts
            while (conflicts.Size() > 0) {
                length++;
                stack.Clear();
                stack.Push(pattern.GetName(), length);
                conflicts.AddAll(previous);
                for (i = 0; i < pattern.GetAlternativeCount(); i++) {
                    alt = pattern.GetAlternative(i);
                    if (alternatives[i].Intersects(conflicts)) {
                        alternatives[i] = FindLookAhead(alt, 
                                                        length,
                                                        0,
                                                        stack,
                                                        conflicts);
                        alt.SetLookAhead(alternatives[i]);
                    }
                    if (alternatives[i].Intersects(conflicts)) {
                        if (pattern.GetDefaultAlternative() == null) {
                            pattern.SetDefaultAlternative(i);
                        } else if (pattern.GetDefaultAlternative() != alt) {
                            result = alternatives[i].CreateIntersection(conflicts);
                            ThrowAmbiguityException(pattern.GetName(), 
                                                    null, 
                                                    result);
                        }
                    }
                }
                previous = conflicts;
                conflicts = FindConflicts(pattern, length);
            }
            
            // Resolve conflicts inside rules
            for (i = 0; i < pattern.GetAlternativeCount(); i++) {
                CalculateLookAhead(pattern.GetAlternative(i), 0);
            }
        }
    
        /**
         * Calculates the look-aheads needed for the specified pattern 
         * alternative. This method attempts to resolve any conflicts in 
         * optional elements by recalculating look-aheads for referenced 
         * productions. 
         * 
         * @param alt            the production pattern alternative
         * @param pos            the pattern element position
         * 
         * @throws ParserCreationException if the look-ahead set couldn't
         *             be determined due to inherent ambiguities
         */
        private void CalculateLookAhead(ProductionPatternAlternative alt,
                                        int pos) {
    
            ProductionPattern         pattern;
            ProductionPatternElement  elem;
            LookAheadSet              first;
            LookAheadSet              follow;
            LookAheadSet              conflicts;
            LookAheadSet              previous = new LookAheadSet(0);
            String                    location;
            int                       length = 1;
    
            // Check trivial cases
            if (pos >= alt.GetElementCount()) {
                return;
            }
    
            // Check for non-optional element
            pattern = alt.GetPattern();
            elem = alt.GetElement(pos);
            if (elem.GetMinCount() == elem.GetMaxCount()) {
                CalculateLookAhead(alt, pos + 1);
                return;
            }
    
            // Calculate simple look-aheads
            first = FindLookAhead(elem, 1, new CallStack(), null);
            follow = FindLookAhead(alt, 1, pos + 1, new CallStack(), null); 
            
            // Resolve conflicts
            location = "at position " + (pos + 1);
            conflicts = FindConflicts(pattern.GetName(), 
                                      location, 
                                      first, 
                                      follow);
            while (conflicts.Size() > 0) {
                length++;
                conflicts.AddAll(previous);
                first = FindLookAhead(elem, 
                                      length, 
                                      new CallStack(), 
                                      conflicts);
                follow = FindLookAhead(alt, 
                                       length, 
                                       pos + 1, 
                                       new CallStack(), 
                                       conflicts);
                first = first.CreateCombination(follow);
                elem.SetLookAhead(first);
                if (first.Intersects(conflicts)) {
                    first = first.CreateIntersection(conflicts);
                    ThrowAmbiguityException(pattern.GetName(), location, first);
                }
                previous = conflicts;
                conflicts = FindConflicts(pattern.GetName(), 
                                          location, 
                                          first, 
                                          follow);
            }
    
            // Check remaining elements
            CalculateLookAhead(alt, pos + 1);
        }
    
        /**
         * Finds the look-ahead set for a production pattern. The maximum 
         * look-ahead length must be specified. It is also possible to 
         * specify a look-ahead set filter, which will make sure that 
         * unnecessary token sequences will be avoided.
         * 
         * @param pattern        the production pattern
         * @param length         the maximum look-ahead length
         * @param stack          the call stack used for loop detection
         * @param filter         the look-ahead set filter
         * 
         * @return the look-ahead set for the production pattern
         * 
         * @throws ParserCreationException if an infinite loop was found
         *             in the grammar
         */
        private LookAheadSet FindLookAhead(ProductionPattern pattern, 
                                           int length,
                                           CallStack stack,
                                           LookAheadSet filter) {
    
            LookAheadSet  result;
            LookAheadSet  temp;
    
            // Check for infinite loop
            if (stack.Contains(pattern.GetName(), length)) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INFINITE_LOOP,
                    pattern.GetName(),
                    (String) null);
            }
    
            // Find pattern look-ahead
            stack.Push(pattern.GetName(), length);
            result = new LookAheadSet(length);
            for (int i = 0; i < pattern.GetAlternativeCount(); i++) {
                temp = FindLookAhead(pattern.GetAlternative(i), 
                                     length,
                                     0,
                                     stack,
                                     filter);
                result.AddAll(temp);
            }
            stack.Pop();
    
            return result;
        }
    
        /**
         * Finds the look-ahead set for a production pattern alternative. 
         * The pattern position and maximum look-ahead length must be 
         * specified. It is also possible to specify a look-ahead set 
         * filter, which will make sure that unnecessary token sequences 
         * will be avoided.
         * 
         * @param alt            the production pattern alternative
         * @param length         the maximum look-ahead length
         * @param pos            the pattern element position
         * @param stack          the call stack used for loop detection
         * @param filter         the look-ahead set filter
         * 
         * @return the look-ahead set for the pattern alternative
         * 
         * @throws ParserCreationException if an infinite loop was found
         *             in the grammar
         */
        private LookAheadSet FindLookAhead(ProductionPatternAlternative alt,
                                           int length,
                                           int pos,
                                           CallStack stack,
                                           LookAheadSet filter) {
    
            LookAheadSet  first;
            LookAheadSet  follow;
            LookAheadSet  overlaps;
    
            // Check trivial cases
            if (length <= 0 || pos >= alt.GetElementCount()) {
                return new LookAheadSet(0);
            }
    
            // Find look-ahead for this element
            first = FindLookAhead(alt.GetElement(pos), length, stack, filter);
            if (alt.GetElement(pos).GetMinCount() == 0) {
                first.AddEmpty();
            }
            
            // Find remaining look-ahead
            if (filter == null) {
                length -= first.GetMinLength();
                if (length > 0) {
                    follow = FindLookAhead(alt, length, pos + 1, stack, null); 
                    first = first.CreateCombination(follow);
                }
            } else if (filter.IsOverlap(first)) {
                overlaps = first.CreateOverlaps(filter);
                length -= overlaps.GetMinLength();
                filter = filter.CreateFilter(overlaps);
                follow = FindLookAhead(alt, length, pos + 1, stack, filter); 
                first.RemoveAll(overlaps);
                first.AddAll(overlaps.CreateCombination(follow));
            }
    
            return first;
        }
    
        /**
         * Finds the look-ahead set for a production pattern element. The
         * maximum look-ahead length must be specified. This method takes
         * the element repeats into consideration when creating the 
         * look-ahead set, but does NOT include an empty sequence even if
         * the minimum count is zero (0). It is also possible to specify a
         * look-ahead set filter, which will make sure that unnecessary 
         * token sequences will be avoided.
         * 
         * @param elem           the production pattern element
         * @param length         the maximum look-ahead length
         * @param stack          the call stack used for loop detection
         * @param filter         the look-ahead set filter
         * 
         * @return the look-ahead set for the pattern element
         * 
         * @throws ParserCreationException if an infinite loop was found
         *             in the grammar
         */
        private LookAheadSet FindLookAhead(ProductionPatternElement elem,
                                           int length,
                                           CallStack stack,
                                           LookAheadSet filter) {
    
            LookAheadSet  result;
            LookAheadSet  first;
            LookAheadSet  follow;
            int           max;
    
            // Find initial element look-ahead
            first = FindLookAhead(elem, length, 0, stack, filter);
            result = new LookAheadSet(length);
            result.AddAll(first);
            if (filter == null || !filter.IsOverlap(result)) {
                return result;
            }
    
            // Handle element repetitions
            if (elem.GetMaxCount() == Int32.MaxValue) {
                first = first.CreateRepetitive();
            }
            max = elem.GetMaxCount();
            if (length < max) {
                max = length;
            }
            for (int i = 1; i < max; i++) {
                first = first.CreateOverlaps(filter);
                if (first.Size() <= 0 || first.GetMinLength() >= length) {
                    break;
                }
                follow = FindLookAhead(elem, 
                                       length, 
                                       0, 
                                       stack, 
                                       filter.CreateFilter(first));
                first = first.CreateCombination(follow);
                result.AddAll(first);
            }
    
            return result;
        }
    
        /**
         * Finds the look-ahead set for a production pattern element. The
         * maximum look-ahead length must be specified. This method does
         * NOT take the element repeat into consideration when creating 
         * the look-ahead set. It is also possible to specify a look-ahead
         * set filter, which will make sure that unnecessary token 
         * sequences will be avoided.
         * 
         * @param elem           the production pattern element
         * @param length         the maximum look-ahead length
         * @param dummy          a parameter to distinguish the method
         * @param stack          the call stack used for loop detection
         * @param filter         the look-ahead set filter
         * 
         * @return the look-ahead set for the pattern element
         * 
         * @throws ParserCreationException if an infinite loop was found
         *             in the grammar
         */
        private LookAheadSet FindLookAhead(ProductionPatternElement elem,
                                           int length,
                                           int dummy,
                                           CallStack stack,
                                           LookAheadSet filter) {
    
            LookAheadSet       result;
            ProductionPattern  pattern;
    
            if (elem.IsToken()) {
                result = new LookAheadSet(length);
                result.Add(elem.GetId());
            } else {
                pattern = GetPattern(elem.GetId());
                result = FindLookAhead(pattern, length, stack, filter);
                if (stack.Contains(pattern.GetName())) {
                    result = result.CreateRepetitive();
                }
            }
    
            return result;
        }
    
        /**
         * Returns a look-ahead set with all conflics between
         * alternatives in a production pattern.
         * 
         * @param pattern        the production pattern
         * @param maxLength      the maximum token sequence length
         * 
         * @return a look-ahead set with the conflicts found
         * 
         * @throws ParserCreationException if an inherent ambiguity was
         *             found among the look-ahead sets
         */
        private LookAheadSet FindConflicts(ProductionPattern pattern,
                                           int maxLength) {

            LookAheadSet  result = new LookAheadSet(maxLength);
            LookAheadSet  set1;
            LookAheadSet  set2;

            for (int i = 0; i < pattern.GetAlternativeCount(); i++) {
                set1 = pattern.GetAlternative(i).GetLookAhead();
                for (int j = 0; j < i; j++) {
                    set2 = pattern.GetAlternative(j).GetLookAhead();
                    result.AddAll(set1.CreateIntersection(set2));
                }
            }
            if (result.IsRepetitive()) {
                ThrowAmbiguityException(pattern.GetName(), null, result);
            }
            return result;
        }
    
        /**
         * Returns a look-ahead set with all conflicts between two
         * look-ahead sets.
         * 
         * @param pattern        the pattern name being analyzed
         * @param location       the pattern location
         * @param set1           the first look-ahead set
         * @param set2           the second look-ahead set
         * 
         * @return a look-ahead set with the conflicts found
         * 
         * @throws ParserCreationException if an inherent ambiguity was
         *             found among the look-ahead sets
         */
        private LookAheadSet FindConflicts(string pattern,
                                           string location,
                                           LookAheadSet set1,
                                           LookAheadSet set2) {

            LookAheadSet  result;
            
            result = set1.CreateIntersection(set2);
            if (result.IsRepetitive()) {
                ThrowAmbiguityException(pattern, location, result);
            }
            return result;
        }

        /**
         * Returns the union of all alternative look-ahead sets in a
         * production pattern.
         * 
         * @param pattern        the production pattern
         * 
         * @return a unified look-ahead set
         */
        private LookAheadSet FindUnion(ProductionPattern pattern) {
            LookAheadSet  result;
            int           length = 0;
            int           i;

            for (i = 0; i < pattern.GetAlternativeCount(); i++) {
                result = pattern.GetAlternative(i).GetLookAhead();
                if (result.GetMaxLength() > length) {
                    length = result.GetMaxLength(); 
                }
            }
            result = new LookAheadSet(length);
            for (i = 0; i < pattern.GetAlternativeCount(); i++) {
                result.AddAll(pattern.GetAlternative(i).GetLookAhead());
            }
            
            return result;
        }

        /**
         * Throws a parse exception that matches the specified look-ahead
         * set. This method will take into account any initial matching 
         * tokens in the look-ahead set.
         * 
         * @param set            the look-ahead set to match
         * 
         * @throws ParseException always thrown by this method
         */
        private void ThrowParseException(LookAheadSet set) {
            Token      token;
            ArrayList  list = new ArrayList();
            int[]      initials;
        
            // Read tokens until mismatch
            while (set.IsNext(this, 1)) {
                set = set.CreateNextSet(NextToken().GetId());
            }
        
            // Find next token descriptions
            initials = set.GetInitialTokens();
            for (int i = 0; i < initials.Length; i++) {
                list.Add(GetTokenDescription(initials[i]));
            }
        
            // Create exception
            token = NextToken();
            throw new ParseException(ParseException.ErrorType.UNEXPECTED_TOKEN,
                                     token.ToShortString(),
                                     list,
                                     token.GetStartLine(),
                                     token.GetStartColumn());
        }

        /**
         * Throws a parser creation exception for an ambiguity. The 
         * specified look-ahead set contains the token conflicts to be 
         * reported.
         *
         * @param pattern        the production pattern name
         * @param location       the production pattern location, or null  
         * @param set            the look-ahead set with conflicts
         * 
         * @throws ParserCreationException always thrown by this method
         */
        private void ThrowAmbiguityException(string pattern, 
                                             string location,
                                             LookAheadSet set) {

            ArrayList  list = new ArrayList();
            int[]      initials;
        
            // Find next token descriptions
            initials = set.GetInitialTokens();
            for (int i = 0; i < initials.Length; i++) {
                list.Add(GetTokenDescription(initials[i]));
            }
        
            // Create exception
            throw new ParserCreationException(
                ParserCreationException.ErrorType.INHERENT_AMBIGUITY,
                pattern,
                location,
                list);
        }


        /**
         * A name value stack. This stack is used to detect loops and 
         * repetitions of the same production during look-ahead analysis.
         */
        private class CallStack {
            
            /**
             * A stack with names.
             */
            private ArrayList nameStack = new ArrayList();

            /**
             * A stack with values.
             */
            private ArrayList valueStack = new ArrayList();

            /**
             * Checks if the specified name is on the stack. 
             * 
             * @param name           the name to search for
             * 
             * @return true if the name is on the stack, or
             *         false otherwise
             */
            public bool Contains(string name) {
                return nameStack.Contains(name);
            }
        
            /**
             * Checks if the specified name and value combination is on
             * the stack.
             * 
             * @param name           the name to search for
             * @param value          the value to search for
             * 
             * @return true if the combination is on the stack, or
             *         false otherwise
             */
            public bool Contains(string name, int value) {
                for (int i = 0; i < nameStack.Count; i++) {
                    if (nameStack[i].Equals(name)
                     && valueStack[i].Equals(value)) {
                        
                        return true;
                    }
                }
                return false;
            }

            /**
             * Clears the stack. This method removes all elements on
             * the stack.
             */
            public void Clear() {
                nameStack.Clear();
                valueStack.Clear();
            }
        
            /**
             * Adds a new element to the top of the stack.
             * 
             * @param name           the stack name 
             * @param value          the stack value
             */
            public void Push(string name, int value) {
                nameStack.Add(name);
                valueStack.Add(value);
            }

            /**
             * Removes the top element of the stack.
             */
            public void Pop() {
                if (nameStack.Count > 0) {
                    nameStack.RemoveAt(nameStack.Count - 1);
                    valueStack.RemoveAt(valueStack.Count - 1);
                }
            }
        }
    }
}
