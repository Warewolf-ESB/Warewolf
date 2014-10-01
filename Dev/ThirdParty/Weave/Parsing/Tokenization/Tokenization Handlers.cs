
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
    #region TokenizationHandler<T, K>
    /// <summary>
    /// Abstract base class that allows inheritors to implement custom logic for use during the
    /// identification cycle of the Tokenizer.
    /// </summary>
    /// <typeparam name="T">The type of Token used by the host tokenizer.</typeparam>
    /// <typeparam name="K">The type of TokenDefinition used by the host tokenizer.</typeparam>
    public abstract class TokenizationHandler<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        internal int Index;

        /// <summary>
        /// Hint used by the AbstractSyntaxTreeBuilder that this particular handler should be called prior
        /// to any unary token identification handlers are called.
        /// </summary>
        public virtual bool IdentifyBeforeUnaryTokens { get { return false; } }
        /// <summary>
        /// Hint used by the AbstractSyntaxTreeBuilder that this particular handler should be called directly after
        /// any unary token identification handlers are called, but before any inconclusive handlers are called (Handlers
        /// that have both <see cref="IdentifyBeforeUnaryTokens"/> and <see cref="IdentifyAfterUnaryTokens"/> set to false)
        /// </summary>
        public virtual bool IdentifyAfterUnaryTokens { get { return false; } }

        /// <summary>
        /// Invoked by the tokenizer on a per tokenization request basis prior to the start of the
        /// tokenization process. This gives handlers the opportunity to setup any TokenizationHandlerExecutionStore
        /// state data required during the tokenization process.
        /// </summary>
        /// <param name="store">The TokenizationExecutionStore that represents the current tokenization request.</param>
        public virtual void Reset(TokenizationExecutionStore<T, K> store) { }
        /// <summary>
        /// Invoked by the tokenizer during the identification cycle of the tokenization process.
        /// </summary>
        /// <param name="tokenizer">The tokenizer that is processing the current tokenization request.</param>
        /// <param name="store">The TokenizationExecutionStore that represents the current tokenization request.</param>
        /// <param name="result">The token definition that represents the identification that was made.</param>
        /// <returns>true to indicate to the tokenizer that identification has been made (regardless of success), false to
        /// indicate to the tokenizer that identification has not been made and any pending handlers should be given the
        /// opportunity to identify the current character.</returns>
        public abstract bool IdentifyToken(Tokenizer<T, K> tokenizer, TokenizationExecutionStore<T, K> store, ref K result);
    }
    #endregion

    #region WhitespaceTokenizationHandler<T, K>
    public sealed class WhitespaceTokenizationHandler<T, K> : TokenizationHandler<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        public override bool IdentifyToken(Tokenizer<T, K> tokenizer, TokenizationExecutionStore<T, K> store, ref K result)
        {
            if (!StringUtility.WhitespaceMask[store.Input.Current]) return false;

            result = store.Input.Current == StringUtility.IntNewLine ? tokenizer.RequiredDefinitions.LineBreak : tokenizer.RequiredDefinitions.Whitespace;

            store.Input.End = store.Input.Length;
            store.Input.Origin = store.Input.Position;

            for (int k = store.Input.Position + 1; k < store.Input.Length; k++)
                if (!StringUtility.WhitespaceMask[store.Input.Previous = store.Input[k]])
                {
                    store.Input.End = k;
                    break;
                }
                else if (store.Input.Previous == StringUtility.IntNewLine)
                    result = tokenizer.RequiredDefinitions.LineBreak;

            return true;
        }
    }
    #endregion

    #region UnaryTokenizationHandler<T, K>
    public sealed class UnaryTokenizationHandler<T, K> : TokenizationHandler<T, K>
        where T : Token<T, K>, new()
        where K : TokenDefinition
    {
        private StringValueCollection<K> _definitions;
        private BooleanArray _mask;
        private int _longest;

        internal StringValueCollection<K> Definitions { get { return _definitions; } }

        public sealed override bool IdentifyBeforeUnaryTokens { get { return false; } }
        public sealed override bool IdentifyAfterUnaryTokens { get { return false; } }

        public UnaryTokenizationHandler(IEnumerable<K> definitions)
        {
            _mask = new BooleanArray(256, false);
            _definitions = new StringValueCollection<K>();
            _definitions.SetNullValue(null);

            foreach (K current in definitions)
            {
                int length = current.Identifier.Length;
                if (length > _longest) _longest = length;
                for (int i = 0; i < length; i++) _mask[current.Identifier[i]] = true;
                _definitions.Add(current.Identifier, current);
            }
        }

        public override bool IdentifyToken(Tokenizer<T, K> tokenizer, TokenizationExecutionStore<T, K> store, ref K result)
        {
            if (!_mask[store.Input.Current]) return false;

            store.Input.End = 1;

            for (int i = 1; i < _longest; i++)
                if (_mask[store.Input[store.Input.Position + i]]) store.Input.End = i + 1;
                else break;

            for (int k = store.Input.End; k > 0; k--)
                if ((store.Input.Origin = _definitions.IndexOf(store.Input.Source, store.Input.Position, store.Input.End)) == -1) store.Input.End = k - 1;
                else break;

            result = null;

            if (store.Input.End == 0) return false;
            else
            {
                result = _definitions[store.Input.Origin];
                store.Input.Origin = store.Input.Position;
                store.Input.End += store.Input.Position;
            }

            return true;
        }
    }
    #endregion
}
