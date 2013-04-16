namespace Dev2.Studio.Core.Interfaces {
    public interface IOperatorType {
        string OperatorName { get; set; }
        string FriendlyName { get; set; }
        string OperatorSymbol { get; set; }
        dynamic Parent { get; set; }
        string TagName { get; set; }
        object Value { get; set; }
        object EndValue { get; set; }
        bool Selected { get; set; }
        bool ShowEndValue { get; set; }
        string Expression { get; }
        bool IsValid { get; }
    }
}