namespace Warewolf.Auditing
{
    public interface IStateNotifierFactory
    {
        IStateNotifier New(IExecutionContext dataObject);
    }
}