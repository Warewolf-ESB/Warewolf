/*
 * ProductionPattern.cs
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
using System.Text;

namespace PerCederberg.Grammatica.Parser {

    /**
     * A production pattern. This class represents a set of production
     * alternatives that together forms a single production. A
     * production pattern is identified by an integer id and a name,
     * both provided upon creation. The pattern id is used for
     * referencing the production pattern from production pattern
     * elements.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    public class ProductionPattern {

        /**
         * The production pattern identity.
         */
        private int id;

        /**
         * The production pattern name.
         */
        private string name;

        /**
         * The syntectic production flag. If this flag is set, the 
         * production identified by this pattern has been artificially 
         * inserted into the grammar.  
         */
        private bool syntetic;

        /**
         * The list of production pattern alternatives.
         */
        private ArrayList alternatives;

        /**
         * The default production pattern alternative. This alternative
         * is used when no other alternatives match. It may be set to 
         * -1, meaning that there is no default (or fallback) alternative.
         */
        private int defaultAlt;

        /**
         * The look-ahead set associated with this pattern.
         */
        private LookAheadSet lookAhead;
        
        /**
         * Creates a new production pattern.
         * 
         * @param id             the production pattern id
         * @param name           the production pattern name
         */
        public ProductionPattern(int id, string name) {
            this.id = id;
            this.name = name;
            this.syntetic = false;
            this.alternatives = new ArrayList();
            this.defaultAlt = -1;
            this.lookAhead = null;
        }

        /**
         * Checks if the syntetic production flag is set. If this flag
         * is set, the production identified by this pattern has been
         * artificially inserted into the grammar. No parse tree nodes
         * will be created for such nodes, instead the child nodes
         * will be added directly to the parent node.
         *   
         * @return true if this production pattern is syntetic, or
         *         false otherwise
         */
        public bool IsSyntetic() {
            return syntetic;
        }

        /**
         * Checks if this pattern is recursive on the left-hand side.
         * This method checks if any of the production pattern
         * alternatives is left-recursive.
         *  
         * @return true if at least one alternative is left recursive, or
         *         false otherwise
         */
        public bool IsLeftRecursive() {
            ProductionPatternAlternative  alt;
            
            for (int i = 0; i < alternatives.Count; i++) {
                alt = (ProductionPatternAlternative) alternatives[i];
                if (alt.IsLeftRecursive()) {
                    return true;
                }
            }
            return false;
        }
    
        /**
         * Checks if this pattern is recursive on the right-hand side.
         * This method checks if any of the production pattern
         * alternatives is right-recursive.
         *  
         * @return true if at least one alternative is right recursive, or
         *         false otherwise
         */
        public bool IsRightRecursive() {
            ProductionPatternAlternative  alt;

            for (int i = 0; i < alternatives.Count; i++) {
                alt = (ProductionPatternAlternative) alternatives[i];
                if (alt.IsRightRecursive()) {
                    return true;
                }
            }
            return false;
        }

        /**
         * Checks if this pattern would match an empty stream of
         * tokens. This method checks if any one of the production
         * pattern alternatives would match the empty token stream.
         *  
         * @return true if at least one alternative match no tokens, or
         *         false otherwise
         */
        public bool IsMatchingEmpty() {
            ProductionPatternAlternative  alt;

            for (int i = 0; i < alternatives.Count; i++) {
                alt = (ProductionPatternAlternative) alternatives[i];
                if (alt.IsMatchingEmpty()) {
                    return true;
                }
            }
            return false;
        }

        /**
         * Returns the unique production pattern identity value.
         * 
         * @return the production pattern id
         */
        public int GetId() {
            return id;
        }
    
        /**
         * Returns the production pattern name.
         * 
         * @return the production pattern name
         */
        public string GetName() {
            return name;
        }

        /**
         * Sets the syntetic production pattern flag. If this flag is set, 
         * the production identified by this pattern has been artificially 
         * inserted into the grammar. By default this flag is set to 
         * false.
         * 
         * @param syntetic       the new value of the syntetic flag
         */
        public void SetSyntetic(bool syntetic) {
            this.syntetic = syntetic;
        }

        /**
         * Returns the number of alternatives in this pattern.
         *  
         * @return the number of alternatives in this pattern
         */
        public int GetAlternativeCount() {
            return alternatives.Count;
        }
    
        /**
         * Returns an alternative in this pattern.
         * 
         * @param pos            the alternative position, 0 &gt;= pos &gt; count
         * 
         * @return the alternative found
         */
        public ProductionPatternAlternative GetAlternative(int pos) {
            return (ProductionPatternAlternative) alternatives[pos];
        }

        /**
         * Adds a production pattern alternative.
         * 
         * @param alt            the production pattern alternative to add
         * 
         * @throws ParserCreationException if an identical alternative has
         *             already been added
         */    
        public void AddAlternative(ProductionPatternAlternative alt) {
            if (alternatives.Contains(alt)) {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    name,
                    "two identical alternatives exist"); 
            }
            alt.SetPattern(this);
            alternatives.Add(alt);
        }
        
        /**
         * Returns a string representation of this object.
         *
         * @return a token string representation
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();
            StringBuilder  indent = new StringBuilder();
            int            i;

            buffer.Append(name);
            buffer.Append("(");
            buffer.Append(id);
            buffer.Append(") ");
            for (i = 0; i < buffer.Length; i++) {
                indent.Append(" ");
            }
            for (i = 0; i < alternatives.Count; i++) {
                if (i == 0) {
                    buffer.Append("= ");
                } else {
                    buffer.Append("\n");
                    buffer.Append(indent);
                    buffer.Append("| ");
                }
                buffer.Append(alternatives[i]);
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

        /**
         * Returns the default pattern alternative. The default 
         * alternative is used when no other alternative matches.  
         * 
         * @return the default pattern alternative, or 
         *         null if none has been set
         */
        internal ProductionPatternAlternative GetDefaultAlternative() {
            if (defaultAlt >= 0) {
                object obj = alternatives[defaultAlt];
                return (ProductionPatternAlternative) obj;
            } else {
                return null;
            }
        }
    
        /**
         * Sets the default pattern alternative. The default alternative
         * is used when no other alternative matches.
         *   
         * @param pos            the position of the default alternative
         */
        internal void SetDefaultAlternative(int pos) {
            if (pos >= 0 && pos < alternatives.Count) {
                this.defaultAlt = pos;
            }
        }
    }
}
