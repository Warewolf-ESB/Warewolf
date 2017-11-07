using System;

namespace Dev2.Common.Annotations
{
    [AttributeUsage(
      AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
      AttributeTargets.Delegate | AttributeTargets.Field | AttributeTargets.Event)]
    public sealed class NotNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
    {
        public NotifyPropertyChangedInvocatorAttribute() { }
        public NotifyPropertyChangedInvocatorAttribute(string parameterName)
        {
          ParameterName = parameterName;
        }
        public string ParameterName { get; private set; }
    }
}