using System;
using System.Data;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Patterns;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

// ReSharper disable once CheckNamespace
namespace Dev2.DataList.Contract
{
    /// <summary>
    /// Responsible for converting to and from various formats
    /// </summary>
    public interface IDataListTranslator : ISpookyLoadable<DataListFormat>
    {

        /// <summary>
        /// The format this IDataListTranslator supports.
        /// </summary>
        DataListFormat Format { get; }
        /// <summary>
        /// The text encoding (if any) that this IDataListTranslator uses to work with the binary representations.
        /// </summary>
        Encoding TextEncoding { get; }

        /// <summary>
        /// Converts from a binary representation in the standard format to the specified <see cref="Format" />.
        /// </summary>
        /// <param name="input">The binary representation of the datalist.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the specified <see cref="Format" />
        /// </returns>
        DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input, out ErrorResultTO errors);

        /// <summary>
        /// Converts from a binary representation in the specified <see cref="Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specified <see cref="Format" /></param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        IBinaryDataList ConvertTo(byte[] input, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts from a binary representation in the specified <see cref="Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specified <see cref="Format" /></param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        IBinaryDataList ConvertTo(object input, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        IBinaryDataList ConvertAndOnlyMapInputs(byte[] input, string shape, out ErrorResultTO errors);

        /// <summary>
        /// Converts from a binary representation in the specified <see cref="Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specified <see cref="Format" /></param>
        /// <param name="targetDl">The target dialog.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors);

        /// <summary>
        /// Converts the and filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="filterShape">The filter shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string ConvertAndFilter(IBinaryDataList input, string filterShape, out ErrorResultTO errors);

        DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions);
    }
}
