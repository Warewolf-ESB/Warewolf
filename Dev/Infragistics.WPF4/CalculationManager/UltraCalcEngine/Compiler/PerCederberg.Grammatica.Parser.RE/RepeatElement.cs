/*
 * RepeatElement.cs
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
using System.IO;

namespace PerCederberg.Grammatica.Parser.RE {

    /**
     * A regular expression element repeater. The element repeats the
     * matches from a specified element, attempting to reach the
     * maximum repetition count.
     *
     * @author   Per Cederberg
     * @version  1.1
     */
    internal class RepeatElement : Element {

        /**
         * The repeat type constants.
         */
        public enum RepeatType {

            /*
             * The greedy repeat type constant.
             */
            GREEDY = 1,

            /*
             * The reluctant repeat type constant.
             */
            RELUCTANT = 2,

            /*
             * The possesive repeat type constant.
             */
            POSSESSIVE = 3
        }

        /**
         * The element to repeat.
         */
        private Element elem;
    
        /**
         * The minimum number of repetitions.
         */
        private int min;

        /**
         * The maximum number of repetitions.
         */    
        private int max;

        /**
         * The repeat type.
         */
        private RepeatType type;

        /**
         * The start position of the last set of matches. 
         */
        private int matchStart;

        /**
         * A set with all matches starting at matchStart. A match with
         * a specific length is reported by a non-zero bit in the bit
         * array.
         */
        private BitArray matches;
        
        /**
         * Creats a new element repeater.
         * 
         * @param elem           the element to repeat
         * @param min            the minimum count
         * @param max            the maximum count
         * @param type           the repeat type constant
         */
        public RepeatElement(Element elem, 
                             int min, 
                             int max, 
                             RepeatType type) {

            this.elem = elem;
            this.min = min;
            if (max <= 0) {
                this.max = Int32.MaxValue;
            } else {
                this.max = max;
            }
            this.type = type;
            this.matchStart = -1;
            this.matches = null;
        }

        /**
         * Creates a copy of this element. The copy will be an
         * instance of the same class matching the same strings.
         * Copies of elements are necessary to allow elements to cache
         * intermediate results while matching strings without
         * interfering with other threads.
         * 
         * @return a copy of this element
         */
        public override object Clone() {
            return new RepeatElement((Element) elem.Clone(), 
                                     min, 
                                     max, 
                                     type);
        }

        /**
         * Returns the length of a matching string starting at the 
         * specified position. The number of matches to skip can also be
         * specified.
         *
         * @param m              the matcher being used 
         * @param str            the string to match
         * @param start          the starting position
         * @param skip           the number of matches to skip
         * 
         * @return the length of the matching string, or
         *         -1 if no match was found
         */
        public override int Match(Matcher m, 
                                  string str, 
                                  int start, 
                                  int skip) {

            if (skip == 0) {
                matchStart = -1;
                matches = null;
            }
            switch (type) {
            case RepeatType.GREEDY:
                return MatchGreedy(m, str, start, skip);
            case RepeatType.RELUCTANT:
                return MatchReluctant(m, str, start, skip);
            case RepeatType.POSSESSIVE:
                if (skip == 0) {
                    return MatchPossessive(m, str, start, 0);
                }
                break;
            }
            return -1;
        }

        /**
         * Returns the length of the longest possible matching string
         * starting at the specified position. The number of matches
         * to skip can also be specified.
         *
         * @param m              the matcher being used 
         * @param str            the string to match
         * @param start          the starting position
         * @param skip           the number of matches to skip
         * 
         * @return the length of the longest matching string, or
         *         -1 if no match was found
         */
        private int MatchGreedy(Matcher m, 
                                string str, 
                                int start,
                                int skip) {

            // Check for simple case
            if (skip == 0) {
                return MatchPossessive(m, str, start, 0);
            }

            // Find all matches
            if (matchStart != start) {
                matchStart = start;
                matches = new BitArray(10);
                FindMatches(m, str, start, 0, 0, 0);
            }

            // Find first non-skipped match
            for (int i = matches.Count - 1; i >= 0; i--) {
                if (matches[i]) {
                    if (skip == 0) {
                        return i;
                    }
                    skip--;
                }
            }
            return -1;
        }

        /**
         * Returns the length of the shortest possible matchine string
         * starting at the specified position. The number of matches to 
         * skip can also be specified.
         *
         * @param m              the matcher being used 
         * @param str            the string to match
         * @param start          the starting position
         * @param skip           the number of matches to skip
         * 
         * @return the length of the shortest matching string, or
         *         -1 if no match was found
         */
        private int MatchReluctant(Matcher m, 
                                   string str, 
                                   int start,
                                   int skip) {
    
            // Find all matches
            if (matchStart != start) {
                matchStart = start;
                matches = new BitArray(10);
                FindMatches(m, str, start, 0, 0, 0);
            }

            // Find first non-skipped match
            for (int i = 0; i < matches.Count; i++) {
                if (matches[i]) {
                    if (skip == 0) {
                        return i;
                    }
                    skip--;
                }
            }
            return -1;
        }

        /**
         * Returns the length of the maximum number of elements matching 
         * the string starting at the specified position. This method 
         * allows no backtracking, i.e. no skips..
         *
         * @param m              the matcher being used 
         * @param str            the string to match
         * @param start          the starting position
         * @param count          the start count, normally zero (0)
         * 
         * @return the length of the longest matching string, or
         *         -1 if no match was found
         */
        private int MatchPossessive(Matcher m, 
                                    string str, 
                                    int start,
                                    int count) {
            
            int  length = 0;
            int  subLength = 1;
        
            // Match as many elements as possible
            while (subLength > 0 && count < max) {
                subLength = elem.Match(m, str, start + length, 0);
                if (subLength >= 0) {
                    count++;
                    length += subLength;
                }
            }
            
            // Return result
            if (min <= count && count <= max) {
                return length;
            } else {
                return -1;
            }
        }

        /**
         * Finds all matches and adds the lengths to the matches set.  
         * 
         * @param m              the matcher being used 
         * @param str            the string to match
         * @param start          the starting position
         * @param length         the match length at the start position
         * @param count          the number of sub-elements matched
         * @param attempt        the number of match attempts here
         */
        private void FindMatches(Matcher m, 
                                 string str, 
                                 int start,
                                 int length,
                                 int count, 
                                 int attempt) {

            int  subLength;

            // Check match ending here
            if (count > max) {
                return;
            }
            if (min <= count && attempt == 0) {
                if (matches.Length <= length) {
                    matches.Length = length + 10;
                }
                matches[length] = true;
            }
        
            // Check element match
            subLength = elem.Match(m, str, start, attempt);
            if (subLength < 0) {
                return;
            } else if (subLength == 0) {
                if (min == count + 1) {
                    if (matches.Length <= length) {
                        matches.Length = length + 10;
                    }
                    matches[length] = true;
                }
                return;
            }
            
            // Find alternative and subsequent matches 
            FindMatches(m, str, start, length, count, attempt + 1);
            FindMatches(m, 
                        str, 
                        start + subLength, 
                        length + subLength, 
                        count + 1, 
                        0); 
        }

        /**
         * Prints this element to the specified output stream.
         * 
         * @param output         the output stream to use
         * @param indent         the current indentation
         */
        public override void PrintTo(TextWriter output, string indent) {
            output.Write(indent + "Repeat (" + min + "," + max + ")");
            if (type == RepeatType.RELUCTANT) {
                output.Write("?");
            } else if (type == RepeatType.POSSESSIVE) {
                output.Write("+");
            }
            output.WriteLine();
            elem.PrintTo(output, indent + "  ");
        }
    }
}
