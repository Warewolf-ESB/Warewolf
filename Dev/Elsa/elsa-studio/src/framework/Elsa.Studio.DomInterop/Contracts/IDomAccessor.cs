using Elsa.Studio.DomInterop.Models;

namespace Elsa.Studio.DomInterop.Contracts;

/// <summary>
/// Provides access to the browser's DOM.
/// </summary>
public interface IDomAccessor
{
    Task<DomRect> GetBoundingClientRectAsync(ElementRef elementRef, CancellationToken cancellationToken = default);
    Task<double> GetVisibleHeightAsync(ElementRef elementRef, CancellationToken cancellationToken = default);
    Task ClickElementAsync(ElementRef elementRef, CancellationToken cancellationToken = default);
}