
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

namespace System.Parsing.Tokenization
{
    #region TokenDefinitionMask
    internal sealed class TokenDefinitionMask
    {
        private int[] _buckets;
        private int _offset;
        private int _length;
       
        public bool this[TokenDefinition index] { get { return GetValue(index); } set { SetValue(index, value); } }

        public TokenDefinitionMask(params TokenDefinition[] definitions)
        {
            int length = 0;

            if (definitions != null && definitions.Length > 0)
            {
                int min = Int32.MaxValue;
                int max = Int32.MinValue;

                for (int i = 0; i < definitions.Length; i++)
                {
                    TokenDefinition current = definitions[i];

                    if (min > current.Serial) min = current.Serial;
                    if (max < current.Serial) max = current.Serial;
                }

                if (max != Int32.MinValue && min != Int32.MaxValue)
                {
                    _offset = (min >> 5) << 5;
                    length = ((max - _offset) + 1);
                }
            }

            _buckets = new int[(length + 31) >> 5];
            _length = length;

            if (definitions != null && definitions.Length > 0)
                for (int i = 0; i < definitions.Length; i++)
                {
                    int index = definitions[i].Serial - _offset;
                    int bucket = index >> 5;
                    _buckets[bucket] |= 1 << (index - (bucket << 5));
                }
        }

        private bool GetValue(TokenDefinition def)
        {
            if (def == null) throw new ArgumentNullException("index");
            int index = def.Serial - _offset;
            if (index < 0 || index >= _length) return false;
            int bucket = index >> 5;
            return ((_buckets[bucket] & (1 << (index - (bucket << 5)))) != 0);
        }

        private void SetValue(TokenDefinition def, bool value)
        {
            if (def == null) throw new ArgumentNullException("index");
            int index = def.Serial - _offset;

            if (index < 0)
            {
                int nBucket = def.Serial >> 5;
                int nOffset = nBucket << 5;
                int nLength = (((_length - 1) + _offset) - nOffset) + 1;
                int ofsBucket = (_offset >> 5) - nBucket;
                int[] nBuckets = new int[(nLength + 31) >> 5];
                for (int i = 0; i < _buckets.Length; i++) nBuckets[i + ofsBucket] = _buckets[i];

                _offset = nOffset;
                _buckets = nBuckets;
                _length = nLength;
                index = def.Serial - _offset;
            }
            else if (index >= _length)
            {
                int nLength = index + 1;
                int[] nBuckets = new int[(nLength + 31) >> 5];
                Array.Copy(_buckets, 0, nBuckets, 0, _buckets.Length);
                _buckets = nBuckets;
                _length = nLength;
            }

            int bucket = index >> 5;
            if (value) _buckets[bucket] |= 1 << (index - (bucket << 5));
            else _buckets[bucket] &= ~(1 << (index - (bucket << 5)));
        }
    }
    #endregion

    #region ITokenBuilder
    public interface ITokenBuilder
    {
        void Append(TokenDefinition definition);
    }
    #endregion

    #region TokenDefinition
    /// <summary>
    /// Abstract base class that represents a particular syntactic element in a tokenization output stream. Used in conjunction
    /// with tokens to represent multiple occurance of particular syntactic elements,
    /// </summary>
    public abstract class TokenDefinition
    {
        #region Static Members
        private static Dictionary<Type, int> _typeSerialCounters = new Dictionary<Type, int>();

        private static int AcquireSerial(Type type)
        {
            int counter;
            if (!_typeSerialCounters.TryGetValue(type, out counter)) counter = 0;
            int result = counter++;
            _typeSerialCounters[type] = counter;
            return result;
        }

        /// <summary>
        /// Gets the total number instances of <paramref name="type"/> that have been instantiated.
        /// </summary>
        /// <param name="type">The type of the definitions to total.</param>
        /// <returns>The total number of instances of <paramref name="type"/></returns>
        public static int GetTotalDefinitionsOfType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            int counter;
            if (!_typeSerialCounters.TryGetValue(type, out counter)) counter = 0;
            return counter;
        }
        #endregion

        #region Instance Fields
        private string _name;
        private string _identifier;
        protected int _serial;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string Name { get { return _name; } }
        /// <summary>
        /// Gets the identifier that is used to identify this definition during tokenization if this definition was added as a keyword
        /// or a unary definition.
        /// </summary>
        public string Identifier { get { return _identifier; } }
        /// <summary>
        /// Gets a unique integer that represents this particular instance of this exact type of TokenDefinition.
        /// </summary>
        public int Serial { get { return _serial; } }
        /// <summary>
        /// Gets a value that indicates if this token definition should be categorized as whitespace.
        /// </summary>
        public virtual bool IsWhitespace { get { return false; } }
        /// <summary>
        /// Gets a value that indicates if this token definition is used to represent non-whitespace characters that do not match any
        /// other definitions.
        /// </summary>
        public virtual bool IsUnknown { get { return false; } }
        /// <summary>
        /// Gets a value that indicates if this token definition represents the end of the input string.
        /// </summary>
        public virtual bool IsEndOfFile { get { return false; } }
        /// <summary>
        /// Gets a value that is used as a hint to the AbstractSyntaxTreeBuilder as to whether or not this token definition should be identified
        /// as a keyword as opposed to a unary identification.
        /// </summary>
        public virtual bool IsKeyword { get { return false; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a TokenDefinition with the given <paramref name="name"/> and <paramref name="identifier"/>
        /// </summary>
        /// <param name="name">The display name of this token definition. (Used for debug/logging purposes)</param>
        /// <param name="identifier">The identifier that is used during tokenization to create occurences of this token definition.</param>
        public TokenDefinition(string name, string identifier)
        {
            _name = name;
            _identifier = identifier;
            _serial = AcquireSerial(GetType());
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return _name;
        }
        #endregion
    }
    #endregion

    #region Token<T, K>
    /// <summary>
    /// Represents an occurance of a TokenDefinition in an input stream, acts as a ordered linked list and maintains references
    /// to previous and post occurences of whitespace and non-whitespace tokens.
    /// </summary>
    /// <typeparam name="T">The type of Token used by the host tokenizer.</typeparam>
    /// <typeparam name="K">The type of TokenDefinition used by the host tokenizer.</typeparam>
    public class Token<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        /// <summary>
        /// The original, unaltered input string that was used in the tokenization request that generated this token.
        /// </summary>
        public string Source;
        /// <summary>
        /// The token that occured directly before this token.
        /// </summary>
        public T Previous;
        /// <summary>
        /// The first token that had a non-whitespace token definition that occured before this token.
        /// </summary>
        public T PreviousNWS;
        /// <summary>
        /// The first token that had a whitespace token definition that occured before this token.
        /// </summary>
        public T PreviousWS;
        /// <summary>
        /// The token that occured directly after this token.
        /// </summary>
        public T Next;
        /// <summary>
        /// The first token that had a non-whitespace token definition that occured after this token.
        /// </summary>
        public T NextNWS;
        /// <summary>
        /// The first token that had a whitespace token definition that occured after this token.
        /// </summary>
        public T NextWS;
        /// <summary>
        /// The index at which this token can be located in the array of tokens returned by the tokenization request.
        /// </summary>
        public int TokenIndex;
        /// <summary>
        /// The index in <see cref="Source"/> at which this token begins representing characters.
        /// </summary>
        public int SourceIndex;
        /// <summary>
        /// The number of characters in <see cref="Source"/> starting at <see cref="SourceIndex"/> that this token represents.
        /// </summary>
        public int SourceLength;
        /// <summary>
        /// The definition that this token represents.
        /// </summary>
        public K Definition;
        /// <summary>
        /// The string content in <see cref="Source"/> that this token represents.
        /// </summary>
        public string Content { get { return Source == null || SourceLength <= 0 ? "(empty)" : Source.Substring(SourceIndex, SourceLength); } }
        /// <summary>
        /// Returns the text that occurs between this token and <paramref name="other"/>
        /// </summary>
        /// <param name="other">Another token from the same tokenization request.</param>
        /// <returns>The text that occurs between the current token and <paramref name="other"/></returns>
        public string GetContentBetween(T other)
        {
            return (SourceIndex < other.SourceIndex) ? Source.Substring(SourceIndex, (other.SourceIndex - SourceIndex) + other.SourceLength) : Source.Substring(other.SourceIndex, (SourceIndex - other.SourceIndex) + SourceLength);
        }

        public ParseEventLogToken ToParseEventLogToken()
        {
            return new ParseEventLogToken() { SourceIndex = SourceIndex, SourceLength = SourceLength, Definition = Definition, TokenIndex = TokenIndex, Contents = Content };
        }

        public static bool operator <(Token<T, K> l, Token<T, K> r)
        {
            return l == r ? false : (l == null ? false : (r == null ? true : (l.SourceIndex < r.SourceIndex)));
        }

        public static bool operator <=(Token<T, K> l, Token<T, K> r)
        {
            return l == r ? true : (l == null ? false : (r == null ? true : (l.SourceIndex <= r.SourceIndex)));
        }

        public static bool operator >(Token<T, K> l, Token<T, K> r)
        {
            return l == r ? false : (l == null ? false : (r == null ? true : (l.SourceIndex > r.SourceIndex)));
        }

        public static bool operator >=(Token<T, K> l, Token<T, K> r)
        {
            return l == r ? true : (l == null ? false : (r == null ? true : (l.SourceIndex >= r.SourceIndex)));
        }
    }
    #endregion

    #region TokenBuilder<T, K>
    internal sealed class TokenBuilder<T, K> : ITokenBuilder
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        private string _source;
        private T _previous;
        private T _previousNWS;
        private T _previousWS;
        private ExposedList<T> _contents;

        internal ExposedList<T> Contents { get { return _contents; } }

        internal TokenBuilder(string source)
        {
            _source = source;
            _contents = new ExposedList<T>();
        }

        public void Append(TokenDefinition definition)
        {
            Append(0, 0, (K)definition);
        }

        public T Append(int index, int length, K definition)
        {
            T token = new T();

            token.Source = _source;
            token.TokenIndex = _contents.Count;
            token.SourceIndex = index;
            token.SourceLength = length;
            token.Definition = definition;

            token.Previous = _previous;
            token.PreviousNWS = _previousNWS;
            token.PreviousWS = _previousWS;

            if (_previous != null) _previous.Next = token;

            if (definition.IsWhitespace)
            {
                for (int i = _previousWS == null ? 0 : _previousWS.TokenIndex; i < _contents.Count; i++) _contents[i].NextWS = token;
                _previousWS = token;
            }
            else
            {
                for (int i = _previousNWS == null ? 0 : _previousNWS.TokenIndex; i < _contents.Count; i++) _contents[i].NextNWS = token;
                _previousNWS = token;
            }

            _previous = token;
            _contents.Add(token);
            return token;
        }
    }
    #endregion
}
