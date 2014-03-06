/*
 * ParserCreationException.cs
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
using System.Collections;
using System.Text;

namespace PerCederberg.Grammatica.Parser {

    /**
     * A parser creation exception. This exception is used for signalling
     * an error in the token or production patterns, making it impossible
     * to create a working parser or tokenizer.
     *
     * @author   Per Cederberg
     * @version  1.0
     */
    public class ParserCreationException : Exception {

        /**
         * The error type enumeration.
         */
        public enum ErrorType {

            /*
             * The internal error type is only used to signal an
             * error that is a result of a bug in the parser or
             * tokenizer code.
             */
            INTERNAL,

            /*
             * The invalid parser error type is used when the parser
             * as such is invalid. This error is typically caused by
             * using a parser without any patterns.
             */
            INVALID_PARSER,

            /*
             * The invalid token error type is used when a token
             * pattern is erroneous. This error is typically caused
             * by an invalid pattern type or an erroneous regular
             * expression.
             */
            INVALID_TOKEN,

            /*
             * The invalid production error type is used when a
             * production pattern is erroneous. This error is
             * typically caused by referencing undeclared productions,
             * or violating some other production pattern constraint.
             */
            INVALID_PRODUCTION,

            /*
             * The infinite loop error type is used when an infinite
             * loop has been detected in the grammar. One of the
             * productions in the loop will be reported.
             */
            INFINITE_LOOP,

            /*
             * The inherent ambiguity error type is used when the set
             * of production patterns (i.e. the grammar) contains
             * ambiguities that cannot be resolved.
             */
            INHERENT_AMBIGUITY
        }

        /**
         * The error type.
         */
        private ErrorType type;

        /**
         * The token or production pattern name. This variable is only
         * set for some error types.
         */
        private string name;

        /**
         * The additional error information string. This variable is only
         * set for some error types.
         */
        private string info;

        /**
         * The error details list. This variable is only set for some
         * error types.
         */
        private ArrayList details;

        /**
         * Creates a new parser creation exception.
         *
         * @param type           the parse error type
         * @param info           the additional error information
         */
        public ParserCreationException(ErrorType type,
                                       String info)
            : this(type, null, info) {
        }

        /**
         * Creates a new parser creation exception.
         *
         * @param type           the parse error type
         * @param name           the token or production pattern name
         * @param info           the additional error information
         */
        public ParserCreationException(ErrorType type,
                                       String name,
                                       String info)
            : this(type, name, info, null) {
        }

        /**
         * Creates a new parser creation exception.
         *
         * @param type           the parse error type
         * @param name           the token or production pattern name
         * @param info           the additional error information
         * @param details        the error details list
         */
        public ParserCreationException(ErrorType type,
                                       String name,
                                       String info,
                                       ArrayList details) {

            this.type = type;
            this.name = name;
            this.info = info;
            this.details = details;
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
         * Returns the error type.
         *
         * @return the error type
         */
        public ErrorType GetErrorType() {
            return type;
        }

        /**
         * Returns the token or production name.
         *
         * @return the token or production name
         */
        public string GetName() {
            return name;
        }

        /**
         * Returns the additional error information.
         *
         * @return the additional error information
         */
        public string GetInfo() {
            return info;
        }

        /**
         * Returns the detailed error information as a string
         *
         * @return the detailed error information
         */
        public string GetDetails() {
            StringBuilder  buffer = new StringBuilder();

            if (details == null) {
                return null;
            }
            for (int i = 0; i < details.Count; i++) {
                if (i > 0) {
                    buffer.Append(", ");
                    if (i + 1 == details.Count) {
                        buffer.Append("and ");
                    }
                }
                buffer.Append(details[i]);
            }

            return buffer.ToString();
        }

        /**
         * Returns the error message. This message will contain all the
         * information available.
         *
         * @return the error message
         */
        public string GetMessage() {
            StringBuilder  buffer = new StringBuilder();

            switch (type) {
            case ErrorType.INVALID_PARSER:
                buffer.Append("parser is invalid, as ");
                buffer.Append(info);
                break;
            case ErrorType.INVALID_TOKEN:
                buffer.Append("token '");
                buffer.Append(name);
                buffer.Append("' is invalid, as ");
                buffer.Append(info);
                break;
            case ErrorType.INVALID_PRODUCTION:
                buffer.Append("production '");
                buffer.Append(name);
                buffer.Append("' is invalid, as ");
                buffer.Append(info);
                break;
            case ErrorType.INFINITE_LOOP:
                buffer.Append("infinite loop found in production pattern '");
                buffer.Append(name);
                buffer.Append("'");
                break;
            case ErrorType.INHERENT_AMBIGUITY:
                buffer.Append("inherent ambiguity in production '");
                buffer.Append(name);
                buffer.Append("'");
                if (info != null) {
                    buffer.Append(" ");
                    buffer.Append(info);
                }
                if (details != null) {
                    buffer.Append(" starting with ");
                    if (details.Count > 1) {
                        buffer.Append("tokens ");
                    } else {
                        buffer.Append("token ");
                    }
                    buffer.Append(GetDetails());
                }
                break;
            default:
                buffer.Append("internal error");
                break;
            }

            return buffer.ToString();
        }
    }
}
