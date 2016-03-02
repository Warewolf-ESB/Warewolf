using System.Diagnostics.CodeAnalysis;

namespace Dev2.Activities.Designers2.Core.Web.Base
{
    [ExcludeFromCodeCoverage]
    public class WebTooRegionDisplayInfo
    {
        public string ToolRegionName { get; set; }
        public bool IsVisible { get; set; }
        public double BaseHeight { get; set; }
    }
}