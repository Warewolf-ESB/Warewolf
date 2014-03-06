/*
 * ProductionPatternElement.cs
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

#pragma warning disable 1570, 1591

using System;
using System.Text;

namespace PerCederberg.Grammatica.Parser {

    /**
     * A production pattern element. This class represents a reference to 
     * either a token or a production. Each element also contains minimum 
     * and maximum occurence counters, controlling the number of 
     * repetitions allowed. A production pattern element is always 
     * contained within a production pattern rule.  
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    public class ProductionPatternElement {

        /**
         * The token flag. This flag is true for token elements, and 
         * false for production elements.
         */
        private bool token;
        
        /**
         * The node identity.
         */
        private int id;
        
        /**
         * The minimum occurance count.
         */
        private int min;
        
        /**
         * The maximum occurance count.
         */
        private int max;
        
        /**
         * The look-ahead set associated with this element.
         */
        private LookAheadSet lookAhead;

        /**
         * Creates a new element. If the maximum value if zero (0) or
         * negative, it will be set to Int32.MaxValue.
         * 
         * @param isToken        the token flag
         * @param id             the node identity
         * @param min            the minimum number of occurancies
         * @param max            the maximum number of occurancies, or
         *                       negative for infinite 
         */
        public ProductionPatternElement(bool isToken, 
                                        int id, 
                                        int min, 
                                        int max) {
        
            this.token = isToken;
            this.id = id;
            if (min < 0) {
                min = 0;
            }
            this.min = min;
            if (max <= 0) {
                max = Int32.MaxValue;
            } else if (max < min) {
                max = min;
            } 
            this.max = max;
            this.lookAhead = null;
        }
    
        /**
         * Returns true if this element represents a token.
         * 
         * @return true if the element is a token, or
         *         false otherwise
         */
        public bool IsToken() {
            return token;
        }
    
        /**
         * Returns true if this element represents a production.
         * 
         * @return true if the element is a production, or
         *         false otherwise
         */
        public bool IsProduction() {
            return !token;
        }
    
        /**
         * Checks if a specific token matches this element. This
         * method will only return true if this element is a token
         * element, and the token has the same id and this element.
         * 
         * @param token          the token to check
         * 
         * @return true if the token matches this element, or
         *         false otherwise
         */
        public bool IsMatch(Token token) {
            return IsToken() && token != null && token.GetId() == id;
        }
        
        /**
         * Returns the node identity.
         * 
         * @return the node identity
         */
        public int GetId() {
            return id;
        }
    
        /**
         * Returns the minimum occurence count. 
         * 
         * @return the minimum occurence count
         */
        public int GetMinCount() {
            return min;
        }
    
        /**
         * Returns the maximum occurence count.
         * 
         * @return the maximum occurence count
         */
        public int GetMaxCount() {
            return max;
        }
    
        /**
         * Checks if this object is equal to another. This method only
         * returns true for another identical production pattern
         * element.
         * 
         * @param obj            the object to compare with
         * 
         * @return true if the object is identical to this one, or
         *         false otherwise
         */
        public override bool Equals(object obj) {
            ProductionPatternElement  elem;

            if (obj is ProductionPatternElement) {
                elem = (ProductionPatternElement) obj;
                return this.token == elem.token
                    && this.id == elem.id
                    && this.min == elem.min
                    && this.max == elem.max;
            } else {
                return false;
            }
        }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

        /**
         * Returns a string representation of this object.
         * 
         * @return a string representation of this object
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();

            buffer.Append(id);
            if (token) {
                buffer.Append("(Token)");
            } else {
                buffer.Append("(Production)");
            }
            if (min != 1 || max != 1) {
                buffer.Append("{");
                buffer.Append(min);
                buffer.Append(",");
                buffer.Append(max);
                buffer.Append("}");
            }
            return buffer.ToString();
        }
    
        /**
         * Returns the look-ahead set associated with this alternative.
         * 
         * @return the look-ahead set associated with this alternative
         */
        internal LookAheadSet GetLookAhead() {
            return lookAhead;
        }
    
        /**
         * Sets the look-ahead set for this alternative.
         * 
         * @param lookAhead      the new look-ahead set
         */
        internal void SetLookAhead(LookAheadSet lookAhead) {
            this.lookAhead = lookAhead;
        }
    }
}
