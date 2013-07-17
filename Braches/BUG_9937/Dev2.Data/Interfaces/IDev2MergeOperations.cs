using Dev2.Data.Enums;

namespace Dev2.Data.Operations
{
    public interface IDev2MergeOperations
    {
        /// <summary>
        /// Gets the merged data.
        /// </summary>
        /// <value>
        /// The merged data.
        /// </value>
        string MergedData { get; }

        /// <summary>
        /// Merges the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mergeType">Type of the merge.</param>
        /// <param name="at">At.</param>
        /// <param name="padding">The padding.</param>
        /// <param name="mergeAlignment">The merge alignment.</param>
        void Merge(string value, enMergeType mergeType, string at, string padding, enMergeAlignment mergeAlignment);

        void Merge(string value, string mergeType, string at, string padding, string align);

        /// <summary>
        /// Clears the Merged Data.
        /// </summary>
        void Clear();
    }
}