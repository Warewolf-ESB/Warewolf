using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IDllListing
    {
        string Name { get; set; }        
        string FullName { get; set; }
        IList<IDllListing> Children { get; set; }
        bool IsDirectory { get; set; }
    }

    public interface IDllListingModel : IDllListing
    {
        void Filter(string searchTerm);
        bool IsExpanded { get; set; }
        bool IsExpanderVisible { get; }
        bool IsVisible { get; set; }
        bool IsSelected { get; set; }
    }
}