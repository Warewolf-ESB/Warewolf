namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public interface IWrappedObject<T>
    {
        T Instance { get; }
    }
}