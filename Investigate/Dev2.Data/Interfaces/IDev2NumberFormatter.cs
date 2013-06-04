using Dev2.Data.TO;

// ReSharper disable CheckNamespace
namespace Dev2.Data.Operations
// ReSharper restore CheckNamespace
{
    public interface IDev2NumberFormatter
    {
        // ReSharper disable InconsistentNaming
        string Format(FormatNumberTO formatNumberTO);
        // ReSharper restore InconsistentNaming
    }
}
