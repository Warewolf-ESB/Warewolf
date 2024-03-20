using Elsa.Workflows.Contracts;
using Elsa.Workflows.UIHints.Dropdown;
using System.Reflection;

namespace Elsa.Samples.AspNet.CustomUIHandler;

/// <summary>
/// A custom dropdown options provider to provide vehicle options for the Brand property of <see cref="VehicleActivity"/>.
/// </summary>
public class CountryUIHandler : DropDownOptionsProviderBase
{
    private readonly Random _random = new();

    protected override ValueTask<ICollection<SelectListItem>> GetItemsAsync(PropertyInfo propertyInfo, object? context, CancellationToken cancellationToken)
    {
        var items = new List<SelectListItem>
        {
            new("BMW", "1"),
            new("Tesla", "2"),
            new("Peugeot", "3"),
            new(_random.Next(100).ToString(), "4")
        };

        return new(items);
    }
}


/// <summary>
/// Configures the specified property to refresh the UI when the property value changes.
/// </summary>
public class CountryRefreshUIHandler : IPropertyUIHandler
{
    public ValueTask<IDictionary<string, object>> GetUIPropertiesAsync(PropertyInfo propertyInfo, object? context, CancellationToken cancellationToken = default)
    {
        IDictionary<string, object> result = new Dictionary<string, object>
        {
            { "Refresh", true }
        };
        return ValueTask.FromResult(result);
    }
}
