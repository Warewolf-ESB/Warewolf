using System.Windows.Media;
using Dev2.Common.Interfaces.Help;

namespace Warewolf.Studio.Models.Help
{
    public class HelpDescriptor:IHelpDescriptor
    {
        public HelpDescriptor(string name, string description, DrawingImage icon)
        {
            Icon = icon;
            Description = description;
            Name = name;
        }

        #region Implementation of IHelpDescriptor

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Help text
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Icon
        /// </summary>
        public DrawingImage Icon { get; private set; }

        #endregion
    }
}