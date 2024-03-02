#if !NETFRAMEWORK
using FontAwesome6;
using System.Windows.Media;
using FontAwesome6.Fonts.Extensions;

namespace Warewolf.Studio.Core.Extensions
{
    public static class ImageAwesomeExtensions
    {
        public static System.Windows.Media.ImageSource CreateImageSource(EFontAwesomeIcon icon, Brush primary, Brush secondary = null, bool? swapOpacity = null, double? primaryOpacity = null, double? secondaryOpacity = null, double emSize = 100)
        {
            return new DrawingImage(icon.CreateDrawing(primary, secondary, swapOpacity, primaryOpacity, secondaryOpacity, emSize));
        }
        
    }
}
#endif