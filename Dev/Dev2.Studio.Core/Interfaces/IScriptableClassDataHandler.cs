
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IScriptableClassDataHandler
    {
        void DataReceived(string data);
        void DataReceived(string data, string uri);
        void Close();
    }
}
