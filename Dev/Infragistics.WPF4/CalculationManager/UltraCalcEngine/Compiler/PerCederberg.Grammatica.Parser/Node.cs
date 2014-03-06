/*
 * Node.cs
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

namespace PerCederberg.Grammatica.Parser {

    /**
     * An abstract parse tree node. This class is inherited by all
     * nodes in the parse tree, i.e. by the token and production
     * classes.
     *
     * @author   Per Cederberg,
     * @version  1.2
     */
    public abstract class Node {

        /**
         * The parent node. 
         */
        private Node parent = null;

        /**
         * The computed node values. 
         */
        private ArrayList values = null;

        /**
         * Checks if this node is hidden, i.e. if it should not be
         * visible outside the parser.
         * 
         * @return true if the node should be hidden, or
         *         false otherwise
         */
        public virtual bool IsHidden() {
            return false;
        }

        /**
         * Returns the node type id. This value is set as a unique
         * identifier for each type of node, in order to simplify
         * later identification.
         * 
         * @return the node type id
         */
        public abstract int GetId();

        /**
         * Returns the node name.
         * 
         * @return the node name
         */
        public abstract string GetName();

        /**
         * The line number of the first character in this node. If the
         * node has child elements, this value will be fetched from
         * the first child.
         * 
         * @return the line number of the first character, or
         *         -1 if not applicable
         */
        public virtual int GetStartLine() {
            int  line;
            
            for (int i = 0; i < GetChildCount(); i++) {
                line = GetChildAt(i).GetStartLine();
                if (line >= 0) {
                    return line;
                }
            }
            return -1;
        }
    
        /**
         * The column number of the first character in this node. If
         * the node has child elements, this value will be fetched
         * from the first child.
         * 
         * @return the column number of the first token character, or
         *         -1 if not applicable
         */
        public virtual int GetStartColumn() {
            int  col;
            
            for (int i = 0; i < GetChildCount(); i++) {
                col = GetChildAt(i).GetStartColumn();
                if (col >= 0) {
                    return col;
                }
            }
            return -1;
        }
    
        /**
         * The line number of the last character in this node. If the
         * node has child elements, this value will be fetched from
         * the last child.
         * 
         * @return the line number of the last token character, or
         *         -1 if not applicable
         */
        public virtual int GetEndLine() {
            int  line;
            
            for (int i = GetChildCount() - 1; i >= 0; i--) {
                line = GetChildAt(i).GetEndLine();
                if (line >= 0) {
                    return line;
                }
            }
            return -1;
        }
    
        /**
         * The column number of the last character in this node. If
         * the node has child elements, this value will be fetched
         * from the last child.
         * 
         * @return the column number of the last token character, or
         *         -1 if not applicable
         */
        public virtual int GetEndColumn() {
            int  col;
            
            for (int i = GetChildCount() - 1; i >= 0; i--) {
                col = GetChildAt(i).GetEndColumn();
                if (col >= 0) {
                    return col;
                }
            }
            return -1;
        }

        /**
         * Returns the parent node. 
         * 
         * @return the parent parse tree node
         */
        public Node GetParent() {
            return parent;
        }

        /**
         * Sets the parent node.
         * 
         * @param parent         the new parent node
         */
        internal void SetParent(Node parent) {
            this.parent = parent;
        }

        /**
         * Returns the number of child nodes.
         * 
         * @return the number of child nodes
         */
        public virtual int GetChildCount() {
            return 0;
        }

        /**
         * Returns the child node with the specified index.
         * 
         * @param index          the child index, 0 &gt;= index &gt; count
         * 
         * @return the child node found, or 
         *         null if index out of bounds
         */
        public virtual Node GetChildAt(int index) {
            return null; 
        }
    
        /**
         * Returns the number of descendant nodes.
         * 
         * @return the number of descendant nodes
         * 
         * @since 1.2
         */
        public int GetDescendantCount() {
            int  count = 0;
        
            for (int i = 0; i < GetChildCount(); i++) {
                count += 1 + GetChildAt(i).GetDescendantCount();
            }
            return count;
        }

        /**
         * Returns the number of computed values associated with this
         * node. Any number of values can be associated with a node
         * through calls to AddValue().
         *
         * @return the number of values associated with this node
         */
        public int GetValueCount() {
            if (values == null) {
                return 0;
            } else {
                return values.Count;
            }
        }

        /**
         * Returns a computed value of this node, if previously set. A
         * value may be used for storing intermediate results in the
         * parse tree during analysis.
         *
         * @param pos             the value position, 0 &gt;= pos &gt; count 
         *
         * @return the computed node value, or
         *         null if not set
         */
        public object GetValue(int pos) {
            if (values == null || pos < 0 || pos >= values.Count) {
                return null;
            } else {
                return values[pos];
            }
        }
    
        /**
         * Returns the list with all the computed values for this
         * node. Note that the list is not a copy, so changes will
         * affect the values in this node (as it is the same object).
         *
         * @return a list with all values, or
         *         null if no values have been set 
         */
        public ArrayList GetAllValues() {
            return values;
        }

        /**
         * Adds a computed value to this node. The computed value may
         * be used for storing intermediate results in the parse tree
         * during analysis.
         * 
         * @param value          the node value
         */
        public void AddValue(object value) {
            if (value != null) {
                if (values == null) {
                    values = new ArrayList();
                }
                values.Add(value);
            }
        }
    
        /**
         * Adds a set of computed values to this node.
         *
         * @param values         the vector with node values
         */
        public void AddValues(ArrayList values) {
        	if (values != null) {
                for (int i = 0; i < values.Count; i++) {
                    AddValue(values[i]);
                }
        	}
        }
        
        /**
         * Removes all computed values stored in this node.
         */
        public void RemoveAllValues() {
            values = null;
        }

        /**
         * Prints this node and all subnodes to the specified output 
         * stream.
         * 
         * @param output         the output stream to use
         */
        public void PrintTo(TextWriter output) {
            PrintTo(output, "");
            output.Flush();
        }
    
        /**
         * Prints this node and all subnodes to the specified output 
         * stream.
         * 
         * @param output         the output stream to use
         * @param indent         the indentation string
         */
        private void PrintTo(TextWriter output, string indent) {
            output.WriteLine(indent + ToString());
            indent = indent + "  ";
            for (int i = 0; i < GetChildCount(); i++) {
                GetChildAt(i).PrintTo(output, indent);
            }
        }
    }
}
