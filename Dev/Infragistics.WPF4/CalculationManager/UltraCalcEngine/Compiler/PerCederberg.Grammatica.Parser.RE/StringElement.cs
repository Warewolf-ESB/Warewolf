/*
 * StringElement.cs
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

using System.IO;

namespace PerCederberg.Grammatica.Parser.RE {

    /**
     * A regular expression string element. This element only matches
     * an exact string. Once created, the string element is immutable.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    internal class StringElement : Element {
    
        /**
         * The string to match with.
         */
        private string value;
    
        /**
         * Creates a new string element.
         * 
         * @param c              the character to match with
         */
        public StringElement(char c) 
            : this(c.ToString()) {
        }

        /**
         * Creates a new string element.
         * 
         * @param str            the string to match with
         */
        public StringElement(string str) {
            value = str;
        }

        /**
         * Returns the string to be matched.
         * 
         * @return the string to be matched
         */
        public string GetString() {
            return value;
        }

        /**
         * Returns this element as it is immutable. 
         * 
         * @return this string element
         */
        public override object Clone() {
            return this;
        }

        /**
         * Returns the length of a matching string starting at the
         * specified position. The number of matches to skip can also
         * be specified, but numbers higher than zero (0) cause a
         * failed match for any element that doesn't attempt to
         * combine other elements.
         *
         * @param m              the matcher being used 
         * @param str            the string to match
         * @param start          the starting position
         * @param skip           the number of matches to skip
         * 
         * @return the length of the longest matching string, or
         *         -1 if no match was found
         */
        public override int Match(Matcher m, 
                                  string str, 
                                  int start, 
                                  int skip) {

            if (skip != 0) {
                return -1;
            }
            for (int i = 0; i < value.Length; i++) {
                if (start + i >= str.Length) {
                    m.SetReadEndOfString();
                    return -1;
                }
                if (str[start + i] != value[i]) {
                    return -1;
                }
            }
            return value.Length;
        }

        /**
         * Prints this element to the specified output stream.
         * 
         * @param output         the output stream to use
         * @param indent         the current indentation
         */
        public override void PrintTo(TextWriter output, string indent) {
            output.WriteLine(indent + "'" + value + "'");
        }
    }
}
