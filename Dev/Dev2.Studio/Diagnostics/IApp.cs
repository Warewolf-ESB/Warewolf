namespace Dev2.Studio.Diagnostics
{
    public interface IApp
    {
        void Shutdown();
        bool ShouldRestart { get; set; }
    }
}
