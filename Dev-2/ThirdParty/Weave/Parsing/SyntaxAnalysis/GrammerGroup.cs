using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Parsing.SyntaxAnalysis
{
    public sealed class GrammerGroup
    {
        #region Literal Grammer Groups
        public static readonly GrammerGroup BooleanLiteralGrammers = new GrammerGroup("Boolean Literal");
        public static readonly GrammerGroup CharLiteralGrammers = new GrammerGroup("Character Literal");
        public static readonly GrammerGroup StringLiteralGrammers = new GrammerGroup("String Literal");
        public static readonly GrammerGroup NumericLiteralGrammers = new GrammerGroup("Numeric Literal");
        public static readonly GrammerGroup NullLiteralGrammers = new GrammerGroup("Null Literal");
        #endregion

        #region Instance Fields
        private string _displayName;
        #endregion

        #region Public Properties
        public string DisplayName { get { return _displayName; } }
        #endregion

        #region Constructor
        public GrammerGroup(string displayName)
        {
            if (String.IsNullOrEmpty(displayName)) throw new ArgumentNullException("displayName");
            _displayName = displayName;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return "GrammerGroup [" + _displayName + "]";
        }
        #endregion
    }
}
