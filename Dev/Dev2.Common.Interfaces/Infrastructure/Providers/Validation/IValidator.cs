
namespace Dev2.Common.Interfaces.Infrastructure.Providers.Validation
{
    public interface IValidator
    {
        bool IsValid { get; }

        void Validate();
    }
}