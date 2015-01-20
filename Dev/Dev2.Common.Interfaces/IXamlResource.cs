using System.Text;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface IXamlResource:IResource
    {
        StringBuilder Xaml { get; set; }
    }
}