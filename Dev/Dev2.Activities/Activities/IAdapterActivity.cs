using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public interface IAdapterActivity
    {
        IFlowNodeActivity GetInnerNode();
    }
}
