namespace Dev2.Studio.Diagnostics
{
    public class MockApp : IApp
    {
        public virtual void Shutdown(){}
        public virtual bool ShouldRestart { get; set; }
    }
}
