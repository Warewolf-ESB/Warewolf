using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Parsing.JSON
{
    public struct TokenPair
    {
        public Token Start;
        public Token End;

        public string Content { get { return Start.Source.Substring(Start.SourceIndex, (End.SourceIndex + End.SourceLength) - Start.SourceIndex); } }

        public TokenPair(TokenPair pair)
        {
            Start = pair.Start;
            End = pair.End;
        }

        public TokenPair(Token token)
        {
            Start = token;
            End = token;
        }

        public TokenPair(Token start, Token end)
        {
            Start = start;
            End = end;
        }

        public bool Contains(int sourceIndex)
        {
            sourceIndex -= Start.SourceIndex;
            return sourceIndex >= 0 && sourceIndex < ((End.SourceIndex + End.SourceLength) - Start.SourceIndex);
        }

        public static implicit operator TokenPair(Token token)
        {
            return new TokenPair(token);
        }
    }
}
