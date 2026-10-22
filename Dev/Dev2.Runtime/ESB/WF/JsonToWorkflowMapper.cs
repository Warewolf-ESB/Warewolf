using Warewolf.Bite;
using Dev2.Communication;

namespace Dev2.Runtime.ESB.WF
{
    public class JsonToWorkflowMapper
    {
        public static void Process(EsbExecuteRequest request)
        {
            var xaml = BiteFormat.X6ToXaml(request.Args);
            if (xaml != null && xaml.Length > 0)
            {
                request.AddArgument("ResourceXaml", xaml);
                return;
            }
        }
    }
}
