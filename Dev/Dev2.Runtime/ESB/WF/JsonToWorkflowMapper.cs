using Dev2.Activities.WF;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.WF
{
    public class JsonToWorkflowMapper
    {
        public static void Process(EsbExecuteRequest request)
        {
            var xaml = X6ToWorkflowConverter.ConvertJsonToXaml(request.Args);
            if (xaml != null && xaml.Length > 0)
            {
                request.AddArgument("ResourceXaml", xaml);
                return;
            }
        }
    }
}
