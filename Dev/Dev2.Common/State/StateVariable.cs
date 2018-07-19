namespace Dev2.Common.State
{
    public struct StateVariable
    {
        public enum StateType { Input, Output, InputOutput };
        public StateType Type;
        public string Value;
    }
}
