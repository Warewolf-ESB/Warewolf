namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IWrappedObject<T>
    {
        T Instance { get; }
    }
}