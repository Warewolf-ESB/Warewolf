using System.Text;
using Dev2.Data.Enums;

namespace Dev2.Data.Operations
{
    public interface IDev2MergeOperations
    {

        StringBuilder MergeData { get; }

        /// <summary>
        /// Merges the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="at">At.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="mergeAlignment">The merge alignment.</param>
        void Merge(string value, enMergeType mergeType, string at, string padding, enMergeAlignment mergeAlignment);

        /// <summary>
        /// Merges the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="at">At.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="align">The align.</param>
        void Merge(string value, string mergeType, string at, string padding, string align);

        /// <summary>
        /// Clears the Merged Data.
        /// </summary>
        void Clear();
    }
}