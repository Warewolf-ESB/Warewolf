using System.Windows;

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// Class which contains the options which influence to performance of spelling
    /// </summary>
    public class PerformanceOptions : DependencyObject
    {
        private ICheckerEngine _checker;
		internal ICheckerEngine Checker
        {
            get { return _checker; }
            set
            {
                if (value == null)
                    return;
                _checker = value;
                _checker.AllowCapitalizedWords = this.AllowCapitalizedWords;
                _checker.CheckCompoundWords = this.CheckCompoundWords;
                _checker.ConsiderationRange = this.ConsiderationRange;
                _checker.SplitWordThreshold = this.SplitWordThreshold;
                _checker.SuggestSplitWords = this.SuggestSplitWords;
            }
        }

        #region AllowCapitalizedWords
        private bool _allowCapitalizedWords;
        /// <summary>
        /// Gets/Sets whether to Allow words with all capital letters. 
        /// </summary>
        /// <remarks>
        /// Performance increases marginally if set to true. When set to "True" words that only have
        /// capitalized letters are ignored. When set to "False", 
        /// words with all capitalized letters will be treated as a normal word. 
        /// </remarks>
        public bool AllowCapitalizedWords
        {
            get { return this._allowCapitalizedWords; }
            set
            {
                this._allowCapitalizedWords = value;
                if (Checker != null)
                {
                    Checker.AllowCapitalizedWords = value;
                }
            }
        }

        #endregion // AllowCapitalizedWords

        #region CheckCompoundWords
        private bool _checkCompoundWords;
        /// <summary>
        /// Gets/Sets whether to check for compound words.
        /// </summary>
        /// <remarks>
        /// <p>Setting this to true is essential for languages such as German which allow for compound words.
        /// For example: if the word is "thesewordsarejoined" when the property is set to "False" the word 
        /// would be marked as misspelled. But when set to "True" the word would be marked as spelled 
        /// correctly. However, if one of the words inside of the
        /// compound word was misspelled, the whole word would be marked as misspelled. 
        /// </p>
        /// <p>Setting this to true will degrade lookup performance by a maximum of approximately 100[1-1/n] percent (where n is the number of letters in the word) for misspelt words.</p>
        /// <p>There is no performance degredation for correctly spelt words.</p>
        /// <p>On average for texts with mostly correctly spelt words the performance degredation for the Check method is roughly 25%.</p>
        /// </remarks>
        public bool CheckCompoundWords
        {
            get { return this._checkCompoundWords; }
            set
            {
                this._checkCompoundWords = value;
                if (Checker != null)
                {
                    Checker.CheckCompoundWords = value;
                }
            }
        }

        #endregion // CheckCompoundWords

        #region ConsiderationRange
        private int _considerationRange;
        /// <summary>
        /// Gets/Sets the factor for how close words need to be to be suggested.
        /// </summary>
        /// <remarks>
        /// The size factor for words to consider for suggestions. Lower values are faster but consider 
        /// less words for suggestions. When the property is set to a value less than 0 it will default to 80. 
        /// </remarks>
        public int ConsiderationRange
        {
            get { return this._considerationRange; }
            set
            {
                this._considerationRange = value;
                if (Checker != null)
                {
                    Checker.ConsiderationRange = value;
                }
            }
        }

        #endregion // ConsiderationRange

        #region SplitWordThreshold
        private int _splitWordThreshold;
        /// <summary>
        /// The value of the property determines the minimum size of each word in order for it to become a suggestion. 
        /// </summary>
        /// <remarks>
        /// The property coincides with SuggestSplitWords. The value of the property determines the minimum 
        /// size of each word in order for it to become a suggestion. For example, "myhouse" has two words, 
        /// "my" and "house." In order for "my house" to be a suggestion, the SplitWordThreshold must be 
        /// set to 2 or less. A lower threshold results in worse performance. 
        /// /// </remarks>
        public int SplitWordThreshold
        {
            get { return this._splitWordThreshold; }
            set
            {
                this._splitWordThreshold = value;
                if (Checker != null)
                {
                    Checker.SplitWordThreshold = value;
                }
            }
        }

        #endregion // SplitWordThreshold

        #region SuggestSplitWords
        private bool _suggestSplitWords;
        /// <summary>
        /// Gets/Sets whether to look for connected words in suggestions.
        /// </summary>
        /// <remarks>
        /// When set to "True" suggestions may include 
        /// joined words. For example: if the misspelled word was "helloworld," suggestions would include 
        /// "hello world." Note: when turned on there will be a slight hit in performance.  
        /// /// </remarks>
        public bool SuggestSplitWords
        {
            get { return this._suggestSplitWords; }
            set
            {
                this._suggestSplitWords = value;
                if (Checker != null)
                {
                    Checker.SuggestSplitWords = value;
                }
            }
        }

        #endregion // SuggestSplitWords
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved