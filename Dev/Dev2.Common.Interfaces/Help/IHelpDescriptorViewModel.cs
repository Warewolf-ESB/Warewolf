using System.Windows.Media;

namespace Dev2.Common.Interfaces.Help
{
    public interface IHelpDescriptorViewModel
    {
        /// <summary>
        /// Display name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The help text
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The icon to display
        /// </summary>
        DrawingImage Icon { get; }

        /// <summary>
        /// Is this help enabled.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}