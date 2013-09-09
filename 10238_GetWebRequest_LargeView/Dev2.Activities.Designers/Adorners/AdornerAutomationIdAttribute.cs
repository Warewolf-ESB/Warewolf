using System;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Attribute used in OverLayType to identify the names for the automation IDs during testing.
    /// Only to be used on the enum: OverlayType
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/23</date>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AdornerAutomationIdAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the id that will be used for the button of the specific overlay type
        /// </summary>
        /// <value>
        /// The button id.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public string ButtonId { get; set; }

        /// <summary>
        /// Gets or sets the id associated with the content of the specific overlay type
        /// </summary>
        /// <value>
        /// The content id.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public string ContentId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdornerAutomationIdAttribute"/> class.
        /// </summary>
        /// <param name="buttonId">The automation button element id for the specific overlaytype</param>
        /// <param name="contentId">The automation content element id for the specific overlaytype</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public AdornerAutomationIdAttribute(string buttonId, string contentId)
        {
            ButtonId = buttonId;
            ContentId = contentId;
        }
    }

}
