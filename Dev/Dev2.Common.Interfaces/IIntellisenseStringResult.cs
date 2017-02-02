namespace Dev2.Common.Interfaces
{
    public  interface IIntellisenseStringResult
    {
        string Result { get; }
        int CaretPosition { get; }
    }
}