/* Written by Brian Lagunas
 * Blog: http://brianlagunas.com
 * Twitter: @brianlagunas
 * Email: blagunas@infragistics.com 
 */

using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IDockAware
    {
        string Header { get; set; }
        ResourceType? Image { get; }
    }
}
