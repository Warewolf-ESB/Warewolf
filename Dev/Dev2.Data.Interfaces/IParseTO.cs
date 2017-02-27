namespace Dev2.Data.Interfaces
{
    public interface IParseTO
    {
        string Payload { get; set; }
        int StartIndex { get; set; }
        int EndIndex { get; set; }
        IParseTO Child { get; set; }
        IParseTO Parent { get; set; }
        bool HangingOpen { get; set; }
        bool IsRecordSet { get; }
        bool IsRoot { get; }
        bool IsLeaf { get; }
    }
}