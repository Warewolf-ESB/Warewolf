using System.Collections.Generic;

namespace Dev2.DataList.Contract.Interfaces {
    public interface IDev2StudioDataLanguageParser {

        /// <summary>
        /// Parses for activity data items.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        IList<string> ParseForActivityDataItems(string payload);
    }
}
