/*
 * Production.cs
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

namespace PerCederberg.Grammatica.Parser {

    /**
     * A production node. This class represents a grammar production
     * (i.e. a list of child nodes) in a parse tree. The productions
     * are created by a parser, that adds children a according to a
     * set of production patterns (i.e. grammar rules).
     *
     * @author   Per Cederberg
     * @version  1.1
     */
    public class Production : Node {

        /**
         * The production pattern used for this production.
         */
        private ProductionPattern pattern;

        /**
         * The child nodes.
         */
        private ArrayList children;

        /**
         * Creates a new production node.
         * 
         * @param pattern        the production pattern
         */
        public Production(ProductionPattern pattern) {
            this.pattern = pattern;
            this.children = new ArrayList();
        }

        /**
         * Checks if this node is hidden, i.e. if it should not be visible
         * outside the parser.
         * 
         * @return true if the node should be hidden, or
         *         false otherwise
         */
        public override bool IsHidden() {
            return pattern.IsSyntetic();
        }

        /**
         * Returns the production pattern for this production.
         * 
         * @return the production pattern
         */
        public ProductionPattern GetPattern() {
            return pattern;
        }

        /**
         * Returns the production (pattern) id. This value is set as a 
         * unique identifier when creating the production pattern to 
         * simplify later identification.
         * 
         * @return the production id
         */
        public override int GetId() {
            return pattern.GetId();    
        }

        /**
         * Returns the production node name.
         * 
         * @return the production node name
         */
        public override string GetName() {
            return pattern.GetName();
        }

        /**
         * Returns the number of child nodes.
         * 
         * @return the number of child nodes
         */
        public override int GetChildCount() {
            return children.Count;
        }

        /**
         * Returns the child node with the specified index.
         * 
         * @param index          the child index, 0 &gt;= index &gt; count
         * 
         * @return the child node found, or 
         *         null if index out of bounds
         */
        public override Node GetChildAt(int index) {
            if (index < 0 || index >= children.Count) {
                return null; 
            } else {
                return (Node) children[index];
            }
        }
    
        /**
         * Adds a child node. The node will be added last in the list of
         * children.
         * 
         * @param child          the child node to add
         */
        public void AddChild(Node child) {
            if (child != null) {
                child.SetParent(this);
                children.Add(child);
            }
        }

        /**
         * Returns a string representation of this production.
         * 
         * @return a string representation of this production
         */
        public override string ToString() {
            return pattern.GetName() + '(' +
                   pattern.GetId() + ')';
        }
    }
}
