/*
 * RegExpException.cs
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

namespace PerCederberg.Grammatica.Parser.RE {

    /**
     * A regular expression exception. This exception is thrown if a
     * regular expression couldn't be processed (or "compiled")
     * properly.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    public class RegExpException : Exception {

        /**
         * The error type enumeration.
         */
        public enum ErrorType {

            /*
             * The unexpected character error constant. This error is
             * used when a character was read that didn't match the
             * allowed set of characters at the given position.
             */
            UNEXPECTED_CHARACTER,

            /*
             * The unterminated pattern error constant. This error is
             * used when more characters were expected in the pattern.
             */
            UNTERMINATED_PATTERN,
    
            /*
             * The unsupported special character error constant. This
             * error is used when special regular expression
             * characters are used in the pattern, but not supported
             * in this implementation.
             */
            UNSUPPORTED_SPECIAL_CHARACTER,
    
            /*
             * The unsupported escape character error constant. This
             * error is used when an escape character construct is
             * used in the pattern, but not supported in this
             * implementation.
             */
            UNSUPPORTED_ESCAPE_CHARACTER,
    
            /*
             * The invalid repeat count error constant. This error is
             * used when a repetition count of zero is specified, or
             * when the minimum exceeds the maximum.
             */
            INVALID_REPEAT_COUNT
        }

        /**
         * The error type constant.
         */
        private ErrorType type;
    
        /**
         * The error position.
         */
        private int position;

        /**
         * The regular expression pattern.
         */    
        private string pattern;

        /**
         * Creates a new regular expression exception.
         * 
         * @param type           the error type constant
         * @param pos            the error position
         * @param pattern        the regular expression pattern
         */
        public RegExpException(ErrorType type, int pos, string pattern) {
            this.type = type;
            this.position = pos;
            this.pattern = pattern;
        }
    
        /**
         * The message property. This property contains the detailed
         * exception error message.
         */
        public override string Message {
            get{
                return GetMessage(); 
            }   
        }

        /**
         * Returns the exception error message.
         * 
         * @return the exception error message
         */
        public string GetMessage() {
            StringBuilder  buffer = new StringBuilder();
            
            // Append error type name
            switch (type) {
            case ErrorType.UNEXPECTED_CHARACTER:
                buffer.Append("unexpected character");
                break;
            case ErrorType.UNTERMINATED_PATTERN:
                buffer.Append("unterminated pattern");
                break;
            case ErrorType.UNSUPPORTED_SPECIAL_CHARACTER:
                buffer.Append("unsupported character");
                break;
            case ErrorType.UNSUPPORTED_ESCAPE_CHARACTER:
                buffer.Append("unsupported escape character");
                break;
            case ErrorType.INVALID_REPEAT_COUNT:
                buffer.Append("invalid repeat count");
                break;
            default:
                buffer.Append("internal error");
                break;
            }

            // Append erroneous character
            buffer.Append(": ");
            if (position < pattern.Length) {
                buffer.Append('\'');
                buffer.Append(pattern.Substring(position));
                buffer.Append('\'');
            } else {
                buffer.Append("<end of pattern>");
            }
            
            // Append position
            buffer.Append(" at position ");
            buffer.Append(position);
            
            return buffer.ToString();
        }
    }
}
