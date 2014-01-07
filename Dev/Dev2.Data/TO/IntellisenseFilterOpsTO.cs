
namespace Dev2.DataList.Contract
{
    public class IntellisenseFilterOpsTO : IIntellisenseFilterOpsTO
    {
        public enIntellisensePartType FilterType { get; set; }
        public string FilterCondition { get; set; }
    }
}
