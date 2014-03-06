/*
 * CombineElement.cs
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
     * A regular expression combination element. This element matches
     * two consecutive elements.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    internal class CombineElement : Element {

        /**
         * The first element.
         */
        private Element elem1;

        /**
         * The second element.
         */    
        private Element elem2;

        /**
         * Creates a new combine element.
         * 
         * @param first          the first element
         * @param second         the second element
         */
        public CombineElement(Element first, Element second) {
            elem1 = first;
            elem2 = second;
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
            return new CombineElement(elem1, elem2);
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
         * @return the length of the longest matching string, or
         *         -1 if no match was found
         */
        public override int Match(Matcher m, 
                                  string str, 
                                  int start, 
                                  int skip) {

            int  length1 = -1;
            int  length2 = 0;
            int  skip1 = 0;
            int  skip2 = 0;
        
            while (skip >= 0) {
                length1 = elem1.Match(m, str, start, skip1);
                if (length1 < 0) {
                    return -1;
                }
                length2 = elem2.Match(m, str, start + length1, skip2);
                if (length2 < 0) {
                    skip1++;
                    skip2 = 0;
                } else {
                    skip2++;
                    skip--;
                }
            }
        
            return length1 + length2;
        }

        /**
         * Prints this element to the specified output stream.
         * 
         * @param output         the output stream to use
         * @param indent         the current indentation
         */
        public override void PrintTo(TextWriter output, string indent) {
            elem1.PrintTo(output, indent);
            elem2.PrintTo(output, indent);
        }
    }
}
