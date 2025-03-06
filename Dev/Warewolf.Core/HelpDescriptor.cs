#if WINDOWS
using System.Windows.Media;
#endif
using Dev2.Common.Interfaces.Help;

namespace Warewolf.Core
{
    public class HelpDescriptor:IHelpDescriptor
    {
#if WINDOWS
        public HelpDescriptor(string name, string description, DrawingImage icon)
        {
            Icon = icon;
            Description = description;
            Name = name;
        }
#endif


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
        public object Icon { get; private set; }

        #endregion
    }
}