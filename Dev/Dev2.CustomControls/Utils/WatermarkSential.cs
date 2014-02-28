
namespace Dev2.CustomControls.Utils
{
    public static class WatermarkSential
    {
        /// <summary>
        /// This value is only set when scrolling a designer
        /// The reason this exist is due to the fact that when a designer is loaded not
        /// all water marks are updated. Only on scroll, hence why this is static. 
        /// You can only scroll 1 designer at a time ;)
        /// </summary>
        public static bool IsWatermarkBeingApplied { get; set; }
    }
}
