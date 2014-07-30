using Dev2.DataList.Contract;
using System;

namespace Dev2.Data.SystemTemplates.Models
{
    public interface IDev2DataModel
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        string Version { get; }

        /// <summary>
        /// Gets the name of the model.
        /// </summary>
        /// <value>
        /// The name of the model.
        /// </value>
        Dev2ModelType ModelName { get; }

        /// <summary>
        /// To the web model.
        /// </summary>
        /// <returns></returns>
        string ToWebModel();

        /// <summary>
        /// Generates the user friendly model.
        /// </summary>
        /// <returns></returns>
        string GenerateUserFriendlyModel(Guid dlid, Dev2DecisionMode mode, out ErrorResultTO errors);

    }
}
