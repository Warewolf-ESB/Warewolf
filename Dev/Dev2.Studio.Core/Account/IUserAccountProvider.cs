
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Account
{
    public interface IUserAccountProvider
    {
        string UserName { get; }
        string Password { get; }
    }
}
