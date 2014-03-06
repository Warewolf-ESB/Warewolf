/*
 * TokenPattern.cs
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
     * A token pattern. This class contains the definition of a token 
     * (i.e. it's pattern), and allows testing a string against this
     * pattern. A token pattern is uniquely identified by an integer id,
     * that must be provided upon creation. 
     *
     * @author   Per Cederberg
     * @version  1.1
     */
    public class TokenPattern {

        /**
         * The pattern type enumeration.
         */
        public enum PatternType {

            /*
             * The string pattern type is used for tokens that only
             * match an exact string.
             */
            STRING,

            /*
             * The regular expression pattern type is used for tokens
             * that match a regular expression.
             */
            REGEXP
        }

        /**
         * The token pattern identity.
         */
        private int id;

        /**
         * The token pattern name.
         */
        private string name;

        /**
         * The token pattern type.
         */
        private PatternType type;

        /**
         * The token pattern.
         */
        private string pattern;

        /**
         * The token error flag. If this flag is set, it means that an
         * error should be reported if the token is found. The error
         * message is present in the errorMessage variable.
         *
         * @see #errorMessage
         */
        private bool error = false;

        /**
         * The token error message. This message will only be set if the
         * token error flag is set.
         *
         * @see #error
         */
        private string errorMessage = null;

        /**
         * The token ignore flag. If this flag is set, it means that the
         * token should be ignored if found. If an ignore message is
         * present in the ignoreMessage variable, it will also be reported
         * as a warning.
         *
         * @see #ignoreMessage
         */
        private bool ignore = false;

        /**
         * The token ignore message. If this message is set when the token
         * ignore flag is also set, a warning message will be printed if
         * the token is found.
         *
         * @see #ignore
         */
        private string ignoreMessage = null;

        /**
         * Creates a new token pattern.
         *
         * @param id             the token pattern id
         * @param name           the token pattern name
         * @param type           the token pattern type
         * @param pattern        the token pattern
         */
        public TokenPattern(int id, 
                            string name, 
                            PatternType type, 
                            string pattern) {

            this.id = id;
            this.name = name;
            this.type = type;
            this.pattern = pattern;
        }

        /**
         * Checks if the pattern corresponds to an error token. If this 
         * is true, it means that an error should be reported if a 
         * matching token is found.
         * 
         * @return true if the pattern maps to an error token, or
         *         false otherwise
         */
        public bool IsError() {
            return error;
        }
    
        /**
         * Checks if the pattern corresponds to an ignored token. If this 
         * is true, it means that the token should be ignored if found.
         * 
         * @return true if the pattern maps to an ignored token, or
         *         false otherwise
         */
        public bool IsIgnore() {
            return ignore;
        }

        /**
         * Returns the unique token pattern identity value.
         * 
         * @return the token pattern id
         */
        public int GetId() {
            return id;
        }

        /**
         * Returns the token pattern name.
         * 
         * @return the token pattern name
         */
        public string GetName() {
            return name;
        }

        /**
         * Returns the token pattern type.
         * 
         * @return the token pattern type
         */
        public PatternType GetPatternType() {
            return type;
        }
    
        /**
         * Returns te token pattern.
         * 
         * @return the token pattern
         */
        public string GetPattern() {
            return pattern;
        }

        /**
         * Returns the token error message if the pattern corresponds to
         * an error token.
         *  
         * @return the token error message
         */
        public string GetErrorMessage() {
            return errorMessage;
        }

        /**
         * Returns the token ignore message if the pattern corresponds to
         * an ignored token.
         *  
         * @return the token ignore message
         */
        public string GetIgnoreMessage() {
            return ignoreMessage;
        }

        /**
         * Sets the token error flag and assigns a default error message.
         */
        public void SetError() {
            SetError("unrecognized token found");
        }

        /**
         * Sets the token error flag and assigns the specified error
         * message.
         *
         * @param message        the error message to display
         */
        public void SetError(string message) {
            error = true;
            errorMessage = message;
        }

        /**
         * Sets the token ignore flag and clears the ignore message.
         */
        public void SetIgnore() {
            SetIgnore(null);
        }

        /**
         * Sets the token ignore flag and assigns the specified ignore
         * message.
         *
         * @param message        the ignore message to display
         */
        public void SetIgnore(string message) {
            ignore = true;
            ignoreMessage = message;
        }

        /**
         * Returns a string representation of this object.
         *
         * @return a token pattern string representation
         */
        public override string ToString() {
            StringBuilder  buffer = new StringBuilder();

            buffer.Append(name);
            buffer.Append(" (");
            buffer.Append(id);
            buffer.Append("): ");
            switch (type) {
            case PatternType.STRING:
                buffer.Append("\"");
                buffer.Append(pattern);
                buffer.Append("\"");
                break;
            case PatternType.REGEXP:
                buffer.Append("<<");
                buffer.Append(pattern);
                buffer.Append(">>");
                break;
            }
            if (error) {
                buffer.Append(" ERROR: \"");
                buffer.Append(errorMessage);
                buffer.Append("\"");
            }
            if (ignore) {
                buffer.Append(" IGNORE");
                if (ignoreMessage != null) {
                    buffer.Append(": \"");
                    buffer.Append(ignoreMessage);
                    buffer.Append("\"");
                }
            }

            return buffer.ToString();
        }

        /**
         * Returns a short string representation of this object.
         *
         * @return a short string representation of this object
         */
        public string ToShortString() {
            StringBuilder  buffer = new StringBuilder();
            int            newline = pattern.IndexOf('\n');

            if (type == PatternType.STRING) {
                buffer.Append("\"");
                if (newline >= 0) {
                    if (newline > 0 && pattern[newline - 1] == '\r') {
                        newline--;
                    }
                    buffer.Append(pattern.Substring(0, newline));
                    buffer.Append("(...)");
                } else {
                    buffer.Append(pattern);
                }
                buffer.Append("\"");
            } else {
                buffer.Append("<");
                buffer.Append(name);
                buffer.Append(">");
            }

            return buffer.ToString();
        }
    }
}
