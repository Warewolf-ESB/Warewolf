using System;

namespace Dev2.Communication
{
    /// <summary>
    /// Used to describe serialized content.
    /// </summary>
    public class Envelope
    {
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the content - typically a JSON string.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the order unique identifier.
        /// </summary>
        /// <value>
        /// The order unique identifier.
        /// </value>
        public int PartID { get; set; }
    }
}