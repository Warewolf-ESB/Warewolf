using System;
using System.Text;
using Dev2.Data.Enums;

namespace Dev2.Data.Operations
{
    public class Dev2MergeOperations : IDev2MergeOperations
    {
        #region Ctor

        public Dev2MergeOperations()
        {
            MergeData = new StringBuilder();
        }

        #endregion Ctor

        #region Properties

        public StringBuilder MergeData { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Merges the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="at">At.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="mergeAlignment">The merge alignment.</param>
        /// <exception cref="System.ArgumentNullException">value;The value can not be null.</exception>
        public void Merge(string value, enMergeType mergeType, string at, string padding, enMergeAlignment mergeAlignment)
        {
            if(value != null)
            {
                switch(mergeType)
                {
                    case enMergeType.Char:
                        CharMergeOp(value, at);
                        break;

                    case enMergeType.Index:
                        IndexMergeOp(value, at, padding, mergeAlignment);
                        break;

                    case enMergeType.NewLine:
                        NewLineMergeOp(value);
                        break;

                    case enMergeType.Tab:
                        TabMergeOp(value);
                        break;

                    case enMergeType.None:
                        NoneMergeOp(value);
                        break;

                    default:
                        throw new Exception("Error In Dev2MergeOperations");
                }
            }
            else
            {
                throw new ArgumentNullException("value", "The value can not be null.");
            }

        }

        public void Merge(string value, string mergeType, string at, string padding, string align)
        {
            enMergeType mergingType;
            enMergeAlignment mergeAlignment = enMergeAlignment.Left;

            switch(mergeType)
            {
                case "Index":
                    switch(align)
                    {
                        case "Left":
                            mergeAlignment = enMergeAlignment.Left;
                            break;

                        case "Right":
                            mergeAlignment = enMergeAlignment.Right;
                            break;
                    }
                    mergingType = enMergeType.Index;
                    break;

                case "Tab":
                    mergingType = enMergeType.Tab;
                    break;

                case "New Line":
                    mergingType = enMergeType.NewLine;
                    break;

                case "Chars":
                    mergingType = enMergeType.Char;
                    break;

                case "None":
                    mergingType = enMergeType.None;
                    break;

                default:
                    throw new Exception("Error In Dev2MergeOperations");
            }
            Merge(value, mergingType, at, padding, mergeAlignment);
        }

        public void Clear()
        {
            MergeData.Clear();
        }

        #endregion Methods

        #region Private Methods

        #region IndexMergeOp

        /// <summary>
        /// Merge data to the class string using a index merge, mainly used for fixed width,if the index is more than the length of the value then the padding will be applyed useing the padding character
        /// </summary>
        /// <param name="value">The value that will be merged to the class string</param>
        /// <param name="at">The numeric index that will be used during the merge</param>
        /// <param name="padding">The padding character that will be used</param>
        /// <param name="mergeAlignment">The alignment used for the padding</param>
        private void IndexMergeOp(string value, string at, string padding, enMergeAlignment mergeAlignment)
        {
            int indexToUse;
            if(Int32.TryParse(at, out indexToUse))
            {
                string paddedString = string.Empty;
                int difference = indexToUse - value.Length;
                if(difference >= 0)
                {
                    var padChar = string.IsNullOrEmpty(padding) || padding.Length < 1 ? ' ' : padding[0];
                    paddedString = paddedString.PadRight(difference, padChar);

                    if(mergeAlignment == enMergeAlignment.Left)
                    {
                        paddedString = paddedString.Insert(0, value);
                    }
                    else
                    {
                        paddedString += value;
                    }
                }
                else if(difference < 0)
                {
                    paddedString = value.Substring(0, indexToUse);
                }

                MergeData.Append(paddedString);
            }
        }

        #endregion

        #region CharMergeOp

        /// <summary>
        /// Merge data to the class string using a Character merge, which will merge the data with the specified characters inbetween
        /// </summary>
        /// <param name="value">The value that will be merged to the class string</param>
        /// <param name="at">The Charecters that will be used as the merge token</param>
        private void CharMergeOp(string value, string at)
        {
            MergeData.Append(value).Append(at);
        }

        #endregion

        #region NewLineMergeOp

        /// <summary>
        /// Merge data to the class string using a NewLine merge, which will merge the data with a new line in between
        /// </summary>
        /// <param name="value">The value that will be merged to the class string</param>
        private void NewLineMergeOp(string value)
        {
            MergeData.Append(value).Append("\r\n");
        }

        #endregion

        #region TabMergeOp

        /// <summary>
        /// Merge data to the class string using a Tab merge, which will merge the data with a Tab inbetween
        /// </summary>
        /// <param name="value">The value that will be merged to the class string</param>
        private void TabMergeOp(string value)
        {
            MergeData.Append(value).Append("\t");
        }

        #endregion

        #region NoneMergeOp

        /// <summary>
        /// Merge data to the class string using a None merge, which will merge the data with nothing inbetween
        /// </summary>
        /// <param name="value">The value that will be merged to the class string</param>
        private void NoneMergeOp(string value)
        {
            MergeData.Append(value);
        }

        #endregion

        #endregion Private Methods
    }
}
