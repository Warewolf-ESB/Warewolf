namespace Dev2.Composition
{
    public interface IPostCompositionInitializable
    {
        void Initialize();
        bool AlreadyInitialized { get; }
    }
}
