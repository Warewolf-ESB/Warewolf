using System.Text;

namespace Dev2.Common.Interfaces
{
    public interface IXamlResource:IResourceDefinition
    {
        StringBuilder Xaml { get; set; }
    }
}