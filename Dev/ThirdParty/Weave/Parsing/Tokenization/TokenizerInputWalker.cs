
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

namespace System.Parsing.Tokenization
{
    /// <summary>
    /// Transforms an input string from the Tokenizer into a binary representation.
    /// </summary>
    /// <typeparam name="T">The type of Token used by the host tokenizer.</typeparam>
    /// <typeparam name="K">The type of TokenDefinition used by the host tokenizer.</typeparam>
    public sealed class TokenizerInputWalker<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        #region Instance Fields
        private TokenizationExecutionStore<T, K> _store;

        private string _source;
        private byte[] _data;
        private int _length;
        private int _range;

        private byte _previous;
        private byte _current;
        private byte _next;

        private int _origin;
        private int _index;
        private int _position;
        private int _end;

        private int _offset;
        #endregion

        #region Public Properties
        /// <summary>
        /// The original input string that is being tokenized.
        /// </summary>
        public string Source { get { return _source; } }
        /// <summary>
        /// The number of characters in <see cref="Source"/>
        /// </summary>
        public int Length { get { return _length; } }
        /// <summary>
        /// The binary representation of the character that occurs directly prior to <see cref="Current"/>
        /// </summary>
        public byte Previous { get { return _previous; } set { _previous = value; } }
        /// <summary>
        /// The binary representation of the character that is currently being examined.
        /// </summary>
        public byte Current { get { return _current; } set { _current = value; } }
        /// <summary>
        /// The binary representation of the character that occurs directly after to <see cref="Current"/>
        /// </summary>
        public byte Next { get { return _next; } set { _next = value; } }

        /// <summary>
        /// The index in <see cref="Source"/> that denotes the first identified character of the current
        /// identification cycle.
        /// </summary>
        public int Origin { get { return _origin; } set { _origin = value; } }
        /// <summary>
        /// The index in <see cref="Source"/> that denotes the first non identified character of the current
        /// identification cycle.
        /// </summary>
        public int Index { get { return _index; } }
        /// <summary>
        /// The index in <see cref="Source"/> at which <see cref="Current"/> was assigned.
        /// </summary>
        public int Position { get { return _position; } }
        /// <summary>
        /// The index in <see cref="Source"/> that denotes the first character at which the next identification
        /// cycle should start.
        /// </summary>
        public int End { get { return _end; } set { _end = value; } }
        #endregion

        #region Indexers
        /// <summary>
        /// The binary representation of the character at <paramref name="index"/> of
        /// <see cref="Source"/>
        /// </summary>
        /// <param name="index">An integer between 0 and <see cref="Length"/> - 1</param>
        /// <returns>The binary representatino of the character at <paramref name="index"/> of <see cref="Source"/></returns>
        public byte this[int index] { get { return _data[index - _offset]; } }
        #endregion

        #region Constructor
        internal TokenizerInputWalker(TokenizationExecutionStore<T, K> tokenizer)
        {
            _store = tokenizer;
        }
        #endregion

        #region Reset Handling
        internal void Reset(string source)
        {
            Reset(source, 0, source.Length);
        }

        internal unsafe void Reset(string source, int index, int length)
        {
            _source = source;
            _offset = index;
            _range = (_length = length) + _offset + 1;
            _data = new byte[_length + Byte.MaxValue];

            fixed (char* data = source)
                for (int i = 0; i < _length; i++)
                    _data[i] = (byte)data[i + _offset];

            _previous = _next = Byte.MinValue;
            _origin = _index = _position = _offset;
            _end = 0;
            _current = _data[0];
            _length += _offset;
        }
        #endregion

        #region Advance Handling
        internal bool BeginAdvance()
        {
            if (_position >= _range)
            {
                if (_index < _length) _store.IdentifyKeyword(_index, _length - _index);
                return false;
            }

            _next = _data[_position + 1 - _offset];
            return true;
        }

        internal bool EndAdvance(K advanceDefinition)
        {
            if (_end == -1) return false;
            else if (_end != 0)
            {
                if (_index < _origin) _store.IdentifyKeyword(_index, _origin - _index);
                _store.Builder.Append(_origin, _end - _origin, advanceDefinition);
                _index = _end;

                if (--_end != _position)
                {
                    _position = _end;
                    _current = _data[_position - _offset];
                    _next = _data[_position + 1 - _offset];
                }

                _end = 0;
            }

            _previous = _current;
            _current = _next;
            ++_position;
            return true;
        }
        #endregion
    }
}
