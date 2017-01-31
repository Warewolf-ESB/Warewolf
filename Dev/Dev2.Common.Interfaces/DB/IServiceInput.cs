﻿namespace Dev2.Common.Interfaces.DB
{
    public interface IServiceInput
    {
        string Name { get; set; }
        string Value { get; set; }
        bool RequiredField { get; set; }
        bool EmptyIsNull { get; set; }
        string TypeName { get; set; }
        enIntellisensePartType IntellisenseFilter { get; set; }
    }
}
