
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Parsing.Intellisense
{
    public static class TokenUtility
    {
        public static TokenPair BuildGroupSimple(Token start, TokenKind close)
        {
            if (start == null) return new TokenPair();
            TokenKind open = start.Definition;
            Token end = null;
            int balance = 1;

            for (Token token = start.NextNWS; token != null; token = token.NextNWS)
            {
                if (token.Definition == open)
                {
                    balance++;
                }
                else if (token.Definition == close)
                {
                    if (--balance == 0)
                    {
                        end = token;
                        break;
                    }
                }
            }

            return new TokenPair(start, end);
        }

        public static TokenPair BuildGroupSimple(Token start, TokenKind close, TokenKind alternateOpen)
        {
            if (start == null) return new TokenPair();
            TokenKind open = start.Definition;
            Token end = null;
            int balance = 1;

            for (Token token = start.NextNWS; token != null; token = token.NextNWS)
            {
                if (token.Definition == open || token.Definition == alternateOpen)
                {
                    balance++;
                }
                else if (token.Definition == close)
                {
                    if (--balance == 0)
                    {
                        end = token;
                        break;
                    }
                }
            }

            return new TokenPair(start, end);
        }

        public static TokenPair BuildReverseGroupSimple(Token end, TokenKind open)
        {
            if (end == null) return new TokenPair();
            TokenKind close = end.Definition;
            Token start = null;
            int balance = -1;

            for (Token token = end.PreviousNWS; token != null; token = token.PreviousNWS)
            {
                if (token.Definition == open)
                {
                    if (++balance == 0)
                    {
                        start = token;
                        break;
                    }
                }
                else if (token.Definition == close)
                {
                    if (--balance == 0)
                    {
                        start = token;
                        break;
                    }
                }
            }

            return new TokenPair(start, end);
        }
    }
}
