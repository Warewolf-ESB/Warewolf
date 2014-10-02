
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Parsing.Tokenization;

namespace System.Parsing.JSON
{
    public sealed class CharacterLiteralTokenizationHandler : TokenizationHandler<Token, TokenKind>
    {
        #region Constants
        private const byte SingleQuote = (byte)'\'';
        private const byte HexadecimalChar = (byte)'x';
        private const byte UnicodeChar = (byte)'u';
        private const byte Backslash = (byte)'\\';

        private const int ToggleState = 0;
        private const int PendingState = 1;
        #endregion

        #region Readonly Fields
        private static readonly char[] RegularStringEscapeCharacters = new char[] { '\'', '"', '\\', '0', 'a', 'b', 'f', 'n', 'r', 't', 'u', 'U', 'x', 'v' };
        private static readonly BooleanArray RegularStringEscapeMask = new BooleanArray(RegularStringEscapeCharacters, 256);
        #endregion

        #region Public Properties
        public sealed override bool IdentifyBeforeUnaryTokens { get { return true; } }
        public sealed override bool IdentifyAfterUnaryTokens { get { return true; } }
        #endregion

        #region Identification Handling
        public override void Reset(TokenizationExecutionStore<Token, TokenKind> store)
        {
            TokenizationHandlerExecutionStore hStore = store.GetHandlerStore(this);

            if (hStore == null)
            {
                hStore = new TokenizationHandlerExecutionStore();
                store.SetHandlerStore(this, hStore);
            }
            else hStore.State = new BitVector();
        }

        public override bool IdentifyToken(Tokenizer<Token, TokenKind> tokenizer, TokenizationExecutionStore<Token, TokenKind> store, ref TokenKind result)
        {
            TokenizationHandlerExecutionStore hStore = store.GetHandlerStore(this);
            bool toggle = hStore[ToggleState];
            hStore[ToggleState] = !toggle;

            if (toggle)
            {
                if (hStore[PendingState])
                {
                    hStore[PendingState] = false;
                    store.Input.Origin = store.Input.Position - 1;

                    if (store.Input.Current == Backslash)
                    {
                        if (store.Input.Next == HexadecimalChar)
                        {
                            result = TokenKind.HexadecimalChar;

                            byte current = Byte.MinValue;
                            int count = 0;

                            for (int i = store.Input.Position + 2; i < store.Input.Length; i++)
                                if ((current = store.Input[i]) == Backslash)
                                {
                                    store.Input.End = -1;
                                    return true;
                                }
                                else if (current == SingleQuote)
                                {
                                    store.Input.End = count > 0 ? i + 1 : -1;
                                    return true;
                                }
                                else if (count++ > 3 || !StringUtility.HexadecimalMask[current])
                                {
                                    store.Input.End = -1;
                                    return true;
                                }

                            store.Input.End = -1;
                        }
                        else if (store.Input.Next == UnicodeChar)
                        {
                            result = TokenKind.UnicodeChar;

                            byte current = Byte.MinValue;
                            int count = 0;

                            for (int i = store.Input.Position + 2; i < store.Input.Length; i++)
                                if ((current = store.Input[i]) == Backslash)
                                {
                                    store.Input.End = -1;
                                    return true;
                                }
                                else if (current == SingleQuote)
                                {
                                    store.Input.End = count == 4 ? i + 1 : -1;
                                    return true;
                                }
                                else if (count++ > 3 || !StringUtility.HexadecimalMask[current])
                                {
                                    store.Input.End = -1;
                                    return true;
                                }

                            store.Input.End = -1;
                        }
                        else
                        {
                            if (store.Input.Position + 2 >= store.Input.Length || store.Input[store.Input.Position + 2] != SingleQuote || !RegularStringEscapeMask[store.Input.Next]) store.Input.End = -1;
                            else
                            {
                                result = TokenKind.EscapedChar;
                                store.Input.End = store.Input.Position + 3;
                            }
                        }
                    }
                    else
                    {
                        if (store.Input.Current == SingleQuote || store.Input.Next != SingleQuote) store.Input.End = -1;
                        else
                        {
                            result = TokenKind.RegularChar;
                            store.Input.End = store.Input.Position + 2;
                        }
                    }

                    return true;
                }

                return false;
            }
            else
            {
                if (store.Input.Current != SingleQuote) return false;
                if (hStore[PendingState]) store.Input.End = -1;
                hStore[PendingState] = true;
                return true;
            }
        }
        #endregion
    }

    public sealed class StringLiteralTokenizationHandler : TokenizationHandler<Token, TokenKind>
    {
        #region Constants
        private const byte CommercialAt = (byte)'@';
        private const byte DoubleQuote = (byte)'"';
        private const byte Backslash = (byte)'\\';
        #endregion

        #region Identification Handling
        public override bool IdentifyToken(Tokenizer<Token, TokenKind> tokenizer, TokenizationExecutionStore<Token, TokenKind> store, ref TokenKind result)
        {
            if (store.Input.Current != DoubleQuote) return false;

            if (store.Input.Previous == CommercialAt)
            {
                result = TokenKind.VerbatimString;
                byte current = Byte.MinValue;

                for (int i = store.Input.Position + 1; i < store.Input.Length; i++)
                    if ((current = store.Input[i]) == DoubleQuote)
                    {
                        if (i + 1 >= store.Input.Length || store.Input[i + 1] != DoubleQuote)
                        {
                            store.Input.End = i + 1;
                            store.Input.Origin = store.Input.Position - 1;
                            return true;
                        }
                        else ++i;
                    }

                if (store.ExpectPartialToken)
                {
                    store.ExpectPartialToken = false;
                    store.Input.End = store.Input.Length;
                    store.Input.Origin = store.Input.Position - 1;
                    return true;
                }

                store.Tokenizer.EventLog.Log(new ParseEventLogEntry(store.Source, new ParseEventLogToken() { SourceIndex = store.Input.Position - 1, SourceLength = 2, Contents = "@\"", TokenIndex = store.Builder.Contents.Count, Definition = result }, null, -1, 1, "Tokenizer", "StringLiteralTokenizationHandler"));
                store.Input.Origin = store.Input.Position - 1;
                store.Input.End = -1;
            }
            else
            {
                result = TokenKind.RegularString;
                byte current = Byte.MinValue;

                for (int i = store.Input.Position + 1; i < store.Input.Length; i++)
                    if ((current = store.Input[i]) == Backslash)
                    {
                        if (i + 1 >= store.Input.Length || !StringUtility.RegularStringEscapeMask[store.Input[i + 1]])
                        {
                            if (store.ExpectPartialToken)
                            {
                                store.ExpectPartialToken = false;
                                store.Input.End = (i + 1 >= store.Input.Length) ? i + 1 : i + 2;
                                store.Input.Origin = store.Input.Position;
                                return true;
                            }

                            break;
                        }
                        else ++i;
                    }
                    else if (current == DoubleQuote)
                    {
                        store.Input.End = i + 1;
                        store.Input.Origin = store.Input.Position;
                        return true;
                    }

                if (store.ExpectPartialToken)
                {
                    store.ExpectPartialToken = false;
                    store.Input.End = store.Input.Length;
                    store.Input.Origin = store.Input.Position;
                    return true;
                }

                store.Tokenizer.EventLog.Log(new ParseEventLogEntry(store.Source, new ParseEventLogToken() { SourceIndex = store.Input.Position, SourceLength = 1, Contents = "\"", TokenIndex = store.Builder.Contents.Count, Definition = result }, null, -1, 2, "Tokenizer", "StringLiteralTokenizationHandler"));
                store.Input.Origin = store.Input.Position;
                store.Input.End = -1;
            }

            return true;
        }
        #endregion
    }

    public sealed class CSharpNumericLiteralTokenizationHandler : TokenizationHandler<Token, TokenKind>
    {
        #region Constants
        private const byte FullStop = (byte)'.';
        #endregion

        #region Identification Handling
        public override bool IdentifyToken(Tokenizer<Token, TokenKind> tokenizer, TokenizationExecutionStore<Token, TokenKind> store, ref TokenKind result)
        {
            if (store.Input.Index != store.Input.Position || !StringUtility.DigitMask[store.Input.Current]) return false;
            store.Input.End = DemarcateNumeric(store, store.Input.Position, out result);
            store.Input.Origin = store.Input.Position;
            return true;
        }

        private int DemarcateNumeric(TokenizationExecutionStore<Token, TokenKind> store, int index, out TokenKind kind)
        {
            kind = TokenKind.IntegerNoSuffix;
            if (index + 1 == store.Input.Length) return index + 1;
            bool fractional = false, hexadecimal = false;
            byte current = Byte.MinValue;
            int exponent = -2;

            BooleanArray mask = StringUtility.DigitMask;

            for (int i = index + 1; i < store.Input.Length; i++)
                if (!mask[current = store.Input[i]])
                {
                    if (!hexadecimal)
                    {
                        if (current == FullStop)
                        {
                            if (exponent != -2 || fractional || i + 1 == store.Input.Length || !mask[store.Input[i + 1]]) return i;
                            kind = TokenKind.RealNoSuffix;
                            fractional = true;
                            continue;
                        }
                        else if (current == 'e' || current == 'E')
                        {
                            if (exponent != -2 || i + 1 == store.Input.Length || (!mask[current = store.Input[i + 1]] && current != '-' && current != '+')) return i;
                            kind = TokenKind.RealNoSuffix;
                            exponent = i;
                            continue;
                        }
                        else if (current == 'x' && i == index + 1 && store.Input[index] == '0')
                        {
                            hexadecimal = true;
                            kind = TokenKind.IntegerHexadecimal;
                            mask = StringUtility.HexadecimalMask;
                            if (i + 1 == store.Input.Length || !mask[store.Input[i + 1]]) return i;
                            continue;
                        }
                        else if (current == '-' || current == '+')
                        {
                            if (exponent != i - 1) return i;
                            continue;
                        }
                        else
                        {
                            if (current == 'd' || current == 'D')
                            {
                                kind = TokenKind.RealSuffixD;
                                return i + 1;
                            }

                            if (current == 'f' || current == 'F')
                            {
                                kind = TokenKind.RealSuffixF;
                                return i + 1;
                            }

                            if (current == 'u' || current == 'U')
                            {
                                if (fractional || exponent != -2) return -1;

                                if (i + 1 == store.Input.Length || ((current = store.Input[i + 1]) != 'l' && current != 'L'))
                                {
                                    kind = TokenKind.IntegerSuffixU;
                                    return i + 1;
                                }

                                kind = TokenKind.IntegerSuffixUL;
                                return i + 2;
                            }

                            if (current == 'l' || current == 'L')
                            {
                                if (fractional || exponent != -2) return -1;

                                if (i + 1 == store.Input.Length || ((current = store.Input[i + 1]) != 'u' && current != 'U'))
                                {
                                    kind = TokenKind.IntegerSuffixL;
                                    return i + 1;
                                }

                                kind = TokenKind.IntegerSuffixUL;
                                return i + 2;
                            }
                        }
                    }

                    return i;
                }

            return store.Input.Length;
        }
        #endregion
    }
}
