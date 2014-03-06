/*
 * CharacterSetElement.cs
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

using System.Collections;
using System.IO;
using System.Text;

namespace PerCederberg.Grammatica.Parser.RE {

    /**
     * A regular expression character set element. This element
     * matches a single character inside (or outside) a character set.
     * The character set is user defined and may contain ranges of
     * characters. The set may also be inverted, meaning that only
     * characters not inside the set will be considered to match.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    internal class CharacterSetElement : Element {

        /**
         * The dot ('.') character set. This element matches a single
         * character that is not equal to a newline character.
         */
        public static CharacterSetElement DOT = 
            new CharacterSetElement(false);

        /**
         * The digit character set. This element matches a single
         * numeric character.
         */
        public static CharacterSetElement DIGIT = 
            new CharacterSetElement(false);

        /**
         * The non-digit character set. This element matches a single
         * non-numeric character.
         */
        public static CharacterSetElement NON_DIGIT = 
            new CharacterSetElement(true);

        /**
         * The whitespace character set. This element matches a single
         * whitespace character.
         */
        public static CharacterSetElement WHITESPACE = 
            new CharacterSetElement(false);

        /**
         * The non-whitespace character set. This element matches a
         * single non-whitespace character.
         */
        public static CharacterSetElement NON_WHITESPACE = 
            new CharacterSetElement(true);

        /**
         * The word character set. This element matches a single word
         * character.
         */
        public static CharacterSetElement WORD = 
            new CharacterSetElement(false);

        /**
         * The non-word character set. This element matches a single
         * non-word character.
         */
        public static CharacterSetElement NON_WORD = 
            new CharacterSetElement(true);

        /**
         * The inverted character set flag.
         */
        private bool inverted;

        /**
         * The character set content. This array may contain either
         * range objects or Character objects.
         */
        private ArrayList contents = new ArrayList();

        /**
         * Creates a new character set element. If the inverted character 
         * set flag is set, only characters NOT in the set will match.
         * 
         * @param inverted       the inverted character set flag
         */
        public CharacterSetElement(bool inverted) {
            this.inverted = inverted;
        }

        /**
         * Adds a single character to this character set.
         * 
         * @param c              the character to add
         */
        public void AddCharacter(char c) {
            contents.Add(c);
        }

        /**
         * Adds multiple characters to this character set.
         * 
         * @param str            the string with characters to add
         */
        public void AddCharacters(string str) {
            for (int i = 0; i < str.Length; i++) {
                AddCharacter(str[i]);
            }
        }

        /**
         * Adds multiple characters to this character set.
         * 
         * @param elem           the string element with characters to add
         */
        public void AddCharacters(StringElement elem) {
            AddCharacters(elem.GetString());
        }

        /**
         * Adds a character range to this character set.
         * 
         * @param min            the minimum character value
         * @param max            the maximum character value
         */
        public void AddRange(char min, char max) {
            contents.Add(new Range(min, max));
        }

        /**
         * Adds a character subset to this character set.
         * 
         * @param elem           the character set to add
         */
        public void AddCharacterSet(CharacterSetElement elem) {
            contents.Add(elem);
        }

        /**
         * Returns this element as the character set shouldn't be
         * modified after creation. This partially breaks the contract
         * of clone(), but as new characters are not added to the
         * character set after creation, this will work correctly.
         * 
         * @return this character set element
         */
        public override object Clone() {
            return this;
        }

        /**
         * Returns the length of a matching string starting at the 
         * specified position. The number of matches to skip can also be
         * specified, but numbers higher than zero (0) cause a failed 
         * match for any element that doesn't attempt to combine other 
         * elements.
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

            char  c;
            
            if (skip != 0) {
                return -1;
            }
            if (start >= str.Length) {
                m.SetReadEndOfString();
                return -1;
            }
            c = str[start];
            return InSet(c) ? 1 : -1;
        }

        /**
         * Checks if the specified character matches this character
         * set. This method takes the inverted flag into account.
         * 
         * @param c               the character to check
         * 
         * @return true if the character matches, or
         *         false otherwise
         */
        private bool InSet(char c) {
            if (this == DOT) {
                return InDotSet(c);
            } else if (this == DIGIT || this == NON_DIGIT) {
                return InDigitSet(c) != inverted;
            } else if (this == WHITESPACE || this == NON_WHITESPACE) {
                return InWhitespaceSet(c) != inverted;
            } else if (this == WORD || this == NON_WORD) {
                return InWordSet(c) != inverted;
            } else {
                return InUserSet(c) != inverted;
            }
        }
        
        /**
         * Checks if the specified character is present in the 'dot'
         * set. This method does not consider the inverted flag.
         * 
         * @param c               the character to check
         * 
         * @return true if the character is present, or
         *         false otherwise
         */
        private bool InDotSet(char c) {
            switch (c) {
            case '\n':
            case '\r':
            case '\u0085':
            case '\u2028':
            case '\u2029':
                return false;
            default:
                return true;
            }
        }

        /**
         * Checks if the specified character is a digit. This method
         * does not consider the inverted flag.
         * 
         * @param c               the character to check
         * 
         * @return true if the character is a digit, or
         *         false otherwise
         */
        private bool InDigitSet(char c) {
            return '0' <= c && c <= '9';
        }

        /**
         * Checks if the specified character is a whitespace
         * character. This method does not consider the inverted flag.
         * 
         * @param c               the character to check
         * 
         * @return true if the character is a whitespace character, or
         *         false otherwise
         */
        private bool InWhitespaceSet(char c) {
            switch (c) {
            case ' ':
            case '\t':
            case '\n':
            case '\f':
            case '\r':
            case (char) 11:
                return true;
            default:
                return false;
            }
        }

        /**
         * Checks if the specified character is a word character. This
         * method does not consider the inverted flag.
         * 
         * @param c               the character to check
         * 
         * @return true if the character is a word character, or
         *         false otherwise
         */
        private bool InWordSet(char c) {
            return ('a' <= c && c <= 'z')
                || ('A' <= c && c <= 'Z')
                || ('0' <= c && c <= '9')
                || c == '_';
        }

        /**
         * Checks if the specified character is present in the user-
         * defined set. This method does not consider the inverted
         * flag.
         * 
         * @param value           the character to check
         * 
         * @return true if the character is present, or
         *         false otherwise
         */
        private bool InUserSet(char value) {
            object               obj;
            char                 c;
            Range                r;
            CharacterSetElement  e;

            for (int i = 0; i < contents.Count; i++) {
                obj = contents[i];
                if (obj is char) {
                    c = (char) obj;
                    if (c == value) {
                        return true;
                    }
                } else if (obj is Range) {
                    r = (Range) obj;
                    if (r.Inside(value)) {
                        return true;
                    }
                } else if (obj is CharacterSetElement) {
                    e = (CharacterSetElement) obj;
                    if (e.InSet(value)) {
                        return true;
                    }
                }
            }
            return false;
        }
    
        /**
         * Prints this element to the specified output stream.
         * 
         * @param output         the output stream to use
         * @param indent         the current indentation
         */
        public override void PrintTo(TextWriter output, string indent) {
            output.WriteLine(indent + ToString());
        }

        /**
         * Returns a string description of this character set.
         * 
         * @return a string description of this character set
         */
        public override string ToString() {
            StringBuilder  buffer;

            // Handle predefined character sets
            if (this == DOT) {
                return ".";
            } else if (this == DIGIT) {
                return "\\d";
            } else if (this == NON_DIGIT) {
                return "\\D";
            } else if (this == WHITESPACE) {
                return "\\s";
            } else if (this == NON_WHITESPACE) {
                return "\\S";
            } else if (this == WORD) {
                return "\\w";
            } else if (this == NON_WORD) {
                return "\\W";
            }

            // Handle user-defined character sets        
            buffer = new StringBuilder();
            if (inverted) {
                buffer.Append("^[");
            } else {
                buffer.Append("[");
            }
            for (int i = 0; i < contents.Count; i++) {
                buffer.Append(contents[i]);
            }
            buffer.Append("]");
            
            return buffer.ToString();
        }


        /**
         * A character range class.
         */    
        private class Range {
            
            /**
             * The minimum character value.
             */
            private char min;
            
            /**
             * The maximum character value.
             */
            private char max;
            
            /**
             * Creates a new character range.
             * 
             * @param min        the minimum character value
             * @param max        the maximum character value
             */
            public Range(char min, char max) {
                this.min = min;
                this.max = max;
            }
            
            /**
             * Checks if the specified character is inside the range.
             * 
             * @param c          the character to check
             * 
             * @return true if the character is in the range, or
             *         false otherwise
             */
            public bool Inside(char c) {
                return min <= c && c <= max;
            }
            
            /**
             * Returns a string representation of this object.
             * 
             * @return a string representation of this object
             */
            public override string ToString() {
                return min + "-" + max;
            }
        }
    }
}
