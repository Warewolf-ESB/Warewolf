/* Written by Brian Lagunas
 * Blog: http://brianlagunas.com
 * Twitter: @brianlagunas
 * Email: blagunas@infragistics.com 
 */

using Dev2.Common.Interfaces.Data;

namespace Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter
{
    public interface IDockAware
    {
        string Header { get; set; }
        ResourceType? Image { get; }
    }
}
