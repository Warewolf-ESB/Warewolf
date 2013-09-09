using System;

namespace Dev2.Activities.Adorners
{
    public static class AutomationIdExtensions
    {
        /// <summary>
        /// Gets the automation id for the button as populated by the <see cref="Dev2.Activities.Adorners.AdornerAutomationIdAttribute"/>.
        /// The attribute gets used on the <see cref="Dev2.Activities.Adorners.OverlayType"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public static string GetButtonAutomationId(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(AdornerAutomationIdAttribute))
                        as AdornerAutomationIdAttribute;

            return attribute == null ? value.ToString() : attribute.ButtonId;
        }

        /// <summary>
        /// Gets the automation id for the button as populated by the <see cref="Dev2.Activities.Adorners.AdornerAutomationIdAttribute"/>.
        /// The attribute gets used on the <see cref="Dev2.Activities.Adorners.OverlayType"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public static string GetContentAutomationId(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                    = Attribute.GetCustomAttribute(field, typeof(AdornerAutomationIdAttribute))
                        as AdornerAutomationIdAttribute;

            return attribute == null ? value.ToString() : attribute.ContentId;
        }
    
    }
}
