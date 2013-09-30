
namespace Dev2.Providers.Validation
{
    public interface IValidator
    {
        bool IsValid { get; }

        void Validate();
    }
}